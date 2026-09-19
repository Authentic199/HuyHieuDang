# Báo cáo kiểm thử — API tính toán: Dashboard, đủ điều kiện, chưa thuộc đợt nào (T12)

Lệnh đã chạy: `cd BE && dotnet test`

Tổng: 285 test — ĐẠT 285, HỎNG 0, BỎ QUA 0.

- `HuyHieuDang.Core.UnitTests` — 113 đạt
- `HuyHieuDang.Infrastructure.UnitTests` — 44 đạt
- `HuyHieuDang.Infrastructure.IntegrationTests` — 1 đạt
- `HuyHieuDang.Web.IntegrationTests` — 127 đạt

Phần dưới liệt kê các lớp kiểm thử mới thêm cho T12. Mọi con số mong đợi đọc thẳng từ
`tests/fixtures/data/expected.json`; hôm nay cố định ở T0 = 19/09/2026, T1 = 15/10/2026,
T2 = 01/12/2026.

## DashboardEndpointTests — mục 6.1 hợp đồng API

- Tại T0, đợt sắp tới là Đợt 7/11 của 2026, còn 12 ngày, 6 người, phân bổ 30:3 · 35:1 · 40:1 · 45:1 — ĐẠT
- Tại T1, Đợt 7/11 chuyển sang đang diễn ra và không còn số ngày đếm ngược — ĐẠT
- Tại T2, mọi đợt của 2026 đã qua nên đợt sắp tới là Đợt 3/2 của 2027, cờ năm sau bật — ĐẠT
- Chưa cài đợt nào thì không trả đợt rỗng giả, bảng rỗng và có cảnh báo chưa có đợt — ĐẠT
- Chưa có đảng viên nào thì có cảnh báo chưa có đảng viên, đợt sắp tới vẫn hiện với 0 người — ĐẠT
- Kho trống hoàn toàn thì có đủ hai cảnh báo cho khối hướng dẫn ba bước — ĐẠT
- Cảnh báo chồng lấn và khoảng trống của Dashboard trùng khớp với màn Đợt — ĐẠT
- Gọi khi thiếu token trả 401 — ĐẠT

## EligibilityEndpointTests — mục 6.2, 6.3 và 6.4 hợp đồng API

- Đủ điều kiện của bốn đợt chính trong các năm 2025, 2026, 2027, 2028 khớp từng dòng với bộ dữ liệu QC (16 ca) — ĐẠT
- Bỏ trống tham số năm thì lấy năm hiện tại của máy chủ — ĐẠT
- Nới Đến ngày của Đợt 2/9 sang 30/09 làm đợt lên 5 người ngay, không cần thao tác nào khác — ĐẠT
- Id đợt không tồn tại trả khóa không tìm thấy đợt — ĐẠT
- Năm ngoài khoảng 1900–2200 trả khóa năm không hợp lệ trên cả ba endpoint (5 ca) — ĐẠT
- Chưa thuộc đợt nào năm 2026 đúng 7 người và đúng nhãn khoảng trống của từng người — ĐẠT
- Chưa thuộc đợt nào các năm 2025, 2027, 2028 khớp bộ dữ liệu QC (3 ca) — ĐẠT
- Đổi Bước từ 5 sang 10 làm danh sách còn 6 người ngay — ĐẠT
- Chưa cài đợt nào thì cả 27 người tròn mốc trong năm đều rơi vào "Trước đợt đầu tiên" — ĐẠT
- Con số badge luôn bằng số dòng của màn chưa thuộc đợt nào — ĐẠT
- Gọi khi thiếu token trả 401 trên cả ba endpoint (3 ca) — ĐẠT

## EligibilityPerformanceTests — ràng buộc hiệu năng

- Với 10.000 đảng viên, cả bốn endpoint đều trả lời dưới một giây — ĐẠT
  (đo được: đủ điều kiện 49 ms · Dashboard 125 ms · chưa thuộc đợt nào 106 ms · badge 98 ms)

## Qt8UpcomingPeriodTests — hai ca bổ sung cho QT8

- Hai đợt cùng Từ ngày thì chọn đợt kết thúc sớm hơn, vẫn bằng nhau thì theo Tên đợt — ĐẠT
- Khi mọi đợt của năm nay đã qua, tiêu chí chọn đó vẫn giữ nguyên cho năm sau — ĐẠT
