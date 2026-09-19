using System.Text.Json.Serialization;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Phần <c>data</c> của một đảng viên (mục 1.10 hợp đồng API). Đọc <c>gender</c> dưới dạng chuỗi
/// để bắt đúng yêu cầu enum phải tuần tự hóa thành <c>"Male"</c> / <c>"Female"</c>.
/// </summary>
public sealed class PartyMemberPayload
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    [JsonPropertyName("dateOfBirth")]
    public DateOnly? DateOfBirth { get; set; }

    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("officialAdmissionDate")]
    public DateOnly OfficialAdmissionDate { get; set; }

    [JsonPropertyName("partyAgeYears")]
    public int PartyAgeYears { get; set; }

    [JsonPropertyName("nextMilestone")]
    public int? NextMilestone { get; set; }

    [JsonPropertyName("nextMilestoneDate")]
    public DateOnly? NextMilestoneDate { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// Phần <c>data</c> của danh sách có phân trang (mục 1.7 hợp đồng API).
/// </summary>
/// <typeparam name="TItem">Kiểu một dòng.</typeparam>
public sealed class PagedPayload<TItem>
{
    [JsonPropertyName("pagedData")]
    public List<TItem> PagedData { get; set; } = new();

    [JsonPropertyName("pageInfo")]
    public PageInfoPayload PageInfo { get; set; } = new();
}

/// <summary>
/// Khối <c>pageInfo</c> của danh sách có phân trang.
/// </summary>
public sealed class PageInfoPayload
{
    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("current")]
    public int Current { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("hasNext")]
    public bool HasNext { get; set; }

    [JsonPropertyName("hasPrevious")]
    public bool HasPrevious { get; set; }
}

/// <summary>
/// Phần <c>data</c> của xóa nhiều: <c>{ "ids": [ … ] }</c>.
/// </summary>
public sealed class IdentifiersPayload
{
    [JsonPropertyName("ids")]
    public List<Guid> Ids { get; set; } = new();
}

/// <summary>
/// Phần <c>data</c> của xóa một: <c>{ "id": "…" }</c>.
/// </summary>
public sealed class IdentifierPayload
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
}
