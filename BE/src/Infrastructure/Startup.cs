using AutoMapper.EquivalencyExpression;
using HuyHieuDang.Infrastructure.Facades.Auth;
using HuyHieuDang.Infrastructure.Facades.Cache;
using HuyHieuDang.Infrastructure.Facades.Common;
using HuyHieuDang.Infrastructure.Facades.Common.HttpClients;
using HuyHieuDang.Infrastructure.Facades.Cors;
using HuyHieuDang.Infrastructure.Facades.HealthChecks;
using HuyHieuDang.Infrastructure.Facades.Identity;
using HuyHieuDang.Infrastructure.Facades.Mapping;
using HuyHieuDang.Infrastructure.Facades.Middleware;
using HuyHieuDang.Infrastructure.Facades.OpenAPI;
using HuyHieuDang.Infrastructure.Facades.Persistence;
using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Facades.Persistence.Initialization;
using HuyHieuDang.Infrastructure.Facades.Validations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace HuyHieuDang.Infrastructure;

public static class Startup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddFluentValidation()
            .AddAutoMapper(cfg => cfg.AddCollectionMappers(), typeof(MappingProfile))
            .AddAuth(configuration)
            .AddCorsPolicy(configuration)
            .AddHealthCheck(configuration)
            .AddOpenApiDocumentation(configuration)
            .AddPersistence()
            .AddCustomIdentity()
            .AddServices()
            .AddHttpClientSender()
            .AddCache()
            .AddSingleton<IActionContextAccessor, ActionContextAccessor>();

        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app, IConfiguration configuration)
    {
        app
            .UseStaticFiles()
            .UseRouting()
            .UseCorsPolicy()
            .UseExceptionHandlerMiddleware()
            .UseAuthentication()
            .UseCurrentUser()
            .UseVerifyJwtUserMiddleware()
            .UseAuthorization()
            .UseHealthCheck()
            .UseOpenApiDocumentation();

        return app;
    }

    public static async Task InitializeDatabasesAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = services.CreateScope();

        await HandleAutoMigrationAsync(scope.ServiceProvider, cancellationToken);
        await InitializeDatabaseAsync(scope.ServiceProvider, cancellationToken);
    }

    private static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        IDbInitializer dbInitializer = serviceProvider.GetRequiredService<IDbInitializer>();
        await dbInitializer.InitializeAsync(cancellationToken);
    }

    private static async Task HandleAutoMigrationAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        DatabaseSettings databaseSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;

        if (databaseSettings.SqlSettings.UseAutoMigration)
        {
            ApplicationDbContext dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if ((await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                await dbContext.Database.MigrateAsync(cancellationToken);
                Log.Information("---> [Migration] Auto-migration succeeded. All pending migrations applied.");
            }
            else
            {
                Log.Information("---> [Migration] No pending migrations found.");
            }
        }
        else
        {
            Log.Warning("---> [Migration] Auto Migration Database is disabled");
        }
    }
}