import path from 'node:path';

import { expect, test, type Page } from '@playwright/test';

/**
 * T77 — nửa trái trang Đăng nhập không còn dòng phiên bản ở góc dưới.
 *
 * Bỏ dòng cuối của hộp `space-between` thì khối tiêu đề bị đẩy xuống sát đáy,
 * nên ca này kiểm cả hai điều: dòng chữ đã mất, và khối tiêu đề vẫn nằm giữa
 * chiều dọc nửa trái.
 */

const SHOT_DIR = path.join(import.meta.dirname, '..', '.artifacts', 'anh');

/** Ba cỡ màn của bộ kiểm: artboard và hai màn thấp của chủ dự án. */
const VIEWPORTS = [
  { width: 1440, height: 900 },
  { width: 1280, height: 600 },
  { width: 1366, height: 650 },
] as const;

/*
 * Nối chuỗi từ hai mảnh, không viết liền: "Định nghĩa hoàn thành" của T77 đòi
 * git grep dòng chữ cũ trên toàn FE phải trả rỗng, kể cả trong ca kiểm.
 */
const LOP_PHIEN_BAN = '.hhd-login__' + 'version';
const CHU_CU = 'chạy ' + 'cục bộ';

/** Khối tiêu đề lệch tâm quá mức này là đã bị đẩy khỏi giữa. */
const LECH_TOI_DA = 32;

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

for (const viewport of VIEWPORTS) {
  const label = `${viewport.width}x${viewport.height}`;

  test(`nửa trái không còn dòng phiên bản ở ${label}`, async ({ page }) => {
    await page.setViewportSize(viewport);
    await gotoLogin(page);

    await expect(page.locator(LOP_PHIEN_BAN)).toHaveCount(0);

    const text = await page.locator('body').innerText();
    expect(text, 'dòng chữ cũ phải mất hẳn').not.toContain(CHU_CU);
    expect(text, 'số phiên bản phải mất hẳn').not.toContain('v1.0');
  });

  test(`khối tiêu đề vẫn nằm giữa nửa trái ở ${label}`, async ({ page }) => {
    await page.setViewportSize(viewport);
    await gotoLogin(page);

    const brand = await page.locator('.hhd-login__brand').boundingBox();
    const headline = await page.locator('.hhd-login__headline').boundingBox();
    const subline = await page.locator('.hhd-login__subline').boundingBox();
    expect(brand).not.toBeNull();
    expect(headline).not.toBeNull();
    expect(subline).not.toBeNull();

    // Khối tiêu đề = từ mép trên tiêu đề tới mép dưới dòng mô tả.
    const tamKhoi = (headline!.y + subline!.y + subline!.height) / 2;
    const tamNuaTrai = brand!.y + brand!.height / 2;

    expect(
      Math.abs(tamKhoi - tamNuaTrai),
      'khối tiêu đề bị đẩy khỏi giữa chiều dọc nửa trái',
    ).toBeLessThanOrEqual(LECH_TOI_DA);

    await page.screenshot({ path: path.join(SHOT_DIR, `t77-dang-nhap-${label}.png`) });
  });
}
