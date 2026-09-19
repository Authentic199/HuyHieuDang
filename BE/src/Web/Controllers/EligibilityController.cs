using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities;
using HuyHieuDang.Infrastructure.Modules.Eligibility.Requests;
using HuyHieuDang.Infrastructure.Modules.Eligibility.Responses;
using HuyHieuDang.Infrastructure.Modules.Eligibility.Services;
using Microsoft.AspNetCore.Mvc;

namespace HuyHieuDang.Web.Controllers;

/// <summary>
/// Danh sách đủ điều kiện theo đợt và năm, danh sách chưa thuộc đợt nào và con số cho badge
/// (mục 6.2 → 6.4 hợp đồng API, UC-34, UC-40). Mọi danh sách được tính lại ở từng lời gọi và
/// không được lưu vào bảng nào (QT5).
/// </summary>
public class EligibilityController : BaseController
{
    private readonly IEligibilityService eligibilityService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EligibilityController"/> class.
    /// </summary>
    /// <param name="eligibilityService">Nghiệp vụ nhóm tính toán.</param>
    public EligibilityController(IEligibilityService eligibilityService)
    {
        this.eligibilityService = eligibilityService;
    }

    /// <summary>
    /// Danh sách đủ điều kiện của một đợt trong một năm (UC-34, QT4).
    /// </summary>
    /// <param name="request">Id đợt và năm đang xét.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đợt đã gắn năm, tổng số, phân bổ theo mốc và danh sách đầy đủ.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(SuccessResultWrapper<EligibilityListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<EligibilityListResponse>>> SearchAsync(
        [FromQuery] EligibilityQueryRequest request, CancellationToken cancellationToken)
        => OkWrapper(
            await eligibilityService.SearchAsync(request, cancellationToken), Messages<AwardPeriod>.Search());

    /// <summary>
    /// Danh sách người tròn mốc trong năm nhưng không rơi vào đợt nào (UC-40, QT7).
    /// </summary>
    /// <param name="request">Năm đang xét.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Tổng số và danh sách kèm khoảng trống của từng người.</returns>
    [HttpGet("Unassigned")]
    [ProducesResponseType(typeof(SuccessResultWrapper<UnassignedListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<UnassignedListResponse>>> SearchUnassignedAsync(
        [FromQuery] EligibilityYearQueryRequest request, CancellationToken cancellationToken)
        => OkWrapper(
            await eligibilityService.SearchUnassignedAsync(request, cancellationToken),
            Messages<AwardPeriod>.Search());

    /// <summary>
    /// Số người bị sót trong năm, dành riêng cho badge trên menu trái.
    /// </summary>
    /// <param name="request">Năm đang xét.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Năm và số người bị sót.</returns>
    [HttpGet("UnassignedCount")]
    [ProducesResponseType(typeof(SuccessResultWrapper<UnassignedCountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<UnassignedCountResponse>>> CountUnassignedAsync(
        [FromQuery] EligibilityYearQueryRequest request, CancellationToken cancellationToken)
        => OkWrapper(
            await eligibilityService.CountUnassignedAsync(request, cancellationToken),
            Messages<AwardPeriod>.Search());
}
