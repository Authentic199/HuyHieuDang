using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Enums;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Notes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using AwardPeriod = HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities.AwardPeriod;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// QT12 — ghi chú đảng viên và ngày ghi (mục 1.6, 1.10, 3.3, 3.4 và 3.6 hợp đồng API v1.6).
/// "Hôm nay" cố định ở T0 = 19/09/2026 09:30 +07:00 nên ngày ghi trong khẳng định là một
/// giá trị biết trước, không lệ thuộc lúc chạy.
/// </summary>
[Collection(ApiCollection.Name)]
public class PartyMemberNoteEndpointTests
{
    private const string BasePath = "/api/PartyMembers";
    private const string DashboardPath = "/api/Dashboard";
    private const string EligibilityPath = "/api/Eligibility";
    private const string CommitPath = "/api/PartyMembers/Import/Commit";

    /// <summary>
    /// Ngày ghi mà máy chủ phải đóng: đúng <c>IDateTimeProvider.Now</c> của host kiểm thử.
    /// </summary>
    private static readonly DateTimeOffset FrozenNow =
        new(HuyHieuDangApiFactory.FixedToday.ToDateTime(new TimeOnly(9, 30)), TimeSpan.FromHours(7));

    /// <summary>
    /// Một ngày ghi cũ, nạp thẳng vào bảng để chứng minh máy chủ giữ nguyên khi nội dung không đổi.
    /// </summary>
    private static readonly DateTimeOffset OldStamp = new(2026, 1, 2, 8, 0, 0, TimeSpan.FromHours(7));

    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

    private readonly HuyHieuDangApiFactory factory;
    private readonly ITestOutputHelper output;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartyMemberNoteEndpointTests"/> class.
    /// </summary>
    /// <param name="factory">Host kiểm thử dùng chung cho cả lớp.</param>
    /// <param name="output">Nơi in lại lời gọi HTTP thật để làm bằng chứng.</param>
    public PartyMemberNoteEndpointTests(HuyHieuDangApiFactory factory, ITestOutputHelper output)
    {
        this.factory = factory;
        this.output = output;
    }

    [Fact(DisplayName = "3.3 · POST kèm ghi chú: trả note đã cắt và noteUpdatedAt bằng giờ máy chủ")]
    public async Task Create_WithNote_StampsServerTime()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await ResetAsync();

        HttpResponseMessage response = await client.PostAsJsonAsync(BasePath, new
        {
            fullName = "Nguyễn Văn An",
            dateOfBirth = "1958-03-12",
            gender = "Male",
            officialAdmissionDate = "1996-10-15",
            note = "  Sức khỏe yếu, con trai nhận thay.  ",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string raw = await response.Content.ReadAsStringAsync();
        output.WriteLine("POST /api/PartyMembers → " + raw);

        PartyMemberPayload data = Read(raw);
        Assert.Equal("Sức khỏe yếu, con trai nhận thay.", data.Note);
        Assert.Equal(FrozenNow, data.NoteUpdatedAt);

        // Mục 1.6: trả kèm +07:00, không quy về Z — nếu không, ghi chú viết lúc rạng sáng
        // sẽ hiện lùi một ngày trên giao diện.
        Assert.EndsWith("+07:00", RawNoteUpdatedAt(raw), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "3.3 · POST: 500 ký tự thì được, 501 ký tự thì 400 Mes.PartyMember.OverLength.Note")]
    public async Task Create_EnforcesMaxLengthAfterTrimming()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await ResetAsync();

        string exactly = new('a', PartyMemberNote.MaxLength);

        HttpResponseMessage ok = await client.PostAsJsonAsync(BasePath, Body("Đúng 500", "   " + exactly + "   "));
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal(exactly, (await ok.ReadApiResponseAsync<PartyMemberPayload>()).Data!.Note);

        HttpResponseMessage tooLong = await client.PostAsJsonAsync(BasePath, Body("Quá 500", exactly + "a"));
        await ApiClientFactory.AssertErrorAsync(tooLong, Messages<PartyMember>.OverLength(nameof(PartyMember.Note)));
    }

    [Fact(DisplayName = "3.3 · POST ghi chú toàn khoảng trắng: lưu null cả note lẫn noteUpdatedAt")]
    public async Task Create_WithBlankNote_StoresNull()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await ResetAsync();

        HttpResponseMessage response = await client.PostAsJsonAsync(BasePath, Body("Khoảng trắng", "   \r\n  "));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PartyMemberPayload data = (await response.ReadApiResponseAsync<PartyMemberPayload>()).Data!;
        Assert.Null(data.Note);
        Assert.Null(data.NoteUpdatedAt);
    }

    [Fact(DisplayName = "3.4 · PUT đổi họ tên, ghi chú y hệt: noteUpdatedAt giữ nguyên")]
    public async Task Update_WithSameNote_KeepsStamp()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await ResetAsync();
        Guid id = await SeedAsync(NotedMember("Tên cũ", "Ghi chú cũ"));

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"{BasePath}/{id}", Body("Tên mới", "   Ghi chú cũ   "));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PartyMemberPayload data = (await response.ReadApiResponseAsync<PartyMemberPayload>()).Data!;
        Assert.Equal("Tên mới", data.FullName);
        Assert.Equal("Ghi chú cũ", data.Note);
        Assert.Equal(OldStamp, data.NoteUpdatedAt);
    }

    [Fact(DisplayName = "3.4 · PUT đổi ghi chú: noteUpdatedAt nhảy sang giờ máy chủ")]
    public async Task Update_WithChangedNote_StampsServerTime()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await ResetAsync();
        Guid id = await SeedAsync(NotedMember("Tên cũ", "Ghi chú cũ"));

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"{BasePath}/{id}", Body("Tên cũ", "Ghi chú mới"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PartyMemberPayload data = (await response.ReadApiResponseAsync<PartyMemberPayload>()).Data!;
        Assert.Equal("Ghi chú mới", data.Note);
        Assert.Equal(FrozenNow, data.NoteUpdatedAt);
    }

    [Fact(DisplayName = "3.4 · PUT không gửi note: thay trọn nên ghi chú và ngày ghi cùng về null")]
    public async Task Update_WithoutNote_ClearsBothFields()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await ResetAsync();
        Guid id = await SeedAsync(NotedMember("Tên cũ", "Ghi chú cũ"));

        HttpResponseMessage response = await client.PutAsJsonAsync($"{BasePath}/{id}", new
        {
            fullName = "Tên cũ",
            officialAdmissionDate = "1996-10-15",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PartyMemberPayload data = (await response.ReadApiResponseAsync<PartyMemberPayload>()).Data!;
        Assert.Null(data.Note);
        Assert.Null(data.NoteUpdatedAt);
    }

    [Fact(DisplayName = "3.6 · PUT /Note đổi ghi chú: bốn trường còn lại y nguyên")]
    public async Task UpdateNote_ChangesOnlyTheNote()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await ResetAsync();
        Guid id = await SeedAsync(NotedMember("Lê Hữu Cường", "Ghi chú cũ"));

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"{BasePath}/{id}/Note", new { note = "  Ghi chú vừa sửa  " });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string raw = await response.Content.ReadAsStringAsync();
        output.WriteLine($"PUT /api/PartyMembers/{id}/Note → " + raw);

        ApiResponse<PartyMemberPayload> body =
            JsonSerializer.Deserialize<ApiResponse<PartyMemberPayload>>(raw, WebJson)!;
        Assert.Equal(Messages<PartyMember>.Update(), body.Message);

        PartyMemberPayload data = body.Data!;
        Assert.Equal("Ghi chú vừa sửa", data.Note);
        Assert.Equal(FrozenNow, data.NoteUpdatedAt);
        Assert.Equal("Lê Hữu Cường", data.FullName);
        Assert.Equal(new DateOnly(1958, 3, 12), data.DateOfBirth);
        Assert.Equal("Male", data.Gender);
        Assert.Equal(new DateOnly(1996, 10, 15), data.OfficialAdmissionDate);
    }

    [Fact(DisplayName = "3.6 · PUT /Note với thân rỗng: xóa ghi chú, ngày ghi về null")]
    public async Task UpdateNote_WithEmptyBody_ClearsNote()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await ResetAsync();
        Guid id = await SeedAsync(NotedMember("Lê Hữu Cường", "Ghi chú cũ"));

        HttpResponseMessage response = await client.PutAsJsonAsync($"{BasePath}/{id}/Note", new { });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        PartyMemberPayload data = (await response.ReadApiResponseAsync<PartyMemberPayload>()).Data!;
        Assert.Null(data.Note);
        Assert.Null(data.NoteUpdatedAt);
    }

    [Fact(DisplayName = "3.6 · PUT /Note với id không tồn tại: 400 Mes.PartyMember.NotFound")]
    public async Task UpdateNote_WithUnknownId_ReturnsNotFoundKey()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await ResetAsync();

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"{BasePath}/{Guid.NewGuid()}/Note", new { note = "Ghi chú" });

        await ApiClientFactory.AssertErrorAsync(response, Messages<PartyMember>.NotFound());
    }

    [Fact(DisplayName = "3.6 · PUT /Note 501 ký tự: 400 Mes.PartyMember.OverLength.Note")]
    public async Task UpdateNote_TooLong_ReturnsOverLengthKey()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await ResetAsync();
        Guid id = await SeedAsync(NotedMember("Lê Hữu Cường", "Ghi chú cũ"));

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"{BasePath}/{id}/Note", new { note = new string('a', PartyMemberNote.MaxLength + 1) });

        await ApiClientFactory.AssertErrorAsync(
            response, Messages<PartyMember>.OverLength(nameof(PartyMember.Note)));
    }

    [Fact(DisplayName = "3.6 · PUT /Note không có token: 401")]
    public async Task UpdateNote_WithoutToken_ReturnsUnauthorized()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"{BasePath}/{Guid.NewGuid()}/Note", new { note = "Ghi chú" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "6.1 và 6.2 · Dashboard và Eligibility trả đúng ghi chú của đúng người")]
    public async Task Dashboard_AndEligibility_CarryTheNoteOfTheRightMember()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await ResetAsync();

        Guid periodId = await SeedPeriodAsync();
        await SeedAsync(NotedMember("Lê Hữu Cường", "Sức khỏe yếu, con trai nhận thay."));
        await SeedAsync(new PartyMember
        {
            FullName = "Trần Thị Bình",
            OfficialAdmissionDate = new DateOnly(1996, 10, 20),
        });

        HttpResponseMessage dashboard = await client.GetAsync(DashboardPath);
        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);
        string raw = await dashboard.Content.ReadAsStringAsync();
        output.WriteLine("GET /api/Dashboard → " + raw);

        DashboardPayload data = JsonSerializer.Deserialize<ApiResponse<DashboardPayload>>(raw, WebJson)!.Data!;
        AssertNotes(data.EligibleMembers);
        Assert.EndsWith("+07:00", RawNoteUpdatedAt(raw), StringComparison.Ordinal);

        EligibilityListPayload list = await ApiClientFactory.GetDataAsync<EligibilityListPayload>(
            client, $"{EligibilityPath}?awardPeriodId={periodId}&year=2026");
        AssertNotes(list.Members);
    }

    [Fact(DisplayName = "4.3 · Nạp Excel: người mới có note và noteUpdatedAt đều null")]
    public async Task Import_LeavesNoteEmpty()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await ResetAsync();

        ByteArrayContent file = new(ImportFixtures.Read("core-hop-le.xlsx"));
        using MultipartFormDataContent form = new() { { file, "file", "core-hop-le.xlsx" } };

        HttpResponseMessage response = await client.PostAsync(CommitPath, form);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        List<PartyMember> imported = await dbContext.Set<PartyMember>().AsNoTracking().ToListAsync();

        Assert.NotEmpty(imported);
        Assert.All(imported, member =>
        {
            Assert.Null(member.Note);
            Assert.Null(member.NoteUpdatedAt);
        });
    }

    /// <summary>
    /// Thân yêu cầu thêm/sửa đầy đủ trường, chỉ thay họ tên và ghi chú.
    /// </summary>
    /// <param name="fullName">Họ tên.</param>
    /// <param name="note">Ghi chú thô, chưa cắt khoảng trắng.</param>
    /// <returns>Thân yêu cầu.</returns>
    private static object Body(string fullName, string? note) => new
    {
        fullName,
        dateOfBirth = "1958-03-12",
        gender = "Male",
        officialAdmissionDate = "1996-10-15",
        note,
    };

    /// <summary>
    /// Bản ghi đã có sẵn ghi chú và một ngày ghi cũ.
    /// </summary>
    /// <param name="fullName">Họ tên.</param>
    /// <param name="note">Ghi chú đang lưu.</param>
    /// <returns>Bản ghi đảng viên.</returns>
    private static PartyMember NotedMember(string fullName, string note) => new()
    {
        FullName = fullName,
        DateOfBirth = new DateOnly(1958, 3, 12),
        Gender = Gender.Male,
        OfficialAdmissionDate = new DateOnly(1996, 10, 15),
        Note = note,
        NoteUpdatedAt = OldStamp,
    };

    private static PartyMemberPayload Read(string raw)
        => JsonSerializer.Deserialize<ApiResponse<PartyMemberPayload>>(raw, WebJson)!.Data!;

    /// <summary>
    /// Giá trị thô của <c>noteUpdatedAt</c> đầu tiên trong thân phản hồi, để soi độ lệch múi giờ.
    /// </summary>
    /// <param name="raw">Thân phản hồi.</param>
    /// <returns>Chuỗi ISO chưa qua bộ đọc.</returns>
    private static string RawNoteUpdatedAt(string raw)
    {
        const string Marker = "\"noteUpdatedAt\":\"";
        int start = raw.IndexOf(Marker, StringComparison.Ordinal) + Marker.Length;

        return raw[start..raw.IndexOf('"', start)];
    }

    private static void AssertNotes(IReadOnlyList<EligibleMemberPayload> members)
    {
        EligibleMemberPayload noted = members.Single(x => x.FullName == "Lê Hữu Cường");
        Assert.Equal("Sức khỏe yếu, con trai nhận thay.", noted.Note);
        Assert.Equal(OldStamp, noted.NoteUpdatedAt);

        EligibleMemberPayload plain = members.Single(x => x.FullName == "Trần Thị Bình");
        Assert.Null(plain.Note);
        Assert.Null(plain.NoteUpdatedAt);
    }

    private async Task ResetAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Set<PartyMember>().ExecuteDeleteAsync();
        await dbContext.Set<AwardPeriod>().ExecuteDeleteAsync();

        // Các lớp kiểm thử dùng chung một cơ sở dữ liệu, nên đưa mốc huy hiệu về mặc định
        // 30/90/5 để danh sách đủ điều kiện của bài này không phụ thuộc thứ tự chạy.
        await dbContext.Set<AppSetting>().ExecuteUpdateAsync(setters => setters
            .SetProperty(x => x.StartYears, AppSetting.DefaultStartYears)
            .SetProperty(x => x.EndYears, AppSetting.DefaultEndYears)
            .SetProperty(x => x.StepYears, AppSetting.DefaultStepYears));
    }

    private async Task<Guid> SeedAsync(PartyMember member)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Set<PartyMember>().Add(member);
        await dbContext.SaveChangesAsync();

        return member.Id;
    }

    /// <summary>
    /// Một đợt duy nhất 01/10 → 07/11, để "đợt sắp tới" của Dashboard là đợt biết trước.
    /// </summary>
    /// <returns>Id của đợt.</returns>
    private async Task<Guid> SeedPeriodAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        AwardPeriod period = new()
        {
            Name = "Đợt ghi chú",
            FromDay = 1,
            FromMonth = 10,
            ToDay = 7,
            ToMonth = 11,
        };

        dbContext.Set<AwardPeriod>().Add(period);
        await dbContext.SaveChangesAsync();

        return period.Id;
    }
}
