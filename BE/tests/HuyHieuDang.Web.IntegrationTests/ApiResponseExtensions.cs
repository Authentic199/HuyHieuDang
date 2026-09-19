using System.Text.Json;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Tiện ích đọc thân phản hồi.
/// </summary>
public static class ApiResponseExtensions
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static async Task<ApiResponse<TData>> ReadApiResponseAsync<TData>(this HttpResponseMessage response)
    {
        string body = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<ApiResponse<TData>>(body, Options)
            ?? throw new InvalidOperationException($"Không đọc được thân phản hồi: {body}");
    }
}
