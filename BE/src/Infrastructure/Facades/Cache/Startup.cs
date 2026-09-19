using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace HuyHieuDang.Infrastructure.Facades.Cache;

internal static class Startup
{
    internal static IServiceCollection AddCache(this IServiceCollection services)
    {
        services.AddOptions<CacheSettings>()
               .BindConfiguration(nameof(CacheSettings))
               .ValidateDataAnnotationsRecursively()
               .ValidateOnStart();
        services.AddSingleton<IMemoryCache, MemoryCache>();
        return services;
    }
}