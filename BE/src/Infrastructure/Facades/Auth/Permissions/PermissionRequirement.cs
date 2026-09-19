using Microsoft.AspNetCore.Authorization;

namespace HuyHieuDang.Infrastructure.Facades.Auth.Permissions;

internal class PermissionRequirement : IAuthorizationRequirement
{
    public string Permissions { get; }

    public PermissionRequirement(string permissions)
    {
        Permissions = permissions;
    }
}
