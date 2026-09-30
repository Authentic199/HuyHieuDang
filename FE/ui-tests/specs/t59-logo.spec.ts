import path from 'node:path';

import { test as anonymous, type Locator, type Page } from '@playwright/test';

import { expect, test } from '../fixtures/app';

/**
 * T59 — ảnh lá cờ thay cho logo cũ ở mọi nơi.
 *
 * Kiểm ba điều: đúng tệp ảnh mới được dùng, ảnh tải được thật (không phải thẻ
 * <img> hỏng), và cờ rộng hơn logo vuông cũ không làm header hay trang Đăng
 * nhập tràn ở những khung nhìn thấp hơn artboard.
 */

const SHOT_DIR = path.join(import.meta.dirname, '..', '.artifacts', 'anh');

/** Tên tệp ảnh mới; Vite gắn thêm chuỗi băm nên chỉ so khớp phần tên. */
const FLAG_FILE = /co-dang-co-to-quoc[^/]*\.png$/;

/** Khung nhìn phải gọn gàng ngoài artboard 1440x900. */
const SMALL_VIEWPORTS = [
  { width: 1280, height: 600 },
  { width: 1366, height: 650 },
] as const;

/**
 * Chờ tới khi ảnh tải xong và có kích thước thật — thẻ <img> hỏng sẽ trả 0.
 * Phải chờ chứ không hỏi một lần: lúc phần tử vừa hiện ra thì trình duyệt
 * thường còn đang tải tệp ảnh.
 */
async function expectLoaded(image: Locator, what: string): Promise<void> {
  await expect
    .poll(
      () =>
        image.evaluate((element: HTMLImageElement) => element.complete && element.naturalWidth > 0),
      { message: `${what} phải tải được` },
    )
    .toBe(true);
}

/** Không có phần tử nào tràn khỏi bề ngang của khung nhìn. */
async function overflowsHorizontally(page: Page, selector: string): Promise<boolean> {
  return page.evaluate((sel) => {
    const root = document.querySelector(sel);
    if (!root) throw new Error(`Không thấy ${sel}`);
    const limit = document.documentElement.clientWidth;
    for (const element of [root, ...root.querySelectorAll('*')]) {
      const box = element.getBoundingClientRect();
      if (box.width > 0 && (box.right > limit + 1 || box.left < -1)) return true;
    }
    return false;
  }, selector);
}

/** Trang Đăng nhập cần phiên chưa đăng nhập, nên không dùng fixture `app`. */
async function gotoLogin(page: Page): Promise<void> {
  await page.route('**/api/**', (route) =>
    route.fulfill({
      status: 401,
      contentType: 'application/json',
      body: JSON.stringify({ message: 'Chưa đăng nhập', data: null }),
    }),
  );
  await page.goto('/dang-nhap');
  await expect(page.locator('.hhd-login__card')).toBeVisible();
}

test('thanh đầu trang hiện ảnh lá cờ mới', async ({ app }) => {
  await app.goto('/');
  const flag = app.locator('.hhd-header img');
  await expect(flag).toHaveCount(1);
  await expect(flag).toHaveAttribute('src', FLAG_FILE);
  await expect(flag).toHaveAttribute('alt', 'Cờ Đảng và cờ Tổ quốc');
  await expectLoaded(flag, 'ảnh cờ trên thanh đầu trang');

  // Cờ nằm ngang: rộng hơn cao. Logo cũ vuông nên đây là thứ phân biệt chắc chắn.
  const box = await flag.boundingBox();
  expect(box).not.toBeNull();
  expect(box!.width).toBeGreaterThan(box!.height);
});

test('không còn thẻ ảnh nào trỏ tới logo cũ', async ({ app }) => {
  // Nói ngược lại cho chắc: mọi thẻ <img> của ứng dụng đều phải là tệp ảnh cờ
  // mới. Như vậy thì bất cứ ảnh cũ nào còn sót cũng lộ ra, kể cả ảnh đổi tên.
  for (const route of ['/', '/dang-vien', '/dot-trao-huy-hieu', '/cai-dat']) {
    await app.goto(route);
    await expect(app.locator('.hhd-header img')).toHaveCount(1);
    const sources = await app
      .locator('img')
      .evaluateAll((nodes) =>
        nodes.map((node) => (node as HTMLImageElement).getAttribute('src') ?? ''),
      );
    expect(sources.length, route).toBeGreaterThan(0);
    expect(
      sources.filter((src) => !FLAG_FILE.test(src)),
      route,
    ).toEqual([]);
  }
});

test('biểu tượng tab trỏ tới tệp ảnh mới và tải được', async ({ app }) => {
  await app.goto('/');
  const href = await app.locator('link[rel="icon"]').getAttribute('href');
  expect(href).toMatch(FLAG_FILE);

  const response = await app.request.get(href!);
  expect(response.status(), 'tệp biểu tượng tab phải tồn tại').toBe(200);
  expect(response.headers()['content-type']).toContain('image/png');
});

for (const viewport of SMALL_VIEWPORTS) {
  const label = `${viewport.width}x${viewport.height}`;

  test(`thanh đầu trang không tràn ở ${label}`, async ({ app }) => {
    await app.setViewportSize(viewport);
    await app.goto('/');
    await expect(app.locator('.hhd-header img')).toBeVisible();

    expect(await overflowsHorizontally(app, '.hhd-header'), 'header tràn ngang').toBe(false);

    // Tên hệ thống, ngày hôm nay và nút Đăng xuất đều phải còn đọc được.
    await expect(app.getByText('Huy Hiệu Đảng', { exact: true })).toBeVisible();
    await expect(app.locator('.hhd-header__today')).toBeVisible();
    await expect(app.locator('.hhd-header__logout')).toBeVisible();
    await expect(app.locator('.hhd-header__logout')).toBeInViewport();

    await app.screenshot({ path: path.join(SHOT_DIR, `t59-header-${label}.png`) });
  });
}

anonymous.describe('trang Đăng nhập', () => {
  anonymous('hàng thương hiệu và hình mờ đều dùng ảnh lá cờ mới', async ({ page }) => {
    await gotoLogin(page);

    const brand = page.locator('.hhd-login__brand > div:nth-child(2) img');
    await expect(brand).toHaveAttribute('src', FLAG_FILE);
    await expect(brand).toHaveAttribute('alt', 'Cờ Đảng và cờ Tổ quốc');
    await expectLoaded(brand, 'cờ ở hàng thương hiệu');

    const watermark = page.locator('.hhd-login__watermark img');
    await expect(watermark).toHaveAttribute('src', FLAG_FILE);
    // Hình mờ chỉ để trang trí nên trình đọc màn hình phải bỏ qua.
    await expect(watermark).toHaveAttribute('alt', '');
    await expect(watermark).toHaveAttribute('aria-hidden', 'true');
    await expectLoaded(watermark, 'cờ ở hình mờ');

    // Không phóng quá bề ngang tệp gốc.
    const box = await watermark.boundingBox();
    expect(box!.width).toBeLessThanOrEqual(498);

    await page.screenshot({ path: path.join(SHOT_DIR, 't59-dang-nhap-1440x900.png') });
  });

  for (const viewport of SMALL_VIEWPORTS) {
    const label = `${viewport.width}x${viewport.height}`;

    anonymous(`không tràn và không đè chữ ở ${label}`, async ({ page }) => {
      await page.setViewportSize(viewport);
      await gotoLogin(page);

      expect(await overflowsHorizontally(page, '.hhd-login'), 'trang Đăng nhập tràn ngang').toBe(
        false,
      );

      // Nửa trái và nửa phải đều phải hiện đủ, không cái nào bị đẩy khỏi màn hình.
      await expect(page.locator('.hhd-login__headline')).toBeInViewport();
      await expect(page.locator('.hhd-login__version')).toBeInViewport();
      await expect(page.getByRole('button', { name: 'Đăng nhập' })).toBeInViewport();

      await page.screenshot({ path: path.join(SHOT_DIR, `t59-dang-nhap-${label}.png`) });
    });
  }
});
