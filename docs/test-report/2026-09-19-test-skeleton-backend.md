# Báo cáo kiểm thử — Skeleton Backend (T00A)

Lệnh chạy: `dotnet test HuyHieuDang.sln` (thư mục `BE/`)

Kết quả: 3 đạt / 0 hỏng / 0 bỏ qua.

## SkeletonTests (HuyHieuDang.Infrastructure.UnitTests)

- Mật khẩu người dùng băm xong thì kiểm tra lại đúng mật khẩu là hợp lệ, sai mật khẩu là không hợp lệ — ĐẠT
- Thực thể người dùng dùng được làm chủ thể phát hành JWT — ĐẠT

## SkeletonIntegrationTests (HuyHieuDang.Infrastructure.IntegrationTests)

- Cấu hình cơ sở dữ liệu mặc định để trống chuỗi kết nối và tắt tự động migration, buộc phải nạp từ biến môi trường — ĐẠT
