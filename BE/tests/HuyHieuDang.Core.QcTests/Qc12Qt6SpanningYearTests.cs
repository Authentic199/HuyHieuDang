using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.QcTests.Support;

namespace HuyHieuDang.Core.QcTests;

/// <summary>
/// QT4, QT6, QT7, QT8, QT11 — đợt trao huy hiệu vắt qua 31/12 (T54).
/// <para>
/// Mã ca: CEO đặt tên U-610 → U-615 trong T54, nhưng ba mã U-610, U-611, U-612 đã có chủ
/// trong <c>docs/test-plan.md</c> (khoảng trống của bộ 4 đợt, bộ phủ kín, hai đợt kề nhau).
/// Để không có hai ca khác nghĩa cùng một mã, khối mới của T54 nhận mã U-620 → U-626; bảng
/// quy đổi nằm trong <c>docs/test-report/2026-09-20-qc-dot-vat-qua-nam.md</c>.
/// </para>
/// </summary>
[Trait("Tang", "Logic")]
[Trait("QuyTac", "QT6")]
public sealed class Qc12Qt6SpanningYearTests
{
    /// <summary>Ca thật của đảng ủy: đợt chạy 01/12 năm nay đến 28/02 năm sau.</summary>
    private static readonly AwardPeriod GiaoThua = new("Đợt Giao thừa", 1, 12, 28, 2);

    private readonly IPartyMilestoneCalculator sut = new PartyMilestoneCalculator();

    [Fact(DisplayName = "U-620 · Đợt 01/12 – 30/11 dài 365 ngày vẫn chỉ trao tối đa 1 mốc/người/đợt")]
    public void U620_AFullLengthSpanningPeriodStillAwardsAtMostOneMilestonePerMember()
    {
        AwardPeriod period = new("Đợt dài", 1, 12, 30, 11);

        // Bước = 1 là cấu hình khắc nghiệt nhất: hai mốc liền nhau chỉ cách nhau đúng một năm,
        // còn đợt thì dài 364–365 ngày. Nếu QT4 hụt ở đâu thì hụt ở đây.
        IReadOnlyList<int> milestones = QcOracle.Milestones(1, 100, 1);
        List<string> loi = new();

        foreach (DateOnly admission in EveryDayMonth(1960))
        {
            for (int year = 2024; year <= 2030; year++)
            {
                PeriodOccurrence occurrence = sut.BindToYear(period, year);
                List<int> inside = milestones
                    .Where(n => IsInside(sut.GetAnniversary(admission, n), occurrence))
                    .ToList();

                if (inside.Count > 1)
                {
                    loi.Add($"{admission:dd/MM/yyyy} năm {year}: {inside.Count} mốc trong một đợt");
                }

                int? awarded = sut.GetEligibleMilestone(admission, period, year, milestones);
                int? expected = QcOracle.EligibleMilestone(admission, period, year, milestones);

                if (awarded != expected)
                {
                    loi.Add($"{admission:dd/MM/yyyy} năm {year}: service {awarded}, QC {expected}");
                }
            }
        }

        loi.ShouldBeEmpty();
    }

    [Fact(DisplayName = "U-621 · Đợt 02/01 – 01/01 phủ trọn năm: không mốc trùng, không vòng lặp vô hạn")]
    public void U621_AnAlmostFullYearSpanningPeriodTerminatesAndNeverDoublesAMilestone()
    {
        AwardPeriod period = new("Đợt trọn năm", 2, 1, 1, 1);
        List<AwardPeriod> periods =[period];

        // Vòng lặp vô hạn trong cách cắt phần sẽ treo cả lần chạy, nên phải chặn bằng đồng hồ
        // thay vì chờ bộ chạy test tự bỏ cuộc.
        bool finished = Task.Run(() =>
        {
            for (int year = 2024; year <= 2030; year++)
            {
                sut.GetSlicesInYear(periods, year);
                sut.GetGaps(periods, year);
                sut.GetOverlaps(periods, year);
                sut.GetUpcomingPeriod(periods, new DateOnly(year, 6, 15));
            }
        }).Wait(TimeSpan.FromSeconds(30));

        finished.ShouldBeTrue("GetSlicesInYear/GetGaps/GetOverlaps không dừng lại với đợt 02/01 – 01/01");

        for (int year = 2024; year <= 2030; year++)
        {
            sut.GetGaps(periods, year).ShouldBeEmpty($"Năm {year} phải được phủ kín");
            sut.GetOverlaps(periods, year).ShouldBeEmpty($"Năm {year}: một đợt không tự chồng lấn");

            // Phủ kín thì không ai bị xếp vào "chưa thuộc đợt nào", và không ai được trao hai mốc.
            foreach (DateOnly admission in EveryDayMonth(1980))
            {
                sut.GetMissedMilestone(admission, periods, year, QcOracle.Milestones(30, 90, 5))
                    .ShouldBeNull($"{admission:dd/MM} năm {year} bị xếp ra ngoài đợt dù năm đã phủ kín");
            }
        }
    }

    [Theory(DisplayName = "U-622 · Đợt vắt năm kết thúc 29/02 lùi về 28/02 khi năm kết thúc không nhuận")]
    [InlineData(2026, "2026-12-01", "2027-02-28")]
    [InlineData(2027, "2027-12-01", "2028-02-29")]
    [InlineData(2028, "2028-12-01", "2029-02-28")]
    public void U622_ASpanningPeriodEndingOn29February(int year, string from, string to)
    {
        PeriodOccurrence occurrence = sut.BindToYear(new AwardPeriod("Đợt 29/02", 1, 12, 29, 2), year);

        occurrence.From.ShouldBe(QcFixtures.ParseDate(from));
        occurrence.To.ShouldBe(QcFixtures.ParseDate(to));
    }

    [Fact(DisplayName = "U-623 · Người tròn mốc đúng 01/12 và đúng 28/02 đều đủ điều kiện")]
    public void U623_BothEdgesOfASpanningPeriodAreInclusive()
    {
        IReadOnlyList<int> milestones = QcOracle.Milestones(30, 90, 5);
        List<AwardPeriod> periods =[GiaoThua];

        // Mốc 30: vào Đảng 01/12/1996 → tròn mốc đúng Từ ngày; 28/02/1997 → tròn mốc đúng Đến ngày.
        DateOnly openingEdge = new(1996, 12, 1);
        DateOnly closingEdge = new(1997, 2, 28);

        sut.GetEligibleMilestone(openingEdge, GiaoThua, 2026, milestones).ShouldBe(30);
        sut.GetEligibleMilestone(closingEdge, GiaoThua, 2026, milestones).ShouldBe(30);

        // Và không ai trong hai người bị đồng thời xếp vào "chưa thuộc đợt nào" của năm chứa
        // ngày tròn mốc của họ — dấu hiệu số 2 trong danh sách "báo ngay" của T54.
        sut.GetMissedMilestone(openingEdge, periods, 2026, milestones).ShouldBeNull();
        sut.GetMissedMilestone(closingEdge, periods, 2027, milestones).ShouldBeNull();

        // Ngày liền ngoài hai biên thì ngược lại: không đủ điều kiện, và rơi vào khoảng trống.
        sut.GetEligibleMilestone(new DateOnly(1996, 11, 30), GiaoThua, 2026, milestones).ShouldBeNull();
        sut.GetEligibleMilestone(new DateOnly(1997, 3, 1), GiaoThua, 2026, milestones).ShouldBeNull();
        sut.GetMissedMilestone(new DateOnly(1996, 11, 30), periods, 2026, milestones).ShouldNotBeNull();
        sut.GetMissedMilestone(new DateOnly(1997, 3, 1), periods, 2027, milestones).ShouldNotBeNull();
    }

    [Fact(DisplayName = "U-624a · Đợt vắt năm chồng lấn một đợt thường ở đầu năm: đúng một cặp, đúng khoảng ngày")]
    public void U624a_ASpanningPeriodOverlappingAPlainPeriodIsReportedOnce()
    {
        List<AwardPeriod> periods =[GiaoThua, new("Đợt Xuân", 15, 1, 10, 3)];

        IReadOnlyList<PeriodOverlap> overlaps = sut.GetOverlaps(periods, 2027);

        overlaps.Count.ShouldBe(1);
        overlaps[0].First.Name.ShouldBe("Đợt Giao thừa");
        overlaps[0].Second.Name.ShouldBe("Đợt Xuân");
        overlaps[0].From.ShouldBe(new DateOnly(2027, 1, 15));
        overlaps[0].To.ShouldBe(new DateOnly(2027, 2, 28));
    }

    [Fact(DisplayName = "U-624b · Hai đợt vắt năm: cặp chồng lấn hiện đúng hai đoạn, đầu năm và cuối năm")]
    public void U624b_TwoSpanningPeriodsOverlapAtBothEndsOfTheYear()
    {
        // Hai đợt vắt năm bao giờ cũng dùng chung mấy ngày cuối tháng 12, nên trong một năm
        // dương lịch chúng gặp nhau ở hai đoạn rời nhau — mỗi đoạn là một lần diễn ra khác.
        List<AwardPeriod> periods =[GiaoThua, new("Đợt Chạp", 20, 11, 20, 1)];

        List<(string First, string Second, DateOnly From, DateOnly To)> actual = sut
            .GetOverlaps(periods, 2027)
            .Select(x => (x.First.Name, x.Second.Name, x.From, x.To))
            .ToList();

        actual.ShouldBe(QcOracle.Overlaps(periods, 2027).ToList());
        actual.Count.ShouldBe(2);
        actual[0].From.ShouldBe(new DateOnly(2027, 1, 1));
        actual[0].To.ShouldBe(new DateOnly(2027, 1, 20));
        actual[1].From.ShouldBe(new DateOnly(2027, 12, 1));
        actual[1].To.ShouldBe(new DateOnly(2027, 12, 31));
    }

    [Fact(DisplayName = "U-625 · Xóa đợt vắt năm thì khoảng trống mới phủ đúng hai đầu năm")]
    public void U625_RemovingASpanningPeriodReopensBothEndsOfTheYear()
    {
        AwardPeriod summer = new("Đợt Hè", 1, 6, 30, 6);
        List<AwardPeriod> before =[GiaoThua, summer];
        List<AwardPeriod> after =[summer];

        sut.GetGaps(before, 2027)
            .Select(x => (x.From, x.To))
            .ShouldBe(
            [
                (new DateOnly(2027, 3, 1), new DateOnly(2027, 5, 31)),
                (new DateOnly(2027, 7, 1), new DateOnly(2027, 11, 30)),
            ]);

        sut.GetGaps(after, 2027)
            .Select(x => (x.From, x.To))
            .ShouldBe(
            [
                (new DateOnly(2027, 1, 1), new DateOnly(2027, 5, 31)),
                (new DateOnly(2027, 7, 1), new DateOnly(2027, 12, 31)),
            ]);

        // Người tròn mốc 20/01/2027 mất chỗ khi đợt vắt năm bị xóa — đúng chiều lan truyền QT5.
        IReadOnlyList<int> milestones = QcOracle.Milestones(30, 90, 5);
        DateOnly admission = new(1997, 1, 20);

        sut.GetMissedMilestone(admission, before, 2027, milestones).ShouldBeNull();
        sut.GetMissedMilestone(admission, after, 2027, milestones).ShouldNotBeNull();
    }

    [Fact(DisplayName = "U-626 · Service và oracle độc lập của QC khớp nhau trên mọi bộ đợt vắt năm, 2024–2030")]
    public void U626_TheServiceMatchesTheIndependentOracleOnSpanningPeriods()
    {
        List<string> lech = new();

        foreach (IReadOnlyList<AwardPeriod> set in SpanningSets())
        {
            for (int year = 2024; year <= 2030; year++)
            {
                CompareCoverage(set, year, lech);
            }

            DateOnly cursor = new(2026, 1, 1);

            while (cursor < new DateOnly(2028, 1, 1))
            {
                CompareClock(set, cursor, lech);
                cursor = cursor.AddDays(1);
            }
        }

        lech.ShouldBeEmpty();
    }

    /// <summary>Độ phủ, khoảng trống và chồng lấn của một bộ đợt trong một năm.</summary>
    private void CompareCoverage(IReadOnlyList<AwardPeriod> set, int year, List<string> lech)
    {
        string name = string.Join(",", set.Select(x => x.Name));

        // Service cắt theo lần diễn ra nên đợt phủ trọn năm để lại HAI phần dính nhau
        // (01/01–30/11 | 01/12–31/12); oracle đi theo ngày nên thấy một dải liền. Hai cách chia
        // khác nhau mà phủ đúng bằng ấy ngày thì không có gì sai — vì vậy so sau khi nối các
        // đoạn liền nhau của cùng một đợt.
        List<string> slices = Merge(sut.GetSlicesInYear(set, year)
            .Select(x => (x.Period.Name, x.From, x.To)));
        List<string> expectedSlices = Merge(QcOracle.Slices(set, year)
            .Select(x => (x.Period.Name, x.From, x.To)));

        if (!slices.SequenceEqual(expectedSlices))
        {
            lech.Add($"[{name}] {year} phần trong năm: service [{string.Join(" | ", slices)}]"
                + $" — QC [{string.Join(" | ", expectedSlices)}]");
        }

        List<string> gaps = sut.GetGaps(set, year)
            .Select(x => $"{x.From:MM-dd}..{x.To:MM-dd} {x.Label}")
            .ToList();
        List<string> expectedGaps = QcOracle.GapRanges(set, year)
            .Select(x => $"{x.From:MM-dd}..{x.To:MM-dd} {x.Label}")
            .ToList();

        if (!gaps.SequenceEqual(expectedGaps))
        {
            lech.Add($"[{name}] {year} khoảng trống: service [{string.Join(" | ", gaps)}]"
                + $" — QC [{string.Join(" | ", expectedGaps)}]");
        }

        // So cả cặp đợt lẫn từng ngày bị đánh dấu chồng lấn: chia đoạn kiểu nào cũng phải phủ
        // đúng bằng ấy ngày.
        List<string> overlaps = sut.GetOverlaps(set, year)
            .Select(x => $"{x.First.Name}+{x.Second.Name}:{x.From:MM-dd}..{x.To:MM-dd}")
            .ToList();
        List<string> expectedOverlaps = QcOracle.Overlaps(set, year)
            .Select(x => $"{x.First}+{x.Second}:{x.From:MM-dd}..{x.To:MM-dd}")
            .ToList();

        if (!overlaps.SequenceEqual(expectedOverlaps))
        {
            lech.Add($"[{name}] {year} chồng lấn: service [{string.Join(" | ", overlaps)}]"
                + $" — QC [{string.Join(" | ", expectedOverlaps)}]");
        }
    }

    /// <summary>Đợt sắp tới và trạng thái của một bộ đợt tại một ngày.</summary>
    private void CompareClock(IReadOnlyList<AwardPeriod> set, DateOnly today, List<string> lech)
    {
        string name = string.Join(",", set.Select(x => x.Name));

        UpcomingPeriod? upcoming = sut.GetUpcomingPeriod(set, today);
        (AwardPeriod Period, int Year)? expected = QcOracle.UpcomingPeriod(set, today);
        string actualText = upcoming is null
            ? "không có"
            : $"{upcoming.Occurrence.Period.Name}/{upcoming.Occurrence.Year}";
        string expectedText = expected is null
            ? "không có"
            : $"{expected.Value.Period.Name}/{expected.Value.Year}";

        if (actualText != expectedText)
        {
            lech.Add($"[{name}] {today:dd/MM/yyyy} đợt sắp tới: service {actualText}, QC {expectedText}");
        }

        foreach (AwardPeriod period in set)
        {
            PeriodStatusResult status = sut.GetPeriodStatus(period, today.Year, today);
            (PeriodStatus Status, int? DaysLeft) expectedStatus = QcOracle.PeriodStatus(period, today.Year, today);

            if (status.Status != expectedStatus.Status || status.DaysLeft != expectedStatus.DaysLeft)
            {
                lech.Add($"[{period.Name}] {today:dd/MM/yyyy} trạng thái: service"
                    + $" {status.Status}/{status.DaysLeft}, QC {expectedStatus.Status}/{expectedStatus.DaysLeft}");
            }
        }
    }

    /// <summary>Các bộ đợt có ít nhất một đợt vắt qua 31/12.</summary>
    private static IEnumerable<IReadOnlyList<AwardPeriod>> SpanningSets()
    {
        yield return[GiaoThua];
        yield return[GiaoThua, new("Đợt Hè", 1, 6, 30, 6)];
        yield return[GiaoThua, new("Đợt Xuân", 15, 1, 10, 3)];
        yield return[GiaoThua, new("Đợt Chạp", 20, 11, 20, 1)];
        yield return[new("Đợt 29/02", 1, 12, 29, 2)];
        yield return[new("Đợt dài", 1, 12, 30, 11)];
        yield return[new("Đợt trọn năm", 2, 1, 1, 1)];
        yield return[new("Đợt một ngày sau Tết", 1, 3, 28, 2)];
        yield return
        [
            GiaoThua,
            new("Đợt 19/5", 15, 5, 25, 5),
            new("Đợt 2/9", 25, 8, 10, 9),
            new("Đợt 7/11", 1, 10, 7, 11),
        ];
    }

    /// <summary>
    /// Nối các đoạn liền nhau của cùng một đợt rồi viết thành chuỗi so sánh được.
    /// </summary>
    /// <param name="slices">Các phần trong năm của mọi đợt.</param>
    /// <returns>Chuỗi mô tả từng đoạn, theo thứ tự ngày tăng dần.</returns>
    private static List<string> Merge(IEnumerable<(string Name, DateOnly From, DateOnly To)> slices)
    {
        List<(string Name, DateOnly From, DateOnly To)> ordered = slices
            .OrderBy(x => x.Name, StringComparer.Ordinal)
            .ThenBy(x => x.From)
            .ToList();
        List<(string Name, DateOnly From, DateOnly To)> merged = new();

        foreach ((string name, DateOnly from, DateOnly to) in ordered)
        {
            if (merged.Count > 0
                && merged[^1].Name == name
                && merged[^1].To.AddDays(1) >= from)
            {
                merged[^1] = (name, merged[^1].From, merged[^1].To > to ? merged[^1].To : to);
                continue;
            }

            merged.Add((name, from, to));
        }

        return merged
            .OrderBy(x => x.From)
            .ThenBy(x => x.To)
            .ThenBy(x => x.Name, StringComparer.Ordinal)
            .Select(x => $"{x.Name}:{x.From:MM-dd}..{x.To:MM-dd}")
            .ToList();
    }

    /// <summary>Mỗi ngày/tháng có thật đúng một lần, lấy trên một năm nhuận.</summary>
    private static IEnumerable<DateOnly> EveryDayMonth(int year)
    {
        DateOnly cursor = new(year, 1, 1);

        while (cursor.Year == year)
        {
            yield return cursor;
            cursor = cursor.AddDays(1);
        }
    }

    private static bool IsInside(DateOnly date, PeriodOccurrence occurrence) =>
        date >= occurrence.From && date <= occurrence.To;
}
