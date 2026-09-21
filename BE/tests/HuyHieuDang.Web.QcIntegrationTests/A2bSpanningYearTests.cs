using System.Globalization;
using System.Net;
using System.Text.Json;
using HuyHieuDang.Web.QcIntegrationTests.Support;

namespace HuyHieuDang.Web.QcIntegrationTests;

/// <summary>
/// A-221 → A-224 · Đợt trao huy hiệu vắt qua 31/12 trên API thật (T54, QT4, QT6, QT7, QT8, QT11).
/// <para>
/// Mã ca: CEO đặt A-210 → A-213 trong T54, nhưng bốn mã ấy đã có chủ trong
/// <c>docs/test-plan.md</c> (5 khoảng trống, bộ phủ kín, nới Đến ngày, xóa đợt). Khối mới nhận
/// A-221 → A-224; bảng quy đổi nằm trong
/// <c>docs/test-report/2026-09-20-qc-dot-vat-qua-nam.md</c>.
/// </para>
/// </summary>
[Collection(QcApiCollection.Name)]
public sealed class A2bSpanningYearTests
{
    /// <summary>Ca thật của đảng ủy: 01/12 năm nay đến 28/02 năm sau.</summary>
    private static readonly QcPeriod GiaoThua = new()
    {
        Code = "PS1",
        Name = "Đợt Giao thừa",
        FromDay = 1,
        FromMonth = 12,
        ToDay = 28,
        ToMonth = 2,
    };

    /// <summary>Tròn mốc 30 vào 20/01/2027 — nằm trong đuôi đầu năm của đợt vắt năm.</summary>
    private static readonly QcMember ThangGieng = new()
    {
        Code = "S01",
        FullName = "Bùi Văn Giêng",
        Gender = "Nam",
        DateOfBirth = new DateOnly(1970, 1, 20),
        OfficialAdmissionDate = new DateOnly(1997, 1, 20),
    };

    /// <summary>Tròn mốc 30 vào 10/02/2027 — nửa sau của lần diễn ra neo ở 2026.</summary>
    private static readonly QcMember ThangHai = new()
    {
        Code = "S02",
        FullName = "Cao Thị Hai",
        Gender = "Nữ",
        DateOfBirth = new DateOnly(1971, 2, 10),
        OfficialAdmissionDate = new DateOnly(1997, 2, 10),
    };

    /// <summary>Tròn mốc 30 vào 15/12/2026 — nửa đầu của cùng lần diễn ra ấy.</summary>
    private static readonly QcMember ThangChap = new()
    {
        Code = "S03",
        FullName = "Đinh Văn Chạp",
        Gender = "Nam",
        DateOfBirth = new DateOnly(1969, 12, 15),
        OfficialAdmissionDate = new DateOnly(1996, 12, 15),
    };

    /// <summary>Tròn mốc 30 vào 15/06/2027 — giữa năm, ngoài mọi đợt.</summary>
    private static readonly QcMember GiuaNam = new()
    {
        Code = "S04",
        FullName = "Hoàng Thị Hè",
        Gender = "Nữ",
        DateOfBirth = new DateOnly(1972, 6, 15),
        OfficialAdmissionDate = new DateOnly(1997, 6, 15),
    };

    private readonly QcApiFactory factory;

    /// <summary>Initializes a new instance of the <see cref="A2bSpanningYearTests"/> class.</summary>
    /// <param name="factory">Host kiểm thử dùng chung.</param>
    public A2bSpanningYearTests(QcApiFactory factory)
    {
        this.factory = factory;
    }

    /// <summary>
    /// A-221 · <c>GET /AwardPeriods?year=Y</c> với đợt vắt năm: <c>toDate</c> thuộc <c>Y+1</c>,
    /// <c>spansNextYear = true</c>, dải độ phủ có hai đoạn mang cùng <c>periodId</c>.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A221_Danh_sach_dot_vat_nam_gan_dung_nam_va_cho_hai_doan_do_phu()
    {
        using HttpClient client = await SeedSpanningAsync();

        JsonElement data = await QcApi.GetDataAsync(client, $"{QcEndpoints.AwardPeriods}?year=2026");
        JsonElement period = data.Array("periods").Single(item => item.Str("name") == GiaoThua.Name);

        period.Str("fromDate").ShouldBe("2026-12-01");
        period.Str("toDate").ShouldBe("2027-02-28");
        period.GetProperty("spansNextYear").GetBoolean().ShouldBeTrue();
        period.Str("fromDisplay").ShouldBe("01/12");
        period.Str("toDisplay").ShouldBe("28/02");

        IReadOnlyList<JsonElement> segments = data.GetProperty("coverage").Array("segments");
        List<JsonElement> mine = segments
            .Where(segment => segment.StringOrNull("periodId") == period.Str("id"))
            .ToList();

        mine.Count.ShouldBe(2, "đợt vắt năm phải để lại hai đoạn trong một năm");
        mine[0].Str("fromDate").ShouldBe("2026-01-01");
        mine[0].Str("toDate").ShouldBe("2026-02-28");
        mine[1].Str("fromDate").ShouldBe("2026-12-01");
        mine[1].Str("toDate").ShouldBe("2026-12-31");

        // Hai đoạn của chính nó không được sinh cảnh báo chồng lấn, còn khoảng trống thì đúng
        // phần giữa năm.
        data.GetProperty("warnings").Array("overlaps").ShouldBeEmpty();

        IReadOnlyList<JsonElement> gaps = data.GetProperty("warnings").Array("gaps");
        gaps.Count.ShouldBe(1);
        gaps[0].Str("fromDate").ShouldBe("2026-03-01");
        gaps[0].Str("toDate").ShouldBe("2026-11-30");

        // Dải độ phủ vẫn phủ liên tục 01/01 – 31/12 (mục 5.1 hợp đồng API).
        segments[0].Str("fromDate").ShouldBe("2026-01-01");
        segments[^1].Str("toDate").ShouldBe("2026-12-31");
    }

    /// <summary>
    /// A-222 · <c>GET /Eligibility</c> và <c>/Eligibility/Unassigned</c>: người tròn mốc 20/01
    /// thuộc đuôi đợt nên **không** bị xếp vào "chưa thuộc đợt nào" của năm 2027.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A222_Nguoi_tron_moc_20_01_khong_bi_xep_vao_chua_thuoc_dot_nao()
    {
        using HttpClient client = await SeedSpanningAsync();
        string periodId = await FindPeriodIdAsync(client, GiaoThua.Name);

        // Lần diễn ra neo ở 2026 gom cả ba người: 15/12/2026, 20/01/2027 và 10/02/2027.
        JsonElement eligibility = await QcApi.GetDataAsync(
            client, $"{QcEndpoints.Eligibility}?awardPeriodId={periodId}&year=2026");

        eligibility.Int("totalCount").ShouldBe(3);
        eligibility.Array("members").Select(member => member.Str("fullName")).ToList()
            .ShouldBe([ThangChap.FullName, ThangGieng.FullName, ThangHai.FullName], ignoreOrder: true);

        JsonElement unassigned = await QcApi.GetDataAsync(client, $"{QcEndpoints.Unassigned}?year=2027");
        List<string> names = unassigned.Array("members").Select(member => member.Str("fullName")).ToList();

        names.ShouldNotContain(ThangGieng.FullName, "20/01 nằm trong đuôi đầu năm của đợt vắt năm");
        names.ShouldNotContain(ThangHai.FullName, "10/02 cũng nằm trong đuôi ấy");
        names.ShouldContain(GiuaNam.FullName, "15/06 mới đúng là người rơi vào khoảng trống");

        // Badge menu trái phải bằng đúng số dòng của danh sách (mục 6.4 hợp đồng API).
        JsonElement badge = await QcApi.GetDataAsync(client, $"{QcEndpoints.UnassignedCount}?year=2027");
        badge.Int("count").ShouldBe(unassigned.Int("totalCount"));
        badge.Int("count").ShouldBe(1);

        // Không ai vừa "đủ điều kiện" vừa "chưa thuộc đợt nào" trong cùng một năm (dấu hiệu 2
        // của T54): mốc 20/01/2027 thuộc năm 2027 và đã được đợt phủ.
        names.ShouldNotContain(ThangChap.FullName);
    }

    /// <summary>
    /// A-223 · <c>GET /Dashboard</c> lúc đợt vắt năm đang mở: đợt sắp tới là lần diễn ra neo ở
    /// **năm trước** và trạng thái là Đang diễn ra (QT8, QT11).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A223_Dashboard_doc_dung_lan_dien_ra_neo_o_nam_truoc()
    {
        DateOnly today = new(2027, 1, 15);
        await SeedSpanningDataAsync();

        using HttpClient client = await QcApi.LoginAsync(factory, today);

        JsonElement dashboard = await QcApi.GetDataAsync(client, QcEndpoints.Dashboard);
        JsonElement upcoming = dashboard.GetProperty("upcomingPeriod");

        dashboard.Str("today").ShouldBe("2027-01-15");
        upcoming.Str("name").ShouldBe(GiaoThua.Name);
        upcoming.Int("year").ShouldBe(2026, "năm neo là năm chứa Từ ngày");
        upcoming.Str("fromDate").ShouldBe("2026-12-01");
        upcoming.Str("toDate").ShouldBe("2027-02-28");
        upcoming.Str("status").ShouldBe("Ongoing");
        upcoming.GetProperty("daysRemaining").ValueKind.ShouldBe(JsonValueKind.Null);
        upcoming.GetProperty("isNextYear").GetBoolean().ShouldBeFalse();

        // Bảng danh sách đợt của năm 2027 cũng phải đọc "Đang diễn ra" chứ không phải
        // "Sắp tới · 320 ngày" của lần kế tiếp.
        JsonElement list = await QcApi.GetDataAsync(client, $"{QcEndpoints.AwardPeriods}?year=2027");
        JsonElement row = list.Array("periods").Single(item => item.Str("name") == GiaoThua.Name);

        row.Str("status").ShouldBe("Ongoing");

        // Tổng số người của đợt trên Dashboard bằng đúng danh sách đủ điều kiện của chính lần
        // diễn ra ấy (dấu hiệu 4 của T54).
        string periodId = await FindPeriodIdAsync(client, GiaoThua.Name);
        JsonElement eligibility = await QcApi.GetDataAsync(
            client, $"{QcEndpoints.Eligibility}?awardPeriodId={periodId}&year=2026");

        upcoming.Int("eligibleCount").ShouldBe(eligibility.Int("totalCount"));
        dashboard.Array("eligibleMembers").Count.ShouldBe(eligibility.Int("totalCount"));
    }

    /// <summary>
    /// A-224 · Xuất Excel đợt vắt năm: tên file theo quy ước mục 1.9, dòng tiêu đề ghi đúng
    /// khoảng ngày đã gắn năm — kể cả phần thuộc năm sau.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A224_Xuat_excel_dot_vat_nam_dung_ten_file_va_dong_tieu_de()
    {
        using HttpClient client = await SeedSpanningAsync();
        string periodId = await FindPeriodIdAsync(client, GiaoThua.Name);
        string url = $"{QcEndpoints.ExportEligibility}?awardPeriodId={periodId}&year=2026";

        using HttpResponseMessage response = await client.GetAsync(url);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.ShouldBe(QcApi.ExcelContentType);
        response.Content.Headers.ContentDisposition!.FileName!.Trim('"')
            .ShouldBe("DuDieuKien_DotGiaothua_2026.xlsx");

        QcWorkbook workbook = QcWorkbook.Read(await response.Content.ReadAsByteArrayAsync());

        workbook.RowText(2).ShouldContain(GiaoThua.Name);
        workbook.RowText(2).ShouldContain("01/12/2026");
        workbook.RowText(2).ShouldContain("28/02/2027");
        workbook.RowText(3).ShouldContain(QcClock.T0.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture));

        // Số dòng dữ liệu bằng đúng danh sách đang xem trên màn hình (mục 1.8 hợp đồng API).
        JsonElement onScreen = await QcApi.GetDataAsync(
            client, $"{QcEndpoints.Eligibility}?awardPeriodId={periodId}&year=2026");

        workbook.RowCount.ShouldBe(5 + onScreen.Int("totalCount"));
    }

    private static async Task<string> FindPeriodIdAsync(HttpClient client, string name)
    {
        JsonElement data = await QcApi.GetDataAsync(client, QcEndpoints.AwardPeriods);

        return data.Array("periods").Single(period => period.Str("name") == name).Str("id");
    }

    /// <summary>Kho chỉ có đợt vắt năm và bốn đảng viên đủ để phân biệt hai đầu đợt.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    private async Task SeedSpanningDataAsync()
    {
        await QcDb.ResetAsync(factory);
        await QcDb.SeedPeriodsAsync(factory, [GiaoThua]);
        await QcDb.SeedMembersAsync(factory, [ThangChap, ThangGieng, ThangHai, GiuaNam]);
    }

    private async Task<HttpClient> SeedSpanningAsync()
    {
        await SeedSpanningDataAsync();

        return await QcApi.LoginAsync(factory);
    }
}
