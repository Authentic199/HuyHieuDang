# Báo cáo kiểm thử T76 — Bỏ khoảng ngày hằng năm ở tiêu đề Chi tiết đợt và dòng "Chọn trong …" ở bảng chọn năm

- Issue: HUYH-87 (T76)
- Nhánh: `fix/T76-bo-khoang-ngay-va-chon-trong`
- Điểm xuất phát: `origin/main` = `7ebadf6` (đã có #71 và #72)
- Ngày chạy: 30/09/2026

## Đã đổi những gì

Chỉ bỏ chữ hiển thị. Logic, giới hạn năm và dữ liệu máy chủ giữ nguyên.

1. **Tiêu đề trang Chi tiết đợt chỉ còn tên đợt.** Bỏ hẳn `.hhd-period-detail__range`,
   cả dạng "… năm sau, hằng năm" của đợt vắt qua 31/12. Từ nay khoảng ngày của đợt
   chỉ xuất hiện ở thanh công cụ của thẻ danh sách (`.hhd-eligibility__dates`), nơi
   nó luôn đã gắn năm — kể cả đợt vắt năm (QT6).
2. **Bảng chọn năm bỏ dòng "Chọn trong … – …".** Đầu bảng chỉ còn nút "Năm nay" neo
   ở góc trái. Giới hạn năm máy chủ ± 100 kẹp trong 1900 – 2200 giữ nguyên: nút `‹ ›`
   vẫn khóa ở hai đầu, ô ngoài khoảng vẫn mờ và không bấm được. Bộ chọn năm dùng
   chung nên màn **Chưa thuộc đợt nào** cũng mất dòng đó — đúng ý: cả ứng dụng chỉ
   có một kiểu bộ chọn năm.
3. **Dọn phần không còn dùng:** luật `.hhd-period-detail__range`,
   luật `.hhd-year-panel__range`, và biến `--hhd-lh-range` ở `tokens.css` và
   `responsive.css` (chỉ luật vừa xóa dùng biến này).

## Quét tĩnh — không còn dấu vết trong mã nguồn

```
$ git grep -n -E "hhd-period-detail__range|hhd-year-panel__range|Chọn trong|lh-range" -- FE/src
(exit 1 — không có dòng nào)
```

## Bốn cổng dựng và kiểm kiểu

```
$ npm run build
✓ built in 7.55s
exit=0

$ npm run lint
exit=0   (eslint . — không cảnh báo)

$ npm run typecheck:e2e
exit=0   (tsc -p tsconfig.e2e.json --noEmit)

$ npm run format:check
Checking formatting...
All matched files use Prettier code style!
exit=0
```

## ui-tests — cổng riêng 4188

```
$ T50_PORT=4188 npm run test:ui
  170 passed (57.3s)
```

Chạy riêng ba tệp liên quan để đọc rõ từng ca:

```
$ T50_PORT=4188 npx playwright test -c ui-tests/playwright.ui.config.ts \
    ui-tests/specs/t76-bo-khoang-ngay-va-chon-trong.spec.ts \
    ui-tests/specs/t66-bo-chon-nam.spec.ts \
    ui-tests/specs/t67-cheo-luot-2.spec.ts
  59 passed (19.3s)
```

Sáu ca mới của t76:

| Ca | Kết quả |
|---|---|
| Tiêu đề đúng bằng tên đợt, không còn khoảng ngày hằng năm | ok |
| Khoảng ngày của đợt vẫn có, ở thanh công cụ và đã gắn năm | ok |
| Chi tiết đợt: đầu bảng chỉ còn nút "Năm nay" ở nửa trái | ok |
| Chưa thuộc đợt nào: đầu bảng chỉ còn nút "Năm nay" ở nửa trái | ok |
| Chi tiết đợt: giới hạn năm máy chủ ± 100 giữ nguyên sau khi bỏ chữ | ok |
| Chưa thuộc đợt nào: giới hạn năm máy chủ ± 100 giữ nguyên sau khi bỏ chữ | ok |

Ca cũ đã sửa trong cùng PR nên không bộ nào đỏ sau khi merge:

- `t66-bo-chon-nam.spec.ts`: ca tiêu đề đổi thành "tiêu đề chỉ có tên đợt"; hai dòng
  `toContainText('Chọn trong …')` đổi thành khẳng định **không** còn chữ đó, giữ
  nguyên các khẳng định khóa ô.
- `t67-cheo-luot-2.spec.ts`: `yearPanelShape` bỏ `range`, `rangeText`,
  `todayLeftOfRange`; thay bằng `headChildren` và `todayInLeftHalf`. Bỏ hai khẳng
  định "Chọn trong". Mọi khẳng định khác giữ nguyên.

## E2E `e7` trên stack docker

Trước khi dựng đã kiểm stack dùng chung — không có container nào đang chạy, nên
dựng mới:

```
$ docker ps -a --filter name=huyhieudang-e2e
(không có container nào)

$ docker compose -f e2e/docker-compose.e2e.yml up -d --build
exit=0

$ docker ps --filter name=huyhieudang-e2e
huyhieudang-e2e-fe        Up (healthy)
huyhieudang-e2e-be        Up
huyhieudang-e2e-postgres  Up (healthy)

$ npx playwright test -c e2e/playwright.e2e.config.ts e2e/specs/e7-dot-vat-qua-nam.spec.ts

───── Môi trường kiểm thử end-to-end (T28) ─────
  Giao diện      : http://localhost:4174
  API            : http://localhost:4174/api
  Mốc đang ép    : 2026-09-19
  Máy chủ báo    : 2026-09-19
  Đồng hồ        : ĐÃ ĐÓNG BĂNG đúng mốc (T-FIX-4 hoạt động)
────────────────────────────────────────────────

Running 2 tests using 1 worker

  ok 1 [chromium] › e2e\specs\e7-dot-vat-qua-nam.spec.ts:170:1 › E2E-7 · Đợt trao huy hiệu vắt qua 31/12 chạy đúng trên mọi màn (33.1s)
  ok 2 [chromium] › e2e\specs\e7-dot-vat-qua-nam.spec.ts:420:1 › QC-T54-01 · Khoảng ngày của đợt vắt năm luôn có năm, không nơi nào nói thiếu (2.0s)

  2 passed (37.0s)
```

Đã `down` stack sau khi chạy xong để trả máy.

Ba chỗ đã sửa trong `e7`:

- **E7-06** — tiêu đề chỉ còn tên đợt; `.hhd-eligibility__dates` đọc
  `01/12/<năm> – 28/02/<năm + 1>`.
- **E7-10** — đợt sau khi sửa về không vắt năm: đọc khoảng ngày ở thanh công cụ,
  `01/10/<năm> – 07/11/<năm>`, không có năm sau.
- **QC-T54-01** — giữ nguyên mã ca, đổi ý ca: không chỗ nào trên trang nói khoảng
  ngày của đợt vắt năm mà thiếu năm. Tiêu đề không có khoảng ngày; thanh công cụ
  gắn đúng hai năm. Hàm `headerRange` đã bỏ vì không còn chỗ dùng.

`e2e/scripts/chup-anh-e7.mjs` — ảnh 4 chờ `.hhd-eligibility__dates` thay cho
`.hhd-period-detail__range`. Giữ nguyên tên tệp ảnh `4-tieu-de-chi-tiet-dot.png`
vì báo cáo cũ còn trỏ tới.

## Không xung đột với PR còn mở

Lúc kiểm có ba PR mở, đã kiểm cả ba:

```
$ git merge-tree --write-tree --name-only HEAD origin/docs/T72-ghi-chu-hop-dong
1bc3b8f7cd560e5d01efc0173d75bf4058bff7e5      (exit 0 — không xung đột)

$ git merge-tree --write-tree --name-only HEAD origin/fix/T78-man-dot-mau-dai-va-ten-cot
3f10a8b12761fe02693de8e558c57ab9f78dd085      (exit 0 — không xung đột)

$ git merge-tree --write-tree --name-only HEAD origin/fix/T77-dang-nhap-bo-dong-phien-ban
de29f8ec65b627845c7e167c15c3f73822611c02      (exit 0 — không xung đột)
```

#71 và #72 đã vào `main` trước khi nhánh này rebase, nên không còn commit gộp nào.

## Phạm vi thay đổi

```
$ git diff --stat origin/main..HEAD
 FE/e2e/scripts/chup-anh-e7.mjs                     |   7 +-
 FE/e2e/specs/e7-dot-vat-qua-nam.spec.ts            |  58 ++++++++----
 FE/src/components/YearPicker.css                   |  11 +--
 FE/src/components/YearPicker.tsx                   |  10 +-
 FE/src/pages/periods/PeriodDetailPage.tsx          |   8 +-
 FE/src/pages/periods/detail/PeriodDetail.css       |   9 --
 FE/src/pages/periods/detail/PeriodDetailHeader.tsx |  20 +---
 FE/src/theme/responsive.css                        |   1 -
 FE/src/theme/tokens.css                            |   1 -
 FE/ui-tests/specs/t66-bo-chon-nam.spec.ts          |  16 ++--
 FE/ui-tests/specs/t67-cheo-luot-2.spec.ts          |  20 ++--
 .../specs/t76-bo-khoang-ngay-va-chon-trong.spec.ts | 102 +++++++++++++++++++++
 12 files changed, 179 insertions(+), 84 deletions(-)
```

Đúng danh sách "Tệp thuộc việc này", cộng chính báo cáo này.

`FE/src/pages/periods/PeriodDetailPage.tsx` chỉ đổi khối chú thích ở dòng 17–24;
`confirmDelete` của #71 và các dòng quanh nó không bị chạm. Không đụng
`EligibilityTab.tsx` và thanh công cụ của thẻ danh sách, để PR của T74 gộp được.

## Ảnh chụp 1440×900

Ba ảnh đính kèm comment kết quả trên HUYH-87:

1. Chi tiết đợt — tiêu đề chỉ còn tên đợt, khoảng ngày `01/01/2026 – 06/01/2026`
   nằm ở thanh công cụ.
2. Bảng chọn năm đang mở ở Chi tiết đợt — đầu bảng chỉ còn "Năm nay" ở góc trái.
3. Bảng chọn năm đang mở ở Chưa thuộc đợt nào — giống hệt, không còn "Chọn trong".
