using FluentValidation;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Notes;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Requests;

/// <summary>
/// Thân yêu cầu của <c>PUT /api/PartyMembers/{id}/Note</c> (UC-26, mục 3.6 hợp đồng API).
/// Chỉ mang ghi chú: bốn trường còn lại của đảng viên giữ nguyên.
/// </summary>
/// <remarks>
/// Thân rỗng <c>{}</c> và <c>{ "note": null }</c> đều có nghĩa là xóa ghi chú.
/// </remarks>
public class UpdatePartyMemberNoteRequest
{
    /// <summary>
    /// Nội dung ghi chú (QT12); <see langword="null"/> hoặc toàn khoảng trắng nghĩa là xóa.
    /// </summary>
    public string? Note { get; set; }
}

/// <summary>
/// Chỉ một ràng buộc: độ dài ghi chú, đếm sau khi cắt khoảng trắng hai đầu.
/// "Không tìm thấy" do service trả lời.
/// </summary>
public class UpdatePartyMemberNoteRequestValidator : AbstractValidator<UpdatePartyMemberNoteRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePartyMemberNoteRequestValidator"/> class.
    /// </summary>
    public UpdatePartyMemberNoteRequestValidator()
    {
        RuleFor(x => x.Note).NoteRule();
    }
}
