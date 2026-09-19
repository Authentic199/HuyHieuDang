using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.QcTests.Support;

namespace HuyHieuDang.Core.QcTests;

/// <summary>QT2 — ngày tròn mốc. Trọng tâm: quy tắc năm nhuận Gregorian và ngày cuối tháng.</summary>
[Trait("Tang", "Logic")]
[Trait("QuyTac", "QT2")]
public sealed class Qc02Qt2AnniversaryTests
{
    private readonly IPartyMilestoneCalculator sut = new PartyMilestoneCalculator();

    [Fact(DisplayName = "U-204 · 29/02/1996 + 4 năm → 2000 chia hết 400 nên vẫn nhuận")]
    public void U204_YearDivisibleBy400_IsLeapAndKeeps29February()
    {
        sut.GetAnniversary(new DateOnly(1996, 2, 29), 4).ShouldBe(new DateOnly(2000, 2, 29));
    }

    [Fact(DisplayName = "U-205 · 29/02/1896 + 4 năm → 1900 chia hết 100 nhưng KHÔNG nhuận → 28/02")]
    public void U205_YearDivisibleBy100ButNot400_IsNotLeapAndFallsBackTo28February()
    {
        sut.GetAnniversary(new DateOnly(1896, 2, 29), 4).ShouldBe(new DateOnly(1900, 2, 28));
    }

    [Fact(DisplayName = "U-206 · 28/02/1996 + 32 năm → 28/02/2028, không được nhảy sang 29/02")]
    public void U206_The28FebruaryAdmission_NeverJumpsTo29FebruaryInALeapYear()
    {
        sut.GetAnniversary(new DateOnly(1996, 2, 28), 32).ShouldBe(new DateOnly(2028, 2, 28));
    }

    [Fact(DisplayName = "U-207 · 31/01/1996 + 30 năm → 31/01/2026, ngày cuối tháng không bị đụng")]
    public void U207_LastDayOfJanuary_StaysOnThe31st()
    {
        sut.GetAnniversary(new DateOnly(1996, 1, 31), 30).ShouldBe(new DateOnly(2026, 1, 31));
    }

    [Theory(DisplayName = "QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc")]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(10)]
    [InlineData(12)]
    public void EndOfLongMonths_KeepTheDay(int month)
    {
        sut.GetAnniversary(new DateOnly(1996, month, 31), 30).ShouldBe(new DateOnly(2026, month, 31));
    }

    [Fact(DisplayName = "U-208 · Mốc 0 trả đúng ngày vào Đảng")]
    public void U208_MilestoneZero_ReturnsTheAdmissionDateItself()
    {
        sut.GetAnniversary(new DateOnly(1996, 2, 29), 0).ShouldBe(new DateOnly(1996, 2, 29));
    }

    [Fact(DisplayName = "QC · Đối chiếu mọi ngày của 4 năm (nhuận và không nhuận) với oracle của QC")]
    public void EveryDateOfFourYears_MatchesTheQcOracle()
    {
        List<string> lech = new();

        foreach (int year in new[] { 1995, 1996, 1900, 2000 })
        {
            DateOnly cursor = new(year, 1, 1);

            while (cursor.Year == year)
            {
                foreach (int milestone in new[] { 0, 1, 4, 30, 35, 90, 100 })
                {
                    DateOnly actual = sut.GetAnniversary(cursor, milestone);
                    DateOnly expected = QcOracle.Anniversary(cursor, milestone);

                    if (actual != expected)
                    {
                        lech.Add($"{cursor:dd/MM/yyyy} + {milestone}: service {actual:dd/MM/yyyy}, QC {expected:dd/MM/yyyy}");
                    }
                }

                cursor = cursor.AddDays(1);
            }
        }

        lech.ShouldBeEmpty();
    }
}
