using System.Reflection;
using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure.Facades.Common.Services;
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

        // Nguồn thời gian dùng chung (T-FIX-1): không trạng thái nên đăng ký singleton.
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}