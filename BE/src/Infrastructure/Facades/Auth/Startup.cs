using HuyHieuDang.Infrastructure.Facades.Auth.Jwt;
using HuyHieuDang.Infrastructure.Facades.Auth.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HuyHieuDang.Infrastructure.Facades.Auth;

internal static class Startup
{
    internal static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddCurrentUser()
            .AddPermissions()
            .AddJwtAuth(configuration);

        return services;
    }

    internal static IApplicationBuilder UseCurrentUser(this IApplicationBuilder app) =>
       app.UseMiddleware<CurrentUserMiddleware>();

    internal static IServiceCollection AddCurrentUser(this IServiceCollection services) =>
        services
            .AddScoped<ICurrentUser, CurrentUser>()
            .AddScoped(sp => (ICurrentUserInitializer)sp.GetRequiredService<ICurrentUser>());

    private static IServiceCollection AddPermissions(this IServiceCollection services) =>
        services
            .AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>()
            .AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
}