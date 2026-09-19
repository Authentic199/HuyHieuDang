using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.Auth.Responses;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Dựng <see cref="HttpClient"/> đã đăng nhập, kèm khả năng đổi "hôm nay" cho từng bài kiểm thử.
/// Nhóm tính toán phải chứng minh cả ba mốc thời gian T0 / T1 / T2 của bộ dữ liệu QC, trong khi
/// host dùng chung chỉ cố định được một ngày, nên bài nào cần mốc khác thì dựng một host dẫn xuất
/// — vẫn trỏ vào đúng container cơ sở dữ liệu đó.
/// </summary>
public static class ApiClientFactory
{
    /// <summary>
    /// Mốc T1 của bộ dữ liệu QC: hôm nay nằm trong Đợt 7/11.
    /// </summary>
    public static readonly DateOnly T1 = new(2026, 10, 15);

    /// <summary>
    /// Mốc T2 của bộ dữ liệu QC: mọi đợt của 2026 đã qua.
    /// </summary>
    public static readonly DateOnly T2 = new(2026, 12, 1);

    /// <summary>
    /// Đăng nhập bằng tài khoản admin được seed và gắn token vào header.
    /// </summary>
    /// <param name="factory">Host kiểm thử.</param>
    /// <param name="today">Ngày hôm nay của host; bỏ trống thì dùng T0 của host dùng chung.</param>
    /// <returns>Client đã có <c>Authorization</c>.</returns>
    public static async Task<HttpClient> CreateAuthenticatedAsync(
        HuyHieuDangApiFactory factory, DateOnly? today = null)
    {
        ArgumentNullException.ThrowIfNull(factory);

        HttpClient client = today is null
            ? factory.CreateClient()
            : factory
                .WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
                    services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(today.Value))))
                .CreateClient();

        HttpResponseMessage login = await client.PostAsJsonAsync(
            "/api/Auth/Login",
            new { username = HuyHieuDangApiFactory.AdminUsername, password = HuyHieuDangApiFactory.AdminPassword });
        login.EnsureSuccessStatusCode();

        ApiResponse<SessionPayload> body = await login.ReadApiResponseAsync<SessionPayload>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(TokenTypes.Bearer, body.Data!.AccessToken);

        return client;
    }

    /// <summary>
    /// Gọi một endpoint <c>GET</c> và đọc phần <c>data</c>, khẳng định mã trạng thái 200.
    /// </summary>
    /// <typeparam name="TData">Kiểu của phần <c>data</c>.</typeparam>
    /// <param name="client">Client đã đăng nhập.</param>
    /// <param name="url">Đường dẫn cần gọi.</param>
    /// <returns>Phần <c>data</c> của phản hồi.</returns>
    public static async Task<TData> GetDataAsync<TData>(HttpClient client, string url)
    {
        ArgumentNullException.ThrowIfNull(client);

        HttpResponseMessage response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return (await response.ReadApiResponseAsync<TData>()).Data!;
    }

    /// <summary>
    /// Khẳng định một phản hồi lỗi <c>400</c> mang đúng khóa thông điệp.
    /// </summary>
    /// <param name="response">Phản hồi cần kiểm tra.</param>
    /// <param name="expectedKey">Khóa mong đợi.</param>
    public static async Task AssertErrorAsync(HttpResponseMessage response, string expectedKey)
    {
        ArgumentNullException.ThrowIfNull(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedKey, (await response.ReadApiResponseAsync<object>()).Message);
    }
}
