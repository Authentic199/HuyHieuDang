using System.Text.Json;
using HuyHieuDang.Web.QcIntegrationTests.Support;

namespace HuyHieuDang.Web.QcIntegrationTests;

/// <summary>
/// A-301 → A-308 · Dashboard (UC-10 → UC-13, QT8). Một lời gọi phải đủ dựng cả màn hình,
/// kể cả các trạng thái trống.
/// </summary>
[Collection(QcApiCollection.Name)]
public sealed class A3DashboardTests
{
    private readonly QcApiFactory factory;

    /// <summary>Initializes a new instance of the <see cref="A3DashboardTests"/> class.</summary>
    /// <param name="factory">Host kiểm thử dùng chung.</param>
    public A3DashboardTests(QcApiFactory factory)
    {
        this.factory = factory;
    }

    /// <summary>A-301 · Dashboard tại T0: đợt sắp tới là Đợt 7/11 năm 2026, còn 12 ngày, 6 người.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A301_Dashboard_tai_T0()
    {
        await QcDb.SeedCoreAsync(factory);

        await AssertUpcomingAsync(QcClock.T0, "core_default_T0", "P4", 2026, "Upcoming");
    }

    /// <summary>A-302 · Dashboard tại T1: Đợt 7/11 đang diễn ra, không còn đếm ngược.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A302_Dashboard_tai_T1_dang_dien_ra()
    {
        await QcDb.SeedCoreAsync(factory);

        JsonElement upcoming = await AssertUpcomingAsync(QcClock.T1, "core_default_T1", "P4", 2026, "Ongoing");

        upcoming.IntOrNull("daysRemaining").ShouldBeNull();
        upcoming.GetProperty("isNextYear").GetBoolean().ShouldBeFalse();
    }

    /// <summary>
    /// A-303 · Dashboard tại T2: mọi đợt của 2026 đã qua nên đợt sắp tới là Đợt 3/2 của **2027**
    /// (QT8), và cờ <c>isNextYear</c> phải bật.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A303_Dashboard_tai_T2_nhay_sang_nam_sau()
    {
        await QcDb.SeedCoreAsync(factory);

        JsonElement upcoming = await AssertUpcomingAsync(QcClock.T2, "core_default_T2", "P1", 2027, "Upcoming");

        upcoming.GetProperty("isNextYear").GetBoolean().ShouldBeTrue();
        upcoming.Int("year").ShouldBe(2027);
    }

    /// <summary>A-304 · Không có đợt nào: báo "chưa cài đợt", không trả đợt rỗng giả, không lỗi 500.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A304_Khong_co_dot_nao_bao_chua_cai_dot()
    {
        await QcDb.ResetAsync(factory);
        await QcDb.SeedMembersAsync(factory, QcFixtures.CoreMembers);

        using HttpClient client = await QcApi.LoginAsync(factory);
        JsonElement data = await QcApi.GetDataAsync(client, QcEndpoints.Dashboard);

        data.GetProperty("upcomingPeriod").ValueKind.ShouldBe(JsonValueKind.Null);
        data.Array("eligibleMembers").ShouldBeEmpty();
        data.Int("periodCount").ShouldBe(0);
        data.Int("memberCount").ShouldBe(QcFixtures.CoreMembers.Count);
        data.GetProperty("warnings").GetProperty("noPeriods").GetBoolean().ShouldBeTrue();
        data.GetProperty("warnings").GetProperty("noMembers").GetBoolean().ShouldBeFalse();
    }

    /// <summary>A-305 · Không có đảng viên nào: cảnh báo đúng, bảng rỗng, đợt vẫn hiện.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A305_Khong_co_dang_vien_nao()
    {
        await QcDb.ResetAsync(factory);
        await QcDb.SeedPeriodsAsync(factory, QcFixtures.MainPeriods);

        using HttpClient client = await QcApi.LoginAsync(factory);
        JsonElement data = await QcApi.GetDataAsync(client, QcEndpoints.Dashboard);

        data.Int("memberCount").ShouldBe(0);
        data.Int("periodCount").ShouldBe(4);
        data.GetProperty("warnings").GetProperty("noMembers").GetBoolean().ShouldBeTrue();
        data.GetProperty("warnings").GetProperty("noPeriods").GetBoolean().ShouldBeFalse();
        data.Array("eligibleMembers").ShouldBeEmpty();
        data.GetProperty("upcomingPeriod").Int("eligibleCount").ShouldBe(0);
    }

    /// <summary>A-306 · Kho trống hoàn toàn: cả hai cảnh báo bật, đủ dữ liệu cho khối 3 bước (UC-13).</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A306_Kho_trong_hoan_toan()
    {
        await QcDb.ResetAsync(factory);

        using HttpClient client = await QcApi.LoginAsync(factory);
        JsonElement data = await QcApi.GetDataAsync(client, QcEndpoints.Dashboard);

        data.Int("memberCount").ShouldBe(0);
        data.Int("periodCount").ShouldBe(0);
        data.GetProperty("warnings").GetProperty("noMembers").GetBoolean().ShouldBeTrue();
        data.GetProperty("warnings").GetProperty("noPeriods").GetBoolean().ShouldBeTrue();
        data.GetProperty("upcomingPeriod").ValueKind.ShouldBe(JsonValueKind.Null);
        data.Str("today").ShouldBe(QcClock.T0.ToString("yyyy-MM-dd"));
        data.Int("currentYear").ShouldBe(QcClock.T0.Year);
    }

    /// <summary>A-307 · Badge "chưa thuộc đợt nào" của năm nay đúng 7 người.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A307_Badge_chua_thuoc_dot_nao_bang_7()
    {
        await QcDb.SeedCoreAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        int expected = QcFixtures.Scenario("core_default_T0").Int("badgeCurrentYear");

        JsonElement dashboard = await QcApi.GetDataAsync(client, QcEndpoints.Dashboard);
        dashboard.GetProperty("warnings").Int("unassignedCount").ShouldBe(expected);
        dashboard.GetProperty("warnings").Int("unassignedYear").ShouldBe(2026);

        JsonElement badge = await QcApi.GetDataAsync(client, QcEndpoints.UnassignedCount);
        badge.Int("count").ShouldBe(expected);
    }

    /// <summary>
    /// A-308 · Cảnh báo chồng lấn và chưa phủ kín trên Dashboard phải khớp từng chữ với cảnh
    /// báo ở màn Đợt — cùng một nguồn tính.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A308_Canh_bao_tren_Dashboard_khop_voi_man_Dot()
    {
        await QcDb.ResetAsync(factory);
        await QcDb.SeedPeriodsAsync(factory, QcFixtures.OverlapPeriods);
        await QcDb.SeedMembersAsync(factory, QcFixtures.CoreMembers);

        using HttpClient client = await QcApi.LoginAsync(factory);

        JsonElement dashboard = (await QcApi.GetDataAsync(client, QcEndpoints.Dashboard)).GetProperty("warnings");
        JsonElement periods = (await QcApi.GetDataAsync(client, $"{QcEndpoints.AwardPeriods}?year=2026"))
            .GetProperty("warnings");

        dashboard.Array("overlaps").Count.ShouldBe(periods.Array("overlaps").Count);
        dashboard.Array("gaps").Count.ShouldBe(periods.Array("gaps").Count);

        dashboard.GetProperty("overlaps").GetRawText().ShouldBe(periods.GetProperty("overlaps").GetRawText());
        dashboard.GetProperty("gaps").GetRawText().ShouldBe(periods.GetProperty("gaps").GetRawText());
    }

    private async Task<JsonElement> AssertUpcomingAsync(
        DateOnly today, string scenario, string periodCode, int year, string expectedStatus)
    {
        using HttpClient client = await QcApi.LoginAsync(factory, today);

        JsonElement data = await QcApi.GetDataAsync(client, QcEndpoints.Dashboard);
        JsonElement expected = QcFixtures.Scenario(scenario).GetProperty("upcomingPeriod");
        JsonElement expectedEligible = QcFixtures.Eligible(scenario, periodCode, year);

        data.Str("today").ShouldBe(today.ToString("yyyy-MM-dd"));
        data.Int("currentYear").ShouldBe(today.Year);
        data.Int("memberCount").ShouldBe(QcFixtures.CoreMembers.Count);

        JsonElement upcoming = data.GetProperty("upcomingPeriod");
        upcoming.Str("name").ShouldBe(expected.Str("name"));
        upcoming.Int("year").ShouldBe(expected.Int("year"));
        upcoming.Str("fromDate").ShouldBe(expected.Str("boundFrom"));
        upcoming.Str("toDate").ShouldBe(expected.Str("boundTo"));
        upcoming.Str("status").ShouldBe(expectedStatus);
        upcoming.IntOrNull("daysRemaining").ShouldBe(expected.IntOrNull("daysLeft"));
        upcoming.Int("eligibleCount").ShouldBe(expectedEligible.Int("total"));

        // Phân bổ mốc của khối đợt sắp tới.
        Dictionary<string, int> breakdown = upcoming.Array("milestoneBreakdown")
            .ToDictionary(
                item => item.Int("milestone").ToString(System.Globalization.CultureInfo.InvariantCulture),
                item => item.Int("count"),
                StringComparer.Ordinal);
        Dictionary<string, int> expectedBreakdown = expectedEligible.GetProperty("byMilestone")
            .EnumerateObject()
            .ToDictionary(item => item.Name, item => item.Value.GetInt32(), StringComparer.Ordinal);
        breakdown.ShouldBe(expectedBreakdown);

        // Danh sách trên Dashboard đúng người và đúng thứ tự mục 1.8.
        data.Array("eligibleMembers").Select(row => row.Str("fullName")).ToList()
            .ShouldBe(expectedEligible.Array("rows").Select(row => row.Str("fullName")).ToList());

        return upcoming;
    }
}
