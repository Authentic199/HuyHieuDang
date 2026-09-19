using HuyHieuDang.Core.Bases;
using HuyHieuDang.Core.Common.Interfaces;
using Humanizer;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;

public class Role : BaseEntity, ICode
{
    public string? Code { get; set; }

    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    public string ModelType { get; set; } = default!;

    public string? Guards { get; set; }

    public ICollection<ModelRole>? ModelRoles { get; set; }

    public ICollection<RolePermission>? RolePermissions { get; set; }
}

public class RoleConfigurations : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entityTypeBuilder)
    {
        entityTypeBuilder
            .ToTable(nameof(Role).Underscore())
            .HasBaseEntity()
            .HasCode();
    }
}
