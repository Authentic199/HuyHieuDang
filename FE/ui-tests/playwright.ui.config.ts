import { defineConfig, devices } from '@playwright/test';

/**
 * Cấu hình cho các ca giao diện của T50: phân trang, tìm, sắp xếp, lọc theo mốc
 * và lỗi cắt dòng của bốn bảng.
 *
 * Chạy trên bản build thật (`npm run preview`) với tầng `/api` bị chặn và thay
 * bằng bộ dữ liệu giả hơn 300 dòng — xem `fixtures/app.ts`. Khác hẳn
 * `FE/e2e/playwright.e2e.config.ts`: sáu luồng nghiệp vụ ở đó chạy trên Backend
 * và PostgreSQL thật và vẫn là nơi kiểm nghiệp vụ.
 *
 * Chạy: npx playwright test -c ui-tests/playwright.ui.config.ts
 */
const PORT = Number(process.env.T50_PORT ?? 4176);
const BASE_URL = process.env.T50_BASE_URL ?? `http://localhost:${PORT}`;

export default defineConfig({
  testDir: './specs',
  timeout: 60_000,
  expect: { timeout: 10_000 },
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: 0,
  reporter: [['list'], ['html', { open: 'never', outputFolder: './.artifacts/bao-cao' }]],
  outputDir: './.artifacts/ket-qua',
  use: {
    baseURL: BASE_URL,
    locale: 'vi-VN',
    timezoneId: 'Asia/Ho_Chi_Minh',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      // Khớp artboard 1440x900 để ảnh chụp đối chiếu được với thiết kế.
      use: { ...devices['Desktop Chrome'], viewport: { width: 1440, height: 900 } },
    },
  ],
  webServer: {
    command: `npm run preview -- --port ${PORT} --strictPort`,
    url: BASE_URL,
    reuseExistingServer: !process.env.CI,
    timeout: 120_000,
  },
});
