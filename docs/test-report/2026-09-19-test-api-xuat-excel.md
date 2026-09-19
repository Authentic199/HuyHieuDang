# Báo cáo kiểm thử — API Xuất Excel (T14)

Lệnh chạy: `dotnet test` (thư mục `BE/`)

Kết quả toàn giải pháp: 335 đạt / 0 hỏng / 0 bỏ qua — 113 `Core.UnitTests`, 44 `Infrastructure.UnitTests`, 1 `Infrastructure.IntegrationTests`, 177 `Web.IntegrationTests`.

Riêng bộ mới của T14: `dotnet test tests/HuyHieuDang.Web.IntegrationTests --filter "FullyQualifiedName~ExportEndpointTests"` — 23 đạt / 0 hỏng.

## ExportEndpointTests (HuyHieuDang.Web.IntegrationTests)

- Tên file của Đợt 7/11 là `DuDieuKien_Dot7-11_2026.xlsx`, kèm đúng kiểu nội dung và cả hai dạng tên trong `Content-Disposition` — ĐẠT
- Tên file của Đợt 19/5 là `DuDieuKien_Dot19-5_2026.xlsx` — ĐẠT
- Tên file của Đợt 3/2 là `DuDieuKien_Dot3-2_2026.xlsx` — ĐẠT
- Tên file của Đợt 2/9 là `DuDieuKien_Dot2-9_2026.xlsx` — ĐẠT
- Tên file danh sách chưa thuộc đợt nào là `ChuaThuocDot_2026.xlsx` — ĐẠT
- File Đợt 7/11 năm 2026 có đúng một sheet, ba dòng tiêu đề, một dòng trống, dòng đầu cột đủ bảy cột đúng thứ tự và đúng sáu dòng dữ liệu theo thứ tự danh sách trả về — ĐẠT
- Cột số thứ tự chạy liên tục từ 1 và cột mốc huy hiệu khớp từng dòng dữ liệu mong đợi — ĐẠT
- Tên đơn vị để trống thì dòng đầu file là tên đợt, không sinh dòng trắng thừa — ĐẠT
- Người thiếu Ngày sinh và Giới tính cho ra hai ô rỗng, không ghi dấu gạch ngang, trong khi người có đủ dữ liệu vẫn ghi Nam hoặc Nữ và ngày dạng ngày/tháng/năm — ĐẠT
- File chưa thuộc đợt nào năm 2026 có bảy dòng dữ liệu, thêm cột Khoảng trống ở cuối và đúng nhãn khoảng trống từng người — ĐẠT
- Xuất Dashboard lấy đúng đợt sắp tới, đúng tên file và cùng danh sách với màn Dashboard — ĐẠT
- Mọi đợt của năm nay đã qua thì file Dashboard mang đợt đầu của năm sau và ngày xuất là ngày đang xét — ĐẠT
- Đợt không có ai đủ điều kiện vẫn trả file hợp lệ chỉ có phần tiêu đề, không lỗi — ĐẠT
- Không có ai bị sót thì file chưa thuộc đợt nào vẫn hợp lệ và vẫn đủ tám cột — ĐẠT
- Bỏ trống tham số năm thì cả hai endpoint lấy năm hiện tại của máy chủ — ĐẠT
- Id đợt lạ trả 400 với khóa `Mes.AwardPeriod.NotFound` — ĐẠT
- Năm 1899 hoặc 2201 ở endpoint xuất theo đợt trả 400 với khóa `Mes.Query.Invalid.Year` — ĐẠT
- Năm 0 hoặc 2201 ở endpoint xuất chưa thuộc đợt nào trả 400 với khóa `Mes.Query.Invalid.Year` — ĐẠT
- Chưa cài đợt nào thì xuất Dashboard trả 400 với khóa `Mes.Dashboard.NotFound.UpcomingPeriod` — ĐẠT
- Thiếu token thì cả ba endpoint trả 401 — ĐẠT

## Các bộ đã có từ trước

Chạy lại nguyên trạng cùng lần này và vẫn xanh: `Core.UnitTests` (QT1 → QT11), `Infrastructure.UnitTests`, `Infrastructure.IntegrationTests` và toàn bộ các lớp kiểm thử API của T08 → T13 — ĐẠT
