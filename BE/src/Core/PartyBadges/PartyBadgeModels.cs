namespace HuyHieuDang.Core.PartyBadges;

/// <summary>
/// Cài đặt sinh dãy mốc huy hiệu (QT1). Mặc định 30 / 90 / 5.
/// </summary>
/// <param name="StartYears">Mốc đầu tiên, tính bằng năm tuổi đảng.</param>
/// <param name="EndYears">Mốc lớn nhất được phép.</param>
/// <param name="StepYears">Khoảng cách giữa hai mốc liên tiếp.</param>
public sealed record MilestoneSettings(int StartYears, int EndYears, int StepYears);

/// <summary>
/// Một đợt trao huy hiệu (QT6). Chỉ lưu ngày/tháng, dùng chung cho mọi năm.
/// </summary>
/// <param name="Name">Tên đợt, duy nhất trong hệ thống.</param>
/// <param name="FromDay">Ngày của Từ ngày.</param>
/// <param name="FromMonth">Tháng của Từ ngày.</param>
/// <param name="ToDay">Ngày của Đến ngày.</param>
/// <param name="ToMonth">Tháng của Đến ngày.</param>
public sealed record AwardPeriod(string Name, int FromDay, int FromMonth, int ToDay, int ToMonth)
{
    /// <summary>
    /// QT6 — đợt vắt qua 31/12. Khi cặp ngày/tháng của Đến ngày đứng trước Từ ngày trong năm
    /// (ví dụ 01/12 – 28/02) thì Đến ngày thuộc năm kế tiếp Từ ngày.
    /// </summary>
    public bool SpansNextYear =>
        FromMonth > ToMonth || (FromMonth == ToMonth && FromDay > ToDay);
}

/// <summary>Một đợt đã được gắn năm cụ thể.</summary>
/// <param name="Period">Đợt gốc.</param>
/// <param name="Year">Năm neo của lần diễn ra — năm chứa Từ ngày.</param>
/// <param name="From">Từ ngày sau khi gắn năm.</param>
/// <param name="To">Đến ngày sau khi gắn năm; thuộc <c>Year + 1</c> khi đợt vắt năm.</param>
public sealed record PeriodOccurrence(AwardPeriod Period, int Year, DateOnly From, DateOnly To);

/// <summary>
/// Phần của một lần diễn ra rơi vào trong một năm dương lịch (QT6, QT7). Đợt vắt năm để lại
/// hai phần trong cùng một năm: đuôi của lần neo năm trước và đầu của lần neo chính năm đó.
/// </summary>
/// <param name="Occurrence">Lần diễn ra sinh ra phần này.</param>
/// <param name="From">Ngày đầu, đã cắt về trong năm.</param>
/// <param name="To">Ngày cuối, đã cắt về trong năm.</param>
public sealed record PeriodSlice(PeriodOccurrence Occurrence, DateOnly From, DateOnly To)
{
    /// <summary>Đợt gốc của phần này.</summary>
    public AwardPeriod Period => Occurrence.Period;
}

/// <summary>Trạng thái một đợt so với hôm nay (QT11).</summary>
public enum PeriodStatus
{
    /// <summary>Đến ngày đã trôi qua.</summary>
    Past = 0,

    /// <summary>Hôm nay nằm trong khoảng của đợt.</summary>
    Ongoing = 1,

    /// <summary>Đợt chưa bắt đầu.</summary>
    Upcoming = 2,
}

/// <summary>Vị trí của một khoảng trống so với các đợt trong năm (QT7).</summary>
public enum GapKind
{
    /// <summary>Nằm trước đợt đầu tiên của năm.</summary>
    BeforeFirstPeriod = 0,

    /// <summary>Nằm giữa hai đợt liền kề.</summary>
    BetweenPeriods = 1,

    /// <summary>Nằm sau đợt cuối cùng của năm.</summary>
    AfterLastPeriod = 2,
}

/// <summary>Kết quả tính trạng thái đợt (QT11).</summary>
/// <param name="Status">Đã qua / Đang diễn ra / Sắp tới.</param>
/// <param name="DaysLeft">Số ngày còn lại tới Từ ngày; chỉ có giá trị khi đợt sắp tới.</param>
public sealed record PeriodStatusResult(PeriodStatus Status, int? DaysLeft);

/// <summary>Một khoảng ngày trong năm chưa được đợt nào phủ (QT6, QT7).</summary>
/// <param name="From">Ngày đầu khoảng trống.</param>
/// <param name="To">Ngày cuối khoảng trống.</param>
/// <param name="Kind">Vị trí của khoảng trống so với các đợt.</param>
/// <param name="Label">Nhãn hiển thị, ví dụ "Giữa Đợt 3/2 và Đợt 19/5".</param>
public sealed record DateGap(DateOnly From, DateOnly To, GapKind Kind, string Label);

/// <summary>Mốc tròn trong năm nhưng không rơi vào đợt nào (QT7).</summary>
/// <param name="Milestone">Mốc huy hiệu.</param>
/// <param name="Anniversary">Ngày tròn mốc.</param>
/// <param name="Gap">Khoảng trống chứa ngày tròn mốc.</param>
public sealed record MissedMilestone(int Milestone, DateOnly Anniversary, DateGap Gap);

/// <summary>Một cặp đợt chồng lấn nhau — cảnh báo nhưng vẫn cho lưu (QT6).</summary>
/// <param name="First">Đợt có Từ ngày sớm hơn.</param>
/// <param name="Second">Đợt còn lại.</param>
/// <param name="From">Ngày đầu của phần dùng chung, đã cắt về trong năm đang xét.</param>
/// <param name="To">Ngày cuối của phần dùng chung.</param>
public sealed record PeriodOverlap(AwardPeriod First, AwardPeriod Second, DateOnly From, DateOnly To);

/// <summary>Đợt sắp tới của Dashboard (QT8), kèm trạng thái của chính lần diễn ra đó.</summary>
/// <param name="Occurrence">Đợt đã gắn năm.</param>
/// <param name="Status">Trạng thái so với hôm nay.</param>
public sealed record UpcomingPeriod(PeriodOccurrence Occurrence, PeriodStatusResult Status);
