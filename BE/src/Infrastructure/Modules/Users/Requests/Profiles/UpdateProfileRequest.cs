using AutoMapper;
using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;

namespace HuyHieuDang.Infrastructure.Modules.Users.Requests.Profiles;

[MessageDisplay(nameof(User))]
public class UpdateProfileRequest
{
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
    public DateTime? DayOfBirth { get; set; }
}

public class UpdateProfileValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessagesType.Required)
            .MaximumLength(250).WithMessage(MessagesType.OverLength);

        RuleFor(x => x.PhoneNumber)
           .NotEmpty().WithMessage(MessagesType.Required)
           .IsValidPhoneNumber().WithMessage(MessagesType.Invalid);
    }
}

public class UpdateProfileRequestProfile : Profile
{
    public UpdateProfileRequestProfile()
    {
        CreateMap<UpdateProfileRequest, User>();
    }
}