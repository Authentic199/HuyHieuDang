using System.Text.Json;
using HuyHieuDang.Web.QcIntegrationTests.Support;

namespace HuyHieuDang.Web.QcIntegrationTests;

/// <summary>
/// A-401 → A-406 · Chưa thuộc đợt nào (UC-40, QT7): ai tròn mốc trong năm nhưng ngày tròn mốc
/// không rơi vào đợt nào.
/// </summary>
[Collection(QcApiCollection.Name)]
public sealed class A4UnassignedTests
{
    private readonly QcApiFactory factory;

    /// <summary>Initializes a new instance of the <see cref="A4UnassignedTests"/> class.</summary>
    /// <param name="factory">Host kiểm thử dùng chung.</param>
    public A4UnassignedTests(QcApiFactory factory)
    {
        this.factory = factory;
    }

    /// <summary>A-401 · Năm 2026 có đúng 7 người, mỗi người mang đúng nhãn khoảng trống.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A401_Nam_2026_dung_bay_nguoi_va_dung_nhan_khoang_trong()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        await AssertUnassignedAsync(client, 2026, "core_default_T0");
    }

    /// <summary>A-402 · Đổi Bước sang 10 làm danh sách còn 6 người (QT1, QT5).</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A402_Doi_buoc_sang_10_con_sau_nguoi()
    {
        await QcDb.SeedCoreAsync(factory, "step10");
        using HttpClient client = await QcApi.LoginAsync(factory);

        JsonElement data = await QcApi.GetDataAsync(client, $"{QcEndpoints.Unassigned}?year=2026");

        data.Int("totalCount").ShouldBe(QcFixtures.Scenario("core_step10_T0").Int("badgeCurrentYear"));
    }

    /// <summary>A-403 · Năm 2025 và 2027 khớp <c>expected.json</c>.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A403_Nam_2025_va_2027_khop_ket_qua_mong_doi()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        await AssertUnassignedAsync(client, 2025, "core_default_T0");
        await AssertUnassignedAsync(client, 2027, "core_default_T0");
    }

    /// <summary>A-404 · Bộ đợt phủ kín thì không ai bị sót.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A404_Bo_dot_phu_kin_thi_khong_ai_bi_sot()
    {
        await QcDb.ResetAsync(factory);
        await QcDb.SeedPeriodsAsync(factory, QcFixtures.FullCoverPeriods);
        await QcDb.SeedMembersAsync(factory, QcFixtures.CoreMembers);

        using HttpClient client = await QcApi.LoginAsync(factory);

        JsonElement data = await QcApi.GetDataAsync(client, $"{QcEndpoints.Unassigned}?year=2026");

        data.Int("totalCount").ShouldBe(QcFixtures.Scenario("core_default_T0_fullCover").Int("badgeCurrentYear"));
        data.Array("members").ShouldBeEmpty();
    }

    /// <summary>
    /// A-405 · Sắp theo Mốc huy hiệu tăng dần, trong cùng mốc thì theo Họ tên đầy đủ theo bảng
    /// chữ cái tiếng Việt.
    /// </summary>
    /// <remarks>
    /// Ca này lấy chuẩn từ tài liệu nghiệp vụ UC-11 ("Sắp theo Mốc rồi Họ tên") — nguồn sự thật
    /// theo quy tắc cứng số 1 của <c>CLAUDE.md</c>. Hợp đồng API mục 6.3 lại ghi "sắp theo
    /// <c>milestoneDate</c> tăng dần" và mục 1.8 ghi "theo tên gọi (từ cuối cùng của Họ tên)";
    /// cả hai câu đó đang lệch với tài liệu nghiệp vụ, với <c>expected.json</c> và với mã đang
    /// chạy. Đã ghi lại thành lỗi tài liệu QC-T27-03.
    /// </remarks>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A405_Sap_theo_Moc_roi_Ho_ten()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        JsonElement data = await QcApi.GetDataAsync(client, $"{QcEndpoints.Unassigned}?year=2026");
        List<JsonElement> rows = data.Array("members").ToList();

        List<(int Milestone, string FullName)> actual = rows
            .Select(row => (row.Int("milestone"), row.Str("fullName")))
            .ToList();

        StringComparer vietnamese = StringComparer.Create(
            System.Globalization.CultureInfo.GetCultureInfo("vi-VN"), ignoreCase: false);

        actual.ShouldBe(actual
            .OrderBy(item => item.Milestone)
            .ThenBy(item => item.FullName, vietnamese)
            .ToList());

        // Và đúng thứ tự mà expected.json chốt.
        rows.Select(row => row.Str("fullName")).ToList()
            .ShouldBe(QcFixtures.Missed("core_default_T0", 2026)
                .Array("rows").Select(row => row.Str("fullName")).ToList());
    }

    /// <summary>
    /// A-406 · Badge trên menu và số dòng của màn hình luôn khớp — hai con số phải đến từ cùng
    /// một nguồn tính. Kiểm ở bốn trạng thái dữ liệu khác nhau.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A406_Badge_luon_khop_so_dong_cua_man_hinh()
    {
        foreach ((string settingsKey, IReadOnlyList<QcPeriod> periods) in new (string, IReadOnlyList<QcPeriod>)[]
                 {
                     ("default", QcFixtures.MainPeriods),
                     ("step10", QcFixtures.MainPeriods),
                     ("default", QcFixtures.FullCoverPeriods),
                     ("default", System.Array.Empty<QcPeriod>()),
                 })
        {
            await QcDb.ResetAsync(factory, settingsKey);
            await QcDb.SeedPeriodsAsync(factory, periods);
            await QcDb.SeedMembersAsync(factory, QcFixtures.CoreMembers);

            using HttpClient client = await QcApi.LoginAsync(factory);

            foreach (int year in new[] { 2025, 2026, 2027 })
            {
                JsonElement list = await QcApi.GetDataAsync(client, $"{QcEndpoints.Unassigned}?year={year}");
                JsonElement badge = await QcApi.GetDataAsync(client, $"{QcEndpoints.UnassignedCount}?year={year}");

                string label = $"cài đặt {settingsKey}, {periods.Count} đợt, năm {year}";

                badge.Int("count").ShouldBe(list.Int("totalCount"), label);
                list.Array("members").Count.ShouldBe(list.Int("totalCount"), label);
                badge.Int("year").ShouldBe(year, label);
            }
        }
    }

    /// <summary>
    /// A-401b · Khi **chưa cài đợt nào**, hợp đồng API mục 1.10 chốt mọi người tròn mốc trong
    /// năm mang <c>gap.type = "BeforeFirst"</c> với hai tên đợt <c>null</c>.
    /// </summary>
    /// <remarks>
    /// Ca này khẳng định theo hợp đồng chứ không theo <c>expected.json</c>: bản oracle
    /// <c>qt_reference.py</c> vẫn đang ghi nhãn "Sau đợt cuối cùng" cho trạng thái này, là chỗ
    /// bất đồng CEO đã chốt theo hợp đồng. Con số tổng vẫn đối chiếu với fixture.
    /// </remarks>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A401b_Chua_cai_dot_nao_thi_nhan_la_Truoc_dot_dau_tien()
    {
        await QcDb.ResetAsync(factory);
        await QcDb.SeedMembersAsync(factory, QcFixtures.CoreMembers);

        using HttpClient client = await QcApi.LoginAsync(factory);

        JsonElement data = await QcApi.GetDataAsync(client, $"{QcEndpoints.Unassigned}?year=2026");

        data.Int("totalCount")
            .ShouldBe(QcFixtures.Scenario("core_default_T0_noPeriod").Int("badgeCurrentYear"));

        foreach (JsonElement member in data.Array("members"))
        {
            JsonElement gap = member.GetProperty("gap");

            gap.Str("type").ShouldBe("BeforeFirst", member.Str("fullName"));
            gap.StringOrNull("previousPeriodName").ShouldBeNull(member.Str("fullName"));
            gap.StringOrNull("nextPeriodName").ShouldBeNull(member.Str("fullName"));
        }
    }

    private static async Task AssertUnassignedAsync(HttpClient client, int year, string scenario)
    {
        JsonElement expected = QcFixtures.Missed(scenario, year);
        JsonElement data = await QcApi.GetDataAsync(client, $"{QcEndpoints.Unassigned}?year={year}");

        string label = $"năm {year}";

        data.Int("year").ShouldBe(year, label);
        data.Int("totalCount").ShouldBe(expected.Int("total"), label);

        IReadOnlyList<JsonElement> members = data.Array("members");
        List<JsonElement> expectedRows = expected.Array("rows").ToList();

        members.Count.ShouldBe(expectedRows.Count, label);

        foreach (JsonElement expectedRow in expectedRows)
        {
            JsonElement actual = members.Single(row => row.Str("fullName") == expectedRow.Str("fullName"));
            string name = $"{expectedRow.Str("fullName")} ({label})";

            actual.Int("milestone").ShouldBe(expectedRow.Int("milestone"), name);
            actual.Str("milestoneDate").ShouldBe(expectedRow.Str("anniversary"), name);
            GapLabel(actual.GetProperty("gap")).ShouldBe(expectedRow.Str("gapLabel"), name);
        }
    }

    /// <summary>
    /// Dựng lại chữ hiển thị của cột "Khoảng trống" đúng khuôn mục 1.10 hợp đồng API, để so
    /// thẳng với nhãn trong <c>expected.json</c>.
    /// </summary>
    /// <param name="gap">Nút <c>gap</c> của một dòng.</param>
    /// <returns>Chữ hiển thị.</returns>
    private static string GapLabel(JsonElement gap) => gap.Str("type") switch
    {
        "Between" => $"Giữa {gap.Str("previousPeriodName")} và {gap.Str("nextPeriodName")}",
        "BeforeFirst" => "Trước đợt đầu tiên",
        "AfterLast" => "Sau đợt cuối cùng",
        _ => throw new InvalidOperationException($"Kiểu khoảng trống lạ: {gap.Str("type")}"),
    };

    private async Task<HttpClient> SeedCoreAndLoginAsync()
    {
        await QcDb.SeedCoreAsync(factory);

        return await QcApi.LoginAsync(factory);
    }
}
