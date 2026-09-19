using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HuyHieuDang.Infrastructure.Facades.Auth.Jwt
{
    public class JwtSettingOptions : IValidatableObject
    {
        public JwtSettings Default { get; set; } = new();


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            => validationContext.Required();
    }

    public class JwtSettings : IValidatableObject
    {
        public string Key { get; set; } = default!;

        public string RefreshKey { get; set; } = default!;

        public double AccessTokenExpirationInMinutes { get; set; }

        public double RefreshTokenExpirationInDays { get; set; }

        public string Issuer { get; set; } = default!;

        public string IsAudience { get; set; } = default!;

        public SymmetricSecurityKey GetSecurityKey()
            => new(Encoding.UTF8.GetBytes(Key));

        public SymmetricSecurityKey GetSecurityRefreshKey()
            => new(Encoding.UTF8.GetBytes(RefreshKey));

        public DateTime GetRefreshTokenExpired() => DateTime.UtcNow.AddDays(RefreshTokenExpirationInDays);

        public DateTime GetAccessTokenExpired() => DateTime.UtcNow.AddMinutes(AccessTokenExpirationInMinutes);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            => validationContext.Required(nameof(Issuer), nameof(IsAudience));
    }
}