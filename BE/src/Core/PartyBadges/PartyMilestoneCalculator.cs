using HuyHieuDang.Core.Common.Exceptions;

namespace HuyHieuDang.Core.PartyBadges;

/// <inheritdoc cref="IPartyMilestoneCalculator"/>
public sealed class PartyMilestoneCalculator : IPartyMilestoneCalculator
{
    /// <inheritdoc/>
    public IReadOnlyList<int> BuildMilestones(MilestoneSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.StartYears <= 0 || settings.EndYears <= 0)
        {
            throw new BadRequestException("Mốc bắt đầu và mốc kết thúc phải là số nguyên dương.");
        }

        if (settings.StepYears < 1)
        {
            throw new BadRequestException("Bước phải là số nguyên lớn hơn hoặc bằng 1.");
        }

        if (settings.StartYears > settings.EndYears)
        {
            throw new BadRequestException("Mốc bắt đầu phải nhỏ hơn hoặc bằng mốc kết thúc.");
        }

        List<int> milestones = new();

        // Cộng bằng long để Bước rất lớn không quay vòng kiểu int thành mốc âm.
        for (long milestone = settings.StartYears; milestone <= settings.EndYears; milestone += settings.StepYears)
        {
            milestones.Add((int)milestone);
        }

        return milestones;
    }

    /// <inheritdoc/>
    public DateOnly GetAnniversary(DateOnly officialAdmissionDate, int milestone)
    {
        if (milestone < 0)
        {
            throw new BadRequestException("Mốc huy hiệu không được là số âm.");
        }

        return BindDayMonth(
            officialAdmissionDate.Day, officialAdmissionDate.Month, officialAdmissionDate.Year + milestone);
    }

    /// <inheritdoc/>
    public int GetPartyAge(DateOnly officialAdmissionDate, DateOnly today)
    {
        int age = today.Year - officialAdmissionDate.Year;

        while (age > 0 && GetAnniversary(officialAdmissionDate, age) > today)
        {
            age--;
        }

        return Math.Max(age, 0);
    }

    /// <inheritdoc/>
    public int? GetNextMilestone(DateOnly officialAdmissionDate, DateOnly today, IReadOnlyList<int> milestones)
    {
        ArgumentNullException.ThrowIfNull(milestones);

        int partyAge = GetPartyAge(officialAdmissionDate, today);

        return milestones.Where(x => x > partyAge).Select(x => (int?)x).FirstOrDefault();
    }

    /// <inheritdoc/>
    public DateOnly? GetNextAnniversary(DateOnly officialAdmissionDate, DateOnly today, IReadOnlyList<int> milestones)
    {
        int? next = GetNextMilestone(officialAdmissionDate, today, milestones);

        return next is null ? null : GetAnniversary(officialAdmissionDate, next.Value);
    }

    /// <inheritdoc/>
    public PeriodOccurrence BindToYear(AwardPeriod period, int year)
    {
        ArgumentNullException.ThrowIfNull(period);

        ThrowIfDayMonthDoesNotExist(period.FromDay, period.FromMonth, "Từ ngày", period.Name);
        ThrowIfDayMonthDoesNotExist(period.ToDay, period.ToMonth, "Đến ngày", period.Name);

        // QT6 — đợt nằm trọn trong một năm dương lịch, không vắt qua 31/12 sang 01/01.
        if (period.FromMonth > period.ToMonth
            || (period.FromMonth == period.ToMonth && period.FromDay > period.ToDay))
        {
            throw new BadRequestException(
                $"Đợt {period.Name} có Từ ngày lớn hơn Đến ngày; đợt phải nằm trọn trong một năm.");
        }

        return new PeriodOccurrence(
            period,
            year,
            BindDayMonth(period.FromDay, period.FromMonth, year),
            BindDayMonth(period.ToDay, period.ToMonth, year));
    }

    /// <inheritdoc/>
    public int? GetEligibleMilestone(
        DateOnly officialAdmissionDate, AwardPeriod period, int year, IReadOnlyList<int> milestones)
    {
        ArgumentNullException.ThrowIfNull(milestones);

        PeriodOccurrence occurrence = BindToYear(period, year);

        // Các mốc cách nhau ít nhất 1 năm còn một đợt luôn ngắn hơn 1 năm, nên tối đa 1 mốc khớp.
        return milestones
            .Where(x => IsWithin(GetAnniversary(officialAdmissionDate, x), occurrence))
            .Select(x => (int?)x)
            .FirstOrDefault();
    }

    /// <inheritdoc/>
    public MissedMilestone? GetMissedMilestone(
        DateOnly officialAdmissionDate, IReadOnlyList<AwardPeriod> periods, int year, IReadOnlyList<int> milestones)
    {
        ArgumentNullException.ThrowIfNull(periods);
        ArgumentNullException.ThrowIfNull(milestones);

        List<PeriodOccurrence> occurrences = periods.Select(x => BindToYear(x, year)).ToList();
        IReadOnlyList<DateGap> gaps = GetGaps(periods, year);

        foreach (int milestone in milestones)
        {
            DateOnly anniversary = GetAnniversary(officialAdmissionDate, milestone);

            if (anniversary.Year != year || occurrences.Exists(x => IsWithin(anniversary, x)))
            {
                continue;
            }

            DateGap? gap = gaps.FirstOrDefault(x => anniversary >= x.From && anniversary <= x.To);

            if (gap is not null)
            {
                return new MissedMilestone(milestone, anniversary, gap);
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public IReadOnlyList<DateGap> GetGaps(IReadOnlyList<AwardPeriod> periods, int year)
    {
        ArgumentNullException.ThrowIfNull(periods);

        List<PeriodOccurrence> ordered = OrderDeterministically(periods.Select(x => BindToYear(x, year)));

        List<DateGap> gaps = new();
        DateOnly cursor = new(year, 1, 1);
        DateOnly lastDayOfYear = new(year, 12, 31);

        for (int index = 0; index < ordered.Count; index++)
        {
            PeriodOccurrence occurrence = ordered[index];

            if (cursor < occurrence.From)
            {
                gaps.Add(index == 0
                    ? new DateGap(cursor, occurrence.From.AddDays(-1), GapKind.BeforeFirstPeriod, "Trước đợt đầu tiên")
                    : new DateGap(
                        cursor,
                        occurrence.From.AddDays(-1),
                        GapKind.BetweenPeriods,
                        $"Giữa {ordered[index - 1].Period.Name} và {occurrence.Period.Name}"));
            }

            if (occurrence.To >= cursor)
            {
                cursor = occurrence.To.AddDays(1);
            }
        }

        if (cursor <= lastDayOfYear)
        {
            // Chưa cài đợt nào thì cả năm là khoảng trống nằm trước đợt đầu tiên, không phải
            // sau đợt cuối cùng — hợp đồng API mục 1.10 chốt `type = "BeforeFirst"`.
            gaps.Add(new DateGap(
                cursor,
                lastDayOfYear,
                ordered.Count == 0 ? GapKind.BeforeFirstPeriod : GapKind.AfterLastPeriod,
                ordered.Count == 0 ? "Trước đợt đầu tiên" : "Sau đợt cuối cùng"));
        }

        return gaps;
    }

    /// <inheritdoc/>
    public IReadOnlyList<PeriodOverlap> GetOverlaps(IReadOnlyList<AwardPeriod> periods, int year)
    {
        ArgumentNullException.ThrowIfNull(periods);

        List<PeriodOccurrence> ordered = OrderDeterministically(periods.Select(x => BindToYear(x, year)));

        List<PeriodOverlap> overlaps = new();

        for (int i = 0; i < ordered.Count; i++)
        {
            for (int j = i + 1; j < ordered.Count; j++)
            {
                if (ordered[i].From <= ordered[j].To && ordered[j].From <= ordered[i].To)
                {
                    overlaps.Add(new PeriodOverlap(ordered[i].Period, ordered[j].Period));
                }
            }
        }

        return overlaps;
    }

    /// <inheritdoc/>
    public UpcomingPeriod? GetUpcomingPeriod(IReadOnlyList<AwardPeriod> periods, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(periods);

        if (periods.Count == 0)
        {
            return null;
        }

        // Hai đợt cùng Từ ngày thì chọn đợt kết thúc sớm hơn, vẫn bằng nhau thì theo Tên đợt
        // — OQ-9 của hợp đồng API, để Dashboard luôn chọn ra đúng một đợt tất định.
        PeriodOccurrence? occurrence = Earliest(periods, today.Year, x => x.To >= today)
            ?? Earliest(periods, today.Year + 1, _ => true)!;

        return new UpcomingPeriod(occurrence, GetStatus(occurrence, today));
    }

    /// <summary>
    /// Lần diễn ra sớm nhất trong một năm trong số các đợt thỏa điều kiện, theo thứ tự
    /// Từ ngày → Đến ngày → Tên đợt (OQ-9).
    /// </summary>
    /// <param name="periods">Toàn bộ đợt đang cấu hình.</param>
    /// <param name="year">Năm cần gắn.</param>
    /// <param name="filter">Điều kiện lọc thêm trên lần diễn ra.</param>
    /// <returns>Lần diễn ra được chọn, hoặc <c>null</c> khi không đợt nào thỏa.</returns>
    private PeriodOccurrence? Earliest(
        IReadOnlyList<AwardPeriod> periods, int year, Func<PeriodOccurrence, bool> filter)
        => periods
            .Select(x => BindToYear(x, year))
            .Where(filter)
            .OrderBy(x => x.From)
            .ThenBy(x => x.To)
            .ThenBy(x => x.Period.Name, StringComparer.Ordinal)
            .FirstOrDefault();

    /// <inheritdoc/>
    public PeriodStatusResult GetPeriodStatus(AwardPeriod period, DateOnly today) =>
        GetPeriodStatus(period, today.Year, today);

    /// <inheritdoc/>
    public PeriodStatusResult GetPeriodStatus(AwardPeriod period, int year, DateOnly today) =>
        GetStatus(BindToYear(period, year), today);

    /// <summary>
    /// Gắn năm vào một cặp ngày/tháng. 29/02 ở năm không nhuận lùi về 28/02 (QT2, QT4).
    /// </summary>
    private static DateOnly BindDayMonth(int day, int month, int year) =>
        month == 2 && day == 29 && !DateTime.IsLeapYear(year)
            ? new DateOnly(year, 2, 28)
            : new DateOnly(year, month, day);

    /// <summary>
    /// Sắp đợt theo Từ ngày, rồi Đến ngày, rồi Tên để kết quả không phụ thuộc thứ tự nạp danh sách.
    /// </summary>
    private static List<PeriodOccurrence> OrderDeterministically(IEnumerable<PeriodOccurrence> occurrences) =>
        occurrences
            .OrderBy(x => x.From)
            .ThenBy(x => x.To)
            .ThenBy(x => x.Period.Name, StringComparer.Ordinal)
            .ToList();

    /// <summary>QT6 — ngày/tháng của đợt phải là một ngày có thật (29/02 hợp lệ vì năm nhuận có).</summary>
    private static void ThrowIfDayMonthDoesNotExist(int day, int month, string field, string periodName)
    {
        const int LeapYear = 2028;

        if (month is < 1 or > 12 || day < 1 || day > DateTime.DaysInMonth(LeapYear, month))
        {
            throw new BadRequestException($"{field} {day:00}/{month:00} của đợt {periodName} không tồn tại.");
        }
    }

    private static bool IsWithin(DateOnly date, PeriodOccurrence occurrence) =>
        date >= occurrence.From && date <= occurrence.To;

    private static PeriodStatusResult GetStatus(PeriodOccurrence occurrence, DateOnly today)
    {
        if (occurrence.To < today)
        {
            return new PeriodStatusResult(PeriodStatus.Past, null);
        }

        if (occurrence.From <= today)
        {
            return new PeriodStatusResult(PeriodStatus.Ongoing, null);
        }

        return new PeriodStatusResult(PeriodStatus.Upcoming, occurrence.From.DayNumber - today.DayNumber);
    }
}
