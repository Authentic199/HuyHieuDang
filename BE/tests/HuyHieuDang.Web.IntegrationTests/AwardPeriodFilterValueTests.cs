using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Modules.Auth.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AwardPeriod = HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities.AwardPeriod;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// QC-T27-05, mặt còn lại: <c>GET /api/AwardPeriods</c> trước đây không đọc tham số
/// <c>filter.*</c> ở bất cứ đâu, nên mọi vế lọc — kể cả vế sai kiểu — bị nuốt lặng lẽ và
/// người gọi nhận về cả kho. Bản sửa T47 chỉ chạm tới danh sách đảng viên nên endpoint này
/// vẫn hở; ca A8 <c>QcT2705</c> đỏ đúng ở dòng đó.
/// <para>
/// Cảnh báo và dải độ phủ cố ý **không** đi theo bộ lọc: chúng mô tả độ phủ của cả năm.
/// </para>
/// </summary>
[Collection(ApiCollection.Name)]
public class AwardPeriodFilterValueTests
{
    private const string BasePath = "/api/AwardPeriods";

    private readonly HuyHieuDangApiFactory factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AwardPeriodFilterValueTests"/> class.
    /// </summary>
    /// <param name="factory">Host kiểm thử dùng chung cho cả bộ.</param>
    public AwardPeriodFilterValueTests(HuyHieuDangApiFactory factory)
    {
        this.factory = factory;
    }

    [Theory(DisplayName = "QC-T27-05 · Giá trị lọc lạ trên danh sách đợt trả 400, không trả cả kho")]
    [InlineData("filter.FromMonth=$eq:khong-phai-so")]
    [InlineData("filter.FromMonth=$eq:")]
    [InlineData("filter.ToDay=$gte:hom-qua")]
    [InlineData("filter.SpansNextYear=$eq:co")]
    [InlineData("filter.Id=$eq:khong-phai-guid")]
    public async Task Search_RejectsFilterValueThatDoesNotMatchTheField(string filter)
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync();

        using HttpResponseMessage response = await client.GetAsync($"{BasePath}?{filter}");
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using JsonDocument document = JsonDocument.Parse(body);
        Assert.Equal(Messages.Common.InvalidParameter, document.RootElement.GetProperty("message").GetString());

        // A-905: phản hồi 400 không được để lọt dấu vết ngăn xếp hay tên lớp nội bộ.
        Assert.DoesNotContain("QueryExpressionExtension", body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "QC-T27-05 · Kho rỗng cũng phải từ chối giá trị lọc sai kiểu")]
    public async Task Search_RejectsFilterValue_EvenWhenThereIsNoPeriodAtAll()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();

        using HttpResponseMessage response = await client.GetAsync($"{BasePath}?filter.FromMonth=$eq:khong-phai-so");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(DisplayName = "5.1 · Giá trị lọc hợp lệ lọc đúng; cảnh báo và độ phủ vẫn tính trên cả năm")]
    public async Task Search_WithValidFilter_NarrowsRowsButKeepsCoverageForTheWholeYear()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync();

        AwardPeriodListPayload all = await GetListAsync(client, BasePath);
        Assert.Equal(3, all.TotalCount);

        AwardPeriodListPayload filtered = await GetListAsync(client, $"{BasePath}?filter.FromMonth=$eq:3");

        Assert.Equal(1, filtered.TotalCount);
        Assert.Equal("Đợt giữa năm", Assert.Single(filtered.Periods).Name);

        // Lọc bớt dòng không làm năm đó hở thêm ngày nào: độ phủ giữ nguyên.
        Assert.Equal(all.Coverage.Segments.Count, filtered.Coverage.Segments.Count);
        Assert.Equal(all.Warnings.Gaps.Count, filtered.Warnings.Gaps.Count);
    }

    [Fact(DisplayName = "5.1 · Tên trường không tồn tại vẫn được bỏ qua, không phải lỗi")]
    public async Task Search_WithUnknownFilterField_IsIgnored()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync();

        AwardPeriodListPayload data = await GetListAsync(client, $"{BasePath}?filter.KhongCoTruongNay=$eq:1");

        Assert.Equal(3, data.TotalCount);
    }

    private static async Task<AwardPeriodListPayload> GetListAsync(HttpClient client, string url)
    {
        using HttpResponseMessage response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiResponse<AwardPeriodListPayload> body = await response.ReadApiResponseAsync<AwardPeriodListPayload>();

        return body.Data!;
    }

    private async Task ResetAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Set<AwardPeriod>().ExecuteDeleteAsync();
    }

    private async Task SeedAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Set<AwardPeriod>().AddRange(
            new AwardPeriod { Name = "Đợt đầu năm", FromDay = 1, FromMonth = 1, ToDay = 28, ToMonth = 2 },
            new AwardPeriod { Name = "Đợt giữa năm", FromDay = 1, FromMonth = 3, ToDay = 30, ToMonth = 6 },
            new AwardPeriod { Name = "Đợt cuối năm", FromDay = 1, FromMonth = 7, ToDay = 31, ToMonth = 12 });

        await dbContext.SaveChangesAsync();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        HttpClient client = factory.CreateClient();

        using HttpResponseMessage login = await client.PostAsJsonAsync(
            "/api/Auth/Login",
            new { username = HuyHieuDangApiFactory.AdminUsername, password = HuyHieuDangApiFactory.AdminPassword });
        login.EnsureSuccessStatusCode();

        ApiResponse<SessionPayload> body = await login.ReadApiResponseAsync<SessionPayload>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TokenTypes.Bearer, body.Data!.AccessToken);

        return client;
    }
}
