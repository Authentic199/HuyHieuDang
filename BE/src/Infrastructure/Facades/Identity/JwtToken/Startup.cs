using Microsoft.Extensions.DependencyInjection;

namespace HuyHieuDang.Infrastructure.Facades.Identity.JwtToken
{
    internal static class Startup
    {
        internal static IServiceCollection AddJwtTokenService(this IServiceCollection services)
        {
            services.AddScoped(typeof(IJwtTokenGenerator), typeof(JwtTokenGenerator));
            services.AddHostedService<CleanRefreshTokenWorker>();
            return services;
        }
    }
}