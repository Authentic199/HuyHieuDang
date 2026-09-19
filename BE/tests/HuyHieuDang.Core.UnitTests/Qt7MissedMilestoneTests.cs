using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.UnitTests.Fixtures;

namespace HuyHieuDang.Core.UnitTests;

/// <summary>QT7 — tròn mốc trong năm nhưng không rơi vào đợt nào, kèm nhãn khoảng trống.</summary>
public sealed class Qt7MissedMilestoneTests
{
    private readonly PartyMilestoneCalculator sut = new();

    private static IReadOnlyList<AwardPeriod> Main =>
        FixtureData.MainPeriods.Select(x => x.Period).ToList();

    private IReadOnlyList<int> Default => sut.BuildMilestones(FixtureData.DefaultSettings);

    private IReadOnlyList<int> StepTen => sut.BuildMilestones(FixtureData.StepTenSettings);

    [Fact]
    public void GetMissedMilestone_WhenTheAnniversaryFallsBeforeTheFirstPeriod_LabelsItBeforeTheFirst()
    {
        // Act — B07 tròn 30 vào 14/01/2026, trước Đợt 3/2.
        MissedMilestone? missed = sut.GetMissedMilestone(
            FixtureData.Member("B07").OfficialAdmissionDate, Main, 2026, Default);

        // Assert
        missed.ShouldNotBeNull();
        missed.Milestone.ShouldBe(30);
        missed.Anniversary.ShouldBe(new DateOnly(2026, 1, 14));
        missed.Gap.Kind.ShouldBe(GapKind.BeforeFirstPeriod);
        missed.Gap.Label.ShouldBe("Trước đợt đầu tiên");
    }

    [Fact]
    public void GetMissedMilestone_WhenTheAnniversaryFallsBetweenTwoPeriods_NamesBothPeriods()
    {
        // Act — B08 tròn 30 vào 06/03/2026, ngay sau Đợt 3/2.
        MissedMilestone? missed = sut.GetMissedMilestone(
            FixtureData.Member("B08").OfficialAdmissionDate, Main, 2026, Default);

        // Assert
        missed.ShouldNotBeNull();
        missed.Gap.Kind.ShouldBe(GapKind.BetweenPeriods);
        missed.Gap.Label.ShouldBe("Giữa Đợt 3/2 và Đợt 19/5");
    }

    [Fact]
    public void GetMissedMilestone_WhenTheAnniversaryFallsAfterTheLastPeriod_LabelsItAfterTheLast()
    {
        // Act — B04 tròn 30 vào 08/11/2026, ngay sau Đợt 7/11.
        MissedMilestone? missed = sut.GetMissedMilestone(
            FixtureData.Member("B04").OfficialAdmissionDate, Main, 2026, Default);

        // Assert
        missed.ShouldNotBeNull();
        missed.Gap.Kind.ShouldBe(GapKind.AfterLastPeriod);
        missed.Gap.Label.ShouldBe("Sau đợt cuối cùng");
    }

    [Fact]
    public void GetMissedMilestone_WhenTheAnniversaryFallsInsideAPeriod_ReturnsNull()
    {
        // Act — B01 tròn 30 đúng ngày đầu Đợt 7/11.
        MissedMilestone? missed = sut.GetMissedMilestone(
            FixtureData.Member("B01").OfficialAdmissionDate, Main, 2026, Default);

        // Assert
        missed.ShouldBeNull();
    }

    [Fact]
    public void GetMissedMilestone_WhenNoMilestoneFallsInTheYear_ReturnsNull()
    {
        // Act — L02 không có mốc nào trong 2026 (mốc 40 rơi vào 29/02/2028).
        MissedMilestone? missed = sut.GetMissedMilestone(
            FixtureData.Member("L02").OfficialAdmissionDate, Main, 2026, Default);

        // Assert
        missed.ShouldBeNull();
    }

    [Fact]
    public void GetMissedMilestone_WhenThereIsNoPeriodAtAll_ReturnsTheWholeYearGap()
    {
        // Act
        MissedMilestone? missed = sut.GetMissedMilestone(
            FixtureData.Member("B01").OfficialAdmissionDate, [], 2026, Default);

        // Assert
        missed.ShouldNotBeNull();
        missed.Anniversary.ShouldBe(new DateOnly(2026, 10, 1));
    }

    [Fact]
    public void GetMissedMilestone_WithStepTen_DropsTheMemberWhoseMilestoneDisappears()
    {
        // Act — S04 tròn mốc 35 vào 15/07/2026, rơi vào khoảng trống; mốc 35 mất khi Bước = 10.
        DateOnly admission = FixtureData.Member("S04").OfficialAdmissionDate;

        // Assert
        sut.GetMissedMilestone(admission, Main, 2026, Default).ShouldNotBeNull();
        sut.GetMissedMilestone(admission, Main, 2026, StepTen).ShouldBeNull();
    }

    [Theory]
    [InlineData(2025)]
    [InlineData(2026)]
    [InlineData(2027)]
    [InlineData(2028)]
    public void GetMissedMilestone_OverTheWholeCoreDataset_MatchesTheExpectedRows(int year)
    {
        // Act
        List<(string Code, int Milestone, DateOnly Anniversary, string GapLabel)> actual =
            FixtureData.CoreMembers
                .Select(m => (Member: m, Missed: sut.GetMissedMilestone(m.OfficialAdmissionDate, Main, year, Default)))
                .Where(x => x.Missed is not null)
                .Select(x => (x.Member.Code, x.Missed!.Milestone, x.Missed.Anniversary, x.Missed.Gap.Label))
                .OrderBy(x => x.Code, StringComparer.Ordinal)
                .ToList();

        // Assert
        List<(string Code, int Milestone, DateOnly Anniversary, string GapLabel)> expected =
            FixtureExpectations.Missed("core_default_T0", year)
                .OrderBy(x => x.Code, StringComparer.Ordinal)
                .ToList();

        actual.ShouldBe(expected, $"Danh sách chưa thuộc đợt nào của năm {year} không khớp bộ dữ liệu của QC.");
    }
}
