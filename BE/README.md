# HuyHieuDang — Backend

API ASP.NET Core 8 cho hệ thống hỗ trợ xét trao Huy hiệu Đảng.

## Cấu trúc

| Dự án | Vai trò |
| --- | --- |
| `src/Core` | Kiểu nền, ngoại lệ, helper — không phụ thuộc hạ tầng. |
| `src/Infrastructure` | `Facades/` (kỹ thuật) và `Modules/` (nghiệp vụ), EF Core, DbContext. |
| `src/Migrators/HuyHieuDang.Migrators.PostgreSql` | Migration EF Core cho PostgreSQL. |
| `src/Web` | Controller, cấu hình, điểm khởi chạy. |
| `tests/HuyHieuDang.Infrastructure.UnitTests` | Kiểm thử đơn vị. |
| `tests/HuyHieuDang.Infrastructure.IntegrationTests` | Kiểm thử tích hợp. |

## Cấu hình

Tệp trong `src/Web/Configurations/` chỉ chứa giá trị mặc định an toàn. Mọi bí mật (chuỗi kết nối,
khoá JWT, tài khoản mở trang tài liệu) đặt qua biến môi trường, tên biến dùng dấu `__` để xuống cấp:

```
DatabaseSettings__SqlSettings__ConnectionStrings__DefaultConnection=Host=...;Port=5432;Database=...;Username=...;Password=...;
SecuritySettings__JwtSettingOptions__Default__Key=...
SecuritySettings__JwtSettingOptions__Default__RefreshKey=...
SwaggerSettings__Credentials__Username=...
SwaggerSettings__Credentials__Password=...
```

## Lệnh thường dùng

```bash
dotnet build HuyHieuDang.sln
dotnet test HuyHieuDang.sln
dotnet run --project src/Web/HuyHieuDang.Web.csproj

# Thêm migration mới
dotnet ef migrations add <TenMigration> \
  --project src/Migrators/HuyHieuDang.Migrators.PostgreSql \
  --startup-project src/Migrators/HuyHieuDang.Migrators.PostgreSql \
  --output-dir Migrations
```

Chạy cả stack (postgres + be + fe) bằng `docker compose up -d` ở thư mục gốc kho mã.

## Tài khoản khởi tạo

Seeder tạo sẵn duy nhất một tài khoản quản trị: `admin` / `Admin@123`. Đổi mật khẩu ngay sau lần
đăng nhập đầu tiên trên môi trường thật.
