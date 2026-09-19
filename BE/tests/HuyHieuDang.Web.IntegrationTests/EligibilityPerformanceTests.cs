using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Xunit.Abstractions;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Ràng buộc hiệu năng của nhóm tính toán: với 10.000 đảng viên, mỗi endpoint phải trả lời dưới
/// một giây. Bài này nạp dữ liệu lớn nên tách riêng khỏi các bài đối chiếu nghiệp vụ.
/// </summary>
[Collection(ApiCollection.Name)]
public class EligibilityPerformanceTests
{
    private const int MemberCount = 10_000;

    private static readonly TimeSpan Budget = TimeSpan.FromSeconds(1);

    private readonly HuyHieuDangApiFactory factory;
    private readonly ITestOutputHelper output;

    /// <summary>
    /// Initializes a new instance of the <see cref="EligibilityPerformanceTests"/> class.
    /// </summary>
    /// <param name="factory">Host kiểm thử dùng chung cho cả lớp.</param>
    /// <param name="output">Cổng ghi số đo ra nhật ký chạy test.</param>
    public EligibilityPerformanceTests(HuyHieuDangApiFactory factory, ITestOutputHelper output)
    {
        this.factory = factory;
        this.output = output;
    }

    [Fact(DisplayName = "6.1 → 6.4 · 10.000 đảng viên: mỗi endpoint tính toán dưới 1 giây")]
    public async Task CalculationEndpoints_WithTenThousandMembers_RespondUnderOneSecond()
    {
        HttpClient client = await ApiClientFactory.CreateAuthenticatedAsync(factory);
        await CoreDataset.ResetAsync(factory);
        await CoreDataset.SeedPeriodsAsync(factory);
        await SeedManyMembersAsync();

        AwardPeriodListPayload periods =
            await ApiClientFactory.GetDataAsync<AwardPeriodListPayload>(client, "/api/AwardPeriods");
        Guid periodId = periods.Periods.Single(x => x.Name == "Đợt 7/11").Id;

        // Lần gọi đầu gánh cả chi phí khởi động của EF và nhóm kết nối, không tính vào số đo.
        await ApiClientFactory.GetDataAsync<EligibilityListPayload>(
            client, $"/api/Eligibility?awardPeriodId={periodId}&year=2026");

        TimeSpan eligibility = await MeasureAsync<EligibilityListPayload>(
            client, $"/api/Eligibility?awardPeriodId={periodId}&year=2026");
        TimeSpan dashboard = await MeasureAsync<DashboardPayload>(client, "/api/Dashboard");
        TimeSpan unassigned = await MeasureAsync<UnassignedListPayload>(client, "/api/Eligibility/Unassigned");
        TimeSpan badge = await MeasureAsync<UnassignedCountPayload>(client, "/api/Eligibility/UnassignedCount");

        string measured =
            $"Đủ điều kiện {eligibility.TotalMilliseconds:F0} ms · Dashboard {dashboard.TotalMilliseconds:F0} ms · "
            + $"Chưa thuộc đợt nào {unassigned.TotalMilliseconds:F0} ms · Badge {badge.TotalMilliseconds:F0} ms "
            + $"— ngân sách {Budget.TotalMilliseconds:F0} ms với {MemberCount} đảng viên.";

        output.WriteLine(measured);

        Assert.True(eligibility < Budget && dashboard < Budget && unassigned < Budget && badge < Budget, measured);
    }

    private static async Task<TimeSpan> MeasureAsync<TData>(HttpClient client, string url)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        await ApiClientFactory.GetDataAsync<TData>(client, url);
        stopwatch.Stop();

        return stopwatch.Elapsed;
    }

    /// <summary>
    /// Nạp 10.000 đảng viên có ngày vào Đảng rải đều 1956–2005, để mọi mốc huy hiệu đều có người.
    /// </summary>
    private async Task SeedManyMembersAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        DateOnly start = new(1956, 1, 1);
        List<PartyMember> members = new(MemberCount);

        for (int index = 0; index < MemberCount; index++)
        {
            members.Add(new PartyMember
            {
                FullName = $"Đảng viên số {index:D5}",
                OfficialAdmissionDate = start.AddDays((index * 18) % 18_000),
            });
        }

        dbContext.Set<PartyMember>().AddRange(members);
        await dbContext.SaveChangesAsync();
    }
}
