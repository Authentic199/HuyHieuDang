using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Requests.Passwords;
using HuyHieuDang.Infrastructure.Modules.Users.Requests.Profiles;
using HuyHieuDang.Infrastructure.Modules.Users.Responses.Users;
using Microsoft.AspNetCore.Mvc;

namespace HuyHieuDang.Web.Controllers.Users
{
    public partial class UsersController : BaseController
    {
        /// <summary>
        /// Retrieve the user details for the authenticated user.
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SuccessResultWrapper<UserDetailResponse>>> GetProfileAsync(CancellationToken cancellation)
        {
            return OkWrapper(await userService.ProfileAsync(cancellation), Messages<User>.Action(ControllerActions.Profile, true));
        }

        /// <summary>
        /// Update the provided user for the authenticated user.
        /// </summary>
        [HttpPut("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SuccessResultWrapper<UserDetailResponse>>> UpdateProfileAsync([FromForm] UpdateProfileRequest request, CancellationToken cancellation)
        {
            return OkWrapper(await userService.UpdateProfileAsync(request, cancellation), Messages<User>.Update());
        }

        /// <summary>
        /// Update the provided user for the authenticated user.
        /// </summary>
        [HttpPut("me/ChangePassword")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SuccessResultWrapper<User>>> ChangePasswordAsync([FromBody] ChangeUserPasswordRequest request, CancellationToken cancellation)
        {
            return OkWrapper(
                await changePasswordService.ChangePasswordAsync<User>(
                request.OldPassword!,
                request.Password!,
                cancellation),
                Messages<User>.Action(ControllerActions.ChangePassword, true));
        }
    }
}