using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.UnitTests.Fixtures;

namespace HuyHieuDang.Core.UnitTests;

/// <summary>QT11 — trạng thái đợt trong năm hiện tại: Đã qua / Đang diễn ra / Sắp tới + số ngày còn lại.</summary>
public sealed class Qt11PeriodStatusTests
{
    private static readonly IReadOnlyDictionary<PeriodStatus, string> StatusLabels =
        new Dictionary<PeriodStatus, string>
        {
            [PeriodStatus.Past] = "Đã qua",
            [PeriodStatus.Ongoing] = "Đang diễn ra",
            [PeriodStatus.Upcoming] = "Sắp tới",
        };

    private readonly PartyMilestoneCalculator sut = new();

    [Fact]
    public void GetPeriodStatus_WhenThePeriodAlreadyEnded_ReturnsPastWithoutDaysLeft()
    {
        // Act — Đợt 3/2 (15/01–05/03) so với T0.
        PeriodStatusResult status = sut.GetPeriodStatus(FixtureData.Period("P1"), FixtureData.T0);

        // Assert
        status.Status.ShouldBe(PeriodStatus.Past);
        status.DaysLeft.ShouldBeNull();
    }

    [Fact]
    public void GetPeriodStatus_WhenTodayIsInsideThePeriod_ReturnsOngoingWithoutDaysLeft()
    {
        // Act — Đợt 7/11 so với T1 = 15/10/2026.
        PeriodStatusResult status = sut.GetPeriodStatus(FixtureData.Period("P4"), FixtureData.T1);

        // Assert
        status.Status.ShouldBe(PeriodStatus.Ongoing);
        status.DaysLeft.ShouldBeNull();
    }

    [Fact]
    public void GetPeriodStatus_OnTheFirstDayOfThePeriod_ReturnsOngoing()
    {
        // Act
        PeriodStatusResult status = sut.GetPeriodStatus(FixtureData.Period("P4"), new DateOnly(2026, 10, 1));

        // Assert
        status.Status.ShouldBe(PeriodStatus.Ongoing);
    }

    [Fact]
    public void GetPeriodStatus_OnTheLastDayOfThePeriod_ReturnsOngoing()
    {
        // Act
        PeriodStatusResult status = sut.GetPeriodStatus(FixtureData.Period("P4"), new DateOnly(2026, 11, 7));

        // Assert
        status.Status.ShouldBe(PeriodStatus.Ongoing);
    }

    [Fact]
    public void GetPeriodStatus_OneDayAfterThePeriodEnds_ReturnsPast()
    {
        // Act
        PeriodStatusResult status = sut.GetPeriodStatus(FixtureData.Period("P4"), new DateOnly(2026, 11, 8));

        // Assert
        status.Status.ShouldBe(PeriodStatus.Past);
    }

    [Fact]
    public void GetPeriodStatus_WhenThePeriodHasNotStarted_ReturnsUpcomingWithDaysLeft()
    {
        // Act — Đợt 7/11 bắt đầu 01/10/2026, hôm nay T0 = 19/09/2026.
        PeriodStatusResult status = sut.GetPeriodStatus(FixtureData.Period("P4"), FixtureData.T0);

        // Assert
        status.Status.ShouldBe(PeriodStatus.Upcoming);
        status.DaysLeft.ShouldBe(12);
    }

    [Fact]
    public void GetPeriodStatus_OneDayBeforeThePeriodStarts_ReturnsOneDayLeft()
    {
        // Act
        PeriodStatusResult status = sut.GetPeriodStatus(FixtureData.Period("P4"), new DateOnly(2026, 9, 30));

        // Assert
        status.DaysLeft.ShouldBe(1);
    }

    [Fact]
    public void GetPeriodStatus_ForAPeriodStartingOn29FebruaryOfANonLeapYear_BindsTo28February()
    {
        // Act — Đợt nhuận 29/02–05/03 gắn năm 2026 thành 28/02–05/03.
        AwardPeriod leapEdge = FixtureData.LeapEdgePeriods[0].Period;
        PeriodStatusResult status = sut.GetPeriodStatus(leapEdge, new DateOnly(2026, 2, 28));

        // Assert
        status.Status.ShouldBe(PeriodStatus.Ongoing);
    }

    [Fact]
    public void BindToYear_ForAPeriodEndingOn29February_KeepsTheDayInALeapYear()
    {
        // Act
        PeriodOccurrence occurrence = sut.BindToYear(new AwardPeriod("Đợt nhuận", 1, 2, 29, 2), 2028);

        // Assert
        occurrence.From.ShouldBe(new DateOnly(2028, 2, 1));
        occurrence.To.ShouldBe(new DateOnly(2028, 2, 29));
    }

    [Theory]
    [InlineData("core_default_T0")]
    [InlineData("core_default_T1")]
    [InlineData("core_default_T2")]
    [InlineData("core_default_T0_leapEdgePeriod")]
    [InlineData("core_default_T0_fullCover")]
    [InlineData("core_default_T0_overlap")]
    [InlineData("core_default_T0_widenedP3")]
    public void GetPeriodStatus_ForEveryFixtureScenario_MatchesTheExpectedStatuses(string scenarioName)
    {
        // Arrange
        FixtureScenario scenario = FixtureData.Scenario(scenarioName);

        // Act
        List<(string Name, string Status, int? DaysLeft)> actual = scenario.Periods
            .Select(x =>
            {
                PeriodStatusResult status = sut.GetPeriodStatus(x.Period, scenario.Today);
                return (x.Name, StatusLabels[status.Status], status.DaysLeft);
            })
            .ToList();

        // Assert
        actual.ShouldBe(FixtureExpectations.PeriodStatuses(scenarioName));
    }
}
