# FE — Giao diện Huy Hiệu Đảng

React + Vite + TypeScript + Ant Design 5. Giao diện tiếng Việt cho cán bộ văn phòng
đảng ủy: chữ to, nhãn rõ, thông báo bằng lời thường ngày.

## Chạy

```bash
npm install
cp .env.example .env.development   # sửa VITE_DEV_API_PROXY nếu BE nghe cổng khác
npm run dev                        # http://localhost:5173
npm run build                      # dựng gói tĩnh vào dist/
npm run preview                    # xem thử gói đã dựng, cổng 4173
npm run lint
npm run format
npx playwright install chromium    # lần đầu
npm run test:e2e
```

## Biến môi trường

| Biến                 | Ý nghĩa                        | Mặc định                |
| -------------------- | ------------------------------ | ----------------------- |
| `VITE_API_BASE_URL`  | Địa chỉ gốc của API            | `/api`                  |
| `VITE_DEV_API_PROXY` | BE thật khi chạy `npm run dev` | `http://localhost:8080` |

Chạy bằng Docker thì `VITE_API_BASE_URL=/api` và nginx trong ảnh chuyển tiếp
`/api/` sang service `be` — xem `nginx.conf`.

## Cấu trúc

```
src/
├── api/        Tầng gọi API — nơi DUY NHẤT được dùng axios
│   ├── httpClient.ts   instance axios, gắn thẻ, bóc lớp vỏ, dịch lỗi
│   ├── messages.ts     khóa thông điệp → chữ tiếng Việt
│   ├── token.ts        đọc/ghi thẻ đăng nhập
│   └── auth|members|periods|uncovered|settings|imports|dashboard|exports.ts
├── auth/       Trạng thái đăng nhập dùng chung
├── components/ Thành phần dùng lại (Logo, PageHeading…)
├── layouts/    Khung chung: header + sider 5 mục (thu gọn được)
├── pages/      Một thư mục một màn
├── routes/     Đường dẫn và lớp chặn khi chưa đăng nhập
├── theme/      Token thiết kế — NGUỒN MÀU DUY NHẤT
├── types/      Kiểu dữ liệu nghiệp vụ và kiểu của API
└── utils/      Định dạng ngày, số, tải file
```

## Quy ước

1. **Màu, chữ, bo góc, khoảng cách chỉ lấy từ `src/theme/`.** Không viết mã màu
   trong component. Token trích từ artboard "Màn 0 — Design system nhỏ" trong
   `docs/design-system/Huy Hieu Dang - 9 man hinh.html`; `tokens.ts` và
   `tokens.css` phải luôn khớp nhau.
2. **Component không gọi axios.** ESLint chặn `import axios` ngoài `src/api/`.
3. **Chữ trong bảng tối thiểu 14px.** Đã đặt sẵn trong `antdTheme.ts`.
4. **Ngày `dd/MM/yyyy`, ngày/tháng của đợt `dd/MM`, số dùng dấu chấm ngăn nghìn,
   ô trống hiển thị `—`.** Dùng các hàm trong `src/utils/format.ts`, không tự định dạng.
5. **Ngày hiện tại lấy từ máy chủ** (`serverDate` trong phiên đăng nhập, `today` của
   Dashboard), không lấy từ đồng hồ trình duyệt và không viết cứng.
6. **Mọi bảng phải có đủ ba trạng thái**: đang tải, trống, lỗi. Câu chữ trạng thái
   trống lấy đúng theo thiết kế.
7. Dùng component của Ant Design (Layout, Table, Modal, Steps, Tabs, Statistic,
   Alert, Form, DatePicker, InputNumber, Tag, Empty, Segmented) thay vì tự chế.

## Xử lý lỗi và hết phiên

Mọi lỗi đi qua `src/api/httpClient.ts`, đổi sang câu tiếng Việt trong
`src/api/messages.ts` rồi mới tới màn hình — không màn nào tự dựng chuỗi lỗi.

- **Bảng tải hỏng**: `TableStates` hiện "Chưa tải được danh sách" kèm nút **Thử lại**.
- **Mất mạng / máy chủ chưa lên lúc mở ứng dụng**: giữ nguyên thẻ đăng nhập, hiện
  "Chưa mở được hệ thống" kèm nút **Thử lại**. Không bắt đăng nhập lại.
- **Hết phiên thật (401)**: xóa thẻ, báo một câu rồi về trang Đăng nhập. Riêng 401
  của chính lời gọi đăng nhập là "sai tài khoản hoặc mật khẩu", không phải hết phiên.
- **Badge "Chưa thuộc đợt nào"**: `httpClient` phát sự kiện `hhd:data-changed` sau
  mỗi lời gọi đổi dữ liệu; `AuthProvider` nghe sự kiện đó và lấy lại số thật, nên
  màn hình không phải tự nhớ gọi.

## Hiện trạng

Task T24 nối toàn bộ giao diện vào Backend thật và gỡ hẳn tầng dữ liệu giả
(`src/mocks/` cùng biến `VITE_USE_MOCK` đã bị xóa). Mọi màn đọc ghi qua `src/api/`,
chạy được bằng `docker compose up -d` ở thư mục gốc kho mã.

Tầng gọi API bám theo `docs/api-contract.md` v1: lớp vỏ `{ message, data }` được bóc
trong `httpClient.ts`, khóa thông điệp tra sang tiếng Việt trong `messages.ts`, phân
trang dùng `current` / `pageSize` / `sortQuery` / `filter.<Trường>`, và ngày hôm nay
lấy từ `serverDate` của máy chủ chứ không từ đồng hồ trình duyệt.
