using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.UnitTests.Fixtures;

namespace HuyHieuDang.Core.UnitTests;

/// <summary>QT4 — đủ điều kiện trong đợt theo năm: tồn tại mốc N sao cho F ≤ D + N năm ≤ T.</summary>
public sealed class Qt4EligibilityTests
{
    private readonly PartyMilestoneCalculator sut = new();

    private IReadOnlyList<int> Default => sut.BuildMilestones(FixtureData.DefaultSettings);

    private IReadOnlyList<int> StepTen => sut.BuildMilestones(FixtureData.StepTenSettings);

    [Fact]
    public void GetEligibleMilestone_WhenAnniversaryEqualsTheFirstDayOfThePeriod_ReturnsTheMilestone()
    {
        // Act — B01 tròn 30 đúng 01/10/2026, Từ ngày Đợt 7/11.
        int? milestone = sut.GetEligibleMilestone(
            FixtureData.Member("B01").OfficialAdmissionDate, FixtureData.Period("P4"), 2026, Default);

        // Assert
        milestone.ShouldBe(30);
    }

    [Fact]
    public void GetEligibleMilestone_WhenAnniversaryEqualsTheLastDayOfThePeriod_ReturnsTheMilestone()
    {
        // Act — B02 tròn 30 đúng 07/11/2026, Đến ngày Đợt 7/11.
        int? milestone = sut.GetEligibleMilestone(
            FixtureData.Member("B02").OfficialAdmissionDate, FixtureData.Period("P4"), 2026, Default);

        // Assert
        milestone.ShouldBe(30);
    }

    [Fact]
    public void GetEligibleMilestone_WhenAnniversaryIsOneDayBeforeThePeriod_ReturnsNull()
    {
        // Act — B03 tròn 30 vào 30/09/2026.
        int? milestone = sut.GetEligibleMilestone(
            FixtureData.Member("B03").OfficialAdmissionDate, FixtureData.Period("P4"), 2026, Default);

        // Assert
        milestone.ShouldBeNull();
    }

    [Fact]
    public void GetEligibleMilestone_WhenAnniversaryIsOneDayAfterThePeriod_ReturnsNull()
    {
        // Act — B04 tròn 30 vào 08/11/2026.
        int? milestone = sut.GetEligibleMilestone(
            FixtureData.Member("B04").OfficialAdmissionDate, FixtureData.Period("P4"), 2026, Default);

        // Assert
        milestone.ShouldBeNull();
    }

    [Fact]
    public void GetEligibleMilestone_WhenLeapDayAnniversaryShiftsTo28February_StillFallsInThePeriod()
    {
        // Act — L01 tròn 30 vào 28/02/2026, trong Đợt 3/2 (15/01–05/03).
        int? milestone = sut.GetEligibleMilestone(
            FixtureData.Member("L01").OfficialAdmissionDate, FixtureData.Period("P1"), 2026, Default);

        // Assert
        milestone.ShouldBe(30);
    }

    [Fact]
    public void GetEligibleMilestone_WhenThePeriodStartsOn29FebruaryOfANonLeapYear_BindsTo28February()
    {
        // Act — Đợt nhuận 29/02–05/03 gắn năm 2026 thành 28/02–05/03; L01 tròn mốc đúng 28/02.
        AwardPeriod leapEdge = FixtureData.LeapEdgePeriods[0].Period;
        int? milestone = sut.GetEligibleMilestone(
            FixtureData.Member("L01").OfficialAdmissionDate, leapEdge, 2026, Default);

        // Assert
        milestone.ShouldBe(30);
    }

    [Fact]
    public void GetEligibleMilestone_ForTheMemberAtTheHighestMilestone_StillReturns90()
    {
        // Act — M02 tròn mốc 90 vào 01/05/2026, Từ ngày Đợt 19/5.
        int? milestone = sut.GetEligibleMilestone(
            FixtureData.Member("M02").OfficialAdmissionDate, FixtureData.Period("P2"), 2026, Default);

        // Assert
        milestone.ShouldBe(90);
    }

    [Fact]
    public void GetEligibleMilestone_WhenTheMemberHasPassedTheHighestMilestone_ReturnsNull()
    {
        // Act — M01 vào Đảng 01/05/1935: mốc 90 rơi vào 1935 + 90 = 2025, không phải 2026.
        int? milestone = sut.GetEligibleMilestone(
            FixtureData.Member("M01").OfficialAdmissionDate, FixtureData.Period("P2"), 2026, Default);

        // Assert
        milestone.ShouldBeNull();
    }

    [Fact]
    public void GetEligibleMilestone_WithStepTen_LosesTheMemberWhoseMilestoneDisappears()
    {
        // Act — S01 tròn mốc 35 vào 05/10/2026 trong Đợt 7/11; mốc 35 biến mất khi Bước = 10.
        DateOnly admission = FixtureData.Member("S01").OfficialAdmissionDate;

        // Assert
        sut.GetEligibleMilestone(admission, FixtureData.Period("P4"), 2026, Default).ShouldBe(35);
        sut.GetEligibleMilestone(admission, FixtureData.Period("P4"), 2026, StepTen).ShouldBeNull();
    }

    [Fact]
    public void GetEligibleMilestone_WithStepTen_KeepsTheMemberWhoseMilestoneSurvives()
    {
        // Act — S02 tròn mốc 40 vào 20/10/2026, giữ nguyên ở cả hai cài đặt.
        DateOnly admission = FixtureData.Member("S02").OfficialAdmissionDate;

        // Assert
        sut.GetEligibleMilestone(admission, FixtureData.Period("P4"), 2026, Default).ShouldBe(40);
        sut.GetEligibleMilestone(admission, FixtureData.Period("P4"), 2026, StepTen).ShouldBe(40);
    }

    [Fact]
    public void GetEligibleMilestone_ForAYearWithoutAnyMilestone_ReturnsNull()
    {
        // Act — N02 tròn mốc 30 vào 15/01/2030, không có mốc nào trong 2026.
        int? milestone = sut.GetEligibleMilestone(
            FixtureData.Member("N02").OfficialAdmissionDate, FixtureData.Period("P1"), 2026, Default);

        // Assert
        milestone.ShouldBeNull();
    }

    [Theory]
    [InlineData("core_default_T0")]
    [InlineData("core_step10_T0")]
    [InlineData("core_default_T0_leapEdgePeriod")]
    [InlineData("core_default_T0_fullCover")]
    [InlineData("core_default_T0_overlap")]
    [InlineData("core_default_T0_widenedP3")]
    public void GetEligibleMilestone_OverTheWholeCoreDataset_MatchesTheExpectedCounts(string scenarioName)
    {
        // Arrange
        FixtureScenario scenario = FixtureData.Scenario(scenarioName);
        IReadOnlyList<int> milestones = sut.BuildMilestones(scenario.Settings);

        // Act & Assert
        foreach (string periodCode in FixtureExpectations.EligiblePeriodCodes(scenarioName))
        {
            AwardPeriod period = scenario.Periods.Single(x => x.Code == periodCode).Period;

            foreach (int year in FixtureExpectations.EligibleYears(scenarioName, periodCode))
            {
                List<int> actual = FixtureData.CoreMembers
                    .Select(m => sut.GetEligibleMilestone(m.OfficialAdmissionDate, period, year, milestones))
                    .Where(x => x is not null)
                    .Select(x => x!.Value)
                    .ToList();

                (int total, IReadOnlyDictionary<int, int> byMilestone) =
                    FixtureExpectations.Eligible(scenarioName, periodCode, year);

                actual.Count.ShouldBe(total, $"{scenarioName} · {periodCode} · {year}: sai tổng số đủ điều kiện.");

                List<string> actualBreakdown = actual.GroupBy(x => x).OrderBy(g => g.Key)
                    .Select(g => $"{g.Key}={g.Count()}").ToList();
                List<string> expectedBreakdown = byMilestone.OrderBy(x => x.Key)
                    .Select(x => $"{x.Key}={x.Value}").ToList();

                actualBreakdown.ShouldBe(
                    expectedBreakdown, $"{scenarioName} · {periodCode} · {year}: sai phân bổ theo mốc.");
            }
        }
    }
}
