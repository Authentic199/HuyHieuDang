# T78 — Màn Đợt trao huy hiệu: dải độ phủ tô theo trạng thái, đổi tên hai cột, bỏ dòng chân bảng

Nhánh `fix/T78-man-dot-mau-dai-va-ten-cot`, tách từ `origin/main` tại `7ebadf6`.

Chủ dự án chốt ngày 30/09/2026 trong chat với CEO. Chỉ đổi giao diện — máy chủ, dữ liệu, thứ tự sắp xếp và QT11 giữ nguyên.

## Đã đổi những gì

### 1. Dải độ phủ tô theo trạng thái

Trước đây dải tô ngược nghĩa: đợt **đã qua** màu đỏ, đợt **đang diễn ra** lại vàng giống hệt đợt **sắp tới**. Giờ mỗi bar mang đúng màu của nhãn tương ứng ở cột Trạng thái.

| Trạng thái | Class | Màu |
|---|---|---|
| Đã qua | `.hhd-coverage__bar--past` | Nền `#ecedf0`, viền 1px `#9aa0a8`, chữ `#4e5158`. Không còn bóng đỏ |
| Đang diễn ra | `.hhd-coverage__bar--ongoing` | Gradient đỏ `#bd2a1c → #991a14 → #6c0f12`, chữ trắng |
| Sắp tới | `.hhd-coverage__bar--upcoming` | Gradient vàng `#ffcd00 → #c99a10 → #8a6a1e`, chữ `#3d2c00` |

Ba mã màu của bar "Đã qua" chép từ `statusBadge.past` trong `FE/src/theme/tokens.ts` — tệp đó chỉ đọc, không sửa, vì HUYH-87 (T76) đang làm việc trên `FE/src/theme/`. Chú thích đầu `CoverageStrip.css` ghi rõ nguồn.

Mọi bar **giữ class gốc `.hhd-coverage__bar`** — bài e2e `e7` đếm bar bằng class này. Class `.hhd-coverage__bar--next` đã bỏ hẳn.

Khoảng trống giữ nguyên màu cát `#f0e2c2`, không viền; bar "Đã qua" có viền và tên đợt nên không lẫn với nó. Vùng chồng lấn, vạch Hôm nay, quầng sáng, tooltip và cách ẩn chữ ở bar hẹp đều giữ nguyên.

**Cách tính trạng thái của từng bar.** So khoảng ngày của chính đoạn đó với `today` máy chủ trả, bằng đúng phép so của QT11: `toDate < today` → Đã qua; `fromDate ≤ today ≤ toDate` → Đang diễn ra; `fromDate > today` → Sắp tới. `today` rỗng, sai, hoặc không thuộc năm đang vẽ thì lấy `status` của đợt trong `periods`.

Đợt không vắt năm chỉ có một đoạn, nên màu bar luôn trùng nhãn ở cột Trạng thái.

Đợt vắt qua 31/12 có hai đoạn trong năm, mỗi đoạn tô theo khoảng ngày riêng. Ví dụ đợt 01/12 – 28/02 với hôm nay 30/09/2026: đoạn 01/01 – 28/02 là phần cuối của lần diễn ra năm trước, đã hết → xám; đoạn 01/12 – 31/12 → vàng, trùng nhãn "Sắp tới · 62 ngày" trong bảng. Lý do: mọi thứ bên trái vạch Hôm nay đều đã qua, tô đoạn tháng 1–2 màu vàng "sắp tới" chính là nghịch lý chủ dự án vừa chỉ ra.

**Hệ quả chấp nhận, CEO đã chốt:** từ 01/01 tới hết đoạn đầu năm, đoạn đó tô đỏ vì lần diễn ra năm trước đang chạy thật, trong khi cột Trạng thái vẫn ghi "Sắp tới" — QT11 xét lần diễn ra neo năm nay. Không sửa QT11, không sửa máy chủ. Ca 5 và ca 6 dưới đây khóa đúng hành vi này.

**Chú giải** bỏ mục "Đợt", giờ đọc theo thứ tự: `Đã qua` · `Đang diễn ra` · `Sắp tới` · `Khoảng trống` · `Chồng lấn` (chỉ khi có chồng lấn) · `Hôm nay`. Ô màu ba mục đầu dùng đúng màu ba loại bar; `.hhd-coverage__swatch--period` đã bỏ. Sáu mục nằm trên một hàng ở cả 1440×900 lẫn 1280×600 — mục chú giải đặt `flex: none` và `white-space: nowrap`, khoảng cách giữa các mục rút từ 16px xuống 14px để có chỗ cho hai mục mới.

### 2. Đổi tên hai cột

Trong `FE/src/pages/periods/PeriodsPage.tsx`: `Trạng thái ${year}` → `Trạng thái`, `Đủ điều kiện năm nay` → `Đủ điều kiện`. Chỉ đổi chữ tiêu đề — độ rộng cột, cách sắp xếp và nội dung ô giữ nguyên. Số người vẫn là số đủ điều kiện trong năm hiện tại của máy chủ. Biến `year` vẫn dùng cho dải độ phủ.

### 3. Bỏ dòng chú thích chân bảng

Bỏ khối `.hhd-periods__footnote` ("Sắp theo Từ ngày. Sửa hoặc xóa đợt có hiệu lực ngay cho mọi năm.") trong `PeriodsPage.tsx`, bỏ luật CSS tương ứng, và bỏ phần `:not(.hhd-periods__footnote)` khỏi bộ chọn `.hhd-periods__panel > div:not(…)`. Thanh phân trang vẫn nằm sát đáy thẻ — xem ảnh bàn giao.

## Giữ nguyên, đã kiểm lại

- Tiêu đề "Độ phủ trong năm" và `aria-label` `Độ phủ trong năm <năm>` — bài e2e `e1` đọc nhãn này.
- Màu nhãn ở cột Trạng thái (`PeriodStatusTag.tsx` không bị chạm).
- `FE/src/theme/**` không bị chạm.
- `FE/ui-tests/fixtures/**` không bị chạm — dữ liệu giả của ca mới dựng ngay trong tệp spec.
- `BE/`, hợp đồng API, QT11 và thứ tự sắp xếp mặc định không bị chạm.

## Bộ kiểm thử

**Ca mới** `FE/ui-tests/specs/t78-man-dot-mau-dai-va-ten-cot.spec.ts` — 15 ca.

Dữ liệu giả dựng ngay trong tệp bằng `app.route('**/api/AwardPeriods', …)` đăng ký sau fixture. Bốn đợt năm 2026 đúng theo ảnh chủ dự án: 01/03 – 31/05, 01/06 – 30/09, 01/10 – 30/11, 01/12 – 28/02 (vắt năm). Năm đoạn đợt, trong đó đợt vắt năm góp hai đoạn. `status` và `daysRemaining` của từng đợt **tính ra** theo QT11 từ `today` của từng kịch bản, không viết cứng.

| # | Ca | Kiểm |
|---|---|---|
| 1 | Tiêu đề bảy cột | Chữ `th` đúng bằng `STT, Tên đợt, Từ ngày, Đến ngày, Trạng thái, Đủ điều kiện, Thao tác`; không ô nào chứa số năm hay chữ "năm nay"; bấm "Trạng thái" và "Đủ điều kiện" vẫn sắp được |
| 2 | Chân bảng | Không còn `.hhd-periods__footnote`; chữ cả trang không chứa "Sắp theo Từ ngày" và "hiệu lực ngay cho mọi năm" |
| 3 | Màu dải, `today` = 30/09/2026 | Đủ năm bar, đều mang `.hhd-coverage__bar`; từ trái sang phải: past, past, ongoing, upcoming, upcoming; ba đợt không vắt năm có class bar trùng nhãn dòng cùng tên; không còn `.hhd-coverage__bar--next`. Cũng là ca biên: 30/09 đúng bằng Đến ngày của đợt 01/06 – 30/09, bar đó phải là ongoing |
| 4 | Màu thật bằng `getComputedStyle` | past: nền `rgb(236, 237, 240)`, chữ `rgb(78, 81, 88)`, viền 1px `rgb(154, 160, 168)`; ongoing: `background-image` chứa `rgb(153, 26, 20)`, chữ `rgb(255, 255, 255)`; upcoming: `background-image` chứa `rgb(255, 205, 0)`, chữ `rgb(61, 44, 0)` |
| 5 | `today` = 01/10/2026 | Bar 01/06 – 30/09 thành past, bar 01/10 – 30/11 thành ongoing |
| 6 | `today` = 15/01/2026 | Đoạn 01/01 – 28/02 là ongoing, đoạn 01/12 – 31/12 là upcoming |
| 7 | `today` = 15/12/2026 | Đoạn 01/01 – 28/02 là past, đoạn 01/12 – 31/12 là ongoing |
| 8 | Chú giải, không chồng lấn | Đúng năm mục theo thứ tự `Đã qua, Đang diễn ra, Sắp tới, Khoảng trống, Hôm nay` |
| 9 | Chú giải, có chồng lấn | Mục "Chồng lấn" nằm giữa "Khoảng trống" và "Hôm nay" |
| 10 | Ô màu chú giải | Ba mục đầu trùng `background-color` và `background-image` với ba loại bar; không còn `.hhd-coverage__swatch--period` |
| 11–12 | Chú giải một hàng | Ở 1440×900 và 1280×600, sáu mục cùng một tọa độ y |
| 13–15 | Ảnh bàn giao | Màn Đợt `today` = 30/09 ở 1440×900 và 1280×600; riêng dải độ phủ ở kịch bản `today` = 15/01 |

**Ca cũ sửa trong cùng PR** — `FE/ui-tests/specs/t64-man-dot.spec.ts`: `SHOT_SORT_COLUMN` đổi từ `'Đủ điều kiện năm nay'` thành `'Đủ điều kiện'`, ba chú thích ghi `"Trạng thái <năm>"` / `"Đủ điều kiện năm nay"` đổi theo. Không đổi khẳng định nào.

## Kết quả chạy

```
git grep -n -E "hhd-coverage__bar--next|hhd-coverage__swatch--period|hhd-periods__footnote|Sắp theo Từ ngày|hiệu lực ngay cho mọi năm|Đủ điều kiện năm nay" -- FE

FE/ui-tests/specs/t78-man-dot-mau-dai-va-ten-cot.spec.ts:14: * 2. Cột "Trạng thái <năm>" còn "Trạng thái", cột "Đủ điều kiện năm nay" còn
FE/ui-tests/specs/t78-man-dot-mau-dai-va-ten-cot.spec.ts:16: * 3. Không còn dòng "Sắp theo Từ ngày…" dưới chân bảng.
FE/ui-tests/specs/t78-man-dot-mau-dai-va-ten-cot.spec.ts:304:    await expect(app.locator('.hhd-periods__footnote')).toHaveCount(0);
FE/ui-tests/specs/t78-man-dot-mau-dai-va-ten-cot.spec.ts:306:    expect(pageText).not.toContain('Sắp theo Từ ngày');
FE/ui-tests/specs/t78-man-dot-mau-dai-va-ten-cot.spec.ts:307:    expect(pageText).not.toContain('hiệu lực ngay cho mọi năm');
FE/ui-tests/specs/t78-man-dot-mau-dai-va-ten-cot.spec.ts:340:    await expect(app.locator('.hhd-coverage__bar--next')).toHaveCount(0);
FE/ui-tests/specs/t78-man-dot-mau-dai-va-ten-cot.spec.ts:427:    await expect(app.locator('.hhd-coverage__swatch--period')).toHaveCount(0);
```

Cả 7 dòng đều thuộc `FE/ui-tests/specs/t78-man-dot-mau-dai-va-ten-cot.spec.ts` và đều là khẳng định "không còn" mà mục Kiểm thử của HUYH-89 bắt buộc phải có, cùng hai dòng chú thích đầu tệp mô tả chính những thứ đã bỏ. Mô tả HUYH-89 tự mâu thuẫn giữa grep 1 và mục Kiểm thử; CEO chốt ngày 30/09/2026 là theo mục Kiểm thử, giữ nguyên bảy dòng này.

Loại tệp spec đó ra thì lệnh trả rỗng — tức mã nguồn và các bộ ca khác không còn chỗ nào dùng thật:

```
git grep -n -E "hhd-coverage__bar--next|hhd-coverage__swatch--period|hhd-periods__footnote|Sắp theo Từ ngày|hiệu lực ngay cho mọi năm|Đủ điều kiện năm nay" -- FE ':!FE/ui-tests/specs/t78-man-dot-mau-dai-va-ten-cot.spec.ts'
→ rỗng
```

`git grep -n -F 'Trạng thái ${year}' -- FE/src` → rỗng.

`git grep -n -E "hhd-coverage__(bar--|swatch|legend)|hhd-periods__footnote|Sắp theo Từ ngày|Đủ điều kiện năm nay|Trạng thái 20" -- FE/e2e` → rỗng.

```
npm run build       → ✓ built in 7.45s
npm run lint        → không lỗi
npm run typecheck:e2e → không lỗi
npm run format:check  → All matched files use Prettier code style!
```

```
T50_PORT=4197 npm run test:ui
  179 passed (46.3s)
```

Trong đó 15 ca mới của t78 và toàn bộ t64 đã sửa đều xanh.

## Không chạy E2E

HUYH-87 (T76) đang cần stack docker `huyhieudang-e2e` cho bài `e7`, nên việc này không dựng stack đó. Bằng chứng thay thế: lượt `git grep` trên `FE/e2e` ở trên trả rỗng — không bài e2e nào đọc tên hai cột, dòng chú thích chân bảng hay class màu của bar. Bài `e1` đọc `aria-label` của dải và chip Hôm nay, hai thứ này không đổi. Bài `e7` đếm `.hhd-coverage__bar`, ca 3 đã khóa class gốc còn nguyên trên mọi bar.
