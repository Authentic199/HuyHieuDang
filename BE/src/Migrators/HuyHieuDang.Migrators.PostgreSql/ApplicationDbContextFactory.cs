using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HuyHieuDang.Migrators.PostgreSql;

/// <summary>
/// Cung cấp <see cref="ApplicationDbContext"/> cho các lệnh `dotnet ef` lúc thiết kế.
/// Chuỗi kết nối lấy từ biến môi trường, không bao giờ lưu trong mã nguồn.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    private const string ConnectionStringVariable = "DatabaseSettings__SqlSettings__ConnectionStrings__DefaultConnection";

    public ApplicationDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable)
            ?? "Host=localhost;Port=5432;Database=huyhieudang;Username=postgres;Password=postgres;";

        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString, builder => builder.MigrationsAssembly(typeof(ApplicationDbContextFactory).Assembly.GetName().Name))
            .Options;

        return new ApplicationDbContext(options);
    }
}
