using FluentValidation;
using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Requests;

/// <summary>
/// Bốn trường chung của thêm mới và sửa (mục 3.3 hợp đồng API).
/// </summary>
/// <remarks>
/// <see cref="Gender"/> khai báo là chuỗi chứ không phải <c>Gender?</c>: giá trị lạ phải trả đúng
/// khóa <c>Mes.PartyMember.Invalid.Gender</c>, trong khi enum sẽ hỏng ngay ở bước đọc JSON và trả
/// một thông điệp thô của bộ tuần tự hóa. Quyết định này đã ghi cho Technical Writer.
/// </remarks>
public abstract class PartyMemberRequest
{
    /// <summary>
    /// Tên hợp lệ của giới tính Nam trên API.
    /// </summary>
    public const string MaleValue = nameof(Enums.Gender.Male);

    /// <summary>
    /// Tên hợp lệ của giới tính Nữ trên API.
    /// </summary>
    public const string FemaleValue = nameof(Enums.Gender.Female);

    /// <summary>
    /// Độ dài tối đa của họ tên.
    /// </summary>
    public const int FullNameMaxLength = 200;

    /// <summary>
    /// Họ và tên đầy đủ. Bắt buộc.
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Ngày sinh, ngày thuần. Để trống khi không rõ.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Giới tính: <c>Male</c>, <c>Female</c> hoặc <see langword="null"/>.
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Ngày vào Đảng chính thức, ngày thuần. Bắt buộc.
    /// </summary>
    public DateOnly? OfficialAdmissionDate { get; set; }

    /// <summary>
    /// Đổi <see cref="Gender"/> sang enum. Chỉ gọi sau khi đã qua kiểm tra hợp lệ.
    /// </summary>
    /// <returns>Giới tính đã đổi kiểu, hoặc <see langword="null"/> khi bỏ trống.</returns>
    public Enums.Gender? ToGender()
        => string.IsNullOrWhiteSpace(Gender) ? null : Enum.Parse<Enums.Gender>(Gender, ignoreCase: false);
}

/// <summary>
/// Bảng ràng buộc dùng chung cho thêm mới và sửa. "Hôm nay" lấy từ <see cref="IDateTimeProvider"/>,
/// không đọc thẳng đồng hồ hệ thống.
/// </summary>
/// <typeparam name="TRequest">Kiểu yêu cầu cụ thể.</typeparam>
public abstract class PartyMemberRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : PartyMemberRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PartyMemberRequestValidator{TRequest}"/> class.
    /// </summary>
    /// <param name="dateTimeProvider">Nguồn thời gian của hệ thống.</param>
    protected PartyMemberRequestValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage(Messages<PartyMember>.Required(x => x.FullName))
            .MaximumLength(PartyMemberRequest.FullNameMaxLength)
            .WithMessage(Messages<PartyMember>.OverLength(x => x.FullName));

        RuleFor(x => x.Gender)
            .Must(value => value is null or PartyMemberRequest.MaleValue or PartyMemberRequest.FemaleValue)
            .WithMessage(Messages<PartyMember>.Invalid(x => x.Gender));

        RuleFor(x => x.OfficialAdmissionDate)
            .NotNull()
            .WithMessage(Messages<PartyMember>.Required(x => x.OfficialAdmissionDate))
            .Must(value => value <= dateTimeProvider.Today)
            .WithMessage(Messages<PartyMember>.Invalid(x => x.OfficialAdmissionDate));

        RuleFor(x => x.DateOfBirth)
            .Must((request, value) => value is null || request.OfficialAdmissionDate is null || value < request.OfficialAdmissionDate)
            .WithMessage(Messages<PartyMember>.Invalid(x => x.DateOfBirth));
    }
}
