# Báo cáo kiểm thử T47 — giá trị lọc không hợp lệ trả 400

Lệnh: `cd BE && dotnet test HuyHieuDang.sln`

Kết quả: 628 ĐẠT · 0 HỎNG · 8 bỏ qua (7 ca QC còn treo cờ Skip, 1 ca Core QC).

| Bộ kiểm thử | ĐẠT | HỎNG | Bỏ qua |
|---|---|---|---|
| HuyHieuDang.Core.UnitTests | 124 | 0 | 0 |
| HuyHieuDang.Core.QcTests | 118 | 0 | 1 |
| HuyHieuDang.Infrastructure.UnitTests | 65 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |
| HuyHieuDang.Web.IntegrationTests | 188 | 0 | 0 |
| HuyHieuDang.Web.QcIntegrationTests | 132 | 0 | 7 |

Dưới đây là hai lớp mới của T47; các lớp còn lại giữ nguyên và vẫn ĐẠT như báo cáo trước.

## QueryFilterValidationTests (Infrastructure.UnitTests)

- Không gửi bộ lọc, hoặc bộ lọc rỗng, thì danh sách giữ nguyên — ĐẠT
- Lọc giới tính bằng tên hằng số hợp lệ vẫn lọc đúng — ĐẠT
- Giá trị giới tính lạ `Khac` bị từ chối 400 — ĐẠT
- Giá trị giới tính rỗng bị từ chối 400 — ĐẠT
- Giá trị giới tính bằng số `1` bị từ chối 400 — ĐẠT
- Danh sách `$in` có một phần tử sai kiểu bị từ chối 400, không lọc một nửa — ĐẠT
- Tiền tố `$not` với giá trị sai kiểu cũng bị từ chối 400 — ĐẠT
- Giá trị số sai kiểu ở `$gte` và ở `$btw` bị từ chối 400 — ĐẠT
- `$btw` thiếu vế thứ hai bị từ chối 400 — ĐẠT
- Thiếu toán tử hoặc toán tử không tồn tại bị từ chối 400 — ĐẠT
- `$in` với mọi giá trị hợp lệ vẫn lọc đúng — ĐẠT
- `$in` trên trường ngày có thể rỗng vẫn lọc đúng — ĐẠT
- `$btw` trên số và trên ngày với giá trị hợp lệ vẫn lọc đúng — ĐẠT
- `$ilike` và `$sw` trên trường chữ vẫn lọc đúng — ĐẠT
- `$ilike` trên trường không phải chữ bị từ chối 400 — ĐẠT
- `$null` trên trường có thể rỗng vẫn lọc đúng — ĐẠT
- `$null` trên trường không thể rỗng bị từ chối 400 — ĐẠT
- `$not:$eq` với giá trị hợp lệ vẫn loại đúng nhóm cần loại — ĐẠT
- Tên trường không tồn tại vẫn được bỏ qua như trước, không báo lỗi — ĐẠT
- Bản lọc trên bộ nhớ cho kết quả giống bản lọc trên truy vấn — ĐẠT
- Bảng rỗng vẫn trả 400 cho giá trị lọc lạ — ĐẠT

## PartyMemberFilterValueTests (Web.IntegrationTests)

- `filter.Gender=$eq:Khac` trả 400 kèm khóa `Mes.Common.Invalid.Parameter` — ĐẠT
- `filter.Gender=$eq:` (giá trị rỗng) trả 400 kèm khóa trên — ĐẠT
- `filter.Gender=$eq:1` trả 400 kèm khóa trên — ĐẠT
- `filter.Gender=$in:Male,Khac` trả 400 kèm khóa trên — ĐẠT
- `filter.Gender=Male` (thiếu toán tử) trả 400 kèm khóa trên — ĐẠT
- `filter.OfficialAdmissionDate=$gte:hom-qua` trả 400 kèm khóa trên — ĐẠT
- Mọi phản hồi 400 trên không để lọt dấu vết ngăn xếp (A-905) — ĐẠT
- Lọc hợp lệ `$eq:Male`, `$in:Male,Female` và không lọc vẫn cho đúng số dòng như trước — ĐẠT

## Bằng chứng gọi thật qua HTTP

```
GET /api/PartyMembers?filter.Gender=$eq:Khac
HTTP 400
{"message":"Mes.Common.Invalid.Parameter","statusCode":400}

GET /api/PartyMembers?filter.Gender=$eq:Male
HTTP 200
{"message":"Mes.PartyMember.Search.Successfully","data":{"pagedData":[{"id":"07150000-00a2-9c6b-afa6-08df171ec220","fullName":"Cao Văn Phúc","dateOfBirth":null,"gender":"Male","officialAdmissionDate":"1976-09-12","partyAgeYears":50,"nextMilestone":55,"nextMilestoneDate":"2031-09-12","createdAt":"2026-09-20T13:54:58.327951Z","updatedAt":"2026-09-19T02:30:00.000000Z"}],"pageInfo":{"totalCount":1,"pageSize":20,"current":1,"totalPages":1,"hasNext":false,"hasPrevious":false}}}
```
