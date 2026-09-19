using HuyHieuDang.Infrastructure.Facades.Definitions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace HuyHieuDang.Infrastructure.Facades.Auth.Permissions;

internal class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(MxmPermissions.PrePermissions, StringComparison.OrdinalIgnoreCase))
        {
            AuthorizationPolicyBuilder authorizationPolicyBuilder = new();
            authorizationPolicyBuilder.AddRequirements(new PermissionRequirement(policyName));
            return Task.FromResult<AuthorizationPolicy?>(authorizationPolicyBuilder.Build());
        }

        return FallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => Task.FromResult<AuthorizationPolicy?>(null);
}