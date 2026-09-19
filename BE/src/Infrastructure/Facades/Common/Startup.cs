using System.Reflection;
using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure.Facades.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        // Dựng bằng factory để chọn đúng hàm khởi tạo nhận IHostEnvironment — nó quyết định
        // biến HUYHIEUDANG_TEST_TODAY có hiệu lực hay không (T-FIX-4, A-903).
        services.AddSingleton<IDateTimeProvider>(provider =>
            new DateTimeProvider(provider.GetRequiredService<IHostEnvironment>()));

        return services;
    }
}