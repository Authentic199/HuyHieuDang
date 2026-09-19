using HuyHieuDang.Core.Bases;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Identity.Base;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;
using HuyHieuDang.Infrastructure.Modules.Auth.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json.Serialization;

namespace HuyHieuDang.Infrastructure.Modules.Users.Entities;

public class User : BaseEntity, IJwtUser, IPasswordVerify
{
    public string Username { get; set; } = default!;

    public string? Email { get; set; }

    [JsonIgnore]
    public string Password { get; set; } = default!;

    public string? Name { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTimeOffset? DayOfBirth { get; set; }

    public string? Avatar { get; set; }

    public OperationStatus Status { get; set; }

    public Guid? RoleId { get; set; }

    public Role? Role { get; set; }

    public ICollection<UserRefreshToken>? UserRefreshTokens { get; set; }
}

public class UserConfigurations : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entityTypeBuilder)
    {
        entityTypeBuilder
            .UnderscoreTable()
            .HasBaseEntity();

        entityTypeBuilder.HasCitextUnique(x => x.Username);
        entityTypeBuilder.HasCitextUnique(x => x.Email);

        entityTypeBuilder.Property(x => x.Password).IsRequired();
        entityTypeBuilder.Property(x => x.Status).IsRequired().HasDefaultValue(OperationStatus.Active);
    }
}