namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports.Models;

/// <summary>
/// Sáu mã lỗi cấp dòng của bước xem trước (bảng "Mã lỗi cấp dòng", mục 4.2 hợp đồng API).
/// Frontend tra mã này ra chữ hiển thị ở cột "Lý do" nên chuỗi phải khớp từng ký tự.
/// </summary>
public static class ImportErrorCodes
{
    /// <summary>Thiếu họ tên.</summary>
    public const string MissingFullName = nameof(MissingFullName);

    /// <summary>Thiếu ngày vào Đảng chính thức.</summary>
    public const string MissingOfficialAdmissionDate = nameof(MissingOfficialAdmissionDate);

    /// <summary>Sai định dạng ngày — kể cả ngày không có thật như <c>31/02/1974</c> (OQ-1).</summary>
    public const string InvalidDateFormat = nameof(InvalidDateFormat);

    /// <summary>Ngày chính thức ở tương lai so với hôm nay.</summary>
    public const string FutureOfficialAdmissionDate = nameof(FutureOfficialAdmissionDate);

    /// <summary>Giới tính khác Nam và Nữ khi có điền.</summary>
    public const string InvalidGender = nameof(InvalidGender);

    /// <summary>Ngày sinh bằng hoặc sau ngày vào Đảng chính thức (OQ-10).</summary>
    public const string BirthDateAfterAdmissionDate = nameof(BirthDateAfterAdmissionDate);
}

/// <summary>
/// Tên bốn cột dùng trong <c>errors[*].field</c> để Frontend tô đỏ đúng ô.
/// </summary>
public static class ImportFields
{
    /// <summary>Cột Họ tên.</summary>
    public const string FullName = nameof(FullName);

    /// <summary>Cột Ngày sinh.</summary>
    public const string DateOfBirth = nameof(DateOfBirth);

    /// <summary>Cột Giới tính.</summary>
    public const string Gender = nameof(Gender);

    /// <summary>Cột Ngày vào Đảng chính thức.</summary>
    public const string OfficialAdmissionDate = nameof(OfficialAdmissionDate);
}
