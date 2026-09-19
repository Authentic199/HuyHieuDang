using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Responses;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Services;
using HuyHieuDang.Infrastructure.Modules.Eligibility.Requests;
using HuyHieuDang.Infrastructure.Modules.Eligibility.Responses;
using HuyHieuDang.Infrastructure.Modules.Eligibility.Services;
using HuyHieuDang.Infrastructure.Modules.Exports.Responses;

namespace HuyHieuDang.Infrastructure.Modules.Exports.Services;

/// <summary>
/// Xuất Excel ba loại (mục 8 hợp đồng API, UC-11, UC-34, UC-40). Lớp này không tự truy vấn và
/// không lặp lại công thức nào: danh sách lấy nguyên từ <see cref="IEligibilityService"/> nên file
/// xuất ra luôn khớp từng dòng, từng thứ tự với bảng đang xem trên màn hình (QT5, mục 1.8).
/// </summary>
public interface IExportService : IScopedService
{
    /// <summary>Xuất danh sách đủ điều kiện của một đợt trong một năm (mục 8.1, UC-34).</summary>
    /// <param name="request">Id đợt và năm đang xét.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Tên file và nội dung.</returns>
    Task<ExportFileResponse> ExportEligibilityAsync(
        EligibilityQueryRequest request, CancellationToken cancellationToken = default);

    /// <summary>Xuất đúng danh sách đang hiện trên Dashboard, tức đợt sắp tới theo QT8 (mục 8.2, UC-11).</summary>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Tên file và nội dung.</returns>
    Task<ExportFileResponse> ExportDashboardAsync(CancellationToken cancellationToken = default);

    /// <summary>Xuất danh sách chưa thuộc đợt nào của một năm (mục 8.3, UC-40).</summary>
    /// <param name="request">Năm đang xét.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Tên file và nội dung.</returns>
    Task<ExportFileResponse> ExportUnassignedAsync(
        EligibilityYearQueryRequest request, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IExportService"/>
public class ExportService : IExportService
{
    private readonly IEligibilityService eligibilityService;
    private readonly IAppSettingService appSettingService;
    private readonly IDateTimeProvider dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportService"/> class.
    /// </summary>
    /// <param name="eligibilityService">Nguồn duy nhất của mọi danh sách đưa vào file.</param>
    /// <param name="appSettingService">Nguồn tên đơn vị cho dòng tiêu đề đầu tiên.</param>
    /// <param name="dateTimeProvider">Nguồn thời gian của hệ thống, dùng cho dòng "Ngày xuất".</param>
    public ExportService(
        IEligibilityService eligibilityService,
        IAppSettingService appSettingService,
        IDateTimeProvider dateTimeProvider)
    {
        this.eligibilityService = eligibilityService;
        this.appSettingService = appSettingService;
        this.dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc/>
    public async Task<ExportFileResponse> ExportEligibilityAsync(
        EligibilityQueryRequest request, CancellationToken cancellationToken = default)
    {
        // Đợt lạ hay năm sai đã bị chặn ngay tại đây bằng chính lỗi của mục 6.2.
        EligibilityListResponse list = await eligibilityService.SearchAsync(request, cancellationToken);

        return await BuildEligibilityFileAsync(
            list.AwardPeriod.Name,
            list.Year,
            list.AwardPeriod.FromDate,
            list.AwardPeriod.ToDate,
            list.Members,
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ExportFileResponse> ExportDashboardAsync(CancellationToken cancellationToken = default)
    {
        DashboardResponse dashboard = await eligibilityService.GetDashboardAsync(cancellationToken);

        // Chưa cài đợt nào thì Dashboard không có danh sách để xuất (mục 8.2).
        UpcomingPeriodResponse upcoming = dashboard.UpcomingPeriod
            ?? throw new BadRequestException(Messages<DashboardResponse>.NotFound(nameof(DashboardResponse.UpcomingPeriod)));

        return await BuildEligibilityFileAsync(
            upcoming.Name,
            upcoming.Year,
            upcoming.FromDate,
            upcoming.ToDate,
            dashboard.EligibleMembers,
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ExportFileResponse> ExportUnassignedAsync(
        EligibilityYearQueryRequest request, CancellationToken cancellationToken = default)
    {
        UnassignedListResponse list = await eligibilityService.SearchUnassignedAsync(request, cancellationToken);
        string? unitName = await GetUnitNameAsync(cancellationToken);

        ExportHeader header = ExportHeader.ForUnassigned(unitName, list.Year, dateTimeProvider.Today);

        return new ExportFileResponse(
            ExportFileName.ForUnassigned(list.Year),
            ExportSheet.BuildUnassigned(header, list.Members));
    }

    /// <summary>
    /// Phần chung của mục 8.1 và 8.2: hai loại file này chỉ khác nhau ở chỗ lấy danh sách, còn
    /// tên file, dòng tiêu đề và bảy cột thì giống hệt.
    /// </summary>
    /// <param name="periodName">Tên đợt.</param>
    /// <param name="year">Năm của lần diễn ra đang xuất.</param>
    /// <param name="fromDate">Từ ngày đã gắn năm.</param>
    /// <param name="toDate">Đến ngày đã gắn năm.</param>
    /// <param name="members">Danh sách đã sắp sẵn.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Tên file và nội dung.</returns>
    private async Task<ExportFileResponse> BuildEligibilityFileAsync(
        string periodName,
        int year,
        DateOnly fromDate,
        DateOnly toDate,
        IReadOnlyList<EligibleMemberResponse> members,
        CancellationToken cancellationToken)
    {
        string? unitName = await GetUnitNameAsync(cancellationToken);

        ExportHeader header = ExportHeader.ForEligibility(
            unitName, periodName, fromDate, toDate, dateTimeProvider.Today);

        return new ExportFileResponse(
            ExportFileName.ForEligibility(periodName, year),
            ExportSheet.BuildEligibility(header, members));
    }

    /// <summary>
    /// Tên đơn vị trong cài đặt (T13); rỗng thì dòng 1 của file bị bỏ hẳn.
    /// </summary>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Tên đơn vị, hoặc <see langword="null"/> khi chưa đặt.</returns>
    private async Task<string?> GetUnitNameAsync(CancellationToken cancellationToken)
    {
        AppSettingResponse setting = await appSettingService.GetAsync(cancellationToken);

        return setting.UnitName;
    }
}
