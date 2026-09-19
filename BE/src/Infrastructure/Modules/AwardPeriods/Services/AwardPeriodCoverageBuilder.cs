using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Responses;
using CorePeriod = HuyHieuDang.Core.PartyBadges.AwardPeriod;
using PeriodEntity = HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities.AwardPeriod;

namespace HuyHieuDang.Infrastructure.Modules.AwardPeriods.Services;

/// <summary>
/// Dựng cảnh báo chồng lấn / khoảng trống và dải độ phủ 12 tháng cho một năm (QT6, UC-36).
/// Mọi phép tính ngày đều gọi <see cref="IPartyMilestoneCalculator"/> của T07; lớp này chỉ cắt
/// đoạn, gắn tên và đổi sang hình dạng của hợp đồng API nên không cần cơ sở dữ liệu.
/// </summary>
public static class AwardPeriodCoverageBuilder
{
    /// <summary>
    /// Cảnh báo của một năm: các cặp đợt chồng lấn và các khoảng chưa phủ 01/01–31/12.
    /// </summary>
    /// <param name="calculator">Service thuần tính mốc tuổi đảng (T07).</param>
    /// <param name="periods">Toàn bộ đợt đang cấu hình.</param>
    /// <param name="year">Năm đang xét.</param>
    /// <returns>Cảnh báo đã gắn ngày của <paramref name="year"/>.</returns>
    public static CoverageWarningsResponse BuildWarnings(
        IPartyMilestoneCalculator calculator, IReadOnlyList<PeriodEntity> periods, int year)
    {
        ArgumentNullException.ThrowIfNull(calculator);
        ArgumentNullException.ThrowIfNull(periods);

        IReadOnlyDictionary<string, PeriodEntity> byName = IndexByName(periods);
        IReadOnlyList<CorePeriod> corePeriods = byName.Values.Select(ToCorePeriod).ToList();
        List<PeriodOccurrence> ordered = Order(calculator, corePeriods, year);

        return new CoverageWarningsResponse
        {
            Overlaps = calculator.GetOverlaps(corePeriods, year)
                .Select(overlap => ToOverlapResponse(calculator, overlap, byName, year))
                .OrderBy(x => x.FromDate)
                .ToList(),
            Gaps = calculator.GetGaps(corePeriods, year)
                .Select(gap => ToGapResponse(gap, ordered))
                .ToList(),
        };
    }

    /// <summary>
    /// Dải độ phủ liên tục 01/01–31/12 của năm. Phần chồng lấn thuộc đoạn của đợt đến trước,
    /// nên một đợt nằm lọt hẳn trong đợt khác không sinh đoạn nào.
    /// </summary>
    /// <param name="calculator">Service thuần tính mốc tuổi đảng (T07).</param>
    /// <param name="periods">Toàn bộ đợt đang cấu hình.</param>
    /// <param name="year">Năm đang xét.</param>
    /// <returns>Các đoạn nối tiếp nhau, sắp theo ngày bắt đầu.</returns>
    public static CoverageResponse BuildCoverage(
        IPartyMilestoneCalculator calculator, IReadOnlyList<PeriodEntity> periods, int year)
    {
        ArgumentNullException.ThrowIfNull(calculator);
        ArgumentNullException.ThrowIfNull(periods);

        IReadOnlyDictionary<string, PeriodEntity> byName = IndexByName(periods);
        List<PeriodOccurrence> ordered = Order(calculator, byName.Values.Select(ToCorePeriod).ToList(), year);

        List<CoverageSegmentResponse> segments = new();
        DateOnly cursor = new(year, 1, 1);
        DateOnly lastDayOfYear = new(year, 12, 31);

        foreach (PeriodOccurrence occurrence in ordered)
        {
            if (cursor < occurrence.From)
            {
                segments.Add(GapSegment(cursor, occurrence.From.AddDays(-1)));
                cursor = occurrence.From;
            }

            if (occurrence.To < cursor)
            {
                // Đợt đã nằm trọn trong đoạn của một đợt đến trước.
                continue;
            }

            segments.Add(PeriodSegment(byName[occurrence.Period.Name], cursor, occurrence.To));
            cursor = occurrence.To.AddDays(1);
        }

        if (cursor <= lastDayOfYear)
        {
            segments.Add(GapSegment(cursor, lastDayOfYear));
        }

        return new CoverageResponse { Segments = segments };
    }

    /// <summary>
    /// Đổi bản ghi bảng sang bản ghi thuần của service tính toán.
    /// </summary>
    /// <param name="entity">Bản ghi đợt.</param>
    /// <returns>Bản ghi thuần dùng cho <see cref="IPartyMilestoneCalculator"/>.</returns>
    public static CorePeriod ToCorePeriod(PeriodEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new CorePeriod(entity.Name, entity.FromDay, entity.FromMonth, entity.ToDay, entity.ToMonth);
    }

    /// <summary>
    /// Tên đợt là duy nhất không phân biệt hoa thường nên dùng làm khóa tra ngược về bản ghi.
    /// </summary>
    private static IReadOnlyDictionary<string, PeriodEntity> IndexByName(IReadOnlyList<PeriodEntity> periods)
    {
        Dictionary<string, PeriodEntity> byName = new(StringComparer.OrdinalIgnoreCase);

        foreach (PeriodEntity period in periods)
        {
            byName[period.Name] = period;
        }

        return byName;
    }

    private static List<PeriodOccurrence> Order(
        IPartyMilestoneCalculator calculator, IReadOnlyList<CorePeriod> periods, int year)
        => periods
            .Select(period => calculator.BindToYear(period, year))
            .OrderBy(x => x.From)
            .ThenByDescending(x => x.To)
            .ThenBy(x => x.Period.Name, StringComparer.Ordinal)
            .ToList();

    private static PeriodOverlapResponse ToOverlapResponse(
        IPartyMilestoneCalculator calculator,
        PeriodOverlap overlap,
        IReadOnlyDictionary<string, PeriodEntity> byName,
        int year)
    {
        PeriodOccurrence first = calculator.BindToYear(overlap.First, year);
        PeriodOccurrence second = calculator.BindToYear(overlap.Second, year);
        DateOnly from = first.From > second.From ? first.From : second.From;
        DateOnly to = first.To < second.To ? first.To : second.To;

        return new PeriodOverlapResponse
        {
            FirstPeriodId = byName[overlap.First.Name].Id,
            FirstPeriodName = overlap.First.Name,
            SecondPeriodId = byName[overlap.Second.Name].Id,
            SecondPeriodName = overlap.Second.Name,
            FromDate = from,
            ToDate = to,
            FromDisplay = AwardPeriodResponse.Display(from),
            ToDisplay = AwardPeriodResponse.Display(to),
        };
    }

    private static PeriodGapResponse ToGapResponse(DateGap gap, IReadOnlyList<PeriodOccurrence> ordered)
        => new()
        {
            FromDate = gap.From,
            ToDate = gap.To,
            FromDisplay = AwardPeriodResponse.Display(gap.From),
            ToDisplay = AwardPeriodResponse.Display(gap.To),
            PreviousPeriodName = gap.Kind == GapKind.BeforeFirstPeriod
                ? null
                : ordered.Where(x => x.To < gap.From).MaxBy(x => x.To)?.Period.Name,
            NextPeriodName = gap.Kind == GapKind.AfterLastPeriod
                ? null
                : ordered.FirstOrDefault(x => x.From > gap.To)?.Period.Name,
        };

    private static CoverageSegmentResponse GapSegment(DateOnly from, DateOnly to)
        => new()
        {
            Type = CoverageSegmentType.Gap,
            PeriodId = null,
            Name = null,
            FromDate = from,
            ToDate = to,
        };

    private static CoverageSegmentResponse PeriodSegment(PeriodEntity entity, DateOnly from, DateOnly to)
        => new()
        {
            Type = CoverageSegmentType.Period,
            PeriodId = entity.Id,
            Name = entity.Name,
            FromDate = from,
            ToDate = to,
        };
}
