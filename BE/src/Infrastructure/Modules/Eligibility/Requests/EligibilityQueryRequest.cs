using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using HuyHieuDang.Infrastructure.Facades.Definitions;

namespace HuyHieuDang.Infrastructure.Modules.Eligibility.Requests;

/// <summary>
/// Tham số năm dùng chung cho <c>GET /api/Eligibility/Unassigned</c> và
/// <c>GET /api/Eligibility/UnassignedCount</c> (mục 6.3 và 6.4 hợp đồng API).
/// Bỏ trống thì lấy năm hiện tại theo lịch máy chủ.
/// </summary>
/// <remarks>
/// Khóa thông điệp cố ý nằm trong mô-đun <c>Query</c>: hợp đồng API dùng chung
/// <c>Mes.Query.Invalid.Year</c> cho mọi màn có tham số năm.
/// </remarks>
[MessageDisplay(QueryMessageModule)]
public class EligibilityYearQueryRequest
{
    /// <summary>
    /// Năm nhỏ nhất được phép xét.
    /// </summary>
    public const int MinYear = 1900;

    /// <summary>
    /// Năm lớn nhất được phép xét.
    /// </summary>
    public const int MaxYear = 2200;

    /// <summary>
    /// Tên mô-đun trong khóa thông điệp.
    /// </summary>
    internal const string QueryMessageModule = "Query";

    /// <summary>
    /// Năm đang xét.
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Năm thật sự dùng để tính: năm được hỏi, hoặc năm hiện tại khi bỏ trống.
    /// </summary>
    /// <param name="today">Hôm nay theo lịch máy chủ.</param>
    /// <returns>Năm đang xét.</returns>
    public int ResolveYear(DateOnly today) => Year ?? today.Year;
}

/// <summary>
/// Tham số truy vấn của <c>GET /api/Eligibility</c> (mục 6.2 hợp đồng API).
/// </summary>
[MessageDisplay(QueryMessageModule)]
public class EligibilityQueryRequest : EligibilityYearQueryRequest
{
    /// <summary>
    /// Id đợt cần xem danh sách đủ điều kiện. Id trống hay id lạ đều trả
    /// <c>Mes.AwardPeriod.NotFound</c>, đúng danh sách lỗi của hợp đồng API.
    /// </summary>
    public Guid AwardPeriodId { get; set; }
}

/// <summary>
/// Năm ngoài khoảng cho phép trả <c>Mes.Query.Invalid.Year</c>.
/// </summary>
public class EligibilityYearQueryRequestValidator : AbstractValidator<EligibilityYearQueryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EligibilityYearQueryRequestValidator"/> class.
    /// </summary>
    public EligibilityYearQueryRequestValidator()
    {
        RuleFor(x => x.Year)
            .Must(IsAllowedYear)
            .WithMessage(Messages<EligibilityYearQueryRequest>.Invalid(x => x.Year));
    }

    /// <summary>
    /// Bỏ trống là hợp lệ; còn lại phải nằm trong 1900–2200.
    /// </summary>
    /// <param name="value">Năm được gửi lên.</param>
    /// <returns><see langword="true"/> khi năm chấp nhận được.</returns>
    internal static bool IsAllowedYear(int? value)
        => value is null or >= EligibilityYearQueryRequest.MinYear and <= EligibilityYearQueryRequest.MaxYear;
}

/// <summary>
/// Cùng luật năm như trên, áp cho truy vấn có kèm id đợt.
/// </summary>
public class EligibilityQueryRequestValidator : AbstractValidator<EligibilityQueryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EligibilityQueryRequestValidator"/> class.
    /// </summary>
    public EligibilityQueryRequestValidator()
    {
        RuleFor(x => x.Year)
            .Must(EligibilityYearQueryRequestValidator.IsAllowedYear)
            .WithMessage(Messages<EligibilityYearQueryRequest>.Invalid(x => x.Year));
    }
}
