using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using Microsoft.AspNetCore.Mvc;

namespace HuyHieuDang.Infrastructure.Modules.AwardPeriods.Requests;

/// <summary>
/// Tham số truy vấn của <c>GET /api/AwardPeriods</c> và <c>GET /api/AwardPeriods/{id}</c>
/// (mục 5.1 và 5.2 hợp đồng API). Bỏ trống thì lấy năm hiện tại theo lịch máy chủ.
/// </summary>
[MessageDisplay(QueryMessageModule)]
public class AwardPeriodQueryRequest
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
    /// Tên mô-đun trong khóa thông điệp: hợp đồng API dùng chung khóa
    /// <c>Mes.Query.Invalid.Year</c> cho mọi màn có tham số năm.
    /// </summary>
    internal const string QueryMessageModule = "Query";

    /// <summary>
    /// Năm đang xét.
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Bộ lọc dùng chung, dạng <c>filter.&lt;Tên trường&gt;=$eq:&lt;giá trị&gt;</c>, áp lên danh
    /// sách đợt đã gắn năm. Endpoint này không phân trang nên chỉ nhận bộ lọc, không nhận
    /// <c>pageSize</c> hay <c>orderBy</c>.
    /// <para>
    /// Có mặt ở đây để tham số <c>filter.*</c> không còn bị nuốt lặng lẽ: giá trị sai kiểu
    /// trả <c>400</c> kèm <c>Mes.Common.Invalid.Parameter</c> thay vì trả về cả kho (QC-T27-05).
    /// </para>
    /// </summary>
    [ModelBinder(BinderType = typeof(CustomFilterBinder))]
    public Dictionary<string, List<string>?>? Filter { get; set; }
}

/// <summary>
/// Năm ngoài khoảng cho phép trả <c>Mes.Query.Invalid.Year</c>.
/// </summary>
public class AwardPeriodQueryRequestValidator : AbstractValidator<AwardPeriodQueryRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AwardPeriodQueryRequestValidator"/> class.
    /// </summary>
    public AwardPeriodQueryRequestValidator()
    {
        RuleFor(x => x.Year)
            .Must(value => value is null or >= AwardPeriodQueryRequest.MinYear and <= AwardPeriodQueryRequest.MaxYear)
            .WithMessage(Messages<AwardPeriodQueryRequest>.Invalid(x => x.Year));
    }
}
