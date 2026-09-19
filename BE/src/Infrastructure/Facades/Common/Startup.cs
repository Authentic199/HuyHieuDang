using System.Reflection;
using HuyHieuDang.Core.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace HuyHieuDang.Infrastructure.Facades.Common;

internal static class Startup
{
    internal static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
            .AddClasses(filter => filter.AssignableTo<ITransientService>())
                .AsImplementedInterfaces()
                .WithTransientLifetime()
            .AddClasses(filter => filter.AssignableTo<IScopedService>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        return services;
    }
}