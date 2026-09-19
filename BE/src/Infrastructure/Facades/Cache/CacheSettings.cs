using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using System.ComponentModel.DataAnnotations;

namespace HuyHieuDang.Infrastructure.Facades.Cache
{
    public class CacheSettings : IValidatableObject
    {
        public PermissionCacheSettings Permission { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
           => validationContext.Required();
    }

    public record PermissionCacheSettings : IValidatableObject
    {
        public double ExpiredInMinute { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            => validationContext.Required();
    }
}