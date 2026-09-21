using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Core.PartyBadges;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Common.Responses;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Requests;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Responses;
using Microsoft.EntityFrameworkCore;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Services;

/// <summary>
/// Nghiệp vụ đảng viên: danh sách, lấy một, thêm, sửa và xóa (UC-20 → UC-23).
/// Xóa chỉ có một đường duy nhất là <see cref="IPartyMemberService.DeleteRangeAsync"/> (T34).
/// </summary>
public interface IPartyMemberService : IScopedService
{
    /// <summary>Danh sách có tìm kiếm, lọc, sắp xếp và phân trang (UC-20).</summary>
    /// <param name="request">Tham số truy vấn.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Một trang đảng viên kèm ba giá trị tuổi đảng tính lại theo hôm nay.</returns>
    Task<PaginationResponse<PartyMemberResponse>> SearchAsync(
        PartyMemberQueryRequest request, CancellationToken cancellationToken = default);

    /// <summary>Lấy một đảng viên theo id (UC-22).</summary>
    /// <param name="id">Id đảng viên.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đảng viên tìm thấy.</returns>
    Task<PartyMemberResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Thêm thủ công một đảng viên (UC-21).</summary>
    /// <param name="request">Dữ liệu đảng viên.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đảng viên vừa tạo.</returns>
    Task<PartyMemberResponse> CreateAsync(
        CreatePartyMemberRequest request, CancellationToken cancellationToken = default);

    /// <summary>Sửa một đảng viên (UC-22).</summary>
    /// <param name="id">Id đảng viên.</param>
    /// <param name="request">Dữ liệu mới, gửi đủ cả bốn trường.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đảng viên sau khi sửa.</returns>
    Task<PartyMemberResponse> UpdateAsync(
        Guid id, UpdatePartyMemberRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa hẳn đảng viên trong một giao dịch (UC-23, QT10). Đường xóa duy nhất: một phần tử
    /// cũng đi qua đây.
    /// </summary>
    /// <param name="request">Danh sách id cần xóa, ít nhất một phần tử.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Đúng những id đã xóa được; id không tồn tại bị bỏ qua.</returns>
    Task<MultipleIdentiferResponse> DeleteRangeAsync(
        DeletePartyMemberRangeRequest request, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IPartyMemberService"/>
public class PartyMemberService : IPartyMemberService
{
    /// <summary>
    /// Sắp xếp mặc định của danh sách đảng viên: toàn bộ Họ tên A→Z (mục 1.8 hợp đồng API).
    /// </summary>
    private const string DefaultSortQuery = nameof(PartyMember.FullName) + " asc";

    /// <summary>
    /// Bốn cột được phép sắp xếp. Ba cột tuổi đảng là giá trị tính ra nên không có mặt ở đây.
    /// </summary>
    private static readonly string[] SortableColumns =
    {
        nameof(PartyMember.FullName),
        nameof(PartyMember.DateOfBirth),
        nameof(PartyMember.Gender),
        nameof(PartyMember.OfficialAdmissionDate),
    };

    /// <summary>
    /// Danh sách đảng viên chỉ tìm theo họ tên (mục 1.7 hợp đồng API).
    /// </summary>
    private static readonly string[] SearchableColumns = { nameof(PartyMember.FullName) };

    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IPartyMilestoneCalculator milestoneCalculator;
    private readonly IDateTimeProvider dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartyMemberService"/> class.
    /// </summary>
    /// <param name="repositoryWrapper">Cổng truy cập dữ liệu.</param>
    /// <param name="milestoneCalculator">Service thuần tính mốc tuổi đảng (T07).</param>
    /// <param name="dateTimeProvider">Nguồn thời gian của hệ thống (T08).</param>
    public PartyMemberService(
        IRepositoryWrapper repositoryWrapper,
        IPartyMilestoneCalculator milestoneCalculator,
        IDateTimeProvider dateTimeProvider)
    {
        this.repositoryWrapper = repositoryWrapper;
        this.milestoneCalculator = milestoneCalculator;
        this.dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc/>
    public async Task<PaginationResponse<PartyMemberResponse>> SearchAsync(
        PartyMemberQueryRequest request, CancellationToken cancellationToken = default)
    {
        IQueryable<PartyMember> query = repositoryWrapper.Repository<PartyMember>()
            .Find(isAsNoTracking: true)
            .ApplyFilter(request.Filter)
            .ApplySearch(request.SearchKeyword, SearchableColumns)
            .ApplySort(DefaultSortQuery, SanitizeSortQuery(request.SortQuery));

        PaginationResponse<PartyMember> page =
            await query.ToPagedListAsync(request.Current, request.PageSize, cancellationToken);

        MilestoneContext context = await LoadMilestoneContextAsync(cancellationToken);

        return new PaginationResponse<PartyMemberResponse>(
            page.PagedData.Select(entity => Project(entity, context)).ToList(),
            page.PageInfo.TotalCount,
            page.PageInfo.PageSize,
            page.PageInfo.Current);
    }

    /// <inheritdoc/>
    public async Task<PartyMemberResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        PartyMember entity = await FindOrThrowAsync(id, isAsNoTracking: true, cancellationToken);

        return Project(entity, await LoadMilestoneContextAsync(cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<PartyMemberResponse> CreateAsync(
        CreatePartyMemberRequest request, CancellationToken cancellationToken = default)
    {
        PartyMember entity = new();
        Apply(request, entity);

        await repositoryWrapper.Repository<PartyMember>().AddAsync(entity, cancellationToken);

        return Project(entity, await LoadMilestoneContextAsync(cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<PartyMemberResponse> UpdateAsync(
        Guid id, UpdatePartyMemberRequest request, CancellationToken cancellationToken = default)
    {
        PartyMember entity = await FindOrThrowAsync(id, isAsNoTracking: false, cancellationToken);
        Apply(request, entity);

        await repositoryWrapper.Repository<PartyMember>().UpdateAsync(entity, cancellationToken);

        return Project(entity, await LoadMilestoneContextAsync(cancellationToken));
    }

    /// <inheritdoc/>
    public async Task<MultipleIdentiferResponse> DeleteRangeAsync(
        DeletePartyMemberRangeRequest request, CancellationToken cancellationToken = default)
    {
        Guid[] requestedIds = request.Ids!.Distinct().ToArray();

        List<PartyMember> entities = await repositoryWrapper.Repository<PartyMember>()
            .Find(x => requestedIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (entities.Count == 0)
        {
            return new MultipleIdentiferResponse(Array.Empty<Guid>());
        }

        await repositoryWrapper.Repository<PartyMember>().DeleteRangeAsync(entities, cancellationToken);

        // Giữ đúng thứ tự Frontend đã gửi lên cho dễ đối chiếu ở màn hình.
        return new MultipleIdentiferResponse(
            requestedIds.Where(id => entities.Exists(entity => entity.Id == id)).ToArray());
    }

    /// <summary>
    /// Bỏ những cột không sắp xếp được để một <c>sortQuery</c> lạ không âm thầm đổi thứ tự.
    /// </summary>
    /// <param name="sortQuery">Chuỗi sắp xếp Frontend gửi lên.</param>
    /// <returns>Chuỗi chỉ còn cột hợp lệ, hoặc <see langword="null"/> để dùng mặc định.</returns>
    private static string? SanitizeSortQuery(string? sortQuery)
    {
        if (string.IsNullOrWhiteSpace(sortQuery))
        {
            return null;
        }

        string[] terms = sortQuery
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(term => Array.Exists(
                SortableColumns,
                column => string.Equals(column, term.Split(' ')[0], StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        return terms.Length == 0 ? null : string.Join(',', terms);
    }

    /// <summary>
    /// Chép bốn trường của yêu cầu vào bản ghi. Không gán <c>UpdatedAt</c>:
    /// <c>UpdatedAtInterceptor</c> đóng dấu ở tầng lưu thay cho mọi module (T-FIX-1).
    /// </summary>
    /// <param name="request">Yêu cầu đã qua kiểm tra hợp lệ.</param>
    /// <param name="entity">Bản ghi đích.</param>
    private static void Apply(PartyMemberRequest request, PartyMember entity)
    {
        entity.FullName = request.FullName!.Trim();
        entity.DateOfBirth = request.DateOfBirth;
        entity.Gender = request.ToGender();
        entity.OfficialAdmissionDate = request.OfficialAdmissionDate!.Value;
    }

    /// <summary>
    /// Tính ba giá trị tuổi đảng cho một bản ghi (QT3, QT3a).
    /// </summary>
    /// <param name="entity">Bản ghi đảng viên.</param>
    /// <param name="context">Hôm nay và dãy mốc huy hiệu.</param>
    /// <returns>Phản hồi đã đủ trường.</returns>
    private PartyMemberResponse Project(PartyMember entity, MilestoneContext context)
        => PartyMemberResponse.From(
            entity,
            milestoneCalculator.GetPartyAge(entity.OfficialAdmissionDate, context.Today),
            milestoneCalculator.GetNextMilestone(entity.OfficialAdmissionDate, context.Today, context.Milestones),
            milestoneCalculator.GetNextAnniversary(entity.OfficialAdmissionDate, context.Today, context.Milestones));

    /// <summary>
    /// Đọc cài đặt mốc huy hiệu một lần cho cả trang rồi sinh dãy mốc (QT1).
    /// </summary>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Hôm nay theo lịch máy chủ và dãy mốc.</returns>
    private async Task<MilestoneContext> LoadMilestoneContextAsync(CancellationToken cancellationToken)
    {
        AppSetting? setting = await repositoryWrapper.Repository<AppSetting>()
            .Find(isAsNoTracking: true)
            .FirstOrDefaultAsync(cancellationToken);

        MilestoneSettings settings = new(
            setting?.StartYears ?? AppSetting.DefaultStartYears,
            setting?.EndYears ?? AppSetting.DefaultEndYears,
            setting?.StepYears ?? AppSetting.DefaultStepYears);

        return new MilestoneContext(dateTimeProvider.Today, milestoneCalculator.BuildMilestones(settings));
    }

    /// <summary>
    /// Lấy bản ghi hoặc ném lỗi <c>400</c> kèm khóa <c>Mes.PartyMember.NotFound</c>
    /// — hợp đồng API không dùng <c>404</c> cho bản ghi không tồn tại.
    /// </summary>
    /// <param name="id">Id đảng viên.</param>
    /// <param name="isAsNoTracking">Có theo dõi thay đổi hay không.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Bản ghi tìm thấy.</returns>
    private async Task<PartyMember> FindOrThrowAsync(
        Guid id, bool isAsNoTracking, CancellationToken cancellationToken)
        => await repositoryWrapper.Repository<PartyMember>()
            .Find(x => x.Id == id, isAsNoTracking)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new BadRequestException(Messages<PartyMember>.NotFound());

    /// <summary>
    /// Hôm nay và dãy mốc dùng chung cho mọi dòng của một lời gọi.
    /// </summary>
    /// <param name="Today">Hôm nay theo lịch máy chủ.</param>
    /// <param name="Milestones">Dãy mốc huy hiệu tăng dần.</param>
    private sealed record MilestoneContext(DateOnly Today, IReadOnlyList<int> Milestones);
}
