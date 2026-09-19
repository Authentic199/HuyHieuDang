using HuyHieuDang.Infrastructure.Facades.Definitions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace HuyHieuDang.Infrastructure.Facades.Auth.Permissions;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string[] schemes = default!, params string[] permissions)
    {
        Policy = string.Join(",", permissions.Select(x => MxmPermissions.PrePermissions + x));

        AuthenticationSchemes = schemes?.Length > 0 ? string.Join(",", schemes.Select(x => x)) : JwtBearerDefaults.AuthenticationScheme;
    }
}
