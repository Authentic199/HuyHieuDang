using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;

namespace HuyHieuDang.Infrastructure.Modules.AppSettings.Requests;

/// <summary>
/// Bảng luật của ba mốc QT1, dùng chung cho <c>PUT /api/Settings</c> và cho ô xem trước
/// <c>GET /api/Settings/Milestones</c>. Xem trước nhận tham số rời và có giá trị mặc định lấy từ
/// cài đặt đang lưu, nên không dùng FluentValidation được — cả hai đường vẫn phải trả về đúng một
/// bộ khóa lỗi, vì thế luật nằm ở đây chứ không chép hai lần.
/// </summary>
public static class AppSettingRules
{
    /// <summary>
    /// Trần của cả ba mốc. Một đảng viên không thể có tuổi đảng quá con số này, nên đây vừa là ràng
    /// buộc nghiệp vỹ vừa là trần chặn dãy mốc phình to: <c>start=1&amp;end=1000000&amp;step=1</c> từng làm
    /// ô xem trước trả gần 7 MB JSON sau mỗi lần gõ phím.
    /// </summary>
    public const int MaxYears = 200;

    /// <summary>
    /// Mốc bắt đầu và mốc kết thúc phải là số nguyên dương, không vượt <see cref="MaxYears"/>.
    /// </summary>
    /// <param name="value">Giá trị cần kiểm tra.</param>
    /// <returns><see langword="true"/> khi giá trị có mặt và nằm trong 1 → <see cref="MaxYears"/>.</returns>
    public static bool IsPositive(int? value) => value is >= 1 and <= MaxYears;

    /// <summary>
    /// Bước nhảy phải từ 1 trở lên, không vượt <see cref="MaxYears"/>.
    /// </summary>
    /// <param name="value">Giá trị cần kiểm tra.</param>
    /// <returns><see langword="true"/> khi giá trị có mặt và nằm trong 1 → <see cref="MaxYears"/>.</returns>
    public static bool IsPositiveStep(int? value) => value is >= 1 and <= MaxYears;

    /// <summary>
    /// Kiểm tra ba mốc và dựng <see cref="MilestoneSettings"/> cho service tính toán của T07.
    /// Ném <see cref="BadRequestException"/> mang đúng khóa thông điệp của mục 7.2 hợp đồng API.
    /// </summary>
    /// <param name="startYears">Mốc bắt đầu.</param>
    /// <param name="endYears">Mốc kết thúc.</param>
    /// <param name="stepYears">Bước nhảy.</param>
    /// <returns>Cài đặt mốc đã hợp lệ.</returns>
    public static MilestoneSettings ToSettings(int? startYears, int? endYears, int? stepYears)
    {
        if (!IsPositive(startYears))
        {
            throw new BadRequestException(Messages<AppSetting>.Invalid(x => x.StartYears));
        }

        if (!IsPositive(endYears))
        {
            throw new BadRequestException(Messages<AppSetting>.Invalid(x => x.EndYears));
        }

        if (!IsPositiveStep(stepYears))
        {
            throw new BadRequestException(Messages<AppSetting>.Invalid(x => x.StepYears));
        }

        if (startYears > endYears)
        {
            throw new BadRequestException(Messages<AppSetting>.Invalid(AppSettingMessageProperties.Range));
        }

        return new MilestoneSettings(startYears!.Value, endYears!.Value, stepYears!.Value);
    }
}
