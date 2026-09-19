using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;

public record Permission(string Code, string Name, string ModelType, string Resource)
{
    public string? Guards { get; set; }

    public ICollection<ModelPermission>? ModelPermissions { get; set; }

    public ICollection<RolePermission>? RolePermissions { get; set; }
}

public class PermissionConfigurations : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> entityTypeBuilder)
    {
        entityTypeBuilder.ToTable(nameof(Permission).Underscore());

        entityTypeBuilder.HasKey(x => x.Code);
    }
}