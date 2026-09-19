using HuyHieuDang.Core.Common.Interfaces;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Requests;

/// <summary>
/// Thân yêu cầu của <c>POST /api/PartyMembers</c> (UC-21).
/// </summary>
public class CreatePartyMemberRequest : PartyMemberRequest
{
}

/// <summary>
/// Không kiểm tra trùng tên: hai người cùng tên, cùng ngày vẫn là hai bản ghi (QT9).
/// </summary>
public class CreatePartyMemberRequestValidator : PartyMemberRequestValidator<CreatePartyMemberRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePartyMemberRequestValidator"/> class.
    /// </summary>
    /// <param name="dateTimeProvider">Nguồn thời gian của hệ thống.</param>
    public CreatePartyMemberRequestValidator(IDateTimeProvider dateTimeProvider)
        : base(dateTimeProvider)
    {
    }
}
