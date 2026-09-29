# Triển khai HuyHieuDang lên Render + Neon

Tài liệu này hướng dẫn đưa hệ thống lên Internet bằng các gói **miễn phí**:

| Thành phần | Nơi chạy | Loại | Ghi chú |
|---|---|---|---|
| Backend (ASP.NET Core) | Render | Web Service (Docker, Free) | Tự "ngủ" sau 15 phút không có request, lần gọi đầu sau đó mất ~1 phút |
| Frontend (React + Vite) | Render | Static Site (Free) | Không ngủ, luôn mở nhanh |
| PostgreSQL | Neon | Free | 0,5 GB/project, tự tạm nghỉ sau 5 phút rảnh, không hết hạn |

Toàn bộ cấu hình Render nằm trong [`render.yaml`](../render.yaml) ở gốc repo (Render Blueprint).
Mỗi lần push lên `main`, Render tự build lại **chỉ service có tệp thay đổi** (`BE/**` hoặc `FE/**`).

> **Dữ liệu cá nhân.** Danh sách đảng viên là dữ liệu cá nhân. Chỉ đưa dữ liệu thật lên cloud khi đã được
> cơ quan cho phép; khi demo/kiểm thử nên dùng dữ liệu giả.

---

## 1. Chuẩn bị chuỗi kết nối Neon

1. Neon Console → project → **Connect**.
2. Chọn đúng branch (`main`) và database. **Tắt** tuỳ chọn *Connection pooling* để lấy host **trực tiếp**
   (không có `-pooler` trong tên host). Ứng dụng chỉ chạy 1 instance nên không cần pooler, và migration
   chạy ổn định hơn trên kết nối trực tiếp.
3. Neon đưa chuỗi dạng URI:

   ```
   postgresql://neondb_owner:AbC123xyz@ep-cool-name-a1b2c3d4.ap-southeast-1.aws.neon.tech/neondb?sslmode=require
   ```

4. Đổi sang dạng **key=value** mà Npgsql dùng (ứng dụng KHÔNG đọc được dạng URI):

   ```
   Host=ep-cool-name-a1b2c3d4.ap-southeast-1.aws.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=AbC123xyz;SSL Mode=Require
   ```

   | Phần trong URI | Khoá Npgsql |
   |---|---|
   | sau `postgresql://` đến trước `:` | `Username` |
   | sau `:` đến trước `@` | `Password` |
   | sau `@` đến trước `/` | `Host` |
   | sau `/` đến trước `?` | `Database` |
   | `sslmode=require` | `SSL Mode=Require` |

   Nếu mật khẩu có ký tự `;` thì bọc giá trị trong nháy kép: `Password="ab;cd"`.

Nên tạo project Neon ở region **AWS Asia Pacific (Singapore)** cho gần server Render (cũng ở Singapore).

---

## 2. Tạo Blueprint trên Render

1. Đăng nhập <https://dashboard.render.com> bằng GitHub, cấp quyền cho repo `HuyHieuDang`.
2. **New → Blueprint** → chọn repo → branch `main`. Render đọc `render.yaml` và liệt kê 2 service:
   `huyhieudang-api`, `huyhieudang-web`.
3. Render hỏi giá trị cho các biến `sync: false` — điền theo bảng ở **mục 3**.
   - URL của service có dạng `https://<tên-service>.onrender.com`. Nếu tên đã có người dùng, Render thêm
     hậu tố (ví dụ `huyhieudang-api-x7k2.onrender.com`). Lần đầu cứ điền theo tên mặc định, sai thì sửa ở **bước 5**.
4. Bấm **Apply / Deploy Blueprint**. Backend build Docker mất khoảng 5–10 phút; Static Site 1–3 phút.
5. Khi cả hai đã có URL thật (xem ở đầu trang của từng service), đối chiếu lại:
   - `huyhieudang-api` → Environment → `CorsSettings__AllowedOrigins` = URL thật của web.
   - `huyhieudang-web` → Environment → `VITE_API_BASE_URL` = URL thật của api + `/api`.
   - Sửa biến của **Static Site** thì phải **Manual Deploy → Deploy latest commit** vì Vite nhúng biến lúc build.
     Sửa biến của backend thì Render tự khởi động lại.

---

## 3. Biến môi trường

### 3.1. `huyhieudang-api` (Backend)

| Biến | Giá trị | Nguồn | Bắt buộc |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | `render.yaml` | ✔ |
| `PORT` | `8080` | `render.yaml` | ✔ |
| `TZ` | `Asia/Ho_Chi_Minh` | `render.yaml` | |
| `DatabaseSettings__SqlSettings__ConnectionStrings__DefaultConnection` | Chuỗi Neon dạng key=value (mục 1) | **Bạn nhập** | ✔ |
| `DatabaseSettings__SqlSettings__UseAutoMigration` | `true` | `render.yaml` | ✔ |
| `SecuritySettings__JwtSettingOptions__Default__Key` | Chuỗi ngẫu nhiên | Render tự sinh | ✔ |
| `SecuritySettings__JwtSettingOptions__Default__RefreshKey` | Chuỗi ngẫu nhiên | Render tự sinh | ✔ |
| `ADMIN_PASSWORD` | Mật khẩu mạnh cho tài khoản `admin` | **Bạn nhập** | ✔ |
| `CorsSettings__AllowedOrigins` | `https://huyhieudang-web.onrender.com` (không có `/` cuối) | **Bạn nhập** | ✔ |
| `SwaggerSettings__Enable` | `true` (đặt `false` để tắt trang `/docs`) | `render.yaml` | |
| `SwaggerSettings__Credentials__Username` | `admin` | `render.yaml` | |
| `SwaggerSettings__Credentials__Password` | Mật khẩu mở trang `/docs` | **Bạn nhập** | ✔ |

Ghi chú:

- `ADMIN_PASSWORD` chỉ dùng khi seed tài khoản `admin` lần đầu (DB trống). Đổi biến này sau khi đã seed
  **không** đổi mật khẩu cũ.
- Hai khoá JWT do Render sinh một lần và giữ nguyên qua các lần deploy. Đổi khoá = mọi phiên đăng nhập hết hiệu lực.
- **Không** đặt `HUYHIEUDANG_TEST_TODAY` trên Render (biến chỉ dùng cho kiểm thử, và bị bỏ qua ở Production).
- Tên biến dùng `__` (hai dấu gạch dưới) thay cho `:` của cấu hình .NET, ghi đè giá trị trong
  `BE/src/Web/Configurations/*.json`.

### 3.2. `huyhieudang-web` (Frontend — Static Site)

| Biến | Giá trị | Nguồn | Bắt buộc |
|---|---|---|---|
| `NODE_VERSION` | `22` | `render.yaml` | |
| `VITE_API_BASE_URL` | `https://huyhieudang-api.onrender.com/api` | **Bạn nhập** | ✔ |

Rule rewrite `/* → /index.html` (để F5 ở trang con không lỗi 404) và header cache cho `/assets/*`
đã khai báo sẵn trong `render.yaml`.

### 3.3. Sửa biến sau này

Dashboard → chọn service → **Environment** → sửa → **Save changes**.
Biến có `value` cố định trong `render.yaml` nên sửa trong tệp rồi push, không sửa trên Dashboard
(lần đồng bộ Blueprint sau sẽ ghi đè).

---

## 4. Migration cơ sở dữ liệu

### Phương án mặc định — tự migrate khi khởi động (đang bật)

Backend đã có sẵn cơ chế này (`InitializeDatabasesAsync` trong `BE/src/Infrastructure/Startup.cs`):
khi `DatabaseSettings__SqlSettings__UseAutoMigration=true`, mỗi lần khởi động app sẽ

1. kiểm tra migration còn thiếu → `MigrateAsync()` nếu có;
2. chạy seeder (tài khoản `admin`, cài đặt mặc định 30/90/5).

Lần deploy đầu tiên vào DB Neon trống sẽ tự tạo toàn bộ bảng — **không cần làm gì thêm**.

Vì sao phù hợp ở đây: chỉ chạy **1 instance** nên không có nguy cơ hai instance cùng migrate;
nếu migration lỗi, app không lên, health check `/livez` thất bại và Render **giữ nguyên bản đang chạy cũ**.

Quy trình khi thêm migration mới:

```bash
cd BE
dotnet ef migrations add <TenMigration> \
  -p src/Migrators/HuyHieuDang.Migrators.PostgreSql -s src/Web
# commit + push lên main → Render build lại → app tự áp migration khi khởi động
```

**Nên làm trước migration có thay đổi/xoá dữ liệu:** Neon Console → Branches → **Create branch** từ `main`
(bản chụp tức thời, miễn phí). Nếu hỏng thì khôi phục từ branch đó.

### Phương án thay thế — migrate thủ công từ máy cá nhân

Dùng khi muốn kiểm soát chặt (xem trước SQL, chọn thời điểm chạy):

1. Trên Render đặt `DatabaseSettings__SqlSettings__UseAutoMigration=false`.
2. Từ máy cá nhân, trỏ lệnh `dotnet ef` vào Neon (factory lúc thiết kế đọc đúng biến này):

   ```powershell
   # PowerShell
   cd BE
   $env:DatabaseSettings__SqlSettings__ConnectionStrings__DefaultConnection = "Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require"
   dotnet ef database update -p src/Migrators/HuyHieuDang.Migrators.PostgreSql -s src/Web
   Remove-Item Env:DatabaseSettings__SqlSettings__ConnectionStrings__DefaultConnection
   ```

3. Hoặc sinh script SQL để đọc trước rồi chạy trong **Neon Console → SQL Editor**:

   ```bash
   cd BE
   dotnet ef migrations script --idempotent \
     -p src/Migrators/HuyHieuDang.Migrators.PostgreSql -s src/Web -o migrate.sql
   ```

   `--idempotent` giúp script chạy lại nhiều lần vẫn an toàn (bỏ qua migration đã áp).

Seeder (admin + cài đặt mặc định) vẫn chạy khi app khởi động, dù tắt auto-migration.

---

## 5. Kiểm tra sau khi deploy

| Kiểm tra | Kỳ vọng |
|---|---|
| `https://<api>.onrender.com/livez` | HTTP 204 |
| `https://<api>.onrender.com/healthz` | JSON, `Healthy` (có kiểm tra kết nối DB) |
| `https://<api>.onrender.com/docs` | Hỏi Basic Auth → mở Swagger |
| `https://<web>.onrender.com` | Trang đăng nhập; đăng nhập `admin` / `ADMIN_PASSWORD` |
| Log backend (Dashboard → Logs) | Có dòng `[Migration] Auto-migration succeeded` hoặc `No pending migrations found` |

---

## 6. Sự cố thường gặp

| Hiện tượng | Nguyên nhân / cách xử lý |
|---|---|
| Lần mở đầu tiên đăng nhập rất lâu hoặc báo lỗi mạng | Backend đang "ngủ", đợi ~1 phút rồi thử lại |
| Trình duyệt báo lỗi **CORS** | `CorsSettings__AllowedOrigins` sai URL (thừa `/` cuối, sai hậu tố, thiếu `https://`) |
| FE gọi API về chính domain web (404 `/api/...`) | Chưa đặt `VITE_API_BASE_URL`, hoặc đặt rồi nhưng chưa deploy lại Static Site |
| Backend log `DefaultConnection is not configured` | Thiếu biến chuỗi kết nối |
| Backend log lỗi `Format of the initialization string` | Dán nhầm URI `postgresql://...`, đổi sang key=value (mục 1) |
| Backend log lỗi SSL | Thiếu `SSL Mode=Require` |
| Neon báo hết compute | Gói Free 100 CU-giờ/tháng; hết thì DB dừng đến kỳ sau |

## 7. Giới hạn gói Free cần nhớ

- Render Web Service: 750 giờ/tháng cho cả workspace, ngủ sau 15 phút, ổ đĩa không lưu lâu dài
  (app đã xử lý Excel trong bộ nhớ nên không bị ảnh hưởng; log file trong container sẽ mất khi deploy lại).
- Neon: 0,5 GB lưu trữ/project, 100 CU-giờ/tháng.
- Sao lưu định kỳ: `pg_dump "<chuỗi URI Neon>" > backup.sql` từ máy có cài PostgreSQL client.
