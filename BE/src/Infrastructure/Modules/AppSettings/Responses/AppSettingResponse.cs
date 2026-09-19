namespace HuyHieuDang.Infrastructure.Modules.AppSettings.Responses;

/// <summary>
/// Cài đặt toàn hệ thống kèm dãy mốc đã sinh (mục 7.1 hợp đồng API). Dãy mốc là nguồn sự thật duy
/// nhất cho giao diện — Frontend không tự suy ra công thức QT1.
/// </summary>
public class AppSettingResponse
{
    /// <summary>
    /// Mốc huy hiệu đầu tiên.
    /// </summary>
    public int StartYears { get; set; }

    /// <summary>
    /// Mốc huy hiệu cuối cùng.
    /// </summary>
    public int EndYears { get; set; }

    /// <summary>
    /// Bước nhảy giữa hai mốc liên tiếp.
    /// </summary>
    public int StepYears { get; set; }

    /// <summary>
    /// Tên đơn vị; <see langword="null"/> nghĩa là chưa đặt.
    /// </summary>
    public string? UnitName { get; set; }

    /// <summary>
    /// Dãy mốc huy hiệu tăng dần sinh theo QT1.
    /// </summary>
    public IReadOnlyList<int> Milestones { get; set; } = Array.Empty<int>();

    /// <summary>
    /// Số mốc trong dãy.
    /// </summary>
    public int MilestoneCount { get; set; }

    /// <summary>
    /// Lần sửa cài đặt gần nhất.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
