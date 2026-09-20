namespace HuyHieuDang.Web.QcIntegrationTests.Support;

/// <summary>
/// 28 endpoint của <c>docs/api-contract.md</c>, chép theo đúng thứ tự mục 2 → mục 8. Bộ kiểm thử
/// lấy đường dẫn từ đây để bảng đối chiếu "endpoint ↔ ca đã chạm" không bao giờ lệch với mã.
/// </summary>
public static class QcEndpoints
{
    /// <summary>2.1 · Đăng nhập. Endpoint duy nhất không đòi token.</summary>
    public const string Login = "/api/Auth/Login";

    /// <summary>2.2 · Đăng xuất.</summary>
    public const string Logout = "/api/Auth/Logout";

    /// <summary>2.3 · Thông tin phiên.</summary>
    public const string Me = "/api/Auth/Me";

    /// <summary>3.1 · Danh sách đảng viên có phân trang.</summary>
    public const string PartyMembers = "/api/PartyMembers";

    /// <summary>3.6 · Xóa nhiều đảng viên.</summary>
    public const string PartyMembersDeleteMany = "/api/PartyMembers/DeleteMany";

    /// <summary>4.1 · Tải file mẫu import.</summary>
    public const string ImportTemplate = "/api/PartyMembers/Import/Template";

    /// <summary>4.2 · Xem trước file import.</summary>
    public const string ImportPreview = "/api/PartyMembers/Import/Preview";

    /// <summary>4.3 · Nạp file import.</summary>
    public const string ImportCommit = "/api/PartyMembers/Import/Commit";

    /// <summary>5.1 · Danh sách đợt trao huy hiệu.</summary>
    public const string AwardPeriods = "/api/AwardPeriods";

    /// <summary>6.1 · Dữ liệu Dashboard.</summary>
    public const string Dashboard = "/api/Dashboard";

    /// <summary>6.2 · Đủ điều kiện theo đợt và năm.</summary>
    public const string Eligibility = "/api/Eligibility";

    /// <summary>6.3 · Chưa thuộc đợt nào.</summary>
    public const string Unassigned = "/api/Eligibility/Unassigned";

    /// <summary>6.4 · Badge số người bị sót.</summary>
    public const string UnassignedCount = "/api/Eligibility/UnassignedCount";

    /// <summary>7.1 và 7.2 · Đọc và lưu cài đặt.</summary>
    public const string Settings = "/api/Settings";

    /// <summary>7.3 · Khôi phục mặc định.</summary>
    public const string SettingsRestoreDefaults = "/api/Settings/RestoreDefaults";

    /// <summary>7.4 · Xem trước dãy mốc.</summary>
    public const string SettingsMilestones = "/api/Settings/Milestones";

    /// <summary>8.1 · Xuất danh sách đủ điều kiện của một đợt.</summary>
    public const string ExportEligibility = "/api/Exports/Eligibility";

    /// <summary>8.2 · Xuất danh sách đợt sắp tới của Dashboard.</summary>
    public const string ExportDashboard = "/api/Exports/Dashboard";

    /// <summary>8.3 · Xuất danh sách chưa thuộc đợt nào.</summary>
    public const string ExportUnassigned = "/api/Exports/Unassigned";

    private static readonly Guid SampleId = new("00000000-0000-0000-0000-0000000000aa");

    /// <summary>
    /// 27 endpoint nghiệp vụ — toàn bộ hợp đồng trừ <c>Auth/Login</c>. Những endpoint có
    /// <c>{id}</c> dùng một guid bất kỳ: ca A-001 chỉ quan tâm cổng xác thực chặn trước khi
    /// chạm tới nghiệp vụ.
    /// </summary>
    public static IReadOnlyList<(string Method, string Url)> All { get; } = new[]
    {
        ("POST", Logout),
        ("GET", Me),
        ("GET", PartyMembers),
        ("GET", $"{PartyMembers}/{SampleId}"),
        ("POST", PartyMembers),
        ("PUT", $"{PartyMembers}/{SampleId}"),
        ("DELETE", $"{PartyMembers}/{SampleId}"),
        ("POST", PartyMembersDeleteMany),
        ("GET", ImportTemplate),
        ("POST", ImportPreview),
        ("POST", ImportCommit),
        ("GET", AwardPeriods),
        ("GET", $"{AwardPeriods}/{SampleId}"),
        ("POST", AwardPeriods),
        ("PUT", $"{AwardPeriods}/{SampleId}"),
        ("DELETE", $"{AwardPeriods}/{SampleId}"),
        ("GET", Dashboard),
        ("GET", $"{Eligibility}?awardPeriodId={SampleId}"),
        ("GET", Unassigned),
        ("GET", UnassignedCount),
        ("GET", Settings),
        ("PUT", Settings),
        ("POST", SettingsRestoreDefaults),
        ("GET", SettingsMilestones),
        ("GET", $"{ExportEligibility}?awardPeriodId={SampleId}"),
        ("GET", ExportDashboard),
        ("GET", ExportUnassigned),
    };
}
