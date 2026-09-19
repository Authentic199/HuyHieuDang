using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.Auth.Requests;
using HuyHieuDang.Infrastructure.Modules.Auth.Responses;
using HuyHieuDang.Infrastructure.Modules.Auth.Services;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HuyHieuDang.Web.Controllers;

/// <summary>
/// Đăng nhập, đăng xuất và thông tin phiên (UC-00, UC-01).
/// </summary>
public class AuthController : BaseController
{
    private readonly IAuthService authService;

    public AuthController(IAuthService authService)
    {
        this.authService = authService;
    }

    /// <summary>
    /// Đăng nhập và nhận JWT. Endpoint duy nhất không cần token.
    /// </summary>
    [HttpPost("Login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SuccessResultWrapper<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<LoginResponse>>> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
        => OkWrapper(await authService.LoginAsync(request, cancellationToken), Messages<User>.Action(ControllerActions.Login, true));

    /// <summary>
    /// Đăng xuất. JWT không trạng thái nên máy chủ chỉ ghi nhận; Frontend tự xóa token.
    /// </summary>
    [HttpPost("Logout")]
    [ProducesResponseType(typeof(SuccessResultWrapper<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<SuccessResultWrapper<object>> Logout()
        => OkWrapper<object>(null, Messages<User>.Action(ControllerActions.Logout, true));

    /// <summary>
    /// Thông tin phiên hiện tại. Trả <c>401</c> khi token thiếu, sai chữ ký hoặc hết hạn.
    /// </summary>
    [HttpGet("Me")]
    [ProducesResponseType(typeof(SuccessResultWrapper<SessionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<SessionResponse>>> MeAsync(CancellationToken cancellationToken)
        => OkWrapper(await authService.GetSessionAsync(cancellationToken), Messages<User>.Detail());
}
