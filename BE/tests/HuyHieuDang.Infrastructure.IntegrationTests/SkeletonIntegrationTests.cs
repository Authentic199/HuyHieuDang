using HuyHieuDang.Infrastructure.Facades.Persistence;

namespace HuyHieuDang.Infrastructure.IntegrationTests;

/// <summary>
/// Chỗ đặt các bài kiểm thử tích hợp. Hiện chỉ xác nhận cấu hình nền dùng PostgreSQL.
/// </summary>
public class SkeletonIntegrationTests
{
    [Fact]
    public void DatabaseSettings_ShouldDefaultToEmptyConnectionString()
    {
        DatabaseSettings settings = new();

        Assert.Equal(string.Empty, settings.SqlSettings.ConnectionStrings.DefaultConnection);
        Assert.False(settings.SqlSettings.UseAutoMigration);
    }
}
