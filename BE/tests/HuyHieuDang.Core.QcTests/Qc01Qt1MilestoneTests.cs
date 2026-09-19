using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Core.QcTests.Support;

namespace HuyHieuDang.Core.QcTests;

/// <summary>QT1 — dãy mốc huy hiệu. Các ca biên mà bộ test của Backend chưa chạm tới.</summary>
[Trait("Tang", "Logic")]
[Trait("QuyTac", "QT1")]
public sealed class Qc01Qt1MilestoneTests
{
    private readonly IPartyMilestoneCalculator sut = new PartyMilestoneCalculator();

    [Fact(DisplayName = "U-104 · Bước lớn hơn cả khoảng 30–90 thì chỉ còn mốc đầu")]
    public void U104_StepBiggerThanTheWholeRange_KeepsOnlyTheFirstMilestone()
    {
        IReadOnlyList<int> milestones = sut.BuildMilestones(new MilestoneSettings(30, 90, 61));

        milestones.ShouldBe([30]);
    }

    [Fact(DisplayName = "U-106 · Mốc cuối đúng bằng Kết thúc thì phải có trong dãy")]
    public void U106_WhenTheLastStepLandsExactlyOnEnd_IncludesIt()
    {
        IReadOnlyList<int> milestones = sut.BuildMilestones(new MilestoneSettings(30, 95, 5));

        milestones.ShouldBe([30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95]);
        milestones[^1].ShouldBe(95);
    }

    [Fact(DisplayName = "U-110 · 1 / 100 / 1 cho đúng 100 mốc, không treo")]
    public void U110_WithStepOneOverAHundredYears_Returns100Milestones()
    {
        IReadOnlyList<int> milestones = sut.BuildMilestones(new MilestoneSettings(1, 100, 1));

        milestones.Count.ShouldBe(100);
        milestones[0].ShouldBe(1);
        milestones[^1].ShouldBe(100);
    }

    [Fact(DisplayName = "U-101 · Dãy mốc luôn tăng dần và không trùng")]
    public void U101_TheSequenceIsStrictlyIncreasing()
    {
        IReadOnlyList<int> milestones = sut.BuildMilestones(QcFixtures.Default);

        milestones.ShouldBe(milestones.OrderBy(x => x).ToList());
        milestones.Distinct().Count().ShouldBe(milestones.Count);
    }

    [Theory(DisplayName = "U-101/U-102 · Dãy mốc khớp expected.json của QC")]
    [InlineData("core_default_T0")]
    [InlineData("core_step10_T0")]
    public void U101_TheSequenceMatchesTheQcExpectedFile(string scenario)
    {
        System.Text.Json.JsonElement node = QcFixtures.Expected("scenarios", scenario);
        System.Text.Json.JsonElement settings = node.GetProperty("settings");

        IReadOnlyList<int> milestones = sut.BuildMilestones(new MilestoneSettings(
            settings.GetProperty("start_years").GetInt32(),
            settings.GetProperty("end_years").GetInt32(),
            settings.GetProperty("step_years").GetInt32()));

        milestones.ShouldBe(node.GetProperty("milestones").EnumerateArray().Select(x => x.GetInt32()).ToList());
        milestones.Count.ShouldBe(node.GetProperty("milestoneCount").GetInt32());
    }

    [Theory(DisplayName = "QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt")]
    [InlineData(30, 90, 5)]
    [InlineData(30, 90, 10)]
    [InlineData(30, 30, 5)]
    [InlineData(30, 90, 61)]
    [InlineData(30, 92, 5)]
    [InlineData(30, 95, 5)]
    [InlineData(1, 100, 1)]
    [InlineData(1, 1, 1)]
    [InlineData(5, 100, 7)]
    public void TheSequenceMatchesTheQcOracle(int start, int end, int step)
    {
        sut.BuildMilestones(new MilestoneSettings(start, end, step))
            .ShouldBe(QcOracle.Milestones(start, end, step));
    }

    [Fact(DisplayName = "U-107/U-108/U-109 · Cài đặt sai bị từ chối bằng lỗi nghiệp vụ, không trả dãy rỗng")]
    public void U107_InvalidSettingsAreRejectedWithABusinessError()
    {
        Should.Throw<BadRequestException>(() => sut.BuildMilestones(new MilestoneSettings(0, 90, 5)));
        Should.Throw<BadRequestException>(() => sut.BuildMilestones(new MilestoneSettings(30, 90, 0)));
        Should.Throw<BadRequestException>(() => sut.BuildMilestones(new MilestoneSettings(90, 30, 5)));
    }

    [Fact(DisplayName = "QC · Gọi hai lần cùng cài đặt cho hai danh sách bằng nhau và tách rời nhau")]
    public void CallingTwiceReturnsEqualButIndependentLists()
    {
        IReadOnlyList<int> first = sut.BuildMilestones(QcFixtures.Default);
        IReadOnlyList<int> second = sut.BuildMilestones(QcFixtures.Default);

        first.ShouldBe(second);
        ReferenceEquals(first, second).ShouldBeFalse();
    }

    // LỖI QC-04 — Bước rất lớn làm phép cộng `milestone += StepYears` tràn kiểu int.
    // Tái hiện: BuildMilestones(new MilestoneSettings(30, 90, int.MaxValue)).
    // 30 + int.MaxValue quay vòng thành -2.147.483.619, vẫn <= 90 nên vòng lặp chạy tiếp.
    // Kết quả thực tế: 31 mốc, trong đó 15 mốc âm, nhỏ nhất là -2.147.483.647.
    // Kết quả mong đợi theo QT1 (giống U-104): chỉ còn mốc đầu, tức [30].
    [Fact(DisplayName = "QC-04 · Bước = int.MaxValue chỉ được cho ra mốc đầu, không được tràn số", Skip = "LỖI QC-04 chưa sửa — hiện trả 31 mốc, 15 mốc âm.")]
    public void Qc04_HugeStepMustNotOverflowIntoNegativeMilestones()
    {
        sut.BuildMilestones(new MilestoneSettings(30, 90, int.MaxValue)).ShouldBe([30]);
    }
}
