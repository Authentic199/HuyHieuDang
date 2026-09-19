# Báo cáo kiểm thử — API Đảng viên (T09)

Lệnh đã chạy: `cd BE && dotnet test HuyHieuDang.sln`

Kết quả: 163 đạt · 0 hỏng · 0 bỏ qua.

| Dự án kiểm thử | Đạt | Hỏng | Bỏ qua |
|---|---|---|---|
| HuyHieuDang.Core.UnitTests | 103 | 0 | 0 |
| HuyHieuDang.Infrastructure.UnitTests | 16 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |
| HuyHieuDang.Web.IntegrationTests | 43 | 0 | 0 |

Báo cáo này ghi chi tiết nhóm mới của T09 là `PartyMemberEndpointTests`. Các nhóm còn lại đã có báo cáo riêng ở những lần bàn giao trước và lần chạy này vẫn xanh nguyên.

## PartyMemberEndpointTests

Danh sách trả giới tính dạng chuỗi và ba giá trị tuổi đảng tính theo ngày 19/09/2026 — ĐẠT
Ô trống trả null và người đã vượt mốc 90 năm không còn mốc kế tiếp — ĐẠT
Phân trang mặc định 20 dòng, chọn được số dòng và nhảy đúng trang cuối — ĐẠT
Số dòng mỗi trang bằng 0 trả lỗi 400 — ĐẠT
Số trang bằng 0 trả lỗi 400 — ĐẠT
Tìm theo họ tên chứa chuỗi, không phân biệt hoa thường — ĐẠT
Lọc giới tính Nam trả đúng một người nam — ĐẠT
Lọc giới tính Nữ trả đúng một người nữ — ĐẠT
Bỏ tham số lọc giới tính thì lấy tất cả — ĐẠT
Sắp xếp theo họ tên giảm dần đúng thứ tự — ĐẠT
Sắp xếp theo ngày vào Đảng chính thức tăng dần đúng thứ tự — ĐẠT
Sắp theo cột tính ra bị bỏ qua và quay về thứ tự mặc định theo họ tên — ĐẠT
Thêm mới trả 200 kèm bản ghi vừa tạo và khóa Mes.PartyMember.Create.Successfully — ĐẠT
Thêm hai người trùng hệt nhau vẫn thành hai bản ghi, đúng QT9 — ĐẠT
Thiếu họ tên trả khóa Mes.PartyMember.Required.FullName — ĐẠT
Họ tên toàn khoảng trắng cũng coi là thiếu họ tên — ĐẠT
Thiếu ngày chính thức trả khóa Mes.PartyMember.Required.OfficialAdmissionDate — ĐẠT
Ngày chính thức ở tương lai trả khóa Mes.PartyMember.Invalid.OfficialAdmissionDate — ĐẠT
Ngày sinh sau ngày chính thức trả khóa Mes.PartyMember.Invalid.DateOfBirth — ĐẠT
Giới tính khác Male và Female trả khóa Mes.PartyMember.Invalid.Gender — ĐẠT
Ngày chính thức đúng bằng hôm nay là hợp lệ, tuổi đảng bằng 0 và mốc kế tiếp là 30 — ĐẠT
Lấy một đảng viên trả đúng bản ghi — ĐẠT
Lấy một đảng viên bằng id không tồn tại trả 400 kèm Mes.PartyMember.NotFound — ĐẠT
Sửa ghi đè cả bốn trường, gửi null thì xóa trắng ngày sinh và giới tính — ĐẠT
Sửa id không tồn tại trả 400 kèm Mes.PartyMember.NotFound — ĐẠT
Xóa một trả id vừa xóa và bản ghi biến mất hẳn, đúng QT10 — ĐẠT
Xóa id không tồn tại trả 400 kèm Mes.PartyMember.NotFound — ĐẠT
Xóa nhiều trả đúng những id đã xóa, id lạ bị bỏ qua lặng lẽ — ĐẠT
Xóa nhiều với danh sách rỗng trả 400 — ĐẠT
Gọi danh sách đảng viên không kèm token trả 401 — ĐẠT
Gọi thêm mới không kèm token trả 401 — ĐẠT
Gọi xóa nhiều không kèm token trả 401 — ĐẠT

## Chạy thử tay trên PostgreSQL thật

Dựng API trên một container PostgreSQL rời rồi gọi thật để lấy bằng chứng enum trả chuỗi.

Thêm mới trả `"gender": "Male"` cùng `partyAgeYears`, `nextMilestone`, `nextMilestoneDate` — ĐẠT
Danh sách sắp theo họ tên cho ra thứ tự tiếng Việt Bùi · Cao · Đỗ — ĐẠT
Lọc `filter.Gender=$eq:Male` trả đúng một dòng — ĐẠT
Giới tính lạ trả 400 kèm `Mes.PartyMember.Invalid.Gender` — ĐẠT
