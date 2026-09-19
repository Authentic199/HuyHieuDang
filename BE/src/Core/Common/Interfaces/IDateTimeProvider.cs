namespace HuyHieuDang.Core.Common.Interfaces;

/// <summary>
/// Nguồn thời gian duy nhất của hệ thống (T-FIX-1). Mọi chỗ cần "bây giờ" hay "hôm nay"
/// đều tiêm giao diện này thay vì đọc thẳng đồng hồ máy, để kiểm thử đóng băng được thời gian.
/// Ca kiểm thử tĩnh A-901 quét mã nguồn và chặn mọi lời gọi đồng hồ ngoài lớp cài đặt provider,
/// nên tệp này cũng không viết tên các lời gọi đó ra.
/// </summary>
/// <remarks>
/// Service tính mốc tuổi đảng là service thuần: nó nhận <c>DateOnly today</c> qua tham số,
/// không tiêm giao diện này. Tầng trên lấy <see cref="Today"/> rồi truyền xuống.
/// </remarks>
public interface IDateTimeProvider
{
    /// <summary>
    /// Thời điểm hiện tại đã quy về múi giờ Asia/Ho_Chi_Minh (UTC+07:00),
    /// không phụ thuộc múi giờ mặc định của tiến trình (T-FIX-2).
    /// </summary>
    DateTimeOffset Now { get; }

    /// <summary>
    /// Ngày hôm nay theo múi giờ Asia/Ho_Chi_Minh.
    /// </summary>
    DateOnly Today { get; }
}
