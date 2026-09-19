namespace HuyHieuDang.Infrastructure.Modules.Auth.Responses;

/// <summary>
/// Phần <c>data</c> của <c>GET /api/Auth/Me</c>: đủ để Frontend dựng phần khung chung
/// mà không phải gọi thêm endpoint nào.
/// </summary>
public class SessionResponse
{
    /// <summary>
    /// Tên đăng nhập của phiên hiện tại.
    /// </summary>
    public string Username { get; set; } = default!;

    /// <summary>
    /// Tên hiển thị trên thanh tiêu đề.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Tên đơn vị lấy từ cài đặt hệ thống; <c>null</c> nghĩa là chưa đặt (UC-51).
    /// </summary>
    public string? UnitName { get; set; }

    /// <summary>
    /// Ngày hôm nay theo đồng hồ máy chủ (múi giờ Asia/Ho_Chi_Minh).
    /// </summary>
    public DateOnly ServerDate { get; set; }

    /// <summary>
    /// Thời điểm token hết hạn, theo UTC.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }
}
