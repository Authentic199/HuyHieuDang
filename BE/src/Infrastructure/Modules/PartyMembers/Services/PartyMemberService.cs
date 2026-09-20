using System.Globalization;
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
/// Nghiệp vụ đảng viên: danh sách, lấy một, thêm, sửa, xóa một và xóa nhiều (UC-20 → UC-23).
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

    /// <summary>Xóa hẳn một đảng viên (UC-23, QT10).</summary>
    /// <param name="id">Id đảng viên.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Id vừa xóa.</returns>
    Task<PartyMemberIdentifierResponse> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Xóa nhiều đảng viên trong một giao dịch (UC-23).</summary>
    /// <param name="request">Danh sách id cần xóa.</param>
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
    /// Khóa lọc theo Mốc kế tiếp (T51). Không phải cột của bảng nên được quy đổi thành khoảng
    /// <see cref="PartyMember.OfficialAdmissionDate"/> trước khi vào <c>ApplyFilter</c>.
    /// </summary>
    private const string NextMilestoneKey = nameof(PartyMemberResponse.NextMilestone);

    /// <summary>
    /// Giá trị lọc của những người đã vượt mốc lớn nhất — giao diện hiển thị dấu "—".
    /// </summary>
    private const string NoneValue = "None";

    /// <summary>
    /// Bốn cột được phép sắp xếp thẳng trên bảng.
    /// </summary>
    private static readonly string[] SortableColumns =
    {
        nameof(PartyMember.FullName),
        nameof(PartyMember.DateOfBirth),
        nameof(PartyMember.Gender),
        nameof(PartyMember.OfficialAdmissionDate),
    };

    /// <summary>
    /// Ba tên cột tính ra sắp xếp được nhờ quy đổi (T51). Tuổi đảng và Mốc kế tiếp đều KHÔNG tăng
    /// theo Ngày chính thức, nên cả ba đổi thành <see cref="PartyMember.OfficialAdmissionDate"/>
    /// với chiều ngược lại. <c>PartyAgeYears</c> là tên trường trên phản hồi, <c>PartyAge</c> là
    /// tên ngắn trong yêu cầu của CEO — nhận cả hai để Frontend khỏi phải nhớ hai cách viết.
    /// </summary>
    private static readonly string[] InvertedSortColumns =
    {
        NextMilestoneKey,
        nameof(PartyMemberResponse.PartyAgeYears),
        "PartyAge",
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
        MilestoneContext context = await LoadMilestoneContextAsync(cancellationToken);

        IQueryable<PartyMember> query = ApplyNextMilestoneFilter(
                repositoryWrapper.Repository<PartyMember>().Find(isAsNoTracking: true),
                request.Filter,
                context)
            .ApplyFilter(WithoutNextMilestone(request.Filter))
            .ApplySearch(request.SearchKeyword, SearchableColumns)
            .ApplySort(DefaultSortQuery, SanitizeSortQuery(request.SortQuery));

        PaginationResponse<PartyMember> page =
            await query.ToPagedListAsync(request.Current, request.PageSize, cancellationToken);

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
    public async Task<PartyMemberIdentifierResponse> DeleteAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        PartyMember entity = await FindOrThrowAsync(id, isAsNoTracking: false, cancellationToken);

        await repositoryWrapper.Repository<PartyMember>().DeleteAsync(entity, cancellationToken);

        return new PartyMemberIdentifierResponse(id);
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
    /// Bỏ những cột không sắp xếp được, và quy đổi Tuổi đảng / Mốc kế tiếp thành Ngày chính thức
    /// theo chiều ngược lại (T51), để mệnh đề sắp xếp nằm trọn trong SQL.
    /// </summary>
    /// <param name="sortQuery">Chuỗi sắp xếp Frontend gửi lên.</param>
    /// <returns>Chuỗi chỉ còn cột của bảng, hoặc <see langword="null"/> để dùng mặc định.</returns>
    private static string? SanitizeSortQuery(string? sortQuery)
    {
        if (string.IsNullOrWhiteSpace(sortQuery))
        {
            return null;
        }

        List<string> terms = new();

        foreach (string term in sortQuery.Split(
            ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] parts = term.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string column = parts[0];
            bool isDescending = parts.Length > 1
                && string.Equals(parts[1], OrderTypeAcronym.Desc, StringComparison.OrdinalIgnoreCase);

            if (Array.Exists(InvertedSortColumns, x => string.Equals(x, column, StringComparison.OrdinalIgnoreCase)))
            {
                column = nameof(PartyMember.OfficialAdmissionDate);
                isDescending = !isDescending;
            }
            else if (!Array.Exists(SortableColumns, x => string.Equals(x, column, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            // Cùng một cột xuất hiện hai lần thì lần sau không đổi được thứ tự, giữ lần đầu cho gọn.
            if (!terms.Exists(x => x.StartsWith(column + ' ', StringComparison.OrdinalIgnoreCase)))
            {
                terms.Add($"{column} {(isDescending ? OrderTypeAcronym.Desc : OrderTypeAcronym.Asc)}");
            }
        }

        return terms.Count == 0 ? null : string.Join(',', terms);
    }

    /// <summary>
    /// Bản sao bộ lọc đã bỏ khóa Mốc kế tiếp — khóa này đã được quy đổi thành khoảng ngày nên
    /// không còn việc gì cho <c>ApplyFilter</c>.
    /// </summary>
    /// <param name="filter">Bộ lọc Frontend gửi lên.</param>
    /// <returns>Bộ lọc còn lại, hoặc chính nó khi không có khóa Mốc kế tiếp.</returns>
    private static Dictionary<string, List<string>?>? WithoutNextMilestone(Dictionary<string, List<string>?>? filter)
        => filter is null
            ? null
            : filter
                .Where(x => !string.Equals(x.Key, NextMilestoneKey, StringComparison.OrdinalIgnoreCase))
                .ToDictionary(x => x.Key, x => x.Value);

    /// <summary>
    /// Đọc giá trị lọc Mốc kế tiếp: <c>$eq:40</c> ra mốc 40, <c>$eq:None</c> ra
    /// <see langword="null"/>. Mọi cách viết khác là lỗi <c>400</c>.
    /// </summary>
    /// <param name="value">Giá trị thô của tham số truy vấn.</param>
    /// <param name="milestones">Dãy mốc của QT1 tại thời điểm gọi.</param>
    /// <returns>Mốc cần lọc, hoặc <see langword="null"/> cho nhóm đã vượt mốc lớn nhất.</returns>
    /// <exception cref="BadRequestException">Toán tử hoặc mốc không hợp lệ.</exception>
    private static int? ParseNextMilestone(string value, IReadOnlyList<int> milestones)
    {
        string[] parts = value.Split(':', 2);

        if (parts.Length != 2 || !string.Equals(parts[0], FilterOperator.Eq, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException(Messages<PartyMember>.Invalid(NextMilestoneKey));
        }

        string raw = parts[1].Trim();

        if (string.Equals(raw, NoneValue, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int milestone)
            || !milestones.Contains(milestone))
        {
            throw new BadRequestException(Messages<PartyMember>.Invalid(NextMilestoneKey));
        }

        return milestone;
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
    /// Quy đổi <c>filter.NextMilestone=$eq:&lt;mốc|None&gt;</c> thành hai bất đẳng thức trên
    /// <see cref="PartyMember.OfficialAdmissionDate"/> (T51), để điều kiện nằm trong <c>WHERE</c>.
    /// </summary>
    /// <param name="query">Truy vấn đang dựng.</param>
    /// <param name="filter">Bộ lọc Frontend gửi lên.</param>
    /// <param name="context">Hôm nay và dãy mốc huy hiệu.</param>
    /// <returns>Truy vấn đã thêm điều kiện; giữ nguyên khi không lọc theo mốc kế tiếp.</returns>
    /// <exception cref="BadRequestException">Giá trị lọc không phải một mốc của dãy hiện hành.</exception>
    private IQueryable<PartyMember> ApplyNextMilestoneFilter(
        IQueryable<PartyMember> query, Dictionary<string, List<string>?>? filter, MilestoneContext context)
    {
        List<string>? values = filter?
            .FirstOrDefault(x => string.Equals(x.Key, NextMilestoneKey, StringComparison.OrdinalIgnoreCase))
            .Value;

        if (values is null || values.Count == 0)
        {
            return query;
        }

        // Một đảng viên chỉ có đúng một mốc kế tiếp, nên hai giá trị lọc cùng lúc là vô nghĩa:
        // báo lỗi thay vì lặng lẽ trả danh sách rỗng.
        if (values.Count > 1)
        {
            throw new BadRequestException(Messages<PartyMember>.Invalid(NextMilestoneKey));
        }

        AdmissionDateRange range = milestoneCalculator.GetAdmissionDateRangeForNextMilestone(
            ParseNextMilestone(values[0], context.Milestones), context.Today, context.Milestones);

        if (range.FromExclusive is DateOnly from)
        {
            query = query.Where(x => x.OfficialAdmissionDate > from);
        }

        if (range.ToInclusive is DateOnly to)
        {
            query = query.Where(x => x.OfficialAdmissionDate <= to);
        }

        return query;
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
