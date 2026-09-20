using AutoMapper.Internal;
using HuyHieuDang.Core.Bases;
using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace HuyHieuDang.Infrastructure.Facades.Common.Extensions
{
    public static class QueryExpressionExtension
    {
        private static readonly ILogger Logger = Log.ForContext(typeof(QueryExpressionExtension));

        private static readonly Dictionary<string, string> FilterOperators = new()
        {
            { FilterOperator.Eq, " ([PropName] == [Value])" },
            { FilterOperator.Null, " == null " },
            { FilterOperator.In, ".Contains(it.[PropName]) " },
            { FilterOperator.Gt, " ([PropName] > [Value]) " },
            { FilterOperator.Lt, " ([PropName] < [Value]) " },
            { FilterOperator.Lte, " ([PropName] <= [Value]) " },
            { FilterOperator.Gte, " ([PropName] >= [Value]) " },
            { FilterOperator.Btw, " ( [PropName]  >= [First] and [PropName] <= [Last] ) " },
            { FilterOperator.Ilike, " ([PropName].Contains([Value])) " },
            { FilterOperator.Sw, " ([PropName].StartsWith([Value])) " },
        };

        public static IQueryable<T> ApplySearch<T>(this IQueryable<T> entities, string? keyword, string[]? searchFields, string[]? searchFieldExcepts = null, bool checkNull = false)
        {
            if (!entities.Any() || string.IsNullOrWhiteSpace(keyword))
            {
                return entities;
            }

            searchFields ??= typeof(T).GetPropertyRecursiveWithMaxDeep(1, typeof(NotSearchableAttribute)).ToArray();

            StringBuilder searchQuery = new();
            foreach (string searchField in searchFields)
            {
                PropertyInfo? propertyInfo = typeof(T).GetPropertyRecursive(searchField);

                if (propertyInfo is null || propertyInfo.PropertyType != typeof(string) ||
                    (searchFieldExcepts != null && Array.Exists(searchFieldExcepts, x => string.Equals(x, searchField, StringComparison.OrdinalIgnoreCase))))
                {
                    continue;
                }

                string query = checkNull ? $"np({searchField}.ToLower().Contains(@0)) == true" : $"{searchField}.ToLower().Contains(@0)";
                searchQuery.Append(query).Append(" or ");
            }

            string queryText = searchQuery.ToString().TrimEnd(' ', 'o', 'r', ' ');
            if (string.IsNullOrEmpty(queryText))
            {
                return entities;
            }

            return entities.Where(queryText, keyword.ToLower());
        }

        public static IEnumerable<T> ApplySearch<T>(this IEnumerable<T> entities, string? keyword, string[]? searchFields, string[]? searchFieldExcepts = null)
        {
            return entities.AsQueryable().ApplySearch(keyword, searchFields, searchFieldExcepts, true).AsEnumerable();
        }

        public static IQueryable<T> ApplySort<T>(this IQueryable<T> entities, string orderByQueryDefault, string? orderByQuery, bool checkNull = false)
        {
            if (!entities.Any())
            {
                return entities;
            }

            orderByQuery ??= orderByQueryDefault;

            string[] orderParams = orderByQuery.Trim().ToLower().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Type entityType = typeof(T);

            StringBuilder orderQueryBuilder = new();
            foreach (string orderParam in orderParams)
            {
                string[] parts = orderParam.Trim().Split(' ');
                if (parts.Length < 1)
                {
                    continue;
                }

                string propertyName = parts[0];
                bool isDescending = parts.Length > 1 && string.Equals(parts[1], OrderTypeAcronym.Desc, StringComparison.OrdinalIgnoreCase);

                PropertyInfo? propertyInfo = entityType.GetPropertyRecursive(propertyName);
                if (propertyInfo is null || !(propertyInfo.PropertyType == typeof(string) || propertyInfo.PropertyType.IsValueType || propertyInfo.PropertyType.IsEnum))
                {
                    continue;
                }

                string propertyExpression = checkNull ? $"np({propertyName})" : propertyName;
                string sortingOrder = isDescending ? "descending" : "ascending";
                orderQueryBuilder
                    .Append(propertyExpression)
                    .Append(' ')
                    .Append(sortingOrder)
                    .Append(", ");
            }

            PropertyInfo? idProperty = entityType.GetProperty(nameof(IIdentify<object>.Id));
            if (idProperty != null)
            {
                orderQueryBuilder.Append(idProperty.Name).Append(' ').Append("descending").Append(", ");
            }

            string orderQueryText = orderQueryBuilder.ToString().TrimEnd(',', ' ');
            if (string.IsNullOrEmpty(orderQueryText))
            {
                return entities;
            }

            return entities.OrderBy(orderQueryText);
        }

        public static IEnumerable<T> ApplySort<T>(this IEnumerable<T> entities, string orderByQueryDefault, string? orderByQuery)
        {
            return entities.AsQueryable().ApplySort(orderByQueryDefault, orderByQuery, true).AsEnumerable();
        }

        public static IEnumerable<T> ApplyFilter<T>(this IEnumerable<T> entities, IDictionary<string, List<string>?>? filter)
        {
            return entities.AsQueryable().ApplyFilter(filter, true).AsEnumerable();
        }

        /// <summary>
        /// Áp bộ lọc vào truy vấn. Giá trị lọc không ép được về kiểu của trường là lỗi của bên gọi:
        /// ném <see cref="BadRequestException"/> mang khóa <c>Mes.Common.Invalid.Parameter</c> thay vì
        /// bỏ qua bộ lọc rồi trả về toàn bộ dữ liệu (QC-T27-05).
        /// Tên trường không tồn tại vẫn được bỏ qua như trước.
        /// </summary>
        /// <typeparam name="T">Kiểu phần tử của truy vấn.</typeparam>
        /// <param name="entities">Truy vấn gốc.</param>
        /// <param name="filter">Bảng bộ lọc <c>tên trường → danh sách biểu thức</c>.</param>
        /// <param name="checkNull">Bọc trường bằng <c>np(...)</c> khi lọc trên bộ nhớ.</param>
        /// <returns>Truy vấn đã áp bộ lọc.</returns>
        /// <exception cref="BadRequestException">Giá trị lọc sai kiểu, sai toán tử hoặc sai định dạng.</exception>
        public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> entities, IDictionary<string, List<string>?>? filter, bool checkNull = false)
        {
            if (filter == null || filter.Count == 0)
            {
                return entities;
            }

            List<QueryFilterResult> queryFilterResults = BuildFilterQueries<T>(filter, checkNull);

            if (queryFilterResults.Count == 0 || !entities.Any())
            {
                return entities;
            }

            foreach (QueryFilterResult queryFilterResult in queryFilterResults)
            {
                try
                {
                    Debug.WriteLine("----> Filter Query: " + queryFilterResult.Query);
                    entities = entities.Where(queryFilterResult.Query, queryFilterResult.Params.ToArray());
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, "Không dựng được biểu thức lọc {FilterQuery}", queryFilterResult.Query);
                    throw InvalidFilterValue();
                }
            }

            return entities;
        }

        /// <summary>
        /// Dựng sẵn mọi biểu thức lọc trước khi chạm vào dữ liệu, để giá trị sai kiểu bị từ chối
        /// ngay cả khi bảng đang rỗng.
        /// </summary>
        private static List<QueryFilterResult> BuildFilterQueries<T>(IDictionary<string, List<string>?> filter, bool checkNull)
        {
            List<QueryFilterResult> queryFilterResults = new();

            foreach (KeyValuePair<string, List<string>?> filterItem in filter)
            {
                PropertyInfo? propertyInfo = typeof(T).GetPropertyRecursive(filterItem.Key.ToLower());

                if (propertyInfo is null || filterItem.Value is null || propertyInfo.GetType().IsGenericType)
                {
                    continue;
                }

                foreach (QueryFilterResult queryFilterResult in GenerateFilterQuery(filterItem!, propertyInfo.PropertyType, checkNull))
                {
                    queryFilterResult.Query = queryFilterResult.Query.TrimEnd(' ', 'a', 'n', 'd', ' ');
                    queryFilterResults.Add(queryFilterResult);
                }
            }

            return queryFilterResults;
        }

        private static BadRequestException InvalidFilterValue() => new(Messages.Common.InvalidParameter);

        /// <summary>
        /// Ép giá trị lọc về kiểu của trường. Chuỗi giữ nguyên; enum chỉ nhận tên hằng số
        /// (<c>Male</c>, <c>Female</c>) chứ không nhận số, vì hợp đồng API chỉ mô tả tên;
        /// các kiểu còn lại ép theo văn hóa bất biến để không phụ thuộc máy chạy.
        /// </summary>
        private static bool TryConvertValue(string value, Type propertyType, out object? converted)
        {
            converted = null;
            Type targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if (targetType == typeof(string))
            {
                converted = value;
                return true;
            }

            if (targetType.IsEnum)
            {
                if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)
                    || !Enum.TryParse(targetType, value, true, out object? parsed)
                    || parsed is null)
                {
                    return false;
                }

                converted = parsed;
                return true;
            }

            TypeConverter typeConverter = TypeDescriptor.GetConverter(targetType);
            if (!typeConverter.CanConvertFrom(typeof(string)))
            {
                return false;
            }

            try
            {
                converted = typeConverter.ConvertFromInvariantString(value);
                return converted is not null;
            }
            catch (Exception exception) when (exception is not OutOfMemoryException and not StackOverflowException)
            {
                return false;
            }
        }

        /// <summary>
        /// Ném 400 khi giá trị không ép được về kiểu của trường.
        /// </summary>
        private static object? ConvertOrThrow(string value, Type propertyType)
        {
            if (!TryConvertValue(value, propertyType, out object? converted))
            {
                throw InvalidFilterValue();
            }

            return converted;
        }

        /// <summary>
        /// Dựng mảng đúng kiểu của trường cho toán tử <c>$in</c>; Dynamic LINQ không tự ép
        /// mảng chuỗi sang enum hay số.
        /// </summary>
        private static Array ConvertValuesOrThrow(IReadOnlyList<string> values, Type propertyType)
        {
            Array typedValues = Array.CreateInstance(propertyType, values.Count);

            for (int index = 0; index < values.Count; index++)
            {
                typedValues.SetValue(ConvertOrThrow(values[index], propertyType), index);
            }

            return typedValues;
        }

        private static IEnumerable<QueryFilterResult> GenerateFilterQuery(KeyValuePair<string, List<string>> filterItem, Type propertyType, bool checkNull)
        {
            var key = checkNull ? $"np({filterItem.Key})" : filterItem.Key;
            const string suffix = "and ";
            List<QueryFilterResult> queryFilterResults = new();

            foreach (string value in filterItem.Value)
            {
                int indexParam = 0;
                QueryFilterResult queryFilterResult = new();
                List<string> result = value.Split(":").ToList();
                bool isFilterPrefixNot = false;

                if (result[0].Equals(FilterPrefix.Not, StringComparison.OrdinalIgnoreCase))
                {
                    result.RemoveAt(0);
                    isFilterPrefixNot = true;
                }

                if (result.Count > 1)
                {
                    result = new List<string>
                    {
                        result[0],
                        string.Join(":", result.Skip(1)),
                    };
                }

                if (result.Count == 1 && result[0].Equals(FilterOperator.Null, StringComparison.OrdinalIgnoreCase))
                {
                    if (!propertyType.IsNullableType() && !propertyType.IsClass)
                    {
                        throw InvalidFilterValue();
                    }

                    queryFilterResult.Query = $"({key} {FilterOperators[FilterOperator.Null]}) {suffix}";
                }
                else if (result.Count == 2 && FilterOperators.ContainsKey(result[0].ToLower()))
                {
                    result[0] = result[0].ToLower();

                    switch (result[0])
                    {
                        case FilterOperator.In:
                            queryFilterResult.Query = $"@{indexParam}{FilterOperators[FilterOperator.In]}";
                            queryFilterResult.Params.Add(ConvertValuesOrThrow(result[1].Split(','), propertyType));
                            break;

                        case FilterOperator.Btw:
                            string[] btwValue = result[1].Split(',');
                            if (btwValue.Length != 2 || string.IsNullOrEmpty(btwValue[0]) || string.IsNullOrEmpty(btwValue[1]))
                            {
                                throw InvalidFilterValue();
                            }

                            queryFilterResult.Query = FilterOperators[FilterOperator.Btw]
                                .Replace("[First]", $"@{indexParam++}", StringComparison.OrdinalIgnoreCase)
                                .Replace("[Last]", $"@{indexParam}", StringComparison.OrdinalIgnoreCase);
                            queryFilterResult.Params.Add(ConvertOrThrow(btwValue[0], propertyType)!);
                            queryFilterResult.Params.Add(ConvertOrThrow(btwValue[1], propertyType)!);
                            break;

                        case FilterOperator.Ilike:
                        case FilterOperator.Sw:
                            if ((Nullable.GetUnderlyingType(propertyType) ?? propertyType) != typeof(string))
                            {
                                throw InvalidFilterValue();
                            }

                            queryFilterResult.Query = FilterOperators[result[0]]
                                .Replace("[Value]", $"@{indexParam}", StringComparison.OrdinalIgnoreCase);
                            queryFilterResult.Params.Add(result[1]);
                            break;

                        default:
                            queryFilterResult.Query = FilterOperators[result[0]]
                                .Replace("[Value]", $"@{indexParam}", StringComparison.OrdinalIgnoreCase);
                            queryFilterResult.Params.Add(ConvertOrThrow(result[1], propertyType)!);
                            break;
                    }
                }
                else
                {
                    throw InvalidFilterValue();
                }

                if (isFilterPrefixNot)
                {
                    queryFilterResult.Query = $"!{queryFilterResult.Query}";
                }

                queryFilterResult.Query = queryFilterResult.Query.Replace("[PropName]", key, StringComparison.OrdinalIgnoreCase);
                queryFilterResults.Add(queryFilterResult);
            }

            return queryFilterResults;
        }
    }

    public static class OrderTypeAcronym
    {
        public const string Asc = nameof(Asc);
        public const string Desc = nameof(Desc);
    }

    public static class FilterOperator
    {
        public const string Eq = "$eq";
        public const string Null = "$null";
        public const string In = "$in";
        public const string Gt = "$gt";
        public const string Lt = "$lt";
        public const string Lte = "$lte";
        public const string Gte = "$gte";
        public const string Btw = "$btw";
        public const string Ilike = "$ilike";
        public const string Sw = "$sw";
    }

    public static class FilterPrefix
    {
        public const string Not = "$not";
    }

    public class QueryFilterResult
    {
        public string Query { get; set; } = string.Empty;

        public List<object> Params { get; set; } = new();
    }
}