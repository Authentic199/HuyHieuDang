using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HuyHieuDang.Web.QcIntegrationTests.Support;

namespace HuyHieuDang.Web.QcIntegrationTests;

/// <summary>
/// A-201 → A-220 · Đợt trao huy hiệu (UC-30 → UC-34, QT6, QT11) và danh sách đủ điều kiện
/// theo đợt và năm (QT4, QT5).
/// </summary>
[Collection(QcApiCollection.Name)]
public sealed class A2AwardPeriodTests
{
    private readonly QcApiFactory factory;

    /// <summary>Initializes a new instance of the <see cref="A2AwardPeriodTests"/> class.</summary>
    /// <param name="factory">Host kiểm thử dùng chung.</param>
    public A2AwardPeriodTests(QcApiFactory factory)
    {
        this.factory = factory;
    }

    /// <summary>A-201 · Bốn đợt chính trả về đúng bốn dòng, sắp theo Từ ngày tăng dần.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A201_Bon_dot_chinh_sap_theo_tu_ngay()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        JsonElement data = await QcApi.GetDataAsync(client, QcEndpoints.AwardPeriods);
        IReadOnlyList<JsonElement> periods = data.Array("periods");

        data.Int("totalCount").ShouldBe(4);
        periods.Select(period => period.Str("name")).ToList()
            .ShouldBe(QcFixtures.MainPeriods.Select(period => period.Name).ToList());

        List<string> fromDates = periods.Select(period => period.Str("fromDate")).ToList();
        fromDates.ShouldBe(fromDates.OrderBy(value => value, StringComparer.Ordinal).ToList());
    }

    /// <summary>A-202 và A-203 · Trạng thái đợt tại T0 và T1 (QT11).</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A202_A203_Trang_thai_dot_tai_T0_va_T1()
    {
        await QcDb.SeedCoreAsync(factory);

        await AssertStatusesAsync(QcClock.T0, "core_default_T0");
        await AssertStatusesAsync(QcClock.T1, "core_default_T1");
    }

    /// <summary>A-204 · Cột "Đủ điều kiện năm nay" của từng dòng đúng 5 · 5 · 4 · 6.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A204_So_nguoi_du_dieu_kien_nam_nay_tren_tung_dong()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        JsonElement data = await QcApi.GetDataAsync(client, QcEndpoints.AwardPeriods);

        foreach (QcPeriod period in QcFixtures.MainPeriods)
        {
            JsonElement row = data.Array("periods").Single(item => item.Str("name") == period.Name);
            int expected = QcFixtures.Eligible("core_default_T0", period.Code, 2026).Int("total");

            row.Int("eligibleCount").ShouldBe(expected, period.Name);
        }
    }

    /// <summary>A-205 → A-207 · Từ sau Đến nay là đợt vắt năm; chặn lưu còn trùng tên và ngày không có thật.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A205_A206_A207_Ba_rang_buoc_chan_luu()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        // CEO chốt ngày 20/09/2026: Từ ngày sau Đến ngày là đợt vắt qua 31/12, được lưu.
        using HttpResponseMessage spanning = await client.PostAsJsonAsync(
            QcEndpoints.AwardPeriods,
            new { name = "Đợt vắt năm", fromDay = 10, fromMonth = 9, toDay = 15, toMonth = 8 });
        spanning.StatusCode.ShouldBe(HttpStatusCode.OK);

        using HttpResponseMessage duplicate = await client.PostAsJsonAsync(
            QcEndpoints.AwardPeriods,
            new { name = "Đợt 7/11", fromDay = 1, fromMonth = 10, toDay = 7, toMonth = 11 });
        await QcApi.AssertErrorAsync(duplicate, HttpStatusCode.BadRequest, QcMessages.PeriodRepeatedName);

        using HttpResponseMessage impossible = await client.PostAsJsonAsync(
            QcEndpoints.AwardPeriods,
            new { name = "Đợt 31/02", fromDay = 31, fromMonth = 2, toDay = 5, toMonth = 3 });
        await QcApi.AssertErrorAsync(impossible, HttpStatusCode.BadRequest, QcMessages.PeriodInvalidFromDate);

        // Thiếu tên cũng bị chặn (mục 5.3 hợp đồng API).
        using HttpResponseMessage noName = await client.PostAsJsonAsync(
            QcEndpoints.AwardPeriods,
            new { name = string.Empty, fromDay = 1, fromMonth = 3, toDay = 5, toMonth = 3 });
        await QcApi.AssertErrorAsync(noName, HttpStatusCode.BadRequest, QcMessages.PeriodRequiredName);
    }

    /// <summary>
    /// A-208 · Đợt 29/02 – 05/03 là hợp lệ; gắn năm không nhuận thì Từ ngày thu về 28/02,
    /// năm nhuận thì giữ 29/02 (QT4, mục 1.10 hợp đồng API).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A208_Dot_bat_dau_29_02_hop_le_va_thu_ve_28_02_o_nam_khong_nhuan()
    {
        await QcDb.ResetAsync(factory);
        await QcDb.SeedPeriodsAsync(factory, QcFixtures.LeapEdgePeriods);
        await QcDb.SeedMembersAsync(factory, QcFixtures.CoreMembers);

        using HttpClient client = await QcApi.LoginAsync(factory);

        foreach (int year in new[] { 2026, 2027, 2028 })
        {
            JsonElement data = await QcApi.GetDataAsync(client, $"{QcEndpoints.AwardPeriods}?year={year}");
            JsonElement period = data.Array("periods").Single();
            JsonElement expected = QcFixtures.Eligible("core_default_T0_leapEdgePeriod", "PL1", year);

            period.Str("fromDate").ShouldBe(expected.Str("boundFrom"), year.ToString(CultureInfo.InvariantCulture));
            period.Str("toDate").ShouldBe(expected.Str("boundTo"), year.ToString(CultureInfo.InvariantCulture));
            period.Int("eligibleCount").ShouldBe(expected.Int("total"), year.ToString(CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// A-209 · Hai đợt chồng lấn vẫn **lưu được**, kèm cảnh báo nêu đúng cặp đợt (QT6 — cảnh
    /// báo không bao giờ chặn lưu).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A209_Hai_dot_chong_lan_van_luu_duoc_va_co_canh_bao_dung_cap()
    {
        await QcDb.ResetAsync(factory);
        await QcDb.SeedMembersAsync(factory, QcFixtures.CoreMembers);

        using HttpClient client = await QcApi.LoginAsync(factory);

        foreach (QcPeriod period in QcFixtures.OverlapPeriods)
        {
            JsonElement created = await QcApi.PostDataAsync(client, QcEndpoints.AwardPeriods, new
            {
                name = period.Name,
                fromDay = period.FromDay,
                fromMonth = period.FromMonth,
                toDay = period.ToDay,
                toMonth = period.ToMonth,
            });

            created.GetProperty("period").Str("name").ShouldBe(period.Name);
        }

        JsonElement warnings = (await QcApi.GetDataAsync(client, QcEndpoints.AwardPeriods)).GetProperty("warnings");
        IReadOnlyList<JsonElement> overlaps = warnings.Array("overlaps");

        JsonElement expected = QcFixtures.Scenario("core_default_T0_overlap").GetProperty("overlapWarnings");
        overlaps.Count.ShouldBe(expected.GetArrayLength());

        List<string[]> actualPairs = overlaps
            .Select(overlap => new[] { overlap.Str("firstPeriodName"), overlap.Str("secondPeriodName") })
            .ToList();
        List<string[]> expectedPairs = expected.EnumerateArray()
            .Select(pair => pair.EnumerateArray().Select(name => name.GetString()!).ToArray())
            .ToList();

        actualPairs.ShouldBe(expectedPairs);
    }

    /// <summary>A-210 · Bốn đợt chính để hở đúng 5 khoảng trống, liệt kê đúng từng khoảng.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A210_Bon_dot_chinh_canh_bao_dung_nam_khoang_trong()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        JsonElement data = await QcApi.GetDataAsync(client, $"{QcEndpoints.AwardPeriods}?year=2026");
        IReadOnlyList<JsonElement> gaps = data.GetProperty("warnings").Array("gaps");

        JsonElement expected = QcFixtures.Scenario("core_default_T0").GetProperty("gapsByYear").GetProperty("2026");

        gaps.Count.ShouldBe(expected.GetArrayLength());

        List<JsonElement> expectedGaps = expected.EnumerateArray().ToList();

        for (int index = 0; index < gaps.Count; index++)
        {
            gaps[index].Str("fromDate").ShouldBe(expectedGaps[index].Str("from"));
            gaps[index].Str("toDate").ShouldBe(expectedGaps[index].Str("to"));
        }
    }

    /// <summary>A-211 · Bộ đợt phủ kín 01/01 – 31/12 không còn cảnh báo chưa phủ kín.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A211_Bo_dot_phu_kin_khong_con_canh_bao()
    {
        await QcDb.ResetAsync(factory);
        await QcDb.SeedPeriodsAsync(factory, QcFixtures.FullCoverPeriods);
        await QcDb.SeedMembersAsync(factory, QcFixtures.CoreMembers);

        using HttpClient client = await QcApi.LoginAsync(factory);

        JsonElement data = await QcApi.GetDataAsync(client, $"{QcEndpoints.AwardPeriods}?year=2026");

        data.GetProperty("warnings").Array("gaps").ShouldBeEmpty();
        data.GetProperty("warnings").Array("overlaps").ShouldBeEmpty();
    }

    /// <summary>
    /// A-212 · Nới Đến ngày của Đợt 2/9 từ 10/09 sang 30/09 làm danh sách đủ điều kiện và badge
    /// đổi ngay, không cần thao tác nào khác (QT5, QT6).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A212_Noi_den_ngay_lam_danh_sach_doi_ngay()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        JsonElement period = await FindPeriodAsync(client, "Đợt 2/9");

        await QcApi.PutDataAsync(client, $"{QcEndpoints.AwardPeriods}/{period.Str("id")}", new
        {
            name = period.Str("name"),
            fromDay = period.Int("fromDay"),
            fromMonth = period.Int("fromMonth"),
            toDay = 30,
            toMonth = 9,
        });

        JsonElement expected = QcFixtures.Eligible("core_default_T0_widenedP3", "P3", 2026);

        JsonElement eligibility = await QcApi.GetDataAsync(
            client, $"{QcEndpoints.Eligibility}?awardPeriodId={period.Str("id")}&year=2026");
        eligibility.Int("totalCount").ShouldBe(expected.Int("total"));

        JsonElement badge = await QcApi.GetDataAsync(client, $"{QcEndpoints.UnassignedCount}?year=2026");
        badge.Int("count").ShouldBe(QcFixtures.Scenario("core_default_T0_widenedP3").Int("badgeCurrentYear"));
    }

    /// <summary>A-213 · Xóa đợt làm đợt biến mất nhưng **không** đụng tới đảng viên nào (QT5).</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A213_Xoa_dot_khong_lam_mat_dang_vien()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        int membersBefore = await QcDb.CountMembersAsync(factory);

        JsonElement period = await FindPeriodAsync(client, "Đợt 19/5");

        using HttpResponseMessage response = await client.DeleteAsync($"{QcEndpoints.AwardPeriods}/{period.Str("id")}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        JsonElement data = await QcApi.GetDataAsync(client, QcEndpoints.AwardPeriods);
        data.Int("totalCount").ShouldBe(3);
        data.Array("periods").ShouldNotContain(item => item.Str("name") == "Đợt 19/5");

        (await QcDb.CountMembersAsync(factory)).ShouldBe(membersBefore);
    }

    /// <summary>
    /// A-214 · Đợt 7/11 năm 2026 có đúng 6 người, đúng phân bổ mốc, và sắp theo Mốc rồi tên gọi
    /// (mục 1.8 hợp đồng API).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A214_Du_dieu_kien_Dot_7_11_nam_2026()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        await AssertEligibleAsync(client, "Đợt 7/11", 2026, "core_default_T0", "P4");
    }

    /// <summary>A-215 và A-216 · Đợt 7/11 năm 2027 và Đợt 3/2 năm 2028 (ngày tròn mốc 29/02).</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A215_A216_Du_dieu_kien_o_nam_khac()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        IReadOnlyList<JsonElement> nextYear = await AssertEligibleAsync(client, "Đợt 7/11", 2027, "core_default_T0", "P4");
        nextYear.ShouldContain(row => row.Str("fullName") == "Đặng Thị Quỳnh" && row.Int("milestone") == 30);

        IReadOnlyList<JsonElement> leapYear = await AssertEligibleAsync(client, "Đợt 3/2", 2028, "core_default_T0", "P1");
        JsonElement lan = leapYear.Single(row => row.Str("fullName") == "Dương Thị Lan");
        lan.Int("milestone").ShouldBe(40);
        lan.Str("milestoneDate").ShouldBe("2028-02-29");
    }

    /// <summary>A-217 · Đợt 3/2 năm 2026 có Ngô Văn Khánh với ngày tròn mốc 28/02/2026.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A217_Du_dieu_kien_Dot_3_2_nam_2026()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        IReadOnlyList<JsonElement> rows = await AssertEligibleAsync(client, "Đợt 3/2", 2026, "core_default_T0", "P1");

        rows.Single(row => row.Str("fullName") == "Ngô Văn Khánh").Str("milestoneDate").ShouldBe("2026-02-28");
    }

    /// <summary>A-218 · Năm ngoài khoảng cho phép được xử lý tất định, không phải lỗi 500.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A218_Nam_bat_thuong_duoc_xu_ly_tat_dinh()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        JsonElement period = await FindPeriodAsync(client, "Đợt 7/11");

        foreach (string year in new[] { "1899", "2201", "0", "-5", "chu-khong-phai-so" })
        {
            foreach (string url in new[]
                     {
                         $"{QcEndpoints.AwardPeriods}?year={year}",
                         $"{QcEndpoints.Eligibility}?awardPeriodId={period.Str("id")}&year={year}",
                         $"{QcEndpoints.Unassigned}?year={year}",
                         $"{QcEndpoints.UnassignedCount}?year={year}",
                     })
            {
                using HttpResponseMessage response = await client.GetAsync(url);

                response.StatusCode.ShouldBeOneOf(
                    new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest }, url);

                if (response.StatusCode is HttpStatusCode.BadRequest)
                {
                    QcMessages.ShouldNotLeakInternals(await response.Content.ReadAsStringAsync());
                }
            }
        }

        // Năm trong khoảng cho phép vẫn phải chạy bình thường sau chuỗi trên.
        JsonElement ok = await QcApi.GetDataAsync(client, $"{QcEndpoints.AwardPeriods}?year=1900");
        ok.Int("year").ShouldBe(1900);
    }

    /// <summary>A-219 · Hỏi danh sách đủ điều kiện của đợt không tồn tại trả lỗi nghiệp vụ rõ ràng.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A219_Dot_khong_ton_tai_tra_loi_nghiep_vu()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        using HttpResponseMessage eligibility = await client.GetAsync(
            $"{QcEndpoints.Eligibility}?awardPeriodId={Guid.NewGuid()}");
        await QcApi.AssertErrorAsync(eligibility, HttpStatusCode.BadRequest, QcMessages.PeriodNotFound);

        using HttpResponseMessage detail = await client.GetAsync($"{QcEndpoints.AwardPeriods}/{Guid.NewGuid()}");
        await QcApi.AssertErrorAsync(detail, HttpStatusCode.BadRequest, QcMessages.PeriodNotFound);

        using HttpResponseMessage remove = await client.DeleteAsync($"{QcEndpoints.AwardPeriods}/{Guid.NewGuid()}");
        await QcApi.AssertErrorAsync(remove, HttpStatusCode.BadRequest, QcMessages.PeriodNotFound);
    }

    /// <summary>A-220 · Nạp thêm bộ lớn không làm lệch con số của bộ lõi.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A220_Bo_lon_khong_gay_nhieu_cho_bo_loi()
    {
        await QcDb.SeedCoreAndBulkAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        await AssertEligibleAsync(client, "Đợt 7/11", 2026, "core_default_T0", "P4");
    }

    /// <summary>
    /// Lấy chi tiết một đợt theo tên, qua endpoint danh sách rồi endpoint chi tiết — chạm cả
    /// mục 5.1 lẫn mục 5.2 của hợp đồng.
    /// </summary>
    /// <param name="client">Client đã đăng nhập.</param>
    /// <param name="name">Tên đợt.</param>
    /// <returns>Nút JSON của đợt.</returns>
    private static async Task<JsonElement> FindPeriodAsync(HttpClient client, string name)
    {
        JsonElement list = await QcApi.GetDataAsync(client, QcEndpoints.AwardPeriods);
        JsonElement fromList = list.Array("periods").Single(period => period.Str("name") == name);

        JsonElement detail = await QcApi.GetDataAsync(client, $"{QcEndpoints.AwardPeriods}/{fromList.Str("id")}");
        detail.Str("name").ShouldBe(name);

        return detail;
    }

    private static async Task<IReadOnlyList<JsonElement>> AssertEligibleAsync(
        HttpClient client, string periodName, int year, string scenario, string periodCode)
    {
        JsonElement period = await FindPeriodAsync(client, periodName);
        JsonElement expected = QcFixtures.Eligible(scenario, periodCode, year);

        JsonElement data = await QcApi.GetDataAsync(
            client, $"{QcEndpoints.Eligibility}?awardPeriodId={period.Str("id")}&year={year}");

        string label = $"{periodName} · {year}";

        data.Int("year").ShouldBe(year, label);
        data.Int("totalCount").ShouldBe(expected.Int("total"), label);
        data.GetProperty("awardPeriod").Str("fromDate").ShouldBe(expected.Str("boundFrom"), label);
        data.GetProperty("awardPeriod").Str("toDate").ShouldBe(expected.Str("boundTo"), label);

        IReadOnlyList<JsonElement> members = data.Array("members");
        members.Count.ShouldBe(expected.Int("total"), label);

        // Phân bổ theo mốc.
        Dictionary<string, int> actualBreakdown = data.Array("milestoneBreakdown")
            .ToDictionary(
                item => item.Int("milestone").ToString(CultureInfo.InvariantCulture),
                item => item.Int("count"),
                StringComparer.Ordinal);
        Dictionary<string, int> expectedBreakdown = expected.GetProperty("byMilestone")
            .EnumerateObject()
            .ToDictionary(item => item.Name, item => item.Value.GetInt32(), StringComparer.Ordinal);
        actualBreakdown.ShouldBe(expectedBreakdown, label);

        // Đúng người, đúng mốc, đúng ngày tròn mốc, và đúng thứ tự mục 1.8.
        List<JsonElement> expectedRows = expected.Array("rows").ToList();
        members.Select(row => row.Str("fullName")).ToList()
            .ShouldBe(expectedRows.Select(row => row.Str("fullName")).ToList(), label);

        for (int index = 0; index < members.Count; index++)
        {
            members[index].Int("milestone").ShouldBe(expectedRows[index].Int("milestone"), label);
            members[index].Str("milestoneDate").ShouldBe(expectedRows[index].Str("anniversary"), label);

            // QT5: danh sách đủ điều kiện không phải bản ghi, nên không có trường id riêng.
            members[index].TryGetProperty("id", out _).ShouldBeFalse(label);
            members[index].Str("partyMemberId").ShouldNotBeNullOrWhiteSpace();
        }

        return members;
    }

    private async Task AssertStatusesAsync(DateOnly today, string scenario)
    {
        using HttpClient client = await QcApi.LoginAsync(factory, today);

        JsonElement data = await QcApi.GetDataAsync(client, QcEndpoints.AwardPeriods);
        data.Str("today").ShouldBe(today.ToString("yyyy-MM-dd"));

        JsonElement expected = QcFixtures.Scenario(scenario).GetProperty("periodStatuses");

        foreach (JsonElement expectedPeriod in expected.EnumerateArray())
        {
            JsonElement actual = data.Array("periods").Single(period => period.Str("name") == expectedPeriod.Str("name"));
            string label = $"{expectedPeriod.Str("name")} tại {today:dd/MM/yyyy}";

            string status = actual.Str("status");
            status.ShouldBe(
                expectedPeriod.Str("status") switch
                {
                    "Đã qua" => "Past",
                    "Đang diễn ra" => "Ongoing",
                    "Sắp tới" => "Upcoming",
                    _ => throw new InvalidOperationException($"Trạng thái lạ: {expectedPeriod.Str("status")}"),
                },
                label);

            actual.IntOrNull("daysRemaining").ShouldBe(expectedPeriod.IntOrNull("daysLeft"), label);
        }
    }

    private async Task<HttpClient> SeedCoreAndLoginAsync()
    {
        await QcDb.SeedCoreAsync(factory);

        return await QcApi.LoginAsync(factory);
    }
}
