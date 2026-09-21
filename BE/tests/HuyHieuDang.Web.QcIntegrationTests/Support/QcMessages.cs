namespace HuyHieuDang.Web.QcIntegrationTests.Support;

/// <summary>
/// Bảng khóa thông điệp chép từ mục 1.5 hợp đồng API. Hợp đồng chốt rằng Backend trả **khóa**
/// chứ không trả chữ tiếng Việt, nên ca A-905 được hiểu đúng theo hợp đồng: mọi khóa lỗi lọt ra
/// người dùng phải nằm trong bảng này (tức Frontend chắc chắn dịch được sang tiếng Việt), và
/// thân phản hồi không được lộ stack trace hay tên bảng/cột.
/// </summary>
public static class QcMessages
{
    /// <summary>Sai tài khoản hoặc mật khẩu.</summary>
    public const string LoginFailed = "Mes.User.Login.Failed";

    /// <summary>Không tìm thấy đảng viên.</summary>
    public const string MemberNotFound = "Mes.PartyMember.NotFound";

    /// <summary>Chưa nhập Họ tên.</summary>
    public const string MemberRequiredFullName = "Mes.PartyMember.Required.FullName";

    /// <summary>Chưa nhập Ngày vào Đảng chính thức.</summary>
    public const string MemberRequiredAdmissionDate = "Mes.PartyMember.Required.OfficialAdmissionDate";

    /// <summary>Ngày chính thức không được ở tương lai.</summary>
    public const string MemberInvalidAdmissionDate = "Mes.PartyMember.Invalid.OfficialAdmissionDate";

    /// <summary>Ngày sinh phải trước Ngày vào Đảng chính thức.</summary>
    public const string MemberInvalidDateOfBirth = "Mes.PartyMember.Invalid.DateOfBirth";

    /// <summary>Không tìm thấy đợt trao huy hiệu.</summary>
    public const string PeriodNotFound = "Mes.AwardPeriod.NotFound";

    /// <summary>Chưa nhập Tên đợt.</summary>
    public const string PeriodRequiredName = "Mes.AwardPeriod.Required.Name";

    /// <summary>Tên đợt đã tồn tại.</summary>
    public const string PeriodRepeatedName = "Mes.AwardPeriod.Repeated.Name";

    /// <summary>Từ ngày không hợp lệ.</summary>
    public const string PeriodInvalidFromDate = "Mes.AwardPeriod.Invalid.FromDate";

    /// <summary>Đến ngày không hợp lệ.</summary>
    public const string PeriodInvalidToDate = "Mes.AwardPeriod.Invalid.ToDate";

    /// <summary>Mốc bắt đầu phải là số nguyên dương.</summary>
    public const string SettingInvalidStartYears = "Mes.AppSetting.Invalid.StartYears";

    /// <summary>Mốc kết thúc phải là số nguyên dương.</summary>
    public const string SettingInvalidEndYears = "Mes.AppSetting.Invalid.EndYears";

    /// <summary>Bước nhảy phải từ 1 trở lên.</summary>
    public const string SettingInvalidStepYears = "Mes.AppSetting.Invalid.StepYears";

    /// <summary>Mốc bắt đầu phải nhỏ hơn hoặc bằng mốc kết thúc.</summary>
    public const string SettingInvalidRange = "Mes.AppSetting.Invalid.Range";

    /// <summary>Chỉ nhận file .xlsx.</summary>
    public const string ImportInvalidExtension = "Mes.Import.Invalid.Extension";

    /// <summary>File vượt quá 10 MB.</summary>
    public const string ImportInvalidFileSize = "Mes.Import.Invalid.FileSize";

    /// <summary>File phải có đúng 4 cột theo thứ tự quy định.</summary>
    public const string ImportInvalidColumns = "Mes.Import.Invalid.Columns";

    /// <summary>File rỗng, không đọc được dữ liệu.</summary>
    public const string ImportInvalidEmpty = "Mes.Import.Invalid.Empty";

    /// <summary>File không có dòng dữ liệu nào.</summary>
    public const string ImportInvalidNoDataRows = "Mes.Import.Invalid.NoDataRows";

    /// <summary>Chưa cài đợt trao huy hiệu.</summary>
    public const string DashboardNoUpcomingPeriod = "Mes.Dashboard.NotFound.UpcomingPeriod";

    /// <summary>Năm không hợp lệ.</summary>
    public const string QueryInvalidYear = "Mes.Query.Invalid.Year";

    /// <summary>
    /// Tham số truy vấn sai kiểu hoặc sai giá trị. Khóa này CEO chốt cho QC-T27-05 ngày
    /// 20/09/2026 và hợp đồng v1.4 (PR #46) đã có; ca <c>QcT2705</c> dùng nó, và ca đó còn
    /// để <c>Skip</c> tới khi T47 gộp.
    /// </summary>
    public const string CommonInvalidParameter = "Mes.Common.Invalid.Parameter";

    /// <summary>
    /// Bảng khóa bắt buộc của mục 1.5 hợp đồng API — hiện là <b>bản chép tay</b>.
    /// <para>
    /// QC-T29-02: bản chép này đã trôi khỏi hợp đồng. Nó có <c>Mes.User.Required.Username</c>
    /// và <c>Mes.User.Required.Password</c> trong khi mục 1.5 trên <c>main</c> chưa có, nên ca
    /// A-905 xanh nhờ bản chép chứ không nhờ hợp đồng. Hợp đồng v1.4 (PR #46) bổ sung đúng hai
    /// khóa đó cùng năm khóa khác. Khi #46 gộp, QC thay bảng chép tay này bằng bản đọc thẳng
    /// <c>docs/api-contract.md</c> để nó không trôi được nữa, rồi gỡ <c>Skip</c> của QcT2707.
    /// Chưa làm ngay ở T29 vì đọc thẳng hợp đồng lúc này làm A-905 đỏ trên <c>main</c>.
    /// </para>
    /// </summary>
    public static IReadOnlySet<string> ContractKeys { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "Mes.User.Login.Successfully",
        LoginFailed,
        "Mes.User.Logout.Successfully",
        "Mes.User.Required.Username",
        "Mes.User.Required.Password",
        "Mes.PartyMember.Create.Successfully",
        "Mes.PartyMember.Update.Successfully",
        "Mes.PartyMember.Delete.Successfully",
        "Mes.PartyMember.Import.Successfully",
        MemberNotFound,
        MemberRequiredFullName,
        MemberRequiredAdmissionDate,
        MemberInvalidAdmissionDate,
        MemberInvalidDateOfBirth,
        "Mes.PartyMember.Invalid.Gender",
        "Mes.AwardPeriod.Create.Successfully",
        "Mes.AwardPeriod.Update.Successfully",
        "Mes.AwardPeriod.Delete.Successfully",
        PeriodNotFound,
        PeriodRequiredName,
        PeriodRepeatedName,
        PeriodInvalidFromDate,
        PeriodInvalidToDate,
        "Mes.AppSetting.Update.Successfully",
        SettingInvalidStartYears,
        SettingInvalidEndYears,
        SettingInvalidStepYears,
        SettingInvalidRange,
        ImportInvalidExtension,
        ImportInvalidFileSize,
        ImportInvalidColumns,
        ImportInvalidEmpty,
        ImportInvalidNoDataRows,
        DashboardNoUpcomingPeriod,
        QueryInvalidYear,
    };

    /// <summary>
    /// Dấu hiệu rò rỉ nội bộ: dấu vết ngăn xếp, tên kiểu .NET, tên trình điều khiển cơ sở dữ liệu
    /// hoặc tên bảng/cột thật.
    /// </summary>
    private static readonly string[] LeakMarkers =
    {
        "   at ",
        "StackTrace",
        "Npgsql",
        "System.",
        "HuyHieuDang.Infrastructure",
        "party_members",
        "award_periods",
        "app_settings",
        "SELECT ",
        "INSERT INTO",
    };

    /// <summary>Khẳng định thân phản hồi lỗi không lộ chi tiết nội bộ ra người dùng (A-905).</summary>
    /// <param name="body">Thân phản hồi dạng chữ.</param>
    public static void ShouldNotLeakInternals(string body)
    {
        ArgumentNullException.ThrowIfNull(body);

        foreach (string marker in LeakMarkers)
        {
            body.ShouldNotContain(marker, Case.Sensitive, $"Thân phản hồi lộ chi tiết nội bộ: {body}");
        }
    }
}
