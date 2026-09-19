using Serilog.Events;

namespace HuyHieuDang.Infrastructure.Facades.Logging;

public class LoggerSettings
{
    public LoggerWriteToConsole WriteToConsole { get; set; } = new();

    public LoggerWriteToElasticsearch WriteToElasticsearch { get; set; } = new();

    public string MinimumLogLevel { get; set; } = "Information";
}

public class LoggerWriteToConsole
{
    public bool StructuredConsoleLogging { get; set; }

    public string? Tag { get; set; }
}

public class LoggerWriteToElasticsearch
{
    public string ServerUrl { get; set; } = string.Empty;

    public LogEventLevel MinimumLogEventLevel { get; set; }

    public LoggerWriteToElasticsearchCredentials Credentials { get; set; } = new();

    /// <summary>
    /// Maximum size limit for each buffer file (in bytes)
    /// </summary>
    public int BufferFileSizeLimitBytes { get; set; }

    /// <summary>
    /// Maximum number of buffer files allowed
    /// </summary>
    public int BufferFileCountLimit { get; set; }

    /// <summary>
    /// Maximum number of retained files allowed
    /// </summary>
    public int RetainedFileCountLimit { get; set; }

    /// <summary>
    /// Base folder to store logs (e.g., "./Files/logs")
    /// </summary>
    public string BaseFolder { get; set; } = "./Files/logs";

    /// <summary>
    /// Relative path for buffer file, appended to BaseFolder
    /// </summary>
    public string BufferRelativePath { get; set; } = "elastic-buffer";

    /// <summary>
    /// Relative path for fallback file, appended to BaseFolder
    /// </summary>
    public string FallbackRelativePath { get; set; } = "es-fallback.log";
}

public class LoggerWriteToElasticsearchCredentials
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}