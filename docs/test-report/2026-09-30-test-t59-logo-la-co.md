# Kiểm thử T59 — thay logo cũ bằng ảnh lá cờ

Ngày chạy: 30/09/2026 · Nhánh `feat/T59-logo-la-co` · Tách từ `main` `b57c8c5`

## Việc đã làm

Chủ dự án gửi ảnh hai lá cờ tung bay — cờ Đảng và cờ Tổ quốc — nền trắng, 512×324 px.
Ảnh gốc giữ nguyên tại `docs/logo/co-dang-co-to-quoc-goc.png`. Mọi bản dẫn xuất sinh từ
đúng tệp đó bằng `docs/logo/tao-anh-co.py`, không vẽ lại, không phóng quá 512 px bề ngang.

| Bản | Tệp | Kích thước |
|---|---|---|
| Gốc chủ dự án gửi | `docs/logo/co-dang-co-to-quoc-goc.png` | 512×324, nền trắng |
| Nền trong suốt, cắt sát mép cờ | `FE/src/assets/co-dang-co-to-quoc.png` | 498×276 |
| Biểu tượng tab, đệm thành vuông | `FE/public/co-dang-co-to-quoc.png` | 498×498 |

Có bản nguồn nét hơn thì chỉ việc thay tệp gốc rồi chạy lại `python docs/logo/tao-anh-co.py`.

## Bốn lệnh bắt buộc

| Lệnh | Kết quả |
|---|---|
| `npm run build` | xanh — `✓ built in 4.79s` |
| `npm run lint` | xanh — không báo lỗi nào |
| `npm run typecheck:e2e` | xanh — không báo lỗi nào |
| `npm run format:check` | xanh — `All matched files use Prettier code style!` |

## Ca giao diện `npm run test:ui`

Tệp mới `FE/ui-tests/specs/t59-logo.spec.ts` — 8 ca, chạy riêng đều xanh:

```
Running 8 tests using 4 workers
  8 passed (3.6s)
```

| Ca | Kiểm điều gì |
|---|---|
| thanh đầu trang hiện ảnh lá cờ mới | đúng tệp, đúng câu alt, ảnh tải được thật, cờ rộng hơn cao |
| không còn thẻ ảnh nào trỏ tới logo cũ | trên 4 màn, MỌI thẻ `<img>` đều là tệp cờ mới |
| biểu tượng tab trỏ tới tệp ảnh mới và tải được | `link[rel=icon]` trỏ đúng tệp, máy chủ trả 200 kèm `image/png` |
| thanh đầu trang không tràn ở 1280×600 | không phần tử nào vượt bề ngang; tên hệ thống, ngày hôm nay, nút Đăng xuất còn đủ |
| thanh đầu trang không tràn ở 1366×650 | như trên |
| hàng thương hiệu và hình mờ đều dùng ảnh lá cờ mới | hình mờ có `alt=""` và `aria-hidden`, bề ngang ≤ 498 px |
| trang Đăng nhập không tràn ở 1280×600 | hai nửa đều hiện đủ, không đè chữ |
| trang Đăng nhập không tràn ở 1366×650 | như trên |

## Toàn bộ bộ ca giao diện

Chạy cả `npm run test:ui` ba lần trên nhánh này: **19 xanh / 1 đỏ** (hai lần), **18 xanh / 2 đỏ** (một lần).

Ca đỏ **không thuộc việc này và có sẵn trên `main`**:

- `t50-bon-bang.spec.ts › Đợt trao huy hiệu › phân trang, tìm theo tên đợt, sắp xếp theo Từ ngày` —
  đỏ y hệt trên `main` `b57c8c5` khi chưa có thay đổi nào (`1 failed, 11 passed`). Đây là lỗi phân
  trang mà HUYH-64 (T58) đang sửa.
- `t50-bon-bang.spec.ts › Dashboard › phân trang, tìm, lọc mốc, sắp xếp` — chập chờn, đỏ 1 trong 3
  lần với `locator.click: Test timeout`. Chạy riêng tệp `t50-bon-bang.spec.ts` trên nhánh này cho
  đúng kết quả như `main`: `1 failed, 8 passed`. Cũng thuộc tệp của HUYH-64.

Cả hai đều nằm trong `FE/ui-tests/specs/t50-bon-bang.spec.ts` — tệp việc này không được sửa.

## Không còn dấu vết logo cũ

```
$ git grep -n "logo-huyhieudang"
(rỗng)
```

Hai tệp logo cũ đã xóa: `FE/src/assets/logo-huyhieudang.png` và
`docs/logo/Gemini_Generated_Image_yze7e5yze7e5yze7 (1).png`.

## Kiểm bằng mắt

- Cờ đặt trên nền đỏ của header: **không có viền trắng lem**. Soi mép ở mức phóng 4× thấy mép
  khử răng cưa chuyển thẳng từ đỏ cờ sang nền, không qua một vệt trắng nào.
- Hình mờ trang Đăng nhập trên nền đỏ gradient: sạch, không có mảng trắng mờ phủ quanh cờ.
- Biểu tượng tab ở cỡ 16 px vẫn nhận ra là lá cờ đỏ có ngôi sao và búa liềm.

## Một điểm chưa kiểm được bằng ca tự động

`FE/src/components/PagePlaceholder.tsx` đã đổi sang ảnh mới, nhưng trên `main` `b57c8c5`
component này **không còn màn nào gọi tới** (`grep -rn PagePlaceholder FE/src` chỉ ra chính nó),
nên không có màn nào để ca giao diện mở ra mà xem. Ảnh của nó đi qua cùng component `Logo`
mà ba ca trên đã kiểm.
