using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HuyHieuDang.Web.QcIntegrationTests.Support;

namespace HuyHieuDang.Web.QcIntegrationTests;

/// <summary>
/// A-801 → A-824 · Ghi chú đảng viên (QT12, UC-26; hợp đồng API v1.6 mục 1.10 và 3.6).
/// <para>
/// Ba đường ghi ghi chú — <c>POST /api/PartyMembers</c>, <c>PUT /api/PartyMembers/{id}</c> và
/// <c>PUT /api/PartyMembers/{id}/Note</c> — phải cho cùng một hành vi, nên mỗi quy tắc biên đều
/// được kiểm trên cả ba nơi có thể chạm tới nó.
/// </para>
/// <para>
/// Ngày ghi đối chiếu bằng <b>chuỗi tuyệt đối</b>, lấy từ ba mốc đóng băng của
/// <see cref="QcClock"/>: viết ghi chú qua client ở mốc T0 thì ngày ghi phải đúng
/// <c>2026-09-19T09:30:00+07:00</c>. Nhờ vậy ca không phụ thuộc ngày chạy và không cần
/// so sánh "gần bằng bây giờ".
/// </para>
/// </summary>
[Collection(QcApiCollection.Name)]
public sealed class A11NoteTests
{
    /// <summary>Trần ký tự của QT12, đếm sau khi máy chủ cắt khoảng trắng hai đầu.</summary>
    private const int MaxLength = 500;

    /// <summary>Người của bộ lõi dùng làm đích cho mọi ca chỉ cần "một đảng viên bất kỳ".</summary>
    private const string ProbeMemberName = "Nguyễn Văn An";

    /// <summary>Ghi chú mẫu dùng xuyên suốt; giữ dấu xuống dòng để kiểm chữ nhiều dòng.</summary>
    private const string SampleNote = "Sức khỏe yếu, con trai nhận thay.\nĐã hẹn lại ngày 05/10.";

    /// <summary>
    /// Dấu hiệu riêng để soi ba file Excel xuất ra: nội dung này không được lọt vào file nào
    /// (hợp đồng v1.6 chốt Excel không có cột ghi chú).
    /// </summary>
    private const string ExportProbe = "GHICHU-KHONG-DUOC-XUAT-EXCEL";

    private readonly QcApiFactory factory;

    /// <summary>Initializes a new instance of the <see cref="A11NoteTests"/> class.</summary>
    /// <param name="factory">Host kiểm thử dùng chung.</param>
    public A11NoteTests(QcApiFactory factory)
    {
        this.factory = factory;
    }

    /// <summary>Ngày ghi mong đợi khi ghi chú được lưu bởi client ở mốc đã cho.</summary>
    /// <param name="today">Mốc đang ép.</param>
    /// <returns>Chuỗi ISO kèm độ lệch <c>+07:00</c> đúng khuôn mục 1.6 hợp đồng API.</returns>
    private static string WrittenAt(DateOnly today)
        => $"{today:yyyy-MM-dd}T09:30:00+07:00";

    /// <summary>
    /// A-801 · Ghi chú đúng 500 ký tự được lưu trọn vẹn — biên trên còn hợp lệ.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-801 · QT12 · Ghi chú 500 ký tự được lưu")]
    public async Task A801_Ghi_chu_500_ky_tu_duoc_luu()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        string id = await ProbeMemberIdAsync(client);
        string note = new('a', MaxLength);

        JsonElement saved = await QcApi.PutDataAsync(client, QcEndpoints.PartyMemberNote(new Guid(id)), new { note });

        saved.Str("note").Length.ShouldBe(MaxLength);
        saved.Str("note").ShouldBe(note);
        saved.Str("noteUpdatedAt").ShouldBe(WrittenAt(QcClock.T0));
    }

    /// <summary>
    /// A-802 · Ghi chú 501 ký tự bị chặn bằng 400 và khóa <c>Mes.PartyMember.OverLength.Note</c>
    /// ở **cả ba** đường ghi.
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-802 · QT12 · Ghi chú 501 ký tự bị chặn ở cả ba đường ghi")]
    public async Task A802_Ghi_chu_501_ky_tu_bi_chan()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        JsonElement member = await ProbeMemberAsync(client);
        string tooLong = new('a', MaxLength + 1);

        using HttpResponseMessage viaNote = await client.PutAsJsonAsync(
            QcEndpoints.PartyMemberNote(new Guid(member.Str("id"))), new { note = tooLong });
        await QcApi.AssertErrorAsync(viaNote, HttpStatusCode.BadRequest, QcMessages.MemberOverLengthNote);

        using HttpResponseMessage viaUpdate = await client.PutAsJsonAsync(
            $"{QcEndpoints.PartyMembers}/{member.Str("id")}",
            new
            {
                fullName = member.Str("fullName"),
                dateOfBirth = member.StringOrNull("dateOfBirth"),
                gender = member.StringOrNull("gender"),
                officialAdmissionDate = member.Str("officialAdmissionDate"),
                note = tooLong,
            });
        await QcApi.AssertErrorAsync(viaUpdate, HttpStatusCode.BadRequest, QcMessages.MemberOverLengthNote);

        using HttpResponseMessage viaCreate = await client.PostAsJsonAsync(
            QcEndpoints.PartyMembers,
            new
            {
                fullName = "Kiểm Thử Ghi Chú Quá Dài",
                officialAdmissionDate = "1996-10-01",
                note = tooLong,
            });
        await QcApi.AssertErrorAsync(viaCreate, HttpStatusCode.BadRequest, QcMessages.MemberOverLengthNote);

        // Ghi chú bị chặn thì không được để lại dấu vết nào: bản ghi cũ nguyên vẹn, người mới
        // không được tạo.
        JsonElement unchanged = await QcApi.GetDataAsync(client, $"{QcEndpoints.PartyMembers}/{member.Str("id")}");
        unchanged.StringOrNull("note").ShouldBeNull();
        (await QcDb.CountMembersAsync(factory)).ShouldBe(QcFixtures.CoreMembers.Count);
    }

    /// <summary>
    /// A-803 · Ghi chú toàn khoảng trắng — kể cả tab và xuống dòng — lưu thành <c>null</c>,
    /// ngày ghi cũng <c>null</c> (QT12.2).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-803 · QT12 · Ghi chú toàn khoảng trắng lưu null")]
    public async Task A803_Ghi_chu_toan_khoang_trang_luu_null()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        string url = QcEndpoints.PartyMemberNote(new Guid(await ProbeMemberIdAsync(client)));

        foreach (string blank in new[] { "   ", "\t\t", "\n\n", " \t\r\n " })
        {
            JsonElement saved = await QcApi.PutDataAsync(client, url, new { note = blank });

            saved.StringOrNull("note").ShouldBeNull($"'{blank.Replace("\n", "\\n", StringComparison.Ordinal)}'");
            saved.StringOrNull("noteUpdatedAt").ShouldBeNull();
        }
    }

    /// <summary>
    /// A-804 · Khoảng trắng hai đầu bị cắt, còn xuống dòng bên trong được giữ nguyên (QT12.2).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-804 · QT12 · Cắt khoảng trắng hai đầu, giữ xuống dòng bên trong")]
    public async Task A804_Cat_khoang_trang_hai_dau_giu_xuong_dong()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        string url = QcEndpoints.PartyMemberNote(new Guid(await ProbeMemberIdAsync(client)));

        JsonElement saved = await QcApi.PutDataAsync(client, url, new { note = $"  \n{SampleNote}\t  " });

        saved.Str("note").ShouldBe(SampleNote);
        saved.Str("note").ShouldContain("\n");
    }

    /// <summary>
    /// A-805 · 500 ký tự kèm khoảng trắng hai đầu vẫn hợp lệ — trần đếm **sau khi** cắt, không
    /// đếm chuỗi thô người dùng gửi lên (QT12.2).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-805 · QT12 · Trần 500 đếm sau khi cắt khoảng trắng")]
    public async Task A805_Tran_500_dem_sau_khi_cat()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        string url = QcEndpoints.PartyMemberNote(new Guid(await ProbeMemberIdAsync(client)));
        string note = new('a', MaxLength);

        JsonElement saved = await QcApi.PutDataAsync(client, url, new { note = $"          {note}          " });

        saved.Str("note").Length.ShouldBe(MaxLength);
    }

    /// <summary>
    /// A-810 · Từ không có ghi chú sang có: ngày ghi bằng thời điểm hiện tại của máy chủ, trả
    /// kèm độ lệch <c>+07:00</c> chứ không phải <c>Z</c> (QT12.3, mục 1.6 hợp đồng API).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-810 · QT12 · Thêm ghi chú đóng ngày ghi kèm +07:00")]
    public async Task A810_Them_ghi_chu_dong_ngay_ghi()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        string id = await ProbeMemberIdAsync(client);

        JsonElement saved = await QcApi.PutDataAsync(
            client, QcEndpoints.PartyMemberNote(new Guid(id)), new { note = SampleNote });

        saved.Str("noteUpdatedAt").ShouldBe(WrittenAt(QcClock.T0));
        saved.Str("noteUpdatedAt").ShouldEndWith("+07:00");

        // Đọc lại từ máy chủ: giá trị đi qua cột timestamptz rồi về vẫn phải ra đúng chuỗi đó —
        // đây là chỗ một bộ chuyển đổi quy về UTC sẽ làm ghi chú viết lúc 00:00–07:00 lùi một ngày.
        JsonElement reloaded = await QcApi.GetDataAsync(client, $"{QcEndpoints.PartyMembers}/{id}");
        reloaded.Str("noteUpdatedAt").ShouldBe(WrittenAt(QcClock.T0));
    }

    /// <summary>
    /// A-811 · Đổi nội dung ghi chú thì ngày ghi nhảy sang thời điểm mới (QT12.3).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-811 · QT12 · Đổi nội dung thì ngày ghi đổi theo")]
    public async Task A811_Doi_noi_dung_thi_ngay_ghi_doi()
    {
        using HttpClient atT0 = await SeedCoreAndLoginAsync();
        string id = await ProbeMemberIdAsync(atT0);
        string url = QcEndpoints.PartyMemberNote(new Guid(id));

        JsonElement first = await QcApi.PutDataAsync(atT0, url, new { note = SampleNote });
        first.Str("noteUpdatedAt").ShouldBe(WrittenAt(QcClock.T0));

        using HttpClient atT1 = await QcApi.LoginAsync(factory, QcClock.T1);
        JsonElement second = await QcApi.PutDataAsync(atT1, url, new { note = "Hồ sơ đã bổ sung đủ." });

        second.Str("note").ShouldBe("Hồ sơ đã bổ sung đủ.");
        second.Str("noteUpdatedAt").ShouldBe(WrittenAt(QcClock.T1));
    }

    /// <summary>
    /// A-812 · Lưu lại **y hệt** nội dung cũ thì ngày ghi giữ nguyên, dù máy chủ đã sang mốc
    /// khác (QT12.3).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-812 · QT12 · Lưu lại y hệt thì ngày ghi giữ nguyên")]
    public async Task A812_Luu_lai_y_het_thi_ngay_ghi_giu_nguyen()
    {
        using HttpClient atT0 = await SeedCoreAndLoginAsync();
        string id = await ProbeMemberIdAsync(atT0);
        string url = QcEndpoints.PartyMemberNote(new Guid(id));

        await QcApi.PutDataAsync(atT0, url, new { note = SampleNote });

        using HttpClient atT2 = await QcApi.LoginAsync(factory, QcClock.T2);

        // Gửi lại đúng nội dung cũ, và gửi cả bản chỉ khác ở khoảng trắng hai đầu: cắt xong hai
        // bản bằng nhau nên cả hai lần đều không được đóng ngày mới.
        foreach (string same in new[] { SampleNote, $"  {SampleNote}  " })
        {
            JsonElement saved = await QcApi.PutDataAsync(atT2, url, new { note = same });

            saved.Str("note").ShouldBe(SampleNote);
            saved.Str("noteUpdatedAt").ShouldBe(WrittenAt(QcClock.T0), "ngày ghi nhảy dù nội dung không đổi");
        }
    }

    /// <summary>
    /// A-813 · Sửa trường khác của đảng viên mà gửi lại đúng ghi chú cũ thì ngày ghi giữ
    /// nguyên (QT12.3).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-813 · QT12 · Sửa trường khác, ghi chú không đổi thì ngày ghi giữ nguyên")]
    public async Task A813_Sua_truong_khac_thi_ngay_ghi_giu_nguyen()
    {
        using HttpClient atT0 = await SeedCoreAndLoginAsync();
        JsonElement member = await ProbeMemberAsync(atT0);
        string id = member.Str("id");

        await QcApi.PutDataAsync(atT0, QcEndpoints.PartyMemberNote(new Guid(id)), new { note = SampleNote });

        using HttpClient atT1 = await QcApi.LoginAsync(factory, QcClock.T1);
        JsonElement saved = await QcApi.PutDataAsync(
            atT1,
            $"{QcEndpoints.PartyMembers}/{id}",
            new
            {
                fullName = "Họ Tên Đã Sửa",
                dateOfBirth = member.StringOrNull("dateOfBirth"),
                gender = member.StringOrNull("gender"),
                officialAdmissionDate = member.Str("officialAdmissionDate"),
                note = SampleNote,
            });

        saved.Str("fullName").ShouldBe("Họ Tên Đã Sửa");
        saved.Str("note").ShouldBe(SampleNote);
        saved.Str("noteUpdatedAt").ShouldBe(WrittenAt(QcClock.T0));
    }

    /// <summary>
    /// A-814 · Xóa ghi chú thì ghi chú và ngày ghi **cùng** về <c>null</c>; xóa xong ghi lại thì
    /// ngày ghi là thời điểm ghi lại (QT12.3).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-814 · QT12 · Xóa ghi chú đưa cả ngày ghi về null")]
    public async Task A814_Xoa_ghi_chu_dua_ca_ngay_ghi_ve_null()
    {
        using HttpClient atT0 = await SeedCoreAndLoginAsync();
        string id = await ProbeMemberIdAsync(atT0);
        string url = QcEndpoints.PartyMemberNote(new Guid(id));

        await QcApi.PutDataAsync(atT0, url, new { note = SampleNote });

        JsonElement cleared = await QcApi.PutDataAsync(atT0, url, new { note = (string?)null });
        cleared.StringOrNull("note").ShouldBeNull();
        cleared.StringOrNull("noteUpdatedAt").ShouldBeNull();

        JsonElement reloaded = await QcApi.GetDataAsync(atT0, $"{QcEndpoints.PartyMembers}/{id}");
        reloaded.StringOrNull("note").ShouldBeNull();
        reloaded.StringOrNull("noteUpdatedAt").ShouldBeNull();

        using HttpClient atT1 = await QcApi.LoginAsync(factory, QcClock.T1);
        JsonElement again = await QcApi.PutDataAsync(atT1, url, new { note = SampleNote });
        again.Str("noteUpdatedAt").ShouldBe(WrittenAt(QcClock.T1));
    }

    /// <summary>
    /// A-815 · <c>PUT /api/PartyMembers/{id}</c> thay trọn: không gửi <c>note</c> nghĩa là
    /// **xóa** ghi chú, không phải giữ nguyên (mục 3.4 hợp đồng v1.6).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-815 · Hợp đồng 3.4 · PUT không gửi note là xóa ghi chú")]
    public async Task A815_Put_khong_gui_note_la_xoa_ghi_chu()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        JsonElement member = await ProbeMemberAsync(client);
        string id = member.Str("id");

        await QcApi.PutDataAsync(client, QcEndpoints.PartyMemberNote(new Guid(id)), new { note = SampleNote });

        JsonElement saved = await QcApi.PutDataAsync(
            client,
            $"{QcEndpoints.PartyMembers}/{id}",
            new
            {
                fullName = member.Str("fullName"),
                dateOfBirth = member.StringOrNull("dateOfBirth"),
                gender = member.StringOrNull("gender"),
                officialAdmissionDate = member.Str("officialAdmissionDate"),
            });

        saved.StringOrNull("note").ShouldBeNull();
        saved.StringOrNull("noteUpdatedAt").ShouldBeNull();
    }

    /// <summary>
    /// A-816 · Thêm người kèm ghi chú thì ngày ghi được đóng ngay; thêm người không kèm ghi chú
    /// thì cả hai trường là <c>null</c> (mục 3.3 hợp đồng v1.6).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-816 · Hợp đồng 3.3 · Thêm người kèm ghi chú đóng ngày ghi ngay")]
    public async Task A816_Them_nguoi_kem_ghi_chu()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        JsonElement withNote = await QcApi.PostDataAsync(
            client,
            QcEndpoints.PartyMembers,
            new { fullName = "Kiểm Thử Có Ghi Chú", officialAdmissionDate = "1996-10-01", note = $"  {SampleNote}  " });

        withNote.Str("note").ShouldBe(SampleNote);
        withNote.Str("noteUpdatedAt").ShouldBe(WrittenAt(QcClock.T0));

        JsonElement withoutNote = await QcApi.PostDataAsync(
            client,
            QcEndpoints.PartyMembers,
            new { fullName = "Kiểm Thử Không Ghi Chú", officialAdmissionDate = "1996-10-01" });

        withoutNote.StringOrNull("note").ShouldBeNull();
        withoutNote.StringOrNull("noteUpdatedAt").ShouldBeNull();
    }

    /// <summary>
    /// A-820 · <c>PUT /api/PartyMembers/{id}/Note</c> chỉ đụng ghi chú: bốn trường còn lại giữ
    /// nguyên, và trường lạ gửi kèm trong thân yêu cầu bị bỏ qua (mục 3.6 hợp đồng v1.6).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-820 · UC-26 · Lưu ghi chú không đụng bốn trường còn lại")]
    public async Task A820_Luu_ghi_chu_khong_dung_truong_khac()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        JsonElement before = await ProbeMemberAsync(client);
        string id = before.Str("id");

        JsonElement after = await QcApi.PutDataAsync(
            client,
            QcEndpoints.PartyMemberNote(new Guid(id)),
            new
            {
                note = SampleNote,

                // Trường lạ: endpoint không được nhận chúng làm lệnh sửa đảng viên.
                fullName = "Tên Kẻ Tấn Công",
                dateOfBirth = "1900-01-01",
                gender = "Female",
                officialAdmissionDate = "1900-01-02",
            });

        foreach (string field in new[] { "fullName", "dateOfBirth", "gender", "officialAdmissionDate" })
        {
            after.StringOrNull(field).ShouldBe(before.StringOrNull(field), field);
        }

        after.Int("partyAgeYears").ShouldBe(before.Int("partyAgeYears"));
        after.IntOrNull("nextMilestone").ShouldBe(before.IntOrNull("nextMilestone"));
        after.StringOrNull("nextMilestoneDate").ShouldBe(before.StringOrNull("nextMilestoneDate"));
        after.Str("note").ShouldBe(SampleNote);
    }

    /// <summary>
    /// A-821 · Lưu ghi chú cho id không tồn tại trả lỗi nghiệp vụ
    /// <c>Mes.PartyMember.NotFound</c>, không phải 500 (mục 3.6 hợp đồng v1.6).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-821 · UC-26 · Id lạ trả Mes.PartyMember.NotFound")]
    public async Task A821_Id_la_tra_khong_tim_thay()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        using HttpResponseMessage response = await client.PutAsJsonAsync(
            QcEndpoints.PartyMemberNote(Guid.NewGuid()), new { note = SampleNote });

        await QcApi.AssertErrorAsync(response, HttpStatusCode.BadRequest, QcMessages.MemberNotFound);
    }

    /// <summary>
    /// A-822 · Gọi endpoint lưu ghi chú khi chưa đăng nhập bị từ chối bằng 401 và không ghi gì
    /// vào kho (mục 1.2 hợp đồng API).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-822 · UC-26 · Chưa đăng nhập thì 401 và không ghi gì")]
    public async Task A822_Chua_dang_nhap_tra_401()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        string id = await ProbeMemberIdAsync(client);

        using HttpClient anonymous = QcApi.Anonymous(factory);
        using HttpResponseMessage response = await anonymous.PutAsJsonAsync(
            QcEndpoints.PartyMemberNote(new Guid(id)), new { note = SampleNote });

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        JsonElement unchanged = await QcApi.GetDataAsync(client, $"{QcEndpoints.PartyMembers}/{id}");
        unchanged.StringOrNull("note").ShouldBeNull();
    }

    /// <summary>
    /// A-823 · Thân rỗng <c>{}</c> cũng là xóa ghi chú, và lưu ghi chú báo thành công bằng khóa
    /// có sẵn <c>Mes.PartyMember.Update.Successfully</c> (mục 3.6 hợp đồng v1.6).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-823 · UC-26 · Thân rỗng là xóa; báo thành công bằng khóa sửa")]
    public async Task A823_Than_rong_la_xoa_va_khoa_bao_thanh_cong()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();
        string url = QcEndpoints.PartyMemberNote(new Guid(await ProbeMemberIdAsync(client)));

        using HttpResponseMessage set = await client.PutAsJsonAsync(url, new { note = SampleNote });
        set.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await QcApi.ReadAsync(set)).Str("message").ShouldBe(QcMessages.MemberUpdateSuccessfully);

        using HttpResponseMessage cleared = await client.PutAsJsonAsync(url, new { });
        cleared.StatusCode.ShouldBe(HttpStatusCode.OK);

        JsonElement data = (await QcApi.ReadAsync(cleared)).GetProperty("data");
        data.StringOrNull("note").ShouldBeNull();
        data.StringOrNull("noteUpdatedAt").ShouldBeNull();
    }

    /// <summary>
    /// A-830 · <c>GET /api/Dashboard</c> và <c>GET /api/Eligibility</c> trả đúng ghi chú của
    /// đúng người; người chưa ghi chú vẫn có hai trường ở dạng <c>null</c> (UC-11, UC-34).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-830 · UC-11, UC-34 · Dashboard và Eligibility trả đúng ghi chú của đúng người")]
    public async Task A830_Dashboard_va_Eligibility_tra_dung_ghi_chu()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        JsonElement dashboard = await QcApi.GetDataAsync(client, QcEndpoints.Dashboard);
        IReadOnlyList<JsonElement> eligible = dashboard.Array("eligibleMembers");
        eligible.Count.ShouldBeGreaterThan(1, "bộ lõi phải có nhiều hơn một người đủ điều kiện ở T0");

        string firstId = eligible[0].Str("partyMemberId");
        string firstName = eligible[0].Str("fullName");
        string secondId = eligible[1].Str("partyMemberId");
        string noteOfFirst = $"Ghi chú của {firstName}.";

        await QcApi.PutDataAsync(client, QcEndpoints.PartyMemberNote(new Guid(firstId)), new { note = noteOfFirst });

        // Dashboard
        JsonElement afterDashboard = await QcApi.GetDataAsync(client, QcEndpoints.Dashboard);
        IReadOnlyList<JsonElement> rows = afterDashboard.Array("eligibleMembers");

        rows.Single(row => row.Str("partyMemberId") == firstId).Str("note").ShouldBe(noteOfFirst);
        rows.Single(row => row.Str("partyMemberId") == firstId).Str("noteUpdatedAt")
            .ShouldBe(WrittenAt(QcClock.T0));
        rows.Single(row => row.Str("partyMemberId") == secondId).StringOrNull("note").ShouldBeNull();
        rows.Single(row => row.Str("partyMemberId") == secondId).StringOrNull("noteUpdatedAt").ShouldBeNull();
        rows.Count(row => row.StringOrNull("note") is not null).ShouldBe(1);

        // Eligibility của đúng đợt sắp tới và năm đang xem
        string periodId = afterDashboard.GetProperty("upcomingPeriod").Str("id");
        int year = afterDashboard.GetProperty("upcomingPeriod").Int("year");

        JsonElement eligibility = await QcApi.GetDataAsync(
            client, $"{QcEndpoints.Eligibility}?awardPeriodId={periodId}&year={year}");
        IReadOnlyList<JsonElement> members = eligibility.Array("members");

        members.Single(row => row.Str("partyMemberId") == firstId).Str("note").ShouldBe(noteOfFirst);
        members.Count(row => row.StringOrNull("note") is not null).ShouldBe(1);

        // Màn "Chưa thuộc đợt nào" không hiện ghi chú, nhưng hợp đồng chốt hai trường vẫn có mặt
        // trong `UnassignedMemberResponse` — giao diện là nơi bỏ qua, không phải máy chủ.
        JsonElement unassigned = await QcApi.GetDataAsync(client, $"{QcEndpoints.Unassigned}?year={year}");
        foreach (JsonElement row in unassigned.Array("members"))
        {
            row.TryGetProperty("note", out _).ShouldBeTrue("UnassignedMemberResponse thiếu trường note");
            row.TryGetProperty("noteUpdatedAt", out _)
                .ShouldBeTrue("UnassignedMemberResponse thiếu trường noteUpdatedAt");
        }
    }

    /// <summary>
    /// A-831 · Ghi chú không tham gia QT1–QT11: thêm, sửa hay xóa ghi chú không đổi tuổi đảng,
    /// mốc kế tiếp, danh sách đủ điều kiện, badge hay cảnh báo (quyết định của CEO ở HUYH-82).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-831 · QT12 · Ghi chú không đổi bất cứ con số nghiệp vụ nào")]
    public async Task A831_Ghi_chu_khong_doi_con_so_nghiep_vu()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        string before = await ReadBusinessNumbersAsync(client);
        string id = (await ProbeMemberAsync(client)).Str("id");
        string url = QcEndpoints.PartyMemberNote(new Guid(id));

        await QcApi.PutDataAsync(client, url, new { note = SampleNote });
        (await ReadBusinessNumbersAsync(client)).ShouldBe(before, "thêm ghi chú làm đổi con số nghiệp vụ");

        await QcApi.PutDataAsync(client, url, new { note = "Nội dung khác." });
        (await ReadBusinessNumbersAsync(client)).ShouldBe(before, "sửa ghi chú làm đổi con số nghiệp vụ");

        await QcApi.PutDataAsync(client, url, new { note = (string?)null });
        (await ReadBusinessNumbersAsync(client)).ShouldBe(before, "xóa ghi chú làm đổi con số nghiệp vụ");
    }

    /// <summary>
    /// A-840 · Nạp Excel thì người mới có <c>note</c> và <c>noteUpdatedAt</c> đều <c>null</c>;
    /// file mẫu vẫn đúng 4 cột, không có cột ghi chú (mục 4 hợp đồng v1.6).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-840 · QT9 + QT12 · Người được nạp bằng Excel có ghi chú rỗng")]
    public async Task A840_Nguoi_nap_bang_excel_co_ghi_chu_rong()
    {
        await QcDb.ResetAsync(factory);
        using HttpClient client = await QcApi.LoginAsync(factory);

        using HttpResponseMessage commit = await QcUpload.SendFileAsync(
            client, QcEndpoints.ImportCommit, QcPaths.Excel("core-hop-le.xlsx"));
        commit.StatusCode.ShouldBe(HttpStatusCode.OK, await commit.Content.ReadAsStringAsync());

        JsonElement list = await QcApi.GetDataAsync(client, $"{QcEndpoints.PartyMembers}?pageSize=100");
        IReadOnlyList<JsonElement> rows = list.Array("pagedData");

        rows.Count.ShouldBe(QcFixtures.CoreMembers.Count);
        rows.ShouldAllBe(row => row.StringOrNull("note") == null);
        rows.ShouldAllBe(row => row.StringOrNull("noteUpdatedAt") == null);

        // File mẫu: đúng 4 cột, không có cột ghi chú.
        using HttpResponseMessage template = await client.GetAsync(QcEndpoints.ImportTemplate);
        template.StatusCode.ShouldBe(HttpStatusCode.OK);

        QcWorkbook workbook = QcWorkbook.Read(await template.Content.ReadAsByteArrayAsync());
        for (int row = 1; row <= workbook.RowCount; row++)
        {
            workbook.RowText(row).ShouldNotContain("Ghi chú", Case.Insensitive, $"dòng {row} của file mẫu");
        }
    }

    /// <summary>
    /// A-841 · Ba file Excel xuất ra không có cột ghi chú và không mang nội dung ghi chú
    /// (mục 4 và mục 8 hợp đồng v1.6).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact(DisplayName = "A-841 · UC-11, UC-34, UC-40 · Ba file Excel xuất ra không có cột ghi chú")]
    public async Task A841_Ba_file_excel_khong_co_cot_ghi_chu()
    {
        using HttpClient client = await SeedCoreAndLoginAsync();

        // Ghi chú cho TẤT CẢ đảng viên: như vậy dù dòng nào rơi vào file nào, dấu hiệu cũng
        // phải hiện ra nếu có cột ghi chú lọt vào.
        JsonElement list = await QcApi.GetDataAsync(client, $"{QcEndpoints.PartyMembers}?pageSize=100");
        foreach (JsonElement row in list.Array("pagedData"))
        {
            await QcApi.PutDataAsync(
                client,
                QcEndpoints.PartyMemberNote(new Guid(row.Str("id"))),
                new { note = $"{ExportProbe} — {row.Str("fullName")}" });
        }

        JsonElement dashboard = await QcApi.GetDataAsync(client, QcEndpoints.Dashboard);
        string periodId = dashboard.GetProperty("upcomingPeriod").Str("id");
        int year = dashboard.GetProperty("upcomingPeriod").Int("year");

        foreach ((string label, string url) in new[]
                 {
                     ("Dashboard", QcEndpoints.ExportDashboard),
                     ("Chi tiết đợt", $"{QcEndpoints.ExportEligibility}?awardPeriodId={periodId}&year={year}"),
                     ("Chưa thuộc đợt nào", $"{QcEndpoints.ExportUnassigned}?year={year}"),
                 })
        {
            using HttpResponseMessage response = await client.GetAsync(url);
            response.StatusCode.ShouldBe(HttpStatusCode.OK, label);

            QcWorkbook workbook = QcWorkbook.Read(await response.Content.ReadAsByteArrayAsync());
            workbook.RowCount.ShouldBeGreaterThan(0, label);

            for (int row = 1; row <= workbook.RowCount; row++)
            {
                workbook.RowText(row)
                    .ShouldNotContain(ExportProbe, Case.Sensitive, $"{label} — dòng {row} mang nội dung ghi chú");
                workbook.Row(row)
                    .ShouldNotContain("Ghi chú", $"{label} — dòng {row} có ô tiêu đề Ghi chú");
            }
        }
    }

    /// <summary>
    /// Bộ con số nghiệp vụ dùng cho A-831: tuổi đảng và mốc kế tiếp của cả bảng, danh sách đủ
    /// điều kiện của đợt sắp tới, badge người bị sót và các cảnh báo của Dashboard.
    /// </summary>
    /// <param name="client">Client đã đăng nhập.</param>
    /// <returns>Chuỗi gộp mọi con số, so sánh nguyên khối.</returns>
    private static async Task<string> ReadBusinessNumbersAsync(HttpClient client)
    {
        JsonElement dashboard = await QcApi.GetDataAsync(client, QcEndpoints.Dashboard);
        JsonElement upcoming = dashboard.GetProperty("upcomingPeriod");
        JsonElement warnings = dashboard.GetProperty("warnings");
        int year = upcoming.Int("year");

        JsonElement members = await QcApi.GetDataAsync(client, $"{QcEndpoints.PartyMembers}?pageSize=100");
        JsonElement unassigned = await QcApi.GetDataAsync(client, $"{QcEndpoints.Unassigned}?year={year}");
        JsonElement badge = await QcApi.GetDataAsync(client, $"{QcEndpoints.UnassignedCount}?year={year}");

        IEnumerable<string> memberNumbers = members.Array("pagedData")
            .Select(row => string.Join(
                '/',
                row.Str("fullName"),
                row.Int("partyAgeYears"),
                row.IntOrNull("nextMilestone"),
                row.StringOrNull("nextMilestoneDate")));

        IEnumerable<string> eligibleNames = dashboard.Array("eligibleMembers")
            .Select(row => $"{row.Str("fullName")}:{row.Int("milestone")}");

        return string.Join(
            '\n',
            memberNumbers
                .Concat(eligibleNames)
                .Append($"upcoming={upcoming.Str("name")}/{year}/{upcoming.Int("eligibleCount")}")
                .Append($"unassigned={unassigned.Array("members").Count}")
                .Append($"badge={badge.Int("count")}")
                .Append($"overlaps={warnings.GetProperty("overlaps").GetRawText()}")
                .Append($"gaps={warnings.GetProperty("gaps").GetRawText()}"));
    }

    /// <summary>
    /// Một người cố định của bộ lõi. Chọn theo tên thay vì "dòng đầu bảng" để ca không phụ
    /// thuộc thứ tự sắp xếp mặc định của endpoint danh sách.
    /// </summary>
    /// <param name="client">Client đã đăng nhập.</param>
    /// <returns>Dòng JSON của người đó.</returns>
    private static async Task<JsonElement> ProbeMemberAsync(HttpClient client)
    {
        JsonElement data = await QcApi.GetDataAsync(
            client,
            $"{QcEndpoints.PartyMembers}?searchKeyword={Uri.EscapeDataString(ProbeMemberName)}&searchFields=FullName");

        return data.Array("pagedData").Single(row => row.Str("fullName") == ProbeMemberName);
    }

    private static async Task<string> ProbeMemberIdAsync(HttpClient client)
        => (await ProbeMemberAsync(client)).Str("id");

    private async Task<HttpClient> SeedCoreAndLoginAsync()
    {
        await QcDb.SeedCoreAsync(factory);

        return await QcApi.LoginAsync(factory);
    }
}
