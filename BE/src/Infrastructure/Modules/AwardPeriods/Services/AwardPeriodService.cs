using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Requests;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Responses;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using Microsoft.EntityFrameworkCore;
using CorePeriod = HuyHieuDang.Core.PartyBadges.AwardPeriod;
using PeriodEntity = HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities.AwardPeriod;

namespace HuyHieuDang.Infrastructure.Modules.AwardPeriods.Services;

/// <summary>
/// Nghiệp vụ đợt trao huy hiệu: danh sách kèm trạng thái và cảnh báo, lấy một, thêm, sửa, xóa
/// (UC-30 → UC-33, UC-36).
/// </summary>
public interface IAwardPeriodService : IScopedService
{
    /// <summary>Danh sách đợt kèm trạng thái, số người đủ điều kiện, cảnh báo và dải độ phủ (UC-30).</summary>
    /// <param name="request">Tham số truy vấn, chủ yếu là năm đang xét.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đủ dữ liệu cho bảng, dải độ phủ và banner cảnh báo.</returns>
    Task<AwardPeriodListResponse> SearchAsync(
        AwardPeriodQueryRequest request, CancellationToken cancellationToken = default);

    /// <summary>Lấy một đợt theo id (UC-34 tab Thông tin).</summary>
    /// <param name="id">Id đợt.</param>
    /// <param name="request">Tham số truy vấn, chủ yếu là năm đang xét.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đợt đã gắn năm.</returns>
    Task<AwardPeriodResponse> GetByIdAsync(
        Guid id, AwardPeriodQueryRequest request, CancellationToken cancellationToken = default);

    /// <summary>Thêm một đợt (UC-31).</summary>
    /// <param name="request">Dữ liệu đợt.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đợt vừa tạo kèm cảnh báo của năm hiện tại.</returns>
    Task<AwardPeriodMutationResponse> CreateAsync(
        CreateAwardPeriodRequest request, CancellationToken cancellationToken = default);

    /// <summary>Sửa một đợt (UC-32). Có hiệu lực ngay cho mọi năm, kể cả năm hiện tại.</summary>
    /// <param name="id">Id đợt.</param>
    /// <param name="request">Dữ liệu mới, gửi đủ cả năm trường.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đợt sau khi sửa kèm cảnh báo của năm hiện tại.</returns>
    Task<AwardPeriodMutationResponse> UpdateAsync(
        Guid id, UpdateAwardPeriodRequest request, CancellationToken cancellationToken = default);

    /// <summary>Xóa hẳn một đợt (UC-33, QT10). Không đụng tới đảng viên nào.</summary>
    /// <param name="id">Id đợt.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Id vừa xóa kèm cảnh báo mới sinh ra.</returns>
    Task<AwardPeriodDeletedResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IAwardPeriodService"/>
public class AwardPeriodService : IAwardPeriodService
{
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IPartyMilestoneCalculator milestoneCalculator;
    private readonly IDateTimeProvider dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AwardPeriodService"/> class.
    /// </summary>
    /// <param name="repositoryWrapper">Cổng truy cập dữ liệu.</param>
    /// <param name="milestoneCalculator">Service thuần tính mốc tuổi đảng (T07).</param>
    /// <param name="dateTimeProvider">Nguồn thời gian của hệ thống (T08).</param>
    public AwardPeriodService(
        IRepositoryWrapper repositoryWrapper,
        IPartyMilestoneCalculator milestoneCalculator,
        IDateTimeProvider dateTimeProvider)
    {
        this.repositoryWrapper = repositoryWrapper;
        this.milestoneCalculator = milestoneCalculator;
        this.dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc/>
    public async Task<AwardPeriodListResponse> SearchAsync(
        AwardPeriodQueryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        int year = request.Year ?? dateTimeProvider.Today.Year;
        List<PeriodEntity> periods = await LoadPeriodsAsync(cancellationToken);
        EligibilityContext context = await LoadEligibilityContextAsync(cancellationToken);

        return new AwardPeriodListResponse
        {
            Year = year,
            Today = dateTimeProvider.Today,
            TotalCount = periods.Count,
            Periods = periods
                .Select(period => Project(period, year, context))
                .OrderBy(x => x.FromDate)
                .ThenBy(x => x.ToDate)
                .ToList(),
            Warnings = AwardPeriodCoverageBuilder.BuildWarnings(milestoneCalculator, periods, year),
            Coverage = AwardPeriodCoverageBuilder.BuildCoverage(milestoneCalculator, periods, year),
        };
    }

    /// <inheritdoc/>
    public async Task<AwardPeriodResponse> GetByIdAsync(
        Guid id, AwardPeriodQueryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        PeriodEntity entity = await FindOrThrowAsync(id, isAsNoTracking: true, cancellationToken);

        return Project(
            entity,
            request.Year ?? dateTimeProvider.Today.Year,
            await LoadEligibilityContextAsync(cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<AwardPeriodMutationResponse> CreateAsync(
        CreateAwardPeriodRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await ThrowIfNameRepeatedAsync(request.Name!, ignoredId: null, cancellationToken);

        PeriodEntity entity = new();
        Apply(request, entity);

        await repositoryWrapper.Repository<PeriodEntity>().AddAsync(entity, cancellationToken);

        return await BuildMutationResponseAsync(entity, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<AwardPeriodMutationResponse> UpdateAsync(
        Guid id, UpdateAwardPeriodRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        PeriodEntity entity = await FindOrThrowAsync(id, isAsNoTracking: false, cancellationToken);
        await ThrowIfNameRepeatedAsync(request.Name!, ignoredId: id, cancellationToken);
        Apply(request, entity);

        await repositoryWrapper.Repository<PeriodEntity>().UpdateAsync(entity, cancellationToken);

        return await BuildMutationResponseAsync(entity, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<AwardPeriodDeletedResponse> DeleteAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        PeriodEntity entity = await FindOrThrowAsync(id, isAsNoTracking: false, cancellationToken);

        await repositoryWrapper.Repository<PeriodEntity>().DeleteAsync(entity, cancellationToken);

        return new AwardPeriodDeletedResponse
        {
            Id = id,
            Warnings = AwardPeriodCoverageBuilder.BuildWarnings(
                milestoneCalculator, await LoadPeriodsAsync(cancellationToken), dateTimeProvider.Today.Year),
        };
    }

    /// <summary>
    /// Chép năm trường của yêu cầu vào bản ghi. Không gán <c>UpdatedAt</c>:
    /// <c>UpdatedAtInterceptor</c> đóng dấu ở tầng lưu thay cho mọi module (T-FIX-1).
    /// </summary>
    /// <param name="request">Yêu cầu đã qua kiểm tra hợp lệ.</param>
    /// <param name="entity">Bản ghi đích.</param>
    private static void Apply(AwardPeriodRequest request, PeriodEntity entity)
    {
        entity.Name = request.Name!.Trim();
        entity.FromDay = request.FromDay!.Value;
        entity.FromMonth = request.FromMonth!.Value;
        entity.ToDay = request.ToDay!.Value;
        entity.ToMonth = request.ToMonth!.Value;
    }

    /// <summary>
    /// Tính trạng thái (QT11) và số người đủ điều kiện trong năm (QT4) cho một đợt.
    /// Không giá trị nào trong hai giá trị này được lưu xuống bảng (QT5).
    /// </summary>
    /// <param name="entity">Bản ghi đợt.</param>
    /// <param name="year">Năm đang xét.</param>
    /// <param name="context">Hôm nay, dãy mốc và ngày vào Đảng của toàn bộ đảng viên.</param>
    /// <returns>Phản hồi đúng hình dạng hợp đồng API.</returns>
    private AwardPeriodResponse Project(PeriodEntity entity, int year, EligibilityContext context)
    {
        CorePeriod period = AwardPeriodCoverageBuilder.ToCorePeriod(entity);

        return AwardPeriodResponse.From(
            entity,
            milestoneCalculator.BindToYear(period, year),
            milestoneCalculator.GetPeriodStatus(period, year, context.Today),
            context.AdmissionDates.Count(admissionDate =>
                milestoneCalculator.GetEligibleMilestone(admissionDate, period, year, context.Milestones) is not null));
    }

    private async Task<AwardPeriodMutationResponse> BuildMutationResponseAsync(
        PeriodEntity entity, CancellationToken cancellationToken)
    {
        // Cảnh báo luôn tính cho năm hiện tại để giao diện cập nhật banner ngay (mục 5.3).
        int year = dateTimeProvider.Today.Year;

        return new AwardPeriodMutationResponse
        {
            Period = Project(entity, year, await LoadEligibilityContextAsync(cancellationToken)),
            Warnings = AwardPeriodCoverageBuilder.BuildWarnings(
                milestoneCalculator, await LoadPeriodsAsync(cancellationToken), year),
        };
    }

    private async Task<List<PeriodEntity>> LoadPeriodsAsync(CancellationToken cancellationToken)
        => await repositoryWrapper.Repository<PeriodEntity>()
            .Find(isAsNoTracking: true)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Đọc một lần dãy mốc huy hiệu (QT1) và ngày vào Đảng của mọi đảng viên, dùng chung cho
    /// mọi đợt của một lời gọi.
    /// </summary>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Ngữ cảnh đủ để đếm số người đủ điều kiện của từng đợt.</returns>
    private async Task<EligibilityContext> LoadEligibilityContextAsync(CancellationToken cancellationToken)
    {
        AppSetting? setting = await repositoryWrapper.Repository<AppSetting>()
            .Find(isAsNoTracking: true)
            .FirstOrDefaultAsync(cancellationToken);

        MilestoneSettings settings = new(
            setting?.StartYears ?? AppSetting.DefaultStartYears,
            setting?.EndYears ?? AppSetting.DefaultEndYears,
            setting?.StepYears ?? AppSetting.DefaultStepYears);

        List<DateOnly> admissionDates = await repositoryWrapper.Repository<PartyMember>()
            .Find(isAsNoTracking: true)
            .Select(x => x.OfficialAdmissionDate)
            .ToListAsync(cancellationToken);

        return new EligibilityContext(
            dateTimeProvider.Today, milestoneCalculator.BuildMilestones(settings), admissionDates);
    }

    /// <summary>
    /// Tên đợt là duy nhất, so sánh không phân biệt hoa thường; khi sửa thì bỏ qua chính nó.
    /// Cột <c>name</c> có kiểu <c>citext</c> nên phép so sánh bằng ngay trong câu truy vấn đã
    /// không phân biệt hoa thường, đúng bằng ngữ nghĩa của chỉ mục duy nhất trên cột đó.
    /// </summary>
    /// <param name="name">Tên đợt trong yêu cầu.</param>
    /// <param name="ignoredId">Id được bỏ qua khi so trùng.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    private async Task ThrowIfNameRepeatedAsync(string name, Guid? ignoredId, CancellationToken cancellationToken)
    {
        string trimmed = name.Trim();

        bool repeated = await repositoryWrapper.Repository<PeriodEntity>()
            .Find(isAsNoTracking: true)
            .Where(x => ignoredId == null || x.Id != ignoredId)
            .AnyAsync(x => x.Name == trimmed, cancellationToken);

        if (repeated)
        {
            throw new BadRequestException(Messages<PeriodEntity>.Repeated(x => x.Name));
        }
    }

    /// <summary>
    /// Lấy bản ghi hoặc ném lỗi <c>400</c> kèm khóa <c>Mes.AwardPeriod.NotFound</c>
    /// — hợp đồng API không dùng <c>404</c> cho bản ghi không tồn tại.
    /// </summary>
    /// <param name="id">Id đợt.</param>
    /// <param name="isAsNoTracking">Có theo dõi thay đổi hay không.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Bản ghi tìm thấy.</returns>
    private async Task<PeriodEntity> FindOrThrowAsync(
        Guid id, bool isAsNoTracking, CancellationToken cancellationToken)
        => await repositoryWrapper.Repository<PeriodEntity>()
            .Find(x => x.Id == id, isAsNoTracking)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new BadRequestException(Messages<PeriodEntity>.NotFound());

    /// <summary>
    /// Dữ liệu dùng chung cho mọi đợt của một lời gọi khi đếm số người đủ điều kiện (QT4).
    /// </summary>
    /// <param name="Today">Hôm nay theo lịch máy chủ.</param>
    /// <param name="Milestones">Dãy mốc huy hiệu tăng dần.</param>
    /// <param name="AdmissionDates">Ngày vào Đảng chính thức của toàn bộ đảng viên.</param>
    private sealed record EligibilityContext(
        DateOnly Today, IReadOnlyList<int> Milestones, IReadOnlyList<DateOnly> AdmissionDates);
}
