# HuyHieuDang — Bộ nhớ dự án

Hệ thống hỗ trợ xét trao Huy hiệu Đảng. Một cán bộ văn phòng đảng ủy dùng, chạy cục bộ trên một máy.
Mục tiêu duy nhất: **từ danh sách đảng viên, tự động lọc ra ai đủ điều kiện nhận huy hiệu trong mỗi đợt, rồi xuất Excel.**

## Đọc trước khi làm bất cứ việc gì

| Tài liệu | Nội dung |
|---|---|
| `docs/2026-09-17-huyhieudang-business-design.md` | **Nguồn sự thật nghiệp vụ.** Quy tắc QT1–QT11, use case UC-xx, mô hình dữ liệu |
| `docs/design-system/Huy Hieu Dang - 9 man hinh.html` | Bộ thiết kế 10 artboard 1440x900 |
| `docs/2026-09-19-team-and-task-plan.md` | Vai trò 5 agent, 32 task, thứ tự phụ thuộc |
| `docs/api-contract.md` | Hợp đồng API (Technical Writer sở hữu — **chưa tồn tại, phải tạo trước**) |

## Cấu trúc

```
HuyHieuDang/
├── BE/    ASP.NET Core Web API — Core / Infrastructure / Migrators / Web / tests
├── FE/    React + Vite + TypeScript + Ant Design 5
├── docs/
└── docker-compose.yml    postgres + be + fe
```

BE dựng từ `D:\Personal\Em\maximus-webapi-boilerplate` — giữ cấu trúc, đổi tên `HuyHieuDang.*`, lược bỏ facade/module không dùng, bỏ `Migrators.MySql`.

## Lệnh

```bash
# BE
cd BE && dotnet build
cd BE && dotnet test
cd BE && dotnet ef migrations add <Ten> -p src/Migrators/Migrators.PostgreSql -s src/Web
cd BE && dotnet ef database update -p src/Migrators/Migrators.PostgreSql -s src/Web

# FE
cd FE && npm install
cd FE && npm run dev
cd FE && npm run build
cd FE && npx playwright test

# Toàn hệ thống
docker compose up -d
docker compose down

# Sao lưu dữ liệu
docker compose exec postgres pg_dump -U postgres huyhieudang > backup.sql
```

## Quy tắc cứng

1. **Tài liệu nghiệp vụ thắng mọi thứ.** Mọi tính toán phải khớp QT1–QT11. Mâu thuẫn → hỏi CEO, không tự quyết.
2. **Logic tính mốc tuổi đảng nằm trong một service thuần**: không phụ thuộc `DbContext`, không gọi `DateTime.Now` trực tiếp (nhận ngày hiện tại qua tham số hoặc abstraction thời gian). Phải có unit test cho các ca biên: 29/02, ngày tròn mốc đúng bằng Từ ngày hoặc Đến ngày, người vượt mốc lớn nhất, đợt sắp tới khi mọi đợt trong năm đã qua.
3. **Danh sách đủ điều kiện không bao giờ được lưu vào bảng** — luôn tính lại khi truy vấn (QT5).
4. **Đợt trao huy hiệu chỉ lưu ngày/tháng**, dùng chung mọi năm. Sửa đợt có hiệu lực ngay cho năm hiện tại (QT6).
5. **Mốc huy hiệu sinh từ cài đặt** Bắt đầu/Kết thúc/Bước (mặc định 30/90/5), không viết cứng dãy mốc (QT1).
6. **Import Excel không kiểm tra trùng** — mọi dòng hợp lệ đều thêm mới (QT9). Xem trước và nạp là hai lời gọi riêng; nạp là một giao dịch.
7. **Giao diện tiếng Việt.** Ngày `dd/MM/yyyy`, ngày/tháng của đợt `dd/MM`, số dùng dấu chấm ngăn nghìn, ô trống hiển thị `—`.
8. **Viết test trước** cho phần tính toán. Không tuyên bố xong khi chưa chạy lệnh kiểm chứng và dán kết quả.

## Không được làm

- Không xây module quản lý user, role hay phân quyền. Chỉ một tài khoản admin seed sẵn.
- Không thêm: đánh dấu đã trao, chốt đợt, trừ tuổi đảng gián đoạn, trao sớm, truy tặng, trạng thái đảng viên, in tờ trình. Tất cả nằm ngoài phạm vi v1 — ghi vào "Cân nhắc cho v2".
- Không hardcode dữ liệu mẫu từ artboard thiết kế vào mã.
- Không viết cứng ngày hôm nay; luôn lấy từ hệ thống.
- Không đẩy mã chưa chạy qua test.
- Không để hai agent sửa cùng một file trong cùng một lúc.

## Quy ước làm việc

- Mỗi task một nhánh: `feat/T<số>-<mô-tả-ngắn>`. Ví dụ `feat/T07-tinh-moc-tuoi-dang`.
- Commit nhỏ, thông điệp tiếng Việt rõ nghĩa, theo dạng `feat:`, `fix:`, `test:`, `docs:`, `chore:`.
- Kết thúc thông điệp commit bằng dòng `Co-Authored-By: Claude Opus 5 <noreply@anthropic.com>`.
- Một task chỉ xong khi có bằng chứng: lệnh đã chạy + kết quả.

## Đội ngũ (Multica squad "HuyHieuDang Team")

| Agent | Sở hữu |
|---|---|
| CEO (leader) | Điều phối, gác cổng, bảo vệ phạm vi. Không viết mã |
| Technical Writer | `docs/api-contract.md`, README, hướng dẫn sử dụng, đồng bộ tài liệu |
| Backend Developer | `BE/` |
| Frontend Developer | `FE/` |
| QC | Unit test logic, integration test API, Playwright E2E (6 luồng bắt buộc) |

Hai cổng duyệt của CEO: sau khi chốt API contract, và sau khi frontend nối API thật.
