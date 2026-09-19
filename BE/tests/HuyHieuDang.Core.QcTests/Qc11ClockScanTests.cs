using System.Text.RegularExpressions;
using HuyHieuDang.Core.QcTests.Support;

namespace HuyHieuDang.Core.QcTests;

/// <summary>
/// A-901 (ca kiểm thử tĩnh, T-FIX-1) — logic nghiệp vụ không được đọc đồng hồ hệ thống.
/// <para>
/// Phạm vi do CEO chốt ngày 19/09/2026: <c>BE/src/Core/**</c> và các module nghiệp vụ trong
/// Infrastructure (đảng viên, đợt, cài đặt, tính toán, import/xuất). Bảy dòng ở tầng khung
/// được loại trừ và liệt kê tường minh trong <see cref="FrameworkExclusions"/>:
/// hạn JWT (2), HttpClientSender (2), CleanRefreshTokenWorker (1), Logging (1),
/// UserRefreshTokenRequest (1). Loại trừ phải ghi rõ ở đây, không được lặng lẽ bỏ qua.
/// </para>
/// </summary>
[Trait("Tang", "Logic")]
[Trait("QuyTac", "T-FIX-1")]
public sealed class Qc11ClockScanTests
{
    /// <summary>Bảy dòng tầng khung nằm ngoài phạm vi logic nghiệp vụ.</summary>
    private static readonly string[] FrameworkExclusions =
    [
        Path.Combine("Infrastructure", "Facades", "Auth", "Jwt", "JwtSettingOptions.cs"),
        Path.Combine("Infrastructure", "Facades", "Common", "HttpClients", "HttpClientSender.cs"),
        Path.Combine("Infrastructure", "Facades", "Identity", "JwtToken", "CleanRefreshTokenWorker.cs"),
        Path.Combine("Infrastructure", "Facades", "Logging", "Startup.cs"),
        Path.Combine("Infrastructure", "Modules", "Users", "Requests", "Authentications", "UserRefreshTokenRequest.cs"),
    ];

    private static readonly Regex LocalClock = new(
        @"DateTime\.(Now|Today)\b|DateTimeOffset\.Now\b", RegexOptions.Compiled);

    private static readonly Regex AnyClock = new(
        @"DateTime\.(Now|Today|UtcNow)\b|DateTimeOffset\.(Now|UtcNow)\b", RegexOptions.Compiled);

    [Fact(DisplayName = "A-901 · Không nơi nào trong BE/src đọc đồng hồ theo giờ máy (T-FIX-2)")]
    public void A901_NoLocalClockAnywhereInTheBackendSource()
    {
        Scan(RepoPaths.BackendSource, LocalClock, applyExclusions: false).ShouldBeEmpty();
    }

    [Fact(DisplayName = "A-901 · Service tính mốc tuổi đảng không đọc đồng hồ, chỉ nhận today qua tham số")]
    public void A901_TheMilestoneCalculatorNeverReadsAClock()
    {
        string folder = Path.Combine(RepoPaths.BackendSource, "Core", "PartyBadges");

        Scan(folder, AnyClock, applyExclusions: false).ShouldBeEmpty();
    }

    [Fact(DisplayName = "A-901 · Module nghiệp vụ trong Infrastructure không đọc đồng hồ (trừ 7 dòng tầng khung)")]
    public void A901_NoClockInTheBusinessModules()
    {
        string folder = Path.Combine(RepoPaths.BackendSource, "Infrastructure", "Modules");

        Scan(folder, AnyClock, applyExclusions: true).ShouldBeEmpty();
    }

    // QC-05 — BE/src/Core/Bases/BaseEntity.cs:14 đặt CreatedAt = DateTimeOffset.UtcNow.
    // CEO đã chốt ngày 19/09/2026: đây là dấu thời gian kiểm toán của tầng khung, dùng UTC chứ
    // không phải giờ máy, không tham gia QT1–QT11, nên được LOẠI TRỪ khỏi A-901 chứ không sửa.
    // Ca này giữ [Skip] làm dấu ghi nhận quyết định đó — bỏ Skip là nó đỏ vì đúng dòng đã loại trừ.
    // Ghi chú: IDateTimeProvider (T-FIX-1) hiện chưa tồn tại; Backend dựng khi lên API (T12/T13).
    [Fact(DisplayName = "QC-05 · Core không được đọc đồng hồ hệ thống", Skip = "QC-05 đã chốt loại trừ BaseEntity.CreatedAt — giữ Skip theo quyết định của CEO.")]
    public void Qc05_NoClockAnywhereInCore()
    {
        Scan(Path.Combine(RepoPaths.BackendSource, "Core"), AnyClock, applyExclusions: false).ShouldBeEmpty();
    }

    private static List<string> Scan(string folder, Regex pattern, bool applyExclusions)
    {
        List<string> hits = new();

        if (!Directory.Exists(folder))
        {
            return hits;
        }

        foreach (string file in Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories))
        {
            if (applyExclusions && FrameworkExclusions.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            string[] lines = File.ReadAllLines(file);

            for (int i = 0; i < lines.Length; i++)
            {
                if (pattern.IsMatch(lines[i]))
                {
                    hits.Add($"{Path.GetRelativePath(RepoPaths.RepositoryRoot, file)}:{i + 1}");
                }
            }
        }

        return hits;
    }
}
