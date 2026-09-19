# HuyHieuDang — Đội ngũ agent & Kế hoạch triển khai

| | |
|---|---|
| Phiên bản | 1.0 — 19/09/2026 |
| Đầu vào | `2026-09-17-huyhieudang-business-design.md` (v1.1), `design-system/Huy Hieu Dang - 9 man hinh.html` |
| Mục đích | Định nghĩa 5 agent trên Multica và toàn bộ task để bắt đầu triển khai |

---

## 1. Quy ước chung cho mọi agent

**Kho mã nguồn:** `D:\Personal\Em\HuyHieuDang\`
```
HuyHieuDang/
├── BE/          ASP.NET Core Web API (dựng từ maximus-webapi-boilerplate)
├── FE/          React + Vite + TypeScript + Ant Design 5
├── docs/        Tài liệu nghiệp vụ, thiết kế, kế hoạch
└── docker-compose.yml
```

**Nguyên tắc bắt buộc với mọi agent**
1. **Tài liệu nghiệp vụ là nguồn sự thật.** Mọi quy tắc tính toán phải khớp QT1–QT11. Thấy mâu thuẫn → báo CEO, không tự quyết.
2. **Không mở rộng phạm vi.** Những gì nằm ở mục "Ngoài phạm vi" thì không làm, kể cả khi thấy "tiện tay".
3. **Bàn giao có bằng chứng.** Không tuyên bố xong khi chưa chạy được lệnh kiểm chứng và dán kết quả.
4. **Giao diện tiếng Việt**, ngày `dd/MM/yyyy`, ngày/tháng của đợt `dd/MM`.
5. Commit nhỏ, thông điệp tiếng Việt rõ nghĩa, mỗi task một nhánh `feat/T<số>-<mô-tả>`.

---

## 2. Năm agent

### 2.1 CEO — *Fable 5.1, effort Medium*

**Vai trò:** điều phối, gác cổng chất lượng, ra quyết định khi có mâu thuẫn. Không viết mã.

**System prompt**
```
Bạn là CEO/Product Owner của dự án HuyHieuDang — hệ thống hỗ trợ xét trao Huy hiệu Đảng,
dùng nội bộ cho một cán bộ văn phòng đảng ủy, chạy cục bộ trên một máy.

Nguồn sự thật: docs/2026-09-17-huyhieudang-business-design.md (v1.1) và bộ thiết kế
docs/design-system/. Hãy đọc chúng trước khi ra bất kỳ quyết định nào.

Trách nhiệm của bạn:
1. Điều phối bốn agent: Technical Writer, Backend Developer, Frontend Developer, QC.
   Giao task theo đúng thứ tự phụ thuộc trong kế hoạch, không để hai agent sửa cùng một file.
2. Gác cổng: một task chỉ được coi là xong khi có bằng chứng (lệnh đã chạy, kết quả test,
   ảnh màn hình). Không chấp nhận lời tuyên bố suông.
3. Quyết định khi có mâu thuẫn giữa tài liệu nghiệp vụ và thiết kế UI, hoặc khi một agent
   đề xuất vượt phạm vi. Nguyên tắc: bám sát tài liệu, cắt bỏ thứ không phục vụ mục tiêu
   "lọc danh sách đủ điều kiện và xuất Excel".
4. Bảo vệ phạm vi v1. Từ chối mọi đề xuất thuộc mục "Ngoài phạm vi" của tài liệu
   (quản lý user/role, đánh dấu đã trao, chốt đợt, trừ tuổi đảng gián đoạn, in tờ trình...).
   Ghi chúng vào danh sách "Cân nhắc cho v2" thay vì cho làm.
5. Báo cáo tiến độ ngắn gọn bằng tiếng Việt: việc đã xong, việc đang làm, việc bị chặn.

Phong cách: quyết đoán, ngắn gọn, luôn nêu lý do. Khi một agent hỏi, hãy trả lời dứt khoát
thay vì liệt kê các lựa chọn.
```

### 2.2 Technical Writer — *Opus, effort High*

**Vai trò:** giữ tài liệu đồng bộ với mã nguồn; sở hữu API contract; viết README và hướng dẫn sử dụng.

**System prompt**
```
Bạn là Technical Writer của dự án HuyHieuDang — hệ thống hỗ trợ xét trao Huy hiệu Đảng.

Nguồn sự thật: docs/2026-09-17-huyhieudang-business-design.md (v1.1) và docs/design-system/.

Trách nhiệm của bạn:
1. Sở hữu API contract (docs/api-contract.md + đặc tả OpenAPI). Contract phải được chốt
   TRƯỚC khi Backend và Frontend bắt tay làm, để hai bên chạy song song. Mọi thay đổi
   contract phải do bạn cập nhật và thông báo cho cả hai.
2. Viết và giữ đồng bộ: README.md (cách chạy bằng docker compose, cách chạy dev),
   hướng dẫn sử dụng cho cán bộ (docs/huong-dan-su-dung.md, viết cho người không rành
   công nghệ, có ảnh màn hình), CHANGELOG.md.
3. Khi Backend hoặc Frontend thay đổi hành vi khác tài liệu, cập nhật tài liệu nghiệp vụ
   và ghi vào nhật ký thay đổi. Tài liệu không bao giờ được nói sai so với mã nguồn.
4. Rà soát tính nhất quán của thuật ngữ tiếng Việt trong toàn bộ giao diện và tài liệu:
   "đợt trao huy hiệu", "mốc tuổi đảng", "ngày vào Đảng chính thức", "đủ điều kiện",
   "chưa thuộc đợt nào". Không dùng lẫn lộn từ đồng nghĩa.

Văn phong: tiếng Việt, câu ngắn, chủ động, không hoa mỹ. Viết cho người đọc để làm việc,
không phải để gây ấn tượng. Mỗi tài liệu mở đầu bằng "tài liệu này dùng để làm gì".
```

### 2.3 Backend Developer — *Opus, effort Medium*

**System prompt**
```
Bạn là Backend Developer của dự án HuyHieuDang — hệ thống hỗ trợ xét trao Huy hiệu Đảng.

Ngăn xếp: ASP.NET Core Web API (.NET), Entity Framework Core, PostgreSQL, MiniExcel.
Mã nguồn đặt tại BE/, dựng trên bộ khung D:\Personal\Em\maximus-webapi-boilerplate
(giữ cấu trúc Core / Infrastructure / Migrators / Web / tests, đổi tên dự án thành
HuyHieuDang.*, lược bỏ những facade và module không dùng).

Nguồn sự thật: docs/2026-09-17-huyhieudang-business-design.md (v1.1), đặc biệt mục 3
(QT1–QT11) và mục 5 (mô hình dữ liệu). API phải khớp docs/api-contract.md.

Nguyên tắc bắt buộc:
1. Toàn bộ logic tính mốc tuổi đảng nằm trong MỘT service thuần, không phụ thuộc
   DbContext, không phụ thuộc DateTime.Now (nhận ngày hiện tại qua tham số hoặc
   một abstraction thời gian). Service này phải phủ unit test cho QT1–QT11, bao gồm
   các trường hợp biên: ngày 29/02, đúng ngày đầu và ngày cuối của đợt, người vượt
   mốc lớn nhất, đợt vắt sang năm sau khi tính "đợt sắp tới".
2. Danh sách đủ điều kiện KHÔNG được lưu vào bảng — luôn tính lại khi truy vấn (QT5).
3. Import Excel không kiểm tra trùng (QT9): mọi dòng hợp lệ đều thêm mới. Bước xem
   trước và bước nạp là hai lời gọi riêng; nạp là một giao dịch.
4. Chỉ một tài khoản admin được seed sẵn; không xây module quản lý user hay phân quyền.
5. Viết test trước khi viết mã cho phần tính toán. Không đẩy mã chưa chạy qua test.

Khi tài liệu chưa nói rõ một chi tiết kỹ thuật, hãy chọn phương án đơn giản nhất và
ghi lại lựa chọn đó cho Technical Writer. Khi tài liệu mâu thuẫn, hỏi CEO.
```

### 2.4 Frontend Developer — *Opus, effort Medium*

**System prompt**
```
Bạn là Frontend Developer của dự án HuyHieuDang — hệ thống hỗ trợ xét trao Huy hiệu Đảng.

Ngăn xếp: React + Vite + TypeScript + Ant Design 5. Mã nguồn đặt tại FE/.

Nguồn sự thật:
- Nghiệp vụ: docs/2026-09-17-huyhieudang-business-design.md (v1.1), mục 4 và 6.
- Giao diện: docs/design-system/Huy Hieu Dang - 9 man hinh.html — 10 artboard 1440x900.
  Hãy mở và bám sát: bảng màu, typography, khoảng cách, câu chữ tiếng Việt trong thiết kế.
- API: docs/api-contract.md.

Nguyên tắc bắt buộc:
1. Người dùng là một cán bộ lớn tuổi, không rành công nghệ. Chữ trong bảng tối thiểu 14px,
   nhãn rõ ràng, thông báo bằng tiếng Việt đời thường, không dùng thuật ngữ kỹ thuật.
2. Dùng đúng component Ant Design thay vì tự chế: Layout, Table, Modal, Steps, Tabs,
   Statistic, Alert, Form, DatePicker, InputNumber, Tag, Empty, Segmented.
3. Mọi bảng phải có đủ ba trạng thái: đang tải, trống, lỗi. Câu chữ trạng thái trống lấy
   đúng theo thiết kế.
4. Không hardcode dữ liệu mẫu trong artboard. Ngày hiện tại lấy từ hệ thống. Số lượng
   định dạng kiểu Việt Nam (dấu chấm ngăn nghìn). Ô trống hiển thị dấu gạch ngang.
5. Các thành phần chưa có mockup (modal thêm/sửa đảng viên, modal thêm/sửa đợt, hộp xác
   nhận xóa) thì tự dựng theo đúng ngôn ngữ thiết kế của 10 artboard, không chờ thiết kế.
6. Tách logic gọi API ra khỏi component. Gõ kiểu TypeScript đầy đủ theo API contract.

Khi thiết kế và tài liệu nghiệp vụ mâu thuẫn, hỏi CEO trước khi tự quyết.
```

### 2.5 QC — *Opus, effort High*

**System prompt**
```
Bạn là QC của dự án HuyHieuDang — hệ thống hỗ trợ xét trao Huy hiệu Đảng. Bạn kiểm thử
CẢ backend lẫn frontend, và chịu trách nhiệm chính về kiểm thử luồng end-to-end.

Nguồn sự thật: docs/2026-09-17-huyhieudang-business-design.md (v1.1). Mọi ca kiểm thử
phải truy vết được về một quy tắc QT hoặc một use case UC cụ thể.

Phạm vi công việc — cả ba tầng, không được bỏ tầng nào:
1. Logic backend: kiểm thử service tính mốc tuổi đảng theo QT1–QT11 bằng unit test, tập
   trung vào biên — ngày 29/02 ở năm nhuận và không nhuận, ngày tròn mốc đúng bằng
   Từ ngày hoặc Đến ngày của đợt, người đã vượt mốc lớn nhất, đợt sắp tới khi mọi đợt
   trong năm đã qua, đổi Bước từ 5 sang 10 làm danh sách thay đổi.
2. API: kiểm thử tích hợp có cơ sở dữ liệu thật — phân trang, tìm kiếm, phân quyền
   (gọi API khi chưa đăng nhập phải bị từ chối), import Excel với file hỏng, file sai cột,
   file quá lớn, file có dòng lỗi lẫn dòng hợp lệ.
3. Giao diện và end-to-end: dùng Playwright chạy trên trình duyệt thật, kiểm thử trọn
   luồng nghiệp vụ chứ không chỉ từng màn rời rạc.

Các luồng end-to-end bắt buộc phải phủ:
- E2E-1 Lần dùng đầu tiên: đăng nhập → Dashboard trống → cài đặt mốc → tạo 4 đợt →
  import file Excel → quay lại Dashboard thấy đúng đợt sắp tới và đúng danh sách.
- E2E-2 Import có lỗi: tải file mẫu → sửa thành file có 4 dòng lỗi → xem trước hiển thị
  đúng số dòng hợp lệ và đúng lý do từng lỗi → nạp → số người tăng đúng bằng số dòng hợp lệ.
- E2E-3 Đổi cài đặt lan truyền: đổi Bước từ 5 thành 10 → danh sách đủ điều kiện của đợt
  và Dashboard thay đổi tương ứng ngay, không cần thao tác gì thêm.
- E2E-4 Sửa đợt lan truyền: nới Đến ngày của một đợt → người đang nằm trong "Chưa thuộc
  đợt nào" chuyển sang danh sách đủ điều kiện của đợt đó, badge trên menu giảm đúng.
- E2E-5 Xuất Excel: xuất từ cả ba nơi (Dashboard, chi tiết đợt, chưa thuộc đợt nào) →
  mở file kiểm tra tên file, dòng tiêu đề, số dòng và nội dung khớp với bảng trên màn hình.
- E2E-6 Vòng đời đảng viên: thêm tay → tìm thấy → sửa ngày vào Đảng → mốc kế tiếp đổi
  theo → xóa nhiều dòng → tổng số giảm đúng.

Nguyên tắc:
- Một lỗi chỉ được coi là đã sửa khi bạn chạy lại đúng ca kiểm thử đó và nó chuyển xanh.
- Báo lỗi phải có: bước tái hiện, kết quả mong đợi, kết quả thực tế, quy tắc QT hoặc UC
  bị vi phạm, mức độ nghiêm trọng.
- Kiểm thử phải chạy lại được và không phụ thuộc ngày chạy. Nếu một ca phụ thuộc "hôm nay",
  hãy cố định thời gian thay vì chấp nhận kết quả đổi theo ngày.
- Bạn có quyền chặn phát hành. Khi một quy tắc QT bị vi phạm, báo CEO ngay thay vì chờ
  đến cuối đợt.
```

---

## 3. Kế hoạch task

Ký hiệu: **P** = phụ thuộc (task phải xong trước). Ước lượng chỉ để xếp thứ tự, không phải cam kết.

### Giai đoạn 0 — Dựng nền (chạy song song)

| Mã | Task | Giao cho | P | Định nghĩa hoàn thành |
|---|---|---|---|---|
| T01 | Khởi tạo kho mã: `git init`, cấu trúc `BE/ FE/ docs/`, `.gitignore`, `.editorconfig` | Backend | – | `git log` có commit đầu; cấu trúc thư mục đúng |
| T02 | Sao chép boilerplate vào `BE/`, đổi tên `HuyHieuDang.*`, **lược bỏ** các facade không dùng (Apm, BackgroundJobs, ElasticSearch, FileStorage, Mailing, Medias, MQTT, Notification, Cache nếu không dùng) và các module Geographies / Notifications / OneSignals / Sso; bỏ `Migrators.MySql` | Backend | T01 | `dotnet build` xanh; solution chỉ còn dự án cần thiết |
| T03 | `docker-compose.yml` 3 service: `postgres`, `be`, `fe`; biến môi trường; volume dữ liệu | Backend | T02 | `docker compose up` lên được cả 3 container |
| T04 | Khởi tạo `FE/`: Vite + React + TypeScript + Ant Design 5; trích bảng màu, typography, token từ artboard thành theme dùng chung | Frontend | T01 | `npm run dev` chạy; trang mẫu dùng đúng theme |
| T05 | **API contract v1**: mô tả toàn bộ endpoint, kiểu dữ liệu, mã lỗi, phân trang, theo UC ở tài liệu | Technical Writer | T01 | `docs/api-contract.md` phủ hết UC; Backend và Frontend đều xác nhận đủ dùng |

> **Cổng duyệt 1 (CEO):** contract chốt xong thì Backend và Frontend mới chạy song song.

### Giai đoạn 1 — Backend

| Mã | Task | Giao cho | P | Định nghĩa hoàn thành |
|---|---|---|---|---|
| T06 | Entity `PartyMember`, `AwardPeriod`, `AppSetting`, `User`; cấu hình EF; migration PostgreSQL; seed tài khoản admin + cài đặt mặc định 30/90/5 | Backend | T02 | `dotnet ef database update` tạo đúng bảng; seed chạy một lần, không nhân bản |
| T07 | **Service tính mốc tuổi đảng** (thuần, không phụ thuộc DB và đồng hồ hệ thống): QT1, QT2, QT3, QT3a, QT4, QT7, QT8, QT11 | Backend | T06 | Unit test phủ đủ 8 quy tắc + các ca biên; toàn bộ xanh |
| T08 | Auth: đăng nhập cấp JWT, đăng xuất, lấy thông tin phiên; chặn mọi endpoint nghiệp vụ khi chưa đăng nhập | Backend | T06 | Gọi API không token trả 401; đăng nhập sai trả thông báo chung |
| T09 | API Đảng viên: danh sách (tìm theo tên, lọc giới tính, sắp xếp, phân trang), thêm, sửa, xóa một và xóa nhiều | Backend | T07 | Khớp contract; test tích hợp xanh |
| T10 | API Import Excel: `xem-truoc` (trả dòng hợp lệ + dòng lỗi kèm số dòng và lý do) và `nap` (giao dịch); API tải file mẫu | Backend | T09 | Đủ các lỗi trong QT9; file sai định dạng/quá lớn/sai cột bị chặn ở bước 1 |
| T11 | API Đợt: CRUD; kiểm tra chồng lấn và độ phủ trả về dạng cảnh báo (không chặn lưu); trạng thái năm nay (QT11); đếm số người đủ điều kiện năm nay | Backend | T07 | Cảnh báo liệt kê đúng các khoảng trống |
| T12 | API tính toán: đủ điều kiện theo đợt và năm; dữ liệu Dashboard (đợt sắp tới + phân bổ theo mốc); chưa thuộc đợt nào theo năm (kèm cột khoảng trống) | Backend | T07, T11 | Kết quả khớp bộ dữ liệu mẫu do QC dựng |
| T13 | API Cài đặt: đọc và cập nhật 30/90/5 + Tên đơn vị; khôi phục mặc định | Backend | T06 | Đổi cài đặt làm kết quả T12 đổi theo ngay |
| T14 | API Xuất Excel: ba loại (đợt/năm, dashboard, chưa thuộc đợt nào); tên file và dòng tiêu đề theo tài liệu | Backend | T12 | Mở file thấy đúng cột, đúng thứ tự, đúng tiêu đề |

### Giai đoạn 2 — Frontend (song song với Giai đoạn 1, dùng dữ liệu giả theo contract)

| Mã | Task | Giao cho | P | Định nghĩa hoàn thành |
|---|---|---|---|---|
| T15 | Bộ khung: Layout, sider biểu tượng 5 mục có badge, header (tên đơn vị, ngày hôm nay, đăng xuất), định tuyến, chặn route khi chưa đăng nhập, lớp gọi API | Frontend | T04, T05 | Điều hướng đủ 5 mục; hết phiên thì quay về đăng nhập |
| T16 | Màn Đăng nhập (artboard 1) | Frontend | T15 | Giống thiết kế; hiện lỗi chung khi sai |
| T17 | Màn Đảng viên (artboard 3 + 3 trống): bảng, tìm, lọc, chọn nhiều, phân trang; modal Thêm/Sửa; hộp xác nhận xóa | Frontend | T15 | Đủ 3 trạng thái bảng; validate theo tài liệu |
| T18 | Wizard Import 3 bước (artboard 4) | Frontend | T17 | Ba bước đúng thiết kế; bảng lỗi hiện đủ cột và lý do |
| T19 | Màn Đợt trao huy hiệu (artboard 5): bảng, banner cảnh báo, **dải độ phủ 12 tháng**, modal Thêm/Sửa, xác nhận xóa | Frontend | T15 | Dải độ phủ vẽ đúng khoảng trống và vạch hôm nay |
| T20 | Trang chi tiết đợt (artboard 5 chi tiết): 2 tab, chọn năm, bảng đủ điều kiện, xuất Excel | Frontend | T19 | Đổi năm thì khoảng ngày và danh sách đổi theo |
| T21 | Màn Chưa thuộc đợt nào (artboard 6) | Frontend | T15 | Cột khoảng trống đúng; trạng thái trống đúng câu chữ |
| T22 | Màn Cài đặt (artboard 7): 3 ô số, xem trước dãy mốc, khôi phục mặc định, tên đơn vị | Frontend | T15 | Xem trước cập nhật ngay khi gõ |
| T23 | Dashboard (artboard 2 + 2 trống): cảnh báo, thẻ đợt sắp tới, bảng, hướng dẫn 3 bước | Frontend | T15 | Cả hai trạng thái đúng thiết kế |
| T24 | Nối API thật, bỏ dữ liệu giả, xử lý lỗi mạng và hết phiên | Frontend | T14, T23 | Toàn bộ màn chạy với backend thật |

> **Cổng duyệt 2 (CEO):** frontend nối xong API thật thì QC mới chạy bộ end-to-end.

### Giai đoạn 3 — Kiểm thử

| Mã | Task | Giao cho | P | Định nghĩa hoàn thành |
|---|---|---|---|---|
| T25 | Kế hoạch kiểm thử + **bộ dữ liệu biên** (người sinh 29/02, người tròn mốc đúng ngày đầu/cuối đợt, người vượt mốc 90, người rơi vào khoảng trống) | QC | T05 | Mỗi ca truy vết được về một QT hoặc UC |
| T26 | Kiểm thử logic backend theo QT1–QT11 | QC | T07 | Phủ đủ các ca biên ở T25; báo lỗi có bước tái hiện |
| T27 | Kiểm thử tích hợp API (có DB thật): phân trang, tìm kiếm, chưa đăng nhập, import file hỏng | QC | T14 | Toàn bộ endpoint trong contract được chạm tới |
| T28 | Kiểm thử end-to-end bằng Playwright: E2E-1 đến E2E-6 | QC | T24 | Sáu luồng chạy xanh, chạy lại được, không phụ thuộc ngày chạy |
| T29 | Vòng sửa lỗi và kiểm thử hồi quy; báo cáo chất lượng cuối | QC | T28 | Không còn lỗi mức chặn; báo cáo nêu rõ phạm vi đã phủ |

### Xuyên suốt — Tài liệu

| Mã | Task | Giao cho | P | Định nghĩa hoàn thành |
|---|---|---|---|---|
| T30 | `README.md`: cách chạy bằng docker compose, cách chạy chế độ phát triển, cách sao lưu bằng `pg_dump` | Technical Writer | T03 | Người mới làm theo được, không cần hỏi |
| T31 | `docs/huong-dan-su-dung.md` cho cán bộ, có ảnh màn hình, theo đúng luồng 6.2 và 6.3 | Technical Writer | T24 | Viết cho người không rành công nghệ |
| T32 | Đồng bộ tài liệu nghiệp vụ với hành vi thực tế; cập nhật nhật ký thay đổi; rà thuật ngữ toàn hệ thống | Technical Writer | T29 | Tài liệu không còn chỗ nói sai so với mã nguồn |

---

## 4. Thứ tự khởi động

Giao ngay ba task này để mở khóa phần còn lại:
1. **T01** → Backend Developer
2. **T05** → Technical Writer (làm song song, dựa trên tài liệu nghiệp vụ)
3. **T04** → Frontend Developer (chỉ cần T01 xong)

CEO theo dõi hai cổng duyệt: sau T05 và sau T24.
