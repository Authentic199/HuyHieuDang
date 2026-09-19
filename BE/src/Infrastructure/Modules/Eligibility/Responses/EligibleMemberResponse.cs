using HuyHieuDang.Infrastructure.Modules.PartyMembers.Enums;

namespace HuyHieuDang.Infrastructure.Modules.Eligibility.Responses;

/// <summary>
/// Một dòng trong danh sách đủ điều kiện (mục 1.10 hợp đồng API). Cố ý không có trường
/// <c>id</c>: danh sách đủ điều kiện không được lưu vào bảng nào, mỗi lời gọi là một lần
/// tính lại (QT5). <c>PartyMemberId</c> chỉ để Frontend làm khóa dòng.
/// </summary>
public class EligibleMemberResponse
{
    /// <summary>
    /// Khóa chính của đảng viên.
    /// </summary>
    public Guid PartyMemberId { get; set; }

    /// <summary>
    /// Họ và tên đầy đủ.
    /// </summary>
    public string FullName { get; set; } = default!;

    /// <summary>
    /// Giới tính; <see langword="null"/> khi để trống.
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// Ngày sinh; <see langword="null"/> khi để trống.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Ngày vào Đảng chính thức.
    /// </summary>
    public DateOnly OfficialAdmissionDate { get; set; }

    /// <summary>
    /// Ngày tròn mốc trong năm đang xét (QT2).
    /// </summary>
    public DateOnly MilestoneDate { get; set; }

    /// <summary>
    /// Mốc huy hiệu được trao (QT4).
    /// </summary>
    public int Milestone { get; set; }
}

/// <summary>
/// Một dòng trong danh sách "chưa thuộc đợt nào" (UC-40, QT7): như dòng đủ điều kiện,
/// thêm khoảng trống chứa ngày tròn mốc.
/// </summary>
public class UnassignedMemberResponse : EligibleMemberResponse
{
    /// <summary>
    /// Khoảng trống chứa ngày tròn mốc.
    /// </summary>
    public UnassignedGapResponse Gap { get; set; } = default!;
}

/// <summary>
/// Vị trí của khoảng trống so với các đợt trong năm. Frontend dựng chữ cột "Khoảng trống"
/// từ ba giá trị này (mục 1.10 hợp đồng API).
/// </summary>
public enum UnassignedGapType
{
    /// <summary>Nằm giữa hai đợt liền kề.</summary>
    Between = 0,

    /// <summary>Nằm trước đợt đầu tiên của năm.</summary>
    BeforeFirst = 1,

    /// <summary>Nằm sau đợt cuối cùng của năm.</summary>
    AfterLast = 2,
}

/// <summary>
/// Khoảng trống chứa ngày tròn mốc của một người bị sót.
/// </summary>
public class UnassignedGapResponse
{
    /// <summary>
    /// Vị trí của khoảng trống.
    /// </summary>
    public UnassignedGapType Type { get; set; }

    /// <summary>
    /// Tên đợt ngay trước khoảng trống; <see langword="null"/> khi khoảng trống nằm đầu năm.
    /// </summary>
    public string? PreviousPeriodName { get; set; }

    /// <summary>
    /// Tên đợt ngay sau khoảng trống; <see langword="null"/> khi khoảng trống nằm cuối năm.
    /// </summary>
    public string? NextPeriodName { get; set; }
}

/// <summary>
/// Một dòng phân bổ theo mốc huy hiệu. Chỉ liệt kê mốc có người, sắp theo mốc tăng dần.
/// </summary>
public class MilestoneBreakdownResponse
{
    /// <summary>
    /// Mốc huy hiệu.
    /// </summary>
    public int Milestone { get; set; }

    /// <summary>
    /// Số người đạt mốc này.
    /// </summary>
    public int Count { get; set; }
}
