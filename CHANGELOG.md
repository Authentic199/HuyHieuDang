# Nhật ký thay đổi

## Tài liệu này dùng để làm gì

Ghi lại những thay đổi **người dùng hoặc thành viên trong đội nhìn thấy được**: màn hình mới, endpoint mới, quy tắc nghiệp vụ đổi, cách chạy đổi. Không ghi từng commit — muốn xem commit thì đọc lịch sử git.

Mỗi giai đoạn (stage) một mục, mới nhất ở trên. Trong mỗi mục chia theo: **Thêm**, **Đổi**, **Sửa lỗi**, **Bỏ**. Ngày theo `dd/MM/yyyy`.

Ai merge một thay đổi đáng ghi thì ghi luôn một dòng vào mục **Chưa phát hành**. Technical Writer gom lại khi đóng giai đoạn.

---

## Chưa phát hành

### Đổi

- **QT6 — đợt trao huy hiệu được phép vắt qua 31/12.** Từ ngày đứng sau Đến ngày (ví dụ `01/12 – 28/02`) nghĩa là đợt kết thúc ở năm sau, không còn bị từ chối. Năm của một đợt luôn là năm chứa Từ ngày.
  - Bỏ khóa lỗi `Mes.AwardPeriod.Invalid.Range`. Chặn lưu chỉ còn: thiếu tên, trùng tên, ngày/tháng không có thật.
  - `AwardPeriodResponse` thêm trường `spansNextYear`; `toDate` có thể thuộc `year + 1`.
  - Độ phủ, khoảng trống và danh sách "Chưa thuộc đợt nào" của một năm tính trên **phần đợt nằm trong năm đó**, gồm cả đuôi của đợt vắt năm neo ở năm trước. `coverage.segments` vì vậy có thể có hai đoạn cùng `periodId`.
  - QT8 (đợt sắp tới) xét thêm lần diễn ra neo ở năm trước; QT11 báo "Đang diễn ra" khi lần neo năm trước còn đang mở hôm nay.
  - Giao diện: bỏ ràng buộc Đến ≥ Từ ở modal thêm/sửa, dòng nhắc nói rõ "vắt qua 31/12", bảng đợt ghi "năm sau" cạnh Đến ngày, tab Thông tin đọc "01/12 – 28/02 năm sau, hằng năm".
- Hợp đồng API lên **v1.4**: bảng khóa thông điệp mục 1.5 có thêm cột “Khi nào Backend trả” và bảy khóa Backend đang trả mà tài liệu chưa ghi; ba mốc của Cài đặt có trần 200 năm.
  - Thêm vào bảng 1.5: `Mes.User.Required.Username`, `Mes.User.Required.Password`, `Mes.PartyMember.OverLength.FullName`, `Mes.PartyMember.Required.Ids`, `Mes.AwardPeriod.OverLength.Name`, `Mes.AppSetting.OverLength.UnitName`, `Mes.Common.Invalid.Parameter`. Frontend thiếu khóa nào thì chỉ hiện được câu mặc định (QC-T27-07).
  - **Mốc bắt đầu / Mốc kết thúc / Bước nhảy**: số nguyên **từ 1 đến 200** thay cho “từ 1 trở lên”, áp dụng cho cả `PUT /api/Settings` lẫn ô xem trước `GET /api/Settings/Milestones`. Vượt trần dùng đúng ba khóa sẵn có, không thêm khóa mới (QC-T27-06).
  - Tham số sai kiểu (`?year=abc`) trả khóa `Mes.Common.Invalid.Parameter` thay cho câu tiếng Anh của khung ASP.NET (QC-T27-04).
  - Chữ tiếng Việt của ba khóa `Mes.AppSetting.Invalid.StartYears` / `.EndYears` / `.StepYears` đổi theo trần mới — Frontend cập nhật bảng tra cho khớp.
- Hợp đồng API lên **v1.1**: chốt 10 điểm tài liệu nghiệp vụ chưa quy định (OQ-1…OQ-10) vào mục 12, ghi 8 quyết định kỹ thuật của bộ khung Backend vào mục 13. Chi tiết trong `docs/api-contract.md` mục 15.
  - Phản hồi xem trước import đổi hình dạng: `errorRows[*].errorCode` + `field` gộp thành mảng `errorRows[*].errors[]` — một dòng lỗi trả đủ mọi lý do thay vì chỉ lý do đầu tiên.
  - Thêm khóa `Mes.Import.Invalid.NoDataRows` cho file chỉ có dòng tiêu đề; `Mes.Import.Invalid.Empty` giờ chỉ dùng cho file rỗng.
  - Tìm theo tên: không phân biệt hoa thường, **có** phân biệt dấu. Sắp xếp Họ tên dùng đối chiếu tiếng Việt ICU `vi`.
  - Sửa cổng Backend khi chạy dev trong tài liệu: 5000 → **8080**, cho khớp `launchSettings.json` và `docker-compose.yml`.

### Thêm

- **Nghiệm thu QC cho đợt vắt qua 31/12 (T54).** Luồng end-to-end mới `e7-dot-vat-qua-nam` chạy trên trình duyệt thật, 8 ca đơn vị `U-620 → U-626` và 4 ca API `A-221 → A-224`. Oracle của QC viết lại theo lối đi từng ngày để không còn là bản chép của service. Báo cáo: `docs/test-report/2026-09-20-qc-dot-vat-qua-nam.md`.

- `README.md`: cách chạy bằng `docker compose`, cách chạy chế độ phát triển, cách sao lưu và khôi phục bằng `pg_dump` / `psql`.
- `CHANGELOG.md` (tài liệu này).

### Sửa lỗi

- Hợp đồng API lên **v1.2** cho khớp mã đã duyệt. Không đổi hình dạng request/response.
  - **Mục 4 Import:** sửa thứ tự lý do trong `errors[]`, tệp đổi đuôi cũng ra `Mes.Import.Invalid.Extension`, ghi khóa thông điệp thành công của bước xem trước và bước nạp.
  - **Mục 5.2:** `year` chỉ nhận 1900–2200, ngoài khoảng trả `Mes.Query.Invalid.Year`.
  - **Mục 1.8 và 6.3:** danh sách đủ điều kiện sắp theo mốc tuổi đảng rồi **Họ tên đầy đủ**, bỏ quy ước sắp theo tên gọi. Đồng bộ mục 8, mục 11 điểm 9 và OQ-3.
  - **Mục 7 Cài đặt:** tên đơn vị rỗng hoặc toàn khoảng trắng đều lưu thành chưa đặt và bị cắt khoảng trắng đầu/cuối; thêm khóa `Mes.AppSetting.OverLength.UnitName`; ba mốc không phải số nguyên bị từ chối 400 không kèm khóa `Mes.*`; kho cài đặt trống trả mặc định 30 / 90 / 5 mà không tự tạo bản ghi.
  - **Mục 8 Xuất Excel:** mỗi file chỉ có một sheet tên `DanhSach`; ba dòng tiêu đề ghi ở ô đầu tiên của dòng, không gộp ô.

---

## Giai đoạn 0 — Nền móng · 19/09/2026

Dựng nền để bốn agent làm song song. Chưa có nghiệp vụ nào chạy được: không có entity đảng viên, không có đợt trao huy hiệu, không có màn hình thật.

### Thêm

- **Bộ khung Backend** (`BE/`) — ASP.NET Core 8 dựng từ boilerplate, đổi tên `HuyHieuDang.*`, giữ `Core` / `Infrastructure` / `Migrators` / `Web` / `tests`. Đã lược bỏ các facade và module không dùng, bỏ `Migrators.MySql`. Migration `InitialSchema` cho lược đồ định danh, seeder tạo tài khoản `admin` / `Admin@123`. API nghe cổng 8080, tài liệu ở `/docs`.
- **Bộ khung Frontend** (`FE/`) — React + Vite + TypeScript + Ant Design 5. Theme lấy từ bộ thiết kế, layout chung (header + sider 5 mục), định tuyến 5 màn có chặn khi chưa đăng nhập, màn Đăng nhập, tầng gọi API có kiểu. Từng màn còn là khối giữ chỗ.
- **`docker-compose.yml`** ba service `postgres` + `be` + `fe`, kèm `.env.example`. Service `fe` tạm dùng nginx phục vụ trang giữ chỗ.
- **Hợp đồng API v1.0** (`docs/api-contract.md`, `docs/openapi.yaml`) — 28 endpoint chia 7 nhóm, phủ toàn bộ UC-00 → UC-51.
- **Kế hoạch kiểm thử** (`docs/test-plan.md`) và **bộ dữ liệu biên** (`tests/fixtures/`) — 120 ca logic, 110 ca tích hợp API, 6 luồng E2E, fixture Excel sinh lại được từng byte.

### Cố ý chưa làm

Quản lý người dùng và phân quyền, đánh dấu đã trao, chốt đợt, trừ tuổi đảng gián đoạn, trao sớm, truy tặng, in tờ trình — tất cả nằm ngoài phạm vi v1.
