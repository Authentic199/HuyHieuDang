# Nhật ký thay đổi

## Tài liệu này dùng để làm gì

Ghi lại những thay đổi **người dùng hoặc thành viên trong đội nhìn thấy được**: màn hình mới, endpoint mới, quy tắc nghiệp vụ đổi, cách chạy đổi. Không ghi từng commit — muốn xem commit thì đọc lịch sử git.

Mỗi giai đoạn (stage) một mục, mới nhất ở trên. Trong mỗi mục chia theo: **Thêm**, **Đổi**, **Sửa lỗi**, **Bỏ**. Ngày theo `dd/MM/yyyy`.

Ai merge một thay đổi đáng ghi thì ghi luôn một dòng vào mục **Chưa phát hành**. Technical Writer gom lại khi đóng giai đoạn.

---

## Chưa phát hành

### Đổi

- Hợp đồng API lên **v1.1**: chốt 10 điểm tài liệu nghiệp vụ chưa quy định (OQ-1…OQ-10) vào mục 12, ghi 8 quyết định kỹ thuật của bộ khung Backend vào mục 13. Chi tiết trong `docs/api-contract.md` mục 15.
  - Phản hồi xem trước import đổi hình dạng: `errorRows[*].errorCode` + `field` gộp thành mảng `errorRows[*].errors[]` — một dòng lỗi trả đủ mọi lý do thay vì chỉ lý do đầu tiên.
  - Thêm khóa `Mes.Import.Invalid.NoDataRows` cho file chỉ có dòng tiêu đề; `Mes.Import.Invalid.Empty` giờ chỉ dùng cho file rỗng.
  - Tìm theo tên: không phân biệt hoa thường, **có** phân biệt dấu. Sắp xếp Họ tên dùng đối chiếu tiếng Việt ICU `vi`.
  - Sửa cổng Backend khi chạy dev trong tài liệu: 5000 → **8080**, cho khớp `launchSettings.json` và `docker-compose.yml`.

### Thêm

- `README.md`: cách chạy bằng `docker compose`, cách chạy chế độ phát triển, cách sao lưu và khôi phục bằng `pg_dump` / `psql`.
- `CHANGELOG.md` (tài liệu này).

---

## Giai đoạn 0 — Nền móng · 19/09/2026

Dựng nền để bốn agent làm song song. Chưa có nghiệp vụ nào chạy được: không có entity đảng viên, không có đợt trao huy hiệu, không có màn hình thật.

### Thêm

- **Bộ khung Backend** (`BE/`) — ASP.NET Core 8 dựng từ boilerplate, đổi tên `HuyHieuDang.*`, giữ `Core` / `Infrastructure` / `Migrators` / `Web` / `tests`. Đã lược bỏ các facade và module không dùng, bỏ `Migrators.MySql`. Migration `InitialSchema` cho lược đồ định danh, seeder tạo tài khoản `admin` / `Admin@123`. API nghe cổng 8080, tài liệu ở `/docs`.
- **Bộ khung Frontend** (`FE/`) — React + Vite + TypeScript + Ant Design 5. Theme lấy từ bộ thiết kế, layout chung (header + sider 5 mục), định tuyến 5 màn có chặn khi chưa đăng nhập, màn Đăng nhập, tầng gọi API có kiểu. Từng màn còn là khối giữ chỗ.
- **`docker-compose.yml`** ba service `postgres` + `be` + `fe`, kèm `.env.example`. Service `fe` tạm dùng nginx phục vụ trang giữ chỗ.
- **Hợp đồng API v1.0** (`docs/api-contract.md`, `docs/openapi.yaml`) — 28 endpoint chia 7 nhóm, phủ toàn bộ UC-00 → UC-51.
- **Kế hoạch kiểm thử** (`docs/test-plan.md`) và **bộ dữ liệu biên** (`tests/fixtures/`) — 120 ca logic, 110 ca tích hợp API, 6 luồng E2E, fixture Excel sinh lại được từng byte.

### Cố ý chưa làm

Quản lý người dùng và phân quyền, đánh dấu đã trao, chốt đợt, trừ tuổi đảng gián đoạn, trao sớm, truy tặng, in tờ trình — tất cả nằm ngoài phạm vi v1.
