using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using System.ComponentModel.DataAnnotations;

namespace HuyHieuDang.Infrastructure.Facades.Persistence;

public class DatabaseSettings
{
    public SqlSettings SqlSettings { get; set; } = new();
}

public class SqlSettings : IValidatableObject
{
    public bool UseAutoMigration { get; set; }

    public string DbProvider { get; set; } = string.Empty;

    public ConnectionStrings ConnectionStrings { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(DbProvider))
        {
            yield return new ValidationResult(
                $"{nameof(DatabaseSettings)}.{nameof(SqlSettings)}.{nameof(DbProvider)} is not configured.",
                new[] { nameof(DbProvider) });
        }
    }
}

public class ConnectionStrings : IValidatableObject
{
    public string DefaultConnection { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(DefaultConnection))
        {
            yield return new ValidationResult(
                $"{nameof(DatabaseSettings)}.{nameof(SqlSettings)}.{nameof(ConnectionStrings)}.{nameof(DefaultConnection)} is not configured.",
                new[] { nameof(DefaultConnection) });
        }
    }

    public void OverrideConnection()
    {
        const string applicationNameKey = "ApplicationName";
        if (DefaultConnection.Contains(applicationNameKey, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Chuoi ket noi viet tay khong phai luc nao cung co dau ; o cuoi; thieu no thi
        // khoa cuoi cung bi dinh lien voi ApplicationName va tro thanh mot gia tri khac.
        string separator = DefaultConnection.Length == 0 || DefaultConnection.TrimEnd().EndsWith(';') ? string.Empty : ";";

        DefaultConnection += $"{separator}{applicationNameKey}={Environment.MachineName};";
    }
}
