import path from 'node:path';

import type { Locator, Page } from '@playwright/test';

import { expect, test } from '../fixtures/app';

/**
 * T71 — nút "Khôi phục mặc định 30 / 90 / 5" ở chân thẻ Mốc tuổi đảng.
 *
 * Nút từng vẽ theo artboard Cài đặt: `type="text"`, không viền, không nền — nhìn
 * ra một dòng chữ chứ không ra nút. Quyết định ngày 30/09/2026 của chủ dự án
 * thay cho artboard ở đúng điểm này: nút mặc định của Ant Design, có viền, thêm
 * icon undo. Tài liệu nghiệp vụ và hướng dẫn sử dụng vốn đã gọi nó là nút.
 *
 * Tên truy cập phải giữ nguyên cụm chữ: ca e2e e3 tìm nút bằng đúng chuỗi đó.
 */

/** Nơi để ảnh chụp bàn giao. */
const SHOT_DIR = path.join(import.meta.dirname, '..', '.artifacts', 'anh');

/** Đúng locator mà `e2e/specs/e3-doi-cai-dat-lan-truyen.spec.ts` đang dùng. */
function restoreButton(scope: Page): Locator {
  return scope.getByRole('button', { name: 'Khôi phục mặc định 30 / 90 / 5' });
}

/** Đọc một nhóm thuộc tính CSS đã tính của một phần tử. */
function styles(target: Locator, names: readonly string[]): Promise<Record<string, string>> {
  return target.evaluate((element, keys) => {
    const computed = window.getComputedStyle(element);
    return Object.fromEntries(keys.map((key) => [key, computed.getPropertyValue(key)]));
  }, names);
}

test.describe('Nút Khôi phục mặc định trông ra một cái nút', () => {
  test('vẫn tìm được bằng đúng locator của ca e2e e3', async ({ app }) => {
    await app.goto('/cai-dat');
    const button = restoreButton(app);

    await expect(button).toBeVisible();
    await expect(button).toBeEnabled();
  });

  test('không còn là nút dạng chữ: có viền nhìn thấy được', async ({ app }) => {
    await app.goto('/cai-dat');
    const button = restoreButton(app);
    await expect(button).toBeVisible();

    await expect(button).not.toHaveClass(/\bant-btn-text\b/);

    const shape = await styles(button, [
      'border-top-width',
      'border-top-style',
      'border-top-color',
      'border-radius',
    ]);
    expect(Number.parseFloat(shape['border-top-width']), 'viền phải dày hơn 0').toBeGreaterThan(0);
    expect(shape['border-top-style']).not.toBe('none');
    expect(Number.parseFloat(shape['border-radius']), 'góc phải bo').toBeGreaterThan(0);

    // Viền phải khác màu nền thẻ, nếu không thì có cũng như không.
    const cardBackground = await app
      .locator('.hhd-settings__card')
      .first()
      .evaluate((element) => window.getComputedStyle(element).backgroundColor);
    expect(shape['border-top-color']).not.toBe(cardBackground);
  });

  test('có icon undo đứng trước chữ', async ({ app }) => {
    await app.goto('/cai-dat');
    const button = restoreButton(app);
    await expect(button).toBeVisible();

    const icon = button.locator('.anticon-undo');
    await expect(icon).toHaveCount(1);

    // Icon đứng trước chữ, không phải sau.
    const iconBox = await icon.boundingBox();
    const labelBox = await button.locator('span', { hasText: 'Khôi phục mặc định' }).boundingBox();
    expect(iconBox && labelBox).toBeTruthy();
    expect(iconBox!.x).toBeLessThan(labelBox!.x);
  });

  test('cao bằng nút Lưu', async ({ app }) => {
    await app.goto('/cai-dat');
    const restore = restoreButton(app);
    const save = app.getByRole('button', { name: 'Lưu', exact: true });
    await expect(restore).toBeVisible();
    await expect(save).toBeVisible();

    const restoreBox = await restore.boundingBox();
    const saveBox = await save.boundingBox();
    expect(restoreBox!.height).toBeCloseTo(saveBox!.height, 1);
  });

  test('chân hai thẻ cách đường kẻ như nhau', async ({ app }) => {
    await app.goto('/cai-dat');
    await expect(restoreButton(app)).toBeVisible();

    const feet = app.locator('.hhd-settings__card-foot');
    const paddings = await feet.evaluateAll((elements) =>
      elements.map((element) => window.getComputedStyle(element).paddingTop),
    );
    expect(paddings.length).toBeGreaterThanOrEqual(2);
    expect(new Set(paddings).size, 'hai chân thẻ phải cùng một khoảng cách').toBe(1);

    await app
      .locator('.hhd-settings__card')
      .first()
      .screenshot({ path: path.join(SHOT_DIR, 't71-the-moc-tuoi-dang.png') });
  });
});
