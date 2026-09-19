using System.Globalization;
using System.Text;

namespace HuyHieuDang.Infrastructure.Modules.Exports;

/// <summary>
/// Quy tắc đặt tên file Excel xuất ra (mục 1.9 hợp đồng API). Tên file do Backend sinh; Frontend
/// chỉ đọc lại từ header <c>Content-Disposition</c> chứ không tự ghép.
/// </summary>
public static class ExportFileName
{
    /// <summary>
    /// Tiền tố tên file danh sách đủ điều kiện.
    /// </summary>
    public const string EligibilityPrefix = "DuDieuKien";

    /// <summary>
    /// Tiền tố tên file danh sách chưa thuộc đợt nào.
    /// </summary>
    public const string UnassignedPrefix = "ChuaThuocDot";

    /// <summary>
    /// Phần mở rộng của mọi file xuất ra.
    /// </summary>
    public const string Extension = ".xlsx";

    /// <summary>
    /// Ký tự thay cho mọi ký tự không phải chữ hoặc số.
    /// </summary>
    private const char Replacement = '-';

    /// <summary>
    /// Tên file danh sách đủ điều kiện: <c>DuDieuKien_&lt;TênĐợtRútGọn&gt;_&lt;Năm&gt;.xlsx</c>.
    /// </summary>
    /// <param name="periodName">Tên đợt như người dùng nhập, ví dụ <c>Đợt 7/11</c>.</param>
    /// <param name="year">Năm đang xét.</param>
    /// <returns>Tên file, ví dụ <c>DuDieuKien_Dot7-11_2026.xlsx</c>.</returns>
    public static string ForEligibility(string periodName, int year)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"{EligibilityPrefix}_{Slug(periodName)}_{year}{Extension}");

    /// <summary>
    /// Tên file danh sách chưa thuộc đợt nào: <c>ChuaThuocDot_&lt;Năm&gt;.xlsx</c>.
    /// </summary>
    /// <param name="year">Năm đang xét.</param>
    /// <returns>Tên file, ví dụ <c>ChuaThuocDot_2026.xlsx</c>.</returns>
    public static string ForUnassigned(int year)
        => string.Create(CultureInfo.InvariantCulture, $"{UnassignedPrefix}_{year}{Extension}");

    /// <summary>
    /// Giá trị header <c>Content-Disposition</c> đúng nguyên văn mục 1.9: có cả <c>filename=</c>
    /// trong ngoặc kép lẫn <c>filename*=UTF-8''</c>. Tự dựng thay vì để MVC sinh, vì MVC bỏ ngoặc
    /// kép khi tên file không có ký tự đặc biệt, còn hợp đồng thì ghi rõ là có.
    /// </summary>
    /// <param name="fileName">Tên file đã sinh.</param>
    /// <returns>Chuỗi header.</returns>
    public static string ContentDisposition(string fileName)
        => $"attachment; filename=\"{fileName}\"; filename*=UTF-8''{Uri.EscapeDataString(fileName ?? string.Empty)}";

    /// <summary>
    /// Rút gọn tên đợt theo mục 1.9: bỏ dấu tiếng Việt (kể cả <c>Đ</c> → <c>D</c>), bỏ khoảng
    /// trắng, mọi ký tự còn lại không phải chữ/số thay bằng <c>-</c>. <c>Đợt 7/11</c> → <c>Dot7-11</c>.
    /// </summary>
    /// <param name="value">Tên đợt gốc.</param>
    /// <returns>Chuỗi chỉ còn chữ ASCII, số và dấu gạch ngang.</returns>
    public static string Slug(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        // Tách dấu thanh và dấu mũ thành ký tự kết hợp riêng rồi bỏ đi; Đ/đ không phân tách được
        // nên phải đổi tay trước.
        string decomposed = value
            .Replace('Đ', 'D')
            .Replace('đ', 'd')
            .Normalize(NormalizationForm.FormD);

        StringBuilder builder = new(decomposed.Length);

        foreach (char character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(character);
                continue;
            }

            // Khoảng trắng biến mất hẳn, ký tự đặc biệt còn lại thành một dấu gạch ngang.
            if (!char.IsWhiteSpace(character))
            {
                builder.Append(Replacement);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
