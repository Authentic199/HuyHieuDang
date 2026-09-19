using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Requests;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Responses;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Services;
using Microsoft.AspNetCore.Mvc;

namespace HuyHieuDang.Web.Controllers;

/// <summary>
/// Đợt trao huy hiệu: danh sách kèm trạng thái và cảnh báo, lấy một, thêm, sửa, xóa
/// (UC-30 → UC-33, UC-36).
/// </summary>
public class AwardPeriodsController : BaseController
{
    private readonly IAwardPeriodService awardPeriodService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AwardPeriodsController"/> class.
    /// </summary>
    /// <param name="awardPeriodService">Nghiệp vụ đợt trao huy hiệu.</param>
    public AwardPeriodsController(IAwardPeriodService awardPeriodService)
    {
        this.awardPeriodService = awardPeriodService;
    }

    /// <summary>
    /// Danh sách đợt kèm trạng thái năm đang xét, số người đủ điều kiện, cảnh báo chồng lấn /
    /// khoảng trống và dải độ phủ 12 tháng (UC-30, UC-36).
    /// </summary>
    /// <param name="request">Tham số truy vấn theo mục 5.1 hợp đồng API.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đủ dữ liệu cho bảng, dải độ phủ và banner cảnh báo.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(SuccessResultWrapper<AwardPeriodListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<AwardPeriodListResponse>>> SearchAsync(
        [FromQuery] AwardPeriodQueryRequest request, CancellationToken cancellationToken)
        => OkWrapper(await awardPeriodService.SearchAsync(request, cancellationToken), Messages<AwardPeriod>.Search());

    /// <summary>
    /// Lấy một đợt theo id (UC-34 tab Thông tin).
    /// </summary>
    /// <param name="id">Id đợt.</param>
    /// <param name="request">Tham số truy vấn theo mục 5.2 hợp đồng API.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đợt đã gắn năm đang xét.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SuccessResultWrapper<AwardPeriodResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<AwardPeriodResponse>>> GetByIdAsync(
        Guid id, [FromQuery] AwardPeriodQueryRequest request, CancellationToken cancellationToken)
        => OkWrapper(
            await awardPeriodService.GetByIdAsync(id, request, cancellationToken), Messages<AwardPeriod>.Detail());

    /// <summary>
    /// Thêm một đợt (UC-31). Đợt chồng lấn hay để hở khoảng trống vẫn lưu được, cảnh báo đi kèm
    /// trong <c>data</c> chứ không chặn lưu.
    /// </summary>
    /// <param name="request">Dữ liệu đợt.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đợt vừa tạo kèm cảnh báo của năm hiện tại.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SuccessResultWrapper<AwardPeriodMutationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<AwardPeriodMutationResponse>>> CreateAsync(
        [FromBody] CreateAwardPeriodRequest request, CancellationToken cancellationToken)
        => OkWrapper(await awardPeriodService.CreateAsync(request, cancellationToken), Messages<AwardPeriod>.Create());

    /// <summary>
    /// Sửa một đợt (UC-32). Có hiệu lực ngay cho mọi năm, kể cả năm hiện tại (QT6).
    /// </summary>
    /// <param name="id">Id đợt.</param>
    /// <param name="request">Dữ liệu mới, gửi đủ cả năm trường.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đợt sau khi sửa kèm cảnh báo của năm hiện tại.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SuccessResultWrapper<AwardPeriodMutationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<AwardPeriodMutationResponse>>> UpdateAsync(
        Guid id, [FromBody] UpdateAwardPeriodRequest request, CancellationToken cancellationToken)
        => OkWrapper(
            await awardPeriodService.UpdateAsync(id, request, cancellationToken), Messages<AwardPeriod>.Update());

    /// <summary>
    /// Xóa hẳn một đợt (UC-33, QT10). Không xóa đảng viên nào.
    /// </summary>
    /// <param name="id">Id đợt.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Id vừa xóa kèm cảnh báo mới sinh ra.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(SuccessResultWrapper<AwardPeriodDeletedResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<AwardPeriodDeletedResponse>>> DeleteAsync(
        Guid id, CancellationToken cancellationToken)
        => OkWrapper(await awardPeriodService.DeleteAsync(id, cancellationToken), Messages<AwardPeriod>.Delete());
}
