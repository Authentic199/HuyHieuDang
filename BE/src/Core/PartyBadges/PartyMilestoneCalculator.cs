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
    public DateOnly GetLatestAdmissionDateForAge(DateOnly today, int age)
    {
        if (age < 0)
        {
            throw new BadRequestException("Tuổi đảng không được là số âm.");
        }

        int year = today.Year - age;

        if (year < 1)
        {
            throw new BadRequestException($"Mốc {age} năm vượt quá lịch dương.");
        }

        DateOnly latest = BindDayMonth(today.Day, today.Month, year);

        // BindDayMonth không bao giờ sinh ra 29/02, nhưng một người vào Đảng ngày 29/02 vẫn có thể
        // tròn `age` năm đúng hôm nay khi năm đích không nhuận (QT2 lùi kỷ niệm về 28/02).
        if (DateTime.IsLeapYear(year))
        {
            DateOnly leapDay = new(year, 2, 29);

            if (leapDay > latest && GetAnniversary(leapDay, age) <= today)
            {
                latest = leapDay;
            }
        }

        return latest;
    }

    /// <inheritdoc/>
    public AdmissionDateRange GetAdmissionDateRangeForNextMilestone(
        int? milestone, DateOnly today, IReadOnlyList<int> milestones)
    {
        ArgumentNullException.ThrowIfNull(milestones);

        if (milestones.Count == 0)
        {
            throw new BadRequestException("Dãy mốc huy hiệu đang rỗng nên không lọc theo mốc kế tiếp được.");
        }

        // Không còn mốc kế tiếp ⟺ tuổi đảng đã đạt mốc lớn nhất ⟺ vào Đảng đủ sớm.
        if (milestone is null)
        {
            return new AdmissionDateRange(null, GetLatestAdmissionDateForAge(today, milestones[^1]));
        }

        int index = -1;

        for (int i = 0; i < milestones.Count && index < 0; i++)
        {
            if (milestones[i] == milestone.Value)
            {
                index = i;
            }
        }

        if (index < 0)
        {
            throw new BadRequestException($"Mốc {milestone} không nằm trong dãy mốc huy hiệu hiện hành.");
        }

        // Mốc kế tiếp là `mi` ⟺ tuổi đảng thuộc [m(i-1), mi). Mốc đầu tiên không có cận trên:
        // người có ngày chính thức ở tương lai vẫn được tính tuổi đảng 0 (QT3) nên phải nằm trong đó.
        return new AdmissionDateRange(
            GetLatestAdmissionDateForAge(today, milestones[index]),
            index == 0 ? null : GetLatestAdmissionDateForAge(today, milestones[index - 1]));
    }

    /// <inheritdoc/>
    public PeriodOccurrence BindToYear(AwardPeriod period, int year)
    {
        ArgumentNullException.ThrowIfNull(period);

        ThrowIfDayMonthDoesNotExist(period.FromDay, period.FromMonth, "Từ ngày", period.Name);
        ThrowIfDayMonthDoesNotExist(period.ToDay, period.ToMonth, "Đến ngày", period.Name);

        // QT6 — Từ ngày đứng sau Đến ngày nghĩa là đợt vắt qua 31/12 (ví dụ 01/12 – 28/02):
        // Từ ngày thuộc năm neo, Đến ngày thuộc năm kế tiếp.
        return new PeriodOccurrence(
            period,
            year,
            BindDayMonth(period.FromDay, period.FromMonth, year),
            BindDayMonth(period.ToDay, period.ToMonth, year + (period.SpansNextYear ? 1 : 0)));
    }

    /// <inheritdoc/>
    public IReadOnlyList<PeriodSlice> GetSlicesInYear(IReadOnlyList<AwardPeriod> periods, int year)
    {
        ArgumentNullException.ThrowIfNull(periods);

        DateOnly firstDayOfYear = new(year, 1, 1);
        DateOnly lastDayOfYear = new(year, 12, 31);
        List<PeriodSlice> slices = new();

        foreach (AwardPeriod period in periods)
        {
            foreach (int anchor in Anchors(period, year))
            {
                PeriodOccurrence occurrence = BindToYear(period, anchor);
                DateOnly from = occurrence.From > firstDayOfYear ? occurrence.From : firstDayOfYear;
                DateOnly to = occurrence.To < lastDayOfYear ? occurrence.To : lastDayOfYear;

                if (from <= to)
                {
                    slices.Add(new PeriodSlice(occurrence, from, to));
                }
            }
        }

        return OrderDeterministically(slices);
    }

    /// <summary>
    /// Các năm neo có thể để lại dấu vết trong <paramref name="year"/>: đợt vắt năm còn phần
    /// đuôi của lần neo năm trước, đợt thường thì chỉ chính năm đó.
    /// </summary>
    /// <param name="period">Đợt trao huy hiệu.</param>
    /// <param name="year">Năm đang xét.</param>
    /// <returns>Danh sách năm neo cần gắn.</returns>
    private static IEnumerable<int> Anchors(AwardPeriod period, int year) =>
        period.SpansNextYear ? [year - 1, year] : [year];

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

        IReadOnlyList<PeriodSlice> slices = GetSlicesInYear(periods, year);
        IReadOnlyList<DateGap> gaps = GetGaps(periods, year);

        foreach (int milestone in milestones)
        {
            DateOnly anniversary = GetAnniversary(officialAdmissionDate, milestone);

            if (anniversary.Year != year || slices.Any(x => IsWithin(anniversary, x)))
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
        // Đợt vắt năm góp hai phần vào cùng một năm nên độ phủ tính trên "phần trong năm",
        // không phải trên lần diễn ra nguyên vẹn.
        IReadOnlyList<PeriodSlice> ordered = GetSlicesInYear(periods, year);

        List<DateGap> gaps = new();
        DateOnly cursor = new(year, 1, 1);
        DateOnly lastDayOfYear = new(year, 12, 31);

        for (int index = 0; index < ordered.Count; index++)
        {
            PeriodSlice slice = ordered[index];

            if (cursor < slice.From)
            {
                gaps.Add(index == 0
                    ? new DateGap(cursor, slice.From.AddDays(-1), GapKind.BeforeFirstPeriod, "Trước đợt đầu tiên")
                    : new DateGap(
                        cursor,
                        slice.From.AddDays(-1),
                        GapKind.BetweenPeriods,
                        $"Giữa {ordered[index - 1].Period.Name} và {slice.Period.Name}"));
            }

            if (slice.To >= cursor)
            {
                cursor = slice.To.AddDays(1);
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
        IReadOnlyList<PeriodSlice> ordered = GetSlicesInYear(periods, year);

        List<PeriodOverlap> overlaps = new();

        for (int i = 0; i < ordered.Count; i++)
        {
            for (int j = i + 1; j < ordered.Count; j++)
            {
                // Hai phần của CÙNG một đợt vắt năm — đuôi đầu năm và đầu cuối năm — không phải
                // hai đợt chồng lấn, không cảnh báo.
                if (string.Equals(
                        ordered[i].Period.Name, ordered[j].Period.Name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                DateOnly from = ordered[i].From > ordered[j].From ? ordered[i].From : ordered[j].From;
                DateOnly to = ordered[i].To < ordered[j].To ? ordered[i].To : ordered[j].To;

                if (from <= to)
                {
                    overlaps.Add(new PeriodOverlap(ordered[i].Period, ordered[j].Period, from, to));
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

        // Xét cả lần neo năm trước — đợt vắt năm có thể vẫn đang mở hôm nay — và lần neo năm sau
        // khi mọi đợt của năm nay đã qua, rồi lấy lần chưa kết thúc có Từ ngày sớm nhất.
        // Hai đợt cùng Từ ngày thì chọn đợt kết thúc sớm hơn, vẫn bằng nhau thì theo Tên đợt
        // — OQ-9 của hợp đồng API, để Dashboard luôn chọn ra đúng một đợt tất định.
        PeriodOccurrence occurrence = periods
            .SelectMany(period => UpcomingAnchors(period, today.Year)
                .Select(anchor => BindToYear(period, anchor)))
            .Where(x => x.To >= today)
            .OrderBy(x => x.From)
            .ThenBy(x => x.To)
            .ThenBy(x => x.Period.Name, StringComparer.Ordinal)
            .First();

        return new UpcomingPeriod(occurrence, GetStatus(occurrence, today));
    }

    /// <summary>
    /// Các năm neo cần xét khi tìm đợt sắp tới: năm nay và năm sau với mọi đợt, thêm năm trước
    /// với đợt vắt năm vì lần neo năm trước của nó có thể còn kéo dài tới hôm nay.
    /// </summary>
    /// <param name="period">Đợt trao huy hiệu.</param>
    /// <param name="currentYear">Năm của hôm nay.</param>
    /// <returns>Danh sách năm neo cần gắn.</returns>
    private static IEnumerable<int> UpcomingAnchors(AwardPeriod period, int currentYear) =>
        period.SpansNextYear
            ? [currentYear - 1, currentYear, currentYear + 1]
            : [currentYear, currentYear + 1];

    /// <inheritdoc/>
    public PeriodStatusResult GetPeriodStatus(AwardPeriod period, DateOnly today) =>
        GetPeriodStatus(period, today.Year, today);

    /// <inheritdoc/>
    public PeriodStatusResult GetPeriodStatus(AwardPeriod period, int year, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(period);

        // Đợt vắt năm đang mở hôm nay bắt đầu từ NĂM TRƯỚC năm đang xét. Cán bộ mở bảng ngày
        // 15/01 phải thấy "Đang diễn ra", không phải "Sắp tới · 320 ngày" của lần kế tiếp.
        if (period.SpansNextYear)
        {
            PeriodOccurrence previous = BindToYear(period, year - 1);

            if (previous.From <= today && today <= previous.To)
            {
                return new PeriodStatusResult(PeriodStatus.Ongoing, null);
            }
        }

        return GetStatus(BindToYear(period, year), today);
    }

    /// <summary>
    /// Gắn năm vào một cặp ngày/tháng. 29/02 ở năm không nhuận lùi về 28/02 (QT2, QT4).
    /// </summary>
    private static DateOnly BindDayMonth(int day, int month, int year) =>
        month == 2 && day == 29 && !DateTime.IsLeapYear(year)
            ? new DateOnly(year, 2, 28)
            : new DateOnly(year, month, day);

    /// <summary>
    /// Sắp các phần trong năm theo Từ ngày, rồi Đến ngày, rồi Tên để kết quả không phụ thuộc
    /// thứ tự nạp danh sách.
    /// </summary>
    private static List<PeriodSlice> OrderDeterministically(IEnumerable<PeriodSlice> slices) =>
        slices
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

    private static bool IsWithin(DateOnly date, PeriodSlice slice) =>
        date >= slice.From && date <= slice.To;

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
