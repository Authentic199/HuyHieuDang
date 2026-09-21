# Kiểm thử đợt trao huy hiệu vắt qua 31/12

Lệnh đã chạy:

```
cd BE && dotnet test HuyHieuDang.sln
cd FE && npm run build
cd FE && npm run lint
cd FE && npx tsc -p tsconfig.e2e.json --noEmit
```

Kết quả `dotnet test`: 620 ĐẠT · 0 HỎNG · 8 bỏ qua, trên sáu dự án test.

| Dự án | Đạt | Hỏng | Bỏ qua |
|---|---|---|---|
| HuyHieuDang.Core.UnitTests | 141 | 0 | 0 |
| HuyHieuDang.Core.QcTests | 119 | 0 | 1 |
| HuyHieuDang.Infrastructure.UnitTests | 46 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |
| HuyHieuDang.Web.IntegrationTests | 181 | 0 | 0 |
| HuyHieuDang.Web.QcIntegrationTests | 132 | 0 | 7 |

`npm run build`, `npm run lint` và kiểm kiểu bộ E2E đều chạy sạch, không cảnh báo.

Dưới đây liệt kê từng ca của các lớp test mà thay đổi này đụng tới. Các lớp còn lại không
sửa dòng nào và vẫn đạt đủ, đã gộp vào bảng tổng ở trên.

## Qt6SpanningYearPeriodTests (mới)

Gắn năm vào đợt 01/12 – 28/02 cho Đến ngày rơi vào năm sau — ĐẠT
Đợt kết thúc 29/02 gắn vào năm nhuận giữ nguyên ngày 29 — ĐẠT
Người tròn mốc ngày 15/12 nằm ở nửa đầu đợt thì đủ điều kiện — ĐẠT
Người tròn mốc ngày 10/02 năm sau nằm ở nửa sau vẫn thuộc chính đợt đó — ĐẠT
Người tròn mốc giữa năm, ngoài hai đầu đợt, thì không đủ điều kiện — ĐẠT
Một đợt vắt năm để lại đúng hai phần trong một năm: đuôi đầu năm và đầu cuối năm — ĐẠT
Đợt vắt năm đứng một mình chỉ để hở khoảng 01/03 – 30/11 — ĐẠT
Hai đợt vắt năm nối nhau phủ kín cả năm, không còn khoảng trống — ĐẠT
Hai phần của cùng một đợt vắt năm không bị báo là chồng lấn — ĐẠT
Đợt thường đè lên đuôi đợt vắt năm thì báo đúng khoảng ngày dùng chung — ĐẠT
Người tròn mốc trong đuôi đợt vắt năm không bị xếp vào "chưa thuộc đợt nào" — ĐẠT
Người tròn mốc giữa năm bị xếp vào khoảng trống đúng ngày bắt đầu — ĐẠT
Ngày 15/01, đợt sắp tới là lần diễn ra khởi đầu từ năm trước và đang mở — ĐẠT
Ngày 01/03, lần diễn ra năm trước vừa đóng nên lấy lần của năm nay — ĐẠT
Ngày nằm trong lần diễn ra khởi đầu từ năm trước đọc là "Đang diễn ra" — ĐẠT
Ngày nằm giữa hai lần diễn ra đếm ngược tới Từ ngày kế tiếp — ĐẠT
Chọn một năm ở xa trong tương lai vẫn tính trạng thái theo đúng năm đó — ĐẠT

## Qt6PeriodWarningTests (sửa một ca)

Từ ngày đứng sau Đến ngày nay cho ra đợt kết thúc ở năm sau thay vì báo lỗi — ĐẠT
Năm ca cũ về khoảng trống, chồng lấn và ngày/tháng không có thật — ĐẠT

## AwardPeriodCoverageBuilderTests (thêm hai ca)

Đợt vắt qua 31/12 cho hai đoạn cùng mã đợt trên dải độ phủ, ở hai đầu năm — ĐẠT
Đợt vắt năm một mình chỉ sinh một cảnh báo khoảng trống giữa năm, không cảnh báo chồng lấn — ĐẠT

## Qc06Qt6PeriodTests (sửa một ca, thêm một ca)

U-602 · Đợt có Từ ngày sau Đến ngày kết thúc ở năm sau — ĐẠT
U-602b · Bản tính độc lập của QC và service của Backend cho cùng kết quả trên đợt vắt năm — ĐẠT

## AwardPeriodEndpointTests (thêm một ca, bớt một dòng dữ liệu)

Tạo đợt 01/12 – 28/02 qua API trả 200, cờ vắt năm bật, Đến ngày thuộc năm sau — ĐẠT
Chặn lưu nay chỉ còn thiếu tên và ngày/tháng không có thật — ĐẠT

## A2AwardPeriodTests (sửa một ca)

A-205 · Tạo đợt có Từ ngày sau Đến ngày trả 200 thay vì 400 — ĐẠT
