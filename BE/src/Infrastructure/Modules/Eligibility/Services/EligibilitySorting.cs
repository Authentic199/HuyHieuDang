using HuyHieuDang.Infrastructure.Modules.Eligibility.Responses;
using System.Globalization;

namespace HuyHieuDang.Infrastructure.Modules.Eligibility.Services;

/// <summary>
/// Quy ước sắp xếp dùng chung cho mọi danh sách đủ điều kiện: Mốc huy hiệu tăng dần, trong cùng
/// mốc thì theo Họ tên đầy đủ theo bảng chữ cái tiếng Việt (Đ sau D, dấu thanh đúng thứ tự).
/// </summary>
/// <remarks>
/// Kế hoạch kiểm thử (ca U-419, A-214, A-405, E1-16) và tệp kết quả mong đợi
/// <c>tests/fixtures/data/expected.json</c> đều sắp theo **Họ tên đầy đủ**; mục 1.8 của
/// <c>docs/api-contract.md</c> lại mô tả sắp theo tên gọi (từ cuối) trước. Chọn theo bộ dữ liệu
/// kiểm thử vì đó là điều kiện nghiệm thu, và đã báo lại điểm lệch để Technical Writer chỉnh mục 1.8.
/// </remarks>
public static class EligibilitySorting
{
    /// <summary>
    /// Bộ so sánh chuỗi theo văn hóa <c>vi-VN</c>: <c>Đào Văn Ân</c> đứng trước
    /// <c>Nguyễn Văn An</c>, đúng bảng chữ cái tiếng Việt chứ không theo mã Unicode (OQ-3).
    /// </summary>
    public static readonly StringComparer VietnameseComparer =
        StringComparer.Create(CultureInfo.GetCultureInfo("vi-VN"), ignoreCase: false);

    /// <summary>
    /// Sắp một danh sách đủ điều kiện theo quy ước chung.
    /// </summary>
    /// <typeparam name="TMember">Kiểu dòng, dòng đủ điều kiện hoặc dòng chưa thuộc đợt nào.</typeparam>
    /// <param name="members">Danh sách cần sắp.</param>
    /// <returns>Danh sách đã sắp theo Mốc rồi Họ tên.</returns>
    public static IOrderedEnumerable<TMember> Order<TMember>(IEnumerable<TMember> members)
        where TMember : EligibleMemberResponse
        => members
            .OrderBy(x => x.Milestone)
            .ThenBy(x => x.FullName, VietnameseComparer);
}
