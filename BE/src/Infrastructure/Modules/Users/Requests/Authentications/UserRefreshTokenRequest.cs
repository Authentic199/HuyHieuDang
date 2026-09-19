using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.JwtToken;
using HuyHieuDang.Infrastructure.Modules.Auth.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using Microsoft.IdentityModel.Tokens;

namespace HuyHieuDang.Infrastructure.Modules.Users.Requests.Authentications;

[MessageDisplay(nameof(User))]
public class UserRefreshTokenRequest
{
    public string? RefreshToken { get; set; }
}

public class RefreshTokenRequestValidator : AbstractValidator<UserRefreshTokenRequest>
{
    private readonly IJwtTokenGenerator jwtTokenGenerator;

    public RefreshTokenRequestValidator(IJwtTokenGenerator jwtTokenGenerator)
    {
        this.jwtTokenGenerator = jwtTokenGenerator;
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(MessagesType.Required)
            .Must((_, token, context) => ValidateToken(token!, context))
            .WithMessage("{TokenError}");
    }

    private bool ValidateToken(string token, ValidationContext<UserRefreshTokenRequest> context)
    {
        SecurityToken? securityToken = jwtTokenGenerator.ValidateRefreshToken(jwtTokenGenerator.GetSettingByScheme(), token!);

        if (securityToken == null)
        {
            context.MessageFormatter.AppendArgument("TokenError", Messages<UserRefreshToken>.Invalid(x => x.Token));
            return false;
        }

        if (securityToken.ValidTo <= DateTime.UtcNow)
        {
            context.MessageFormatter.AppendArgument("TokenError", Messages<UserRefreshToken>.Expired(x => x.Token));
            return false;
        }

        return true;
    }
}