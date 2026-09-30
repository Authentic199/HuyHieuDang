# T66 — Chi tiết đợt một trang, bộ chọn năm ‹ › có bảng chọn năm

Ngày 30/09/2026 · HUYH-76 · nhánh `feat/T66-chi-tiet-dot-mot-trang-bo-chon-nam`
tách từ đầu nhánh PR #64 (`e94f153`).

## Việc đã làm

1. Trang Chi tiết đợt bỏ `Tabs`, gộp thành một trang. `detail/PeriodInfoTab.tsx`
   đã xóa cùng các luật CSS `.hhd-period-info` và `.hhd-period-detail__tabs…`.
   Tiêu đề trang giữ nguyên và là nơi duy nhất còn nói khoảng ngày hằng năm.
2. Component dùng chung `src/components/YearPicker.tsx` (+ `.css`) theo **kiểu B**
   chủ dự án đã duyệt: rãnh xám chứa `‹ [ô năm trắng] ›`, số năm Noto Serif đậm
   màu `--hhd-red`. Hai token bóng mới — `--hhd-shadow-year-cell` và
   `--hhd-shadow-year-cell-active` — thêm vào **cả** `tokens.ts` lẫn `tokens.css`.
3. Bảng chọn năm mở khi bấm ô năm: Popover rộng 370px, đầu bảng có nút "Năm nay"
   và câu "Chọn trong 1926 – 2126", thân là lưới 5 cột cuộn dọc căn theo bội số
   của 5, mở ra đã cuộn sẵn để hàng chứa năm đang xem nằm giữa.

   **Vòng đầu (`a13e5ee`) làm sai chỗ này** và bản báo cáo đầu tiên ghi là đã
   làm, trong khi đo thật thì chưa. CEO bắt được khi gác cổng: hàng của năm đang
   xem luôn rơi vào hàng thứ hai chứ không nằm giữa. Ba nguyên nhân chồng lên
   nhau, đã sửa cả ba ở vòng này:

   - `cell.offsetTop` được đo khi `offsetParent` của ô năm là `.ant-popover-content`
     chứ không phải lưới, nên cộng thừa đệm bảng và dòng đầu "Năm nay" — lưới
     cuộn lố gần một hàng. Nay lưới mang `position: relative` để chính nó là
     `offsetParent`.
   - Tiêu điểm đặt vào ô năm làm trình duyệt kéo ô đó lên đầu vùng cuộn ngay sau
     khi lớp phủ hiện xong, đạp đổ phép căn giữa. Nay tiêu điểm trao cho lưới;
     ô năm chỉ nhận tiêu điểm khi người dùng bấm mũi tên.
   - Hiệu ứng phóng to của lớp phủ làm mọi số đo đổi liên tục trong lúc chạy.
     Nay tắt bằng `transitionName=""`, và vòng căn giữa thử lại từng khung hình
     cho tới khi vị trí cuộn đứng yên.

   Ca `hàng của năm đang xem nằm giữa lưới` trong `t66-bo-chon-nam` canh đúng ba
   chỗ này: nó **đỏ trên `a13e5ee`** (lệch 46px ở năm máy chủ, 96px ở năm máy chủ
   − 37) và **xanh sau khi sửa**. Ca cũ chỉ kiểm `toBeInViewport()` nên không bắt
   được.
4. Nhãn ngữ cảnh ghi đúng khoảng cách: `Năm nay` / `Năm sau` / `Năm trước` /
   `N năm nữa` / `N năm trước`; mọi năm tương lai vẫn ghi "· chuẩn bị trước".
5. Màn Chưa thuộc đợt nào thay `Segmented` bằng cùng `YearPicker`. `yearsAround`
   đã bỏ vì không còn nơi nào dùng.

Giới hạn: `minYear = max(1900, nămMáyChủ − 100)`,
`maxYear = min(2200, nămMáyChủ + 100)` — áp cho cả nút `‹ ›` lẫn bảng chọn.
Năm mặc định vẫn là năm hiện tại của **máy chủ**; chưa biết thì cả bộ chọn khóa.

## Lệnh đã chạy và kết quả

```
$ npm run build
✓ built in 6.94s

$ npm run lint
(không cảnh báo nào)

$ npm run typecheck:e2e
(không lỗi nào)

$ npm run format:check
All matched files use Prettier code style!

$ T50_PORT=4183 npm run test:ui
109 passed (34.0s)
```

Bộ `test:ui` chạy trên **cổng riêng 4183** vì HUYH-72 (4181) và HUYH-73 (4182)
chạy song song trên cùng một máy.

E2E docker không chạy ở việc này theo đúng yêu cầu của issue — QC chạy trọn bộ ở
HUYH-77 trên bản gộp ba PR. Các bài e2e đã sửa để đọc đúng giao diện mới và
`typecheck:e2e` xanh.

## Ca kiểm thử mới — `ui-tests/specs/t66-bo-chon-nam.spec.ts` (17 ca)

| Nhóm | Ca |
|---|---|
| Một trang | không còn `role="tablist"` và không tab nào; tiêu đề có tên đợt + khoảng ngày hằng năm; bảng hiện ngay |
| Bộ chọn năm | mở trang là năm máy chủ; `›` tăng 1 năm, lời gọi `GET /api/Eligibility` mang `year` mới, nhãn "Năm sau", có "chuẩn bị trước" |
| Bộ chọn năm | `‹` hai lần từ năm nay → nhãn "2 năm trước", không còn "chuẩn bị trước" |
| Bộ chọn năm | năm tương lai xa → "6 năm nữa" và vẫn "chuẩn bị trước" |
| Giới hạn | ở năm máy chủ + 100 thì `›` khóa, ô ngoài khoảng trong bảng bị vô hiệu |
| Giới hạn | ở năm máy chủ − 100 thì `‹` khóa, ô ngoài khoảng bị vô hiệu |
| Bảng chọn | mở đúng tại năm đang xem, năm đó được đánh dấu và nằm trong vùng nhìn; chọn năm máy chủ − 37 → đóng bảng, đổi năm, nhãn "37 năm trước", lời gọi mang đúng `year` |
| Bảng chọn | hàng của năm đang xem nằm giữa lưới — đo ở năm máy chủ và ở năm máy chủ − 37, tâm ô lệch tâm lưới dưới nửa hàng |
| Bảng chọn | `Esc` đóng mà không đổi năm, tiêu điểm trả về ô năm |
| Bảng chọn | nút "Năm nay" đưa về năm máy chủ; năm máy chủ có dấu riêng khi không phải năm đang xem |
| Bàn phím | mở bảng, `↓` rồi `Enter` → năm tăng 5 |
| Chưa thuộc đợt nào | `‹ ›` đổi năm và gọi lại `GET /api/Eligibility/Unassigned`; bảng chọn nhảy thẳng tới năm xa; giới hạn y hệt |
| 1280x600 | bảng chọn nằm trọn trong khung nhìn, trang không có thanh cuộn ngang (cả hai màn) |

## Bài kiểm thử cũ đã sửa trong cùng PR

- `e2e/fixtures/app.ts` — `selectYear` chọn năm qua bảng chọn: bấm ô năm rồi bấm
  năm cần tới.
- `e2e/specs/e3`, `e4`, `e5` — bỏ bước bấm tab trong `openEligibilityTab`.
- `e2e/specs/e7` — bỏ `openTab`/`infoTab`, thay bằng `openDetail`/`headerRange`.
  E7-06 đọc "01/12 – 28/02 năm sau, hằng năm" ở **tiêu đề trang**.
- `e2e/scripts/chup-anh-e7.mjs` — ảnh 4 chụp tiêu đề trang
  (`4-tieu-de-chi-tiet-dot.png`) thay cho tab Thông tin.
- `ui-tests/specs/t50`, `t58` — bỏ bước bấm tab, đổi tên nhóm ca.
- `ui-tests/specs/t60` — gộp hai mục "tab Thông tin" và "tab Danh sách đủ điều
  kiện" thành một mục "Chi tiết đợt", đánh lại số thứ tự ảnh.

## Câu chữ

`git diff e94f153...HEAD -- FE/src | grep -iE '^\+.*\b(bác|bạn|anh chị|quý vị|vui lòng|xin mời)\b'`
trả rỗng — không câu mới nào có xưng hô.

## Ảnh bằng chứng

Đính kèm comment của HUYH-76: Chi tiết đợt ở 1440×900, 1366×650 và 1280×600;
bảng chọn năm đang mở khi năm đang xem khác năm nay (thấy cả viên đỏ đặc của năm
đang xem và chấm đỏ của năm máy chủ); `›` bị khóa ở năm máy chủ + 100; màn Chưa
thuộc đợt nào với bộ chọn năm mới.
