using System.Text.Json.Serialization;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Phần <c>data</c> của <c>GET</c> / <c>PUT /api/Settings</c> và
/// <c>POST /api/Settings/RestoreDefaults</c> (mục 7.1 hợp đồng API).
/// </summary>
public sealed class AppSettingPayload
{
    [JsonPropertyName("startYears")]
    public int StartYears { get; set; }

    [JsonPropertyName("endYears")]
    public int EndYears { get; set; }

    [JsonPropertyName("stepYears")]
    public int StepYears { get; set; }

    [JsonPropertyName("unitName")]
    public string? UnitName { get; set; }

    [JsonPropertyName("milestones")]
    public List<int> Milestones { get; set; } = new();

    [JsonPropertyName("milestoneCount")]
    public int MilestoneCount { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// Phần <c>data</c> của <c>GET /api/Settings/Milestones</c> (mục 7.4 hợp đồng API).
/// </summary>
public sealed class MilestonePreviewPayload
{
    [JsonPropertyName("milestones")]
    public List<int> Milestones { get; set; } = new();

    [JsonPropertyName("milestoneCount")]
    public int MilestoneCount { get; set; }
}
