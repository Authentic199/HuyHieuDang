using HuyHieuDang.Core.Bases;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Enums;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Notes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;

/// <summary>
/// Đảng viên. Hệ thống cố ý không chống trùng: hai người cùng tên, cùng ngày vẫn là hai bản ghi (QT9).
/// </summary>
public class PartyMember : BaseEntity, IHasUpdatedAt
{
    /// <summary>
    /// Họ và tên đầy đủ. Bắt buộc.
    /// </summary>
    public string FullName { get; set; } = default!;

    /// <summary>
    /// Ngày sinh. Ngày thuần, không có giờ.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Giới tính. Để trống khi không rõ.
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// Ngày vào Đảng (dự bị) — gốc để tính tuổi đảng. Bắt buộc, ngày thuần.
    /// </summary>
    public DateOnly OfficialAdmissionDate { get; set; }

    /// <summary>
    /// Ghi chú tự do của đảng viên (QT12); <see langword="null"/> khi chưa có ghi chú.
    /// Máy chủ luôn lưu bản đã cắt khoảng trắng hai đầu.
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// Ngày ghi: thời điểm <b>nội dung</b> ghi chú đổi lần cuối (QT12). Về
    /// <see langword="null"/> cùng lúc với <see cref="Note"/>.
    /// </summary>
    public DateTimeOffset? NoteUpdatedAt { get; set; }

    /// <summary>
    /// Lần sửa gần nhất.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}

/// <summary>
/// Cấu hình bảng <c>party_member</c>.
/// </summary>
public class PartyMemberConfiguration : IEntityTypeConfiguration<PartyMember>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PartyMember> builder)
    {
        builder
            .UnderscoreTable()
            .HasBaseEntity();

        // OQ-3: đối chiếu tiếng Việt để `Đào Văn Ân` đứng trước `Nguyễn Văn An`.
        builder.Property(x => x.FullName).UseCollation("vi-x-icu");

        builder.Property(x => x.DateOfBirth).HasColumnType("date");
        builder.Property(x => x.OfficialAdmissionDate).HasColumnType("date");

        builder.Property(x => x.Note).HasMaxLength(PartyMemberNote.MaxLength);
        builder.Property(x => x.NoteUpdatedAt).HasColumnType("timestamptz");

        builder.HasIndex(x => x.OfficialAdmissionDate);
    }
}
