# Báo cáo nghiệm thu bản gộp lượt 2 (T63 + T64 + T66)

- Việc: HUYH-77 (T67) — QC nghiệm thu bản gộp lượt 2 trước khi chủ dự án merge
- Nhánh: `test/T67-nghiem-thu-luot-2`, tách từ đầu nhánh PR #64 (`e94f153`)
- Commit bản gộp: `922b5e7` · commit thêm ca kiểm chéo: `d1dfb42`
- Ngày chạy: 30/09/2026
- Phạm vi: kiểm **bản gộp cả ba PR**, không kiểm từng PR riêng. Không sửa mã sản phẩm, không sửa `CHANGELOG.md`, không chạm `BE/`.

| Việc | Issue | PR | Nhánh | Đầu nhánh đã gác |
| --- | --- | --- | --- | --- |
| T63 — bỏ hẳn xưng hô khỏi giao diện | HUYH-72 | #66 | `fix/T63-bo-xung-ho` | `2f17456` |
| T64 — màn Đợt: bỏ dòng tóm tắt và nền vàng | HUYH-73 | #67 | `fix/T64-man-dot-bo-tom-tat-va-nen-vang` | `d250a6b` |
| T66 — Chi tiết đợt một trang, bộ chọn năm | HUYH-76 | #68 | `feat/T66-chi-tiet-dot-mot-trang-bo-chon-nam` | `9c82061` |

Cả ba đầu nhánh khớp đúng bảng CEO đã gác, và cả ba đều có `git merge-base` với `e94f153` bằng chính `e94f153`.

## Phán quyết

| PR | Phán quyết |
| --- | --- |
| #66 — HUYH-72 (T63) | **ĐẠT** |
| #67 — HUYH-73 (T64) | **ĐẠT** |
| #68 — HUYH-76 (T66) | **ĐẠT** |
| Bản gộp cả ba | **ĐẠT** — không có lỗi nào phải báo về issue gốc |

## 1. Gộp ba nhánh — không có xung đột

```
2b471b1 T67: gộp PR #66 (HUYH-72/T63 bỏ xưng hô)                       — 15 tệp
029ef27 T67: gộp PR #67 (HUYH-73/T64 màn Đợt bỏ tóm tắt và nền vàng)   —  4 tệp
922b5e7 T67: gộp PR #68 (HUYH-76/T66 chi tiết đợt một trang, bộ chọn năm) — 23 tệp
```

Cả ba lần `git merge` đều báo `Merge made by the 'ort' strategy`, `git status` sạch ngay sau đó — **không tệp nào phải giải xung đột bằng tay**.

Danh sách tệp mà `git diff e94f153 HEAD` cho ra **trùng khít** hợp của ba `git diff e94f153 origin/<nhánh>`: bản gộp không thêm, không bớt tệp nào ngoài ba PR.

### Hai ngoại lệ có kiểm soát — git tự gộp đúng

So bản gộp với từng nhánh, phần còn lại đúng bằng phần của việc kia:

| Tệp | So với nhánh T66/T64 thì bản gộp chỉ khác ở |
| --- | --- |
| `EligibilityTab.tsx` | hai câu `Bác thử …` → `Thử …` (phần của T63) |
| `UncoveredPage.tsx` | một câu `Bác thử …` → `Thử …` (phần của T63) |
| `PeriodsPage.tsx` | một câu `Bác thử …` → `Thử …` (phần của T63) |

Ngược lại, so với nhánh T63 thì ba tệp ấy khác đúng phần của T64 và T66. Hai lớp thay đổi cùng nằm trong bản gộp, không lớp nào bị nuốt.

## 2. Kết quả lệnh — chạy nguyên văn trên bản gộp

### Backend

```
> cd BE && dotnet build
    67 Warning(s)
    0 Error(s)
Time Elapsed 00:00:19.22

> cd BE && dotnet test
Passed! - Failed: 0, Passed: 129, Skipped: 1, Total: 130 - HuyHieuDang.Core.QcTests.dll
Passed! - Failed: 0, Passed: 157, Skipped: 0, Total: 157 - HuyHieuDang.Core.UnitTests.dll
Passed! - Failed: 0, Passed:   1, Skipped: 0, Total:   1 - HuyHieuDang.Infrastructure.IntegrationTests.dll
Passed! - Failed: 0, Passed:  78, Skipped: 0, Total:  78 - HuyHieuDang.Infrastructure.UnitTests.dll
Passed! - Failed: 0, Passed: 143, Skipped: 0, Total: 143 - HuyHieuDang.Web.QcIntegrationTests.dll
Passed! - Failed: 0, Passed: 225, Skipped: 0, Total: 225 - HuyHieuDang.Web.IntegrationTests.dll
```

Tổng **733 ĐẠT, 1 bỏ qua, 0 HỎNG** — y hệt mốc đối chứng của lượt 1. Ca bỏ qua là `QC-05 · Core không được đọc đồng hồ hệ thống`, vốn đã bỏ qua từ trước. Ba PR đều chỉ chạm `FE/`, nên con số không xê dịch là đúng.

### Frontend — bốn cổng

```
> cd FE && npm run build
✓ built in 6.46s

> cd FE && npm run lint
(eslint không in gì — không lỗi)

> cd FE && npm run typecheck:e2e
(tsc --noEmit không in gì — không lỗi)

> cd FE && npm run format:check
Checking formatting...
All matched files use Prettier code style!
```

### ui-tests — cổng riêng 4184

```
> cd FE && T50_PORT=4184 npm run test:ui
Running 155 tests using 12 workers
  ...
  155 passed (53.7s)
```

Cổng 4184 đúng như việc yêu cầu. `playwright.ui.config.ts` đặt `reuseExistingServer: false` nên trùng cổng sẽ báo lỗi to chứ không lặng lẽ kiểm nhầm bản build.

155 ca chia theo tệp:

| Tệp | Số ca |
| --- | --- |
| `t50-bon-bang` | 9 |
| `t50-khung-man-hinh` | 3 |
| `t58-quyet-dinh-giao-dien` | 8 |
| `t59-logo` | 9 |
| `t60-responsive` (4 khung nhìn) | 56 |
| `t60-stt-dot` | 1 |
| `t61-cheo-gop-ba-pr` | 8 |
| `t63-khong-xung-ho` | 1 |
| `t64-man-dot` | 7 |
| `t66-bo-chon-nam` | 20 |
| `t67-cheo-luot-2` (**mới**) | 33 |

### E2E docker — mốc T0

```
> docker compose -f e2e/docker-compose.e2e.yml up -d --build
> npx playwright test -c e2e/playwright.e2e.config.ts

  Mốc đang ép : 2026-09-19
  Máy chủ báo : 2026-09-19
  Đồng hồ     : ĐÃ ĐÓNG BĂNG đúng mốc (T-FIX-4 hoạt động)

Running 15 tests using 1 worker
  ok  1 E0-01 · Backend đóng băng "hôm nay" theo HUYHIEUDANG_TEST_TODAY
  ok  2 E0-02 · Ngày máy chủ nằm trong khoảng an toàn của bộ dữ liệu biên
  ok  3 E0-03 · Đồng hồ trình duyệt đóng băng đúng mốc và đúng múi giờ
  ok  4 E2E-1 · Lần dùng đầu tiên: đăng nhập → cài đặt → tạo đợt → import → Dashboard
  ok  5 E2E-2 · Import có lỗi: xem trước đúng, chỉ dòng hợp lệ được nạp
  ok  6 E2E-3 · Đổi Bước từ 5 sang 10 lan truyền ngay sang mọi màn
  ok  7 E2E-4 · Nới Đến ngày của một đợt lan truyền ngay sang mọi màn
  ok  8 E2E-5 · Xuất Excel từ Dashboard, chi tiết đợt và Chưa thuộc đợt nào
  ok  9 E2E-6 · Vòng đời đảng viên: thêm tay → tìm → sửa → xóa nhiều dòng
  ok 10 E2E-7 · Đợt trao huy hiệu vắt qua 31/12 chạy đúng trên mọi màn
  ok 11 QC-T54-01 · Tiêu đề trang chi tiết đợt phải nói rõ Đến ngày thuộc năm sau
  -  12 E-903 · Chạy lại ở mốc T1 = 2026-10-15   (tự bỏ qua, mốc khác)
  -  13 E-904 · Chạy lại ở mốc T2 = 2026-12-01   (tự bỏ qua, mốc khác)
  ok 14 E-905 · Không ca nào dùng waitForTimeout
  ok 15 E-906 · Mỗi luồng tự dựng trạng thái đầu ngay ở dòng đầu tiên

  2 skipped
  13 passed (3.5m)
```

Bảy luồng nghiệp vụ bắt buộc đều xanh trên Backend và PostgreSQL thật.

### E2E docker — mốc T1 và T2

Dựng lại dịch vụ `be` với `E2E_TODAY` tương ứng, rồi chạy **các luồng chạy được ở mốc khác T0**, đúng như `FE/e2e/README.md` mô tả: ra khỏi mốc mặc định thì bộ số của bảy luồng nghiệp vụ được phép đổi, lúc đó thước đo là `E-903` / `E-904`.

```
> E2E_TODAY=2026-10-15 docker compose -f e2e/docker-compose.e2e.yml up -d --force-recreate be
> E2E_TODAY=2026-10-15 npx playwright test -c e2e/playwright.e2e.config.ts e0-tien-de e9-chay-lai

  Mốc đang ép : 2026-10-15   Máy chủ báo : 2026-10-15
  ok 1 E0-01   -  2 E0-02 (tự bỏ qua)   ok 3 E0-03
  ok 4 E-903 · Chạy lại ở mốc T1 = 2026-10-15: Dashboard báo "Đang diễn ra"
  -  5 E-904 (tự bỏ qua, mốc khác)   ok 6 E-905   ok 7 E-906
  2 skipped
  5 passed (2.7s)
```

```
> E2E_TODAY=2026-12-01 docker compose -f e2e/docker-compose.e2e.yml up -d --force-recreate be
> E2E_TODAY=2026-12-01 npx playwright test -c e2e/playwright.e2e.config.ts e0-tien-de e9-chay-lai

  Mốc đang ép : 2026-12-01   Máy chủ báo : 2026-12-01
  ok 1 E0-01   -  2 E0-02 (tự bỏ qua)   ok 3 E0-03
  -  4 E-903 (tự bỏ qua, mốc khác)
  ok 5 E-904 · Chạy lại ở mốc T2 = 2026-12-01: Dashboard nhảy sang Đợt 3/2 năm 2027
  ok 6 E-905   ok 7 E-906
  2 skipped
  5 passed (5.5s)
```

Hai con số 5 đạt / 2 bỏ qua trùng đúng mốc đối chứng trong `docs/test-report/2026-09-21-test-e2e-playwright-ba-moc-thoi-gian.md`.

**Đã chạy thêm cả trọn bộ 15 ca ở mốc T1 để soi**: bốn luồng đỏ, và cả bốn đều là con số theo lịch, không dính gì tới giao diện ba PR sửa — E2E-1 trạng thái đợt `Sắp tới · -14 ngày` thành `Đang diễn ra`; E2E-2 số dòng hợp lệ/lỗi 6/4 thành 7/3 vì dòng "ngày ở tương lai" 20/09/2026 nay đã là quá khứ; E2E-4 đợt sắp tới đổi từ `Đợt 2/9` sang `Đợt 7/11`; E2E-6 tuổi đảng 29 thành 30. Đây đúng là điều `FE/e2e/README.md` nói trước, không tính là lỗi.

Chạy xong đã `docker compose ... down -v`, máy đã trả lại (`docker ps -a --filter name=huyhieudang-e2e` rỗng).

## 3. Ca kiểm chéo mới — `FE/ui-tests/specs/t67-cheo-luot-2.spec.ts`

Vì sao phải có: lượt này có **hai ngoại lệ có kiểm soát**, ba tệp bị hai việc cùng sửa. Ở cả ba chỗ, phần T63 sửa là **câu gợi ý khi bảng trống** — thứ chỉ hiện ra khi tìm không thấy gì hoặc khi năm đó không ai tròn mốc. Bộ ca của T63 quét mã tĩnh nên không bao giờ dựng những câu đó lên màn hình; bộ ca của T64 và T66 không biết câu đó phải đọc ra sao. Nếu phép gộp lấy nhầm bản cũ của một câu, **cả ba bộ ca kia vẫn xanh**.

33 ca, tất cả ĐẠT:

### T63 x T64 x T66 · bản gộp không còn xưng hô

- Quét tĩnh `FE/src` sau khi gộp, gồm cả tệp `.css`, bằng thước đo riêng — ĐẠT
- Ba tệp của hai ngoại lệ có kiểm soát vẫn sạch sau khi gộp — ĐẠT

### T63 x T66 · câu gợi ý trong hai tệp dùng chung, dựng thật lên màn hình

- Chi tiết đợt, tìm không thấy gì: câu gợi ý đọc đúng `Thử xóa bớt chữ trong ô tìm, hoặc chọn lại Mốc: Tất cả.` — ĐẠT
- Chi tiết đợt, năm đó không ai tròn mốc: câu gợi ý đọc đúng `Thử chọn năm khác ở trên, hoặc xem mục "Chưa thuộc đợt nào" để biết ai đang bị sót.` — ĐẠT
- Chưa thuộc đợt nào, tìm không thấy gì: câu gợi ý đúng, không xưng hô — ĐẠT

### T63 x T64 · câu gợi ý của màn Đợt, dựng thật lên màn hình

- Tìm không thấy đợt nào: câu chính và câu gợi ý `Thử xóa bớt chữ trong ô tìm.` đều không xưng hô — ĐẠT
- Chưa cài đợt nào: câu trống đúng, và dưới tiêu đề vẫn không có dòng tóm tắt nào — ĐẠT

### T66 · hai màn dùng cùng một bộ chọn năm

- Hình dáng bộ chọn năm giống nhau từng chi tiết trên hai màn (lớp CSS, nhãn trợ năng, màu nền, bóng, kiểu chữ, số đo) — ĐẠT
- Bảng chọn năm giống nhau trên hai màn; đầu bảng ghi đúng `Chọn trong 1926 – 2126`; lưới 5 cột; "Năm nay" ở góc trái — ĐẠT
- Chi tiết đợt: không còn `Segmented` năm, không còn tab — ĐẠT
- Chưa thuộc đợt nào: không còn `Segmented` năm, không còn tab — ĐẠT
- Màn Đợt trao huy hiệu không có bộ chọn năm — ĐẠT

### T66 · bộ chọn năm đúng kiểu B chủ dự án đã duyệt

- Rãnh xám `--hhd-fill`, ô năm nền trắng không viền có bóng mềm, số năm Noto Serif đậm màu đỏ, hai mũi tên nền trong suốt — ĐẠT (điều kiện "nền trắng" loại kiểu C, "không viền" loại kiểu A)
- Bảng chọn: năm đang xem là viên đỏ đặc gradient chữ trắng; năm máy chủ chữ đỏ, có chấm đỏ tròn 4px dưới số — ĐẠT
- Vừa mở bảng bấm `Enter` rồi bấm `Space` trong cùng một ca: cả hai lần bảng đóng và năm không đổi — ĐẠT (ca từng chập chờn, đã sửa ở `9c82061`)

### T66 · giới hạn năm máy chủ ± 100

- Chi tiết đợt: nút `‹ ›` dừng đúng ở 1926 và 2126 — ĐẠT
- Chi tiết đợt: bảng chọn khóa đúng mọi ô ngoài 1926 – 2126, không khóa nhầm ô trong khoảng — ĐẠT
- Chi tiết đợt: bàn phím trong lưới cũng không vượt được 2126 — ĐẠT
- Chưa thuộc đợt nào: nút `‹ ›` dừng đúng ở 1926 và 2126 — ĐẠT
- Chưa thuộc đợt nào: bảng chọn khóa đúng mọi ô ngoài khoảng — ĐẠT
- Chưa thuộc đợt nào: bàn phím trong lưới cũng không vượt được 2126 — ĐẠT

### T64 · màn Đợt trao huy hiệu trên bản gộp

- Tiêu đề đứng một mình ở mọi trạng thái của màn, không còn chữ "dùng chung cho mọi năm" — ĐẠT
- Không ô nào trong bảng còn nền vàng `--hhd-gold-soft`; không còn lớp `hhd-periods__row--next`; ba nhãn trạng thái vẫn hiện đúng chữ — ĐẠT

Ca này đo theo **mã màu thật** chứ không so ba dòng với nhau như ca của T64: nếu một lần gộp sau này tô vàng cả bảng thì phép so của T64 vẫn xanh, còn ca này đỏ.

### Ảnh soi tay

10 ca chụp ảnh ở 1440×900 và 1280×600: Chi tiết đợt, cận bộ chọn năm, bảng chọn năm mở tại năm khác năm nay, giới hạn trần khóa nút `›`, Chưa thuộc đợt nào, màn Đợt — tất cả ĐẠT.

### Một lỗi của chính ca kiểm thử, đã sửa

Lần chạy đầu, ba ca đỏ vì **ca kiểm thử sai**, không phải mã sản phẩm sai:

1. Màu mũi tên `‹ ›` đọc ra ba giá trị khác nhau ở ba lần đo. Nguyên nhân: `YearPicker.css` cho mũi tên `transition: color 0.15s`, nên ngay sau khi lớp kiểu dáng được nạp, màu đang trên đường đi từ màu mặc định của trình duyệt về `--hhd-text-secondary`; đọc một lần bắt được màu giữa đường. Đã sửa: đọc lại tới khi hai lần liền nhau giống hệt.
2. Tự đổi mã hex của token sang `rgb(...)` bằng tay cho ra `rgb(255, 15, NaN)` với màu viết tắt. Đã sửa: để chính trình duyệt quy đổi.

Sau khi sửa, chạy lại đúng ba ca đó: xanh. Đây là lý do ba ca này không được tính là lỗi sản phẩm.

## 4. Soi tay theo ảnh dựng thử của HUYH-76

| Điểm trong ảnh dựng thử | Trên bản gộp | Kết quả |
| --- | --- | --- |
| Bộ chọn năm **kiểu B**: rãnh xám, ô năm trắng nổi bóng mềm, số đỏ chữ có chân, mũi tên xám | Đúng như vậy; không viền đỏ (kiểu A), không tô đỏ đặc (kiểu C) | ĐẠT |
| Chi tiết đợt là **một trang**: tiêu đề, rồi thẻ danh sách với thanh công cụ `‹ 2026 ›` · khoảng ngày · nhãn ngữ cảnh · số người · Xuất Excel | Đúng bố cục ảnh dựng thử `de-xuat-chi-tiet-dot-1440x900.png` | ĐẠT |
| Bảng chọn năm: "Năm nay" góc trái, "Chọn trong 1926 – 2126" bên phải, lưới 5 cột căn theo bội số của 5 | Đúng | ĐẠT |
| Năm đang xem: viên **đỏ đặc**, số trắng | Đúng (ảnh: 2027 đỏ đặc) | ĐẠT |
| Năm nay khi không phải năm đang xem: **chữ đỏ, chấm đỏ dưới số** | Đúng (ảnh: 2026 chữ đỏ có chấm, ngay cạnh 2027) | ĐẠT |
| Nhãn ngữ cảnh đọc đúng khoảng cách năm | `Năm nay`, `Năm sau`, `12 năm trước`, `100 năm nữa` — đúng bảng ở mục 4 của HUYH-76 | ĐẠT |
| Nút `›` khóa ở năm máy chủ + 100 | Ở 2126 nút `›` mờ, không bấm được | ĐẠT |
| Màn Chưa thuộc đợt nào dùng đúng bộ chọn ấy, đặt đúng chỗ cũ của `Segmented` | Đúng | ĐẠT |
| Khung 1280×600 | Bảng chọn nằm trọn trong khung nhìn, trang không cuộn ngang | ĐẠT |

Ảnh chụp ở `FE/ui-tests/.artifacts/anh-t67/`, 12 tệp, hai khung nhìn.

## 5. Ghi chú phạm vi

- Nền vàng nhạt còn lại trên màn Đợt là **dải độ phủ 12 tháng** (vùng "Khoảng trống") và **nhãn "Sắp tới"** ở cột Trạng thái. HUYH-73 nói rõ hai thứ này giữ nguyên; chỉ nền **dòng bảng** phải bỏ. Ca kiểm chéo vì vậy chỉ đo nền các ô `td` của thân bảng.
- Chữ "Ngày chính thức" còn trên bản gộp là đúng: bản gộp này không gồm HUYH-79 (T69).
- Không sửa mã sản phẩm. Nhánh này chỉ thêm hai tệp: ca kiểm thử mới và báo cáo này.
