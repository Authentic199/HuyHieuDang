using System.Text.Json;
using System.Text.Json.Serialization;

namespace HuyHieuDang.Web.QcIntegrationTests.Support;

/// <summary>
/// Đọc bộ dữ liệu biên ở <c>tests/fixtures/data/</c>. Mọi con số trong bộ kiểm thử này đối chiếu
/// thẳng với các tệp đó, không chép lại vào mã nguồn (T-FIX-6).
/// </summary>
public static class QcFixtures
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    private static readonly Lazy<string> DataDirectory = new(Locate);

    /// <summary>32 đảng viên biên của bộ lõi.</summary>
    public static IReadOnlyList<QcMember> CoreMembers { get; } = Read<List<QcMember>>("members-core.json");

    /// <summary>1200 đảng viên của bộ lớn, chỉ dùng cho phân trang / tìm kiếm / hiệu năng.</summary>
    public static IReadOnlyList<QcMember> BulkMembers { get; } = Read<List<QcMember>>("members-bulk.json");

    /// <summary>Các bộ đợt trao huy hiệu; khóa <c>main</c> là bốn đợt chính.</summary>
    public static IReadOnlyDictionary<string, List<QcPeriod>> Periods { get; }
        = Read<Dictionary<string, List<QcPeriod>>>("periods.json");

    /// <summary>Các bộ cài đặt mốc huy hiệu; khóa <c>default</c> là 30 / 90 / 5.</summary>
    public static IReadOnlyDictionary<string, QcSettings> Settings { get; }
        = Read<Dictionary<string, QcSettings>>("settings.json");

    /// <summary>Toàn bộ kết quả mong đợi, giữ nguyên dạng cây JSON.</summary>
    public static JsonElement Expected { get; } = Read<JsonElement>("expected.json");

    /// <summary>Bốn đợt chính của bộ dữ liệu.</summary>
    public static IReadOnlyList<QcPeriod> MainPeriods => Periods["main"];

    /// <summary>Đợt biên 29/02 – 05/03.</summary>
    public static IReadOnlyList<QcPeriod> LeapEdgePeriods => Periods["leapEdge"];

    /// <summary>
    /// Hai đợt chồng lấn của <c>dataset.py</c> (<c>PERIODS_OVERLAP</c>). Không nằm trong
    /// <c>periods.json</c> nên dựng lại ở đây, đúng hai đợt mà <c>expected.json</c> cảnh báo.
    /// </summary>
    public static IReadOnlyList<QcPeriod> OverlapPeriods { get; } = new[]
    {
        new QcPeriod { Code = "PO1", Name = "Đợt A", FromDay = 1, FromMonth = 10, ToDay = 7, ToMonth = 11 },
        new QcPeriod { Code = "PO2", Name = "Đợt B", FromDay = 1, FromMonth = 11, ToDay = 30, ToMonth = 11 },
    };

    /// <summary>Bộ đợt phủ kín 01/01 – 31/12 của <c>dataset.py</c> (<c>PERIODS_FULL_COVER</c>).</summary>
    public static IReadOnlyList<QcPeriod> FullCoverPeriods { get; } = new[]
    {
        new QcPeriod { Code = "PF1", Name = "Nửa đầu năm", FromDay = 1, FromMonth = 1, ToDay = 30, ToMonth = 6 },
        new QcPeriod { Code = "PF2", Name = "Nửa cuối năm", FromDay = 1, FromMonth = 7, ToDay = 31, ToMonth = 12 },
    };

    /// <summary>Lấy một kịch bản trong <c>expected.json</c>.</summary>
    /// <param name="name">Tên kịch bản, ví dụ <c>core_default_T0</c>.</param>
    /// <returns>Nút JSON của kịch bản.</returns>
    public static JsonElement Scenario(string name) => Expected.GetProperty("scenarios").GetProperty(name);

    /// <summary>Lấy một nhánh gốc của <c>expected.json</c>, ví dụ <c>counts</c> hay <c>pagination</c>.</summary>
    /// <param name="name">Tên nhánh.</param>
    /// <returns>Nút JSON.</returns>
    public static JsonElement Node(string name) => Expected.GetProperty(name);

    /// <summary>Danh sách đủ điều kiện mong đợi của một đợt trong một năm.</summary>
    /// <param name="scenario">Tên kịch bản.</param>
    /// <param name="periodCode">Mã đợt, ví dụ <c>P4</c>.</param>
    /// <param name="year">Năm đang xét.</param>
    /// <returns>Nút JSON chứa <c>total</c>, <c>byMilestone</c> và <c>rows</c>.</returns>
    public static JsonElement Eligible(string scenario, string periodCode, int year)
        => Scenario(scenario)
            .GetProperty("eligibleByPeriod")
            .GetProperty(periodCode)
            .GetProperty("byYear")
            .GetProperty(year.ToString(System.Globalization.CultureInfo.InvariantCulture));

    /// <summary>Danh sách "chưa thuộc đợt nào" mong đợi của một năm.</summary>
    /// <param name="scenario">Tên kịch bản.</param>
    /// <param name="year">Năm đang xét.</param>
    /// <returns>Nút JSON chứa <c>total</c> và <c>rows</c>.</returns>
    public static JsonElement Missed(string scenario, int year)
        => Scenario(scenario)
            .GetProperty("missedByYear")
            .GetProperty(year.ToString(System.Globalization.CultureInfo.InvariantCulture));

    private static TValue Read<TValue>(string fileName)
    {
        string path = Path.Combine(DataDirectory.Value, fileName);

        return JsonSerializer.Deserialize<TValue>(File.ReadAllText(path), Options)
            ?? throw new InvalidOperationException($"Không đọc được fixture {fileName}.");
    }

    private static string Locate()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            foreach (string candidate in new[]
                     {
                         Path.Combine(directory.FullName, "fixtures"),
                         Path.Combine(directory.FullName, "tests", "fixtures", "data"),
                     })
            {
                if (File.Exists(Path.Combine(candidate, "expected.json")))
                {
                    return candidate;
                }
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Không tìm thấy thư mục fixture chứa expected.json.");
    }
}

/// <summary>Một đảng viên trong <c>members-core.json</c> hoặc <c>members-bulk.json</c>.</summary>
public sealed class QcMember
{
    /// <summary>Mã ca biên, ví dụ <c>B01</c>.</summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = default!;

    /// <summary>Họ tên đầy đủ.</summary>
    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = default!;

    /// <summary>Ngày sinh; có thể bỏ trống.</summary>
    [JsonPropertyName("dateOfBirth")]
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>Giới tính ghi bằng chữ tiếng Việt <c>Nam</c> / <c>Nữ</c> / rỗng.</summary>
    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    /// <summary>Ngày vào Đảng chính thức.</summary>
    [JsonPropertyName("officialAdmissionDate")]
    public DateOnly OfficialAdmissionDate { get; set; }

    /// <summary>Ca biên mà người này phục vụ.</summary>
    [JsonPropertyName("intent")]
    public string? Intent { get; set; }
}

/// <summary>Một đợt trao huy hiệu trong <c>periods.json</c>.</summary>
public sealed class QcPeriod
{
    /// <summary>Mã đợt, ví dụ <c>P4</c>.</summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = default!;

    /// <summary>Tên đợt.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>Ngày của Từ ngày.</summary>
    [JsonPropertyName("fromDay")]
    public int FromDay { get; set; }

    /// <summary>Tháng của Từ ngày.</summary>
    [JsonPropertyName("fromMonth")]
    public int FromMonth { get; set; }

    /// <summary>Ngày của Đến ngày.</summary>
    [JsonPropertyName("toDay")]
    public int ToDay { get; set; }

    /// <summary>Tháng của Đến ngày.</summary>
    [JsonPropertyName("toMonth")]
    public int ToMonth { get; set; }
}

/// <summary>Một bộ cài đặt mốc huy hiệu trong <c>settings.json</c>.</summary>
public sealed class QcSettings
{
    /// <summary>Mốc bắt đầu.</summary>
    [JsonPropertyName("start_years")]
    public int StartYears { get; set; }

    /// <summary>Mốc kết thúc.</summary>
    [JsonPropertyName("end_years")]
    public int EndYears { get; set; }

    /// <summary>Bước nhảy.</summary>
    [JsonPropertyName("step_years")]
    public int StepYears { get; set; }

    /// <summary>Tên đơn vị; có thể bỏ trống.</summary>
    [JsonPropertyName("unit_name")]
    public string? UnitName { get; set; }
}
