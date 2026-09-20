using HuyHieuDang.Core.Common.Interfaces;

namespace HuyHieuDang.Web.QcIntegrationTests.Support;

/// <summary>
/// Đồng hồ cố định của bộ kiểm thử QC (T-FIX-3). Ba mốc T0 / T1 / T2 của kế hoạch kiểm thử
/// nằm ngay đây để không ca nào phải viết lại ngày.
/// </summary>
public sealed class QcClock : IDateTimeProvider
{
    /// <summary>Mốc mặc định: 19/09/2026. Đợt sắp tới là Đợt 7/11, còn 12 ngày.</summary>
    public static readonly DateOnly T0 = new(2026, 9, 19);

    /// <summary>Mốc T1: 15/10/2026. Hôm nay nằm trong Đợt 7/11 (QT11 "Đang diễn ra").</summary>
    public static readonly DateOnly T1 = new(2026, 10, 15);

    /// <summary>Mốc T2: 01/12/2026. Mọi đợt của 2026 đã qua, đợt sắp tới thuộc 2027 (QT8).</summary>
    public static readonly DateOnly T2 = new(2026, 12, 1);

    private readonly DateTimeOffset now;

    /// <summary>Dựng đồng hồ đứng yên lúc 09:30 giờ Việt Nam của ngày đã cho.</summary>
    /// <param name="today">Ngày cần ép.</param>
    public QcClock(DateOnly today)
        : this(new DateTimeOffset(today.ToDateTime(new TimeOnly(9, 30)), TimeSpan.FromHours(7)))
    {
    }

    /// <summary>Dựng đồng hồ đứng yên tại đúng thời điểm đã cho, kể cả độ lệch múi giờ.</summary>
    /// <param name="now">Thời điểm cần ép.</param>
    public QcClock(DateTimeOffset now)
    {
        this.now = now;
    }

    /// <inheritdoc/>
    public DateTimeOffset Now => now;

    /// <inheritdoc/>
    public DateOnly Today => DateOnly.FromDateTime(now.DateTime);
}
