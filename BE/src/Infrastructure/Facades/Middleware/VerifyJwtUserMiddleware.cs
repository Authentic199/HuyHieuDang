using HuyHieuDang.Core.Bases;
using HuyHieuDang.Infrastructure.Exceptions.HttpExceptions;
using HuyHieuDang.Infrastructure.Facades.Auth;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.Base;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace HuyHieuDang.Infrastructure.Facades.Middleware;

public class VerifyJwtUserMiddleware
{
    private readonly RequestDelegate next;

    public VerifyJwtUserMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, ICurrentUser currentUser, IRepositoryWrapper repositoryWrapper)
    {
        if (
            httpContext.User.Identity?.IsAuthenticated == true
            &&
            httpContext.GetEndpoint()?.Metadata?.GetMetadata<IAllowAnonymous>() is null)
        {
            // Mot yeu cau da xac thuc ma khong doc duoc kieu chu the la token hong hoac lac hau:
            // tu choi thay vi cho di tiep, neu khong ba buoc kiem tra duoi day se lang le bi bo qua.
            Type? type = Type.GetType(currentUser.GetModelType());

            if (type?.IsAssignableTo(typeof(IJwtUser)) != true)
            {
                throw new UnAuthorizedException(Messages<User>.NotFound());
            }

            Guid userId = currentUser.GetUserId();
            const string repository = nameof(repositoryWrapper.Repository);
            object repositoryBase = repositoryWrapper.GetType().GetMethod(repository)?.MakeGenericMethod(type).Invoke(repositoryWrapper, null)
                ?? throw new InvalidOperationException($"method {repository} is not found from {nameof(repositoryWrapper)}");

            const string findMethodName = nameof(IRepositoryBase<IJwtUser>.Find);

            ParameterExpression x = Expression.Parameter(type, "x");
            LambdaExpression e = DynamicExpressionParser.ParseLambda(new ParameterExpression[] { x }, null, $"x => x.{nameof(BaseEntity.Id)} == @0", userId);

            MethodInfo findMethod = repositoryBase.GetType().GetMethod(findMethodName, new Type[] { e.GetType(), typeof(bool) })
                ?? throw new InvalidOperationException($"method {findMethodName} is not found from {nameof(repositoryBase)}");

            dynamic userQuery = findMethod.Invoke(repositoryBase, new object[] { e, true })
                ?? throw new InvalidOperationException("querry invalid");

            IJwtUser? user = (IJwtUser?)((IQueryable)userQuery).FirstOrDefault();

            Type messages = typeof(Messages<>).MakeGenericType(type);
            if (user == null)
            {
                const string notfoundMethodName = nameof(Messages<IJwtUser>.NotFound);
                MethodInfo notfoundMethod = messages.GetMethod(notfoundMethodName, Type.EmptyTypes)
                     ?? throw new InvalidOperationException($"method {notfoundMethodName} is not found from {nameof(messages)}");
                throw new UnAuthorizedException(notfoundMethod.Invoke(null, null)?.ToString());
            }

            if (user.Status is OperationStatus.Lock)
            {
                const string blockedMethodName = nameof(Messages<IJwtUser>.Blocked);
                MethodInfo blockedMethod = messages.GetMethod(blockedMethodName, Type.EmptyTypes)
                     ?? throw new InvalidOperationException($"method {blockedMethodName} is not found from {nameof(messages)}");
                throw new UnAuthorizedException(blockedMethod.Invoke(null, null)?.ToString());
            }
        }

        await next(httpContext);
    }
}