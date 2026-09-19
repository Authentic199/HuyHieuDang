using HuyHieuDang.Infrastructure.Facades.Auth.Jwt;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using System.ComponentModel.DataAnnotations;

namespace HuyHieuDang.Infrastructure.Facades.Auth
{
    public class SecuritySettings : IValidatableObject
    {
        public JwtSettingOptions JwtSettingOptions { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            => validationContext.Required();
    }
}