using AutoMapper;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace HuyHieuDang.Infrastructure.Facades.Common.HttpClients;

public interface IHttpClientSender
{
    /// <summary>
    /// Use to set client instance if not using default client
    /// </summary>
    IHttpClientSender UseClient(HttpClient httpClient);

    /// <summary>
    /// set http method (default is get)
    /// </summary>
    IHttpClientSender UseMethod(HttpMethod method);

    /// <summary>
    /// set request uri (default is empty)
    /// </summary>
    IHttpClientSender WithUri(string uri);

    /// <summary>
    /// set request uri (default is empty)
    /// </summary>
    IHttpClientSender WithUri(Uri uri);

    /// <summary>
    /// Header is object or keyvaluepair
    /// </summary>
    /// <param name="headers">Names/values of HTTP headers to set. Typically an anonymous object or IDictionary.</param>
    /// <param name="replaceUnderscoreWithHyphen">If true, underscores in property names will be replaced by hyphens. Default is true.</param>
    /// <exception cref="ArgumentNullException"><paramref name="headers"/> is <c>null</c>.</exception>
    IHttpClientSender WithHeaders(object headers, bool replaceUnderscoreWithHyphen = true);

    /// <summary>
    /// set request content (default is empty)
    /// </summary>
    IHttpClientSender WithContent(HttpContent content);

    Task<HttpResult> SendAsync(CancellationToken cancellationToken = default);
}

public class HttpClientSender : IHttpClientSender
{
    private static readonly HttpClient DefaultHttpClient = new(new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(2),
    });

    private readonly IMapper mapper;
    private readonly RequestBuilder builder = new();

    public HttpClientSender(IMapper mapper)
    {
        this.mapper = mapper;
    }

    public IHttpClientSender UseClient(HttpClient httpClient)
    {
        builder.CustomClient = httpClient;
        builder.UseLogging = false;
        return this;
    }

    public IHttpClientSender UseMethod(HttpMethod method)
    {
        builder.Method = method;
        return this;
    }

    public IHttpClientSender WithUri(string uri) => WithUri(new Uri(uri));

    public IHttpClientSender WithUri(Uri uri)
    {
        builder.Uri = uri;
        return this;
    }

    public IHttpClientSender WithContent(HttpContent content)
    {
        builder.Content = content;
        return this;
    }

    public IHttpClientSender WithHeaders(object headers, bool replaceUnderscoreWithHyphen = true)
    {
        foreach (var (key, value) in ParseKeyValuePairs(headers))
        {
            string headerKey = replaceUnderscoreWithHyphen ? key.Replace("_", "-", StringComparison.Ordinal) : key;
            if (!string.IsNullOrWhiteSpace(headerKey) && value != null)
            {
                builder.Headers[headerKey] = value.ToString()!;
            }
        }

        return this;
    }

    public async Task<HttpResult> SendAsync(CancellationToken cancellationToken = default)
    {
        var request = builder.Build();
        var client = builder.CustomClient ?? DefaultHttpClient;

        TimeSpan duration = TimeSpan.Zero;

        try
        {
            if (builder.UseLogging)
            {
                Log.Information("---> Request Info: \n{request}\n---> End", request.ToString());
            }

            DateTime start = DateTime.UtcNow;
            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            DateTime end = DateTime.UtcNow;

            var result = mapper.Map<HttpResult>(response);
            result.Duration = end - start;

            if (builder.UseLogging)
            {
                Log.Information("---> Response Info: \n{response}\n---> End", result.ToString());
            }

            return result;
        }
        catch (Exception ex)
        {
            return new(duration, ex, request);
        }
    }

    /// <summary>
    /// Returns a key-value-pairs representation of the object.
    /// For strings, URL query string format assumed and pairs are parsed from that.
    /// For objects that already implement IEnumerable&lt;KeyValuePair&gt;, the object itself is simply returned.
    /// For all other objects, all publicly readable properties are extracted and returned as pairs.
    /// </summary>
    /// <param name="obj">The object to parse into key-value pairs</param>
    /// <exception cref="ArgumentNullException"><paramref name="obj"/> is <see langword="null" />.</exception>
    private static IEnumerable<(string Key, object? Value)> ParseKeyValuePairs(object obj)
    {
        if (obj == null)
        {
            throw new ArgumentNullException(nameof(obj));
        }

        if (obj is IEnumerable e)
        {
            return
            obj is string s ? StringToKeyValue(s) :
            (IEnumerable<(string, object? Value)>)CollectionToKeyPair(e);
        }
        else
        {
            return
            obj is string s ? StringToKeyValue(s) :
            ObjectToKeyValue(obj);
        }
    }

    private static IEnumerable<(string Key, object? Value)> StringToKeyValue(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return Enumerable.Empty<(string, object?)>();
        }

        return
            from p in s.Split('&')
            let pair = SplitOnFirstOccurence(p, "=")
            let name = pair[0]
            let value = pair.Length == 1 ? null : pair[1]
            select (name, (object)value);
    }

    /// <summary>
    /// Splits at the first occurrence of the given separator.
    /// </summary>
    /// <param name="s">The string to split.</param>
    /// <param name="separator">The separator to split on.</param>
    /// <returns>Array of at most 2 strings. (1 if separator is not found.)</returns>
    private static string[] SplitOnFirstOccurence(string s, string separator)
    {
        // Needed because full PCL profile doesn't support Split(char[], int) (#119)
        if (string.IsNullOrEmpty(s))
        {
            return new[] { s };
        }

        var i = s.IndexOf(separator);
        return i == -1 ?
            new[] { s } :
            new[] { s[..i], s[(i + separator.Length)..] };
    }

    private static IEnumerable<(string Name, object? Value)> ObjectToKeyValue(object obj) =>
        from prop in obj.GetType().GetProperties()
        let getter = prop.GetGetMethod(false)
        where getter != null
        let val = getter.Invoke(obj, null)
        select (prop.Name, GetDeclaredTypeValue(val, prop.PropertyType));

    private static object? GetDeclaredTypeValue(object value, Type declaredType)
    {
        if (value == null || value.GetType() == declaredType)
        {
            return value;
        }

        declaredType = Nullable.GetUnderlyingType(declaredType) ?? declaredType;

        if (value is IEnumerable col
            && declaredType.IsGenericType
            && declaredType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
            && !col.GetType().GetInterfaces().Contains(declaredType)
            && declaredType.IsInstanceOfType(col))
        {
            var elementType = declaredType.GetGenericArguments()[0];
            return col.Cast<object>().Select(element => Convert.ChangeType(element, elementType));
        }

        return value;
    }

    private static IEnumerable<(string Key, object? Value)> CollectionToKeyPair(IEnumerable col)
    {
        bool TryGetProp(object obj, string name, out object? value)
        {
            var prop = obj.GetType().GetProperty(name);
            var field = obj.GetType().GetField(name);

            if (prop != null)
            {
                value = prop.GetValue(obj, null);
                return true;
            }

            if (field != null)
            {
                value = field.GetValue(obj);
                return true;
            }

            value = null;
            return false;
        }

        bool IsTuple2(object item, out object? name, out object? val)
        {
            name = null;
            val = null;
            return
                OrdinalContains(item.GetType().Name, "Tuple") &&
                TryGetProp(item, "Item1", out name) &&
                TryGetProp(item, "Item2", out val) &&
                !TryGetProp(item, "Item3", out _);
        }

        bool LooksLikeKV(object item, out object? name, out object? val)
        {
            name = null;
            val = null;
            return
                (TryGetProp(item, "Key", out name) || TryGetProp(item, "key", out name) || TryGetProp(item, "Name", out name) || TryGetProp(item, "name", out name)) &&
                (TryGetProp(item, "Value", out val) || TryGetProp(item, "value", out val));
        }

        foreach (var item in col)
        {
            if (item == null)
            {
                continue;
            }

            if (!IsTuple2(item, out var name, out var val) && !LooksLikeKV(item, out name, out val))
            {
                yield return (ToInvariantString(name) ?? throw new ArgumentNullException(nameof(col)), null);
            }
            else if (name != null)
            {
                yield return (ToInvariantString(name) ?? throw new ArgumentNullException(nameof(col)), val);
            }
        }
    }

    private static bool OrdinalContains(string s, string value, bool ignoreCase = false) =>
            s?.IndexOf(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0;

    /// <summary>
    /// Returns a string that represents the current object, using CultureInfo.InvariantCulture where possible.
    /// Dates are represented in IS0 8601.
    /// </summary>
    private static string? ToInvariantString(object? obj)
    {
        if (obj == null)
        {
            return null;
        }
        else
        {
            if (obj is DateTime dt)
            {
                return dt.ToString("o", CultureInfo.InvariantCulture);
            }
            else if (obj is DateTimeOffset dto)
            {
                return dto.ToString("o", CultureInfo.InvariantCulture);
            }
            else if (obj is IConvertible c)
            {
                return c.ToString(CultureInfo.InvariantCulture);
            }
            else if (obj is IFormattable f)
            {
                return f.ToString(null, CultureInfo.InvariantCulture);
            }
            else
            {
                return obj.ToString();
            }
        }
    }

    private sealed class RequestBuilder
    {
        public HttpMethod Method { get; set; } = HttpMethod.Get;

        public Uri? Uri { get; set; }

        public Dictionary<string, string> Headers { get; } = new();

        public HttpContent? Content { get; set; }

        public bool UseLogging { get; set; } = true;

        public HttpClient? CustomClient { get; set; }

        public HttpRequestMessage Build()
        {
            if (Uri == null)
            {
                throw new InvalidOperationException("URI must be specified.");
            }

            var request = new HttpRequestMessage(Method, Uri)
            {
                Content = Content,
            };

            foreach (var header in Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return request;
        }
    }
}

public class HttpResult : HttpResponseMessage
{
    public event Action<Exception>? OnError;

    private readonly Action<Exception> defaultLogError = (ex) =>
    {
        Log.Error("---> An error occurred: {error}", ex);
    };

    public HttpResult()
    {
    }

    public HttpResult(TimeSpan duration, Exception requestException, HttpRequestMessage request)
    {
        Duration = duration;
        RequestMessage = request;
        StatusCode = HttpStatusCode.InternalServerError;
        LogException(requestException);
    }

    [AllowNull]
    public new HttpResponseHeaders Headers { get; set; }

    [AllowNull]
    public new HttpResponseHeaders TrailingHeaders { get; set; }

    public TimeSpan Duration { get; internal set; }

    public async Task<string?> ReadAsStringAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogException(ex);
            StatusCode = HttpStatusCode.InternalServerError;
            return default;
        }
    }

    public async Task<TResponse?> ReadFromJsonAsync<TResponse>(JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await Content.ReadFromJsonAsync<TResponse>(options, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogException(ex);
            StatusCode = HttpStatusCode.InternalServerError;
            return default;
        }
    }

    public async Task<Stream?> ReadAsStreamAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogException(ex);
            StatusCode = HttpStatusCode.InternalServerError;
            return default;
        }
    }

    public async Task<byte[]> ReadAsByteArrayAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogException(ex);
            StatusCode = HttpStatusCode.InternalServerError;
            return Array.Empty<byte>();
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());
        sb.AppendLine("', Duration: ");
        sb.Append(Duration);
        return sb.ToString();
    }

    private void LogException(Exception ex)
    {
        if (OnError != null)
        {
            OnError?.Invoke(ex);
        }
        else
        {
            defaultLogError.Invoke(ex);
        }
    }
}

public class HttpResultProfile : Profile
{
    public HttpResultProfile()
    {
        CreateMap<HttpResponseMessage, HttpResult>();
    }
}