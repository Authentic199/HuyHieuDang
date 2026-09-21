using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Requests;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Năm lỗi QC phát hiện ở T27 quanh tham số truy vấn: số trang tràn <see cref="int"/>,
/// <c>pageSize</c> không trần, lỗi ép kiểu trả câu tiếng Anh và ô xem trước dãy mốc không trần.
/// Mọi khẳng định ở đây đều gọi API thật trên PostgreSQL thật.
/// </summary>
[Collection(ApiCollection.Name)]
public class QueryParameterGuardTests
{
    private const string MembersPath = "/api/PartyMembers";

    private readonly HuyHieuDangApiFactory factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryParameterGuardTests"/> class.
    /// </summary>
    /// <param name="factory">Host kiểm thử dùng chung cho cả lớp.</param>
    public QueryParameterGuardTests(HuyHieuDangApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact(DisplayName = "QC-T27-02 · Số trang rất lớn trả 200 với danh sách rỗng, không 500")]
    public async Task Search_ReturnsEmptyPage_WhenCurrentIsHugeInsteadOfCrashing()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync(5);

        PagedPayload<PartyMemberPayload> page =
            await ApiClientFactory.GetDataAsync<PagedPayload<PartyMemberPayload>>(
                client, $"{MembersPath}?current=1000000000&pageSize=20");

        Assert.Empty(page.PagedData);
        Assert.Equal(5, page.PageInfo.TotalCount);
        Assert.Equal(20, page.PageInfo.PageSize);
        Assert.False(page.PageInfo.HasNext);
    }

    [Fact(DisplayName = "QC-T27-02 · Số trang nhỏ hơn 1 được coi như trang 1")]
    public async Task Search_TreatsNonPositiveCurrentAsTheFirstPage()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync(3);

        foreach (string query in new[] { "?current=0&pageSize=2", "?current=-5&pageSize=2" })
        {
            PagedPayload<PartyMemberPayload> page =
                await ApiClientFactory.GetDataAsync<PagedPayload<PartyMemberPayload>>(client, MembersPath + query);

            Assert.Equal(1, page.PageInfo.Current);
            Assert.Equal(2, page.PagedData.Count);
            Assert.Equal("Nguoi 01", page.PagedData[0].FullName);
        }
    }

    [Fact(DisplayName = "QC-T27-03 · pageSize vượt trần bị kẹp về 200, không báo lỗi")]
    public async Task Search_ClampsPageSizeToTheCeiling()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync(QueryContainer.MaxPageSize + 12);

        PagedPayload<PartyMemberPayload> page =
            await ApiClientFactory.GetDataAsync<PagedPayload<PartyMemberPayload>>(
                client, $"{MembersPath}?pageSize=1000000000");

        Assert.Equal(QueryContainer.MaxPageSize, page.PageInfo.PageSize);
        Assert.Equal(QueryContainer.MaxPageSize, page.PagedData.Count);
        Assert.Equal(QueryContainer.MaxPageSize + 12, page.PageInfo.TotalCount);
        Assert.True(page.PageInfo.HasNext);
    }

    [Fact(DisplayName = "QC-T27-03 · pageSize nhỏ hơn 1 lấy mặc định 20")]
    public async Task Search_FallsBackToTheDefaultPageSize()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync(25);

        PagedPayload<PartyMemberPayload> page =
            await ApiClientFactory.GetDataAsync<PagedPayload<PartyMemberPayload>>(
                client, $"{MembersPath}?pageSize=0");

        Assert.Equal(QueryContainer.DefaultPageSize, page.PageInfo.PageSize);
        Assert.Equal(QueryContainer.DefaultPageSize, page.PagedData.Count);
    }

    [Theory(DisplayName = "QC-T27-04 · Lỗi ép kiểu tham số trả khóa chung, không trả câu tiếng Anh")]
    [InlineData("/api/AwardPeriods?year=2147483648")]
    [InlineData("/api/AwardPeriods?year=khong-phai-so")]
    [InlineData("/api/Eligibility?awardPeriodId=khong-phai-guid")]
    [InlineData("/api/PartyMembers?current=khong-phai-so")]
    public async Task BindingFailure_ReturnsTheSharedMessageKey(string url)
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response = await client.GetAsync(url);
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(Messages.Common.InvalidParameter, (await response.ReadApiResponseAsync<object>()).Message);

        // Không được lọt tên tham số hay giá trị người dùng nhập ra ngoài.
        Assert.DoesNotContain("is not valid", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("khong-phai", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "QC-T27-04 · Khóa của FluentValidation vẫn đi thẳng ra ngoài")]
    public async Task ValidationFailure_KeepsItsOwnMessageKey()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response = await client.GetAsync("/api/AwardPeriods?year=1800");

        await ApiClientFactory.AssertErrorAsync(
            response, Messages<AwardPeriodQueryRequest>.Invalid(x => x.Year));
    }

    [Fact(DisplayName = "QC-T27-06 · Xem trước dãy mốc từ chối khoảng vượt trần, đúng khóa của PUT")]
    public async Task PreviewMilestones_RejectsAnUnboundedRange()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response =
            await client.GetAsync("/api/Settings/Milestones?start=1&end=1000000&step=1");

        await ApiClientFactory.AssertErrorAsync(response, Messages<AppSetting>.Invalid(x => x.EndYears));
    }

    [Fact(DisplayName = "QC-T27-06 · Xem trước và lưu dùng đúng một bộ luật")]
    public async Task PreviewMilestones_AndUpdate_ShareTheSameRules()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        string expected = Messages<AppSetting>.Invalid(x => x.EndYears);

        HttpResponseMessage preview =
            await client.GetAsync("/api/Settings/Milestones?start=1&end=1000000&step=1");
        HttpResponseMessage update = await client.PutAsJsonAsync(
            "/api/Settings",
            new { startYears = 1, endYears = 1000000, stepYears = 1 });

        await ApiClientFactory.AssertErrorAsync(preview, expected);
        await ApiClientFactory.AssertErrorAsync(update, expected);
    }

    [Fact(DisplayName = "QC-T27-06 · Khoảng hợp lệ vẫn xem trước được bình thường")]
    public async Task PreviewMilestones_StillAnswersAValidRange()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        MilestonePreviewPayload preview =
            await ApiClientFactory.GetDataAsync<MilestonePreviewPayload>(
                client, "/api/Settings/Milestones?start=30&end=90&step=10");

        Assert.Equal(7, preview.MilestoneCount);
        Assert.Equal(new[] { 30, 40, 50, 60, 70, 80, 90 }, preview.Milestones);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
        => await ApiClientFactory.CreateAuthenticatedAsync(factory);

    private async Task ResetAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Set<PartyMember>().ExecuteDeleteAsync();
    }

    private async Task SeedAsync(int count)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Set<PartyMember>().AddRange(Enumerable.Range(1, count).Select(i => new PartyMember
        {
            FullName = $"Nguoi {i:D2}",
            OfficialAdmissionDate = new DateOnly(1990, 1, 1),
        }));

        await context.SaveChangesAsync();
    }
}
