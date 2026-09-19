# HuyHieuDang — Hợp đồng API v1

| | |
|---|---|
| Phiên bản | 1.0 — 19/09/2026 |
| Trạng thái | Chờ cổng duyệt 1 của CEO |
| Chủ sở hữu | Technical Writer |
| Nguồn nghiệp vụ | `docs/2026-09-17-huyhieudang-business-design.md` (v1.1) |
| Nguồn giao diện | `docs/design-system/Huy Hieu Dang - 9 man hinh.html` |
| Bản máy đọc | `docs/openapi.yaml` (OpenAPI 3.0.3, sinh từ tài liệu này) |

## Tài liệu này dùng để làm gì

Đây là bản cam kết giữa Backend và Frontend về mọi lời gọi HTTP của hệ thống HuyHieuDang. Chốt xong tài liệu này, hai bên làm song song: Backend dựng đúng những endpoint ở đây, Frontend gọi đúng những endpoint ở đây và có thể dựng dữ liệu giả theo đúng hình dạng mô tả.

Tài liệu bao gồm: quy ước chung (xác thực, phân trang, định dạng ngày, hình dạng lỗi, cách trả file), 28 endpoint chia theo 7 nhóm, bảng đối chiếu với từng use case UC-xx, và những quyết định kỹ thuật đã chốt.

**Quy tắc thay đổi:** không ai được đổi hình dạng request/response mà không báo. Xem mục 12.

---

## 1. Quy ước chung

### 1.1 Địa chỉ gốc

| Môi trường | Địa chỉ Frontend gọi | Thực tế trỏ tới |
|---|---|---|
| Dev | `/api` (Vite proxy) | `http://localhost:5000/api` |
| Docker compose | `/api` (nginx proxy) | `http://be:8080/api` |

Frontend luôn gọi đường dẫn tương đối `/api/...`, lấy từ biến `VITE_API_BASE_URL` (mặc định `/api`). Không viết cứng `http://localhost:5000` trong mã Frontend.

Mọi đường dẫn trong tài liệu này viết đầy đủ từ `/api/`.

### 1.2 Xác thực

- Cơ chế: **JWT Bearer**. Đăng nhập một lần, nhận `accessToken`, gắn vào mọi lời gọi sau:
  ```
  Authorization: Bearer <accessToken>
  ```
- Hạn token: **8 giờ** kể từ lúc đăng nhập. Hết hạn → mọi lời gọi trả `401`, Frontend xóa token và đưa về màn hình đăng nhập.
- **Không có refresh token trong v1.** Một người dùng, một máy, phiên 8 giờ là đủ.
- Không có phân quyền. Mọi endpoint (trừ `POST /api/Auth/Login`) đều yêu cầu token hợp lệ; không có endpoint nào yêu cầu quyền riêng.
- Frontend lưu token ở `localStorage`. Đăng xuất là xóa token phía client; endpoint `Logout` chỉ để ghi nhận, không huỷ token phía máy chủ (JWT không trạng thái).

### 1.3 Hình dạng phản hồi thành công

Mọi phản hồi thành công đều bọc trong một lớp vỏ chung:

```json
{
  "message": "Mes.PartyMember.Search.Successfully",
  "data": { }
}
```

| Trường | Kiểu | Ghi chú |
|---|---|---|
| `message` | string | **Khóa thông điệp**, không phải chữ hiển thị. Xem mục 1.5 |
| `data` | object \| array \| null | Nội dung thật. Mô tả `data` ở từng endpoint bên dưới |

Trong tài liệu này, phần "Phản hồi" của mỗi endpoint **chỉ mô tả `data`**, bỏ qua lớp vỏ. Mã trạng thái thành công luôn là `200 OK`, kể cả khi thêm mới (theo quy ước sẵn có của bộ khung Backend — không dùng `201`).

Tên trường JSON dùng **camelCase**.

### 1.4 Hình dạng lỗi

Có đúng hai hình dạng lỗi. Frontend phải xử lý được cả hai.

**Dạng A — lỗi xác thực dữ liệu đầu vào** (FluentValidation, mã `400`). Chỉ có một trường:

```json
{ "message": "Mes.PartyMember.Required.FullName" }
```

Chỉ trả **lỗi đầu tiên** tìm thấy, không trả danh sách lỗi theo từng trường. Vì vậy Frontend **phải tự kiểm tra hợp lệ tại chỗ** theo đúng bảng ràng buộc của từng form (Ant Design Form rules); lỗi từ máy chủ chỉ là lưới an toàn, hiển thị dạng banner.

**Dạng B — lỗi nghiệp vụ và lỗi hệ thống** (ngoại lệ có kiểm soát):

```json
{
  "statusCode": 400,
  "message": "Mes.AwardPeriod.Repeated.Name"
}
```

Khi là lỗi `500`, có thêm `traceId` và `supportMessage`:

```json
{
  "statusCode": 500,
  "message": "...",
  "traceId": "0HN7...",
  "supportMessage": "Please furnish the TraceId 0HN7... to our dedicated support team..."
}
```

Các trường gỡ lỗi (`exception`, `source`, `method`, `line`) **bị ẩn ở môi trường chạy thật**; Frontend không được phụ thuộc vào chúng. Trường có giá trị mặc định sẽ bị lược khỏi JSON — luôn kiểm tra `null` trước khi đọc.

**Bảng mã trạng thái**

| Mã | Khi nào | Frontend làm gì |
|---|---|---|
| `200` | Thành công (mọi thao tác, kể cả thêm mới) | Hiển thị dữ liệu / thông báo thành công |
| `400` | Dữ liệu sai, vi phạm ràng buộc nghiệp vụ, **không tìm thấy bản ghi** | Hiển thị thông điệp tương ứng với khóa |
| `401` | Sai tài khoản/mật khẩu, thiếu token, token hết hạn | Màn đăng nhập: báo "Sai tài khoản hoặc mật khẩu". Màn khác: xóa token, về đăng nhập |
| `500` | Lỗi máy chủ | Báo "Hệ thống gặp sự cố, vui lòng thử lại" + trạng thái lỗi của bảng |

> **Không tìm thấy trả `400`, không phải `404`.** Đây là quy ước của bộ khung Backend: `404` chỉ xảy ra khi gọi sai đường dẫn. Lấy một đảng viên bằng id không tồn tại → `400` kèm khóa `Mes.PartyMember.NotFound`.

### 1.5 Khóa thông điệp và chữ tiếng Việt

Backend trả **khóa** dạng `Mes.<ThựcThể>.<HànhĐộng>.<KếtQuả>`, không trả chữ tiếng Việt. Frontend giữ một bảng tra khóa → chữ, và có câu mặc định cho khóa lạ ("Thao tác không thực hiện được").

Lý do: bộ khung Backend sinh khóa tự động; để chữ tiếng Việt nằm một chỗ (Frontend) thì câu chữ trên giao diện luôn thống nhất với thiết kế.

**Bảng khóa bắt buộc phải có trong Frontend**

| Khóa | Chữ hiển thị |
|---|---|
| `Mes.User.Login.Successfully` | Đăng nhập thành công |
| `Mes.User.Login.Failed` | Sai tài khoản hoặc mật khẩu |
| `Mes.User.Logout.Successfully` | Đã đăng xuất |
| `Mes.PartyMember.Create.Successfully` | Đã thêm đảng viên |
| `Mes.PartyMember.Update.Successfully` | Đã lưu thay đổi |
| `Mes.PartyMember.Delete.Successfully` | Đã xóa |
| `Mes.PartyMember.Import.Successfully` | Đã nạp danh sách |
| `Mes.PartyMember.NotFound` | Không tìm thấy đảng viên |
| `Mes.PartyMember.Required.FullName` | Chưa nhập Họ tên |
| `Mes.PartyMember.Required.OfficialAdmissionDate` | Chưa nhập Ngày vào Đảng chính thức |
| `Mes.PartyMember.Invalid.OfficialAdmissionDate` | Ngày chính thức không được ở tương lai |
| `Mes.PartyMember.Invalid.DateOfBirth` | Ngày sinh phải trước Ngày vào Đảng chính thức |
| `Mes.PartyMember.Invalid.Gender` | Giới tính chỉ nhận Nam hoặc Nữ |
| `Mes.AwardPeriod.Create.Successfully` | Đã thêm đợt trao huy hiệu |
| `Mes.AwardPeriod.Update.Successfully` | Đã lưu thay đổi |
| `Mes.AwardPeriod.Delete.Successfully` | Đã xóa đợt trao huy hiệu |
| `Mes.AwardPeriod.NotFound` | Không tìm thấy đợt trao huy hiệu |
| `Mes.AwardPeriod.Required.Name` | Chưa nhập Tên đợt |
| `Mes.AwardPeriod.Repeated.Name` | Tên đợt đã tồn tại |
| `Mes.AwardPeriod.Invalid.FromDate` | Từ ngày không hợp lệ |
| `Mes.AwardPeriod.Invalid.ToDate` | Đến ngày không hợp lệ |
| `Mes.AwardPeriod.Invalid.Range` | Đến ngày phải bằng hoặc sau Từ ngày trong cùng một năm |
| `Mes.AppSetting.Update.Successfully` | Đã lưu cài đặt |
| `Mes.AppSetting.Invalid.StartYears` | Mốc bắt đầu phải là số nguyên dương |
| `Mes.AppSetting.Invalid.EndYears` | Mốc kết thúc phải là số nguyên dương |
| `Mes.AppSetting.Invalid.StepYears` | Bước nhảy phải từ 1 trở lên |
| `Mes.AppSetting.Invalid.Range` | Mốc bắt đầu phải nhỏ hơn hoặc bằng mốc kết thúc |
| `Mes.Import.Invalid.Extension` | Chỉ nhận file .xlsx |
| `Mes.Import.Invalid.FileSize` | File vượt quá 10 MB |
| `Mes.Import.Invalid.Columns` | File phải có đúng 4 cột theo thứ tự Họ tên · Ngày sinh · Giới tính · Ngày vào Đảng chính thức |
| `Mes.Import.Invalid.Empty` | File không có dòng dữ liệu nào |
| `Mes.Dashboard.NotFound.UpcomingPeriod` | Chưa cài đợt trao huy hiệu |
| `Mes.Query.Invalid.Year` | Năm không hợp lệ |

Các khóa `*.Search.Successfully`, `*.Detail.Successfully` không cần hiển thị gì; Frontend bỏ qua.

### 1.6 Định dạng ngày và số

| Loại | Trên API | Trên giao diện |
|---|---|---|
| Ngày (không giờ) | chuỗi `"yyyy-MM-dd"`, ví dụ `"1996-10-15"` | `dd/MM/yyyy` → `15/10/1996` |
| Ngày/tháng của đợt | hai số nguyên `fromDay` + `fromMonth` | `dd/MM` → `01/10` |
| Mốc thời gian hệ thống | ISO UTC `"2026-09-19T08:30:00Z"` | thường không hiển thị |
| Số lượng | số nguyên JSON | dấu chấm ngăn nghìn: `1.248` |
| Ô trống | `null` | hiển thị `—` (trừ trong file Excel: để rỗng) |

**Bắt buộc:** mọi trường ngày nghiệp vụ (`dateOfBirth`, `officialAdmissionDate`, `milestoneDate`, `fromDate`, `toDate`, `today`) là **ngày thuần**, kiểu `DateOnly` phía Backend, tuần tự hóa thành chuỗi `yyyy-MM-dd`. **Không dùng `DateTime`** cho các trường này: bộ khung Backend có bộ chuyển đổi tự quy `DateTime` về UTC, sẽ làm lệch ngày một đơn vị.

Ngày hôm nay luôn lấy từ **máy chủ** (`GET /api/Auth/Me` → `serverDate`, và `GET /api/Dashboard` → `today`), không lấy từ đồng hồ trình duyệt.

### 1.7 Phân trang, tìm kiếm, sắp xếp, lọc

Chỉ **một** endpoint có phân trang: danh sách đảng viên (`GET /api/PartyMembers`). Các danh sách còn lại (đợt trao huy hiệu, đủ điều kiện, chưa thuộc đợt nào) trả về **toàn bộ**, vì mỗi lần xem chỉ vài chục đến vài trăm dòng và thiết kế hiển thị cả bảng; Frontend tự phân trang phía client nếu cần.

**Tham số truy vấn của endpoint có phân trang**

| Tham số | Kiểu | Mặc định | Ý nghĩa |
|---|---|---|---|
| `current` | int ≥ 1 | `1` | Trang số mấy |
| `pageSize` | int ≥ 1 | `20` | Số dòng mỗi trang |
| `searchKeyword` | string | – | Từ khóa tìm (chứa chuỗi, không phân biệt hoa thường và dấu) |
| `searchFields` | string[] | – | Trường để tìm. Danh sách đảng viên chỉ dùng `FullName` |
| `sortQuery` | string | `FullName asc` | Ví dụ `FullName desc`, `OfficialAdmissionDate asc` |
| `filter.<Trường>` | string | – | Bộ lọc dạng `$<toán tử>:<giá trị>`. Dùng `filter.Gender=$eq:Male` |

Ví dụ đầy đủ:

```
GET /api/PartyMembers?current=1&pageSize=20&searchKeyword=an&searchFields=FullName&sortQuery=FullName%20asc&filter.Gender=$eq:Male
```

**Hình dạng phản hồi có phân trang** (`data`):

```json
{
  "pagedData": [ /* mảng bản ghi */ ],
  "pageInfo": {
    "totalCount": 1248,
    "pageSize": 20,
    "current": 1,
    "totalPages": 63,
    "hasNext": true,
    "hasPrevious": false
  }
}
```

**Cột sắp xếp được** (`sortQuery`): `FullName`, `DateOfBirth`, `Gender`, `OfficialAdmissionDate`.
**Cột KHÔNG sắp xếp được:** `partyAgeYears`, `nextMilestone`, `nextMilestoneDate` — là giá trị tính ra, không có trong cơ sở dữ liệu.
- Muốn sắp theo **Tuổi đảng tăng dần** → gửi `sortQuery=OfficialAdmissionDate desc` (vào Đảng muộn thì tuổi đảng nhỏ). Ngược lại cho giảm dần.
- Hai cột "Mốc kế tiếp" và "Ngày tròn mốc kế tiếp" **không có nút sắp xếp** trên giao diện v1.

### 1.8 Quy ước sắp xếp danh sách đủ điều kiện

Áp dụng cho mọi danh sách đủ điều kiện (Dashboard, chi tiết đợt, chưa thuộc đợt nào) và cho file Excel xuất ra:

1. Mốc huy hiệu tăng dần (30 → 35 → 40 …).
2. Trong cùng mốc: theo **tên gọi** (từ cuối cùng của Họ tên) theo bảng chữ cái tiếng Việt; trùng tên gọi thì so tiếp toàn bộ Họ tên. So sánh theo văn hóa `vi-VN` (Đ sau D, dấu thanh đúng thứ tự tiếng Việt).

Ví dụ đúng thứ tự: `Nguyễn Văn An` · `Trần Thị Bích` · `Lê Minh Châu` · `Phạm Thị Dung`.

Backend chịu trách nhiệm sắp xếp; Frontend hiển thị đúng thứ tự nhận được, **không sắp lại**.

> Riêng danh sách đảng viên (M2) mặc định sắp theo **toàn bộ Họ tên** A→Z (`Bùi Thị Lan` · `Cao Văn Phúc` · `Đỗ Thị Mai`), đúng như bản thiết kế. Hai quy tắc khác nhau là có chủ ý.

### 1.9 Cách trả file Excel

Ba endpoint xuất Excel và một endpoint tải file mẫu đều trả **file nhị phân**, không bọc trong `SuccessResultWrapper`:

```
HTTP/1.1 200 OK
Content-Type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
Content-Disposition: attachment; filename="DuDieuKien_Dot7-11_2026.xlsx"; filename*=UTF-8''DuDieuKien_Dot7-11_2026.xlsx
```

**Frontend phải tải bằng XHR, không dùng thẻ `<a href>`** — vì cần gửi header `Authorization`. Cách làm: `axios.get(url, { responseType: 'blob' })`, đọc tên file từ `Content-Disposition`, rồi tạo `URL.createObjectURL` để lưu.

Khi lỗi, các endpoint này trả JSON theo mục 1.4 (Frontend phải đọc lại blob thành text khi `status !== 200`).

**Quy tắc đặt tên file** (Backend sinh, Frontend chỉ dùng lại):
- Đủ điều kiện: `DuDieuKien_<TênĐợtRútGọn>_<Năm>.xlsx`
- Chưa thuộc đợt nào: `ChuaThuocDot_<Năm>.xlsx`
- File mẫu import: `MauDanhSachDangVien.xlsx`

`<TênĐợtRútGọn>` = tên đợt bỏ dấu tiếng Việt, mọi ký tự không phải chữ/số thay bằng `-`, bỏ khoảng trắng: `Đợt 7/11` → `Dot7-11`.

### 1.10 Kiểu dữ liệu dùng chung

**`Gender`** — chuỗi, một trong: `"Male"`, `"Female"`, hoặc `null`.

| Giá trị API | Giao diện | Ô Excel |
|---|---|---|
| `"Male"` | Nam | Nam |
| `"Female"` | Nữ | Nữ |
| `null` | — | (rỗng) |

**`AwardPeriodStatus`** — trạng thái của đợt trong năm đang xét (QT11):

| Giá trị API | Giao diện | Điều kiện |
|---|---|---|
| `"Past"` | Đã qua | `Đến < hôm nay` |
| `"Ongoing"` | Đang diễn ra | `Từ ≤ hôm nay ≤ Đến` |
| `"Upcoming"` | Sắp tới · N ngày | `Từ > hôm nay` |

**`PartyMemberResponse`** — một đảng viên trong danh sách M2:

```json
{
  "id": "5a1f0b3c-7d2e-4a91-9c44-0e2b8f6d1a77",
  "fullName": "Cao Văn Phúc",
  "dateOfBirth": "1951-04-18",
  "gender": "Male",
  "officialAdmissionDate": "1976-09-12",
  "partyAgeYears": 50,
  "nextMilestone": 55,
  "nextMilestoneDate": "2031-09-12",
  "createdAt": "2026-09-19T08:30:00Z",
  "updatedAt": "2026-09-19T08:30:00Z"
}
```

| Trường | Kiểu | Ghi chú |
|---|---|---|
| `id` | guid | |
| `fullName` | string | |
| `dateOfBirth` | date \| null | |
| `gender` | Gender \| null | |
| `officialAdmissionDate` | date | |
| `partyAgeYears` | int | QT3 — tính đến hôm nay, tính lại mỗi lần gọi |
| `nextMilestone` | int \| null | QT3a — `null` khi đã vượt mốc lớn nhất |
| `nextMilestoneDate` | date \| null | `null` cùng lúc với `nextMilestone` |
| `createdAt` / `updatedAt` | datetime | Không hiển thị trên giao diện v1 |

**`EligibleMemberResponse`** — một dòng trong danh sách đủ điều kiện:

```json
{
  "partyMemberId": "5a1f0b3c-7d2e-4a91-9c44-0e2b8f6d1a77",
  "fullName": "Nguyễn Văn An",
  "gender": "Male",
  "dateOfBirth": "1958-03-12",
  "officialAdmissionDate": "1996-10-15",
  "milestoneDate": "2026-10-15",
  "milestone": 30
}
```

> **Không có trường `id`.** Danh sách đủ điều kiện không được lưu vào bảng nào (QT5); mỗi lời gọi là một lần tính lại. `partyMemberId` chỉ để Frontend làm `rowKey` và để bấm sang màn sửa đảng viên. Không được dùng nó như id của "bản ghi đủ điều kiện" — không tồn tại thứ đó.

**`UnassignedMemberResponse`** — như trên, thêm thông tin khoảng trống:

```json
{
  "partyMemberId": "…",
  "fullName": "Trịnh Thị Oanh",
  "gender": "Female",
  "dateOfBirth": "1962-08-08",
  "officialAdmissionDate": "1986-06-05",
  "milestoneDate": "2026-06-05",
  "milestone": 40,
  "gap": {
    "type": "Between",
    "previousPeriodName": "Đợt 19/5",
    "nextPeriodName": "Đợt 2/9"
  }
}
```

`gap.type` ∈ `"Between"` | `"BeforeFirst"` | `"AfterLast"`. Frontend dựng chữ cho cột "Khoảng trống" theo đúng khuôn:

| `type` | Chữ hiển thị |
|---|---|
| `Between` | `Giữa {previousPeriodName} và {nextPeriodName}` |
| `BeforeFirst` | `Trước đợt đầu tiên` |
| `AfterLast` | `Sau đợt cuối cùng` |

Khi chưa cài đợt nào, mọi người tròn mốc trong năm đều có `type = "BeforeFirst"` với hai tên đợt `null`.

**`AwardPeriodResponse`** — một đợt, đã gắn năm đang xét:

```json
{
  "id": "9b7c…",
  "name": "Đợt 7/11",
  "fromDay": 1, "fromMonth": 10,
  "toDay": 7,  "toMonth": 11,
  "fromDisplay": "01/10",
  "toDisplay": "07/11",
  "year": 2026,
  "fromDate": "2026-10-01",
  "toDate": "2026-11-07",
  "status": "Upcoming",
  "daysRemaining": 14,
  "eligibleCount": 12
}
```

| Trường | Ghi chú |
|---|---|
| `fromDay`…`toMonth` | Dữ liệu lưu thật — **không có năm** (QT6) |
| `fromDisplay` / `toDisplay` | Tiện cho bảng, dạng `dd/MM` |
| `year` | Năm đang xét, lấy từ tham số truy vấn |
| `fromDate` / `toDate` | Đã gắn `year`; 29/02 ở năm không nhuận → `28/02` |
| `status` | QT11, so với hôm nay theo lịch máy chủ |
| `daysRemaining` | Chỉ khác `null` khi `status = "Upcoming"` |
| `eligibleCount` | Số người đủ điều kiện của đợt trong `year` (QT4) |

**`CoverageWarnings`** — cảnh báo chồng lấn và chưa phủ kín (QT6):

```json
{
  "overlaps": [
    {
      "firstPeriodId": "…", "firstPeriodName": "Đợt 19/5",
      "secondPeriodId": "…", "secondPeriodName": "Đợt 2/9",
      "fromDate": "2026-05-19", "toDate": "2026-05-25",
      "fromDisplay": "19/05", "toDisplay": "25/05"
    }
  ],
  "gaps": [
    {
      "fromDate": "2026-02-04", "toDate": "2026-02-28",
      "fromDisplay": "04/02", "toDisplay": "28/02",
      "previousPeriodName": "Đợt 3/2", "nextPeriodName": "Đợt 19/5"
    }
  ]
}
```

> **Cảnh báo không bao giờ chặn lưu.** Tạo hay sửa đợt chồng lấn / để hở khoảng trống vẫn trả `200`; cảnh báo đi kèm trong `data` để giao diện hiện banner. Chỉ ba thứ chặn lưu: thiếu tên, trùng tên, `Từ > Đến`.

---

## 2. Nhóm 1 — Đăng nhập (M0)

### 2.1 `POST /api/Auth/Login` — Đăng nhập (UC-00)

Không cần token.

**Thân yêu cầu**

```json
{ "username": "admin", "password": "……" }
```

| Trường | Kiểu | Bắt buộc |
|---|---|---|
| `username` | string | ✔ |
| `password` | string | ✔ |

**Phản hồi `data`**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9…",
  "tokenType": "Bearer",
  "expiresAt": "2026-09-19T16:30:00Z",
  "username": "admin",
  "displayName": "Quản trị viên",
  "unitName": "Đảng ủy Phường X",
  "serverDate": "2026-09-19"
}
```

`unitName` và `serverDate` trả luôn ở đây để dựng header ngay sau khi đăng nhập, khỏi gọi thêm.

**Lỗi**

| Mã | Khóa | Khi nào |
|---|---|---|
| `400` | `Mes.User.Required.Username` / `Mes.User.Required.Password` | Bỏ trống |
| `401` | `Mes.User.Login.Failed` | Sai tài khoản **hoặc** sai mật khẩu — một thông điệp chung, không nói rõ sai cái nào |

### 2.2 `POST /api/Auth/Logout` — Đăng xuất (UC-01)

Cần token. Không có thân yêu cầu.

**Phản hồi:** `data = null`, `message = "Mes.User.Logout.Successfully"`.

Máy chủ không làm gì thêm (JWT không trạng thái). Frontend xóa token khỏi `localStorage` và chuyển về màn đăng nhập, kể cả khi lời gọi này lỗi.

### 2.3 `GET /api/Auth/Me` — Thông tin phiên

Cần token. Dùng khi tải lại trang: nếu trả `200` thì token còn hiệu lực, vào thẳng Dashboard; nếu `401` thì về màn đăng nhập.

**Phản hồi `data`**

```json
{
  "username": "admin",
  "displayName": "Quản trị viên",
  "unitName": "Đảng ủy Phường X",
  "serverDate": "2026-09-19",
  "expiresAt": "2026-09-19T16:30:00Z"
}
```

`unitName` có thể là `null` (chưa đặt) → header chỉ hiện tên hệ thống (UC-51).

---

## 3. Nhóm 2 — Đảng viên (M2)

### 3.1 `GET /api/PartyMembers` — Danh sách (UC-20)

Tham số truy vấn theo mục 1.7.

| Nhu cầu trên giao diện | Tham số |
|---|---|
| Tìm theo họ tên | `searchKeyword=an&searchFields=FullName` |
| Lọc giới tính Nam | `filter.Gender=$eq:Male` |
| Lọc giới tính Nữ | `filter.Gender=$eq:Female` |
| Lọc "Tất cả" | bỏ hẳn tham số `filter.Gender` |
| Sắp xếp | `sortQuery=FullName asc` |
| Phân trang | `current=1&pageSize=20` |

**Phản hồi `data`**

```json
{
  "pagedData": [ /* PartyMemberResponse[] */ ],
  "pageInfo": { "totalCount": 1248, "pageSize": 20, "current": 1, "totalPages": 63, "hasNext": true, "hasPrevious": false }
}
```

Dòng tóm tắt "1.248 người" lấy từ `pageInfo.totalCount`.

**Lỗi:** `400` khi `pageSize` hoặc `current` ≤ 0.

**Ví dụ**

```
GET /api/PartyMembers?current=1&pageSize=8&sortQuery=FullName%20asc
Authorization: Bearer …
```

```json
{
  "message": "Mes.PartyMember.Search.Successfully",
  "data": {
    "pagedData": [
      { "id": "…", "fullName": "Bùi Thị Lan", "dateOfBirth": "1972-09-02", "gender": "Female",
        "officialAdmissionDate": "1998-06-12", "partyAgeYears": 28, "nextMilestone": 30,
        "nextMilestoneDate": "2028-06-12", "createdAt": "…", "updatedAt": "…" }
    ],
    "pageInfo": { "totalCount": 1248, "pageSize": 8, "current": 1, "totalPages": 156, "hasNext": true, "hasPrevious": false }
  }
}
```

### 3.2 `GET /api/PartyMembers/{id}` — Lấy một đảng viên (UC-22)

**Phản hồi `data`:** một `PartyMemberResponse`.
**Lỗi:** `400` + `Mes.PartyMember.NotFound`.

### 3.3 `POST /api/PartyMembers` — Thêm thủ công (UC-21)

**Thân yêu cầu** (`application/json`)

```json
{
  "fullName": "Nguyễn Văn An",
  "dateOfBirth": "1958-03-12",
  "gender": "Male",
  "officialAdmissionDate": "1996-10-15"
}
```

| Trường | Kiểu | Bắt buộc | Ràng buộc |
|---|---|---|---|
| `fullName` | string | ✔ | Không rỗng, ≤ 200 ký tự |
| `dateOfBirth` | date \| null | – | Nếu có: phải trước `officialAdmissionDate` |
| `gender` | Gender \| null | – | `Male` / `Female` / `null` |
| `officialAdmissionDate` | date | ✔ | ≤ ngày hôm nay (theo lịch máy chủ) |

**Phản hồi `data`:** `PartyMemberResponse` vừa tạo.

**Lỗi**

| Khóa | Khi nào |
|---|---|
| `Mes.PartyMember.Required.FullName` | Thiếu họ tên |
| `Mes.PartyMember.Required.OfficialAdmissionDate` | Thiếu ngày chính thức |
| `Mes.PartyMember.Invalid.OfficialAdmissionDate` | Ngày chính thức ở tương lai |
| `Mes.PartyMember.Invalid.DateOfBirth` | Ngày sinh sau ngày chính thức |
| `Mes.PartyMember.Invalid.Gender` | Giới tính khác `Male`/`Female` |

> **Không kiểm tra trùng tên.** Thêm hai người cùng tên, cùng ngày là hợp lệ (QT9 nói rõ hệ thống không chống trùng).

### 3.4 `PUT /api/PartyMembers/{id}` — Sửa (UC-22)

Thân yêu cầu và ràng buộc giống `POST`. Gửi **đủ cả 4 trường**, kể cả trường muốn xóa (gửi `null`).

**Phản hồi `data`:** `PartyMemberResponse` sau khi sửa.
**Lỗi:** như `POST`, thêm `Mes.PartyMember.NotFound`.

### 3.5 `DELETE /api/PartyMembers/{id}` — Xóa một (UC-23)

**Phản hồi `data`:** `{ "id": "5a1f0b3c-…" }`
**Lỗi:** `Mes.PartyMember.NotFound`.

Xóa hẳn, không có thùng rác (QT10). Hộp xác nhận là việc của Frontend.

### 3.6 `POST /api/PartyMembers/DeleteMany` — Xóa nhiều (UC-23)

**Thân yêu cầu**

```json
{ "ids": ["5a1f…", "7c2b…", "9e3d…"] }
```

`ids` bắt buộc, ít nhất 1 phần tử.

**Phản hồi `data`**

```json
{ "ids": ["5a1f…", "7c2b…", "9e3d…"] }
```

Trả đúng danh sách id **đã xóa được**. Id không tồn tại bị bỏ qua lặng lẽ, không làm hỏng cả lời gọi — người dùng vừa chọn trên màn hình thì không có lý do báo lỗi. Frontend hiển thị "Đã xóa {ids.length} đảng viên".

Dùng `POST` thay vì `DELETE` có thân — theo quy ước sẵn có của bộ khung Backend.

---

## 4. Nhóm 3 — Import Excel (M2 → wizard 3 bước)

Ba endpoint tương ứng ba bước của UC-24.

### 4.1 `GET /api/PartyMembers/Import/Template` — Tải file mẫu (UC-25)

Trả file nhị phân theo mục 1.9, tên `MauDanhSachDangVien.xlsx`.

Nội dung file: dòng 1 là tiêu đề `Họ tên` · `Ngày sinh` · `Giới tính` · `Ngày vào Đảng chính thức`; dòng 2–3 là ví dụ với ngày dạng `dd/MM/yyyy`.

### 4.2 `POST /api/PartyMembers/Import/Preview` — Xem trước (UC-24 bước 2)

`Content-Type: multipart/form-data`, một trường `file`.

**Kiểm tra cấp file — chặn ngay, trả `400`** (QT9):

| Khóa | Khi nào |
|---|---|
| `Mes.Import.Invalid.Extension` | Không phải `.xlsx` |
| `Mes.Import.Invalid.FileSize` | > 10 MB |
| `Mes.Import.Invalid.Columns` | Không đúng 4 cột theo thứ tự quy định |
| `Mes.Import.Invalid.Empty` | Không có dòng dữ liệu nào sau dòng tiêu đề |

**Phản hồi `data`**

```json
{
  "fileName": "DangVien_2026.xlsx",
  "totalRows": 129,
  "validCount": 125,
  "errorCount": 4,
  "validRows": [
    { "rowNumber": 2, "fullName": "Nguyễn Văn An", "dateOfBirth": "1958-03-12",
      "gender": "Male", "officialAdmissionDate": "1996-10-15" }
  ],
  "errorRows": [
    { "rowNumber": 5, "fullName": "Nguyễn Thị Hạnh", "dateOfBirth": "04/08/1963",
      "gender": "Nữ", "officialAdmissionDate": "",
      "errorCode": "MissingOfficialAdmissionDate", "field": "OfficialAdmissionDate" }
  ]
}
```

| Trường | Ghi chú |
|---|---|
| `rowNumber` | **Số dòng trong file Excel**, dòng tiêu đề là 1 nên dữ liệu bắt đầu từ 2. Đây chính là số hiện ở cột "Dòng" trong bảng lỗi |
| `validRows[*]` | Đã chuẩn hóa: ngày `yyyy-MM-dd`, giới tính `Male`/`Female`/`null` |
| `errorRows[*]` | **Giữ nguyên chữ thô đọc từ ô Excel**, mọi trường là chuỗi (có thể rỗng) — vì chính chúng đang sai, không chuẩn hóa được |
| `errorCode` | Mã lý do, xem bảng dưới |
| `field` | Cột gây lỗi, để Frontend tô đỏ ô đó |

**Bảng mã lỗi cấp dòng**

| `errorCode` | `field` | Chữ hiển thị ở cột "Lý do" |
|---|---|---|
| `MissingFullName` | `FullName` | Thiếu họ tên |
| `MissingOfficialAdmissionDate` | `OfficialAdmissionDate` | Thiếu ngày vào Đảng chính thức |
| `InvalidDateFormat` | `DateOfBirth` hoặc `OfficialAdmissionDate` | Sai định dạng ngày (cần dd/MM/yyyy) |
| `FutureOfficialAdmissionDate` | `OfficialAdmissionDate` | Ngày chính thức ở tương lai |
| `InvalidGender` | `Gender` | Giới tính chỉ nhận Nam hoặc Nữ |
| `BirthDateAfterAdmissionDate` | `DateOfBirth` | Ngày sinh phải trước ngày vào Đảng chính thức |

Mỗi dòng lỗi chỉ trả **một** lý do — lý do đầu tiên gặp phải, theo đúng thứ tự trong bảng trên.

**Lời gọi này không ghi gì vào cơ sở dữ liệu.**

### 4.3 `POST /api/PartyMembers/Import/Commit` — Nạp (UC-24 bước 3)

`multipart/form-data`, một trường `file` — **gửi lại đúng file vừa xem trước**.

> **Vì sao gửi lại file?** Để máy chủ không phải giữ trạng thái giữa hai bước (không cần cache, không cần id phiên, không lo hết hạn). Trình duyệt vẫn giữ đối tượng `File` từ bước 1, nên Frontend chỉ việc gửi lại. Máy chủ đọc và kiểm tra lại từ đầu; cùng một file thì kết quả y hệt bước xem trước.

Máy chủ **nạp mọi dòng hợp lệ và bỏ qua dòng lỗi**, không kiểm tra trùng (QT9). Toàn bộ việc nạp nằm trong **một giao dịch**: lỗi kỹ thuật giữa chừng → không dòng nào được thêm.

**Phản hồi `data`**

```json
{ "importedCount": 125, "skippedCount": 4 }
```

Frontend hiển thị: "Đã thêm 125 người, bỏ qua 4 dòng lỗi".

**Lỗi:** giống bước xem trước (4 khóa cấp file), thêm `500` khi giao dịch hỏng.

---

## 5. Nhóm 4 — Đợt trao huy hiệu (M3)

### 5.1 `GET /api/AwardPeriods` — Danh sách + trạng thái + cảnh báo (UC-30, UC-36)

| Tham số | Kiểu | Mặc định |
|---|---|---|
| `year` | int, 1900–2200 | Năm hiện tại theo lịch máy chủ |

Một lời gọi trả đủ dữ liệu cho cả bảng, dải độ phủ và banner cảnh báo.

**Phản hồi `data`**

```json
{
  "year": 2026,
  "today": "2026-09-17",
  "totalCount": 4,
  "periods": [ /* AwardPeriodResponse[], sắp theo fromDate tăng dần */ ],
  "warnings": { "overlaps": [], "gaps": [ /* … */ ] },
  "coverage": {
    "segments": [
      { "type": "Period", "periodId": "…", "name": "Đợt 3/2", "fromDate": "2026-01-01", "toDate": "2026-02-03" },
      { "type": "Gap",    "periodId": null, "name": null,      "fromDate": "2026-02-04", "toDate": "2026-02-28" }
    ]
  }
}
```

`coverage.segments` phủ liên tục từ `01/01` đến `31/12` của `year`, sắp theo `fromDate`, dùng để vẽ dải 12 tháng của UC-36. Khi hai đợt chồng lấn, phần chồng lấn vẫn nằm trong đoạn `Period` của đợt đến trước; chi tiết chồng lấn đọc ở `warnings.overlaps`.

`eligibleCount` của từng đợt là số người đủ điều kiện **trong `year`** (QT4) — cột "Đủ điều kiện năm nay".

### 5.2 `GET /api/AwardPeriods/{id}` — Lấy một đợt (UC-34 tab Thông tin)

| Tham số | Kiểu | Mặc định |
|---|---|---|
| `year` | int | Năm hiện tại |

**Phản hồi `data`:** một `AwardPeriodResponse` (đã gắn `year`).
**Lỗi:** `Mes.AwardPeriod.NotFound`.

### 5.3 `POST /api/AwardPeriods` — Thêm đợt (UC-31)

**Thân yêu cầu**

```json
{ "name": "Đợt 7/11", "fromDay": 1, "fromMonth": 10, "toDay": 7, "toMonth": 11 }
```

| Trường | Kiểu | Bắt buộc | Ràng buộc |
|---|---|---|---|
| `name` | string | ✔ | Không rỗng, ≤ 100 ký tự, **không trùng** tên đợt đã có (không phân biệt hoa thường) |
| `fromDay` / `toDay` | int | ✔ | 1–31, phải là ngày có thật của tháng tương ứng (cho phép `29/02`) |
| `fromMonth` / `toMonth` | int | ✔ | 1–12 |

Ràng buộc chung: `(fromMonth, fromDay) ≤ (toMonth, toDay)` — đợt phải nằm trọn trong một năm, không vắt qua 31/12 → 01/01 (QT6).

**Phản hồi `data`**

```json
{
  "period": { /* AwardPeriodResponse, year = năm hiện tại */ },
  "warnings": { "overlaps": [], "gaps": [] }
}
```

Cảnh báo tính cho **năm hiện tại**, trả kèm để giao diện cập nhật banner ngay. **Có cảnh báo vẫn là `200` — đã lưu.**

**Lỗi**

| Khóa | Khi nào |
|---|---|
| `Mes.AwardPeriod.Required.Name` | Thiếu tên |
| `Mes.AwardPeriod.Repeated.Name` | Trùng tên đợt khác |
| `Mes.AwardPeriod.Invalid.FromDate` | Ngày/tháng không có thật, ví dụ `31/04` |
| `Mes.AwardPeriod.Invalid.ToDate` | Như trên |
| `Mes.AwardPeriod.Invalid.Range` | `Từ` sau `Đến` |

### 5.4 `PUT /api/AwardPeriods/{id}` — Sửa đợt (UC-32)

Thân yêu cầu, ràng buộc và phản hồi giống `POST`, thêm lỗi `Mes.AwardPeriod.NotFound`. Kiểm tra trùng tên bỏ qua chính đợt đang sửa.

Sửa có hiệu lực ngay cho **mọi năm**, kể cả năm hiện tại (QT6). Dòng nhắc trên form là việc của Frontend.

### 5.5 `DELETE /api/AwardPeriods/{id}` — Xóa đợt (UC-33)

**Phản hồi `data`**

```json
{
  "id": "9b7c…",
  "warnings": { "overlaps": [], "gaps": [ /* khoảng trống mới sinh ra sau khi xóa */ ] }
}
```

**Lỗi:** `Mes.AwardPeriod.NotFound`.

Xóa đợt **không** xóa đảng viên nào — danh sách đủ điều kiện vốn không được lưu (QT5). Người từng thuộc đợt vừa xóa sẽ xuất hiện ở màn "Chưa thuộc đợt nào".

---

## 6. Nhóm 5 — Tính toán (Dashboard, đủ điều kiện, chưa thuộc đợt nào)

Toàn bộ nhóm này **tính lại mỗi lời gọi** (QT5). Không endpoint nào ở đây ghi dữ liệu; mọi lời gọi đều là `GET` và có thể gọi lại tùy ý.

### 6.1 `GET /api/Dashboard` — Toàn bộ dữ liệu Dashboard (UC-10, UC-11, UC-12, UC-13)

Không tham số. Máy chủ tự lấy ngày hôm nay và năm hiện tại.

**Phản hồi `data`**

```json
{
  "today": "2026-09-17",
  "currentYear": 2026,
  "unitName": "Đảng ủy Phường X",
  "memberCount": 1248,
  "periodCount": 4,
  "upcomingPeriod": {
    "id": "…", "name": "Đợt 7/11",
    "year": 2026, "isNextYear": false,
    "fromDate": "2026-10-01", "toDate": "2026-11-07",
    "fromDisplay": "01/10", "toDisplay": "07/11",
    "status": "Upcoming", "daysRemaining": 14,
    "eligibleCount": 12,
    "milestoneBreakdown": [
      { "milestone": 30, "count": 3 },
      { "milestone": 40, "count": 1 }
    ]
  },
  "eligibleMembers": [ /* EligibleMemberResponse[] của đợt sắp tới */ ],
  "warnings": {
    "noMembers": false,
    "noPeriods": false,
    "unassignedYear": 2026,
    "unassignedCount": 3,
    "overlaps": [],
    "gaps": [ /* … */ ]
  }
}
```

| Trường | Ghi chú |
|---|---|
| `upcomingPeriod` | QT8. `null` khi chưa có đợt nào → giao diện hiện "Chưa cài đợt trao huy hiệu" |
| `isNextYear` | `true` khi mọi đợt của năm nay đã qua và đợt sắp tới thuộc **năm sau**. Lúc đó `year` = năm sau, `fromDate`/`toDate` đã gắn năm sau |
| `daysRemaining` | `null` khi `status = "Ongoing"` → giao diện hiện "Đang diễn ra" |
| `milestoneBreakdown` | Sắp theo `milestone` tăng dần; chỉ liệt kê mốc có người |
| `eligibleMembers` | Sắp theo mục 1.8. Rỗng khi `upcomingPeriod = null` |
| `warnings.noMembers` | `memberCount = 0` → giao diện hiện khối hướng dẫn 3 bước (UC-13) |
| `warnings.unassignedCount` | Số người "chưa thuộc đợt nào" trong `unassignedYear` (QT7) |

Một lời gọi này đủ dựng cả màn Dashboard, kể cả trạng thái trống.

### 6.2 `GET /api/Eligibility` — Đủ điều kiện theo đợt và năm (UC-34)

| Tham số | Kiểu | Bắt buộc | Ghi chú |
|---|---|---|---|
| `awardPeriodId` | guid | ✔ | |
| `year` | int, 1900–2200 | – | Mặc định năm hiện tại |

**Phản hồi `data`**

```json
{
  "awardPeriod": { /* AwardPeriodResponse đã gắn year */ },
  "year": 2026,
  "totalCount": 7,
  "milestoneBreakdown": [ { "milestone": 30, "count": 4 }, { "milestone": 40, "count": 3 } ],
  "members": [ /* EligibleMemberResponse[] */ ]
}
```

Không phân trang (mục 1.7). Danh sách sắp theo mục 1.8.

**Lỗi:** `Mes.AwardPeriod.NotFound`, `Mes.Query.Invalid.Year`.

Bộ chọn năm segmented (năm trước · năm nay · năm sau) chỉ là ba lần gọi endpoint này với `year` khác nhau.

### 6.3 `GET /api/Eligibility/Unassigned` — Chưa thuộc đợt nào (UC-40)

| Tham số | Kiểu | Bắt buộc | Ghi chú |
|---|---|---|---|
| `year` | int, 1900–2200 | – | Mặc định năm hiện tại |

**Phản hồi `data`**

```json
{
  "year": 2026,
  "totalCount": 3,
  "members": [ /* UnassignedMemberResponse[] */ ]
}
```

Sắp theo `milestoneDate` tăng dần, rồi theo quy ước tên ở mục 1.8.
`totalCount = 0` → giao diện hiện "Không có ai bị sót trong năm 2026."

### 6.4 `GET /api/Eligibility/UnassignedCount` — Số người bị sót (badge menu trái)

| Tham số | Kiểu | Mặc định |
|---|---|---|
| `year` | int | Năm hiện tại |

**Phản hồi `data`**

```json
{ "year": 2026, "count": 3 }
```

Endpoint nhẹ, dành riêng cho badge trên menu "Chưa thuộc đợt nào" (Khung chung mục 4 của tài liệu nghiệp vụ). Frontend gọi khi vào ứng dụng và gọi lại sau mỗi thao tác đổi dữ liệu (thêm/sửa/xóa đảng viên, đổi đợt, đổi cài đặt). `count = 0` → ẩn badge.

---

## 7. Nhóm 6 — Cài đặt (M5)

### 7.1 `GET /api/Settings` — Đọc cài đặt (UC-50, UC-51)

**Phản hồi `data`**

```json
{
  "startYears": 30,
  "endYears": 90,
  "stepYears": 5,
  "unitName": "Đảng ủy Phường X",
  "milestones": [30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90],
  "milestoneCount": 13,
  "updatedAt": "2026-09-19T08:30:00Z"
}
```

`milestones` là dãy mốc đã sinh theo QT1 — nguồn sự thật duy nhất, Frontend không tự suy ra để hiển thị.

### 7.2 `PUT /api/Settings` — Lưu cài đặt (UC-50, UC-51)

**Thân yêu cầu**

```json
{ "startYears": 30, "endYears": 90, "stepYears": 5, "unitName": "Đảng ủy Phường X" }
```

| Trường | Kiểu | Bắt buộc | Ràng buộc |
|---|---|---|---|
| `startYears` | int | ✔ | ≥ 1 |
| `endYears` | int | ✔ | ≥ 1, ≥ `startYears` |
| `stepYears` | int | ✔ | ≥ 1 |
| `unitName` | string \| null | – | ≤ 200 ký tự; `null` hoặc rỗng = không đặt |

**Phản hồi `data`:** như `GET /api/Settings` sau khi lưu.

**Lỗi:** `Mes.AppSetting.Invalid.StartYears`, `.EndYears`, `.StepYears`, `.Range`.

Đổi cài đặt làm mọi danh sách đủ điều kiện thay đổi ngay (QT5) → Frontend nên tải lại Dashboard và badge sau khi lưu.

### 7.3 `POST /api/Settings/RestoreDefaults` — Khôi phục mặc định (UC-50)

Không có thân yêu cầu.

Đặt lại `startYears = 30`, `endYears = 90`, `stepYears = 5`. **Không đụng tới `unitName`** — nút trên giao diện ghi rõ "Khôi phục mặc định 30 / 90 / 5", chỉ nói về mốc.

**Phản hồi `data`:** như `GET /api/Settings`.

### 7.4 `GET /api/Settings/Milestones` — Xem trước dãy mốc (UC-50)

Dùng cho ô "Xem trước dãy mốc" cập nhật ngay khi người dùng gõ, **trước khi bấm Lưu**.

| Tham số | Kiểu | Mặc định |
|---|---|---|
| `start` | int | Giá trị đang lưu |
| `end` | int | Giá trị đang lưu |
| `step` | int | Giá trị đang lưu |

**Phản hồi `data`**

```json
{ "milestones": [30, 40, 50, 60, 70, 80, 90], "milestoneCount": 7 }
```

Không ghi gì vào cơ sở dữ liệu. Giữ QT1 ở một chỗ duy nhất (Backend), Frontend không cài lại công thức. Frontend nên hoãn 300 ms sau mỗi lần gõ rồi mới gọi.

**Lỗi:** như `PUT /api/Settings`.

---

## 8. Nhóm 7 — Xuất Excel

Cả ba đều trả file nhị phân theo mục 1.9. Nội dung file:

- **Dòng 1:** Tên đơn vị (bỏ dòng này nếu `unitName` trống).
- **Dòng 2:** Tên đợt + khoảng ngày đã gắn năm, ví dụ `Đợt 7/11 · 01/10/2026 – 07/11/2026`. Với file "chưa thuộc đợt nào": `Chưa thuộc đợt nào · năm 2026`.
- **Dòng 3:** `Ngày xuất: 19/09/2026`.
- **Dòng 4:** trống.
- **Dòng 5:** tiêu đề cột.
- **Từ dòng 6:** dữ liệu, đúng cột và đúng thứ tự như bảng đang xem.
- Ô trống **để rỗng**, không ghi `—`. Ngày ghi dạng `dd/MM/yyyy`. Giới tính ghi `Nam` / `Nữ`.

Cột của file đủ điều kiện: `STT` · `Họ tên` · `Giới tính` · `Ngày sinh` · `Ngày chính thức` · `Ngày tròn mốc` · `Mốc huy hiệu`.
File "chưa thuộc đợt nào" có thêm cột cuối: `Khoảng trống`.

### 8.1 `GET /api/Exports/Eligibility` — Xuất danh sách một đợt (UC-34)

| Tham số | Kiểu | Bắt buộc |
|---|---|---|
| `awardPeriodId` | guid | ✔ |
| `year` | int | – (mặc định năm hiện tại) |

Tên file: `DuDieuKien_<TênĐợtRútGọn>_<year>.xlsx`, ví dụ `DuDieuKien_Dot7-11_2026.xlsx`.
**Lỗi:** `Mes.AwardPeriod.NotFound`, `Mes.Query.Invalid.Year`.

### 8.2 `GET /api/Exports/Dashboard` — Xuất danh sách đợt sắp tới (UC-11)

Không tham số. Xuất đúng danh sách đang hiện trên Dashboard, tức đợt sắp tới theo QT8 (có thể thuộc năm sau).

Tên file giống mục 8.1, với năm là năm của đợt sắp tới.
**Lỗi:** `400` + `Mes.Dashboard.NotFound.UpcomingPeriod` khi chưa có đợt nào.

### 8.3 `GET /api/Exports/Unassigned` — Xuất danh sách chưa thuộc đợt nào (UC-40)

| Tham số | Kiểu | Bắt buộc |
|---|---|---|
| `year` | int | – (mặc định năm hiện tại) |

Tên file: `ChuaThuocDot_<year>.xlsx`.
**Lỗi:** `Mes.Query.Invalid.Year`.

Khi không có ai bị sót, vẫn trả file hợp lệ chỉ có phần tiêu đề — không trả lỗi. Giao diện đã chặn trước bằng trạng thái trống.

---

## 9. Bảng đối chiếu use case ↔ endpoint

| UC | Tên | Endpoint |
|---|---|---|
| UC-00 | Đăng nhập | `POST /api/Auth/Login` |
| UC-01 | Đăng xuất | `POST /api/Auth/Logout` |
| UC-10 | Thẻ đợt sắp tới | `GET /api/Dashboard` → `upcomingPeriod` |
| UC-11 | Bảng đủ điều kiện của đợt sắp tới | `GET /api/Dashboard` → `eligibleMembers`; xuất: `GET /api/Exports/Dashboard` |
| UC-12 | Cảnh báo nhanh | `GET /api/Dashboard` → `warnings` |
| UC-13 | Hướng dẫn 3 bước (trạng thái trống) | `GET /api/Dashboard` → `warnings.noMembers`, `warnings.noPeriods` |
| UC-20 | Danh sách đảng viên | `GET /api/PartyMembers` |
| UC-21 | Thêm thủ công | `POST /api/PartyMembers` |
| UC-22 | Sửa | `GET /api/PartyMembers/{id}` + `PUT /api/PartyMembers/{id}` |
| UC-23 | Xóa | `DELETE /api/PartyMembers/{id}` · `POST /api/PartyMembers/DeleteMany` |
| UC-24 | Import Excel | `POST /api/PartyMembers/Import/Preview` + `POST /api/PartyMembers/Import/Commit` |
| UC-25 | Tải file mẫu | `GET /api/PartyMembers/Import/Template` |
| UC-30 | Danh sách đợt | `GET /api/AwardPeriods` |
| UC-31 | Thêm đợt | `POST /api/AwardPeriods` |
| UC-32 | Sửa đợt | `PUT /api/AwardPeriods/{id}` |
| UC-33 | Xóa đợt | `DELETE /api/AwardPeriods/{id}` |
| UC-34 | Chi tiết đợt + đủ điều kiện | `GET /api/AwardPeriods/{id}` + `GET /api/Eligibility`; xuất: `GET /api/Exports/Eligibility` |
| UC-36 | Dải độ phủ trong năm | `GET /api/AwardPeriods` → `coverage.segments` |
| UC-40 | Chưa thuộc đợt nào | `GET /api/Eligibility/Unassigned`; xuất: `GET /api/Exports/Unassigned` |
| UC-50 | Cài mốc tuổi đảng | `GET /api/Settings` · `PUT /api/Settings` · `POST /api/Settings/RestoreDefaults` · `GET /api/Settings/Milestones` |
| UC-51 | Tên đơn vị | `PUT /api/Settings` (trường `unitName`); đọc ở `GET /api/Auth/Me` và `GET /api/Dashboard` |
| Khung chung | Badge "Chưa thuộc đợt nào" | `GET /api/Eligibility/UnassignedCount` |
| Khung chung | Header: tên đơn vị, ngày hôm nay | `GET /api/Auth/Me` |

Tổng: **28 endpoint**. Mọi use case trong tài liệu nghiệp vụ v1.1 đều có endpoint tương ứng. Không còn chỗ nào "sẽ bổ sung sau".

---

## 10. Quy tắc nghiệp vụ thể hiện trong hợp đồng

| Quy tắc | Thể hiện ở đâu |
|---|---|
| **QT1** dãy mốc sinh từ cài đặt | `GET /api/Settings` → `milestones`; `GET /api/Settings/Milestones` để xem trước. Không endpoint nào nhận dãy mốc viết cứng |
| **QT2** ngày tròn mốc, 29/02 | `milestoneDate` trong `EligibleMemberResponse`; `fromDate`/`toDate` của đợt khi gắn năm không nhuận |
| **QT3 / QT3a** tuổi đảng, mốc kế tiếp | `partyAgeYears`, `nextMilestone`, `nextMilestoneDate` — chỉ để hiển thị, không lọc/sắp xếp được |
| **QT4** đủ điều kiện theo đợt và năm | `GET /api/Eligibility?awardPeriodId=&year=` |
| **QT5** không lưu kết quả | Không có endpoint nào tạo/sửa/xóa "bản ghi đủ điều kiện". Các dòng đủ điều kiện **không có `id`**, chỉ có `partyMemberId`. Mọi endpoint nhóm 5 đều là `GET` |
| **QT6** đợt chỉ có ngày/tháng | Thân yêu cầu chỉ có `fromDay/fromMonth/toDay/toMonth`. Năm luôn là **tham số truy vấn**, không bao giờ là dữ liệu lưu |
| **QT7** chưa thuộc đợt nào | `GET /api/Eligibility/Unassigned?year=` |
| **QT8** đợt sắp tới | `GET /api/Dashboard` → `upcomingPeriod`, có cờ `isNextYear` |
| **QT9** import không chống trùng | Hai endpoint riêng cho xem trước và nạp; nạp là một giao dịch; không có tham số nào bật chống trùng |
| **QT10** xóa hẳn | `DELETE` trả về id đã xóa, không có endpoint khôi phục |
| **QT11** trạng thái đợt trong năm | `status` + `daysRemaining` trong `AwardPeriodResponse` |
| Cảnh báo **không chặn lưu** | `POST`/`PUT`/`DELETE` đợt vẫn trả `200` kèm `warnings`; cảnh báo không bao giờ xuất hiện dưới dạng lỗi `400` |

---

## 11. Quyết định đã chốt và điều cố ý không làm

**Đã chốt**

1. **Không có refresh token.** Phiên 8 giờ; hết hạn thì đăng nhập lại.
2. **Không tìm thấy trả `400`**, không phải `404` — theo quy ước của bộ khung Backend.
3. **Thêm mới trả `200`**, không phải `201` — cũng theo bộ khung.
4. **`message` là khóa, không phải chữ tiếng Việt.** Frontend giữ bảng tra ở mục 1.5.
5. **Ngày nghiệp vụ dùng `DateOnly`** và chuỗi `yyyy-MM-dd`, tuyệt đối không dùng `DateTime` (tránh lệch múi giờ).
6. **Bước nạp import gửi lại file**, không dùng id phiên — máy chủ không giữ trạng thái giữa hai bước.
7. **Chỉ danh sách đảng viên có phân trang.** Các danh sách khác trả đủ.
8. **Giới tính trên API là `Male`/`Female`**, chữ "Nam"/"Nữ" chỉ nằm ở giao diện và file Excel.
9. **Danh sách đủ điều kiện sắp theo tên gọi** (từ cuối của họ tên), khác với danh sách đảng viên sắp theo cả họ tên — theo đúng dữ liệu trong bản thiết kế. *Cần CEO xác nhận lại điểm này; nếu chọn sắp theo cả họ tên cho thống nhất, chỉ cần sửa mục 1.8, không ảnh hưởng hình dạng API.*

**Cố ý không có trong v1**

- Endpoint quản lý người dùng, vai trò, phân quyền.
- Endpoint đánh dấu đã trao, chốt đợt, lịch sử trao.
- Tham số chống trùng khi import.
- Lọc/sắp xếp theo tuổi đảng, mốc kế tiếp ở tầng cơ sở dữ liệu.
- Lọc danh sách đảng viên theo khoảng ngày vào Đảng.
- Xuất Excel danh sách toàn bộ đảng viên (M2) — thiết kế không có nút này.

---

## 12. Quy trình thay đổi hợp đồng

1. Ai thấy cần đổi (Backend, Frontend, QC) thì **ghi vào issue của mình** và gắn Technical Writer, không tự sửa tài liệu.
2. Technical Writer sửa `docs/api-contract.md` và `docs/openapi.yaml`, tăng số phiên bản ở mục 13, rồi báo cả Backend và Frontend.
3. Thay đổi làm hỏng mã đang chạy (đổi tên trường, bỏ endpoint) phải có xác nhận của CEO trước khi sửa tài liệu.
4. **Mã nguồn không bao giờ được khác tài liệu.** Nếu Backend đã làm khác, hoặc sửa mã cho khớp, hoặc báo để sửa tài liệu — không để tồn tại hai sự thật.

---

## 13. Nhật ký phiên bản

| Phiên bản | Ngày | Thay đổi |
|---|---|---|
| 1.0 | 19/09/2026 | Bản đầu tiên. 28 endpoint, phủ toàn bộ UC-00 → UC-51 của tài liệu nghiệp vụ v1.1 |
