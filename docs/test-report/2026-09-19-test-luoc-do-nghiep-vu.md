# Báo cáo kiểm thử — Lược đồ nghiệp vụ, migration và seed (T06)

Lệnh chạy: `dotnet test HuyHieuDang.sln` (thư mục `BE/`)

Kết quả: 14 đạt / 0 hỏng / 0 bỏ qua.

## BusinessSchemaTests (HuyHieuDang.Infrastructure.UnitTests)

- Ba thực thể nghiệp vụ ánh xạ đúng tên bảng `party_member` — ĐẠT
- Ba thực thể nghiệp vụ ánh xạ đúng tên bảng `award_period` — ĐẠT
- Ba thực thể nghiệp vụ ánh xạ đúng tên bảng `app_setting` — ĐẠT
- Ngày sinh và Ngày vào Đảng chính thức lưu kiểu ngày thuần `date`, không kèm giờ — ĐẠT
- Họ tên và Ngày vào Đảng chính thức là bắt buộc; Ngày sinh và Giới tính được để trống — ĐẠT
- Cột Họ tên dùng đối chiếu tiếng Việt `vi-x-icu` để sắp đúng bảng chữ cái (OQ-3) — ĐẠT
- Tên đợt trao huy hiệu là duy nhất và không phân biệt hoa thường — ĐẠT
- Đợt trao huy hiệu không lưu bất kỳ cột năm nào, chỉ ngày và tháng (QT6) — ĐẠT
- Cài đặt mới mặc định 30 / 90 / 5 và bỏ trống tên đơn vị — ĐẠT
- Giới tính chỉ có hai giá trị Male và Female, đúng hợp đồng API — ĐẠT
- Không có bảng nào lưu danh sách đủ điều kiện, luôn tính lại khi truy vấn (QT5) — ĐẠT

## SkeletonTests (HuyHieuDang.Infrastructure.UnitTests)

- Mật khẩu người dùng băm xong thì kiểm tra lại đúng mật khẩu là hợp lệ, sai mật khẩu là không hợp lệ — ĐẠT
- Thực thể người dùng dùng được làm chủ thể phát hành JWT — ĐẠT

## SkeletonIntegrationTests (HuyHieuDang.Infrastructure.IntegrationTests)

- Cấu hình cơ sở dữ liệu mặc định để trống chuỗi kết nối và tắt tự động migration, buộc phải nạp từ biến môi trường — ĐẠT
