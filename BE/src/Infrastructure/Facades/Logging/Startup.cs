using Figgle;
using Humanizer;
using HuyHieuDang.Infrastructure.Facades.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Elasticsearch;
using System.Diagnostics;

namespace HuyHieuDang.Infrastructure.Facades.Logging;

public static class Startup
{
    public static WebApplicationBuilder RegisterSerilog(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<ApplicationInfos>()
            .BindConfiguration(nameof(ApplicationInfos))
            .ValidateDataAnnotationsRecursively()
            .ValidateOnStart();

        builder.Services.AddOptions<LoggerSettings>()
            .BindConfiguration(nameof(LoggerSettings))
            .ValidateDataAnnotationsRecursively()
            .ValidateOnStart();

        LoggerSettings loggerSettings = builder.Configuration.GetRequiredSection(nameof(LoggerSettings)).Get<LoggerSettings>()!;

        builder.Host.UseSerilog((_, provider, loggerConfiguration) =>
        {
            ApplicationInfos applicationInfos = provider.GetRequiredService<IOptions<ApplicationInfos>>().Value;
            string applicationName = applicationInfos.Name;
            ConfigureEnrichers(loggerConfiguration, applicationName);
            ConfigureConsoleLogging(loggerConfiguration, loggerSettings.WriteToConsole);
            ConfigureElasticSearch(builder, loggerConfiguration, loggerSettings, applicationName);
            SetMinimumLogLevel(loggerConfiguration, loggerSettings.MinimumLogLevel);
            OverideMinimumLogLevel(loggerConfiguration);
            Console.WriteLine(FiggleFonts.Standard.Render(applicationName));
        });

        return builder;
    }

    private static void ConfigureEnrichers(LoggerConfiguration loggerConfiguration, string applicationName)
    {
        loggerConfiguration
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", applicationName)
            .Enrich.WithExceptionDetails()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.FromLogContext();
    }

    private static void ConfigureConsoleLogging(
        LoggerConfiguration loggerConfiguration,
        LoggerWriteToConsole consoleSettings
    )
    {
        if (consoleSettings.StructuredConsoleLogging)
        {
            loggerConfiguration.WriteTo.Async(wt => wt.Console(new CompactJsonFormatter()));
        }
        else
        {
            string outputTemplate;

            if (!string.IsNullOrEmpty(consoleSettings.Tag))
            {
                outputTemplate = $"[{{Timestamp:HH:mm:ss}} {{Level:u3}}] {consoleSettings.Tag} {{Message:lj}}{{NewLine}}{{Exception}}";
            }
            else
            {
                outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
            }

            loggerConfiguration.WriteTo.Async(wt => wt.Console(outputTemplate: outputTemplate));
        }
    }

    private static void ConfigureElasticSearch(
    WebApplicationBuilder builder,
    LoggerConfiguration loggerConfiguration,
    LoggerSettings loggerSetting,
    string applicationName
)
    {
        if (!string.IsNullOrEmpty(loggerSetting.WriteToElasticsearch.ServerUrl))
        {
            string indexFormat = $"{applicationName.Kebaberize()}-logs-{builder.Environment.EnvironmentName.ToLower()}-{DateTime.UtcNow:yyyy-MM}";

            // Build buffer & fallback paths
            var baseFolder = loggerSetting.WriteToElasticsearch.BaseFolder ?? "./Files/logs";

            // Ensure base folder exists
            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }

            var esOptions = new ElasticsearchSinkOptions(new Uri(loggerSetting.WriteToElasticsearch.ServerUrl))
            {
                AutoRegisterTemplate = true,
                IndexFormat = indexFormat,
                ModifyConnectionSettings = x => x.BasicAuthentication(
                    loggerSetting.WriteToElasticsearch.Credentials.Username,
                    loggerSetting.WriteToElasticsearch.Credentials.Password
                ),
                MinimumLogEventLevel = loggerSetting.WriteToElasticsearch.MinimumLogEventLevel,
            };

            if (!string.IsNullOrEmpty(loggerSetting.WriteToElasticsearch.BufferRelativePath))
            {
                esOptions.BufferBaseFilename = Path.Combine(baseFolder, loggerSetting.WriteToElasticsearch.BufferRelativePath);
                esOptions.BufferFileSizeLimitBytes = loggerSetting.WriteToElasticsearch.BufferFileSizeLimitBytes;
                esOptions.BufferFileCountLimit = loggerSetting.WriteToElasticsearch.BufferFileCountLimit;
            }

            var fallbackFilePath = Path.Combine(baseFolder, loggerSetting.WriteToElasticsearch.FallbackRelativePath);
            if (!string.IsNullOrEmpty(loggerSetting.WriteToElasticsearch.FallbackRelativePath))
            {
                esOptions.EmitEventFailure = EmitEventFailureHandling.WriteToFailureSink
                                      | EmitEventFailureHandling.WriteToSelfLog
                                      | EmitEventFailureHandling.RaiseCallback;
                var fallbackLoggerConfig = new LoggerConfiguration()
                    .WriteTo.File(
                        new Serilog.Formatting.Json.JsonFormatter(),
                        fallbackFilePath,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: loggerSetting.WriteToElasticsearch.RetainedFileCountLimit
                    );

                esOptions.FailureSink = fallbackLoggerConfig.CreateLogger();
                esOptions.FailureCallback = e => Console.WriteLine("Failed to write log to Elasticsearch: " + e.MessageTemplate);

                // Enable SelfLog
                Serilog.Debugging.SelfLog.Enable(File.CreateText(Path.Combine(baseFolder, "selflog.txt")));
            }

            loggerConfiguration.WriteTo.Elasticsearch(esOptions)
                               .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName!);
        }
    }

    private static void SetMinimumLogLevel(LoggerConfiguration loggerConfiguration, string minimumLevel)
    {
        switch (minimumLevel.ToLower().Camelize())
        {
            case nameof(LogEventLevel.Debug):
                loggerConfiguration.MinimumLevel.Debug();
                break;

            case nameof(LogEventLevel.Information):
                loggerConfiguration.MinimumLevel.Information();
                break;

            case nameof(LogEventLevel.Warning):
                loggerConfiguration.MinimumLevel.Warning();
                break;

            default:
                loggerConfiguration.MinimumLevel.Information();
                break;
        }
    }

    private static void OverideMinimumLogLevel(LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Hangfire", LogEventLevel.Warning)
            .MinimumLevel.Override("Elastic.Apm", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Error);

        if (Debugger.IsAttached)
        {
            loggerConfiguration.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information);
        }
    }
}