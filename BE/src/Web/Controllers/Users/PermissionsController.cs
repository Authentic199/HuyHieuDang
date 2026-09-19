using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Auth.Jwt;
using HuyHieuDang.Infrastructure.Facades.Auth.Permissions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Responses.Permissions;
using HuyHieuDang.Infrastructure.Modules.Users.Services;
using Microsoft.AspNetCore.Mvc;

namespace HuyHieuDang.Web.Controllers.Users
{
    public class PermissionsController : BaseController
    {
        private readonly IPermissionService permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            this.permissionService = permissionService;
        }

        /// <summary>
        /// Retrieve listing of security permissions.
        /// </summary>
        [HttpGet]
        [HasPermission(schemes: new string[] { JwtScheme.Default })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SuccessResultWrapper<IEnumerable<PermissionResponse>>>> GetAllAsync()
            => OkWrapper(await permissionService.GetAllAsync(), Messages<Permission>.Action(ControllerActions.ViewAll));
    }
}