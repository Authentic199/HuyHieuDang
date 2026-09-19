using System.Text.Json;
using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.QcTests.Support;

namespace HuyHieuDang.Core.QcTests;

/// <summary>QT11 — trạng thái đợt trong năm hiện tại.</summary>
[Trait("Tang", "Logic")]
[Trait("QuyTac", "QT11")]
public sealed class Qc09Qt11PeriodStatusTests
{
    private readonly IPartyMilestoneCalculator sut = new PartyMilestoneCalculator();

    private IReadOnlyList<AwardPeriod> MainPeriods => QcFixtures.MainPeriods.Select(x => x.Period).ToList();

    [Fact(DisplayName = "QC · Trạng thái và số ngày còn lại khớp oracle của QC ở mọi ngày 2026–2028")]
    public void TheStatusMatchesTheQcOracleEveryDayOfThreeYears()
    {
        List<string> lech = new();
        List<AwardPeriod> periods = MainPeriods
            .Concat(QcFixtures.LeapEdgePeriods.Select(x => x.Period))
            .ToList();

        DateOnly cursor = new(2026, 1, 1);

        while (cursor < new DateOnly(2029, 1, 1))
        {
            foreach (AwardPeriod period in periods)
            {
                PeriodStatusResult actual = sut.GetPeriodStatus(period, cursor);
                (PeriodStatus Status, int? DaysLeft) expected = QcOracle.PeriodStatus(period, cursor);

                if (actual.Status != expected.Status || actual.DaysLeft != expected.DaysLeft)
                {
                    lech.Add($"{period.Name} {cursor:dd/MM/yyyy}: service {actual.Status}/{actual.DaysLeft}, QC {expected.Status}/{expected.DaysLeft}");
                }
            }

            cursor = cursor.AddDays(1);
        }

        lech.ShouldBeEmpty();
    }

    [Theory(DisplayName = "QC · Trạng thái 4 đợt chính khớp expected.json ở T0, T1, T2")]
    [InlineData("core_default_T0")]
    [InlineData("core_default_T1")]
    [InlineData("core_default_T2")]
    public void TheStatusesMatchTheQcExpectedFile(string scenario)
    {
        JsonElement node = QcFixtures.Expected("scenarios", scenario);
        DateOnly today = QcFixtures.ParseDate(node.GetProperty("today").GetString()!);

        foreach (JsonElement expected in node.GetProperty("periodStatuses").EnumerateArray())
        {
            PeriodStatusResult actual = sut.GetPeriodStatus(QcFixtures.Period(expected.GetProperty("code").GetString()!), today);

            ToVietnamese(actual.Status).ShouldBe(expected.GetProperty("status").GetString());

            int? daysLeft = expected.GetProperty("daysLeft").ValueKind == JsonValueKind.Null
                ? null
                : expected.GetProperty("daysLeft").GetInt32();

            actual.DaysLeft.ShouldBe(daysLeft);
        }
    }

    [Fact(DisplayName = "U-1102/U-1107 · Số ngày còn lại đếm đúng từng ngày trước Từ ngày của Đợt 7/11")]
    public void U1102_TheDaysLeftCountDownDayByDay()
    {
        AwardPeriod p4 = QcFixtures.Period("P4");

        sut.GetPeriodStatus(p4, QcFixtures.T0).DaysLeft.ShouldBe(12);
        sut.GetPeriodStatus(p4, new DateOnly(2026, 9, 30)).DaysLeft.ShouldBe(1);
        sut.GetPeriodStatus(p4, new DateOnly(2026, 10, 1)).DaysLeft.ShouldBeNull();
        sut.GetPeriodStatus(p4, new DateOnly(2026, 1, 1)).DaysLeft.ShouldBe(273);
    }

    [Fact(DisplayName = "QC · Chỉ trạng thái Sắp tới mới có số ngày còn lại, và luôn dương")]
    public void OnlyUpcomingCarriesAPositiveDaysLeft()
    {
        List<string> loi = new();
        DateOnly cursor = new(2026, 1, 1);

        while (cursor.Year == 2026)
        {
            foreach (AwardPeriod period in MainPeriods)
            {
                PeriodStatusResult status = sut.GetPeriodStatus(period, cursor);

                if (status.Status == PeriodStatus.Upcoming && status.DaysLeft is not > 0)
                {
                    loi.Add($"{period.Name} {cursor:dd/MM/yyyy} sap toi nhung con {status.DaysLeft} ngay");
                }

                if (status.Status != PeriodStatus.Upcoming && status.DaysLeft is not null)
                {
                    loi.Add($"{period.Name} {cursor:dd/MM/yyyy} trang thai {status.Status} nhung van co so ngay");
                }
            }

            cursor = cursor.AddDays(1);
        }

        loi.ShouldBeEmpty();
    }

    private static string ToVietnamese(PeriodStatus status) => status switch
    {
        PeriodStatus.Past => "Đã qua",
        PeriodStatus.Ongoing => "Đang diễn ra",
        _ => "Sắp tới",
    };
}
