# HuyHieuDang — Hệ thống hỗ trợ xét trao Huy hiệu Đảng

Từ danh sách đảng viên, hệ thống tự lọc ra ai đủ điều kiện nhận huy hiệu trong mỗi đợt trao huy hiệu, rồi xuất Excel.

## Tài liệu này dùng để làm gì

Hướng dẫn **cài và chạy** hệ thống trên một máy: chạy cả cụm bằng `docker compose`, chạy chế độ phát triển (Backend và Frontend riêng), sao lưu và khôi phục dữ liệu. Đọc từ trên xuống là chạy được.

Cần tìm thứ khác:

| Muốn biết | Đọc |
|---|---|
| Nghiệp vụ: quy tắc QT1–QT11, use case | `docs/2026-09-17-huyhieudang-business-design.md` |
| Hợp đồng API giữa Backend và Frontend | `docs/api-contract.md` (bản máy đọc: `docs/openapi.yaml`) |
| Kế hoạch kiểm thử và bộ dữ liệu biên | `docs/test-plan.md`, `tests/fixtures/README.md` |
| Đội ngũ, danh sách task | `docs/2026-09-19-team-and-task-plan.md` |
| Nhật ký thay đổi | `CHANGELOG.md` |

## Cần có sẵn

| Việc | Cần |
|---|---|
| Chạy bằng docker compose | Docker Engine 24 trở lên, có sẵn `docker compose` |
| Chạy chế độ phát triển | .NET SDK 8 trở lên, Node.js 20 trở lên, một PostgreSQL 16 |

---

## 1. Chạy cả cụm bằng docker compose

Cụm gồm ba container: `postgres` (dữ liệu), `be` (API), `fe` (giao diện).

### Bước 1 — tạo tệp `.env`

```bash
cp .env.example .env
```

Mở `.env` và sửa ít nhất ba dòng sau. Đừng để nguyên giá trị mẫu:

| Biến | Đặt gì |
|---|---|
| `POSTGRES_PASSWORD` | Mật khẩu cơ sở dữ liệu |
| `JWT_KEY`, `JWT_REFRESH_KEY` | Hai chuỗi ngẫu nhiên, mỗi chuỗi từ 32 ký tự trở lên |
| `SWAGGER_PASSWORD` | Mật khẩu mở trang tài liệu API khi chạy môi trường thật |

Sinh nhanh một chuỗi ngẫu nhiên:

```bash
openssl rand -hex 32
```

Tệp `.env` **không được commit** — `.gitignore` đã chặn sẵn.

### Bước 2 — bật cụm

```bash
docker compose up -d --build
```

Lần đầu mất vài phút vì phải dựng ảnh Backend. Kiểm tra:

```bash
docker compose ps
```

Đủ ba container, `postgres` phải ở trạng thái `healthy`:

```
NAME                   IMAGE                COMMAND                  SERVICE    STATUS                    PORTS
huyhieudang-be         huyhieudang-be       "dotnet HuyHieuDang.…"   be         Up 5 seconds              0.0.0.0:8080->8080/tcp
huyhieudang-fe         nginx:alpine         "/docker-entrypoint.…"   fe         Up 5 seconds              0.0.0.0:3000->80/tcp
huyhieudang-postgres   postgres:16-alpine   "docker-entrypoint.s…"   postgres   Up 11 seconds (healthy)   0.0.0.0:5433->5432/tcp
```

### Bước 3 — mở hệ thống

| Địa chỉ | Là gì |
|---|---|
| http://localhost:3000 | Giao diện cho cán bộ |
| http://localhost:8080/docs | Trang tài liệu API (Swagger) |
| http://localhost:8080/healthz | Kiểm tra sống chết, trả `{"status":"Healthy",…}` |

Tài khoản khởi tạo: **`admin` / `Admin@123`**. Đổi mật khẩu ngay sau lần đăng nhập đầu tiên trên máy dùng thật.

Kiểm tra nhanh bằng dòng lệnh:

```bash
curl http://localhost:8080/healthz
```

> Trang `/docs` **không hỏi mật khẩu** khi `ASPNETCORE_ENVIRONMENT=Development` (mặc định của `.env.example`). Đặt biến này thành `Production` thì `/docs` yêu cầu đăng nhập Basic bằng `SWAGGER_USERNAME` / `SWAGGER_PASSWORD`.

### Tắt cụm

```bash
docker compose down            # dừng, giữ nguyên dữ liệu
docker compose down -v         # dừng và XÓA SẠCH dữ liệu — cân nhắc kỹ
```

### Khi cổng 5432 đã bị chiếm

Máy đã chạy sẵn một PostgreSQL khác thì container `postgres` không bật được, báo `port is already allocated`. Sửa `.env`:

```
POSTGRES_PORT=5433
```

Rồi `docker compose up -d` lại. Con số này chỉ là cổng **nhìn từ máy thật**; bên trong cụm, `be` vẫn nối tới `postgres:5432` nên không phải sửa gì thêm. Cổng `3000` (giao diện) và `8080` (API) cũng đổi được bằng `FE_PORT` và `BE_PORT` theo đúng cách đó.

Xem cổng nào đang bị chiếm:

```bash
docker ps --format "{{.Names}}\t{{.Ports}}"   # do container khác giữ
netstat -ano | findstr :5432                  # Windows
ss -ltnp | grep 5432                          # Linux
```

### Xem log khi có trục trặc

```bash
docker compose logs -f be         # log Backend, gồm cả migration và seed
docker compose logs postgres
docker compose restart be
```

> **Service `fe`**: từ khi T06 merge, `fe` được dựng thẳng từ `./FE`. Trước đó nó chỉ là một trang nginx giữ chỗ — cụm vẫn lên đủ ba container, nhưng http://localhost:3000 chưa phải giao diện thật.

---

## 2. Chạy chế độ phát triển

Dùng khi sửa mã. Backend và Frontend chạy riêng, sửa tới đâu thấy tới đó.

### Cơ sở dữ liệu

Chỉ bật mỗi PostgreSQL trong cụm, khỏi phải cài riêng:

```bash
docker compose up -d postgres
```

### Backend

```bash
cd BE
dotnet build HuyHieuDang.sln
dotnet test HuyHieuDang.sln
```

Chạy API thì phải **đặt bốn biến môi trường trước** — xem ô cảnh báo ngay bên dưới:

```bash
# Bash
export DatabaseSettings__SqlSettings__ConnectionStrings__DefaultConnection="Host=localhost;Port=5433;Database=huyhieudang;Username=huyhieudang;Password=<mật khẩu trong .env>;Include Error Detail=true;"
export DatabaseSettings__SqlSettings__UseAutoMigration=true
export SecuritySettings__JwtSettingOptions__Default__Key="<chuỗi ngẫu nhiên từ 32 ký tự>"
export SecuritySettings__JwtSettingOptions__Default__RefreshKey="<chuỗi ngẫu nhiên khác>"

dotnet run --project src/Web/HuyHieuDang.Web.csproj
```

```powershell
# PowerShell
$env:DatabaseSettings__SqlSettings__ConnectionStrings__DefaultConnection = "Host=localhost;Port=5433;Database=huyhieudang;Username=huyhieudang;Password=<mật khẩu trong .env>;Include Error Detail=true;"
$env:DatabaseSettings__SqlSettings__UseAutoMigration = "true"
$env:SecuritySettings__JwtSettingOptions__Default__Key = "<chuỗi ngẫu nhiên từ 32 ký tự>"
$env:SecuritySettings__JwtSettingOptions__Default__RefreshKey = "<chuỗi ngẫu nhiên khác>"

dotnet run --project src/Web/HuyHieuDang.Web.csproj
```

API nghe ở **http://localhost:8080** (số cổng nằm trong `BE/src/Web/Configurations/appsettings.json`, khóa `Urls`), tài liệu ở http://localhost:8080/docs. Migration chạy lúc khởi động, seeder tạo tài khoản `admin`.

Cổng `5433` trong chuỗi kết nối là `POSTGRES_PORT` ở `.env`; để mặc định `5432` thì sửa lại cho khớp.

> **Vì sao phải tự đặt biến?** `BE/src/Web/Properties/launchSettings.json` đang có dòng chú thích kiểu `//`, mà `dotnet run` đọc tệp này bằng bộ đọc JSON thuần nên báo `'/' is an invalid start of a property name` rồi **bỏ qua toàn bộ profile**. Khóa JWT trong đó không được nạp, ứng dụng tắt ngay với `IDX10703: key length is zero`. Bỏ các dòng `//` trong `launchSettings.json` là hết, lúc đó `dotnet run` chạy thẳng không cần đặt biến. Chạy bằng `docker compose` không dính lỗi này vì biến lấy từ `.env`.

Thêm migration mới:

```bash
cd BE
dotnet ef migrations add <TenMigration> --project src/Migrators/HuyHieuDang.Migrators.PostgreSql --startup-project src/Web
```

### Frontend

```bash
cd FE
npm install
cp .env.example .env.development
npm run dev
```

Giao diện ở **http://localhost:5173**. Vite chuyển tiếp mọi lời gọi `/api` sang Backend.

**Sửa `FE/.env.development`** cho khớp cổng Backend:

```
VITE_API_BASE_URL=/api
VITE_DEV_API_PROXY=http://localhost:8080
```

Các lệnh khác:

```bash
npm run build          # dựng gói tĩnh vào dist/
npm run preview        # xem thử gói đã dựng, cổng 4173
npm run lint
npm run format
npx playwright install chromium   # lần đầu
npm run test:e2e
```

---

## 3. Sao lưu và khôi phục dữ liệu

Toàn bộ dữ liệu nằm trong một cơ sở dữ liệu PostgreSQL, gắn vào volume `huyhieudang_postgres-data`. Sao lưu là đổ ra một tệp `.sql`.

### Sao lưu

```bash
docker compose exec -T postgres pg_dump -U huyhieudang -d huyhieudang --clean --if-exists > backup-2026-09-19.sql
```

- `-U` và `-d` lấy đúng giá trị `POSTGRES_USER` và `POSTGRES_DB` trong `.env`.
- `-T` **bắt buộc** khi chuyển hướng ra tệp, nếu không tệp nhận sẽ dính ký tự điều khiển của terminal.
- `--clean --if-exists` khiến tệp tự dọn bảng cũ trước khi nạp lại, nhờ vậy khôi phục đè lên được.

Đặt tên tệp theo ngày. Chép tệp sang ổ khác hoặc USB — để cùng một máy thì hỏng máy là mất cả hai.

Sao lưu định kỳ trên Linux, mỗi ngày một lần lúc 22 giờ (`crontab -e`):

```
0 22 * * * cd /duong/dan/HuyHieuDang && docker compose exec -T postgres pg_dump -U huyhieudang -d huyhieudang --clean --if-exists > /backup/huyhieudang-$(date +\%F).sql
```

### Khôi phục

Dừng Backend trước để không ai ghi vào giữa chừng:

```bash
docker compose stop be
docker compose exec -T postgres psql -U huyhieudang -d huyhieudang < backup-2026-09-19.sql
docker compose start be
```

Kiểm tra lại sau khi khôi phục:

```bash
docker compose exec -T postgres psql -U huyhieudang -d huyhieudang -c 'select count(*) from "user";'
curl http://localhost:8080/healthz
```

> **Khôi phục là đè.** Dữ liệu hiện có bị thay bằng dữ liệu trong tệp. Sao lưu trạng thái hiện tại trước khi đè, kể cả khi tin chắc.

### Chuyển sang máy khác

1. Máy cũ: sao lưu theo mục trên.
2. Máy mới: chép cả kho mã, tạo `.env` (chép nguyên từ máy cũ để giữ khóa JWT), chạy `docker compose up -d --build`.
3. Máy mới: khôi phục theo mục trên.

---

## 4. Cấu trúc kho mã

```
HuyHieuDang/
├── BE/                  ASP.NET Core 8 — Core / Infrastructure / Migrators / Web / tests
├── FE/                  React + Vite + TypeScript + Ant Design 5
├── docs/                Tài liệu nghiệp vụ, hợp đồng API, kế hoạch kiểm thử, bộ thiết kế
├── tests/fixtures/      Bộ dữ liệu biên dùng chung cho kiểm thử
├── docker-compose.yml   postgres + be + fe
├── .env.example         Danh sách biến môi trường
└── CHANGELOG.md
```

`BE/README.md` và `FE/README.md` nói kỹ hơn về từng bên.

## 5. Biến môi trường

Tất cả khai trong `.env` ở gốc kho mã, `docker-compose.yml` đọc từ đó.

| Biến | Mặc định | Ý nghĩa |
|---|---|---|
| `POSTGRES_DB` | `huyhieudang` | Tên cơ sở dữ liệu |
| `POSTGRES_USER` | `huyhieudang` | Tài khoản cơ sở dữ liệu |
| `POSTGRES_PASSWORD` | — | **Phải đổi.** Mật khẩu cơ sở dữ liệu |
| `POSTGRES_PORT` | `5432` | Cổng PostgreSQL nhìn từ máy thật |
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` thì trang `/docs` hỏi mật khẩu |
| `BE_PORT` | `8080` | Cổng API |
| `JWT_KEY` | — | **Phải đổi.** Khóa ký thẻ đăng nhập |
| `JWT_REFRESH_KEY` | — | **Phải đổi.** Khóa ký thẻ làm mới |
| `SWAGGER_USERNAME` | `admin` | Tài khoản mở trang `/docs` |
| `SWAGGER_PASSWORD` | — | **Phải đổi.** Mật khẩu mở trang `/docs` |
| `FE_PORT` | `3000` | Cổng giao diện |

Bí mật chỉ nằm trong `.env` và biến môi trường. Không có chuỗi kết nối hay khóa thật nào trong `BE/src/Web/Configurations/`.
