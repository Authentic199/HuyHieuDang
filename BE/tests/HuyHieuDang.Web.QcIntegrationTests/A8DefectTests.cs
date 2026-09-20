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
    [Fact(Skip = "QC-T27-02 · current lớn gây tràn số, trả 500 OFFSET must not be negative")]
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
    [Fact(Skip = "QC-T27-03 · pageSize không có trần, một lời gọi kéo về cả kho dữ liệu")]
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
    [Fact(Skip = "QC-T27-04 · lỗi ép kiểu tham số trả câu tiếng Anh, không phải khóa Mes.*")]
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
    /// QC-T27-05 · A-113 · Bộ lọc giới tính mang giá trị lạ (<c>filter.Gender=$eq:Khac</c>)
    /// hoặc rỗng bị bỏ qua lặng lẽ và trả **toàn bộ** danh sách. Người dùng tưởng đang lọc
    /// nhưng đang nhìn cả kho; hoặc phải trả 400, hoặc phải trả 0 dòng.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(Skip = "QC-T27-05 · giá trị lọc giới tính lạ bị bỏ qua lặng lẽ, trả toàn bộ danh sách")]
    public async Task QcT2705_Gia_tri_loc_la_khong_duoc_bo_qua_lang_le()
    {
        await QcDb.SeedCoreAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        foreach (string value in new[] { "Khac", string.Empty, "1" })
        {
            using HttpResponseMessage response = await client.GetAsync(
                $"{QcEndpoints.PartyMembers}?filter.Gender=$eq:{value}&pageSize=1");

            if (response.StatusCode is HttpStatusCode.BadRequest)
            {
                continue;
            }

            JsonElement data = (await QcApi.ReadAsync(response)).GetProperty("data");
            data.GetProperty("pageInfo").Int("totalCount")
                .ShouldBe(0, $"filter.Gender=$eq:{value} trả cả danh sách thay vì lọc");
        }
    }

    /// <summary>
    /// QC-T27-06 · A-905 · <c>GET /api/Settings/Milestones?start=1&amp;end=1000000&amp;step=1</c>
    /// trả <c>200</c> với một triệu mốc, gần 7 MB JSON. Ô xem trước gọi endpoint này sau mỗi
    /// lần gõ phím, nên một lần gõ nhầm là treo cả trình duyệt.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(Skip = "QC-T27-06 · xem trước dãy mốc không chặn dãy khổng lồ, trả gần 7 MB JSON")]
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
    [Fact(Skip = "QC-T27-07 · ba khóa OverLength / Required.Ids thiếu trong bảng mục 1.5 hợp đồng API")]
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
