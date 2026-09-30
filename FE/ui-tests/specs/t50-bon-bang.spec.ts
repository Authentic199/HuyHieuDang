import path from 'node:path';

import type { Locator, Page } from '@playwright/test';

import { DETAIL_PERIOD, expect, mockData, test } from '../fixtures/app';
import { formatNumber } from '../fixtures/format';
import { TableView } from '../fixtures/table';

/**
 * T50 — bốn bảng phải có phân trang, tìm, sắp xếp, lọc theo mốc, và không được
 * cắt mất dòng nào.
 *
 * Mỗi màn một ca, bên trong kiểm đúng ba việc mà chủ dự án yêu cầu: đổi trang
 * thì dòng đổi, gõ từ khóa thì tổng đổi, chọn mốc thì chỉ còn mốc đó. Thêm một
 * ca riêng cho lỗi gốc (cuộn tới được dòng cuối khi chọn 100 dòng mỗi trang) và
 * một ca cho trạng thái rỗng-do-lọc.
 */

/** Nơi để ảnh chụp bàn giao. */
const SHOT_DIR = path.join(import.meta.dirname, '..', '.artifacts', 'anh');

/** Từ khóa đem gõ vào ô tìm — một họ có thật trong bộ dữ liệu. */
const KEYWORD = 'Hoàng';

/** Số dòng mỗi trang khi mới vào màn, theo quy ước của màn Đảng viên. */
const PAGE_SIZE = 20;

/** Bảng đủ điều kiện trên Dashboard mở 10 dòng để vừa một màn — xem T58. */
const DASHBOARD_PAGE_SIZE = 10;

/** Chỉ số cột của bảng "đủ điều kiện": STT, Họ tên, …, Mốc huy hiệu. */
const ELIGIBLE_COLUMN = { index: 0, fullName: 1, milestone: 6 } as const;

function countNames(rows: { fullName: string }[], keyword: string): number {
  return rows.filter((row) => row.fullName.toLowerCase().includes(keyword.toLowerCase())).length;
}

function countMilestone(rows: { milestone: number }[], milestone: number): number {
  return rows.filter((row) => row.milestone === milestone).length;
}

/** Nhãn của một lựa chọn trong ô lọc mốc, ví dụ "30 năm (46)". */
function milestoneLabel(rows: { milestone: number }[], milestone: number): string {
  return `${formatNumber(milestone)} năm (${formatNumber(countMilestone(rows, milestone))})`;
}

async function shoot(page: Page, name: string): Promise<void> {
  await page.screenshot({ path: path.join(SHOT_DIR, `${name}.png`) });
}

/** Ba việc phải đúng ở cả ba bảng có cột mốc huy hiệu. */
async function checkEligibleTable(
  page: Page,
  panel: Locator,
  rows: { fullName: string; milestone: number }[],
  shotName: string,
  pageSize = PAGE_SIZE,
): Promise<void> {
  const table = new TableView(page, panel);
  const total = rows.length;

  await test.step(`Trang đầu: đủ ${pageSize} dòng, STT bắt đầu từ 1`, async () => {
    await table.expectPage(pageSize, total, pageSize);
    await table.expectPageSize(pageSize);
    await expect(table.cell(0, ELIGIBLE_COLUMN.index)).toHaveText('1');
    await expect(table.pagination).toBeVisible();
    // Chân bảng không in tổng — tổng đã nằm ở dòng mô tả dưới tiêu đề màn.
    await table.expectNoTotalText();
    await shoot(page, shotName);
  });

  await test.step('Đổi trang thì dòng đổi và STT chạy tiếp', async () => {
    const firstNameOnPageOne = await table.cell(0, ELIGIBLE_COLUMN.fullName).innerText();
    await table.goToPage(2);
    await expect(table.cell(0, ELIGIBLE_COLUMN.index)).toHaveText(String(pageSize + 1));
    await expect(table.cell(0, ELIGIBLE_COLUMN.fullName)).not.toHaveText(firstNameOnPageOne);
    await table.expectPage(pageSize, total, pageSize);
  });

  await test.step('Gõ từ khóa thì tổng đổi và nhảy về trang 1', async () => {
    const expected = countNames(rows, KEYWORD);
    expect(expected, 'bộ dữ liệu phải có người khớp từ khóa').toBeGreaterThan(pageSize);

    await table.search.fill(KEYWORD);
    await table.expectPage(pageSize, expected, pageSize);
    await expect(table.cell(0, ELIGIBLE_COLUMN.index)).toHaveText('1');
    for (const name of await table.columnTexts(ELIGIBLE_COLUMN.fullName)) {
      expect(name).toContain(KEYWORD);
    }
    await table.search.fill('');
    await table.expectPage(pageSize, total, pageSize);
  });

  await test.step('Chọn mốc thì chỉ còn mốc đó', async () => {
    const milestone = rows[0].milestone;
    const expected = countMilestone(rows, milestone);

    await table.chooseMilestone(milestoneLabel(rows, milestone));
    await table.expectPage(Math.min(pageSize, expected), expected, pageSize);
    for (const text of await table.columnTexts(ELIGIBLE_COLUMN.milestone)) {
      expect(text).toBe(`${milestone} năm`);
    }
  });

  await test.step('Sắp xếp: nhấn lần thứ ba trả về thứ tự máy chủ', async () => {
    await table.chooseMilestone('Tất cả mốc');
    const serverOrder = await table.columnTexts(ELIGIBLE_COLUMN.fullName);
    // Sắp xếp là việc của CẢ danh sách chứ không riêng trang đang xem, nên trang 1
    // phải mở đúng bằng tên đứng đầu (hoặc đứng cuối) của toàn bộ dữ liệu.
    const compare = new Intl.Collator('vi').compare;
    const allNames = rows.map((row) => row.fullName).sort(compare);

    await table.clickSort('Họ tên');
    const ascending = await table.columnTexts(ELIGIBLE_COLUMN.fullName);
    expect(ascending).not.toEqual(serverOrder);
    expect(ascending).toEqual([...ascending].sort(compare));
    expect(ascending[0]).toBe(allNames[0]);

    await table.clickSort('Họ tên');
    const descending = await table.columnTexts(ELIGIBLE_COLUMN.fullName);
    expect(descending).toEqual([...descending].sort(compare).reverse());
    expect(descending[0]).toBe(allNames[allNames.length - 1]);

    await table.clickSort('Họ tên');
    expect(await table.columnTexts(ELIGIBLE_COLUMN.fullName)).toEqual(serverOrder);
  });
}

/** Lỗi gốc: 100 dòng mỗi trang vẫn cuộn tới được dòng cuối, phân trang không bị cắt. */
async function checkScrollReachesLastRow(
  page: Page,
  panel: Locator,
  shotName?: string,
): Promise<void> {
  const table = new TableView(page, panel);
  await table.choosePageSize(100);
  await expect(table.rows).toHaveCount(100);

  // Thanh phân trang phải còn nằm trong khung nhìn, không bị đẩy ra ngoài thẻ.
  await expect(table.pagination).toBeInViewport();

  const scroller = table.scroller;
  await expect(scroller).toBeVisible();
  const overflowing = await scroller.evaluate(
    (element) => element.scrollHeight > element.clientHeight + 1,
  );
  expect(overflowing, 'vùng thân bảng phải cuộn được, không được cắt mất dòng').toBe(true);

  const lastRow = table.rows.nth(99);
  await lastRow.scrollIntoViewIfNeeded();
  await expect(lastRow).toBeInViewport();
  await expect(table.pagination).toBeInViewport();
  // Hàng tiêu đề dính trên: cuộn tới đâu cũng còn biết đang đọc cột nào.
  await expect(panel.locator('.ant-table-thead > tr > th').first()).toBeInViewport();
  if (shotName) await shoot(page, shotName);
}

test.describe('Dashboard — bảng đủ điều kiện', () => {
  test('phân trang, tìm, lọc mốc, sắp xếp', async ({ app }) => {
    await app.goto('/');
    const panel = app.locator('.hhd-dashboard__panel');
    await expect(panel).toBeVisible();
    await checkEligibleTable(
      app,
      panel,
      mockData.eligible,
      '1-dashboard-du-dieu-kien',
      DASHBOARD_PAGE_SIZE,
    );
  });

  test('100 dòng mỗi trang vẫn cuộn tới được dòng cuối', async ({ app }) => {
    await app.goto('/');
    await checkScrollReachesLastRow(
      app,
      app.locator('.hhd-dashboard__panel'),
      '1b-dashboard-100-dong-cuon-toi-cuoi',
    );
  });

  test('rỗng do lọc nói đúng lý do và cho xóa bộ lọc', async ({ app }) => {
    await app.goto('/');
    const panel = app.locator('.hhd-dashboard__panel');
    const table = new TableView(app, panel);

    await table.search.fill('Không có ai tên như vậy');
    await expect(panel.getByText('Không tìm thấy đảng viên nào khớp.')).toBeVisible();
    // Tuyệt đối không được hiện lại câu của trạng thái chưa có dữ liệu.
    await expect(panel.getByText('Chưa cài đợt trao huy hiệu.')).toHaveCount(0);
    await expect(panel.getByText('Không có đảng viên nào tròn mốc trong')).toHaveCount(0);
    await shoot(app, '5-rong-do-loc');

    await panel.getByRole('button', { name: 'Xóa bộ lọc' }).click();
    await table.expectPage(DASHBOARD_PAGE_SIZE, mockData.eligible.length, DASHBOARD_PAGE_SIZE);
  });
});

test.describe('Chi tiết đợt — danh sách đủ điều kiện', () => {
  test('phân trang, tìm, lọc mốc, sắp xếp', async ({ app }) => {
    await app.goto(`/dot-trao-huy-hieu/${DETAIL_PERIOD.id}`);
    const panel = app.locator('.hhd-eligibility');
    await expect(panel).toBeVisible();
    await checkEligibleTable(app, panel, mockData.eligible, '3-chi-tiet-dot-du-dieu-kien');
  });

  test('100 dòng mỗi trang vẫn cuộn tới được dòng cuối', async ({ app }) => {
    await app.goto(`/dot-trao-huy-hieu/${DETAIL_PERIOD.id}`);
    await checkScrollReachesLastRow(app, app.locator('.hhd-eligibility'));
  });
});

test.describe('Chưa thuộc đợt nào', () => {
  test('phân trang, tìm, lọc mốc, sắp xếp', async ({ app }) => {
    await app.goto('/chua-thuoc-dot-nao');
    const panel = app.locator('.hhd-uncovered__panel');
    await expect(panel).toBeVisible();
    await checkEligibleTable(app, panel, mockData.unassigned, '4-chua-thuoc-dot-nao');
  });

  test('100 dòng mỗi trang vẫn cuộn tới được dòng cuối', async ({ app }) => {
    await app.goto('/chua-thuoc-dot-nao');
    await checkScrollReachesLastRow(app, app.locator('.hhd-uncovered__panel'));
  });
});

test.describe('Đợt trao huy hiệu', () => {
  /**
   * Bảng đợt không có cột mốc nên KHÔNG có ô lọc mốc. Cột đầu là STT (thêm ở
   * T49), nên Tên đợt là cột thứ hai — chỉ số cũ trỏ vào STT và ca "đổi trang
   * thì dòng đổi" đọc mãi con số 1 nên không bao giờ đỏ được.
   */
  const PERIOD_COLUMN = { name: 1, fromDay: 2 } as const;

  test('phân trang, tìm theo tên đợt, sắp xếp theo Từ ngày', async ({ app }) => {
    await app.goto('/dot-trao-huy-hieu');
    const panel = app.locator('.hhd-periods__panel');
    const table = new TableView(app, panel);
    const total = mockData.periods.length;

    await table.expectPage(PAGE_SIZE, total, PAGE_SIZE);
    await table.expectPageSize(PAGE_SIZE);
    await expect(table.pagination).toBeVisible();
    await table.expectNoTotalText();
    // Màn này không có cột mốc huy hiệu nên cũng không có ô lọc mốc.
    await expect(table.milestoneSelect).toHaveCount(0);
    await app.screenshot({ path: path.join(SHOT_DIR, '2-dot-trao-huy-hieu.png') });

    await test.step('Đổi trang thì dòng đổi', async () => {
      const firstOnPageOne = await table.cell(0, PERIOD_COLUMN.name).innerText();
      await table.goToPage(2);
      await expect(table.cell(0, PERIOD_COLUMN.name)).not.toHaveText(firstOnPageOne);
      await table.expectPage(total - PAGE_SIZE, total, PAGE_SIZE);
    });

    await test.step('Gõ từ khóa thì tổng đổi và nhảy về trang 1', async () => {
      const keyword = 'nhóm 1';
      const expected = mockData.periods.filter((row) =>
        row.name.toLowerCase().includes(keyword),
      ).length;
      expect(expected).toBeGreaterThan(0);

      await table.search.fill(keyword);
      await table.expectPage(Math.min(PAGE_SIZE, expected), expected, PAGE_SIZE);
      for (const name of await table.columnTexts(PERIOD_COLUMN.name)) {
        expect(name).toContain('nhóm 1');
      }
      await table.search.fill('');
      await table.expectPage(PAGE_SIZE, total, PAGE_SIZE);
    });

    await test.step('Sắp xếp Từ ngày: nhấn lần thứ ba trả về thứ tự máy chủ', async () => {
      const serverOrder = await table.columnTexts(PERIOD_COLUMN.fromDay);
      // Máy chủ đã sắp theo Từ ngày tăng dần, nên lần nhấn đầu chỉ đảo trong các
      // đợt trùng ngày/tháng; lần thứ hai (giảm dần) mới đổi hẳn thứ tự.
      await table.clickSort('Từ ngày');
      await table.clickSort('Từ ngày');
      expect(await table.columnTexts(PERIOD_COLUMN.fromDay)).not.toEqual(serverOrder);

      await table.clickSort('Từ ngày');
      expect(await table.columnTexts(PERIOD_COLUMN.fromDay)).toEqual(serverOrder);
    });
  });

  test('100 dòng mỗi trang vẫn cuộn tới được dòng cuối', async ({ app }) => {
    await app.goto('/dot-trao-huy-hieu');
    const panel = app.locator('.hhd-periods__panel');
    const table = new TableView(app, panel);
    await table.choosePageSize(100);
    // Bộ dữ liệu có 36 đợt nên một trang 100 dòng hiện hết — điều phải đúng là
    // thanh phân trang vẫn thấy được và dòng cuối cuộn tới được.
    await expect(table.rows).toHaveCount(mockData.periods.length);
    await expect(table.pagination).toBeInViewport();

    const lastRow = table.rows.nth(mockData.periods.length - 1);
    await lastRow.scrollIntoViewIfNeeded();
    await expect(lastRow).toBeInViewport();
    await expect(table.pagination).toBeInViewport();
  });
});
