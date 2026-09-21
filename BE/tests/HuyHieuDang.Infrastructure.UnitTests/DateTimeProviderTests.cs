using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure.Facades.Common.Services;

namespace HuyHieuDang.Infrastructure.UnitTests;

/// <summary>
/// T-FIX-1 và T-FIX-2: provider phải tự quy về Asia/Ho_Chi_Minh, không dựa vào
/// múi giờ mặc định của tiến trình.
/// </summary>
public class DateTimeProviderTests
{
    private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

    [Fact]
    public void Now_ShouldUseVietnamOffset()
    {
        IDateTimeProvider provider = new DateTimeProvider();

        Assert.Equal(VietnamOffset, provider.Now.Offset);
    }

    [Fact]
    public void Now_ShouldTrackUtcClock()
    {
        IDateTimeProvider provider = new DateTimeProvider();

        DateTimeOffset before = DateTimeOffset.UtcNow;
        DateTimeOffset now = provider.Now;
        DateTimeOffset after = DateTimeOffset.UtcNow;

        Assert.InRange(now.UtcDateTime, before.UtcDateTime, after.UtcDateTime);
    }

    [Fact]
    public void Today_ShouldBeTheVietnamCalendarDay()
    {
        IDateTimeProvider provider = new DateTimeProvider();

        DateOnly expected = DateOnly.FromDateTime(DateTime.UtcNow.Add(VietnamOffset));
        DateOnly today = provider.Today;

        // Chấp nhận lệch một ngày khi bài kiểm thử chạy đúng lúc nửa đêm giờ Việt Nam.
        Assert.InRange(today, expected.AddDays(-1), expected.AddDays(1));
        Assert.Equal(DateOnly.FromDateTime(provider.Now.DateTime), today);
    }

    [Fact]
    public void Today_ShouldFollowTestVariable_OutsideProduction()
    {
        IDateTimeProvider provider = new DateTimeProvider("Development", "2026-10-15");

        Assert.Equal(new DateOnly(2026, 10, 15), provider.Today);
        Assert.Equal(new DateOnly(2026, 10, 15), DateOnly.FromDateTime(provider.Now.DateTime));
        Assert.Equal(VietnamOffset, provider.Now.Offset);
    }

    [Fact]
    public void Today_ShouldIgnoreTestVariable_InProduction()
    {
        IDateTimeProvider forced = new DateTimeProvider("Production", "2026-10-15");
        IDateTimeProvider real = new DateTimeProvider();

        Assert.Equal(real.Today, forced.Today);
    }

    [Theory]
    [InlineData("khong-phai-ngay")]
    [InlineData("15/10/2026")]
    [InlineData("2026-13-40")]
    [InlineData("2026-10-15T00:00:00")]
    [InlineData("   ")]
    public void Today_ShouldIgnoreMalformedTestVariable(string value)
    {
        IDateTimeProvider forced = new DateTimeProvider("Development", value);
        IDateTimeProvider real = new DateTimeProvider();

        Assert.Equal(real.Today, forced.Today);
    }

    [Fact]
    public void TestTodayVariable_ShouldKeepTheAgreedName()
    {
        Assert.Equal("HUYHIEUDANG_TEST_TODAY", DateTimeProvider.TestTodayVariable);
    }
}
