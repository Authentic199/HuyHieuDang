using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Auth.Jwt;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.Base;
using HuyHieuDang.Infrastructure.Modules.Auth.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Requests.Authentications;
using HuyHieuDang.Infrastructure.Modules.Users.Responses.Authentications;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HuyHieuDang.Infrastructure.Modules.Users.Services
{
    public partial interface IUserService
    {
        Task<AuthUserResponse> AuthenticateAsync(AuthUserRequest request, CancellationToken cancellationToken = default);

        Task<AuthUserResponse> RefreshAsync(UserRefreshTokenRequest request, CancellationToken cancellationToken = default);
    }

    public partial class UserService
    {
        public async Task<AuthUserResponse> AuthenticateAsync(AuthUserRequest request, CancellationToken cancellationToken = default)
        {
            User user = await repositoryWrapper.Repository<User>()
                .Find(x => x.Username == request.Username || x.Email == request.Username)
                .FirstOrDefaultAsync(cancellationToken) ?? throw new BadRequestException(Messages<User>.NotFound());

            if (user.Status == OperationStatus.Lock)
            {
                throw new BadRequestException(Messages<User>.Blocked());
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                throw new BadRequestException(Messages<User>.Invalid(x => x.Password));
            }

            return await AuthenticateAsync(user, cancellationToken);
        }

        public async Task<AuthUserResponse> RefreshAsync(UserRefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            string sessionIdRaw = tokenGenerator.GetClaimsValue(request.RefreshToken, JwtTokenPayload.Session)
                ?? throw new BadRequestException(Messages<UserRefreshToken>.Invalid(x => x.Token));
            Guid sessionId = Guid.Parse(sessionIdRaw);

            IQueryable<UserRefreshToken> refreshTokens = repositoryWrapper.Repository<UserRefreshToken>()
                .Find(x => x.SessionId == sessionId)
                .Include(x => x.User)
                .OrderByDescending(x => x.CreatedAt);

            UserRefreshToken refreshToken = await refreshTokens.FirstOrDefaultAsync(cancellationToken) ?? throw new BadRequestException(Messages<UserRefreshToken>.Invalid(x => x.Token));

            if (refreshToken.Token != request.RefreshToken)
            {
                await repositoryWrapper.Repository<UserRefreshToken>().DeleteRangeAsync(refreshTokens, cancellationToken);
                throw new BadRequestException(Messages<UserRefreshToken>.WasUsed());
            }

            if (refreshToken.User!.Status == OperationStatus.Lock)
            {
                throw new BadRequestException(Messages<User>.Blocked());
            }

            return await AuthenticateAsync(refreshToken.User, cancellationToken, refreshToken.SessionId);
        }

        private async Task<AuthUserResponse> AuthenticateAsync(User user, CancellationToken cancellationToken, Guid? sessionId = null)
        {
            sessionId ??= NewId.Next().ToGuid();
            JwtSettings setting = tokenGenerator.GetSettingByScheme();
            string accessToken = tokenGenerator.GenerateAccessToken(setting, user);
            string refreshToken = tokenGenerator.GenerateRefreshToken(
                setting,
                new Claim(JwtTokenPayload.Session, sessionId.Value.ToString())
            );
            await repositoryWrapper.Repository<UserRefreshToken>().AddAsync(
                new UserRefreshToken()
                {
                    Token = refreshToken,
                    SessionId = sessionId.Value,
                    UserId = user.Id,
                    ExpireTime = setting.GetRefreshTokenExpired(),
                },
                cancellationToken
            );
            return new(accessToken, refreshToken);
        }
    }
}