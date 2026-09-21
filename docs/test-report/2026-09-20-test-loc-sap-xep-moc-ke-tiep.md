# Kết quả kiểm thử — T51: lọc theo Mốc kế tiếp và sắp xếp theo Tuổi đảng / Mốc kế tiếp

Lệnh đã chạy: `cd BE && dotnet test`

Tổng: 632 đạt, 0 hỏng, 8 bỏ qua.

| Bộ test | Đạt | Hỏng | Bỏ qua |
|---|---|---|---|
| HuyHieuDang.Core.UnitTests | 140 | 0 | 0 |
| HuyHieuDang.Core.QcTests | 118 | 0 | 1 |
| HuyHieuDang.Infrastructure.UnitTests | 44 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |
| HuyHieuDang.Web.IntegrationTests | 197 | 0 | 0 |
| HuyHieuDang.Web.QcIntegrationTests | 132 | 0 | 7 |

Dưới đây là các lớp test của T51. Các lớp còn lại không đổi và vẫn đạt toàn bộ.

## Qt3bAdmissionDateRangeTests — phép quy đổi mốc thành khoảng ngày

Trừ đúng số năm tròn ra ngày vào Đảng muộn nhất — ĐẠT
Mốc 0 năm cho ra đúng ngày hôm nay — ĐẠT
Hôm nay 28/02 năm không nhuận thì người vào Đảng 29/02 vẫn được tính đủ mốc — ĐẠT
Hôm nay 28/02 năm nhuận thì người vào Đảng 29/02 chưa được tính đủ mốc — ĐẠT
Hôm nay 29/02 thì khoảng ngày lùi về 28/02 của năm không nhuận — ĐẠT
Số năm âm bị từ chối — ĐẠT
Mốc đầu tiên của dãy không có cận trên, để người vào Đảng ngày tương lai vẫn nằm trong đó — ĐẠT
Mốc ở giữa dãy cho khoảng nửa mở đúng hai đầu — ĐẠT
Giá trị None không có cận dưới, chỉ chặn cận trên tại mốc lớn nhất — ĐẠT
Mốc không nằm trong dãy hiện hành bị từ chối — ĐẠT
Đổi Bước từ 5 sang 10 thì khoảng ngày của cùng một mốc đổi theo — ĐẠT
Khoảng ngày chọn ra đúng những người mà QT3a trả cùng mốc, trên toàn bộ 32 đảng viên của bộ dữ liệu QC — ĐẠT
Quét từng ngày quanh mọi biên mốc ở bốn ngày "hôm nay" khác nhau (28/02, 29/02, 31/12, 01/01) không lệch ngày nào — ĐẠT

## PartyMemberSqlTranslationTests — bằng chứng điều kiện nằm trong SQL

Lọc theo mốc 40 sinh ra hai bất đẳng thức trên cột ngày chính thức trong mệnh đề WHERE — ĐẠT
Câu đếm tổng số dòng cũng mang đúng hai bất đẳng thức đó — ĐẠT
Lọc None chỉ sinh ra cận trên, không có cận dưới — ĐẠT
Sắp xếp `PartyAge asc` sinh ra ORDER BY giảm dần trên cột ngày chính thức — ĐẠT
Sắp xếp `PartyAge desc` sinh ra ORDER BY tăng dần trên cột ngày chính thức — ĐẠT
Sắp xếp `partyAgeYears asc` cho kết quả giống `PartyAge asc` — ĐẠT
Sắp xếp `NextMilestone asc` và `NextMilestone desc` cũng quy về cột ngày chính thức theo chiều ngược lại — ĐẠT

## PartyMemberEndpointTests — phần thêm mới của T51

Sắp xếp theo Tuổi đảng tăng dần đưa người vào Đảng muộn nhất lên đầu, nhận cả hai cách viết `PartyAge` và `partyAgeYears` — ĐẠT
Sắp xếp theo Tuổi đảng giảm dần cho thứ tự ngược lại — ĐẠT
Sắp xếp theo Mốc kế tiếp tăng dần xếp nhóm không còn mốc xuống cuối — ĐẠT
Sắp xếp theo Mốc kế tiếp giảm dần đưa nhóm không còn mốc lên đầu — ĐẠT
Lọc mốc 40 trả đúng hai người kể cả khi chỉ lấy một dòng mỗi trang, chứng tỏ tổng số dòng tính trong SQL — ĐẠT
Người tròn đúng 35 năm vào hôm nay được xếp vào mốc kế tiếp 40 — ĐẠT
Lọc mốc 30 trả đúng nhóm tương ứng — ĐẠT
Lọc None trả đúng người đã vượt mốc lớn nhất — ĐẠT
Lọc kết hợp với tìm theo họ tên vẫn cộng dồn đúng — ĐẠT
Đổi Bước trong Cài đặt sang 10 thì mốc 35 biến mất và bị trả 400, mốc 40 vẫn dùng được — ĐẠT
Mốc lạ `$eq:33`, `$eq:abc`, `$eq:` đều trả 400 kèm khóa `Mes.PartyMember.Invalid.NextMilestone` — ĐẠT
Toán tử khác `$eq` và giá trị thiếu toán tử cũng trả 400 với cùng khóa — ĐẠT
Cột sắp xếp lạ vẫn bị bỏ qua và quay về thứ tự mặc định theo Họ tên — ĐẠT
