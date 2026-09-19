using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.UnitTests.Fixtures;

namespace HuyHieuDang.Core.UnitTests;

/// <summary>QT8 — đợt sắp tới: Đến ≥ hôm nay và Từ nhỏ nhất; hết đợt thì lấy đợt sớm nhất năm sau.</summary>
public sealed class Qt8UpcomingPeriodTests
{
    private readonly PartyMilestoneCalculator sut = new();

    private static IReadOnlyList<AwardPeriod> Main =>
        FixtureData.MainPeriods.Select(x => x.Period).ToList();

    [Fact]
    public void GetUpcomingPeriod_AtT0_ReturnsTheNextPeriodOfTheCurrentYearWithDaysLeft()
    {
        // Act
        UpcomingPeriod? upcoming = sut.GetUpcomingPeriod(Main, FixtureData.T0);

        // Assert
        upcoming.ShouldNotBeNull();
        upcoming.Occurrence.Period.Name.ShouldBe("Đợt 7/11");
        upcoming.Occurrence.Year.ShouldBe(2026);
        upcoming.Occurrence.From.ShouldBe(new DateOnly(2026, 10, 1));
        upcoming.Occurrence.To.ShouldBe(new DateOnly(2026, 11, 7));
        upcoming.Status.Status.ShouldBe(PeriodStatus.Upcoming);
        upcoming.Status.DaysLeft.ShouldBe(12);
    }

    [Fact]
    public void GetUpcomingPeriod_WhenTodayIsInsideAPeriod_ReturnsThatPeriodAsOngoing()
    {
        // Act — T1 = 15/10/2026 nằm trong Đợt 7/11.
        UpcomingPeriod? upcoming = sut.GetUpcomingPeriod(Main, FixtureData.T1);

        // Assert
        upcoming.ShouldNotBeNull();
        upcoming.Occurrence.Period.Name.ShouldBe("Đợt 7/11");
        upcoming.Status.Status.ShouldBe(PeriodStatus.Ongoing);
        upcoming.Status.DaysLeft.ShouldBeNull();
    }

    [Fact]
    public void GetUpcomingPeriod_WhenEveryPeriodOfTheYearHasPassed_ReturnsTheEarliestOfNextYear()
    {
        // Act — T2 = 01/12/2026, mọi đợt 2026 đã qua.
        UpcomingPeriod? upcoming = sut.GetUpcomingPeriod(Main, FixtureData.T2);

        // Assert
        upcoming.ShouldNotBeNull();
        upcoming.Occurrence.Period.Name.ShouldBe("Đợt 3/2");
        upcoming.Occurrence.Year.ShouldBe(2027);
        upcoming.Occurrence.From.ShouldBe(new DateOnly(2027, 1, 15));
        upcoming.Status.DaysLeft.ShouldBe(45);
    }

    [Fact]
    public void GetUpcomingPeriod_OnTheLastDayOfAPeriod_StillReturnsThatPeriodAsOngoing()
    {
        // Act — 07/11/2026 là Đến ngày của Đợt 7/11.
        UpcomingPeriod? upcoming = sut.GetUpcomingPeriod(Main, new DateOnly(2026, 11, 7));

        // Assert
        upcoming.ShouldNotBeNull();
        upcoming.Occurrence.Period.Name.ShouldBe("Đợt 7/11");
        upcoming.Status.Status.ShouldBe(PeriodStatus.Ongoing);
    }

    [Fact]
    public void GetUpcomingPeriod_OneDayAfterTheLastPeriodEnds_RollsOverToNextYear()
    {
        // Act
        UpcomingPeriod? upcoming = sut.GetUpcomingPeriod(Main, new DateOnly(2026, 11, 8));

        // Assert
        upcoming.ShouldNotBeNull();
        upcoming.Occurrence.Year.ShouldBe(2027);
        upcoming.Occurrence.Period.Name.ShouldBe("Đợt 3/2");
    }

    [Fact]
    public void GetUpcomingPeriod_WhenThereIsNoPeriod_ReturnsNull()
    {
        // Act & Assert
        sut.GetUpcomingPeriod([], FixtureData.T0).ShouldBeNull();
        FixtureExpectations.Upcoming("core_default_T0_noPeriod").ShouldBeNull();
    }

    [Fact]
    public void GetUpcomingPeriod_ForAPeriodStartingOn29February_BindsNextYearTo28February()
    {
        // Act — Đợt nhuận 29/02–05/03 đã qua ở 2026, đợt sắp tới là bản 2027 (không nhuận).
        UpcomingPeriod? upcoming =
            sut.GetUpcomingPeriod(FixtureData.LeapEdgePeriods.Select(x => x.Period).ToList(), FixtureData.T0);

        // Assert
        upcoming.ShouldNotBeNull();
        upcoming.Occurrence.Year.ShouldBe(2027);
        upcoming.Occurrence.From.ShouldBe(new DateOnly(2027, 2, 28));
        upcoming.Status.DaysLeft.ShouldBe(162);
    }

    [Fact]
    public void GetUpcomingPeriod_WhenTwoPeriodsOverlap_PicksTheOneWithTheEarliestStart()
    {
        // Act — Đợt A 01/10–07/11 và Đợt B 01/11–30/11.
        UpcomingPeriod? upcoming =
            sut.GetUpcomingPeriod(FixtureData.OverlapPeriods.Select(x => x.Period).ToList(), FixtureData.T0);

        // Assert
        upcoming.ShouldNotBeNull();
        upcoming.Occurrence.Period.Name.ShouldBe("Đợt A");
    }

    [Theory]
    [InlineData("core_default_T0")]
    [InlineData("core_default_T1")]
    [InlineData("core_default_T2")]
    [InlineData("core_default_T0_leapEdgePeriod")]
    [InlineData("core_default_T0_fullCover")]
    [InlineData("core_default_T0_overlap")]
    [InlineData("core_default_T0_widenedP3")]
    [InlineData("core_default_T0_noPeriod")]
    public void GetUpcomingPeriod_ForEveryFixtureScenario_MatchesTheExpectedPeriod(string scenarioName)
    {
        // Arrange
        FixtureScenario scenario = FixtureData.Scenario(scenarioName);

        // Act
        UpcomingPeriod? upcoming =
            sut.GetUpcomingPeriod(scenario.Periods.Select(x => x.Period).ToList(), scenario.Today);

        // Assert
        (string Name, int Year, DateOnly From, DateOnly To, string Status, int? DaysLeft)? expected =
            FixtureExpectations.Upcoming(scenarioName);

        if (expected is null)
        {
            upcoming.ShouldBeNull();
            return;
        }

        upcoming.ShouldNotBeNull();
        upcoming.Occurrence.Period.Name.ShouldBe(expected.Value.Name);
        upcoming.Occurrence.Year.ShouldBe(expected.Value.Year);
        upcoming.Occurrence.From.ShouldBe(expected.Value.From);
        upcoming.Occurrence.To.ShouldBe(expected.Value.To);
        upcoming.Status.DaysLeft.ShouldBe(expected.Value.DaysLeft);
    }
}
