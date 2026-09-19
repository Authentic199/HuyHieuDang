using HuyHieuDang.Core.PartyBadges;
using PeriodEntity = HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities.AwardPeriod;

namespace HuyHieuDang.Infrastructure.Modules.AwardPeriods.Responses;

/// <summary>
/// Một đợt trao huy hiệu đã gắn năm đang xét (mục 1.10 hợp đồng API). Trạng thái và số người
/// đủ điều kiện là giá trị tính lại mỗi lần gọi, không lưu vào bảng (QT5).
/// </summary>
public class AwardPeriodResponse
{
    /// <summary>
    /// Khóa chính của đợt.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tên đợt.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Ngày của Từ ngày.
    /// </summary>
    public int FromDay { get; set; }

    /// <summary>
    /// Tháng của Từ ngày.
    /// </summary>
    public int FromMonth { get; set; }

    /// <summary>
    /// Ngày của Đến ngày.
    /// </summary>
    public int ToDay { get; set; }

    /// <summary>
    /// Tháng của Đến ngày.
    /// </summary>
    public int ToMonth { get; set; }

    /// <summary>
    /// Từ ngày dạng <c>dd/MM</c>.
    /// </summary>
    public string FromDisplay { get; set; } = default!;

    /// <summary>
    /// Đến ngày dạng <c>dd/MM</c>.
    /// </summary>
    public string ToDisplay { get; set; } = default!;

    /// <summary>
    /// Năm đang xét.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Từ ngày đã gắn năm; 29/02 ở năm không nhuận lùi về 28/02.
    /// </summary>
    public DateOnly FromDate { get; set; }

    /// <summary>
    /// Đến ngày đã gắn năm.
    /// </summary>
    public DateOnly ToDate { get; set; }

    /// <summary>
    /// Trạng thái của đợt so với hôm nay (QT11).
    /// </summary>
    public PeriodStatus Status { get; set; }

    /// <summary>
    /// Số ngày còn lại tới Từ ngày; chỉ khác <see langword="null"/> khi đợt sắp tới.
    /// </summary>
    public int? DaysRemaining { get; set; }

    /// <summary>
    /// Số người đủ điều kiện của đợt trong <see cref="Year"/> (QT4).
    /// </summary>
    public int EligibleCount { get; set; }

    /// <summary>
    /// Dựng phản hồi từ bản ghi và các giá trị đã tính sẵn bằng service thuần của T07.
    /// </summary>
    /// <param name="entity">Bản ghi đợt.</param>
    /// <param name="occurrence">Đợt đã gắn năm.</param>
    /// <param name="status">Trạng thái theo QT11.</param>
    /// <param name="eligibleCount">Số người đủ điều kiện trong năm.</param>
    /// <returns>Phản hồi đúng hình dạng hợp đồng API.</returns>
    public static AwardPeriodResponse From(
        PeriodEntity entity, PeriodOccurrence occurrence, PeriodStatusResult status, int eligibleCount)
        => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            FromDay = entity.FromDay,
            FromMonth = entity.FromMonth,
            ToDay = entity.ToDay,
            ToMonth = entity.ToMonth,
            FromDisplay = Display(entity.FromDay, entity.FromMonth),
            ToDisplay = Display(entity.ToDay, entity.ToMonth),
            Year = occurrence.Year,
            FromDate = occurrence.From,
            ToDate = occurrence.To,
            Status = status.Status,
            DaysRemaining = status.DaysLeft,
            EligibleCount = eligibleCount,
        };

    /// <summary>
    /// Ghép một cặp ngày/tháng thành chuỗi <c>dd/MM</c>.
    /// </summary>
    /// <param name="day">Ngày.</param>
    /// <param name="month">Tháng.</param>
    /// <returns>Chuỗi hai số ngăn bởi dấu gạch chéo.</returns>
    public static string Display(int day, int month) => $"{day:D2}/{month:D2}";

    /// <summary>
    /// Ghép một ngày đã gắn năm thành chuỗi <c>dd/MM</c>.
    /// </summary>
    /// <param name="date">Ngày cần hiển thị.</param>
    /// <returns>Chuỗi hai số ngăn bởi dấu gạch chéo.</returns>
    public static string Display(DateOnly date) => Display(date.Day, date.Month);
}
