using System.Globalization;
using MiniExcelLibs;

namespace HuyHieuDang.Web.QcIntegrationTests.Support;

/// <summary>
/// Đọc một file Excel do API sinh ra, bằng đúng một thư viện đọc Excel thật — ca A-710 đòi mở
/// được file mà không có cảnh báo hỏng. Mọi ô đọc dưới dạng chữ, đúng như người dùng nhìn thấy.
/// </summary>
public sealed class QcWorkbook
{
    private readonly List<List<string>> rows;

    private QcWorkbook(List<List<string>> rows)
    {
        this.rows = rows;
    }

    /// <summary>Số dòng đọc được, kể cả dòng tiêu đề.</summary>
    public int RowCount => rows.Count;

    /// <summary>Số sheet trong file.</summary>
    public int SheetCount { get; private init; }

    /// <summary>Đọc một khối byte thành bảng.</summary>
    /// <param name="content">Nội dung file <c>.xlsx</c>.</param>
    /// <returns>Bảng đã đọc.</returns>
    public static QcWorkbook Read(byte[] content)
    {
        ArgumentNullException.ThrowIfNull(content);

        using MemoryStream stream = new(content);

        List<string> sheets = stream.GetSheetNames().ToList();

        stream.Position = 0;
        List<List<string>> rows = stream
            .Query(useHeaderRow: false)
            .Cast<IDictionary<string, object?>>()
            .Select(row => row.Values.Select(Format).ToList())
            .ToList();

        return new QcWorkbook(rows) { SheetCount = sheets.Count };
    }

    /// <summary>Một dòng, đánh số từ 1 như trong Excel, đã cắt các ô trống ở cuối.</summary>
    /// <param name="rowNumber">Số dòng.</param>
    /// <returns>Các ô của dòng.</returns>
    public IReadOnlyList<string> Row(int rowNumber)
    {
        List<string> row = rows[rowNumber - 1];
        int last = row.FindLastIndex(cell => !string.IsNullOrEmpty(cell));

        return row.Take(last + 1).ToList();
    }

    /// <summary>Một ô, đánh số dòng và cột từ 1.</summary>
    /// <param name="rowNumber">Số dòng.</param>
    /// <param name="columnNumber">Số cột.</param>
    /// <returns>Nội dung ô dưới dạng chữ; ô trống là chuỗi rỗng.</returns>
    public string Cell(int rowNumber, int columnNumber)
    {
        List<string> row = rows[rowNumber - 1];

        return columnNumber <= row.Count ? row[columnNumber - 1] : string.Empty;
    }

    /// <summary>Chữ của cả dòng ghép lại, tiện cho việc kiểm dòng tiêu đề.</summary>
    /// <param name="rowNumber">Số dòng.</param>
    /// <returns>Các ô ghép bằng khoảng trắng.</returns>
    public string RowText(int rowNumber)
        => string.Join(" ", rows[rowNumber - 1].Where(cell => !string.IsNullOrEmpty(cell)));

    private static string Format(object? value) => value switch
    {
        null => string.Empty,
        DateTime date => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        DateOnly date => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };
}
