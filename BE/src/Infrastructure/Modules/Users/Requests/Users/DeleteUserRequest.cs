using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Common.Requests;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;

namespace HuyHieuDang.Infrastructure.Modules.Users.Requests.Users
{
    [MessageDisplay(nameof(User))]
    public class DeleteUserRequest : RangeItemRequest<Guid>
    {
    }

    public class DeleteUserRequestValidator : AbstractValidator<DeleteUserRequest>
    {
        public DeleteUserRequestValidator()
        {
            RuleFor(x => x.Items)
                .NotEmpty().WithMessage(MessagesType.Required)
                .NotDuplicate().WithMessage(MessagesType.Repeated);
        }
    }
}