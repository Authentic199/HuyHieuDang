using System.IO.Compression;
using System.Net.Http.Headers;

namespace HuyHieuDang.Web.QcIntegrationTests.Support;

/// <summary>
/// Gửi file lên hai endpoint import và dựng những file biên mà <c>tests/fixtures/excel</c>
/// không chứa sẵn (file quá 10 MB và file ngay dưới ngưỡng) — chúng quá lớn để đưa vào kho mã
/// nên sinh tại chỗ lúc chạy.
/// </summary>
public static class QcUpload
{
    /// <summary>Ngưỡng dung lượng của QT9.</summary>
    public const long MaxFileSizeInBytes = 10L * 1024 * 1024;

    /// <summary>Gửi một file trên đĩa lên endpoint import.</summary>
    /// <param name="client">Client đã đăng nhập.</param>
    /// <param name="url">Đường dẫn endpoint.</param>
    /// <param name="path">Đường dẫn file.</param>
    /// <param name="cancellationToken">Thẻ hủy, dùng cho ca cắt kết nối giữa chừng.</param>
    /// <returns>Phản hồi thô.</returns>
    public static Task<HttpResponseMessage> SendFileAsync(
        HttpClient client, string url, string path, CancellationToken cancellationToken = default)
        => SendBytesAsync(client, url, File.ReadAllBytes(path), Path.GetFileName(path), cancellationToken);

    /// <summary>Gửi một khối byte như một file.</summary>
    /// <param name="client">Client đã đăng nhập.</param>
    /// <param name="url">Đường dẫn endpoint.</param>
    /// <param name="content">Nội dung file.</param>
    /// <param name="fileName">Tên file gửi kèm.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Phản hồi thô.</returns>
    public static async Task<HttpResponseMessage> SendBytesAsync(
        HttpClient client,
        string url,
        byte[] content,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);

        using MultipartFormDataContent form = new();
        ByteArrayContent part = new(content);
        part.Headers.ContentType = new MediaTypeHeaderValue(QcApi.ExcelContentType);
        form.Add(part, "file", fileName);

        return await client.PostAsync(url, form, cancellationToken);
    }

    /// <summary>Gửi hai file cùng lúc trong một biểu mẫu (A-615).</summary>
    /// <param name="client">Client đã đăng nhập.</param>
    /// <param name="url">Đường dẫn endpoint.</param>
    /// <param name="firstPath">File thứ nhất.</param>
    /// <param name="secondPath">File thứ hai.</param>
    /// <returns>Phản hồi thô.</returns>
    public static async Task<HttpResponseMessage> SendTwoFilesAsync(
        HttpClient client, string url, string firstPath, string secondPath)
    {
        ArgumentNullException.ThrowIfNull(client);

        using MultipartFormDataContent form = new();

        foreach (string path in new[] { firstPath, secondPath })
        {
            ByteArrayContent part = new(File.ReadAllBytes(path));
            part.Headers.ContentType = new MediaTypeHeaderValue(QcApi.ExcelContentType);
            form.Add(part, "file", Path.GetFileName(path));
        }

        return await client.PostAsync(url, form);
    }

    /// <summary>
    /// Một khối byte lớn hơn 10 MB mang tên <c>.xlsx</c> nhưng **không** phải file Excel hợp lệ:
    /// nếu máy chủ trả đúng lỗi dung lượng thì chứng tỏ nó chặn trước khi đọc nội dung (A-612).
    /// </summary>
    /// <returns>Khối byte 11 MB.</returns>
    public static byte[] OversizedBytes()
    {
        byte[] content = new byte[11L * 1024 * 1024];
        Random.Shared.NextBytes(content);

        return content;
    }

    /// <summary>
    /// Một file <c>.xlsx</c> **hợp lệ** nặng ngay dưới 10 MB: lấy file lõi rồi nhét thêm một
    /// phần phụ không nén vào trong gói zip, phần mà trình đọc Excel bỏ qua (A-613).
    /// </summary>
    /// <param name="targetSize">Dung lượng đích, tính bằng byte.</param>
    /// <returns>Đường dẫn file tạm vừa dựng.</returns>
    public static string BuildLargeValidWorkbook(long targetSize)
    {
        string path = Path.Combine(Path.GetTempPath(), $"qc-a613-{Guid.NewGuid():N}.xlsx");
        File.Copy(QcPaths.Excel("core-hop-le.xlsx"), path);

        long padding = targetSize - new FileInfo(path).Length;

        using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Update))
        {
            ZipArchiveEntry entry = archive.CreateEntry("docProps/qc-padding.bin", CompressionLevel.NoCompression);
            using Stream stream = entry.Open();

            byte[] block = new byte[64 * 1024];
            Random.Shared.NextBytes(block);

            long written = 0;

            while (written < padding)
            {
                int size = (int)Math.Min(block.Length, padding - written);
                stream.Write(block, 0, size);
                written += size;
            }
        }

        return path;
    }
}
