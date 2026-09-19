using System.Globalization;
using System.Net;
using System.Text.Json;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.Eligibility.Requests;
using HuyHieuDang.Infrastructure.Modules.Eligibility.Responses;
using HuyHieuDang.Infrastructure.Modules.Exports;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports;
using AwardPeriod = HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities.AwardPeriod;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Mục 8 hợp đồng API (UC-11, UC-34, UC-40) và các ca A-701 → A-710 của kế hoạch kiểm thử.
/// Mọi khẳng định đọc lại từ file thật tải về, và mọi con số mong đợi lấy từ
/// <c>tests/fixtures/data/expected.json</c>.
/// </summary>
[Collection(ApiCollection.Name)]
public class ExportEndpointTests
{
    private const string BasePath = "/api/Exports";
    private const string EligibilityPath = BasePath + "/Eligibility";
    private const string DashboardPath = BasePath + "/Dashboard";
    private const string UnassignedPath = BasePath + "/Unassigned";

    /// <summary>
    /// Ba dòng chữ + một dòng trống + một dòng đầu cột.
    /// </summary>
    private const int HeaderRowsWithUnitName = 5;

    /// <summary>
    /// Bỏ dòng tên đơn vị thì phần tiêu đề ngắn đi đúng một dòng (A-705).
    /// </summary>
    private const int HeaderRowsWithoutUnitName = HeaderRowsWithUnitName - 1;

    private readonly HuyHieuDangApiFactory factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportEndpointTests"/> class.
    /// </summary>
    /// <param name="factory">Host kiểm thử dùng chung cho cả lớp.</param>
    public ExportEndpointTests(HuyHieuDangApiFactory factory)
    {
        this.factory = factory;
    }

    [Theory(DisplayName = "A-701, A-702, A-703 · Tên file đúng quy tắc rút gọn của mục 1.9")]
    [InlineData("P4", "Đợt 7/11 năm 2026")]
    [InlineData("P2", "Đợt 19/5 năm 2026")]
    [InlineData("P1", "Đợt 3/2 năm 2026")]
    [InlineData("P3", "Đợt 2/9 năm 2026")]
    public async Task ExportEligibility_UsesFileNameFromFixture(string periodCode, string expectedKey)
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        Guid periodId = await FindPeriodIdAsync(client, periodCode);
        ExportedWorkbook file = await ExportedWorkbook.DownloadAsync(
            client, $"{EligibilityPath}?awardPeriodId={periodId}&year=2026");

        Assert.Equal(ExpectedFileName(expectedKey), file.FileName);
        Assert.Equal(PartyMemberImportFile.ExcelContentType, file.ContentType);
        Assert.Contains($"filename=\"{file.FileName}\"", file.ContentDisposition, StringComparison.Ordinal);
        Assert.Contains($"filename*=UTF-8''{file.FileName}", file.ContentDisposition, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "A-703 · Tên file chưa thuộc đợt nào là ChuaThuocDot_<năm>.xlsx")]
    public async Task ExportUnassigned_UsesFileNameFromFixture()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        ExportedWorkbook file = await ExportedWorkbook.DownloadAsync(client, $"{UnassignedPath}?year=2026");

        Assert.Equal(ExpectedFileName("Chưa thuộc đợt nào năm 2026"), file.FileName);
        Assert.Equal(PartyMemberImportFile.ExcelContentType, file.ContentType);
    }

    [Fact(DisplayName = "A-704, A-706, A-708, A-710 · Đợt 7/11 · 2026: tiêu đề, cột và 6 dòng dữ liệu")]
    public async Task ExportEligibility_HasExpectedHeaderColumnsAndRows()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        Guid periodId = await FindPeriodIdAsync(client, "P4");
        JsonElement expected = EligibleOfYear("P4", 2026);

        ExportedWorkbook file = await ExportedWorkbook.DownloadAsync(
            client, $"{EligibilityPath}?awardPeriodId={periodId}&year=2026");

        Assert.Equal(1, file.SheetCount);
        Assert.Equal(CoreFixtures.Settings["default"].UnitName, file.Line(0));
        Assert.Equal("Đợt 7/11 · 01/10/2026 – 07/11/2026", file.Line(1));
        Assert.Equal($"Ngày xuất: {Display(HuyHieuDangApiFactory.FixedToday)}", file.Line(2));
        Assert.Equal(string.Empty, file.Line(3));
        Assert.Equal(ExportSheet.EligibilityColumns, file.Rows[4]);

        IReadOnlyList<IReadOnlyList<string>> dataRows = file.DataRows(HeaderRowsWithUnitName);

        Assert.Equal(6, expected.GetProperty("total").GetInt32());
        Assert.Equal(6, dataRows.Count);
        AssertOrderMatches(expected, dataRows);

        // Cột STT chạy liên tục từ 1 và cột Mốc huy hiệu khớp từng dòng của expected.json.
        Assert.Equal(
            Enumerable.Range(1, dataRows.Count).Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray(),
            dataRows.Select(row => row[0]).ToArray());
    }

    [Fact(DisplayName = "A-705 · Tên đơn vị trống: dòng 1 là tên đợt, không có dòng trắng thừa")]
    public async Task ExportEligibility_WithoutUnitName_DropsTheUnitLine()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory, settingsKey: "noUnitName");

        Guid periodId = await FindPeriodIdAsync(client, "P4");
        ExportedWorkbook file = await ExportedWorkbook.DownloadAsync(
            client, $"{EligibilityPath}?awardPeriodId={periodId}&year=2026");

        Assert.Equal("Đợt 7/11 · 01/10/2026 – 07/11/2026", file.Line(0));
        Assert.Equal($"Ngày xuất: {Display(HuyHieuDangApiFactory.FixedToday)}", file.Line(1));
        Assert.Equal(string.Empty, file.Line(2));
        Assert.Equal(ExportSheet.EligibilityColumns, file.Rows[3]);
        Assert.Equal(6, file.DataRows(HeaderRowsWithoutUnitName).Count);
    }

    [Fact(DisplayName = "A-707 · E01 thiếu Ngày sinh và Giới tính: hai ô rỗng, không phải dấu gạch ngang")]
    public async Task ExportEligibility_WithBlankFields_WritesEmptyCells()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        // E01 "Cao Thị Thu" đủ điều kiện Đợt 19/5 mốc 30 — đó là file duy nhất có người này.
        Guid periodId = await FindPeriodIdAsync(client, "P2");
        FixtureMember blank = CoreFixtures.CoreMembers.Single(x => x.Code == "E01");

        ExportedWorkbook file = await ExportedWorkbook.DownloadAsync(
            client, $"{EligibilityPath}?awardPeriodId={periodId}&year=2026");

        IReadOnlyList<string> row = file
            .DataRows(HeaderRowsWithUnitName)
            .Single(x => x[1] == blank.FullName);

        Assert.Equal(string.Empty, row[2]);
        Assert.Equal(string.Empty, row[3]);
        Assert.Equal(Display(blank.OfficialAdmissionDate), row[4]);

        // Người có đủ dữ liệu vẫn ghi Nam / Nữ và ngày dd/MM/yyyy, để ô rỗng không phải do lỗi chung.
        FixtureMember filled = CoreFixtures.CoreMembers.Single(x => x.Code == "B09");
        IReadOnlyList<string> other = file
            .DataRows(HeaderRowsWithUnitName)
            .Single(x => x[1] == filled.FullName);

        Assert.Equal(filled.Gender, other[2]);
        Assert.Equal(Display(filled.DateOfBirth!.Value), other[3]);
    }

    [Fact(DisplayName = "8.3 · Chưa thuộc đợt nào 2026: 7 dòng, có cột Khoảng trống đúng nhãn")]
    public async Task ExportUnassigned_HasGapColumnAndSevenRows()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        JsonElement expected = MissedOfYear(2026);
        ExportedWorkbook file = await ExportedWorkbook.DownloadAsync(client, $"{UnassignedPath}?year=2026");

        Assert.Equal(1, file.SheetCount);
        Assert.Equal(CoreFixtures.Settings["default"].UnitName, file.Line(0));
        Assert.Equal("Chưa thuộc đợt nào · năm 2026", file.Line(1));
        Assert.Equal($"Ngày xuất: {Display(HuyHieuDangApiFactory.FixedToday)}", file.Line(2));
        Assert.Equal(string.Empty, file.Line(3));
        Assert.Equal(ExportSheet.UnassignedColumns, file.Rows[4]);
        Assert.Equal(ExportSheet.GapColumn, file.Rows[4][^1]);

        IReadOnlyList<IReadOnlyList<string>> dataRows = file.DataRows(HeaderRowsWithUnitName);

        Assert.Equal(7, expected.GetProperty("total").GetInt32());
        Assert.Equal(7, dataRows.Count);
        AssertOrderMatches(expected, dataRows);
        Assert.Equal(
            expected.GetProperty("rows").EnumerateArray().Select(x => x.GetProperty("gapLabel").GetString()).ToArray(),
            dataRows.Select(row => row[7]).ToArray());
    }

    [Fact(DisplayName = "8.2 · Xuất Dashboard lấy đúng đợt sắp tới theo QT8")]
    public async Task ExportDashboard_UsesUpcomingPeriod()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        JsonElement upcoming = CoreFixtures.Scenario("core_default_T0").GetProperty("upcomingPeriod");
        ExportedWorkbook file = await ExportedWorkbook.DownloadAsync(client, DashboardPath);

        Assert.Equal(ExpectedFileName("Đợt 7/11 năm 2026"), file.FileName);
        Assert.Equal(
            $"{upcoming.GetProperty("name").GetString()} · "
            + $"{upcoming.GetProperty("boundFromDisplay").GetString()} – {upcoming.GetProperty("boundToDisplay").GetString()}",
            file.Line(1));
        AssertOrderMatches(EligibleOfYear("P4", 2026), file.DataRows(HeaderRowsWithUnitName));
    }

    [Fact(DisplayName = "8.2 · Mọi đợt của năm nay đã qua: file mang đợt đầu năm sau")]
    public async Task ExportDashboard_WhenYearIsOver_UsesNextYearPeriod()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory, ApiClientFactory.T2);
        await CoreDataset.SeedAllAsync(factory);

        JsonElement upcoming = CoreFixtures.Scenario("core_default_T2").GetProperty("upcomingPeriod");
        ExportedWorkbook file = await ExportedWorkbook.DownloadAsync(client, DashboardPath);

        Assert.Equal(2027, upcoming.GetProperty("year").GetInt32());
        Assert.Equal(
            ExportFileName.ForEligibility(upcoming.GetProperty("name").GetString()!, 2027), file.FileName);
        Assert.Equal($"Ngày xuất: {Display(ApiClientFactory.T2)}", file.Line(2));
    }

    [Fact(DisplayName = "A-709 · Danh sách rỗng vẫn trả file hợp lệ chỉ có phần tiêu đề")]
    public async Task ExportEligibility_WithEmptyList_StillReturnsValidFile()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        // Đợt 3/2 năm 2027 không có ai đủ điều kiện theo expected.json.
        Guid periodId = await FindPeriodIdAsync(client, "P1");
        ExportedWorkbook file = await ExportedWorkbook.DownloadAsync(
            client, $"{EligibilityPath}?awardPeriodId={periodId}&year=2027");

        Assert.Equal(0, EligibleOfYear("P1", 2027).GetProperty("total").GetInt32());
        Assert.Equal(1, file.SheetCount);
        Assert.Equal(HeaderRowsWithUnitName, file.Rows.Count);
        Assert.Equal(ExportSheet.EligibilityColumns, file.Rows[4]);
        Assert.Empty(file.DataRows(HeaderRowsWithUnitName));
    }

    [Fact(DisplayName = "A-709 · Không ai bị sót: file chưa thuộc đợt nào vẫn hợp lệ")]
    public async Task ExportUnassigned_WithNobodyMissed_StillReturnsValidFile()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.ResetAsync(factory);
        await CoreDataset.SeedPeriodsAsync(factory);

        ExportedWorkbook file = await ExportedWorkbook.DownloadAsync(client, $"{UnassignedPath}?year=2026");

        Assert.Equal(HeaderRowsWithUnitName, file.Rows.Count);
        Assert.Equal(ExportSheet.UnassignedColumns, file.Rows[4]);
    }

    [Fact(DisplayName = "8.1, 8.3 · Bỏ trống year thì lấy năm hiện tại của máy chủ")]
    public async Task ExportEndpoints_WithoutYear_UseCurrentYear()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        int year = HuyHieuDangApiFactory.FixedToday.Year;
        Guid periodId = await FindPeriodIdAsync(client, "P4");

        ExportedWorkbook eligibility =
            await ExportedWorkbook.DownloadAsync(client, $"{EligibilityPath}?awardPeriodId={periodId}");
        ExportedWorkbook unassigned = await ExportedWorkbook.DownloadAsync(client, UnassignedPath);

        Assert.Equal(ExportFileName.ForEligibility("Đợt 7/11", year), eligibility.FileName);
        Assert.Equal(ExportFileName.ForUnassigned(year), unassigned.FileName);
    }

    [Fact(DisplayName = "8.1 · Id đợt lạ trả Mes.AwardPeriod.NotFound")]
    public async Task ExportEligibility_WithUnknownPeriod_ReturnsNotFound()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        await ApiClientFactory.AssertErrorAsync(
            await client.GetAsync($"{EligibilityPath}?awardPeriodId={Guid.NewGuid()}"),
            Messages<AwardPeriod>.NotFound());
    }

    [Theory(DisplayName = "8.1, 8.3 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year")]
    [InlineData(EligibilityPath, 1899)]
    [InlineData(EligibilityPath, 2201)]
    [InlineData(UnassignedPath, 0)]
    [InlineData(UnassignedPath, 2201)]
    public async Task ExportEndpoints_WithYearOutOfRange_ReturnInvalidYear(string path, int year)
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        string url = path == EligibilityPath
            ? $"{path}?awardPeriodId={await FindPeriodIdAsync(client, "P4")}&year={year}"
            : $"{path}?year={year}";

        await ApiClientFactory.AssertErrorAsync(
            await client.GetAsync(url), Messages<EligibilityYearQueryRequest>.Invalid(x => x.Year));
    }

    [Fact(DisplayName = "8.2 · Chưa cài đợt nào trả Mes.Dashboard.NotFound.UpcomingPeriod")]
    public async Task ExportDashboard_WithoutPeriods_ReturnsNotFound()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.ResetAsync(factory);
        await CoreDataset.SeedMembersAsync(factory);

        await ApiClientFactory.AssertErrorAsync(
            await client.GetAsync(DashboardPath),
            Messages<DashboardResponse>.NotFound(nameof(DashboardResponse.UpcomingPeriod)));
    }

    [Theory(DisplayName = "8.1 → 8.3 · Thiếu token trả 401")]
    [InlineData(EligibilityPath)]
    [InlineData(DashboardPath)]
    [InlineData(UnassignedPath)]
    public async Task ExportEndpoints_WithoutToken_ReturnUnauthorized(string path)
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>
    /// Thứ tự dòng trong file phải trùng từng người và từng ngày tròn mốc với <c>expected.json</c>
    /// — nghĩa là trùng thứ tự API trả về (mục 1.8).
    /// </summary>
    /// <param name="expected">Nút kịch bản chứa mảng <c>rows</c>.</param>
    /// <param name="dataRows">Các dòng dữ liệu đọc từ file.</param>
    private static void AssertOrderMatches(JsonElement expected, IReadOnlyList<IReadOnlyList<string>> dataRows)
    {
        JsonElement[] rows = expected.GetProperty("rows").EnumerateArray().ToArray();

        Assert.Equal(
            rows.Select(x => x.GetProperty("fullName").GetString()).ToArray(),
            dataRows.Select(row => row[1]).ToArray());
        Assert.Equal(
            rows.Select(x => x.GetProperty("anniversaryDisplay").GetString()).ToArray(),
            dataRows.Select(row => row[5]).ToArray());
        Assert.Equal(
            rows.Select(x => x.GetProperty("milestone").GetInt32().ToString(CultureInfo.InvariantCulture)).ToArray(),
            dataRows.Select(row => row[6]).ToArray());
    }

    private static string ExpectedFileName(string key)
        => CoreFixtures.Expected.GetProperty("exportFileNames").GetProperty(key).GetString()!;

    private static string Display(DateOnly date) => date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    private static JsonElement EligibleOfYear(string periodCode, int year)
        => CoreFixtures
            .Scenario("core_default_T0")
            .GetProperty("eligibleByPeriod")
            .GetProperty(periodCode)
            .GetProperty("byYear")
            .GetProperty(year.ToString(CultureInfo.InvariantCulture));

    private static JsonElement MissedOfYear(int year)
        => CoreFixtures
            .Scenario("core_default_T0")
            .GetProperty("missedByYear")
            .GetProperty(year.ToString(CultureInfo.InvariantCulture));

    private static async Task<Guid> FindPeriodIdAsync(HttpClient client, string code)
    {
        string name = CoreFixtures.MainPeriods().Single(x => x.Code == code).Name;

        AwardPeriodListPayload list =
            await ApiClientFactory.GetDataAsync<AwardPeriodListPayload>(client, "/api/AwardPeriods");

        return list.Periods.Single(x => x.Name == name).Id;
    }
}
