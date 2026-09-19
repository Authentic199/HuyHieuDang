using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Services;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using Microsoft.AspNetCore.Authorization;

namespace HuyHieuDang.Infrastructure.Facades.Auth.Permissions;

internal class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IGpModelPermissionService gpModelPermissionService;
    private readonly ICurrentUser currentUser;

    public PermissionAuthorizationHandler(IGpModelPermissionService gpModelPermissionService, ICurrentUser currentUser)
    {
        this.gpModelPermissionService = gpModelPermissionService;
        this.currentUser = currentUser;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        IEnumerable<string> permissionCodes = requirement.Permissions.Split(",")
            .Select(x => x.Replace(MxmPermissions.PrePermissions, string.Empty, StringComparison.OrdinalIgnoreCase));
        if (gpModelPermissionService.HasAnyPermissionWithCache<User>(currentUser.GetUserId(), permissionCodes))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
