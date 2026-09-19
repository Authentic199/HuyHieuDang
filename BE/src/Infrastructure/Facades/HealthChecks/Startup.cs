using HealthChecks.UI.Client;
using HuyHieuDang.Infrastructure.Facades.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace HuyHieuDang.Infrastructure.Facades.HealthChecks;

internal static class Startup
{
    internal static IServiceCollection AddHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy())
            .AddDatabase(configuration);

        services.AddOptions<HealthCheckUISettings>()
            .BindConfiguration("HealthChecksUI")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var healthCheckUISettings = configuration.GetRequiredSection("HealthChecksUI").Get<HealthCheckUISettings>()!;

        services.AddHealthChecksUI(setupSettings =>
        {
            foreach (var webhook in healthCheckUISettings.CustomWebhooks)
            {
                if (!string.IsNullOrWhiteSpace(webhook.Uri))
                {
                    setupSettings.AddWebhookNotification(
                        webhook.Name ?? "Unnamed",
                        webhook.Uri,
                        webhook.Payload,
                        webhook.RestoredPayload
                    );
                }
            }
        })
        .AddInMemoryStorage();

        return services;
    }

    internal static IApplicationBuilder UseHealthCheck(this IApplicationBuilder app)
    {
        app.UseEndpoints(configure =>
        {
            using var scope = configure.ServiceProvider.CreateScope();
            var healthCheckUISettings = scope.ServiceProvider.GetRequiredService<IOptions<HealthCheckUISettings>>().Value;

            var readyOptions = new HealthCheckOptions
            {
                Predicate = c => c.Tags.Contains("ready"),
                ResponseWriter = async (ctx, rpt) =>
                {
                    ctx.Response.Headers.CacheControl = "no-store, no-cache";
                    await UIResponseWriter.WriteHealthCheckUIResponse(ctx, rpt);
                },
            };

            var liveOptions = new HealthCheckOptions
            {
                Predicate = r => r.Name == "self",
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status204NoContent,
                    [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
                },
                ResponseWriter = async (ctx, _) =>
                {
                    ctx.Response.Headers.CacheControl = "no-store, no-cache";
                    await Task.CompletedTask;
                },
            };

            configure.MapHealthChecks(HealthCheckEndpoints.Liveness, liveOptions);
            configure.MapHealthChecks("/healthz", readyOptions);

            configure.MapHealthChecksUI(setupOptions =>
            {
                setupOptions.UIPath = string.IsNullOrEmpty(healthCheckUISettings.UIPath)
                    ? "/healthchecks-ui"
                    : healthCheckUISettings.UIPath;
                setupOptions.AddCustomStylesheet("Files/HealthChecks/CustomStyle.css");
            });

            configure.MapControllers();
        });

        return app;
    }

    private static IHealthChecksBuilder AddDatabase(this IHealthChecksBuilder builder, IConfiguration configuration)
    {
        var databaseSettings = configuration.GetRequiredSection(nameof(DatabaseSettings)).Get<DatabaseSettings>()!;
        if (databaseSettings is null)
        {
            return builder;
        }

        builder.AddSql(
            databaseSettings.SqlSettings.DbProvider,
            databaseSettings.SqlSettings.ConnectionStrings.DefaultConnection,
            HealthCheckTags.Sql,
            new[] { databaseSettings.SqlSettings.DbProvider, "ready" }
        );

        return builder;
    }

    private static IHealthChecksBuilder AddSql(
        this IHealthChecksBuilder builder,
        string dbProvider,
        string connectionString,
        string healthCheckName,
        string[] healthCheckTags = default!)
        => dbProvider switch
        {
            DbProviderKeys.PostgreSql => builder.AddNpgSql(connectionString, name: healthCheckName, tags: healthCheckTags),
            _ => builder,
        };
}
