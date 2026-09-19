using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Auth.Jwt;
using HuyHieuDang.Infrastructure.Facades.Auth.Permissions;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Common.Responses;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.Password.Services;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Requests.Users;
using HuyHieuDang.Infrastructure.Modules.Users.Responses.Users;
using HuyHieuDang.Infrastructure.Modules.Users.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HuyHieuDang.Web.Controllers.Users;

[Authorize]
public partial class UsersController : BaseController
{
    private readonly IUserService userService;
    private readonly IChangePasswordService changePasswordService;

    public UsersController(IUserService userService, IChangePasswordService changePasswordService)
    {
        this.userService = userService;
        this.changePasswordService = changePasswordService;
    }

    /// <summary>
    /// Add the provided user.
    /// </summary>
    [HttpPost]
    [HasPermission(schemes: new string[] { JwtScheme.Default }, permissions: MxmResource.Users + MxmAction.Create)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SuccessResultWrapper<UserDetailResponse>>> CreateAsync([FromForm] CreateUserRequest request, CancellationToken cancellation)
    {
        return OkWrapper(await userService.CreateAsync(request, cancellation), Messages<User>.Create());
    }

    /// <summary>
    /// Update the provided user.
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission(schemes: new string[] { JwtScheme.Default }, permissions: MxmResource.Users + MxmAction.Update)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SuccessResultWrapper<UserDetailResponse>>> UpdateAsync([FromRoute] Guid id, [FromForm] UpdateUserRequest request, CancellationToken cancellation)
    {
        return OkWrapper(await userService.UpdateAsync(id, request, cancellation), Messages<User>.Update());
    }

    /// <summary>
    /// Delete multiple users;
    /// </summary>
    [HttpPost("DeleteMany")]
    [HasPermission(schemes: new string[] { JwtScheme.Default }, permissions: MxmResource.Users + MxmAction.Delete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SuccessResultWrapper<MultipleIdentiferResponse>>> DeleteRangeAsync(DeleteUserRequest request, CancellationToken cancellation)
    {
        return OkWrapper(await userService.DeleteRangeAsync(request, cancellation), Messages<User>.Delete());
    }

    /// <summary>
    /// Retrieve listing of users.
    /// </summary>
    [HttpGet]
    [HasPermission(schemes: new string[] { JwtScheme.Default }, permissions: MxmResource.Users + MxmAction.View)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SuccessResultWrapper<PaginationResponse<UserDetailResponse>>>> SearchAsync([FromQuery] QueryContainer request, CancellationToken cancellation)
    {
        return OkWrapper(await userService.SearchAsync(request, cancellation), Messages<User>.Search());
    }

    /// <summary>
    /// Retrieve the user details for the provided id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(schemes: new string[] { JwtScheme.Default }, permissions: MxmResource.Users + MxmAction.ViewDetail)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SuccessResultWrapper<UserDetailResponse>>> GetDetailAsync([FromRoute] Guid id, CancellationToken cancellation)
    {
        return OkWrapper(await userService.DetailAsync(id, cancellation), Messages<User>.Detail());
    }
}