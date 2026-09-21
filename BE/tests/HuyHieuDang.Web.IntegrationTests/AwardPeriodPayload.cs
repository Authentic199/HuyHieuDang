namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Một đợt trong phần <c>data</c>, đọc đúng tên trường camelCase của mục 1.10 hợp đồng API.
/// <c>status</c> để kiểu chuỗi để khẳng định đúng chữ mà Frontend nhận được.
/// </summary>
public sealed class AwardPeriodPayload
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int FromDay { get; set; }

    public int FromMonth { get; set; }

    public int ToDay { get; set; }

    public int ToMonth { get; set; }

    public string FromDisplay { get; set; } = string.Empty;

    public string ToDisplay { get; set; } = string.Empty;

    public int Year { get; set; }

    public DateOnly FromDate { get; set; }

    public DateOnly ToDate { get; set; }

    public bool SpansNextYear { get; set; }

    public string Status { get; set; } = string.Empty;

    public int? DaysRemaining { get; set; }

    public int EligibleCount { get; set; }
}

/// <summary>
/// Phần <c>data</c> của <c>GET /api/AwardPeriods</c> (mục 5.1).
/// </summary>
public sealed class AwardPeriodListPayload
{
    public int Year { get; set; }

    public DateOnly Today { get; set; }

    public int TotalCount { get; set; }

    public List<AwardPeriodPayload> Periods { get; set; } = new();

    public CoverageWarningsPayload Warnings { get; set; } = new();

    public CoveragePayload Coverage { get; set; } = new();
}

/// <summary>
/// Phần <c>data</c> của thêm mới và sửa (mục 5.3, 5.4).
/// </summary>
public sealed class AwardPeriodMutationPayload
{
    public AwardPeriodPayload Period { get; set; } = new();

    public CoverageWarningsPayload Warnings { get; set; } = new();
}

/// <summary>
/// Phần <c>data</c> của xóa (mục 5.5).
/// </summary>
public sealed class AwardPeriodDeletedPayload
{
    public Guid Id { get; set; }

    public CoverageWarningsPayload Warnings { get; set; } = new();
}

/// <summary>
/// Cảnh báo chồng lấn và khoảng trống.
/// </summary>
public sealed class CoverageWarningsPayload
{
    public List<PeriodOverlapPayload> Overlaps { get; set; } = new();

    public List<PeriodGapPayload> Gaps { get; set; } = new();
}

/// <summary>
/// Một cặp đợt chồng lấn.
/// </summary>
public sealed class PeriodOverlapPayload
{
    public Guid FirstPeriodId { get; set; }

    public string FirstPeriodName { get; set; } = string.Empty;

    public Guid SecondPeriodId { get; set; }

    public string SecondPeriodName { get; set; } = string.Empty;

    public DateOnly FromDate { get; set; }

    public DateOnly ToDate { get; set; }

    public string FromDisplay { get; set; } = string.Empty;

    public string ToDisplay { get; set; } = string.Empty;
}

/// <summary>
/// Một khoảng trống chưa đợt nào phủ.
/// </summary>
public sealed class PeriodGapPayload
{
    public DateOnly FromDate { get; set; }

    public DateOnly ToDate { get; set; }

    public string FromDisplay { get; set; } = string.Empty;

    public string ToDisplay { get; set; } = string.Empty;

    public string? PreviousPeriodName { get; set; }

    public string? NextPeriodName { get; set; }
}

/// <summary>
/// Dải độ phủ 12 tháng.
/// </summary>
public sealed class CoveragePayload
{
    public List<CoverageSegmentPayload> Segments { get; set; } = new();
}

/// <summary>
/// Một đoạn trên dải độ phủ.
/// </summary>
public sealed class CoverageSegmentPayload
{
    public string Type { get; set; } = string.Empty;

    public Guid? PeriodId { get; set; }

    public string? Name { get; set; }

    public DateOnly FromDate { get; set; }

    public DateOnly ToDate { get; set; }
}
