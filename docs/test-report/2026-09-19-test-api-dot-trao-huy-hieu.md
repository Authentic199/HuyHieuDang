# Báo cáo kiểm thử — API Đợt trao huy hiệu (T11)

Lệnh đã chạy: `cd BE && dotnet test`

Kết quả: 240 đạt · 0 hỏng · 0 bỏ qua.

| Dự án kiểm thử | Đạt | Hỏng | Bỏ qua |
|---|---|---|---|
| HuyHieuDang.Core.UnitTests | 111 | 0 | 0 |
| HuyHieuDang.Infrastructure.UnitTests | 44 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |
| HuyHieuDang.Web.IntegrationTests | 84 | 0 | 0 |

Báo cáo này ghi chi tiết ba nhóm mới của T11. Các nhóm còn lại đã có báo cáo riêng ở những lần bàn giao trước và lần chạy này vẫn xanh nguyên.

## Qt11PeriodStatusByYearTests

Đợt đã kết thúc trước hôm nay là Đã qua, không có số ngày còn lại — ĐẠT
Đợt kết thúc ngày 10/09 khi hôm nay là 19/09 vẫn là Đã qua — ĐẠT
Đợt bao trùm hôm nay là Đang diễn ra — ĐẠT
Đợt gói gọn đúng ngày hôm nay là Đang diễn ra — ĐẠT
Đợt bắt đầu 01/10 là Sắp tới và còn 12 ngày — ĐẠT
Xem năm đã qua thì mọi đợt là Đã qua — ĐẠT
Xem năm sau thì đợt là Sắp tới, số ngày còn lại đếm tới ngày đầu đợt của năm đó — ĐẠT
Không truyền năm thì vẫn xét đúng năm của hôm nay — ĐẠT
Đợt 29/02 ở năm không nhuận xét theo ngày 28/02 — ĐẠT

## AwardPeriodCoverageBuilderTests

Bộ bốn đợt mẫu cho đúng một cặp chồng lấn kèm khoảng ngày dùng chung 25/05–31/05 — ĐẠT
Bộ bốn đợt mẫu cho đúng hai khoảng trống kèm tên hai đợt kề hai bên — ĐẠT
Dải độ phủ liền mạch từ 01/01 đến 31/12, phần chồng lấn thuộc đoạn của đợt đến trước — ĐẠT
Chưa cài đợt nào thì cả năm là một khoảng trống, không có tên đợt kề — ĐẠT
Đợt nằm lọt trong đợt khác được báo chồng lấn và không cắt thêm đoạn nào — ĐẠT
Đợt 29/02 lùi về 28/02 ở năm không nhuận và giữ nguyên ở năm nhuận — ĐẠT

## AwardPeriodEndpointTests

Danh sách bốn đợt sắp theo ngày bắt đầu, đủ ba trạng thái và số người đủ điều kiện từng đợt — ĐẠT
Cảnh báo liệt kê đúng một cặp chồng lấn và một khoảng trống cuối năm — ĐẠT
Dải độ phủ trả về liền mạch 01/01–31/12, đoạn của đợt đến sau bắt đầu sau phần chồng lấn — ĐẠT
Hỏi năm khác thì ngày gắn đúng năm đó, trạng thái vẫn so với hôm nay — ĐẠT
Năm 1899 nằm ngoài khoảng cho phép trả khóa Mes.Query.Invalid.Year — ĐẠT
Năm 2201 nằm ngoài khoảng cho phép trả khóa Mes.Query.Invalid.Year — ĐẠT
Lấy một đợt theo id và theo năm được hỏi trả đúng ngày của năm đó — ĐẠT
Lấy một đợt bằng id không tồn tại trả khóa Mes.AwardPeriod.NotFound — ĐẠT
Thêm đợt chồng lấn vẫn trả 200, đã lưu và kèm cảnh báo — ĐẠT
Thiếu tên đợt trả khóa Mes.AwardPeriod.Required.Name — ĐẠT
Ngày 31/04 không có thật trả khóa Mes.AwardPeriod.Invalid.FromDate — ĐẠT
Ngày kết thúc 31/11 không có thật trả khóa Mes.AwardPeriod.Invalid.ToDate — ĐẠT
Đợt vắt qua năm từ 15/12 đến 20/01 trả khóa Mes.AwardPeriod.Invalid.Range — ĐẠT
Trùng tên đợt dù khác hoa thường trả khóa Mes.AwardPeriod.Repeated.Name — ĐẠT
Sửa đợt giữ nguyên tên của chính nó và có hiệu lực ngay cho năm hiện tại — ĐẠT
Sửa sang tên của đợt khác trả khóa Mes.AwardPeriod.Repeated.Name — ĐẠT
Sửa đợt bằng id không tồn tại trả khóa Mes.AwardPeriod.NotFound — ĐẠT
Xóa hẳn một đợt, trả khoảng trống mới sinh ra và giữ nguyên bốn đảng viên — ĐẠT
Xóa lại đợt vừa xóa trả khóa Mes.AwardPeriod.NotFound — ĐẠT
Gọi danh sách khi thiếu token trả 401 — ĐẠT
Gọi thêm mới khi thiếu token trả 401 — ĐẠT
Gọi xóa khi thiếu token trả 401 — ĐẠT
