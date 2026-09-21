# Kiểm thử đơn vị trên nhánh gộp các PR đang mở

Chạy trên nhánh `release/gop-pr-dang-mo` — nhánh gộp chín PR đang mở thành một PR duy nhất, sau khi gỡ xung đột ở `FE/src/App.tsx` và `FE/src/pages/dashboard/EligibleTableCard.tsx`.

Chỉ chạy tầng đơn vị. Tầng tích hợp và tầng end-to-end cần PostgreSQL và nginx trong Docker, không chạy ở lần này.

## Lệnh đã chạy và kết quả

```
dotnet build HuyHieuDang.sln          → 0 lỗi, 43 cảnh báo StyleCop có sẵn từ trước
dotnet test tests/HuyHieuDang.Core.UnitTests --no-build
  → Đạt: 140, Hỏng: 0, Bỏ qua: 0, tổng 140 (263 ms)
dotnet test tests/HuyHieuDang.Infrastructure.UnitTests --no-build
  → Đạt: 76, Hỏng: 0, Bỏ qua: 0, tổng 76 (2 s)
```

Tổng: **216 ca đạt, 0 ca hỏng.**

Phía Frontend chạy kèm, cùng nhánh: `npm run lint` sạch, `npm run build` dựng xong trong 7,83 giây, `npm run typecheck:e2e` sạch.

## HuyHieuDang.Core.UnitTests

### Qt1MilestoneSequenceTests — dãy mốc tuổi đảng

- Cài đặt mặc định 30–90 bước 5 sinh ra đúng 13 mốc — ĐẠT
- Bước 10 sinh ra đúng 7 mốc — ĐẠT
- Bước không rơi đúng mốc cuối thì dừng trước khi vượt quá — ĐẠT
- Bắt đầu bằng kết thúc thì chỉ có một mốc — ĐẠT
- Bước lớn hơn cả khoảng thì chỉ có mốc đầu — ĐẠT
- Bắt đầu lớn hơn kết thúc thì báo lỗi 400 — ĐẠT
- Biên không dương thì báo lỗi 400 (4 ca) — ĐẠT
- Bước nhỏ hơn 1 thì báo lỗi 400 (2 ca) — ĐẠT

### Qt2AnniversaryTests — ngày tròn mốc

- Ngày thường thì giữ nguyên ngày và tháng — ĐẠT
- Vào Đảng 29/02, năm đích nhuận thì giữ 29/02 — ĐẠT
- Vào Đảng 29/02, năm đích không nhuận thì lùi về 28/02 — ĐẠT
- Mốc bằng 0 thì trả lại chính ngày vào Đảng — ĐẠT
- Mốc âm thì báo lỗi 400 — ĐẠT

### Qt3PartyAgeTests — tuổi đảng

- Trước ngày kỷ niệm trong năm thì chưa tính năm hiện tại — ĐẠT
- Đúng ngày kỷ niệm thì tính thêm năm đó — ĐẠT
- Trước ngày kỷ niệm một ngày thì vẫn giữ số cũ — ĐẠT
- Vào Đảng hôm nay thì tuổi đảng bằng 0 — ĐẠT
- Ngày vào Đảng ở tương lai thì tuổi đảng bằng 0 — ĐẠT
- Vào Đảng 29/02, năm không nhuận tính mốc vào 28/02 — ĐẠT
- Vào Đảng 29/02, trước 28/02 một ngày thì chưa tính năm — ĐẠT
- Người cao tuổi đảng nhất cho ra số vượt mốc cao nhất — ĐẠT

### Qt3aNextMilestoneTests — mốc kế tiếp

- Tuổi đảng dưới mốc đầu thì mốc kế tiếp là mốc đầu — ĐẠT
- Tuổi đảng sát dưới một mốc thì mốc kế tiếp là mốc đó — ĐẠT
- Tuổi đảng bằng đúng một mốc thì nhảy sang mốc sau — ĐẠT
- Tuổi đảng bằng mốc cao nhất thì không còn mốc kế tiếp — ĐẠT
- Tuổi đảng vượt mốc cao nhất thì không còn mốc kế tiếp — ĐẠT
- Ngày tròn mốc kế tiếp của người dưới mốc đầu trả đúng ngày — ĐẠT
- Ngày tròn mốc kế tiếp của người vượt mốc cao nhất là rỗng — ĐẠT
- Bước 10 thì bỏ qua các mốc biến mất — ĐẠT
- Mốc kế tiếp khớp danh sách mẫu của bộ dữ liệu biên — ĐẠT
- Tuổi đảng khớp danh sách mẫu của bộ dữ liệu biên — ĐẠT

### Qt3bAdmissionDateRangeTests — khoảng ngày vào Đảng suy ra từ mốc

- Suy ngày vào Đảng muộn nhất theo số năm tròn — ĐẠT
- Tuổi 0 thì ngày muộn nhất là hôm nay — ĐẠT
- Hôm nay 28/02 năm thường thì khoảng có chứa ngày nhuận — ĐẠT
- Hôm nay 28/02 năm nhuận thì khoảng loại ngày nhuận ra — ĐẠT
- Hôm nay 29/02 thì rơi đúng 28/02 của năm không nhuận — ĐẠT
- Tuổi âm thì báo lỗi — ĐẠT
- Mốc đầu tiên thì khoảng không có cận trên — ĐẠT
- Mốc giữa thì khoảng nửa mở — ĐẠT
- Lọc "chưa tới mốc nào" thì khoảng không có cận dưới — ĐẠT
- Mốc không nằm trong dãy thì báo lỗi — ĐẠT
- Khoảng bám theo cài đặt mốc hiện hành — ĐẠT
- Khoảng cho cùng kết quả với mốc kế tiếp trên mọi người của bộ dữ liệu — ĐẠT
- Khoảng cho cùng kết quả với mốc kế tiếp quanh các ngày nhuận (4 ca) — ĐẠT

### Qt4EligibilityTests — đủ điều kiện theo đợt

- Ngày tròn mốc rơi đúng ngày đầu đợt thì đủ điều kiện — ĐẠT
- Ngày tròn mốc rơi đúng ngày cuối đợt thì đủ điều kiện — ĐẠT
- Trước đợt một ngày thì không đủ điều kiện — ĐẠT
- Sau đợt một ngày thì không đủ điều kiện — ĐẠT
- Mốc 29/02 lùi về 28/02 vẫn rơi trong đợt — ĐẠT
- Đợt bắt đầu 29/02 ở năm không nhuận thì xét theo 28/02 — ĐẠT
- Người đúng mốc cao nhất vẫn trả về mốc 90 — ĐẠT
- Người đã vượt mốc cao nhất thì không đủ điều kiện — ĐẠT
- Bước 10 thì mất người có mốc biến mất — ĐẠT
- Bước 10 thì giữ người có mốc còn lại — ĐẠT
- Năm không có mốc nào rơi vào thì không ai đủ điều kiện — ĐẠT
- Chạy trên trọn bộ dữ liệu lõi thì khớp số người mong đợi (6 kịch bản) — ĐẠT

### Qt6PeriodWarningTests — cảnh báo khoảng trống và chồng lấn

- Bộ đợt chính của năm 2026 sinh đúng 5 khoảng trống — ĐẠT
- Khoảng trống đầu và cuối được gọi tên theo vị trí — ĐẠT
- Các đợt phủ kín năm thì không còn khoảng trống — ĐẠT
- Chưa có đợt nào thì cả năm là một khoảng trống — ĐẠT
- Chưa có đợt nào thì khoảng trống mang nhãn trước đợt đầu tiên — ĐẠT
- Năm nhuận thì khoảng trống kết thúc đúng ngày tháng Hai — ĐẠT
- Bộ đợt chính không có chồng lấn nào — ĐẠT
- Hai đợt cắt nhau thì báo đúng cặp theo thứ tự — ĐẠT
- Hai đợt chung đúng một ngày vẫn bị báo — ĐẠT
- Hai đợt sát nhau nhưng không đè thì không báo — ĐẠT
- Từ ngày sau đến ngày thì báo lỗi 400 — ĐẠT
- Ngày hoặc tháng không tồn tại thì báo lỗi 400 (6 ca) — ĐẠT
- Đợt bắt đầu 29/02 vẫn được chấp nhận — ĐẠT

### Qt7MissedMilestoneTests — mốc bị bỏ sót

- Mốc rơi trước đợt đầu tiên thì gắn nhãn trước đợt đầu — ĐẠT
- Mốc rơi giữa hai đợt thì gọi tên cả hai đợt — ĐẠT
- Mốc rơi sau đợt cuối thì gắn nhãn sau đợt cuối — ĐẠT
- Mốc rơi trong một đợt thì không bị coi là bỏ sót — ĐẠT
- Năm không có mốc nào thì không có gì bỏ sót — ĐẠT
- Chưa có đợt nào thì cả năm là khoảng trống — ĐẠT
- Bước 10 thì bỏ người có mốc biến mất — ĐẠT
- Chạy trên trọn bộ dữ liệu lõi thì khớp bảng mong đợi (4 năm 2025–2028) — ĐẠT

### Qt8UpcomingPeriodTests — đợt sắp tới

- Tại mốc thời gian T0 trả đúng đợt kế tiếp trong năm kèm số ngày còn lại — ĐẠT
- Hôm nay nằm trong một đợt thì trả chính đợt đó, trạng thái đang diễn ra — ĐẠT
- Đúng ngày cuối đợt vẫn tính là đang diễn ra — ĐẠT
- Mọi đợt trong năm đã qua thì lấy đợt sớm nhất năm sau — ĐẠT
- Sau ngày kết thúc đợt cuối một ngày thì chuyển sang năm sau — ĐẠT
- Chưa có đợt nào thì trả rỗng — ĐẠT
- Đợt bắt đầu 29/02 thì năm sau xét theo 28/02 — ĐẠT
- Hai đợt chồng nhau thì chọn đợt bắt đầu sớm hơn — ĐẠT
- Hai đợt cùng ngày bắt đầu thì kết quả không phụ thuộc thứ tự đầu vào — ĐẠT
- Hai đợt cùng ngày bắt đầu thì ưu tiên đợt kết thúc sớm hơn, rồi đến tên — ĐẠT
- Quy tắc phân định đó giữ nguyên khi chuyển sang năm sau — ĐẠT
- Chạy trên mọi kịch bản của bộ dữ liệu biên thì khớp đợt mong đợi (8 kịch bản) — ĐẠT

### Qt11PeriodStatusTests — trạng thái đợt

- Đợt đã kết thúc thì là Đã qua, không có số ngày còn lại — ĐẠT
- Hôm nay nằm trong đợt thì là Đang diễn ra, không có số ngày còn lại — ĐẠT
- Đúng ngày đầu đợt thì là Đang diễn ra — ĐẠT
- Đúng ngày cuối đợt thì là Đang diễn ra — ĐẠT
- Sau ngày kết thúc một ngày thì là Đã qua — ĐẠT
- Đợt chưa bắt đầu thì là Sắp tới kèm số ngày còn lại — ĐẠT
- Trước ngày bắt đầu một ngày thì còn đúng một ngày — ĐẠT
- Đợt bắt đầu 29/02 ở năm không nhuận thì xét theo 28/02 — ĐẠT
- Đợt kết thúc 29/02 thì năm nhuận giữ nguyên ngày — ĐẠT
- Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026 (5 ca) — ĐẠT
- Năm đã qua thì mọi đợt là Đã qua, năm sau thì là Sắp tới — ĐẠT
- Quá tải không tham số năm vẫn xét đúng năm hiện tại — ĐẠT
- Đợt 29/02 ở năm không nhuận xét theo 28/02 — ĐẠT
- Chạy trên mọi kịch bản của bộ dữ liệu biên thì khớp trạng thái mong đợi (7 kịch bản) — ĐẠT

## HuyHieuDang.Infrastructure.UnitTests

### BusinessSchemaTests — lược đồ cơ sở dữ liệu

- Cài đặt mặc định là 30 / 90 / 5 và chưa có tên đơn vị — ĐẠT
- Tên đợt là duy nhất và không phân biệt hoa thường — ĐẠT
- Bảng đợt không lưu năm — ĐẠT
- Các bảng nghiệp vụ đặt tên kiểu gạch dưới — ĐẠT
- Giới tính chỉ nhận Nam và Nữ — ĐẠT
- Không tồn tại bảng lưu sẵn kết quả xét duyệt — ĐẠT
- Các cột ngày của đảng viên là kiểu ngày thuần, không kèm giờ — ĐẠT
- Cột họ tên dùng kiểu so sánh tiếng Việt — ĐẠT
- Ngày vào Đảng chính thức là cột bắt buộc — ĐẠT

### DateTimeProviderTests — đồng hồ hệ thống

- Giờ hiện tại bám theo đồng hồ UTC — ĐẠT
- Giờ hiện tại quy về múi giờ Việt Nam — ĐẠT
- Hôm nay là ngày theo lịch Việt Nam — ĐẠT
- Ngoài môi trường sản xuất, biến ngày giả lập có tác dụng — ĐẠT
- Trong môi trường sản xuất, biến ngày giả lập bị bỏ qua — ĐẠT
- Biến ngày giả lập sai định dạng thì bị bỏ qua — ĐẠT
- Tên biến ngày giả lập giữ đúng như đã thống nhất — ĐẠT

### TestTodayVariableTests — biến ngày giả lập

- Trong môi trường sản xuất thì bị bỏ qua và có ghi cảnh báo — ĐẠT
- Ngoài môi trường sản xuất thì ép được ngày hôm nay — ĐẠT
- Không đặt biến thì không ép gì và không cảnh báo gì — ĐẠT

### QueryFilterValidationTests — chặn giá trị lọc lạ (T47)

- Không có bộ lọc thì giữ nguyên danh sách — ĐẠT
- Danh sách rỗng vẫn trả 400 cho giá trị lạ — ĐẠT
- Tên trường không tồn tại thì giữ nguyên hành vi cũ — ĐẠT
- Giá trị enum hợp lệ vẫn lọc đúng — ĐẠT
- Giá trị enum sai kiểu thì trả 400 — ĐẠT
- Giá trị số sai kiểu thì trả 400 — ĐẠT
- Toán tử không tồn tại hoặc thiếu toán tử thì trả 400 — ĐẠT
- Toán tử `in` hợp lệ vẫn lọc đúng — ĐẠT
- Toán tử `in` trên trường có thể rỗng vẫn lọc đúng — ĐẠT
- Toán tử `btw` hợp lệ vẫn lọc đúng — ĐẠT
- Toán tử `btw` trên trường ngày hợp lệ vẫn lọc đúng — ĐẠT
- Toán tử `ilike` và `sw` trên chuỗi vẫn lọc đúng — ĐẠT
- Toán tử `ilike` trên trường không phải chuỗi thì trả 400 — ĐẠT
- Toán tử `null` trên trường có thể rỗng vẫn lọc đúng — ĐẠT
- Toán tử `null` trên trường không thể rỗng thì trả 400 — ĐẠT
- Tiền tố `not` với giá trị hợp lệ vẫn lọc đúng — ĐẠT
- Chặn `IEnumerable` cho cùng hành vi như chặn danh sách thường — ĐẠT

### SkeletonTests — nền tảng sẵn có

- Người dùng là một chủ thể JWT — ĐẠT
- Người dùng kiểm tra được mật khẩu — ĐẠT
