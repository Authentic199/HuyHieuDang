using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Notes;

/// <summary>
/// Quy tắc ghi chú đảng viên (QT12). Đây là chỗ <b>duy nhất</b> biết luật cắt khoảng trắng,
/// giới hạn độ dài và cách đóng ngày ghi; <c>POST /api/PartyMembers</c>,
/// <c>PUT /api/PartyMembers/{id}</c> và <c>PUT /api/PartyMembers/{id}/Note</c> đều gọi vào đây
/// nên ba đường ghi không thể lệch nhau.
/// </summary>
/// <remarks>
/// Lớp thuần: nhận "bây giờ" qua tham số thay vì đọc đồng hồ, để bộ kiểm thử đóng băng được
/// thời gian (ca A-901 chặn mọi lời gọi đồng hồ ngoài lớp cài đặt <c>IDateTimeProvider</c>).
/// </remarks>
public static class PartyMemberNote
{
    /// <summary>
    /// Độ dài tối đa của ghi chú, tính <b>sau khi</b> cắt khoảng trắng hai đầu.
    /// </summary>
    public const int MaxLength = 500;

    /// <summary>
    /// Cắt khoảng trắng hai đầu; rỗng sau khi cắt thì coi như không có ghi chú.
    /// </summary>
    /// <param name="raw">Nội dung người dùng gửi lên.</param>
    /// <returns>Nội dung đã chuẩn hóa, hoặc <see langword="null"/> khi không có ghi chú.</returns>
    public static string? Normalize(string? raw)
        => string.IsNullOrWhiteSpace(raw) ? null : raw.Trim();

    /// <summary>
    /// Ghi chú có nằm trong giới hạn <see cref="MaxLength"/> hay không, đếm sau khi cắt.
    /// </summary>
    /// <param name="raw">Nội dung người dùng gửi lên.</param>
    /// <returns><see langword="true"/> khi hợp lệ.</returns>
    public static bool IsWithinMaxLength(string? raw)
        => Normalize(raw) is not { Length: > MaxLength };

    /// <summary>
    /// Gán ghi chú và ngày ghi theo QT12: chỉ đổi <see cref="PartyMember.NoteUpdatedAt"/> khi
    /// nội dung sau khi cắt <b>khác</b> nội dung đang lưu; xóa ghi chú thì cả hai cùng về
    /// <see langword="null"/>.
    /// </summary>
    /// <param name="entity">Bản ghi đích.</param>
    /// <param name="raw">Nội dung người dùng gửi lên.</param>
    /// <param name="now">Thời điểm hiện tại của máy chủ, lấy từ <c>IDateTimeProvider.Now</c>.</param>
    public static void Apply(PartyMember entity, string? raw, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(entity);

        string? normalized = Normalize(raw);

        if (string.Equals(normalized, entity.Note, StringComparison.Ordinal))
        {
            return;
        }

        entity.Note = normalized;
        entity.NoteUpdatedAt = normalized is null ? null : now;
    }

    /// <summary>
    /// Ràng buộc độ dài dùng chung cho mọi yêu cầu mang ghi chú.
    /// </summary>
    /// <typeparam name="TRequest">Kiểu yêu cầu chứa trường ghi chú.</typeparam>
    /// <param name="ruleBuilder">Bộ dựng luật của FluentValidation.</param>
    /// <returns>Luật đã gắn khóa <c>Mes.PartyMember.OverLength.Note</c>.</returns>
    public static IRuleBuilderOptions<TRequest, string?> NoteRule<TRequest>(
        this IRuleBuilder<TRequest, string?> ruleBuilder)
    {
        ArgumentNullException.ThrowIfNull(ruleBuilder);

        return ruleBuilder
            .Must(IsWithinMaxLength)
            .WithMessage(Messages<PartyMember>.OverLength(nameof(PartyMember.Note)));
    }
}
