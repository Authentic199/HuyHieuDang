import { defineConfig, devices } from '@playwright/test';

import { BASE_URL } from './fixtures/env';

/**
 * Cấu hình cho sáu luồng end-to-end của T28.
 *
 * Giữ nguyên tinh thần của `FE/playwright.config.ts` (task T00B): khung
 * 1440x900 đúng artboard, ngôn ngữ `vi-VN`, múi giờ `Asia/Ho_Chi_Minh` theo
 * T-FIX-5. Khác ở hai điểm bắt buộc của T28:
 *
 *  - Không tự dựng máy chủ xem trước của Vite: sáu luồng chạy trên hệ thống
 *    thật (nginx + Backend + PostgreSQL) dựng bằng `docker-compose.e2e.yml`,
 *    vì bản xem trước của Vite không có chỗ chuyển tiếp `/api`.
 *  - Chạy tuần tự một luồng một lúc: sáu luồng dùng chung một cơ sở dữ liệu và
 *    mỗi luồng tự dọn kho trước khi chạy, nên chạy song song sẽ giẫm chân nhau.
 *
 * Chạy: xem `FE/e2e/README.md`.
 */
export default defineConfig({
  testDir: './specs',
  // Một luồng nghiệp vụ dài hơn một ca giao diện rời rạc.
  timeout: 180_000,
  expect: { timeout: 15_000 },
  // Sáu luồng dùng chung một kho dữ liệu — phải đi lần lượt.
  fullyParallel: false,
  workers: 1,
  forbidOnly: !!process.env.CI,
  // Không thử lại: một luồng đỏ phải đỏ ngay, không được đợi may rủi lần hai.
  retries: 0,
  reporter: [['list'], ['html', { open: 'never', outputFolder: './.artifacts/bao-cao' }]],
  outputDir: './.artifacts/ket-qua',
  globalSetup: './global-setup.ts',
  use: {
    baseURL: BASE_URL,
    locale: 'vi-VN',
    timezoneId: 'Asia/Ho_Chi_Minh',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'off',
    actionTimeout: 20_000,
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'], viewport: { width: 1440, height: 900 } },
    },
  ],
});
