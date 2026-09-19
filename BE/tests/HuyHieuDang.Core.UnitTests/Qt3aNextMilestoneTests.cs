using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.UnitTests.Fixtures;

namespace HuyHieuDang.Core.UnitTests;

/// <summary>QT3a — mốc kế tiếp và ngày tròn mốc kế tiếp; vượt mốc lớn nhất trả null.</summary>
public sealed class Qt3aNextMilestoneTests
{
    private readonly PartyMilestoneCalculator sut = new();

    private IReadOnlyList<int> Default => sut.BuildMilestones(FixtureData.DefaultSettings);

    private IReadOnlyList<int> StepTen => sut.BuildMilestones(FixtureData.StepTenSettings);

    [Fact]
    public void GetNextMilestone_WhenPartyAgeIsBelowTheFirstMilestone_ReturnsTheFirstMilestone()
    {
        // Act — V01 tuổi đảng 0.
        int? next = sut.GetNextMilestone(FixtureData.Member("V01").OfficialAdmissionDate, FixtureData.T0, Default);

        // Assert
        next.ShouldBe(30);
    }

    [Fact]
    public void GetNextMilestone_WhenPartyAgeIsJustUnderAMilestone_ReturnsThatMilestone()
    {
        // Act — B01 tuổi đảng 29.
        int? next = sut.GetNextMilestone(FixtureData.Member("B01").OfficialAdmissionDate, FixtureData.T0, Default);

        // Assert
        next.ShouldBe(30);
    }

    [Fact]
    public void GetNextMilestone_WhenPartyAgeEqualsAMilestone_ReturnsTheFollowingMilestone()
    {
        // Act — B05 tuổi đảng đúng 30 vào 15/01/2026.
        int? next = sut.GetNextMilestone(
            FixtureData.Member("B05").OfficialAdmissionDate, new DateOnly(2026, 1, 15), Default);

        // Assert
        next.ShouldBe(35);
    }

    [Fact]
    public void GetNextMilestone_WhenPartyAgeEqualsTheHighestMilestone_ReturnsNull()
    {
        // Act — M02 tuổi đảng đúng 90.
        int? next = sut.GetNextMilestone(FixtureData.Member("M02").OfficialAdmissionDate, FixtureData.T0, Default);

        // Assert
        next.ShouldBeNull();
    }

    [Fact]
    public void GetNextMilestone_WhenPartyAgeIsAboveTheHighestMilestone_ReturnsNull()
    {
        // Act — M01 tuổi đảng 91.
        int? next = sut.GetNextMilestone(FixtureData.Member("M01").OfficialAdmissionDate, FixtureData.T0, Default);

        // Assert
        next.ShouldBeNull();
    }

    [Fact]
    public void GetNextAnniversary_WhenPartyAgeIsAboveTheHighestMilestone_ReturnsNull()
    {
        // Act
        DateOnly? anniversary = sut.GetNextAnniversary(
            FixtureData.Member("M01").OfficialAdmissionDate, FixtureData.T0, Default);

        // Assert
        anniversary.ShouldBeNull();
    }

    [Fact]
    public void GetNextAnniversary_ForAMemberBelowTheFirstMilestone_ReturnsTheMilestoneDate()
    {
        // Act — B01 tròn 30 vào 01/10/2026.
        DateOnly? anniversary = sut.GetNextAnniversary(
            FixtureData.Member("B01").OfficialAdmissionDate, FixtureData.T0, Default);

        // Assert
        anniversary.ShouldBe(new DateOnly(2026, 10, 1));
    }

    [Fact]
    public void GetNextMilestone_WithStepTen_SkipsTheMilestonesThatDisappear()
    {
        // Act — S01 tuổi đảng 34: bước 5 cho mốc 35, bước 10 cho mốc 40.
        DateOnly admission = FixtureData.Member("S01").OfficialAdmissionDate;

        // Assert
        sut.GetNextMilestone(admission, FixtureData.T0, Default).ShouldBe(35);
        sut.GetNextMilestone(admission, FixtureData.T0, StepTen).ShouldBe(40);
    }

    [Fact]
    public void GetNextMilestone_MatchesTheExpectedMemberListOfTheFixtures()
    {
        // Act & Assert — đối chiếu toàn bộ 32 đảng viên với expected.json (memberList.T0_default).
        foreach (FixtureMember member in FixtureData.CoreMembers)
        {
            int? next = sut.GetNextMilestone(member.OfficialAdmissionDate, FixtureData.T0, Default);
            int? expected = FixtureExpectations.NextMilestone(member.Code);

            next.ShouldBe(expected, $"Mốc kế tiếp của {member.Code} không khớp bộ dữ liệu của QC.");
        }
    }

    [Fact]
    public void GetPartyAge_MatchesTheExpectedMemberListOfTheFixtures()
    {
        // Act & Assert
        foreach (FixtureMember member in FixtureData.CoreMembers)
        {
            int age = sut.GetPartyAge(member.OfficialAdmissionDate, FixtureData.T0);

            age.ShouldBe(
                FixtureExpectations.PartyAge(member.Code),
                $"Tuổi đảng của {member.Code} không khớp bộ dữ liệu của QC.");
        }
    }
}
