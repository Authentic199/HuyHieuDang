using HuyHieuDang.Infrastructure.Facades.Persistence.Initialization;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;
using Serilog;

namespace HuyHieuDang.Infrastructure.Modules.AppSettings.Seeders;

/// <summary>
/// Tạo bản ghi cài đặt mặc định 30 / 90 / 5 với tên đơn vị trống.
/// Chạy lại nhiều lần không tạo thêm bản ghi: bảng luôn giữ đúng một dòng.
/// </summary>
public class AppSettingSeeder : IDataSeedContributor
{
    private readonly IRepositoryWrapper repositoryWrapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSettingSeeder"/> class.
    /// </summary>
    /// <param name="repositoryWrapper">Cổng truy cập dữ liệu.</param>
    public AppSettingSeeder(IRepositoryWrapper repositoryWrapper) => this.repositoryWrapper = repositoryWrapper;

    /// <inheritdoc/>
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await repositoryWrapper.Repository<AppSetting>().AnyAsync(cancellationToken: cancellationToken))
        {
            return;
        }

        AppSetting appSetting = new()
        {
            StartYears = AppSetting.DefaultStartYears,
            EndYears = AppSetting.DefaultEndYears,
            StepYears = AppSetting.DefaultStepYears,
            UnitName = null,
        };

        await repositoryWrapper.Repository<AppSetting>().AddAsync(appSetting, cancellationToken);

        Log.Information(
            "[Seeding][AppSetting] --> đã tạo cài đặt mặc định {Start}/{End}/{Step}",
            appSetting.StartYears,
            appSetting.EndYears,
            appSetting.StepYears);
    }
}
