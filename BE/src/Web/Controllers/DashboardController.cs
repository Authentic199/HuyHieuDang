using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.Eligibility.Responses;
using HuyHieuDang.Infrastructure.Modules.Eligibility.Services;
using Microsoft.AspNetCore.Mvc;

namespace HuyHieuDang.Web.Controllers;

/// <summary>
/// Toàn bộ dữ liệu Dashboard trong một lời gọi (mục 6.1 hợp đồng API, UC-10 → UC-13).
/// Không tham số: máy chủ tự lấy hôm nay và năm hiện tại.
/// </summary>
public class DashboardController : BaseController
{
    private readonly IEligibilityService eligibilityService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardController"/> class.
    /// </summary>
    /// <param name="eligibilityService">Nghiệp vụ nhóm tính toán.</param>
    public DashboardController(IEligibilityService eligibilityService)
    {
        this.eligibilityService = eligibilityService;
    }

    /// <summary>
    /// Đợt sắp tới theo QT8, danh sách đủ điều kiện của chính đợt đó và các cảnh báo của năm nay.
    /// </summary>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đủ dữ liệu dựng cả màn hình, kể cả các trạng thái trống.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(SuccessResultWrapper<DashboardResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<DashboardResponse>>> GetAsync(
        CancellationToken cancellationToken)
        => OkWrapper(
            await eligibilityService.GetDashboardAsync(cancellationToken), Messages<DashboardResponse>.Detail());
}
