namespace HuyHieuDang.Infrastructure.Modules.AwardPeriods.Responses;

/// <summary>
/// Dải độ phủ 12 tháng của UC-36: các đoạn liên tiếp phủ kín 01/01–31/12 của năm đang xét.
/// </summary>
public class CoverageResponse
{
    /// <summary>
    /// Các đoạn nối tiếp nhau, sắp theo <see cref="CoverageSegmentResponse.FromDate"/>.
    /// </summary>
    public IReadOnlyList<CoverageSegmentResponse> Segments { get; set; }
        = Array.Empty<CoverageSegmentResponse>();
}

/// <summary>
/// Loại của một đoạn trên dải độ phủ.
/// </summary>
public enum CoverageSegmentType
{
    /// <summary>Đoạn do một đợt phủ.</summary>
    Period = 0,

    /// <summary>Đoạn chưa đợt nào phủ.</summary>
    Gap = 1,
}

/// <summary>
/// Một đoạn trên dải độ phủ. Khi hai đợt chồng lấn, phần chồng lấn thuộc đoạn của đợt đến trước.
/// </summary>
public class CoverageSegmentResponse
{
    /// <summary>
    /// Đoạn của một đợt hay một khoảng trống.
    /// </summary>
    public CoverageSegmentType Type { get; set; }

    /// <summary>
    /// Id đợt phủ đoạn này; <see langword="null"/> với đoạn trống.
    /// </summary>
    public Guid? PeriodId { get; set; }

    /// <summary>
    /// Tên đợt phủ đoạn này; <see langword="null"/> với đoạn trống.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Ngày đầu của đoạn.
    /// </summary>
    public DateOnly FromDate { get; set; }

    /// <summary>
    /// Ngày cuối của đoạn.
    /// </summary>
    public DateOnly ToDate { get; set; }
}
