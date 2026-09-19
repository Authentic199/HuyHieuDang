using System.Globalization;
using System.Text.Json;
using HuyHieuDang.Core.PartyBadges;

namespace HuyHieuDang.Core.QcTests.Support;

/// <summary>
/// Nạp bộ dữ liệu biên T25 (tests/fixtures/data) cho test của QC. Bản nạp này độc lập với
/// lớp nạp của Backend: nếu cả hai cùng đọc sai một chỗ thì đối chiếu không còn ý nghĩa.
/// </summary>
public static class QcFixtures
{
    /// <summary>T0 — mốc mặc định của kế hoạch kiểm thử (19/09/2026).</summary>
    public static DateOnly T0 => new(2026, 9, 19);

    /// <summary>T1 — hôm nay nằm trong Đợt 7/11 (15/10/2026).</summary>
    public static DateOnly T1 => new(2026, 10, 15);

    /// <summary>T2 — mọi đợt của 2026 đã qua (01/12/2026).</summary>
    public static DateOnly T2 => new(2026, 12, 1);

    /// <summary>Cài đặt mặc định 30 / 90 / 5.</summary>
    public static MilestoneSettings Default => new(30, 90, 5);

    /// <summary>Cài đặt Bước = 10.</summary>
    public static MilestoneSettings StepTen => new(30, 90, 10);

    private static readonly Lazy<IReadOnlyList<QcMember>> MemberList = new(ReadMembers);
    private static readonly Lazy<IReadOnlyDictionary<string, IReadOnlyList<QcPeriod>>> PeriodMap = new(ReadPeriods);
    private static readonly Lazy<JsonDocument> ExpectedDocument = new(() => Load("expected.json"));

    /// <summary>32 đảng viên của bộ lõi.</summary>
    public static IReadOnlyList<QcMember> CoreMembers => MemberList.Value;

    /// <summary>Bộ 4 đợt chính: Đợt 3/2, Đợt 19/5, Đợt 2/9, Đợt 7/11.</summary>
    public static IReadOnlyList<QcPeriod> MainPeriods => PeriodMap.Value["main"];

    /// <summary>Bộ đợt có biên 29/02.</summary>
    public static IReadOnlyList<QcPeriod> LeapEdgePeriods => PeriodMap.Value["leapEdge"];

    /// <summary>Hai đợt chồng lấn.</summary>
    public static IReadOnlyList<QcPeriod> OverlapPeriods { get; } =
    [
        new("PO1", "Đợt A", new AwardPeriod("Đợt A", 1, 10, 7, 11)),
        new("PO2", "Đợt B", new AwardPeriod("Đợt B", 1, 11, 30, 11)),
    ];

    /// <summary>Hai đợt phủ kín 01/01–31/12.</summary>
    public static IReadOnlyList<QcPeriod> FullCoverPeriods { get; } =
    [
        new("PF1", "Nửa đầu năm", new AwardPeriod("Nửa đầu năm", 1, 1, 30, 6)),
        new("PF2", "Nửa cuối năm", new AwardPeriod("Nửa cuối năm", 1, 7, 31, 12)),
    ];

    /// <summary>Tra đảng viên theo mã (B01, L02, M03...).</summary>
    public static QcMember Member(string code) =>
        CoreMembers.SingleOrDefault(x => x.Code == code)
        ?? throw new InvalidOperationException($"Bộ lõi không có đảng viên mã {code}.");

    /// <summary>Tra đợt chính theo mã (P1–P4).</summary>
    public static AwardPeriod Period(string code) =>
        MainPeriods.SingleOrDefault(x => x.Code == code)?.Period
        ?? throw new InvalidOperationException($"Bộ đợt chính không có đợt mã {code}.");

    /// <summary>Danh sách ngày vào Đảng chính thức của cả bộ lõi.</summary>
    public static IReadOnlyList<DateOnly> CoreAdmissions =>
        CoreMembers.Select(x => x.OfficialAdmissionDate).ToList();

    /// <summary>Một nhánh của expected.json, ví dụ <c>scenarios/core_default_T0_noPeriod</c>.</summary>
    public static JsonElement Expected(params string[] path)
    {
        JsonElement node = ExpectedDocument.Value.RootElement;

        foreach (string segment in path)
        {
            node = node.GetProperty(segment);
        }

        return node;
    }

    private static JsonDocument Load(string fileName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "fixtures", fileName);
        return JsonDocument.Parse(File.ReadAllText(path));
    }

    private static IReadOnlyList<QcMember> ReadMembers()
    {
        using JsonDocument document = Load("members-core.json");

        return document.RootElement.EnumerateArray()
            .Select(x => new QcMember(
                x.GetProperty("code").GetString()!,
                x.GetProperty("fullName").GetString()!,
                ParseDate(x.GetProperty("officialAdmissionDate").GetString()!)))
            .ToList();
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<QcPeriod>> ReadPeriods()
    {
        using JsonDocument document = Load("periods.json");

        return document.RootElement.EnumerateObject().ToDictionary(
            set => set.Name,
            set => (IReadOnlyList<QcPeriod>)set.Value.EnumerateArray()
                .Select(x => new QcPeriod(
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

    /// <summary>Đọc ngày kiểu <c>yyyy-MM-dd</c> của bộ dữ liệu.</summary>
    public static DateOnly ParseDate(string value) => DateOnly.Parse(value, CultureInfo.InvariantCulture);
}

/// <summary>Một đảng viên của bộ lõi.</summary>
public sealed record QcMember(string Code, string FullName, DateOnly OfficialAdmissionDate);

/// <summary>Một đợt của bộ dữ liệu, kèm mã để đối chiếu với expected.json.</summary>
public sealed record QcPeriod(string Code, string Name, AwardPeriod Period);
