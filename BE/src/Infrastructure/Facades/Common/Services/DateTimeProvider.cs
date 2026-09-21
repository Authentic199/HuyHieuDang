using System.Globalization;
using HuyHieuDang.Core.Common.Interfaces;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace HuyHieuDang.Infrastructure.Facades.Common.Services;

/// <summary>
/// Bản cài đặt thật của <see cref="IDateTimeProvider"/>: đọc đồng hồ UTC của máy rồi tự quy
/// về múi giờ Asia/Ho_Chi_Minh. Không dựa vào biến môi trường <c>TZ</c> nên chạy ở múi giờ
/// nào cũng cho cùng một kết quả (T-FIX-2, ca A-902).
/// <para>
/// Ngoài Production, biến môi trường <see cref="TestTodayVariable"/> ép được "hôm nay" cho cả
/// tiến trình (T-FIX-4) để bộ kiểm thử đầu-cuối chạy trên một ngày cố định. Ở Production biến
/// bị bỏ qua kèm một dòng cảnh báo — không được mở đường đổi ngày trên máy chủ thật (A-903).
/// </para>
/// </summary>
public sealed class DateTimeProvider : IDateTimeProvider
{
    /// <summary>
    /// Định danh IANA của múi giờ Việt Nam.
    /// </summary>
    public const string VietnamTimeZoneId = "Asia/Ho_Chi_Minh";

    /// <summary>
    /// Định danh Windows tương đương, dùng khi máy chạy không có cơ sở dữ liệu múi giờ IANA.
    /// </summary>
    public const string VietnamWindowsTimeZoneId = "SE Asia Standard Time";

    /// <summary>
    /// Biến môi trường ép "hôm nay", dạng <c>yyyy-MM-dd</c>. Chỉ dùng cho kiểm thử.
    /// </summary>
    public const string TestTodayVariable = "HUYHIEUDANG_TEST_TODAY";

    /// <summary>
    /// Định dạng bắt buộc của <see cref="TestTodayVariable"/>.
    /// </summary>
    public const string TestTodayFormat = "yyyy-MM-dd";

    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

    private readonly DateOnly? forcedToday;

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeProvider"/> class dùng cho bộ chứa
    /// phụ thuộc: lấy tên môi trường từ host rồi đọc biến môi trường ép ngày.
    /// </summary>
    /// <param name="environment">Môi trường đang chạy, để chặn ép ngày ở Production.</param>
    public DateTimeProvider(IHostEnvironment environment)
        : this(environment?.EnvironmentName, Environment.GetEnvironmentVariable(TestTodayVariable))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeProvider"/> class chạy theo đồng hồ
    /// thật, không ép ngày.
    /// </summary>
    public DateTimeProvider()
        : this(environmentName: null, testTodayValue: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DateTimeProvider"/> class với hai giá trị
    /// truyền thẳng, để bài kiểm thử đơn vị dựng được mọi tổ hợp môi trường / giá trị biến.
    /// </summary>
    /// <param name="environmentName">Tên môi trường; <c>Production</c> thì bỏ qua ép ngày.</param>
    /// <param name="testTodayValue">Giá trị thô của biến môi trường ép ngày.</param>
    public DateTimeProvider(string? environmentName, string? testTodayValue)
    {
        forcedToday = ResolveForcedToday(environmentName, testTodayValue);
    }

    /// <inheritdoc/>
    public DateTimeOffset Now
    {
        get
        {
            DateTimeOffset now = RealNow;

            // Giữ nguyên giờ thật để dấu thời gian vẫn nhích theo đồng hồ, chỉ thay phần ngày.
            return forcedToday is null
                ? now
                : new DateTimeOffset(forcedToday.Value.ToDateTime(TimeOnly.FromTimeSpan(now.TimeOfDay)), now.Offset);
        }
    }

    /// <inheritdoc/>
    public DateOnly Today => forcedToday ?? DateOnly.FromDateTime(RealNow.DateTime);

    private static DateTimeOffset RealNow => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, VietnamTimeZone);

    private static DateOnly? ResolveForcedToday(string? environmentName, string? testTodayValue)
    {
        if (string.IsNullOrWhiteSpace(testTodayValue))
        {
            return null;
        }

        if (string.Equals(environmentName, Environments.Production, StringComparison.OrdinalIgnoreCase))
        {
            Log.Warning(
                "Bỏ qua biến {variable} vì môi trường là Production; hệ thống dùng ngày thật.",
                TestTodayVariable);
            return null;
        }

        if (!DateOnly.TryParseExact(
                testTodayValue.Trim(),
                TestTodayFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateOnly parsed))
        {
            Log.Warning(
                "Biến {variable} không đúng định dạng {format} nên bị bỏ qua; hệ thống dùng ngày thật.",
                TestTodayVariable,
                TestTodayFormat);
            return null;
        }

        Log.Warning(
            "Biến {variable} đang ép hôm nay thành {today}. Chỉ dùng cho kiểm thử, không bật ở máy chủ thật.",
            TestTodayVariable,
            parsed.ToString(TestTodayFormat, CultureInfo.InvariantCulture));

        return parsed;
    }

    private static TimeZoneInfo ResolveVietnamTimeZone()
    {
        foreach (string id in new[] { VietnamTimeZoneId, VietnamWindowsTimeZoneId })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (Exception exception) when (exception is TimeZoneNotFoundException or InvalidTimeZoneException)
            {
                // Thử định danh kế tiếp.
            }
        }

        // Việt Nam không dùng giờ mùa hè từ năm 1975, nên độ lệch cố định +07:00 là an toàn.
        return TimeZoneInfo.CreateCustomTimeZone(VietnamTimeZoneId, TimeSpan.FromHours(7), VietnamTimeZoneId, VietnamTimeZoneId);
    }
}
