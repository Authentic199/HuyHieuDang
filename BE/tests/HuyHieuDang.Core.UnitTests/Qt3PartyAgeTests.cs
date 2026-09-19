using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.UnitTests.Fixtures;

namespace HuyHieuDang.Core.UnitTests;

/// <summary>QT3 — tuổi đảng: số năm tròn đã qua hoặc đúng ngày kỷ niệm.</summary>
public sealed class Qt3PartyAgeTests
{
    private readonly PartyMilestoneCalculator sut = new();

    [Fact]
    public void GetPartyAge_BeforeThisYearAnniversary_DoesNotCountTheCurrentYear()
    {
        // Act — B01 vào Đảng 01/10/1996, hôm nay 19/09/2026 (chưa tới kỷ niệm 30).
        int age = sut.GetPartyAge(FixtureData.Member("B01").OfficialAdmissionDate, FixtureData.T0);

        // Assert
        age.ShouldBe(29);
    }

    [Fact]
    public void GetPartyAge_OnTheAnniversaryDay_CountsThatYear()
    {
        // Act — B05 vào Đảng 15/01/1996, hôm nay đúng 15/01/2026.
        int age = sut.GetPartyAge(FixtureData.Member("B05").OfficialAdmissionDate, new DateOnly(2026, 1, 15));

        // Assert
        age.ShouldBe(30);
    }

    [Fact]
    public void GetPartyAge_OneDayBeforeTheAnniversary_ReturnsPreviousCount()
    {
        // Act
        int age = sut.GetPartyAge(FixtureData.Member("B05").OfficialAdmissionDate, new DateOnly(2026, 1, 14));

        // Assert
        age.ShouldBe(29);
    }

    [Fact]
    public void GetPartyAge_WhenAdmittedToday_ReturnsZero()
    {
        // Act — V01 vào Đảng đúng hôm nay (T0).
        int age = sut.GetPartyAge(FixtureData.Member("V01").OfficialAdmissionDate, FixtureData.T0);

        // Assert
        age.ShouldBe(0);
    }

    [Fact]
    public void GetPartyAge_ForLeapDayAdmission_CountsOn28FebruaryOfNonLeapYear()
    {
        // Act — L01 vào Đảng 29/02/1996, hôm nay 28/02/2026.
        int age = sut.GetPartyAge(FixtureData.Member("L01").OfficialAdmissionDate, new DateOnly(2026, 2, 28));

        // Assert
        age.ShouldBe(30);
    }

    [Fact]
    public void GetPartyAge_ForLeapDayAdmission_OneDayBefore28February_DoesNotCountTheYear()
    {
        // Act
        int age = sut.GetPartyAge(FixtureData.Member("L01").OfficialAdmissionDate, new DateOnly(2026, 2, 27));

        // Assert
        age.ShouldBe(29);
    }

    [Fact]
    public void GetPartyAge_ForTheOldestMember_ReturnsAgeBeyondTheHighestMilestone()
    {
        // Act — M01 vào Đảng 01/05/1935.
        int age = sut.GetPartyAge(FixtureData.Member("M01").OfficialAdmissionDate, FixtureData.T0);

        // Assert
        age.ShouldBe(91);
    }

    [Fact]
    public void GetPartyAge_WhenAdmissionIsInTheFuture_ReturnsZero()
    {
        // Act
        int age = sut.GetPartyAge(new DateOnly(2027, 1, 1), FixtureData.T0);

        // Assert
        age.ShouldBe(0);
    }
}
