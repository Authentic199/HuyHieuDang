using HuyHieuDang.Infrastructure.Facades.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HuyHieuDang.Infrastructure.Facades.Cors;

internal static class Startup
{
    private const string CorsPolicy = nameof(CorsPolicy);

    internal static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CorsSettings>()
           .BindConfiguration(nameof(CorsSettings))
           .ValidateDataAnnotationsRecursively()
           .ValidateOnStart();

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicy, builder =>
            {
                List<string> allowedOrigins = new();
                var allowedOriginsSection = configuration.GetSection($"{nameof(CorsSettings)}:{nameof(CorsSettings.AllowedOrigins)}");

                string? allowedOriginsValue = allowedOriginsSection?.Get<string>();

                if (!string.IsNullOrEmpty(allowedOriginsValue) && !Array.Exists(allowedOriginsValue.Split(';'), x => x.Equals("*", StringComparison.Ordinal)))
                {
                    allowedOrigins.AddRange(allowedOriginsValue.Split(';'));
                }
                else
                {
                    allowedOrigins.Add("*");
                }

                builder.AllowAnyMethod()
                       .AllowAnyHeader()
                       .SetIsOriginAllowedToAllowWildcardSubdomains()
                       .WithOrigins(allowedOrigins.ToArray());
            });
        });

        return services;
    }

    internal static IApplicationBuilder UseCorsPolicy(this IApplicationBuilder app) =>
        app.UseCors(CorsPolicy);
}