# Báo cáo kiểm thử — T12c: cảnh báo gaps rỗng khi chưa có đợt nào

Lệnh đã chạy: `cd BE && dotnet test`

Tổng hợp toàn solution: 469 ca — 465 ĐẠT, 2 HỎNG, 1 bỏ qua.

| Dự án kiểm thử | Đạt | Hỏng | Bỏ qua |
|---|---|---|---|
| HuyHieuDang.Core.UnitTests | 123 | 1 | 0 |
| HuyHieuDang.Core.QcTests | 117 | 1 | 1 |
| HuyHieuDang.Infrastructure.UnitTests | 44 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |
| HuyHieuDang.Web.IntegrationTests | 181 | 0 | 0 |

Hai ca HỎNG đã đỏ sẵn trên `origin/main` trước khi sửa, không liên quan thay đổi này (xem mục cuối).

## AwardPeriodCoverageBuilderTests

- Bộ đợt mẫu cho đúng một cặp chồng lấn kèm khoảng ngày dùng chung — ĐẠT
- Bộ đợt mẫu cho đúng hai khoảng trống kèm tên hai đợt kề — ĐẠT
- Dải độ phủ liền mạch 01/01–31/12, phần chồng lấn thuộc đợt đến trước — ĐẠT
- Chưa cài đợt nào: dải vẫn là một khoảng trống cả năm nhưng cảnh báo gaps rỗng — ĐẠT
- Đợt nằm lọt trong đợt khác: báo chồng lấn, không cắt thêm đoạn — ĐẠT
- Đợt 29/02 ở năm không nhuận lùi về 28/02 — ĐẠT

## DashboardEndpointTests

- Ngày 19/09/2026: đợt sắp tới là Đợt 7/11 · 2026, còn 12 ngày, 6 người — ĐẠT
- Ngày 15/10/2026: Đợt 7/11 đang diễn ra, không còn đếm ngày — ĐẠT
- Ngày 01/12/2026: mọi đợt 2026 đã qua nên đợt sắp tới là Đợt 3/2 · 2027 — ĐẠT
- Chưa cài đợt nào: không có đợt sắp tới, bảng rỗng, cảnh báo noPeriods và gaps rỗng — ĐẠT
- Chưa có đảng viên nào: cảnh báo noMembers, đợt sắp tới vẫn có, 0 người — ĐẠT
- Kho trống hoàn toàn: đúng hai cảnh báo, không thừa dòng khoảng trống cả năm — ĐẠT
- Cảnh báo của Dashboard trùng khớp cảnh báo màn Đợt — ĐẠT
- Thiếu token trả 401 — ĐẠT

## AwardPeriodEndpointTests

- Bốn đợt mẫu: sắp theo ngày bắt đầu, ba trạng thái và số người đủ điều kiện — ĐẠT
- Cảnh báo liệt kê đúng một cặp chồng lấn và một khoảng trống — ĐẠT
- Chưa cài đợt nào: cảnh báo gaps rỗng nhưng dải vẫn phủ trọn 01/01–31/12 — ĐẠT
- Dải độ phủ liền mạch 01/01–31/12, phần chồng lấn thuộc đợt đến trước — ĐẠT
- Năm khác: trạng thái vẫn so với hôm nay, ngày gắn đúng năm được hỏi — ĐẠT
- Năm ngoài 1900–2200 báo lỗi năm không hợp lệ — ĐẠT
- Lấy một đợt theo id và theo năm được hỏi; id lạ báo không tìm thấy — ĐẠT
- Thêm đợt chồng lấn vẫn thành công và trả kèm cảnh báo — ĐẠT
- Ba lỗi chặn lưu: thiếu tên, ngày không có thật, đợt vắt qua năm — ĐẠT
- Trùng tên không phân biệt hoa thường bị chặn — ĐẠT
- Sửa đợt: giữ nguyên tên của chính nó, có hiệu lực ngay cho năm hiện tại — ĐẠT
- Sửa sang tên đợt khác bị chặn; id lạ báo không tìm thấy — ĐẠT
- Xóa hẳn đợt, trả khoảng trống mới và không đụng đảng viên — ĐẠT
- Thiếu token trả 401 — ĐẠT

## Hai ca đỏ sẵn có trên main (ngoài phạm vi T12c)

- `Qt6PeriodWarningTests.GetGaps_WhenThereIsNoPeriod_LabelsTheWholeYearAsAfterTheLastPeriod` — HỎNG: ca mong nhãn "Sau đợt cuối cùng" còn mã trả "Trước đợt đầu tiên" theo quyết định T12b.
- `QC-02 · Nhãn khoảng trống khi chưa cài đợt nào phải khớp expected.json` — HỎNG: cùng một lệch nhãn như trên.

Cả hai đều nằm ở tầng tính toán `HuyHieuDang.Core` mà T12c không chạm; hai dự án kiểm thử này chỉ tham chiếu `HuyHieuDang.Core`. Chạy lại trên `origin/main` (commit `d8cb40e`) cho đúng hai ca đỏ ấy với cùng con số.
