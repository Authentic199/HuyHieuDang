using System.Net;
using System.Text.Json;
using HuyHieuDang.Web.QcIntegrationTests.Support;

namespace HuyHieuDang.Web.QcIntegrationTests;

/// <summary>
/// A-701 → A-710 · Xuất Excel (UC-11, UC-34, UC-40). Ba endpoint xuất phải cho đúng tên file,
/// đúng dòng tiêu đề, đúng số dòng và đúng nội dung như bảng đang xem.
/// </summary>
[Collection(QcApiCollection.Name)]
public sealed class A7ExportTests
{
    private const int HeaderRow = 5;
    private const int FirstDataRow = 6;

    private static readonly string[] EligibilityColumns =
    {
        "STT", "Họ tên", "Giới tính", "Ngày sinh", "Ngày chính thức", "Ngày tròn mốc", "Mốc huy hiệu",
    };

    private readonly QcApiFactory factory;

    /// <summary>Initializes a new instance of the <see cref="A7ExportTests"/> class.</summary>
    /// <param name="factory">Host kiểm thử dùng chung.</param>
    public A7ExportTests(QcApiFactory factory)
    {
        this.factory = factory;
    }

    /// <summary>A-701 và A-702 · Tên file bỏ dấu, đổi <c>/</c> thành <c>-</c>, gắn năm.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A701_A702_Ten_file_dung_quy_uoc()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        JsonElement expected = QcFixtures.Node("exportFileNames");

        foreach ((string periodName, string key) in new[]
                 {
                     ("Đợt 7/11", "Đợt 7/11 năm 2026"),
                     ("Đợt 19/5", "Đợt 19/5 năm 2026"),
                     ("Đợt 3/2", "Đợt 3/2 năm 2026"),
                     ("Đợt 2/9", "Đợt 2/9 năm 2026"),
                 })
        {
            string periodId = await FindPeriodIdAsync(client, periodName);

            using HttpResponseMessage response = await client.GetAsync(
                $"{QcEndpoints.ExportEligibility}?awardPeriodId={periodId}&year=2026");

            FileNameOf(response).ShouldBe(expected.Str(key), periodName);
        }
    }

    /// <summary>A-703 · Tên file của danh sách chưa thuộc đợt nào.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A703_Ten_file_chua_thuoc_dot_nao()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        using HttpResponseMessage response = await client.GetAsync($"{QcEndpoints.ExportUnassigned}?year=2026");

        FileNameOf(response).ShouldBe(QcFixtures.Node("exportFileNames").Str("Chưa thuộc đợt nào năm 2026"));
    }

    /// <summary>
    /// A-704 · Dòng tiêu đề: tên đơn vị, tên đợt kèm khoảng ngày **đã gắn năm**, và ngày xuất
    /// bằng đúng hôm nay của máy chủ.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A704_Dong_tieu_de_du_ba_phan()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        string periodId = await FindPeriodIdAsync(client, "Đợt 7/11");

        QcWorkbook workbook = await DownloadAsync(
            client, $"{QcEndpoints.ExportEligibility}?awardPeriodId={periodId}&year=2026");

        workbook.RowText(1).ShouldBe(QcFixtures.Settings["default"].UnitName);
        workbook.RowText(2).ShouldContain("Đợt 7/11");
        workbook.RowText(2).ShouldContain("01/10/2026");
        workbook.RowText(2).ShouldContain("07/11/2026");
        workbook.RowText(3).ShouldContain(QcClock.T0.ToString("dd/MM/yyyy"));
        workbook.RowText(4).ShouldBeEmpty();
    }

    /// <summary>A-705 · Tên đơn vị để trống thì tiêu đề bỏ hẳn dòng đó, không để dòng trắng lạ.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A705_Ten_don_vi_de_trong_thi_bo_dong_do()
    {
        await QcDb.SeedCoreAsync(factory, "noUnitName");
        using HttpClient client = await QcApi.LoginAsync(factory);

        string periodId = await FindPeriodIdAsync(client, "Đợt 7/11");
        QcWorkbook workbook = await DownloadAsync(
            client, $"{QcEndpoints.ExportEligibility}?awardPeriodId={periodId}&year=2026");

        workbook.RowText(1).ShouldContain("Đợt 7/11");
        workbook.RowText(2).ShouldContain(QcClock.T0.ToString("dd/MM/yyyy"));

        // Dòng tiêu đề cột lùi lên một dòng so với khi có tên đơn vị.
        workbook.Row(4).ShouldBe(EligibilityColumns);
    }

    /// <summary>
    /// A-706 → A-708 · Đúng 6 dòng dữ liệu cho Đợt 7/11 · 2026, đúng cột, đúng thứ tự, ô trống
    /// để **rỗng** chứ không ghi dấu gạch ngang.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A706_A707_A708_Noi_dung_file_khop_bang_dang_xem()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        string periodId = await FindPeriodIdAsync(client, "Đợt 7/11");

        JsonElement onScreen = await QcApi.GetDataAsync(
            client, $"{QcEndpoints.Eligibility}?awardPeriodId={periodId}&year=2026");
        IReadOnlyList<JsonElement> members = onScreen.Array("members");

        QcWorkbook workbook = await DownloadAsync(
            client, $"{QcEndpoints.ExportEligibility}?awardPeriodId={periodId}&year=2026");

        workbook.Row(HeaderRow).ShouldBe(EligibilityColumns);
        workbook.RowCount.ShouldBe(FirstDataRow - 1 + members.Count);
        members.Count.ShouldBe(QcFixtures.Eligible("core_default_T0", "P4", 2026).Int("total"));

        for (int index = 0; index < members.Count; index++)
        {
            JsonElement member = members[index];
            int row = FirstDataRow + index;
            string label = member.Str("fullName");

            workbook.Cell(row, 1).ShouldBe((index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture), label);
            workbook.Cell(row, 2).ShouldBe(member.Str("fullName"), label);
            workbook.Cell(row, 3).ShouldBe(GenderText(member.StringOrNull("gender")), label);
            workbook.Cell(row, 4).ShouldBe(DateText(member.StringOrNull("dateOfBirth")), label);
            workbook.Cell(row, 5).ShouldBe(DateText(member.Str("officialAdmissionDate")), label);
            workbook.Cell(row, 6).ShouldBe(DateText(member.Str("milestoneDate")), label);
            workbook.Cell(row, 7).ShouldBe(member.Int("milestone").ToString(System.Globalization.CultureInfo.InvariantCulture), label);
        }

        // A-707: người không có Ngày sinh và Giới tính phải cho ô rỗng, không phải "—".
        string wholeFile = string.Join("\n", Enumerable.Range(1, workbook.RowCount).Select(workbook.RowText));
        wholeFile.ShouldNotContain("—");
    }

    /// <summary>A-709 · Xuất khi danh sách rỗng vẫn ra file có tiêu đề và 0 dòng dữ liệu.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A709_Xuat_danh_sach_rong_van_ra_file_hop_le()
    {
        await QcDb.ResetAsync(factory);
        await QcDb.SeedPeriodsAsync(factory, QcFixtures.MainPeriods);

        using HttpClient client = await QcApi.LoginAsync(factory);
        string periodId = await FindPeriodIdAsync(client, "Đợt 7/11");

        QcWorkbook eligibility = await DownloadAsync(
            client, $"{QcEndpoints.ExportEligibility}?awardPeriodId={periodId}&year=2026");
        eligibility.Row(HeaderRow).ShouldBe(EligibilityColumns);
        eligibility.RowCount.ShouldBe(HeaderRow);

        QcWorkbook unassigned = await DownloadAsync(client, $"{QcEndpoints.ExportUnassigned}?year=2026");
        unassigned.RowCount.ShouldBe(HeaderRow);
    }

    /// <summary>
    /// A-710 · Mở file bằng thư viện đọc Excel thật: đọc được, đúng một sheet, không cảnh báo
    /// hỏng. Kiểm cả ba endpoint xuất.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A710_Ba_endpoint_xuat_deu_mo_duoc_bang_thu_vien_doc_Excel()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        string periodId = await FindPeriodIdAsync(client, "Đợt 7/11");

        foreach (string url in new[]
                 {
                     $"{QcEndpoints.ExportEligibility}?awardPeriodId={periodId}&year=2026",
                     QcEndpoints.ExportDashboard,
                     $"{QcEndpoints.ExportUnassigned}?year=2026",
                 })
        {
            QcWorkbook workbook = await DownloadAsync(client, url);

            workbook.SheetCount.ShouldBe(1, url);
            workbook.RowCount.ShouldBeGreaterThanOrEqualTo(HeaderRow, url);
        }
    }

    /// <summary>
    /// A-711 · Xuất từ Dashboard lấy đúng đợt sắp tới theo QT8, kể cả khi đợt đó thuộc năm sau
    /// (mục 8.2 hợp đồng API), và báo lỗi nói được khi chưa cài đợt nào.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A711_Xuat_tu_Dashboard_theo_dot_sap_toi()
    {
        await QcDb.SeedCoreAsync(factory);

        using HttpClient atT0 = await QcApi.LoginAsync(factory);
        using HttpResponseMessage t0 = await atT0.GetAsync(QcEndpoints.ExportDashboard);
        FileNameOf(t0).ShouldBe(QcFixtures.Node("exportFileNames").Str("Đợt 7/11 năm 2026"));

        // Tại T2 mọi đợt 2026 đã qua nên Dashboard xuất đợt của năm sau.
        using HttpClient atT2 = await QcApi.LoginAsync(factory, QcClock.T2);
        using HttpResponseMessage t2 = await atT2.GetAsync(QcEndpoints.ExportDashboard);
        FileNameOf(t2).ShouldBe("DuDieuKien_Dot3-2_2027.xlsx");

        // Chưa cài đợt nào thì báo lỗi nghiệp vụ, không phải file rỗng.
        await QcDb.ResetAsync(factory);
        await QcDb.SeedMembersAsync(factory, QcFixtures.CoreMembers);

        using HttpClient empty = await QcApi.LoginAsync(factory);
        using HttpResponseMessage noPeriod = await empty.GetAsync(QcEndpoints.ExportDashboard);

        await QcApi.AssertErrorAsync(
            noPeriod, HttpStatusCode.BadRequest, QcMessages.DashboardNoUpcomingPeriod);
    }

    /// <summary>
    /// A-712 · File "chưa thuộc đợt nào" có thêm cột cuối "Khoảng trống", nội dung khớp bảng
    /// đang xem.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A712_File_chua_thuoc_dot_nao_co_cot_khoang_trong()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        JsonElement onScreen = await QcApi.GetDataAsync(client, $"{QcEndpoints.Unassigned}?year=2026");
        IReadOnlyList<JsonElement> members = onScreen.Array("members");

        QcWorkbook workbook = await DownloadAsync(client, $"{QcEndpoints.ExportUnassigned}?year=2026");

        workbook.Row(HeaderRow).ShouldBe(EligibilityColumns.Append("Khoảng trống").ToArray());
        workbook.RowText(2).ShouldContain("2026");
        workbook.RowCount.ShouldBe(HeaderRow + members.Count);

        for (int index = 0; index < members.Count; index++)
        {
            workbook.Cell(FirstDataRow + index, 2).ShouldBe(members[index].Str("fullName"));
            workbook.Cell(FirstDataRow + index, 8).ShouldNotBeNullOrWhiteSpace();
        }
    }

    private static string GenderText(string? gender) => gender switch
    {
        "Male" => "Nam",
        "Female" => "Nữ",
        _ => string.Empty,
    };

    private static string DateText(string? isoDate)
        => isoDate is null
            ? string.Empty
            : DateOnly.Parse(isoDate, System.Globalization.CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");

    private static string FileNameOf(HttpResponseMessage response)
    {
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.ShouldBe(QcApi.ExcelContentType);

        return response.Content.Headers.ContentDisposition!.FileName!.Trim('"');
    }

    private static async Task<QcWorkbook> DownloadAsync(HttpClient client, string url)
    {
        using HttpResponseMessage response = await client.GetAsync(url);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, url);
        response.Content.Headers.ContentType!.MediaType.ShouldBe(QcApi.ExcelContentType, url);

        return QcWorkbook.Read(await response.Content.ReadAsByteArrayAsync());
    }

    private static async Task<string> FindPeriodIdAsync(HttpClient client, string name)
    {
        JsonElement data = await QcApi.GetDataAsync(client, QcEndpoints.AwardPeriods);

        return data.Array("periods").Single(period => period.Str("name") == name).Str("id");
    }

    private async Task<HttpClient> SeedCoreAndLoginAsync()
    {
        await QcDb.SeedCoreAsync(factory);

        return await QcApi.LoginAsync(factory);
    }
}
