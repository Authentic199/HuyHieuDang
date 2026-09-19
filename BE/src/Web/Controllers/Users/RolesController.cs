using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Auth.Jwt;
using HuyHieuDang.Infrastructure.Facades.Auth.Permissions;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Common.Responses;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Requests.Roles;
using HuyHieuDang.Infrastructure.Modules.Users.Responses.Roles;
using HuyHieuDang.Infrastructure.Modules.Users.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HuyHieuDang.Web.Controllers.Users
{
    [Authorize]
    public class RolesController : BaseController
    {
        private readonly IRoleService roleService;

        public RolesController(IRoleService roleService)
        {
            this.roleService = roleService;
        }

        /// <summary>
        /// Add the provided role.
        /// </summary>
        [HttpPost]
        [HasPermission(schemes: new string[] { JwtScheme.Default }, permissions: MxmResource.Roles + MxmAction.Create)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SuccessResultWrapper<RoleDetailResponse>>> CreateAsync(CreateRoleRequest request, CancellationToken cancellation)
            => OkWrapper(await roleService.CreateAsync(request, cancellation), Messages<Role>.Create());

        /// <summary>
        /// Retrieve listing of roles.
        /// </summary>
        [HttpGet]
        [HasPermission(schemes: new string[] { JwtScheme.Default }, permissions: MxmResource.Roles + MxmAction.View)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SuccessResultWrapper<PaginationResponse<RoleDetailResponse>>>> SearchAsync([FromQuery] QueryContainer request, CancellationToken cancellation)
            => OkWrapper(await roleService.SearchAsync(request, cancellation), Messages<Role>.Search());

        /// <summary>
        /// Retrieve the role details for the provided id.
        /// </summary>
        [HttpGet("{id:guid}")]
        [HasPermission(schemes: new string[] { JwtScheme.Default }, permissions: MxmResource.Roles + MxmAction.ViewDetail)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SuccessResultWrapper<RoleDetailResponse>>> GetDetailAsync(Guid id, CancellationToken cancellation)
            => OkWrapper(await roleService.GetAsync(id, cancellation), Messages<Role>.Detail());

        /// <summary>
        /// Delete multiple roles;
        /// </summary>
        [HttpPost("DeleteMany")]
        [HasPermission(schemes: new string[] { JwtScheme.Default }, permissions: MxmResource.Roles + MxmAction.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SuccessResultWrapper<MultipleIdentiferResponse>>> DeleteRangeAsync(DeleteRoleRangeRequest request, CancellationToken cancellation)
        {
            return OkWrapper(await roleService.DeleteRangeAsync(request, cancellation), Messages<Role>.Delete());
        }

        /// <summary>
        /// Update the provided role.
        /// </summary>
        [HttpPut("{id:guid}")]
        [HasPermission(schemes: new string[] { JwtScheme.Default }, permissions: MxmResource.Roles + MxmAction.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SuccessResultWrapper<RoleDetailResponse>>> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellation)
            => OkWrapper(await roleService.UpdateAsync(id, request, cancellation), Messages<Role>.Update());
    }
}