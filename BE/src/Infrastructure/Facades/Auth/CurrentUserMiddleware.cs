using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace HuyHieuDang.Infrastructure.Facades.Auth;

public class CurrentUserMiddleware
{
    private readonly RequestDelegate next;

    public CurrentUserMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext httpContext, ICurrentUserInitializer currentUserInitializer)
    {
        if (
            httpContext.User.Identity is not null
            && httpContext.User.Identity.IsAuthenticated
            && httpContext.GetEndpoint()?.Metadata?.GetMetadata<IAllowAnonymous>() is null)
        {
            currentUserInitializer.SetCurrentUser(httpContext.User);
        }

        await next(httpContext);
    }
}
