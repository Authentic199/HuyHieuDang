namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Responses;

/// <summary>
/// Phản hồi của xóa một đảng viên: <c>{ "id": "…" }</c> (mục 3.5 hợp đồng API).
/// </summary>
/// <param name="Id">Id của bản ghi vừa xóa.</param>
public record PartyMemberIdentifierResponse(Guid Id);
