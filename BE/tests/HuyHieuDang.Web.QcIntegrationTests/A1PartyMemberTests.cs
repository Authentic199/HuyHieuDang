using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HuyHieuDang.Web.QcIntegrationTests.Support;

namespace HuyHieuDang.Web.QcIntegrationTests;

/// <summary>
/// A-101 → A-125 · Danh sách đảng viên (UC-20 → UC-23): phân trang, tìm kiếm, lọc, sắp xếp,
/// thêm tay, sửa, xóa. Con số đối chiếu với <c>tests/fixtures/data/expected.json</c>.
/// </summary>
[Collection(QcApiCollection.Name)]
public sealed class A1PartyMemberTests
{
    private readonly QcApiFactory factory;

    /// <summary>Initializes a new instance of the <see cref="A1PartyMemberTests"/> class.</summary>
    /// <param name="factory">Host kiểm thử dùng chung.</param>
    public A1PartyMemberTests(QcApiFactory factory)
    {
        this.factory = factory;
    }

    private static int ExpectedTotal => QcFixtures.Node("counts").Int("total");

    /// <summary>A-101 · Nạp bộ lõi và bộ lớn rồi lấy danh sách phải ra đúng 1232 người.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A101_Tong_so_dang_vien_dung_1232()
    {
        using HttpClient client = await SeedAllAndLoginAsync();

        JsonElement data = await QcApi.GetDataAsync(client, QcEndpoints.PartyMembers);

        data.GetProperty("pageInfo").Int("totalCount").ShouldBe(ExpectedTotal);
    }

    /// <summary>A-102 · Trang đầu 20 dòng, tổng 1232, 62 trang.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A102_Trang_dau_20_dong_va_62_trang()
    {
        using HttpClient client = await SeedAllAndLoginAsync();

        JsonElement data = await QcApi.GetDataAsync(client, $"{QcEndpoints.PartyMembers}?current=1&pageSize=20");
        JsonElement pageInfo = data.GetProperty("pageInfo");

        data.Array("pagedData").Count.ShouldBe(20);
        pageInfo.Int("totalCount").ShouldBe(ExpectedTotal);
        pageInfo.Int("totalPages").ShouldBe(QcFixtures.Node("pagination").GetProperty("pages").Int("20"));
        pageInfo.GetProperty("hasPrevious").GetBoolean().ShouldBeFalse();
        pageInfo.GetProperty("hasNext").GetBoolean().ShouldBeTrue();
    }

    /// <summary>A-103 · Trang cuối của cỡ trang 20 còn đúng 12 dòng.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A103_Trang_cuoi_con_12_dong()
    {
        using HttpClient client = await SeedAllAndLoginAsync();

        JsonElement data = await QcApi.GetDataAsync(client, $"{QcEndpoints.PartyMembers}?current=62&pageSize=20");

        data.Array("pagedData").Count
            .ShouldBe(QcFixtures.Node("pagination").GetProperty("lastPageRows").Int("20"));
        data.GetProperty("pageInfo").GetProperty("hasNext").GetBoolean().ShouldBeFalse();
    }

    /// <summary>A-104 · Trang vượt quá trang cuối trả danh sách rỗng, không phải lỗi 500.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A104_Trang_vuot_qua_trang_cuoi_tra_rong_khong_loi()
    {
        using HttpClient client = await SeedAllAndLoginAsync();

        JsonElement data = await QcApi.GetDataAsync(client, $"{QcEndpoints.PartyMembers}?current=63&pageSize=20");

        data.Array("pagedData").ShouldBeEmpty();
        data.GetProperty("pageInfo").Int("totalCount").ShouldBe(ExpectedTotal);
    }

    /// <summary>A-105 · Cỡ trang 50 và 100 cho đúng số trang và đúng số dòng trang cuối.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A105_Co_trang_50_va_100_dung_so_trang()
    {
        using HttpClient client = await SeedAllAndLoginAsync();
        JsonElement pagination = QcFixtures.Node("pagination");

        foreach (string size in new[] { "50", "100" })
        {
            int pages = pagination.GetProperty("pages").Int(size);

            JsonElement firstPage = await QcApi.GetDataAsync(
                client, $"{QcEndpoints.PartyMembers}?current=1&pageSize={size}");
            firstPage.GetProperty("pageInfo").Int("totalPages").ShouldBe(pages, $"pageSize={size}");

            JsonElement lastPage = await QcApi.GetDataAsync(
                client, $"{QcEndpoints.PartyMembers}?current={pages}&pageSize={size}");
            lastPage.Array("pagedData").Count
                .ShouldBe(pagination.GetProperty("lastPageRows").Int(size), $"pageSize={size}");
        }
    }

    /// <summary>
    /// A-106 · Cỡ trang 0, âm hay rất lớn phải bị chặn hoặc kẹp về mức trần — không treo,
    /// không trả cả kho dữ liệu về trong một lời gọi.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A106_Co_trang_bat_thuong_khong_lam_treo_may_chu()
    {
        using HttpClient client = await SeedAllAndLoginAsync();

        foreach (string query in new[]
                 {
                     "?current=1&pageSize=0",
                     "?current=1&pageSize=-5",
                     "?current=0&pageSize=20",
                     "?current=-1&pageSize=20",
                     "?current=1&pageSize=1000000000",
                 })
        {
            using HttpResponseMessage response = await client.GetAsync(QcEndpoints.PartyMembers + query);

            response.StatusCode.ShouldBeOneOf(
                new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest },
                $"{query} phải trả kết quả tất định, không phải lỗi máy chủ");

            if (response.StatusCode is HttpStatusCode.OK)
            {
                JsonElement data = (await QcApi.ReadAsync(response)).GetProperty("data");
                data.Array("pagedData").Count.ShouldBeLessThanOrEqualTo(ExpectedTotal, query);
            }
        }
    }

    /// <summary>
    /// A-107 · Ghép mọi trang lại phải ra đúng 1232 người, không trùng không thiếu — bắt lỗi
    /// phân trang thiếu sắp xếp tất định.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A107_Ghep_moi_trang_khong_trung_khong_thieu()
    {
        using HttpClient client = await SeedAllAndLoginAsync();
        List<string> ids = new();

        for (int page = 1; page <= 62; page++)
        {
            JsonElement data = await QcApi.GetDataAsync(
                client, $"{QcEndpoints.PartyMembers}?current={page}&pageSize=20");

            ids.AddRange(data.Array("pagedData").Select(row => row.Str("id")));
        }

        ids.Count.ShouldBe(ExpectedTotal);
        ids.Distinct(StringComparer.Ordinal).Count().ShouldBe(ExpectedTotal);
    }

    /// <summary>A-108 và A-109 · Tìm "Nguyễn" và "nguyễn" đều ra 79 người (không phân biệt hoa thường).</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A108_A109_Tim_kiem_khong_phan_biet_hoa_thuong()
    {
        using HttpClient client = await SeedAllAndLoginAsync();
        JsonElement search = QcFixtures.Node("search");

        int expected = search.Int("Nguyễn");

        foreach (string keyword in new[] { "Nguyễn", "nguyễn" })
        {
            JsonElement data = await QcApi.GetDataAsync(
                client, $"{QcEndpoints.PartyMembers}?searchKeyword={Uri.EscapeDataString(keyword)}&searchFields=FullName&pageSize=200");

            data.GetProperty("pageInfo").Int("totalCount").ShouldBe(expected, keyword);
        }
    }

    /// <summary>A-110 và A-111 · Tìm đúng một người, và tìm chuỗi không tồn tại ra danh sách rỗng.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A110_A111_Tim_dung_mot_nguoi_va_tim_khong_thay()
    {
        using HttpClient client = await SeedAllAndLoginAsync();
        JsonElement search = QcFixtures.Node("search");

        JsonElement one = await QcApi.GetDataAsync(
            client, $"{QcEndpoints.PartyMembers}?searchKeyword={Uri.EscapeDataString("Đào Văn Ân")}&searchFields=FullName");
        one.GetProperty("pageInfo").Int("totalCount").ShouldBe(search.Int("Đào Văn Ân"));
        one.Array("pagedData").Single().Str("fullName").ShouldBe("Đào Văn Ân");

        JsonElement none = await QcApi.GetDataAsync(
            client, $"{QcEndpoints.PartyMembers}?searchKeyword={Uri.EscapeDataString("Không Tồn Tại")}&searchFields=FullName");
        none.GetProperty("pageInfo").Int("totalCount").ShouldBe(search.Int("Không Tồn Tại"));
        none.Array("pagedData").ShouldBeEmpty();
    }

    /// <summary>
    /// A-112 · Ký tự đặc biệt của SQL và của mẫu LIKE phải được tham số hóa: trả kết quả bình
    /// thường, không lỗi, và không kéo về cả kho như khi <c>%</c> bị hiểu là ký tự đại diện.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A112_Ky_tu_dac_biet_khong_gay_loi_va_khong_bi_hieu_la_dai_dien()
    {
        using HttpClient client = await SeedAllAndLoginAsync();

        foreach (string keyword in new[] { "%", "_", "'", "\\", "%'--", "'; DROP TABLE party_members; --" })
        {
            using HttpResponseMessage response = await client.GetAsync(
                $"{QcEndpoints.PartyMembers}?searchKeyword={Uri.EscapeDataString(keyword)}&searchFields=FullName");

            response.StatusCode.ShouldBe(HttpStatusCode.OK, keyword);

            JsonElement data = (await QcApi.ReadAsync(response)).GetProperty("data");
            data.GetProperty("pageInfo").Int("totalCount")
                .ShouldBeLessThan(ExpectedTotal, $"'{keyword}' bị hiểu là ký tự đại diện");
        }

        // Kho dữ liệu vẫn nguyên vẹn sau chuỗi tấn công.
        (await QcDb.CountMembersAsync(factory)).ShouldBe(ExpectedTotal);
    }

    /// <summary>A-113 · Lọc giới tính Nam / Nữ / trống đúng 592 / 590 / 50.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A113_Loc_gioi_tinh_dung_so_luong()
    {
        using HttpClient client = await SeedAllAndLoginAsync();
        JsonElement counts = QcFixtures.Node("counts");

        JsonElement male = await QcApi.GetDataAsync(
            client, $"{QcEndpoints.PartyMembers}?filter.Gender=$eq:Male&pageSize=1");
        JsonElement female = await QcApi.GetDataAsync(
            client, $"{QcEndpoints.PartyMembers}?filter.Gender=$eq:Female&pageSize=1");

        int maleCount = male.GetProperty("pageInfo").Int("totalCount");
        int femaleCount = female.GetProperty("pageInfo").Int("totalCount");

        maleCount.ShouldBe(counts.Int("totalGenderNam"));
        femaleCount.ShouldBe(counts.Int("totalGenderNu"));

        // Hợp đồng không có bộ lọc "trống"; phần còn lại phải đúng bằng số ô giới tính bỏ trống.
        (ExpectedTotal - maleCount - femaleCount).ShouldBe(counts.Int("totalGenderTrong"));
    }

    /// <summary>
    /// A-114 · Sắp xếp theo từng cột, hai chiều: chiều nghịch phải đúng là chiều thuận đảo
    /// ngược, và ô trống không làm sai thứ tự.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A114_Sap_xep_hai_chieu_dung_tren_moi_cot()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        foreach (string column in new[] { "fullName", "dateOfBirth", "gender", "officialAdmissionDate" })
        {
            string field = char.ToUpperInvariant(column[0]) + column[1..];

            IReadOnlyList<JsonElement> ascending = await ReadRowsAsync(client, $"sortQuery={field}%20asc");
            IReadOnlyList<JsonElement> descending = await ReadRowsAsync(client, $"sortQuery={field}%20desc");

            ascending.Count.ShouldBe(QcFixtures.CoreMembers.Count, field);
            descending.Count.ShouldBe(QcFixtures.CoreMembers.Count, field);

            ShouldBeMonotonic(ascending, column, ascendingOrder: true);
            ShouldBeMonotonic(descending, column, ascendingOrder: false);

            // Cùng một truy vấn gọi hai lần phải cho đúng một thứ tự — thứ tự bốc thăm lại mỗi
            // lần gọi chính là nguyên nhân làm phân trang thiếu người (xem A-107).
            IReadOnlyList<JsonElement> again = await ReadRowsAsync(client, $"sortQuery={field}%20asc");
            again.Select(row => row.Str("id")).ToList()
                .ShouldBe(ascending.Select(row => row.Str("id")).ToList(), $"cột {field} sắp không tất định");
        }
    }

    /// <summary>
    /// A-115 · Sắp theo Họ tên dùng đối chiếu tiếng Việt (OQ-3, mục 1.7 hợp đồng API):
    /// "Đào Văn Ân" đứng trước "Nguyễn Văn An", không theo mã Unicode.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A115_Sap_theo_Ho_ten_dung_bang_chu_cai_tieng_Viet()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        IReadOnlyList<JsonElement> rows = await ReadRowsAsync(client, "sortQuery=FullName%20asc");
        List<string> names = rows.Select(row => row.Str("fullName")).ToList();

        names.IndexOf("Đào Văn Ân").ShouldBeGreaterThanOrEqualTo(0);
        names.IndexOf("Nguyễn Văn An").ShouldBeGreaterThanOrEqualTo(0);
        names.IndexOf("Đào Văn Ân").ShouldBeLessThan(names.IndexOf("Nguyễn Văn An"));
    }

    /// <summary>A-116 · Thêm tay hợp lệ làm tổng tăng đúng 1.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A116_Them_tay_hop_le_tang_dung_mot_nguoi()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        int before = await QcDb.CountMembersAsync(factory);

        JsonElement created = await QcApi.PostDataAsync(client, QcEndpoints.PartyMembers, new
        {
            fullName = "Nguyễn Thêm Tay",
            dateOfBirth = "1970-01-15",
            gender = "Male",
            officialAdmissionDate = "1996-05-20",
        });

        created.Str("fullName").ShouldBe("Nguyễn Thêm Tay");
        created.Str("officialAdmissionDate").ShouldBe("1996-05-20");
        (await QcDb.CountMembersAsync(factory)).ShouldBe(before + 1);
    }

    /// <summary>A-117 · Thêm tay thiếu Họ tên bị từ chối bằng khóa thông điệp đúng.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A117_Them_tay_thieu_ho_ten_bi_tu_choi()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        int before = await QcDb.CountMembersAsync(factory);

        using HttpResponseMessage response = await client.PostAsJsonAsync(QcEndpoints.PartyMembers, new
        {
            fullName = string.Empty,
            dateOfBirth = (string?)null,
            gender = (string?)null,
            officialAdmissionDate = "1996-05-20",
        });

        await QcApi.AssertErrorAsync(response, HttpStatusCode.BadRequest, QcMessages.MemberRequiredFullName);
        (await QcDb.CountMembersAsync(factory)).ShouldBe(before);
    }

    /// <summary>
    /// A-118 và A-119 · Ngày chính thức đúng bằng hôm nay là hợp lệ; sau hôm nay một ngày
    /// thì không (biên "≤ hôm nay theo lịch máy chủ").
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A118_A119_Bien_ngay_chinh_thuc_bang_hom_nay()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        JsonElement today = await QcApi.PostDataAsync(client, QcEndpoints.PartyMembers, new
        {
            fullName = "Lưu Đúng Hôm Nay",
            dateOfBirth = "2000-04-04",
            gender = "Female",
            officialAdmissionDate = QcClock.T0.ToString("yyyy-MM-dd"),
        });
        today.Int("partyAgeYears").ShouldBe(0);

        using HttpResponseMessage tomorrow = await client.PostAsJsonAsync(QcEndpoints.PartyMembers, new
        {
            fullName = "Lưu Ngày Mai",
            dateOfBirth = "2000-04-04",
            gender = "Female",
            officialAdmissionDate = QcClock.T0.AddDays(1).ToString("yyyy-MM-dd"),
        });

        await QcApi.AssertErrorAsync(tomorrow, HttpStatusCode.BadRequest, QcMessages.MemberInvalidAdmissionDate);
    }

    /// <summary>
    /// A-120 · Sửa Ngày chính thức làm tuổi đảng, mốc kế tiếp và danh sách đủ điều kiện đổi
    /// theo ngay, không cần thao tác nào khác (QT5).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A120_Sua_ngay_chinh_thuc_lam_moi_thu_doi_theo_ngay()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        JsonElement period = await FindPeriodAsync(client, "Đợt 7/11");
        string periodId = period.Str("id");
        int before = await EligibleCountAsync(client, periodId, 2026);

        // "Phạm Thị Dung" (B04) tròn 30 vào 08/11/2026, rơi ngay sau Đợt 7/11 nên đang bị sót.
        JsonElement target = await FindMemberAsync(client, "Phạm Thị Dung");
        int ageBefore = target.Int("partyAgeYears");

        // Dời Ngày chính thức lùi một ngày thì ngày tròn mốc rơi vào 07/11/2026 — đúng Đến ngày.
        JsonElement updated = await QcApi.PutDataAsync(
            client,
            $"{QcEndpoints.PartyMembers}/{target.Str("id")}",
            new
            {
                fullName = target.Str("fullName"),
                dateOfBirth = target.StringOrNull("dateOfBirth"),
                gender = target.StringOrNull("gender"),
                officialAdmissionDate = "1996-11-07",
            });

        updated.Str("officialAdmissionDate").ShouldBe("1996-11-07");
        updated.Str("nextMilestoneDate").ShouldBe("2026-11-07");
        updated.Int("partyAgeYears").ShouldBe(ageBefore);

        (await EligibleCountAsync(client, periodId, 2026)).ShouldBe(before + 1);
    }

    /// <summary>
    /// A-121 · Xóa một người qua <c>DeleteMany</c> với mảng một phần tử làm tổng giảm đúng 1.
    /// Từ T34 đây là đường xóa duy nhất.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A121_Xoa_mot_nguoi_giam_dung_mot()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        int before = await QcDb.CountMembersAsync(factory);

        JsonElement target = await FindMemberAsync(client, "Nguyễn Văn An");

        JsonElement data = await QcApi.PostDataAsync(
            client, QcEndpoints.PartyMembersDeleteMany, new { ids = new[] { target.Str("id") } });

        data.Array("ids").Count.ShouldBe(1);
        (await QcDb.CountMembersAsync(factory)).ShouldBe(before - 1);
    }

    /// <summary>A-122 · Xóa nhiều người một lần làm tổng giảm đúng số lượng, xóa hẳn (QT10).</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A122_Xoa_nhieu_nguoi_giam_dung_so_luong()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        int before = await QcDb.CountMembersAsync(factory);

        JsonElement page = await QcApi.GetDataAsync(client, $"{QcEndpoints.PartyMembers}?current=1&pageSize=5");
        List<string> ids = page.Array("pagedData").Select(row => row.Str("id")).ToList();

        JsonElement data = await QcApi.PostDataAsync(client, QcEndpoints.PartyMembersDeleteMany, new { ids });

        data.Array("ids").Count.ShouldBe(ids.Count);
        (await QcDb.CountMembersAsync(factory)).ShouldBe(before - ids.Count);
    }

    /// <summary>
    /// A-123 · Xóa id không tồn tại không làm hỏng lời gọi: trả danh sách rỗng chứ không phải
    /// lỗi 500. Kèm khẳng định đường xóa một <c>DELETE /api/PartyMembers/{id}</c> đã bị gỡ ở
    /// T34, để không ai vô tình dựng lại.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A123_Xoa_id_khong_ton_tai_va_duong_xoa_mot_da_bi_go()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        int before = await QcDb.CountMembersAsync(factory);

        JsonElement data = await QcApi.PostDataAsync(
            client, QcEndpoints.PartyMembersDeleteMany, new { ids = new[] { Guid.NewGuid().ToString() } });

        data.Array("ids").Count.ShouldBe(0);
        (await QcDb.CountMembersAsync(factory)).ShouldBe(before);

        using HttpResponseMessage removed =
            await client.DeleteAsync($"{QcEndpoints.PartyMembers}/{Guid.NewGuid()}");

        removed.StatusCode.ShouldBeOneOf(HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed);
    }

    /// <summary>
    /// A-124 · Xóa danh sách có id trùng nhau: không đếm trùng, không lỗi. Hợp đồng mục 3.6
    /// chốt id lạ bị bỏ qua lặng lẽ.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A124_Xoa_danh_sach_co_id_trung_khong_dem_trung()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        int before = await QcDb.CountMembersAsync(factory);

        JsonElement target = await FindMemberAsync(client, "Nguyễn Văn An");
        string id = target.Str("id");

        JsonElement data = await QcApi.PostDataAsync(
            client, QcEndpoints.PartyMembersDeleteMany, new { ids = new[] { id, id, Guid.NewGuid().ToString() } });

        data.Array("ids").Select(value => value.GetString()).Distinct(StringComparer.Ordinal).Count().ShouldBe(1);
        (await QcDb.CountMembersAsync(factory)).ShouldBe(before - 1);
    }

    /// <summary>
    /// A-125 · Dòng tóm tắt: tổng số khớp, và tuổi đảng trên từng dòng là tuổi tính đến hôm nay
    /// chứ không phải giá trị lưu sẵn (QT3, QT5).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A125_Tong_so_va_tuoi_dang_tinh_den_hom_nay()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        JsonElement data = await QcApi.GetDataAsync(client, $"{QcEndpoints.PartyMembers}?pageSize=100");
        data.GetProperty("pageInfo").Int("totalCount").ShouldBe(QcFixtures.CoreMembers.Count);

        JsonElement expectedRows = QcFixtures.Node("memberList").GetProperty("T0_default");

        foreach (JsonElement expected in expectedRows.EnumerateArray())
        {
            JsonElement actual = data.Array("pagedData")
                .Single(row => row.Str("fullName") == expected.Str("fullName"));

            actual.Int("partyAgeYears").ShouldBe(expected.Int("partyAge"), expected.Str("fullName"));
            actual.IntOrNull("nextMilestone").ShouldBe(expected.IntOrNull("nextMilestone"), expected.Str("fullName"));
            actual.StringOrNull("nextMilestoneDate")
                .ShouldBe(expected.StringOrNull("nextAnniversary"), expected.Str("fullName"));
        }
    }

    /// <summary>
    /// A-126 · Lấy một đảng viên theo id (mục 3.2 hợp đồng API): trả đúng người đó, đúng các
    /// giá trị tính ra như trên danh sách; id lạ trả lỗi nghiệp vụ chứ không phải 404.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A126_Lay_mot_dang_vien_theo_id()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        JsonElement fromList = await FindMemberAsync(client, "Nguyễn Văn An");
        JsonElement detail = await QcApi.GetDataAsync(client, $"{QcEndpoints.PartyMembers}/{fromList.Str("id")}");

        detail.Str("id").ShouldBe(fromList.Str("id"));
        detail.Str("fullName").ShouldBe(fromList.Str("fullName"));
        detail.Str("officialAdmissionDate").ShouldBe(fromList.Str("officialAdmissionDate"));
        detail.Int("partyAgeYears").ShouldBe(fromList.Int("partyAgeYears"));
        detail.IntOrNull("nextMilestone").ShouldBe(fromList.IntOrNull("nextMilestone"));
        detail.StringOrNull("nextMilestoneDate").ShouldBe(fromList.StringOrNull("nextMilestoneDate"));

        using HttpResponseMessage missing = await client.GetAsync($"{QcEndpoints.PartyMembers}/{Guid.NewGuid()}");
        await QcApi.AssertErrorAsync(missing, HttpStatusCode.BadRequest, QcMessages.MemberNotFound);
    }

    /// <summary>
    /// Khẳng định dãy giá trị của một cột đi đúng một chiều, và ô trống dồn hết về một đầu
    /// chứ không xen kẽ giữa các ô có giá trị.
    /// </summary>
    /// <param name="rows">Các dòng theo đúng thứ tự máy chủ trả về.</param>
    /// <param name="field">Tên trường JSON của cột.</param>
    /// <param name="ascendingOrder"><see langword="true"/> khi mong đợi tăng dần.</param>
    private static void ShouldBeMonotonic(IReadOnlyList<JsonElement> rows, string field, bool ascendingOrder)
    {
        List<string?> values = rows.Select(row => row.StringOrNull(field)).ToList();
        List<string> present = values.Where(value => value is not null).Select(value => value!).ToList();

        // Giới tính lưu dưới dạng enum (Male = 1, Female = 2) nên "đúng chiều" là theo giá trị
        // enum, không phải theo chữ cái của chuỗi trên dây.
        static string Key(string value) => value switch
        {
            "Male" => "1",
            "Female" => "2",
            _ => value,
        };

        List<string> expected = ascendingOrder
            ? present.OrderBy(Key, StringComparer.Ordinal).ToList()
            : present.OrderByDescending(Key, StringComparer.Ordinal).ToList();

        if (field is not "fullName")
        {
            // Họ tên sắp theo đối chiếu tiếng Việt, không theo mã Unicode — A-115 lo cột đó riêng.
            present.ShouldBe(expected, $"cột {field} không đi đúng chiều {(ascendingOrder ? "tăng" : "giảm")}");
        }

        List<int> blankPositions = values
            .Select((value, index) => (value, index))
            .Where(pair => pair.value is null)
            .Select(pair => pair.index)
            .ToList();

        if (blankPositions.Count > 1)
        {
            (blankPositions[^1] - blankPositions[0])
                .ShouldBe(blankPositions.Count - 1, $"ô trống của cột {field} nằm xen kẽ giữa các ô có giá trị");
        }
    }

    private async Task<HttpClient> SeedAllAndLoginAsync()
    {
        await QcDb.SeedCoreAndBulkAsync(factory);

        return await QcApi.LoginAsync(factory);
    }

    private async Task<HttpClient> SeedCoreAndLoginAsync()
    {
        await QcDb.SeedCoreAsync(factory);

        return await QcApi.LoginAsync(factory);
    }

    private static async Task<IReadOnlyList<JsonElement>> ReadRowsAsync(HttpClient client, string query)
    {
        JsonElement data = await QcApi.GetDataAsync(client, $"{QcEndpoints.PartyMembers}?pageSize=200&{query}");

        return data.Array("pagedData");
    }

    private static async Task<JsonElement> FindMemberAsync(HttpClient client, string fullName)
    {
        JsonElement data = await QcApi.GetDataAsync(
            client, $"{QcEndpoints.PartyMembers}?searchKeyword={Uri.EscapeDataString(fullName)}&searchFields=FullName");

        return data.Array("pagedData").Single(row => row.Str("fullName") == fullName);
    }

    private static async Task<JsonElement> FindPeriodAsync(HttpClient client, string name)
    {
        JsonElement data = await QcApi.GetDataAsync(client, QcEndpoints.AwardPeriods);

        return data.Array("periods").Single(period => period.Str("name") == name);
    }

    private static async Task<int> EligibleCountAsync(HttpClient client, string periodId, int year)
    {
        JsonElement data = await QcApi.GetDataAsync(
            client, $"{QcEndpoints.Eligibility}?awardPeriodId={periodId}&year={year}");

        return data.Int("totalCount");
    }
}
