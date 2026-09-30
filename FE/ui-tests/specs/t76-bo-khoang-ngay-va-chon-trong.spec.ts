import type { Locator, Page } from '@playwright/test';

import { DETAIL_PERIOD, expect, test } from '../fixtures/app';
import { SERVER_YEAR } from '../fixtures/data';

/**
 * T76 — Chủ dự án bỏ hai chỗ chữ: khoảng ngày hằng năm đứng cạnh tên đợt ở tiêu
 * đề trang Chi tiết đợt, và dòng "Chọn trong … – …" ở đầu bảng chọn năm.
 *
 * Chỉ chữ bị bỏ. Giới hạn năm máy chủ ± 100 giữ nguyên, nên ca này canh cả hai
 * mặt: chữ đã mất, còn giới hạn vẫn khóa đúng ô.
 *
 * Chạy trên dữ liệu giả của `fixtures/app.ts`, năm "máy chủ" là `SERVER_YEAR`.
 * Không con số nào viết cứng theo năm chạy thật.
 */

/** Đi xa nhất 100 năm mỗi phía — khớp `REACH_YEARS` trong `YearPicker.tsx`. */
const REACH_YEARS = 100;
const MIN_YEAR = SERVER_YEAR - REACH_YEARS;
const MAX_YEAR = SERVER_YEAR + REACH_YEARS;

const DETAIL_URL = `/dot-trao-huy-hieu/${DETAIL_PERIOD.id}`;
const UNCOVERED_URL = '/chua-thuoc-dot-nao';

const YEAR_PICKER_SCREENS = [
  { label: 'Chi tiết đợt', url: DETAIL_URL },
  { label: 'Chưa thuộc đợt nào', url: UNCOVERED_URL },
];

function panel(app: Page): Locator {
  return app.locator('.hhd-year-panel');
}

/** Một ô năm trong bảng chọn — tìm theo đúng con số, không khớp một phần. */
function panelYear(app: Page, year: number): Locator {
  return panel(app).getByRole('button', { name: String(year), exact: true });
}

async function openPanel(app: Page): Promise<Locator> {
  await app.getByRole('button', { name: /^Chọn năm khác, đang xem/ }).click();
  await expect(panel(app)).toBeVisible();
  return panel(app);
}

test.describe('Tiêu đề trang Chi tiết đợt chỉ còn tên đợt', () => {
  test('tiêu đề đúng bằng tên đợt, không còn khoảng ngày hằng năm', async ({ app }) => {
    await app.goto(DETAIL_URL);

    const heading = app.locator('.hhd-page-heading__title');
    await expect(heading).toHaveText(DETAIL_PERIOD.name);
    await expect(heading).not.toContainText('hằng năm');
    await expect(app.locator('.hhd-period-detail__range')).toHaveCount(0);
  });

  test('khoảng ngày của đợt vẫn có, ở thanh công cụ và đã gắn năm', async ({ app }) => {
    await app.goto(DETAIL_URL);

    // Thanh công cụ của thẻ danh sách là chỗ DUY NHẤT còn nói khoảng ngày, và ở
    // đó khoảng ngày luôn mang năm — kể cả đợt vắt qua 31/12 (QT6).
    const dates = app.locator('.hhd-eligibility__dates');
    await expect(dates).toBeVisible();
    await expect(dates).toHaveText(/^\d{2}\/\d{2}\/\d{4} – \d{2}\/\d{2}\/\d{4}$/);
  });
});

test.describe('Bảng chọn năm bỏ dòng "Chọn trong …"', () => {
  for (const { label, url } of YEAR_PICKER_SCREENS) {
    test(`${label}: đầu bảng chỉ còn nút "Năm nay" ở nửa trái`, async ({ app }) => {
      await app.goto(url);
      await openPanel(app);

      await expect(panel(app)).not.toContainText('Chọn trong');
      await expect(app.locator('.hhd-year-panel__range')).toHaveCount(0);

      const today = panel(app).locator('.hhd-year-panel__today');
      await expect(today).toHaveText('Năm nay');

      const inLeftHalf = await panel(app).evaluate((root) => {
        const head = root.querySelector('.hhd-year-panel__head') as HTMLElement;
        const button = root.querySelector('.hhd-year-panel__today') as HTMLElement;
        const headBox = head.getBoundingClientRect();
        return {
          children: head.children.length,
          leftHalf: button.getBoundingClientRect().right <= headBox.left + headBox.width / 2 + 1,
        };
      });
      expect(inLeftHalf.children, `${label}: đầu bảng chỉ còn một phần tử`).toBe(1);
      expect(inLeftHalf.leftHalf, `${label}: "Năm nay" phải nằm ở nửa trái`).toBe(true);
    });

    test(`${label}: giới hạn năm máy chủ ± 100 giữ nguyên sau khi bỏ chữ`, async ({ app }) => {
      await app.goto(url);
      await openPanel(app);

      // Hai ô sát mép trong khoảng bấm được; hai ô vừa vượt mép thì khóa.
      await expect(panelYear(app, MAX_YEAR)).toBeEnabled();
      await expect(panelYear(app, MIN_YEAR)).toBeEnabled();
      await expect(panelYear(app, MAX_YEAR + 1)).toBeDisabled();
      await expect(panelYear(app, MIN_YEAR - 1)).toBeDisabled();
    });
  }
});
