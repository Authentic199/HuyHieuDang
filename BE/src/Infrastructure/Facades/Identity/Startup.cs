using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission;
using HuyHieuDang.Infrastructure.Facades.Identity.JwtToken;
using HuyHieuDang.Infrastructure.Facades.Identity.Password;
using Microsoft.Extensions.DependencyInjection;

namespace HuyHieuDang.Infrastructure.Facades.Identity;

internal static class Startup
{
    internal static IServiceCollection AddCustomIdentity(this IServiceCollection services)
    {
        services.AddGrantPermission();
        services.AddPasswordService();
        services.AddJwtTokenService();
        return services;
    }
}