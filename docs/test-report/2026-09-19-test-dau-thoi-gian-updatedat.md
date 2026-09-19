# Báo cáo kiểm thử — Dấu thời gian UpdatedAt và hai ca A-901

Lệnh đã chạy: `cd BE && dotnet test HuyHieuDang.sln`

Tổng: **267 đạt, 0 hỏng, 1 bỏ qua**.

```
Passed! - Failed: 0, Passed: 118, Skipped: 1, Total: 119 - HuyHieuDang.Core.QcTests.dll
Passed! - Failed: 0, Passed: 114, Skipped: 0, Total: 114 - HuyHieuDang.Core.UnitTests.dll
Passed! - Failed: 0, Passed:   1, Skipped: 0, Total:   1 - HuyHieuDang.Infrastructure.IntegrationTests.dll
Passed! - Failed: 0, Passed:  16, Skipped: 0, Total:  16 - HuyHieuDang.Infrastructure.UnitTests.dll
Passed! - Failed: 0, Passed:  18, Skipped: 0, Total:  18 - HuyHieuDang.Web.IntegrationTests.dll
```

Lần chạy trước có hai ca hỏng; báo cáo này ghi lại tình trạng sau khi sửa.

## Qc11ClockScanTests — hai ca A-901 trước đó hỏng

- Không nơi nào trong `BE/src` đọc đồng hồ theo giờ máy — ĐẠT (trước đó HỎNG vì chú thích trong `IDateTimeProvider.cs` có nhắc tên lời gọi bị cấm; đã viết lại chú thích)
- Module nghiệp vụ trong Infrastructure không đọc đồng hồ — ĐẠT (trước đó HỎNG vì ba thực thể tự gán mặc định lần sửa gần nhất bằng đồng hồ máy; nay tầng lưu trữ đóng dấu)
- Service tính mốc tuổi đảng không đọc đồng hồ, chỉ nhận ngày qua tham số — ĐẠT
- Core không được đọc đồng hồ hệ thống — BỎ QUA (quyết định của CEO, giữ nguyên)

## UpdatedAtStampTests — ca mới

- Cài đặt seed sẵn mang dấu thời gian lấy từ nguồn thời gian chung, không phải giờ máy — ĐẠT
- Thêm mới đảng viên được đóng dấu dù không ai gán lần sửa gần nhất — ĐẠT
- Sửa đảng viên ghi đè dấu thời gian cũ, không giữ nguyên giá trị lỗi thời — ĐẠT

## Các bộ còn lại

Toàn bộ ca của `Core.QcTests`, `Core.UnitTests`, `Infrastructure.UnitTests`,
`Infrastructure.IntegrationTests` và 15 ca đăng nhập trong `Web.IntegrationTests`
giữ nguyên kết quả ĐẠT như các báo cáo trước; không ca nào đổi trạng thái vì thay đổi lần này.
