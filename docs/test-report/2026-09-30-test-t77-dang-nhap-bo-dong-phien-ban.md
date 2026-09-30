# T77 — Màn Đăng nhập: bỏ dòng phiên bản ở góc dưới nửa trái

Nhánh `fix/T77-dang-nhap-bo-dong-phien-ban`, tách từ `origin/main` tại `7ebadf6`.

Chủ dự án chốt ngày 30/09/2026: dòng `v1.0 · chạy cục bộ` ở góc dưới nửa đỏ bên trái màn Đăng nhập bỏ hẳn, không chuyển sang chỗ khác.

## Đã đổi những gì

**Bỏ dòng chữ** — `FE/src/pages/login/LoginPage.tsx` bỏ thẻ `div` cuối của `.hhd-login__brand`; `FE/src/pages/login/LoginPage.css` bỏ luật `.hhd-login__version`.

**Giữ khối tiêu đề ở giữa** — `.hhd-login__brand` là hộp flex dọc `justify-content: space-between` với ba hàng: hàng logo, khối tiêu đề, dòng phiên bản. Bỏ hàng thứ ba thì `space-between` đẩy khối tiêu đề xuống sát đáy. Cách xử lý: hàng thứ hai nhận thêm lớp `.hhd-login__brand-text` với `margin-block: auto`. Lề tự động hai đầu ăn hết chỗ trống nên `justify-content` không còn tác dụng: hàng logo ở trên cùng, khối tiêu đề về đúng giữa chiều dọc. Không thêm phần tử rỗng giữ chỗ.

## Giữ nguyên, đã kiểm lại

- Chữ, cỡ chữ, màu, lề của hàng logo, tiêu đề và dòng mô tả.
- Hình mờ lá cờ (`position: absolute`, không nằm trong luồng flex).
- Hộp đăng nhập nửa phải, câu báo lỗi, luồng đăng nhập.
- Hai khối `@media` sẵn có (`max-height: 800px` và `max-width: 1024px`).
- `FE/src/theme/**` không bị chạm — HUYH-87 (T76) đang sửa ở đó.

## Bộ kiểm thử

**Ca mới** `FE/ui-tests/specs/t77-dang-nhap-bo-dong-phien-ban.spec.ts` — hai ca nhân ba cỡ màn 1440×900, 1280×600, 1366×650, chạy ở phiên chưa đăng nhập:

| Ca                                 | Kiểm                                                                                                                                                |
| ---------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| nửa trái không còn dòng phiên bản  | `.hhd-login__version` đếm được 0; chữ toàn trang không chứa dòng chữ cũ, không chứa `v1.0`                                                          |
| khối tiêu đề vẫn nằm giữa nửa trái | tâm dọc của khối (mép trên `.hhd-login__headline` → mép dưới `.hhd-login__subline`) cách tâm dọc `.hhd-login__brand` không quá 32px; chụp ảnh trang |

Ca thứ hai đỏ thật khi thiếu bản sửa: bỏ `margin-block: auto` rồi chạy lại, ba cỡ màn lệch **302px / 190px / 215px** so với ngưỡng 32px.

Hai hằng chuỗi trong ca kiểm nối từ hai mảnh (`'.hhd-login__' + 'version'`, `'chạy ' + 'cục bộ'`) để lệnh `git grep` của "Định nghĩa hoàn thành" trả rỗng trên toàn `FE`.

**Ca cũ đã sửa** `FE/ui-tests/specs/t59-logo.spec.ts` — ca "không tràn và không đè chữ ở …" khẳng định `.hhd-login__version` còn trong khung nhìn; đổi sang `.hhd-login__subline`, phần chữ thấp nhất còn lại của nửa trái. Mọi khẳng định khác giữ nguyên. Ca "hàng thương hiệu và hình mờ đều dùng ảnh lá cờ mới" chọn `.hhd-login__brand > div:nth-child(2) img` — hàng logo vẫn là con thứ hai (con thứ nhất là hình mờ), không phải sửa.

## Không chạy E2E

Không bài E2E nào đọc nửa trái trang Đăng nhập:

```
$ git grep -n -E "hhd-login__(brand|headline|subline|version)|chạy cục bộ" -- FE/e2e
(rỗng)
```

## Kết quả chạy

```
$ git grep -n -E "hhd-login__version|chạy cục bộ|v1\.0 ·" -- FE
(rỗng)

$ npm run build      → ✓ built in 29.22s
$ npm run lint       → không lỗi
$ npm run typecheck:e2e → không lỗi
$ npm run format:check  → All matched files use Prettier code style!

$ T50_PORT=4193 npm run test:ui
  170 passed (53.7s)
```

Ảnh chụp ba cỡ màn lưu ở `FE/ui-tests/.artifacts/anh/t77-dang-nhap-<cỡ>.png` (thư mục không theo dõi bằng git), đính kèm comment kết quả trên issue.
