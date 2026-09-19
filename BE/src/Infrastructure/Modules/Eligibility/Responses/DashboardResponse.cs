using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Responses;

namespace HuyHieuDang.Infrastructure.Modules.Eligibility.Responses;

/// <summary>
/// Phần <c>data</c> của <c>GET /api/Dashboard</c> (mục 6.1 hợp đồng API, UC-10 → UC-13).
/// Một lời gọi đủ dựng cả màn hình, kể cả các trạng thái trống.
/// </summary>
[MessageDisplay("Dashboard")]
public class DashboardResponse
{
    /// <summary>
    /// Hôm nay theo lịch máy chủ.
    /// </summary>
    public DateOnly Today { get; set; }

    /// <summary>
    /// Năm hiện tại theo lịch máy chủ.
    /// </summary>
    public int CurrentYear { get; set; }

    /// <summary>
    /// Tên đơn vị trong cài đặt; <see langword="null"/> khi chưa đặt.
    /// </summary>
    public string? UnitName { get; set; }

    /// <summary>
    /// Tổng số đảng viên đang có.
    /// </summary>
    public int MemberCount { get; set; }

    /// <summary>
    /// Tổng số đợt đang cấu hình.
    /// </summary>
    public int PeriodCount { get; set; }

    /// <summary>
    /// Đợt sắp tới (QT8); <see langword="null"/> khi chưa cài đợt nào.
    /// </summary>
    public UpcomingPeriodResponse? UpcomingPeriod { get; set; }

    /// <summary>
    /// Danh sách đủ điều kiện của đợt sắp tới; rỗng khi chưa có đợt nào.
    /// </summary>
    public IReadOnlyList<EligibleMemberResponse> EligibleMembers { get; set; } = Array.Empty<EligibleMemberResponse>();

    /// <summary>
    /// Các cảnh báo hiển thị dưới dạng banner.
    /// </summary>
    public DashboardWarningsResponse Warnings { get; set; } = new();
}

/// <summary>
/// Đợt sắp tới của Dashboard (QT8), đã gắn đúng năm của lần diễn ra được chọn.
/// </summary>
public class UpcomingPeriodResponse
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
    /// Năm của lần diễn ra được chọn.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// <see langword="true"/> khi mọi đợt của năm nay đã qua và đợt sắp tới thuộc năm sau.
    /// </summary>
    public bool IsNextYear { get; set; }

    /// <summary>
    /// Từ ngày đã gắn <see cref="Year"/>.
    /// </summary>
    public DateOnly FromDate { get; set; }

    /// <summary>
    /// Đến ngày đã gắn <see cref="Year"/>.
    /// </summary>
    public DateOnly ToDate { get; set; }

    /// <summary>
    /// Từ ngày dạng <c>dd/MM</c>.
    /// </summary>
    public string FromDisplay { get; set; } = default!;

    /// <summary>
    /// Đến ngày dạng <c>dd/MM</c>.
    /// </summary>
    public string ToDisplay { get; set; } = default!;

    /// <summary>
    /// Trạng thái so với hôm nay (QT11).
    /// </summary>
    public PeriodStatus Status { get; set; }

    /// <summary>
    /// Số ngày còn lại tới Từ ngày; <see langword="null"/> khi đợt đang diễn ra.
    /// </summary>
    public int? DaysRemaining { get; set; }

    /// <summary>
    /// Số người đủ điều kiện của lần diễn ra này (QT4).
    /// </summary>
    public int EligibleCount { get; set; }

    /// <summary>
    /// Phân bổ theo mốc huy hiệu.
    /// </summary>
    public IReadOnlyList<MilestoneBreakdownResponse> MilestoneBreakdown { get; set; }
        = Array.Empty<MilestoneBreakdownResponse>();
}

/// <summary>
/// Các cảnh báo của Dashboard: hai trạng thái trống, số người bị sót trong năm nay và
/// cảnh báo độ phủ của năm nay (QT6, QT7).
/// </summary>
public class DashboardWarningsResponse
{
    /// <summary>
    /// Chưa có đảng viên nào — giao diện hiện khối hướng dẫn ba bước (UC-13).
    /// </summary>
    public bool NoMembers { get; set; }

    /// <summary>
    /// Chưa cài đợt trao huy hiệu nào.
    /// </summary>
    public bool NoPeriods { get; set; }

    /// <summary>
    /// Năm được đếm cho <see cref="UnassignedCount"/> — luôn là năm hiện tại.
    /// </summary>
    public int UnassignedYear { get; set; }

    /// <summary>
    /// Số người tròn mốc trong năm nhưng không rơi vào đợt nào (QT7).
    /// </summary>
    public int UnassignedCount { get; set; }

    /// <summary>
    /// Các cặp đợt chồng lấn trong năm hiện tại.
    /// </summary>
    public IReadOnlyList<PeriodOverlapResponse> Overlaps { get; set; } = Array.Empty<PeriodOverlapResponse>();

    /// <summary>
    /// Các khoảng chưa được đợt nào phủ trong năm hiện tại.
    /// </summary>
    public IReadOnlyList<PeriodGapResponse> Gaps { get; set; } = Array.Empty<PeriodGapResponse>();
}
