using System.Text.RegularExpressions;
using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.QcTests.Support;

namespace HuyHieuDang.Core.QcTests;

/// <summary>QT5 — không lưu kết quả: tính lại mỗi lần xem, không có tác dụng phụ.</summary>
[Trait("Tang", "Logic")]
[Trait("QuyTac", "QT5")]
public sealed class Qc05Qt5NoStoredResultTests
{
    private readonly IPartyMilestoneCalculator sut = new PartyMilestoneCalculator();

    [Fact(DisplayName = "U-501 · Gọi hai lần với cùng dữ liệu cho cùng kết quả, không tác dụng phụ")]
    public void U501_CallingTwiceGivesTheSameResult()
    {
        List<AwardPeriod> periods = QcFixtures.MainPeriods.Select(x => x.Period).ToList();
        List<int> milestones = sut.BuildMilestones(QcFixtures.Default).ToList();
        DateOnly admission = QcFixtures.Member("B01").OfficialAdmissionDate;

        int? first = sut.GetEligibleMilestone(admission, periods[3], 2026, milestones);
        IReadOnlyList<DateGap> gapsFirst = sut.GetGaps(periods, 2026);
        UpcomingPeriod? upcomingFirst = sut.GetUpcomingPeriod(periods, QcFixtures.T0);

        int? second = sut.GetEligibleMilestone(admission, periods[3], 2026, milestones);
        IReadOnlyList<DateGap> gapsSecond = sut.GetGaps(periods, 2026);
        UpcomingPeriod? upcomingSecond = sut.GetUpcomingPeriod(periods, QcFixtures.T0);

        second.ShouldBe(first);
        gapsSecond.ShouldBe(gapsFirst);
        upcomingSecond.ShouldBe(upcomingFirst);
    }

    [Fact(DisplayName = "U-501 · Service không sửa danh sách đợt và danh sách mốc được truyền vào")]
    public void U501_TheInputCollectionsAreNotMutated()
    {
        List<AwardPeriod> periods = QcFixtures.MainPeriods.Select(x => x.Period).ToList();
        List<AwardPeriod> periodsSnapshot = periods.ToList();
        List<int> milestones = sut.BuildMilestones(QcFixtures.Default).ToList();
        List<int> milestonesSnapshot = milestones.ToList();

        sut.GetGaps(periods, 2026);
        sut.GetOverlaps(periods, 2026);
        sut.GetUpcomingPeriod(periods, QcFixtures.T0);
        sut.GetMissedMilestone(QcFixtures.Member("B07").OfficialAdmissionDate, periods, 2026, milestones);
        sut.GetEligibleMilestone(QcFixtures.Member("B01").OfficialAdmissionDate, periods[0], 2026, milestones);

        periods.ShouldBe(periodsSnapshot);
        milestones.ShouldBe(milestonesSnapshot);
    }

    [Fact(DisplayName = "U-502 · Đổi cài đặt giữa hai lần gọi thì lần thứ hai đổi theo ngay")]
    public void U502_ChangingTheSettingsBetweenTwoCallsChangesTheResultImmediately()
    {
        DateOnly admission = QcFixtures.Member("S04").OfficialAdmissionDate;
        AwardPeriod period = new("Đợt bao trọn tháng 7", 1, 7, 31, 7);

        // Bước 5 → S04 tròn mốc 35 vào 15/07/2026, nằm trong đợt.
        sut.GetEligibleMilestone(admission, period, 2026, sut.BuildMilestones(QcFixtures.Default)).ShouldBe(35);

        // Bước 10 → mốc 35 biến mất khỏi dãy, cùng người cùng đợt không còn đủ điều kiện.
        sut.GetEligibleMilestone(admission, period, 2026, sut.BuildMilestones(QcFixtures.StepTen)).ShouldBeNull();
    }

    [Fact(DisplayName = "U-502 · Nới Đến ngày của đợt thì người đang bị sót chuyển sang đủ điều kiện ngay")]
    public void U502_WideningAPeriodMovesAMissedMemberIntoTheEligibleList()
    {
        DateOnly admission = QcFixtures.Member("B03").OfficialAdmissionDate;
        List<int> milestones = sut.BuildMilestones(QcFixtures.Default).ToList();
        List<AwardPeriod> before = QcFixtures.MainPeriods.Select(x => x.Period).ToList();

        sut.GetEligibleMilestone(admission, QcFixtures.Period("P3"), 2026, milestones).ShouldBeNull();
        sut.GetMissedMilestone(admission, before, 2026, milestones).ShouldNotBeNull();

        AwardPeriod widened = new("Đợt 2/9", 15, 8, 30, 9);
        List<AwardPeriod> after = before.Select(x => x.Name == "Đợt 2/9" ? widened : x).ToList();

        sut.GetEligibleMilestone(admission, widened, 2026, milestones).ShouldBe(30);
        sut.GetMissedMilestone(admission, after, 2026, milestones).ShouldBeNull();
    }

    [Fact(DisplayName = "U-503 · Không có bảng/entity nào lưu danh sách đủ điều kiện")]
    public void U503_NoEntityStoresTheEligibilityResult()
    {
        List<string> nghiNgo = new();
        Regex entityDeclaration = new(@"class\s+(\w+)\s*:\s*[^\r\n{]*BaseEntity", RegexOptions.Compiled);

        foreach (string file in Directory.EnumerateFiles(RepoPaths.BackendSource, "*.cs", SearchOption.AllDirectories))
        {
            foreach (Match match in entityDeclaration.Matches(File.ReadAllText(file)))
            {
                string name = match.Groups[1].Value;

                // "AwardPeriod" là đợt trao huy hiệu — được phép lưu. Cái bị cấm là bảng lưu
                // KẾT QUẢ xét: đủ điều kiện, đã trao, mốc đã tính.
                string[] cam = ["Eligib", "AwardRecord", "AwardResult", "MilestoneResult", "DuDieuKien"];

                if (cam.Any(x => name.Contains(x, StringComparison.OrdinalIgnoreCase)))
                {
                    nghiNgo.Add($"{name} ({Path.GetFileName(file)})");
                }
            }
        }

        nghiNgo.ShouldBeEmpty();
    }
}
