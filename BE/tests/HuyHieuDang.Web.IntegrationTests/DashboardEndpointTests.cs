using System.Globalization;
using System.Net;
using System.Text.Json;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Mục 6.1 hợp đồng API (UC-10 → UC-13, QT5, QT6, QT7, QT8, QT11). Ba mốc thời gian T0, T1, T2
/// của bộ dữ liệu QC đều được chứng minh, kể cả nhánh "đợt sắp tới thuộc năm sau" của QT8.
/// </summary>
[Collection(ApiCollection.Name)]
public class DashboardEndpointTests
{
    private const string BasePath = "/api/Dashboard";

    private readonly HuyHieuDangApiFactory factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardEndpointTests"/> class.
    /// </summary>
    /// <param name="factory">Host kiểm thử dùng chung cho cả lớp.</param>
    public DashboardEndpointTests(HuyHieuDangApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact(DisplayName = "6.1 · T0 = 19/09/2026: đợt sắp tới là Đợt 7/11 · 2026, còn 12 ngày, 6 người")]
    public async Task GetDashboard_AtT0_MatchesExpectedFixture()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        JsonElement scenario = CoreFixtures.Scenario("core_default_T0");
        DashboardPayload data = await ApiClientFactory.GetDataAsync<DashboardPayload>(client, BasePath);

        Assert.Equal(HuyHieuDangApiFactory.FixedToday, data.Today);
        Assert.Equal(2026, data.CurrentYear);
        Assert.Equal(CoreFixtures.Settings["default"].UnitName, data.UnitName);
        Assert.Equal(CoreFixtures.CoreMembers.Count, data.MemberCount);
        Assert.Equal(CoreFixtures.MainPeriods().Count, data.PeriodCount);

        UpcomingPeriodPayload upcoming = data.UpcomingPeriod!;
        JsonElement expectedUpcoming = scenario.GetProperty("upcomingPeriod");
        Assert.Equal(expectedUpcoming.GetProperty("name").GetString(), upcoming.Name);
        Assert.Equal(expectedUpcoming.GetProperty("year").GetInt32(), upcoming.Year);
        Assert.False(upcoming.IsNextYear);
        Assert.Equal(expectedUpcoming.GetProperty("boundFrom").GetString(), Iso(upcoming.FromDate));
        Assert.Equal(expectedUpcoming.GetProperty("boundTo").GetString(), Iso(upcoming.ToDate));
        Assert.Equal("01/10", upcoming.FromDisplay);
        Assert.Equal("07/11", upcoming.ToDisplay);
        Assert.Equal("Upcoming", upcoming.Status);
        Assert.Equal(expectedUpcoming.GetProperty("daysLeft").GetInt32(), upcoming.DaysRemaining);

        JsonElement expectedRows = scenario
            .GetProperty("eligibleByPeriod").GetProperty("P4").GetProperty("byYear").GetProperty("2026");
        Assert.Equal(expectedRows.GetProperty("total").GetInt32(), upcoming.EligibleCount);
        Assert.Equal(
            new[] { (30, 3), (35, 1), (40, 1), (45, 1) },
            upcoming.MilestoneBreakdown.Select(x => (x.Milestone, x.Count)).ToArray());
        Assert.Equal(
            expectedRows.GetProperty("rows").EnumerateArray().Select(x => x.GetProperty("fullName").GetString()),
            data.EligibleMembers.Select(x => x.FullName));

        DashboardWarningsPayload warnings = data.Warnings;
        Assert.False(warnings.NoMembers);
        Assert.False(warnings.NoPeriods);
        Assert.Equal(2026, warnings.UnassignedYear);
        Assert.Equal(scenario.GetProperty("badgeCurrentYear").GetInt32(), warnings.UnassignedCount);
        Assert.Empty(warnings.Overlaps);
        AssertGaps(scenario.GetProperty("gapsByYear").GetProperty("2026"), warnings.Gaps);
    }

    [Fact(DisplayName = "6.1 · T1 = 15/10/2026: Đợt 7/11 đang diễn ra, daysRemaining = null")]
    public async Task GetDashboard_AtT1_ReportsOngoingPeriod()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory, ApiClientFactory.T1);
        await CoreDataset.SeedAllAsync(factory);

        JsonElement expected = CoreFixtures.Scenario("core_default_T1").GetProperty("upcomingPeriod");
        DashboardPayload data = await ApiClientFactory.GetDataAsync<DashboardPayload>(client, BasePath);

        Assert.Equal(ApiClientFactory.T1, data.Today);
        Assert.Equal(expected.GetProperty("name").GetString(), data.UpcomingPeriod!.Name);
        Assert.Equal("Ongoing", data.UpcomingPeriod.Status);
        Assert.Null(data.UpcomingPeriod.DaysRemaining);
        Assert.False(data.UpcomingPeriod.IsNextYear);
        Assert.Equal(6, data.UpcomingPeriod.EligibleCount);
    }

    [Fact(DisplayName = "6.1 · T2 = 01/12/2026: mọi đợt 2026 đã qua nên đợt sắp tới là Đợt 3/2 · 2027 (QT8)")]
    public async Task GetDashboard_AtT2_RollsOverToNextYear()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory, ApiClientFactory.T2);
        await CoreDataset.SeedAllAsync(factory);

        JsonElement scenario = CoreFixtures.Scenario("core_default_T2");
        JsonElement expected = scenario.GetProperty("upcomingPeriod");
        DashboardPayload data = await ApiClientFactory.GetDataAsync<DashboardPayload>(client, BasePath);

        Assert.Equal(ApiClientFactory.T2, data.Today);
        Assert.Equal(2026, data.CurrentYear);

        UpcomingPeriodPayload upcoming = data.UpcomingPeriod!;
        Assert.Equal(expected.GetProperty("name").GetString(), upcoming.Name);
        Assert.Equal(2027, upcoming.Year);
        Assert.True(upcoming.IsNextYear);
        Assert.Equal(expected.GetProperty("boundFrom").GetString(), Iso(upcoming.FromDate));
        Assert.Equal(expected.GetProperty("boundTo").GetString(), Iso(upcoming.ToDate));
        Assert.Equal("Upcoming", upcoming.Status);
        Assert.Equal(expected.GetProperty("daysLeft").GetInt32(), upcoming.DaysRemaining);

        // Danh sách đi kèm là của chính lần diễn ra 2027, không phải của 2026.
        JsonElement expectedRows = scenario
            .GetProperty("eligibleByPeriod").GetProperty("P1").GetProperty("byYear").GetProperty("2027");
        Assert.Equal(expectedRows.GetProperty("total").GetInt32(), upcoming.EligibleCount);
        Assert.Equal(upcoming.EligibleCount, data.EligibleMembers.Count);

        // Cảnh báo vẫn nói về năm hiện tại (2026).
        Assert.Equal(2026, data.Warnings.UnassignedYear);
        Assert.Equal(scenario.GetProperty("badgeCurrentYear").GetInt32(), data.Warnings.UnassignedCount);
    }

    [Fact(DisplayName = "6.1 · Chưa cài đợt nào: upcomingPeriod = null, bảng rỗng, cảnh báo noPeriods")]
    public async Task GetDashboard_WithoutPeriods_ReturnsNullUpcomingPeriod()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.ResetAsync(factory);
        await CoreDataset.SeedMembersAsync(factory);

        DashboardPayload data = await ApiClientFactory.GetDataAsync<DashboardPayload>(client, BasePath);

        Assert.Null(data.UpcomingPeriod);
        Assert.Empty(data.EligibleMembers);
        Assert.Equal(0, data.PeriodCount);
        Assert.True(data.Warnings.NoPeriods);
        Assert.False(data.Warnings.NoMembers);

        // Chưa có đợt nào thì không kèm cảnh báo "chưa phủ kín": chỉ còn "Chưa cài đợt trao
        // huy hiệu". Người tròn mốc trong năm vẫn bị tính là sót (QT7).
        Assert.Empty(data.Warnings.Gaps);
        Assert.Equal(
            CoreFixtures.Scenario("core_default_T0_noPeriod")
                .GetProperty("missedByYear").GetProperty("2026").GetProperty("total").GetInt32(),
            data.Warnings.UnassignedCount);
    }

    [Fact(DisplayName = "6.1 · Chưa có đảng viên nào: cảnh báo noMembers, đợt sắp tới vẫn có, 0 người")]
    public async Task GetDashboard_WithoutMembers_ReportsNoMembers()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.ResetAsync(factory);
        await CoreDataset.SeedPeriodsAsync(factory);

        DashboardPayload data = await ApiClientFactory.GetDataAsync<DashboardPayload>(client, BasePath);

        Assert.Equal(0, data.MemberCount);
        Assert.True(data.Warnings.NoMembers);
        Assert.False(data.Warnings.NoPeriods);
        Assert.Equal(0, data.Warnings.UnassignedCount);
        Assert.Equal("Đợt 7/11", data.UpcomingPeriod!.Name);
        Assert.Equal(0, data.UpcomingPeriod.EligibleCount);
        Assert.Empty(data.UpcomingPeriod.MilestoneBreakdown);
        Assert.Empty(data.EligibleMembers);
    }

    [Fact(DisplayName = "6.1 · Kho trống hoàn toàn: đủ hai cảnh báo cho khối hướng dẫn ba bước (UC-13)")]
    public async Task GetDashboard_WithEmptyStore_ReportsBothWarnings()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.ResetAsync(factory);

        DashboardPayload data = await ApiClientFactory.GetDataAsync<DashboardPayload>(client, BasePath);

        Assert.Equal(0, data.MemberCount);
        Assert.Equal(0, data.PeriodCount);
        Assert.Null(data.UpcomingPeriod);
        Assert.True(data.Warnings.NoMembers);
        Assert.True(data.Warnings.NoPeriods);
        Assert.Equal(0, data.Warnings.UnassignedCount);
        Assert.Empty(data.Warnings.Overlaps);

        // Đúng hai cảnh báo, không thừa dòng "trống 01/01-31/12".
        Assert.Empty(data.Warnings.Gaps);
    }

    [Fact(DisplayName = "6.1 · Cảnh báo của Dashboard trùng khớp với cảnh báo màn Đợt")]
    public async Task GetDashboard_Warnings_MatchAwardPeriodScreen()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        DashboardPayload dashboard = await ApiClientFactory.GetDataAsync<DashboardPayload>(client, BasePath);
        AwardPeriodListPayload periods =
            await ApiClientFactory.GetDataAsync<AwardPeriodListPayload>(client, "/api/AwardPeriods");

        Assert.Equal(
            periods.Warnings.Gaps.Select(x => (x.FromDate, x.ToDate)).ToArray(),
            dashboard.Warnings.Gaps.Select(x => (x.FromDate, x.ToDate)).ToArray());
        Assert.Equal(periods.Warnings.Overlaps.Count, dashboard.Warnings.Overlaps.Count);
    }

    [Fact(DisplayName = "6.1 · Thiếu token trả 401")]
    public async Task GetDashboard_WithoutToken_ReturnsUnauthorized()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(BasePath);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static void AssertGaps(JsonElement expectedGaps, IReadOnlyList<PeriodGapPayload> actual)
    {
        Assert.Equal(
            expectedGaps.EnumerateArray().Select(x => x.GetProperty("from").GetString()).ToArray(),
            actual.Select(x => Iso(x.FromDate)).ToArray());
        Assert.Equal(
            expectedGaps.EnumerateArray().Select(x => x.GetProperty("to").GetString()).ToArray(),
            actual.Select(x => Iso(x.ToDate)).ToArray());
    }

    private static string Iso(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
