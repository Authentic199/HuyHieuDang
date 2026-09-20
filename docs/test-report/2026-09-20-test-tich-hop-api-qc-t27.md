# Báo cáo kiểm thử — Kiểm thử tích hợp API với PostgreSQL thật (T27)

Lệnh đã chạy: `cd BE && dotnet test HuyHieuDang.sln`

Toàn giải pháp: 596 đạt · 2 hỏng · 8 bỏ qua trên tổng 606.

| Dự án kiểm thử | Đạt | Hỏng | Bỏ qua |
|---|---|---|---|
| HuyHieuDang.Web.QcIntegrationTests (dự án mới của QC) | 132 | 0 | 7 |
| HuyHieuDang.Web.IntegrationTests | 180 | 0 | 0 |
| HuyHieuDang.Core.UnitTests | 123 | 1 | 0 |
| HuyHieuDang.Core.QcTests | 117 | 1 | 1 |
| HuyHieuDang.Infrastructure.UnitTests | 44 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |

Hai ca hỏng là bất đồng nhãn khoảng trống có từ trước nhánh này, không do T27 sinh ra.

Mọi ca chạy trên PostgreSQL 16 thật trong container qua `WebApplicationFactory` + Testcontainers.
Ngày cố định T0 = 19/09/2026, T1 = 15/10/2026, T2 = 01/12/2026. Mỗi ca tự dựng dữ liệu mình cần
nên thứ tự chạy không ảnh hưởng kết quả, và chạy lại ngày nào cũng cho cùng kết quả.

Bảy ca bỏ qua là bảy lỗi QC tìm được: mỗi ca mô tả hành vi đúng, hiện đang đỏ, để `Skip` kèm mã
lỗi. Bỏ `Skip` là ca đỏ lại ngay.

## A0AuthorizationTests — phân quyền

- Gọi cả 27 endpoint nghiệp vụ khi chưa đăng nhập đều bị từ chối và không lộ dữ liệu — ĐẠT
- Token ký bằng khóa khác bị từ chối — ĐẠT
- Token đã hết hạn bị từ chối — ĐẠT
- Gửi token mà thiếu tiền tố Bearer bị từ chối — ĐẠT
- Sai mật khẩu và không có tài khoản cho cùng một thông điệp chung — ĐẠT
- Đăng nhập đúng trả token dùng được ngay cho endpoint khác — ĐẠT
- Chỉ endpoint đăng nhập được phép gọi khi chưa xác thực — ĐẠT

## A1PartyMemberTests — danh sách đảng viên

- Nạp bộ lõi và bộ lớn cho đúng 1232 người — ĐẠT
- Trang đầu 20 dòng, tổng 1232, 62 trang — ĐẠT
- Trang cuối còn đúng 12 dòng — ĐẠT
- Trang vượt quá trang cuối trả danh sách rỗng chứ không lỗi — ĐẠT
- Cỡ trang 50 và 100 cho đúng số trang và đúng số dòng trang cuối — ĐẠT
- Cỡ trang 0, âm và rất lớn không làm treo máy chủ — ĐẠT
- Ghép mọi trang lại đủ 1232 người, không trùng không thiếu — ĐẠT
- Tìm "Nguyễn" và "nguyễn" đều ra 79 người — ĐẠT
- Tìm đúng một người, và tìm chuỗi không tồn tại ra danh sách rỗng — ĐẠT
- Ký tự đặc biệt của SQL không gây lỗi và không bị hiểu là ký tự đại diện — ĐẠT
- Lọc giới tính Nam, Nữ, để trống đúng 592, 590, 50 — ĐẠT
- Sắp xếp hai chiều đúng trên cả bốn cột, ô trống không xen kẽ, thứ tự tất định — ĐẠT
- Sắp theo Họ tên đặt "Đào Văn Ân" trước "Nguyễn Văn An" đúng bảng chữ cái tiếng Việt — ĐẠT
- Thêm tay hợp lệ làm tổng tăng đúng một — ĐẠT
- Thêm tay thiếu Họ tên bị từ chối — ĐẠT
- Ngày chính thức đúng bằng hôm nay là hợp lệ, sau hôm nay một ngày thì không — ĐẠT
- Sửa Ngày chính thức làm tuổi đảng, mốc kế tiếp và danh sách đủ điều kiện đổi theo ngay — ĐẠT
- Xóa một người làm tổng giảm đúng một — ĐẠT
- Xóa nhiều người làm tổng giảm đúng số lượng — ĐẠT
- Xóa id không tồn tại trả lỗi nghiệp vụ rõ ràng — ĐẠT
- Xóa danh sách có id trùng nhau không đếm trùng — ĐẠT
- Tuổi đảng, mốc kế tiếp và ngày tròn mốc của cả 32 người khớp kết quả mong đợi — ĐẠT
- Lấy một đảng viên theo id trả đúng người, id lạ trả lỗi nghiệp vụ — ĐẠT

## A2AwardPeriodTests — đợt trao huy hiệu

- Bốn đợt chính trả đủ bốn dòng, sắp theo Từ ngày — ĐẠT
- Trạng thái đợt tại T0 và T1 đúng Đã qua, Đang diễn ra, Sắp tới còn 12 ngày — ĐẠT
- Số người đủ điều kiện năm nay trên từng dòng đúng 5, 5, 4, 6 — ĐẠT
- Từ ngày sau Đến ngày, trùng tên, ngày 31/02 và thiếu tên đều bị chặn — ĐẠT
- Đợt bắt đầu 29/02 thu về 28/02 ở năm không nhuận, giữ 29/02 ở năm nhuận — ĐẠT
- Hai đợt chồng lấn vẫn lưu được và cảnh báo nêu đúng cặp đợt — ĐẠT
- Bốn đợt chính cảnh báo đúng năm khoảng trống — ĐẠT
- Bộ đợt phủ kín cả năm thì không còn cảnh báo — ĐẠT
- Nới Đến ngày Đợt 2/9 làm danh sách lên 5 người và badge còn 6, không cần thao tác khác — ĐẠT
- Xóa đợt không làm mất đảng viên nào — ĐẠT
- Đợt 7/11 năm 2026 đúng 6 người, đúng phân bổ mốc, đúng thứ tự — ĐẠT
- Đợt 7/11 năm 2027 và Đợt 3/2 năm 2028 với ngày tròn mốc 29/02 đều đúng — ĐẠT
- Đợt 3/2 năm 2026 có Ngô Văn Khánh với ngày tròn mốc 28/02/2026 — ĐẠT
- Năm 1899, 2201, 0, âm và chữ đều được xử lý tất định, không lỗi máy chủ — ĐẠT
- Hỏi đợt không tồn tại trả lỗi nghiệp vụ rõ ràng ở cả ba endpoint — ĐẠT
- Nạp thêm bộ lớn không làm lệch con số của bộ lõi — ĐẠT

## A3DashboardTests — Dashboard

- Dashboard tại T0 chỉ đúng Đợt 7/11 năm 2026, còn 12 ngày, 6 người — ĐẠT
- Dashboard tại T1 báo đang diễn ra và không còn đếm ngược — ĐẠT
- Dashboard tại T2 nhảy sang Đợt 3/2 năm 2027 và bật cờ năm sau — ĐẠT
- Chưa cài đợt nào thì báo chưa cài đợt, không trả đợt rỗng giả — ĐẠT
- Chưa có đảng viên nào thì cảnh báo đúng và bảng rỗng — ĐẠT
- Kho trống hoàn toàn bật cả hai cảnh báo — ĐẠT
- Badge chưa thuộc đợt nào của năm nay bằng 7 — ĐẠT
- Cảnh báo trên Dashboard khớp từng chữ với cảnh báo ở màn Đợt — ĐẠT

## A4UnassignedTests — chưa thuộc đợt nào

- Năm 2026 có đúng 7 người, mỗi người đúng nhãn khoảng trống — ĐẠT
- Đổi Bước sang 10 làm danh sách còn 6 người — ĐẠT
- Năm 2025 và 2027 khớp kết quả mong đợi — ĐẠT
- Bộ đợt phủ kín thì không ai bị sót — ĐẠT
- Sắp theo Mốc rồi Họ tên đúng bảng chữ cái tiếng Việt — ĐẠT
- Badge và số dòng màn hình khớp nhau ở bốn trạng thái dữ liệu và ba năm — ĐẠT
- Chưa cài đợt nào thì nhãn là "Trước đợt đầu tiên" với hai tên đợt để trống — ĐẠT

## A5SettingsTests — cài đặt

- Đọc cài đặt trả đúng mặc định 30 / 90 / 5 và đúng dãy mốc — ĐẠT
- Đổi Bước sang 10 làm danh sách đợt, Dashboard và badge đổi theo ngay — ĐẠT
- Bắt đầu lớn hơn Kết thúc, Bước 0, Bước âm, giá trị không phải số đều bị từ chối — ĐẠT
- Khôi phục mặc định đưa mốc về 30 / 90 / 5 và không đụng tên đơn vị — ĐẠT
- Tên đơn vị lưu được và để trống cũng được — ĐẠT
- Gọi lưu hai lần liên tiếp vẫn chỉ có đúng một bản ghi cài đặt — ĐẠT
- Xem trước dãy mốc không ghi gì vào cơ sở dữ liệu và từ chối tham số sai — ĐẠT
- Đăng xuất trả thành công và không hủy token phía máy chủ — ĐẠT

## A6ImportTests — import Excel

- Xem trước file lõi cho 32 dòng hợp lệ, 0 lỗi, chưa ghi gì vào cơ sở dữ liệu — ĐẠT
- Nạp sau xem trước làm tổng tăng đúng 32 — ĐẠT
- File bốn dòng lỗi báo đúng 6 hợp lệ, 4 lỗi, đúng dòng 8, 9, 10, 11 và đúng lý do — ĐẠT
- Nạp file bốn dòng lỗi chỉ thêm đúng 6 người — ĐẠT
- Chỉ xem trước rồi bỏ ngang thì tổng không đổi — ĐẠT
- Nạp cùng một file hai lần làm tổng tăng gấp đôi, đúng quy tắc không kiểm trùng — ĐẠT
- File sai cột, file rỗng, file chỉ có tiêu đề, file không phải Excel, file .csv đều bị chặn ngay — ĐẠT
- Thông báo của file chỉ có tiêu đề khác thông báo của file rỗng — ĐẠT
- File 11 MB bị chặn bằng lỗi dung lượng trước khi đọc nội dung — ĐẠT
- File hợp lệ nặng 9,9 MB vẫn được chấp nhận — ĐẠT
- Không gửi file nào trả lỗi nói được, không phải lỗi máy chủ — ĐẠT
- Gửi hai file cùng lúc được xử lý tất định — ĐẠT
- Cắt kết nối giữa lúc nạp không để lại dữ liệu nửa vời — ĐẠT
- Lỗi kỹ thuật ở dòng cuối cuộn lại toàn bộ, tổng không đổi — ĐẠT
- Nạp file 1200 dòng chạy trọn vẹn trong thời gian hợp lý — ĐẠT
- File mẫu đúng bốn cột, có dòng ví dụ, ngày dd/MM/yyyy, và nạp lại chính nó được — ĐẠT
- Ô ngày kiểu ngày của Excel vẫn đọc được đủ 32 dòng — ĐẠT
- Một dòng nhiều lỗi trả đủ mọi lý do, đủ cả sáu mã lỗi cấp dòng — ĐẠT

## A7ExportTests — xuất Excel

- Tên file của bốn đợt đúng quy ước bỏ dấu và đổi dấu gạch chéo — ĐẠT
- Tên file chưa thuộc đợt nào đúng quy ước — ĐẠT
- Dòng tiêu đề có tên đơn vị, tên đợt kèm khoảng ngày đã gắn năm, ngày xuất đúng hôm nay — ĐẠT
- Tên đơn vị để trống thì bỏ hẳn dòng đó, không để dòng trắng lạ — ĐẠT
- Nội dung file khớp từng ô với bảng đang xem, ô trống để rỗng chứ không ghi dấu gạch — ĐẠT
- Xuất danh sách rỗng vẫn ra file có tiêu đề và 0 dòng dữ liệu — ĐẠT
- Cả ba endpoint xuất đều mở được bằng thư viện đọc Excel, đúng một sheet — ĐẠT
- Xuất từ Dashboard lấy đúng đợt sắp tới kể cả khi đợt thuộc năm sau — ĐẠT
- File chưa thuộc đợt nào có thêm cột Khoảng trống đúng nội dung — ĐẠT

## A9TechnicalTests — ràng buộc kỹ thuật

- Không nơi nào trong BE/src đọc đồng hồ máy ngoài lớp provider — ĐẠT
- Kết quả không phụ thuộc múi giờ của tiến trình — ĐẠT
- Biến ép ngày không có hiệu lực ở Production — ĐẠT
- Một vạn đảng viên trong kho vẫn tính danh sách đủ điều kiện dưới một giây — ĐẠT
- Mười tám lối lỗi đều trả khóa thông điệp dịch được và không lộ nội bộ — ĐẠT
- Mọi trường ngày nghiệp vụ trên dây là ngày thuần yyyy-MM-dd — ĐẠT
- Ngoài Production biến ép ngày phải có hiệu lực — BỎ QUA (lỗi QC-T27-01: chưa cài đặt)

## A8DefectTests — sáu lỗi đã tìm được ở vòng này

- Số trang lớn không được làm máy chủ lỗi 500 — BỎ QUA (lỗi QC-T27-02)
- Cỡ trang phải bị chặn hoặc kẹp về mức trần — BỎ QUA (lỗi QC-T27-03)
- Lỗi ép kiểu tham số phải trả khóa thông điệp — BỎ QUA (lỗi QC-T27-04)
- Giá trị lọc lạ không được bỏ qua lặng lẽ — BỎ QUA (lỗi QC-T27-05)
- Xem trước dãy mốc phải có trần — BỎ QUA (lỗi QC-T27-06)
- Mọi khóa Backend trả ra đều phải có trong hợp đồng — BỎ QUA (lỗi QC-T27-07)

## Bảng đối chiếu 28 endpoint

| # | Endpoint | Ca đã chạm |
|---|---|---|
| 1 | `POST /api/Auth/Login` | A-005, A-006 |
| 2 | `POST /api/Auth/Logout` | A-001, A-511 |
| 3 | `GET /api/Auth/Me` | A-001, A-006, A-507 |
| 4 | `GET /api/PartyMembers` | A-001, A-101 → A-115, A-125 |
| 5 | `GET /api/PartyMembers/{id}` | A-001, A-126 |
| 6 | `POST /api/PartyMembers` | A-001, A-116 → A-119, A-905 |
| 7 | `PUT /api/PartyMembers/{id}` | A-001, A-120 |
| 8 | `DELETE /api/PartyMembers/{id}` | A-001, A-121, A-123 |
| 9 | `POST /api/PartyMembers/DeleteMany` | A-001, A-122, A-124 |
| 10 | `GET /api/PartyMembers/Import/Template` | A-001, A-619 |
| 11 | `POST /api/PartyMembers/Import/Preview` | A-001, A-601, A-603, A-605, A-607 → A-615, A-620, A-621 |
| 12 | `POST /api/PartyMembers/Import/Commit` | A-001, A-602, A-604, A-606, A-616 → A-618 |
| 13 | `GET /api/AwardPeriods` | A-001, A-201 → A-204, A-210, A-211, A-218 |
| 14 | `GET /api/AwardPeriods/{id}` | A-001, A-214 → A-217, A-219 |
| 15 | `POST /api/AwardPeriods` | A-001, A-205 → A-207, A-209 |
| 16 | `PUT /api/AwardPeriods/{id}` | A-001, A-212 |
| 17 | `DELETE /api/AwardPeriods/{id}` | A-001, A-213, A-219 |
| 18 | `GET /api/Dashboard` | A-001, A-301 → A-308 |
| 19 | `GET /api/Eligibility` | A-001, A-212, A-214 → A-220, A-502 |
| 20 | `GET /api/Eligibility/Unassigned` | A-001, A-401 → A-406 |
| 21 | `GET /api/Eligibility/UnassignedCount` | A-001, A-212, A-307, A-406, A-502 |
| 22 | `GET /api/Settings` | A-001, A-501, A-503, A-509 |
| 23 | `PUT /api/Settings` | A-001, A-502 → A-505, A-507, A-509 |
| 24 | `POST /api/Settings/RestoreDefaults` | A-001, A-501, A-506 |
| 25 | `GET /api/Settings/Milestones` | A-001, A-510 |
| 26 | `GET /api/Exports/Eligibility` | A-001, A-701 → A-710 |
| 27 | `GET /api/Exports/Dashboard` | A-001, A-710, A-711 |
| 28 | `GET /api/Exports/Unassigned` | A-001, A-703, A-709, A-710, A-712 |

## Điều đã soi nhưng không phải lỗi

Gọi một đường dẫn có id sai định dạng trong host kiểm thử trả lỗi 500 kèm đường dẫn tệp của máy.
Gọi đúng đường dẫn đó trên tiến trình thật ở cổng 8080 trả 404 đúng như hợp đồng, nên đây là hiện
tượng riêng của host kiểm thử, không phải lỗi sản phẩm.

Thứ tự của danh sách đủ điều kiện là Mốc rồi Họ tên đầy đủ, khớp tài liệu nghiệp vụ UC-11 và khớp
`expected.json`. Hợp đồng API mục 1.8 và mục 6.3 đang mô tả khác đi; đó là lỗi tài liệu QC-T27-08,
không phải lỗi mã.
