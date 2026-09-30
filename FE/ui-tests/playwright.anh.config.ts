import { defineConfig, devices } from '@playwright/test';

/**
 * Cấu hình riêng để CHỤP ẢNH màn hình làm bằng chứng cho báo cáo kiểm thử.
 *
 * Tách khỏi `playwright.ui.config.ts` vì hai việc khác nhau: bộ kia kiểm hành
 * vi và chạy ở mọi lần kiểm thử, bộ này chỉ chạy khi cần ảnh cho báo cáo. Vẫn
 * dùng chung tầng dữ liệu giả ở `fixtures/app.ts` nên ảnh chụp đúng cái mà các
 * ca kiểm thử đang kiểm.
 *
 * Chạy: T50_PORT=4185 npx playwright test -c ui-tests/playwright.anh.config.ts
 */

const PORT = Number(process.env.T50_PORT ?? 4176);
const BASE_URL = process.env.T50_BASE_URL ?? `http://localhost:${PORT}`;

export default defineConfig({
  testDir: './scripts',
  timeout: 120_000,
  expect: { timeout: 10_000 },
  fullyParallel: false,
  workers: 1,
  reporter: [['list']],
  outputDir: './.artifacts/ket-qua-anh',
  use: {
    baseURL: BASE_URL,
    locale: 'vi-VN',
    timezoneId: 'Asia/Ho_Chi_Minh',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: {
    command: `npm run preview -- --port ${PORT} --strictPort`,
    url: BASE_URL,
    reuseExistingServer: false,
    timeout: 120_000,
  },
});
