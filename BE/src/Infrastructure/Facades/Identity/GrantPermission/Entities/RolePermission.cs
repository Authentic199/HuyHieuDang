using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;

public class RolePermission
{
    public Guid RoleId { get; set; }

    public Role? Role { get; set; }

    public string PermissionCode { get; set; } = default!;

    public Permission? Permission { get; set; }
}

public class RolePermissionConfigurations : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> entityTypeBuilder)
    {
        entityTypeBuilder.ToTable(nameof(RolePermission).Underscore());

        entityTypeBuilder.HasKey(x => new { x.RoleId, x.PermissionCode });

        entityTypeBuilder.HasOne(x => x.Role)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.RoleId);

        entityTypeBuilder.HasOne(x => x.Permission)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.PermissionCode)
            .OnDelete(DeleteBehavior.Cascade);
    }
}