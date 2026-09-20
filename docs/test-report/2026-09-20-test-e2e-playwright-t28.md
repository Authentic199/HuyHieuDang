# Báo cáo kiểm thử — End-to-end Playwright, sáu luồng E2E-1 đến E2E-6 (T28)

| | |
|---|---|
| Task | T28 — Kiểm thử end-to-end Playwright |
| Người chạy | QC |
| Ngày chạy | 20/09/2026 |
| Nhánh | `test/T28-e2e-playwright` |
| Mã nguồn bộ kiểm thử | `FE/e2e/` |
| Cách dựng lại môi trường | `FE/e2e/README.md` |

Sáu luồng chạy trên **trình duyệt thật** (Chromium 1440×900, `vi-VN`,
`Asia/Ho_Chi_Minh`) với **Backend và PostgreSQL 16 thật** dựng bằng
`FE/e2e/docker-compose.e2e.yml`. Không dùng tầng dữ liệu giả nào của Frontend.

Mọi con số mong đợi lấy từ `tests/fixtures/data/expected.json` — oracle do
`tests/fixtures/qt_reference.py` tính từ QT1–QT11. Bộ kiểm thử không tự tính lại
con số nghiệp vụ nào.

---

## 1. Lệnh đã chạy và kết quả

```bash
cd FE
docker compose -f e2e/docker-compose.e2e.yml up -d --build
npm ci && npx playwright install chromium
npx playwright test -c e2e/playwright.e2e.config.ts
```

### Lần 1

```
───── Môi trường kiểm thử end-to-end (T28) ─────
  Giao diện      : http://localhost:4174
  API            : http://localhost:4174/api
  Mốc đang ép    : 2026-09-19
  Máy chủ báo    : 2026-09-20
  Đồng hồ        : CHƯA đóng băng được — Backend bỏ qua HUYHIEUDANG_TEST_TODAY
                   (lỗi QC-T27-01 / QC-T28-01, ca E0-01 báo chi tiết).
                   Ngày máy chủ vẫn nằm trong khoảng an toàn 2026-09-11 … 2026-09-29
                   nên mọi con số của bộ dữ liệu biên còn nguyên giá trị;
                   riêng số ngày đếm ngược tính theo ngày máy chủ báo về.
────────────────────────────────────────────────
Running 13 tests using 1 worker
  -   1 e0-tien-de.spec.ts:21:8 › E0-01 · Backend đóng băng "hôm nay" theo HUYHIEUDANG_TEST_TODAY [QC-T28-01]
  ok  2 e0-tien-de.spec.ts:36:3 › E0-02 · Ngày máy chủ nằm trong khoảng an toàn của bộ dữ liệu biên (116ms)
  ok  3 e0-tien-de.spec.ts:47:3 › E0-03 · Đồng hồ trình duyệt đóng băng đúng mốc và đúng múi giờ (280ms)
  ok  4 e1-lan-dung-dau-tien.spec.ts:36:1 › E2E-1 · Lần dùng đầu tiên: đăng nhập → cài đặt → tạo đợt → import → Dashboard (33.2s)
  ok  5 e2-import-co-loi.spec.ts:30:1 › E2E-2 · Import có lỗi: xem trước đúng, chỉ dòng hợp lệ được nạp (12.9s)
  ok  6 e3-doi-cai-dat-lan-truyen.spec.ts:35:1 › E2E-3 · Đổi Bước từ 5 sang 10 lan truyền ngay sang mọi màn (13.7s)
  ok  7 e4-sua-dot-lan-truyen.spec.ts:54:1 › E2E-4 · Nới Đến ngày của một đợt lan truyền ngay sang mọi màn (18.0s)
  ok  8 e5-xuat-excel.spec.ts:70:1 › E2E-5 · Xuất Excel từ Dashboard, chi tiết đợt và Chưa thuộc đợt nào (33.4s)
  ok  9 e6-vong-doi-dang-vien.spec.ts:95:1 › E2E-6 · Vòng đời đảng viên: thêm tay → tìm → sửa → xóa nhiều dòng (34.6s)
  -  10 e9-chay-lai-va-doc-lap.spec.ts:30:8 › E-903 · Chạy lại ở mốc T1 = 2026-10-15 [QC-T28-01]
  -  11 e9-chay-lai-va-doc-lap.spec.ts:38:8 › E-904 · Chạy lại ở mốc T2 = 2026-12-01 [QC-T28-01]
  ok 12 e9-chay-lai-va-doc-lap.spec.ts:44:3 › E-905 · Không ca nào dùng waitForTimeout (9ms)
  ok 13 e9-chay-lai-va-doc-lap.spec.ts:73:3 › E-906 · Mỗi luồng tự dựng trạng thái đầu ngay ở dòng đầu tiên (3ms)

  3 skipped
  10 passed (2.5m)
EXIT=0
```

### Lần 2 — chạy lại ngay sau lần 1, **không** reset tay (ca E-901)

```
Running 13 tests using 1 worker
  -   1 E0-01 · Backend đóng băng "hôm nay" theo HUYHIEUDANG_TEST_TODAY [QC-T28-01]
  ok  2 E0-02 · Ngày máy chủ nằm trong khoảng an toàn của bộ dữ liệu biên (131ms)
  ok  3 E0-03 · Đồng hồ trình duyệt đóng băng đúng mốc và đúng múi giờ (219ms)
  ok  4 E2E-1 · Lần dùng đầu tiên: đăng nhập → cài đặt → tạo đợt → import → Dashboard (33.9s)
  ok  5 E2E-2 · Import có lỗi: xem trước đúng, chỉ dòng hợp lệ được nạp (12.1s)
  ok  6 E2E-3 · Đổi Bước từ 5 sang 10 lan truyền ngay sang mọi màn (14.9s)
  ok  7 E2E-4 · Nới Đến ngày của một đợt lan truyền ngay sang mọi màn (18.9s)
  ok  8 E2E-5 · Xuất Excel từ Dashboard, chi tiết đợt và Chưa thuộc đợt nào (34.3s)
  ok  9 E2E-6 · Vòng đời đảng viên: thêm tay → tìm → sửa → xóa nhiều dòng (35.6s)
  -  10 E-903 · Chạy lại ở mốc T1 = 2026-10-15 [QC-T28-01]
  -  11 E-904 · Chạy lại ở mốc T2 = 2026-12-01 [QC-T28-01]
  ok 12 E-905 · Không ca nào dùng waitForTimeout (10ms)
  ok 13 E-906 · Mỗi luồng tự dựng trạng thái đầu ngay ở dòng đầu tiên (3ms)

  3 skipped
  10 passed (2.5m)
EXIT=0
```

### Lần 3 — chạy **đảo thứ tự**, mỗi luồng một lần gọi riêng (ca E-902)

```
  ok 1 e6-vong-doi-dang-vien.spec.ts › E2E-6 · Vòng đời đảng viên (35.3s)          1 passed (36.0s)
  ok 1 e5-xuat-excel.spec.ts › E2E-5 · Xuất Excel từ ba nơi (33.7s)                1 passed (34.4s)
  ok 1 e4-sua-dot-lan-truyen.spec.ts › E2E-4 · Nới Đến ngày của một đợt (18.6s)    1 passed (19.3s)
  ok 1 e3-doi-cai-dat-lan-truyen.spec.ts › E2E-3 · Đổi Bước 5 → 10 (15.2s)         1 passed (15.8s)
  ok 1 e2-import-co-loi.spec.ts › E2E-2 · Import có lỗi (13.1s)                    1 passed (13.7s)
  ok 1 e1-lan-dung-dau-tien.spec.ts › E2E-1 · Lần dùng đầu tiên (33.0s)            1 passed (33.6s)
```

Ba lần chạy cho cùng kết quả. Mỗi luồng gọi `api.seedBaseline()` hoặc
`api.resetAll()` ở dòng đầu tiên, nên chạy độc lập và theo thứ tự bất kỳ.

---

## 2. Phủ của sáu luồng

83 bước trong `docs/test-plan.md` mục 6 đều được viết thành `test.step` mang
đúng mã ca, nên báo cáo Playwright chỉ đúng bước nào đỏ.

| Luồng | Bước | Điều đã chứng minh |
|---|---|---|
| **E2E-1** Lần dùng đầu tiên | E1-01 → E1-19 | Chặn truy cập khi chưa đăng nhập; sai mật khẩu báo một câu chung; Dashboard trống hiện khối 3 bước và hai cảnh báo, không có badge; Cài đặt hiện 30/90/5 và 13 mốc, gõ Bước 10 xem trước đổi ngay mà dữ liệu chưa đổi; lưu tên đơn vị thì header đổi theo; tạo 4 đợt **lệch thứ tự** vẫn ra bảng sắp theo Từ ngày với trạng thái Đã qua · Đã qua · Đã qua · Sắp tới; banner nêu đúng 5 khoảng trống, dải độ phủ có vạch Hôm nay; file mẫu đúng 4 cột; xem trước 32 người 0 lỗi và **không ghi gì vào kho**; nạp xong danh sách 32 người, ô trống hiện `—`, hai người vượt mốc lớn nhất có Mốc kế tiếp `—`; Dashboard đúng Đợt 7/11, đúng khoảng ngày, đúng 6 người và đúng phân bổ mốc; bảng 6 dòng đúng thứ tự và đúng ngày tròn mốc; badge 7; đăng xuất rồi mở lại URL thì bị đẩy về Đăng nhập |
| **E2E-2** Import có lỗi | E2-01 → E2-15 | Bước 1 mô tả đủ 4 cột, dòng ví dụ, `.xlsx` ≤ 10 MB, định dạng ngày; file mẫu tải về khớp `mau-dang-vien.xlsx` và **nạp lại chính nó được**; xóa nhiều dòng về 0; xem trước báo đúng số hợp lệ / số lỗi kèm cảnh báo không kiểm trùng; hai tab có số khớp; bảng lỗi đúng 6 cột, đúng số dòng Excel 8-9-10-11 và đúng lý do, ô sai giữ nguyên chữ thô; Hủy không ghi gì; nạp xong tăng đúng bằng số dòng hợp lệ; nạp lại cùng file tăng gấp đôi (QT9 không kiểm trùng); bốn lỗi cấp file (sai cột, không phải Excel, vượt 10 MB, file rỗng) đều bị chặn ngay bước 1 bằng tiếng Việt, không lộ khóa thông điệp, không lộ lỗi máy chủ, và không ghi gì vào kho |
| **E2E-3** Đổi cài đặt lan truyền | E3-01 → E3-10 | Chưa lưu thì không đổi gì; lưu Bước = 10 làm chi tiết đợt còn 4 người (mất mốc 35 và 45, giữ mốc 40), Dashboard đổi theo **mà không cần thao tác gì thêm**, badge còn 6, màn Chưa thuộc đợt nào còn 6 dòng, cột Mốc kế tiếp của danh sách Đảng viên đổi theo dãy mốc mới, người vượt mốc lớn nhất vẫn `—`; Khôi phục mặc định đưa mọi con số về đúng trạng thái đầu |
| **E2E-4** Sửa đợt lan truyền | E4-01 → E4-12 | Lê Văn Cường nằm ở khoảng trống "Giữa Đợt 2/9 và Đợt 7/11" ngày 30/09/2026; nới Đến ngày 10/09 → 30/09 (modal nhắc hiệu lực ngay) kéo anh vào Đợt 2/9, đợt lên 5 người, badge còn 6, banner còn 4 khoảng trống; nới tiếp tới 07/11 vẫn **lưu được** kèm cảnh báo chồng lấn nêu đúng cặp đợt (QT6); hoàn nguyên về 10/09 đưa mọi con số về đúng E4-01 |
| **E2E-5** Xuất Excel | E5-01 → E5-10 | Ba nơi xuất đều đúng tên tệp theo quy ước bỏ dấu (`DuDieuKien_Dot7-11_2026.xlsx`, `DuDieuKien_Dot3-2_2026.xlsx`, `ChuaThuocDot_2026.xlsx`); **mở tệp ra đọc**: dòng tiêu đề có tên đơn vị, tên đợt kèm khoảng ngày đã gắn năm, ngày xuất; đúng 1 sheet; số dòng và **từng ô** khớp bảng trên màn hình; Ngô Văn Khánh tròn mốc 28/02/2026 (QT2 năm không nhuận); đổi năm sang 2027 thì tên tệp và khoảng ngày gắn năm 2027; tệp M4 có thêm cột Khoảng trống đúng nội dung; ô trống trong tệp là **rỗng**, không phải `—`; xóa tên đơn vị thì tiêu đề bỏ hẳn dòng đó, không để dòng trắng lạ; xuất danh sách rỗng vẫn ra tệp có tiêu đề 0 dòng; nạp thêm bộ lớn cho "1.232 người" dùng dấu chấm ngăn nghìn và không làm lệch con số của bộ lõi |
| **E2E-6** Vòng đời đảng viên | E6-01 → E6-17 | Thêm tay lên 33; tìm thấy đúng một dòng với tuổi đảng 29, mốc kế tiếp 30 ngày 01/10/2026; Dashboard lên 7 ngay (QT5); ngày chính thức ở tương lai bị chặn ở **cả lịch giao diện lẫn máy chủ**; bỏ trống Họ tên bị chặn; người chỉ có Họ tên và Ngày chính thức hiện hai ô `—`; sửa ngày chính thức làm tuổi đảng thành 34, mốc kế tiếp 35 ngày 05/10/2026 và Dashboard xếp đúng mốc 35; sửa tiếp thành 91 tuổi đảng thì mốc kế tiếp `—` và người đó rời danh sách; lọc Giới tính đúng số; đổi trang không mất không trùng người; chọn 3 dòng báo "Đang chọn 3 dòng", hộp xác nhận nêu rõ "3 người", xóa xong tổng giảm đúng 3; bấm Xóa rồi Hủy thì tổng không đổi; kho trở về đúng 32 người, Dashboard và badge về đúng trạng thái đầu |

---

## 3. Lỗi phát hiện

### QC-T28-01 — Backend không đọc `HUYHIEUDANG_TEST_TODAY`, không đóng băng được "hôm nay"

| | |
|---|---|
| **Quy tắc bị vi phạm** | `docs/test-plan.md` mục 2, **T-FIX-4**: "Playwright cần một tiến trình BE thật. BE đọc biến môi trường `HUYHIEUDANG_TEST_TODAY=2026-09-19`. Chỉ đọc khi `ASPNETCORE_ENVIRONMENT != Production`." |
| **Ca kiểm thử** | `E0-01` (đang `Skip`), `E-903`, `E-904` (đang `Skip`). Cùng gốc với `QC-T27-01` / ca `A903b` trong `BE/tests/HuyHieuDang.Web.QcIntegrationTests/A9TechnicalTests.cs` |
| **Mức độ** | **Chặn** — chặn tính tất định của cả tầng E2E |

**Bước tái hiện**

1. `cd FE && docker compose -f e2e/docker-compose.e2e.yml up -d --build`
   (tệp này đặt sẵn `HUYHIEUDANG_TEST_TODAY: 2026-09-19` cho service `be`).
2. Đăng nhập rồi gọi `GET /api/Dashboard`.

**Kết quả mong đợi** — `data.today = "2026-09-19"`, thẻ Dashboard hiện
"Còn **12** ngày" đúng `expected.json` kịch bản `core_default_T0`.

**Kết quả thực tế** — `data.today = "2026-09-20"` (ngày thật của máy), thẻ hiện
"Còn **11** ngày". Xem ảnh `anh-dashboard.png` đính kèm: header "Hôm nay
20/09/2026". `grep -r HUYHIEUDANG_TEST_TODAY BE/src` không ra kết quả nào.

**Hệ quả đã đo được, không phải suy đoán**

`tests/fixtures/excel/loi-4-dong.xlsx` có dòng Excel 11 với Ngày chính thức
**20/09/2026**, cố ý đặt ở tương lai so với T0. Chạy hôm nay:

```
POST /api/PartyMembers/Import/Preview  loi-4-dong.xlsx
→ valid 7, errors 3      (kế hoạch và expected.json: valid 6, errors 4)
```

Hôm qua bộ dữ liệu cho 6/4, hôm nay cho 7/3 — đúng kiểu "bộ kiểm thử tự đổi màu
theo ngày chạy" mà T-FIX sinh ra để chặn.

**Cách T28 sống chung trong lúc chờ sửa** — ca `E0-02` bắt buộc ngày máy chủ nằm
trong khoảng **11/09/2026 – 29/09/2026**, khoảng mà mọi con số của bộ dữ liệu
biên còn nguyên (ngày tròn mốc gần T0 nhất về trước là 10/09 của Trương Thị
Nhàn, về sau là 30/09 của Lê Văn Cường; Đợt 2/9 đóng 10/09, Đợt 7/11 mở 01/10).
Ra ngoài khoảng đó, bộ kiểm thử dừng ngay với một câu giải thích thay vì đỏ
hàng loạt vì lý do khác. Chỉ **hai** giá trị phải suy theo ngày máy chủ báo về:
số ngày đếm ngược tới Đợt 7/11, và dòng lỗi "ngày ở tương lai" nói trên.
Sửa xong T-FIX-4 thì bỏ `Skip` của `E0-01`, `E-903`, `E-904` là ba ca chạy
được ngay, và hai giá trị kia tự chốt về 12 ngày và 6/4.

**Đề nghị** — Backend bổ sung đúng như T-FIX-4 mô tả: đọc biến khi
`ASPNETCORE_ENVIRONMENT != "Production"`, ghi một dòng log `Warning` lúc khởi
động nêu rõ ngày đang bị ép, và bỏ qua biến ở Production (ca `A-903` hiện đã đạt
phải giữ nguyên đạt).

---

### QC-T28-02 — Kế hoạch kiểm thử ca E4-10 mâu thuẫn với QT8

| | |
|---|---|
| **Quy tắc bị vi phạm** | `docs/2026-09-17-huyhieudang-business-design.md` **QT8**: "chọn đợt có `Đến ≥ hôm nay` và `Từ` nhỏ nhất (**nếu hôm nay đang nằm trong một đợt thì chính là đợt đó**)" |
| **Ca kiểm thử** | `E4-10` |
| **Mức độ** | **Nhẹ** — lỗi tài liệu, không phải lỗi mã |

`docs/test-plan.md` mục 6 viết: "E4-10 · Dashboard | Đợt sắp tới **vẫn** là Đợt
7/11, vẫn 6 người". Nhưng ca E4-04 vừa nới Đợt 2/9 thành 15/08 – 30/09, nên hôm
nay (T0 = 19/09) **nằm trong** Đợt 2/9 — theo QT8, đợt sắp tới phải là Đợt 2/9 ở
trạng thái "Đang diễn ra". Chính oracle `expected.json` kịch bản
`core_default_T0_widenedP3` cũng tính ra như vậy:

```json
"upcomingPeriod": { "name": "Đợt 2/9", "status": "Đang diễn ra", "daysLeft": null }
```

Sản phẩm làm **đúng**. Ca E4-10 trong bộ kiểm thử được viết theo QT8 và theo
oracle, kèm chú thích tại chỗ; đồng thời vẫn kiểm Đợt 7/11 không bị đụng tới
(vẫn đúng 6 người). Đề nghị CEO cho phép QC sửa lại dòng E4-10 trong
`docs/test-plan.md` ở vòng T29.

---

### QC-T28-03 — Kế hoạch kiểm thử ca E6-11 thiếu một bước

| | |
|---|---|
| **Quy tắc bị vi phạm** | Ràng buộc "Ngày sinh phải trước Ngày vào Đảng chính thức" (`Mes.PartyMember.Invalid.DateOfBirth`, có ở cả biểu mẫu lẫn máy chủ) |
| **Ca kiểm thử** | `E6-11` |
| **Mức độ** | **Nhẹ** — lỗi tài liệu, không phải lỗi mã |

E6-02 thêm người với Ngày sinh **10/10/1970**; E6-11 bảo sửa Ngày chính thức
thành **01/05/1935**. Hai giá trị đó không sống chung được — biểu mẫu chặn đúng
quy tắc. Ca trong bộ kiểm thử lùi Ngày sinh cùng lúc (12/03/1915) và giữ nguyên
phần cốt lõi phải chứng minh: tuổi đảng 91, Mốc kế tiếp `—`, người đó rời danh
sách Đợt 7/11. Đề nghị bổ sung một chữ vào dòng E6-11 của kế hoạch ở vòng T29.

---

### QC-T28-04 — Nút "Hủy" ở bước 2 của Import quay về bước 1, không về danh sách

| | |
|---|---|
| **Use case** | UC-24, ca `E2-08` của `docs/test-plan.md`: "Bấm Hủy | **Về danh sách**, vẫn 0 người" |
| **Mức độ** | **Nhẹ** — khác biệt điều hướng, không sai số liệu |

Thực tế: "Hủy" ở bước 2 bỏ file và quay về **bước 1**; phải bấm "Hủy" lần nữa ở
bước 1 mới về danh sách Đảng viên. Bảo đảm quan trọng nhất vẫn đúng — không có
dòng nào được ghi vào kho. Ca `E2-08` viết theo hành vi hiện tại và vẫn kiểm
trọn ý định của kế hoạch (về được danh sách, vẫn 0 người). Cần CEO chốt: sửa mã
theo kế hoạch, hay sửa kế hoạch theo mã.

---

## 4. Điều đã soi nhưng **không** phải lỗi

- **Thứ tự sắp xếp tiếng Việt.** Bảng đủ điều kiện đặt "Đào Văn Ân" trước
  "Nguyễn Văn An", khớp `expected.json` và khớp OQ-3 (collation `vi`).
- **Giao diện không tự tính lại ngày.** Header lấy "hôm nay" từ `serverDate` của
  máy chủ, không từ `new Date()` của trình duyệt. Ca `E6-17` kiểm bằng cách ép
  đồng hồ trình duyệt về 19/09 trong khi máy chủ ở 20/09: header hiện đúng ngày
  **máy chủ**. Đây chính là phần T-FIX-5 mà Frontend đã làm đúng.
- **Xem trước import không ghi gì vào kho.** Kiểm lại bằng API ngay sau mỗi lần
  xem trước ở E1-12, E2-05, và sau cả bốn ca lỗi cấp file.
- **Nạp lại cùng một file làm tổng tăng gấp đôi.** Đây là hành vi **đúng** theo
  QT9 (không kiểm trùng), và giao diện có cảnh báo trước.

---

## 5. Ghi chú kỹ thuật của bộ kiểm thử

- **Môi trường riêng.** `FE/e2e/docker-compose.e2e.yml` dựng stack riêng (giao
  diện `4174`, API `18080`, PostgreSQL `55433`) vì bộ kiểm thử xóa sạch dữ liệu
  trước mỗi luồng — không được đụng vào stack phát triển ở `docker-compose.yml`
  gốc kho.
- **Chờ theo điều kiện.** Ca `E-905` quét mã và chặn `waitForTimeout`; mọi chỗ
  chờ đều chờ theo điều kiện.
- **Selector theo `aria-label`.** Màn Đảng viên sắp đổi ở T33 (bỏ nút xóa từng
  dòng, nút xóa nhiều thành biểu tượng thùng rác đỏ) — E2E-6 bám
  `aria-label` (`Sửa <tên>`, `Xóa <tên>`) và vai trò, không bám chữ trên nút.
- **Đọc Excel không thêm phụ thuộc.** `FE/e2e/fixtures/xlsx.ts` tự giải nén ZIP
  và đọc SpreadsheetML bằng thư viện chuẩn của Node, vì `FE/package.json` không
  có gói đọc Excel và T28 không được phép thêm phụ thuộc vào đó. Trình đọc chịu
  được cả tệp của `openpyxl` (bộ dữ liệu) lẫn của MiniExcel (Backend xuất), kể
  cả tiền tố không gian tên `x:` và khoảng trắng kiểu `t ="str"`.
- **Tệp 11 MB** cho ca E2-14 được `global-setup.ts` dựng tại chỗ, vì bộ dữ liệu
  cố ý không commit nó (`tests/fixtures/.gitignore`).

---

## 6. Việc còn lại

| Việc | Của ai | Khi nào |
|---|---|---|
| Cài `HUYHIEUDANG_TEST_TODAY` theo T-FIX-4 (QC-T28-01) | Backend | Trước T29 |
| Bỏ `Skip` của `E0-01`, `E-903`, `E-904` rồi chạy lại ba mốc T0/T1/T2 | QC | Ngay sau khi QC-T28-01 xong |
| Chốt QC-T28-02, QC-T28-03, QC-T28-04 rồi đồng bộ `docs/test-plan.md` | CEO + QC | Vòng T29 |
