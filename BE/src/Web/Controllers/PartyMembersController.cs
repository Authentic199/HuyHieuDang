using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Common.Responses;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Requests;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Responses;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Services;
using Microsoft.AspNetCore.Mvc;

namespace HuyHieuDang.Web.Controllers;

/// <summary>
/// Đảng viên: danh sách, lấy một, thêm, sửa, xóa một và xóa nhiều (UC-20 → UC-23).
/// </summary>
public class PartyMembersController : BaseController
{
    private readonly IPartyMemberService partyMemberService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartyMembersController"/> class.
    /// </summary>
    /// <param name="partyMemberService">Nghiệp vụ đảng viên.</param>
    public PartyMembersController(IPartyMemberService partyMemberService)
    {
        this.partyMemberService = partyMemberService;
    }

    /// <summary>
    /// Danh sách đảng viên có tìm theo họ tên, lọc giới tính, sắp xếp và phân trang (UC-20).
    /// </summary>
    /// <param name="request">Tham số truy vấn theo mục 1.7 hợp đồng API.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Một trang đảng viên kèm tuổi đảng tính theo hôm nay.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(SuccessResultWrapper<PaginationResponse<PartyMemberResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<PaginationResponse<PartyMemberResponse>>>> SearchAsync(
        [FromQuery] PartyMemberQueryRequest request, CancellationToken cancellationToken)
        => OkWrapper(await partyMemberService.SearchAsync(request, cancellationToken), Messages<PartyMember>.Search());

    /// <summary>
    /// Lấy một đảng viên theo id (UC-22).
    /// </summary>
    /// <param name="id">Id đảng viên.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đảng viên tìm thấy.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SuccessResultWrapper<PartyMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<PartyMemberResponse>>> GetByIdAsync(
        Guid id, CancellationToken cancellationToken)
        => OkWrapper(await partyMemberService.GetByIdAsync(id, cancellationToken), Messages<PartyMember>.Detail());

    /// <summary>
    /// Thêm thủ công một đảng viên (UC-21). Hệ thống cố ý không chống trùng (QT9).
    /// </summary>
    /// <param name="request">Dữ liệu đảng viên.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đảng viên vừa tạo.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SuccessResultWrapper<PartyMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<PartyMemberResponse>>> CreateAsync(
        [FromBody] CreatePartyMemberRequest request, CancellationToken cancellationToken)
        => OkWrapper(await partyMemberService.CreateAsync(request, cancellationToken), Messages<PartyMember>.Create());

    /// <summary>
    /// Sửa một đảng viên (UC-22). Gửi đủ cả bốn trường, trường muốn xóa thì gửi <c>null</c>.
    /// </summary>
    /// <param name="id">Id đảng viên.</param>
    /// <param name="request">Dữ liệu mới.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đảng viên sau khi sửa.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SuccessResultWrapper<PartyMemberResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<PartyMemberResponse>>> UpdateAsync(
        Guid id, [FromBody] UpdatePartyMemberRequest request, CancellationToken cancellationToken)
        => OkWrapper(await partyMemberService.UpdateAsync(id, request, cancellationToken), Messages<PartyMember>.Update());

    /// <summary>
    /// Xóa hẳn một đảng viên (UC-23). Không có thùng rác (QT10).
    /// </summary>
    /// <param name="id">Id đảng viên.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Id vừa xóa.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(SuccessResultWrapper<PartyMemberIdentifierResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<PartyMemberIdentifierResponse>>> DeleteAsync(
        Guid id, CancellationToken cancellationToken)
        => OkWrapper(await partyMemberService.DeleteAsync(id, cancellationToken), Messages<PartyMember>.Delete());

    /// <summary>
    /// Xóa nhiều đảng viên (UC-23). Dùng <c>POST</c> vì <c>DELETE</c> có thân không phải quy ước
    /// của bộ khung. Id không tồn tại bị bỏ qua lặng lẽ.
    /// </summary>
    /// <param name="request">Danh sách id cần xóa.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đúng những id đã xóa được.</returns>
    [HttpPost("DeleteMany")]
    [ProducesResponseType(typeof(SuccessResultWrapper<MultipleIdentiferResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResultWrapper<MultipleIdentiferResponse>>> DeleteManyAsync(
        [FromBody] DeletePartyMemberRangeRequest request, CancellationToken cancellationToken)
        => OkWrapper(await partyMemberService.DeleteRangeAsync(request, cancellationToken), Messages<PartyMember>.Delete());
}
