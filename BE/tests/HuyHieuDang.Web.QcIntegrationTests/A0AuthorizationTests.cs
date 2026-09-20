using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using HuyHieuDang.Web.QcIntegrationTests.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace HuyHieuDang.Web.QcIntegrationTests;

/// <summary>
/// A-001 → A-007 · Phân quyền. Hợp đồng API mục 1.2: mọi endpoint trừ <c>POST /api/Auth/Login</c>
/// đều đòi token hợp lệ.
/// </summary>
[Collection(QcApiCollection.Name)]
public sealed class A0AuthorizationTests
{
    private readonly QcApiFactory factory;

    /// <summary>Initializes a new instance of the <see cref="A0AuthorizationTests"/> class.</summary>
    /// <param name="factory">Host kiểm thử dùng chung.</param>
    public A0AuthorizationTests(QcApiFactory factory)
    {
        this.factory = factory;
    }

    /// <summary>
    /// Toàn bộ 27 endpoint nghiệp vụ của hợp đồng API, tức 28 endpoint trừ <c>Auth/Login</c>.
    /// Danh sách này cũng là bảng đối chiếu độ phủ endpoint của báo cáo T27.
    /// </summary>
    /// <returns>Bộ dữ liệu cho lý thuyết kiểm thử.</returns>
    public static TheoryData<string, string> BusinessEndpoints()
    {
        TheoryData<string, string> data = new();

        foreach ((string method, string url) in QcEndpoints.All)
        {
            data.Add(method, url);
        }

        return data;
    }

    /// <summary>A-001 · Gọi mọi endpoint nghiệp vụ không kèm JWT phải bị từ chối bằng 401.</summary>
    /// <param name="method">Động từ HTTP.</param>
    /// <param name="url">Đường dẫn.</param>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Theory]
    [MemberData(nameof(BusinessEndpoints))]
    public async Task A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(string method, string url)
    {
        await QcDb.SeedCoreAsync(factory);
        using HttpClient client = QcApi.Anonymous(factory);

        using HttpRequestMessage request = new(new HttpMethod(method), url);

        // Hai endpoint import nhận multipart; gửi JSON vào đó sẽ dừng ở 415 trước cả cổng xác
        // thực, nên ca này gửi đúng kiểu nội dung để thật sự kiểm được cổng xác thực (A-001).
        if (url.Contains("/Import/", StringComparison.Ordinal))
        {
            MultipartFormDataContent form = new();
            form.Add(new ByteArrayContent(new byte[] { 1, 2, 3 }), "file", "bat-ky.xlsx");
            request.Content = form;
        }
        else if (method is "POST" or "PUT")
        {
            request.Content = JsonContent.Create(new { });
        }

        using HttpResponseMessage response = await client.SendAsync(request);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized, $"{method} {url}");

        string body = await response.Content.ReadAsStringAsync();
        body.ShouldNotContain("Nguyễn", Case.Sensitive, $"{method} {url} lộ dữ liệu đảng viên");
        body.ShouldNotContain("pagedData", Case.Sensitive, $"{method} {url} lộ dữ liệu danh sách");
        body.ShouldNotContain("Đợt", Case.Sensitive, $"{method} {url} lộ dữ liệu đợt");
    }

    /// <summary>A-002 · Token ký bằng khóa khác phải bị từ chối.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A002_Jwt_sai_chu_ky_tra_401()
    {
        using HttpClient client = QcApi.Anonymous(factory);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            BuildToken("khoa-gia-mao-cua-ke-tan-cong-du-dai-256-bit-de-ky-duoc", DateTime.UtcNow.AddHours(8)));

        using HttpResponseMessage response = await client.GetAsync(QcEndpoints.Dashboard);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>A-003 · Token đúng khóa nhưng đã hết hạn phải bị từ chối.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A003_Jwt_het_han_tra_401()
    {
        using HttpClient client = QcApi.Anonymous(factory);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            BuildToken(QcApiFactory.JwtKey, DateTime.UtcNow.AddMinutes(-1)));

        using HttpResponseMessage response = await client.GetAsync(QcEndpoints.Dashboard);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>A-004 · Token gửi kèm mà thiếu tiền tố <c>Bearer</c> phải bị từ chối.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A004_Thieu_tien_to_Bearer_tra_401()
    {
        using HttpClient authenticated = await QcApi.LoginAsync(factory);
        string token = authenticated.DefaultRequestHeaders.Authorization!.Parameter!;

        using HttpClient client = QcApi.Anonymous(factory);
        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);

        using HttpResponseMessage response = await client.GetAsync(QcEndpoints.Dashboard);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// A-005 · Sai mật khẩu và không có tài khoản phải cho **cùng một** thông điệp chung,
    /// không tiết lộ tài khoản nào có thật (UC-00).
    /// </summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A005_Dang_nhap_sai_tra_401_va_thong_diep_chung()
    {
        using HttpClient client = QcApi.Anonymous(factory);

        using HttpResponseMessage wrongPassword = await client.PostAsJsonAsync(
            QcEndpoints.Login, new { username = QcApiFactory.AdminUsername, password = "SaiMatKhau@123" });
        using HttpResponseMessage unknownUser = await client.PostAsJsonAsync(
            QcEndpoints.Login, new { username = "khong-ton-tai", password = QcApiFactory.AdminPassword });

        JsonElement wrongPasswordBody =
            await QcApi.AssertErrorAsync(wrongPassword, HttpStatusCode.Unauthorized, QcMessages.LoginFailed);
        JsonElement unknownUserBody =
            await QcApi.AssertErrorAsync(unknownUser, HttpStatusCode.Unauthorized, QcMessages.LoginFailed);

        unknownUserBody.Str("message").ShouldBe(wrongPasswordBody.Str("message"));
    }

    /// <summary>A-006 · Đăng nhập đúng trả token dùng được ngay cho endpoint khác.</summary>
    /// <returns>Tác vụ bất đồng bộ.</returns>
    [Fact]
    public async Task A006_Dang_nhap_dung_tra_token_dung_duoc_ngay()
    {
        using HttpClient client = QcApi.Anonymous(factory);

        using HttpResponseMessage login = await client.PostAsJsonAsync(
            QcEndpoints.Login, new { username = QcApiFactory.AdminUsername, password = QcApiFactory.AdminPassword });

        login.StatusCode.ShouldBe(HttpStatusCode.OK);

        JsonElement data = (await QcApi.ReadAsync(login)).GetProperty("data");
        data.Str("accessToken").ShouldNotBeNullOrWhiteSpace();
        data.Str("tokenType").ShouldBe("Bearer");
        data.Str("username").ShouldBe(QcApiFactory.AdminUsername);

        // serverDate là ngày của máy chủ, không phải của trình duyệt (mục 1.6 hợp đồng API).
        data.Str("serverDate").ShouldBe(QcClock.T0.ToString("yyyy-MM-dd"));

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", data.Str("accessToken"));

        using HttpResponseMessage me = await client.GetAsync(QcEndpoints.Me);
        me.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    /// <summary>
    /// A-007 · Ca kiểm tra tĩnh: quét mọi action của mọi controller, chỉ <c>Auth/Login</c>
    /// được phép mang <see cref="AllowAnonymousAttribute"/>.
    /// </summary>
    [Fact]
    public void A007_Chi_Login_duoc_phep_AllowAnonymous()
    {
        List<string> anonymous = new();

        foreach (Type controller in typeof(Program).Assembly.GetTypes()
                     .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract))
        {
            bool controllerAnonymous = controller.GetCustomAttribute<AllowAnonymousAttribute>() is not null;

            foreach (MethodInfo action in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                         .Where(method => method.DeclaringType == controller))
            {
                if (controllerAnonymous || action.GetCustomAttribute<AllowAnonymousAttribute>() is not null)
                {
                    anonymous.Add($"{controller.Name}.{action.Name}");
                }
            }
        }

        anonymous.ShouldBe(new[] { "AuthController.LoginAsync" });
    }

    private static string BuildToken(string key, DateTime expiresUtc)
    {
        SigningCredentials credentials = new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            claims: new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) },
            notBefore: expiresUtc.AddHours(-9),
            expires: expiresUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
