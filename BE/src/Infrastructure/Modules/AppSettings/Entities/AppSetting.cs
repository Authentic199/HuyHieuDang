using HuyHieuDang.Core.Bases;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;

/// <summary>
/// Cài đặt toàn hệ thống. Bảng luôn có đúng một bản ghi, tạo sẵn khi seed.
/// </summary>
public class AppSetting : BaseEntity
{
    /// <summary>
    /// Mốc huy hiệu đầu tiên, mặc định 30 năm.
    /// </summary>
    public const int DefaultStartYears = 30;

    /// <summary>
    /// Mốc huy hiệu cuối cùng, mặc định 90 năm.
    /// </summary>
    public const int DefaultEndYears = 90;

    /// <summary>
    /// Bước nhảy giữa hai mốc, mặc định 5 năm.
    /// </summary>
    public const int DefaultStepYears = 5;

    /// <summary>
    /// Mốc bắt đầu của dãy mốc huy hiệu (QT1).
    /// </summary>
    public int StartYears { get; set; } = DefaultStartYears;

    /// <summary>
    /// Mốc kết thúc của dãy mốc huy hiệu (QT1).
    /// </summary>
    public int EndYears { get; set; } = DefaultEndYears;

    /// <summary>
    /// Bước nhảy giữa hai mốc liên tiếp (QT1).
    /// </summary>
    public int StepYears { get; set; } = DefaultStepYears;

    /// <summary>
    /// Tên đơn vị in trên file Excel xuất ra. Để trống nghĩa là chưa đặt.
    /// </summary>
    public string? UnitName { get; set; }

    /// <summary>
    /// Lần sửa cài đặt gần nhất.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Cấu hình bảng <c>app_setting</c>.
/// </summary>
public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder
            .UnderscoreTable()
            .HasBaseEntity();
    }
}
