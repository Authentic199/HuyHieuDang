using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using MiniExcelLibs;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Tải một file Excel do API sinh ra rồi mở lại bằng chính MiniExcel. Bộ kiểm thử mục 8 không
/// khẳng định gì trên đối tượng trong bộ nhớ: mọi khẳng định đều đọc từ file thật đã đi qua
/// đường truyền, đúng tinh thần ca A-710.
/// </summary>
public sealed class ExportedWorkbook
{
    private ExportedWorkbook(string fileName, string? contentType, string contentDisposition, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        FileName = fileName;
        ContentType = contentType;
        ContentDisposition = contentDisposition;
        Rows = rows;
    }

    /// <summary>
    /// Tên file đọc từ header <c>Content-Disposition</c>.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Header <c>Content-Type</c> nguyên văn.
    /// </summary>
    public string? ContentType { get; }

    /// <summary>
    /// Header <c>Content-Disposition</c> nguyên văn, để kiểm cả <c>filename*=UTF-8''</c>.
    /// </summary>
    public string ContentDisposition { get; }

    /// <summary>
    /// Toàn bộ ô của sheet duy nhất, đã quy về chữ; ô rỗng là chuỗi rỗng.
    /// </summary>
    public IReadOnlyList<IReadOnlyList<string>> Rows { get; }

    /// <summary>
    /// Số sheet trong file — phải luôn bằng 1.
    /// </summary>
    public int SheetCount { get; private set; }

    /// <summary>
    /// Gọi một endpoint xuất Excel, khẳng định 200 rồi đọc lại toàn bộ file.
    /// </summary>
    /// <param name="client">Client đã đăng nhập.</param>
    /// <param name="url">Đường dẫn endpoint xuất.</param>
    /// <returns>File đã mở lại.</returns>
    public static async Task<ExportedWorkbook> DownloadAsync(HttpClient client, string url)
    {
        ArgumentNullException.ThrowIfNull(client);

        HttpResponseMessage response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ContentDispositionHeaderValue disposition = response.Content.Headers.ContentDisposition!;
        byte[] bytes = await response.Content.ReadAsByteArrayAsync();

        using MemoryStream stream = new(bytes);

        List<IReadOnlyList<string>> rows = stream
            .Query(useHeaderRow: false, sheetName: ExportSheetNames.Single)
            .Cast<IDictionary<string, object?>>()
            .Select(row => (IReadOnlyList<string>)row.Values.Select(Text).ToList())
            .ToList();

        stream.Seek(0, SeekOrigin.Begin);

        return new ExportedWorkbook(
            disposition.FileName!.Trim('"'),
            response.Content.Headers.ContentType?.ToString(),
            disposition.ToString(),
            rows)
        {
            SheetCount = stream.GetSheetNames().Count,
        };
    }

    /// <summary>
    /// Một ô theo tọa độ, trả chuỗi rỗng khi dòng ngắn hơn.
    /// </summary>
    /// <param name="row">Số dòng, bắt đầu từ 0.</param>
    /// <param name="column">Số cột, bắt đầu từ 0.</param>
    /// <returns>Chữ trong ô.</returns>
    public string Cell(int row, int column)
        => row < Rows.Count && column < Rows[row].Count ? Rows[row][column] : string.Empty;

    /// <summary>
    /// Chữ của ô đầu tiên trên một dòng.
    /// </summary>
    /// <param name="row">Số dòng, bắt đầu từ 0.</param>
    /// <returns>Chữ trong ô đầu dòng.</returns>
    public string Line(int row) => Cell(row, 0);

    /// <summary>
    /// Toàn bộ dòng dữ liệu, tức phần sau dòng đầu cột.
    /// </summary>
    /// <param name="headerRowCount">Số dòng của phần tiêu đề, kể cả dòng đầu cột.</param>
    /// <returns>Các dòng dữ liệu.</returns>
    public IReadOnlyList<IReadOnlyList<string>> DataRows(int headerRowCount)
        => Rows.Skip(headerRowCount).ToList();

    /// <summary>
    /// Quy một ô MiniExcel trả về thành chữ. Ô rỗng cho ra chuỗi rỗng để khẳng định "ô để rỗng"
    /// đọc được trực tiếp (A-707).
    /// </summary>
    /// <param name="value">Giá trị ô.</param>
    /// <returns>Chữ trong ô.</returns>
    private static string Text(object? value) => value switch
    {
        null => string.Empty,
        DateTime date => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };
}

/// <summary>
/// Tên sheet mà bộ kiểm thử mong đợi trong mọi file xuất ra.
/// </summary>
public static class ExportSheetNames
{
    /// <summary>
    /// Sheet duy nhất của file.
    /// </summary>
    public const string Single = "DanhSach";
}
