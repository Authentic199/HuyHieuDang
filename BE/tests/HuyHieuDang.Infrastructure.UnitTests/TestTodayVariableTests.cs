using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure.Facades.Common.Services;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace HuyHieuDang.Infrastructure.UnitTests;

/// <summary>
/// A-903 của kế hoạch kiểm thử (T-FIX-4) kiểm **hành vi**, không kiểm sự vắng mặt của một chuỗi:
/// đặt <c>HUYHIEUDANG_TEST_TODAY</c> khi <c>ASPNETCORE_ENVIRONMENT=Production</c> thì biến phải
/// bị bỏ qua kèm log mức <c>Warning</c>. Lớp này đi qua đúng đường mà bộ chứa phụ thuộc dùng —
/// hàm khởi tạo nhận <see cref="IHostEnvironment"/> và đọc biến thật của tiến trình — chứ không
/// đi tắt qua hàm khởi tạo nhận chuỗi.
/// </summary>
/// <remarks>
/// Lớp này sửa biến môi trường của tiến trình và <see cref="Log.Logger"/> dùng chung nên phải
/// chạy một mình; <see cref="ProcessWideCollection"/> tách nó khỏi mọi lớp khác.
/// </remarks>
[Collection(ProcessWideCollection.Name)]
public sealed class TestTodayVariableTests : IDisposable
{
    private const string ForcedDay = "2026-10-15";

    private readonly ILogger previousLogger = Log.Logger;

    /// <inheritdoc/>
    public void Dispose()
    {
        Environment.SetEnvironmentVariable(DateTimeProvider.TestTodayVariable, null);
        Log.Logger = previousLogger;
    }

    [Fact]
    public void OutsideProduction_TheVariableForcesToday()
    {
        CaptureSink sink = Arrange();

        IDateTimeProvider provider = new DateTimeProvider(HostEnvironment(Environments.Development));

        Assert.Equal(new DateOnly(2026, 10, 15), provider.Today);
        Assert.Equal(new DateOnly(2026, 10, 15), DateOnly.FromDateTime(provider.Now.DateTime));
        Assert.Contains(sink.Warnings, message => message.Contains(DateTimeProvider.TestTodayVariable, StringComparison.Ordinal));
    }

    [Fact]
    public void InProduction_TheVariableIsIgnoredAndWarned()
    {
        CaptureSink sink = Arrange();

        IDateTimeProvider forced = new DateTimeProvider(HostEnvironment(Environments.Production));
        IDateTimeProvider real = new DateTimeProvider();

        Assert.Equal(real.Today, forced.Today);
        Assert.NotEqual(new DateOnly(2026, 10, 15), forced.Today);
        Assert.Contains(sink.Warnings, message => message.Contains(DateTimeProvider.TestTodayVariable, StringComparison.Ordinal));
    }

    [Fact]
    public void WithoutTheVariable_NothingIsForcedAndNothingIsWarned()
    {
        CaptureSink sink = new();
        Environment.SetEnvironmentVariable(DateTimeProvider.TestTodayVariable, null);
        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Sink(sink).CreateLogger();

        IDateTimeProvider provider = new DateTimeProvider(HostEnvironment(Environments.Development));
        IDateTimeProvider real = new DateTimeProvider();

        Assert.Equal(real.Today, provider.Today);
        Assert.Empty(sink.Warnings);
    }

    private static IHostEnvironment HostEnvironment(string environmentName)
        => new FakeHostEnvironment { EnvironmentName = environmentName };

    private static CaptureSink Arrange()
    {
        CaptureSink sink = new();

        Environment.SetEnvironmentVariable(DateTimeProvider.TestTodayVariable, ForcedDay);
        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Sink(sink).CreateLogger();

        return sink;
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "HuyHieuDang.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; }
            = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }

    private sealed class CaptureSink : ILogEventSink
    {
        private readonly List<string> warnings = new();

        public IReadOnlyList<string> Warnings
        {
            get
            {
                lock (warnings)
                {
                    return warnings.ToList();
                }
            }
        }

        public void Emit(LogEvent logEvent)
        {
            ArgumentNullException.ThrowIfNull(logEvent);

            if (logEvent.Level >= LogEventLevel.Warning)
            {
                lock (warnings)
                {
                    warnings.Add(logEvent.RenderMessage());
                }
            }
        }
    }
}

/// <summary>
/// Bộ dành cho những lớp kiểm thử phải sửa trạng thái dùng chung của cả tiến trình
/// (biến môi trường, logger tĩnh) nên không chạy song song với bất kỳ lớp nào khác.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ProcessWideCollection
{
    /// <summary>Tên bộ.</summary>
    public const string Name = "ProcessWide";
}
