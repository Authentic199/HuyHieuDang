using System.Text.Json.Serialization;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Trỏ tới bộ tệp Excel dùng chung của QC ở <c>tests/fixtures/excel/</c>. Bộ kiểm thử tích hợp
/// chạy qua chính những tệp này, không tự dựng tệp khác thay thế.
/// </summary>
public static class ImportFixtures
{
    /// <summary>
    /// Đường dẫn thư mục <c>tests/fixtures/excel</c> của kho mã.
    /// </summary>
    public static string ExcelDirectory { get; } = Locate();

    /// <summary>
    /// Lấy đường dẫn đầy đủ của một tệp fixture.
    /// </summary>
    /// <param name="fileName">Tên tệp, ví dụ <c>loi-4-dong.xlsx</c>.</param>
    /// <returns>Đường dẫn đầy đủ.</returns>
    public static string Path(string fileName) => System.IO.Path.Combine(ExcelDirectory, fileName);

    /// <summary>
    /// Đọc toàn bộ nội dung một tệp fixture.
    /// </summary>
    /// <param name="fileName">Tên tệp.</param>
    /// <returns>Nội dung nhị phân.</returns>
    public static byte[] Read(string fileName) => File.ReadAllBytes(Path(fileName));

    /// <summary>
    /// Đi ngược cây thư mục từ nơi chạy bản dựng cho tới khi gặp <c>tests/fixtures/excel</c>.
    /// </summary>
    /// <returns>Đường dẫn thư mục fixture.</returns>
    private static string Locate()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string candidate = System.IO.Path.Combine(directory.FullName, "tests", "fixtures", "excel");

            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Không tìm thấy thư mục tests/fixtures/excel trong kho mã.");
    }
}

/// <summary>
/// Phần <c>data</c> của <c>POST /api/PartyMembers/Import/Preview</c> (mục 4.2 hợp đồng API).
/// </summary>
public sealed class ImportPreviewPayload
{
    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }

    [JsonPropertyName("totalRows")]
    public int TotalRows { get; set; }

    [JsonPropertyName("validCount")]
    public int ValidCount { get; set; }

    [JsonPropertyName("errorCount")]
    public int ErrorCount { get; set; }

    [JsonPropertyName("validRows")]
    public List<ImportValidRowPayload> ValidRows { get; set; } = new();

    [JsonPropertyName("errorRows")]
    public List<ImportErrorRowPayload> ErrorRows { get; set; } = new();
}

/// <summary>
/// Một dòng hợp lệ trong kết quả xem trước.
/// </summary>
public sealed class ImportValidRowPayload
{
    [JsonPropertyName("rowNumber")]
    public int RowNumber { get; set; }

    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    [JsonPropertyName("dateOfBirth")]
    public DateOnly? DateOfBirth { get; set; }

    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("officialAdmissionDate")]
    public DateOnly OfficialAdmissionDate { get; set; }
}

/// <summary>
/// Một dòng lỗi trong kết quả xem trước; bốn trường giữ nguyên chữ thô.
/// </summary>
public sealed class ImportErrorRowPayload
{
    [JsonPropertyName("rowNumber")]
    public int RowNumber { get; set; }

    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    [JsonPropertyName("dateOfBirth")]
    public string? DateOfBirth { get; set; }

    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("officialAdmissionDate")]
    public string? OfficialAdmissionDate { get; set; }

    [JsonPropertyName("errors")]
    public List<ImportRowErrorPayload> Errors { get; set; } = new();
}

/// <summary>
/// Một lý do của dòng lỗi.
/// </summary>
public sealed class ImportRowErrorPayload
{
    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("field")]
    public string? Field { get; set; }
}

/// <summary>
/// Phần <c>data</c> của <c>POST /api/PartyMembers/Import/Commit</c> (mục 4.3 hợp đồng API).
/// </summary>
public sealed class ImportCommitPayload
{
    [JsonPropertyName("importedCount")]
    public int ImportedCount { get; set; }

    [JsonPropertyName("skippedCount")]
    public int SkippedCount { get; set; }
}
