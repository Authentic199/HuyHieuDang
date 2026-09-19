# Báo cáo kiểm thử — API Cài đặt (T13)

Lệnh chạy: `dotnet test` (thư mục `BE/`)

Kết quả toàn giải pháp: 312 đạt / 0 hỏng / 0 bỏ qua — 113 `Core.UnitTests`, 44 `Infrastructure.UnitTests`, 1 `Infrastructure.IntegrationTests`, 154 `Web.IntegrationTests`.

Riêng bộ mới của T13: `dotnet test tests/HuyHieuDang.Web.IntegrationTests --filter "FullyQualifiedName~SettingsEndpointTests"` — 27 đạt / 0 hỏng.

## SettingsEndpointTests (HuyHieuDang.Web.IntegrationTests)

- Kho chưa có bản ghi cài đặt nào thì đọc ra mặc định 30 / 90 / 5 và không tự tạo bản ghi — ĐẠT
- Kho đã seed thì đọc ra đúng 30 / 90 / 5, 13 mốc và tên đơn vị đang lưu — ĐẠT
- Đổi Bước 5 thành 10 làm danh sách đủ điều kiện của Đợt 7/11 năm 2026 tụt từ 6 xuống 4 người và badge sót từ 7 xuống 6 ngay trong lời gọi kế tiếp — ĐẠT
- Lưu Tên đơn vị xong thì đọc lại thấy ngay và Dashboard cũng thấy — ĐẠT
- Tên đơn vị bỏ trống, chuỗi rỗng hay toàn khoảng trắng đều được lưu thành chưa đặt — ĐẠT
- Mốc bắt đầu lớn hơn mốc kết thúc bị từ chối 400 với khóa `Mes.AppSetting.Invalid.Range` — ĐẠT
- Bước bằng 0 hoặc âm bị từ chối 400 với khóa `Mes.AppSetting.Invalid.StepYears` — ĐẠT
- Mốc bắt đầu hoặc mốc kết thúc nhỏ hơn 1 bị từ chối 400 với đúng khóa lỗi của từng trường — ĐẠT
- Giá trị thiếu, để rỗng, ghi bằng chữ hoặc có phần thập phân đều bị từ chối 400 và không lọt xuống cơ sở dữ liệu — ĐẠT
- Tên đơn vị dài quá 200 ký tự bị từ chối 400 với khóa `Mes.AppSetting.OverLength.UnitName` — ĐẠT
- Khôi phục mặc định đưa ba mốc về 30 / 90 / 5 và giữ nguyên Tên đơn vị đang lưu — ĐẠT
- Gọi lưu hai lần liên tiếp thì bảng cài đặt vẫn đúng một bản ghi — ĐẠT
- Kho trống, lưu lần đầu tạo đúng một bản ghi và lần lưu sau không nhân bản thêm — ĐẠT
- Xem trước dãy mốc trả đúng dãy Bước 10 mà không ghi gì xuống cơ sở dữ liệu — ĐẠT
- Bỏ trống tham số xem trước nào thì lấy giá trị đang lưu của tham số đó — ĐẠT
- Tham số xem trước sai trả về đúng bộ khóa lỗi của lời gọi lưu — ĐẠT
- Chưa đăng nhập thì cả bốn endpoint cài đặt đều trả 401 — ĐẠT

## Các bộ kiểm thử có sẵn

Toàn bộ bài của T00A đến T12 chạy lại cùng lượt và vẫn đạt: 285 bài, không bài nào hỏng.
