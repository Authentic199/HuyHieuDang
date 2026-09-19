using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;

public class ModelRole
{
    public Guid ModelId { get; set; }

    public string ModelType { get; set; } = default!;

    public Guid RoleId { get; set; }

    public Role? Role { get; set; }
}

public class ModelRoleConfigurations : IEntityTypeConfiguration<ModelRole>
{
    public void Configure(EntityTypeBuilder<ModelRole> entityTypeBuilder)
    {
        entityTypeBuilder.ToTable(nameof(ModelRole).Underscore());

        entityTypeBuilder.HasKey(x => new { x.ModelId, x.ModelType, x.RoleId });

        entityTypeBuilder.HasOne(x => x.Role)
            .WithMany(x => x.ModelRoles)
            .HasForeignKey(x => x.RoleId);
    }
}
