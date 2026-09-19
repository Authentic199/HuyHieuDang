using HuyHieuDang.Core.Bases;
using HuyHieuDang.Core.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HuyHieuDang.Infrastructure.Facades.Persistence.Interceptors;

/// <summary>
/// Đóng dấu <see cref="IHasUpdatedAt.UpdatedAt"/> ngay trước mỗi lần lưu, lấy thời điểm từ
/// <see cref="IDateTimeProvider"/>.
/// </summary>
/// <remarks>
/// Đặt ở một chỗ duy nhất vì hai lẽ: người viết service không thể quên đóng dấu, và không
/// module nghiệp vụ nào phải đọc đồng hồ (T-FIX-1). Cột lưu kiểu <c>timestamp with time zone</c>
/// nên giá trị giờ Việt Nam và giá trị UTC cùng một mốc thời gian.
/// </remarks>
public sealed class UpdatedAtInterceptor : SaveChangesInterceptor
{
    private readonly IDateTimeProvider dateTimeProvider;

    public UpdatedAtInterceptor(IDateTimeProvider dateTimeProvider)
    {
        this.dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Stamp(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        DateTimeOffset now = dateTimeProvider.Now;

        IEnumerable<EntityEntry<IHasUpdatedAt>> touched = context.ChangeTracker
            .Entries<IHasUpdatedAt>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified);

        foreach (EntityEntry<IHasUpdatedAt> entry in touched)
        {
            entry.Entity.UpdatedAt = now;
        }
    }
}
