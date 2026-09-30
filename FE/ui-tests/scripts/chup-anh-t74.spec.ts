import type { Page } from '@playwright/test';

import { DETAIL_PERIOD, expect, mockData, test } from '../fixtures/app';
import { notedRowsOf, SERVER_YEAR } from '../fixtures/data';

/**
 * Chụp ảnh màn hình làm bằng chứng cho báo cáo kiểm thử T74 (ghi chú đảng viên).
 *
 * Không kiểm hành vi — việc đó là của `specs/t74-ghi-chu.spec.ts`. Ở đây chỉ mở
 * đúng những màn mà "Định nghĩa hoàn thành" của HUYH-85 đòi ảnh, ở hai khung
 * nhìn 1366x650 (laptop của chủ dự án) và 1440x900 (bộ thiết kế).
 *
 * Chạy: T50_PORT=4185 npx playwright test -c ui-tests/playwright.anh.config.ts
 */

/** Thư mục ảnh — nằm trong `.artifacts` nên không lọt vào git. */
const SHOTS = 'ui-tests/.artifacts/anh-t74';

/** Vỏ `{ message, data }` của mọi phản hồi thành công (mục 1.3 hợp đồng API). */
function envelope(data: unknown) {
  return {
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify({ message: null, data }),
  };
}

const NOTED = notedRowsOf(mockData.eligible);
const WITH_NOTE = NOTED[0];
const WITHOUT_NOTE = mockData.eligible.find((row) => row.note === null)!;

/** Màn Đảng viên rút gọn còn hai người: một đã ghi chú, một chưa. */
async function installTwoMembers(page: Page): Promise<void> {
  const rows = [WITH_NOTE, WITHOUT_NOTE].map((row) => ({
    id: row.partyMemberId,
    fullName: row.fullName,
    dateOfBirth: row.dateOfBirth,
    gender: row.gender,
    officialAdmissionDate: row.officialAdmissionDate,
    partyAgeYears: row.milestone,
    nextMilestone: row.milestone + 5,
    nextMilestoneDate: row.milestoneDate,
    createdAt: `${SERVER_YEAR}-01-01T00:00:00Z`,
    updatedAt: `${SERVER_YEAR}-01-01T00:00:00Z`,
    note: row.note,
    noteUpdatedAt: row.noteUpdatedAt,
  }));
  await page.route('**/api/**', async (route) => {
    const path = new URL(route.request().url()).pathname.replace(/^.*\/api/, '');
    if (path !== '/PartyMembers') return route.fallback();
    return route.fulfill(
      envelope({
        pagedData: rows,
        pageInfo: { current: 1, pageSize: 20, totalCount: rows.length, totalPages: 1 },
      }),
    );
  });
}

async function shoot(page: Page, name: string): Promise<void> {
  // Ant Design mở tooltip và modal bằng hiệu ứng mờ dần; chụp ngay thì ảnh bắt
  // được khung dở dang. Chờ một nhịp ngắn cho hiệu ứng chạy xong.
  await page.waitForTimeout(500);
  await page.screenshot({ path: `${SHOTS}/${name}.png` });
}

test.describe('khung laptop 1366x650', () => {
  test.use({ viewport: { width: 1366, height: 650 } });

  test('màn Đảng viên, hộp ghi chú và hai form', async ({ app }) => {
    await installTwoMembers(app);
    await app.goto('/dang-vien');

    await app.getByLabel(`Ghi chú ${WITH_NOTE.fullName}`, { exact: true }).hover();
    await expect(app.getByRole('tooltip').first()).toBeVisible();
    await shoot(app, '01-dang-vien-tooltip-da-co-ghi-chu');

    await app.getByRole('heading', { name: 'Đảng viên' }).hover();
    await app.getByLabel(`Ghi chú ${WITHOUT_NOTE.fullName}`, { exact: true }).hover();
    await expect(app.getByRole('tooltip', { name: 'Thêm ghi chú' })).toBeVisible();
    await shoot(app, '02-dang-vien-tooltip-chua-co-ghi-chu');

    // Tooltip đang mở che mất nút, đẩy chuột ra chỗ trống rồi mới bấm.
    await app.mouse.move(10, 10);
    await expect(app.getByRole('tooltip', { name: 'Thêm ghi chú' })).toBeHidden();
    await app.getByLabel(`Ghi chú ${WITH_NOTE.fullName}`, { exact: true }).click();
    await expect(app.locator('.ant-modal-title')).toContainText('Ghi chú —');
    await shoot(app, '03-hop-ghi-chu-mot-nguoi');
    await app.getByRole('button', { name: 'Đóng' }).click();

    await app.getByRole('button', { name: '+ Thêm', exact: true }).click();
    await expect(app.locator('.ant-modal-title')).toHaveText('Thêm đảng viên');
    await shoot(app, '04-form-them-trong');
    await app.getByRole('button', { name: 'Đóng' }).click();

    await app.getByLabel(`Sửa ${WITH_NOTE.fullName}`, { exact: true }).click();
    await expect(app.locator('.ant-modal-title')).toHaveText('Sửa đảng viên');
    await shoot(app, '05-form-sua-co-ghi-chu');
  });

  test('Dashboard, hộp đầy đủ ba trạng thái và Chi tiết đợt', async ({ app }) => {
    await app.goto('/');
    await expect(app.getByTestId('note-summary')).toBeVisible();
    await shoot(app, '06-dashboard-khoi-gon');

    await app.getByTestId('note-summary').click();
    await expect(app.locator('.hhd-note-dialog__item').first()).toBeVisible();
    await shoot(app, '07-hop-day-du-chua-tim');

    await app.getByLabel('Tìm trong ghi chú', { exact: true }).fill('Khu phố 3');
    await expect(app.locator('.hhd-note-text mark').first()).toBeVisible();
    await shoot(app, '08-hop-day-du-dang-tim-co-to-sang');

    await app.getByLabel('Tìm trong ghi chú', { exact: true }).fill('xyz không có thật');
    await expect(app.locator('.hhd-note-dialog')).toContainText('Không tìm thấy ghi chú nào khớp.');
    await shoot(app, '09-hop-day-du-khong-khop');
    await app.getByRole('button', { name: 'Đóng' }).click();

    await app.goto(`/dot-trao-huy-hieu/${DETAIL_PERIOD.id}`);
    await expect(app.getByTestId('note-summary')).toBeVisible();
    await shoot(app, '10-chi-tiet-dot-khoi-gon');
  });
});

test.describe('khung thiết kế 1440x900', () => {
  test.use({ viewport: { width: 1440, height: 900 } });

  test('form Sửa và hộp đầy đủ', async ({ app }) => {
    await installTwoMembers(app);
    await app.goto('/dang-vien');
    await app.getByLabel(`Sửa ${WITH_NOTE.fullName}`, { exact: true }).click();
    await expect(app.locator('.ant-modal-title')).toHaveText('Sửa đảng viên');
    await shoot(app, '11-1440-form-sua-co-ghi-chu');
    await app.getByRole('button', { name: 'Đóng' }).click();

    await app.goto('/');
    await app.getByTestId('note-summary').click();
    await expect(app.locator('.hhd-note-dialog__item').first()).toBeVisible();
    await shoot(app, '12-1440-hop-day-du');
  });
});
