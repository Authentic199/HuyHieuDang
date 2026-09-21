# Ca kiểm thử giao diện (T50)

Phân trang, tìm, sắp xếp, lọc theo mốc và lỗi cắt dòng của **bốn bảng**: Dashboard,
Đợt trao huy hiệu, tab Đủ điều kiện của chi tiết đợt, Chưa thuộc đợt nào.

Chạy trên bản build thật (`npm run preview`) nhưng tầng `/api` bị chặn và thay
bằng bộ dữ liệu giả hơn 300 dòng. Lý do có một bộ riêng thay vì viết thêm vào
`FE/e2e/`: bốn thứ này chạy **trọn vẹn ở máy khách** nên không cần máy chủ, còn
dựng 300 người thật qua giao diện trước mỗi lần chạy thì quá chậm.

`FE/e2e/` vẫn là nơi kiểm nghiệp vụ, trên Backend và PostgreSQL thật.

## Chạy

```bash
cd FE
npm ci
npx playwright install chromium
npm run test:ui
```

Không cần Docker, không cần Backend. Cổng mặc định `4176`, đổi bằng `T50_PORT`.

Ảnh chụp bàn giao rơi vào `ui-tests/.artifacts/anh/`, báo cáo HTML ở
`ui-tests/.artifacts/bao-cao/`.

## Cấu trúc

```
FE/ui-tests/
├── playwright.ui.config.ts   Cấu hình riêng (1440x900, vi-VN, tự dựng preview)
├── fixtures/
│   ├── data.ts    Bộ dữ liệu giả: 320 người đủ điều kiện, 310 người bị sót, 36 đợt
│   ├── app.ts     Chặn /api/**, đặt sẵn thẻ đăng nhập
│   ├── table.ts   Cách đọc một bảng: dòng, ô, thanh phân trang, ô tìm, ô lọc mốc
│   └── format.ts  Số kiểu Việt Nam, giống src/utils/format.ts
└── specs/
    ├── t50-bon-bang.spec.ts       Bốn bảng + trạng thái rỗng-do-lọc
    └── t50-khung-man-hinh.spec.ts Hệ quả của việc chặn chiều cao khung ngoài cùng
```

## Quy ước

- **Mọi con số mong đợi tính ra từ bộ dữ liệu**, không viết cứng — sửa bộ dữ
  liệu thì ca kiểm thử vẫn đúng.
- **Định vị theo `aria-label`, vai trò hoặc lớp `hhd-*`**, không theo chữ in trên
  nút, để màn hình đổi câu chữ thì ca kiểm thử không vỡ.
- Danh sách dài thì Ant Design dùng danh sách ảo: phần tử mang `role="option"`
  nằm trong lớp đo 0x0 bị ẩn, dòng bấm được thật lại không có vai trò nào — định
  vị lựa chọn theo `title` (xem `fixtures/table.ts`).
