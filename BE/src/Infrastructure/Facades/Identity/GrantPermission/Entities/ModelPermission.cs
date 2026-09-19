using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;

public class ModelPermission
{
    public Guid ModelId { get; set; }

    public string ModelType { get; set; } = default!;

    public string PermissionCode { get; set; } = default!;

    public Permission? Permission { get; set; }
}

public class ModelPermissionConfigurations : IEntityTypeConfiguration<ModelPermission>
{
    public void Configure(EntityTypeBuilder<ModelPermission> entityTypeBuilder)
    {
        entityTypeBuilder.ToTable(nameof(ModelPermission).Underscore());

        entityTypeBuilder.HasKey(x => new { x.ModelId, x.ModelType, x.PermissionCode });

        entityTypeBuilder.HasOne(x => x.Permission)
            .WithMany(x => x.ModelPermissions)
            .HasForeignKey(x => x.PermissionCode)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
