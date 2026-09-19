namespace HuyHieuDang.Infrastructure.Modules.AppSettings.Responses;

/// <summary>
/// Dãy mốc xem trước của mục 7.4 hợp đồng API. Chỉ là kết quả tính, không ghi gì xuống cơ sở dữ liệu.
/// </summary>
public class MilestonePreviewResponse
{
    /// <summary>
    /// Dãy mốc huy hiệu tăng dần sinh theo QT1.
    /// </summary>
    public IReadOnlyList<int> Milestones { get; set; } = Array.Empty<int>();

    /// <summary>
    /// Số mốc trong dãy.
    /// </summary>
    public int MilestoneCount { get; set; }
}
