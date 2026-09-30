# T60 — Responsive mức 1: màn máy tính nhỏ không còn mất nội dung

Nhánh `feat/T60-responsive-man-nho`, tách từ `main` tại `b57c8c5`.

Việc cần trả lời: trên laptop 1366×768 (khung nhìn trình duyệt khoảng 1366×650) bảng "Danh sách đủ điều kiện" chỉ còn hàng tiêu đề cột rồi tới ngay thanh phân trang — không dòng nào hiện được. Cùng cơ chế áp cho mọi màn có bảng.

## Cách chữa

Hai lớp, theo đúng thứ tự "gọn trước, cuộn sau":

1. **Thang gọn.** Khung nhìn thấp hơn 900 px thì cỡ chữ, chiều cao nút và khoảng đệm lùi một nấc, nên màn nhỏ giữ đúng dáng của màn lớn, chỉ nhỏ hơn một cỡ. Làm bằng biến `--hhd-fs-*` / `--hhd-lh-*` trong `FE/src/theme/tokens.css` cộng `@media` trong `FE/src/theme/responsive.css`, và bản token Ant Design `compactAntdTheme` nạp qua một `ConfigProvider` lồng trong `AppLayout`. Không dùng `zoom` hay `transform: scale`.
2. **Cuộn vùng nội dung.** Gọn rồi vẫn thiếu chỗ thì vùng `.hhd-main` cuộn như một trang thường; header và menu trái đứng yên vì nằm ngoài vùng đó. `min-height: 100%` giữ nguyên dáng cũ khi còn đủ chỗ, nên cuộn chỉ là phần bù.

Sàn đã chốt và đã kiểm bằng máy: chữ thân và chữ trong bảng ≥ 13 px, chữ phụ ≥ 12 px, nút và ô nhập cao ≥ 32 px.

Màn hẹp dưới 1440 px: bảng nào rộng hơn thẻ thì cuộn ngang **trong thẻ** theo `--hhd-table-min-width` của từng màn; menu trái tự thu về dải biểu tượng, nút thu gọn có sẵn vẫn mở lại được.

## Lệnh đã chạy

```
npm run build          ✓ built in 5.67s
npm run lint           ✓ không cảnh báo
npm run typecheck:e2e  ✓ không lỗi
npm run format:check   ✓ All matched files use Prettier code style!
npm run test:ui        71 đạt · 1 hỏng (24.0s)
```

Ca hỏng duy nhất là `t50-bon-bang.spec.ts › Đợt trao huy hiệu › phân trang, tìm theo tên đợt, sắp xếp theo Từ ngày` — **hỏng sẵn trên `main`**, không do việc này. Đã kiểm chứng bằng cách khôi phục `FE/src` và `FE/ui-tests` về đúng `b57c8c5` rồi chạy lại một mình ca đó: vẫn hỏng. Nguyên nhân là bản đồ cột trong chính tệp spec (`PERIOD_COLUMN = { name: 0 }`) chưa cập nhật sau khi cột STT được thêm ở `5bd95a2`, nên ca so nhầm ô STT với ô Tên đợt. Tệp đó thuộc HUYH-64 nên việc này không sửa.

## Ca mới `t60-responsive`

Chạy ở bốn khung nhìn, khai báo trong `FE/ui-tests/playwright.ui.config.ts`.

| Khung nhìn | Thang gọn | Ca | Kết quả |
|---|---|---|---|
| 1280×600 | bật | 15 | 15 đạt |
| 1366×650 | bật | 15 | 15 đạt |
| 1440×900 | tắt | 15 | 15 đạt |
| 3440×1440 | tắt | 15 | 15 đạt |

Mỗi khung chạy 11 màn (Dashboard, Đảng viên, Import bước 1–2–3, Đợt trao huy hiệu, Chi tiết đợt hai tab, Chưa thuộc đợt nào, Cài đặt, trang 404), hai modal (Thêm đảng viên, Thêm đợt), một ca thang gọn và một ca menu trái.

Với mỗi màn có bảng, ca khẳng định:

- số dòng **nhìn thấy được** ≥ min(8, số dòng của trang) — phép đếm trừ cả phần bị mọi lớp `overflow` cắt, không chỉ dựa vào khung nhìn như `toBeInViewport`;
- thanh phân trang tới được: cuộn tới rồi nằm trong khung nhìn;
- trang không có thanh cuộn ngang, cả trước và sau khi cuộn.

Riêng 1440×900 và 3440×1440 khẳng định thêm hành vi cũ: cửa sổ không cuộn, thanh phân trang nằm trong khung nhìn ngay không cần cuộn.

## Một điểm phải báo: sàn 8 dòng chỉ đặt cho màn thấp

Khi ca chạy lần đầu, bốn màn **ở 1440×900** không đạt sàn 8 dòng: Dashboard 4/20, Đợt trao huy hiệu 4/20, Chưa thuộc đợt nào 6/20, Chi tiết đợt — Danh sách đủ điều kiện 7/20. Đó là bố cục **đang có trên `main`**, chưa đụng tới: thẻ đợt sắp tới, dải độ phủ và khối gợi ý chiếm chỗ nên thân bảng vốn chỉ cao chừng ấy.

Ép 8 dòng ở 1440×900 nghĩa là đổi khung của bộ thiết kế — trái yêu cầu 1 ("màn cao giữ nguyên như hiện nay"). Nên ca đặt sàn 8 dòng cho 1280×600 và 1366×650, còn hai khung cao chỉ khẳng định không mất dòng nào và phân trang thấy ngay. Nếu chủ dự án muốn 8 dòng ở cả màn cao thì đó là một việc khác: phải rút gọn các khối phía trên bảng.

## Bằng chứng ảnh

`FE/ui-tests/.artifacts/anh-t60/<khung nhìn>/` — 13 ảnh mỗi khung nhìn, sinh lại mỗi lần chạy `npm run test:ui`.

## Để biết

- Ô trạng thái trống của bảng Dashboard còn một dòng chữ 22 px viết cứng trong `EligibleTableCard.tsx` — tệp thuộc HUYH-64 nên không đổi được sang thang biến. Dòng đó chỉ hiện khi danh sách rỗng.
- Header và trang Đăng nhập viết cỡ chữ bằng số chứ không qua biến nên thang gọn không kéo theo. Chiều cao header thu 56 → 48 px qua biến trong `theme/**` đúng như việc này cho phép.
