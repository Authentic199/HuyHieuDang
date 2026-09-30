# Báo cáo kiểm thử T58 — khôi phục hai quyết định giao diện bị T50 lật ngược

Ngày chạy: 30/09/2026 · Nhánh `fix/T58-khoi-phuc-cum-dem-va-co-trang` (tách từ `main` `b57c8c5`)
Phạm vi: chỉ `FE/`. Không chạm `BE/`, không chạm `CHANGELOG.md`.

## 1. Hai việc phải sửa

| # | Quyết định | Nơi bị lật | Cách khôi phục |
|---|---|---|---|
| 1 | Chân bảng không in cụm đếm `1–20 / 1.342` (chốt ở T49, PR #52) | `useClientTable.ts` dựng lại `showTotal`, nên cụm đếm hiện ở **cả bốn** bảng | Bỏ hẳn `showTotal` khỏi `pagination` của hook |
| 2 | Bảng đủ điều kiện trên Dashboard mở 10 dòng mỗi trang (chốt ở PR #51) | Hook không nhận cỡ trang, luôn dùng `DEFAULT_TABLE_PAGE_SIZE = 20` | Hook nhận thêm `defaultPageSize` (mặc định vẫn 20); `EligibleTableCard` truyền 10 |

Tổng số người vẫn đọc được ở hai chỗ sẵn có: dòng mô tả dưới tiêu đề màn và chân thẻ — đó là lý do cụm đếm ở thanh phân trang là thừa. Ô `… / trang` và các số trang giữ nguyên.

## 2. Ca chặn hồi quy

Tệp mới `FE/ui-tests/specs/t58-quyet-dinh-giao-dien.spec.ts`, 8 ca:

| Ca | Chặn điều gì |
|---|---|
| Bốn ca `<màn>: chân bảng không còn "1–20 / 1.342"` | Cụm đếm không tồn tại ở cả bốn bảng; đồng thời chân bảng vẫn còn ô `… / trang` và số trang |
| `chọn 100 dòng mỗi trang cũng không làm cụm đếm hiện lại` | Đổi cỡ trang không dựng lại cụm đếm |
| `mở ra đúng 10 dòng, ngắn hơn hẳn 20 dòng nên ít phải cuộn` | Cỡ trang mặc định của thẻ Dashboard là 10 |
| `vẫn đổi được sang 20 dòng và nhớ lựa chọn khi đổi trang` | Cỡ trang riêng không làm hỏng ô chọn cỡ trang |
| `ba bảng chiếm trọn màn vẫn giữ 20 dòng mặc định` | Cỡ trang riêng của Dashboard không lan sang ba bảng kia |

### Chứng minh ca kiểm thử thật sự cắn

Tạm dựng lại đúng hai lỗi (thêm lại `showTotal`, bỏ `defaultPageSize`) rồi chạy tệp này: **6/8 ca đỏ**. Khôi phục mã, 8/8 xanh trở lại. Ca kiểm thử không phải thứ trang trí.

## 3. Bộ ui-tests của T50 đang khẳng định chính cái lỗi

`FE/ui-tests/fixtures/table.ts` có `expectTotal(first, last, total)` khẳng định cụm đếm **phải** hiện đúng chữ, và `t50-bon-bang.spec.ts` gọi nó 12 lần. Vì thế lỗi lật ngược quay lại mà bộ kiểm thử vẫn xanh.

Thay bằng ba phương thức nói đúng điều cần nói:

- `expectPage(rowsOnPage, total, pageSize)` — trang đang xem đủ số dòng, và số trang cuối đúng bằng tổng chia cỡ trang. Hai điều đó cùng nói lên tổng vẫn đếm đúng mà không cần in con số nào ra chân bảng.
- `expectNoTotalText()` — cụm đếm không tồn tại.
- `expectPageSize(size)` — ô `… / trang` đang ở cỡ nào.

## 4. Một ca của T50 đang đỏ sẵn trên `main`, đã sửa luôn

`Đợt trao huy hiệu › phân trang, tìm theo tên đợt, sắp xếp theo Từ ngày` đỏ trên `main` `b57c8c5` trước khi đụng gì vào mã — đã chạy để đối chứng. Nguyên nhân: `PERIOD_COLUMN = { name: 0, fromDay: 1 }` viết từ trước khi T49 thêm cột STT, nên cột 0 nay là STT chứ không phải Tên đợt. Sửa thành `{ name: 1, fromDay: 2 }`.

## 5. Bảng Dashboard trong bộ E2E

`e1-lan-dung-dau-tien.spec.ts` (E1-16) và `e5-xuat-excel.spec.ts` (E5-03) khẳng định bảng Dashboard có đủ `list.total` dòng. Ở mọi kịch bản mà hai tệp đó dùng, `list.total` lớn nhất là **6**, nên cỡ trang 10 không làm đỏ. Đã thêm một dòng `expect(list.total).toBeLessThanOrEqual(10)` kèm ghi chú ở cả hai chỗ: bộ dữ liệu nào về sau vượt 10 người thì phải đổi cỡ trang trước khi đếm, thay vì đỏ một cách khó hiểu.

Bộ E2E cần Docker + PostgreSQL thật nên không chạy ở task này — QC chạy trọn ở HUYH-69 trên bản gộp cả ba PR.

## 6. Kết quả lệnh

```
> npm run build
✓ built in 285ms

> npm run lint
(eslint không in gì — không lỗi)

> npm run typecheck:e2e
(tsc --noEmit không in gì — không lỗi)

> npm run test:ui
Running 20 tests using 12 workers
  ...
  20 passed (8.1s)
```

Trong 20 ca đó: 12 ca của T50 (bao gồm ca `Đợt trao huy hiệu` vừa sửa) và 8 ca mới của T58.

## 7. Ảnh chụp

Sinh ra ở `FE/ui-tests/.artifacts/anh/` khi chạy `npm run test:ui`, đính kèm ở comment bàn giao của HUYH-64:

- `t58-dashboard-10-dong.png` — Dashboard, ô `10 / trang`, chân bảng không còn cụm đếm.
- `1-dashboard-du-dieu-kien.png`, `2-dot-trao-huy-hieu.png`, `3-chi-tiet-dot-du-dieu-kien.png`, `4-chua-thuoc-dot-nao.png` — bốn bảng, ô `… / trang` và số trang còn nguyên, không còn `1–20 / 1.342`.

## 8. Ca chập chờn trong `t50-bon-bang.spec.ts` — đã gỡ nguyên nhân

Gác cổng HUYH-67 bắt được bước `Sắp xếp: nhấn lần thứ ba trả về thứ tự máy chủ` thỉnh thoảng hỏng với `locator.click: Test timeout of 60000ms exceeded`, mỗi lần ở một bảng khác. Đã dựng lại được ngay trên nhánh này:

```
$ T50_PORT=4191 npx playwright test -c ui-tests/playwright.ui.config.ts specs/t50-bon-bang.spec.ts --repeat-each=5
    Test timeout of 60000ms exceeded.
    Error: locator.click: Test timeout of 60000ms exceeded.
  1 failed
  44 passed (1.0m)
```

**Nguyên nhân.** Danh sách thả xuống của ô lọc mốc đè lên hàng tiêu đề bảng. Bấm chọn một mục xong, danh sách đóng lại và chuột nằm lại ngay trên một tiêu đề cột có sắp xếp. Ant Design hiện tooltip `Nhấp để sắp xếp tăng dần` đúng chỗ ô lọc mốc, và vì chuột không tự rời đi nên tooltip đó chặn cú bấm tiếp theo. Playwright thử lại suốt 60 giây rồi hết giờ. Tooltip hiện kịp hay không tùy thời điểm — nên ca này lúc đỏ lúc xanh.

**Cách gỡ.** Thêm `leaveTable()` vào `ui-tests/fixtures/table.ts`: đưa chuột về `(0, 0)` rồi chờ `.ant-tooltip:not(.ant-tooltip-hidden)` về 0, gọi sau `goToPage`, `choosePageSize`, `chooseMilestone` và `clickSort`. Không tắt tooltip của bảng, không nới thời gian chờ, không `force: true` — ba cách đó giấu lỗi chứ không gỡ nguyên nhân. Chỉ sửa tệp kiểm thử, mã sản phẩm không đụng tới.

**Bằng chứng sau khi sửa:**

```
$ T50_PORT=4177 npx playwright test -c ui-tests/playwright.ui.config.ts specs/t50-bon-bang.spec.ts --repeat-each=10
  90 passed (53.1s)

$ T50_PORT=4178 npx playwright test -c ui-tests/playwright.ui.config.ts specs/t58-quyet-dinh-giao-dien.spec.ts --repeat-each=10
  80 passed (31.6s)

$ T50_PORT=4177 npm run test:ui
  20 passed (9.9s)

$ npm run build
✓ built in 614ms

$ npm run lint
(eslint không in gì — không lỗi)

$ npm run typecheck:e2e
(tsc --noEmit không in gì — không lỗi)

$ npm run format:check
Checking formatting...
All matched files use Prettier code style!
```

## 9. Còn tồn, không thuộc phạm vi T58

Cột STT của bảng Đợt trao huy hiệu (`FE/src/pages/periods/PeriodsPage.tsx`) dựng số bằng `index + 1` chứ không cộng độ lệch trang, nên trang 2 đánh số lại từ 1. Tệp đó không nằm trong danh sách tệp của việc này nên để nguyên, chỉ ghi lại đây.
