using HuyHieuDang.Infrastructure.Facades.Definitions;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Globalization;
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

    /// <summary>
    /// Thoi diem token het han, doc tu claim chuan <c>exp</c> (so giay Unix, theo UTC).
    /// Tra ve <c>null</c> khi token khong mang claim nay.
    /// </summary>
    public static DateTimeOffset? GetExpiresAt(this ClaimsPrincipal principal)
    {
        string? raw = principal.FindFirstValue(JwtRegisteredClaimNames.Exp);

        return long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long seconds)
            ? DateTimeOffset.FromUnixTimeSeconds(seconds)
            : null;
    }

    private static string? FindFirstValue(this ClaimsPrincipal principal, string claimType) =>
        principal is null
            ? throw new ArgumentNullException(nameof(principal))
            : principal.FindFirst(claimType)?.Value;
}