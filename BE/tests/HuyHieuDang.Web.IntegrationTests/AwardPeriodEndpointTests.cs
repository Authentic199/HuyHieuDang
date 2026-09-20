using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Modules.Auth.Responses;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Requests;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AwardPeriod = HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities.AwardPeriod;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Nhóm 4 của hợp đồng API (mục 5.1 → 5.5, UC-30 → UC-33, UC-36) chạy trên PostgreSQL thật,
/// với "hôm nay" cố định <see cref="HuyHieuDangApiFactory.FixedToday"/> = 19/09/2026 nên trạng
/// thái đợt và số ngày còn lại trong khẳng định không lệ thuộc ngày chạy.
/// </summary>
[Collection(ApiCollection.Name)]
public class AwardPeriodEndpointTests
{
    private const string BasePath = "/api/AwardPeriods";

    private readonly HuyHieuDangApiFactory factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="AwardPeriodEndpointTests"/> class.
    /// </summary>
    /// <param name="factory">Host kiểm thử dùng chung cho cả lớp.</param>
    public AwardPeriodEndpointTests(HuyHieuDangApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact(DisplayName = "5.1 · Bốn đợt mẫu: sắp theo fromDate, ba trạng thái và số người đủ điều kiện")]
    public async Task Search_SamplePeriods_ReturnsOrderedPeriodsWithStatusAndEligibleCount()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedSampleAsync();

        AwardPeriodListPayload data = await GetListAsync(client, BasePath);

        Assert.Equal(2026, data.Year);
        Assert.Equal(HuyHieuDangApiFactory.FixedToday, data.Today);
        Assert.Equal(4, data.TotalCount);
        Assert.Equal(
            new[] { "Đợt 3/2", "Đợt 19/5", "Đợt 2/9", "Đợt 7/11" },
            data.Periods.Select(x => x.Name).ToArray());

        AwardPeriodPayload past = data.Periods[0];
        Assert.Equal("Past", past.Status);
        Assert.Null(past.DaysRemaining);
        Assert.Equal("01/01", past.FromDisplay);
        Assert.Equal(new DateOnly(2026, 1, 1), past.FromDate);
        Assert.Equal(new DateOnly(2026, 3, 5), past.ToDate);
        Assert.Equal(1, past.EligibleCount);

        // Đợt 19/5 không có ai tròn mốc trong 2026.
        Assert.Equal(0, data.Periods[1].EligibleCount);

        AwardPeriodPayload ongoing = data.Periods[2];
        Assert.Equal("Ongoing", ongoing.Status);
        Assert.Null(ongoing.DaysRemaining);
        Assert.Equal(1, ongoing.EligibleCount);

        AwardPeriodPayload upcoming = data.Periods[3];
        Assert.Equal("Upcoming", upcoming.Status);
        Assert.Equal(12, upcoming.DaysRemaining);
        Assert.Equal(1, upcoming.EligibleCount);
    }

    [Fact(DisplayName = "5.1 · Cảnh báo liệt kê đúng một cặp chồng lấn và một khoảng trống")]
    public async Task Search_SamplePeriods_ReturnsOverlapAndGapWarnings()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedSampleAsync();

        AwardPeriodListPayload data = await GetListAsync(client, BasePath);

        PeriodOverlapPayload overlap = Assert.Single(data.Warnings.Overlaps);
        Assert.Equal("Đợt 19/5", overlap.FirstPeriodName);
        Assert.Equal("Đợt 2/9", overlap.SecondPeriodName);
        Assert.Equal(new DateOnly(2026, 5, 25), overlap.FromDate);
        Assert.Equal(new DateOnly(2026, 5, 31), overlap.ToDate);

        PeriodGapPayload gap = Assert.Single(data.Warnings.Gaps);
        Assert.Equal(new DateOnly(2026, 11, 8), gap.FromDate);
        Assert.Equal(new DateOnly(2026, 12, 31), gap.ToDate);
        Assert.Equal("Đợt 7/11", gap.PreviousPeriodName);
        Assert.Null(gap.NextPeriodName);
    }

    [Fact(DisplayName = "5.1 · Chưa cài đợt nào: cảnh báo gaps rỗng, dải vẫn phủ trọn 01/01–31/12")]
    public async Task Search_WithoutPeriods_ReturnsEmptyGapsButFullYearCoverage()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();

        AwardPeriodListPayload data = await GetListAsync(client, BasePath);

        Assert.Equal(0, data.TotalCount);
        Assert.Empty(data.Warnings.Overlaps);

        // Chưa có đợt nào thì không có gì để khuyên chỉnh lại, nên không báo khoảng trống.
        Assert.Empty(data.Warnings.Gaps);

        // Dải 12 tháng của UC-36 vẫn phải vẽ được: một đoạn trống phủ trọn năm.
        CoverageSegmentPayload segment = Assert.Single(data.Coverage.Segments);
        Assert.Equal(new DateOnly(2026, 1, 1), segment.FromDate);
        Assert.Equal(new DateOnly(2026, 12, 31), segment.ToDate);
    }

    [Fact(DisplayName = "5.1 · Dải độ phủ liền mạch 01/01–31/12, phần chồng lấn thuộc đợt đến trước")]
    public async Task Search_SamplePeriods_ReturnsContinuousCoverage()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedSampleAsync();

        AwardPeriodListPayload data = await GetListAsync(client, BasePath);
        IReadOnlyList<CoverageSegmentPayload> segments = data.Coverage.Segments;

        Assert.Equal(new DateOnly(2026, 1, 1), segments[0].FromDate);
        Assert.Equal(new DateOnly(2026, 12, 31), segments[^1].ToDate);

        for (int index = 1; index < segments.Count; index++)
        {
            Assert.Equal(segments[index - 1].ToDate.AddDays(1), segments[index].FromDate);
        }

        Assert.Equal(new[] { "Period", "Period", "Period", "Period", "Gap" }, segments.Select(x => x.Type).ToArray());
        Assert.Equal(new DateOnly(2026, 6, 1), segments[2].FromDate);
        Assert.Equal("Đợt 2/9", segments[2].Name);
        Assert.Null(segments[^1].PeriodId);
    }

    [Fact(DisplayName = "5.1 · Năm khác: trạng thái vẫn so với hôm nay, ngày gắn đúng năm được hỏi")]
    public async Task Search_WithOtherYear_BindsPeriodsToThatYear()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedSampleAsync();

        AwardPeriodListPayload data = await GetListAsync(client, $"{BasePath}?year=2027");

        Assert.Equal(2027, data.Year);
        Assert.All(data.Periods, x => Assert.Equal(2027, x.Year));
        Assert.All(data.Periods, x => Assert.Equal("Upcoming", x.Status));
        Assert.Equal(new DateOnly(2027, 1, 1), data.Coverage.Segments[0].FromDate);
    }

    [Theory(DisplayName = "5.1 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year")]
    [InlineData("?year=1899")]
    [InlineData("?year=2201")]
    public async Task Search_WithYearOutOfRange_ReturnsInvalidYear(string query)
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response = await client.GetAsync(BasePath + query);

        await AssertErrorAsync(response, Messages<AwardPeriodQueryRequest>.Invalid(x => x.Year));
    }

    [Fact(DisplayName = "5.2 · Lấy một đợt theo id và theo năm được hỏi; id lạ trả NotFound")]
    public async Task GetById_ReturnsPeriodBoundToYear()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedSampleAsync();

        AwardPeriodListPayload list = await GetListAsync(client, BasePath);
        Guid id = list.Periods[3].Id;

        HttpResponseMessage response = await client.GetAsync($"{BasePath}/{id}?year=2028");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        AwardPeriodPayload period = (await response.ReadApiResponseAsync<AwardPeriodPayload>()).Data!;
        Assert.Equal("Đợt 7/11", period.Name);
        Assert.Equal(2028, period.Year);
        Assert.Equal(new DateOnly(2028, 10, 1), period.FromDate);

        await AssertErrorAsync(
            await client.GetAsync($"{BasePath}/{Guid.NewGuid()}"), Messages<AwardPeriod>.NotFound());
    }

    [Fact(DisplayName = "5.3 · Thêm đợt chồng lấn vẫn là 200 và trả kèm cảnh báo")]
    public async Task Create_OverlappingPeriod_SavesAndWarns()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedSampleAsync();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            BasePath,
            new { name = "Đợt chèn thêm", fromDay = 1, fromMonth = 3, toDay = 30, toMonth = 4 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ApiResponse<AwardPeriodMutationPayload> body =
            await response.ReadApiResponseAsync<AwardPeriodMutationPayload>();
        Assert.Equal(Messages<AwardPeriod>.Create(), body.Message);

        AwardPeriodMutationPayload data = body.Data!;
        Assert.Equal("01/03", data.Period.FromDisplay);
        Assert.Equal(2026, data.Period.Year);
        Assert.Equal("Past", data.Period.Status);

        // Đợt chèn thêm 01/03–30/04 chồng lấn cả Đợt 3/2 lẫn Đợt 19/5, cộng cặp sẵn có là ba.
        Assert.Equal(3, data.Warnings.Overlaps.Count);
        Assert.Single(data.Warnings.Gaps);

        AwardPeriodListPayload list = await GetListAsync(client, BasePath);
        Assert.Equal(5, list.TotalCount);
    }

    [Theory(DisplayName = "5.3 · Ba lỗi chặn lưu: thiếu tên, ngày không có thật, đợt vắt qua năm")]
    [InlineData(null, 1, 10, 7, 11, "Mes.AwardPeriod.Required.Name")]
    [InlineData("Đợt 31/04", 31, 4, 7, 11, "Mes.AwardPeriod.Invalid.FromDate")]
    [InlineData("Đợt đến 31/11", 1, 10, 31, 11, "Mes.AwardPeriod.Invalid.ToDate")]
    [InlineData("Đợt vắt qua năm", 15, 12, 20, 1, "Mes.AwardPeriod.Invalid.Range")]
    public async Task Create_WithInvalidBody_ReturnsExpectedKey(
        string? name, int fromDay, int fromMonth, int toDay, int toMonth, string expectedKey)
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            BasePath,
            new { name, fromDay, fromMonth, toDay, toMonth });

        await AssertErrorAsync(response, expectedKey);
    }

    [Fact(DisplayName = "5.3 · Trùng tên không phân biệt hoa thường trả Mes.AwardPeriod.Repeated.Name")]
    public async Task Create_WithRepeatedName_ReturnsRepeatedKey()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedSampleAsync();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            BasePath,
            new { name = "đợt 7/11", fromDay = 1, fromMonth = 10, toDay = 7, toMonth = 11 });

        await AssertErrorAsync(response, Messages<AwardPeriod>.Repeated(x => x.Name));
    }

    [Fact(DisplayName = "5.4 · Sửa đợt: giữ nguyên tên của chính nó, có hiệu lực ngay cho năm hiện tại")]
    public async Task Update_KeepsOwnNameAndRebindsDates()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedSampleAsync();

        AwardPeriodListPayload list = await GetListAsync(client, BasePath);
        Guid id = list.Periods[3].Id;

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"{BasePath}/{id}",
            new { name = "Đợt 7/11", fromDay = 1, fromMonth = 11, toDay = 30, toMonth = 11 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AwardPeriodMutationPayload data =
            (await response.ReadApiResponseAsync<AwardPeriodMutationPayload>()).Data!;

        Assert.Equal(new DateOnly(2026, 11, 1), data.Period.FromDate);
        Assert.Equal("Upcoming", data.Period.Status);
        Assert.Equal(43, data.Period.DaysRemaining);

        // Sửa xong sinh thêm khoảng trống 01/10–31/10 trong chính năm hiện tại (QT6).
        Assert.Equal(2, data.Warnings.Gaps.Count);
    }

    [Fact(DisplayName = "5.4 · Sửa sang tên đợt khác trả Repeated.Name; id lạ trả NotFound")]
    public async Task Update_WithRepeatedNameOrUnknownId_ReturnsExpectedKey()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedSampleAsync();

        AwardPeriodListPayload list = await GetListAsync(client, BasePath);
        object body = new { name = "Đợt 3/2", fromDay = 1, fromMonth = 10, toDay = 7, toMonth = 11 };

        await AssertErrorAsync(
            await client.PutAsJsonAsync($"{BasePath}/{list.Periods[3].Id}", body),
            Messages<AwardPeriod>.Repeated(x => x.Name));

        await AssertErrorAsync(
            await client.PutAsJsonAsync($"{BasePath}/{Guid.NewGuid()}", body),
            Messages<AwardPeriod>.NotFound());
    }

    [Fact(DisplayName = "5.5 · Xóa hẳn đợt, trả khoảng trống mới và không đụng đảng viên (QT10)")]
    public async Task Delete_RemovesPeriodAndKeepsMembers()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedSampleAsync();

        AwardPeriodListPayload list = await GetListAsync(client, BasePath);
        Guid id = list.Periods[0].Id;

        HttpResponseMessage response = await client.DeleteAsync($"{BasePath}/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiResponse<AwardPeriodDeletedPayload> body =
            await response.ReadApiResponseAsync<AwardPeriodDeletedPayload>();
        Assert.Equal(Messages<AwardPeriod>.Delete(), body.Message);
        Assert.Equal(id, body.Data!.Id);

        // Xóa Đợt 3/2 (01/01–05/03) mở ra khoảng trống đầu năm 01/01–05/03.
        PeriodGapPayload firstGap = body.Data.Warnings.Gaps[0];
        Assert.Equal(new DateOnly(2026, 1, 1), firstGap.FromDate);
        Assert.Equal(new DateOnly(2026, 3, 5), firstGap.ToDate);
        Assert.Null(firstGap.PreviousPeriodName);
        Assert.Equal("Đợt 19/5", firstGap.NextPeriodName);

        Assert.Equal(3, (await GetListAsync(client, BasePath)).TotalCount);
        Assert.Equal(4, await CountMembersAsync());

        await AssertErrorAsync(
            await client.DeleteAsync($"{BasePath}/{id}"), Messages<AwardPeriod>.NotFound());
    }

    [Theory(DisplayName = "5.1 → 5.5 · Thiếu token trả 401")]
    [InlineData("GET", BasePath)]
    [InlineData("POST", BasePath)]
    [InlineData("DELETE", BasePath + "/9b7c0f9c-0000-0000-0000-000000000000")]
    public async Task AwardPeriodEndpoint_WithoutToken_ReturnsUnauthorized(string method, string path)
    {
        HttpClient client = factory.CreateClient();

        using HttpRequestMessage request = new(new HttpMethod(method), path);
        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task<AwardPeriodListPayload> GetListAsync(HttpClient client, string url)
    {
        HttpResponseMessage response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiResponse<AwardPeriodListPayload> body = await response.ReadApiResponseAsync<AwardPeriodListPayload>();
        Assert.Equal(Messages<AwardPeriod>.Search(), body.Message);

        return body.Data!;
    }

    private static async Task AssertErrorAsync(HttpResponseMessage response, string expectedKey)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        ApiResponse<object> body = await response.ReadApiResponseAsync<object>();
        Assert.Equal(expectedKey, body.Message);
    }

    /// <summary>
    /// Bốn đợt mẫu của năm 2026: một cặp chồng lấn (Đợt 19/5 và Đợt 2/9 dùng chung 25/05–31/05),
    /// một khoảng trống cuối năm (08/11–31/12) và đủ ba trạng thái so với 19/09/2026.
    /// Kèm bốn đảng viên có ngày tròn mốc rơi vào ba đợt khác nhau.
    /// </summary>
    private async Task SeedSampleAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Set<AwardPeriod>().AddRange(
            new AwardPeriod { Name = "Đợt 3/2", FromDay = 1, FromMonth = 1, ToDay = 5, ToMonth = 3 },
            new AwardPeriod { Name = "Đợt 19/5", FromDay = 6, FromMonth = 3, ToDay = 31, ToMonth = 5 },
            new AwardPeriod { Name = "Đợt 2/9", FromDay = 25, FromMonth = 5, ToDay = 30, ToMonth = 9 },
            new AwardPeriod { Name = "Đợt 7/11", FromDay = 1, FromMonth = 10, ToDay = 7, ToMonth = 11 });

        // Bốn ngày vào Đảng lần lượt tròn 60 năm ngày 10/02/2026 (Đợt 3/2), tròn 40 năm ngày
        // 05/06/2026 (Đợt 2/9), tròn 30 năm ngày 15/10/2026 (Đợt 7/11) và chưa tới mốc nào.
        dbContext.Set<PartyMember>().AddRange(
            new PartyMember { FullName = "Lê Văn Cường", OfficialAdmissionDate = new DateOnly(1966, 2, 10) },
            new PartyMember { FullName = "Trần Thị Bình", OfficialAdmissionDate = new DateOnly(1986, 6, 5) },
            new PartyMember { FullName = "Nguyễn Văn An", OfficialAdmissionDate = new DateOnly(1996, 10, 15) },
            new PartyMember { FullName = "Phạm Thị Dung", OfficialAdmissionDate = new DateOnly(1990, 12, 20) });

        await dbContext.SaveChangesAsync();
    }

    private async Task<int> CountMembersAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.Set<PartyMember>().CountAsync();
    }

    /// <summary>
    /// Dọn cả đợt lẫn đảng viên: số người đủ điều kiện của mỗi đợt tính từ bảng đảng viên nên
    /// dữ liệu sót lại của lớp kiểm thử khác sẽ làm lệch khẳng định.
    /// </summary>
    private async Task ResetAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Set<AwardPeriod>().ExecuteDeleteAsync();
        await dbContext.Set<PartyMember>().ExecuteDeleteAsync();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage login = await client.PostAsJsonAsync(
            "/api/Auth/Login",
            new { username = HuyHieuDangApiFactory.AdminUsername, password = HuyHieuDangApiFactory.AdminPassword });
        login.EnsureSuccessStatusCode();

        ApiResponse<SessionPayload> body = await login.ReadApiResponseAsync<SessionPayload>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(TokenTypes.Bearer, body.Data!.AccessToken);

        return client;
    }
}
