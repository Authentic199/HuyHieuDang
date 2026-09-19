using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace HuyHieuDang.Infrastructure.Facades.Common.Extensions;

public static class PaginationExtension
{
    public static PaginationResponse<T> ToPagedList<T>(this IEnumerable<T> entities, int current, int pageSize)
    {
        PageRequest page = PageRequest.From(current, pageSize);
        IEnumerable<T> items = page.IsBeyondAnyData ? Array.Empty<T>() : entities.Skip(page.Skip).Take(page.PageSize);
        return new PaginationResponse<T>(items, entities.Count(), page.PageSize, page.Current);
    }

    public static PaginationResponse<TEntity, TMoreInfo> ToPagedList<TEntity, TMoreInfo>(this IEnumerable<TEntity> entities, int current, int pageSize, TMoreInfo moreInfo)
    {
        PageRequest page = PageRequest.From(current, pageSize);
        IEnumerable<TEntity> items = page.IsBeyondAnyData ? Array.Empty<TEntity>() : entities.Skip(page.Skip).Take(page.PageSize);
        return new PaginationResponse<TEntity, TMoreInfo>(items, entities.Count(), page.PageSize, page.Current, moreInfo);
    }

    public static async Task<PaginationResponse<T>> ToPagedListAsync<T>(this IQueryable<T> entities, int current, int pageSize, CancellationToken cancellationToken = default)
    {
        PageRequest page = PageRequest.From(current, pageSize);
        IEnumerable<T> items = page.IsBeyondAnyData
            ? Array.Empty<T>()
            : await entities.Skip(page.Skip).Take(page.PageSize).ToListAsync(cancellationToken);
        return new PaginationResponse<T>(items, entities.Count(), page.PageSize, page.Current);
    }

    public static async Task<PaginationResponse<TEntity, TMoreInfo>> ToPagedListAsync<TEntity, TMoreInfo>(this IQueryable<TEntity> entities, int current, int pageSize, TMoreInfo moreInfo, CancellationToken cancellationToken = default)
    {
        PageRequest page = PageRequest.From(current, pageSize);
        IEnumerable<TEntity> items = page.IsBeyondAnyData
            ? Array.Empty<TEntity>()
            : await entities.Skip(page.Skip).Take(page.PageSize).ToListAsync(cancellationToken);
        return new PaginationResponse<TEntity, TMoreInfo>(items, entities.Count(), page.PageSize, page.Current, moreInfo);
    }
}

/// <summary>
/// Cặp <c>current</c> / <c>pageSize</c> đã được kẹp về khoảng an toàn (A-104, A-106).
/// Đây là chỗ duy nhất tính vị trí bỏ qua, nên mọi endpoint có phân trang cùng được vá một lần:
/// <list type="bullet">
/// <item><description><c>current &lt; 1</c> coi như trang 1.</description></item>
/// <item><description><c>pageSize</c> vượt <see cref="QueryContainer.MaxPageSize"/> bị kẹp về trần, dưới 1 thì lấy mặc định.</description></item>
/// <item><description>Vị trí bỏ qua tính bằng <see cref="long"/> nên số trang rất lớn không còn tràn <see cref="int"/> rồi sinh <c>OFFSET</c> âm; trang vượt kho dữ liệu chỉ trả danh sách rỗng với mã 200.</description></item>
/// </list>
/// </summary>
public readonly struct PageRequest : IEquatable<PageRequest>
{
    private PageRequest(int current, int pageSize, long skip)
    {
        Current = current;
        PageSize = pageSize;
        Skip = skip > int.MaxValue ? int.MaxValue : (int)skip;
        IsBeyondAnyData = skip > int.MaxValue;
    }

    /// <summary>Trang đang xin, luôn ≥ 1.</summary>
    public int Current { get; }

    /// <summary>Số dòng mỗi trang sau khi kẹp, luôn trong khoảng 1 → trần.</summary>
    public int PageSize { get; }

    /// <summary>Số dòng cần bỏ qua, luôn ≥ 0.</summary>
    public int Skip { get; }

    /// <summary>
    /// Trang xin nằm quá xa (vị trí bỏ qua vượt <see cref="int.MaxValue"/>): không kho dữ liệu nào
    /// của hệ thống với tới, nên trả thẳng danh sách rỗng thay vì hỏi cơ sở dữ liệu.
    /// </summary>
    public bool IsBeyondAnyData { get; }

    public static bool operator ==(PageRequest left, PageRequest right) => left.Equals(right);

    public static bool operator !=(PageRequest left, PageRequest right) => !left.Equals(right);

    /// <summary>
    /// Kẹp một cặp tham số phân trang thô về khoảng an toàn.
    /// </summary>
    /// <param name="current">Số trang người dùng gửi.</param>
    /// <param name="pageSize">Số dòng mỗi trang người dùng gửi.</param>
    /// <returns>Cặp đã kẹp kèm vị trí bỏ qua.</returns>
    public static PageRequest From(int current, int pageSize)
    {
        int safeCurrent = current < 1 ? 1 : current;
        int safePageSize = QueryContainer.ClampPageSize(pageSize);

        return new PageRequest(safeCurrent, safePageSize, (long)(safeCurrent - 1) * safePageSize);
    }

    /// <inheritdoc/>
    public bool Equals(PageRequest other)
        => Current == other.Current && PageSize == other.PageSize && Skip == other.Skip
            && IsBeyondAnyData == other.IsBeyondAnyData;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is PageRequest other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Current, PageSize, Skip, IsBeyondAnyData);
}

public class PaginationResponse<T>
{
    public PaginationResponse(IEnumerable<T> items, int totalCount, int pageSize, int current)
    {
        PagedData = items;
        PageInfo = new(totalCount, pageSize, current);
    }

    public IEnumerable<T> PagedData { get; set; }

    public PageInfo PageInfo { get; set; }
}

public sealed class PaginationResponse<T, TMoreInfo> : PaginationResponse<T>
{
    public PaginationResponse(IEnumerable<T> items, int totalCount, int pageSize, int current, TMoreInfo moreInfo)
        : base(items, totalCount, pageSize, current)
    {
        MoreInfo = moreInfo;
    }

    public TMoreInfo MoreInfo { get; set; }
}

public class PageInfo
{
    public PageInfo(int totalCount, int pageSize, int current)
    {
        TotalCount = totalCount;
        PageSize = pageSize;
        Current = current;
    }

    public int TotalCount { get; set; }

    public int PageSize { get; set; }

    public int Current { get; set; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNext => Current < TotalPages;

    public bool HasPrevious => Current > 1 && Current <= TotalPages;
}

public class QueryContainer
{
    /// <summary>
    /// Trần số dòng mỗi trang (A-106). Gửi lớn hơn thì được phục vụ đúng trần, không báo lỗi —
    /// giao diện chỉ dùng 10 / 20 / 50 nên không ai chạm tới trần này.
    /// </summary>
    public const int MaxPageSize = 200;

    /// <summary>
    /// Số dòng mỗi trang khi không gửi <c>pageSize</c> hoặc gửi giá trị nhỏ hơn 1 (mục 1.7).
    /// </summary>
    public const int DefaultPageSize = 20;

    private int pageSize = DefaultPageSize;
    private int current = 1;

    /// <summary>
    /// filter data by operator($eq, $null, $in, $gt, $lt, $lte, $gte, $btw, $ilike, $sw) ex: { filter.propName : "$eq:mxm" }
    /// </summary>
    [ModelBinder(BinderType = typeof(CustomFilterBinder))]
    public Dictionary<string, List<string>?>? Filter { get; set; }

    /// <summary>
    /// Number elements on a page. Kẹp về <see cref="MaxPageSize"/>; nhỏ hơn 1 thì lấy mặc định.
    /// </summary>
    public int PageSize
    {
        get => pageSize;
        set => pageSize = ClampPageSize(value);
    }

    /// <summary>
    /// Pages number to take out of the total pages. Nhỏ hơn 1 thì coi như trang 1.
    /// </summary>
    public int Current
    {
        get => current;
        set => current = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Search field. Ex: '["Name","Relatives.Name"]'.
    /// </summary>
    public string[]? SearchFields { get; set; }

    /// <summary>
    /// Search keyword. Ex: 'Magnus Maximus'.
    /// </summary>
    public string? SearchKeyword { get; set; }

    /// <summary>
    /// Sort query string. Ex: 'Name desc,Region.Name'.
    /// </summary>
    public string? SortQuery { get; set; }

    /// <summary>
    /// Kẹp số dòng mỗi trang về khoảng 1 → <see cref="MaxPageSize"/>.
    /// </summary>
    /// <param name="value">Giá trị người dùng gửi.</param>
    /// <returns>Số dòng mỗi trang an toàn.</returns>
    public static int ClampPageSize(int value)
        => value < 1 ? DefaultPageSize : Math.Min(value, MaxPageSize);
}

public class CustomFilterBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        if (bindingContext.HttpContext.Request.QueryString.HasValue)
        {
            const string filterKey = nameof(QueryContainer.Filter);
            var filterQueries = bindingContext.HttpContext.Request.QueryString.Value![1..]
                .Split('&')
                .Where(x => x.StartsWith(filterKey, StringComparison.OrdinalIgnoreCase))
                .GroupBy(x => x.Split('=')[0])
                .ToDictionary(x => x.Key[(filterKey.Length + 1)..], x => x.Select(x =>
                {
                    var compareValue = x.Split('=');
                    return compareValue.Length > 1 ? HttpUtility.UrlDecode(compareValue.GetValue(1)?.ToString()) : string.Empty;
                }).ToList());

            bindingContext.Result = ModelBindingResult.Success(filterQueries);
        }

        return Task.CompletedTask;
    }
}