using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Requests;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Responses;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Services;
using Microsoft.AspNetCore.Mvc;

namespace HuyHieuDang.Web.Controllers;

/// <summary>
/// Cài đặt toàn hệ thống: ba mốc huy hiệu, tên đơn vị, khôi phục mặc định và xem trước dãy mốc
/// (mục 7 hợp đồng API, UC-50, UC-51). Đổi cài đặt xong là mọi danh sách đủ điều kiện đổi theo
/// ngay vì không có kết quả nào được lưu sẵn (QT5).
/// </summary>
public class SettingsController : BaseController
{
    private readonly IAppSettingService appSettingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsController"/> class.
    /// </summary>
    /// <param name="appSettingService">Nghiệp vụ cài đặt.</param>
    public SettingsController(IAppSettingService appSettingService)
    {
        this.appSettingService = appSettingService;
    }

    /// <summary>
    /// Đọc cài đặt kèm dãy mốc đang có hiệu lực (UC-50, UC-51).
    /// </summary>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Ba mốc, tên đơn vị và dãy mốc đã sinh.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(SuccessResultWrapper<AppSettingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<AppSettingResponse>>> GetAsync(
        CancellationToken cancellationToken)
        => OkWrapper(await appSettingService.GetAsync(cancellationToken), Messages<AppSetting>.Detail());

    /// <summary>
    /// Lưu ba mốc và tên đơn vị (UC-50, UC-51).
    /// </summary>
    /// <param name="request">Giá trị mới, gửi đủ ba mốc.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Cài đặt sau khi lưu.</returns>
    [HttpPut]
    [ProducesResponseType(typeof(SuccessResultWrapper<AppSettingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<AppSettingResponse>>> UpdateAsync(
        [FromBody] UpdateAppSettingRequest request, CancellationToken cancellationToken)
        => OkWrapper(
            await appSettingService.UpdateAsync(request, cancellationToken), Messages<AppSetting>.Update());

    /// <summary>
    /// Khôi phục ba mốc về 30 / 90 / 5; tên đơn vị giữ nguyên (UC-50).
    /// </summary>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Cài đặt sau khi khôi phục.</returns>
    [HttpPost("RestoreDefaults")]
    [ProducesResponseType(typeof(SuccessResultWrapper<AppSettingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<AppSettingResponse>>> RestoreDefaultsAsync(
        CancellationToken cancellationToken)
        => OkWrapper(
            await appSettingService.RestoreDefaultsAsync(cancellationToken), Messages<AppSetting>.Update());

    /// <summary>
    /// Xem trước dãy mốc khi người dùng đang gõ, trước khi bấm Lưu (UC-50). Không ghi gì xuống
    /// cơ sở dữ liệu.
    /// </summary>
    /// <param name="request">Ba tham số, bỏ trống thì lấy giá trị đang lưu.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Dãy mốc và số mốc.</returns>
    [HttpGet("Milestones")]
    [ProducesResponseType(typeof(SuccessResultWrapper<MilestonePreviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<MilestonePreviewResponse>>> PreviewMilestonesAsync(
        [FromQuery] MilestonePreviewRequest request, CancellationToken cancellationToken)
        => OkWrapper(
            await appSettingService.PreviewMilestonesAsync(request, cancellationToken),
            Messages<AppSetting>.Detail());
}
