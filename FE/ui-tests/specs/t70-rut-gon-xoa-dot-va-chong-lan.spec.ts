import type { Page } from '@playwright/test';

import { DETAIL_PERIOD, expect, mockData, test } from '../fixtures/app';
import { SERVER_TODAY, SERVER_YEAR } from '../fixtures/data';

/**
 * T70 — hộp xóa đợt còn một dòng tiêu đề, cảnh báo chồng lấn còn một câu.
 *
 * Chủ dự án thấy hai thứ này ghi quá nhiều chữ: hộp xóa có thêm một đoạn giải
 * thích dài, còn cảnh báo chồng lấn nêu cả khoảng ngày, cả cặp đợt trong ngoặc,
 * rồi thêm một câu hệ quả. Bốn ca dưới đây giữ cho bản rút gọn không quay lại.
 *
 * Dữ liệu giả cho cảnh báo dựng ngay trong tệp này bằng `page.route` đăng ký
 * SAU fixture — Playwright ưu tiên route đăng ký sau — để không sửa
 * `fixtures/**` mà các ca khác đang dùng chung.
 */

/** Vỏ `{ message, data }` của mọi phản hồi thành công (mục 1.3 hợp đồng API). */
function envelope(data: unknown) {
  return {
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify({ message: null, data }),
  };
}

/**
 * Ba đợt và hai cặp chồng lấn, đợt GIỮA dính cả hai cặp — đúng bộ đợt trong ảnh
 * chủ dự án gửi. Tên đợt giữa cố tình không có chữ "Đợt" để thấy rõ câu cảnh báo
 * chỉ nối tên, không tự thêm chữ nào.
 */
const OVERLAP_PERIODS = {
  first: { id: 't70-dot-02-9', name: 'Trao Huy hiệu Đảng đợt ngày 02/9' },
  middle: { id: 't70-khu-vuc-pickleball', name: 'Khu vực Pickleball' },
  last: { id: 't70-dot-07-11', name: 'Trao Huy hiệu Đảng đợt ngày 07/11' },
} as const;

const OVERLAPS = [
  {
    firstPeriodId: OVERLAP_PERIODS.first.id,
    firstPeriodName: OVERLAP_PERIODS.first.name,
    secondPeriodId: OVERLAP_PERIODS.middle.id,
    secondPeriodName: OVERLAP_PERIODS.middle.name,
    fromDate: `${SERVER_YEAR}-09-28`,
    toDate: `${SERVER_YEAR}-09-30`,
    fromDisplay: '28/09',
    toDisplay: '30/09',
  },
  {
    firstPeriodId: OVERLAP_PERIODS.middle.id,
    firstPeriodName: OVERLAP_PERIODS.middle.name,
    secondPeriodId: OVERLAP_PERIODS.last.id,
    secondPeriodName: OVERLAP_PERIODS.last.name,
    fromDate: `${SERVER_YEAR}-10-01`,
    toDate: `${SERVER_YEAR}-10-09`,
    fromDisplay: '01/10',
    toDisplay: '09/10',
  },
];

/** Một khoảng trống để kiểm dòng "chưa phủ kín" vẫn giữ nguyên câu chữ cũ. */
const GAPS = [
  {
    fromDate: `${SERVER_YEAR}-02-04`,
    toDate: `${SERVER_YEAR}-02-28`,
    fromDisplay: '04/02',
    toDisplay: '28/02',
  },
];

/** Câu cảnh báo chồng lấn sau khi rút gọn — mỗi đợt đúng một lần. */
const OVERLAP_SENTENCE = `Có đợt chồng lấn nhau — ${OVERLAP_PERIODS.first.name}, ${OVERLAP_PERIODS.middle.name}, ${OVERLAP_PERIODS.last.name}`;

/** Dòng chưa phủ kín ở banner màn Đợt — không thuộc việc rút gọn, phải y như cũ. */
const GAP_SENTENCE =
  'Các đợt chưa phủ kín cả năm — trống 04/02–28/02 — người tròn mốc trong các khoảng này sẽ xuất hiện ở “Chưa thuộc đợt nào”.';

/** Ba đợt giả ở dạng đầy đủ để bảng màn Đợt dựng được dòng cho chúng. */
function overlapPeriodRows() {
  const template = mockData.periods[0];
  const ranges = [
    { from: [1, 6], to: [30, 9] },
    { from: [28, 9], to: [9, 10] },
    { from: [1, 10], to: [30, 11] },
  ] as const;
  const pad = (value: number) => String(value).padStart(2, '0');

  return [OVERLAP_PERIODS.first, OVERLAP_PERIODS.middle, OVERLAP_PERIODS.last].map(
    (period, index) => {
      const [fromDay, fromMonth] = ranges[index].from;
      const [toDay, toMonth] = ranges[index].to;
      return {
        ...template,
        id: period.id,
        name: period.name,
        fromDay,
        fromMonth,
        toDay,
        toMonth,
        fromDisplay: `${pad(fromDay)}/${pad(fromMonth)}`,
        toDisplay: `${pad(toDay)}/${pad(toMonth)}`,
        fromDate: `${SERVER_YEAR}-${pad(fromMonth)}-${pad(fromDay)}`,
        toDate: `${SERVER_YEAR}-${pad(toMonth)}-${pad(toDay)}`,
      };
    },
  );
}

/**
 * Chặn thêm hai endpoint có cảnh báo, các endpoint khác để fixture xử lý tiếp
 * bằng `route.fallback()`.
 */
async function installOverlapWarnings(page: Page): Promise<void> {
  await page.route('**/api/**', async (route) => {
    const path = new URL(route.request().url()).pathname.replace(/^.*\/api/, '');

    if (path === '/AwardPeriods') {
      const periods = overlapPeriodRows();
      return route.fulfill(
        envelope({
          year: SERVER_YEAR,
          today: SERVER_TODAY,
          totalCount: periods.length,
          periods,
          warnings: { overlaps: OVERLAPS, gaps: GAPS },
          coverage: { segments: [] },
        }),
      );
    }

    if (path === '/Dashboard') {
      return route.fulfill(
        envelope({
          today: SERVER_TODAY,
          currentYear: SERVER_YEAR,
          unitName: 'Đảng ủy Phường Kiểm Thử',
          memberCount: 1342,
          periodCount: 3,
          upcomingPeriod: null,
          eligibleMembers: [],
          warnings: {
            noMembers: false,
            noPeriods: false,
            unassignedYear: SERVER_YEAR,
            unassignedCount: 0,
            overlaps: OVERLAPS,
            gaps: GAPS,
          },
        }),
      );
    }

    return route.fallback();
  });
}

const DELETE_MODAL = '.ant-modal-confirm-confirm';

/** Ghi lại mọi lời gọi DELETE để khẳng định nút "Để lại" không xóa gì. */
function trackDeleteCalls(page: Page): string[] {
  const calls: string[] = [];
  page.on('request', (request) => {
    if (request.method() === 'DELETE') calls.push(request.url());
  });
  return calls;
}

/** Hộp xác nhận xóa đợt: đúng một dòng tiêu đề, không đoạn nội dung nào. */
async function expectDeleteBox(page: Page, periodName: string): Promise<void> {
  const box = page.locator(DELETE_MODAL);
  await expect(box).toBeVisible();
  await expect(box.locator('.ant-modal-confirm-title')).toHaveText(`Xóa đợt “${periodName}”?`);
  await expect(box.locator('.anticon-exclamation-circle')).toBeVisible();

  // Vùng nội dung của antd vẫn được dựng nhưng phải trống không chữ nào.
  await expect(box.locator('.ant-modal-confirm-content')).toHaveText('');
  await expect(box).not.toContainText('xóa khỏi mọi năm');
  await expect(box).not.toContainText('Chưa thuộc đợt nào');

  await expect(box.getByRole('button', { name: 'Để lại' })).toBeVisible();
  await expect(box.getByRole('button', { name: 'Xóa đợt này' })).toBeVisible();
}

test('ca 1 · hộp xóa đợt ở màn Đợt chỉ còn dòng tiêu đề', async ({ app }) => {
  const deleteCalls = trackDeleteCalls(app);
  const period = mockData.periods[0];

  await app.goto('/dot-trao-huy-hieu');
  await app.getByLabel(`Xóa ${period.name}`, { exact: true }).click();

  await expectDeleteBox(app, period.name);

  await app.locator(DELETE_MODAL).getByRole('button', { name: 'Để lại' }).click();
  await expect(app.locator(DELETE_MODAL)).toBeHidden();
  expect(deleteCalls).toEqual([]);
});

test('ca 2 · hộp xóa đợt ở trang Chi tiết đợt giống hệt màn Đợt', async ({ app }) => {
  const deleteCalls = trackDeleteCalls(app);

  await app.goto(`/dot-trao-huy-hieu/${DETAIL_PERIOD.id}`);
  await app.getByRole('button', { name: 'Xóa', exact: true }).click();

  await expectDeleteBox(app, DETAIL_PERIOD.name);

  await app.locator(DELETE_MODAL).getByRole('button', { name: 'Để lại' }).click();
  await expect(app.locator(DELETE_MODAL)).toBeHidden();
  expect(deleteCalls).toEqual([]);
});

test('ca 3 · banner màn Đợt nêu mỗi đợt chồng lấn một lần, không khoảng ngày', async ({ app }) => {
  await installOverlapWarnings(app);
  await app.goto('/dot-trao-huy-hieu');

  const banner = app.locator('.hhd-periods__banner');
  const lines = banner.locator('.ant-alert-message > div > div');

  await expect(lines.first()).toHaveText(OVERLAP_SENTENCE);
  // Dòng chưa phủ kín không thuộc việc rút gọn này.
  await expect(lines.nth(1)).toHaveText(GAP_SENTENCE);

  // Tên đợt giữa dính cả hai cặp nhưng chỉ được nêu một lần.
  const overlapLine = (await lines.first().innerText()).trim();
  expect(overlapLine.split(OVERLAP_PERIODS.middle.name)).toHaveLength(2);
  expect(overlapLine).not.toContain('28/09');
  expect(overlapLine).not.toContain('09/10');
});

test('ca 4 · thẻ cảnh báo ở Dashboard dùng đúng câu của màn Đợt', async ({ app }) => {
  await installOverlapWarnings(app);
  await app.goto('/');

  const card = app.locator('.hhd-dashboard__alert', { hasText: 'Có đợt chồng lấn nhau' });
  await expect(card.locator('.hhd-dashboard__alert-text')).toHaveText(OVERLAP_SENTENCE);

  await card.getByRole('button', { name: 'Điều chỉnh →' }).click();
  await expect(app).toHaveURL(/\/dot-trao-huy-hieu$/);
});
