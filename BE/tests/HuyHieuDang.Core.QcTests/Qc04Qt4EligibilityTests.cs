using System.Text.Json;
using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.QcTests.Support;

namespace HuyHieuDang.Core.QcTests;

/// <summary>QT4 — đủ điều kiện trong đợt, theo năm.</summary>
[Trait("Tang", "Logic")]
[Trait("QuyTac", "QT4")]
public sealed class Qc04Qt4EligibilityTests
{
    private readonly IPartyMilestoneCalculator sut = new PartyMilestoneCalculator();

    private IReadOnlyList<int> Default => sut.BuildMilestones(QcFixtures.Default);

    private IReadOnlyList<AwardPeriod> MainPeriods => QcFixtures.MainPeriods.Select(x => x.Period).ToList();

    [Fact(DisplayName = "U-415 · Đợt một ngày (Từ = Đến = 01/10) chỉ nhận đúng người tròn mốc hôm đó")]
    public void U415_OneDayPeriod_AcceptsOnlyTheExactAnniversary()
    {
        AwardPeriod oneDay = new("Đợt một ngày", 1, 10, 1, 10);

        sut.GetEligibleMilestone(QcFixtures.Member("B01").OfficialAdmissionDate, oneDay, 2026, Default).ShouldBe(30);
        sut.GetEligibleMilestone(QcFixtures.Member("B03").OfficialAdmissionDate, oneDay, 2026, Default).ShouldBeNull();
        sut.GetEligibleMilestone(QcFixtures.Member("B04").OfficialAdmissionDate, oneDay, 2026, Default).ShouldBeNull();
    }

    [Fact(DisplayName = "U-418 · Không ai đủ điều kiện ở hai đợt cùng một năm")]
    public void U418_NobodyIsEligibleInTwoPeriodsOfTheSameYear()
    {
        List<string> trung = new();

        foreach (int year in new[] { 2025, 2026, 2027, 2028 })
        {
            foreach (QcMember member in QcFixtures.CoreMembers)
            {
                List<string> matched = MainPeriods
                    .Where(x => sut.GetEligibleMilestone(member.OfficialAdmissionDate, x, year, Default) is not null)
                    .Select(x => x.Name)
                    .ToList();

                if (matched.Count > 1)
                {
                    trung.Add($"{member.Code} nam {year}: {string.Join(", ", matched)}");
                }
            }
        }

        trung.ShouldBeEmpty();
    }

    [Fact(DisplayName = "U-411/U-412 · N01 chỉ đủ điều kiện Đợt 7/11 của năm 2027, không phải 2026")]
    public void U411_NextYearSelectorChangesTheResult()
    {
        DateOnly admission = QcFixtures.Member("N01").OfficialAdmissionDate;
        AwardPeriod p4 = QcFixtures.Period("P4");

        sut.GetEligibleMilestone(admission, p4, 2026, Default).ShouldBeNull();
        sut.GetEligibleMilestone(admission, p4, 2027, Default).ShouldBe(30);
    }

    [Fact(DisplayName = "U-413/U-414 · Đợt có Từ ngày 29/02: thu về 28/02 ở 2026, giữ 29/02 ở 2028")]
    public void U413_PeriodStartingOn29February_BindsPerYear()
    {
        AwardPeriod leapStart = new("Đợt 29/02", 29, 2, 5, 3);

        sut.BindToYear(leapStart, 2026).From.ShouldBe(new DateOnly(2026, 2, 28));
        sut.BindToYear(leapStart, 2028).From.ShouldBe(new DateOnly(2028, 2, 29));

        // L01 vào Đảng 29/02/1996 → tròn mốc 30 vào 28/02/2026, đúng Từ ngày đã thu.
        sut.GetEligibleMilestone(QcFixtures.Member("L01").OfficialAdmissionDate, leapStart, 2026, Default)
            .ShouldBe(30);
    }

    [Fact(DisplayName = "U-408/U-409 · L02 chỉ đủ điều kiện ở năm nhuận 2028 với mốc 40")]
    public void U408_LeapDayMemberIsEligibleOnlyIn2028()
    {
        DateOnly admission = QcFixtures.Member("L02").OfficialAdmissionDate;
        AwardPeriod p1 = QcFixtures.Period("P1");

        sut.GetEligibleMilestone(admission, p1, 2026, Default).ShouldBeNull();
        sut.GetEligibleMilestone(admission, p1, 2028, Default).ShouldBe(40);
        sut.GetAnniversary(admission, 40).ShouldBe(new DateOnly(2028, 2, 29));
    }

    [Theory(DisplayName = "U-416/U-417 · Số người đủ điều kiện Đợt 7/11 năm 2026 theo Bước 5 và Bước 10")]
    [InlineData(5, 6)]
    [InlineData(10, 4)]
    public void U416_EligibleCountOfPeriod4In2026(int step, int expectedCount)
    {
        IReadOnlyList<int> milestones = sut.BuildMilestones(new MilestoneSettings(30, 90, step));

        List<int> awarded = QcFixtures.CoreMembers
            .Select(x => sut.GetEligibleMilestone(x.OfficialAdmissionDate, QcFixtures.Period("P4"), 2026, milestones))
            .Where(x => x is not null)
            .Select(x => x!.Value)
            .ToList();

        awarded.Count.ShouldBe(expectedCount);
    }

    [Fact(DisplayName = "U-416 · Phân bổ mốc của Đợt 7/11 năm 2026: 30×3, 35×1, 40×1, 45×1")]
    public void U416_MilestoneBreakdownOfPeriod4In2026()
    {
        Dictionary<int, int> byMilestone = QcFixtures.CoreMembers
            .Select(x => sut.GetEligibleMilestone(x.OfficialAdmissionDate, QcFixtures.Period("P4"), 2026, Default))
            .Where(x => x is not null)
            .GroupBy(x => x!.Value)
            .ToDictionary(x => x.Key, x => x.Count());

        byMilestone.ShouldBe(new Dictionary<int, int> { [30] = 3, [35] = 1, [40] = 1, [45] = 1 });
    }

    [Theory(DisplayName = "QC · Danh sách đủ điều kiện từng đợt/từng năm khớp expected.json của QC")]
    [InlineData("core_default_T0")]
    [InlineData("core_step10_T0")]
    public void EligibleListsMatchTheQcExpectedFile(string scenario)
    {
        JsonElement node = QcFixtures.Expected("scenarios", scenario);
        JsonElement settings = node.GetProperty("settings");
        IReadOnlyList<int> milestones = sut.BuildMilestones(new MilestoneSettings(
            settings.GetProperty("start_years").GetInt32(),
            settings.GetProperty("end_years").GetInt32(),
            settings.GetProperty("step_years").GetInt32()));

        List<string> lech = new();

        foreach (JsonProperty periodNode in node.GetProperty("eligibleByPeriod").EnumerateObject())
        {
            AwardPeriod period = QcFixtures.Period(periodNode.Name);

            foreach (JsonProperty yearNode in periodNode.Value.GetProperty("byYear").EnumerateObject())
            {
                int year = int.Parse(yearNode.Name);

                Dictionary<string, int> expected = yearNode.Value.GetProperty("rows")
                    .EnumerateArray()
                    .ToDictionary(x => x.GetProperty("code").GetString()!, x => x.GetProperty("milestone").GetInt32());

                Dictionary<string, int> actual = QcFixtures.CoreMembers
                    .Select(x => (x.Code, Milestone: sut.GetEligibleMilestone(x.OfficialAdmissionDate, period, year, milestones)))
                    .Where(x => x.Milestone is not null)
                    .ToDictionary(x => x.Code, x => x.Milestone!.Value);

                foreach (string code in expected.Keys.Union(actual.Keys))
                {
                    expected.TryGetValue(code, out int e);
                    actual.TryGetValue(code, out int a);

                    if (e != a)
                    {
                        lech.Add($"{periodNode.Name}/{year}/{code}: service {a}, QC {e}");
                    }
                }
            }
        }

        lech.ShouldBeEmpty();
    }

    [Fact(DisplayName = "QC · Đủ điều kiện khớp oracle của QC trên mọi đợt × mọi năm 2020–2035")]
    public void EligibilityMatchesTheQcOracleOverSixteenYears()
    {
        List<string> lech = new();
        List<AwardPeriod> periods = MainPeriods
            .Concat(QcFixtures.LeapEdgePeriods.Select(x => x.Period))
            .Concat(QcFixtures.FullCoverPeriods.Select(x => x.Period))
            .ToList();

        for (int year = 2020; year <= 2035; year++)
        {
            foreach (AwardPeriod period in periods)
            {
                foreach (QcMember member in QcFixtures.CoreMembers)
                {
                    int? actual = sut.GetEligibleMilestone(member.OfficialAdmissionDate, period, year, Default);
                    int? expected = QcOracle.EligibleMilestone(member.OfficialAdmissionDate, period, year, Default);

                    if (actual != expected)
                    {
                        lech.Add($"{member.Code} {period.Name} {year}: service {actual}, QC {expected}");
                    }
                }
            }
        }

        lech.ShouldBeEmpty();
    }
}
