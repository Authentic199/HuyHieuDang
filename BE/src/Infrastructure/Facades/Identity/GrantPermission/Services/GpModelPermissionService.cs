using HuyHieuDang.Infrastructure.Facades.Cache;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Services;

public interface IGpModelPermissionService
{
    Task GivePermissionAsync<T>(Guid modelId, string permissionCode, CancellationToken cancellationToken = default);

    Task RevokePermissionAsync<T>(Guid modelId, string permissionCode, CancellationToken cancellationToken = default);

    Task SyncPermissionsAsync<T>(Guid modelId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken = default);

    Task<bool> HasPermissionAsync<T>(Guid modelId, string permissionCode, CancellationToken cancellationToken = default);

    Task<bool> HasAnyPermissionAsync<T>(Guid modelId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken = default);

    bool HasAnyPermissionWithCache<T>(Guid modelId, IEnumerable<string> permissionCodes);

    Task<bool> HasAllPermissionsAsync<T>(Guid modelId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken = default);

    Task<bool> HasDirectPermissionAsync<T>(Guid modelId, string permissionCode, CancellationToken cancellationToken = default);

    Task<bool> HasAnyDirectPermissionAsync<T>(Guid modelId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken = default);

    Task<bool> HasAllDirectPermissionsAsync<T>(Guid modelId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken = default);

    IQueryable<Permission> GetDirectPermissions<T>(Guid modelId);

    IQueryable<Permission> GetPermissionsViaRoles<T>(Guid modelId);

    IQueryable<Permission> GetAllPermissions<T>(Guid modelId);
}

public class GpModelPermissionService : IGpModelPermissionService
{
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IMemoryCache memoryCache;
    private readonly PermissionCacheSettings cacheSettings;

    public GpModelPermissionService(IRepositoryWrapper repositoryWrapper, IMemoryCache memoryCache, IOptions<CacheSettings> cacheOptions)
    {
        this.repositoryWrapper = repositoryWrapper;
        this.memoryCache = memoryCache;
        cacheSettings = cacheOptions.Value.Permission;
    }

    public async Task GivePermissionAsync<T>(Guid modelId, string permissionCode, CancellationToken cancellationToken = default)
    {
        Permission? permission = await repositoryWrapper.Repository<Permission>()
            .Find(x => x.Code == permissionCode)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"{nameof(permissionCode)} not found '{permissionCode}'.");

        ModelPermission modelPermission = new()
        {
            ModelId = modelId,
            ModelType = typeof(T).FullName!,
            PermissionCode = permission.Code,
        };
        await repositoryWrapper.Repository<ModelPermission>().AddAsync(modelPermission, cancellationToken);
    }

    public async Task RevokePermissionAsync<T>(Guid modelId, string permissionCode, CancellationToken cancellationToken = default)
    {
        ModelPermission? modelPermission = await repositoryWrapper.Repository<ModelPermission>().Find(x =>
                x.ModelId == modelId
                && x.ModelType == typeof(T).FullName
                && x.PermissionCode == permissionCode)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"{nameof(permissionCode)} not found in model '{permissionCode}'.");

        await repositoryWrapper.Repository<ModelPermission>().DeleteAsync(modelPermission, cancellationToken);
    }

    public async Task SyncPermissionsAsync<T>(Guid modelId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken = default)
    {
        IQueryable<Permission> permissions = repositoryWrapper.Repository<Permission>().Find(x =>
            permissionCodes.Any(y => y == x.Code));

        if (permissions.Count() != permissionCodes.Count())
        {
            throw new InvalidOperationException($"{nameof(permissionCodes)} invalid or does not match the database.");
        }

        string modelType = typeof(T).FullName!;

        IQueryable<ModelPermission> oldModelPermissions = repositoryWrapper.Repository<ModelPermission>().Find(x =>
            x.ModelId == modelId
            && x.ModelType == modelType);
        if (oldModelPermissions.Any())
        {
            await repositoryWrapper.Repository<ModelPermission>().DeleteRangeAsync(oldModelPermissions, cancellationToken);
        }

        IEnumerable<ModelPermission> newModelPermissions = permissions.Select(x => new ModelPermission()
        {
            ModelId = modelId,
            ModelType = modelType,
            PermissionCode = x.Code,
        }).ToList();
        await repositoryWrapper.Repository<ModelPermission>().AddRangeAsync(newModelPermissions, cancellationToken);
        memoryCache.Remove(CacheKeys.GetKeyByModel<T>(modelId));
    }

    public async Task<bool> HasPermissionAsync<T>(Guid modelId, string permissionCode, CancellationToken cancellationToken = default)
    {
        bool hasDirectPermission = await HasDirectPermissionAsync<T>(modelId, permissionCode);

        IQueryable<Guid> ownershipRoleIds = repositoryWrapper.Repository<ModelRole>().Find(x =>
            x.ModelId == modelId
            && x.ModelType == typeof(T).FullName!)
            .Select(x => x.RoleId);
        bool hasPermissionsViaRoles = await repositoryWrapper.Repository<RolePermission>().AnyAsync(
            x =>
            ownershipRoleIds.Any(y => y == x.RoleId)
            && x.PermissionCode == permissionCode,
            cancellationToken);

        return hasDirectPermission || hasPermissionsViaRoles;
    }

    public async Task<bool> HasAnyPermissionAsync<T>(Guid modelId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken = default)
    {
        bool hasAnyDirectPermission = await HasAnyDirectPermissionAsync<T>(modelId, permissionCodes, cancellationToken);

        IQueryable<Guid> ownershipRoleIds = repositoryWrapper.Repository<ModelRole>().Find(x =>
                x.ModelId == modelId
                && x.ModelType == typeof(T).FullName)
                .Select(x => x.RoleId);
        bool hasAnyPermissionsViaRoles = await repositoryWrapper.Repository<RolePermission>().AnyAsync(
            x =>
            ownershipRoleIds.Any(y => y == x.RoleId)
            && permissionCodes.Any(y => y == x.PermissionCode),
            cancellationToken);

        return hasAnyDirectPermission || hasAnyPermissionsViaRoles;
    }

    public bool HasAnyPermissionWithCache<T>(Guid modelId, IEnumerable<string> permissionCodes)
    {
        List<string> modelPermissionCodes = new();
        modelPermissionCodes.AddRange(CacheModelPermission<T>(modelId));
        IEnumerable<Guid> ownershipRoleIds = repositoryWrapper.Repository<ModelRole>().Find(x =>
                x.ModelId == modelId
                && x.ModelType == typeof(T).FullName)
                .Select(x => x.RoleId).ToArray();
        modelPermissionCodes.AddRange(CacheRolePermission(ownershipRoleIds));

        return MxmPermissions
            .GetAllPermission(modelPermissionCodes)
            .Distinct()
            .Any(code => permissionCodes.Contains(code));
    }

    public async Task<bool> HasAllPermissionsAsync<T>(Guid modelId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken = default)
    {
        IQueryable<Permission> permissions = GetAllPermissions<T>(modelId);
        return permissionCodes.Distinct().Count() == await permissions.CountAsync(x => permissionCodes.Any(y => y == x.Code), cancellationToken);
    }

    public async Task<bool> HasDirectPermissionAsync<T>(Guid modelId, string permissionCode, CancellationToken cancellationToken = default)
    {
        return await repositoryWrapper.Repository<ModelPermission>().AnyAsync(
            x =>
            x.ModelId == modelId
            && x.ModelType == typeof(T).FullName!
            && x.PermissionCode == permissionCode,
            cancellationToken);
    }

    public async Task<bool> HasAnyDirectPermissionAsync<T>(Guid modelId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken = default)
    {
        return await repositoryWrapper.Repository<ModelPermission>().AnyAsync(
            x =>
            x.ModelId == modelId
            && x.ModelType == typeof(T).FullName!
            && permissionCodes.Any(y => y == x.PermissionCode),
            cancellationToken);
    }

    public async Task<bool> HasAllDirectPermissionsAsync<T>(Guid modelId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken = default)
    {
        return permissionCodes.Distinct().Count() == await repositoryWrapper.Repository<ModelPermission>().CountAsync(
            x =>
            x.ModelId == modelId
            && x.ModelType == typeof(T).FullName!
            && permissionCodes.Any(y => y == x.PermissionCode),
            cancellationToken);
    }

    public IQueryable<Permission> GetDirectPermissions<T>(Guid modelId)
    {
        return repositoryWrapper.Repository<ModelPermission>().Find(x =>
            x.ModelId == modelId
            && x.ModelType == typeof(T).FullName!)
            .Include(x => x.Permission)
            .Select(x => x.Permission)!;
    }

    public IQueryable<Permission> GetPermissionsViaRoles<T>(Guid modelId)
    {
        IQueryable<Guid> ownershipRoleIds = repositoryWrapper.Repository<ModelRole>().Find(x =>
            x.ModelId == modelId
            && x.ModelType == typeof(T).FullName!)
            .Select(x => x.RoleId);

        return repositoryWrapper.Repository<RolePermission>().Find(x =>
            ownershipRoleIds.Any(y => y == x.RoleId))
            .Include(x => x.Permission)
            .Select(x => x.Permission)!;
    }

    public IQueryable<Permission> GetAllPermissions<T>(Guid modelId)
    {
        IQueryable<Permission> directPermissions = GetDirectPermissions<T>(modelId);

        IQueryable<Permission> permissionsViaRoles = GetPermissionsViaRoles<T>(modelId);

        return directPermissions.Concat(permissionsViaRoles)
            .GroupBy(x => x.Code)
            .Select(x => x.First());
    }

    private IEnumerable<string> CacheModelPermission<T>(Guid modelId)
    {
        IEnumerable<string> permissionCodes;
        string key = CacheKeys.GetKeyByModel<T>(modelId);
        TimeSpan expired = TimeSpan.FromMinutes(cacheSettings.ExpiredInMinute);
        permissionCodes = memoryCache.GetOrCreate(key, cacheEntry =>
        {
            cacheEntry.SlidingExpiration = expired;
            return repositoryWrapper.Repository<ModelPermission>().Find(x =>
                        x.ModelId == modelId
                        && x.ModelType == typeof(T).FullName!).Select(x => x.PermissionCode!
                    ).ToArray();
        })!;

        return permissionCodes;
    }

    private IEnumerable<string> CacheRolePermission(IEnumerable<Guid> roleIds)
    {
        List<string> permissionCodes = new();
        TimeSpan expired = TimeSpan.FromMinutes(cacheSettings.ExpiredInMinute);
        foreach (Guid roleId in roleIds)
        {
            string key = CacheKeys.GetKeyByModel<Role>(roleId);

            permissionCodes.AddRange(memoryCache.GetOrCreate(key, cacheEntry =>
            {
                cacheEntry.SlidingExpiration = expired;
                return repositoryWrapper.Repository<RolePermission>()
                    .Find(x => roleId == x.RoleId, isAsNoTracking: true)
                    .Select(x => x.PermissionCode!)
                    .ToArray();
            })!);
        }

        return permissionCodes;
    }
}