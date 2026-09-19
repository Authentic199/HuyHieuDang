using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Responses;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Services;
using HuyHieuDang.Infrastructure.Modules.Eligibility.Requests;
using HuyHieuDang.Infrastructure.Modules.Eligibility.Responses;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using Microsoft.EntityFrameworkCore;
using CorePeriod = HuyHieuDang.Core.PartyBadges.AwardPeriod;
using PeriodEntity = HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities.AwardPeriod;

namespace HuyHieuDang.Infrastructure.Modules.Eligibility.Services;

/// <summary>
/// Nhóm tính toán: Dashboard, danh sách đủ điều kiện theo đợt và năm, danh sách chưa thuộc đợt nào
/// (UC-10 → UC-13, UC-34, UC-40). Không endpoint nào ở đây ghi dữ liệu — mọi danh sách được tính
/// lại từ đầu ở mỗi lời gọi (QT5). Toàn bộ luật ngày tháng nằm ở
/// <see cref="IPartyMilestoneCalculator"/> của T07; lớp này chỉ đọc dữ liệu, gọi luật rồi đổi
/// sang hình dạng của hợp đồng API.
/// </summary>
public interface IEligibilityService : IScopedService
{
    /// <summary>Danh sách đủ điều kiện của một đợt trong một năm (UC-34, QT4).</summary>
    /// <param name="request">Id đợt và năm đang xét.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đợt đã gắn năm, tổng số, phân bổ theo mốc và danh sách.</returns>
    Task<EligibilityListResponse> SearchAsync(
        EligibilityQueryRequest request, CancellationToken cancellationToken = default);

    /// <summary>Danh sách chưa thuộc đợt nào của một năm (UC-40, QT7).</summary>
    /// <param name="request">Năm đang xét.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Tổng số và danh sách kèm khoảng trống của từng người.</returns>
    Task<UnassignedListResponse> SearchUnassignedAsync(
        EligibilityYearQueryRequest request, CancellationToken cancellationToken = default);

    /// <summary>Số người chưa thuộc đợt nào, dành cho badge menu trái (QT7).</summary>
    /// <param name="request">Năm đang xét.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Năm và số người bị sót.</returns>
    Task<UnassignedCountResponse> CountUnassignedAsync(
        EligibilityYearQueryRequest request, CancellationToken cancellationToken = default);

    /// <summary>Toàn bộ dữ liệu Dashboard trong một lời gọi (UC-10 → UC-13, QT8).</summary>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đợt sắp tới, danh sách đi kèm và các cảnh báo.</returns>
    Task<DashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IEligibilityService"/>
public class EligibilityService : IEligibilityService
{
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IPartyMilestoneCalculator milestoneCalculator;
    private readonly IDateTimeProvider dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="EligibilityService"/> class.
    /// </summary>
    /// <param name="repositoryWrapper">Cổng truy cập dữ liệu.</param>
    /// <param name="milestoneCalculator">Service thuần tính mốc tuổi đảng (T07).</param>
    /// <param name="dateTimeProvider">Nguồn thời gian của hệ thống (T08).</param>
    public EligibilityService(
        IRepositoryWrapper repositoryWrapper,
        IPartyMilestoneCalculator milestoneCalculator,
        IDateTimeProvider dateTimeProvider)
    {
        this.repositoryWrapper = repositoryWrapper;
        this.milestoneCalculator = milestoneCalculator;
        this.dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc/>
    public async Task<EligibilityListResponse> SearchAsync(
        EligibilityQueryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        EligibilityDataset dataset = await LoadAsync(cancellationToken);
        int year = request.ResolveYear(dataset.Today);

        PeriodEntity entity = dataset.Periods.FirstOrDefault(x => x.Id == request.AwardPeriodId)
            ?? throw new BadRequestException(Messages<PeriodEntity>.NotFound());

        CorePeriod period = AwardPeriodCoverageBuilder.ToCorePeriod(entity);
        IReadOnlyList<EligibleMemberResponse> members = BuildEligibleMembers(dataset, period, year);

        return new EligibilityListResponse
        {
            AwardPeriod = AwardPeriodResponse.From(
                entity,
                milestoneCalculator.BindToYear(period, year),
                milestoneCalculator.GetPeriodStatus(period, year, dataset.Today),
                members.Count),
            Year = year,
            TotalCount = members.Count,
            MilestoneBreakdown = BuildBreakdown(members),
            Members = members,
        };
    }

    /// <inheritdoc/>
    public async Task<UnassignedListResponse> SearchUnassignedAsync(
        EligibilityYearQueryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        EligibilityDataset dataset = await LoadAsync(cancellationToken);
        int year = request.ResolveYear(dataset.Today);
        IReadOnlyList<UnassignedMemberResponse> members = BuildUnassignedMembers(dataset, year);

        return new UnassignedListResponse
        {
            Year = year,
            TotalCount = members.Count,
            Members = members,
        };
    }

    /// <inheritdoc/>
    public async Task<UnassignedCountResponse> CountUnassignedAsync(
        EligibilityYearQueryRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        EligibilityDataset dataset = await LoadAsync(cancellationToken);
        int year = request.ResolveYear(dataset.Today);

        return new UnassignedCountResponse
        {
            Year = year,

            // Cùng một phép tính với danh sách của mục 6.3 nên hai con số không thể lệch nhau.
            Count = CountMissed(dataset, year),
        };
    }

    /// <inheritdoc/>
    public async Task<DashboardResponse> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        EligibilityDataset dataset = await LoadAsync(cancellationToken);
        int currentYear = dataset.Today.Year;

        UpcomingPeriod? upcoming = milestoneCalculator.GetUpcomingPeriod(dataset.CorePeriods, dataset.Today);
        IReadOnlyList<EligibleMemberResponse> members = upcoming is null
            ? Array.Empty<EligibleMemberResponse>()
            : BuildEligibleMembers(dataset, upcoming.Occurrence.Period, upcoming.Occurrence.Year);

        CoverageWarningsResponse coverage =
            AwardPeriodCoverageBuilder.BuildWarnings(milestoneCalculator, dataset.Periods, currentYear);

        return new DashboardResponse
        {
            Today = dataset.Today,
            CurrentYear = currentYear,
            UnitName = dataset.UnitName,
            MemberCount = dataset.Members.Count,
            PeriodCount = dataset.Periods.Count,
            UpcomingPeriod = upcoming is null ? null : BuildUpcoming(dataset, upcoming, members),
            EligibleMembers = members,
            Warnings = new DashboardWarningsResponse
            {
                NoMembers = dataset.Members.Count == 0,
                NoPeriods = dataset.Periods.Count == 0,
                UnassignedYear = currentYear,
                UnassignedCount = CountMissed(dataset, currentYear),
                Overlaps = coverage.Overlaps,
                Gaps = coverage.Gaps,
            },
        };
    }

    /// <summary>
    /// Phân bổ theo mốc huy hiệu: chỉ liệt kê mốc có người, sắp theo mốc tăng dần.
    /// </summary>
    /// <param name="members">Danh sách đã tính.</param>
    /// <returns>Các dòng phân bổ.</returns>
    private static IReadOnlyList<MilestoneBreakdownResponse> BuildBreakdown(
        IReadOnlyList<EligibleMemberResponse> members)
        => members
            .GroupBy(x => x.Milestone)
            .OrderBy(x => x.Key)
            .Select(x => new MilestoneBreakdownResponse { Milestone = x.Key, Count = x.Count() })
            .ToList();

    private static UpcomingPeriodResponse BuildUpcoming(
        EligibilityDataset dataset, UpcomingPeriod upcoming, IReadOnlyList<EligibleMemberResponse> members)
    {
        PeriodOccurrence occurrence = upcoming.Occurrence;
        PeriodEntity entity = dataset.FindByName(occurrence.Period.Name);

        return new UpcomingPeriodResponse
        {
            Id = entity.Id,
            Name = entity.Name,
            Year = occurrence.Year,
            IsNextYear = occurrence.Year > dataset.Today.Year,
            FromDate = occurrence.From,
            ToDate = occurrence.To,
            FromDisplay = AwardPeriodResponse.Display(entity.FromDay, entity.FromMonth),
            ToDisplay = AwardPeriodResponse.Display(entity.ToDay, entity.ToMonth),
            Status = upcoming.Status.Status,
            DaysRemaining = upcoming.Status.DaysLeft,
            EligibleCount = members.Count,
            MilestoneBreakdown = BuildBreakdown(members),
        };
    }

    private static UnassignedGapType ToGapType(GapKind kind) => kind switch
    {
        GapKind.BeforeFirstPeriod => UnassignedGapType.BeforeFirst,
        GapKind.AfterLastPeriod => UnassignedGapType.AfterLast,
        _ => UnassignedGapType.Between,
    };

    /// <summary>
    /// Danh sách đủ điều kiện của một đợt trong một năm (QT4), đã sắp theo quy ước chung.
    /// </summary>
    /// <param name="dataset">Dữ liệu đọc một lần cho cả lời gọi.</param>
    /// <param name="period">Đợt thuần dùng cho service tính toán.</param>
    /// <param name="year">Năm đang xét.</param>
    /// <returns>Danh sách đã sắp xếp.</returns>
    private IReadOnlyList<EligibleMemberResponse> BuildEligibleMembers(
        EligibilityDataset dataset, CorePeriod period, int year)
    {
        List<EligibleMemberResponse> rows = new();

        foreach (PartyMember member in dataset.Members)
        {
            int? milestone = milestoneCalculator.GetEligibleMilestone(
                member.OfficialAdmissionDate, period, year, dataset.Milestones);

            if (milestone is null)
            {
                continue;
            }

            rows.Add(new EligibleMemberResponse
            {
                PartyMemberId = member.Id,
                FullName = member.FullName,
                Gender = member.Gender,
                DateOfBirth = member.DateOfBirth,
                OfficialAdmissionDate = member.OfficialAdmissionDate,
                Milestone = milestone.Value,
                MilestoneDate = milestoneCalculator.GetAnniversary(member.OfficialAdmissionDate, milestone.Value),
            });
        }

        return EligibilitySorting.Order(rows).ToList();
    }

    /// <summary>
    /// Danh sách chưa thuộc đợt nào của một năm (QT7), kèm khoảng trống chứa ngày tròn mốc.
    /// </summary>
    /// <param name="dataset">Dữ liệu đọc một lần cho cả lời gọi.</param>
    /// <param name="year">Năm đang xét.</param>
    /// <returns>Danh sách đã sắp xếp.</returns>
    private IReadOnlyList<UnassignedMemberResponse> BuildUnassignedMembers(EligibilityDataset dataset, int year)
    {
        IReadOnlyList<PeriodOccurrence> ordered = dataset.CorePeriods
            .Select(period => milestoneCalculator.BindToYear(period, year))
            .OrderBy(x => x.From)
            .ThenBy(x => x.To)
            .ToList();

        List<UnassignedMemberResponse> rows = new();

        foreach (PartyMember member in dataset.Members)
        {
            MissedMilestone? missed = milestoneCalculator.GetMissedMilestone(
                member.OfficialAdmissionDate, dataset.CorePeriods, year, dataset.Milestones);

            if (missed is null)
            {
                continue;
            }

            rows.Add(new UnassignedMemberResponse
            {
                PartyMemberId = member.Id,
                FullName = member.FullName,
                Gender = member.Gender,
                DateOfBirth = member.DateOfBirth,
                OfficialAdmissionDate = member.OfficialAdmissionDate,
                Milestone = missed.Milestone,
                MilestoneDate = missed.Anniversary,
                Gap = BuildGap(missed.Gap, ordered),
            });
        }

        return EligibilitySorting.Order(rows).ToList();
    }

    /// <summary>
    /// Đếm số người bị sót mà không dựng danh sách — dùng cho badge và cho khối cảnh báo.
    /// </summary>
    /// <param name="dataset">Dữ liệu đọc một lần cho cả lời gọi.</param>
    /// <param name="year">Năm đang xét.</param>
    /// <returns>Số người bị sót.</returns>
    private int CountMissed(EligibilityDataset dataset, int year)
        => dataset.Members.Count(member => milestoneCalculator.GetMissedMilestone(
            member.OfficialAdmissionDate, dataset.CorePeriods, year, dataset.Milestones) is not null);

    /// <summary>
    /// Gắn tên hai đợt kề hai bên vào khoảng trống, đúng khuôn chữ của cột "Khoảng trống".
    /// </summary>
    /// <param name="gap">Khoảng trống do T07 tính.</param>
    /// <param name="ordered">Các lần diễn ra trong năm, đã sắp theo ngày bắt đầu.</param>
    /// <returns>Khoảng trống đúng hình dạng hợp đồng API.</returns>
    private static UnassignedGapResponse BuildGap(DateGap gap, IReadOnlyList<PeriodOccurrence> ordered)
        => new()
        {
            Type = ToGapType(gap.Kind),
            PreviousPeriodName = gap.Kind == GapKind.BeforeFirstPeriod
                ? null
                : ordered.Where(x => x.To < gap.From).MaxBy(x => x.To)?.Period.Name,
            NextPeriodName = gap.Kind == GapKind.AfterLastPeriod
                ? null
                : ordered.FirstOrDefault(x => x.From > gap.To)?.Period.Name,
        };

    /// <summary>
    /// Đọc một lần toàn bộ dữ liệu cần cho một lời gọi: cài đặt mốc, các đợt và các đảng viên.
    /// Ba câu truy vấn cố định, không phụ thuộc số đợt hay số đảng viên.
    /// </summary>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Ngữ cảnh dùng chung cho mọi phép tính của lời gọi.</returns>
    private async Task<EligibilityDataset> LoadAsync(CancellationToken cancellationToken)
    {
        AppSetting? setting = await repositoryWrapper.Repository<AppSetting>()
            .Find(isAsNoTracking: true)
            .FirstOrDefaultAsync(cancellationToken);

        MilestoneSettings settings = new(
            setting?.StartYears ?? AppSetting.DefaultStartYears,
            setting?.EndYears ?? AppSetting.DefaultEndYears,
            setting?.StepYears ?? AppSetting.DefaultStepYears);

        List<PeriodEntity> periods = await repositoryWrapper.Repository<PeriodEntity>()
            .Find(isAsNoTracking: true)
            .ToListAsync(cancellationToken);

        List<PartyMember> members = await repositoryWrapper.Repository<PartyMember>()
            .Find(isAsNoTracking: true)
            .ToListAsync(cancellationToken);

        return new EligibilityDataset(
            dateTimeProvider.Today,
            milestoneCalculator.BuildMilestones(settings),
            setting?.UnitName,
            periods,
            members);
    }

    /// <summary>
    /// Dữ liệu đọc một lần, dùng chung cho mọi phép tính của một lời gọi.
    /// </summary>
    /// <param name="Today">Hôm nay theo lịch máy chủ.</param>
    /// <param name="Milestones">Dãy mốc huy hiệu tăng dần (QT1).</param>
    /// <param name="UnitName">Tên đơn vị trong cài đặt.</param>
    /// <param name="Periods">Toàn bộ đợt đang cấu hình.</param>
    /// <param name="Members">Toàn bộ đảng viên.</param>
    private sealed record EligibilityDataset(
        DateOnly Today,
        IReadOnlyList<int> Milestones,
        string? UnitName,
        IReadOnlyList<PeriodEntity> Periods,
        IReadOnlyList<PartyMember> Members)
    {
        /// <summary>
        /// Các đợt dưới dạng bản ghi thuần của service tính toán, dựng một lần cho cả lời gọi.
        /// </summary>
        public IReadOnlyList<CorePeriod> CorePeriods { get; }
            = Periods.Select(AwardPeriodCoverageBuilder.ToCorePeriod).ToList();

        /// <summary>
        /// Tra ngược bản ghi đợt từ tên — tên đợt là duy nhất không phân biệt hoa thường.
        /// </summary>
        /// <param name="name">Tên đợt.</param>
        /// <returns>Bản ghi tương ứng.</returns>
        public PeriodEntity FindByName(string name)
            => Periods.First(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
    }
}
