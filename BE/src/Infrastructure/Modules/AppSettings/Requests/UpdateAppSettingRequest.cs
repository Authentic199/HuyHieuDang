using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;

namespace HuyHieuDang.Infrastructure.Modules.AppSettings.Requests;

/// <summary>
/// Tên trường trong khóa thông điệp không phải lúc nào cũng là tên cột: hợp đồng API gọi ràng buộc
/// Bắt đầu ≤ Kết thúc là <c>Range</c>.
/// </summary>
public static class AppSettingMessageProperties
{
    /// <summary>Khóa lỗi của ràng buộc Bắt đầu ≤ Kết thúc.</summary>
    public const string Range = "Range";
}

/// <summary>
/// Thân yêu cầu của <c>PUT /api/Settings</c> (mục 7.2 hợp đồng API, UC-50, UC-51).
/// Gửi đủ ba mốc; <see cref="UnitName"/> được phép bỏ trống.
/// </summary>
public class UpdateAppSettingRequest
{
    /// <summary>
    /// Độ dài tối đa của tên đơn vị.
    /// </summary>
    public const int UnitNameMaxLength = 200;

    /// <summary>
    /// Mốc huy hiệu đầu tiên (QT1). Bắt buộc, ≥ 1.
    /// </summary>
    public int? StartYears { get; set; }

    /// <summary>
    /// Mốc huy hiệu cuối cùng (QT1). Bắt buộc, ≥ 1 và ≥ <see cref="StartYears"/>.
    /// </summary>
    public int? EndYears { get; set; }

    /// <summary>
    /// Bước nhảy giữa hai mốc liên tiếp (QT1). Bắt buộc, ≥ 1.
    /// </summary>
    public int? StepYears { get; set; }

    /// <summary>
    /// Tên đơn vị in trên file Excel xuất ra. <see langword="null"/> hoặc rỗng nghĩa là không đặt.
    /// </summary>
    public string? UnitName { get; set; }
}

/// <summary>
/// Ràng buộc của mục 7.2 hợp đồng API. Ba mốc dùng chung bảng luật với
/// <see cref="AppSettingRules"/> để xem trước và lưu không bao giờ lệch nhau.
/// </summary>
public class UpdateAppSettingRequestValidator : AbstractValidator<UpdateAppSettingRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAppSettingRequestValidator"/> class.
    /// </summary>
    public UpdateAppSettingRequestValidator()
    {
        RuleFor(x => x.StartYears)
            .Must(AppSettingRules.IsPositive)
            .WithMessage(Messages<AppSetting>.Invalid(x => x.StartYears));

        RuleFor(x => x.EndYears)
            .Must(AppSettingRules.IsPositive)
            .WithMessage(Messages<AppSetting>.Invalid(x => x.EndYears));

        RuleFor(x => x.StepYears)
            .Must(AppSettingRules.IsPositiveStep)
            .WithMessage(Messages<AppSetting>.Invalid(x => x.StepYears));

        RuleFor(x => x.EndYears)
            .Must((request, _) => request.StartYears <= request.EndYears)
            .When(request => AppSettingRules.IsPositive(request.StartYears)
                && AppSettingRules.IsPositive(request.EndYears))
            .WithMessage(Messages<AppSetting>.Invalid(AppSettingMessageProperties.Range));

        RuleFor(x => x.UnitName)
            .MaximumLength(UpdateAppSettingRequest.UnitNameMaxLength)
            .WithMessage(Messages<AppSetting>.OverLength(x => x.UnitName));
    }
}
