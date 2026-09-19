using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.QcTests.Support;

namespace HuyHieuDang.Core.QcTests;

/// <summary>QT8 — đợt sắp tới của Dashboard.</summary>
[Trait("Tang", "Logic")]
[Trait("QuyTac", "QT8")]
public sealed class Qc08Qt8UpcomingPeriodTests
{
    private readonly IPartyMilestoneCalculator sut = new PartyMilestoneCalculator();

    private IReadOnlyList<AwardPeriod> MainPeriods => QcFixtures.MainPeriods.Select(x => x.Period).ToList();

    [Fact(DisplayName = "QC · Đợt sắp tới khớp oracle của QC ở mọi ngày của 2026 và 2028")]
    public void TheUpcomingPeriodMatchesTheQcOracleEveryDayOfTwoYears()
    {
        List<string> lech = new();

        foreach (IReadOnlyList<AwardPeriod> set in AllPeriodSets())
        {
            foreach (int year in new[] { 2026, 2028 })
            {
                DateOnly cursor = new(year, 1, 1);

                while (cursor.Year == year)
                {
                    UpcomingPeriod? actual = sut.GetUpcomingPeriod(set, cursor);
                    (AwardPeriod Period, int Year)? expected = QcOracle.UpcomingPeriod(set, cursor);

                    string a = actual is null ? "không có" : $"{actual.Occurrence.Period.Name}/{actual.Occurrence.Year}";
                    string e = expected is null ? "không có" : $"{expected.Value.Period.Name}/{expected.Value.Year}";

                    if (a != e)
                    {
                        lech.Add($"{cursor:dd/MM/yyyy}: service {a}, QC {e}");
                    }

                    cursor = cursor.AddDays(1);
                }
            }
        }

        lech.ShouldBeEmpty();
    }

    [Fact(DisplayName = "U-807 · Chưa cài đợt nào thì không có đợt sắp tới")]
    public void U807_WithoutAnyPeriod_ThereIsNoUpcomingPeriod()
    {
        sut.GetUpcomingPeriod([], QcFixtures.T0).ShouldBeNull();
    }

    [Theory(DisplayName = "U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch")]
    [InlineData("2026-09-19", "Đợt 7/11", 2026)]
    [InlineData("2026-10-15", "Đợt 7/11", 2026)]
    [InlineData("2026-11-07", "Đợt 7/11", 2026)]
    [InlineData("2026-11-08", "Đợt 3/2", 2027)]
    [InlineData("2026-12-01", "Đợt 3/2", 2027)]
    [InlineData("2026-12-31", "Đợt 3/2", 2027)]
    [InlineData("2026-01-01", "Đợt 3/2", 2026)]
    public void U801_TheUpcomingPeriodAtTheFixedDates(string today, string name, int year)
    {
        UpcomingPeriod? upcoming = sut.GetUpcomingPeriod(MainPeriods, QcFixtures.ParseDate(today));

        upcoming.ShouldNotBeNull();
        upcoming.Occurrence.Period.Name.ShouldBe(name);
        upcoming.Occurrence.Year.ShouldBe(year);
    }

    [Fact(DisplayName = "U-809 · Đợt 29/02 sang năm không nhuận: thu về 28/02 và đếm ngày theo ngày đã thu")]
    public void U809_LeapPeriodInANonLeapYear_CountsDaysFromTheShiftedDate()
    {
        IReadOnlyList<AwardPeriod> leapEdge = QcFixtures.LeapEdgePeriods.Select(x => x.Period).ToList();

        UpcomingPeriod? upcoming = sut.GetUpcomingPeriod(leapEdge, new DateOnly(2027, 1, 1));

        upcoming.ShouldNotBeNull();
        upcoming.Occurrence.From.ShouldBe(new DateOnly(2027, 2, 28));
        upcoming.Status.Status.ShouldBe(PeriodStatus.Upcoming);
        upcoming.Status.DaysLeft.ShouldBe(58);
    }

    [Fact(DisplayName = "QC · Đợt sắp tới luôn có Đến ngày không nhỏ hơn hôm nay")]
    public void TheUpcomingPeriodNeverEndsBeforeToday()
    {
        List<string> loi = new();
        DateOnly cursor = new(2026, 1, 1);

        while (cursor.Year == 2026)
        {
            UpcomingPeriod? upcoming = sut.GetUpcomingPeriod(MainPeriods, cursor);

            if (upcoming is not null && upcoming.Occurrence.To < cursor)
            {
                loi.Add($"{cursor:dd/MM/yyyy} chon dot da ket thuc {upcoming.Occurrence.To:dd/MM/yyyy}");
            }

            cursor = cursor.AddDays(1);
        }

        loi.ShouldBeEmpty();
    }

    // LỖI QC-03 — Hai đợt cùng Từ ngày: GetUpcomingPeriod sắp bằng OrderBy(From) (sắp ổn định)
    // nên kết quả phụ thuộc thứ tự nạp danh sách. U-808 đòi kết quả TẤT ĐỊNH, không phụ thuộc
    // thứ tự nạp — nạp từ DB không có ORDER BY tất định sẽ làm Dashboard đổi đợt giữa hai lần F5.
    [Fact(DisplayName = "U-808 · Hai đợt cùng Từ ngày phải cho kết quả tất định")]
    public void U808_TwoPeriodsWithTheSameStartDate_MustResolveDeterministically()
    {
        AwardPeriod first = new("Đợt A cùng ngày", 1, 10, 7, 11);
        AwardPeriod second = new("Đợt B cùng ngày", 1, 10, 20, 10);

        UpcomingPeriod? forward = sut.GetUpcomingPeriod([first, second], QcFixtures.T0);
        UpcomingPeriod? backward = sut.GetUpcomingPeriod([second, first], QcFixtures.T0);

        forward.ShouldNotBeNull();
        backward.ShouldNotBeNull();
        backward.Occurrence.Period.Name.ShouldBe(forward.Occurrence.Period.Name);
    }

    private static IEnumerable<IReadOnlyList<AwardPeriod>> AllPeriodSets()
    {
        yield return QcFixtures.MainPeriods.Select(x => x.Period).ToList();
        yield return QcFixtures.LeapEdgePeriods.Select(x => x.Period).ToList();
        yield return QcFixtures.OverlapPeriods.Select(x => x.Period).ToList();
        yield return QcFixtures.FullCoverPeriods.Select(x => x.Period).ToList();
    }
}
