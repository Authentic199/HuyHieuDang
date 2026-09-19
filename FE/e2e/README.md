# Kiểm thử đầu-cuối

Cấu hình Playwright đã sẵn sàng (`playwright.config.ts`), chạy ở khung 1440x900
đúng bằng artboard thiết kế.

Chạy:

```bash
npm run test:e2e          # chạy toàn bộ
npm run test:e2e -- --ui  # chạy có giao diện
```

Lần đầu cần tải trình duyệt: `npx playwright install chromium`.

Các ca kiểm thử được viết cùng từng màn ở task riêng, task T00B chỉ dựng cấu hình.
