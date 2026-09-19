using System.Text.Json.Serialization;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Lớp vỏ phản hồi chung của API (mục 1.3 và 1.4 của hợp đồng API).
/// </summary>
/// <typeparam name="TData">Kiểu của phần <c>data</c>.</typeparam>
public sealed class ApiResponse<TData>
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public TData? Data { get; set; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }
}

/// <summary>
/// Phần <c>data</c> của <c>Login</c> và <c>Me</c>, đọc đúng tên trường camelCase của hợp đồng.
/// </summary>
public sealed class SessionPayload
{
    [JsonPropertyName("accessToken")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("tokenType")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expiresAt")]
    public DateTimeOffset ExpiresAt { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("unitName")]
    public string? UnitName { get; set; }

    [JsonPropertyName("serverDate")]
    public DateOnly ServerDate { get; set; }
}
