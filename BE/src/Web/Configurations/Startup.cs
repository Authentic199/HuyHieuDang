namespace HuyHieuDang.Web.Configurations;

internal static class Startup
{
    internal static WebApplicationBuilder AddConfigurations(this WebApplicationBuilder builder)
    {
        string environmentName = builder.Environment.EnvironmentName;
        builder.Configuration
                .AddJsonFiles(environmentName, "appsettings")
                .AddJsonFiles(environmentName, "logger")
                .AddJsonFiles(environmentName, "healthcheck")
                .AddJsonFiles(environmentName, "openapi")
                .AddJsonFiles(environmentName, "cors")
                .AddJsonFiles(environmentName, "security")
                .AddJsonFiles(environmentName, "database")
                .AddJsonFiles(environmentName, "httpclient")
                .AddJsonFiles(environmentName, "cache")
                .AddEnvironmentVariables();

        return builder;
    }

    private static IConfigurationBuilder AddJsonFiles(this IConfigurationBuilder builder, string environmentName, string fileName)
    {
        const string configurationsDirectory = "Configurations";

        return builder
            .AddJsonFile($"{configurationsDirectory}/{fileName}.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"{configurationsDirectory}/{fileName}.{environmentName}.json", optional: true, reloadOnChange: true);
    }
}