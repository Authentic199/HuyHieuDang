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

    /// <summary>Gắn năm vào một đợt.</summary>
    public static (DateOnly From, DateOnly To) Bind(AwardPeriod period, int year) =>
        (BindDayMonth(period.FromDay, period.FromMonth, year), BindDayMonth(period.ToDay, period.ToMonth, year));

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
        List<AwardPeriod> ordered = periods
            .OrderBy(x => x.FromMonth)
            .ThenBy(x => x.FromDay)
            .ToList();

        List<(DateOnly From, DateOnly To, string Label)> gaps = new();
        DateOnly cursor = new(year, 1, 1);

        for (int index = 0; index < ordered.Count; index++)
        {
            (DateOnly from, DateOnly to) = Bind(ordered[index], year);

            if (cursor < from)
            {
                string label = index == 0
                    ? "Trước đợt đầu tiên"
                    : $"Giữa {ordered[index - 1].Name} và {ordered[index].Name}";

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

            bool inside = periods
                .Select(x => Bind(x, year))
                .Any(x => x.From <= a && a <= x.To);

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
        List<AwardPeriod> candidates = periods.Where(x => Bind(x, y).To >= today).ToList();

        return candidates.Count > 0
            ? (candidates.OrderBy(x => Bind(x, y).From).First(), y)
            : (periods.OrderBy(x => Bind(x, y + 1).From).First(), y + 1);
    }

    /// <summary>QT11 — trạng thái đợt trong năm hiện tại.</summary>
    public static (PeriodStatus Status, int? DaysLeft) PeriodStatus(AwardPeriod period, DateOnly today)
    {
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
        List<AwardPeriod> ordered = periods
            .OrderBy(x => x.FromMonth)
            .ThenBy(x => x.FromDay)
            .ToList();

        List<(string First, string Second)> output = new();

        for (int i = 0; i < ordered.Count; i++)
        {
            for (int j = i + 1; j < ordered.Count; j++)
            {
                (DateOnly fa, DateOnly ta) = Bind(ordered[i], year);
                (DateOnly fb, DateOnly tb) = Bind(ordered[j], year);

                if (fa <= tb && fb <= ta)
                {
                    output.Add((ordered[i].Name, ordered[j].Name));
                }
            }
        }

        return output;
    }
}
