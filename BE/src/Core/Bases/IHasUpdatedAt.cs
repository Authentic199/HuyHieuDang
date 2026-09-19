namespace HuyHieuDang.Core.Bases;

/// <summary>
/// Thực thể có ghi lại lần sửa gần nhất.
/// </summary>
/// <remarks>
/// Giá trị <see cref="UpdatedAt"/> do tầng lưu trữ đóng dấu lúc lưu, lấy từ
/// <see cref="Common.Interfaces.IDateTimeProvider"/>; không nơi nào tự gán. Nhờ vậy dấu thời gian
/// luôn đúng cả khi thêm lẫn khi sửa, và kiểm thử đóng băng được thời gian (T-FIX-1).
/// </remarks>
public interface IHasUpdatedAt
{
    /// <summary>
    /// Lần sửa gần nhất.
    /// </summary>
    DateTimeOffset UpdatedAt { get; set; }
}
