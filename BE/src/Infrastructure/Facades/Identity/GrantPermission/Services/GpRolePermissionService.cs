using System.Data;
using HuyHieuDang.Infrastructure.Facades.Cache;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Services;

public interface IGpRolePermissionService
{
    IQueryable<Permission> GetAllPermissions(Guid roleId);

    Task GivePermissionAsync(Guid roleId, string permissionCode, CancellationToken cancellationToken = default);

    Task<bool> HasPermissionAsync(Guid roleId, string permissionCode, CancellationToken cancellationToken = default);

    Task RevokePermissionAsync(Guid roleId, string permissionCode, CancellationToken cancellationToken = default);

    Task SyncPermissionsAsync(Guid roleId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken = default);
}

public class GpRolePermissionService : IGpRolePermissionService
{
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IMemoryCache memoryCache;

    public GpRolePermissionService(IRepositoryWrapper repositoryWrapper, IMemoryCache memoryCache)
    {
        this.repositoryWrapper = repositoryWrapper;
        this.memoryCache = memoryCache;
    }

    public async Task GivePermissionAsync(Guid roleId, string permissionCode, CancellationToken cancellationToken = default)
    {
        Permission? permission = await repositoryWrapper.Repository<Permission>()
            .Find(x => x.Code == permissionCode)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"{nameof(permissionCode)} not found '{permissionCode}'.");

        RolePermission rolePermission = new()
        {
            RoleId = roleId,
            PermissionCode = permission.Code,
        };
        await repositoryWrapper.Repository<RolePermission>().AddAsync(rolePermission, cancellationToken);
    }

    public async Task RevokePermissionAsync(Guid roleId, string permissionCode, CancellationToken cancellationToken = default)
    {
        _ = await repositoryWrapper.Repository<Permission>()
            .Find(x => x.Code == permissionCode)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"{nameof(permissionCode)} not found '{permissionCode}'.");

        RolePermission? rolePermission = await repositoryWrapper.Repository<RolePermission>().Find(x =>
                x.RoleId == roleId
                && x.PermissionCode == permissionCode)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"{nameof(permissionCode)} not found in role '{permissionCode}'.");

        await repositoryWrapper.Repository<RolePermission>().DeleteAsync(rolePermission, cancellationToken);
    }

    public async Task SyncPermissionsAsync(Guid roleId, IEnumerable<string> permissionCodes, CancellationToken cancellationToken = default)
    {
        IQueryable<Permission> permissions = repositoryWrapper.Repository<Permission>().Find(x =>
            permissionCodes.Any(y => y == x.Code));

        if (permissions.Count() != permissionCodes.Count())
        {
            throw new InvalidOperationException($"{nameof(permissionCodes)} invalid or does not match the database.");
        }

        IQueryable<RolePermission> oldRolePermissions = repositoryWrapper.Repository<RolePermission>().Find(x =>
                x.RoleId == roleId);
        if (oldRolePermissions.Any())
        {
            await repositoryWrapper.Repository<RolePermission>().DeleteRangeAsync(oldRolePermissions, cancellationToken);
        }

        IEnumerable<RolePermission> newRolePermissions = permissions.Select(x => new RolePermission()
        {
            RoleId = roleId,
            PermissionCode = x.Code,
        });
        await repositoryWrapper.Repository<RolePermission>().AddRangeAsync(newRolePermissions, cancellationToken);
        memoryCache.Remove(CacheKeys.GetKeyByModel<Role>(roleId));
    }

    public async Task<bool> HasPermissionAsync(Guid roleId, string permissionCode, CancellationToken cancellationToken = default)
    {
        return await repositoryWrapper.Repository<RolePermission>().AnyAsync(
            x =>
            x.RoleId == roleId
            && x.PermissionCode == permissionCode,
            cancellationToken);
    }

    public IQueryable<Permission> GetAllPermissions(Guid roleId)
    {
        return repositoryWrapper.Repository<RolePermission>().Find(x =>
            x.RoleId == roleId)
            .Include(x => x.Permission)
            .Select(x => x.Permission)!;
    }
}