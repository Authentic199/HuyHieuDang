using HuyHieuDang.Core.Common.Interfaces;
using Newtonsoft.Json.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HuyHieuDang.Infrastructure.Facades.Common.Services;

public interface IJsonSerializerService : ITransientService
{
    string Serialize<T>(T obj, Action<JsonSerializerOptions>? configs = null);

    T? Deserialize<T>(string text);
}

public class JsonSerializerService : IJsonSerializerService
{
    public string Serialize<T>(T obj, Action<JsonSerializerOptions>? configs = null)
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };
        configs?.Invoke(options);
        return JsonSerializer.Serialize(obj, options);
    }

    public T? Deserialize<T>(string text)
    {
        return JsonSerializer.Deserialize<T>(text);
    }
}
