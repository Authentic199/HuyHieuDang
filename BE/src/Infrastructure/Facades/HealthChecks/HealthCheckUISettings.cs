using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using System.ComponentModel.DataAnnotations;

namespace HuyHieuDang.Infrastructure.Facades.HealthChecks
{
    public class HealthCheckUISettings : IValidatableObject
    {
        public string UIPath { get; set; } = string.Empty;

        public List<HealthCheckItem> HealthChecks { get; set; } = new();

        public List<WebhookItem> CustomWebhooks { get; set; } = new();

        public int EvaluationTimeInSeconds { get; set; }

        public int MinimumSecondsBetweenFailureNotifications { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            => validationContext.Required();
    }

    public class HealthCheckItem
    {
        public string Name { get; set; } = string.Empty;

        public string Uri { get; set; } = string.Empty;
    }

    public class WebhookItem
    {
        public string Name { get; set; } = string.Empty;

        public string Uri { get; set; } = string.Empty;

        public string Payload { get; set; } = string.Empty;

        public string RestoredPayload { get; set; } = string.Empty;
    }
}
