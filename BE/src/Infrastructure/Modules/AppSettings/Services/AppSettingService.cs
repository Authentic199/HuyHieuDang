using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Requests;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Responses;
using Microsoft.EntityFrameworkCore;

namespace HuyHieuDang.Infrastructure.Modules.AppSettings.Services;

/// <summary>
/// Nghiệp vụ cài đặt toàn hệ thống (UC-50, UC-51): đọc, lưu ba mốc và tên đơn vị, khôi phục mặc
/// định 30 / 90 / 5 và xem trước dãy mốc. Công thức QT1 nằm nguyên ở
/// <see cref="IPartyMilestoneCalculator"/> của T07; lớp này chỉ đọc/ghi bản ghi rồi gọi lại.
/// Không có lớp đệm nào ở đây: đổi cài đặt xong là mọi danh sách đủ điều kiện đổi theo ngay (QT5).
/// </summary>
public interface IAppSettingService : IScopedService
{
    /// <summary>Đọc cài đặt kèm dãy mốc đang có hiệu lực (UC-50, UC-51).</summary>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Cài đặt và dãy mốc.</returns>
    Task<AppSettingResponse> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Lưu ba mốc và tên đơn vị (UC-50, UC-51).</summary>
    /// <param name="request">Giá trị mới, gửi đủ ba mốc.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Cài đặt sau khi lưu.</returns>
    Task<AppSettingResponse> UpdateAsync(
        UpdateAppSettingRequest request, CancellationToken cancellationToken = default);

    /// <summary>Khôi phục ba mốc về 30 / 90 / 5, giữ nguyên tên đơn vị (UC-50).</summary>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Cài đặt sau khi khôi phục.</returns>
    Task<AppSettingResponse> RestoreDefaultsAsync(CancellationToken cancellationToken = default);

    /// <summary>Xem trước dãy mốc trước khi bấm Lưu (UC-50). Không ghi gì xuống cơ sở dữ liệu.</summary>
    /// <param name="request">Ba tham số, bỏ trống thì lấy giá trị đang lưu.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Dãy mốc và số mốc.</returns>
    Task<MilestonePreviewResponse> PreviewMilestonesAsync(
        MilestonePreviewRequest request, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IAppSettingService"/>
public class AppSettingService : IAppSettingService
{
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IPartyMilestoneCalculator milestoneCalculator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSettingService"/> class.
    /// </summary>
    /// <param name="repositoryWrapper">Cổng truy cập dữ liệu.</param>
    /// <param name="milestoneCalculator">Service thuần tính mốc tuổi đảng (T07).</param>
    public AppSettingService(
        IRepositoryWrapper repositoryWrapper, IPartyMilestoneCalculator milestoneCalculator)
    {
        this.repositoryWrapper = repositoryWrapper;
        this.milestoneCalculator = milestoneCalculator;
    }

    /// <inheritdoc/>
    public async Task<AppSettingResponse> GetAsync(CancellationToken cancellationToken = default)
        => Project(await FindAsync(isAsNoTracking: true, cancellationToken));

    /// <inheritdoc/>
    public async Task<AppSettingResponse> UpdateAsync(
        UpdateAppSettingRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Chạy lại bảng luật ngay cả khi FluentValidation đã chạy: service còn được gọi từ nơi khác
        // và cả hai đường phải ném cùng một khóa lỗi.
        MilestoneSettings settings =
            AppSettingRules.ToSettings(request.StartYears, request.EndYears, request.StepYears);

        return await SaveAsync(settings, Normalize(request.UnitName), keepUnitName: false, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<AppSettingResponse> RestoreDefaultsAsync(CancellationToken cancellationToken = default)
        => await SaveAsync(
            new MilestoneSettings(
                AppSetting.DefaultStartYears, AppSetting.DefaultEndYears, AppSetting.DefaultStepYears),
            unitName: null,
            keepUnitName: true,
            cancellationToken);

    /// <inheritdoc/>
    public async Task<MilestonePreviewResponse> PreviewMilestonesAsync(
        MilestonePreviewRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        AppSetting? stored = await FindAsync(isAsNoTracking: true, cancellationToken);

        MilestoneSettings settings = AppSettingRules.ToSettings(
            request.Start ?? stored?.StartYears ?? AppSetting.DefaultStartYears,
            request.End ?? stored?.EndYears ?? AppSetting.DefaultEndYears,
            request.Step ?? stored?.StepYears ?? AppSetting.DefaultStepYears);

        IReadOnlyList<int> milestones = milestoneCalculator.BuildMilestones(settings);

        return new MilestonePreviewResponse { Milestones = milestones, MilestoneCount = milestones.Count };
    }

    /// <summary>
    /// Bỏ khoảng trắng thừa của tên đơn vị; chuỗi rỗng và chuỗi toàn khoảng trắng đều thành
    /// <see langword="null"/> để "chưa đặt" chỉ có một cách biểu diễn.
    /// </summary>
    /// <param name="unitName">Giá trị người dùng nhập.</param>
    /// <returns>Tên đơn vị đã chuẩn hóa.</returns>
    private static string? Normalize(string? unitName)
        => string.IsNullOrWhiteSpace(unitName) ? null : unitName.Trim();

    /// <summary>
    /// Ghi ba mốc xuống bản ghi cài đặt duy nhất. Có bản ghi thì sửa, chưa có thì tạo đúng một bản
    /// ghi — gọi bao nhiêu lần bảng vẫn chỉ có một dòng (A-509).
    /// </summary>
    /// <param name="settings">Ba mốc đã hợp lệ.</param>
    /// <param name="unitName">Tên đơn vị mới; bỏ qua khi <paramref name="keepUnitName"/> bật.</param>
    /// <param name="keepUnitName">Giữ nguyên tên đơn vị đang lưu (khôi phục mặc định, mục 7.3).</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Cài đặt sau khi ghi.</returns>
    private async Task<AppSettingResponse> SaveAsync(
        MilestoneSettings settings, string? unitName, bool keepUnitName, CancellationToken cancellationToken)
    {
        AppSetting? found = await FindAsync(isAsNoTracking: false, cancellationToken);
        bool isNew = found is null;
        AppSetting entity = found ?? new AppSetting();

        entity.StartYears = settings.StartYears;
        entity.EndYears = settings.EndYears;
        entity.StepYears = settings.StepYears;

        if (!keepUnitName)
        {
            entity.UnitName = unitName;
        }

        // Không gán UpdatedAt ở đây: UpdatedAtInterceptor đóng dấu ở tầng lưu cho mọi module (T-FIX-1).
        if (isNew)
        {
            await repositoryWrapper.Repository<AppSetting>().AddAsync(entity, cancellationToken);
        }
        else
        {
            await repositoryWrapper.Repository<AppSetting>().UpdateAsync(entity, cancellationToken);
        }

        return Project(entity);
    }

    /// <summary>
    /// Bản ghi cài đặt duy nhất của hệ thống, hoặc <see langword="null"/> khi kho còn trống.
    /// </summary>
    /// <param name="isAsNoTracking">Chỉ đọc hay đọc để sửa.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Bản ghi cài đặt.</returns>
    private async Task<AppSetting?> FindAsync(bool isAsNoTracking, CancellationToken cancellationToken)
        => await repositoryWrapper.Repository<AppSetting>()
            .Find(isAsNoTracking: isAsNoTracking)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Đổi bản ghi sang hình dạng của mục 7.1. Kho còn trống thì trả mặc định 30 / 90 / 5 mà
    /// không tạo bản ghi nào (A-501).
    /// </summary>
    /// <param name="entity">Bản ghi cài đặt, có thể chưa tồn tại.</param>
    /// <returns>Phản hồi đúng hợp đồng API.</returns>
    private AppSettingResponse Project(AppSetting? entity)
    {
        MilestoneSettings settings = new(
            entity?.StartYears ?? AppSetting.DefaultStartYears,
            entity?.EndYears ?? AppSetting.DefaultEndYears,
            entity?.StepYears ?? AppSetting.DefaultStepYears);

        IReadOnlyList<int> milestones = milestoneCalculator.BuildMilestones(settings);

        return new AppSettingResponse
        {
            StartYears = settings.StartYears,
            EndYears = settings.EndYears,
            StepYears = settings.StepYears,
            UnitName = entity?.UnitName,
            Milestones = milestones,
            MilestoneCount = milestones.Count,
            UpdatedAt = entity?.UpdatedAt ?? default,
        };
    }
}
