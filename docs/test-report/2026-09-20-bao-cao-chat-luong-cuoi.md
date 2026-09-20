# Báo cáo chất lượng cuối — Hệ thống hỗ trợ xét trao Huy hiệu Đảng (T29)

| | |
|---|---|
| Người viết | QC |
| Ngày | 20/09/2026 |
| Commit `main` đã chạy hồi quy | `5784988` (gộp PR #42 — T28) |
| Nhánh | `test/T29-hoi-quy` |
| Bằng chứng từng ca | `docs/test-report/2026-09-20-test-hoi-quy-ba-tang-tren-main.md` |

## 1. Kết luận

**Còn một lỗi mức chặn phát hành: QC-T29-01 — `npm run build` của Frontend gãy trên
`main`, không dựng được ảnh Docker.** QC đã sửa ngay trong nhánh này vì lỗi do chính
commit T28 của QC gây ra; xin CEO duyệt pull request kèm theo để `main` xanh lại.

Ngoài lỗi đó, chất lượng nghiệp vụ đạt yêu cầu phát hành:

- Ba tầng chạy hết: **610 đạt · 0 hỏng · 11 bỏ qua** trên **621** ca.
- Không quy tắc QT nào bị vi phạm. Mọi con số nghiệp vụ đều đối chiếu với oracle độc
  lập của QC (`tests/fixtures/qt_reference.py` → `expected.json`), không phải với
  chính lời của sản phẩm.
- 11 ca bỏ qua **không** che lỗi nghiệp vụ nào: 1 ca CEO đã chốt loại trừ, 6 ca đã
  được sửa trên pull request đang chờ gộp và QC xác minh xanh hôm nay, 2 ca là lỗi
  còn mở mức nhẹ, 2 ca chờ chính pull request đó để viết được.

## 2. Ba tầng đã phủ những gì

| Tầng | Bộ kiểm thử | Số ca | Cách chạy |
|---|---|---|---|
| 1 · Logic | `Core.UnitTests` (124) · `Core.QcTests` (119) · `Infrastructure.UnitTests` (44) | 287 | Thuần, không cơ sở dữ liệu, ngày cố định T0/T1/T2 |
| 2 · API | `Web.IntegrationTests` (181) · `Web.QcIntegrationTests` (139) · `Infrastructure.IntegrationTests` (1) | 321 | `WebApplicationFactory` + PostgreSQL 16 thật trong container |
| 3 · Đầu-cuối | `FE/e2e` — 6 luồng (83 bước) + 7 ca tiền đề và tự kiểm | 13 | Chromium thật, Backend và PostgreSQL thật, `vi-VN`, `Asia/Ho_Chi_Minh` |

Tầng 1 có **hai bản hiện thực độc lập**: `Core.UnitTests` do Backend viết, `Core.QcTests`
do QC viết lại quy tắc QT1–QT11 từ tài liệu nghiệp vụ rồi quét đối chiếu trên toàn dải
ngày 2024–2035. Một lỗi hiểu sai quy tắc phải lọt qua cả hai bản mới không bị phát hiện.

### Các ca biên đã phủ

- **29/02**: người vào Đảng 29/02 xét ở năm nhuận và năm không nhuận; đợt bắt đầu hoặc
  kết thúc 29/02 gắn vào năm nhuận và không nhuận; năm 1900 (chia hết 100, không nhuận)
  và năm 2000 (chia hết 400, nhuận).
- **Ngày tròn mốc đúng bằng biên đợt**: đúng bằng Từ ngày, đúng bằng Đến ngày, trước
  Từ ngày một ngày, sau Đến ngày một ngày, và đợt chỉ dài đúng một ngày.
- **Người đã vượt mốc lớn nhất**: tuổi đảng đúng bằng 90 và tuổi đảng 91 — mốc kế tiếp
  là `—` và người đó rời mọi danh sách, kể cả khi đổi Bước.
- **Đợt sắp tới khi mọi đợt trong năm đã qua**: nhảy sang đợt sớm nhất của năm sau, cờ
  năm sau bật; đã kiểm ở cả ba tầng.
- **Đổi Bước 5 → 10**: danh sách đủ điều kiện, badge, Dashboard và cột Mốc kế tiếp đều
  đổi theo ngay, không cần thao tác gì thêm (QT5).

## 3. Truy vết QT1–QT11 qua ba tầng

| Quy tắc | Tầng 1 · Logic | Tầng 2 · API | Tầng 3 · E2E |
|---|---|---|---|
| **QT1** Dãy mốc sinh từ cài đặt | U-101 → U-110, quét lưới 9 bộ cài đặt đối chiếu oracle | A-501 → A-506, A-510 | E1-06, E1-07, E3-02 |
| **QT2** Ngày tròn mốc, 29/02 | U-201 → U-208, quét mọi ngày cuối tháng | A-216, A-217 | E5-04 |
| **QT3** Tuổi đảng | U-301 → U-309, quét bộ lõi mọi ngày 2024–2030 | A-125 | E1-14, E6-03, E6-09 |
| **QT3a** Mốc kế tiếp và dấu `—` | U-351 → U-357, đối chiếu oracle ở T0/T1/T2 × hai cài đặt | A-120 | E1-14, E3-09, E6-09, E6-11 |
| **QT4** Đủ điều kiện theo đợt và năm | U-401 → U-419, quét mọi đợt × mọi năm 2020–2035 | A-214 → A-220 | E1-15, E1-16, E3-05, E5-03 |
| **QT5** Không lưu kết quả | U-501 → U-503 + ca lược đồ "không có bảng kết quả" | A-120, A-212, A-502 | E3-05, E3-06, E4-06, E6-04 |
| **QT6** Đợt trao huy hiệu, chồng lấn, khoảng trống | U-601 → U-612, đối chiếu oracle mọi bộ đợt 2024–2030 | A-205 → A-213 | E1-09, E1-10, E4-04, E4-09, E4-11 |
| **QT7** Chưa thuộc đợt nào | U-701 → U-712, quét 10.000 người | A-401 → A-406 | E1-17, E3-07, E3-08, E4-01, E4-08 |
| **QT8** Đợt sắp tới | U-801 → U-809, đối chiếu oracle mọi ngày của 2026 và 2028 | A-301 → A-304 | E1-15, E4-10 |
| **QT9** Import Excel, không kiểm trùng | U-901 → U-921 | A-601 → A-621 | E2-01 → E2-15 |
| **QT10** Xóa hẳn | U-1001, U-1002 | A-121 → A-124, A-213 | E4-12, E6-15, E6-16 |
| **QT11** Trạng thái đợt và đếm ngược | U-1101 → U-1107, đối chiếu oracle mọi ngày 2026–2028 | A-202, A-203 | E1-09 |

## 4. Truy vết UC-00 → UC-51

| Use case | Tầng 2 · API | Tầng 3 · E2E |
|---|---|---|
| UC-00, UC-01 · Đăng nhập, đăng xuất | A-001 → A-007, A-511 | E1-01 → E1-03, E1-19 |
| UC-10 · Dashboard đợt sắp tới | A-301 → A-304 | E1-15 |
| UC-11 · Danh sách đủ điều kiện | A-214 → A-220 | E1-16, E5-01 |
| UC-12, UC-13 · Cảnh báo trên Dashboard | A-305 → A-308 | E1-04, E1-18 |
| UC-20 · Danh sách đảng viên | A-101 → A-115, A-126 | E6-01, E6-12 → E6-14 |
| UC-21, UC-22 · Thêm, sửa đảng viên | A-116 → A-120 | E6-02, E6-05 → E6-09 |
| UC-23 · Xóa đảng viên | A-121 → A-124 | E6-15, E6-16 |
| UC-24 · Import Excel ba bước | A-601 → A-618, A-620, A-621 | E2-01 → E2-15 |
| UC-25 · Tải file mẫu | A-619 | E2-02, E2-03 |
| UC-30 · Màn Đợt trao huy hiệu | A-201 → A-204 | E1-09 |
| UC-31, UC-32, UC-33 · Thêm, sửa, xóa đợt | A-205 → A-213 | E4-04, E4-11, E4-12 |
| UC-34 · Chi tiết đợt theo năm | A-214 → A-219 | E5-04, E5-05 |
| UC-36 · Dải độ phủ 12 tháng | A-210, A-211 | E1-10 |
| UC-40 · Chưa thuộc đợt nào | A-401 → A-406 | E4-01, E4-08, E5-06 |
| UC-50 · Cài đặt mốc | A-501 → A-506, A-510 | E1-06, E3-02 |
| UC-51 · Tên đơn vị và xuất Excel | A-507, A-508, A-701 → A-712 | E1-08, E5-02, E5-08 |

Cả 28 endpoint của `docs/api-contract.md` đều có ít nhất một ca chạm tới — bảng đối
chiếu đầy đủ ở `docs/test-report/2026-09-20-test-tich-hop-api-qc-t27.md`.

## 5. Lỗi còn lại

### QC-T29-01 · `npm run build` của Frontend gãy trên `main` — **CHẶN**

| | |
|---|---|
| **Mức độ** | **Chặn** — không dựng được ảnh Docker của Frontend, tức không phát hành được |
| **Nguồn gốc** | Commit `36b8af6` (T28, do QC viết) |
| **Trạng thái** | QC đã sửa trong nhánh `test/T29-hoi-quy`, chờ CEO duyệt |

**Bước tái hiện** — trên `main` tại `5784988`: `cd FE && npm run build`.

**Kết quả mong đợi** — dựng xong thư mục `dist`, mã thoát 0.

**Kết quả thực tế** — 106 lỗi TypeScript, mã thoát 2; toàn bộ nằm trong `FE/e2e/`.
`docker compose -f e2e/docker-compose.e2e.yml up --build` gãy ở đúng bước
`RUN npm run build` của `FE/Dockerfile`.

**Nguyên nhân** — `FE/tsconfig.node.json` đã `include` thư mục `e2e` từ commit dựng
khung T00B, khi thư mục đó còn rỗng. T28 đổ mã Playwright vào đấy, nên `tsc -b` bắt
đầu kiểm bộ e2e bằng cấu hình dành cho mã chạy bằng Node: `module: nodenext` (đòi
đuôi `.js` ở mọi import tương đối) và không có thư viện `DOM` (trong khi `page.evaluate`
cần `window`).

**Cách sửa** — tách `FE/e2e` thành một dự án TypeScript riêng (`FE/e2e/tsconfig.json`,
có `lib: DOM`, `moduleResolution: bundler`) và tham chiếu nó từ `FE/tsconfig.json`.
Bộ e2e **vẫn được kiểm kiểu đầy đủ** khi chạy `tsc -b` — đã chứng minh bằng cách chèn
một lỗi giả vào thư mục e2e và thấy `tsc` bắt được.

### QC-T27-05 · Giá trị lọc giới tính lạ bị bỏ qua lặng lẽ — **Trung bình**

| | |
|---|---|
| **Use case / ca** | UC-20, ca `A-113` · `A8DefectTests.QcT2705` |
| **Mức độ** | Trung bình — sai dữ liệu người dùng nhìn thấy, không hỏng dữ liệu đã lưu |
| **Trạng thái** | **Còn mở.** PR #35 chưa chạm tới. Xác minh lại hôm nay vẫn đỏ |

**Bước tái hiện** — `GET /api/PartyMembers?filter.Gender=$eq:Khac&pageSize=1`.

**Kết quả mong đợi** — hoặc 400, hoặc `totalCount = 0`.

**Kết quả thực tế** — 200 kèm `totalCount = 32`, tức trả cả kho. Người dùng tưởng đang
lọc nhưng đang nhìn toàn bộ danh sách. Cùng hành vi với giá trị rỗng và giá trị `1`.

### QC-T27-07 · Hợp đồng API thiếu ba khóa thông điệp — **Nhẹ**

| | |
|---|---|
| **Use case / ca** | Mục 1.5 `docs/api-contract.md`, ca `A-905` · `A8DefectTests.QcT2707` |
| **Mức độ** | Nhẹ — lỗi tài liệu; hệ quả là người dùng thấy câu chung chung thay vì câu nói rõ chỗ sai |
| **Trạng thái** | **Còn mở.** Chủ sở hữu tài liệu là Technical Writer |

**Bước tái hiện** — `POST /api/PartyMembers` với `fullName` dài 300 ký tự.

**Kết quả mong đợi** — khóa trả về có trong bảng mục 1.5 để Frontend dịch được.

**Kết quả thực tế** — Backend trả `Mes.PartyMember.OverLength.FullName`, bảng mục 1.5
không có khóa này, nên Frontend chỉ hiện "Thao tác không thực hiện được". Hai khóa
`Mes.AwardPeriod.OverLength.Name` và `Mes.PartyMember.Required.Ids` cùng tình trạng.

### Lỗi đã xác minh là ĐÃ SỬA nhưng pull request chưa vào `main`

QC dựng lại môi trường từ nhánh của từng pull request rồi chạy lại đúng ca đã báo:

| Mã lỗi | Ca kiểm thử | Sửa ở | Kết quả chạy lại hôm nay |
|---|---|---|---|
| QC-T27-01 / QC-T28-01 · Backend không đọc `HUYHIEUDANG_TEST_TODAY` | `A-903b`, `E0-01` | PR #38 | **Xanh.** Máy chủ báo đúng 2026-09-19; chạy lại cả 6 luồng E2E với đồng hồ đã đóng băng: 11 đạt, 0 hỏng |
| QC-T27-02 · Số trang lớn tràn số, trả 500 | `A8DefectTests.QcT2702` | PR #35 | **Xanh** |
| QC-T27-03 · Cỡ trang không có trần | `A8DefectTests.QcT2703` | PR #35 | **Xanh** |
| QC-T27-04 · Lỗi ép kiểu trả câu tiếng Anh | `A8DefectTests.QcT2704` | PR #35 | **Xanh** |
| QC-T27-06 · Xem trước dãy mốc không có trần | `A8DefectTests.QcT2706` | PR #35 | **Xanh** |
| QC-T28-04 · Nút Hủy ở bước 2 của Import về bước 1 | `E2-08` | PR #43 | **Xanh** với ca viết theo hành vi mới (Hủy về thẳng danh sách đảng viên) |

Thêm bằng chứng đồng hồ đã đóng băng thật, gọi thẳng API với hai mốc khác:

```
HUYHIEUDANG_TEST_TODAY=2026-10-15 → today 2026-10-15 · đợt sắp tới Đợt 7/11 · Ongoing (QT11)
HUYHIEUDANG_TEST_TODAY=2026-12-01 → today 2026-12-01 · đợt sắp tới Đợt 3/2 năm 2027 · Upcoming (QT8)
```

### Lỗi đã đóng từ các vòng trước

| Mã lỗi | Nội dung | Ca chạy lại |
|---|---|---|
| QC-01 | Đợt có Từ ngày sau Đến ngày, ngày 31/02 không bị chặn | `U-602`, `U-606`, `U-607` — ĐẠT |
| QC-02 | Nhãn khoảng trống khi chưa cài đợt nào | `U-712`, `A-401b` — ĐẠT |
| QC-03 | Hai đợt cùng Từ ngày cho kết quả đổi theo thứ tự nạp | `U-808` — ĐẠT |
| QC-04 | Bước bằng `int.MaxValue` sinh mốc âm | `QC-04` — ĐẠT |
| QC-T12 | Cảnh báo phủ kín báo nhầm khi chưa có đợt nào | `QT6 · Chưa cài đợt nào` — ĐẠT |
| QC-T28-02 | Ca `E4-10` của kế hoạch trái QT8 | Kế hoạch đã sửa trong nhánh này |
| QC-T28-03 | Ca `E6-11` của kế hoạch thiếu bước lùi Ngày sinh | Kế hoạch đã sửa trong nhánh này |

## 6. Vì sao 11 ca còn để `Skip`

| Ca | Lý do | Ai gỡ, khi nào |
|---|---|---|
| `QC-05` | CEO đã chốt loại trừ `BaseEntity.CreatedAt` khỏi lệnh cấm đọc đồng hồ | Giữ nguyên |
| `A-903b`, `E0-01` | Lỗi đã sửa ở PR #38, giữ `Skip` để `main` không đỏ trước khi PR gộp | QC, ngay sau khi PR #38 vào `main` |
| `QcT2702`, `QcT2703`, `QcT2704`, `QcT2706` | Lỗi đã sửa ở PR #35, cùng lý do trên | QC, ngay sau khi PR #35 vào `main` |
| `QcT2705`, `QcT2707` | Lỗi **còn mở**, chưa ai sửa | Gỡ khi có bản sửa |
| `E-903`, `E-904` | Cần đồng hồ đóng băng của PR #38 mới viết được ca thật: chạy lại cả sáu luồng ở mốc T1 và T2 | QC, ngay sau khi PR #38 vào `main` |

## 7. Việc còn lại và khuyến nghị

**Trước khi phát hành**

1. Gộp pull request của T29 để `main` dựng được ảnh Frontend trở lại (QC-T29-01).
2. Gộp PR #35 và PR #38 — sáu lỗi đã sửa và đã được xác minh xanh, nhưng người dùng
   chưa được hưởng vì chúng chưa vào `main`.
3. Sửa QC-T27-05: bộ lọc giới tính mang giá trị lạ phải trả 400 hoặc 0 dòng.
4. Technical Writer bổ sung ba khóa thiếu vào bảng mục 1.5 hợp đồng API (QC-T27-07).

**Ngay sau khi PR #35 và #38 vào `main`** — QC gỡ `Skip` của 6 ca, viết ca thật cho
`E-903` và `E-904`, rồi chạy lại cả ba tầng ở ba mốc T0, T1, T2.

**Phần phải chạy lại vì chạy trước khi các pull request giao diện gộp** — lúc QC chạy
hồi quy, `main` đang ở `5784988` và bảy pull request còn mở: #35, #37 (T33 màn Đảng
viên), #38, #39 (T39, T40), #40 (T43), #41 (T42, T44), #43 (T45). Bộ E2E bám
`aria-label` và vai trò chứ không bám chữ trên nút, nên phần lớn chịu được các chỉnh
giao diện đó, nhưng **sau khi gộp hết** vẫn phải chạy lại trọn tầng 3 — riêng E2E-6
(ảnh hưởng bởi #37) và E2E-2 (ảnh hưởng bởi #43) là bắt buộc.

**Khuyến nghị dài hạn**

- Cho `npm run build` và `dotnet test` chạy tự động trên mỗi pull request. Lỗi
  QC-T29-01 lọt vào `main` chỉ vì không ai dựng gói giao diện sau khi gộp.
- Giữ nguyên lối làm hai bản hiện thực độc lập cho quy tắc nghiệp vụ. Bốn lỗi QT ở
  vòng T26 đều do bản đối chứng của QC phát hiện, không phải do test của Backend.
- Bộ kiểm thử không được phụ thuộc ngày chạy. Sau khi PR #38 vào `main`, ca `E0-02`
  (lưới an toàn cho khoảng ngày) nên **giữ lại** — nó vẫn bắt được trường hợp biến ép
  ngày bị cấu hình sai và máy chủ lặng lẽ quay về ngày thật.
