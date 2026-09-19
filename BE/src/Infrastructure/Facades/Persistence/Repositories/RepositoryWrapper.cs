using System.Collections;
using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;

namespace HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;

public interface IRepositoryWrapper
{
    IRepositoryBase<T> Repository<T>()
        where T : class;

    public bool CanConnect { get; }

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

public class RepositoryWrapper : IRepositoryWrapper
{
    private readonly Hashtable repositories = new();
    private readonly ApplicationDbContext applicationDbContext;

    public RepositoryWrapper(ApplicationDbContext applicationDbContext) => this.applicationDbContext = applicationDbContext;

    public IRepositoryBase<T> Repository<T>()
        where T : class
    {
        if (!repositories.ContainsKey(typeof(T).FullName!))
        {
            repositories.Add(typeof(T).FullName!, Activator.CreateInstance(typeof(RepositoryBase<>).MakeGenericType(typeof(T)), applicationDbContext));
        }

        return (IRepositoryBase<T>)repositories[typeof(T).FullName!]!;
    }

    public bool CanConnect { get => applicationDbContext.Database.CanConnect(); }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) => await applicationDbContext.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default) => await applicationDbContext.Database.CommitTransactionAsync(cancellationToken);

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default) => await applicationDbContext.Database.RollbackTransactionAsync(cancellationToken);
}
