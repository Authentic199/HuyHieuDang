using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AwardPeriodEntity = HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities.AwardPeriod;

namespace HuyHieuDang.Web.QcIntegrationTests.Support;

/// <summary>
/// Dọn và nạp cơ sở dữ liệu kiểm thử. Mỗi ca tự dựng trạng thái mình cần rồi mới gọi API, nên
/// thứ tự chạy không ảnh hưởng kết quả — yêu cầu "chạy lại được" của mục 2 kế hoạch kiểm thử.
/// Nạp thẳng qua <see cref="ApplicationDbContext"/> để không phụ thuộc endpoint của nhóm khác.
/// </summary>
public static class QcDb
{
    /// <summary>Xóa sạch đảng viên và đợt, đưa cài đặt về bộ đã chọn.</summary>
    /// <param name="factory">Host kiểm thử.</param>
    /// <param name="settingsKey">Khóa bộ cài đặt trong <c>settings.json</c>.</param>
    public static async Task ResetAsync(QcApiFactory factory, string settingsKey = "default")
    {
        ArgumentNullException.ThrowIfNull(factory);

        using IServiceScope scope = factory.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Set<PartyMember>().ExecuteDeleteAsync();
        await dbContext.Set<AwardPeriodEntity>().ExecuteDeleteAsync();

        QcSettings settings = QcFixtures.Settings[settingsKey];
        await ApplySettingsAsync(dbContext, settings.StartYears, settings.EndYears, settings.StepYears, settings.UnitName);
    }

    /// <summary>Ghi thẳng một bộ cài đặt vào bảng, bỏ qua endpoint cài đặt.</summary>
    /// <param name="factory">Host kiểm thử.</param>
    /// <param name="startYears">Mốc bắt đầu.</param>
    /// <param name="endYears">Mốc kết thúc.</param>
    /// <param name="stepYears">Bước nhảy.</param>
    /// <param name="unitName">Tên đơn vị.</param>
    public static async Task SetSettingsAsync(
        QcApiFactory factory, int startYears, int endYears, int stepYears, string? unitName)
    {
        ArgumentNullException.ThrowIfNull(factory);

        using IServiceScope scope = factory.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await ApplySettingsAsync(dbContext, startYears, endYears, stepYears, unitName);
    }

    /// <summary>Nạp một danh sách đảng viên.</summary>
    /// <param name="factory">Host kiểm thử.</param>
    /// <param name="members">Danh sách cần nạp.</param>
    public static async Task SeedMembersAsync(QcApiFactory factory, IEnumerable<QcMember> members)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(members);

        using IServiceScope scope = factory.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Set<PartyMember>().AddRange(members.Select(member => new PartyMember
        {
            FullName = member.FullName,
            DateOfBirth = member.DateOfBirth,
            Gender = ToGender(member.Gender),
            OfficialAdmissionDate = member.OfficialAdmissionDate,
        }));

        await dbContext.SaveChangesAsync();
    }

    /// <summary>Nạp một bộ đợt trao huy hiệu.</summary>
    /// <param name="factory">Host kiểm thử.</param>
    /// <param name="periods">Các đợt cần nạp.</param>
    public static async Task SeedPeriodsAsync(QcApiFactory factory, IEnumerable<QcPeriod> periods)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(periods);

        using IServiceScope scope = factory.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Set<AwardPeriodEntity>().AddRange(periods.Select(period => new AwardPeriodEntity
        {
            Name = period.Name,
            FromDay = period.FromDay,
            FromMonth = period.FromMonth,
            ToDay = period.ToDay,
            ToMonth = period.ToMonth,
        }));

        await dbContext.SaveChangesAsync();
    }

    /// <summary>Trạng thái chuẩn của hầu hết ca: cài đặt mặc định, bốn đợt chính, 32 đảng viên lõi.</summary>
    /// <param name="factory">Host kiểm thử.</param>
    /// <param name="settingsKey">Khóa bộ cài đặt.</param>
    public static async Task SeedCoreAsync(QcApiFactory factory, string settingsKey = "default")
    {
        await ResetAsync(factory, settingsKey);
        await SeedPeriodsAsync(factory, QcFixtures.MainPeriods);
        await SeedMembersAsync(factory, QcFixtures.CoreMembers);
    }

    /// <summary>Bộ lõi cộng bộ lớn: 1232 đảng viên, dùng cho phân trang và tìm kiếm.</summary>
    /// <param name="factory">Host kiểm thử.</param>
    public static async Task SeedCoreAndBulkAsync(QcApiFactory factory)
    {
        await SeedCoreAsync(factory);
        await SeedMembersAsync(factory, QcFixtures.BulkMembers);
    }

    /// <summary>Đếm số đảng viên đang có trong bảng, không qua API.</summary>
    /// <param name="factory">Host kiểm thử.</param>
    /// <returns>Số bản ghi.</returns>
    public static async Task<int> CountMembersAsync(QcApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        using IServiceScope scope = factory.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.Set<PartyMember>().CountAsync();
    }

    /// <summary>Đếm số bản ghi cài đặt — A-509 đòi luôn đúng một bản ghi.</summary>
    /// <param name="factory">Host kiểm thử.</param>
    /// <returns>Số bản ghi cài đặt.</returns>
    public static async Task<int> CountSettingsAsync(QcApiFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        using IServiceScope scope = factory.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await dbContext.Set<AppSetting>().CountAsync();
    }

    /// <summary>Chạy một câu lệnh SQL thô — dùng khi nạp 10.000 dòng cho ca hiệu năng A-904.</summary>
    /// <param name="factory">Host kiểm thử.</param>
    /// <param name="sql">Câu lệnh cần chạy.</param>
    public static async Task ExecuteSqlAsync(QcApiFactory factory, string sql)
    {
        ArgumentNullException.ThrowIfNull(factory);

        using IServiceScope scope = factory.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.ExecuteSqlRawAsync(sql);
    }

    /// <summary>Tên bảng thật của một kiểu thực thể, đọc từ mô hình EF.</summary>
    /// <param name="factory">Host kiểm thử.</param>
    /// <typeparam name="TEntity">Kiểu thực thể.</typeparam>
    /// <returns>Tên bảng kèm lược đồ nếu có.</returns>
    public static string TableName<TEntity>(QcApiFactory factory)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(factory);

        using IServiceScope scope = factory.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType =
            dbContext.Model.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException($"Không tìm thấy kiểu thực thể {typeof(TEntity).Name}.");

        string table = entityType.GetTableName()
            ?? throw new InvalidOperationException($"Kiểu {typeof(TEntity).Name} không gắn với bảng nào.");

        string? schema = entityType.GetSchema();

        return schema is null ? $"\"{table}\"" : $"\"{schema}\".\"{table}\"";
    }

    /// <summary>Tên cột thật của một thuộc tính, đọc từ mô hình EF.</summary>
    /// <param name="factory">Host kiểm thử.</param>
    /// <param name="propertyName">Tên thuộc tính trong lớp thực thể.</param>
    /// <typeparam name="TEntity">Kiểu thực thể.</typeparam>
    /// <returns>Tên cột.</returns>
    public static string ColumnName<TEntity>(QcApiFactory factory, string propertyName)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(factory);

        using IServiceScope scope = factory.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType =
            dbContext.Model.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException($"Không tìm thấy kiểu thực thể {typeof(TEntity).Name}.");

        Microsoft.EntityFrameworkCore.Metadata.IProperty property =
            entityType.FindProperty(propertyName)
            ?? throw new InvalidOperationException($"Không tìm thấy thuộc tính {propertyName}.");

        return $"\"{property.GetColumnName()}\"";
    }

    private static async Task ApplySettingsAsync(
        ApplicationDbContext dbContext, int startYears, int endYears, int stepYears, string? unitName)
    {
        int updated = await dbContext.Set<AppSetting>().ExecuteUpdateAsync(setters => setters
            .SetProperty(x => x.StartYears, startYears)
            .SetProperty(x => x.EndYears, endYears)
            .SetProperty(x => x.StepYears, stepYears)
            .SetProperty(x => x.UnitName, unitName));

        if (updated == 0)
        {
            dbContext.Set<AppSetting>().Add(new AppSetting
            {
                StartYears = startYears,
                EndYears = endYears,
                StepYears = stepYears,
                UnitName = unitName,
            });

            await dbContext.SaveChangesAsync();
        }
    }

    private static Gender? ToGender(string? value) => value switch
    {
        "Nam" => Gender.Male,
        "Nữ" => Gender.Female,
        _ => null,
    };
}
