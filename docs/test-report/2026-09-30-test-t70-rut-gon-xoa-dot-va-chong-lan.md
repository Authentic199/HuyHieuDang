# T70 — Hộp xóa đợt còn một dòng tiêu đề, cảnh báo chồng lấn còn một câu

Nhánh `fix/T70-rut-gon-xoa-dot-va-chong-lan`, tách từ `origin/main` tại `30a901a`.

Chủ dự án chốt ngày 30/09/2026: hai chỗ này ghi quá nhiều chữ. Chỉ đổi chữ hiển thị — logic, dữ liệu máy chủ trả về và cách lưu giữ nguyên.

## Đã đổi những gì

**Hộp xác nhận xóa đợt** — bỏ hẳn `content`, chỉ còn dòng tiêu đề `Xóa đợt “<tên đợt>”?` kèm biểu tượng chấm than, hai nút **Để lại** · **Xóa đợt này**. Sửa ở cả hai chỗ mở hộp này, để hai nơi giống hệt nhau:

- `FE/src/pages/periods/PeriodsPage.tsx`, hàm `confirmDelete`
- `FE/src/pages/periods/PeriodDetailPage.tsx`, hàm `confirmDelete`

**Cảnh báo chồng lấn** — còn một câu: **Có đợt chồng lấn nhau** — `<tên các đợt>`. Bỏ khoảng ngày, chữ "và", ngoặc đơn và câu hệ quả; không dấu chấm cuối câu.

Máy chủ trả chồng lấn theo **từng cặp**, nên một đợt dính hai cặp sẽ xuất hiện hai lần trong `warnings.overlaps`. Hàm dựng câu vì vậy khử trùng theo `id` chứ không theo tên — hai đợt khác nhau vẫn có thể trùng tên. Thứ tự giữ đúng lần xuất hiện đầu tiên; trong mỗi cặp thì `first…` đứng trước `second…`.

Hàm đặt ở tệp mới `FE/src/utils/overlapWarning.ts`, dùng chung cho hai màn để câu ở đâu cũng giống nhau:

- `FE/src/pages/periods/CoverageWarningBanner.tsx` — banner vàng màn Đợt trao huy hiệu
- `FE/src/pages/dashboard/DashboardAlerts.tsx` — thẻ cảnh báo nhanh ở Dashboard

Ví dụ với ba đợt trong ảnh chủ dự án gửi (Đợt 02/9: 01/06–30/09 · Khu vực Pickleball: 28/09–09/10 · Đợt 07/11: 01/10–30/11), tức hai cặp chồng lấn và đợt giữa dính cả hai:

> **Có đợt chồng lấn nhau** — Trao Huy hiệu Đảng đợt ngày 02/9, Khu vực Pickleball, Trao Huy hiệu Đảng đợt ngày 07/11

## Giữ nguyên, đã kiểm lại

- Dòng **Các đợt chưa phủ kín cả năm** ở cả banner lẫn Dashboard — ca 3 so nguyên văn dòng này.
- Nút **Điều chỉnh →** trên thẻ Dashboard — ca 4 bấm và khẳng định sang được màn Đợt.
- Chú thích khi rê chuột lên vạch sọc chồng lấn của dải độ phủ trong `CoverageStrip.tsx` — chỗ duy nhất còn cho biết ngày cụ thể. Tệp không bị chạm.
- Hộp xác nhận xóa đảng viên ở màn Đảng viên. Không bị chạm.
- `BE/` không bị chạm; `warnings.overlaps` vẫn trả theo cặp như cũ.

## Bộ kiểm thử

**Ca giao diện mới** `FE/ui-tests/specs/t70-rut-gon-xoa-dot-va-chong-lan.spec.ts` — bốn ca:

| Ca | Kiểm |
|---|---|
| 1 | Màn Đợt: hộp xóa đúng tiêu đề, vùng nội dung rỗng, hai nút. Bấm Để lại thì hộp đóng và không có lời gọi `DELETE` nào |
| 2 | Trang Chi tiết đợt: y như ca 1 |
| 3 | Banner màn Đợt với 3 đợt / 2 cặp, đợt giữa dính cả hai: dòng chồng lấn khớp nguyên văn, tên đợt giữa đúng một lần, không còn `28/09` hay `09/10`. Dòng chưa phủ kín giữ nguyên |
| 4 | Dashboard cùng bộ cảnh báo giả: thẻ khớp nguyên văn cùng câu đó; bấm Điều chỉnh → sang màn Đợt |

Dữ liệu giả của ca 3 và 4 dựng ngay trong tệp ca kiểm thử bằng một `page.route` đăng ký **sau** fixture — Playwright ưu tiên route đăng ký sau — và trả `route.fallback()` cho mọi endpoint khác. `FE/ui-tests/fixtures/**` không bị chạm vì các ca khác đang dùng chung.

**E2E bước E4-11** của `FE/e2e/specs/e4-sua-dot-lan-truyen.spec.ts` đang so chuỗi `"<Đợt 2/9> và <Đợt 7/11>"` nên sẽ đỏ với câu mới. Đổi thành so đúng câu mới, thêm một dòng khẳng định banner không còn chữ "nằm trong cả hai đợt". Chỉ sửa các dòng trong bước E4-11.

## Lệnh đã chạy

Bộ ui-tests chạy trên **cổng riêng 4186** của việc này.

```
$ git grep -n -E "xóa khỏi mọi năm|nằm trong cả hai đợt" -- FE/src
exit=1        (rỗng — không còn chỗ nào)

$ npm run build
dist/assets/table-DS4WY5gm.js      329.43 kB │ gzip: 101.01 kB
dist/assets/index-1bQhKKWO.js      422.14 kB │ gzip: 138.15 kB

✓ built in 6.46s

$ npm run lint
npm notice run huyhieudang-fe@1.0.0 lint
npm notice run eslint .
exit=0

$ npm run typecheck:e2e
npm notice run huyhieudang-fe@1.0.0 typecheck:e2e
npm notice run tsc -p tsconfig.e2e.json --noEmit
exit=0

$ npm run format:check
npm notice run huyhieudang-fe@1.0.0 format:check
npm notice run prettier --check .
Checking formatting...
All matched files use Prettier code style!

$ T50_PORT=4186 npx playwright test -c ui-tests/playwright.ui.config.ts ui-tests/specs/t70-rut-gon-xoa-dot-va-chong-lan.spec.ts
Running 4 tests using 4 workers

  ok 1 [chromium] › t70-rut-gon-xoa-dot-va-chong-lan.spec.ts:211:1 › ca 3 · banner màn Đợt nêu mỗi đợt chồng lấn một lần, không khoảng ngày (1.0s)
  ok 3 [chromium] › t70-rut-gon-xoa-dot-va-chong-lan.spec.ts:229:1 › ca 4 · thẻ cảnh báo ở Dashboard dùng đúng câu của màn Đợt (1.0s)
  ok 4 [chromium] › t70-rut-gon-xoa-dot-va-chong-lan.spec.ts:198:1 › ca 2 · hộp xóa đợt ở trang Chi tiết đợt giống hệt màn Đợt (1.7s)
  ok 2 [chromium] › t70-rut-gon-xoa-dot-va-chong-lan.spec.ts:184:1 › ca 1 · hộp xóa đợt ở màn Đợt chỉ còn dòng tiêu đề (1.9s)

  4 passed (4.0s)

$ T50_PORT=4186 npm run test:ui
  102 passed (41.7s)
```

Cả bộ ui-tests là **102 ca**, tăng đúng 4 so với 98 ca trên `main`.

## E2E

Stack e2e dùng chung cả máy (`huyhieudang-e2e`, cổng 4174 / 18080 / 55433). Kết quả lượt chạy này: xem comment cuối của HUYH-80.
