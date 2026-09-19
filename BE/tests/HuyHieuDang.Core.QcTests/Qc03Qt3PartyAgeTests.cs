using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.QcTests.Support;

namespace HuyHieuDang.Core.QcTests;

/// <summary>QT3 và QT3a — tuổi đảng và mốc kế tiếp.</summary>
[Trait("Tang", "Logic")]
[Trait("QuyTac", "QT3")]
public sealed class Qc03Qt3PartyAgeTests
{
    private readonly IPartyMilestoneCalculator sut = new PartyMilestoneCalculator();

    [Theory(DisplayName = "U-301/U-302/U-303 · Tuổi đảng quanh đúng ngày kỷ niệm tại T0")]
    [InlineData(1996, 9, 19, 30)]
    [InlineData(1996, 9, 20, 29)]
    [InlineData(1996, 9, 18, 30)]
    public void U301_PartyAgeAroundTheAnniversary(int year, int month, int day, int expected)
    {
        sut.GetPartyAge(new DateOnly(year, month, day), QcFixtures.T0).ShouldBe(expected);
    }

    [Theory(DisplayName = "U-307/U-308/U-309 · Người vào Đảng 29/02 xét ở năm không nhuận")]
    [InlineData(2026, 2, 27, 29)]
    [InlineData(2026, 2, 28, 30)]
    [InlineData(2026, 3, 1, 30)]
    public void U307_LeapDayAdmission_CountsFrom28FebruaryInANonLeapYear(int y, int m, int d, int expected)
    {
        sut.GetPartyAge(new DateOnly(1996, 2, 29), new DateOnly(y, m, d)).ShouldBe(expected);
    }

    [Theory(DisplayName = "U-304/U-305/U-306 · Các mốc tuổi đảng đặc biệt của bộ lõi tại T0")]
    [InlineData("M01", 91)]
    [InlineData("M02", 90)]
    [InlineData("V01", 0)]
    public void U305_SpecialMembersOfTheCoreDataset(string code, int expected)
    {
        sut.GetPartyAge(QcFixtures.Member(code).OfficialAdmissionDate, QcFixtures.T0).ShouldBe(expected);
    }

    [Fact(DisplayName = "QC · Tuổi đảng chỉ tăng theo thời gian, không bao giờ tụt (quét bộ lõi 2024–2030)")]
    public void PartyAgeIsMonotonicOverTime()
    {
        List<string> loi = new();

        foreach (QcMember member in QcFixtures.CoreMembers)
        {
            int previous = -1;
            DateOnly cursor = new(2024, 1, 1);

            while (cursor < new DateOnly(2030, 1, 1))
            {
                int age = sut.GetPartyAge(member.OfficialAdmissionDate, cursor);

                if (age < previous)
                {
                    loi.Add($"{member.Code} tut tu {previous} xuong {age} vao {cursor:dd/MM/yyyy}");
                }

                previous = age;
                cursor = cursor.AddDays(1);
            }
        }

        loi.ShouldBeEmpty();
    }

    [Fact(DisplayName = "QC · Tuổi đảng khớp oracle của QC trên mọi ngày 2024–2030 của cả bộ lõi")]
    public void PartyAgeMatchesTheQcOracleOverSixYears()
    {
        List<string> lech = new();

        foreach (QcMember member in QcFixtures.CoreMembers)
        {
            DateOnly cursor = new(2024, 1, 1);

            while (cursor < new DateOnly(2030, 1, 1))
            {
                int actual = sut.GetPartyAge(member.OfficialAdmissionDate, cursor);
                int expected = QcOracle.PartyAge(member.OfficialAdmissionDate, cursor);

                if (actual != expected)
                {
                    lech.Add($"{member.Code} tai {cursor:dd/MM/yyyy}: service {actual}, QC {expected}");
                }

                cursor = cursor.AddDays(1);
            }
        }

        lech.ShouldBeEmpty();
    }

    [Fact(DisplayName = "U-356 · Mốc kế tiếp phải LỚN HƠN tuổi đảng, không được bằng")]
    public void U356_NextMilestoneIsStrictlyGreaterThanThePartyAge()
    {
        DateOnly admission = new(1996, 9, 19);

        sut.GetPartyAge(admission, QcFixtures.T0).ShouldBe(30);
        sut.GetNextMilestone(admission, QcFixtures.T0, sut.BuildMilestones(QcFixtures.Default)).ShouldBe(35);
    }

    [Fact(DisplayName = "U-355 · L02 (29/02/1988) có mốc kế tiếp 40 rơi đúng 29/02/2028")]
    public void U355_LeapDayMemberKeeps29FebruaryForTheNextMilestone()
    {
        DateOnly admission = QcFixtures.Member("L02").OfficialAdmissionDate;
        IReadOnlyList<int> milestones = sut.BuildMilestones(QcFixtures.Default);

        sut.GetPartyAge(admission, QcFixtures.T0).ShouldBe(38);
        sut.GetNextMilestone(admission, QcFixtures.T0, milestones).ShouldBe(40);
        sut.GetNextAnniversary(admission, QcFixtures.T0, milestones).ShouldBe(new DateOnly(2028, 2, 29));
    }

    [Theory(DisplayName = "U-353/U-354/U-357 · Người đã vượt mốc lớn nhất hết mốc kế tiếp, kể cả khi đổi Bước")]
    [InlineData("M01", 5)]
    [InlineData("M01", 10)]
    [InlineData("M02", 5)]
    [InlineData("M02", 10)]
    public void U353_MembersBeyondTheHighestMilestone_HaveNoNextMilestone(string code, int step)
    {
        DateOnly admission = QcFixtures.Member(code).OfficialAdmissionDate;
        IReadOnlyList<int> milestones = sut.BuildMilestones(new MilestoneSettings(30, 90, step));

        sut.GetNextMilestone(admission, QcFixtures.T0, milestones).ShouldBeNull();
        sut.GetNextAnniversary(admission, QcFixtures.T0, milestones).ShouldBeNull();
    }

    [Fact(DisplayName = "QC · Mốc kế tiếp khớp oracle của QC trên cả bộ lõi ở T0/T1/T2, hai cài đặt")]
    public void NextMilestoneMatchesTheQcOracle()
    {
        List<string> lech = new();

        foreach (MilestoneSettings settings in new[] { QcFixtures.Default, QcFixtures.StepTen })
        {
            IReadOnlyList<int> milestones = sut.BuildMilestones(settings);

            foreach (DateOnly today in new[] { QcFixtures.T0, QcFixtures.T1, QcFixtures.T2 })
            {
                foreach (QcMember member in QcFixtures.CoreMembers)
                {
                    int? actual = sut.GetNextMilestone(member.OfficialAdmissionDate, today, milestones);
                    int? expected = QcOracle.NextMilestone(member.OfficialAdmissionDate, today, milestones);

                    if (actual != expected)
                    {
                        lech.Add($"{member.Code} buoc {settings.StepYears} {today:dd/MM/yyyy}: service {actual}, QC {expected}");
                    }
                }
            }
        }

        lech.ShouldBeEmpty();
    }
}
