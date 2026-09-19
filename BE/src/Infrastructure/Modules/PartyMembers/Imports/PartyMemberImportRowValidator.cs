using System.Globalization;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports.Models;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Requests;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports;

/// <summary>
/// Kiểm tra một dòng nhập Excel. Hàm thuần: không đụng cơ sở dữ liệu, không đọc đồng hồ —
/// "hôm nay" truyền vào qua tham số, giống nguyên tắc của service tính mốc tuổi đảng.
/// Bộ quy tắc trùng khít <see cref="PartyMemberRequestValidator{TRequest}"/> của T09 để màn hình
/// thêm tay và nhập Excel không lệch nhau.
/// </summary>
public static class PartyMemberImportRowValidator
{
    /// <summary>
    /// Chữ tiếng Việt của giới tính Nam trong file Excel.
    /// </summary>
    private const string MaleText = "Nam";

    /// <summary>
    /// Chữ tiếng Việt của giới tính Nữ trong file Excel.
    /// </summary>
    private const string FemaleText = "Nữ";

    /// <summary>
    /// Hai dạng ngày chấp nhận được. <c>d/M/yyyy</c> nhận cả <c>1/10/1996</c> lẫn <c>01/10/1996</c>
    /// vì Excel hay tự bỏ số 0 đứng đầu (OQ-6).
    /// </summary>
    private static readonly string[] DateFormats = { "d/M/yyyy", "dd/MM/yyyy" };

    /// <summary>
    /// Kiểm tra một dòng thô.
    /// </summary>
    /// <param name="row">Dòng đã đọc từ Excel và cắt khoảng trắng.</param>
    /// <param name="today">Ngày hôm nay theo lịch máy chủ.</param>
    /// <param name="validRow">Dòng đã chuẩn hóa khi hợp lệ.</param>
    /// <returns>Danh sách lý do; rỗng nghĩa là dòng hợp lệ.</returns>
    public static IReadOnlyList<ImportRowErrorResponse> Validate(
        ImportRawRow row, DateOnly today, out ImportValidRowResponse? validRow)
    {
        validRow = null;

        bool hasBirthDate = TryParseDate(row.DateOfBirth, out DateOnly birthDate);
        bool hasAdmissionDate = TryParseDate(row.OfficialAdmissionDate, out DateOnly admissionDate);
        string? gender = ToGenderValue(row.Gender);

        List<ImportRowErrorResponse> errors = new();

        // Thứ tự thêm lỗi bám đúng thứ tự bảng "Mã lỗi cấp dòng" của mục 4.2 hợp đồng API.
        if (row.FullName.Length == 0)
        {
            errors.Add(new ImportRowErrorResponse(ImportErrorCodes.MissingFullName, ImportFields.FullName));
        }

        if (row.OfficialAdmissionDate.Length == 0)
        {
            errors.Add(new ImportRowErrorResponse(
                ImportErrorCodes.MissingOfficialAdmissionDate, ImportFields.OfficialAdmissionDate));
        }

        if (row.DateOfBirth.Length > 0 && !hasBirthDate)
        {
            errors.Add(new ImportRowErrorResponse(ImportErrorCodes.InvalidDateFormat, ImportFields.DateOfBirth));
        }

        if (row.OfficialAdmissionDate.Length > 0 && !hasAdmissionDate)
        {
            errors.Add(new ImportRowErrorResponse(
                ImportErrorCodes.InvalidDateFormat, ImportFields.OfficialAdmissionDate));
        }

        if (hasAdmissionDate && admissionDate > today)
        {
            errors.Add(new ImportRowErrorResponse(
                ImportErrorCodes.FutureOfficialAdmissionDate, ImportFields.OfficialAdmissionDate));
        }

        if (row.Gender.Length > 0 && gender is null)
        {
            errors.Add(new ImportRowErrorResponse(ImportErrorCodes.InvalidGender, ImportFields.Gender));
        }

        // OQ-10: bằng nhau cũng là lỗi, ngày sinh phải thực sự trước ngày chính thức.
        if (hasBirthDate && hasAdmissionDate && birthDate >= admissionDate)
        {
            errors.Add(new ImportRowErrorResponse(
                ImportErrorCodes.BirthDateAfterAdmissionDate, ImportFields.DateOfBirth));
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        validRow = new ImportValidRowResponse(
            row.RowNumber,
            row.FullName,
            hasBirthDate ? birthDate : null,
            gender,
            admissionDate);

        return Array.Empty<ImportRowErrorResponse>();
    }

    /// <summary>
    /// Đọc một ô ngày. Ngày không có thật như <c>31/02/1974</c> cũng trượt ở đây (OQ-1).
    /// </summary>
    /// <param name="text">Chữ trong ô, đã cắt khoảng trắng.</param>
    /// <param name="value">Ngày đọc được.</param>
    /// <returns><see langword="true"/> khi đọc được.</returns>
    private static bool TryParseDate(string text, out DateOnly value)
    {
        value = default;

        return text.Length > 0
            && DateOnly.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
    }

    /// <summary>
    /// Đổi chữ giới tính trong Excel sang giá trị của API. Không phân biệt hoa thường (OQ-5).
    /// </summary>
    /// <param name="text">Chữ trong ô, đã cắt khoảng trắng.</param>
    /// <returns><c>Male</c>, <c>Female</c>, hoặc <see langword="null"/> khi bỏ trống hoặc không hợp lệ.</returns>
    private static string? ToGenderValue(string text)
    {
        if (string.Equals(text, MaleText, StringComparison.InvariantCultureIgnoreCase))
        {
            return PartyMemberRequest.MaleValue;
        }

        return string.Equals(text, FemaleText, StringComparison.InvariantCultureIgnoreCase)
            ? PartyMemberRequest.FemaleValue
            : null;
    }
}
