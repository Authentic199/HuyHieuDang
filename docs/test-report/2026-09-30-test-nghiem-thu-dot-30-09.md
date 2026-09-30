# Báo cáo nghiệm thu bản gộp ba PR của đợt 30/09 (T58 + T59 + T60)

Ngày chạy: 30/09/2026 · Nhánh `test/T61-nghiem-thu-dot-30-09` (tách từ `main` `b57c8c5`)
Phạm vi: kiểm **bản gộp cả ba**, không kiểm từng PR riêng. Không sửa mã sản phẩm, không sửa `CHANGELOG.md`.

| Việc | Issue | PR | Nhánh |
| --- | --- | --- | --- |
| T58 — khôi phục cụm đếm và cỡ trang Dashboard | HUYH-64 | #60 | `fix/T58-khoi-phuc-cum-dem-va-co-trang` |
| T59 — thay logo bằng ảnh lá cờ | HUYH-67 | #61 | `feat/T59-logo-la-co` |
| T60 — responsive mức 1, màn máy tính nhỏ | HUYH-68 | #62 | `feat/T60-responsive-man-nho` |

## 1. Gộp ba nhánh — không có xung đột

Gộp lần lượt theo thứ tự T58 → T59 → T60, mỗi lần một commit gộp riêng:

```
94049fc merge: T58 (HUYH-64) vào nhánh nghiệm thu T61   — 8 tệp, không xung đột
aeb891b merge: T59 (HUYH-67) vào nhánh nghiệm thu T61   — 16 tệp, không xung đột
aae6760 merge: T60 (HUYH-68) vào nhánh nghiệm thu T61   — 38 tệp, không xung đột
```

Cả ba lần `git merge` đều báo `Merge made by the 'ort' strategy` và `git status` sạch ngay sau đó — **không tệp nào phải giải xung đột bằng tay**. Việc chia tệp từ đầu đã có tác dụng đúng như dự tính.

## 2. Kết quả lệnh — chạy nguyên văn trên bản gộp

### Backend

```
> cd BE && dotnet build
    67 Warning(s)
    0 Error(s)
Time Elapsed 00:00:23.49

> cd BE && dotnet test
Passed! - Failed: 0, Passed: 129, Skipped: 1, Total: 130 - HuyHieuDang.Core.QcTests.dll
Passed! - Failed: 0, Passed: 157, Skipped: 0, Total: 157 - HuyHieuDang.Core.UnitTests.dll
Passed! - Failed: 0, Passed:   1, Skipped: 0, Total:   1 - HuyHieuDang.Infrastructure.IntegrationTests.dll
Passed! - Failed: 0, Passed:  78, Skipped: 0, Total:  78 - HuyHieuDang.Infrastructure.UnitTests.dll
Passed! - Failed: 0, Passed: 143, Skipped: 0, Total: 143 - HuyHieuDang.Web.QcIntegrationTests.dll
Passed! - Failed: 0, Passed: 225, Skipped: 0, Total: 225 - HuyHieuDang.Web.IntegrationTests.dll
```

Tổng **733 ĐẠT, 1 bỏ qua, 0 HỎNG**. Ca bỏ qua là `QC-05 · Core không được đọc đồng hồ hệ thống`, vốn đã bỏ qua từ trước, không liên quan ba PR. Ba PR đều chỉ chạm `FE/` nên kết quả này là mốc đối chứng: bản gộp không làm xê dịch gì ở Backend.

### Frontend — bốn cổng

```
> cd FE && npm run build
✓ built in 6.94s

> cd FE && npm run lint
(eslint không in gì — không lỗi)

> cd FE && npm run typecheck:e2e
(tsc --noEmit không in gì — không lỗi)

> cd FE && npm run format:check
All matched files use Prettier code style!
```

### ui-tests — cổng riêng 4180

```
> cd FE && T50_PORT=4180 npm run test:ui
Running 98 tests using 12 workers
  ...
  98 passed (33.0s)
```

98 ca = 90 ca sẵn có (T50 12, T58 8, T59 9, T60 STT 1, T60 responsive 60 ở bốn khung nhìn) + **8 ca chéo mới** của việc này. Không ca nào đỏ, kể cả ca `t50-bon-bang › Đợt trao huy hiệu › phân trang, tìm theo tên đợt, sắp xếp theo Từ ngày` vốn đỏ trên `main` — T58 đã sửa bản đồ cột.

Cổng 4176 và 4180 đều rảnh trước khi chạy (`netstat` đã kiểm). `playwright.ui.config.ts` trên bản gộp đã đặt `reuseExistingServer: false`, nên trùng cổng sẽ báo lỗi to chứ không lặng lẽ kiểm nhầm bản build — gốc rủi ro CEO bắt được lúc 11:25 đã được bịt.

### E2E docker trọn bộ ở mốc T0

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
  13 passed (3.0m)
```

Bảy luồng nghiệp vụ bắt buộc đều xanh trên Backend và PostgreSQL thật. Hai ca bỏ qua là hai mốc thời gian khác, tự bỏ qua theo thiết kế của bộ E2E.

## 3. Kiểm độc lập từng yêu cầu

### T58 — ĐẠT

| Yêu cầu | Cách kiểm | Kết quả |
| --- | --- | --- |
| Chân cả bốn bảng không còn cụm `1–20 / …` | 4 ca `t58` + khẳng định chân bảng không khớp `\d[–-]\d+\s*/\s*\d` | ĐẠT |
| Ô `… / trang` và số trang vẫn còn | `expectPageSize` + số trang đầu là `1`; ảnh chụp chân bảng | ĐẠT |
| Dashboard mặc định **10** dòng | `toHaveCount(10)` + ô chọn hiện `10 / trang`, 32 trang cho 320 người | ĐẠT |
| Ba bảng còn lại mặc định **20** | ca `ba bảng chiếm trọn màn vẫn giữ 20 dòng mặc định` | ĐẠT |

Đọc mã đối chứng: `useClientTable.ts` không còn `showTotal` (chỉ còn ghi chú giải thích vì sao cố ý bỏ), `EligibleTableCard.tsx` truyền `defaultPageSize: DASHBOARD_PAGE_SIZE = 10`, mặc định chung vẫn `DEFAULT_TABLE_PAGE_SIZE = 20`.

### T59 — ĐẠT

| Yêu cầu | Cách kiểm | Kết quả |
| --- | --- | --- |
| Header dùng ảnh lá cờ | Ca `t59` + ảnh chụp: cờ đặt thẳng trên nền đỏ, **không ô trắng** | ĐẠT |
| Trang Đăng nhập — hàng thương hiệu và hình mờ | Ca `t59` (đúng tệp, ảnh tải được, không lớp lót) + ảnh chụp | ĐẠT |
| Hình mờ không có viền trắng | Soi ảnh chụp ở 1280×600 và 1440×900 trên nền đỏ sẫm | ĐẠT |
| Cờ trên header đỏ vẫn nhìn rõ | Soi ảnh chụp header ở cả bốn khung nhìn | ĐẠT |
| Placeholder | `PagePlaceholder.tsx` dùng `<Logo size={64} />` | ĐẠT |
| Favicon | `index.html` trỏ `/co-dang-co-to-quoc.png`; tệp có thật, **498×498 RGBA** (đã đệm vuông) | ĐẠT |
| `git grep -n "logo-huyhieudang"` rỗng | Chỉ còn 2 dòng, cả hai trong `docs/test-report/2026-09-30-test-t59-logo-la-co.md` | ĐẠT |

Không phóng quá ảnh gốc: nguồn `docs/logo/co-dang-co-to-quoc-goc.png` 512×324, bản dùng trong ứng dụng 498×276 — nhỏ hơn bản gốc. Hai tệp logo cũ đã xóa khỏi cây mã; bộ thiết kế không còn chuỗi `assets/logo-huyhieudang.png`.

### T60 — ĐẠT

| Yêu cầu | Cách kiểm | Kết quả |
| --- | --- | --- |
| 1280×600 và 1366×650: không bảng nào mất dòng | 11 màn × 2 khung, sàn `min(8, số dòng của trang)` | ĐẠT |
| Không tràn ngang | Khẳng định `scrollWidth <= innerWidth` trước và sau khi cuộn | ĐẠT |
| Tới được thanh phân trang | `scrollIntoViewIfNeeded` rồi `toBeInViewport` | ĐẠT |
| Modal thêm/sửa vừa màn | 2 modal × 4 khung nhìn, nút Lưu tới được | ĐẠT |
| 1440×900 và 3440×1440 giống `main` | Đối chiếu ảnh từng điểm ảnh — xem mục 4 | ĐẠT |
| Sàn cỡ chữ màn nhỏ | ≥13px chữ thân và bảng, ≥12px chữ phụ, nút và ô nhập ≥32px | ĐẠT |
| STT nối tiếp qua trang ở **cả năm bảng** | Ca `t60-stt-dot` + đọc mã cả năm bảng | ĐẠT |

Ca sàn cỡ chữ khẳng định đúng quyết định ngày 30/09 (người dùng trẻ): màn thấp dùng 13px, còn ở khung nhìn cao vẫn khóa cứng cỡ cũ 14px, nút 40px, ô tìm 48px. Không đánh trượt theo câu "tối thiểu 14px" của kế hoạch cũ.

STT nối tiếp — đọc mã cả năm bảng, mỗi bảng đều cộng phần bù của trang:

```
EligibleTableCard.tsx:89   table.indexOffset + index + 1
MembersPage.tsx:184        rowOffset + index + 1
PeriodsPage.tsx:149        table.indexOffset + index + 1   ← chỗ T60 vừa sửa
EligibilityTab.tsx:93      table.indexOffset + index + 1
UncoveredPage.tsx:94       table.indexOffset + index + 1
```

Bản đồ cột trong `t50-bon-bang.spec.ts` đã khớp thứ tự cột thật (`{ name: 1, fromDay: 2 }` sau khi có cột STT) — ca so **tên đợt**, không so STT.

### Kiểm chéo — 8 ca mới, tệp `FE/ui-tests/specs/t61-cheo-gop-ba-pr.spec.ts` — ĐẠT

Ba PR chia tệp nên từng PR xanh khi chạy riêng cũng không nói gì về chỗ chúng gặp nhau. Hai chỗ gặp nhau thật:

**T58 × T60 — cỡ trang 10 sống trong khung responsive.** `t58` chỉ chạy ở khung mặc định 1440×900; `t60-responsive` lấy sàn 8 dòng nên không biết bảng này có đúng 10 dòng. Nếu thang gọn kéo cỡ trang về 20 hoặc nuốt mất thanh phân trang thì **cả hai bộ ca kia vẫn xanh**. Ca mới khẳng định ở cả bốn khung nhìn: đúng 10 dòng, ô chọn hiện `10 / trang`, chân bảng vẫn không có cụm đếm, **cả mười dòng đều tới được**, thanh phân trang tới được, cửa sổ không cuộn ngang.

**T59 × T60 — cờ mới trong header đã bị thu thấp.** T60 hạ `--hhd-header-height` từ 56px xuống 48px ở khung nhìn thấp, còn T59 đặt vào đó ảnh cờ cao 28px, rộng hơn hẳn logo vuông cũ. T59 có kiểm header ở 1280×600, nhưng trên nhánh của nó header vẫn cao 56px — **chưa ai từng thấy cờ trong header 48px**. Ca mới khẳng định: header đúng 48px ở khung thấp và 56px ở khung cao, ảnh cờ tải được thật, cờ nằm trọn trong header (không thò ra ngoài nền đỏ), ba cụm chữ bên phải không bị đè, header không tràn ngang, nút Đăng xuất vẫn trong khung nhìn.

Cả 8 ca ĐẠT ở cả bốn khung nhìn.

## 4. Chứng minh T60 không đụng màn lớn — đối chiếu từng điểm ảnh

"Giống hệt `main`" là khẳng định kiểm được, nên đã kiểm thay vì tin lời. Cách làm: dựng một cây thứ hai `qc/T61-base-t58-t59` = `main` + T58 + T59 (**không có T60**), bê nguyên hạ tầng kiểm thử của T60 sang (chỉ tệp test, không đụng mã sản phẩm), build, chạy cùng bộ ca trên cổng riêng 4181, rồi so ảnh từng điểm ảnh với bản gộp. Cách cô lập này đo đúng phần T60 gây ra, vì T58 và T59 cố ý đổi giao diện ở mọi cỡ màn.

Đối chứng đầu tiên — **bộ ca của T60 thật sự cắn**: trên cây không có T60, 17 ca đỏ, trong đó 15 ca ở hai khung nhìn thấp (đúng lỗi T60 sinh ra để chữa) và 2 ca sàn cỡ chữ. Ở hai khung nhìn cao thì mọi ca "không mất nội dung" vẫn xanh trên cả hai cây — khớp với "màn lớn không đổi".

Kết quả so ảnh 13 màn × 2 khung nhìn cao:

| Khung nhìn | Giống hệt | Khác ≤ 0,09% điểm ảnh | Khác nhiều |
| --- | --- | --- | --- |
| 1440×900 | 4 màn | 9 màn | 0 |
| 3440×1440 | 6 màn | 7 màn | 0 |

Ba chỗ lệch đáng nhìn đã truy tới cùng, **không chỗ nào là lỗi bố cục**:

- `13-modal-them-dot.png` ở 3440×1440 lệch 93% điểm ảnh — nhưng lệch lớn nhất chỉ **19/255** và **không một điểm nào lệch quá 40**: đó là một lớp phủ mờ đều khắp ảnh, tức lớp nền mờ của modal bắt đúng lúc đang tan. Ảnh động, không phải bố cục.
- `13-modal-them-dot.png` ở 1440×900 và `12-modal-them-dang-vien.png` ở 3440×1440 trông như modal bị dịch ngang. Đã đo trực tiếp thay vì đoán: mở modal, chờ hết hoạt ảnh rồi đọc `boundingBox` trên **cả hai cây** — `{"x":480,"y":100,"width":480,"height":417}`, **giống nhau từng pixel**. Lệch trong ảnh là do chụp giữa hoạt ảnh phóng của Ant Design.
- `7-chi-tiet-dot-thong-tin.png` lệch một vùng 152×43 px ở góc trên phải, đúng hai nút `Sửa đợt` và `Xóa`. Phóng to soi: hai nút **trùng khít vị trí và kích thước**, chỉ khác khử răng cưa viền — 5 điểm ảnh lệch quá 40 trên cả ảnh, lệch trung bình 0,02/255.

Kết luận: ở 1440×900 và 3440×1440, bản gộp **không có thay đổi bố cục nào** so với cây không có T60. Thang gọn và lớp cuộn vùng nội dung bị chặn đúng trong media query, không rò sang màn lớn.

## 5. Nhận xét gửi CEO — không chặn phát hành

1. **Sàn 8 dòng không áp cho khung 1440×900.** `t60-responsive.spec.ts` đặt `MIN_VISIBLE_ROWS_DESIGN_FRAME = 4` riêng cho khung nhìn cao đúng 900 px, trong khi yêu cầu HUYH-68 viết "không bảng nào được co dưới 8 dòng". Hai yêu cầu của chính issue đó đá nhau: giữ nguyên dáng 1440×900 thì phải chấp nhận 4 dòng, vì trên `main` hôm nay thân bảng Dashboard cuộn trong thẻ và cũng chỉ hiện 4 dòng. T60 chọn giữ nguyên màn lớn — theo tôi là chọn đúng, nhưng con số 8 trong mô tả issue nên được sửa lại cho khỏi hiểu nhầm về sau.
2. **Favicon ở 16 px.** Bản 498×498 đã đệm vuông đúng yêu cầu. Phóng to xem ở đúng cỡ 16 px thì đọc ra dải đỏ với hai vệt vàng — nhận ra là cờ, nhưng ngôi sao và búa liềm nhòe hẳn. Nếu chủ dự án gửi bản nét hơn thì nên sinh lại; chưa đủ để chặn.
3. **Một ghi chú trong mã còn xưng hô với người dùng.** `EligibleTableCard.tsx:57` viết "10 dòng vừa đúng một màn nên bác không phải cuộn". HUYH-71 đang gỡ lối xưng hô này khỏi tài liệu; dòng ghi chú trong mã cũng nên gỡ theo. Không ảnh hưởng người dùng.

## 6. Kết luận

| PR | Kết luận | Lý do |
| --- | --- | --- |
| #60 — T58 (HUYH-64) | **ĐẠT** | Cụm đếm đã bỏ ở cả bốn bảng, Dashboard đúng 10 dòng, ba bảng kia vẫn 20; ca chặn hồi quy có thật và cắn được |
| #61 — T59 (HUYH-67) | **ĐẠT** | Cờ thay logo ở cả năm nơi, đặt thẳng trên nền đỏ không ô trắng, hình mờ không viền trắng, không còn tham chiếu logo cũ |
| #62 — T60 (HUYH-68) | **ĐẠT** | Bốn khung nhìn đều không mất nội dung và không tràn ngang; màn lớn chứng minh được là không đổi; STT nối tiếp ở cả năm bảng |

Gộp ba nhánh **không có xung đột**. Thứ tự merge khuyến nghị: **T58 (#60) → T59 (#61) → T60 (#62)** — đúng thứ tự đã nghiệm thu ở đây, và cũng là thứ tự phụ thuộc: T60 dọn `.ant-pagination-total-text` mà T58 làm cho thành thừa, nên T58 phải vào trước.
