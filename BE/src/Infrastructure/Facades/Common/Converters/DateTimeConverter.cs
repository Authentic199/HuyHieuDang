using HuyHieuDang.Core.Common.Exceptions;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HuyHieuDang.Infrastructure.Facades.Common.Converters;

public class DateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        bool success = DateTimeOffset.TryParse(reader.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset result);
        if (!success)
        {
            throw new BadRequestException("Invalid datetime format");
        }

        return result.ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffffffZ"));
    }
}

public class DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        bool success = DateTime.TryParse(reader.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result);
        if (!success)
        {
            throw new BadRequestException("Invalid datetime format");
        }

        return result.ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ"));
    }
}