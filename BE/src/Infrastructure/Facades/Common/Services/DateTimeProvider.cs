using HuyHieuDang.Core.Common.Interfaces;

namespace HuyHieuDang.Infrastructure.Facades.Common.Services;

/// <summary>
/// Bản cài đặt thật của <see cref="IDateTimeProvider"/>: đọc đồng hồ UTC của máy rồi tự quy
/// về múi giờ Asia/Ho_Chi_Minh. Không dựa vào biến môi trường <c>TZ</c> nên chạy ở múi giờ
/// nào cũng cho cùng một kết quả (T-FIX-2, ca A-902).
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

    private static readonly TimeZoneInfo VietnamTimeZone = ResolveVietnamTimeZone();

    /// <inheritdoc/>
    public DateTimeOffset Now => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, VietnamTimeZone);

    /// <inheritdoc/>
    public DateOnly Today => DateOnly.FromDateTime(Now.DateTime);

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
