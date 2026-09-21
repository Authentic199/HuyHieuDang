using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure;
using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Facades.Persistence.Interceptors;
using HuyHieuDang.Infrastructure.Modules.Users.Seeders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Dựng toàn bộ API trên một PostgreSQL thật trong container, chạy migration và seed
/// như lúc khởi động thật, rồi thay <see cref="IDateTimeProvider"/> bằng bản cố định (T-FIX-3).
/// </summary>
public sealed class HuyHieuDangApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>
    /// Tài khoản admin được seed trong host kiểm thử.
    /// </summary>
    public const string AdminUsername = "admin";

    /// <summary>
    /// Mật khẩu tương ứng. Chỉ tồn tại trong bộ kiểm thử.
    /// </summary>
    public const string AdminPassword = "Kiem@Thu123";

    /// <summary>
    /// Khóa ký token dùng trong host kiểm thử; các bài kiểm thử tự ký token giả bằng khóa này.
    /// </summary>
    public const string JwtKey = "khoa-ky-chi-dung-cho-kiem-thu-tich-hop-du-dai-256-bit";

    /// <summary>
    /// Ngày "hôm nay" mà mọi bài kiểm thử trong bộ này nhìn thấy.
    /// </summary>
    public static readonly DateOnly FixedToday = new(2026, 9, 19);

    /// <summary>
    /// Câu lệnh SQL mà EF Core đã gửi xuống PostgreSQL trên host này (T51). Bài kiểm thử gọi
    /// <see cref="SqlCapture.Clear"/> ngay trước lời gọi cần đo.
    /// </summary>
    public SqlCapture CapturedSql { get; } = new();

    private const string JwtRefreshKey = "khoa-refresh-chi-dung-cho-kiem-thu-tich-hop-du-dai-256-bit";

    private readonly PostgreSqlContainer database = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("huyhieudang_test")
        .WithUsername("huyhieudang")
        .WithPassword("huyhieudang")
        .Build();

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        await database.StartAsync();

        // Program.cs nạp các tệp Configurations/*.json rồi mới tới biến môi trường, nên chỉ
        // biến môi trường mới ghi đè được giá trị rỗng trong tệp — giống hệt cách docker-compose làm.
        Environment.SetEnvironmentVariable("DatabaseSettings__SqlSettings__ConnectionStrings__DefaultConnection", database.GetConnectionString());
        Environment.SetEnvironmentVariable("DatabaseSettings__SqlSettings__UseAutoMigration", "true");
        Environment.SetEnvironmentVariable("SecuritySettings__JwtSettingOptions__Default__Key", JwtKey);
        Environment.SetEnvironmentVariable("SecuritySettings__JwtSettingOptions__Default__RefreshKey", JwtRefreshKey);
        Environment.SetEnvironmentVariable(AdminUserSeeder.AdminPasswordVariable, AdminPassword);

        // Host kiểm thử không chạy hết thân Program.cs nên phải tự áp migration và seed.
        await Services.InitializeDatabasesAsync();
    }

    /// <inheritdoc/>
    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        await database.DisposeAsync();
    }

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(FixedToday));

            // EF Core 8 chưa lấy IInterceptor từ DI, mà AddDbContextPool lại đăng ký
            // DbContextOptions bằng TryAdd, nên phải gỡ bản cũ rồi dựng lại kèm bộ ghi câu lệnh (T51).
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.AddDbContextPool<ApplicationDbContext>((provider, options) => options
                .UseNpgsql(
                    database.GetConnectionString(),
                    npgsql => npgsql.MigrationsAssembly("HuyHieuDang.Migrators.PostgreSql"))
                .AddInterceptors(
                    provider.GetRequiredService<UpdatedAtInterceptor>(),
                    new SqlCaptureInterceptor(CapturedSql)));
        });
    }
}
