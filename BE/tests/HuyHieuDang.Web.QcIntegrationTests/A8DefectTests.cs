using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HuyHieuDang.Web.QcIntegrationTests.Support;

namespace HuyHieuDang.Web.QcIntegrationTests;

/// <summary>
/// Các lỗi QC tìm được ở vòng T27. Mỗi ca mô tả **hành vi đúng** theo hợp đồng API và kế hoạch
/// kiểm thử, hiện đang đỏ, nên để <c>Skip</c> kèm mã lỗi — đúng cách đã làm ở T26. Bỏ
/// <c>Skip</c> là ca đỏ lại ngay, và đó cũng là cách xác nhận Backend đã sửa xong.
/// </summary>
[Collection(QcApiCollection.Name)]
public sealed class A8DefectTests
{
    private readonly QcApiFactory factory;

    /// <summary>Initializes a new instance of the <see cref="A8DefectTests"/> class.</summary>
    /// <param name="factory">Host kiểm thử dùng chung.</param>
    public A8DefectTests(QcApiFactory factory)
    {
        this.factory = factory;
    }

    /// <summary>
    /// QC-T27-02 · A-104, A-106 · Số trang lớn làm tràn số nguyên khi nhân với cỡ trang: máy
    /// chủ trả <c>500</c> kèm nguyên văn <c>Npgsql.PostgresException: OFFSET must not be
    /// negative</c> và cả dấu vết ngăn xếp. Kế hoạch kiểm thử đòi trang vượt quá trang cuối
    /// trả danh sách rỗng, không phải lỗi 500.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(Skip = "QC-T27-02 · ĐÃ SỬA ở PR #35, xác minh xanh ngày 20/09/2026 — gỡ Skip ngay khi PR vào main")]
    public async Task QcT2702_So_trang_lon_khong_duoc_lam_may_chu_loi_500()
    {
        await QcDb.SeedCoreAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        foreach (int current in new[] { 200_000_000, 1_000_000_000, int.MaxValue })
        {
            using HttpResponseMessage response = await client.GetAsync(
                $"{QcEndpoints.PartyMembers}?current={current}&pageSize=20");

            string body = await response.Content.ReadAsStringAsync();

            ((int)response.StatusCode).ShouldBeLessThan(500, $"current={current} → {body}");
            QcMessages.ShouldNotLeakInternals(body);
        }
    }

    /// <summary>
    /// QC-T27-03 · A-106 · Cỡ trang không có trần: gửi <c>pageSize=1000000000</c> vẫn trả
    /// <c>200</c> kèm **toàn bộ** kho dữ liệu trong một phản hồi. Kế hoạch kiểm thử đòi "bị
    /// chặn hoặc kẹp về mức trần".
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(Skip = "QC-T27-03 · ĐÃ SỬA ở PR #35, xác minh xanh ngày 20/09/2026 — gỡ Skip ngay khi PR vào main")]
    public async Task QcT2703_Co_trang_phai_bi_chan_hoac_kep_ve_muc_tran()
    {
        await QcDb.SeedCoreAndBulkAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        const int Cap = 1000;

        foreach (int pageSize in new[] { 100_000, 1_000_000_000 })
        {
            using HttpResponseMessage response = await client.GetAsync(
                $"{QcEndpoints.PartyMembers}?current=1&pageSize={pageSize}");

            if (response.StatusCode is HttpStatusCode.BadRequest)
            {
                continue;
            }

            JsonElement data = (await QcApi.ReadAsync(response)).GetProperty("data");
            data.Array("pagedData").Count.ShouldBeLessThanOrEqualTo(Cap, $"pageSize={pageSize}");
        }
    }

    /// <summary>
    /// QC-T27-04 · A-905 · Lỗi ép kiểu tham số do tầng gắn dữ liệu sinh ra trả câu tiếng Anh
    /// (<c>"The value '2147483648' is not valid for Year."</c>) thay vì một khóa
    /// <c>Mes.*</c>. Mục 1.5 hợp đồng API chốt Backend chỉ trả khóa, Frontend mới dựng chữ;
    /// câu tiếng Anh này hiện thẳng lên banner của người dùng.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(Skip = "QC-T27-04 · ĐÃ SỬA ở PR #35, xác minh xanh ngày 20/09/2026 — gỡ Skip ngay khi PR vào main")]
    public async Task QcT2704_Loi_ep_kieu_tham_so_phai_tra_khoa_thong_diep()
    {
        await QcDb.SeedCoreAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        foreach (string url in new[]
                 {
                     $"{QcEndpoints.AwardPeriods}?year=2147483648",
                     $"{QcEndpoints.AwardPeriods}?year=99999999999999999999",
                     $"{QcEndpoints.Eligibility}?awardPeriodId=khong-phai-guid",
                     $"{QcEndpoints.Unassigned}?year=chu-khong-phai-so",
                 })
        {
            using HttpResponseMessage response = await client.GetAsync(url);
            string body = await response.Content.ReadAsStringAsync();

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, url);

            string? key = JsonDocument.Parse(body).RootElement.StringOrNull("message");
            key.ShouldNotBeNull(url);
            key!.ShouldStartWith("Mes.", Case.Sensitive, $"{url} → {body}");
        }
    }

    /// <summary>
    /// QC-T27-05 · A-113 · Giá trị lọc không ép được về kiểu của trường bị bỏ qua lặng lẽ
    /// và endpoint trả <b>toàn bộ</b> danh sách. Người dùng tưởng đang lọc nhưng đang nhìn
    /// cả kho.
    /// <para>
    /// Gốc lỗi CEO tìm ra ở <c>QueryExpressionExtension.ApplyFilter</c>: vế <c>Where</c>
    /// được bọc trong <c>try/catch</c> rồi nuốt lỗi, nên biểu thức lọc hỏng bị bỏ qua và
    /// <c>entities</c> giữ nguyên chưa lọc. Vì vậy đây <b>không</b> phải lỗi riêng của
    /// <c>Gender</c>: mọi trường, mọi endpoint có phân trang đều dính. Ca này vì thế quét
    /// nhiều trường trên nhiều endpoint, chứ không chỉ một chỗ đã phát hiện ra nó.
    /// </para>
    /// <para>
    /// Hành vi đúng do CEO chốt ngày 20/09/2026: <b>400</b> kèm khóa
    /// <c>Mes.Common.Invalid.Parameter</c> — không phải trả 0 dòng, vì trả 0 dòng thì
    /// người dùng không phân biệt được "không ai thỏa" với "tôi gõ sai". Thống nhất với
    /// QC-T27-04, và khóa đã có sẵn trong hợp đồng API v1.4.
    /// </para>
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(Skip = "QC-T27-05 · đang sửa ở T47 (HUYH-53) tại tầng lọc dùng chung; hành vi đúng CEO chốt là 400 + Mes.Common.Invalid.Parameter. Gỡ Skip khi T47 gộp")]
    public async Task QcT2705_Gia_tri_loc_la_khong_duoc_bo_qua_lang_le()
    {
        await QcDb.SeedCoreAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        // Trường enum, trường ngày và trường Guid — ba kiểu khác nhau, ba endpoint khác
        // nhau. Nếu bản sửa chỉ vá riêng Gender thì hai dòng sau vẫn đỏ.
        (string Url, string Mo_ta)[] cases =
        [
            ($"{QcEndpoints.PartyMembers}?filter.Gender=$eq:Khac&pageSize=1", "enum Gender sai giá trị"),
            ($"{QcEndpoints.PartyMembers}?filter.Gender=$eq:1&pageSize=1", "enum Gender nhận số"),
            ($"{QcEndpoints.PartyMembers}?filter.OfficialAdmissionDate=$eq:khong-phai-ngay&pageSize=1", "ngày sai định dạng"),
            ($"{QcEndpoints.PartyMembers}?filter.Id=$eq:khong-phai-guid&pageSize=1", "Guid sai định dạng"),
            ($"{QcEndpoints.AwardPeriods}?filter.FromMonth=$eq:khong-phai-so&pageSize=1", "số nguyên sai kiểu"),
        ];

        int total = QcFixtures.CoreMembers.Count;

        foreach ((string url, string moTa) in cases)
        {
            using HttpResponseMessage response = await client.GetAsync(url);
            string body = await response.Content.ReadAsStringAsync();

            response.StatusCode.ShouldBe(
                HttpStatusCode.BadRequest,
                $"{moTa} — {url} phải bị từ chối, không được lặng lẽ bỏ qua vế lọc. Thân: {body}");

            JsonDocument.Parse(body).RootElement.Str("message")
                .ShouldBe(QcMessages.CommonInvalidParameter, $"{moTa} — {url}");
        }

        // Chốt lại điều quan trọng nhất: không lời gọi nào ở trên được trả về cả kho.
        using HttpResponseMessage ok = await client.GetAsync(
            $"{QcEndpoints.PartyMembers}?filter.Gender=$eq:Male&pageSize=1");
        (await QcApi.ReadAsync(ok)).GetProperty("data").GetProperty("pageInfo").Int("totalCount")
            .ShouldBeLessThan(total, "Lọc hợp lệ phải thật sự thu hẹp danh sách");
    }

    /// <summary>
    /// QC-T27-06 · A-905 · <c>GET /api/Settings/Milestones?start=1&amp;end=1000000&amp;step=1</c>
    /// trả <c>200</c> với một triệu mốc, gần 7 MB JSON. Ô xem trước gọi endpoint này sau mỗi
    /// lần gõ phím, nên một lần gõ nhầm là treo cả trình duyệt.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(Skip = "QC-T27-06 · ĐÃ SỬA ở PR #35, xác minh xanh ngày 20/09/2026 — gỡ Skip ngay khi PR vào main")]
    public async Task QcT2706_Xem_truoc_day_moc_phai_co_tran()
    {
        await QcDb.ResetAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        using HttpResponseMessage response = await client.GetAsync(
            $"{QcEndpoints.SettingsMilestones}?start=1&end=1000000&step=1");

        if (response.StatusCode is HttpStatusCode.BadRequest)
        {
            return;
        }

        JsonElement data = (await QcApi.ReadAsync(response)).GetProperty("data");
        data.Int("milestoneCount").ShouldBeLessThanOrEqualTo(1000);
    }

    /// <summary>
    /// QC-T27-07 · A-905 · Ba khóa thông điệp Backend thật sự trả ra nhưng bảng mục 1.5 hợp
    /// đồng API không có, nên Frontend chỉ hiện câu mặc định "Thao tác không thực hiện được"
    /// thay vì nói rõ chỗ sai.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(Skip = "QC-T27-07 · đã sửa ở PR #46 (contract v1.4) — gỡ Skip khi #46 gộp")]
    public async Task QcT2707_Moi_khoa_Backend_tra_ra_deu_phai_co_trong_hop_dong()
    {
        await QcDb.SeedCoreAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        List<HttpResponseMessage> responses = new()
        {
            await client.PostAsJsonAsync(
                QcEndpoints.PartyMembers,
                new { fullName = new string('x', 300), officialAdmissionDate = "1996-01-01" }),
            await client.PostAsJsonAsync(
                QcEndpoints.AwardPeriods,
                new { name = new string('y', 300), fromDay = 1, fromMonth = 1, toDay = 2, toMonth = 1 }),
            await client.PostAsJsonAsync(QcEndpoints.PartyMembersDeleteMany, new { ids = System.Array.Empty<string>() }),
        };

        try
        {
            foreach (HttpResponseMessage response in responses)
            {
                string body = await response.Content.ReadAsStringAsync();
                string key = JsonDocument.Parse(body).RootElement.Str("message");

                QcMessages.ContractKeys.ShouldContain(key, body);
            }
        }
        finally
        {
            foreach (HttpResponseMessage response in responses)
            {
                response.Dispose();
            }
        }
    }
}
