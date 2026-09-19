using System.ComponentModel.DataAnnotations;

namespace HuyHieuDang.Infrastructure.Facades.Common;

public class ApplicationInfos : IValidatableObject
{
    public string Name { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Name))
        {
            yield return new ValidationResult(
                $"{nameof(ApplicationInfos)}.{nameof(Name)} is not configured.",
                new[] { nameof(Name) });
        }
    }
}
