using System.Text.Json.Serialization;

namespace HuyHieuDang.Infrastructure.Modules.Users.Responses.Authentications;

public class Data
{
    [JsonPropertyName("user")]
    public UserSso? User { get; set; }

    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("refreshToken")]
    public string? RefreshToken { get; set; }
}

public class GetTokenSsoResponse
{
    [JsonPropertyName("data")]
    public Data? Data { get; set; }
}

public class UserSso
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("userName")]
    public string Username { get; set; } = default!;

    [JsonPropertyName("emailAddress")]
    public string Email { get; set; } = default!;

    [JsonPropertyName("fullName")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("dob")]
    public DateTime? DayOfBirth { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }
}