using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Modules.Auth.Responses;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// T47 · QC-T27-05: giá trị lọc không hợp lệ trên <c>GET /api/PartyMembers</c> phải trả
/// <c>400</c> kèm khóa <c>Mes.Common.Invalid.Parameter</c>, chứ không âm thầm bỏ bộ lọc
/// rồi trả về toàn bộ kho dữ liệu. Giá trị hợp lệ phải giữ nguyên kết quả cũ.
/// </summary>
[Collection(ApiCollection.Name)]
public class PartyMemberFilterValueTests
{
    private const string BasePath = "/api/PartyMembers";

    private readonly HuyHieuDangApiFactory factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartyMemberFilterValueTests"/> class.
    /// </summary>
    /// <param name="factory">Host kiểm thử dùng chung cho cả bộ.</param>
    public PartyMemberFilterValueTests(HuyHieuDangApiFactory factory)
    {
        this.factory = factory;
    }

    [Theory(DisplayName = "QC-T27-05 · Giá trị lọc lạ trả 400 với khóa Mes.Common.Invalid.Parameter")]
    [InlineData("filter.Gender=$eq:Khac")]
    [InlineData("filter.Gender=$eq:")]
    [InlineData("filter.Gender=$eq:1")]
    [InlineData("filter.Gender=$in:Male,Khac")]
    [InlineData("filter.Gender=Male")]
    [InlineData("filter.OfficialAdmissionDate=$gte:hom-qua")]
    public async Task Search_RejectsFilterValueThatDoesNotMatchTheField(string filter)
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync(Members());

        using HttpResponseMessage response = await client.GetAsync($"{BasePath}?pageSize=50&{filter}");
        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using JsonDocument document = JsonDocument.Parse(body);
        Assert.Equal(Messages.Common.InvalidParameter, document.RootElement.GetProperty("message").GetString());

        // A-905: phản hồi 400 không được để lọt dấu vết ngăn xếp.
        Assert.False(document.RootElement.TryGetProperty("exception", out _), body);
        Assert.DoesNotContain("QueryExpressionExtension", body, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "QC-T27-05 · Giá trị lọc hợp lệ vẫn lọc đúng như trước")]
    public async Task Search_KeepsFilteringWithValidValues()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync(Members());

        PagedPayload<PartyMemberPayload> male = await GetPageAsync(client, $"{BasePath}?pageSize=50&filter.Gender=$eq:Male");
        Assert.Equal(1, male.PageInfo.TotalCount);
        Assert.Equal("Cao Văn Phúc", Assert.Single(male.PagedData).FullName);

        PagedPayload<PartyMemberPayload> both = await GetPageAsync(client, $"{BasePath}?pageSize=50&filter.Gender=$in:Male,Female");
        Assert.Equal(2, both.PageInfo.TotalCount);

        PagedPayload<PartyMemberPayload> all = await GetPageAsync(client, $"{BasePath}?pageSize=50");
        Assert.Equal(3, all.PageInfo.TotalCount);
    }

    private static PartyMember[] Members() => new[]
    {
        new PartyMember { FullName = "Cao Văn Phúc", Gender = Gender.Male, OfficialAdmissionDate = new DateOnly(1976, 9, 12) },
        new PartyMember { FullName = "Bùi Thị Lan", Gender = Gender.Female, OfficialAdmissionDate = new DateOnly(1998, 6, 12) },
        new PartyMember { FullName = "Đỗ Thị Mai", OfficialAdmissionDate = new DateOnly(1990, 1, 1) },
    };

    private static async Task<PagedPayload<PartyMemberPayload>> GetPageAsync(HttpClient client, string url)
    {
        using HttpResponseMessage response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiResponse<PagedPayload<PartyMemberPayload>> body =
            await response.ReadApiResponseAsync<PagedPayload<PartyMemberPayload>>();

        return body.Data!;
    }

    private async Task ResetAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Set<PartyMember>().ExecuteDeleteAsync();
    }

    private async Task SeedAsync(params PartyMember[] members)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Set<PartyMember>().AddRange(members);
        await dbContext.SaveChangesAsync();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        HttpClient client = factory.CreateClient();

        using HttpResponseMessage login = await client.PostAsJsonAsync(
            "/api/Auth/Login",
            new { username = HuyHieuDangApiFactory.AdminUsername, password = HuyHieuDangApiFactory.AdminPassword });
        login.EnsureSuccessStatusCode();

        ApiResponse<SessionPayload> body = await login.ReadApiResponseAsync<SessionPayload>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TokenTypes.Bearer, body.Data!.AccessToken);

        return client;
    }
}
