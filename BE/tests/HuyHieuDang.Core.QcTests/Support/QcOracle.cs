using HuyHieuDang.Core.PartyBadges;

namespace HuyHieuDang.Core.QcTests.Support;

/// <summary>
/// Bản hiện thực độc lập QT1–QT11 của QC, port từ <c>tests/fixtures/qt_reference.py</c> —
/// chính là oracle đã sinh ra <c>expected.json</c>. Dùng để đối chiếu từng bước với service
/// của Backend trên dải dữ liệu rộng: hai bản viết tách nhau mà ra cùng kết quả thì kết quả
/// đó đáng tin; lệch nhau thì một trong hai sai và QC soi lại cả hai.
/// </summary>
public static class QcOracle
{
    /// <summary>QT1 — dãy mốc.</summary>
    public static IReadOnlyList<int> Milestones(int start, int end, int step)
    {
        if (start <= 0 || end <= 0 || step < 1)
        {
            throw new ArgumentException("QT1: cả 3 tham số phải dương, Bước >= 1.");
        }

        if (start > end)
        {
            throw new ArgumentException("QT1: Bắt đầu phải <= Kết thúc.");
        }

        List<int> output = new();
        for (long n = start; n <= end; n += step)
        {
            output.Add((int)n);
        }

        return output;
    }

    /// <summary>Gắn năm vào cặp ngày/tháng; 29/02 ở năm không nhuận lùi về 28/02.</summary>
    public static DateOnly BindDayMonth(int day, int month, int year) =>
        month == 2 && day == 29 && !DateTime.IsLeapYear(year)
            ? new DateOnly(year, 2, 28)
            : new DateOnly(year, month, day);

    /// <summary>QT2 — ngày tròn mốc.</summary>
    public static DateOnly Anniversary(DateOnly d, int n) => BindDayMonth(d.Day, d.Month, d.Year + n);

    /// <summary>QT3 — tuổi đảng.</summary>
    public static int PartyAge(DateOnly d, DateOnly today)
    {
        int k = today.Year - d.Year;

        while (k > 0 && Anniversary(d, k) > today)
        {
            k--;
        }

        return k < 0 ? 0 : k;
    }

    /// <summary>QT3a — mốc kế tiếp.</summary>
    public static int? NextMilestone(DateOnly d, DateOnly today, IReadOnlyList<int> milestones)
    {
        int age = PartyAge(d, today);

        foreach (int n in milestones)
        {
            if (n > age)
            {
                return n;
            }
        }

        return null;
    }

    /// <summary>QT6 — đợt vắt qua 31/12 khi Đến ngày đứng trước Từ ngày trong vòng năm.</summary>
    public static bool SpansNextYear(AwardPeriod period) =>
        period.FromMonth > period.ToMonth
        || (period.FromMonth == period.ToMonth && period.FromDay > period.ToDay);

    /// <summary>Gắn năm vào một đợt; đợt vắt năm có Đến ngày thuộc năm kế tiếp.</summary>
    public static (DateOnly From, DateOnly To) Bind(AwardPeriod period, int year) =>
        (BindDayMonth(period.FromDay, period.FromMonth, year),
            BindDayMonth(period.ToDay, period.ToMonth, year + (SpansNextYear(period) ? 1 : 0)));

    /// <summary>QT6, QT7 — phần của từng đợt rơi vào trong một năm, đã sắp theo ngày bắt đầu.</summary>
    public static IReadOnlyList<(AwardPeriod Period, DateOnly From, DateOnly To)> Slices(
        IReadOnlyList<AwardPeriod> periods, int year)
    {
        DateOnly firstDay = new(year, 1, 1);
        DateOnly lastDay = new(year, 12, 31);
        List<(AwardPeriod Period, DateOnly From, DateOnly To)> slices = new();

        foreach (AwardPeriod period in periods)
        {
            int[] anchors = SpansNextYear(period) ? [year - 1, year] : [year];

            foreach (int anchor in anchors)
            {
                (DateOnly from, DateOnly to) = Bind(period, anchor);
                DateOnly cut = from > firstDay ? from : firstDay;
                DateOnly end = to < lastDay ? to : lastDay;

                if (cut <= end)
                {
                    slices.Add((period, cut, end));
                }
            }
        }

        return slices
            .OrderBy(x => x.From)
            .ThenBy(x => x.To)
            .ThenBy(x => x.Period.Name, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>QT4 — mốc được trao trong đợt/năm.</summary>
    public static int? EligibleMilestone(DateOnly d, AwardPeriod period, int year, IReadOnlyList<int> milestones)
    {
        (DateOnly from, DateOnly to) = Bind(period, year);

        foreach (int n in milestones)
        {
            DateOnly a = Anniversary(d, n);

            if (from <= a && a <= to)
            {
                return n;
            }
        }

        return null;
    }

    /// <summary>QT6/QT7 — các khoảng trống trong năm, kèm nhãn.</summary>
    public static IReadOnlyList<(DateOnly From, DateOnly To, string Label)> GapRanges(
        IReadOnlyList<AwardPeriod> periods, int year)
    {
        IReadOnlyList<(AwardPeriod Period, DateOnly From, DateOnly To)> ordered = Slices(periods, year);

        List<(DateOnly From, DateOnly To, string Label)> gaps = new();
        DateOnly cursor = new(year, 1, 1);

        for (int index = 0; index < ordered.Count; index++)
        {
            (AwardPeriod _, DateOnly from, DateOnly to) = ordered[index];

            if (cursor < from)
            {
                string label = index == 0
                    ? "Trước đợt đầu tiên"
                    : $"Giữa {ordered[index - 1].Period.Name} và {ordered[index].Period.Name}";

                gaps.Add((cursor, from.AddDays(-1), label));
            }

            if (to >= cursor)
            {
                cursor = to.AddDays(1);
            }
        }

        if (cursor <= new DateOnly(year, 12, 31))
        {
            // Chưa cài đợt nào thì cả năm nằm trước đợt đầu tiên sắp được tạo (contract mục 1.10).
            gaps.Add((
                cursor,
                new DateOnly(year, 12, 31),
                ordered.Count == 0 ? "Trước đợt đầu tiên" : "Sau đợt cuối cùng"));
        }

        return gaps;
    }

    /// <summary>QT7 — mốc tròn trong năm nhưng không rơi vào đợt nào.</summary>
    public static (int Milestone, DateOnly Anniversary, string Label)? MissedMilestone(
        DateOnly d, IReadOnlyList<AwardPeriod> periods, int year, IReadOnlyList<int> milestones)
    {
        foreach (int n in milestones)
        {
            DateOnly a = Anniversary(d, n);

            if (a.Year != year)
            {
                continue;
            }

            bool inside = Slices(periods, year).Any(x => x.From <= a && a <= x.To);

            if (inside)
            {
                continue;
            }

            string label = GapRanges(periods, year)
                .Where(x => x.From <= a && a <= x.To)
                .Select(x => x.Label)
                .FirstOrDefault() ?? "Không xác định";

            return (n, a, label);
        }

        return null;
    }

    /// <summary>QT8 — đợt sắp tới.</summary>
    public static (AwardPeriod Period, int Year)? UpcomingPeriod(IReadOnlyList<AwardPeriod> periods, DateOnly today)
    {
        if (periods.Count == 0)
        {
            return null;
        }

        int y = today.Year;

        return periods
            .SelectMany(period => (SpansNextYear(period) ? new[] { y - 1, y, y + 1 } : [y, y + 1])
                .Select(anchor => (Period: period, Year: anchor, Bound: Bind(period, anchor))))
            .Where(x => x.Bound.To >= today)
            .OrderBy(x => x.Bound.From)
            .ThenBy(x => x.Bound.To)
            .ThenBy(x => x.Period.Name, StringComparer.Ordinal)
            .Select(x => ((AwardPeriod Period, int Year)?)(x.Period, x.Year))
            .First();
    }

    /// <summary>QT11 — trạng thái đợt trong năm hiện tại.</summary>
    public static (PeriodStatus Status, int? DaysLeft) PeriodStatus(AwardPeriod period, DateOnly today)
    {
        if (SpansNextYear(period))
        {
            (DateOnly previousFrom, DateOnly previousTo) = Bind(period, today.Year - 1);

            if (previousFrom <= today && today <= previousTo)
            {
                return (HuyHieuDang.Core.PartyBadges.PeriodStatus.Ongoing, null);
            }
        }

        (DateOnly from, DateOnly to) = Bind(period, today.Year);

        if (to < today)
        {
            return (HuyHieuDang.Core.PartyBadges.PeriodStatus.Past, null);
        }

        if (from <= today && today <= to)
        {
            return (HuyHieuDang.Core.PartyBadges.PeriodStatus.Ongoing, null);
        }

        return (HuyHieuDang.Core.PartyBadges.PeriodStatus.Upcoming, from.DayNumber - today.DayNumber);
    }

    /// <summary>QT6 — các cặp đợt chồng lấn.</summary>
    public static IReadOnlyList<(string First, string Second)> Overlaps(
        IReadOnlyList<AwardPeriod> periods, int year)
    {
        IReadOnlyList<(AwardPeriod Period, DateOnly From, DateOnly To)> ordered = Slices(periods, year);

        List<(string First, string Second)> output = new();

        for (int i = 0; i < ordered.Count; i++)
        {
            for (int j = i + 1; j < ordered.Count; j++)
            {
                // Hai phần của cùng một đợt vắt năm không phải hai đợt chồng lấn.
                if (string.Equals(
                        ordered[i].Period.Name, ordered[j].Period.Name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (ordered[i].From <= ordered[j].To && ordered[j].From <= ordered[i].To)
                {
                    output.Add((ordered[i].Period.Name, ordered[j].Period.Name));
                }
            }
        }

        return output;
    }
}
