using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.UnitTests.Fixtures;

namespace HuyHieuDang.Core.UnitTests;

/// <summary>QT6 — cảnh báo chồng lấn và liệt kê khoảng trống chưa phủ 01/01–31/12.</summary>
public sealed class Qt6PeriodWarningTests
{
    private readonly PartyMilestoneCalculator sut = new();

    private static IReadOnlyList<AwardPeriod> Main =>
        FixtureData.MainPeriods.Select(x => x.Period).ToList();

    [Fact]
    public void GetGaps_ForTheMainPeriods_ReturnsTheFiveExpectedGapsOf2026()
    {
        // Act
        IReadOnlyList<DateGap> gaps = sut.GetGaps(Main, 2026);

        // Assert
        IReadOnlyList<(DateOnly From, DateOnly To, string Label)> expected =
            FixtureExpectations.Gaps("core_default_T0", 2026);

        gaps.Count.ShouldBe(5);
        gaps.Select(x => (x.From, x.To, x.Label)).ShouldBe(expected);
    }

    [Fact]
    public void GetGaps_ForTheMainPeriods_LabelsTheFirstAndLastGapByPosition()
    {
        // Act
        IReadOnlyList<DateGap> gaps = sut.GetGaps(Main, 2026);

        // Assert
        gaps[0].Kind.ShouldBe(GapKind.BeforeFirstPeriod);
        gaps[0].Label.ShouldBe("Trước đợt đầu tiên");
        gaps[1].Kind.ShouldBe(GapKind.BetweenPeriods);
        gaps[1].Label.ShouldBe("Giữa Đợt 3/2 và Đợt 19/5");
        gaps[^1].Kind.ShouldBe(GapKind.AfterLastPeriod);
        gaps[^1].Label.ShouldBe("Sau đợt cuối cùng");
    }

    [Fact]
    public void GetGaps_WhenThePeriodsCoverTheWholeYear_ReturnsNoGap()
    {
        // Act
        IReadOnlyList<DateGap> gaps =
            sut.GetGaps(FixtureData.FullCoverPeriods.Select(x => x.Period).ToList(), 2026);

        // Assert
        gaps.ShouldBeEmpty();
        FixtureExpectations.Gaps("core_default_T0_fullCover", 2026).ShouldBeEmpty();
    }

    [Fact]
    public void GetGaps_WhenThereIsNoPeriod_ReturnsTheWholeYearAsOneGap()
    {
        // Act
        IReadOnlyList<DateGap> gaps = sut.GetGaps([], 2026);

        // Assert
        gaps.Count.ShouldBe(1);
        gaps[0].From.ShouldBe(new DateOnly(2026, 1, 1));
        gaps[0].To.ShouldBe(new DateOnly(2026, 12, 31));
    }

    [Fact]
    public void GetGaps_InALeapYear_EndsTheGapOnTheCorrectFebruaryDay()
    {
        // Act — Đợt nhuận 29/02–05/03 gắn năm nhuận 2028 giữ nguyên 29/02.
        IReadOnlyList<DateGap> gaps =
            sut.GetGaps(FixtureData.LeapEdgePeriods.Select(x => x.Period).ToList(), 2028);

        // Assert
        gaps[0].To.ShouldBe(new DateOnly(2028, 2, 28));
    }

    [Fact]
    public void GetGaps_WhenThereIsNoPeriod_LabelsTheWholeYearAsBeforeTheFirstPeriod()
    {
        // Act — hợp đồng API mục 1.10: chưa cài đợt nào thì cả năm là "Trước đợt đầu tiên".
        // CEO chốt ngày 19/09/2026; oracle của QC đã sửa theo.
        IReadOnlyList<DateGap> gaps = sut.GetGaps([], 2026);

        // Assert
        gaps[0].Kind.ShouldBe(GapKind.BeforeFirstPeriod);
        gaps[0].Label.ShouldBe("Trước đợt đầu tiên");
    }

    [Fact]
    public void BindToYear_WhenTheFromDateIsAfterTheToDate_EndsThePeriodInTheNextYear()
    {
        // Act — QT6 cho phép đợt vắt qua 31/12: Đến ngày thuộc năm kế tiếp.
        PeriodOccurrence occurrence = sut.BindToYear(new AwardPeriod("Đợt vắt năm", 7, 11, 1, 10), 2026);

        // Assert
        occurrence.From.ShouldBe(new DateOnly(2026, 11, 7));
        occurrence.To.ShouldBe(new DateOnly(2027, 10, 1));
    }

    [Theory]
    [InlineData(31, 2)]
    [InlineData(30, 2)]
    [InlineData(32, 1)]
    [InlineData(31, 4)]
    [InlineData(1, 13)]
    [InlineData(0, 1)]
    public void BindToYear_WhenTheDayOrMonthDoesNotExist_ThrowsBadRequest(int day, int month)
    {
        // Act — QC-01: trước đây lọt ArgumentOutOfRangeException, tức lỗi kỹ thuật 500.
        Action act = () => sut.BindToYear(new AwardPeriod("Đợt sai ngày", day, month, 5, 12), 2026);

        // Assert
        act.ShouldThrow<BadRequestException>();
    }

    [Fact]
    public void BindToYear_WhenThePeriodStartsOn29February_IsStillAccepted()
    {
        // Act — 29/02 là ngày có thật ở năm nhuận nên không được coi là ngày sai.
        PeriodOccurrence occurrence = sut.BindToYear(new AwardPeriod("Đợt nhuận 29/02", 29, 2, 5, 3), 2026);

        // Assert
        occurrence.From.ShouldBe(new DateOnly(2026, 2, 28));
    }

    [Fact]
    public void GetOverlaps_ForTheMainPeriods_ReturnsNoWarning()
    {
        // Act
        IReadOnlyList<PeriodOverlap> overlaps = sut.GetOverlaps(Main, 2026);

        // Assert
        overlaps.ShouldBeEmpty();
        FixtureExpectations.Overlaps("core_default_T0").ShouldBeEmpty();
    }

    [Fact]
    public void GetOverlaps_WhenTwoPeriodsIntersect_ReturnsThePairInOrder()
    {
        // Act
        IReadOnlyList<PeriodOverlap> overlaps =
            sut.GetOverlaps(FixtureData.OverlapPeriods.Select(x => x.Period).ToList(), 2026);

        // Assert
        overlaps.Select(x => (x.First.Name, x.Second.Name))
            .ShouldBe(FixtureExpectations.Overlaps("core_default_T0_overlap"));
    }

    [Fact]
    public void GetOverlaps_WhenTwoPeriodsShareASingleDay_ReportsThem()
    {
        // Arrange — 01/01–01/02 và 01/02–01/03 dùng chung ngày 01/02.
        List<AwardPeriod> periods =
        [
            new("Đợt trước", 1, 1, 1, 2),
            new("Đợt sau", 1, 2, 1, 3),
        ];

        // Act
        IReadOnlyList<PeriodOverlap> overlaps = sut.GetOverlaps(periods, 2026);

        // Assert
        overlaps.Count.ShouldBe(1);
    }

    [Fact]
    public void GetOverlaps_WhenPeriodsAreBackToBack_ReturnsNoWarning()
    {
        // Arrange — 01/01–31/01 và 01/02–01/03 không dùng chung ngày nào.
        List<AwardPeriod> periods =
        [
            new("Đợt trước", 1, 1, 31, 1),
            new("Đợt sau", 1, 2, 1, 3),
        ];

        // Act & Assert
        sut.GetOverlaps(periods, 2026).ShouldBeEmpty();
    }
}
