using System.Text.Json;
using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.QcTests.Support;

namespace HuyHieuDang.Core.QcTests;

/// <summary>QT7 — chưa thuộc đợt nào.</summary>
[Trait("Tang", "Logic")]
[Trait("QuyTac", "QT7")]
public sealed class Qc07Qt7MissedMilestoneTests
{
    private readonly IPartyMilestoneCalculator sut = new PartyMilestoneCalculator();

    private IReadOnlyList<int> Default => sut.BuildMilestones(QcFixtures.Default);

    [Fact(DisplayName = "U-712 · Không cài đợt nào thì 27 người của bộ lõi đều bị sót ở 2026")]
    public void U712_WithoutAnyPeriod_TwentySevenCoreMembersAreMissed()
    {
        int missed = QcFixtures.CoreMembers
            .Count(x => sut.GetMissedMilestone(x.OfficialAdmissionDate, [], 2026, Default) is not null);

        missed.ShouldBe(QcFixtures.Expected("scenarios", "core_default_T0_noPeriod", "badgeCurrentYear").GetInt32());
        missed.ShouldBe(27);
    }

    [Fact(DisplayName = "U-711 · Bộ đợt phủ kín cả năm thì không ai bị sót")]
    public void U711_FullCoverLeavesNobodyBehind()
    {
        List<AwardPeriod> fullCover = QcFixtures.FullCoverPeriods.Select(x => x.Period).ToList();

        QcFixtures.CoreMembers
            .Count(x => sut.GetMissedMilestone(x.OfficialAdmissionDate, fullCover, 2026, Default) is not null)
            .ShouldBe(0);
    }

    [Theory(DisplayName = "QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch")]
    [InlineData("core_default_T0", "main")]
    [InlineData("core_default_T0_widenedP3", "widened")]
    [InlineData("core_default_T0_leapEdgePeriod", "leapEdge")]
    [InlineData("core_default_T0_overlap", "overlap")]
    [InlineData("core_default_T0_fullCover", "fullCover")]
    [InlineData("core_default_T0_noPeriod", "none")]
    public void MissedCountMatchesTheQcExpectedFile(string scenario, string periodSet)
    {
        IReadOnlyList<AwardPeriod> periods = PeriodSet(periodSet);

        int missed = QcFixtures.CoreMembers
            .Count(x => sut.GetMissedMilestone(x.OfficialAdmissionDate, periods, 2026, Default) is not null);

        missed.ShouldBe(QcFixtures.Expected("scenarios", scenario, "badgeCurrentYear").GetInt32());
    }

    [Theory(DisplayName = "QC · Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp expected.json")]
    [InlineData("core_default_T0", "main")]
    [InlineData("core_default_T0_widenedP3", "widened")]
    [InlineData("core_default_T0_leapEdgePeriod", "leapEdge")]
    [InlineData("core_default_T0_overlap", "overlap")]
    public void MissedRowsMatchTheQcExpectedFile(string scenario, string periodSet)
    {
        IReadOnlyList<AwardPeriod> periods = PeriodSet(periodSet);
        List<string> lech = new();

        Dictionary<string, string> expected = QcFixtures
            .Expected("scenarios", scenario, "missedByYear", "2026", "rows")
            .EnumerateArray()
            .ToDictionary(
                x => x.GetProperty("code").GetString()!,
                x => $"{x.GetProperty("milestone").GetInt32()} {x.GetProperty("anniversary").GetString()} {x.GetProperty("gapLabel").GetString()}");

        foreach (QcMember member in QcFixtures.CoreMembers)
        {
            MissedMilestone? missed = sut.GetMissedMilestone(member.OfficialAdmissionDate, periods, 2026, Default);
            string? actual = missed is null
                ? null
                : $"{missed.Milestone} {missed.Anniversary:yyyy-MM-dd} {missed.Gap.Label}";

            expected.TryGetValue(member.Code, out string? want);

            if (actual != want)
            {
                lech.Add($"{member.Code}: service [{actual ?? "không sót"}], QC [{want ?? "không sót"}]");
            }
        }

        lech.ShouldBeEmpty();
    }

    [Fact(DisplayName = "U-710 · Đổi Bước sang 10 thì S04 rời danh sách bị sót, còn 6 người")]
    public void U710_StepTenDropsOneMissedMember()
    {
        IReadOnlyList<AwardPeriod> main = QcFixtures.MainPeriods.Select(x => x.Period).ToList();
        IReadOnlyList<int> stepTen = sut.BuildMilestones(QcFixtures.StepTen);

        QcFixtures.CoreMembers
            .Count(x => sut.GetMissedMilestone(x.OfficialAdmissionDate, main, 2026, stepTen) is not null)
            .ShouldBe(6);

        sut.GetMissedMilestone(QcFixtures.Member("S04").OfficialAdmissionDate, main, 2026, stepTen).ShouldBeNull();
    }

    [Fact(DisplayName = "QC · Người bị sót và người đủ điều kiện là hai tập rời nhau")]
    public void MissedAndEligibleAreDisjoint()
    {
        IReadOnlyList<AwardPeriod> main = QcFixtures.MainPeriods.Select(x => x.Period).ToList();
        List<string> loi = new();

        for (int year = 2025; year <= 2028; year++)
        {
            foreach (QcMember member in QcFixtures.CoreMembers)
            {
                bool eligible = main.Any(x =>
                    sut.GetEligibleMilestone(member.OfficialAdmissionDate, x, year, Default) is not null);
                bool missed = sut.GetMissedMilestone(member.OfficialAdmissionDate, main, year, Default) is not null;

                if (eligible && missed)
                {
                    loi.Add($"{member.Code} nam {year} vua du dieu kien vua bi sot");
                }
            }
        }

        loi.ShouldBeEmpty();
    }

    // LỖI QC-02 — Khi chưa cài đợt nào, nhãn khoảng trống của service là "Trước đợt đầu tiên",
    // trong khi oracle của QC (tests/fixtures/qt_reference.py) và expected.json ghi
    // "Sau đợt cuối cùng". Hai bên phải thống nhất vì nhãn này hiện thẳng lên UC-40.
    [Fact(DisplayName = "QC-02 · Nhãn khoảng trống khi chưa cài đợt nào phải khớp expected.json")]
    public void Qc02_GapLabelWithoutAnyPeriod_MatchesTheExpectedFile()
    {
        JsonElement row = QcFixtures
            .Expected("scenarios", "core_default_T0_noPeriod", "missedByYear", "2026", "rows")
            .EnumerateArray()
            .First();

        MissedMilestone? missed = sut.GetMissedMilestone(
            QcFixtures.Member(row.GetProperty("code").GetString()!).OfficialAdmissionDate, [], 2026, Default);

        missed.ShouldNotBeNull();
        missed.Gap.Label.ShouldBe(row.GetProperty("gapLabel").GetString());
    }

    private static IReadOnlyList<AwardPeriod> PeriodSet(string name) => name switch
    {
        "main" => QcFixtures.MainPeriods.Select(x => x.Period).ToList(),
        "leapEdge" => QcFixtures.LeapEdgePeriods.Select(x => x.Period).ToList(),
        "overlap" => QcFixtures.OverlapPeriods.Select(x => x.Period).ToList(),
        "fullCover" => QcFixtures.FullCoverPeriods.Select(x => x.Period).ToList(),
        "widened" => QcFixtures.MainPeriods
            .Select(x => x.Code == "P3" ? new AwardPeriod("Đợt 2/9", 15, 8, 30, 9) : x.Period)
            .ToList(),
        _ => [],
    };
}
