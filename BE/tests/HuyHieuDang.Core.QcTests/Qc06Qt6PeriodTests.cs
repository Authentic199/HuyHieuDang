using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.QcTests.Support;

namespace HuyHieuDang.Core.QcTests;

/// <summary>QT6 — đợt trao huy hiệu: ràng buộc, khoảng trống, chồng lấn.</summary>
[Trait("Tang", "Logic")]
[Trait("QuyTac", "QT6")]
public sealed class Qc06Qt6PeriodTests
{
    private readonly IPartyMilestoneCalculator sut = new PartyMilestoneCalculator();

    private IReadOnlyList<AwardPeriod> MainPeriods => QcFixtures.MainPeriods.Select(x => x.Period).ToList();

    [Theory(DisplayName = "U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày")]
    [InlineData(1, 10, 7, 11, 2026, "2026-10-01", "2026-11-07")]
    [InlineData(1, 10, 1, 10, 2026, "2026-10-01", "2026-10-01")]
    [InlineData(1, 1, 31, 12, 2026, "2026-01-01", "2026-12-31")]
    [InlineData(29, 2, 5, 3, 2026, "2026-02-28", "2026-03-05")]
    [InlineData(29, 2, 5, 3, 2028, "2028-02-29", "2028-03-05")]
    public void U601_ValidPeriodsBindToTheExpectedDates(
        int fromDay, int fromMonth, int toDay, int toMonth, int year, string from, string to)
    {
        PeriodOccurrence occurrence = sut.BindToYear(
            new AwardPeriod("Đợt kiểm thử", fromDay, fromMonth, toDay, toMonth), year);

        occurrence.From.ShouldBe(QcFixtures.ParseDate(from));
        occurrence.To.ShouldBe(QcFixtures.ParseDate(to));
    }

    [Theory(DisplayName = "U-610 · Bộ 4 đợt chính để hở đúng 5 khoảng trống ở mọi năm xét")]
    [InlineData(2025)]
    [InlineData(2026)]
    [InlineData(2027)]
    [InlineData(2028)]
    public void U610_TheFourMainPeriodsLeaveFiveGapsEveryYear(int year)
    {
        sut.GetGaps(MainPeriods, year).Count.ShouldBe(5);
    }

    [Fact(DisplayName = "QC · Các khoảng trống không chồng nhau, không thủng, phủ đúng phần còn lại của năm")]
    public void TheGapsPartitionTheUncoveredPartOfTheYear()
    {
        List<string> loi = new();

        foreach (IReadOnlyList<AwardPeriod> set in AllPeriodSets())
        {
            for (int year = 2024; year <= 2030; year++)
            {
                IReadOnlyList<DateGap> gaps = sut.GetGaps(set, year);
                List<PeriodOccurrence> bound = set.Select(x => sut.BindToYear(x, year)).ToList();
                DateOnly cursor = new(year, 1, 1);

                while (cursor.Year == year)
                {
                    bool inPeriod = bound.Exists(x => cursor >= x.From && cursor <= x.To);
                    int inGap = gaps.Count(x => cursor >= x.From && cursor <= x.To);

                    if (inPeriod && inGap > 0)
                    {
                        loi.Add($"{cursor:dd/MM/yyyy} vua trong dot vua trong khoang trong");
                    }

                    if (!inPeriod && inGap != 1)
                    {
                        loi.Add($"{cursor:dd/MM/yyyy} khong thuoc dot nao nhung nam trong {inGap} khoang trong");
                    }

                    cursor = cursor.AddDays(1);
                }
            }
        }

        loi.ShouldBeEmpty();
    }

    [Fact(DisplayName = "QC · Khoảng trống khớp oracle độc lập của QC trên mọi bộ đợt, 2024–2030")]
    public void TheGapsMatchTheQcOracle()
    {
        List<string> lech = new();

        foreach (IReadOnlyList<AwardPeriod> set in AllPeriodSets())
        {
            for (int year = 2024; year <= 2030; year++)
            {
                List<string> actual = sut.GetGaps(set, year)
                    .Select(x => $"{x.From:yyyy-MM-dd}..{x.To:yyyy-MM-dd} {x.Label}")
                    .ToList();

                List<string> expected = QcOracle.GapRanges(set, year)
                    .Select(x => $"{x.From:yyyy-MM-dd}..{x.To:yyyy-MM-dd} {x.Label}")
                    .ToList();

                if (!actual.SequenceEqual(expected))
                {
                    lech.Add($"{year}: service [{string.Join(" | ", actual)}] — QC [{string.Join(" | ", expected)}]");
                }
            }
        }

        lech.ShouldBeEmpty();
    }

    [Fact(DisplayName = "U-609/U-611/U-612 · Cảnh báo chồng lấn khớp oracle của QC")]
    public void TheOverlapWarningsMatchTheQcOracle()
    {
        List<AwardPeriod> backToBack =
        [
            new("Đợt kề 1", 1, 10, 7, 11),
            new("Đợt kề 2", 8, 11, 31, 12),
        ];

        foreach (IReadOnlyList<AwardPeriod> set in AllPeriodSets().Append(backToBack))
        {
            for (int year = 2024; year <= 2030; year++)
            {
                List<string> actual = sut.GetOverlaps(set, year).Select(x => $"{x.First.Name}+{x.Second.Name}").ToList();
                List<string> expected = QcOracle.Overlaps(set, year).Select(x => $"{x.First}+{x.Second}").ToList();

                actual.ShouldBe(expected, $"Bộ đợt {string.Join(",", set.Select(x => x.Name))} năm {year}");
            }
        }
    }

    [Fact(DisplayName = "U-609 · Hai đợt chồng lấn vẫn được tính bình thường, chỉ là cảnh báo")]
    public void U609_OverlappingPeriodsAreStillComputable()
    {
        List<AwardPeriod> overlap = QcFixtures.OverlapPeriods.Select(x => x.Period).ToList();

        sut.GetOverlaps(overlap, 2026).Count.ShouldBe(1);
        sut.GetGaps(overlap, 2026).ShouldNotBeEmpty();
        sut.GetUpcomingPeriod(overlap, QcFixtures.T0).ShouldNotBeNull();
    }

    // LỖI QC-01 — Tầng logic chưa có chỗ nào kiểm ràng buộc QT6 của đợt.
    // Hai ca dưới đây mô tả hành vi ĐÚNG theo U-602, U-606, U-607 và đang đỏ:
    //  - Từ ngày > Đến ngày: service nhận bừa, GetGaps trả ra các khoảng trống chồng lên nhau.
    //  - Ngày/tháng không tồn tại (31/02, 31/04): BindToYear ném ArgumentOutOfRangeException
    //    (lỗi kỹ thuật → 500) thay vì lỗi nghiệp vụ 400.
    // Để [Skip] cho tới khi Backend bổ sung ràng buộc; xem báo cáo bàn giao T26.
    [Fact(DisplayName = "U-602 · Đợt có Từ ngày > Đến ngày phải bị từ chối")]
    public void U602_PeriodWithFromAfterTo_MustBeRejected()
    {
        Should.Throw<BadRequestException>(() => sut.BindToYear(new AwardPeriod("Đợt vắt năm", 7, 11, 1, 10), 2026));
    }

    [Fact(DisplayName = "U-606/U-607 · Ngày/tháng không tồn tại phải báo lỗi nghiệp vụ")]
    public void U606_ImpossibleDayMonth_MustRaiseABusinessError()
    {
        Should.Throw<BadRequestException>(() => sut.BindToYear(new AwardPeriod("Đợt 31/02", 31, 2, 5, 3), 2026));
        Should.Throw<BadRequestException>(() => sut.BindToYear(new AwardPeriod("Đợt 31/04", 31, 4, 5, 5), 2026));
    }

    private static IEnumerable<IReadOnlyList<AwardPeriod>> AllPeriodSets()
    {
        yield return QcFixtures.MainPeriods.Select(x => x.Period).ToList();
        yield return QcFixtures.LeapEdgePeriods.Select(x => x.Period).ToList();
        yield return QcFixtures.OverlapPeriods.Select(x => x.Period).ToList();
        yield return QcFixtures.FullCoverPeriods.Select(x => x.Period).ToList();
    }
}
