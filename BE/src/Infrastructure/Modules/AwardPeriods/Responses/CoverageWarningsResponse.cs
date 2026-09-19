namespace HuyHieuDang.Infrastructure.Modules.AwardPeriods.Responses;

/// <summary>
/// Cảnh báo chồng lấn và chưa phủ kín của một năm (QT6). Cảnh báo không bao giờ chặn lưu.
/// </summary>
public class CoverageWarningsResponse
{
    /// <summary>
    /// Các cặp đợt dùng chung ngày.
    /// </summary>
    public IReadOnlyList<PeriodOverlapResponse> Overlaps { get; set; } = Array.Empty<PeriodOverlapResponse>();

    /// <summary>
    /// Các khoảng 01/01–31/12 chưa được đợt nào phủ.
    /// </summary>
    public IReadOnlyList<PeriodGapResponse> Gaps { get; set; } = Array.Empty<PeriodGapResponse>();
}

/// <summary>
/// Một cặp đợt chồng lấn kèm đúng khoảng ngày dùng chung.
/// </summary>
public class PeriodOverlapResponse
{
    /// <summary>
    /// Id của đợt có Từ ngày sớm hơn.
    /// </summary>
    public Guid FirstPeriodId { get; set; }

    /// <summary>
    /// Tên của đợt đến trước.
    /// </summary>
    public string FirstPeriodName { get; set; } = default!;

    /// <summary>
    /// Id của đợt còn lại.
    /// </summary>
    public Guid SecondPeriodId { get; set; }

    /// <summary>
    /// Tên của đợt còn lại.
    /// </summary>
    public string SecondPeriodName { get; set; } = default!;

    /// <summary>
    /// Ngày đầu của phần chồng lấn.
    /// </summary>
    public DateOnly FromDate { get; set; }

    /// <summary>
    /// Ngày cuối của phần chồng lấn.
    /// </summary>
    public DateOnly ToDate { get; set; }

    /// <summary>
    /// Ngày đầu dạng <c>dd/MM</c>.
    /// </summary>
    public string FromDisplay { get; set; } = default!;

    /// <summary>
    /// Ngày cuối dạng <c>dd/MM</c>.
    /// </summary>
    public string ToDisplay { get; set; } = default!;
}

/// <summary>
/// Một khoảng trống chưa đợt nào phủ, kèm hai đợt kề hai bên.
/// </summary>
public class PeriodGapResponse
{
    /// <summary>
    /// Ngày đầu khoảng trống.
    /// </summary>
    public DateOnly FromDate { get; set; }

    /// <summary>
    /// Ngày cuối khoảng trống.
    /// </summary>
    public DateOnly ToDate { get; set; }

    /// <summary>
    /// Ngày đầu dạng <c>dd/MM</c>.
    /// </summary>
    public string FromDisplay { get; set; } = default!;

    /// <summary>
    /// Ngày cuối dạng <c>dd/MM</c>.
    /// </summary>
    public string ToDisplay { get; set; } = default!;

    /// <summary>
    /// Tên đợt ngay trước khoảng trống; <see langword="null"/> khi khoảng trống nằm đầu năm.
    /// </summary>
    public string? PreviousPeriodName { get; set; }

    /// <summary>
    /// Tên đợt ngay sau khoảng trống; <see langword="null"/> khi khoảng trống nằm cuối năm.
    /// </summary>
    public string? NextPeriodName { get; set; }
}
