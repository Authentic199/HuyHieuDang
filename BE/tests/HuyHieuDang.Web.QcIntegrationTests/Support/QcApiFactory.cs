using System.Collections.Concurrent;
using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure;
using HuyHieuDang.Infrastructure.Modules.Users.Seeders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace HuyHieuDang.Web.QcIntegrationTests.Support;

/// <summary>
/// Host kiểm thử của QC: dựng trọn API trên một PostgreSQL thật trong container, chạy migration
/// và seed đúng như lúc khởi động thật, rồi thay <see cref="IDateTimeProvider"/> bằng đồng hồ
/// đứng yên (T-FIX-3). Ca nào cần T1 hay T2 thì gọi <see cref="At(DateOnly)"/>: host dẫn xuất
/// vẫn trỏ vào đúng container đó, chỉ khác "hôm nay".
/// </summary>
public sealed class QcApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>Tài khoản admin được seed trong host kiểm thử.</summary>
    public const string AdminUsername = "admin";

    /// <summary>Mật khẩu tương ứng. Chỉ tồn tại trong bộ kiểm thử.</summary>
    public const string AdminPassword = "Kiem@ThuQc123";

    /// <summary>Khóa ký token của host kiểm thử; ca A-002 tự ký token giả bằng khóa khác.</summary>
    public const string JwtKey = "khoa-ky-cua-qc-chi-dung-cho-kiem-thu-tich-hop-du-dai-256-bit";

    private const string JwtRefreshKey = "khoa-refresh-cua-qc-chi-dung-cho-kiem-thu-tich-hop-du-dai-256";

    private readonly ConcurrentDictionary<DateOnly, WebApplicationFactory<Program>> hostsByDate = new();

    private readonly PostgreSqlContainer database = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("huyhieudang_qc")
        .WithUsername("huyhieudang")
        .WithPassword("huyhieudang")
        .Build();

    /// <summary>Chuỗi kết nối tới container — dùng cho ca cần chạy SQL thô (A-904).</summary>
    public string ConnectionString => database.GetConnectionString();

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        await database.StartAsync();

        // Program.cs nạp Configurations/*.json rồi mới tới biến môi trường, nên chỉ biến môi
        // trường mới ghi đè được giá trị rỗng trong tệp — giống hệt cách docker-compose làm.
        Environment.SetEnvironmentVariable(
            "DatabaseSettings__SqlSettings__ConnectionStrings__DefaultConnection", database.GetConnectionString());
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
        foreach (WebApplicationFactory<Program> host in hostsByDate.Values)
        {
            host.Dispose();
        }

        await DisposeAsync();
        await database.DisposeAsync();
    }

    /// <summary>
    /// Host nhìn thấy "hôm nay" là ngày đã cho. Dùng lại host cũ nếu đã dựng, vì mỗi host là
    /// một cây dịch vụ đầy đủ và dựng lại cho từng ca thì bộ kiểm thử chậm hẳn.
    /// </summary>
    /// <param name="today">Ngày cần ép.</param>
    /// <returns>Host tương ứng.</returns>
    public WebApplicationFactory<Program> At(DateOnly today)
        => today == QcClock.T0
            ? this
            : hostsByDate.GetOrAdd(today, date => WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                    services.AddSingleton<IDateTimeProvider>(new QcClock(date)))));

    /// <summary>
    /// Host **không** thay <see cref="IDateTimeProvider"/>: gỡ đúng những đăng ký trỏ tới
    /// <see cref="QcClock"/> nên đăng ký thật của <c>Facades/Common/Startup.cs</c> là cái còn
    /// lại và là cái được dùng. Nhờ vậy ca A-903b đo được provider thật đọc biến môi trường,
    /// thay vì đo lại đồng hồ giả của chính bộ kiểm thử.
    /// <para>
    /// Provider là singleton nên chỉ đọc biến đúng một lần, lúc bộ chứa phụ thuộc tạo ra nó —
    /// người gọi phải đặt biến môi trường **trước** khi gọi hàm này.
    /// </para>
    /// </summary>
    /// <returns>Host mới; người gọi tự giải phóng.</returns>
    public WebApplicationFactory<Program> WithRealDateTimeProvider()
        => WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                List<ServiceDescriptor> fakeClocks = services
                    .Where(descriptor => descriptor.ServiceType == typeof(IDateTimeProvider)
                        && descriptor.ImplementationInstance is QcClock)
                    .ToList();

                // Không có gì để gỡ nghĩa là ConfigureWebHost đã đổi cách thay đồng hồ và ca
                // A-903b sẽ lặng lẽ đo nhầm — dừng ngay còn hơn cho một màu xanh vô nghĩa.
                fakeClocks.ShouldNotBeEmpty(
                    "không tìm thấy đăng ký QcClock nào để gỡ — A-903b sẽ không đo được provider thật");

                foreach (ServiceDescriptor descriptor in fakeClocks)
                {
                    services.Remove(descriptor);
                }
            }));

    /// <summary>Cấp một phạm vi dịch vụ trỏ vào đúng cơ sở dữ liệu của container.</summary>
    /// <returns>Phạm vi dịch vụ; người gọi tự giải phóng.</returns>
    public IServiceScope CreateScope() => Services.CreateScope();

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
            services.AddSingleton<IDateTimeProvider>(new QcClock(QcClock.T0)));
    }
}

/// <summary>
/// Gom mọi lớp kiểm thử của QC vào một bộ để cả bộ dùng chung đúng một container PostgreSQL,
/// và để các ca chạy tuần tự — chúng cùng ghi vào một cơ sở dữ liệu.
/// </summary>
[CollectionDefinition(Name)]
public sealed class QcApiCollection : ICollectionFixture<QcApiFactory>
{
    /// <summary>Tên bộ, dùng ở thuộc tính <c>[Collection]</c> của từng lớp.</summary>
    public const string Name = "QC API";
}
