using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities;

namespace HuyHieuDang.Infrastructure.Modules.AwardPeriods.Requests;

/// <summary>
/// Tên trường trong khóa thông điệp không trùng với tên cột của bảng: hợp đồng API gọi cặp
/// ngày/tháng là <c>FromDate</c> / <c>ToDate</c>.
/// </summary>
public static class AwardPeriodMessageProperties
{
    /// <summary>Khóa lỗi của Từ ngày.</summary>
    public const string FromDate = "FromDate";

    /// <summary>Khóa lỗi của Đến ngày.</summary>
    public const string ToDate = "ToDate";
}

/// <summary>
/// Năm trường chung của thêm mới và sửa đợt trao huy hiệu (mục 5.3 hợp đồng API).
/// Đợt chỉ lưu ngày/tháng, không lưu năm (QT6).
/// </summary>
public abstract class AwardPeriodRequest
{
    /// <summary>
    /// Độ dài tối đa của tên đợt.
    /// </summary>
    public const int NameMaxLength = 100;

    /// <summary>
    /// Tên đợt. Bắt buộc, không trùng tên đợt khác.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Ngày của Từ ngày (1–31).
    /// </summary>
    public int? FromDay { get; set; }

    /// <summary>
    /// Tháng của Từ ngày (1–12).
    /// </summary>
    public int? FromMonth { get; set; }

    /// <summary>
    /// Ngày của Đến ngày (1–31).
    /// </summary>
    public int? ToDay { get; set; }

    /// <summary>
    /// Tháng của Đến ngày (1–12).
    /// </summary>
    public int? ToMonth { get; set; }

    /// <summary>
    /// Một cặp ngày/tháng có thật hay không. Cho phép <c>29/02</c> vì đợt dùng chung cho mọi năm,
    /// năm không nhuận sẽ được lùi về 28/02 lúc gắn năm.
    /// </summary>
    /// <param name="day">Ngày.</param>
    /// <param name="month">Tháng.</param>
    /// <returns><see langword="true"/> khi ngày/tháng có thật.</returns>
    public static bool IsRealDayMonth(int? day, int? month)
    {
        if (day is null || month is null || month is < 1 or > 12 || day < 1)
        {
            return false;
        }

        // 2000 là năm nhuận nên 29/02 được coi là hợp lệ.
        return day <= DateTime.DaysInMonth(2000, month.Value);
    }
}

/// <summary>
/// Bảng ràng buộc dùng chung cho thêm mới và sửa. Trùng tên do service trả lời vì cần đọc dữ liệu.
/// </summary>
/// <typeparam name="TRequest">Kiểu yêu cầu cụ thể.</typeparam>
public abstract class AwardPeriodRequestValidator<TRequest> : AbstractValidator<TRequest>
    where TRequest : AwardPeriodRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AwardPeriodRequestValidator{TRequest}"/> class.
    /// </summary>
    protected AwardPeriodRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage(Messages<AwardPeriod>.Required(x => x.Name))
            .MaximumLength(AwardPeriodRequest.NameMaxLength)
            .WithMessage(Messages<AwardPeriod>.OverLength(x => x.Name));

        RuleFor(x => x.FromDay)
            .Must((request, _) => AwardPeriodRequest.IsRealDayMonth(request.FromDay, request.FromMonth))
            .WithMessage(Messages<AwardPeriod>.Invalid(AwardPeriodMessageProperties.FromDate));

        RuleFor(x => x.ToDay)
            .Must((request, _) => AwardPeriodRequest.IsRealDayMonth(request.ToDay, request.ToMonth))
            .WithMessage(Messages<AwardPeriod>.Invalid(AwardPeriodMessageProperties.ToDate));

        // QT6 không còn buộc (fromMonth, fromDay) ≤ (toMonth, toDay): Từ ngày đứng sau Đến ngày
        // là đợt vắt qua 31/12, ví dụ 01/12 – 28/02. Chỉ còn ba thứ chặn lưu: thiếu tên, trùng
        // tên, ngày/tháng không có thật.
    }
}
