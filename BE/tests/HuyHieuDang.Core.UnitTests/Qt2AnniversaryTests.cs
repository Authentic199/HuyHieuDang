using HuyHieuDang.Core.PartyBadges;

namespace HuyHieuDang.Core.UnitTests;

/// <summary>QT2 — ngày tròn mốc = D + N năm; 29/02 ở năm đích không nhuận lấy 28/02.</summary>
public sealed class Qt2AnniversaryTests
{
    private readonly PartyMilestoneCalculator sut = new();

    [Fact]
    public void GetAnniversary_ForOrdinaryDate_KeepsDayAndMonth()
    {
        // Act — B01: vào Đảng 01/10/1996, tròn 30 năm.
        DateOnly anniversary = sut.GetAnniversary(new DateOnly(1996, 10, 1), 30);

        // Assert
        anniversary.ShouldBe(new DateOnly(2026, 10, 1));
    }

    [Fact]
    public void GetAnniversary_WhenAdmittedOn29FebruaryAndTargetYearIsNotLeap_FallsBackTo28February()
    {
        // Act — L01: vào Đảng 29/02/1996, tròn 30 năm vào 2026 (không nhuận).
        DateOnly anniversary = sut.GetAnniversary(new DateOnly(1996, 2, 29), 30);

        // Assert
        anniversary.ShouldBe(new DateOnly(2026, 2, 28));
    }

    [Fact]
    public void GetAnniversary_WhenAdmittedOn29FebruaryAndTargetYearIsLeap_Keeps29February()
    {
        // Act — L02: vào Đảng 29/02/1988, tròn 40 năm vào 2028 (nhuận).
        DateOnly anniversary = sut.GetAnniversary(new DateOnly(1988, 2, 29), 40);

        // Assert
        anniversary.ShouldBe(new DateOnly(2028, 2, 29));
    }

    [Fact]
    public void GetAnniversary_WithMilestoneZero_ReturnsAdmissionDate()
    {
        // Act
        DateOnly anniversary = sut.GetAnniversary(new DateOnly(2026, 9, 19), 0);

        // Assert
        anniversary.ShouldBe(new DateOnly(2026, 9, 19));
    }

    [Fact]
    public void GetAnniversary_WhenMilestoneIsNegative_ThrowsBadRequest()
    {
        // Act
        Action act = () => sut.GetAnniversary(new DateOnly(1996, 10, 1), -1);

        // Assert
        act.ShouldThrow<Core.Common.Exceptions.BadRequestException>();
    }
}
