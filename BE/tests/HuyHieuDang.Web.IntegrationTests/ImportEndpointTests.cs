using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Modules.Auth.Responses;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Enums;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniExcelLibs;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Nhóm 3 của hợp đồng API (mục 4.1 → 4.3, UC-24 và UC-25) chạy trên PostgreSQL thật, với
/// "hôm nay" cố định ở T0 = 19/09/2026 và đúng bộ tệp Excel của QC ở <c>tests/fixtures/excel/</c>.
/// </summary>
[Collection(ApiCollection.Name)]
public class ImportEndpointTests
{
    private const string TemplatePath = "/api/PartyMembers/Import/Template";
    private const string PreviewPath = "/api/PartyMembers/Import/Preview";
    private const string CommitPath = "/api/PartyMembers/Import/Commit";

    private readonly HuyHieuDangApiFactory factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportEndpointTests"/> class.
    /// </summary>
    /// <param name="factory">Host kiểm thử dùng chung cho cả lớp.</param>
    public ImportEndpointTests(HuyHieuDangApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact(DisplayName = "4.1 · File mẫu trả file nhị phân đúng tên, 4 cột đúng thứ tự, 2 dòng ví dụ dd/MM/yyyy")]
    public async Task Template_ReturnsFourColumnsWithTwoExampleRows()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response = await client.GetAsync(TemplatePath);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(PartyMemberImportFile.ExcelContentType, response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(PartyMemberImportFile.TemplateFileName, response.Content.Headers.ContentDisposition?.FileName?.Trim('"'));

        using MemoryStream stream = new(await response.Content.ReadAsByteArrayAsync());
        List<IDictionary<string, object?>> rows = stream.Query(useHeaderRow: false)
            .Cast<IDictionary<string, object?>>()
            .ToList();

        Assert.Equal(3, rows.Count);
        Assert.Equal(PartyMemberImportFile.HeaderTitles, rows[0].Values.Select(value => value?.ToString()).ToArray());
        Assert.Equal(
            new object?[] { "Nguyễn Văn Mẫu", "12/03/1974", "Nam", "01/10/1996" },
            rows[1].Values.Select(value => value?.ToString()).ToArray());
        Assert.Equal(
            new object?[] { "Trần Thị Mẫu", "05/07/1973", "Nữ", "07/11/1996" },
            rows[2].Values.Select(value => value?.ToString()).ToArray());
    }

    [Fact(DisplayName = "4.1 · File mẫu hệ thống sinh khớp fixture mau-dang-vien.xlsx của QC")]
    public async Task Template_MatchesQcFixture()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response = await client.GetAsync(TemplatePath);
        using MemoryStream generated = new(await response.Content.ReadAsByteArrayAsync());
        using MemoryStream fixture = new(ImportFixtures.Read("mau-dang-vien.xlsx"));

        Assert.Equal(ReadCells(fixture), ReadCells(generated));
    }

    [Fact(DisplayName = "4.2 · loi-4-dong.xlsx: 10 dòng, 6 hợp lệ, 4 lỗi ở dòng 8, 9, 10, 11")]
    public async Task Preview_FourErrorRows_ReportsRowNumbersAndReasons()
    {
        ImportPreviewPayload preview = await PreviewAsync("loi-4-dong.xlsx");

        Assert.Equal("loi-4-dong.xlsx", preview.FileName);
        Assert.Equal(10, preview.TotalRows);
        Assert.Equal(6, preview.ValidCount);
        Assert.Equal(4, preview.ErrorCount);
        Assert.Equal(new[] { 2, 3, 4, 5, 6, 7 }, preview.ValidRows.Select(row => row.RowNumber));
        Assert.Equal(new[] { 8, 9, 10, 11 }, preview.ErrorRows.Select(row => row.RowNumber));

        AssertErrors(preview, 8, (ImportErrorCodes.MissingFullName, ImportFields.FullName));
        AssertErrors(preview, 9, (ImportErrorCodes.MissingOfficialAdmissionDate, ImportFields.OfficialAdmissionDate));
        AssertErrors(preview, 10, (ImportErrorCodes.InvalidDateFormat, ImportFields.OfficialAdmissionDate));
        AssertErrors(preview, 11, (ImportErrorCodes.FutureOfficialAdmissionDate, ImportFields.OfficialAdmissionDate));

        // Dòng lỗi giữ nguyên chữ thô đọc từ ô Excel.
        ImportErrorRowPayload row10 = preview.ErrorRows.Single(row => row.RowNumber == 10);
        Assert.Equal("Trần Sai Định Dạng", row10.FullName);
        Assert.Equal("1996-10-01", row10.OfficialAdmissionDate);

        // Dòng hợp lệ đã chuẩn hóa: ngày yyyy-MM-dd, giới tính Male/Female/null.
        ImportValidRowPayload row2 = preview.ValidRows.Single(row => row.RowNumber == 2);
        Assert.Equal("Nguyễn Hợp Lệ Một", row2.FullName);
        Assert.Equal(new DateOnly(1974, 3, 12), row2.DateOfBirth);
        Assert.Equal("Male", row2.Gender);
        Assert.Equal(new DateOnly(1996, 10, 1), row2.OfficialAdmissionDate);

        ImportValidRowPayload row4 = preview.ValidRows.Single(row => row.RowNumber == 4);
        Assert.Null(row4.DateOfBirth);
        Assert.Null(row4.Gender);
    }

    [Fact(DisplayName = "4.2 · loi-moi-loai-mot-dong.xlsx: 8 dòng lỗi, mỗi loại một dòng, dòng nhiều lỗi trả đủ lý do")]
    public async Task Preview_OneRowPerErrorKind_ReportsEveryReason()
    {
        ImportPreviewPayload preview = await PreviewAsync("loi-moi-loai-mot-dong.xlsx");

        Assert.Equal(10, preview.TotalRows);
        Assert.Equal(2, preview.ValidCount);
        Assert.Equal(8, preview.ErrorCount);

        AssertErrors(preview, 2, (ImportErrorCodes.MissingFullName, ImportFields.FullName));
        AssertErrors(preview, 3, (ImportErrorCodes.MissingOfficialAdmissionDate, ImportFields.OfficialAdmissionDate));
        AssertErrors(preview, 4, (ImportErrorCodes.InvalidDateFormat, ImportFields.OfficialAdmissionDate));
        AssertErrors(preview, 5, (ImportErrorCodes.FutureOfficialAdmissionDate, ImportFields.OfficialAdmissionDate));
        AssertErrors(preview, 6, (ImportErrorCodes.InvalidGender, ImportFields.Gender));
        AssertErrors(preview, 7, (ImportErrorCodes.BirthDateAfterAdmissionDate, ImportFields.DateOfBirth));
        AssertErrors(preview, 8, (ImportErrorCodes.InvalidDateFormat, ImportFields.DateOfBirth));

        // OQ-2: dòng 9 sai cả bốn chỗ, trả đủ bốn lý do theo thứ tự bảng mã lỗi.
        AssertErrors(
            preview,
            9,
            (ImportErrorCodes.MissingFullName, ImportFields.FullName),
            (ImportErrorCodes.MissingOfficialAdmissionDate, ImportFields.OfficialAdmissionDate),
            (ImportErrorCodes.InvalidDateFormat, ImportFields.DateOfBirth),
            (ImportErrorCodes.InvalidGender, ImportFields.Gender));

        // Dòng 11 là biên "ngày chính thức đúng bằng hôm nay" — hợp lệ.
        Assert.Equal(new[] { 10, 11 }, preview.ValidRows.Select(row => row.RowNumber));
        Assert.Equal(
            HuyHieuDangApiFactory.FixedToday,
            preview.ValidRows.Single(row => row.RowNumber == 11).OfficialAdmissionDate);
    }

    [Theory(DisplayName = "4.2 · Bộ lõi 32 dòng hợp lệ, đọc được cả ngày dạng chuỗi lẫn ngày kiểu ngày")]
    [InlineData("core-hop-le.xlsx")]
    [InlineData("core-hop-le-ngay-kieu-date.xlsx")]
    public async Task Preview_CoreDataset_HasNoErrors(string fileName)
    {
        ImportPreviewPayload preview = await PreviewAsync(fileName);

        Assert.Equal(32, preview.TotalRows);
        Assert.Equal(32, preview.ValidCount);
        Assert.Equal(0, preview.ErrorCount);
        Assert.Equal(new DateOnly(1996, 10, 1), preview.ValidRows[0].OfficialAdmissionDate);
    }

    [Fact(DisplayName = "4.2 · bien-chuan-hoa.xlsx: cắt khoảng trắng (OQ-4), giới tính hoa thường (OQ-5), ngày một chữ số (OQ-6)")]
    public async Task Preview_NormalizationCases_AreAllValid()
    {
        ImportPreviewPayload preview = await PreviewAsync("bien-chuan-hoa.xlsx");

        Assert.Equal(4, preview.TotalRows);
        Assert.Equal(4, preview.ValidCount);
        Assert.Equal(0, preview.ErrorCount);
        Assert.Equal("Nguyễn Có Khoảng Trắng", preview.ValidRows[0].FullName);
        Assert.Equal("Female", preview.ValidRows[1].Gender);
        Assert.Equal("Male", preview.ValidRows[2].Gender);
        Assert.Equal(new DateOnly(1975, 2, 9), preview.ValidRows[3].DateOfBirth);
        Assert.Equal(new DateOnly(1996, 10, 1), preview.ValidRows[3].OfficialAdmissionDate);
    }

    [Theory(DisplayName = "4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa")]
    [InlineData("loi-dinh-dang-csv.csv", PartyMemberImport.ExtensionProperty)]
    [InlineData("loi-khong-phai-xlsx.xlsx", PartyMemberImport.ExtensionProperty)]
    [InlineData("loi-sai-cot.xlsx", PartyMemberImport.ColumnsProperty)]
    [InlineData("loi-rong.xlsx", PartyMemberImport.EmptyProperty)]
    [InlineData("loi-chi-co-tieu-de.xlsx", PartyMemberImport.NoDataRowsProperty)]
    public async Task Preview_FileLevelErrors_AreBlocked(string fileName, string expectedProperty)
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response = await client.PostAsync(PreviewPath, BuildForm(ImportFixtures.Read(fileName), fileName));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            Messages<PartyMemberImport>.Invalid(expectedProperty),
            (await response.ReadApiResponseAsync<object>()).Message);
    }

    [Fact(DisplayName = "4.2 · File quá 10 MB bị chặn trước khi mở, trả Mes.Import.Invalid.FileSize")]
    public async Task Preview_OversizeFile_IsBlocked()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        // tests/fixtures không commit tệp 11 MB (sinh bằng generate.py --with-oversize) nên dựng tại chỗ:
        // phần kiểm tra này chặn theo dung lượng, chưa đọc tới nội dung.
        byte[] oversize = new byte[PartyMemberImportFile.MaxFileSizeInBytes + 1];

        HttpResponseMessage response = await client.PostAsync(PreviewPath, BuildForm(oversize, "qua-10mb.xlsx"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            Messages<PartyMemberImport>.Invalid(PartyMemberImport.FileSizeProperty),
            (await response.ReadApiResponseAsync<object>()).Message);
    }

    [Fact(DisplayName = "4.2 · Xem trước không ghi gì vào cơ sở dữ liệu")]
    public async Task Preview_DoesNotWriteAnything()
    {
        await ResetAsync();

        await PreviewAsync("loi-4-dong.xlsx");

        Assert.Equal(0, await CountAsync());
    }

    [Fact(DisplayName = "4.3 · Nạp loi-4-dong.xlsx: thêm 6 người, bỏ qua 4 dòng lỗi, không kiểm tra trùng")]
    public async Task Commit_AddsValidRowsAndSkipsErrorRows()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();

        ImportCommitPayload first = await CommitAsync(client, "loi-4-dong.xlsx");

        Assert.Equal(6, first.ImportedCount);
        Assert.Equal(4, first.SkippedCount);
        Assert.Equal(6, await CountAsync());

        List<PartyMember> members = await ListAsync();
        PartyMember one = members.Single(member => member.FullName == "Nguyễn Hợp Lệ Một");
        Assert.Equal(new DateOnly(1974, 3, 12), one.DateOfBirth);
        Assert.Equal(Gender.Male, one.Gender);
        Assert.Equal(new DateOnly(1996, 10, 1), one.OfficialAdmissionDate);
        Assert.Null(members.Single(member => member.FullName == "Lê Hợp Lệ Ba").Gender);

        // QT9: nạp lại đúng file đó thêm lần nữa vẫn thêm mới toàn bộ, không chống trùng.
        ImportCommitPayload second = await CommitAsync(client, "loi-4-dong.xlsx");

        Assert.Equal(6, second.ImportedCount);
        Assert.Equal(12, await CountAsync());
    }

    [Fact(DisplayName = "4.3 · Nạp file toàn lỗi: không thêm ai, không ném lỗi")]
    public async Task Commit_AllRowsInvalid_ImportsNothing()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();

        ImportCommitPayload result = await CommitAsync(client, "loi-moi-loai-mot-dong.xlsx");

        Assert.Equal(2, result.ImportedCount);
        Assert.Equal(8, result.SkippedCount);
        Assert.Equal(2, await CountAsync());
    }

    [Theory(DisplayName = "4.3 · Lỗi cấp file cũng chặn ở bước nạp")]
    [InlineData("loi-sai-cot.xlsx", PartyMemberImport.ColumnsProperty)]
    [InlineData("loi-khong-phai-xlsx.xlsx", PartyMemberImport.ExtensionProperty)]
    public async Task Commit_FileLevelErrors_AreBlocked(string fileName, string expectedProperty)
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();

        HttpResponseMessage response = await client.PostAsync(CommitPath, BuildForm(ImportFixtures.Read(fileName), fileName));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            Messages<PartyMemberImport>.Invalid(expectedProperty),
            (await response.ReadApiResponseAsync<object>()).Message);
        Assert.Equal(0, await CountAsync());
    }

    [Fact(DisplayName = "Ba endpoint đều yêu cầu token")]
    public async Task Endpoints_RequireToken()
    {
        HttpClient client = factory.CreateClient();
        byte[] content = ImportFixtures.Read("core-hop-le.xlsx");

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(TemplatePath)).StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.PostAsync(PreviewPath, BuildForm(content, "core-hop-le.xlsx"))).StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.PostAsync(CommitPath, BuildForm(content, "core-hop-le.xlsx"))).StatusCode);
    }

    private static MultipartFormDataContent BuildForm(byte[] content, string fileName)
    {
        ByteArrayContent file = new(content);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        return new MultipartFormDataContent { { file, "file", fileName } };
    }

    private static List<object?[]> ReadCells(Stream stream)
        => stream.Query(useHeaderRow: false)
            .Cast<IDictionary<string, object?>>()
            .Select(row => row.Values.Select(value => value?.ToString()).Cast<object?>().ToArray())
            .ToList();

    private static void AssertErrors(
        ImportPreviewPayload preview, int rowNumber, params (string Code, string Field)[] expected)
    {
        ImportErrorRowPayload row = preview.ErrorRows.Single(item => item.RowNumber == rowNumber);

        Assert.Equal(expected.Select(item => item.Code), row.Errors.Select(error => error.ErrorCode));
        Assert.Equal(expected.Select(item => item.Field), row.Errors.Select(error => error.Field));
    }

    private async Task<ImportPreviewPayload> PreviewAsync(string fileName)
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response = await client.PostAsync(PreviewPath, BuildForm(ImportFixtures.Read(fileName), fileName));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return (await response.ReadApiResponseAsync<ImportPreviewPayload>()).Data!;
    }

    private async Task<ImportCommitPayload> CommitAsync(HttpClient client, string fileName)
    {
        HttpResponseMessage response = await client.PostAsync(CommitPath, BuildForm(ImportFixtures.Read(fileName), fileName));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiResponse<ImportCommitPayload> body = await response.ReadApiResponseAsync<ImportCommitPayload>();
        Assert.Equal(Messages<PartyMember>.Import(), body.Message);

        return body.Data!;
    }

    private async Task ResetAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Set<PartyMember>().ExecuteDeleteAsync();
    }

    private async Task<int> CountAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.Set<PartyMember>().CountAsync();
    }

    private async Task<List<PartyMember>> ListAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.Set<PartyMember>().AsNoTracking().ToListAsync();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage login = await client.PostAsJsonAsync(
            "/api/Auth/Login",
            new { username = HuyHieuDangApiFactory.AdminUsername, password = HuyHieuDangApiFactory.AdminPassword });
        login.EnsureSuccessStatusCode();

        ApiResponse<SessionPayload> body = await login.ReadApiResponseAsync<SessionPayload>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TokenTypes.Bearer, body.Data!.AccessToken);

        return client;
    }
}
