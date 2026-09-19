using System.Text.Json.Serialization;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Một dòng đủ điều kiện (mục 1.10 hợp đồng API — <c>EligibleMemberResponse</c>).
/// </summary>
public class EligibleMemberPayload
{
    [JsonPropertyName("partyMemberId")]
    public Guid PartyMemberId { get; set; }

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = default!;

    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("dateOfBirth")]
    public DateOnly? DateOfBirth { get; set; }

    [JsonPropertyName("officialAdmissionDate")]
    public DateOnly OfficialAdmissionDate { get; set; }

    [JsonPropertyName("milestoneDate")]
    public DateOnly MilestoneDate { get; set; }

    [JsonPropertyName("milestone")]
    public int Milestone { get; set; }
}

/// <summary>
/// Một dòng chưa thuộc đợt nào (<c>UnassignedMemberResponse</c>), thêm khoảng trống chứa ngày tròn mốc.
/// </summary>
public sealed class UnassignedMemberPayload : EligibleMemberPayload
{
    [JsonPropertyName("gap")]
    public UnassignedGapPayload Gap { get; set; } = default!;
}

/// <summary>
/// Khoảng trống của một dòng chưa thuộc đợt nào.
/// </summary>
public sealed class UnassignedGapPayload
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;

    [JsonPropertyName("previousPeriodName")]
    public string? PreviousPeriodName { get; set; }

    [JsonPropertyName("nextPeriodName")]
    public string? NextPeriodName { get; set; }

    /// <summary>
    /// Dựng lại đúng câu chữ cột "Khoảng trống" của giao diện (mục 1.10 hợp đồng API),
    /// để đối chiếu thẳng với <c>gapLabel</c> trong <c>expected.json</c>.
    /// </summary>
    /// <returns>Chữ hiển thị của khoảng trống.</returns>
    public string Label() => Type switch
    {
        "Between" => $"Giữa {PreviousPeriodName} và {NextPeriodName}",
        "BeforeFirst" => "Trước đợt đầu tiên",
        "AfterLast" => "Sau đợt cuối cùng",
        _ => throw new InvalidOperationException($"Loại khoảng trống lạ: {Type}"),
    };
}

/// <summary>
/// Một dòng phân bổ theo mốc.
/// </summary>
public sealed class MilestoneBreakdownPayload
{
    [JsonPropertyName("milestone")]
    public int Milestone { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}

/// <summary>
/// Phần <c>data</c> của <c>GET /api/Eligibility</c> (mục 6.2 hợp đồng API).
/// </summary>
public sealed class EligibilityListPayload
{
    [JsonPropertyName("awardPeriod")]
    public AwardPeriodPayload AwardPeriod { get; set; } = default!;

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("milestoneBreakdown")]
    public List<MilestoneBreakdownPayload> MilestoneBreakdown { get; set; } = new();

    [JsonPropertyName("members")]
    public List<EligibleMemberPayload> Members { get; set; } = new();
}

/// <summary>
/// Phần <c>data</c> của <c>GET /api/Eligibility/Unassigned</c> (mục 6.3 hợp đồng API).
/// </summary>
public sealed class UnassignedListPayload
{
    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("members")]
    public List<UnassignedMemberPayload> Members { get; set; } = new();
}

/// <summary>
/// Phần <c>data</c> của <c>GET /api/Eligibility/UnassignedCount</c> (mục 6.4 hợp đồng API).
/// </summary>
public sealed class UnassignedCountPayload
{
    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}

/// <summary>
/// Phần <c>data</c> của <c>GET /api/Dashboard</c> (mục 6.1 hợp đồng API).
/// </summary>
public sealed class DashboardPayload
{
    [JsonPropertyName("today")]
    public DateOnly Today { get; set; }

    [JsonPropertyName("currentYear")]
    public int CurrentYear { get; set; }

    [JsonPropertyName("unitName")]
    public string? UnitName { get; set; }

    [JsonPropertyName("memberCount")]
    public int MemberCount { get; set; }

    [JsonPropertyName("periodCount")]
    public int PeriodCount { get; set; }

    [JsonPropertyName("upcomingPeriod")]
    public UpcomingPeriodPayload? UpcomingPeriod { get; set; }

    [JsonPropertyName("eligibleMembers")]
    public List<EligibleMemberPayload> EligibleMembers { get; set; } = new();

    [JsonPropertyName("warnings")]
    public DashboardWarningsPayload Warnings { get; set; } = default!;
}

/// <summary>
/// Đợt sắp tới của Dashboard (QT8).
/// </summary>
public sealed class UpcomingPeriodPayload
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("isNextYear")]
    public bool IsNextYear { get; set; }

    [JsonPropertyName("fromDate")]
    public DateOnly FromDate { get; set; }

    [JsonPropertyName("toDate")]
    public DateOnly ToDate { get; set; }

    [JsonPropertyName("fromDisplay")]
    public string FromDisplay { get; set; } = default!;

    [JsonPropertyName("toDisplay")]
    public string ToDisplay { get; set; } = default!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = default!;

    [JsonPropertyName("daysRemaining")]
    public int? DaysRemaining { get; set; }

    [JsonPropertyName("eligibleCount")]
    public int EligibleCount { get; set; }

    [JsonPropertyName("milestoneBreakdown")]
    public List<MilestoneBreakdownPayload> MilestoneBreakdown { get; set; } = new();
}

/// <summary>
/// Khối cảnh báo của Dashboard.
/// </summary>
public sealed class DashboardWarningsPayload
{
    [JsonPropertyName("noMembers")]
    public bool NoMembers { get; set; }

    [JsonPropertyName("noPeriods")]
    public bool NoPeriods { get; set; }

    [JsonPropertyName("unassignedYear")]
    public int UnassignedYear { get; set; }

    [JsonPropertyName("unassignedCount")]
    public int UnassignedCount { get; set; }

    [JsonPropertyName("overlaps")]
    public List<PeriodOverlapPayload> Overlaps { get; set; } = new();

    [JsonPropertyName("gaps")]
    public List<PeriodGapPayload> Gaps { get; set; } = new();
}
