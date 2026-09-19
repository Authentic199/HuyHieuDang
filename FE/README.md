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
| `VITE_DEV_API_PROXY` | BE thật khi chạy `npm run dev` | `http://localhost:5000` |

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
├── layouts/    Khung chung: header + sider 5 mục
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

## Hiện trạng

Task T00B dựng khung: theme, layout, định tuyến 5 màn, chặn route, trang Đăng nhập,
tầng gọi API, Dockerfile. Nội dung từng màn (bảng, form, wizard import) thuộc các
task sau; hiện mỗi màn chỉ có tiêu đề và khối giữ chỗ.

Tầng gọi API bám theo `docs/api-contract.md` v1: lớp vỏ `{ message, data }` được bóc
trong `httpClient.ts`, khóa thông điệp tra sang tiếng Việt trong `messages.ts`, phân
trang dùng `current` / `pageSize` / `sortQuery` / `filter.<Trường>`, và ngày hôm nay
lấy từ `serverDate` của máy chủ chứ không từ đồng hồ trình duyệt.
