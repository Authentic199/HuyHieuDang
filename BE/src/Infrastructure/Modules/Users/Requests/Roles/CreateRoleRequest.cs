using AutoMapper;
using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.Users.Validations;

namespace HuyHieuDang.Infrastructure.Modules.Users.Requests.Roles;

[MessageDisplay(nameof(Role))]
public class CreateRoleRequest : RoleRequest
{
    public string? Code { get; set; }
}

public class CreateRoleRequestValidator : AbstractValidator<CreateRoleRequest>
{
    public CreateRoleRequestValidator(IRepositoryWrapper repositoryWrapper)
    {
        Include(new RoleBaseRequestValidator(repositoryWrapper));

        RuleFor(x => x.Code)
           .NotEmpty().WithMessage(MessagesType.Required)
           .MaximumLength(250).WithMessage(MessagesType.OverLength)
           .NotSpecialCharacter().WithMessage(MessagesType.NotSpecialCharacter)
           .Must(code => !repositoryWrapper.IsExistRoleCode(code!))
           .WithMessage(MessagesType.AlreadyExist);
    }
}

public class CreateRoleRequestProfile : Profile
{
    public CreateRoleRequestProfile()
    {
        CreateMap<CreateRoleRequest, Role>();
    }
}