using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.UnitTests.Fixtures;

namespace HuyHieuDang.Core.UnitTests;

/// <summary>QT1 — dãy mốc huy hiệu sinh từ cài đặt Bắt đầu / Kết thúc / Bước.</summary>
public sealed class Qt1MilestoneSequenceTests
{
    private readonly PartyMilestoneCalculator sut = new();

    [Fact]
    public void BuildMilestones_WithDefaultSettings_Returns13MilestonesFrom30To90()
    {
        // Act
        IReadOnlyList<int> milestones = sut.BuildMilestones(FixtureData.DefaultSettings);

        // Assert
        milestones.ShouldBe([30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90]);
    }

    [Fact]
    public void BuildMilestones_WithStepTen_Returns7Milestones()
    {
        // Act
        IReadOnlyList<int> milestones = sut.BuildMilestones(FixtureData.StepTenSettings);

        // Assert
        milestones.ShouldBe([30, 40, 50, 60, 70, 80, 90]);
    }

    [Fact]
    public void BuildMilestones_WhenStepDoesNotLandOnEnd_StopsBeforeExceedingEnd()
    {
        // Act
        IReadOnlyList<int> milestones = sut.BuildMilestones(new MilestoneSettings(30, 90, 7));

        // Assert
        milestones.ShouldBe([30, 37, 44, 51, 58, 65, 72, 79, 86]);
    }

    [Fact]
    public void BuildMilestones_WhenStartEqualsEnd_ReturnsSingleMilestone()
    {
        // Act
        IReadOnlyList<int> milestones = sut.BuildMilestones(new MilestoneSettings(30, 30, 5));

        // Assert
        milestones.ShouldBe([30]);
    }

    [Theory]
    [InlineData(0, 90, 5)]
    [InlineData(-30, 90, 5)]
    [InlineData(30, 0, 5)]
    [InlineData(30, -90, 5)]
    public void BuildMilestones_WhenAnyBoundIsNotPositive_ThrowsBadRequest(int start, int end, int step)
    {
        // Act
        Action act = () => sut.BuildMilestones(new MilestoneSettings(start, end, step));

        // Assert
        act.ShouldThrow<BadRequestException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BuildMilestones_WhenStepIsLessThanOne_ThrowsBadRequest(int step)
    {
        // Act
        Action act = () => sut.BuildMilestones(new MilestoneSettings(30, 90, step));

        // Assert
        act.ShouldThrow<BadRequestException>();
    }

    [Fact]
    public void BuildMilestones_WithAStepLargerThanTheRange_ReturnsOnlyTheFirstMilestone()
    {
        // Act — QC-04: Bước = int.MaxValue từng làm phép cộng tràn kiểu int rồi quay vòng sang mốc âm.
        IReadOnlyList<int> milestones = sut.BuildMilestones(new MilestoneSettings(30, 90, int.MaxValue));

        // Assert
        milestones.ShouldBe([30]);
    }

    [Fact]
    public void BuildMilestones_WhenStartIsGreaterThanEnd_ThrowsBadRequest()
    {
        // Act
        Action act = () => sut.BuildMilestones(new MilestoneSettings(90, 30, 5));

        // Assert
        act.ShouldThrow<BadRequestException>();
    }
}
