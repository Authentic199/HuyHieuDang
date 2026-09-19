using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Requests;

/// <summary>
/// Thân yêu cầu của <c>POST /api/PartyMembers/DeleteMany</c> (UC-23).
/// </summary>
public class DeletePartyMemberRangeRequest
{
    /// <summary>
    /// Danh sách id cần xóa. Bắt buộc, ít nhất một phần tử.
    /// Id không tồn tại bị bỏ qua lặng lẽ chứ không làm hỏng cả lời gọi.
    /// </summary>
    public ICollection<Guid>? Ids { get; set; }
}

/// <summary>
/// Chỉ đòi danh sách id có mặt và không rỗng.
/// </summary>
public class DeletePartyMemberRangeRequestValidator : AbstractValidator<DeletePartyMemberRangeRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeletePartyMemberRangeRequestValidator"/> class.
    /// </summary>
    public DeletePartyMemberRangeRequestValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty()
            .WithMessage(Messages<PartyMember>.Required(nameof(DeletePartyMemberRangeRequest.Ids)));
    }
}
