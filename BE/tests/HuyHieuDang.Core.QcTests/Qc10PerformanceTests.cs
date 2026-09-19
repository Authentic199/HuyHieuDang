using System.Diagnostics;
using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.QcTests.Support;

namespace HuyHieuDang.Core.QcTests;

/// <summary>Yêu cầu phi chức năng mục 7 — U-1201.</summary>
[Trait("Tang", "Logic")]
[Trait("QuyTac", "PhiChucNang")]
public sealed class Qc10PerformanceTests
{
    private readonly IPartyMilestoneCalculator sut = new PartyMilestoneCalculator();

    [Fact(DisplayName = "U-1201 · Tính đủ điều kiện 1 đợt / 1 năm cho 10.000 đảng viên dưới 1 giây")]
    public void U1201_TenThousandMembersInOnePeriodTakeLessThanOneSecond()
    {
        List<DateOnly> members = BuildMembers(10_000);
        IReadOnlyList<int> milestones = sut.BuildMilestones(QcFixtures.Default);
        AwardPeriod period = QcFixtures.Period("P4");

        // Chạy nóng một lượt để không tính thời gian JIT vào phép đo.
        _ = members.Count(x => sut.GetEligibleMilestone(x, period, 2026, milestones) is not null);

        Stopwatch stopwatch = Stopwatch.StartNew();
        int eligible = members.Count(x => sut.GetEligibleMilestone(x, period, 2026, milestones) is not null);
        stopwatch.Stop();

        eligible.ShouldBeGreaterThan(0);
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(1000);
    }

    [Fact(DisplayName = "QC · Quét 'chưa thuộc đợt nào' cho 10.000 đảng viên dưới 1 giây")]
    public void TenThousandMembersMissedScanTakesLessThanOneSecond()
    {
        List<DateOnly> members = BuildMembers(10_000);
        IReadOnlyList<int> milestones = sut.BuildMilestones(QcFixtures.Default);
        IReadOnlyList<AwardPeriod> periods = QcFixtures.MainPeriods.Select(x => x.Period).ToList();

        _ = members.Count(x => sut.GetMissedMilestone(x, periods, 2026, milestones) is not null);

        Stopwatch stopwatch = Stopwatch.StartNew();
        int missed = members.Count(x => sut.GetMissedMilestone(x, periods, 2026, milestones) is not null);
        stopwatch.Stop();

        missed.ShouldBeGreaterThan(0);
        stopwatch.ElapsedMilliseconds.ShouldBeLessThan(1000);
    }

    /// <summary>Sinh ngày vào Đảng tất định (không phụ thuộc ngày chạy, không dùng số ngẫu nhiên).</summary>
    private static List<DateOnly> BuildMembers(int count)
    {
        List<DateOnly> members = new(count);
        DateOnly start = new(1930, 1, 1);

        for (int i = 0; i < count; i++)
        {
            members.Add(start.AddDays(i * 3));
        }

        return members;
    }
}
