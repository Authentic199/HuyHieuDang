using HuyHieuDang.Infrastructure.Modules.PartyMembers.Enums;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports.Models;

/// <summary>
/// Một dòng dữ liệu đọc thô từ Excel: bốn ô đã đổi sang chuỗi, chưa kiểm tra gì.
/// </summary>
/// <param name="RowNumber">Số dòng trong file Excel — dòng tiêu đề là 1 nên dữ liệu bắt đầu từ 2.</param>
/// <param name="FullName">Ô Họ tên.</param>
/// <param name="DateOfBirth">Ô Ngày sinh.</param>
/// <param name="Gender">Ô Giới tính.</param>
/// <param name="OfficialAdmissionDate">Ô Ngày vào Đảng chính thức.</param>
public sealed record ImportRawRow(
    int RowNumber,
    string FullName,
    string DateOfBirth,
    string Gender,
    string OfficialAdmissionDate)
{
    /// <summary>
    /// Dòng hoàn toàn trống — bốn ô đều rỗng. Những dòng này bị bỏ qua, không tính là dòng lỗi.
    /// </summary>
    public bool IsBlank =>
        FullName.Length == 0
        && DateOfBirth.Length == 0
        && Gender.Length == 0
        && OfficialAdmissionDate.Length == 0;
}

/// <summary>
/// Một lý do khiến dòng bị loại.
/// </summary>
/// <param name="ErrorCode">Mã lý do, xem <see cref="ImportErrorCodes"/>.</param>
/// <param name="Field">Cột gây lỗi, xem <see cref="ImportFields"/>.</param>
public sealed record ImportRowErrorResponse(string ErrorCode, string Field);

/// <summary>
/// Một dòng hợp lệ đã chuẩn hóa, sẵn sàng ghi vào cơ sở dữ liệu.
/// </summary>
/// <param name="RowNumber">Số dòng trong file Excel.</param>
/// <param name="FullName">Họ tên đã cắt khoảng trắng (OQ-4).</param>
/// <param name="DateOfBirth">Ngày sinh, hoặc <see langword="null"/> khi bỏ trống.</param>
/// <param name="Gender">Giới tính dạng <c>Male</c> / <c>Female</c> / <see langword="null"/>.</param>
/// <param name="OfficialAdmissionDate">Ngày vào Đảng chính thức.</param>
public sealed record ImportValidRowResponse(
    int RowNumber,
    string FullName,
    DateOnly? DateOfBirth,
    string? Gender,
    DateOnly OfficialAdmissionDate)
{
    /// <summary>
    /// Đổi giới tính chuỗi sang enum để dựng bản ghi.
    /// </summary>
    /// <returns>Giới tính đã đổi kiểu, hoặc <see langword="null"/> khi bỏ trống.</returns>
    public Gender? ToGender()
        => Gender is null ? null : Enum.Parse<Gender>(Gender, ignoreCase: false);
}

/// <summary>
/// Một dòng bị loại. Bốn trường giữ nguyên chữ thô đọc từ ô Excel vì chính chúng đang sai.
/// </summary>
/// <param name="RowNumber">Số dòng trong file Excel.</param>
/// <param name="FullName">Ô Họ tên, chữ thô.</param>
/// <param name="DateOfBirth">Ô Ngày sinh, chữ thô.</param>
/// <param name="Gender">Ô Giới tính, chữ thô.</param>
/// <param name="OfficialAdmissionDate">Ô Ngày vào Đảng chính thức, chữ thô.</param>
/// <param name="Errors">Đủ mọi lý do của dòng, sắp theo thứ tự bảng mã lỗi (OQ-2).</param>
public sealed record ImportErrorRowResponse(
    int RowNumber,
    string FullName,
    string DateOfBirth,
    string Gender,
    string OfficialAdmissionDate,
    IReadOnlyList<ImportRowErrorResponse> Errors);
