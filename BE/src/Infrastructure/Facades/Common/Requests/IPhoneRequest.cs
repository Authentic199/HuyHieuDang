using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Definitions;

namespace HuyHieuDang.Infrastructure.Facades.Common.Requests
{
    public interface IPhoneRequest
    {
        public string? PhoneNumber { get; set; }
    }

    public class PhoneValidator<TEntity> : AbstractValidator<IPhoneRequest>
    {
        public PhoneValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage(Messages<TEntity>.Required(nameof(IPhoneRequest.PhoneNumber)))
                .IsValidPhoneNumber().WithMessage(Messages<TEntity>.Invalid(nameof(IPhoneRequest.PhoneNumber)));
        }
    }
}
