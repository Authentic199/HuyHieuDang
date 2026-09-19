using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Common.Requests;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.Base;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Validations;
using Microsoft.AspNetCore.Http;

namespace HuyHieuDang.Infrastructure.Modules.Users.Requests.Users
{
    [MessageDisplay(nameof(User))]
    public abstract class UserRequest : IPhoneRequest
    {
        /// <summary>
        /// Địa chỉ email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Avatar
        /// </summary>
        public IFormFile? AvatarPicture { get; set; }

        /// <summary>
        /// Tên đầy đủ
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Ngày sinh
        /// </summary>
        public DateTimeOffset? DayOfBirth { get; set; }

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public OperationStatus? Status { get; set; } = OperationStatus.Active;

        /// <summary>
        /// Định danh vai trò
        /// </summary>
        public Guid RoleId { get; set; }
    }

    public class BaseUserRequestValidator : AbstractValidator<UserRequest>
    {
        public BaseUserRequestValidator(IRepositoryWrapper repositoryWrapper)
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(MessagesType.Required)
                .MaximumLength(250).WithMessage(MessagesType.OverLength);

            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage(MessagesType.Required)
                .Must(repositoryWrapper.IsExistRole)
                .WithMessage(MessagesType.NotFound);

            RuleFor(x => x.Email)
                .MaximumLength(255).WithMessage(MessagesType.OverLength)
                .EmailAddress().WithMessage(MessagesType.Invalid)
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage(MessagesType.Required)
                .IsInEnum().WithMessage(MessagesType.Invalid);

            RuleFor(x => x.AvatarPicture)
                .IsValidContentType("image").WithMessage(MessagesType.Invalid)
                .When(x => x.AvatarPicture?.Length > 0);
        }
    }
}