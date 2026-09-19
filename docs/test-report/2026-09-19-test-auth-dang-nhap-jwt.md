# Báo cáo kiểm thử T08 — Đăng nhập JWT, đăng xuất, thông tin phiên

Lệnh đã chạy: `cd BE && dotnet test HuyHieuDang.sln`

Tổng: 32 đạt, 0 hỏng, 0 bỏ qua.

| Dự án | Đạt | Hỏng | Bỏ qua |
|---|---|---|---|
| HuyHieuDang.Infrastructure.UnitTests | 16 | 0 | 0 |
| HuyHieuDang.Web.IntegrationTests | 15 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |

## AuthEndpointTests (tích hợp, PostgreSQL thật trong container)

- Đăng nhập đúng tài khoản admin trả 200, kèm token, tên đăng nhập, ngày máy chủ và tên đơn vị — ĐẠT
- Token cấp ra có hạn đúng 8 giờ và khớp với trường `expiresAt` trả về — ĐẠT
- Token vừa cấp dùng được ngay cho `GET /api/Auth/Me` — ĐẠT
- Đăng xuất khi có token trả 200 và khóa `Mes.User.Logout.Successfully` — ĐẠT
- Sai mật khẩu và không có tài khoản trả cùng một thông báo 401 `Mes.User.Login.Failed` — ĐẠT
- Bỏ trống tên đăng nhập trả 400 kèm khóa `Mes.User.Required.Username` — ĐẠT
- Bỏ trống mật khẩu trả 400 kèm khóa `Mes.User.Required.Password` — ĐẠT
- Gọi `GET /api/Auth/Me` không kèm token trả 401 với thân phản hồi rỗng — ĐẠT
- Gọi `POST /api/Auth/Logout` không kèm token trả 401 với thân phản hồi rỗng — ĐẠT
- Token ký bằng khóa khác trả 401 — ĐẠT
- Token đã quá hạn trả 401 — ĐẠT
- Token gửi thiếu tiền tố `Bearer` trả 401 — ĐẠT
- Token hợp lệ nhưng chủ thể không còn trong cơ sở dữ liệu trả 401 — ĐẠT

## AnonymousEndpointTests (tích hợp, kiểm tra tĩnh)

- Chỉ đúng một action được gắn `[AllowAnonymous]` là `POST /api/Auth/Login` — ĐẠT
- Mọi controller đều kế thừa `BaseController` nên mặc định yêu cầu đăng nhập — ĐẠT

## DateTimeProviderTests (đơn vị)

- Thời điểm hiện tại luôn mang độ lệch +07:00 của Asia/Ho_Chi_Minh — ĐẠT
- Thời điểm hiện tại bám đúng đồng hồ UTC của máy — ĐẠT
- Ngày hôm nay là ngày theo lịch Việt Nam, không theo múi giờ của tiến trình — ĐẠT

## BusinessSchemaTests (đơn vị, kế thừa từ T06)

- Cài đặt mặc định là 30/90/5 và chưa đặt tên đơn vị — ĐẠT
- Tên đợt trao huy hiệu là duy nhất và không phân biệt hoa thường — ĐẠT
- Đợt trao huy hiệu không lưu năm — ĐẠT
- Ba thực thể nghiệp vụ ánh xạ đúng bảng `app_setting`, `award_period`, `party_member` — ĐẠT
- Giới tính chỉ nhận Nam hoặc Nữ — ĐẠT
- Lược đồ không có bảng lưu kết quả đủ điều kiện — ĐẠT
- Các cột ngày của đảng viên đều kiểu ngày thuần — ĐẠT
- Họ tên đảng viên dùng đối chiếu tiếng Việt — ĐẠT
- Ngày vào Đảng chính thức là bắt buộc — ĐẠT

## SkeletonTests và SkeletonIntegrationTests (đơn vị, nền sẵn có)

- Thực thể người dùng là một chủ thể JWT hợp lệ — ĐẠT
- Thực thể người dùng hỗ trợ kiểm tra mật khẩu băm — ĐẠT
- Cấu hình cơ sở dữ liệu mặc định để trống chuỗi kết nối — ĐẠT
