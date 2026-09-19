using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Responses;

namespace HuyHieuDang.Infrastructure.Modules.Eligibility.Responses;

/// <summary>
/// Phần <c>data</c> của <c>GET /api/Eligibility</c> (mục 6.2 hợp đồng API, UC-34).
/// Không phân trang: màn chi tiết đợt hiển thị cả bảng.
/// </summary>
public class EligibilityListResponse
{
    /// <summary>
    /// Đợt đang xem, đã gắn năm đang xét.
    /// </summary>
    public AwardPeriodResponse AwardPeriod { get; set; } = default!;

    /// <summary>
    /// Năm đang xét.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Tổng số người đủ điều kiện.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Phân bổ theo mốc huy hiệu.
    /// </summary>
    public IReadOnlyList<MilestoneBreakdownResponse> MilestoneBreakdown { get; set; }
        = Array.Empty<MilestoneBreakdownResponse>();

    /// <summary>
    /// Danh sách đủ điều kiện, đã sắp theo quy ước chung.
    /// </summary>
    public IReadOnlyList<EligibleMemberResponse> Members { get; set; } = Array.Empty<EligibleMemberResponse>();
}

/// <summary>
/// Phần <c>data</c> của <c>GET /api/Eligibility/Unassigned</c> (mục 6.3 hợp đồng API, UC-40).
/// </summary>
public class UnassignedListResponse
{
    /// <summary>
    /// Năm đang xét.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Tổng số người tròn mốc trong năm nhưng không rơi vào đợt nào.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Danh sách người bị sót, kèm khoảng trống của từng người.
    /// </summary>
    public IReadOnlyList<UnassignedMemberResponse> Members { get; set; } = Array.Empty<UnassignedMemberResponse>();
}

/// <summary>
/// Phần <c>data</c> của <c>GET /api/Eligibility/UnassignedCount</c> (mục 6.4 hợp đồng API):
/// con số cho badge trên menu trái.
/// </summary>
public class UnassignedCountResponse
{
    /// <summary>
    /// Năm đang xét.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Số người bị sót trong năm.
    /// </summary>
    public int Count { get; set; }
}
