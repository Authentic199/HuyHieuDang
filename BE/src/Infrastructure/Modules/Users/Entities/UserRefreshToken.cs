using HuyHieuDang.Core.Bases;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Identity.Base;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HuyHieuDang.Infrastructure.Modules.Auth.Entities
{
    public class UserRefreshToken : BaseEntity, IRefreshToken
    {
        public string? Token { get; set; }

        public DateTime ExpireTime { get; set; }

        public Guid SessionId { get; set; }

        public Guid UserId { get; set; }

        public User? User { get; set; }
    }

    public class UserRefreshTokenConfigurations : IEntityTypeConfiguration<UserRefreshToken>
    {
        public void Configure(EntityTypeBuilder<UserRefreshToken> entityTypeBuilder)
        {
            entityTypeBuilder
                .UnderscoreTable()
                .HasBaseEntity();

            entityTypeBuilder.Property(x => x.Token).IsRequired();
            entityTypeBuilder.HasIndex(x => x.Token).IsUnique();

            entityTypeBuilder.Property(x => x.ExpireTime).IsRequired();

            entityTypeBuilder.HasOne(x => x.User).WithMany(x => x.UserRefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}