using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using System.ComponentModel.DataAnnotations;

namespace HuyHieuDang.Infrastructure.Facades.Cors
{
    public class CorsSettings : IValidatableObject
    {
        public string AllowedOrigins { get; set; } = default!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            => validationContext.Required();
    }
}