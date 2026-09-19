using HuyHieuDang.Core.Helpers;
using HuyHieuDang.Core.Helpers.PropertyFlatten;
using Microsoft.AspNetCore.Http;
using System.Collections;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

namespace HuyHieuDang.Infrastructure.Facades.Common.HttpClients;

/// <summary>
/// Extension methods for working with System.Net.Http.HttpClient.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Converts data to StringContent using System.Text.Json.JsonSerializer with default encoding as Encoding.UTF8.
    /// </summary>
    /// <typeparam name="T">The type of the data. Should be a class or record type, not abstract or interface.</typeparam>
    /// <param name="request">The data to be serialized.</param>
    /// <param name="encoding">The encoding to be used for the content. Defaults to Encoding.UTF8.</param>
    /// <param name="mediatype">The media type of the content. Defaults to "application/json".</param>
    /// <param name="options">Options for customizing the serialization process.</param>
    /// <returns>StringContent representing the serialized data.</returns>
    public static StringContent ToStringContent<T>(this T request, Encoding? encoding = null, string mediatype = MediaTypeNames.Application.Json, JsonSerializerOptions? options = null)
        where T : class
        => new(JsonSerializer.Serialize(request, options), encoding, mediatype);

    /// <summary>
    /// Converts data to MultipartFormDataContent.
    /// </summary>
    /// <typeparam name="T">The type of the data. Should be a class or record type, not abstract or interface.</typeparam>
    /// <param name="request">The data to be converted to MultipartFormDataContent.</param>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <c>null</c>.</exception>
    /// <exception cref="NotSupportedException">Not supported for IEnumerable types.</exception>
    /// <returns>MultipartFormDataContent representing the data.</returns>
    public static MultipartFormDataContent ToFormDataContent<T>(this T request)
        where T : class
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.GetType().IsAssignableTo(typeof(IEnumerable)))
        {
            throw new NotSupportedException($"Type based {nameof(IEnumerable)} does not support conversion.");
        }

        HttpPropertyFlattener flattener = new()
        {
            TypeAcceptFiller = x => typeof(IFormFile).IsAssignableFrom(x),
        };
        IEnumerable<PropertyFlatten> propertyFlattens = flattener.FlattenData(request);
        MultipartFormDataContent content = new();

        foreach (var property in propertyFlattens)
        {
            object? value = property.Value;
            if (value == null)
            {
                continue;
            }

            Type valueType = value.GetType();
            Type? elementType = valueType.GetElementTypeOfCollection();

            if (value is IFormFile file)
            {
                StreamContent stream = new(file.OpenReadStream());
                stream.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

                content.Add(stream, property.Path, file.FileName);
            }
            else if (elementType != null && value is IEnumerable items)
            {
                foreach (var item in items)
                {
                    if (item is IFormFile fileInCollection)
                    {
                        StreamContent stream = new(fileInCollection.OpenReadStream());
                        stream.Headers.ContentType = new MediaTypeHeaderValue(fileInCollection.ContentType);

                        content.Add(stream, property.Path, fileInCollection.FileName);
                    }
                    else
                    {
                        content.Add(new StringContent(item?.ToString() ?? string.Empty, Encoding.UTF8), property.Path);
                    }
                }
            }

            // If the property is a simple type, add it to the content as StringContent.
            else
            {
                content.Add(new StringContent(value.ToString()!, Encoding.UTF8), property.Path);
            }
        }

        return content;
    }

    /// <summary>
    /// Converts data to FormUrlEncodedContent.
    /// </summary>
    /// <typeparam name="T">The type of the data. Should be a class or record type, not abstract or interface.</typeparam>
    /// <param name="request">The data to be converted to FormUrlEncodedContent.</param>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <c>null</c>.</exception>
    /// <returns>FormUrlEncodedContent representing the data.</returns>
    public static FormUrlEncodedContent ToFormUrlEncodedContent<T>(this T request)
        where T : class
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.GetType().IsAssignableTo(typeof(IEnumerable)))
        {
            throw new NotSupportedException($"Type based {nameof(IEnumerable)} does not support conversion.");
        }

        HttpPropertyFlattener flattener = new();
        IEnumerable<PropertyFlatten> propertyFlattens = flattener.FlattenData(request);
        List<KeyValuePair<string, string>> formData = new();
        foreach (var property in propertyFlattens)
        {
            formData.Add(new KeyValuePair<string, string>(property.Path, property.Value?.ToString() ?? string.Empty));
        }

        return new FormUrlEncodedContent(formData);
    }
}