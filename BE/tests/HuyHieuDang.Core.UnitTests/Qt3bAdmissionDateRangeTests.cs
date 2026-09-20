using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.UnitTests.Fixtures;

namespace HuyHieuDang.Core.UnitTests;

/// <summary>
/// T51 — phép quy đổi "Mốc kế tiếp" và "Tuổi đảng" thành khoảng Ngày chính thức, để điều kiện
/// lọc nằm được trong SQL thay vì lọc sau khi nạp.
/// </summary>
public sealed class Qt3bAdmissionDateRangeTests
{
    private readonly PartyMilestoneCalculator sut = new();

    private IReadOnlyList<int> Default => sut.BuildMilestones(FixtureData.DefaultSettings);

    private IReadOnlyList<int> StepTen => sut.BuildMilestones(FixtureData.StepTenSettings);

    [Fact]
    public void GetLatestAdmissionDateForAge_SubtractsWholeYears()
    {
        // Act
        DateOnly latest = sut.GetLatestAdmissionDateForAge(new DateOnly(2026, 9, 19), 30);

        // Assert — vào Đảng đúng 19/09/1996 thì hôm nay tròn 30 năm, vẫn được tính là đủ 30.
        latest.ShouldBe(new DateOnly(1996, 9, 19));
    }

    [Fact]
    public void GetLatestAdmissionDateForAge_WhenAgeIsZero_ReturnsToday()
    {
        // Act & Assert
        sut.GetLatestAdmissionDateForAge(FixtureData.T0, 0).ShouldBe(FixtureData.T0);
    }

    [Fact]
    public void GetLatestAdmissionDateForAge_WhenTodayIs28February_IncludesTheLeapDay()
    {
        // Act — 29/02/1996 tròn 30 năm vào 28/02/2026 (QT2 lùi về 28/02), nên phải nằm trong khoảng.
        DateOnly latest = sut.GetLatestAdmissionDateForAge(new DateOnly(2026, 2, 28), 30);

        // Assert
        latest.ShouldBe(new DateOnly(1996, 2, 29));
        sut.GetPartyAge(latest, new DateOnly(2026, 2, 28)).ShouldBe(30);
    }

    [Fact]
    public void GetLatestAdmissionDateForAge_WhenTodayIs28FebruaryOfALeapYear_ExcludesTheLeapDay()
    {
        // Act — 29/02/1996 chỉ tròn 32 năm vào 29/02/2028, chưa phải 28/02/2028.
        DateOnly latest = sut.GetLatestAdmissionDateForAge(new DateOnly(2028, 2, 28), 32);

        // Assert
        latest.ShouldBe(new DateOnly(1996, 2, 28));
        sut.GetPartyAge(new DateOnly(1996, 2, 29), new DateOnly(2028, 2, 28)).ShouldBe(31);
    }

    [Fact]
    public void GetLatestAdmissionDateForAge_WhenTodayIs29February_LandsOn28FebruaryOfANonLeapYear()
    {
        // Act
        DateOnly latest = sut.GetLatestAdmissionDateForAge(new DateOnly(2024, 2, 29), 30);

        // Assert
        latest.ShouldBe(new DateOnly(1994, 2, 28));
    }

    [Fact]
    public void GetLatestAdmissionDateForAge_WhenAgeIsNegative_Throws()
    {
        // Act & Assert
        Should.Throw<BadRequestException>(() => sut.GetLatestAdmissionDateForAge(FixtureData.T0, -1));
    }

    [Fact]
    public void GetAdmissionDateRange_ForTheFirstMilestone_HasNoUpperBound()
    {
        // Act — mốc 30 là mốc đầu tiên: mọi người tuổi đảng dưới 30 đều rơi vào đây.
        AdmissionDateRange range = sut.GetAdmissionDateRangeForNextMilestone(30, FixtureData.T0, Default);

        // Assert
        range.FromExclusive.ShouldBe(new DateOnly(1996, 9, 19));
        range.ToInclusive.ShouldBeNull();
    }

    [Fact]
    public void GetAdmissionDateRange_ForAMiddleMilestone_IsHalfOpen()
    {
        // Act — mốc kế tiếp 40 ⟺ tuổi đảng thuộc [35, 40).
        AdmissionDateRange range = sut.GetAdmissionDateRangeForNextMilestone(40, FixtureData.T0, Default);

        // Assert
        range.FromExclusive.ShouldBe(new DateOnly(1986, 9, 19));
        range.ToInclusive.ShouldBe(new DateOnly(1991, 9, 19));
    }

    [Fact]
    public void GetAdmissionDateRange_ForNone_HasNoLowerBound()
    {
        // Act — đã vượt mốc lớn nhất 90.
        AdmissionDateRange range = sut.GetAdmissionDateRangeForNextMilestone(null, FixtureData.T0, Default);

        // Assert
        range.FromExclusive.ShouldBeNull();
        range.ToInclusive.ShouldBe(new DateOnly(1936, 9, 19));
    }

    [Fact]
    public void GetAdmissionDateRange_WhenMilestoneIsNotInTheSequence_Throws()
    {
        // Act & Assert — 33 không nằm trong dãy 30/90/5.
        Should.Throw<BadRequestException>(
            () => sut.GetAdmissionDateRangeForNextMilestone(33, FixtureData.T0, Default));
    }

    [Fact]
    public void GetAdmissionDateRange_FollowsTheMilestoneSettings()
    {
        // Act — cùng mốc 40, bước 5 cho khoảng [35, 40) còn bước 10 cho khoảng [30, 40).
        AdmissionDateRange byFive = sut.GetAdmissionDateRangeForNextMilestone(40, FixtureData.T0, Default);
        AdmissionDateRange byTen = sut.GetAdmissionDateRangeForNextMilestone(40, FixtureData.T0, StepTen);

        // Assert
        byFive.ToInclusive.ShouldBe(new DateOnly(1991, 9, 19));
        byTen.ToInclusive.ShouldBe(new DateOnly(1996, 9, 19));
        byTen.FromExclusive.ShouldBe(byFive.FromExclusive);
    }

    [Fact]
    public void GetAdmissionDateRange_AgreesWithGetNextMilestoneOnEveryFixtureMember()
    {
        // Arrange — lọc bằng khoảng ngày phải chọn đúng những người mà QT3a trả về cùng mốc.
        List<int?> values = Default.Select(x => (int?)x).Append(null).ToList();

        foreach (int? milestone in values)
        {
            AdmissionDateRange range = sut.GetAdmissionDateRangeForNextMilestone(milestone, FixtureData.T0, Default);

            foreach (FixtureMember member in FixtureData.CoreMembers)
            {
                // Act
                bool inRange = (range.FromExclusive is null || member.OfficialAdmissionDate > range.FromExclusive)
                    && (range.ToInclusive is null || member.OfficialAdmissionDate <= range.ToInclusive);
                bool expected =
                    sut.GetNextMilestone(member.OfficialAdmissionDate, FixtureData.T0, Default) == milestone;

                // Assert
                inRange.ShouldBe(
                    expected,
                    $"Khoảng ngày của mốc {milestone?.ToString() ?? "None"} không khớp QT3a ở {member.Code}.");
            }
        }
    }

    [Theory]
    [InlineData(2026, 2, 28)]
    [InlineData(2024, 2, 29)]
    [InlineData(2026, 12, 31)]
    [InlineData(2027, 1, 1)]
    public void GetAdmissionDateRange_AgreesWithGetNextMilestoneAroundLeapDays(int year, int month, int day)
    {
        // Arrange — quét từng ngày quanh mọi biên mốc, gồm cả 29/02, để bắt lệch biên một ngày.
        DateOnly today = new(year, month, day);
        List<int?> values = Default.Select(x => (int?)x).Append(null).ToList();
        List<DateOnly> probes = Default
            .Prepend(0)
            .SelectMany(age => Enumerable.Range(-45, 90).Select(offset => new DateOnly(today.Year - age, 1, 1)
                .AddDays(today.DayOfYear - 1 + offset)))
            .ToList();

        foreach (int? milestone in values)
        {
            AdmissionDateRange range = sut.GetAdmissionDateRangeForNextMilestone(milestone, today, Default);

            foreach (DateOnly admission in probes)
            {
                // Act
                bool inRange = (range.FromExclusive is null || admission > range.FromExclusive)
                    && (range.ToInclusive is null || admission <= range.ToInclusive);
                bool expected = sut.GetNextMilestone(admission, today, Default) == milestone;

                // Assert
                inRange.ShouldBe(
                    expected,
                    $"Mốc {milestone?.ToString() ?? "None"} lệch ở ngày chính thức {admission:dd/MM/yyyy}, hôm nay {today:dd/MM/yyyy}.");
            }
        }
    }
}
