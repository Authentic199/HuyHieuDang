using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;

namespace HuyHieuDang.Infrastructure.Modules.Users.Requests.Passwords;

[MessageDisplay(nameof(User))]
public class ChangeUserPasswordRequest
{
    public string? OldPassword { get; set; }

    public string? Password { get; set; }
}

public class ChangeUserPasswordValidator : AbstractValidator<ChangeUserPasswordRequest>
{
    public ChangeUserPasswordValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage(MessagesType.Required);
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(MessagesType.Required)
            .IsValidPassword().WithMessage(MessagesType.Invalid);
    }
}