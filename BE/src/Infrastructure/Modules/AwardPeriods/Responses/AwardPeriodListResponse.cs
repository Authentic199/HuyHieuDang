namespace HuyHieuDang.Infrastructure.Modules.AwardPeriods.Responses;

/// <summary>
/// Phản hồi của <c>GET /api/AwardPeriods</c> (mục 5.1): đủ dữ liệu cho bảng, dải độ phủ
/// và banner cảnh báo trong một lời gọi.
/// </summary>
public class AwardPeriodListResponse
{
    /// <summary>
    /// Năm đang xét.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Hôm nay theo lịch máy chủ.
    /// </summary>
    public DateOnly Today { get; set; }

    /// <summary>
    /// Số đợt đang cấu hình.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Các đợt sắp theo <c>fromDate</c> tăng dần.
    /// </summary>
    public IReadOnlyList<AwardPeriodResponse> Periods { get; set; } = Array.Empty<AwardPeriodResponse>();

    /// <summary>
    /// Cảnh báo chồng lấn và khoảng trống của năm đang xét.
    /// </summary>
    public CoverageWarningsResponse Warnings { get; set; } = new();

    /// <summary>
    /// Dải độ phủ 12 tháng.
    /// </summary>
    public CoverageResponse Coverage { get; set; } = new();
}

/// <summary>
/// Phản hồi của thêm mới và sửa (mục 5.3, 5.4): đợt vừa lưu kèm cảnh báo của năm hiện tại.
/// </summary>
public class AwardPeriodMutationResponse
{
    /// <summary>
    /// Đợt vừa lưu.
    /// </summary>
    public AwardPeriodResponse Period { get; set; } = default!;

    /// <summary>
    /// Cảnh báo tính lại sau khi lưu; có cảnh báo vẫn là <c>200</c>.
    /// </summary>
    public CoverageWarningsResponse Warnings { get; set; } = new();
}

/// <summary>
/// Phản hồi của xóa (mục 5.5): id vừa xóa kèm cảnh báo mới sinh ra.
/// </summary>
public class AwardPeriodDeletedResponse
{
    /// <summary>
    /// Id của đợt vừa xóa.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Cảnh báo của năm hiện tại sau khi xóa.
    /// </summary>
    public CoverageWarningsResponse Warnings { get; set; } = new();
}
