import type { Locator, Page } from '@playwright/test';

import { DETAIL_PERIOD, expect, test } from '../fixtures/app';
import { SERVER_YEAR } from '../fixtures/data';

/**
 * T66 — Chi tiết đợt gộp thành một trang, và bộ chọn năm `‹ ›` có bảng chọn năm
 * dùng chung với màn Chưa thuộc đợt nào (quyết định của chủ dự án ngày 30/09).
 *
 * Chạy trên dữ liệu giả của `fixtures/app.ts`, nơi năm "máy chủ" là
 * `SERVER_YEAR`. Không con số nào viết cứng theo năm chạy thật: mọi mốc đều
 * suy từ năm máy chủ giả, nên sang năm ca vẫn đúng.
 */

/** Đi xa nhất 100 năm mỗi phía — khớp `REACH_YEARS` trong `YearPicker.tsx`. */
const REACH_YEARS = 100;
const MIN_YEAR = SERVER_YEAR - REACH_YEARS;
const MAX_YEAR = SERVER_YEAR + REACH_YEARS;

const DETAIL_URL = `/dot-trao-huy-hieu/${DETAIL_PERIOD.id}`;
const UNCOVERED_URL = '/chua-thuoc-dot-nao';

function yearCell(app: Page): Locator {
  return app.getByRole('button', { name: /^Chọn năm khác, đang xem/ });
}

function backButton(app: Page): Locator {
  return app.getByRole('button', { name: 'Lùi một năm' });
}

function forwardButton(app: Page): Locator {
  return app.getByRole('button', { name: 'Tiến một năm' });
}

function panel(app: Page): Locator {
  return app.locator('.hhd-year-panel');
}

/** Một ô năm trong bảng chọn — tìm theo đúng con số, không khớp một phần. */
function panelYear(app: Page, year: number): Locator {
  return panel(app).getByRole('button', { name: String(year), exact: true });
}

async function openPanel(app: Page): Promise<Locator> {
  await yearCell(app).click();
  await expect(panel(app)).toBeVisible();
  return panel(app);
}

/**
 * Chờ đúng lời gọi danh sách mang năm mong đợi, trong lúc `action` đang chạy.
 * Đây là bằng chứng năm đã đi tới tận máy chủ chứ không chỉ đổi chữ trên màn.
 */
async function expectYearRequest(
  app: Page,
  endpoint: string,
  year: number,
  action: () => Promise<void>,
): Promise<void> {
  await Promise.all([
    app.waitForRequest((request) => {
      const url = new URL(request.url());
      return url.pathname.endsWith(endpoint) && url.searchParams.get('year') === String(year);
    }),
    action(),
  ]);
}

/** Chờ danh sách của năm mới dựng xong trước khi đọc tiếp. */
async function expectYear(app: Page, year: number): Promise<void> {
  await expect(yearCell(app)).toHaveText(String(year));
}

/**
 * Hàng chứa năm đang xem lệch tâm lưới bao nhiêu, tính theo SỐ HÀNG. 0 là nằm
 * đúng giữa, 1 là lệch trọn một hàng.
 *
 * Đo bằng `getBoundingClientRect` chứ không dùng `offsetTop`: ô năm nằm trong
 * lớp phủ của Ant Design nên `offsetParent` của nó là `.ant-popover-content`,
 * không phải lưới — đó đúng là cái bẫy đã làm lưới cuộn lố gần một hàng. Lấy
 * tỉ lệ thay vì số pixel nên hiệu ứng phóng to của lớp phủ lúc mới hiện ra
 * không làm sai kết quả.
 */
async function centerOffsetInRows(app: Page): Promise<number> {
  return app.evaluate(() => {
    const grid = document.querySelector('.hhd-year-panel__grid')!;
    const cell = grid.querySelector('.hhd-year-panel__cell--selected')!;
    const cells = grid.querySelectorAll('.hhd-year-panel__cell');
    const gridBox = grid.getBoundingClientRect();
    const cellBox = cell.getBoundingClientRect();
    // Hai ô cách nhau 5 vị trí là hai hàng liền nhau — đó là chiều cao một hàng.
    const rowHeight = cells[5].getBoundingClientRect().top - cells[0].getBoundingClientRect().top;
    const offset = Math.abs(cellBox.top + cellBox.height / 2 - (gridBox.top + gridBox.height / 2));
    return offset / rowHeight;
  });
}

test.describe('Chi tiết đợt — một trang, không còn tab', () => {
  test('không còn dải tab; tiêu đề có tên đợt và khoảng ngày hằng năm; bảng hiện ngay', async ({
    app,
  }) => {
    await app.goto(DETAIL_URL);

    await expect(app.getByRole('tablist')).toHaveCount(0);
    await expect(app.getByRole('tab')).toHaveCount(0);
    await expect(app.locator('.hhd-period-info')).toHaveCount(0);

    const heading = app.locator('.hhd-page-heading__title');
    await expect(heading).toContainText(DETAIL_PERIOD.name);
    await expect(app.locator('.hhd-period-detail__range')).toHaveText(
      `${DETAIL_PERIOD.fromDisplay} – ${DETAIL_PERIOD.toDisplay} hằng năm`,
    );

    await expect(app.locator('.hhd-eligibility .ant-table-row').first()).toBeVisible();
  });
});

test.describe('Bộ chọn năm ở màn Chi tiết đợt', () => {
  test('mở trang là năm máy chủ; bấm › sang năm sau và gọi lại đúng năm', async ({ app }) => {
    await app.goto(DETAIL_URL);
    await expectYear(app, SERVER_YEAR);
    await expect(app.locator('.hhd-eligibility__range')).toContainText('Năm nay');

    await expectYearRequest(app, '/Eligibility', SERVER_YEAR + 1, () => forwardButton(app).click());
    await expectYear(app, SERVER_YEAR + 1);
    await expect(app.locator('.hhd-eligibility__range')).toContainText('Năm sau');
    await expect(app.locator('.hhd-eligibility__count')).toContainText('chuẩn bị trước');
  });

  test('bấm ‹ hai lần từ năm nay thì nhãn đọc "2 năm trước"', async ({ app }) => {
    await app.goto(DETAIL_URL);
    await expectYear(app, SERVER_YEAR);

    await backButton(app).click();
    await expectYear(app, SERVER_YEAR - 1);
    await expect(app.locator('.hhd-eligibility__range')).toContainText('Năm trước');

    await expectYearRequest(app, '/Eligibility', SERVER_YEAR - 2, () => backButton(app).click());
    await expectYear(app, SERVER_YEAR - 2);
    await expect(app.locator('.hhd-eligibility__range')).toContainText('2 năm trước');
    await expect(app.locator('.hhd-eligibility__count')).not.toContainText('chuẩn bị trước');
  });

  test('năm tương lai xa đọc "N năm nữa" và vẫn ghi "chuẩn bị trước"', async ({ app }) => {
    await app.goto(DETAIL_URL);
    await openPanel(app);
    await panelYear(app, SERVER_YEAR + 6).click();

    await expectYear(app, SERVER_YEAR + 6);
    await expect(app.locator('.hhd-eligibility__range')).toContainText('6 năm nữa');
    await expect(app.locator('.hhd-eligibility__count')).toContainText('chuẩn bị trước');
  });
});

test.describe('Giới hạn 100 năm mỗi phía', () => {
  test('tới năm máy chủ + 100 thì › bị khóa và bảng chọn hết ô bấm được ở trên', async ({
    app,
  }) => {
    await app.goto(DETAIL_URL);
    await openPanel(app);
    await panelYear(app, MAX_YEAR).click();
    await expectYear(app, MAX_YEAR);

    await expect(forwardButton(app)).toBeDisabled();
    await expect(backButton(app)).toBeEnabled();

    await openPanel(app);
    await expect(panel(app)).toContainText(`Chọn trong ${MIN_YEAR} – ${MAX_YEAR}`);
    for (let year = MAX_YEAR + 1; year <= MAX_YEAR + 3; year += 1) {
      await expect(panelYear(app, year)).toBeDisabled();
    }
    await expect(panelYear(app, MAX_YEAR)).toBeEnabled();
  });

  test('tới năm máy chủ − 100 thì ‹ bị khóa và bảng chọn hết ô bấm được ở dưới', async ({
    app,
  }) => {
    await app.goto(DETAIL_URL);
    await openPanel(app);
    await panelYear(app, MIN_YEAR).click();
    await expectYear(app, MIN_YEAR);

    await expect(backButton(app)).toBeDisabled();
    await expect(forwardButton(app)).toBeEnabled();

    await openPanel(app);
    await expect(panelYear(app, MIN_YEAR - 1)).toBeDisabled();
    await expect(panelYear(app, MIN_YEAR)).toBeEnabled();
  });
});

test.describe('Bảng chọn năm', () => {
  test('mở đúng tại năm đang xem, chọn năm xa thì đổi năm và gọi lại đúng year', async ({
    app,
  }) => {
    await app.goto(DETAIL_URL);
    await openPanel(app);

    // Năm đang xem được đánh dấu và đã nằm sẵn trong vùng nhìn, không phải cuộn.
    const selected = panel(app).locator('.hhd-year-panel__cell--selected');
    await expect(selected).toHaveText(String(SERVER_YEAR));
    await expect(selected).toBeInViewport();

    const target = SERVER_YEAR - 37;
    await expectYearRequest(app, '/Eligibility', target, () => panelYear(app, target).click());
    await expect(panel(app)).toBeHidden();
    await expectYear(app, target);
    await expect(app.locator('.hhd-eligibility__range')).toContainText('37 năm trước');
  });

  // Lưới cao hơn khung nhìn của nó, nên năm nào cũng cuộn được vào giữa; chỉ khi
  // năm sát hai đầu khoảng 1926 – 2126 mới hết chỗ cuộn, và hai năm đo ở đây thì
  // không.
  for (const offsetFromServer of [0, -37]) {
    test(`hàng của năm đang xem nằm giữa lưới (năm máy chủ ${offsetFromServer || ''})`, async ({
      app,
    }) => {
      await app.goto(DETAIL_URL);
      if (offsetFromServer !== 0) {
        await openPanel(app);
        await panelYear(app, SERVER_YEAR + offsetFromServer).click();
        await expectYear(app, SERVER_YEAR + offsetFromServer);
      }
      await openPanel(app);

      // Nửa hàng là mức chặt nhất còn đúng: lưới cuộn theo pixel nên tâm hàng
      // khó trùng tuyệt đối tâm lưới khi số hàng thấy được là số chẵn.
      await expect.poll(() => centerOffsetInRows(app), { timeout: 5_000 }).toBeLessThan(0.5);
    });
  }

  test('Esc đóng bảng mà không đổi năm, tiêu điểm về ô năm', async ({ app }) => {
    await app.goto(DETAIL_URL);
    await openPanel(app);

    await app.keyboard.press('Escape');
    await expect(panel(app)).toBeHidden();
    await expectYear(app, SERVER_YEAR);
    await expect(yearCell(app)).toBeFocused();
  });

  test('nút "Năm nay" đưa về năm của máy chủ', async ({ app }) => {
    await app.goto(DETAIL_URL);
    await openPanel(app);
    await panelYear(app, SERVER_YEAR - 12).click();
    await expectYear(app, SERVER_YEAR - 12);

    await openPanel(app);
    // Năm máy chủ khi không phải năm đang xem thì có chấm đỏ dưới số.
    await expect(panel(app).locator('.hhd-year-panel__cell--today')).toHaveText(
      String(SERVER_YEAR),
    );

    await panel(app).getByRole('button', { name: 'Năm nay' }).click();
    await expect(panel(app)).toBeHidden();
    await expectYear(app, SERVER_YEAR);
  });

  test('bàn phím: ↓ rồi Enter thì năm tăng 5', async ({ app }) => {
    await app.goto(DETAIL_URL);
    await openPanel(app);

    await app.keyboard.press('ArrowDown');
    await app.keyboard.press('Enter');

    await expect(panel(app)).toBeHidden();
    await expectYear(app, SERVER_YEAR + 5);
  });
});

test.describe('Màn Chưa thuộc đợt nào dùng cùng bộ chọn năm', () => {
  test('bấm ‹ › đổi năm và gọi lại danh sách', async ({ app }) => {
    await app.goto(UNCOVERED_URL);
    await expectYear(app, SERVER_YEAR);

    await expectYearRequest(app, '/Eligibility/Unassigned', SERVER_YEAR + 1, () =>
      forwardButton(app).click(),
    );
    await expectYear(app, SERVER_YEAR + 1);
    await expect(app.locator('.hhd-uncovered__title')).toContainText(String(SERVER_YEAR + 1));

    await expectYearRequest(app, '/Eligibility/Unassigned', SERVER_YEAR, () =>
      backButton(app).click(),
    );
    await expectYear(app, SERVER_YEAR);
  });

  test('bảng chọn năm nhảy thẳng tới năm xa', async ({ app }) => {
    await app.goto(UNCOVERED_URL);
    await openPanel(app);

    const target = SERVER_YEAR - 37;
    await expectYearRequest(app, '/Eligibility/Unassigned', target, () =>
      panelYear(app, target).click(),
    );
    await expect(panel(app)).toBeHidden();
    await expectYear(app, target);
    await expect(app.locator('.hhd-uncovered__title')).toContainText(String(target));
  });

  test('giới hạn giống hệt màn Chi tiết đợt', async ({ app }) => {
    await app.goto(UNCOVERED_URL);
    await openPanel(app);
    await expect(panel(app)).toContainText(`Chọn trong ${MIN_YEAR} – ${MAX_YEAR}`);
    await panelYear(app, MAX_YEAR).click();
    await expectYear(app, MAX_YEAR);
    await expect(forwardButton(app)).toBeDisabled();
  });
});

test.describe('Khung 1280x600', () => {
  test.use({ viewport: { width: 1280, height: 600 } });

  for (const [label, url] of [
    ['Chi tiết đợt', DETAIL_URL],
    ['Chưa thuộc đợt nào', UNCOVERED_URL],
  ] as const) {
    test(`${label}: bảng chọn nằm trọn trong khung nhìn, trang không cuộn ngang`, async ({
      app,
    }) => {
      await app.goto(url);
      await openPanel(app);
      await expect(panel(app)).toBeInViewport({ ratio: 1 });

      const overflow = await app.evaluate(
        () => document.documentElement.scrollWidth - document.documentElement.clientWidth,
      );
      expect(overflow).toBeLessThanOrEqual(0);
    });
  }
});
