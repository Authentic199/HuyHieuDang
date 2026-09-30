# Kiểm thử T59 — thay logo cũ bằng ảnh lá cờ

Ngày chạy: 30/09/2026 · Nhánh `feat/T59-logo-la-co` · Tách từ `main` `b57c8c5`

Bản này viết lại sau khi CEO trả PR #61 ngày 30/09: cờ **đặt thẳng lên nền đỏ, không lót ô nền trắng**.

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

## Bốn lệnh bắt buộc — nguyên văn đầu ra

```
$ npm run build

> huyhieudang-fe@1.0.0 build
> tsc -b && vite build

dist/assets/table-uiZzDma5.js                                   329.44 kB │ gzip: 101.01 kB
dist/assets/index-BWybeyky.js                                   421.11 kB │ gzip: 137.77 kB
✓ built in 372ms

$ npm run lint

> huyhieudang-fe@1.0.0 lint
> eslint .

[mã thoát: 0]

$ npm run typecheck:e2e

> huyhieudang-fe@1.0.0 typecheck:e2e
> tsc -p tsconfig.e2e.json --noEmit

[mã thoát: 0]

$ npm run format:check

> huyhieudang-fe@1.0.0 format:check
> prettier --check .

Checking formatting...
All matched files use Prettier code style!
[mã thoát: 0]
```

## Ca giao diện — chạy trên cổng riêng 4178

Cổng 4176 mặc định dùng chung với hai việc song song nên kết quả trên cổng đó không được tính.

```
$ T50_PORT=4178 npm run test:ui -- specs/t59-logo.spec.ts

Running 9 tests using 9 workers

  ok 3 [chromium] › ui-tests\specs\t59-logo.spec.ts:170:1 › biểu tượng tab trỏ tới tệp ảnh mới và tải được (841ms)
  ok 2 [chromium] › ui-tests\specs\t59-logo.spec.ts:183:3 › thanh đầu trang không tràn ở 1280x600 (1.1s)
  ok 9 [chromium] › ui-tests\specs\t59-logo.spec.ts:246:3 › trang Đăng nhập › tiêu đề ở 1440x900 ngắt đúng chỗ — mốc để so (1.0s)
  ok 1 [chromium] › ui-tests\specs\t59-logo.spec.ts:136:1 › thanh đầu trang hiện ảnh lá cờ mới (1.2s)
  ok 8 [chromium] › ui-tests\specs\t59-logo.spec.ts:183:3 › thanh đầu trang không tràn ở 1366x650 (1.2s)
  ok 4 [chromium] › ui-tests\specs\t59-logo.spec.ts:201:3 › trang Đăng nhập › hàng thương hiệu và hình mờ đều dùng ảnh lá cờ mới (1.2s)
  ok 5 [chromium] › ui-tests\specs\t59-logo.spec.ts:227:5 › trang Đăng nhập › không tràn và không đè chữ ở 1280x600 (1.2s)
  ok 6 [chromium] › ui-tests\specs\t59-logo.spec.ts:227:5 › trang Đăng nhập › không tràn và không đè chữ ở 1366x650 (1.2s)
  ok 7 [chromium] › ui-tests\specs\t59-logo.spec.ts:151:1 › không còn thẻ ảnh nào trỏ tới logo cũ (1.5s)

  9 passed (2.7s)
```

Cả bộ:

```
$ T50_PORT=4178 npm run test:ui

Running 21 tests using 12 workers
  1) [chromium] › ui-tests\specs\t50-bon-bang.spec.ts:220:3 › Đợt trao huy hiệu › phân trang, tìm theo tên đợt, sắp xếp theo Từ ngày › Đổi trang thì dòng đổi

    Error: expect(locator).not.toHaveText(expected) failed

  1 failed
  20 passed (15.6s)
```

Đúng một ca đỏ, là ca có sẵn trên `main` thuộc HUYH-64.

### Ca mới trong `t59-logo.spec.ts`

| Ca | Kiểm điều gì |
|---|---|
| thanh đầu trang hiện ảnh lá cờ mới | đúng tệp, đúng câu alt, ảnh tải được thật, cờ rộng hơn cao, **không lót ô nền** |
| không còn thẻ ảnh nào trỏ tới logo cũ | trên 4 màn, MỌI thẻ `<img>` đều là tệp cờ mới |
| biểu tượng tab trỏ tới tệp ảnh mới và tải được | `link[rel=icon]` trỏ đúng tệp, máy chủ trả 200 kèm `image/png` |
| thanh đầu trang không tràn ở 1280×600 / 1366×650 | không phần tử nào vượt bề ngang; tên hệ thống, ngày, nút Đăng xuất còn đủ |
| hàng thương hiệu và hình mờ đều dùng ảnh lá cờ mới | **không lót ô nền**, hình mờ có `alt=""` và `aria-hidden`, bề ngang ≤ 498 px |
| trang Đăng nhập không tràn ở 1280×600 / 1366×650 | hai nửa hiện đủ, không đè chữ, **tiêu đề ngắt đúng chỗ** |
| tiêu đề ở 1440×900 ngắt đúng chỗ — mốc để so | chốt chỗ ngắt chuẩn để hai khung nhìn kia so vào |

### Hai khẳng định mới có thật sự bắt lỗi không

Đã thử dựng lại đúng hai lỗi cũ rồi chạy: cả hai đều đỏ đúng chỗ.

```
Error: thanh đầu trang: thẻ cha trực tiếp phải trong suốt, không lót ô nền
Error: hàng thương hiệu trang Đăng nhập: thẻ cha trực tiếp phải trong suốt, không lót ô nền
Error: "Huy hiệu Đảng" phải xuống dòng 2 nguyên cụm, không bị tách đôi   (1280x600)
Error: "Huy hiệu Đảng" phải xuống dòng 2 nguyên cụm, không bị tách đôi   (1366x650)
```

## Ca đỏ của `t50-bon-bang.spec.ts` — không thuộc việc này

Tệp `FE/ui-tests/specs/t50-bon-bang.spec.ts` thuộc HUYH-64, việc này không được sửa. Trong đó có hai kiểu đỏ:

**1. Đỏ chắc chắn, lần nào cũng đỏ** — `Đợt trao huy hiệu › Đổi trang thì dòng đổi`. Dựng lại `main` `b57c8c5` sạch và chạy cùng cổng 4178, một luồng:

```
$ T50_PORT=4178 npm run test:ui -- --workers=1     [trên main b57c8c5 sạch]
Running 12 tests using 1 worker
  1) t50-bon-bang.spec.ts:220:3 › Đợt trao huy hiệu › … › Đổi trang thì dòng đổi
    Error: expect(locator).not.toHaveText(expected) failed
  1 failed
  11 passed (25.9s)
```

**2. Đỏ chập chờn** — bước `Sắp xếp: nhấn lần thứ ba trả về thứ tự máy chủ`, lỗi `locator.click: Test timeout of 60000ms exceeded`. Bước này chạy giống nhau ở cả bốn bảng nên lần nào đỏ cũng nhảy sang bảng khác. **Chạy trên `main` sạch cũng đỏ y hệt**, ba lần liên tiếp cùng cổng 4178 và một luồng:

| Lần | Kết quả trên `main` `b57c8c5` sạch |
|---|---|
| 1 | 2 đỏ — `Chi tiết đợt › Sắp xếp…` (click timeout) + `Đợt trao huy hiệu › Đổi trang…` |
| 2 | 1 đỏ — chỉ `Đợt trao huy hiệu › Đổi trang…` |
| 3 | 3 đỏ — `Sắp xếp…` ở cả Dashboard, Chi tiết đợt và Chưa thuộc đợt nào |

Nguyên văn một lần đỏ của ca Dashboard mà CEO hỏi:

```
1) [chromium] › ui-tests\specs\t50-bon-bang.spec.ts:153:3 › Dashboard — bảng đủ điều kiện ›
   phân trang, tìm, lọc mốc, sắp xếp › Sắp xếp: nhấn lần thứ ba trả về thứ tự máy chủ

   Error: locator.click: Test timeout of 60000ms exceeded.
```

Kết luận: cả hai kiểu đỏ đều có sẵn trên `main`, không do thay đổi của việc này. Máy đang chạy ba việc song song nên bước `Sắp xếp` càng dễ quá hạn.

## Không còn dấu vết logo cũ

Chạy trên toàn nhánh thì lệnh trả về đúng hai dòng, cả hai nằm trong chính báo cáo này — một
dòng là lệnh in ở khối trên, một dòng là câu nhắc tên tệp cũ ở ngay dưới. Lồng nguyên đầu ra đó
vào đây sẽ tự sinh thêm dòng khớp, nên báo cáo chạy bản loại trừ thư mục báo cáo:

```
$ git grep -n "logo-huyhieudang" -- ':!docs/test-report'
(rỗng)
```

Hai tệp logo cũ đã xóa: `FE/src/assets/logo-huyhieudang.png` và
`docs/logo/Gemini_Generated_Image_yze7e5yze7e5yze7 (1).png`. Không còn tệp mã hay tệp
thiết kế nào trỏ tới logo cũ.

## Kiểm bằng mắt

- Header và hàng thương hiệu trang Đăng nhập: cờ **đặt thẳng lên nền đỏ, không có ô trắng**.
  Thân cờ đỏ tươi nổi rõ trên nền đỏ sẫm.
- Hình mờ trang Đăng nhập trên nền đỏ gradient: sạch, không có mảng trắng mờ quanh cờ.
- Tiêu đề trang Đăng nhập ngắt "Hệ thống hỗ trợ xét trao" / "Huy hiệu Đảng" ở cả
  1440×900, 1366×650 và 1280×600.
- Biểu tượng tab ở cỡ 16 px vẫn nhận ra là lá cờ đỏ có ngôi sao và búa liềm.

## Một điểm chưa kiểm được bằng ca tự động

`FE/src/components/PagePlaceholder.tsx` đã đổi sang ảnh mới, nhưng trên `main` `b57c8c5`
component này không còn màn nào gọi tới, nên không có màn nào để ca giao diện mở ra mà xem.
Ảnh của nó đi qua cùng component `Logo` mà các ca trên đã kiểm. CEO đã xác nhận chấp nhận.
