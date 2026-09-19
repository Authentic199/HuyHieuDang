using HuyHieuDang.Infrastructure.Facades.Identity.Base;
using System.Security.Claims;

namespace HuyHieuDang.Infrastructure.Facades.Auth;

public interface ICurrentUserInitializer
{
    void SetCurrentUser(ClaimsPrincipal user);
}

public interface ICurrentUser
{
    string? Name { get; }

    Guid GetUserId();

    T? TryGetCurrentJwtUser<T>()
        where T : IJwtUser;

    void SetJwtUser(IJwtUser user);

    string GetModelType();

    DateTimeOffset? GetExpiresAt();

    bool IsAuthenticated();
}

public sealed class CurrentUser : ICurrentUser, ICurrentUserInitializer
{
    private readonly Guid userId = Guid.Empty;

    private ClaimsPrincipal? user;

    private IJwtUser? currentUser;

    public string? Name => user?.Identity?.Name;

    public Guid GetUserId() =>
        IsAuthenticated()
            ? Guid.Parse(user?.GetUserId() ?? Guid.Empty.ToString())
            : userId;

    public bool IsAuthenticated() =>
        user?.Identity?.IsAuthenticated is true;

    public string GetModelType() =>
        IsAuthenticated()
            ? user?.GetModelType() ?? string.Empty
            : string.Empty;

    public DateTimeOffset? GetExpiresAt() =>
        IsAuthenticated() ? user?.GetExpiresAt() : null;

    public void SetCurrentUser(ClaimsPrincipal user)
    {
        this.user ??= user;
    }

    public void SetJwtUser(IJwtUser user) => currentUser = user;

    public T? TryGetCurrentJwtUser<T>()
        where T : IJwtUser
        => currentUser is T typed ? typed : default;
}