using HuyHieuDang.Infrastructure.Facades.Common.Extensions;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Requests;

/// <summary>
/// Tham số truy vấn của <c>GET /api/PartyMembers</c> (mục 1.7 hợp đồng API).
/// Mặc định 20 dòng mỗi trang và trần <see cref="QueryContainer.MaxPageSize"/> nằm ở
/// <see cref="QueryContainer"/> để mọi endpoint có phân trang dùng chung một bộ luật.
/// </summary>
public class PartyMemberQueryRequest : QueryContainer
{
}
