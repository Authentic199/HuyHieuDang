import path from 'node:path';

import type { Locator, Page } from '@playwright/test';

import { DETAIL_PERIOD, expect, test } from '../fixtures/app';
import { TableView } from '../fixtures/table';

/**
 * T58 — hai quyết định giao diện đã bị lật ngược một lần, nay có ca chặn riêng.
 *
 * 1. Chân bảng KHÔNG in cụm đếm `1–20 / 1.342`. Chủ dự án cho bỏ ở T49 vì tổng
 *    số đã nằm ở dòng mô tả dưới tiêu đề màn và ở chân thẻ; T50 gom bốn bảng về
 *    `useClientTable` và dựng lại cụm đó ở cả bốn bảng.
 * 2. Bảng đủ điều kiện trên Dashboard mở ra 10 dòng mỗi trang, không phải 20:
 *    thẻ nằm dưới thẻ đợt sắp tới, 10 dòng vừa đúng một màn. `useClientTable`
 *    không nhận cỡ trang nên thẻ này bị kéo về 20 theo mặc định chung.
 *
 * Ô "… / trang" và các số trang thì vẫn phải còn — bỏ cụm đếm chứ không bỏ
 * thanh phân trang.
 */

/** Nơi để ảnh chụp bàn giao. */
const SHOT_DIR = path.join(import.meta.dirname, '..', '.artifacts', 'anh');

/** Cỡ trang mặc định của riêng thẻ đủ điều kiện trên Dashboard. */
const DASHBOARD_PAGE_SIZE = 10;

/** Cỡ trang mặc định của ba bảng chiếm trọn một màn. */
const FULL_PAGE_SIZE = 20;

/** Bốn bảng dùng chung `useClientTable`, kèm cách mở màn của từng bảng. */
const TABLES: {
  name: string;
  panel: string;
  pageSize: number;
  open: (page: Page) => Promise<void>;
}[] = [
  {
    name: 'Dashboard — đủ điều kiện',
    panel: '.hhd-dashboard__panel',
    pageSize: DASHBOARD_PAGE_SIZE,
    open: async (page) => {
      await page.goto('/');
    },
  },
  {
    name: 'Đợt trao huy hiệu',
    panel: '.hhd-periods__panel',
    pageSize: FULL_PAGE_SIZE,
    open: async (page) => {
      await page.goto('/dot-trao-huy-hieu');
    },
  },
  {
    name: 'Chi tiết đợt — tab Đủ điều kiện',
    panel: '.hhd-eligibility',
    pageSize: FULL_PAGE_SIZE,
    open: async (page) => {
      await page.goto(`/dot-trao-huy-hieu/${DETAIL_PERIOD.id}`);
      await page.getByRole('tab', { name: 'Danh sách đủ điều kiện' }).click();
    },
  },
  {
    name: 'Chưa thuộc đợt nào',
    panel: '.hhd-uncovered__panel',
    pageSize: FULL_PAGE_SIZE,
    open: async (page) => {
      await page.goto('/chua-thuoc-dot-nao');
    },
  },
];

/** Chân bảng còn đủ thứ cần dùng: ô "… / trang", số trang, và không có cụm đếm. */
async function checkFooter(page: Page, panel: Locator, pageSize: number): Promise<void> {
  const table = new TableView(page, panel);
  await expect(panel).toBeVisible();
  await expect(table.pagination).toBeVisible();

  await table.expectNoTotalText();
  // Không chỉ cụm đếm của Ant Design: cả chân bảng không được có chuỗi "a–b / c".
  await expect(table.pagination).not.toHaveText(/\d[–-]\d+\s*\/\s*\d/);

  await table.expectPageSize(pageSize);
  await expect(table.pagination.locator('.ant-pagination-item').first()).toHaveText('1');
}

test.describe('Cụm đếm ở chân bảng đã bỏ và không được quay lại', () => {
  for (const { name, panel, pageSize, open } of TABLES) {
    test(`${name}: chân bảng không còn "1–20 / 1.342"`, async ({ app }) => {
      await open(app);
      await checkFooter(app, app.locator(panel), pageSize);
    });
  }

  test('chọn 100 dòng mỗi trang cũng không làm cụm đếm hiện lại', async ({ app }) => {
    await app.goto('/chua-thuoc-dot-nao');
    const table = new TableView(app, app.locator('.hhd-uncovered__panel'));

    await table.choosePageSize(100);
    await table.expectPageSize(100);
    await table.expectNoTotalText();
  });
});

test.describe('Cỡ trang của bảng đủ điều kiện trên Dashboard', () => {
  test('mở ra đúng 10 dòng, ngắn hơn hẳn 20 dòng nên ít phải cuộn', async ({ app }) => {
    await app.goto('/');
    const panel = app.locator('.hhd-dashboard__panel');
    const table = new TableView(app, panel);

    await expect(table.rows).toHaveCount(DASHBOARD_PAGE_SIZE);
    await table.expectPageSize(DASHBOARD_PAGE_SIZE);
    await expect(table.pagination).toBeInViewport();
    await app.screenshot({ path: path.join(SHOT_DIR, 't58-dashboard-10-dong.png') });

    // Lý do chọn 10: thân bảng ngắn hơn hẳn, bác cuộn ít hơn. Đo chiều cao thật
    // thay vì đòi "không phải cuộn" — thẻ này nằm dưới thẻ đợt sắp tới nên phần
    // còn lại của màn hình luôn thấp hơn cả mười dòng.
    const heightOf = () => table.scroller.evaluate((element) => element.scrollHeight);
    const tenRows = await heightOf();
    await table.choosePageSize(FULL_PAGE_SIZE);
    await expect(table.rows).toHaveCount(FULL_PAGE_SIZE);
    const twentyRows = await heightOf();
    expect(tenRows, 'mười dòng phải thấp hơn hai mươi dòng').toBeLessThan(twentyRows);
  });

  test('vẫn đổi được sang 20 dòng và nhớ lựa chọn khi đổi trang', async ({ app }) => {
    await app.goto('/');
    const table = new TableView(app, app.locator('.hhd-dashboard__panel'));

    await table.choosePageSize(FULL_PAGE_SIZE);
    await expect(table.rows).toHaveCount(FULL_PAGE_SIZE);
    await table.goToPage(2);
    await expect(table.rows).toHaveCount(FULL_PAGE_SIZE);
    await table.expectPageSize(FULL_PAGE_SIZE);
  });

  test('ba bảng chiếm trọn màn vẫn giữ 20 dòng mặc định', async ({ app }) => {
    for (const { panel, open, pageSize } of TABLES.filter(
      (item) => item.pageSize !== DASHBOARD_PAGE_SIZE,
    )) {
      await open(app);
      const table = new TableView(app, app.locator(panel));
      await expect(table.rows).toHaveCount(pageSize);
      await table.expectPageSize(pageSize);
    }
  });
});
