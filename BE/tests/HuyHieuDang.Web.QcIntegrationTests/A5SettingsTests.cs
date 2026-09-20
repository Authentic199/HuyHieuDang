using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HuyHieuDang.Web.QcIntegrationTests.Support;

namespace HuyHieuDang.Web.QcIntegrationTests;

/// <summary>
/// A-501 → A-509 · Cài đặt mốc huy hiệu và tên đơn vị (UC-50, UC-51, QT1). Đổi cài đặt phải làm
/// mọi danh sách đủ điều kiện đổi theo ngay (QT5).
/// </summary>
[Collection(QcApiCollection.Name)]
public sealed class A5SettingsTests
{
    private readonly QcApiFactory factory;

    /// <summary>Initializes a new instance of the <see cref="A5SettingsTests"/> class.</summary>
    /// <param name="factory">Host kiểm thử dùng chung.</param>
    public A5SettingsTests(QcApiFactory factory)
    {
        this.factory = factory;
    }

    /// <summary>A-501 · Kho trống vẫn đọc được cài đặt, trả đúng mặc định 30 / 90 / 5.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A501_Kho_trong_van_tra_cai_dat_mac_dinh()
    {
        await QcDb.ResetAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        await QcApi.PostDataAsync(client, QcEndpoints.SettingsRestoreDefaults, new { });

        JsonElement data = await QcApi.GetDataAsync(client, QcEndpoints.Settings);

        data.Int("startYears").ShouldBe(30);
        data.Int("endYears").ShouldBe(90);
        data.Int("stepYears").ShouldBe(5);

        List<int> milestones = data.Array("milestones").Select(value => value.GetInt32()).ToList();
        milestones.ShouldBe(QcFixtures.Scenario("core_default_T0")
            .Array("milestones").Select(value => value.GetInt32()).ToList());
        data.Int("milestoneCount").ShouldBe(milestones.Count);
    }

    /// <summary>
    /// A-502 · Lưu 30 / 90 / 10: dãy mốc đổi và **mọi** danh sách đủ điều kiện đổi theo ngay,
    /// không cần thao tác nào khác (QT1, QT5).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A502_Doi_buoc_lam_moi_danh_sach_doi_theo_ngay()
    {
        await QcDb.SeedCoreAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        JsonElement period = (await QcApi.GetDataAsync(client, QcEndpoints.AwardPeriods))
            .Array("periods").Single(item => item.Str("name") == "Đợt 7/11");
        string periodId = period.Str("id");

        int before = (await QcApi.GetDataAsync(
            client, $"{QcEndpoints.Eligibility}?awardPeriodId={periodId}&year=2026")).Int("totalCount");
        before.ShouldBe(QcFixtures.Eligible("core_default_T0", "P4", 2026).Int("total"));

        QcSettings step10 = QcFixtures.Settings["step10"];
        JsonElement saved = await QcApi.PutDataAsync(client, QcEndpoints.Settings, new
        {
            startYears = step10.StartYears,
            endYears = step10.EndYears,
            stepYears = step10.StepYears,
            unitName = step10.UnitName,
        });

        saved.Array("milestones").Select(value => value.GetInt32()).ToList()
            .ShouldBe(QcFixtures.Scenario("core_step10_T0").Array("milestones").Select(value => value.GetInt32()).ToList());

        // Danh sách của đợt, Dashboard và badge đều phải đổi ngay.
        JsonElement after = await QcApi.GetDataAsync(
            client, $"{QcEndpoints.Eligibility}?awardPeriodId={periodId}&year=2026");
        after.Int("totalCount").ShouldBe(QcFixtures.Eligible("core_step10_T0", "P4", 2026).Int("total"));

        JsonElement dashboard = await QcApi.GetDataAsync(client, QcEndpoints.Dashboard);
        dashboard.GetProperty("upcomingPeriod").Int("eligibleCount")
            .ShouldBe(QcFixtures.Eligible("core_step10_T0", "P4", 2026).Int("total"));

        JsonElement badge = await QcApi.GetDataAsync(client, QcEndpoints.UnassignedCount);
        badge.Int("count").ShouldBe(QcFixtures.Scenario("core_step10_T0").Int("badgeCurrentYear"));
    }

    /// <summary>A-503 → A-505 · Cài đặt sai bị từ chối bằng đúng khóa thông điệp.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A503_A504_A505_Cai_dat_sai_bi_tu_choi()
    {
        await QcDb.ResetAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        using HttpResponseMessage reversed = await client.PutAsJsonAsync(
            QcEndpoints.Settings, new { startYears = 90, endYears = 30, stepYears = 5, unitName = "Đảng ủy" });
        await QcApi.AssertErrorAsync(reversed, HttpStatusCode.BadRequest, QcMessages.SettingInvalidRange);

        foreach (int step in new[] { 0, -1 })
        {
            using HttpResponseMessage badStep = await client.PutAsJsonAsync(
                QcEndpoints.Settings, new { startYears = 30, endYears = 90, stepYears = step, unitName = "Đảng ủy" });
            await QcApi.AssertErrorAsync(badStep, HttpStatusCode.BadRequest, QcMessages.SettingInvalidStepYears);
        }

        using HttpResponseMessage badStart = await client.PutAsJsonAsync(
            QcEndpoints.Settings, new { startYears = 0, endYears = 90, stepYears = 5, unitName = "Đảng ủy" });
        await QcApi.AssertErrorAsync(badStart, HttpStatusCode.BadRequest, QcMessages.SettingInvalidStartYears);

        // Giá trị không phải số và thân rỗng: phải là 400 tiếng nói được, không phải 500.
        foreach (string body in new[]
                 {
                     "{\"startYears\":\"ba mươi\",\"endYears\":90,\"stepYears\":5}",
                     "{}",
                 })
        {
            using StringContent content = new(body, System.Text.Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await client.PutAsync(QcEndpoints.Settings, content);

            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, body);
            QcMessages.ShouldNotLeakInternals(await response.Content.ReadAsStringAsync());
        }

        // Cài đặt vẫn nguyên vẹn sau chuỗi lời gọi sai.
        JsonElement current = await QcApi.GetDataAsync(client, QcEndpoints.Settings);
        current.Int("startYears").ShouldBe(QcFixtures.Settings["default"].StartYears);
        current.Int("stepYears").ShouldBe(QcFixtures.Settings["default"].StepYears);
    }

    /// <summary>A-506 · Khôi phục mặc định đưa mốc về 30 / 90 / 5 và **không** đụng tên đơn vị.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A506_Khoi_phuc_mac_dinh_khong_dung_ten_don_vi()
    {
        await QcDb.ResetAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        await QcApi.PutDataAsync(
            client, QcEndpoints.Settings,
            new { startYears = 20, endYears = 100, stepYears = 10, unitName = "Đảng ủy Phường Đổi Tên" });

        JsonElement restored = await QcApi.PostDataAsync(client, QcEndpoints.SettingsRestoreDefaults, new { });

        restored.Int("startYears").ShouldBe(30);
        restored.Int("endYears").ShouldBe(90);
        restored.Int("stepYears").ShouldBe(5);
        restored.StringOrNull("unitName").ShouldBe("Đảng ủy Phường Đổi Tên");
    }

    /// <summary>
    /// A-507 và A-508 · Tên đơn vị hiện ở phiên đăng nhập; để trống vẫn hợp lệ. Phần tiêu đề
    /// file Excel do ca A-704 và A-705 kiểm.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A507_A508_Ten_don_vi_luu_duoc_va_de_trong_duoc()
    {
        await QcDb.ResetAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        await QcApi.PutDataAsync(
            client, QcEndpoints.Settings,
            new { startYears = 30, endYears = 90, stepYears = 5, unitName = "Đảng ủy Phường Kiểm Thử" });

        (await QcApi.GetDataAsync(client, QcEndpoints.Me)).StringOrNull("unitName")
            .ShouldBe("Đảng ủy Phường Kiểm Thử");
        (await QcApi.GetDataAsync(client, QcEndpoints.Dashboard)).StringOrNull("unitName")
            .ShouldBe("Đảng ủy Phường Kiểm Thử");

        JsonElement blank = await QcApi.PutDataAsync(
            client, QcEndpoints.Settings,
            new { startYears = 30, endYears = 90, stepYears = 5, unitName = (string?)null });

        blank.StringOrNull("unitName").ShouldBeNull();
        (await QcApi.GetDataAsync(client, QcEndpoints.Me)).StringOrNull("unitName").ShouldBeNull();
    }

    /// <summary>A-509 · Gọi lưu hai lần liên tiếp vẫn chỉ có đúng một bản ghi cài đặt.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A509_Luu_hai_lan_van_chi_mot_ban_ghi_cai_dat()
    {
        await QcDb.ResetAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        (await QcDb.CountSettingsAsync(factory)).ShouldBe(1);

        for (int time = 0; time < 2; time++)
        {
            await QcApi.PutDataAsync(
                client, QcEndpoints.Settings,
                new { startYears = 25, endYears = 95, stepYears = 5, unitName = "Đảng ủy Lặp" });
        }

        (await QcDb.CountSettingsAsync(factory)).ShouldBe(1);

        JsonElement current = await QcApi.GetDataAsync(client, QcEndpoints.Settings);
        current.Int("startYears").ShouldBe(25);
        current.Int("endYears").ShouldBe(95);
    }

    /// <summary>
    /// A-510 · Xem trước dãy mốc (mục 7.4 hợp đồng API): không ghi gì vào cơ sở dữ liệu và
    /// từ chối tham số sai giống endpoint lưu.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A510_Xem_truoc_day_moc_khong_ghi_gi_vao_co_so_du_lieu()
    {
        await QcDb.ResetAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        JsonElement preview = await QcApi.GetDataAsync(
            client, $"{QcEndpoints.SettingsMilestones}?start=30&end=90&step=10");

        preview.Array("milestones").Select(value => value.GetInt32()).ToList()
            .ShouldBe(QcFixtures.Scenario("core_step10_T0").Array("milestones").Select(value => value.GetInt32()).ToList());
        preview.Int("milestoneCount").ShouldBe(7);

        // Bỏ trống tham số thì lấy giá trị đang lưu.
        JsonElement stored = await QcApi.GetDataAsync(client, QcEndpoints.SettingsMilestones);
        stored.Int("milestoneCount").ShouldBe(QcFixtures.Scenario("core_default_T0").Int("milestoneCount"));

        // Cài đặt đang lưu không đổi sau khi xem trước.
        JsonElement settings = await QcApi.GetDataAsync(client, QcEndpoints.Settings);
        settings.Int("stepYears").ShouldBe(QcFixtures.Settings["default"].StepYears);

        using HttpResponseMessage invalid = await client.GetAsync(
            $"{QcEndpoints.SettingsMilestones}?start=90&end=30&step=5");
        await QcApi.AssertErrorAsync(invalid, HttpStatusCode.BadRequest, QcMessages.SettingInvalidRange);
    }

    /// <summary>
    /// A-511 · Đăng xuất (mục 2.2 hợp đồng API): trả 200 và không hủy token phía máy chủ —
    /// JWT không trạng thái, việc xóa token là của Frontend.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A511_Dang_xuat_tra_200_va_khong_huy_token_phia_may_chu()
    {
        using HttpClient client = await QcApi.LoginAsync(factory);

        using HttpResponseMessage logout = await client.PostAsync(QcEndpoints.Logout, content: null);
        logout.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await QcApi.ReadAsync(logout)).Str("message").ShouldBe("Mes.User.Logout.Successfully");

        using HttpResponseMessage me = await client.GetAsync(QcEndpoints.Me);
        me.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
