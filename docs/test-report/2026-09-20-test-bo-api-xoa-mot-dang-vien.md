# Kiểm thử sau khi bỏ API xóa một đảng viên (T34)

Lệnh đã chạy, tại `BE/`:

```
dotnet build HuyHieuDang.sln
dotnet test HuyHieuDang.sln
```

Trên nhánh `feat/T34-bo-api-xoa-mot`. Dựng: **0 lỗi**, 40 cảnh báo phân tích mã có sẵn từ trước.
Chạy thử: **600 đạt · 0 hỏng · 8 bỏ qua**.

| Dự án kiểm thử | Đạt | Hỏng | Bỏ qua |
|---|---|---|---|
| HuyHieuDang.Core.UnitTests | 124 | 0 | 0 |
| HuyHieuDang.Core.QcTests | 118 | 0 | 1 |
| HuyHieuDang.Infrastructure.UnitTests | 44 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |
| HuyHieuDang.Web.IntegrationTests | 182 | 0 | 0 |
| HuyHieuDang.Web.QcIntegrationTests | 131 | 0 | 7 |

## HuyHieuDang.Web.IntegrationTests · PartyMemberEndpointTests

Lớp duy nhất có ca bị đổi. Liệt kê đủ cả lớp:

- Danh sách trả giới tính dạng chuỗi và ba giá trị tuổi đảng tính theo hôm nay — ĐẠT
- Tìm theo họ tên chứa chuỗi, không phân biệt hoa thường — ĐẠT
- Lọc giới tính Nam — ĐẠT
- Lọc giới tính Nữ — ĐẠT
- Bỏ tham số lọc thì lấy tất cả — ĐẠT
- Sắp xếp theo cột hợp lệ; cột tính ra bị bỏ qua và quay về thứ tự mặc định — ĐẠT
- Phân trang mặc định 20 dòng, chọn được số dòng và số trang — ĐẠT
- `current` không dương trả 400 — ĐẠT
- `pageSize` không dương trả 400 — ĐẠT
- Lấy một trả đúng bản ghi; id lạ trả 400 `Mes.PartyMember.NotFound` — ĐẠT
- Thêm mới trả 200 kèm bản ghi vừa tạo và khóa `Create.Successfully` — ĐẠT
- QT9 · Thêm hai người trùng hệt nhau vẫn thành hai bản ghi — ĐẠT
- Đúng ngày hôm nay vẫn là ngày vào Đảng chính thức hợp lệ — ĐẠT
- Họ tên rỗng trả 400 `Required` — ĐẠT
- Họ tên chỉ có khoảng trắng trả 400 `Required` — ĐẠT
- Thiếu ngày chính thức trả 400 `Required` — ĐẠT
- Ngày chính thức ở tương lai trả 400 `Invalid` — ĐẠT
- Ngày sinh sau ngày chính thức trả 400 `Invalid` — ĐẠT
- Giới tính ngoài danh mục trả 400 `Invalid` — ĐẠT
- Sửa ghi đè cả bốn trường, kể cả xóa bằng `null` — ĐẠT
- Sửa id không tồn tại trả 400 `Mes.PartyMember.NotFound` — ĐẠT
- Xóa một người qua `DeleteMany` với mảng một phần tử trả đúng một id, bản ghi biến mất hẳn — ĐẠT *(ca mới, thay ca xóa một cũ)*
- Xóa mảng chỉ có id lạ trả danh sách rỗng chứ không phải lỗi — ĐẠT *(ca mới, thay ca xóa một id lạ cũ)*
- Xóa nhiều trả đúng id đã xóa, id lạ bị bỏ qua lặng lẽ — ĐẠT
- Xóa nhiều với danh sách rỗng trả 400 — ĐẠT
- `DELETE /api/PartyMembers/{id}` đã bị gỡ, gọi vào trả 404/405 — ĐẠT *(ca mới, chốt không ai dựng lại đường xóa một)*
- Gọi `GET /api/PartyMembers` không kèm token trả 401 — ĐẠT
- Gọi `POST /api/PartyMembers` không kèm token trả 401 — ĐẠT
- Gọi `POST /api/PartyMembers/DeleteMany` không kèm token trả 401 — ĐẠT

## HuyHieuDang.Web.QcIntegrationTests · các ca đã sửa theo hợp đồng mới

- A-121 · Xóa một người qua mảng một phần tử làm tổng giảm đúng 1 — ĐẠT *(trước gọi `DELETE /{id}`)*
- A-122 · Xóa nhiều người một lần làm tổng giảm đúng số lượng, xóa hẳn (QT10) — ĐẠT
- A-123 · Xóa id không tồn tại trả danh sách rỗng, và đường xóa một đã bị gỡ — ĐẠT *(trước khẳng định 400 `Mes.PartyMember.NotFound`)*
- A-124 · Xóa danh sách có id trùng nhau: không đếm trùng, không lỗi — ĐẠT
- A-001 · Bảng đối chiếu cổng xác thực rút từ 27 xuống 26 endpoint, tất cả không token đều trả 401 — ĐẠT
- A-905 · Mọi lỗi trả ra đều mang khóa dịch được và không lộ nội bộ, sau khi bỏ lời gọi xóa một khỏi danh sách mẫu — ĐẠT

## Các lớp còn lại

Không đổi trong lần này; từng ca đã liệt kê ở các báo cáo trước:

- Logic mốc tuổi đảng QT1–QT11: `2026-09-19-test-kiem-thu-logic-qc.md`, `2026-09-19-test-qc-sau-khi-sua-bon-loi.md`.
- Tích hợp API trên PostgreSQL thật: `2026-09-20-test-tich-hop-api-qc-t27.md`.

Tám ca bỏ qua vẫn là tám ca cũ, không liên quan T34: QC-05 (Core không đọc đồng hồ hệ thống)
và bảy ca chờ sửa lỗi T36.
