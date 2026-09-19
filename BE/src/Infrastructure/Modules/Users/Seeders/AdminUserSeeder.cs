using AutoMapper;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;
using HuyHieuDang.Infrastructure.Facades.Persistence.Initialization;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using BC = BCrypt.Net.BCrypt;

namespace HuyHieuDang.Infrastructure.Modules.Users.Seeders;

public class AdminUserSeeder : IDataSeedContributor
{
    /// <summary>
    /// Tên biến môi trường chứa mật khẩu của tài khoản admin seed sẵn.
    /// </summary>
    public const string AdminPasswordVariable = "ADMIN_PASSWORD";

    /// <summary>
    /// Mật khẩu dùng khi biến môi trường không được đặt. Chỉ hợp lệ cho môi trường phát triển.
    /// </summary>
    public const string DefaultAdminPassword = "Admin@123";

    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IMapper mapper;
    private readonly IConfiguration configuration;

    public AdminUserSeeder(IRepositoryWrapper repositoryWrapper, IMapper mapper, IConfiguration configuration)
    {
        this.repositoryWrapper = repositoryWrapper;
        this.mapper = mapper;
        this.configuration = configuration;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Permission> newPermissions = await SeedPermissionAsync(cancellationToken);

        // Role inital
        Role adminRole = new()
        {
            Id = Guid.Parse(EntityBuiderConstants.AdminRoleId),
            Code = "ADMINISTRATOR",
            Name = "Administrator",
            Description = "Administrator",
            ModelType = typeof(User).FullName!,
            Guards = MxmPermissions.AdminGuard,
        };

        Role[] initRoles = new[] { adminRole };
        IReadOnlyCollection<Role> addRoles = initRoles.Where(x => !repositoryWrapper.Repository<Role>().Any(y => y.Id == x.Id)).ToArray();
        if (addRoles.Count > 0)
        {
            await repositoryWrapper.Repository<Role>().AddRangeAsync(addRoles, cancellationToken);
            Log.Information("[Seeding][Role] --> roles: {name} have been added from Db", string.Join(',', addRoles.Select(x => x.Name)));
        }

        if (newPermissions.Count > 0)
        {
            await repositoryWrapper.Repository<RolePermission>().AddRangeAsync(
                    newPermissions.Select(x =>
                        new RolePermission
                        {
                            PermissionCode = x.Code,
                            RoleId = Guid.Parse(EntityBuiderConstants.AdminRoleId),
                        }
                    ),
                    cancellationToken
                );
        }

        // User inital
        User admin = new()
        {
            Id = Guid.Parse(EntityBuiderConstants.AdminUserId),
            Username = "admin",
            Email = "admin@huyhieudang.local",
            Password = BC.HashPassword(AdminPassword),
            Name = "Administrator",
            RoleId = Guid.Parse(EntityBuiderConstants.AdminRoleId),
        };

        User[] initUsers = new[] { admin };

        IReadOnlyCollection<User> addUsers = initUsers.Where(x => !repositoryWrapper.Repository<User>().Any(y => y.Id == x.Id)).ToArray();
        if (addUsers.Count > 0)
        {
            await repositoryWrapper.Repository<User>().AddRangeAsync(
                addUsers,
                cancellationToken
            );

            Log.Information("[Seeding][User] --> users: {name} have been added from Db", string.Join(',', addUsers.Select(x => x.Name)));
        }

        if (addUsers.Contains(admin))
        {
            await repositoryWrapper.Repository<ModelRole>().AddAsync(
              new ModelRole()
              {
                  ModelId = Guid.Parse(EntityBuiderConstants.AdminUserId),
                  ModelType = typeof(User).FullName!,
                  RoleId = Guid.Parse(EntityBuiderConstants.AdminRoleId),
              },
              cancellationToken
            );
        }
    }

    private string AdminPassword
    {
        get
        {
            string? configured = configuration[AdminPasswordVariable];

            return string.IsNullOrWhiteSpace(configured) ? DefaultAdminPassword : configured;
        }
    }

    private async Task<List<Permission>> SeedPermissionAsync(CancellationToken cancellationToken)
    {
        Dictionary<string, Permission> allPermissions = MxmPermissions.All
            .Select(x => mapper.Map<Permission>(x))
            .ToDictionary(x => x.Code, x => x);
        IReadOnlyCollection<Permission> allDbPermissions = await repositoryWrapper.Repository<Permission>()
            .Find()
            .ToArrayAsync(cancellationToken);
        IReadOnlyCollection<Permission> existDbPermissions = allDbPermissions.Where(x => allPermissions.ContainsKey(x.Code)).ToArray();
        List<Permission> newPermissions = allPermissions.Values.ExceptBy(existDbPermissions.Select(x => x.Code), x => x.Code).ToList();
        if (newPermissions.Count > 0)
        {
            await repositoryWrapper.Repository<Permission>().AddRangeAsync(
                 newPermissions,
                 cancellationToken
             );
            Log.Information("[Seeding][Permission] --> {Length} permissions have been added from Db \nData: {Data}", newPermissions.Count, string.Join(',', newPermissions.Select(x => x.Code)));
        }

        IReadOnlyCollection<Permission> deletePermissions = allDbPermissions
            .ExceptBy(existDbPermissions.Select(x => x.Code), x => x.Code).ToArray();
        if (deletePermissions.Count > 0)
        {
            await repositoryWrapper.Repository<Permission>().DeleteRangeAsync(deletePermissions, cancellationToken);
            Log.Information("[Seeding][Permission] --> {Length} permission have been deleted from Db \nData: {Data}", deletePermissions.Count, string.Join(',', deletePermissions.Select(x => x.Code)));
        }

        List<Permission> updatePermissions = new();
        foreach (Permission dbPermission in existDbPermissions)
        {
            if (allPermissions.ContainsKey(dbPermission.Code) && !dbPermission.Equals(allPermissions[dbPermission.Code]))
            {
                mapper.Map(allPermissions[dbPermission.Code], dbPermission);
                updatePermissions.Add(dbPermission);
            }
        }

        if (updatePermissions.Count > 0)
        {
            await repositoryWrapper.Repository<Permission>().UpdateRangeAsync(updatePermissions, cancellationToken);
            Log.Information("[Seeding][Permission] --> {Length} permission have been updated\nData: {Data}", updatePermissions.Count, string.Join(',', updatePermissions.Select(x => x.Code)));
        }

        return newPermissions;
    }
}