import path from 'node:path';

import type { Locator, Page } from '@playwright/test';

import { expect, test } from '../fixtures/app';
import { TableView } from '../fixtures/table';

/**
 * T64 — hai quyết định giao diện của chủ dự án trên màn Đợt trao huy hiệu, có
 * ca chặn riêng vì ở dự án này quyết định giao diện đã từng bị việc sau lật
 * ngược (T50 lật T49, phải mở T58 để khôi phục).
 *
 * 1. Dưới tiêu đề "Đợt trao huy hiệu" KHÔNG còn dòng tóm tắt nào — bỏ hẳn cả
 *    câu "… đợt · dùng chung cho mọi năm, chỉ lưu ngày/tháng" lẫn câu "Đang tải
 *    danh sách…" hiện lúc đang tải. Giữ câu lúc tải thì tiêu đề nhảy lên xuống;
 *    khung xương 5 dòng của bảng đã báo là đang tải.
 * 2. Các dòng "Đang diễn ra" và "Sắp tới" KHÔNG còn được tô nền vàng nhạt: mọi
 *    dòng cùng một nền như ở các bảng khác.
 *
 * Nhãn ở cột "Trạng thái <năm>" thì vẫn phải còn nguyên, kể cả nền vàng nhạt
 * của riêng nhãn "Sắp tới" — bỏ nền dòng chứ không bỏ nhãn.
 */

/** Nơi để ảnh chụp bàn giao. */
const SHOT_DIR = path.join(import.meta.dirname, '..', '.artifacts', 'anh');

/** Thẻ trắng bao bảng đợt. */
const PANEL = '.hhd-periods__panel';

/** Ô "Tên đợt" — ô thường, không có luật màu riêng nào, nên đo nền ở đây. */
const NAME_COLUMN = 1;

/** Ô "Trạng thái <năm>" — nơi in nhãn trạng thái. */
const STATUS_COLUMN = 4;

/**
 * Cỡ trang đủ chứa cả 36 đợt của bộ dữ liệu giả. Mặc định 20 dòng thì trang đầu
 * chỉ có đợt "Đã qua" (máy chủ sắp theo Từ ngày tăng dần), không so được ba
 * trạng thái với nhau.
 */
const ALL_ON_ONE_PAGE = 100;

/** Mở màn đợt, dàn cả ba trạng thái lên một trang, rồi đưa chuột ra khỏi bảng. */
async function openAllPeriods(app: Page): Promise<TableView> {
  await app.goto('/dot-trao-huy-hieu');
  const table = new TableView(app, app.locator(PANEL));
  await table.choosePageSize(ALL_ON_ONE_PAGE);
  // Hover mặc định của Ant Design cũng đổi nền dòng, nên phải rời bảng mới đo được.
  await app.mouse.move(0, 0);
  return table;
}

/** Dòng đầu tiên có nhãn trạng thái khớp `label`. */
async function rowByStatus(table: TableView, label: string): Promise<Locator> {
  const statuses = await table.columnTexts(STATUS_COLUMN);
  const index = statuses.findIndex((text) => text.startsWith(label));
  expect(index, `phải có ít nhất một đợt "${label}" trong dữ liệu mẫu`).toBeGreaterThanOrEqual(0);
  return table.cell(index, NAME_COLUMN);
}

/** Màu nền thật của một ô, sau khi tính hết các luật CSS. */
function backgroundOf(cell: Locator): Promise<string> {
  return cell.evaluate((element) => window.getComputedStyle(element).backgroundColor);
}

test.describe('Màn Đợt trao huy hiệu không còn dòng tóm tắt dưới tiêu đề', () => {
  test('tiêu đề đứng một mình, không có dòng phụ nào', async ({ app }) => {
    await app.goto('/dot-trao-huy-hieu');
    await expect(app.getByRole('heading', { name: 'Đợt trao huy hiệu' })).toBeVisible();

    await expect(app.locator('.hhd-page-heading__description')).toHaveCount(0);
    await expect(app.getByText('dùng chung cho mọi năm')).toHaveCount(0);
    await expect(app.getByText('Đang tải danh sách')).toHaveCount(0);
  });

  test('lúc đang tải cũng không hiện dòng phụ, nên tiêu đề không nhảy', async ({ app }) => {
    // Giữ lời gọi danh sách đợt lại để chụp đúng lúc màn đang tải.
    let release = () => {};
    const held = new Promise<void>((resolve) => {
      release = resolve;
    });
    await app.route('**/api/AwardPeriods', async (route) => {
      await held;
      await route.fallback();
    });

    await app.goto('/dot-trao-huy-hieu');
    const heading = app.getByRole('heading', { name: 'Đợt trao huy hiệu' });
    await expect(heading).toBeVisible();
    // Khung xương của bảng là thứ báo "đang tải", không phải chữ dưới tiêu đề.
    await expect(app.locator(`${PANEL} .ant-skeleton`).first()).toBeVisible();
    await expect(app.locator('.hhd-page-heading__description')).toHaveCount(0);
    const whileLoading = await heading.boundingBox();

    release();
    await expect(app.locator(`${PANEL} .ant-table-row`).first()).toBeVisible();
    await expect(app.locator('.hhd-page-heading__description')).toHaveCount(0);
    expect(await heading.boundingBox(), 'tiêu đề phải đứng nguyên chỗ sau khi tải xong').toEqual(
      whileLoading,
    );
  });
});

test.describe('Bảng đợt không còn tô nền vàng dòng chưa qua', () => {
  test('ô của ba trạng thái có cùng một màu nền', async ({ app }) => {
    const table = await openAllPeriods(app);

    const past = await backgroundOf(await rowByStatus(table, 'Đã qua'));
    const ongoing = await backgroundOf(await rowByStatus(table, 'Đang diễn ra'));
    const upcoming = await backgroundOf(await rowByStatus(table, 'Sắp tới'));

    expect(ongoing, 'dòng "Đang diễn ra" phải cùng nền với dòng "Đã qua"').toBe(past);
    expect(upcoming, 'dòng "Sắp tới" phải cùng nền với dòng "Đã qua"').toBe(past);
  });

  test('không còn lớp hhd-periods__row--next trong DOM', async ({ app }) => {
    const table = await openAllPeriods(app);

    await expect(table.rows.first()).toBeVisible();
    await expect(app.locator('.hhd-periods__row--next')).toHaveCount(0);
  });

  test('ba nhãn trạng thái vẫn hiện đúng chữ', async ({ app }) => {
    const table = await openAllPeriods(app);
    const statuses = await table.columnTexts(STATUS_COLUMN);

    expect(statuses.some((text) => text === 'Đã qua')).toBe(true);
    expect(statuses.some((text) => text === 'Đang diễn ra')).toBe(true);
    expect(statuses.some((text) => text.startsWith('Sắp tới'))).toBe(true);
  });
});

/**
 * Ảnh bàn giao cho chủ dự án: khung 1440x900 của bộ thiết kế và khung 1366x650
 * của laptop chủ dự án.
 */
const SHOT_VIEWPORTS = [
  { width: 1440, height: 900 },
  { width: 1366, height: 650 },
] as const;

/** Lọc theo "nhóm 3" thu 36 đợt mẫu còn 8 đợt trải đủ ba trạng thái. */
const SHOT_KEYWORD = 'nhóm 3';

/**
 * Dải độ phủ chiếm nửa trên màn nên thân bảng chỉ hở chừng bốn dòng. Sắp theo
 * "Đủ điều kiện năm nay" tăng dần xen ba trạng thái vào ngay bốn dòng đầu, nên
 * một khung ảnh có đủ cả tiêu đề lẫn ba trạng thái mà không phải cuộn.
 */
const SHOT_SORT_COLUMN = 'Đủ điều kiện năm nay';
const SHOT_VISIBLE_ROWS = 4;

test.describe('Ảnh bàn giao màn Đợt trao huy hiệu', () => {
  for (const { width, height } of SHOT_VIEWPORTS) {
    test(`chụp màn có đủ ba trạng thái ở ${width}x${height}`, async ({ app }) => {
      await app.setViewportSize({ width, height });
      await app.goto('/dot-trao-huy-hieu');
      const table = new TableView(app, app.locator(PANEL));
      await expect(table.rows.first()).toBeVisible();

      await table.search.fill(SHOT_KEYWORD);
      await expect(table.rows).toHaveCount(8);
      await table.clickSort(SHOT_SORT_COLUMN);

      const visible = (await table.columnTexts(STATUS_COLUMN)).slice(0, SHOT_VISIBLE_ROWS);
      expect(visible.some((text) => text === 'Đã qua')).toBe(true);
      expect(visible.some((text) => text === 'Đang diễn ra')).toBe(true);
      expect(visible.some((text) => text.startsWith('Sắp tới'))).toBe(true);

      await app.mouse.move(0, 0);
      await app.screenshot({
        path: path.join(SHOT_DIR, `t64-dot-ba-trang-thai-${width}x${height}.png`),
      });
    });
  }
});
