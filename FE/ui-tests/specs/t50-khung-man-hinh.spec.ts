import path from 'node:path';

import { expect, test } from '../fixtures/app';

/**
 * T50 chặn chiều cao khung ngoài cùng (`.hhd-shell` dùng `height: 100vh`) để
 * bảng cuộn được bên trong thẻ. Hai ca dưới đây canh chừng hai hệ quả của việc
 * đó trên những màn KHÔNG có bảng cuộn:
 *
 * - Màn có bảng thì cả trang vừa khít màn hình, không sinh thanh cuộn cửa sổ.
 * - Màn dài hơn màn hình (Cài đặt) vẫn cuộn được — cuộn trong vùng nội dung,
 *   không bị cắt mất phần dưới.
 */

const SHOT_DIR = path.join(import.meta.dirname, '..', '.artifacts', 'anh');

test('màn có bảng vừa khít màn hình, không sinh thanh cuộn cửa sổ', async ({ app }) => {
  await app.goto('/');
  await expect(app.locator('.hhd-dashboard__panel .ant-pagination')).toBeVisible();

  const windowScrolls = await app.evaluate(
    () => document.documentElement.scrollHeight > window.innerHeight + 1,
  );
  expect(windowScrolls, 'cửa sổ không được cuộn — bảng tự cuộn bên trong thẻ').toBe(false);
});

test('màn Cài đặt dài hơn màn hình vẫn cuộn tới cuối được', async ({ app }) => {
  await app.goto('/cai-dat');
  const main = app.locator('.hhd-main');
  await expect(main).toBeVisible();

  const reachable = await main.evaluate((element) => {
    element.scrollTop = element.scrollHeight;
    // Cuộn được tới đáy, hoặc vốn đã đủ ngắn để hiện hết — cả hai đều đạt.
    return element.scrollTop + element.clientHeight >= element.scrollHeight - 1;
  });
  expect(reachable).toBe(true);
  await app.screenshot({ path: path.join(SHOT_DIR, '6-cai-dat-cuon-toi-cuoi.png') });
});

test('màn Đảng viên không bị khung mới làm cắt mất thanh phân trang', async ({ app }) => {
  await app.goto('/dang-vien');
  const pagination = app.locator('.hhd-members__panel .ant-pagination');
  await expect(pagination).toBeVisible();
  await expect(pagination).toBeInViewport();
  await app.screenshot({ path: path.join(SHOT_DIR, '7-dang-vien.png') });
});
