using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Responses;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Services;
using System.Globalization;
using AwardPeriod = HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities.AwardPeriod;

namespace HuyHieuDang.Infrastructure.UnitTests;

/// <summary>
/// Cảnh báo chồng lấn, khoảng trống và dải độ phủ của mục 5.1 hợp đồng API (QT6).
/// Toàn bộ phép tính ngày do <see cref="PartyMilestoneCalculator"/> của T07 làm; lớp này chỉ
/// cắt đoạn và gắn tên nên kiểm thử được mà không cần cơ sở dữ liệu.
/// </summary>
public class AwardPeriodCoverageBuilderTests
{
    private const int Year = 2026;

    private static readonly IPartyMilestoneCalculator Calculator = new PartyMilestoneCalculator();

    [Fact(DisplayName = "QT6 · Bộ đợt mẫu cho đúng một cặp chồng lấn kèm khoảng ngày dùng chung")]
    public void BuildWarnings_SamplePeriods_ReportsOverlapRange()
    {
        CoverageWarningsResponse warnings = AwardPeriodCoverageBuilder.BuildWarnings(Calculator, SamplePeriods(), Year);

        PeriodOverlapResponse overlap = Assert.Single(warnings.Overlaps);
        Assert.Equal("Đợt 19/5", overlap.FirstPeriodName);
        Assert.Equal("Đợt 2/9", overlap.SecondPeriodName);
        Assert.Equal(new DateOnly(2026, 5, 25), overlap.FromDate);
        Assert.Equal(new DateOnly(2026, 5, 31), overlap.ToDate);
        Assert.Equal("25/05", overlap.FromDisplay);
        Assert.Equal("31/05", overlap.ToDisplay);
    }

    [Fact(DisplayName = "QT6 · Bộ đợt mẫu cho đúng hai khoảng trống kèm tên hai đợt kề")]
    public void BuildWarnings_SamplePeriods_ReportsGapsWithNeighbourNames()
    {
        CoverageWarningsResponse warnings = AwardPeriodCoverageBuilder.BuildWarnings(Calculator, SamplePeriods(), Year);

        Assert.Equal(2, warnings.Gaps.Count);

        PeriodGapResponse first = warnings.Gaps[0];
        Assert.Equal(new DateOnly(2026, 1, 1), first.FromDate);
        Assert.Equal(new DateOnly(2026, 1, 14), first.ToDate);
        Assert.Null(first.PreviousPeriodName);
        Assert.Equal("Đợt 3/2", first.NextPeriodName);

        PeriodGapResponse second = warnings.Gaps[1];
        Assert.Equal(new DateOnly(2026, 3, 6), second.FromDate);
        Assert.Equal(new DateOnly(2026, 4, 30), second.ToDate);
        Assert.Equal("06/03", second.FromDisplay);
        Assert.Equal("30/04", second.ToDisplay);
        Assert.Equal("Đợt 3/2", second.PreviousPeriodName);
        Assert.Equal("Đợt 19/5", second.NextPeriodName);
    }

    [Fact(DisplayName = "UC-36 · Dải độ phủ liền mạch 01/01–31/12, phần chồng lấn thuộc đợt đến trước")]
    public void BuildCoverage_SamplePeriods_ReturnsContinuousSegments()
    {
        CoverageResponse coverage = AwardPeriodCoverageBuilder.BuildCoverage(Calculator, SamplePeriods(), Year);

        AssertContinuous(coverage, Year);

        // Phần chồng lấn 25/05–31/05 đã thuộc Đợt 19/5 nên đoạn của Đợt 2/9 bắt đầu từ 01/06.
        Assert.Collection(
            coverage.Segments,
            x => AssertSegment(x, CoverageSegmentType.Gap, null, "2026-01-01", "2026-01-14"),
            x => AssertSegment(x, CoverageSegmentType.Period, "Đợt 3/2", "2026-01-15", "2026-03-05"),
            x => AssertSegment(x, CoverageSegmentType.Gap, null, "2026-03-06", "2026-04-30"),
            x => AssertSegment(x, CoverageSegmentType.Period, "Đợt 19/5", "2026-05-01", "2026-05-31"),
            x => AssertSegment(x, CoverageSegmentType.Period, "Đợt 2/9", "2026-06-01", "2026-09-10"),
            x => AssertSegment(x, CoverageSegmentType.Period, "Đợt 7/11", "2026-09-11", "2026-12-31"));
    }

    [Fact(DisplayName = "QT6 · Chưa cài đợt nào: dải vẫn là một khoảng trống cả năm, cảnh báo gaps rỗng")]
    public void BuildCoverage_WithoutPeriods_KeepsFullYearGapSegmentButReportsNoGapWarning()
    {
        IReadOnlyList<AwardPeriod> none = Array.Empty<AwardPeriod>();

        CoverageResponse coverage = AwardPeriodCoverageBuilder.BuildCoverage(Calculator, none, Year);
        CoverageWarningsResponse warnings = AwardPeriodCoverageBuilder.BuildWarnings(Calculator, none, Year);

        AssertContinuous(coverage, Year);
        AssertSegment(Assert.Single(coverage.Segments), CoverageSegmentType.Gap, null, "2026-01-01", "2026-12-31");

        // "Chưa phủ kín" là lời khuyên chỉnh lại các đợt đang có; chưa có đợt nào thì chỉ còn
        // cảnh báo "Chưa cài đợt trao huy hiệu", nên không kèm khoảng trống 01/01-31/12 nữa.
        Assert.Empty(warnings.Gaps);
        Assert.Empty(warnings.Overlaps);
    }

    [Fact(DisplayName = "QT6 · Đợt nằm lọt trong đợt khác: báo chồng lấn, không cắt thêm đoạn")]
    public void BuildCoverage_NestedPeriod_KeepsOuterSegmentOnly()
    {
        IReadOnlyList<AwardPeriod> periods = new[]
        {
            Period("Đợt cả năm", 1, 1, 31, 12),
            Period("Đợt lọt trong", 1, 6, 30, 6),
        };

        CoverageResponse coverage = AwardPeriodCoverageBuilder.BuildCoverage(Calculator, periods, Year);
        CoverageWarningsResponse warnings = AwardPeriodCoverageBuilder.BuildWarnings(Calculator, periods, Year);

        AssertContinuous(coverage, Year);
        AssertSegment(Assert.Single(coverage.Segments), CoverageSegmentType.Period, "Đợt cả năm", "2026-01-01", "2026-12-31");

        Assert.Single(warnings.Overlaps);
        Assert.Empty(warnings.Gaps);
    }

    [Fact(DisplayName = "QT2 · Đợt 29/02 ở năm không nhuận lùi về 28/02")]
    public void BuildCoverage_LeapDayPeriod_BindsToLastDayOfFebruary()
    {
        IReadOnlyList<AwardPeriod> periods = new[] { Period("Đợt nhuận 29/02", 29, 2, 5, 3) };

        CoverageResponse leapYear = AwardPeriodCoverageBuilder.BuildCoverage(Calculator, periods, 2028);
        CoverageResponse commonYear = AwardPeriodCoverageBuilder.BuildCoverage(Calculator, periods, 2026);

        AssertContinuous(leapYear, 2028);
        AssertContinuous(commonYear, 2026);
        Assert.Equal(new DateOnly(2028, 2, 29), leapYear.Segments[1].FromDate);
        Assert.Equal(new DateOnly(2026, 2, 28), commonYear.Segments[1].FromDate);
    }

    [Fact(DisplayName = "QT6 · Đợt vắt qua 31/12 để lại hai đoạn trong cùng một năm")]
    public void BuildCoverage_PeriodSpanningNewYear_SplitsIntoTailAndHead()
    {
        IReadOnlyList<AwardPeriod> periods = new[] { Period("Đợt Giao thừa", 1, 12, 28, 2) };

        CoverageResponse coverage = AwardPeriodCoverageBuilder.BuildCoverage(Calculator, periods, Year);

        AssertContinuous(coverage, Year);

        // Đuôi của lần neo năm 2025 phủ đầu năm, đầu của lần neo năm 2026 phủ cuối năm.
        Assert.Collection(
            coverage.Segments,
            x => AssertSegment(x, CoverageSegmentType.Period, "Đợt Giao thừa", "2026-01-01", "2026-02-28"),
            x => AssertSegment(x, CoverageSegmentType.Gap, null, "2026-03-01", "2026-11-30"),
            x => AssertSegment(x, CoverageSegmentType.Period, "Đợt Giao thừa", "2026-12-01", "2026-12-31"));

        // Hai đoạn cùng một đợt nên cùng một periodId — giao diện phải tự lo khóa của React.
        Assert.Equal(coverage.Segments[0].PeriodId, coverage.Segments[^1].PeriodId);
    }

    [Fact(DisplayName = "QT6 · Đợt vắt năm một mình không tự chồng lấn, chỉ hở khoảng giữa năm")]
    public void BuildWarnings_PeriodSpanningNewYear_ReportsOnlyTheMiddleGap()
    {
        IReadOnlyList<AwardPeriod> periods = new[] { Period("Đợt Giao thừa", 1, 12, 28, 2) };

        CoverageWarningsResponse warnings = AwardPeriodCoverageBuilder.BuildWarnings(Calculator, periods, Year);

        Assert.Empty(warnings.Overlaps);

        PeriodGapResponse gap = Assert.Single(warnings.Gaps);
        Assert.Equal(new DateOnly(2026, 3, 1), gap.FromDate);
        Assert.Equal(new DateOnly(2026, 11, 30), gap.ToDate);
        Assert.Equal("Đợt Giao thừa", gap.PreviousPeriodName);
        Assert.Equal("Đợt Giao thừa", gap.NextPeriodName);
    }

    /// <summary>
    /// Bộ đợt mẫu: bốn đợt, một cặp chồng lấn (Đợt 19/5 và Đợt 2/9 dùng chung 25/05–31/05)
    /// và hai khoảng trống (đầu năm, giữa Đợt 3/2 và Đợt 19/5).
    /// </summary>
    private static IReadOnlyList<AwardPeriod> SamplePeriods() => new[]
    {
        Period("Đợt 3/2", 15, 1, 5, 3),
        Period("Đợt 19/5", 1, 5, 31, 5),
        Period("Đợt 2/9", 25, 5, 10, 9),
        Period("Đợt 7/11", 11, 9, 31, 12),
    };

    private static AwardPeriod Period(string name, int fromDay, int fromMonth, int toDay, int toMonth)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            FromDay = fromDay,
            FromMonth = fromMonth,
            ToDay = toDay,
            ToMonth = toMonth,
        };

    /// <summary>
    /// Dải độ phủ phải bắt đầu 01/01, kết thúc 31/12 và các đoạn nối liền nhau không hở, không đè.
    /// </summary>
    private static void AssertContinuous(CoverageResponse coverage, int year)
    {
        Assert.NotEmpty(coverage.Segments);
        Assert.Equal(new DateOnly(year, 1, 1), coverage.Segments[0].FromDate);
        Assert.Equal(new DateOnly(year, 12, 31), coverage.Segments[^1].ToDate);

        for (int index = 1; index < coverage.Segments.Count; index++)
        {
            Assert.Equal(coverage.Segments[index - 1].ToDate.AddDays(1), coverage.Segments[index].FromDate);
        }
    }

    private static void AssertSegment(
        CoverageSegmentResponse segment, CoverageSegmentType type, string? name, string from, string to)
    {
        Assert.Equal(type, segment.Type);
        Assert.Equal(name, segment.Name);
        Assert.Equal(DateOnly.Parse(from, CultureInfo.InvariantCulture), segment.FromDate);
        Assert.Equal(DateOnly.Parse(to, CultureInfo.InvariantCulture), segment.ToDate);
        Assert.Equal(type == CoverageSegmentType.Period, segment.PeriodId is not null);
    }
}
