import type { Page } from '@playwright/test';

import {
  dismissToasts,
  ELIGIBLE_COLUMN,
  expect,
  modal,
  navItem,
  PERIOD_COLUMN,
  readTable,
  signIn,
  tableRows,
  test,
  uncoveredBadge,
  UNCOVERED_COLUMN,
} from '../fixtures/app';
import { coreMember, eligibility, missed, scenario } from '../fixtures/expected';
import { pickDayMonth } from '../fixtures/pickers';

/**
 * E2E-4 · Sửa đợt lan truyền (UC-32, QT5, QT6).
 *
 * Trạng thái đầu: 4 đợt, cài đặt 30/90/5, bộ lõi 32 người.
 * Nới Đến ngày của Đợt 2/9 từ 10/09 sang 30/09 kéo Lê Văn Cường từ "Chưa thuộc
 * đợt nào" vào đúng đợt đó — mọi màn đổi theo ngay, badge giảm đúng một.
 */

const BEFORE = scenario('core_default_T0');
const AFTER = scenario('core_default_T0_widenedP3');
const WIDENED = 'Đợt 2/9';
const MOVED = coreMember('B03'); // Lê Văn Cường, tròn 30 ngày 30/09/2026

async function openEligibilityTab(page: Page, periodId: string) {
  await page.goto(`/dot-trao-huy-hieu/${periodId}`);
  await page.getByRole('tab', { name: 'Danh sách đủ điều kiện' }).click();
}

/** Sửa Đến ngày của một đợt qua modal, đúng cách cán bộ thao tác. */
async function editToDate(page: Page, periodName: string, day: number, month: number) {
  await navItem(page, 'Đợt trao huy hiệu').click();
  await page.getByRole('button', { name: `Sửa ${periodName}` }).click();

  const form = modal(page);
  await expect(
    form.getByText('Thay đổi áp dụng ngay cho năm hiện tại và các năm sau'),
  ).toBeVisible();
  await pickDayMonth(page, form.getByLabel('Đến ngày'), day, month);
  await form.getByRole('button', { name: 'Lưu thay đổi' }).click();
  await expect(form).toBeHidden();
  await expect(page.getByText('Đã lưu thay đổi')).toBeVisible();
  await dismissToasts(page);
}

test('E2E-4 · Nới Đến ngày của một đợt lan truyền ngay sang mọi màn', async ({ page, api }) => {
  await api.seedBaseline();
  await signIn(page, api);
  const widenedId = await api.periodIdByName(WIDENED);
  const upcomingId = await api.periodIdByName(BEFORE.upcomingPeriod!.name);

  const missedBefore = missed('core_default_T0', 2026);
  const missedAfter = missed('core_default_T0_widenedP3', 2026);
  const widenedBefore = eligibility('core_default_T0', 'P3', 2026);
  const widenedAfter = eligibility('core_default_T0_widenedP3', 'P3', 2026);

  await test.step('E4-01 · M4 năm 2026 có 7 người, Lê Văn Cường rơi vào khoảng giữa Đợt 2/9 và Đợt 7/11', async () => {
    await page.goto('/chua-thuoc-dot-nao');
    await expect(tableRows(page)).toHaveCount(missedBefore.total);

    const rows = await readTable(page);
    const target = rows.find((row) => row[UNCOVERED_COLUMN.fullName] === MOVED.fullName);
    const fixture = missedBefore.rows.find((item) => item.fullName === MOVED.fullName)!;
    expect(target, `${MOVED.fullName} phải nằm trong danh sách bị sót`).toBeDefined();
    expect(target![UNCOVERED_COLUMN.gap]).toBe(fixture.gapLabel);
    expect(target![UNCOVERED_COLUMN.milestoneDate]).toBe(fixture.anniversaryDisplay);
    expect(fixture.anniversaryDisplay).toBe('30/09/2026');
  });

  await test.step('E4-02 · Badge trên menu bằng 7', async () => {
    await expect(uncoveredBadge(page)).toHaveText(String(missedBefore.total));
  });

  await test.step('E4-03 · Khối gợi ý "Nới Đến ngày của đợt trước" dẫn sang màn Đợt', async () => {
    const hint = page.locator('.hhd-uncovered__hint');
    await expect(hint).toContainText(`${missedBefore.total} người dưới đây rơi vào khoảng trống`);
    await expect(hint).toContainText('Đến ngày');
    await hint.getByRole('button', { name: 'Điều chỉnh đợt →' }).click();
    await expect(page).toHaveURL(/\/dot-trao-huy-hieu$/);
  });

  await test.step('E4-04 · Sửa Đợt 2/9: Đến ngày 10/09 → 30/09, modal nhắc hiệu lực ngay', async () => {
    await editToDate(page, WIDENED, 30, 9);
  });

  await test.step('E4-05 · Bảng đợt hiện 15/08 – 30/09 và số người đủ điều kiện năm nay thành 5', async () => {
    const row = (await readTable(page)).find((item) => item[PERIOD_COLUMN.name] === WIDENED)!;
    expect(row[PERIOD_COLUMN.fromDisplay]).toBe('15/08');
    expect(row[PERIOD_COLUMN.toDisplay]).toBe('30/09');
    expect(row[PERIOD_COLUMN.eligibleCount]).toBe(`${widenedAfter.total} người`);
    expect(widenedBefore.total).toBe(4);
    expect(widenedAfter.total).toBe(5);
  });

  await test.step('E4-06 · Chi tiết Đợt 2/9 · 2026 có 5 người, trong đó có Lê Văn Cường mốc 30 ngày 30/09/2026', async () => {
    await openEligibilityTab(page, widenedId);
    await expect(tableRows(page)).toHaveCount(widenedAfter.total);

    const rows = await readTable(page);
    expect(rows.map((row) => row[ELIGIBLE_COLUMN.fullName])).toEqual(
      widenedAfter.rows.map((item) => item.fullName),
    );
    const moved = rows.find((row) => row[ELIGIBLE_COLUMN.fullName] === MOVED.fullName)!;
    expect(moved[ELIGIBLE_COLUMN.milestone]).toBe('30 năm');
    expect(moved[ELIGIBLE_COLUMN.milestoneDate]).toBe('30/09/2026');
  });

  await test.step('E4-07 · Badge trên menu còn 6', async () => {
    await expect(uncoveredBadge(page)).toHaveText(String(missedAfter.total));
    expect(missedAfter.total).toBe(6);
  });

  await test.step('E4-08 · M4 năm 2026 còn 6 dòng và không còn Lê Văn Cường', async () => {
    await navItem(page, 'Chưa thuộc đợt nào').click();
    await expect(tableRows(page)).toHaveCount(missedAfter.total);

    const names = (await readTable(page)).map((row) => row[UNCOVERED_COLUMN.fullName]);
    expect(names).toEqual(missedAfter.rows.map((item) => item.fullName));
    expect(names).not.toContain(MOVED.fullName);
  });

  await test.step('E4-09 · Banner phủ kín ở màn Đợt còn 4 khoảng trống', async () => {
    await navItem(page, 'Đợt trao huy hiệu').click();
    const banner = page.locator('.hhd-periods__banner');
    const gaps = AFTER.gapsByYear['2026'];
    expect(gaps).toHaveLength(4);

    for (const gap of gaps) {
      await expect(banner).toContainText(
        `${gap.fromDisplay.slice(0, 5)}–${gap.toDisplay.slice(0, 5)}`,
      );
    }
    // Khoảng trống 11/09–30/09 đã biến mất vì Đợt 2/9 phủ kín nó.
    await expect(banner).not.toContainText('11/09–30/09');
  });

  await test.step('E4-10 · Dashboard chuyển sang đợt đang diễn ra; Đợt 7/11 vẫn giữ nguyên 6 người', async () => {
    /**
     * Kế hoạch kiểm thử (mục 6, ca E4-10) viết "Dashboard vẫn là Đợt 7/11".
     * Điều đó KHÔNG khớp QT8: "nếu hôm nay đang nằm trong một đợt thì chính là
     * đợt đó". Nới Đến ngày sang 30/09 làm hôm nay rơi vào Đợt 2/9, nên đợt sắp
     * tới phải là Đợt 2/9 ở trạng thái "Đang diễn ra" — đúng như oracle
     * `expected.json` kịch bản `core_default_T0_widenedP3` đã tính sẵn.
     * Đây là lỗi của kế hoạch kiểm thử, không phải lỗi sản phẩm (QC-T28-02).
     */
    const expectedUpcoming = AFTER.upcomingPeriod!;
    expect(expectedUpcoming.name).toBe(WIDENED);
    expect(expectedUpcoming.status).toBe('Đang diễn ra');

    await navItem(page, 'Dashboard').click();
    const card = page.locator('.hhd-dashboard__card').first();
    await expect(card.locator('.hhd-dashboard__period-name')).toHaveText(expectedUpcoming.name);
    await expect(card.locator('.hhd-dashboard__status')).toHaveText('Đang diễn ra');
    await expect(card.locator('.hhd-dashboard__count-value')).toHaveText(
      String(widenedAfter.total),
    );

    // Đợt 7/11 không bị đụng tới: vẫn đúng 6 người như trước khi sửa.
    const untouched = eligibility('core_default_T0', BEFORE.upcomingPeriod!.code, 2026);
    await openEligibilityTab(page, upcomingId);
    await expect(tableRows(page)).toHaveCount(untouched.total);
  });

  await test.step('E4-11 · Nới tiếp Đến ngày sang 07/11 vẫn lưu được, kèm cảnh báo chồng lấn nêu đúng cặp đợt', async () => {
    await editToDate(page, WIDENED, 7, 11);

    const banner = page.locator('.hhd-periods__banner');
    await expect(banner).toContainText('Có đợt chồng lấn nhau');
    await expect(banner).toContainText(`${WIDENED} và ${BEFORE.upcomingPeriod!.name}`);

    const server = await api.periods(2026);
    expect(server.warnings.overlaps.length).toBeGreaterThan(0);
  });

  await test.step('E4-12 · Hoàn nguyên Đến ngày về 10/09 thì mọi con số trở về đúng E4-01', async () => {
    await editToDate(page, WIDENED, 10, 9);

    const row = (await readTable(page)).find((item) => item[PERIOD_COLUMN.name] === WIDENED)!;
    expect(row[PERIOD_COLUMN.toDisplay]).toBe('10/09');
    expect(row[PERIOD_COLUMN.eligibleCount]).toBe(`${widenedBefore.total} người`);
    await expect(uncoveredBadge(page)).toHaveText(String(missedBefore.total));

    await navItem(page, 'Chưa thuộc đợt nào').click();
    await expect(tableRows(page)).toHaveCount(missedBefore.total);
    const names = (await readTable(page)).map((item) => item[UNCOVERED_COLUMN.fullName]);
    expect(names).toEqual(missedBefore.rows.map((item) => item.fullName));

    await openEligibilityTab(page, upcomingId);
    await expect(tableRows(page)).toHaveCount(
      eligibility('core_default_T0', BEFORE.upcomingPeriod!.code, 2026).total,
    );
  });
});
