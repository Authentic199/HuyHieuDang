using HuyHieuDang.Core.PartyBadges;

namespace HuyHieuDang.Core.QcTests.Support;

/// <summary>
/// Bản hiện thực độc lập QT1–QT11 của QC, port từ <c>tests/fixtures/qt_reference.py</c> —
/// chính là oracle đã sinh ra <c>expected.json</c>. Dùng để đối chiếu từng bước với service
/// của Backend trên dải dữ liệu rộng: hai bản viết tách nhau mà ra cùng kết quả thì kết quả
/// đó đáng tin; lệch nhau thì một trong hai sai và QC soi lại cả hai.
/// <para>
/// T54 — phần đợt vắt qua 31/12 được viết lại theo một cách khác hẳn service: service cắt
/// mỗi lần diễn ra thành "phần trong năm" rồi làm toán trên khoảng, còn oracle **đi từng
/// ngày** của năm và hỏi một câu duy nhất "ngày này có nằm trong đợt không?" (xem
/// <see cref="Covers"/>) — không mượn khái niệm lần diễn ra, năm neo hay phần cắt nào của
/// service. Độ phủ, khoảng trống và chồng lấn đều suy ra từ tập ngày ấy, nên một lỗi trong
/// cách cắt phần của service không thể lọt qua vì oracle sai cùng kiểu.
/// </para>
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

    /// <summary>
    /// Câu hỏi gốc của cả oracle: ngày <paramref name="date"/> có nằm trong đợt không? Chỉ so
    /// cặp (tháng, ngày) của chính ngày đó với hai cặp của đợt, đã lùi 29/02 về 28/02 theo năm
    /// của ngày đang hỏi. Đợt vắt năm là phép <c>hoặc</c> — từ Từ ngày tới cuối năm, hoặc từ
    /// đầu năm tới Đến ngày — nên không cần biết lần diễn ra nào sinh ra nó.
    /// </summary>
    /// <param name="period">Đợt trao huy hiệu.</param>
    /// <param name="date">Ngày cần hỏi.</param>
    /// <returns><see langword="true"/> khi ngày nằm trong đợt.</returns>
    public static bool Covers(AwardPeriod period, DateOnly date)
    {
        (int Month, int Day) day = (date.Month, date.Day);
        (int Month, int Day) from = Key(period.FromDay, period.FromMonth, date.Year);
        (int Month, int Day) to = Key(period.ToDay, period.ToMonth, date.Year);

        return SpansNextYear(period)
            ? Compare(day, from) >= 0 || Compare(day, to) <= 0
            : Compare(day, from) >= 0 && Compare(day, to) <= 0;
    }

    /// <summary>
    /// Các đoạn ngày liền nhau mà một đợt phủ trong một năm, tìm bằng cách đi hết 365 (hoặc 366)
    /// ngày. Đợt vắt năm để lại hai đoạn; đợt phủ trọn năm để lại một đoạn.
    /// </summary>
    /// <param name="period">Đợt trao huy hiệu.</param>
    /// <param name="year">Năm đang xét.</param>
    /// <returns>Các đoạn, theo thứ tự ngày tăng dần.</returns>
    public static IReadOnlyList<(DateOnly From, DateOnly To)> Runs(AwardPeriod period, int year) =>
        RunsOf(year, date => Covers(period, date));

    /// <summary>QT6, QT7 — phần của từng đợt rơi vào trong một năm, đã sắp theo ngày bắt đầu.</summary>
    public static IReadOnlyList<(AwardPeriod Period, DateOnly From, DateOnly To)> Slices(
        IReadOnlyList<AwardPeriod> periods, int year) =>
        periods
            .SelectMany(period => Runs(period, year).Select(run => (Period: period, run.From, run.To)))
            .OrderBy(x => x.From)
            .ThenBy(x => x.To)
            .ThenBy(x => x.Period.Name, StringComparer.Ordinal)
            .ToList();

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

    /// <summary>
    /// QT6/QT7 — các khoảng trống trong năm, kèm nhãn. Khoảng trống là các đoạn ngày mà
    /// <b>không đợt nào</b> phủ; nhãn lấy theo tên đợt kề hai bên đúng khuôn chữ của màn hình.
    /// </summary>
    /// <param name="periods">Toàn bộ đợt đang cấu hình.</param>
    /// <param name="year">Năm đang xét.</param>
    /// <returns>Các khoảng trống theo thứ tự ngày tăng dần.</returns>
    public static IReadOnlyList<(DateOnly From, DateOnly To, string Label)> GapRanges(
        IReadOnlyList<AwardPeriod> periods, int year)
    {
        IReadOnlyList<(AwardPeriod Period, DateOnly From, DateOnly To)> slices = Slices(periods, year);
        IReadOnlyList<(DateOnly From, DateOnly To)> gaps =
            RunsOf(year, date => !periods.Any(period => Covers(period, date)));

        List<(DateOnly From, DateOnly To, string Label)> output = new();

        foreach ((DateOnly from, DateOnly to) in gaps)
        {
            string? previous = slices.LastOrDefault(x => x.From < from).Period?.Name;
            string? next = slices.FirstOrDefault(x => x.From > to).Period?.Name;

            // Chưa cài đợt nào thì cả năm nằm trước đợt đầu tiên sắp được tạo (contract mục 1.10).
            string label = previous is null
                ? "Trước đợt đầu tiên"
                : next is null ? "Sau đợt cuối cùng" : $"Giữa {previous} và {next}";

            output.Add((from, to, label));
        }

        return output;
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

            if (periods.Any(period => Covers(period, a)))
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

    /// <summary>
    /// QT8 — đợt sắp tới. Không gắn năm rồi lọc như service: với từng đợt, oracle lùi từ hôm nay
    /// về trước để xem hôm nay có đang nằm trong một lần diễn ra hay không, nếu không thì tiến
    /// tới trước tìm Từ ngày gần nhất. Năm của lần diễn ra là năm chứa Từ ngày.
    /// </summary>
    /// <param name="periods">Toàn bộ đợt đang cấu hình.</param>
    /// <param name="today">Hôm nay.</param>
    /// <returns>Đợt và năm neo của lần diễn ra được chọn.</returns>
    public static (AwardPeriod Period, int Year)? UpcomingPeriod(IReadOnlyList<AwardPeriod> periods, DateOnly today)
    {
        if (periods.Count == 0)
        {
            return null;
        }

        return periods
            .Select(period => (Period: period, Window: Window(period, today)))
            .OrderBy(x => x.Window.From)
            .ThenBy(x => x.Window.To)
            .ThenBy(x => x.Period.Name, StringComparer.Ordinal)
            .Select(x => ((AwardPeriod Period, int Year)?)(x.Period, x.Window.From.Year))
            .First();
    }

    /// <summary>QT11 — trạng thái đợt trong năm hiện tại.</summary>
    public static (PeriodStatus Status, int? DaysLeft) PeriodStatus(AwardPeriod period, DateOnly today) =>
        PeriodStatus(period, today.Year, today);

    /// <summary>
    /// QT11 — trạng thái của lần diễn ra neo ở <paramref name="year"/> so với hôm nay. Lần diễn
    /// ra neo ở năm trước còn đang mở thì đọc là Đang diễn ra.
    /// </summary>
    /// <param name="period">Đợt trao huy hiệu.</param>
    /// <param name="year">Năm neo đang xét.</param>
    /// <param name="today">Hôm nay.</param>
    /// <returns>Trạng thái và số ngày còn lại.</returns>
    public static (PeriodStatus Status, int? DaysLeft) PeriodStatus(AwardPeriod period, int year, DateOnly today)
    {
        (DateOnly from, DateOnly to) = Bind(period, year);

        bool ongoing = (from <= today && today <= to)
            || (SpansNextYear(period) && IsInside(Bind(period, year - 1), today));

        if (ongoing)
        {
            return (HuyHieuDang.Core.PartyBadges.PeriodStatus.Ongoing, null);
        }

        return to < today
            ? (HuyHieuDang.Core.PartyBadges.PeriodStatus.Past, null)
            : (HuyHieuDang.Core.PartyBadges.PeriodStatus.Upcoming, from.DayNumber - today.DayNumber);
    }

    /// <summary>
    /// QT6 — các cặp đợt chồng lấn trong một năm, kèm khoảng ngày dùng chung. Tính bằng giao của
    /// hai tập ngày: một đợt không bao giờ tự chồng lấn với chính nó vì chỉ ghép các cặp đợt khác
    /// nhau.
    /// </summary>
    /// <param name="periods">Toàn bộ đợt đang cấu hình.</param>
    /// <param name="year">Năm đang xét.</param>
    /// <returns>Các cặp chồng lấn, theo thứ tự đoạn ngày tăng dần.</returns>
    public static IReadOnlyList<(string First, string Second, DateOnly From, DateOnly To)> Overlaps(
        IReadOnlyList<AwardPeriod> periods, int year)
    {
        List<(string First, string Second, DateOnly From, DateOnly To, DateOnly Start, DateOnly End)>
            output = new();

        for (int i = 0; i < periods.Count; i++)
        {
            for (int j = i + 1; j < periods.Count; j++)
            {
                AwardPeriod a = periods[i];
                AwardPeriod b = periods[j];

                foreach ((DateOnly from, DateOnly to) in
                    RunsOf(year, date => Covers(a, date) && Covers(b, date)))
                {
                    // Đợt nào mở đoạn chứa phần dùng chung trước thì đứng trước trong cặp.
                    (DateOnly From, DateOnly To) runA = RunAround(a, year, from);
                    (DateOnly From, DateOnly To) runB = RunAround(b, year, from);
                    bool aFirst = Compare((runA.From, runA.To, a.Name), (runB.From, runB.To, b.Name)) <= 0;

                    output.Add(aFirst
                        ? (a.Name, b.Name, from, to, runA.From, runA.To)
                        : (b.Name, a.Name, from, to, runB.From, runB.To));
                }
            }
        }

        return output
            .OrderBy(x => x.Start)
            .ThenBy(x => x.End)
            .ThenBy(x => x.First, StringComparer.Ordinal)
            .ThenBy(x => x.From)
            .ThenBy(x => x.Second, StringComparer.Ordinal)
            .Select(x => (x.First, x.Second, x.From, x.To))
            .ToList();
    }

    /// <summary>Cặp (tháng, ngày) của một ngày/tháng trong một năm, 29/02 lùi về 28/02.</summary>
    private static (int Month, int Day) Key(int day, int month, int year) =>
        month == 2 && day == 29 && !DateTime.IsLeapYear(year) ? (2, 28) : (month, day);

    private static int Compare((int Month, int Day) left, (int Month, int Day) right) =>
        left.Month != right.Month ? left.Month.CompareTo(right.Month) : left.Day.CompareTo(right.Day);

    private static int Compare(
        (DateOnly From, DateOnly To, string Name) left, (DateOnly From, DateOnly To, string Name) right)
    {
        if (left.From != right.From)
        {
            return left.From.CompareTo(right.From);
        }

        return left.To != right.To
            ? left.To.CompareTo(right.To)
            : string.CompareOrdinal(left.Name, right.Name);
    }

    /// <summary>Đi hết một năm, gom các ngày thỏa điều kiện thành những đoạn liền nhau.</summary>
    /// <param name="year">Năm cần đi.</param>
    /// <param name="predicate">Điều kiện của một ngày.</param>
    /// <returns>Các đoạn, theo thứ tự ngày tăng dần.</returns>
    private static IReadOnlyList<(DateOnly From, DateOnly To)> RunsOf(int year, Func<DateOnly, bool> predicate)
    {
        List<(DateOnly From, DateOnly To)> runs = new();
        DateOnly? start = null;
        DateOnly cursor = new(year, 1, 1);

        while (cursor.Year == year)
        {
            bool inside = predicate(cursor);

            if (inside && start is null)
            {
                start = cursor;
            }

            if (!inside && start is not null)
            {
                runs.Add((start.Value, cursor.AddDays(-1)));
                start = null;
            }

            cursor = cursor.AddDays(1);
        }

        if (start is not null)
        {
            runs.Add((start.Value, new DateOnly(year, 12, 31)));
        }

        return runs;
    }

    /// <summary>Đoạn của một đợt trong năm có chứa một ngày cho trước.</summary>
    private static (DateOnly From, DateOnly To) RunAround(AwardPeriod period, int year, DateOnly date) =>
        Runs(period, year).First(run => run.From <= date && date <= run.To);

    /// <summary>
    /// Lần diễn ra của một đợt mà hôm nay đang ở trong, hoặc lần kế tiếp sẽ mở. Đi lùi rồi đi
    /// tới từng ngày, không gắn năm sẵn.
    /// </summary>
    /// <param name="period">Đợt trao huy hiệu.</param>
    /// <param name="today">Hôm nay.</param>
    /// <returns>Khoảng ngày của lần diễn ra được chọn.</returns>
    private static (DateOnly From, DateOnly To) Window(AwardPeriod period, DateOnly today)
    {
        // Hôm nay đang trong đợt: lùi về ngày mở gần nhất; đợt dài nhất cũng dưới 400 ngày.
        if (Covers(period, today))
        {
            DateOnly cursor = today;

            while (!IsFirstDay(period, cursor))
            {
                cursor = cursor.AddDays(-1);
            }

            return (cursor, LastDay(period, cursor));
        }

        DateOnly ahead = today.AddDays(1);

        while (!IsFirstDay(period, ahead))
        {
            ahead = ahead.AddDays(1);
        }

        return (ahead, LastDay(period, ahead));
    }

    /// <summary>Ngày này có phải Từ ngày của đợt trong chính năm của nó hay không.</summary>
    private static bool IsFirstDay(AwardPeriod period, DateOnly date) =>
        Compare((date.Month, date.Day), Key(period.FromDay, period.FromMonth, date.Year)) == 0;

    /// <summary>Đến ngày của lần diễn ra mở vào <paramref name="from"/>.</summary>
    private static DateOnly LastDay(AwardPeriod period, DateOnly from) =>
        BindDayMonth(period.ToDay, period.ToMonth, from.Year + (SpansNextYear(period) ? 1 : 0));

    private static bool IsInside((DateOnly From, DateOnly To) window, DateOnly date) =>
        window.From <= date && date <= window.To;
}
