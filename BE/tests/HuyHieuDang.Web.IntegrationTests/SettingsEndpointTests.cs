using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Mục 7 hợp đồng API (UC-50, UC-51) chạy trên PostgreSQL thật. Điểm phải chứng minh không chỉ là
/// bốn endpoint trả đúng hình dạng, mà là đổi cài đặt xong thì nhóm tính toán đổi theo ngay
/// (QT5, A-502): các con số trước và sau đọc thẳng từ <c>tests/fixtures/data/expected.json</c>,
/// hai kịch bản <c>core_default_T0</c> và <c>core_step10_T0</c>.
/// </summary>
[Collection(ApiCollection.Name)]
public class SettingsEndpointTests
{
    private const string BasePath = "/api/Settings";

    private readonly HuyHieuDangApiFactory factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsEndpointTests"/> class.
    /// </summary>
    /// <param name="factory">Host kiểm thử dùng chung cho cả lớp.</param>
    public SettingsEndpointTests(HuyHieuDangApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact(DisplayName = "7.1 · A-501 · Kho chưa có bản ghi cài đặt nào thì đọc ra mặc định 30/90/5")]
    public async Task Get_WithoutStoredSetting_ReturnsDefaults()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await DeleteSettingAsync();

        try
        {
            AppSettingPayload data = await ApiClientFactory.GetDataAsync<AppSettingPayload>(client, BasePath);

            AssertDefaultMilestones(data);
            Assert.Null(data.UnitName);

            // Đọc không được tạo bản ghi: bảng vẫn trống cho tới khi có lệnh ghi thật sự.
            Assert.Equal(0, await CountSettingsAsync());
        }
        finally
        {
            await RestoreSettingRowAsync();
        }
    }

    [Fact(DisplayName = "7.1 · Kho đã seed: đọc ra 30/90/5, 13 mốc và tên đơn vị đang lưu")]
    public async Task Get_WithSeededSetting_ReturnsStoredValuesAndMilestones()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        AppSettingPayload data = await ApiClientFactory.GetDataAsync<AppSettingPayload>(client, BasePath);

        AssertDefaultMilestones(data);
        Assert.Equal(CoreFixtures.Settings["default"].UnitName, data.UnitName);
        Assert.Equal(
            ExpectedMilestones("core_default_T0"),
            data.Milestones);
    }

    [Fact(DisplayName = "7.2 · A-502 · Đổi Bước 5 → 10: danh sách đủ điều kiện và badge đổi ngay (QT5)")]
    public async Task Update_WithStepTen_RecomputesEligibilityImmediately()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        Guid periodId = await FindPeriodIdAsync(client, "P4");
        string eligibilityUrl = $"/api/Eligibility?awardPeriodId={periodId}&year=2026";
        const string badgeUrl = "/api/Eligibility/UnassignedCount?year=2026";

        EligibilityListPayload before =
            await ApiClientFactory.GetDataAsync<EligibilityListPayload>(client, eligibilityUrl);
        UnassignedCountPayload badgeBefore =
            await ApiClientFactory.GetDataAsync<UnassignedCountPayload>(client, badgeUrl);

        Assert.Equal(ExpectedTotal("core_default_T0", "P4", 2026), before.TotalCount);
        Assert.Equal(ExpectedBadge("core_default_T0"), badgeBefore.Count);

        AppSettingPayload saved = await PutAsync(client, new { startYears = 30, endYears = 90, stepYears = 10 });

        Assert.Equal(10, saved.StepYears);
        Assert.Equal(ExpectedMilestones("core_step10_T0"), saved.Milestones);
        Assert.Equal(7, saved.MilestoneCount);

        EligibilityListPayload after =
            await ApiClientFactory.GetDataAsync<EligibilityListPayload>(client, eligibilityUrl);
        UnassignedCountPayload badgeAfter =
            await ApiClientFactory.GetDataAsync<UnassignedCountPayload>(client, badgeUrl);

        Assert.Equal(ExpectedTotal("core_step10_T0", "P4", 2026), after.TotalCount);
        Assert.Equal(ExpectedBadge("core_step10_T0"), badgeAfter.Count);
        Assert.NotEqual(before.TotalCount, after.TotalCount);
    }

    [Fact(DisplayName = "7.2 · A-507 · Lưu Tên đơn vị: đọc lại thấy ngay, Dashboard cũng thấy")]
    public async Task Update_WithUnitName_StoresAndExposesIt()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        const string unitName = "Đảng ủy Phường Bàn Giao";
        AppSettingPayload saved = await PutAsync(
            client, new { startYears = 30, endYears = 90, stepYears = 5, unitName });

        Assert.Equal(unitName, saved.UnitName);
        Assert.Equal(unitName, (await ApiClientFactory.GetDataAsync<AppSettingPayload>(client, BasePath)).UnitName);
        Assert.Equal(
            unitName,
            (await ApiClientFactory.GetDataAsync<DashboardPayload>(client, "/api/Dashboard")).UnitName);
    }

    [Theory(DisplayName = "7.2 · A-508 · Tên đơn vị bỏ trống hoặc toàn khoảng trắng đều thành chưa đặt")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Update_WithBlankUnitName_StoresNull(string? unitName)
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        AppSettingPayload saved = await PutAsync(
            client, new { startYears = 30, endYears = 90, stepYears = 5, unitName });

        Assert.Null(saved.UnitName);
        Assert.Null((await ApiClientFactory.GetDataAsync<AppSettingPayload>(client, BasePath)).UnitName);
    }

    [Fact(DisplayName = "7.2 · A-503 · Bắt đầu > Kết thúc trả 400 Mes.AppSetting.Invalid.Range")]
    public async Task Update_WithStartGreaterThanEnd_ReturnsRangeError()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        await ApiClientFactory.AssertErrorAsync(
            await client.PutAsJsonAsync(BasePath, new { startYears = 90, endYears = 30, stepYears = 5 }),
            Messages<AppSetting>.Invalid(AppSettingMessageProperties.Range));

        await AssertUnchangedAsync(client);
    }

    [Theory(DisplayName = "7.2 · A-504 · Bước 0 hoặc âm trả 400 Mes.AppSetting.Invalid.StepYears")]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Update_WithNonPositiveStep_ReturnsStepError(int stepYears)
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        await ApiClientFactory.AssertErrorAsync(
            await client.PutAsJsonAsync(BasePath, new { startYears = 30, endYears = 90, stepYears }),
            Messages<AppSetting>.Invalid(x => x.StepYears));

        await AssertUnchangedAsync(client);
    }

    [Theory(DisplayName = "7.2 · A-504 · Mốc bắt đầu / kết thúc ≤ 0 trả đúng khóa lỗi của từng trường")]
    [InlineData(0, 90, "StartYears")]
    [InlineData(-1, 90, "StartYears")]
    [InlineData(30, 0, "EndYears")]
    [InlineData(30, -30, "EndYears")]
    public async Task Update_WithNonPositiveBound_ReturnsBoundError(int startYears, int endYears, string property)
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        await ApiClientFactory.AssertErrorAsync(
            await client.PutAsJsonAsync(BasePath, new { startYears, endYears, stepYears = 5 }),
            Messages<AppSetting>.Invalid(property));

        await AssertUnchangedAsync(client);
    }

    [Theory(DisplayName = "7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400")]
    [InlineData("{\"startYears\":null,\"endYears\":90,\"stepYears\":5}")]
    [InlineData("{\"endYears\":90,\"stepYears\":5}")]
    [InlineData("{\"startYears\":\"\",\"endYears\":90,\"stepYears\":5}")]
    [InlineData("{\"startYears\":\"ba mươi\",\"endYears\":90,\"stepYears\":5}")]
    [InlineData("{\"startYears\":30.5,\"endYears\":90,\"stepYears\":5}")]
    public async Task Update_WithNonNumericValue_ReturnsBadRequest(string body)
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        HttpResponseMessage response = await client.PutAsync(
            BasePath, new StringContent(body, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertUnchangedAsync(client);
    }

    [Fact(DisplayName = "7.2 · Tên đơn vị quá 200 ký tự trả 400 Mes.AppSetting.OverLength.UnitName")]
    public async Task Update_WithTooLongUnitName_ReturnsOverLengthError()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        await ApiClientFactory.AssertErrorAsync(
            await client.PutAsJsonAsync(
                BasePath,
                new
                {
                    startYears = 30,
                    endYears = 90,
                    stepYears = 5,
                    unitName = new string('a', UpdateAppSettingRequest.UnitNameMaxLength + 1),
                }),
            Messages<AppSetting>.OverLength(x => x.UnitName));
    }

    [Fact(DisplayName = "7.3 · A-506 · Khôi phục mặc định đưa về 30/90/5 và giữ nguyên Tên đơn vị")]
    public async Task RestoreDefaults_AfterChange_ResetsMilestonesAndKeepsUnitName()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        const string unitName = "Đảng ủy Phường Giữ Nguyên";
        AppSettingPayload changed = await PutAsync(
            client, new { startYears = 20, endYears = 100, stepYears = 10, unitName });

        Assert.Equal(20, changed.StartYears);

        AppSettingPayload restored = await PostRestoreAsync(client);

        AssertDefaultMilestones(restored);
        Assert.Equal(unitName, restored.UnitName);
        Assert.Equal(unitName, (await ApiClientFactory.GetDataAsync<AppSettingPayload>(client, BasePath)).UnitName);
    }

    [Fact(DisplayName = "7.2 · A-509 · Lưu hai lần liên tiếp vẫn chỉ có đúng một bản ghi cài đặt")]
    public async Task Update_CalledTwice_KeepsExactlyOneRow()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        await PutAsync(client, new { startYears = 30, endYears = 90, stepYears = 10, unitName = "Lần một" });
        AppSettingPayload second = await PutAsync(
            client, new { startYears = 25, endYears = 95, stepYears = 5, unitName = "Lần hai" });

        await PostRestoreAsync(client);

        Assert.Equal("Lần hai", second.UnitName);
        Assert.Equal(1, await CountSettingsAsync());
    }

    [Fact(DisplayName = "7.2 · A-509 · Kho trống: lưu lần đầu tạo đúng một bản ghi, không nhân bản")]
    public async Task Update_OnEmptyStore_CreatesExactlyOneRow()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await DeleteSettingAsync();

        try
        {
            await PutAsync(client, new { startYears = 30, endYears = 90, stepYears = 5 });
            await PutAsync(client, new { startYears = 30, endYears = 90, stepYears = 10 });

            Assert.Equal(1, await CountSettingsAsync());
        }
        finally
        {
            await RestoreSettingRowAsync();
        }
    }

    [Fact(DisplayName = "7.4 · Xem trước dãy mốc không ghi gì xuống cơ sở dữ liệu")]
    public async Task PreviewMilestones_WithExplicitValues_DoesNotTouchStoredSetting()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        MilestonePreviewPayload preview = await ApiClientFactory.GetDataAsync<MilestonePreviewPayload>(
            client, $"{BasePath}/Milestones?start=30&end=90&step=10");

        Assert.Equal(ExpectedMilestones("core_step10_T0"), preview.Milestones);
        Assert.Equal(7, preview.MilestoneCount);

        await AssertUnchangedAsync(client);
    }

    [Fact(DisplayName = "7.4 · Bỏ trống tham số nào thì lấy giá trị đang lưu của tham số đó")]
    public async Task PreviewMilestones_WithoutParameters_FallsBackToStoredSetting()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        MilestonePreviewPayload all = await ApiClientFactory.GetDataAsync<MilestonePreviewPayload>(
            client, $"{BasePath}/Milestones");
        MilestonePreviewPayload stepOnly = await ApiClientFactory.GetDataAsync<MilestonePreviewPayload>(
            client, $"{BasePath}/Milestones?step=10");

        Assert.Equal(ExpectedMilestones("core_default_T0"), all.Milestones);
        Assert.Equal(ExpectedMilestones("core_step10_T0"), stepOnly.Milestones);
    }

    [Fact(DisplayName = "7.4 · Tham số xem trước sai trả cùng bộ khóa lỗi với 7.2")]
    public async Task PreviewMilestones_WithInvalidValues_ReturnsSameErrorKeys()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.SeedAllAsync(factory);

        await ApiClientFactory.AssertErrorAsync(
            await client.GetAsync($"{BasePath}/Milestones?start=90&end=30&step=5"),
            Messages<AppSetting>.Invalid(AppSettingMessageProperties.Range));

        await ApiClientFactory.AssertErrorAsync(
            await client.GetAsync($"{BasePath}/Milestones?start=30&end=90&step=0"),
            Messages<AppSetting>.Invalid(x => x.StepYears));
    }

    [Fact(DisplayName = "7.1 → 7.4 · Chưa đăng nhập thì cả bốn endpoint đều trả 401")]
    public async Task Endpoints_WithoutToken_ReturnUnauthorized()
    {
        HttpClient client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync(BasePath)).StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.PutAsJsonAsync(BasePath, new { startYears = 30, endYears = 90, stepYears = 5 })).StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await client.PostAsync($"{BasePath}/RestoreDefaults", content: null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync($"{BasePath}/Milestones")).StatusCode);
    }

    /// <summary>
    /// Dãy mốc mong đợi của một kịch bản trong <c>expected.json</c>.
    /// </summary>
    /// <param name="scenario">Tên kịch bản.</param>
    /// <returns>Dãy mốc tăng dần.</returns>
    private static List<int> ExpectedMilestones(string scenario)
        => CoreFixtures.Scenario(scenario)
            .GetProperty("milestones")
            .EnumerateArray()
            .Select(x => x.GetInt32())
            .ToList();

    private static int ExpectedTotal(string scenario, string periodCode, int year)
        => CoreFixtures.Scenario(scenario)
            .GetProperty("eligibleByPeriod")
            .GetProperty(periodCode)
            .GetProperty("byYear")
            .GetProperty(year.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .GetProperty("total")
            .GetInt32();

    private static int ExpectedBadge(string scenario)
        => CoreFixtures.Scenario(scenario).GetProperty("badgeCurrentYear").GetInt32();

    private static void AssertDefaultMilestones(AppSettingPayload data)
    {
        Assert.Equal(AppSetting.DefaultStartYears, data.StartYears);
        Assert.Equal(AppSetting.DefaultEndYears, data.EndYears);
        Assert.Equal(AppSetting.DefaultStepYears, data.StepYears);
        Assert.Equal(13, data.MilestoneCount);
        Assert.Equal(13, data.Milestones.Count);
        Assert.Equal(30, data.Milestones[0]);
        Assert.Equal(90, data.Milestones[^1]);
    }

    private static async Task<Guid> FindPeriodIdAsync(HttpClient client, string code)
    {
        string name = CoreFixtures.MainPeriods().Single(x => x.Code == code).Name;

        AwardPeriodListPayload list =
            await ApiClientFactory.GetDataAsync<AwardPeriodListPayload>(client, "/api/AwardPeriods");

        return list.Periods.Single(x => x.Name == name).Id;
    }

    private static async Task<AppSettingPayload> PutAsync(HttpClient client, object body)
    {
        HttpResponseMessage response = await client.PutAsJsonAsync(BasePath, body);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiResponse<AppSettingPayload> payload = await response.ReadApiResponseAsync<AppSettingPayload>();
        Assert.Equal(Messages<AppSetting>.Update(), payload.Message);

        return payload.Data!;
    }

    private static async Task<AppSettingPayload> PostRestoreAsync(HttpClient client)
    {
        HttpResponseMessage response = await client.PostAsync($"{BasePath}/RestoreDefaults", content: null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return (await response.ReadApiResponseAsync<AppSettingPayload>()).Data!;
    }

    /// <summary>
    /// Khẳng định bản ghi cài đặt vẫn đúng bộ <c>default</c> của fixture — dùng sau mỗi yêu cầu
    /// bị từ chối và sau mỗi lần xem trước, để chứng minh không có gì lọt xuống cơ sở dữ liệu.
    /// </summary>
    /// <param name="client">Client đã đăng nhập.</param>
    private static async Task AssertUnchangedAsync(HttpClient client)
    {
        AppSettingPayload data = await ApiClientFactory.GetDataAsync<AppSettingPayload>(client, BasePath);

        AssertDefaultMilestones(data);
        Assert.Equal(CoreFixtures.Settings["default"].UnitName, data.UnitName);
    }

    private async Task DeleteSettingAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Set<AppSetting>().ExecuteDeleteAsync();
    }

    /// <summary>
    /// Dựng lại bản ghi cài đặt sau một bài kiểm thử kho trống, rồi trả dữ liệu về bộ
    /// <c>default</c>. <see cref="CoreDataset.ResetAsync"/> chỉ biết sửa bản ghi có sẵn, nên phải
    /// chèn lại trước khi gọi, nếu không các bài sau sẽ chạy trên một kho không có cài đặt.
    /// </summary>
    private async Task RestoreSettingRowAsync()
    {
        using (IServiceScope scope = factory.Services.CreateScope())
        {
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (!await dbContext.Set<AppSetting>().AnyAsync())
            {
                dbContext.Set<AppSetting>().Add(new AppSetting());
                await dbContext.SaveChangesAsync();
            }
        }

        await CoreDataset.ResetAsync(factory);
    }

    private async Task<int> CountSettingsAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.Set<AppSetting>().CountAsync();
    }
}
