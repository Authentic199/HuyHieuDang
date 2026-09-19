using HuyHieuDang.Core.Bases;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities;

/// <summary>
/// Đợt trao huy hiệu. Chỉ lưu ngày/tháng, không lưu năm — một đợt lặp lại hằng năm (QT6).
/// </summary>
public class AwardPeriod : BaseEntity, IHasUpdatedAt
{
    /// <summary>
    /// Tên đợt, duy nhất không phân biệt hoa thường.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Ngày bắt đầu đợt (1–31).
    /// </summary>
    public int FromDay { get; set; }

    /// <summary>
    /// Tháng bắt đầu đợt (1–12).
    /// </summary>
    public int FromMonth { get; set; }

    /// <summary>
    /// Ngày kết thúc đợt (1–31).
    /// </summary>
    public int ToDay { get; set; }

    /// <summary>
    /// Tháng kết thúc đợt (1–12).
    /// </summary>
    public int ToMonth { get; set; }

    /// <summary>
    /// Lần sửa gần nhất.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// Cấu hình bảng <c>award_period</c>.
/// </summary>
public class AwardPeriodConfiguration : IEntityTypeConfiguration<AwardPeriod>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<AwardPeriod> builder)
    {
        builder
            .UnderscoreTable()
            .HasBaseEntity();

        builder.HasCitextUnique(x => x.Name);
    }
}
