using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.Eligibility.Requests;
using System.Globalization;
using System.Net;
using System.Text.Json;
using AwardPeriod = HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities.AwardPeriod;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Mục 6.2, 6.3 và 6.4 hợp đồng API (UC-34, UC-40, QT4, QT5, QT7) chạy trên bộ dữ liệu biên
/// dùng chung của QC. Mọi con số mong đợi đọc thẳng từ <c>tests/fixtures/data/expected.json</c>,
/// không chép lại vào mã kiểm thử.
/// </summary>
[Collection(ApiCollection.Name)]
public class EligibilityEndpointTests
{
    private const string BasePath = "/api/Eligibility";

    private readonly HuyHieuDangApiFactory factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="EligibilityEndpointTests"/> class.
    /// </summary>
    /// <param name="factory">Host kiểm thử dùng chung cho cả lớp.</param>
    public EligibilityEndpointTests(HuyHieuDangApiFactory factory)
    {
        this.factory = factory;
    }

    [Theory(DisplayName = "6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json")]
    [InlineData("P1", 2025)]
    [InlineData("P1", 2026)]
    [InlineData("P1", 2027)]
    [InlineData("P1", 2028)]
    [InlineData("P2", 2025)]
    [InlineData("P2", 2026)]
    [InlineData("P2", 2027)]
    [InlineData("P2", 2028)]
    [InlineData("P3", 2025)]
    [InlineData("P3", 2026)]
    [InlineData("P3", 2027)]
    [InlineData("P3", 2028)]
    [InlineData("P4", 2025)]
    [InlineData("P4", 2026)]
    [InlineData("P4", 2027)]
    [InlineData("P4", 2028)]
    public async Task GetEligibility_MatchesExpectedFixture(string periodCode, int year)
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        Guid periodId = await FindPeriodIdAsync(client, periodCode);
        JsonElement expected = CoreFixtures
            .Scenario("core_default_T0")
            .GetProperty("eligibleByPeriod")
            .GetProperty(periodCode)
            .GetProperty("byYear")
            .GetProperty(year.ToString(CultureInfo.InvariantCulture));

        EligibilityListPayload data = await ApiClientFactory.GetDataAsync<EligibilityListPayload>(
            client, $"{BasePath}?awardPeriodId={periodId}&year={year}");

        Assert.Equal(year, data.Year);
        Assert.Equal(year, data.AwardPeriod.Year);
        Assert.Equal(expected.GetProperty("boundFrom").GetString(), Iso(data.AwardPeriod.FromDate));
        Assert.Equal(expected.GetProperty("boundTo").GetString(), Iso(data.AwardPeriod.ToDate));
        Assert.Equal(expected.GetProperty("total").GetInt32(), data.TotalCount);
        Assert.Equal(data.TotalCount, data.AwardPeriod.EligibleCount);

        AssertBreakdown(expected.GetProperty("byMilestone"), data.MilestoneBreakdown);
        AssertRows(expected.GetProperty("rows"), data.Members);
    }

    [Fact(DisplayName = "6.2 · Bỏ trống year thì lấy năm hiện tại của máy chủ")]
    public async Task GetEligibility_WithoutYear_UsesCurrentYear()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        Guid periodId = await FindPeriodIdAsync(client, "P4");

        EligibilityListPayload data = await ApiClientFactory.GetDataAsync<EligibilityListPayload>(
            client, $"{BasePath}?awardPeriodId={periodId}");

        Assert.Equal(HuyHieuDangApiFactory.FixedToday.Year, data.Year);
        Assert.Equal(6, data.TotalCount);
    }

    [Fact(DisplayName = "6.2 · Nới Đến ngày Đợt 2/9 sang 30/09: đợt lên 5 người ngay, không lưu gì (QT5)")]
    public async Task GetEligibility_WithWidenedPeriod_RecomputesImmediately()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.ResetAsync(factory);
        await CoreDataset.SeedPeriodsAsync(factory, WidenedPeriods());
        await CoreDataset.SeedMembersAsync(factory);

        Guid periodId = await FindPeriodIdAsync(client, "P3");
        JsonElement expected = CoreFixtures
            .Scenario("core_default_T0_widenedP3")
            .GetProperty("eligibleByPeriod")
            .GetProperty("P3")
            .GetProperty("byYear")
            .GetProperty("2026");

        EligibilityListPayload data = await ApiClientFactory.GetDataAsync<EligibilityListPayload>(
            client, $"{BasePath}?awardPeriodId={periodId}&year=2026");

        Assert.Equal(5, data.TotalCount);
        AssertRows(expected.GetProperty("rows"), data.Members);
    }

    [Fact(DisplayName = "6.2 · Id đợt lạ trả Mes.AwardPeriod.NotFound")]
    public async Task GetEligibility_WithUnknownPeriod_ReturnsNotFound()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        await ApiClientFactory.AssertErrorAsync(
            await client.GetAsync($"{BasePath}?awardPeriodId={Guid.NewGuid()}"),
            Messages<AwardPeriod>.NotFound());
    }

    [Theory(DisplayName = "6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year")]
    [InlineData(BasePath, 1899)]
    [InlineData(BasePath, 2201)]
    [InlineData(BasePath + "/Unassigned", 0)]
    [InlineData(BasePath + "/Unassigned", -5)]
    [InlineData(BasePath + "/UnassignedCount", 2201)]
    public async Task EligibilityEndpoint_WithYearOutOfRange_ReturnsInvalidYear(string path, int year)
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        string url = path == BasePath
            ? $"{path}?awardPeriodId={await FindPeriodIdAsync(client, "P4")}&year={year}"
            : $"{path}?year={year}";

        await ApiClientFactory.AssertErrorAsync(
            await client.GetAsync(url), Messages<EligibilityYearQueryRequest>.Invalid(x => x.Year));
    }

    [Fact(DisplayName = "6.3 · Chưa thuộc đợt nào năm 2026: 7 người, đúng nhãn khoảng trống (QT7)")]
    public async Task GetUnassigned_MatchesExpectedFixture()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        JsonElement expected = MissedOfYear("core_default_T0", 2026);

        UnassignedListPayload data = await ApiClientFactory.GetDataAsync<UnassignedListPayload>(
            client, $"{BasePath}/Unassigned?year=2026");

        Assert.Equal(2026, data.Year);
        Assert.Equal(expected.GetProperty("total").GetInt32(), data.TotalCount);
        AssertRows(expected.GetProperty("rows"), data.Members);
        Assert.Equal(
            expected.GetProperty("rows").EnumerateArray().Select(x => x.GetProperty("gapLabel").GetString()).ToArray(),
            data.Members.Select(x => x.Gap.Label()).ToArray());
    }

    [Theory(DisplayName = "6.3 · Các năm khác của bộ lõi khớp expected.json")]
    [InlineData(2025)]
    [InlineData(2027)]
    [InlineData(2028)]
    public async Task GetUnassigned_ForOtherYears_MatchesExpectedFixture(int year)
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        JsonElement expected = MissedOfYear("core_default_T0", year);

        UnassignedListPayload data = await ApiClientFactory.GetDataAsync<UnassignedListPayload>(
            client, $"{BasePath}/Unassigned?year={year}");

        Assert.Equal(expected.GetProperty("total").GetInt32(), data.TotalCount);
        AssertRows(expected.GetProperty("rows"), data.Members);
    }

    [Fact(DisplayName = "6.3 · Đổi Bước 5 → 10 làm danh sách còn 6 người ngay (QT1, QT5)")]
    public async Task GetUnassigned_WithStepTen_MatchesExpectedFixture()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory, settingsKey: "step10");

        JsonElement expected = MissedOfYear("core_step10_T0", 2026);

        UnassignedListPayload data = await ApiClientFactory.GetDataAsync<UnassignedListPayload>(
            client, $"{BasePath}/Unassigned?year=2026");

        Assert.Equal(6, expected.GetProperty("total").GetInt32());
        Assert.Equal(6, data.TotalCount);
        AssertRows(expected.GetProperty("rows"), data.Members);
    }

    [Fact(DisplayName = "6.3 · Chưa cài đợt nào: mọi người tròn mốc đều rơi vào Trước đợt đầu tiên")]
    public async Task GetUnassigned_WithoutPeriods_ReturnsBeforeFirstForEveryone()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.ResetAsync(factory);
        await CoreDataset.SeedMembersAsync(factory);

        JsonElement expected = MissedOfYear("core_default_T0_noPeriod", 2026);

        UnassignedListPayload data = await ApiClientFactory.GetDataAsync<UnassignedListPayload>(
            client, $"{BasePath}/Unassigned?year=2026");

        Assert.Equal(27, expected.GetProperty("total").GetInt32());
        Assert.Equal(27, data.TotalCount);
        AssertRows(expected.GetProperty("rows"), data.Members);
        Assert.All(data.Members, member =>
        {
            Assert.Equal("BeforeFirst", member.Gap.Type);
            Assert.Null(member.Gap.PreviousPeriodName);
            Assert.Null(member.Gap.NextPeriodName);
        });
    }

    [Fact(DisplayName = "6.4 · Badge luôn bằng số dòng của màn Chưa thuộc đợt nào")]
    public async Task GetUnassignedCount_AlwaysMatchesListLength()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        UnassignedCountPayload badge =
            await ApiClientFactory.GetDataAsync<UnassignedCountPayload>(client, $"{BasePath}/UnassignedCount");
        UnassignedListPayload list = await ApiClientFactory.GetDataAsync<UnassignedListPayload>(
            client, $"{BasePath}/Unassigned?year={HuyHieuDangApiFactory.FixedToday.Year}");

        Assert.Equal(HuyHieuDangApiFactory.FixedToday.Year, badge.Year);
        Assert.Equal(7, badge.Count);
        Assert.Equal(list.TotalCount, badge.Count);
    }

    [Theory(DisplayName = "6.2 → 6.4 · Thiếu token trả 401")]
    [InlineData(BasePath)]
    [InlineData(BasePath + "/Unassigned")]
    [InlineData(BasePath + "/UnassignedCount")]
    public async Task EligibilityEndpoint_WithoutToken_ReturnsUnauthorized(string path)
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Đối chiếu từng dòng: đúng thứ tự, đúng người, đúng mốc và đúng ngày tròn mốc.
    /// </summary>
    /// <param name="expectedRows">Mảng <c>rows</c> của <c>expected.json</c>.</param>
    /// <param name="actual">Danh sách do API trả về.</param>
    private static void AssertRows(JsonElement expectedRows, IReadOnlyList<EligibleMemberPayload> actual)
    {
        Assert.Equal(
            expectedRows.EnumerateArray().Select(x => x.GetProperty("fullName").GetString()).ToArray(),
            actual.Select(x => x.FullName).ToArray());
        Assert.Equal(
            expectedRows.EnumerateArray().Select(x => x.GetProperty("milestone").GetInt32()).ToArray(),
            actual.Select(x => x.Milestone).ToArray());
        Assert.Equal(
            expectedRows.EnumerateArray().Select(x => x.GetProperty("anniversary").GetString()).ToArray(),
            actual.Select(x => Iso(x.MilestoneDate)).ToArray());
    }

    /// <summary>
    /// Đối chiếu phân bổ theo mốc: chỉ liệt kê mốc có người, sắp tăng dần.
    /// </summary>
    /// <param name="expectedBreakdown">Nút <c>byMilestone</c> của <c>expected.json</c>.</param>
    /// <param name="actual">Phân bổ do API trả về.</param>
    private static void AssertBreakdown(JsonElement expectedBreakdown, IReadOnlyList<MilestoneBreakdownPayload> actual)
    {
        List<KeyValuePair<int, int>> expected = expectedBreakdown
            .EnumerateObject()
            .Select(x => new KeyValuePair<int, int>(int.Parse(x.Name, CultureInfo.InvariantCulture), x.Value.GetInt32()))
            .OrderBy(x => x.Key)
            .ToList();

        Assert.Equal(expected.Select(x => x.Key).ToArray(), actual.Select(x => x.Milestone).ToArray());
        Assert.Equal(expected.Select(x => x.Value).ToArray(), actual.Select(x => x.Count).ToArray());
    }

    private static string Iso(DateOnly date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static JsonElement MissedOfYear(string scenario, int year)
        => CoreFixtures
            .Scenario(scenario)
            .GetProperty("missedByYear")
            .GetProperty(year.ToString(CultureInfo.InvariantCulture));

    /// <summary>
    /// Bốn đợt chính nhưng Đợt 2/9 kéo dài tới 30/09 (ca A-212 của kế hoạch kiểm thử).
    /// </summary>
    /// <returns>Bộ đợt đã nới.</returns>
    private static IReadOnlyList<FixturePeriod> WidenedPeriods()
        => CoreFixtures.MainPeriods()
            .Select(period => period.Code != "P3"
                ? period
                : new FixturePeriod
                {
                    Code = period.Code,
                    Name = period.Name,
                    FromDay = period.FromDay,
                    FromMonth = period.FromMonth,
                    ToDay = 30,
                    ToMonth = 9,
                })
            .ToList();

    private static async Task<Guid> FindPeriodIdAsync(HttpClient client, string code)
    {
        string name = CoreFixtures.MainPeriods().Single(x => x.Code == code).Name;

        AwardPeriodListPayload list =
            await ApiClientFactory.GetDataAsync<AwardPeriodListPayload>(client, "/api/AwardPeriods");

        return list.Periods.Single(x => x.Name == name).Id;
    }
}
