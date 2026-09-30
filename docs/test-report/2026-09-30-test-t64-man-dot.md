# Báo cáo kiểm thử T64 — Màn Đợt trao huy hiệu: bỏ dòng tóm tắt và bỏ nền vàng

- Việc: HUYH-73 (T64)
- Nhánh: `fix/T64-man-dot-bo-tom-tat-va-nen-vang`, tách từ đầu nhánh PR #64 `test/T61-nghiem-thu-dot-30-09` (`e94f153`)
- Ngày chạy: 30/09/2026
- Người chạy: Frontend Developer

## Yêu cầu của chủ dự án

1. Bỏ dòng chữ dưới tiêu đề "Đợt trao huy hiệu" — cả câu `… đợt · dùng chung cho mọi năm, chỉ lưu ngày/tháng` lẫn câu "Đang tải danh sách…" hiện lúc đang tải. Giữ câu lúc tải thì tiêu đề nhảy lên xuống; khung xương 5 dòng của bảng đã báo là đang tải.
2. Bỏ tô nền vàng nhạt các dòng "Đang diễn ra" và "Sắp tới". Mọi dòng nay cùng một nền như ở các bảng khác.

Giữ nguyên: nhãn ở cột "Trạng thái 2026" (kể cả nền vàng nhạt của riêng nhãn "Sắp tới"), chữ đậm ở cột "Đủ điều kiện năm nay", token `--hhd-gold-soft`, tiêu đề, nút "+ Thêm đợt", banner cảnh báo, dải độ phủ 12 tháng và dòng chân bảng.

## Thay đổi mã

| Tệp | Nội dung |
|---|---|
| `FE/src/pages/periods/PeriodsPage.tsx` | Bỏ biến `headingDescription` và prop `description` của `<PageHeading>`; bỏ `totalCount` khỏi phần destructure `usePeriods()`; bỏ prop `rowClassName` cùng dòng chú thích ngay trên nó |
| `FE/src/pages/periods/PeriodsPage.css` | Bỏ luật `.hhd-periods__row--next > td` |
| `FE/ui-tests/specs/t64-man-dot.spec.ts` | Mới — 7 ca chặn hồi quy và chụp ảnh bàn giao |

`usePeriods.ts` không đổi. Phần destructure gom lại thành một dòng là do Prettier, sau khi bỏ `totalCount` thì nó vừa một dòng.

## Ca chặn hồi quy

Viết theo cách của `t58-quyet-dinh-giao-dien.spec.ts`, chạy trên bộ dữ liệu giả 36 đợt có đủ ba trạng thái Đã qua, Đang diễn ra và Sắp tới.

| Ca | Khẳng định |
|---|---|
| tiêu đề đứng một mình | Không còn `.hhd-page-heading__description`, không còn chữ "dùng chung cho mọi năm" và "Đang tải danh sách" |
| lúc đang tải không hiện dòng phụ | Giữ lời gọi `/AwardPeriods` lại, kiểm khung xương hiện mà dưới tiêu đề vẫn trống, và ô bao tiêu đề đứng nguyên chỗ sau khi tải xong |
| ba trạng thái cùng màu nền | Dàn cả 36 đợt lên một trang, đưa chuột ra khỏi bảng, so màu nền thật của ô "Tên đợt" ở dòng Đang diễn ra và Sắp tới với dòng Đã qua |
| không còn lớp trong DOM | `.hhd-periods__row--next` đếm được 0 phần tử |
| ba nhãn trạng thái vẫn đúng chữ | Cột "Trạng thái 2026" vẫn có "Đã qua", "Đang diễn ra" và "Sắp tới · … ngày" |
| ảnh bàn giao 1440x900 và 1366x650 | Lọc còn 8 đợt và sắp theo "Đủ điều kiện năm nay" để ba trạng thái cùng lên bốn dòng đầu, rồi chụp |

## Chứng minh ca bắt được lỗi thật

Ca mới chạy trên mã **chưa sửa** (`git checkout e94f153 -- PeriodsPage.tsx PeriodsPage.css`, build lại, giữ nguyên tệp spec mới):

```
  1) t64-man-dot.spec.ts:67 › tiêu đề đứng một mình, không có dòng phụ nào
     Error: expect(locator).toHaveCount(expected) failed
     Expected: 0
     Received: 1        (.hhd-page-heading__description)

  2) t64-man-dot.spec.ts:76 › lúc đang tải cũng không hiện dòng phụ, nên tiêu đề không nhảy
     Error: expect(locator).toHaveCount(expected) failed
     Expected: 0
     Received: 1

  3) t64-man-dot.spec.ts:105 › ô của ba trạng thái có cùng một màu nền
     Error: dòng "Đang diễn ra" phải cùng nền với dòng "Đã qua"
     Expected: "rgba(0, 0, 0, 0)"
     Received: "rgb(253, 245, 216)"

  4) t64-man-dot.spec.ts:116 › không còn lớp hhd-periods__row--next trong DOM
     Error: expect(locator).toHaveCount(expected) failed
     Expected: 0
     Received: 12

  4 failed
  3 passed (14.3s)
```

Ba ca xanh sẵn là ca nhãn trạng thái và hai ca chụp ảnh — đúng như thiết kế, vì việc này không được làm mất nhãn.

Sau khi sửa, cùng bảy ca đó:

```
  ok t64-man-dot.spec.ts:67  › tiêu đề đứng một mình, không có dòng phụ nào (2.6s)
  ok t64-man-dot.spec.ts:76  › lúc đang tải cũng không hiện dòng phụ, nên tiêu đề không nhảy (1.8s)
  ok t64-man-dot.spec.ts:105 › ô của ba trạng thái có cùng một màu nền (2.7s)
  ok t64-man-dot.spec.ts:116 › không còn lớp hhd-periods__row--next trong DOM (2.2s)
  ok t64-man-dot.spec.ts:123 › ba nhãn trạng thái vẫn hiện đúng chữ (2.2s)
  ok t64-man-dot.spec.ts:155 › chụp màn có đủ ba trạng thái ở 1440x900 (2.0s)
  ok t64-man-dot.spec.ts:155 › chụp màn có đủ ba trạng thái ở 1366x650 (1.8s)
```

## Định nghĩa hoàn thành

```
$ git grep -n -E "dùng chung cho mọi năm, chỉ lưu|hhd-periods__row--next" -- FE/src
(rỗng, mã thoát 1)

$ npm run build          -> exit 0    ✓ built in 384ms
$ npm run lint           -> exit 0    (không báo gì)
$ npm run typecheck:e2e  -> exit 0    (không báo gì)
$ npm run format:check   -> exit 0    All matched files use Prettier code style!

$ T50_PORT=4182 npm run test:ui
  105 passed (38.4s)
  suite exit=0
```

105 ca gồm cả 7 ca mới của `t64-man-dot`. Số ca trước việc này là 98.

## Ảnh bàn giao

- `FE/ui-tests/.artifacts/anh/t64-dot-ba-trang-thai-1440x900.png`
- `FE/ui-tests/.artifacts/anh/t64-dot-ba-trang-thai-1366x650.png`

Cả hai ảnh cho thấy: dưới tiêu đề "Đợt trao huy hiệu" không còn dòng phụ nào, bốn dòng đầu của bảng trải đủ ba trạng thái mà cùng một nền trắng, ba nhãn trạng thái còn nguyên. Ảnh đính kèm comment bàn giao trên HUYH-73.

## Không chạy

E2E docker — không ca E2E nào kiểm dòng tóm tắt hay nền vàng, CEO đã xác nhận bằng `git grep` trên `main`.
