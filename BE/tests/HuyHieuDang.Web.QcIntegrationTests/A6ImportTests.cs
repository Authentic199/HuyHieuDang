using System.Net;
using System.Text.Json;
using HuyHieuDang.Web.QcIntegrationTests.Support;

namespace HuyHieuDang.Web.QcIntegrationTests;

/// <summary>
/// A-601 → A-620 · Import Excel qua API (UC-24, UC-25, QT9). Xem trước và nạp là hai lời gọi
/// riêng; xem trước không ghi gì, nạp là một giao dịch và không kiểm tra trùng.
/// </summary>
[Collection(QcApiCollection.Name)]
public sealed class A6ImportTests
{
    private readonly QcApiFactory factory;

    /// <summary>Initializes a new instance of the <see cref="A6ImportTests"/> class.</summary>
    /// <param name="factory">Host kiểm thử dùng chung.</param>
    public A6ImportTests(QcApiFactory factory)
    {
        this.factory = factory;
    }

    /// <summary>A-601 · Xem trước file lõi: 32 dòng hợp lệ, 0 lỗi, và **chưa** ghi vào cơ sở dữ liệu.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A601_Xem_truoc_file_loi_32_dong_hop_le_va_chua_ghi_gi()
    {
        using HttpClient client = await ResetAndLoginAsync();

        JsonElement preview = await PreviewAsync(client, "core-hop-le.xlsx");

        preview.Int("totalRows").ShouldBe(32);
        preview.Int("validCount").ShouldBe(32);
        preview.Int("errorCount").ShouldBe(0);
        preview.Str("fileName").ShouldBe("core-hop-le.xlsx");
        preview.Array("errorRows").ShouldBeEmpty();

        // Dòng dữ liệu đầu tiên mang số 2 vì dòng tiêu đề là dòng 1.
        preview.Array("validRows")[0].Int("rowNumber").ShouldBe(2);

        (await QcDb.CountMembersAsync(factory)).ShouldBe(0);
    }

    /// <summary>A-602 · Nạp sau xem trước làm tổng tăng đúng 32.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A602_Nap_sau_xem_truoc_tang_dung_32()
    {
        using HttpClient client = await ResetAndLoginAsync();

        await PreviewAsync(client, "core-hop-le.xlsx");
        JsonElement commit = await CommitAsync(client, "core-hop-le.xlsx");

        commit.Int("importedCount").ShouldBe(32);
        commit.Int("skippedCount").ShouldBe(0);
        (await QcDb.CountMembersAsync(factory)).ShouldBe(32);
    }

    /// <summary>
    /// A-603 · File có 4 dòng lỗi: xem trước báo 6 hợp lệ · 4 lỗi, đúng số dòng Excel 8/9/10/11
    /// và đúng lý do từng dòng.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A603_Xem_truoc_file_bon_dong_loi_dung_so_dong_va_dung_ly_do()
    {
        using HttpClient client = await ResetAndLoginAsync();

        JsonElement preview = await PreviewAsync(client, "loi-4-dong.xlsx");

        preview.Int("totalRows").ShouldBe(10);
        preview.Int("validCount").ShouldBe(6);
        preview.Int("errorCount").ShouldBe(4);

        IReadOnlyList<JsonElement> errors = preview.Array("errorRows");
        errors.Select(row => row.Int("rowNumber")).ToList().ShouldBe(new[] { 8, 9, 10, 11 });

        Dictionary<int, string> expectedCodes = new()
        {
            [8] = "MissingFullName",
            [9] = "MissingOfficialAdmissionDate",
            [10] = "InvalidDateFormat",
            [11] = "FutureOfficialAdmissionDate",
        };

        foreach (JsonElement row in errors)
        {
            int rowNumber = row.Int("rowNumber");
            IReadOnlyList<JsonElement> reasons = row.Array("errors");

            reasons.ShouldNotBeEmpty($"dòng {rowNumber}");
            reasons.Select(reason => reason.Str("errorCode")).ToList()
                .ShouldContain(expectedCodes[rowNumber], $"dòng {rowNumber}");
            reasons[0].Str("field").ShouldNotBeNullOrWhiteSpace();
        }

        // Dòng lỗi giữ nguyên chữ thô đọc từ ô Excel (mục 4.2 hợp đồng API).
        errors.Single(row => row.Int("rowNumber") == 10).Str("officialAdmissionDate").ShouldBe("1996-10-01");
    }

    /// <summary>A-604 · Nạp file có 4 dòng lỗi chỉ thêm đúng 6 người.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A604_Nap_file_bon_dong_loi_chi_them_sau_nguoi()
    {
        using HttpClient client = await ResetAndLoginAsync();

        JsonElement commit = await CommitAsync(client, "loi-4-dong.xlsx");

        commit.Int("importedCount").ShouldBe(6);
        commit.Int("skippedCount").ShouldBe(4);
        (await QcDb.CountMembersAsync(factory)).ShouldBe(6);
    }

    /// <summary>A-605 · Chỉ xem trước rồi bỏ ngang thì tổng không đổi.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A605_Chi_xem_truoc_roi_bo_thi_tong_khong_doi()
    {
        await QcDb.SeedCoreAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        int before = await QcDb.CountMembersAsync(factory);

        await PreviewAsync(client, "core-hop-le.xlsx");
        await PreviewAsync(client, "bulk-1200.xlsx");

        (await QcDb.CountMembersAsync(factory)).ShouldBe(before);
    }

    /// <summary>
    /// A-606 · Nạp cùng một file hai lần làm tổng tăng gấp đôi — QT9 nói rõ hệ thống **không**
    /// kiểm tra trùng, nên đây là hành vi đúng chứ không phải lỗi.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A606_Nap_hai_lan_cung_mot_file_tang_gap_doi()
    {
        using HttpClient client = await ResetAndLoginAsync();

        await CommitAsync(client, "core-hop-le.xlsx");
        await CommitAsync(client, "core-hop-le.xlsx");

        (await QcDb.CountMembersAsync(factory)).ShouldBe(64);
    }

    /// <summary>
    /// A-607 → A-611 · Năm lỗi cấp file bị chặn ngay bằng 400 kèm đúng khóa thông điệp, không
    /// sang được bước xem trước và không ghi gì.
    /// </summary>
    /// <param name="fileName">File mẫu.</param>
    /// <param name="expectedKey">Khóa thông điệp mong đợi.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Theory]
    [InlineData("loi-sai-cot.xlsx", QcMessages.ImportInvalidColumns)]
    [InlineData("loi-rong.xlsx", QcMessages.ImportInvalidEmpty)]
    [InlineData("loi-chi-co-tieu-de.xlsx", QcMessages.ImportInvalidNoDataRows)]

    // Tệp mang đuôi .xlsx nhưng ruột là chữ thường: máy chủ không mở được gói nên quy về lỗi
    // "Chỉ nhận file .xlsx" — vẫn là 400 với khóa có trong bảng mục 1.5, đúng yêu cầu A-610.
    [InlineData("loi-khong-phai-xlsx.xlsx", QcMessages.ImportInvalidExtension)]
    [InlineData("loi-dinh-dang-csv.csv", QcMessages.ImportInvalidExtension)]
    public async Task A607_A611_Loi_cap_file_bi_chan_ngay(string fileName, string expectedKey)
    {
        using HttpClient client = await ResetAndLoginAsync();

        foreach (string url in new[] { QcEndpoints.ImportPreview, QcEndpoints.ImportCommit })
        {
            using HttpResponseMessage response =
                await QcUpload.SendFileAsync(client, url, QcPaths.Excel(fileName));

            await QcApi.AssertErrorAsync(response, HttpStatusCode.BadRequest, expectedKey);
        }

        (await QcDb.CountMembersAsync(factory)).ShouldBe(0);
    }

    /// <summary>A-609 · Thông báo của "chỉ có tiêu đề" phải khác thông báo của "file rỗng" (OQ-7).</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A609_File_chi_co_tieu_de_bao_khac_file_rong()
    {
        using HttpClient client = await ResetAndLoginAsync();

        using HttpResponseMessage empty = await QcUpload.SendFileAsync(
            client, QcEndpoints.ImportPreview, QcPaths.Excel("loi-rong.xlsx"));
        using HttpResponseMessage headerOnly = await QcUpload.SendFileAsync(
            client, QcEndpoints.ImportPreview, QcPaths.Excel("loi-chi-co-tieu-de.xlsx"));

        JsonElement emptyBody = await QcApi.AssertErrorAsync(empty, HttpStatusCode.BadRequest);
        JsonElement headerOnlyBody = await QcApi.AssertErrorAsync(headerOnly, HttpStatusCode.BadRequest);

        headerOnlyBody.Str("message").ShouldNotBe(emptyBody.Str("message"));
    }

    /// <summary>
    /// A-612 · File 11 MB bị chặn bằng khóa dung lượng. File này cố ý không phải Excel hợp lệ:
    /// trả đúng khóa dung lượng chứng tỏ máy chủ chặn **trước khi** đọc nội dung.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A612_File_qua_10MB_bi_chan_truoc_khi_doc_noi_dung()
    {
        using HttpClient client = await ResetAndLoginAsync();

        byte[] oversized = QcUpload.OversizedBytes();
        oversized.Length.ShouldBeGreaterThan((int)QcUpload.MaxFileSizeInBytes);

        using HttpResponseMessage response = await QcUpload.SendBytesAsync(
            client, QcEndpoints.ImportPreview, oversized, "loi-qua-10mb.xlsx");

        await QcApi.AssertErrorAsync(response, HttpStatusCode.BadRequest, QcMessages.ImportInvalidFileSize);
        (await QcDb.CountMembersAsync(factory)).ShouldBe(0);
    }

    /// <summary>A-613 · File hợp lệ nặng 9,9 MB — biên dưới của ràng buộc — vẫn được chấp nhận.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A613_File_hop_le_9_9MB_van_duoc_chap_nhan()
    {
        using HttpClient client = await ResetAndLoginAsync();

        string path = QcUpload.BuildLargeValidWorkbook((long)(9.9 * 1024 * 1024));

        try
        {
            new FileInfo(path).Length.ShouldBeLessThan(QcUpload.MaxFileSizeInBytes);

            using HttpResponseMessage response =
                await QcUpload.SendFileAsync(client, QcEndpoints.ImportPreview, path);

            response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
            (await QcApi.ReadAsync(response)).GetProperty("data").Int("validCount").ShouldBe(32);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>A-614 · Không gửi file nào là lỗi 400 nói được, không phải 500.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A614_Khong_gui_file_tra_400()
    {
        using HttpClient client = await ResetAndLoginAsync();

        foreach (string url in new[] { QcEndpoints.ImportPreview, QcEndpoints.ImportCommit })
        {
            using MultipartFormDataContent empty = new();
            using HttpResponseMessage response = await client.PostAsync(url, empty);

            await QcApi.AssertErrorAsync(response, HttpStatusCode.BadRequest);
        }
    }

    /// <summary>A-615 · Gửi hai file cùng lúc được xử lý tất định, không phải lỗi 500.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A615_Gui_hai_file_duoc_xu_ly_tat_dinh()
    {
        using HttpClient client = await ResetAndLoginAsync();

        using HttpResponseMessage response = await QcUpload.SendTwoFilesAsync(
            client, QcEndpoints.ImportPreview, QcPaths.Excel("core-hop-le.xlsx"), QcPaths.Excel("loi-4-dong.xlsx"));

        response.StatusCode.ShouldBeOneOf(new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest });

        if (response.StatusCode is HttpStatusCode.OK)
        {
            // Nhận thì phải nhận đúng một file, không trộn dữ liệu của hai file.
            JsonElement data = (await QcApi.ReadAsync(response)).GetProperty("data");
            data.Int("totalRows").ShouldBeOneOf(new[] { 32, 10 });
        }
        else
        {
            QcMessages.ShouldNotLeakInternals(await response.Content.ReadAsStringAsync());
        }

        (await QcDb.CountMembersAsync(factory)).ShouldBe(0);
    }

    /// <summary>
    /// A-616 · Cắt kết nối giữa lúc nạp không để lại dữ liệu nửa vời: sau khi hủy, số người
    /// phải là 0 hoặc trọn vẹn 1200, không bao giờ ở giữa (QT9 — nạp là một giao dịch).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A616_Cat_ket_noi_giua_luc_nap_khong_de_lai_du_lieu_nua_voi()
    {
        using HttpClient client = await ResetAndLoginAsync();

        using CancellationTokenSource cancellation = new(TimeSpan.FromMilliseconds(120));

        try
        {
            using HttpResponseMessage response = await QcUpload.SendFileAsync(
                client, QcEndpoints.ImportCommit, QcPaths.Excel("bulk-1200.xlsx"), cancellation.Token);

            response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }
        catch (OperationCanceledException)
        {
            // Đúng kịch bản cần dựng: người dùng đóng trình duyệt giữa chừng.
        }

        // Cho máy chủ kịp kết thúc giao dịch đang dở.
        await Task.Delay(TimeSpan.FromSeconds(2));

        (await QcDb.CountMembersAsync(factory)).ShouldBeOneOf(
            new[] { 0, 1200 }, "nạp bị cắt giữa chừng để lại dữ liệu nửa vời");
    }

    /// <summary>
    /// A-617 · Ép một lỗi kỹ thuật ở dòng cuối bằng một ràng buộc cơ sở dữ liệu tạm thời:
    /// toàn bộ lời gọi phải cuộn lại, tổng không đổi.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A617_Loi_ky_thuat_giua_chung_cuon_lai_toan_bo()
    {
        await QcDb.SeedCoreAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        int before = await QcDb.CountMembersAsync(factory);

        string table = QcDb.TableName<Infrastructure.Modules.PartyMembers.Entities.PartyMember>(factory);
        string column = QcDb.ColumnName<Infrastructure.Modules.PartyMembers.Entities.PartyMember>(
            factory, nameof(Infrastructure.Modules.PartyMembers.Entities.PartyMember.FullName));

        // "Vũ Hợp Lệ Sáu" là dòng hợp lệ cuối cùng của loi-4-dong.xlsx.
        await QcDb.ExecuteSqlAsync(
            factory,
            $"ALTER TABLE {table} ADD CONSTRAINT qc_a617 CHECK ({column} <> 'Vũ Hợp Lệ Sáu')");

        try
        {
            using HttpResponseMessage response = await QcUpload.SendFileAsync(
                client, QcEndpoints.ImportCommit, QcPaths.Excel("loi-4-dong.xlsx"));

            response.StatusCode.ShouldBeOneOf(
                new[] { HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError },
                await response.Content.ReadAsStringAsync());

            (await QcDb.CountMembersAsync(factory))
                .ShouldBe(before, "lỗi kỹ thuật giữa chừng không cuộn lại hết — QT9 đòi nạp là một giao dịch");
        }
        finally
        {
            await QcDb.ExecuteSqlAsync(factory, $"ALTER TABLE {table} DROP CONSTRAINT qc_a617");
        }
    }

    /// <summary>A-618 · Nạp file 1200 dòng chạy trọn vẹn trong thời gian hợp lý.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A618_Nap_file_1200_dong()
    {
        using HttpClient client = await ResetAndLoginAsync();

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        JsonElement commit = await CommitAsync(client, "bulk-1200.xlsx");
        stopwatch.Stop();

        commit.Int("importedCount").ShouldBe(1200);
        commit.Int("skippedCount").ShouldBe(0);
        (await QcDb.CountMembersAsync(factory)).ShouldBe(1200);
        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(30), "nạp 1200 dòng quá chậm");
    }

    /// <summary>
    /// A-619 · File mẫu (UC-25): đúng 4 cột đúng thứ tự, có dòng ví dụ, ngày <c>dd/MM/yyyy</c>,
    /// và **nạp lại chính file mẫu đó phải hợp lệ**.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A619_File_mau_dung_cot_va_nap_lai_duoc()
    {
        using HttpClient client = await ResetAndLoginAsync();

        using HttpResponseMessage template = await client.GetAsync(QcEndpoints.ImportTemplate);
        template.StatusCode.ShouldBe(HttpStatusCode.OK);
        template.Content.Headers.ContentType!.MediaType.ShouldBe(QcApi.ExcelContentType);
        template.Content.Headers.ContentDisposition!.FileName!.Trim('"').ShouldBe("MauDanhSachDangVien.xlsx");

        byte[] bytes = await template.Content.ReadAsByteArrayAsync();
        QcWorkbook workbook = QcWorkbook.Read(bytes);

        workbook.Row(1).ShouldBe(new[] { "Họ tên", "Ngày sinh", "Giới tính", "Ngày vào Đảng chính thức" });
        workbook.RowCount.ShouldBeGreaterThanOrEqualTo(2, "file mẫu phải có dòng ví dụ");

        foreach (string cell in new[] { workbook.Cell(2, 2), workbook.Cell(2, 4) })
        {
            cell.ShouldMatch(@"^\d{2}/\d{2}/\d{4}$");
        }

        // Nạp lại chính file mẫu vừa tải về.
        using HttpResponseMessage commit = await QcUpload.SendBytesAsync(
            client, QcEndpoints.ImportCommit, bytes, "MauDanhSachDangVien.xlsx");

        commit.StatusCode.ShouldBe(HttpStatusCode.OK, await commit.Content.ReadAsStringAsync());
        (await QcApi.ReadAsync(commit)).GetProperty("data").Int("importedCount")
            .ShouldBe(workbook.RowCount - 1);
    }

    /// <summary>A-620 · Ô ngày kiểu ngày của Excel (số serial) cũng đọc được, đủ 32 dòng hợp lệ.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A620_O_ngay_kieu_ngay_cua_Excel_van_doc_duoc()
    {
        using HttpClient client = await ResetAndLoginAsync();

        JsonElement preview = await PreviewAsync(client, "core-hop-le-ngay-kieu-date.xlsx");

        preview.Int("validCount").ShouldBe(32);
        preview.Int("errorCount").ShouldBe(0);
    }

    /// <summary>
    /// A-621 · Mỗi loại lỗi cấp dòng một dòng: bảng lỗi phải nêu đủ mọi lý do của một dòng,
    /// không dừng ở lý do đầu tiên (OQ-2).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A621_Mot_dong_nhieu_loi_tra_du_moi_ly_do()
    {
        using HttpClient client = await ResetAndLoginAsync();

        JsonElement preview = await PreviewAsync(client, "loi-moi-loai-mot-dong.xlsx");

        preview.Int("validCount").ShouldBe(2);
        preview.Int("errorCount").ShouldBe(8);

        IReadOnlyList<JsonElement> errors = preview.Array("errorRows");

        // Sáu mã lỗi cấp dòng của hợp đồng đều phải xuất hiện.
        List<string> codes = errors
            .SelectMany(row => row.Array("errors").Select(reason => reason.Str("errorCode")))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        foreach (string code in new[]
                 {
                     "MissingFullName",
                     "MissingOfficialAdmissionDate",
                     "InvalidDateFormat",
                     "FutureOfficialAdmissionDate",
                     "InvalidGender",
                     "BirthDateAfterAdmissionDate",
                 })
        {
            codes.ShouldContain(code);
        }

        // Dòng cuối cùng của file gom nhiều lỗi cùng lúc.
        errors.ShouldContain(row => row.Array("errors").Count > 1, "không dòng nào trả quá một lý do (OQ-2)");
    }

    private async Task<HttpClient> ResetAndLoginAsync()
    {
        await QcDb.ResetAsync(factory);

        return await QcApi.LoginAsync(factory);
    }

    private static async Task<JsonElement> PreviewAsync(HttpClient client, string fileName)
    {
        using HttpResponseMessage response = await QcUpload.SendFileAsync(
            client, QcEndpoints.ImportPreview, QcPaths.Excel(fileName));

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());

        return (await QcApi.ReadAsync(response)).GetProperty("data");
    }

    private static async Task<JsonElement> CommitAsync(HttpClient client, string fileName)
    {
        using HttpResponseMessage response = await QcUpload.SendFileAsync(
            client, QcEndpoints.ImportCommit, QcPaths.Excel(fileName));

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());

        return (await QcApi.ReadAsync(response)).GetProperty("data");
    }
}
