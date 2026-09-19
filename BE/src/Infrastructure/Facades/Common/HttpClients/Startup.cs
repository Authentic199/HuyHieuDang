using Microsoft.Extensions.DependencyInjection;

namespace HuyHieuDang.Infrastructure.Facades.Common.HttpClients
{
    internal static class Startup
    {
        internal static IServiceCollection AddHttpClientSender(this IServiceCollection services)
        {
            services.AddTransient<IHttpClientSender, HttpClientSender>();
            return services;
        }

        internal static IServiceCollection AddClientSetting(this IServiceCollection services)
        {
            services
                .AddOptions<HttpClientSettings>()
                .BindConfiguration(nameof(HttpClientSettings))
                .ValidateDataAnnotationsRecursively()
                .ValidateOnStart();

            return services;
        }
    }
}