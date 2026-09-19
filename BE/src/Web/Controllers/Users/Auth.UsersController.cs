using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Requests.Authentications;
using HuyHieuDang.Infrastructure.Modules.Users.Responses.Authentications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HuyHieuDang.Web.Controllers.Users
{
    public partial class UsersController : BaseController
    {
        /// <summary>
        /// Returns an access token to allow for Bearer authentication along with a refresh token.
        /// </summary>
        [HttpPost("Auth")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SuccessResultWrapper<AuthUserResponse>>> AuthenticateAsync([FromBody] AuthUserRequest request, CancellationToken cancellation)
        {
            return OkWrapper(await userService.AuthenticateAsync(request, cancellation), Messages<User>.Action(ControllerActions.Login, true));
        }

        [HttpPost("RefreshToken")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SuccessResultWrapper<AuthUserResponse>>> RefreshAsync([FromBody] UserRefreshTokenRequest request, CancellationToken cancellation)
        {
            return OkWrapper(await userService.RefreshAsync(request, cancellation), Messages<User>.Action(ControllerActions.RefreshToken, true));
        }
    }
}