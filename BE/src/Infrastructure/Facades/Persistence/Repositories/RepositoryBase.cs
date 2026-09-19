using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;

public interface IRepositoryBase<T>
    where T : class
{
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    T? GetById<TId>(TId id)
        where TId : notnull;

    Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : notnull;

    IQueryable<T> Find(Expression<Func<T, bool>>? expression = default, bool isAsNoTracking = default);

    int Count(Expression<Func<T, bool>>? expression = default);

    Task<int> CountAsync(Expression<Func<T, bool>>? expression = default, CancellationToken cancellationToken = default);

    bool Any(Expression<Func<T, bool>>? expression = default);

    Task<bool> AnyAsync(Expression<Func<T, bool>>? expression = default, CancellationToken cancellationToken = default);

    IKey? FindPrimaryKey();

    Task<IEnumerable<T>> FromSqlRaw(object[] param, string sqlQuery);

    Task<int> ExecuteSqlRawAsync(object[] param, string sqlQuery);
}

public class RepositoryBase<T> : IRepositoryBase<T>
    where T : class
{
    private readonly DbContext dbContext;

    public RepositoryBase(DbContext dbContext) => this.dbContext = dbContext;

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<T>().Add(entity);

        await dbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        dbContext.Set<T>().AddRange(entities);

        await dbContext.SaveChangesAsync(cancellationToken);

        return entities;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<T>().Update(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        dbContext.Set<T>().UpdateRange(entities);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        dbContext.Set<T>().Remove(entity);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        dbContext.Set<T>().RemoveRange(entities);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual T? GetById<TId>(TId id)
        where TId : notnull
    {
        return dbContext.Set<T>().Find(id);
    }

    public virtual async Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : notnull
    {
        return await dbContext.Set<T>().FindAsync(new object?[] { id }, cancellationToken: cancellationToken);
    }

    public virtual IQueryable<T> Find(Expression<Func<T, bool>>? expression = default, bool isAsNoTracking = default)
    {
        if (expression == null)
        {
            return isAsNoTracking ? dbContext.Set<T>().AsNoTracking() : dbContext.Set<T>();
        }

        return isAsNoTracking ? dbContext.Set<T>().AsNoTracking().Where(expression) : dbContext.Set<T>().Where(expression);
    }

    public virtual int Count(Expression<Func<T, bool>>? expression = default)
    {
        return expression == null ? dbContext.Set<T>().Count() : dbContext.Set<T>().Count(expression);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? expression = default, CancellationToken cancellationToken = default)
    {
        return expression == null ? await dbContext.Set<T>().CountAsync(cancellationToken) : await dbContext.Set<T>().CountAsync(expression, cancellationToken);
    }

    public virtual bool Any(Expression<Func<T, bool>>? expression = default)
    {
        return expression == null ? dbContext.Set<T>().Any() : dbContext.Set<T>().Any(expression);
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>>? expression = default, CancellationToken cancellationToken = default)
    {
        return expression == null ? await dbContext.Set<T>().AnyAsync(cancellationToken) : await dbContext.Set<T>().AnyAsync(expression, cancellationToken);
    }

    public IKey? FindPrimaryKey()
    {
        return dbContext.Set<T>().EntityType.FindPrimaryKey();
    }

    public async Task<IEnumerable<T>> FromSqlRaw(object[] param, string sqlQuery) =>
        await dbContext.Set<T>().FromSqlRaw(sqlQuery, param).ToListAsync();

    public async Task<int> ExecuteSqlRawAsync(object[] param, string sqlQuery) =>
        await dbContext.Database.ExecuteSqlRawAsync(sqlQuery, param);
}
