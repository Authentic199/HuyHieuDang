using HuyHieuDang.Core.PartyBadges;

namespace HuyHieuDang.Core.UnitTests;

/// <summary>
/// QT4, QT6, QT7, QT8, QT11 với đợt vắt qua 31/12 — ca thật của đảng ủy: đợt "Giao thừa"
/// chạy từ 01/12 tới 28/02 năm sau. Năm neo của một lần diễn ra luôn là năm chứa Từ ngày.
/// </summary>
public sealed class Qt6SpanningYearPeriodTests
{
    private readonly PartyMilestoneCalculator sut = new();

    /// <summary>Đợt vắt năm của ca thật: 01/12 – 28/02.</summary>
    private static AwardPeriod Spanning => new("Đợt Giao thừa", 1, 12, 28, 2);

    /// <summary>Đợt thường nằm gọn trong năm, dùng để ghép cảnh báo.</summary>
    private static AwardPeriod Spring => new("Đợt 3/2", 20, 1, 3, 2);

    /// <summary>Chỉ một mình đợt vắt năm.</summary>
    private static IReadOnlyList<AwardPeriod> SpanningOnly =>
    [
        Spanning,
    ];

    /// <summary>Đợt vắt năm đứng cạnh một đợt thường.</summary>
    private static IReadOnlyList<AwardPeriod> SpanningAndSpring =>
    [
        Spanning,
        Spring,
    ];

    private static IReadOnlyList<int> Milestones =>
    [
        30, 35, 40, 45, 50,
    ];

    [Fact]
    public void BindToYear_ForASpanningPeriod_PutsTheToDateInTheFollowingYear()
    {
        // Act
        PeriodOccurrence occurrence = sut.BindToYear(Spanning, 2026);

        // Assert
        occurrence.Year.ShouldBe(2026);
        occurrence.From.ShouldBe(new DateOnly(2026, 12, 1));
        occurrence.To.ShouldBe(new DateOnly(2027, 2, 28));
    }

    [Fact]
    public void BindToYear_ForASpanningPeriodEndingOn29February_KeepsTheLeapDayInALeapYear()
    {
        // Act — 29/02 chỉ có thật ở năm nhuận; năm kết thúc mới là năm quyết định.
        PeriodOccurrence occurrence = sut.BindToYear(new AwardPeriod("Đợt nhuận", 1, 12, 29, 2), 2027);

        // Assert
        occurrence.To.ShouldBe(new DateOnly(2028, 2, 29));
    }

    [Fact]
    public void GetEligibleMilestone_WhenTheAnniversaryFallsInTheFirstCalendarYear_ReturnsTheMilestone()
    {
        // Act — tròn 30 năm ngày 15/12/2026, nằm ở nửa đầu đợt neo năm 2026.
        int? milestone = sut.GetEligibleMilestone(new DateOnly(1996, 12, 15), Spanning, 2026, Milestones);

        // Assert
        milestone.ShouldBe(30);
    }

    [Fact]
    public void GetEligibleMilestone_WhenTheAnniversaryFallsAfterTheNewYear_ReturnsTheMilestone()
    {
        // Act — tròn 40 năm ngày 10/02/2027, nằm ở nửa sau của chính đợt neo năm 2026.
        int? milestone = sut.GetEligibleMilestone(new DateOnly(1987, 2, 10), Spanning, 2026, Milestones);

        // Assert
        milestone.ShouldBe(40);
    }

    [Fact]
    public void GetEligibleMilestone_WhenTheAnniversaryFallsInTheGapOfTheYear_ReturnsNull()
    {
        // Act — tròn mốc ngày 10/06/2027 rơi ra ngoài cả hai đầu của đợt.
        int? milestone = sut.GetEligibleMilestone(new DateOnly(1987, 6, 10), Spanning, 2026, Milestones);

        // Assert
        milestone.ShouldBeNull();
    }

    [Fact]
    public void GetSlicesInYear_ForASpanningPeriod_ReturnsTheTailAndTheHeadOfTheYear()
    {
        // Act
        IReadOnlyList<PeriodSlice> slices = sut.GetSlicesInYear(SpanningOnly, 2027);

        // Assert — đuôi của lần neo 2026 rồi đầu của lần neo 2027.
        slices.Count.ShouldBe(2);
        slices[0].From.ShouldBe(new DateOnly(2027, 1, 1));
        slices[0].To.ShouldBe(new DateOnly(2027, 2, 28));
        slices[0].Occurrence.Year.ShouldBe(2026);
        slices[1].From.ShouldBe(new DateOnly(2027, 12, 1));
        slices[1].To.ShouldBe(new DateOnly(2027, 12, 31));
        slices[1].Occurrence.Year.ShouldBe(2027);
    }

    [Fact]
    public void GetGaps_ForASpanningPeriodAlone_LeavesOnlyTheMiddleOfTheYearUncovered()
    {
        // Act
        IReadOnlyList<DateGap> gaps = sut.GetGaps(SpanningOnly, 2027);

        // Assert — đầu năm và cuối năm đã được hai phần của đợt phủ.
        gaps.Count.ShouldBe(1);
        gaps[0].From.ShouldBe(new DateOnly(2027, 3, 1));
        gaps[0].To.ShouldBe(new DateOnly(2027, 11, 30));
        gaps[0].Kind.ShouldBe(GapKind.BetweenPeriods);
    }

    [Fact]
    public void GetGaps_WhenTwoSpanningPeriodsCoverTheWholeYear_ReturnsNoGap()
    {
        // Arrange — 01/12–31/05 và 01/06–30/11 nối nhau kín vòng năm.
        List<AwardPeriod> periods =
        [
            new("Đợt mùa đông", 1, 12, 31, 5),
            new("Đợt mùa hè", 1, 6, 30, 11),
        ];

        // Act & Assert
        sut.GetGaps(periods, 2027).ShouldBeEmpty();
    }

    [Fact]
    public void GetOverlaps_ForASpanningPeriodAlone_DoesNotReportThePeriodAgainstItself()
    {
        // Act — hai phần trong cùng một năm là cùng một đợt, không phải hai đợt chồng lấn.
        IReadOnlyList<PeriodOverlap> overlaps = sut.GetOverlaps(SpanningOnly, 2027);

        // Assert
        overlaps.ShouldBeEmpty();
    }

    [Fact]
    public void GetOverlaps_WhenAnotherPeriodTouchesTheTailOfTheSpanningPeriod_ReportsTheSharedDays()
    {
        // Act — đuôi 01/01–28/02/2027 dùng chung 20/01–03/02 với Đợt 3/2.
        IReadOnlyList<PeriodOverlap> overlaps = sut.GetOverlaps(SpanningAndSpring, 2027);

        // Assert
        overlaps.Count.ShouldBe(1);
        overlaps[0].From.ShouldBe(new DateOnly(2027, 1, 20));
        overlaps[0].To.ShouldBe(new DateOnly(2027, 2, 3));
    }

    [Fact]
    public void GetMissedMilestone_WhenTheAnniversaryFallsInTheTailOfTheSpanningPeriod_ReturnsNull()
    {
        // Act — 20/02/2027 nằm trong đuôi của lần neo năm 2026, không bị sót.
        MissedMilestone? missed =
            sut.GetMissedMilestone(new DateOnly(1997, 2, 20), SpanningOnly, 2027, Milestones);

        // Assert
        missed.ShouldBeNull();
    }

    [Fact]
    public void GetMissedMilestone_WhenTheAnniversaryFallsInTheMiddleOfTheYear_ReturnsTheGap()
    {
        // Act — 20/06/2027 rơi vào khoảng trống giữa hai phần của đợt.
        MissedMilestone? missed =
            sut.GetMissedMilestone(new DateOnly(1997, 6, 20), SpanningOnly, 2027, Milestones);

        // Assert
        missed.ShouldNotBeNull();
        missed.Milestone.ShouldBe(30);
        missed.Gap.From.ShouldBe(new DateOnly(2027, 3, 1));
    }

    [Fact]
    public void GetUpcomingPeriod_WhenTheSpanningPeriodStartedLastYear_PicksThatOccurrence()
    {
        // Act — hôm nay 15/01/2027, đợt neo năm 2026 vẫn đang mở.
        UpcomingPeriod? upcoming = sut.GetUpcomingPeriod(SpanningOnly, new DateOnly(2027, 1, 15));

        // Assert
        upcoming.ShouldNotBeNull();
        upcoming.Occurrence.Year.ShouldBe(2026);
        upcoming.Occurrence.From.ShouldBe(new DateOnly(2026, 12, 1));
        upcoming.Occurrence.To.ShouldBe(new DateOnly(2027, 2, 28));
        upcoming.Status.Status.ShouldBe(PeriodStatus.Ongoing);
    }

    [Fact]
    public void GetUpcomingPeriod_WhenTheSpanningPeriodOfLastYearHasEnded_PicksThisYearOccurrence()
    {
        // Act — hôm nay 01/03/2027, lần neo 2026 vừa đóng hôm qua.
        UpcomingPeriod? upcoming = sut.GetUpcomingPeriod(SpanningOnly, new DateOnly(2027, 3, 1));

        // Assert
        upcoming.ShouldNotBeNull();
        upcoming.Occurrence.Year.ShouldBe(2027);
        upcoming.Status.Status.ShouldBe(PeriodStatus.Upcoming);
        upcoming.Status.DaysLeft.ShouldBe(new DateOnly(2027, 12, 1).DayNumber - new DateOnly(2027, 3, 1).DayNumber);
    }

    [Fact]
    public void GetPeriodStatus_OnADayInsideTheOccurrenceStartedLastYear_IsOngoing()
    {
        // Act — bảng đợt ngày 15/01/2027 phải đọc là "Đang diễn ra" (QT11).
        PeriodStatusResult status = sut.GetPeriodStatus(Spanning, new DateOnly(2027, 1, 15));

        // Assert
        status.Status.ShouldBe(PeriodStatus.Ongoing);
        status.DaysLeft.ShouldBeNull();
    }

    [Fact]
    public void GetPeriodStatus_OnADayBetweenTwoOccurrences_CountsDownToTheNextFromDate()
    {
        // Act
        PeriodStatusResult status = sut.GetPeriodStatus(Spanning, new DateOnly(2027, 6, 1));

        // Assert
        status.Status.ShouldBe(PeriodStatus.Upcoming);
        status.DaysLeft.ShouldBe(new DateOnly(2027, 12, 1).DayNumber - new DateOnly(2027, 6, 1).DayNumber);
    }

    [Fact]
    public void GetPeriodStatus_ForAYearFarInTheFuture_StillBindsToThatYear()
    {
        // Act — chọn năm 2030 ở tab chi tiết: trạng thái vẫn của lần neo năm 2030.
        PeriodStatusResult status = sut.GetPeriodStatus(Spanning, 2030, new DateOnly(2027, 1, 15));

        // Assert
        status.Status.ShouldBe(PeriodStatus.Upcoming);
        status.DaysLeft.ShouldBe(new DateOnly(2030, 12, 1).DayNumber - new DateOnly(2027, 1, 15).DayNumber);
    }
}
