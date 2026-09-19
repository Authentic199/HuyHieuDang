using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Common.Requests;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;

namespace HuyHieuDang.Infrastructure.Modules.Users.Requests.Roles
{
    [MessageDisplay(nameof(Role))]
    public class DeleteRoleRangeRequest : RangeItemRequest<Guid>
    {
    }

    public class DeleteRoleRangeRequestValidator : AbstractValidator<DeleteRoleRangeRequest>
    {
        public DeleteRoleRangeRequestValidator()
        {
            RuleFor(x => x.Items).NotEmpty().WithMessage(MessagesType.Required);
        }
    }
}