using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;

namespace HuyHieuDang.Infrastructure.Modules.Users.Requests.Authentications;

[MessageDisplay(nameof(User))]
public class AuthUserRequest
{
    public string? Username { get; set; }

    public string? Password { get; set; }
}

public class AuthUserRequestValidator : AbstractValidator<AuthUserRequest>
{
    public AuthUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(MessagesType.Required);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(MessagesType.Required);
    }
}