using System.Globalization;
using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports.Models;
using MiniExcelLibs;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports;

/// <summary>
/// Đọc file Excel nhập đảng viên bằng MiniExcel và kiểm tra bốn lỗi cấp file của QT9.
/// Mọi lỗi cấp file ném <see cref="BadRequestException"/> nên bước xem trước và bước nạp
/// chặn giống hệt nhau, đúng mục 4.2 và 4.3 hợp đồng API.
/// </summary>
public static class PartyMemberImportFile
{
    /// <summary>
    /// Phần mở rộng duy nhất được nhận.
    /// </summary>
    public const string AllowedExtension = ".xlsx";

    /// <summary>
    /// Kiểu nội dung trả về khi tải file mẫu (mục 1.9 hợp đồng API).
    /// </summary>
    public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>
    /// Tên file mẫu hệ thống sinh ra (mục 1.9 và 4.1 hợp đồng API).
    /// </summary>
    public const string TemplateFileName = "MauDanhSachDangVien.xlsx";

    /// <summary>
    /// Dung lượng tối đa của file nhập: 10 MB.
    /// </summary>
    public const long MaxFileSizeInBytes = 10L * 1024 * 1024;

    /// <summary>
    /// Bốn tiêu đề bắt buộc, đúng thứ tự này.
    /// </summary>
    public static readonly string[] HeaderTitles =
    {
        "Họ tên",
        "Ngày sinh",
        "Giới tính",
        "Ngày vào Đảng chính thức",
    };

    /// <summary>
    /// Chặn ngay hai lỗi không cần mở file: sai phần mở rộng và quá dung lượng.
    /// </summary>
    /// <param name="fileName">Tên file người dùng gửi lên.</param>
    /// <param name="length">Dung lượng file, tính bằng byte.</param>
    public static void EnsureFileAllowed(string? fileName, long length)
    {
        if (!Path.GetExtension(fileName ?? string.Empty)
                .Equals(AllowedExtension, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException(
                Messages<PartyMemberImport>.Invalid(PartyMemberImport.ExtensionProperty));
        }

        if (length > MaxFileSizeInBytes)
        {
            throw new BadRequestException(
                Messages<PartyMemberImport>.Invalid(PartyMemberImport.FileSizeProperty));
        }
    }

    /// <summary>
    /// Đọc toàn bộ dòng dữ liệu. Dòng tiêu đề là dòng 1 nên dòng dữ liệu đầu tiên mang số 2.
    /// </summary>
    /// <param name="stream">Luồng nội dung file, đã ở đầu luồng.</param>
    /// <returns>Các dòng thô, đã bỏ những dòng trống hoàn toàn.</returns>
    public static IReadOnlyList<ImportRawRow> ReadRows(Stream stream)
    {
        List<IDictionary<string, object?>> sheetRows = QuerySheet(stream);

        // Không đọc được ô nào — kể cả dòng tiêu đề.
        if (sheetRows.Count == 0 || sheetRows[0].Values.All(IsBlankCell))
        {
            throw new BadRequestException(Messages<PartyMemberImport>.Invalid(PartyMemberImport.EmptyProperty));
        }

        EnsureHeaderMatches(sheetRows[0]);

        string[] columnKeys = sheetRows[0].Keys.Take(HeaderTitles.Length).ToArray();

        List<ImportRawRow> rows = sheetRows
            .Skip(1)
            .Select((row, index) => new ImportRawRow(
                index + 2,
                CellText(row, columnKeys[0]),
                CellText(row, columnKeys[1]),
                CellText(row, columnKeys[2]),
                CellText(row, columnKeys[3])))
            .Where(row => !row.IsBlank)
            .ToList();

        // OQ-7: đúng tiêu đề nhưng không có dòng dữ liệu nào là một lỗi riêng, khác file rỗng.
        if (rows.Count == 0)
        {
            throw new BadRequestException(
                Messages<PartyMemberImport>.Invalid(PartyMemberImport.NoDataRowsProperty));
        }

        return rows;
    }

    /// <summary>
    /// Sinh nội dung file mẫu: dòng tiêu đề và hai dòng ví dụ, ngày dạng <c>dd/MM/yyyy</c> (UC-25).
    /// </summary>
    /// <returns>Luồng nhị phân của file mẫu, đã ở đầu luồng.</returns>
    public static Stream BuildTemplate()
    {
        List<Dictionary<string, object>> rows = new()
        {
            BuildTemplateRow("Nguyễn Văn Mẫu", "12/03/1974", "Nam", "01/10/1996"),
            BuildTemplateRow("Trần Thị Mẫu", "05/07/1973", "Nữ", "07/11/1996"),
        };

        MemoryStream stream = new();
        stream.SaveAs(rows, sheetName: "DanhSach");
        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }

    /// <summary>
    /// Một dòng ví dụ của file mẫu, khóa từ điển chính là bốn tiêu đề cột.
    /// </summary>
    /// <param name="fullName">Họ tên ví dụ.</param>
    /// <param name="dateOfBirth">Ngày sinh ví dụ.</param>
    /// <param name="gender">Giới tính ví dụ.</param>
    /// <param name="officialAdmissionDate">Ngày vào Đảng chính thức ví dụ.</param>
    /// <returns>Từ điển bốn cột.</returns>
    private static Dictionary<string, object> BuildTemplateRow(
        string fullName, string dateOfBirth, string gender, string officialAdmissionDate)
        => new()
        {
            [HeaderTitles[0]] = fullName,
            [HeaderTitles[1]] = dateOfBirth,
            [HeaderTitles[2]] = gender,
            [HeaderTitles[3]] = officialAdmissionDate,
        };

    /// <summary>
    /// Nhờ MiniExcel đọc sheet đầu tiên thành các dòng thô, giữ nguyên số cột của sheet.
    /// </summary>
    /// <param name="stream">Luồng nội dung file.</param>
    /// <returns>Các dòng của sheet.</returns>
    private static List<IDictionary<string, object?>> QuerySheet(Stream stream)
    {
        try
        {
            return stream.Query(useHeaderRow: false)
                .Cast<IDictionary<string, object?>>()
                .ToList();
        }
        catch (Exception exception) when (exception is not CustomException)
        {
            // Tệp đổi phần mở rộng thành .xlsx nhưng ruột không phải zip/xlsx: phải là 400, không phải 500.
            throw new BadRequestException(
                Messages<PartyMemberImport>.Invalid(PartyMemberImport.ExtensionProperty), exception);
        }
    }

    /// <summary>
    /// Dòng đầu phải có đúng bốn cột, đúng thứ tự và đúng chữ.
    /// </summary>
    /// <param name="headerRow">Dòng tiêu đề đọc từ sheet.</param>
    private static void EnsureHeaderMatches(IDictionary<string, object?> headerRow)
    {
        string[] titles = headerRow.Values
            .Select(ToText)
            .Reverse()
            .SkipWhile(text => text.Length == 0)
            .Reverse()
            .ToArray();

        bool matches = titles.Length == HeaderTitles.Length
            && !titles.Where((title, index) =>
                !title.Equals(HeaderTitles[index], StringComparison.OrdinalIgnoreCase)).Any();

        if (!matches)
        {
            throw new BadRequestException(
                Messages<PartyMemberImport>.Invalid(PartyMemberImport.ColumnsProperty));
        }
    }

    /// <summary>
    /// Lấy chữ của một ô theo khóa cột.
    /// </summary>
    /// <param name="row">Dòng của sheet.</param>
    /// <param name="columnKey">Khóa cột MiniExcel sinh ra (<c>A</c>, <c>B</c>, ...).</param>
    /// <returns>Chữ trong ô, đã cắt khoảng trắng.</returns>
    private static string CellText(IDictionary<string, object?> row, string columnKey)
        => row.TryGetValue(columnKey, out object? value) ? ToText(value) : string.Empty;

    /// <summary>
    /// Đổi giá trị một ô sang chữ. Ô kiểu ngày của Excel quy về <c>dd/MM/yyyy</c> để cả hai kiểu
    /// file — ngày dạng chuỗi và ngày kiểu ngày — đi chung một đường kiểm tra.
    /// </summary>
    /// <param name="value">Giá trị ô MiniExcel trả về.</param>
    /// <returns>Chữ đã cắt khoảng trắng đầu và cuối (OQ-4).</returns>
    private static string ToText(object? value) => value switch
    {
        null => string.Empty,
        DateTime date => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        DateTimeOffset date => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        DateOnly date => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture).Trim(),
        _ => value.ToString()?.Trim() ?? string.Empty,
    };

    /// <summary>
    /// Ô trống hay chỉ có khoảng trắng.
    /// </summary>
    /// <param name="value">Giá trị ô.</param>
    /// <returns><see langword="true"/> khi ô không mang chữ nào.</returns>
    private static bool IsBlankCell(object? value) => ToText(value).Length == 0;
}
