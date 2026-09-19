using HuyHieuDang.Infrastructure.Facades.Common;
using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Facades.Persistence.Initialization;
using HuyHieuDang.Infrastructure.Facades.Persistence.Interceptors;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace HuyHieuDang.Infrastructure.Facades.Persistence;

internal static class Startup
{
    private static readonly ILogger Logger = Log.ForContext(typeof(Startup));

    internal static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddOptions<DatabaseSettings>()
            .BindConfiguration(nameof(DatabaseSettings))
            .Configure(x => x.SqlSettings.ConnectionStrings.OverrideConnection())
            .ValidateDataAnnotationsRecursively()
            .ValidateOnStart();

        services.AddSingleton<UpdatedAtInterceptor>();

        services.AddDbContextPool<ApplicationDbContext>((provider, options) =>
        {
            DatabaseSettings databaseSettings = provider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
            Logger.Information("Current sql db provider: {dbProvider}", databaseSettings.SqlSettings.DbProvider);
            options.UseDatabase(databaseSettings.SqlSettings.DbProvider, databaseSettings.SqlSettings.ConnectionStrings.DefaultConnection);
            options.AddInterceptors(provider.GetRequiredService<UpdatedAtInterceptor>());
        });

        services.Scan(scan => scan
            .FromAssemblyOf<IDataSeedContributor>()
            .AddClasses(classes => classes.AssignableTo<IDataSeedContributor>())
                .AsImplementedInterfaces()
                .WithTransientLifetime()
        );

        services.AddTransient<IDbInitializer, DbInitializer>()
            .AddTransient<CustomSeederRunner>()
            .AddRepositories();

        return services;
    }

    internal static DbContextOptionsBuilder UseDatabase(this DbContextOptionsBuilder builder, string dbProvider, string connectionString)
    {
        return dbProvider switch
        {
            DbProviderKeys.PostgreSql => builder.UseNpgsql(connectionString, options => options.MigrationsAssembly($"HuyHieuDang.Migrators.{dbProvider}")),
            _ => throw new InvalidOperationException($"Persistence db provider '{dbProvider}' is not supported."),
        };
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepositoryWrapper), typeof(RepositoryWrapper));

        return services;
    }
}