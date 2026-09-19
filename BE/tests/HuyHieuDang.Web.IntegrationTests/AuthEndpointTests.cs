using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.Auth.Responses;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Các ca A-001 → A-006 của mục 5.1 kế hoạch kiểm thử, cùng hình dạng phản hồi
/// của nhóm 1 trong hợp đồng API.
/// </summary>
public class AuthEndpointTests : IClassFixture<HuyHieuDangApiFactory>
{
    private const string LoginPath = "/api/Auth/Login";
    private const string LogoutPath = "/api/Auth/Logout";
    private const string MePath = "/api/Auth/Me";

    private static readonly string LoginSuccessKey = Messages<User>.Action(ControllerActions.Login, true);
    private static readonly string LoginFailedKey = Messages<User>.Action(ControllerActions.Login, false);
    private static readonly string LogoutSuccessKey = Messages<User>.Action(ControllerActions.Logout, true);

    private readonly HuyHieuDangApiFactory factory;

    public AuthEndpointTests(HuyHieuDangApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact(DisplayName = "A-006 · Đăng nhập đúng trả 200 kèm token đúng hợp đồng")]
    public async Task Login_WithSeededAdmin_ReturnsTokenAndSessionInfo()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await LoginAsync(client, HuyHieuDangApiFactory.AdminUsername, HuyHieuDangApiFactory.AdminPassword);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiResponse<SessionPayload> body = await response.ReadApiResponseAsync<SessionPayload>();
        Assert.Equal(LoginSuccessKey, body.Message);
        Assert.NotNull(body.Data);
        Assert.False(string.IsNullOrWhiteSpace(body.Data!.AccessToken));
        Assert.Equal(TokenTypes.Bearer, body.Data.TokenType);
        Assert.Equal(HuyHieuDangApiFactory.AdminUsername, body.Data.Username);
        Assert.Equal(HuyHieuDangApiFactory.FixedToday, body.Data.ServerDate);
        Assert.Null(body.Data.UnitName);
        Assert.True(body.Data.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact(DisplayName = "Hạn token đúng 8 giờ theo hợp đồng mục 1.2")]
    public async Task Login_IssuesTokenValidForEightHours()
    {
        HttpClient client = factory.CreateClient();

        SessionPayload payload = await LoginAndReadAsync(client);

        TimeSpan lifetime = payload.ExpiresAt - DateTimeOffset.UtcNow;
        Assert.InRange(lifetime, TimeSpan.FromHours(7.9), TimeSpan.FromHours(8));

        JwtSecurityToken token = new JwtSecurityTokenHandler().ReadJwtToken(payload.AccessToken);
        Assert.Equal(payload.ExpiresAt.ToUnixTimeSeconds(), new DateTimeOffset(token.ValidTo, TimeSpan.Zero).ToUnixTimeSeconds());
    }

    [Fact(DisplayName = "A-006 · Token vừa cấp dùng được ngay cho endpoint khác")]
    public async Task Me_WithFreshToken_ReturnsSession()
    {
        HttpClient client = factory.CreateClient();
        SessionPayload login = await LoginAndReadAsync(client);

        using HttpRequestMessage request = new(HttpMethod.Get, MePath);
        request.Headers.Authorization = new AuthenticationHeaderValue(TokenTypes.Bearer, login.AccessToken);
        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiResponse<SessionPayload> body = await response.ReadApiResponseAsync<SessionPayload>();
        Assert.NotNull(body.Data);
        Assert.Equal(HuyHieuDangApiFactory.AdminUsername, body.Data!.Username);
        Assert.Equal(HuyHieuDangApiFactory.FixedToday, body.Data.ServerDate);
        Assert.Equal(login.ExpiresAt.ToUnixTimeSeconds(), body.Data.ExpiresAt.ToUnixTimeSeconds());
    }

    [Fact(DisplayName = "Đăng xuất có token trả 200 và khóa Mes.User.Logout.Successfully")]
    public async Task Logout_WithToken_ReturnsSuccessKey()
    {
        HttpClient client = factory.CreateClient();
        SessionPayload login = await LoginAndReadAsync(client);

        using HttpRequestMessage request = new(HttpMethod.Post, LogoutPath);
        request.Headers.Authorization = new AuthenticationHeaderValue(TokenTypes.Bearer, login.AccessToken);
        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiResponse<object> body = await response.ReadApiResponseAsync<object>();
        Assert.Equal(LogoutSuccessKey, body.Message);
        Assert.Null(body.Data);
    }

    [Fact(DisplayName = "A-005 · Sai mật khẩu và không có tài khoản trả cùng một thông báo 401")]
    public async Task Login_WithWrongCredentials_ReturnsSameGenericMessage()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage wrongPassword = await LoginAsync(client, HuyHieuDangApiFactory.AdminUsername, "sai-mat-khau");
        HttpResponseMessage unknownUser = await LoginAsync(client, "khong-ton-tai", HuyHieuDangApiFactory.AdminPassword);

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownUser.StatusCode);

        ApiResponse<object> wrongPasswordBody = await wrongPassword.ReadApiResponseAsync<object>();
        ApiResponse<object> unknownUserBody = await unknownUser.ReadApiResponseAsync<object>();

        Assert.Equal(LoginFailedKey, wrongPasswordBody.Message);
        Assert.Equal(LoginFailedKey, unknownUserBody.Message);
        Assert.Equal((int)HttpStatusCode.Unauthorized, wrongPasswordBody.StatusCode);
    }

    [Theory(DisplayName = "Thiếu tài khoản hoặc mật khẩu trả 400 kèm khóa Required")]
    [InlineData("", "Kiem@Thu123", nameof(User.Username))]
    [InlineData("admin", "", nameof(User.Password))]
    public async Task Login_WithMissingField_ReturnsRequiredKey(string username, string password, string expectedProperty)
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await LoginAsync(client, username, password);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        ApiResponse<object> body = await response.ReadApiResponseAsync<object>();
        Assert.Equal(Messages<User>.Required(expectedProperty), body.Message);
    }

    [Theory(DisplayName = "A-001 · Gọi endpoint nghiệp vụ không kèm token trả 401 và không lộ dữ liệu")]
    [InlineData("GET", MePath)]
    [InlineData("POST", LogoutPath)]
    public async Task BusinessEndpoint_WithoutToken_ReturnsUnauthorized(string method, string path)
    {
        HttpClient client = factory.CreateClient();

        using HttpRequestMessage request = new(new HttpMethod(method), path);
        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
    }

    [Fact(DisplayName = "A-002 · JWT sai chữ ký trả 401")]
    public async Task Me_WithForgedSignature_ReturnsUnauthorized()
    {
        string forged = CreateToken("khoa-gia-mao-hoan-toan-khac-du-dai-256-bit-cho-kiem-thu", DateTime.UtcNow.AddHours(1));

        Assert.Equal(HttpStatusCode.Unauthorized, await CallMeAsync(forged, TokenTypes.Bearer));
    }

    [Fact(DisplayName = "A-003 · JWT đã hết hạn trả 401")]
    public async Task Me_WithExpiredToken_ReturnsUnauthorized()
    {
        string expired = CreateToken(HuyHieuDangApiFactory.JwtKey, DateTime.UtcNow.AddMinutes(-1));

        Assert.Equal(HttpStatusCode.Unauthorized, await CallMeAsync(expired, TokenTypes.Bearer));
    }

    [Fact(DisplayName = "A-004 · JWT thiếu tiền tố Bearer trả 401")]
    public async Task Me_WithoutBearerPrefix_ReturnsUnauthorized()
    {
        HttpClient client = factory.CreateClient();
        SessionPayload login = await LoginAndReadAsync(client);

        using HttpRequestMessage request = new(HttpMethod.Get, MePath);
        request.Headers.TryAddWithoutValidation("Authorization", login.AccessToken);
        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "Token hợp lệ nhưng chủ thể không còn tồn tại trả 401")]
    public async Task Me_WithTokenOfUnknownUser_ReturnsUnauthorized()
    {
        string ghost = CreateToken(
            HuyHieuDangApiFactory.JwtKey,
            DateTime.UtcNow.AddHours(1),
            new Claim(JwtTokenPayload.Identification, Guid.NewGuid().ToString()),
            new Claim(JwtTokenPayload.ModelType, typeof(User).FullName!));

        Assert.Equal(HttpStatusCode.Unauthorized, await CallMeAsync(ghost, TokenTypes.Bearer));
    }

    private static string CreateToken(string key, DateTime expiresAtUtc, params Claim[] claims)
    {
        SigningCredentials credentials = new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(claims: claims, expires: expiresAtUtc, signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static Task<HttpResponseMessage> LoginAsync(HttpClient client, string username, string password)
        => client.PostAsJsonAsync(LoginPath, new { username, password });

    private async Task<HttpStatusCode> CallMeAsync(string token, string scheme)
    {
        HttpClient client = factory.CreateClient();

        using HttpRequestMessage request = new(HttpMethod.Get, MePath);
        request.Headers.Authorization = new AuthenticationHeaderValue(scheme, token);

        return (await client.SendAsync(request)).StatusCode;
    }

    private async Task<SessionPayload> LoginAndReadAsync(HttpClient client)
    {
        HttpResponseMessage response = await LoginAsync(client, HuyHieuDangApiFactory.AdminUsername, HuyHieuDangApiFactory.AdminPassword);
        response.EnsureSuccessStatusCode();

        ApiResponse<SessionPayload> body = await response.ReadApiResponseAsync<SessionPayload>();

        return body.Data!;
    }
}
