using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Services;

public interface IGpModelRoleService
{
    Task AssignRoleAsync<T>(Guid modelId, Guid roleId, CancellationToken cancellationToken = default);

    Task RemoveRoleAsync<T>(Guid modelId, Guid roleId, CancellationToken cancellationToken = default);

    Task RemoveModelRoleAsync<T>(IEnumerable<Guid> modelIds, CancellationToken cancellationToken = default);

    Task SyncRolesAsync<T>(Guid modelId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);

    Task<bool> HasRoleAsync<T>(Guid modelId, Guid roleId, CancellationToken cancellationToken = default);

    Task<bool> HasAnyRoleAsync<T>(Guid modelId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);

    Task<bool> HasAllRolesAsync<T>(Guid modelId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default);

    IQueryable<Role> GetRoles<T>(Guid modelId);
}

public class GpModelRoleService : IGpModelRoleService
{
    private readonly IRepositoryWrapper repositoryWrapper;

    public GpModelRoleService(IRepositoryWrapper repositoryWrapper)
    {
        this.repositoryWrapper = repositoryWrapper;
    }

    public async Task AssignRoleAsync<T>(Guid modelId, Guid roleId, CancellationToken cancellationToken = default)
    {
        Role? role = await repositoryWrapper.Repository<Role>().Find(x => x.Id == roleId).FirstOrDefaultAsync(cancellationToken);

        if (role == null)
        {
            throw new InvalidOperationException($"{nameof(roleId)} not found '{roleId}'.");
        }

        ModelRole modelRole = new()
        {
            ModelId = modelId,
            ModelType = typeof(T).FullName!,
            RoleId = roleId,
        };
        await repositoryWrapper.Repository<ModelRole>().AddAsync(modelRole, cancellationToken);
    }

    public async Task RemoveRoleAsync<T>(Guid modelId, Guid roleId, CancellationToken cancellationToken = default)
    {
        ModelRole? modelRole = await repositoryWrapper.Repository<ModelRole>().Find(x =>
            x.ModelId == modelId
            && x.ModelType == typeof(T).FullName!)
            .FirstOrDefaultAsync(cancellationToken);

        if (modelRole == null)
        {
            throw new InvalidOperationException($"{nameof(roleId)} not found in model '{roleId}'.");
        }

        await repositoryWrapper.Repository<ModelRole>().DeleteAsync(modelRole, cancellationToken);
    }

    public async Task RemoveModelRoleAsync<T>(IEnumerable<Guid> modelIds, CancellationToken cancellationToken = default)
    {
        ICollection<ModelRole>? modelRoles = await repositoryWrapper.Repository<ModelRole>().Find(x =>
           modelIds.Contains(x.ModelId) && x.ModelType == typeof(T).FullName!)
           .ToArrayAsync(cancellationToken);

        await repositoryWrapper.Repository<ModelRole>().DeleteRangeAsync(modelRoles, cancellationToken);
    }

    public async Task SyncRolesAsync<T>(Guid modelId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        IQueryable<Role> roles = repositoryWrapper.Repository<Role>().Find(x =>
            roleIds.Any(y => y == x.Id));

        if (roles.Count() != roleIds.Count())
        {
            throw new InvalidOperationException($"{nameof(roleIds)} invalid or does not match the database.");
        }

        string modelType = typeof(T).FullName!;

        IQueryable<ModelRole> oldModelRoles = repositoryWrapper.Repository<ModelRole>().Find(x =>
            x.ModelId == modelId
            && x.ModelType == modelType);
        if (oldModelRoles.Any())
        {
            await repositoryWrapper.Repository<ModelRole>().DeleteRangeAsync(oldModelRoles, cancellationToken);
        }

        IEnumerable<ModelRole> newModelRoles = roles.Select(x => new ModelRole()
        {
            ModelId = modelId,
            ModelType = modelType,
            RoleId = x.Id,
        }).ToList();
        await repositoryWrapper.Repository<ModelRole>().AddRangeAsync(newModelRoles, cancellationToken);
    }

    public async Task<bool> HasRoleAsync<T>(Guid modelId, Guid roleId, CancellationToken cancellationToken = default)
    {
        return await repositoryWrapper.Repository<ModelRole>().AnyAsync(
            x =>
            x.ModelId == modelId
            && x.ModelType == typeof(T).FullName!
            && x.RoleId == roleId,
            cancellationToken);
    }

    public async Task<bool> HasAnyRoleAsync<T>(Guid modelId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        return await repositoryWrapper.Repository<ModelRole>().AnyAsync(
            x =>
            x.ModelId == modelId
            && x.ModelType == typeof(T).FullName!
            && roleIds.Any(y => y == x.RoleId),
            cancellationToken);
    }

    public async Task<bool> HasAllRolesAsync<T>(Guid modelId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        return roleIds.Distinct().Count() == await repositoryWrapper.Repository<ModelRole>().CountAsync(x =>
            x.ModelId == modelId
            && x.ModelType == typeof(T).FullName!
            && roleIds.Any(y => y == x.RoleId));
    }

    public IQueryable<Role> GetRoles<T>(Guid modelId)
    {
        return repositoryWrapper.Repository<ModelRole>().Find(x =>
            x.ModelId == modelId
            && x.ModelType == typeof(T).FullName!)
            .Include(x => x.Role)
            .Select(x => x.Role)!;
    }
}