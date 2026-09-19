using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Enums;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Responses;

/// <summary>
/// Một đảng viên trên API (mục 1.10 hợp đồng API). Ba trường tuổi đảng là giá trị tính lại
/// mỗi lần gọi, không lưu vào bảng.
/// </summary>
public class PartyMemberResponse
{
    /// <summary>
    /// Khóa chính của đảng viên.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Họ và tên đầy đủ.
    /// </summary>
    public string FullName { get; set; } = default!;

    /// <summary>
    /// Ngày sinh; <see langword="null"/> khi để trống.
    /// </summary>
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Giới tính; <see langword="null"/> khi để trống.
    /// </summary>
    public Gender? Gender { get; set; }

    /// <summary>
    /// Ngày vào Đảng chính thức.
    /// </summary>
    public DateOnly OfficialAdmissionDate { get; set; }

    /// <summary>
    /// Tuổi đảng tính đến hôm nay (QT3).
    /// </summary>
    public int PartyAgeYears { get; set; }

    /// <summary>
    /// Mốc huy hiệu kế tiếp (QT3a); <see langword="null"/> khi đã vượt mốc lớn nhất.
    /// </summary>
    public int? NextMilestone { get; set; }

    /// <summary>
    /// Ngày tròn mốc kế tiếp; <see langword="null"/> cùng lúc với <see cref="NextMilestone"/>.
    /// </summary>
    public DateOnly? NextMilestoneDate { get; set; }

    /// <summary>
    /// Thời điểm tạo bản ghi.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Thời điểm sửa gần nhất.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Dựng phản hồi từ bản ghi và ba giá trị tuổi đảng đã tính sẵn.
    /// </summary>
    /// <param name="entity">Bản ghi đảng viên.</param>
    /// <param name="partyAgeYears">Tuổi đảng hôm nay.</param>
    /// <param name="nextMilestone">Mốc kế tiếp.</param>
    /// <param name="nextMilestoneDate">Ngày tròn mốc kế tiếp.</param>
    /// <returns>Phản hồi đúng hình dạng hợp đồng API.</returns>
    public static PartyMemberResponse From(
        PartyMember entity, int partyAgeYears, int? nextMilestone, DateOnly? nextMilestoneDate)
        => new()
        {
            Id = entity.Id,
            FullName = entity.FullName,
            DateOfBirth = entity.DateOfBirth,
            Gender = entity.Gender,
            OfficialAdmissionDate = entity.OfficialAdmissionDate,
            PartyAgeYears = partyAgeYears,
            NextMilestone = nextMilestone,
            NextMilestoneDate = nextMilestoneDate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
}
