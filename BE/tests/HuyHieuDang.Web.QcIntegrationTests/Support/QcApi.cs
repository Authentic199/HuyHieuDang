using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HuyHieuDang.Web.QcIntegrationTests.Support;

/// <summary>
/// Lớp gọi API của QC. Cố ý đọc thân phản hồi dưới dạng <see cref="JsonElement"/> thay vì
/// tham chiếu kiểu phản hồi của Backend: ca kiểm thử phải khẳng định đúng tên trường trên
/// dây như hợp đồng API viết, chứ không đi qua lớp kiểu có thể cùng sai.
/// </summary>
public static class QcApi
{
    /// <summary>Kiểu nội dung của file Excel (mục 1.9 hợp đồng API).</summary>
    public const string ExcelContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>Client đã đăng nhập, nhìn thấy "hôm nay" là <paramref name="today"/>.</summary>
    /// <param name="factory">Host kiểm thử.</param>
    /// <param name="today">Ngày cần ép; bỏ trống là T0.</param>
    /// <returns>Client đã gắn header <c>Authorization</c>.</returns>
    public static async Task<HttpClient> LoginAsync(QcApiFactory factory, DateOnly? today = null)
    {
        ArgumentNullException.ThrowIfNull(factory);

        WebApplicationFactory<Program> host = factory.At(today ?? QcClock.T0);
        HttpClient client = host.CreateClient();

        HttpResponseMessage login = await client.PostAsJsonAsync(
            "/api/Auth/Login",
            new { username = QcApiFactory.AdminUsername, password = QcApiFactory.AdminPassword });

        login.StatusCode.ShouldBe(HttpStatusCode.OK, await login.Content.ReadAsStringAsync());

        string token = (await ReadAsync(login)).GetProperty("data").GetProperty("accessToken").GetString()!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    /// <summary>Client chưa đăng nhập (A-001).</summary>
    /// <param name="factory">Host kiểm thử.</param>
    /// <param name="today">Ngày cần ép; bỏ trống là T0.</param>
    /// <returns>Client không có header <c>Authorization</c>.</returns>
    public static HttpClient Anonymous(QcApiFactory factory, DateOnly? today = null)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return factory.At(today ?? QcClock.T0).CreateClient();
    }

    /// <summary>Đọc cả thân phản hồi thành cây JSON.</summary>
    /// <param name="response">Phản hồi cần đọc.</param>
    /// <returns>Nút gốc của thân phản hồi.</returns>
    public static async Task<JsonElement> ReadAsync(HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);

        string body = await response.Content.ReadAsStringAsync();

        try
        {
            return JsonDocument.Parse(body).RootElement.Clone();
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException($"Thân phản hồi không phải JSON: {body}", exception);
        }
    }

    /// <summary>Gọi <c>GET</c>, khẳng định <c>200</c> và trả phần <c>data</c>.</summary>
    /// <param name="client">Client đã đăng nhập.</param>
    /// <param name="url">Đường dẫn cần gọi.</param>
    /// <returns>Phần <c>data</c>.</returns>
    public static async Task<JsonElement> GetDataAsync(HttpClient client, string url)
    {
        ArgumentNullException.ThrowIfNull(client);

        HttpResponseMessage response = await client.GetAsync(url);
        string body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.OK, $"GET {url} → {body}");

        return JsonDocument.Parse(body).RootElement.Clone().GetProperty("data");
    }

    /// <summary>Gọi <c>POST</c> JSON, khẳng định <c>200</c> và trả phần <c>data</c>.</summary>
    /// <param name="client">Client đã đăng nhập.</param>
    /// <param name="url">Đường dẫn cần gọi.</param>
    /// <param name="payload">Thân yêu cầu.</param>
    /// <returns>Phần <c>data</c>.</returns>
    public static async Task<JsonElement> PostDataAsync(HttpClient client, string url, object payload)
    {
        ArgumentNullException.ThrowIfNull(client);

        HttpResponseMessage response = await client.PostAsJsonAsync(url, payload);
        string body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.OK, $"POST {url} → {body}");

        return JsonDocument.Parse(body).RootElement.Clone().GetProperty("data");
    }

    /// <summary>Gọi <c>PUT</c> JSON, khẳng định <c>200</c> và trả phần <c>data</c>.</summary>
    /// <param name="client">Client đã đăng nhập.</param>
    /// <param name="url">Đường dẫn cần gọi.</param>
    /// <param name="payload">Thân yêu cầu.</param>
    /// <returns>Phần <c>data</c>.</returns>
    public static async Task<JsonElement> PutDataAsync(HttpClient client, string url, object payload)
    {
        ArgumentNullException.ThrowIfNull(client);

        HttpResponseMessage response = await client.PutAsJsonAsync(url, payload);
        string body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(HttpStatusCode.OK, $"PUT {url} → {body}");

        return JsonDocument.Parse(body).RootElement.Clone().GetProperty("data");
    }

    /// <summary>
    /// Khẳng định một phản hồi lỗi mang đúng mã trạng thái và đúng khóa thông điệp, đồng thời
    /// không để lọt stack trace hay tên bảng/cột ra ngoài (A-905).
    /// </summary>
    /// <param name="response">Phản hồi cần kiểm tra.</param>
    /// <param name="expectedStatus">Mã trạng thái mong đợi.</param>
    /// <param name="expectedKey">Khóa thông điệp mong đợi; bỏ trống thì chỉ kiểm mã trạng thái.</param>
    /// <returns>Thân phản hồi, để ca gọi khẳng định thêm.</returns>
    public static async Task<JsonElement> AssertErrorAsync(
        HttpResponseMessage response, HttpStatusCode expectedStatus, string? expectedKey = null)
    {
        ArgumentNullException.ThrowIfNull(response);

        string body = await response.Content.ReadAsStringAsync();
        response.StatusCode.ShouldBe(expectedStatus, body);

        JsonElement root = JsonDocument.Parse(body).RootElement.Clone();

        if (expectedKey is not null)
        {
            root.GetProperty("message").GetString().ShouldBe(expectedKey, body);
        }

        QcMessages.ShouldNotLeakInternals(body);

        return root;
    }

    /// <summary>Đọc một chuỗi có thể vắng mặt hoặc <c>null</c>.</summary>
    /// <param name="element">Nút cha.</param>
    /// <param name="name">Tên trường.</param>
    /// <returns>Giá trị chuỗi hoặc <see langword="null"/>.</returns>
    public static string? StringOrNull(this JsonElement element, string name)
        => element.TryGetProperty(name, out JsonElement value) && value.ValueKind is not JsonValueKind.Null
            ? value.GetString()
            : null;

    /// <summary>Đọc một số nguyên bắt buộc.</summary>
    /// <param name="element">Nút cha.</param>
    /// <param name="name">Tên trường.</param>
    /// <returns>Giá trị số.</returns>
    public static int Int(this JsonElement element, string name) => element.GetProperty(name).GetInt32();

    /// <summary>Đọc một số nguyên có thể <c>null</c>.</summary>
    /// <param name="element">Nút cha.</param>
    /// <param name="name">Tên trường.</param>
    /// <returns>Giá trị số hoặc <see langword="null"/>.</returns>
    public static int? IntOrNull(this JsonElement element, string name)
        => element.TryGetProperty(name, out JsonElement value) && value.ValueKind is not JsonValueKind.Null
            ? value.GetInt32()
            : null;

    /// <summary>Đọc một chuỗi bắt buộc.</summary>
    /// <param name="element">Nút cha.</param>
    /// <param name="name">Tên trường.</param>
    /// <returns>Giá trị chuỗi.</returns>
    public static string Str(this JsonElement element, string name) => element.GetProperty(name).GetString()!;

    /// <summary>Đọc một mảng thành danh sách nút.</summary>
    /// <param name="element">Nút cha.</param>
    /// <param name="name">Tên trường.</param>
    /// <returns>Danh sách phần tử.</returns>
    public static IReadOnlyList<JsonElement> Array(this JsonElement element, string name)
        => element.GetProperty(name).EnumerateArray().ToList();
}
