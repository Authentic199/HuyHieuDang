using System.Text.Json;
using System.Text.Json.Serialization;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Đọc bộ dữ liệu biên dùng chung của QC ở <c>tests/fixtures/data/</c>: 32 đảng viên lõi, bốn đợt
/// chính và tệp kết quả mong đợi <c>expected.json</c>. Bộ kiểm thử đối chiếu thẳng với các tệp này,
/// không chép lại con số nào sang mã nguồn.
/// </summary>
public static class CoreFixtures
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    private static readonly Lazy<string> Directory = new(Locate);

    /// <summary>
    /// 32 đảng viên biên của bộ lõi.
    /// </summary>
    public static IReadOnlyList<FixtureMember> CoreMembers { get; } = Read<List<FixtureMember>>("members-core.json");

    /// <summary>
    /// Các bộ đợt trao huy hiệu; khóa <c>main</c> là bốn đợt chính.
    /// </summary>
    public static IReadOnlyDictionary<string, List<FixturePeriod>> Periods { get; }
        = Read<Dictionary<string, List<FixturePeriod>>>("periods.json");

    /// <summary>
    /// Các bộ cài đặt mốc huy hiệu; khóa <c>default</c> là 30 / 90 / 5.
    /// </summary>
    public static IReadOnlyDictionary<string, FixtureSettings> Settings { get; }
        = Read<Dictionary<string, FixtureSettings>>("settings.json");

    /// <summary>
    /// Toàn bộ kết quả mong đợi, đọc dưới dạng cây <see cref="JsonElement"/> để bám sát tệp gốc.
    /// </summary>
    public static JsonElement Expected { get; } = Read<JsonElement>("expected.json");

    /// <summary>
    /// Lấy một kịch bản trong <c>expected.json</c>.
    /// </summary>
    /// <param name="name">Tên kịch bản, ví dụ <c>core_default_T0</c>.</param>
    /// <returns>Nút JSON của kịch bản.</returns>
    public static JsonElement Scenario(string name) => Expected.GetProperty("scenarios").GetProperty(name);

    /// <summary>
    /// Bốn đợt chính của bộ dữ liệu.
    /// </summary>
    /// <returns>Danh sách đợt theo thứ tự trong tệp.</returns>
    public static IReadOnlyList<FixturePeriod> MainPeriods() => Periods["main"];

    private static TValue Read<TValue>(string fileName)
    {
        string path = System.IO.Path.Combine(Directory.Value, fileName);

        return JsonSerializer.Deserialize<TValue>(File.ReadAllText(path), Options)
            ?? throw new InvalidOperationException($"Không đọc được fixture {fileName}.");
    }

    /// <summary>
    /// Đi ngược cây thư mục từ nơi chạy bản dựng cho tới khi gặp <c>tests/fixtures/data</c>.
    /// </summary>
    /// <returns>Đường dẫn thư mục fixture.</returns>
    private static string Locate()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            string candidate = System.IO.Path.Combine(directory.FullName, "tests", "fixtures", "data");

            if (System.IO.Directory.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Không tìm thấy thư mục tests/fixtures/data trong kho mã.");
    }
}

/// <summary>
/// Một đảng viên trong <c>members-core.json</c>.
/// </summary>
public sealed class FixtureMember
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = default!;

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = default!;

    [JsonPropertyName("dateOfBirth")]
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Giới tính ghi bằng chữ tiếng Việt <c>Nam</c> / <c>Nữ</c> / rỗng.
    /// </summary>
    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("officialAdmissionDate")]
    public DateOnly OfficialAdmissionDate { get; set; }
}

/// <summary>
/// Một đợt trong <c>periods.json</c>.
/// </summary>
public sealed class FixturePeriod
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("fromDay")]
    public int FromDay { get; set; }

    [JsonPropertyName("fromMonth")]
    public int FromMonth { get; set; }

    [JsonPropertyName("toDay")]
    public int ToDay { get; set; }

    [JsonPropertyName("toMonth")]
    public int ToMonth { get; set; }
}

/// <summary>
/// Một bộ cài đặt trong <c>settings.json</c>.
/// </summary>
public sealed class FixtureSettings
{
    [JsonPropertyName("start_years")]
    public int StartYears { get; set; }

    [JsonPropertyName("end_years")]
    public int EndYears { get; set; }

    [JsonPropertyName("step_years")]
    public int StepYears { get; set; }

    [JsonPropertyName("unit_name")]
    public string? UnitName { get; set; }
}
