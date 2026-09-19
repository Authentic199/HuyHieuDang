using HuyHieuDang.Core.Common.Interfaces;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Bản thay thế cố định của <see cref="IDateTimeProvider"/> dùng trong host kiểm thử (T-FIX-3).
/// </summary>
public sealed class FixedDateTimeProvider : IDateTimeProvider
{
    private readonly DateTimeOffset now;

    public FixedDateTimeProvider(DateOnly today)
        : this(new DateTimeOffset(today.ToDateTime(new TimeOnly(9, 30)), TimeSpan.FromHours(7)))
    {
    }

    public FixedDateTimeProvider(DateTimeOffset now)
    {
        this.now = now;
    }

    /// <inheritdoc/>
    public DateTimeOffset Now => now;

    /// <inheritdoc/>
    public DateOnly Today => DateOnly.FromDateTime(now.DateTime);
}
