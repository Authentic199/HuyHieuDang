using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AwardPeriod = HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities.AwardPeriod;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Nạp bộ dữ liệu biên của QC vào cơ sở dữ liệu kiểm thử: 32 đảng viên lõi, bốn đợt chính và
/// một bộ cài đặt mốc huy hiệu. Nạp thẳng bằng <see cref="ApplicationDbContext"/> để không
/// phụ thuộc endpoint nào của nhóm khác.
/// </summary>
public static class CoreDataset
{
    /// <summary>
    /// Xóa sạch đảng viên và đợt, đưa cài đặt về đúng bộ được chọn.
    /// </summary>
    /// <param name="factory">Host kiểm thử.</param>
    /// <param name="settingsKey">Khóa bộ cài đặt trong <c>settings.json</c>.</param>
    public static async Task ResetAsync(HuyHieuDangApiFactory factory, string settingsKey = "default")
    {
        ArgumentNullException.ThrowIfNull(factory);

        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Set<PartyMember>().ExecuteDeleteAsync();
        await dbContext.Set<AwardPeriod>().ExecuteDeleteAsync();

        FixtureSettings settings = CoreFixtures.Settings[settingsKey];
        await dbContext.Set<AppSetting>().ExecuteUpdateAsync(setters => setters
            .SetProperty(x => x.StartYears, settings.StartYears)
            .SetProperty(x => x.EndYears, settings.EndYears)
            .SetProperty(x => x.StepYears, settings.StepYears)
            .SetProperty(x => x.UnitName, settings.UnitName));
    }

    /// <summary>
    /// Nạp 32 đảng viên lõi.
    /// </summary>
    /// <param name="factory">Host kiểm thử.</param>
    public static async Task SeedMembersAsync(HuyHieuDangApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Set<PartyMember>().AddRange(CoreFixtures.CoreMembers.Select(member => new PartyMember
        {
            FullName = member.FullName,
            DateOfBirth = member.DateOfBirth,
            Gender = ToGender(member.Gender),
            OfficialAdmissionDate = member.OfficialAdmissionDate,
        }));

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Nạp một bộ đợt trao huy hiệu.
    /// </summary>
    /// <param name="factory">Host kiểm thử.</param>
    /// <param name="periods">Các đợt cần nạp; bỏ trống thì nạp bốn đợt chính.</param>
    public static async Task SeedPeriodsAsync(
        HuyHieuDangApiFactory factory, IReadOnlyList<FixturePeriod>? periods = null)
    {
        ArgumentNullException.ThrowIfNull(factory);

        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Set<AwardPeriod>().AddRange((periods ?? CoreFixtures.MainPeriods()).Select(period => new AwardPeriod
        {
            Name = period.Name,
            FromDay = period.FromDay,
            FromMonth = period.FromMonth,
            ToDay = period.ToDay,
            ToMonth = period.ToMonth,
        }));

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Nạp trọn bộ lõi: cài đặt, bốn đợt chính và 32 đảng viên.
    /// </summary>
    /// <param name="factory">Host kiểm thử.</param>
    /// <param name="settingsKey">Khóa bộ cài đặt trong <c>settings.json</c>.</param>
    public static async Task SeedAllAsync(HuyHieuDangApiFactory factory, string settingsKey = "default")
    {
        await ResetAsync(factory, settingsKey);
        await SeedPeriodsAsync(factory);
        await SeedMembersAsync(factory);
    }

    private static Gender? ToGender(string? value) => value switch
    {
        "Nam" => Gender.Male,
        "Nữ" => Gender.Female,
        _ => null,
    };
}
