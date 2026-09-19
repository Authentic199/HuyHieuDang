using System.Globalization;
using System.Text.Json;
using HuyHieuDang.Core.PartyBadges;

namespace HuyHieuDang.Core.UnitTests.Fixtures;

/// <summary>
/// Nạp bộ dữ liệu biên của QC (tests/fixtures/data) để test của Backend và bảng kết quả
/// mong đợi luôn dùng chung một nguồn số liệu.
/// </summary>
public static class FixtureData
{
    private static readonly Lazy<JsonDocument> ExpectedDocument = new(() => Load("expected.json"));
    private static readonly Lazy<IReadOnlyList<FixtureMember>> CoreMemberList = new(ReadCoreMembers);
    private static readonly Lazy<IReadOnlyDictionary<string, IReadOnlyList<FixturePeriod>>> PeriodSetMap = new(ReadPeriodSets);

    /// <summary>T0 — mốc thời gian mặc định của kế hoạch kiểm thử.</summary>
    public static DateOnly T0 => new(2026, 9, 19);

    /// <summary>T1 — hôm nay nằm trong Đợt 7/11.</summary>
    public static DateOnly T1 => new(2026, 10, 15);

    /// <summary>T2 — mọi đợt của 2026 đã qua.</summary>
    public static DateOnly T2 => new(2026, 12, 1);

    public static MilestoneSettings DefaultSettings => new(30, 90, 5);

    public static MilestoneSettings StepTenSettings => new(30, 90, 10);

    public static IReadOnlyList<FixtureMember> CoreMembers => CoreMemberList.Value;

    public static IReadOnlyList<FixturePeriod> MainPeriods => PeriodSetMap.Value["main"];

    public static IReadOnlyList<FixturePeriod> LeapEdgePeriods => PeriodSetMap.Value["leapEdge"];

    /// <summary>Hai đợt chồng lấn (dataset.py · PERIODS_OVERLAP).</summary>
    public static IReadOnlyList<FixturePeriod> OverlapPeriods { get; } =
    [
        new("PO1", "Đợt A", new AwardPeriod("Đợt A", 1, 10, 7, 11)),
        new("PO2", "Đợt B", new AwardPeriod("Đợt B", 1, 11, 30, 11)),
    ];

    /// <summary>Hai đợt phủ kín 01/01–31/12 (dataset.py · PERIODS_FULL_COVER).</summary>
    public static IReadOnlyList<FixturePeriod> FullCoverPeriods { get; } =
    [
        new("PF1", "Nửa đầu năm", new AwardPeriod("Nửa đầu năm", 1, 1, 30, 6)),
        new("PF2", "Nửa cuối năm", new AwardPeriod("Nửa cuối năm", 1, 7, 31, 12)),
    ];

    /// <summary>Bộ đợt chính với Đợt 2/9 nới Đến ngày sang 30/09.</summary>
    public static IReadOnlyList<FixturePeriod> WidenedP3Periods { get; } =
        MainPeriods
            .Select(x => x.Code == "P3"
                ? new FixturePeriod("P3", "Đợt 2/9", new AwardPeriod("Đợt 2/9", 15, 8, 30, 9))
                : x)
            .ToList();

    public static FixtureMember Member(string code) =>
        CoreMembers.SingleOrDefault(x => x.Code == code)
        ?? throw new InvalidOperationException($"Không có đảng viên mã {code} trong bộ lõi.");

    public static AwardPeriod Period(string code) =>
        MainPeriods.SingleOrDefault(x => x.Code == code)?.Period
        ?? throw new InvalidOperationException($"Không có đợt mã {code} trong bộ đợt chính.");

    /// <summary>Một kịch bản trong expected.json, kèm bộ đợt tương ứng.</summary>
    public static FixtureScenario Scenario(string name)
    {
        JsonElement node = ExpectedDocument.Value.RootElement.GetProperty("scenarios").GetProperty(name);
        JsonElement settingsNode = node.GetProperty("settings");

        return new FixtureScenario(
            name,
            DateOnly.Parse(node.GetProperty("today").GetString()!, CultureInfo.InvariantCulture),
            new MilestoneSettings(
                settingsNode.GetProperty("start_years").GetInt32(),
                settingsNode.GetProperty("end_years").GetInt32(),
                settingsNode.GetProperty("step_years").GetInt32()),
            PeriodSetFor(name),
            node);
    }

    private static IReadOnlyList<FixturePeriod> PeriodSetFor(string scenario) => scenario switch
    {
        "core_default_T0_leapEdgePeriod" => LeapEdgePeriods,
        "core_default_T0_overlap" => OverlapPeriods,
        "core_default_T0_fullCover" => FullCoverPeriods,
        "core_default_T0_noPeriod" => Array.Empty<FixturePeriod>(),
        "core_default_T0_widenedP3" => WidenedP3Periods,
        _ => MainPeriods,
    };

    private static JsonDocument Load(string fileName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "fixtures", fileName);
        return JsonDocument.Parse(File.ReadAllText(path));
    }

    private static IReadOnlyList<FixtureMember> ReadCoreMembers()
    {
        using JsonDocument document = Load("members-core.json");

        return document.RootElement.EnumerateArray()
            .Select(x => new FixtureMember(
                x.GetProperty("code").GetString()!,
                x.GetProperty("fullName").GetString()!,
                DateOnly.Parse(x.GetProperty("officialAdmissionDate").GetString()!, CultureInfo.InvariantCulture)))
            .ToList();
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<FixturePeriod>> ReadPeriodSets()
    {
        using JsonDocument document = Load("periods.json");

        return document.RootElement.EnumerateObject().ToDictionary(
            set => set.Name,
            set => (IReadOnlyList<FixturePeriod>)set.Value.EnumerateArray()
                .Select(x => new FixturePeriod(
                    x.GetProperty("code").GetString()!,
                    x.GetProperty("name").GetString()!,
                    new AwardPeriod(
                        x.GetProperty("name").GetString()!,
                        x.GetProperty("fromDay").GetInt32(),
                        x.GetProperty("fromMonth").GetInt32(),
                        x.GetProperty("toDay").GetInt32(),
                        x.GetProperty("toMonth").GetInt32())))
                .ToList());
    }
}

public sealed record FixtureMember(string Code, string FullName, DateOnly OfficialAdmissionDate);

public sealed record FixturePeriod(string Code, string Name, AwardPeriod Period);

public sealed record FixtureScenario(
    string Name,
    DateOnly Today,
    MilestoneSettings Settings,
    IReadOnlyList<FixturePeriod> Periods,
    JsonElement Node);
