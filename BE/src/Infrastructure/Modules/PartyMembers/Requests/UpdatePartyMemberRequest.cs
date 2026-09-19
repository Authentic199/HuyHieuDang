using HuyHieuDang.Core.Common.Interfaces;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Requests;

/// <summary>
/// Thân yêu cầu của <c>PUT /api/PartyMembers/{id}</c> (UC-22). Gửi đủ cả bốn trường,
/// kể cả trường muốn xóa thì gửi <see langword="null"/>.
/// </summary>
public class UpdatePartyMemberRequest : PartyMemberRequest
{
}

/// <summary>
/// Ràng buộc giống thêm mới; riêng "không tìm thấy" do service trả lời.
/// </summary>
public class UpdatePartyMemberRequestValidator : PartyMemberRequestValidator<UpdatePartyMemberRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePartyMemberRequestValidator"/> class.
    /// </summary>
    /// <param name="dateTimeProvider">Nguồn thời gian của hệ thống.</param>
    public UpdatePartyMemberRequestValidator(IDateTimeProvider dateTimeProvider)
        : base(dateTimeProvider)
    {
    }
}
