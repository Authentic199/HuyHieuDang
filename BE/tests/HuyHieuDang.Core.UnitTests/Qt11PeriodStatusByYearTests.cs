using HuyHieuDang.Core.PartyBadges;

namespace HuyHieuDang.Core.UnitTests;

/// <summary>
/// QT11 cho một năm bất kỳ: bảng đợt của màn Đợt trao huy hiệu xem được năm khác năm hiện tại,
/// nhưng trạng thái vẫn so với hôm nay (mục 5.1 hợp đồng API).
/// </summary>
public class Qt11PeriodStatusByYearTests
{
    private static readonly DateOnly Today = new(2026, 9, 19);

    private readonly IPartyMilestoneCalculator calculator = new PartyMilestoneCalculator();

    [Theory(DisplayName = "QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026")]
    [InlineData(15, 1, 5, 3, PeriodStatus.Past, null)]
    [InlineData(15, 8, 10, 9, PeriodStatus.Past, null)]
    [InlineData(1, 9, 30, 9, PeriodStatus.Ongoing, null)]
    [InlineData(19, 9, 19, 9, PeriodStatus.Ongoing, null)]
    [InlineData(1, 10, 7, 11, PeriodStatus.Upcoming, 12)]
    public void GetPeriodStatus_ForCurrentYear_MatchesRule(
        int fromDay, int fromMonth, int toDay, int toMonth, PeriodStatus expected, int? daysLeft)
    {
        AwardPeriod period = new("Đợt xét", fromDay, fromMonth, toDay, toMonth);

        PeriodStatusResult result = calculator.GetPeriodStatus(period, Today.Year, Today);

        Assert.Equal(expected, result.Status);
        Assert.Equal(daysLeft, result.DaysLeft);
    }

    [Fact(DisplayName = "QT11 · Năm đã qua thì mọi đợt là Đã qua, năm sau thì là Sắp tới")]
    public void GetPeriodStatus_ForOtherYears_ComparesWithToday()
    {
        AwardPeriod period = new("Đợt 3/2", 15, 1, 5, 3);

        Assert.Equal(PeriodStatus.Past, calculator.GetPeriodStatus(period, 2025, Today).Status);

        PeriodStatusResult nextYear = calculator.GetPeriodStatus(period, 2027, Today);
        Assert.Equal(PeriodStatus.Upcoming, nextYear.Status);
        Assert.Equal(new DateOnly(2027, 1, 15).DayNumber - Today.DayNumber, nextYear.DaysLeft);
    }

    [Fact(DisplayName = "QT11 · Quá tải không tham số năm vẫn xét đúng năm hiện tại")]
    public void GetPeriodStatus_WithoutYear_UsesYearOfToday()
    {
        AwardPeriod period = new("Đợt 7/11", 1, 10, 7, 11);

        PeriodStatusResult withoutYear = calculator.GetPeriodStatus(period, Today);
        PeriodStatusResult withYear = calculator.GetPeriodStatus(period, Today.Year, Today);

        Assert.Equal(withYear, withoutYear);
    }

    [Fact(DisplayName = "QT11 · Đợt 29/02 ở năm không nhuận xét theo 28/02")]
    public void GetPeriodStatus_LeapDayPeriod_UsesBoundDates()
    {
        AwardPeriod period = new("Đợt nhuận 29/02", 29, 2, 29, 2);

        Assert.Equal(
            new DateOnly(2027, 2, 28).DayNumber - Today.DayNumber,
            calculator.GetPeriodStatus(period, 2027, Today).DaysLeft);
    }
}
