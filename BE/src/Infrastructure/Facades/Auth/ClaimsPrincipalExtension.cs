using HuyHieuDang.Infrastructure.Facades.Definitions;
using System.Security.Claims;

namespace HuyHieuDang.Infrastructure.Facades.Auth;

public static class ClaimsPrincipalExtension
{
    public static string? GetModelType(this ClaimsPrincipal principal)
       => principal.FindFirstValue(JwtTokenPayload.ModelType);

    public static string? GetUserId(this ClaimsPrincipal principal)
       => principal.FindFirstValue(JwtTokenPayload.Identification);

    public static string? GetFullName(this ClaimsPrincipal principal)
   => principal.FindFirstValue(JwtTokenPayload.FullName);

    private static string? FindFirstValue(this ClaimsPrincipal principal, string claimType) =>
        principal is null
            ? throw new ArgumentNullException(nameof(principal))
            : principal.FindFirst(claimType)?.Value;
}