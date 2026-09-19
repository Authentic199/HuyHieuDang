using HuyHieuDang.Infrastructure.Facades.Common.Extensions;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Requests;

/// <summary>
/// Tham số truy vấn của <c>GET /api/PartyMembers</c> (mục 1.7 hợp đồng API).
/// Khác <see cref="QueryContainer"/> ở đúng một điểm: mặc định 20 dòng mỗi trang.
/// </summary>
public class PartyMemberQueryRequest : QueryContainer
{
    /// <summary>
    /// Số dòng mỗi trang khi Frontend không gửi <c>pageSize</c>.
    /// </summary>
    public const int DefaultPageSize = 20;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartyMemberQueryRequest"/> class.
    /// </summary>
    public PartyMemberQueryRequest()
    {
        PageSize = DefaultPageSize;
    }
}
