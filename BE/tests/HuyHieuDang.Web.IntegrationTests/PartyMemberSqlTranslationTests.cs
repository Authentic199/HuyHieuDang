using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Modules.Auth.Responses;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// T51 — bằng chứng điều kiện lọc và mệnh đề sắp xếp của Mốc kế tiếp / Tuổi đảng nằm trong SQL,
/// chứ không phải nạp cả bảng rồi lọc bằng LINQ-to-Objects: đọc thẳng câu lệnh EF Core đã gửi
/// xuống PostgreSQL.
/// </summary>
[Collection(ApiCollection.Name)]
public class PartyMemberSqlTranslationTests
{
    private const string BasePath = "/api/PartyMembers";

    private readonly HuyHieuDangApiFactory factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartyMemberSqlTranslationTests"/> class.
    /// </summary>
    /// <param name="factory">Host kiểm thử dùng chung cho cả lớp.</param>
    public PartyMemberSqlTranslationTests(HuyHieuDangApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact(DisplayName = "T51 · Lọc Mốc kế tiếp sinh ra hai bất đẳng thức trong WHERE")]
    public async Task Search_FilteringByNextMilestone_PutsThePredicateInSql()
    {
        IReadOnlyList<string> commands = await CaptureSqlAsync($"{BasePath}?filter.NextMilestone=$eq:40");

        string page = PageQuery(commands);

        Assert.Contains("WHERE", page, StringComparison.Ordinal);
        Assert.Contains("\"OfficialAdmissionDate\" > @", page, StringComparison.Ordinal);
        Assert.Contains("\"OfficialAdmissionDate\" <= @", page, StringComparison.Ordinal);

        // Tổng số dòng cũng phải đếm kèm điều kiện, nếu không phân trang sẽ sai tổng.
        string count = Assert.Single(commands.Where(IsCountQuery));
        Assert.Contains("\"OfficialAdmissionDate\" > @", count, StringComparison.Ordinal);
        Assert.Contains("\"OfficialAdmissionDate\" <= @", count, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "T51 · Lọc None chỉ chặn cận trên")]
    public async Task Search_FilteringByNone_PutsOnlyTheUpperBoundInSql()
    {
        IReadOnlyList<string> commands = await CaptureSqlAsync($"{BasePath}?filter.NextMilestone=$eq:None");

        string page = PageQuery(commands);

        Assert.Contains("\"OfficialAdmissionDate\" <= @", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"OfficialAdmissionDate\" > @", page, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "T51 · Sắp xếp theo cột tính ra sinh ra ORDER BY trên cột ngày")]
    [InlineData("PartyAge asc", true)]
    [InlineData("PartyAge desc", false)]
    [InlineData("partyAgeYears asc", true)]
    [InlineData("NextMilestone asc", true)]
    [InlineData("NextMilestone desc", false)]
    public async Task Search_SortingByComputedColumn_PutsTheOrderByInSql(string sortQuery, bool isDescending)
    {
        IReadOnlyList<string> commands =
            await CaptureSqlAsync($"{BasePath}?sortQuery={Uri.EscapeDataString(sortQuery)}");

        string page = PageQuery(commands);
        string orderBy = page[page.LastIndexOf("ORDER BY", StringComparison.Ordinal)..];

        Assert.Contains(
            isDescending ? "\"OfficialAdmissionDate\" DESC" : "\"OfficialAdmissionDate\",",
            orderBy,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Câu lệnh lấy trang dữ liệu — câu duy nhất vừa đọc bảng đảng viên vừa có <c>ORDER BY</c>.
    /// </summary>
    /// <param name="commands">Toàn bộ câu lệnh đã chạy trong lời gọi.</param>
    /// <returns>Câu lệnh tìm được.</returns>
    private static string PageQuery(IReadOnlyList<string> commands)
        => Assert.Single(commands.Where(x =>
            x.Contains("FROM party_member", StringComparison.Ordinal)
            && x.Contains("ORDER BY", StringComparison.Ordinal)));

    private static bool IsCountQuery(string command)
        => command.Contains("FROM party_member", StringComparison.Ordinal)
            && command.Contains("count(*)", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gọi endpoint rồi trả về mọi câu lệnh EF Core đã chạy trong đúng lời gọi đó.
    /// </summary>
    /// <param name="url">Đường dẫn cần gọi.</param>
    /// <returns>Danh sách câu lệnh SQL.</returns>
    private async Task<IReadOnlyList<string>> CaptureSqlAsync(string url)
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage login = await client.PostAsJsonAsync(
            "/api/Auth/Login",
            new { username = HuyHieuDangApiFactory.AdminUsername, password = HuyHieuDangApiFactory.AdminPassword });
        login.EnsureSuccessStatusCode();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            TokenTypes.Bearer,
            (await login.ReadApiResponseAsync<SessionPayload>()).Data!.AccessToken);

        using (IServiceScope scope = factory.Services.CreateScope())
        {
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.Set<PartyMember>().ExecuteDeleteAsync();
            dbContext.Set<PartyMember>().AddRange(
                new PartyMember { FullName = "Nguoi A", OfficialAdmissionDate = new DateOnly(1990, 1, 1) },
                new PartyMember { FullName = "Nguoi B", OfficialAdmissionDate = new DateOnly(1930, 1, 1) });
            await dbContext.SaveChangesAsync();
        }

        factory.CapturedSql.Clear();

        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        IReadOnlyList<string> captured = factory.CapturedSql.Commands;
        Assert.NotEmpty(captured);

        return captured;
    }
}
