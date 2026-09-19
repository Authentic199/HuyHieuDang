using HuyHieuDang.Infrastructure.Facades.Identity.Base;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Collections;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace HuyHieuDang.Infrastructure.Facades.Identity.JwtToken
{
    public sealed class CleanRefreshTokenWorker : IHostedService, IDisposable
    {
        private readonly ILogger<CleanRefreshTokenWorker> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ICollection<Type> refreshTokens;
        private int executionCount;
        private Timer? timer;

        public CleanRefreshTokenWorker(ILogger<CleanRefreshTokenWorker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
            refreshTokens = typeof(IRefreshToken).Assembly.GetTypes().Where(x => x.IsClass && x.IsAssignableTo(typeof(IRefreshToken))).ToArray();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Clean RefreshToken Worker running.");

            timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromDays(7));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Clean RefreshToken Worker is stopping.");

            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        private void DoWork(object? state)
        {
            var count = Interlocked.Increment(ref executionCount);
            logger.LogInformation("Clean RefreshToken Worker is working. Count: {Count}", count);
            CleanRefreshToken();
        }

        private void CleanRefreshToken()
        {
            if (refreshTokens.Count > 0)
            {
                logger.LogInformation("---> Begin clean expired refreshtoken");
                using var scope = serviceScopeFactory.CreateScope();
                IRepositoryWrapper repositoryWrapper = scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>();

                if (!repositoryWrapper.CanConnect)
                {
                    logger.LogError("Unable to connect to database");
                    return;
                }

                foreach (Type type in refreshTokens)
                {
                    const string repository = nameof(repositoryWrapper.Repository);
                    object repositoryBase = repositoryWrapper.GetType().GetMethod(repository)?.MakeGenericMethod(type).Invoke(repositoryWrapper, null)
                        ?? throw new InvalidOperationException($"method {repository} is not found from {nameof(repositoryWrapper)}");

                    ParameterExpression x = Expression.Parameter(type, "x");
                    LambdaExpression e = DynamicExpressionParser.ParseLambda(new ParameterExpression[] { x }, null, $"x => x.{nameof(IRefreshToken.ExpireTime)} <= @0", DateTime.UtcNow);
                    const string findMethodName = nameof(IRepositoryBase<IRefreshToken>.Find);
                    MethodInfo findMethod = repositoryBase.GetType().GetMethod(findMethodName, new Type[] { e.GetType(), typeof(bool) })
                        ?? throw new InvalidOperationException($"method {findMethodName} is not found from {nameof(repositoryBase)}");

                    object expiredTokens = findMethod.Invoke(repositoryBase, new object?[] { e, default })
                        ?? throw new InvalidOperationException($"method {findMethodName} is required to return an empty IEnumerable");

                    Type expiredTokensType = expiredTokens.GetType();
                    int tokenCount = 0;
                    if (expiredTokensType.IsGenericType && expiredTokensType.IsAssignableTo(typeof(IEnumerable)))
                    {
                        tokenCount = ((IQueryable)expiredTokens).Count();
                        Log.Information("Have {tokenCount} token of {type} is expired", tokenCount, type.FullName);
                    }

                    if (tokenCount > 0)
                    {
                        const string deleteMethodName = nameof(IRepositoryBase<IRefreshToken>.DeleteRangeAsync);
                        MethodInfo deleteMethod = repositoryBase.GetType().GetMethod(
                                deleteMethodName,
                                new Type[]
                                {
                                    typeof(IEnumerable<>).MakeGenericType(type),
                                    typeof(CancellationToken),
                                }
                            )
                            ?? throw new InvalidOperationException($"method {deleteMethodName} is not found from {nameof(repositoryBase)}");
                        var result = deleteMethod.Invoke(repositoryBase, new object?[] { expiredTokens, default });
                        if (result?.GetType().IsSubclassOf(typeof(Task)) == true)
                        {
                            ((Task)result).Wait();
                        }
                    }
                }

                logger.LogInformation("---> End clean expired refreshtoken");
            }
        }
    }
}