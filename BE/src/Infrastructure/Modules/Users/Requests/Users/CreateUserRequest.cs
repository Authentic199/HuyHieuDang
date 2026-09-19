using AutoMapper;
using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Validations;

namespace HuyHieuDang.Infrastructure.Modules.Users.Requests.Users;

[MessageDisplay(nameof(User))]
public class CreateUserRequest : UserRequest
{
    public string? Username { get; set; }

    /// <summary>
    /// Mật khẩu
    /// </summary>
    public string? Password { get; set; }
}

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator(IRepositoryWrapper repositoryWrapper)
    {
        Include(new BaseUserRequestValidator(repositoryWrapper));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(MessagesType.Required)
            .IsValidPassword().WithMessage(MessagesType.Invalid);

        RuleFor(x => x.Username)
           .NotEmpty().WithMessage(MessagesType.Required)
           .NotWhiteSpace().WithMessage(MessagesType.NotWhiteSpace)
           .NotSpecialCharacter("@._").WithMessage(MessagesType.NotSpecialCharacter)
           .MaximumLength(250).WithMessage(MessagesType.OverLength)
           .Must(userName => !repositoryWrapper.IsExistUsername(userName))
           .WithMessage(MessagesType.AlreadyExist);

        When(
            x => !string.IsNullOrEmpty(x.Email),
            () => RuleFor(x => x.Email)
                .EmailAddress().WithMessage(MessagesType.Invalid)
                .Must(email => !repositoryWrapper.IsExistUserEmail(email!))
                .WithMessage(MessagesType.AlreadyExist)
            );
    }
}

public class CreateUserRequestProfile : Profile
{
    public CreateUserRequestProfile()
    {
        CreateMap<CreateUserRequest, User>();
    }
}