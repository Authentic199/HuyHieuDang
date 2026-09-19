using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure.Exceptions.HttpExceptions;
using HuyHieuDang.Infrastructure.Facades.Auth;
using HuyHieuDang.Infrastructure.Facades.Auth.Jwt;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.Base;
using HuyHieuDang.Infrastructure.Facades.Identity.JwtToken;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;
using HuyHieuDang.Infrastructure.Modules.Auth.Requests;
using HuyHieuDang.Infrastructure.Modules.Auth.Responses;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace HuyHieuDang.Infrastructure.Modules.Auth.Services;

/// <summary>
/// Đăng nhập và đọc thông tin phiên. Không quản lý người dùng, không đổi mật khẩu,
/// không refresh token — ngoài phạm vi v1.
/// </summary>
public interface IAuthService : IScopedService
{
    /// <summary>
    /// Kiểm tra tài khoản rồi cấp JWT.
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Đọc thông tin phiên của token đang gửi kèm lời gọi.
    /// </summary>
    Task<SessionResponse> GetSessionAsync(CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IAuthService"/>
public class AuthService : IAuthService
{
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IJwtTokenGenerator tokenGenerator;
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly ICurrentUser currentUser;

    public AuthService(
        IRepositoryWrapper repositoryWrapper,
        IJwtTokenGenerator tokenGenerator,
        IDateTimeProvider dateTimeProvider,
        ICurrentUser currentUser)
    {
        this.repositoryWrapper = repositoryWrapper;
        this.tokenGenerator = tokenGenerator;
        this.dateTimeProvider = dateTimeProvider;
        this.currentUser = currentUser;
    }

    /// <summary>
    /// Một thông điệp chung cho mọi lý do đăng nhập hỏng: không có tài khoản, tài khoản bị khóa,
    /// hay sai mật khẩu. Không được tách ra thành nhiều thông điệp (UC-00).
    /// </summary>
    public static string LoginFailedMessage => Messages<User>.Action(ControllerActions.Login, false);

    /// <inheritdoc/>
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        User? user = await repositoryWrapper.Repository<User>()
            .Find(x => x.Username == request.Username, isAsNoTracking: true)
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null || user.Status is OperationStatus.Lock || !VerifyPassword(request.Password, user.Password))
        {
            throw new UnAuthorizedException(LoginFailedMessage);
        }

        JwtSettings jwtSettings = tokenGenerator.GetSettingByScheme();
        DateTime expiresAtUtc = jwtSettings.GetAccessTokenExpired();

        return new LoginResponse
        {
            AccessToken = tokenGenerator.GenerateAccessToken(jwtSettings, user, expiresAtUtc),
            TokenType = TokenTypes.Bearer,
            ExpiresAt = new DateTimeOffset(expiresAtUtc, TimeSpan.Zero),
            Username = user.Username,
            DisplayName = user.Name,
            UnitName = await GetUnitNameAsync(cancellationToken),
            ServerDate = dateTimeProvider.Today,
        };
    }

    /// <inheritdoc/>
    public async Task<SessionResponse> GetSessionAsync(CancellationToken cancellationToken = default)
    {
        Guid userId = currentUser.GetUserId();

        User user = await repositoryWrapper.Repository<User>()
            .Find(x => x.Id == userId, isAsNoTracking: true)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new UnAuthorizedException(Messages<User>.NotFound());

        return new SessionResponse
        {
            Username = user.Username,
            DisplayName = user.Name,
            UnitName = await GetUnitNameAsync(cancellationToken),
            ServerDate = dateTimeProvider.Today,
            ExpiresAt = currentUser.GetExpiresAt() ?? default,
        };
    }

    /// <summary>
    /// Một chuỗi băm hỏng trong cơ sở dữ liệu là dữ liệu sai, không phải lỗi hệ thống:
    /// coi như sai mật khẩu để giữ đúng một thông điệp chung.
    /// </summary>
    private static bool VerifyPassword(string password, string? hashed)
    {
        if (string.IsNullOrEmpty(hashed))
        {
            return false;
        }

        try
        {
            return BC.Verify(password, hashed);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }

    /// <summary>
    /// Bảng cài đặt luôn có đúng một bản ghi; chưa đặt tên đơn vị thì trả <c>null</c>.
    /// </summary>
    private async Task<string?> GetUnitNameAsync(CancellationToken cancellationToken)
        => await repositoryWrapper.Repository<AppSetting>()
            .Find(isAsNoTracking: true)
            .Select(x => x.UnitName)
            .FirstOrDefaultAsync(cancellationToken);
}
