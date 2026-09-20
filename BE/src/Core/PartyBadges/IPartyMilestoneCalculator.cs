using HuyHieuDang.Core.Common.Interfaces;

namespace HuyHieuDang.Core.PartyBadges;

/// <summary>
/// Service thuần tính mốc tuổi đảng (QT1–QT11). Không phụ thuộc DbContext, không đọc đồng hồ
/// hệ thống: "hôm nay" luôn được truyền vào qua tham số <c>today</c>.
/// </summary>
public interface IPartyMilestoneCalculator : IScopedService
{
    /// <summary>QT1 — sinh dãy mốc huy hiệu từ cài đặt.</summary>
    /// <param name="settings">Cài đặt Bắt đầu / Kết thúc / Bước.</param>
    /// <returns>Dãy mốc tăng dần.</returns>
    IReadOnlyList<int> BuildMilestones(MilestoneSettings settings);

    /// <summary>QT2 — ngày tròn mốc: ngày chính thức cộng thêm N năm.</summary>
    /// <param name="officialAdmissionDate">Ngày vào Đảng chính thức.</param>
    /// <param name="milestone">Số năm cần cộng.</param>
    /// <returns>Ngày tròn mốc; 29/02 ở năm đích không nhuận lùi về 28/02.</returns>
    DateOnly GetAnniversary(DateOnly officialAdmissionDate, int milestone);

    /// <summary>QT3 — tuổi đảng hiện tại tính theo năm tròn.</summary>
    /// <param name="officialAdmissionDate">Ngày vào Đảng chính thức.</param>
    /// <param name="today">Ngày hôm nay.</param>
    /// <returns>Số năm tròn đã qua hoặc đúng ngày kỷ niệm.</returns>
    int GetPartyAge(DateOnly officialAdmissionDate, DateOnly today);

    /// <summary>QT3a — mốc nhỏ nhất lớn hơn tuổi đảng hiện tại.</summary>
    /// <param name="officialAdmissionDate">Ngày vào Đảng chính thức.</param>
    /// <param name="today">Ngày hôm nay.</param>
    /// <param name="milestones">Dãy mốc của QT1.</param>
    /// <returns>Mốc kế tiếp, hoặc <c>null</c> khi đã vượt mốc lớn nhất.</returns>
    int? GetNextMilestone(DateOnly officialAdmissionDate, DateOnly today, IReadOnlyList<int> milestones);

    /// <summary>QT3a — ngày tròn mốc kế tiếp.</summary>
    /// <param name="officialAdmissionDate">Ngày vào Đảng chính thức.</param>
    /// <param name="today">Ngày hôm nay.</param>
    /// <param name="milestones">Dãy mốc của QT1.</param>
    /// <returns>Ngày tròn mốc kế tiếp, hoặc <c>null</c> khi đã vượt mốc lớn nhất.</returns>
    DateOnly? GetNextAnniversary(DateOnly officialAdmissionDate, DateOnly today, IReadOnlyList<int> milestones);

    /// <summary>Gắn một năm cụ thể vào đợt (QT4).</summary>
    /// <param name="period">Đợt chỉ có ngày/tháng.</param>
    /// <param name="year">Năm neo — năm chứa Từ ngày.</param>
    /// <returns>
    /// Đợt đã có ngày đầy đủ; 29/02 ở năm không nhuận lùi về 28/02. Đợt vắt năm (QT6) có
    /// Đến ngày thuộc <paramref name="year"/> + 1.
    /// </returns>
    PeriodOccurrence BindToYear(AwardPeriod period, int year);

    /// <summary>QT6, QT7 — phần của các đợt rơi vào trong một năm dương lịch.</summary>
    /// <param name="periods">Toàn bộ đợt đang cấu hình.</param>
    /// <param name="year">Năm đang xét.</param>
    /// <returns>
    /// Các phần đã cắt về 01/01–31/12 của năm, sắp theo ngày bắt đầu. Một đợt vắt năm góp hai
    /// phần: đuôi của lần neo năm trước và đầu của lần neo chính năm đó.
    /// </returns>
    IReadOnlyList<PeriodSlice> GetSlicesInYear(IReadOnlyList<AwardPeriod> periods, int year);

    /// <summary>QT4 — mốc được trao cho một đảng viên trong một đợt của một năm.</summary>
    /// <param name="officialAdmissionDate">Ngày vào Đảng chính thức.</param>
    /// <param name="period">Đợt trao huy hiệu.</param>
    /// <param name="year">Năm xét.</param>
    /// <param name="milestones">Dãy mốc của QT1.</param>
    /// <returns>Mốc được trao, hoặc <c>null</c> khi không đủ điều kiện.</returns>
    int? GetEligibleMilestone(
        DateOnly officialAdmissionDate, AwardPeriod period, int year, IReadOnlyList<int> milestones);

    /// <summary>QT7 — mốc tròn trong năm nhưng rơi ra ngoài mọi đợt.</summary>
    /// <param name="officialAdmissionDate">Ngày vào Đảng chính thức.</param>
    /// <param name="periods">Toàn bộ đợt đang cấu hình.</param>
    /// <param name="year">Năm xét.</param>
    /// <param name="milestones">Dãy mốc của QT1.</param>
    /// <returns>Mốc bị bỏ lỡ kèm khoảng trống chứa nó, hoặc <c>null</c>.</returns>
    MissedMilestone? GetMissedMilestone(
        DateOnly officialAdmissionDate, IReadOnlyList<AwardPeriod> periods, int year, IReadOnlyList<int> milestones);

    /// <summary>QT6 — các khoảng 01/01–31/12 chưa được đợt nào phủ.</summary>
    /// <param name="periods">Toàn bộ đợt đang cấu hình.</param>
    /// <param name="year">Năm xét.</param>
    /// <returns>Các khoảng trống theo thứ tự thời gian.</returns>
    IReadOnlyList<DateGap> GetGaps(IReadOnlyList<AwardPeriod> periods, int year);

    /// <summary>QT6 — các cặp đợt chồng lấn nhau.</summary>
    /// <param name="periods">Toàn bộ đợt đang cấu hình.</param>
    /// <param name="year">Năm xét.</param>
    /// <returns>Các cặp đợt có ngày dùng chung.</returns>
    IReadOnlyList<PeriodOverlap> GetOverlaps(IReadOnlyList<AwardPeriod> periods, int year);

    /// <summary>QT8 — đợt sắp tới của Dashboard.</summary>
    /// <param name="periods">Toàn bộ đợt đang cấu hình.</param>
    /// <param name="today">Ngày hôm nay.</param>
    /// <returns>Đợt sắp tới, hoặc <c>null</c> khi chưa cài đợt nào.</returns>
    UpcomingPeriod? GetUpcomingPeriod(IReadOnlyList<AwardPeriod> periods, DateOnly today);

    /// <summary>QT11 — trạng thái của đợt trong năm hiện tại.</summary>
    /// <param name="period">Đợt trao huy hiệu.</param>
    /// <param name="today">Ngày hôm nay.</param>
    /// <returns>Trạng thái kèm số ngày còn lại khi đợt sắp tới.</returns>
    PeriodStatusResult GetPeriodStatus(AwardPeriod period, DateOnly today);

    /// <summary>QT11 — trạng thái của đợt trong một năm bất kỳ, vẫn so với hôm nay.</summary>
    /// <param name="period">Đợt trao huy hiệu.</param>
    /// <param name="year">Năm đang xét, có thể khác năm của <paramref name="today"/>.</param>
    /// <param name="today">Ngày hôm nay.</param>
    /// <returns>Trạng thái kèm số ngày còn lại khi đợt sắp tới.</returns>
    PeriodStatusResult GetPeriodStatus(AwardPeriod period, int year, DateOnly today);
}
