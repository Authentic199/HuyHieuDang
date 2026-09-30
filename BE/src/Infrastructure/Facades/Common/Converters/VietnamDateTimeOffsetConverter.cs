using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using HuyHieuDang.Core.Common.Exceptions;

namespace HuyHieuDang.Infrastructure.Facades.Common.Converters;

/// <summary>
/// Ghi một mốc thời gian dưới dạng ISO-8601 <b>kèm độ lệch +07:00</b> thay vì quy về UTC
/// (mục 1.6 hợp đồng API v1.6).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DateTimeOffsetConverter"/> đăng ký chung ở <c>Program.cs</c> ghi mọi
/// <see cref="DateTimeOffset"/> về <c>Z</c>, còn cột <c>timestamptz</c> thì Npgsql luôn đọc lên
/// với độ lệch <c>+00:00</c>. Ghép hai điều đó lại, ghi chú viết trong khoảng 00:00–07:00 giờ
/// Việt Nam sẽ hiện lùi một ngày trên giao diện, vì Frontend lấy nguyên phần <c>yyyy-MM-dd</c>
/// của chuỗi. Bộ chuyển đổi này quy giá trị về giờ Việt Nam trước khi ghi nên phần ngày luôn
/// là ngày người dùng thấy.
/// </para>
/// <para>
/// Gắn bằng <c>[JsonConverter]</c> trên đúng những trường cần — hiện chỉ là <c>noteUpdatedAt</c>;
/// <c>createdAt</c> và <c>updatedAt</c> vẫn là UTC như cũ.
/// </para>
/// </remarks>
public class VietnamDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
{
    /// <summary>
    /// Độ lệch của Asia/Ho_Chi_Minh. Việt Nam bỏ giờ mùa hè từ 1975 nên giá trị này cố định.
    /// </summary>
    public static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

    /// <summary>
    /// Khuôn chữ của mục 1.6 hợp đồng API: <c>2026-09-30T14:05:00+07:00</c>.
    /// </summary>
    public const string Format = "yyyy'-'MM'-'dd'T'HH':'mm':'sszzz";

    /// <inheritdoc/>
    public override bool HandleNull => true;

    /// <inheritdoc/>
    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
        {
            return null;
        }

        if (!DateTimeOffset.TryParse(
                reader.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset result))
        {
            throw new BadRequestException("Invalid datetime format");
        }

        return result.ToOffset(VietnamOffset);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.Value.ToOffset(VietnamOffset).ToString(Format, CultureInfo.InvariantCulture));
    }
}
