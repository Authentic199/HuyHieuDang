namespace HuyHieuDang.Infrastructure.Modules.AppSettings.Requests;

/// <summary>
/// Tham số của <c>GET /api/Settings/Milestones</c> (mục 7.4 hợp đồng API, UC-50). Bỏ trống tham số
/// nào thì lấy giá trị đang lưu của tham số đó, nên cả ba đều không bắt buộc.
/// </summary>
public class MilestonePreviewRequest
{
    /// <summary>
    /// Mốc bắt đầu muốn xem trước; bỏ trống thì lấy giá trị đang lưu.
    /// </summary>
    public int? Start { get; set; }

    /// <summary>
    /// Mốc kết thúc muốn xem trước; bỏ trống thì lấy giá trị đang lưu.
    /// </summary>
    public int? End { get; set; }

    /// <summary>
    /// Bước nhảy muốn xem trước; bỏ trống thì lấy giá trị đang lưu.
    /// </summary>
    public int? Step { get; set; }
}
