using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure.Facades.Common.Services;
using HuyHieuDang.Web.QcIntegrationTests.Support;
using PartyMemberEntity = HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities.PartyMember;

namespace HuyHieuDang.Web.QcIntegrationTests;

/// <summary>
/// A-901 → A-905 · Ràng buộc kỹ thuật của kế hoạch kiểm thử mục 5.9: không đọc đồng hồ máy,
/// không phụ thuộc múi giờ tiến trình, không để biến môi trường dịch chuyển "hôm nay" ở
/// Production, hiệu năng, và thông báo lỗi không lộ nội bộ.
/// </summary>
[Collection(QcApiCollection.Name)]
public sealed class A9TechnicalTests
{
    /// <summary>Tên biến môi trường ép ngày cho E2E theo T-FIX-4.</summary>
    private const string TestTodayVariable = "HUYHIEUDANG_TEST_TODAY";

    private readonly QcApiFactory factory;

    /// <summary>Initializes a new instance of the <see cref="A9TechnicalTests"/> class.</summary>
    /// <param name="factory">Host kiểm thử dùng chung.</param>
    public A9TechnicalTests(QcApiFactory factory)
    {
        this.factory = factory;
    }

    /// <summary>
    /// A-901 · Quét toàn bộ <c>BE/src</c>: không nơi nào ngoài lớp provider đọc đồng hồ máy
    /// (T-FIX-1). Vi phạm là lỗi chặn.
    /// </summary>
    [Fact]
    public void A901_Khong_noi_nao_trong_src_doc_dong_ho_may()
    {
        string[] forbidden =
        {
            "DateTime.Now",
            "DateTime.Today",
            "DateTimeOffset.Now",
            "TimeZoneInfo.Local",
        };

        List<string> violations = new();

        foreach (string file in Directory.EnumerateFiles(QcPaths.BackendSource, "*.cs", SearchOption.AllDirectories))
        {
            // Lớp provider là nơi duy nhất được phép chạm tới đồng hồ thật.
            if (Path.GetFileName(file) is "DateTimeProvider.cs")
            {
                continue;
            }

            string[] lines = File.ReadAllLines(file);

            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index];

                if (line.TrimStart().StartsWith("//", StringComparison.Ordinal))
                {
                    continue;
                }

                foreach (string marker in forbidden)
                {
                    if (line.Contains(marker, StringComparison.Ordinal))
                    {
                        violations.Add($"{Path.GetRelativePath(QcPaths.RepositoryRoot, file)}:{index + 1} · {marker}");
                    }
                }
            }
        }

        violations.ShouldBeEmpty();
    }

    /// <summary>
    /// A-902 · Kết quả không phụ thuộc múi giờ của tiến trình (T-FIX-2): provider tự quy đổi
    /// sang Asia/Ho_Chi_Minh thay vì đọc múi giờ máy, nên chạy ở <c>TZ=UTC</c> cho cùng kết quả.
    /// </summary>
    [Fact]
    public void A902_Ket_qua_khong_phu_thuoc_mui_gio_cua_tien_trinh()
    {
        // Phần hành vi: độ lệch luôn là +07:00 và ngày đúng bằng giờ UTC quy sang +07:00,
        // bất kể máy chạy đang ở múi giờ nào.
        IDateTimeProvider provider = new DateTimeProvider();
        DateTimeOffset now = provider.Now;

        now.Offset.ShouldBe(TimeSpan.FromHours(7));
        provider.Today.ShouldBe(DateOnly.FromDateTime(DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).DateTime));

        // Phần tĩnh: lớp provider không đọc múi giờ của tiến trình ở bất cứ đâu.
        string source = File.ReadAllText(
            Path.Combine(QcPaths.BackendSource, "Infrastructure", "Facades", "Common", "Services", "DateTimeProvider.cs"));

        source.ShouldNotContain("TimeZoneInfo.Local");
        source.ShouldNotContain("DateTime.Now");
        source.ShouldNotContain("DateTimeOffset.Now");
        source.ShouldContain("Asia/Ho_Chi_Minh");
    }

    /// <summary>
    /// A-903 · Biến môi trường ép ngày **không** được có hiệu lực ở Production. Hiện <c>BE/src</c>
    /// không đọc biến này ở bất cứ đâu, nên yêu cầu bảo mật của T-FIX-4 đang được thỏa mãn.
    /// </summary>
    [Fact]
    public void A903_Bien_ep_ngay_khong_co_hieu_luc_o_Production()
    {
        List<string> readers = Directory
            .EnumerateFiles(QcPaths.BackendSource, "*.cs", SearchOption.AllDirectories)
            .Where(file => File.ReadAllText(file).Contains(TestTodayVariable, StringComparison.Ordinal))
            .Select(file => Path.GetRelativePath(QcPaths.RepositoryRoot, file))
            .ToList();

        readers.ShouldBeEmpty(
            $"{TestTodayVariable} được đọc ở đây — phải chứng minh thêm rằng Production bỏ qua nó");
    }

    /// <summary>
    /// A-903b · Mặt còn lại của T-FIX-4: ngoài Production, biến môi trường phải ép được "hôm nay"
    /// để Playwright chạy được ở T28. Hiện chưa cài đặt nên ca này để <c>Skip</c> kèm mã lỗi.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(Skip = "QC-T27-01 · BE chưa đọc HUYHIEUDANG_TEST_TODAY nên T-FIX-4 chưa dùng được cho T28")]
    public async Task A903b_Ngoai_Production_bien_ep_ngay_phai_co_hieu_luc()
    {
        Environment.SetEnvironmentVariable(TestTodayVariable, "2026-10-15");

        try
        {
            using HttpClient client = await QcApi.LoginAsync(factory);
            JsonElement data = await QcApi.GetDataAsync(client, QcEndpoints.Dashboard);

            data.Str("today").ShouldBe("2026-10-15");
        }
        finally
        {
            Environment.SetEnvironmentVariable(TestTodayVariable, null);
        }
    }

    /// <summary>
    /// A-904 · Tính danh sách đủ điều kiện của một đợt trong một năm với 10.000 đảng viên trong
    /// cơ sở dữ liệu phải xong dưới 1 giây (mục 7 tài liệu nghiệp vụ).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A904_Mot_van_dang_vien_van_tinh_duoi_mot_giay()
    {
        await QcDb.SeedCoreAsync(factory);

        string table = QcDb.TableName<PartyMemberEntity>(factory);
        string id = QcDb.ColumnName<PartyMemberEntity>(factory, nameof(PartyMemberEntity.Id));
        string fullName = QcDb.ColumnName<PartyMemberEntity>(factory, nameof(PartyMemberEntity.FullName));
        string admission = QcDb.ColumnName<PartyMemberEntity>(
            factory, nameof(PartyMemberEntity.OfficialAdmissionDate));
        string createdAt = QcDb.ColumnName<PartyMemberEntity>(factory, nameof(PartyMemberEntity.CreatedAt));
        string updatedAt = QcDb.ColumnName<PartyMemberEntity>(factory, nameof(PartyMemberEntity.UpdatedAt));

        // Ngày chính thức trải đều 1970–2005 để dữ liệu giống thật; ngày cố định, không theo
        // đồng hồ máy, nên ca này chạy lại lúc nào cũng cho cùng kết quả.
        await QcDb.ExecuteSqlAsync(
            factory,
            $"""
             INSERT INTO {table} ({id}, {fullName}, {admission}, {createdAt}, {updatedAt})
             SELECT gen_random_uuid(),
                    'Đảng viên hiệu năng ' || n,
                    DATE '1970-01-01' + ((n * 37) % 12800),
                    TIMESTAMPTZ '2026-01-01 00:00:00+07',
                    TIMESTAMPTZ '2026-01-01 00:00:00+07'
             FROM generate_series(1, 10000) AS n
             """);

        (await QcDb.CountMembersAsync(factory)).ShouldBe(10_000 + QcFixtures.CoreMembers.Count);

        using HttpClient client = await QcApi.LoginAsync(factory);
        string periodId = (await QcApi.GetDataAsync(client, QcEndpoints.AwardPeriods))
            .Array("periods").Single(period => period.Str("name") == "Đợt 7/11").Str("id");

        string url = $"{QcEndpoints.Eligibility}?awardPeriodId={periodId}&year=2026";

        // Lời gọi đầu gánh cả chi phí khởi động truy vấn; đo từ lời gọi thứ hai trở đi.
        await QcApi.GetDataAsync(client, url);

        List<TimeSpan> samples = new();

        for (int time = 0; time < 3; time++)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            JsonElement data = await QcApi.GetDataAsync(client, url);
            stopwatch.Stop();

            data.Int("totalCount").ShouldBeGreaterThanOrEqualTo(
                QcFixtures.Eligible("core_default_T0", "P4", 2026).Int("total"));
            samples.Add(stopwatch.Elapsed);
        }

        TimeSpan median = samples.OrderBy(sample => sample).ElementAt(1);
        median.ShouldBeLessThan(
            TimeSpan.FromSeconds(1),
            $"đo được {string.Join(" · ", samples.Select(sample => sample.TotalMilliseconds.ToString("F0", CultureInfo.InvariantCulture) + " ms"))}");
    }

    /// <summary>
    /// A-905 · Mọi lỗi trả cho người dùng đều mang khóa thông điệp có trong bảng mục 1.5 hợp
    /// đồng API — tức Frontend dịch được sang tiếng Việt — và không lộ dấu vết ngăn xếp, tên
    /// kiểu .NET hay tên bảng/cột.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A905_Moi_thong_bao_loi_deu_tra_khoa_dich_duoc_va_khong_lo_noi_bo()
    {
        await QcDb.SeedCoreAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);
        using HttpClient anonymous = QcApi.Anonymous(factory);

        List<HttpResponseMessage> responses = new()
        {
            await anonymous.PostAsJsonAsync(QcEndpoints.Login, new { username = "admin", password = "sai" }),
            await anonymous.PostAsJsonAsync(QcEndpoints.Login, new { username = string.Empty, password = string.Empty }),
            await client.GetAsync($"{QcEndpoints.PartyMembers}/{Guid.NewGuid()}"),
            await client.PostAsJsonAsync(QcEndpoints.PartyMembers, new { fullName = string.Empty, officialAdmissionDate = "1996-01-01" }),
            await client.PostAsJsonAsync(QcEndpoints.PartyMembers, new { fullName = "Người Tương Lai", officialAdmissionDate = "2030-01-01" }),
            await client.PostAsJsonAsync(QcEndpoints.PartyMembers, new { fullName = "Người Sinh Sau", dateOfBirth = "2000-01-01", officialAdmissionDate = "1996-01-01" }),
            await client.DeleteAsync($"{QcEndpoints.PartyMembers}/{Guid.NewGuid()}"),
            await client.GetAsync($"{QcEndpoints.AwardPeriods}/{Guid.NewGuid()}"),
            await client.PostAsJsonAsync(QcEndpoints.AwardPeriods, new { name = "Đợt 7/11", fromDay = 1, fromMonth = 10, toDay = 7, toMonth = 11 }),
            await client.PostAsJsonAsync(QcEndpoints.AwardPeriods, new { name = "Đợt sai ngày", fromDay = 31, fromMonth = 4, toDay = 7, toMonth = 11 }),
            await client.PostAsJsonAsync(QcEndpoints.AwardPeriods, new { name = "Đợt ngược", fromDay = 10, fromMonth = 11, toDay = 1, toMonth = 10 }),
            await client.PutAsJsonAsync(QcEndpoints.Settings, new { startYears = 90, endYears = 30, stepYears = 5 }),
            await client.PutAsJsonAsync(QcEndpoints.Settings, new { startYears = 30, endYears = 90, stepYears = 0 }),
            await client.GetAsync($"{QcEndpoints.Eligibility}?awardPeriodId={Guid.NewGuid()}"),
            await client.GetAsync($"{QcEndpoints.Unassigned}?year=99999"),
            await QcUpload.SendFileAsync(client, QcEndpoints.ImportPreview, QcPaths.Excel("loi-sai-cot.xlsx")),
            await QcUpload.SendFileAsync(client, QcEndpoints.ImportPreview, QcPaths.Excel("loi-rong.xlsx")),
            await QcUpload.SendFileAsync(client, QcEndpoints.ImportPreview, QcPaths.Excel("loi-dinh-dang-csv.csv")),
        };

        try
        {
            foreach (HttpResponseMessage response in responses)
            {
                string body = await response.Content.ReadAsStringAsync();

                ((int)response.StatusCode).ShouldBeLessThan(500, body);
                QcMessages.ShouldNotLeakInternals(body);

                string? key = JsonDocument.Parse(body).RootElement.StringOrNull("message");
                key.ShouldNotBeNull(body);

                QcMessages.ContractKeys.ShouldContain(
                    key!, $"khóa {key} không có trong bảng mục 1.5 hợp đồng API nên Frontend không dịch được");

                // Khóa phải đúng khuôn Mes.<ThựcThể>.<HànhĐộng>[.<KếtQuả>] của mục 1.5.
                Regex.IsMatch(key!, @"^Mes(\.[A-Za-z]+){2,3}$").ShouldBeTrue(key);
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

    /// <summary>
    /// A-906 · Mọi trường ngày nghiệp vụ trên dây là ngày thuần <c>yyyy-MM-dd</c>, không phải
    /// mốc thời gian có giờ (mục 1.6 hợp đồng API — dùng <c>DateTime</c> sẽ lệch một ngày).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A906_Moi_truong_ngay_nghiep_vu_la_ngay_thuan()
    {
        await QcDb.SeedCoreAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        string[] dateFields =
        {
            "dateOfBirth", "officialAdmissionDate", "milestoneDate", "nextMilestoneDate",
            "fromDate", "toDate", "today", "serverDate",
        };

        foreach (string url in new[]
                 {
                     QcEndpoints.Dashboard,
                     QcEndpoints.Me,
                     $"{QcEndpoints.PartyMembers}?pageSize=50",
                     $"{QcEndpoints.AwardPeriods}?year=2026",
                     $"{QcEndpoints.Unassigned}?year=2026",
                 })
        {
            using HttpResponseMessage response = await client.GetAsync(url);
            response.StatusCode.ShouldBe(HttpStatusCode.OK, url);

            JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

            foreach ((string field, string value) in FindDates(document.RootElement, dateFields))
            {
                Regex.IsMatch(value, @"^\d{4}-\d{2}-\d{2}$")
                    .ShouldBeTrue($"{url} · {field} = \"{value}\" không phải ngày thuần yyyy-MM-dd");
            }
        }
    }

    private static IEnumerable<(string Field, string Value)> FindDates(JsonElement element, string[] fields)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (fields.Contains(property.Name, StringComparer.Ordinal)
                        && property.Value.ValueKind is JsonValueKind.String)
                    {
                        yield return (property.Name, property.Value.GetString()!);
                    }

                    foreach ((string Field, string Value) found in FindDates(property.Value, fields))
                    {
                        yield return found;
                    }
                }

                break;

            case JsonValueKind.Array:
                foreach (JsonElement item in element.EnumerateArray())
                {
                    foreach ((string Field, string Value) found in FindDates(item, fields))
                    {
                        yield return found;
                    }
                }

                break;

            default:
                break;
        }
    }
}
