using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.Users.Validations;

namespace HuyHieuDang.Infrastructure.Modules.Users.Requests
{
    [MessageDisplay(nameof(Role))]
    public abstract class RoleRequest
    {
        public string? Name { get; set; }

        public ICollection<string>? PermissionCodes { get; set; }
    }

    public class RoleBaseRequestValidator : AbstractValidator<RoleRequest>
    {
        public RoleBaseRequestValidator(IRepositoryWrapper repositoryWrapper)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(MessagesType.Required)
                .MaximumLength(255).WithMessage(MessagesType.OverLength);

            RuleFor(x => x.PermissionCodes)
                   .NotEmpty().WithMessage(MessagesType.Required)
                   .Must((request, _) => repositoryWrapper.IsExistPermissionCodes(request.PermissionCodes!))
                   .WithMessage(MessagesType.NotFound);
        }
    }
}