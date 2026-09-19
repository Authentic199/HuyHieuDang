using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission;

internal static class Startup
{
    internal static IServiceCollection AddGrantPermission(this IServiceCollection services)
    {
        services.AddScoped(typeof(IGpModelRoleService), typeof(GpModelRoleService));
        services.AddScoped(typeof(IGpRolePermissionService), typeof(GpRolePermissionService));
        services.AddScoped(typeof(IGpModelPermissionService), typeof(GpModelPermissionService));

        return services;
    }
}
