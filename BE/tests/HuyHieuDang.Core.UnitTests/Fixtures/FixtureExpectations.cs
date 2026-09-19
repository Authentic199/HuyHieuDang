using System.Text.Json;

namespace HuyHieuDang.Core.UnitTests.Fixtures;

/// <summary>
/// Đọc các con số mong đợi trong tests/fixtures/data/expected.json — bảng oracle do QC sinh ra
/// từ qt_reference.py. Backend không được chép lại con số, phải đối chiếu thẳng với tệp này.
/// </summary>
public static class FixtureExpectations
{
    private static readonly Lazy<JsonDocument> Document = new(() =>
        JsonDocument.Parse(File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "fixtures", "expected.json"))));

    private static JsonElement Root => Document.Value.RootElement;

    public static int PartyAge(string memberCode) => MemberRow(memberCode).GetProperty("partyAge").GetInt32();

    public static int? NextMilestone(string memberCode) => ReadNullableInt(MemberRow(memberCode), "nextMilestone");

    public static DateOnly? NextAnniversary(string memberCode) =>
        ReadNullableDate(MemberRow(memberCode), "nextAnniversary");

    /// <summary>Tổng số người đủ điều kiện và phân bổ theo mốc của một đợt trong một năm.</summary>
    public static (int Total, IReadOnlyDictionary<int, int> ByMilestone) Eligible(
        string scenario, string periodCode, int year)
    {
        JsonElement node = Scenario(scenario)
            .GetProperty("eligibleByPeriod")
            .GetProperty(periodCode)
            .GetProperty("byYear")
            .GetProperty(year.ToString());

        Dictionary<int, int> byMilestone = node.TryGetProperty("byMilestone", out JsonElement breakdown)
            ? breakdown.EnumerateObject().ToDictionary(x => int.Parse(x.Name), x => x.Value.GetInt32())
            : [];

        return (node.GetProperty("total").GetInt32(), byMilestone);
    }

    public static IReadOnlyList<string> EligiblePeriodCodes(string scenario) =>
        Scenario(scenario).GetProperty("eligibleByPeriod").EnumerateObject().Select(x => x.Name).ToList();

    public static IReadOnlyList<int> EligibleYears(string scenario, string periodCode) =>
        Scenario(scenario)
            .GetProperty("eligibleByPeriod")
            .GetProperty(periodCode)
            .GetProperty("byYear")
            .EnumerateObject()
            .Select(x => int.Parse(x.Name))
            .ToList();

    public static IReadOnlyList<(string Code, int Milestone, DateOnly Anniversary, string GapLabel)> Missed(
        string scenario, int year)
    {
        JsonElement node = Scenario(scenario).GetProperty("missedByYear").GetProperty(year.ToString());

        return node.GetProperty("rows").EnumerateArray()
            .Select(x => (
                x.GetProperty("code").GetString()!,
                x.GetProperty("milestone").GetInt32(),
                DateOnly.Parse(x.GetProperty("anniversary").GetString()!),
                x.GetProperty("gapLabel").GetString()!))
            .ToList();
    }

    public static IReadOnlyList<(DateOnly From, DateOnly To, string Label)> Gaps(string scenario, int year)
    {
        JsonElement node = Scenario(scenario).GetProperty("gapsByYear").GetProperty(year.ToString());

        return node.EnumerateArray()
            .Select(x => (
                DateOnly.Parse(x.GetProperty("from").GetString()!),
                DateOnly.Parse(x.GetProperty("to").GetString()!),
                x.GetProperty("label").GetString()!))
            .ToList();
    }

    public static IReadOnlyList<(string First, string Second)> Overlaps(string scenario) =>
        Scenario(scenario).GetProperty("overlapWarnings").EnumerateArray()
            .Select(x => (x[0].GetString()!, x[1].GetString()!))
            .ToList();

    public static (string Name, int Year, DateOnly From, DateOnly To, string Status, int? DaysLeft)? Upcoming(
        string scenario)
    {
        JsonElement node = Scenario(scenario).GetProperty("upcomingPeriod");
        if (node.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return (
            node.GetProperty("name").GetString()!,
            node.GetProperty("year").GetInt32(),
            DateOnly.Parse(node.GetProperty("boundFrom").GetString()!),
            DateOnly.Parse(node.GetProperty("boundTo").GetString()!),
            node.GetProperty("status").GetString()!,
            ReadNullableInt(node, "daysLeft"));
    }

    public static IReadOnlyList<(string Name, string Status, int? DaysLeft)> PeriodStatuses(string scenario) =>
        Scenario(scenario).GetProperty("periodStatuses").EnumerateArray()
            .Select(x => (
                x.GetProperty("name").GetString()!,
                x.GetProperty("status").GetString()!,
                ReadNullableInt(x, "daysLeft")))
            .ToList();

    private static JsonElement Scenario(string name) => Root.GetProperty("scenarios").GetProperty(name);

    private static JsonElement MemberRow(string memberCode) =>
        Root.GetProperty("memberList").GetProperty("T0_default").EnumerateArray()
            .First(x => x.GetProperty("code").GetString() == memberCode);

    private static int? ReadNullableInt(JsonElement node, string property) =>
        node.GetProperty(property) is { ValueKind: not JsonValueKind.Null } value ? value.GetInt32() : null;

    private static DateOnly? ReadNullableDate(JsonElement node, string property) =>
        node.GetProperty(property) is { ValueKind: not JsonValueKind.Null } value
            ? DateOnly.Parse(value.GetString()!)
            : null;
}
