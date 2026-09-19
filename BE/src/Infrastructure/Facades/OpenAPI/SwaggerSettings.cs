using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using System.ComponentModel.DataAnnotations;

namespace HuyHieuDang.Infrastructure.Facades.OpenAPI;

public class SwaggerSettings : IValidatableObject
{
    public bool Enable { get; set; }

    public SwaggerDefaultInfos DefaultInfos { get; set; } = new();

    public SwaggerCredentials Credentials { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        => validationContext.Required(nameof(Enable));
}

public interface ISwaggerInfos
{
    public string Title { get; set; }

    public string Version { get; set; }

    public string Description { get; set; }

    public string DocKey { get; set; }

    public string DocName { get; set; }

    public string RoutePrefix { get; }
}

public class SwaggerDefaultInfos : ISwaggerInfos, IValidatableObject
{
    public string Title { get; set; } = default!;

    public string Version { get; set; } = default!;

    public string Description { get; set; } = default!;

    public string DocKey { get; set; } = default!;

    public string DocName { get; set; } = default!;

    public string RoutePrefix { get; set; } = default!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
       => validationContext.Required();
}

public class SwaggerCredentials : IValidatableObject
{
    public string UserName { get; set; } = default!;

    public string Password { get; set; } = default!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
      => validationContext.Required();
}