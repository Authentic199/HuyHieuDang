using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Mục 4.3 hợp đồng API: nạp nằm trọn trong MỘT giao dịch — lỗi kỹ thuật giữa chừng thì
/// không dòng nào được thêm. Lỗi được mô phỏng bằng một <see cref="IRepositoryWrapper"/> bọc
/// ngoài bản thật, dùng chung <see cref="ApplicationDbContext"/> nên giao dịch vẫn là giao dịch thật.
/// </summary>
[Collection(ApiCollection.Name)]
public class ImportTransactionTests
{
    private readonly HuyHieuDangApiFactory factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImportTransactionTests"/> class.
    /// </summary>
    /// <param name="factory">Host kiểm thử dùng chung cho cả lớp.</param>
    public ImportTransactionTests(HuyHieuDangApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact(DisplayName = "4.3 · Hỏng ở lô thứ hai của bulk-1200.xlsx: 200 dòng đầu đã ghi vẫn bị thu hồi hết")]
    public async Task Commit_FailsMidway_RollsBackEveryRow()
    {
        await ResetAsync();

        using IServiceScope scope = factory.Services.CreateScope();
        FailingRepositoryWrapper wrapper = new(
            scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>(),
            failOnAddRangeCall: 2);

        PartyMemberImportService service = new(
            wrapper, scope.ServiceProvider.GetRequiredService<IDateTimeProvider>());

        using FileStream stream = File.OpenRead(ImportFixtures.Path("bulk-1200.xlsx"));

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CommitAsync(stream, "bulk-1200.xlsx", stream.Length));

        Assert.Equal(FailingRepositoryWrapper.FailureMessage, exception.Message);
        Assert.True(wrapper.RolledBack);
        Assert.Equal(0, await CountAsync());
    }

    [Fact(DisplayName = "4.3 · Hỏng đúng lúc chốt giao dịch: 32 dòng của core-hop-le.xlsx bị thu hồi hết")]
    public async Task Commit_FailsOnCommit_RollsBackEveryRow()
    {
        await ResetAsync();

        using IServiceScope scope = factory.Services.CreateScope();
        FailingRepositoryWrapper wrapper = new(
            scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>(),
            failOnCommit: true);

        PartyMemberImportService service = new(
            wrapper, scope.ServiceProvider.GetRequiredService<IDateTimeProvider>());

        using FileStream stream = File.OpenRead(ImportFixtures.Path("core-hop-le.xlsx"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CommitAsync(stream, "core-hop-le.xlsx", stream.Length));

        Assert.True(wrapper.RolledBack);
        Assert.Equal(0, await CountAsync());
    }

    [Fact(DisplayName = "4.3 · Cùng đường đi đó nhưng không hỏng: bulk-1200.xlsx thêm đủ 1200 người")]
    public async Task Commit_WithoutFailure_AddsEveryRow()
    {
        await ResetAsync();

        using IServiceScope scope = factory.Services.CreateScope();
        PartyMemberImportService service = new(
            scope.ServiceProvider.GetRequiredService<IRepositoryWrapper>(),
            scope.ServiceProvider.GetRequiredService<IDateTimeProvider>());

        using FileStream stream = File.OpenRead(ImportFixtures.Path("bulk-1200.xlsx"));

        Assert.Equal(1200, (await service.CommitAsync(stream, "bulk-1200.xlsx", stream.Length)).ImportedCount);
        Assert.Equal(1200, await CountAsync());

        await ResetAsync();
    }

    private async Task ResetAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Set<PartyMember>().ExecuteDeleteAsync();
    }

    private async Task<int> CountAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.Set<PartyMember>().CountAsync();
    }

    /// <summary>
    /// Bọc ngoài <see cref="IRepositoryWrapper"/> thật để ném lỗi đúng chỗ cần mô phỏng.
    /// </summary>
    private sealed class FailingRepositoryWrapper : IRepositoryWrapper
    {
        /// <summary>
        /// Thông điệp của lỗi mô phỏng.
        /// </summary>
        public const string FailureMessage = "Lỗi kỹ thuật mô phỏng giữa chừng khi nạp";

        private readonly IRepositoryWrapper inner;
        private readonly int failOnAddRangeCall;
        private readonly bool failOnCommit;
        private int addRangeCalls;

        public FailingRepositoryWrapper(
            IRepositoryWrapper inner, int failOnAddRangeCall = 0, bool failOnCommit = false)
        {
            this.inner = inner;
            this.failOnAddRangeCall = failOnAddRangeCall;
            this.failOnCommit = failOnCommit;
        }

        /// <summary>
        /// Gets a value indicating whether service đã gọi hoàn tác.
        /// </summary>
        public bool RolledBack { get; private set; }

        /// <inheritdoc/>
        public bool CanConnect => inner.CanConnect;

        /// <inheritdoc/>
        public IRepositoryBase<T> Repository<T>()
            where T : class
            => new FailingRepository<T>(inner.Repository<T>(), this);

        /// <inheritdoc/>
        public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
            => inner.BeginTransactionAsync(cancellationToken);

        /// <inheritdoc/>
        public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
            => failOnCommit
                ? throw new InvalidOperationException(FailureMessage)
                : inner.CommitTransactionAsync(cancellationToken);

        /// <inheritdoc/>
        public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            RolledBack = true;

            return inner.RollbackTransactionAsync(cancellationToken);
        }

        /// <summary>
        /// Đếm số lần ghi hàng loạt và ném lỗi ở đúng lần đã hẹn.
        /// </summary>
        private void OnAddRange()
        {
            addRangeCalls++;

            if (failOnAddRangeCall > 0 && addRangeCalls == failOnAddRangeCall)
            {
                throw new InvalidOperationException(FailureMessage);
            }
        }

        /// <summary>
        /// Repository chỉ can thiệp vào <c>AddRangeAsync</c>, còn lại chuyển tiếp nguyên vẹn.
        /// </summary>
        /// <typeparam name="T">Kiểu thực thể.</typeparam>
        private sealed class FailingRepository<T> : IRepositoryBase<T>
            where T : class
        {
            private readonly IRepositoryBase<T> inner;
            private readonly FailingRepositoryWrapper owner;

            public FailingRepository(IRepositoryBase<T> inner, FailingRepositoryWrapper owner)
            {
                this.inner = inner;
                this.owner = owner;
            }

            /// <inheritdoc/>
            public Task<IEnumerable<T>> AddRangeAsync(
                IEnumerable<T> entities, CancellationToken cancellationToken = default)
            {
                owner.OnAddRange();

                return inner.AddRangeAsync(entities, cancellationToken);
            }

            /// <inheritdoc/>
            public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
                => inner.AddAsync(entity, cancellationToken);

            /// <inheritdoc/>
            public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
                => inner.UpdateAsync(entity, cancellationToken);

            /// <inheritdoc/>
            public Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
                => inner.UpdateRangeAsync(entities, cancellationToken);

            /// <inheritdoc/>
            public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
                => inner.DeleteAsync(entity, cancellationToken);

            /// <inheritdoc/>
            public Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
                => inner.DeleteRangeAsync(entities, cancellationToken);

            /// <inheritdoc/>
            public T? GetById<TId>(TId id)
                where TId : notnull
                => inner.GetById(id);

            /// <inheritdoc/>
            public Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
                where TId : notnull
                => inner.GetByIdAsync(id, cancellationToken);

            /// <inheritdoc/>
            public IQueryable<T> Find(
                System.Linq.Expressions.Expression<Func<T, bool>>? expression = default, bool isAsNoTracking = default)
                => inner.Find(expression, isAsNoTracking);

            /// <inheritdoc/>
            public int Count(System.Linq.Expressions.Expression<Func<T, bool>>? expression = default)
                => inner.Count(expression);

            /// <inheritdoc/>
            public Task<int> CountAsync(
                System.Linq.Expressions.Expression<Func<T, bool>>? expression = default,
                CancellationToken cancellationToken = default)
                => inner.CountAsync(expression, cancellationToken);

            /// <inheritdoc/>
            public bool Any(System.Linq.Expressions.Expression<Func<T, bool>>? expression = default)
                => inner.Any(expression);

            /// <inheritdoc/>
            public Task<bool> AnyAsync(
                System.Linq.Expressions.Expression<Func<T, bool>>? expression = default,
                CancellationToken cancellationToken = default)
                => inner.AnyAsync(expression, cancellationToken);

            /// <inheritdoc/>
            public Microsoft.EntityFrameworkCore.Metadata.IKey? FindPrimaryKey() => inner.FindPrimaryKey();

            /// <inheritdoc/>
            public Task<IEnumerable<T>> FromSqlRaw(object[] param, string sqlQuery) => inner.FromSqlRaw(param, sqlQuery);

            /// <inheritdoc/>
            public Task<int> ExecuteSqlRawAsync(object[] param, string sqlQuery)
                => inner.ExecuteSqlRawAsync(param, sqlQuery);
        }
    }
}
