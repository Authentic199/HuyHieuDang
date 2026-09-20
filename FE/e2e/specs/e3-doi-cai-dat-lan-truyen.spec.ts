import {
  dismissToasts,
  ELIGIBLE_COLUMN,
  expect,
  MEMBER_COLUMN,
  navItem,
  readTable,
  searchMember,
  signIn,
  tableRows,
  test,
  uncoveredBadge,
  UNCOVERED_COLUMN,
} from '../fixtures/app';
import { coreMember, eligibility, missed, scenario } from '../fixtures/expected';

/**
 * E2E-3 · Đổi cài đặt lan truyền (UC-50, QT1, QT5).
 *
 * Trạng thái đầu: 4 đợt, cài đặt 30/90/5, bộ lõi 32 người.
 * Luồng chứng minh QT5: danh sách đủ điều kiện KHÔNG được lưu, nên đổi dãy mốc
 * là mọi màn đổi theo ngay, không cần thao tác gì thêm.
 */

const BEFORE = scenario('core_default_T0');
const AFTER = scenario('core_step10_T0');
const UPCOMING = BEFORE.upcomingPeriod!;

/** Mở tab "Danh sách đủ điều kiện" của trang chi tiết một đợt. */
async function openEligibilityTab(page: import('@playwright/test').Page, periodId: string) {
  await page.goto(`/dot-trao-huy-hieu/${periodId}`);
  await page.getByRole('tab', { name: 'Danh sách đủ điều kiện' }).click();
}

test('E2E-3 · Đổi Bước từ 5 sang 10 lan truyền ngay sang mọi màn', async ({ page, api }) => {
  await api.seedBaseline();
  await signIn(page, api);
  const upcomingId = await api.periodIdByName(UPCOMING.name);

  const before = eligibility('core_default_T0', UPCOMING.code, 2026);
  const after = eligibility('core_step10_T0', UPCOMING.code, 2026);

  await test.step('E3-01 · Ghi lại trạng thái đầu: Dashboard 6 người, chi tiết đợt 6 người, badge 7', async () => {
    await page.goto('/');
    await expect(page.locator('.hhd-dashboard__count-value')).toHaveText(String(before.total));
    await expect(uncoveredBadge(page)).toHaveText(String(BEFORE.badgeCurrentYear));

    await openEligibilityTab(page, upcomingId);
    await expect(tableRows(page)).toHaveCount(before.total);
  });

  const preview = page.locator('.hhd-settings__preview');

  await test.step('E3-02 · Cài đặt: Bước 5 → 10 (chưa lưu) thì xem trước đổi ngay, kèm dòng nhắc ảnh hưởng', async () => {
    await navItem(page, 'Cài đặt').click();
    await expect(page.getByLabel('Bước (năm)', { exact: true })).toHaveValue('5');

    await page.getByLabel('Bước (năm)', { exact: true }).fill('10');
    await expect(preview).toContainText(`${AFTER.milestoneCount} mốc`);
    expect(AFTER.milestones).toEqual([30, 40, 50, 60, 70, 80, 90]);
    for (const milestone of AFTER.milestones) {
      await expect(preview.getByText(`${milestone} năm`, { exact: true })).toBeVisible();
    }
    await expect(preview.getByText('35 năm', { exact: true })).toHaveCount(0);

    await expect(
      page.getByText('Thay đổi ảnh hưởng ngay đến mọi danh sách đủ điều kiện'),
    ).toBeVisible();
  });

  await test.step('E3-03 · Chưa lưu thì Dashboard vẫn giữ nguyên 6 người', async () => {
    await navItem(page, 'Dashboard').click();
    await expect(page.locator('.hhd-dashboard__count-value')).toHaveText(String(before.total));
    expect((await api.settings()).stepYears).toBe(5);
  });

  await test.step('E3-04 · Về Cài đặt bấm Lưu thì lưu thành công', async () => {
    await navItem(page, 'Cài đặt').click();
    await page.getByLabel('Bước (năm)', { exact: true }).fill('10');
    await expect(preview).toContainText(`${AFTER.milestoneCount} mốc`);
    await page.getByRole('button', { name: 'Lưu', exact: true }).click();

    await expect(page.getByText('Đã lưu cài đặt')).toBeVisible();
    expect((await api.settings()).stepYears).toBe(10);
    await dismissToasts(page);
  });

  await test.step('E3-05 · Chi tiết Đợt 7/11 · 2026 còn 4 người, đúng ai ở lại ai rời đi', async () => {
    await openEligibilityTab(page, upcomingId);
    await expect(tableRows(page)).toHaveCount(after.total);
    expect(after.total).toBe(4);

    const names = (await readTable(page)).map((row) => row[ELIGIBLE_COLUMN.fullName]);
    expect(names).toEqual(after.rows.map((item) => item.fullName));

    // Hai người chỉ tròn mốc lẻ 35 và 45 nên rời danh sách khi Bước thành 10.
    expect(names).not.toContain(coreMember('S01').fullName);
    expect(names).not.toContain(coreMember('S03').fullName);
    expect(names).toContain(coreMember('S02').fullName);

    // Đối chiếu ngay với máy chủ — giao diện không được tự tính lại (T-FIX-5).
    const server = await api.get<{ totalCount: number }>(
      `/Eligibility?awardPeriodId=${upcomingId}&year=2026`,
    );
    expect(server.totalCount).toBe(after.total);
  });

  await test.step('E3-06 · Về Dashboard mà không làm gì thêm: 4 người, phân bổ mốc đổi theo', async () => {
    await navItem(page, 'Dashboard').click();
    const card = page.locator('.hhd-dashboard__card').first();
    await expect(card.locator('.hhd-dashboard__count-value')).toHaveText(String(after.total));

    for (const [milestone, count] of Object.entries(after.byMilestone)) {
      await expect(card.locator('.hhd-dashboard__pills')).toContainText(`${milestone} năm${count}`);
    }
    await expect(card.locator('.hhd-dashboard__pills')).not.toContainText('35 năm');
  });

  await test.step('E3-07 · Badge trên menu còn 6', async () => {
    await expect(uncoveredBadge(page)).toHaveText(String(AFTER.badgeCurrentYear));
    expect(AFTER.badgeCurrentYear).toBe(6);
  });

  await test.step('E3-08 · Màn Chưa thuộc đợt nào năm 2026 còn 6 dòng, không còn Ngô Thị Cẩm', async () => {
    await navItem(page, 'Chưa thuộc đợt nào').click();
    const list = missed('core_step10_T0', 2026);
    await expect(tableRows(page)).toHaveCount(list.total);

    const rows = await readTable(page);
    expect(rows.map((row) => row[UNCOVERED_COLUMN.fullName])).toEqual(
      list.rows.map((item) => item.fullName),
    );
    expect(rows.map((row) => row[UNCOVERED_COLUMN.fullName])).not.toContain(
      coreMember('S04').fullName,
    );
  });

  await test.step('E3-09 · Cột Mốc kế tiếp của danh sách Đảng viên đổi theo dãy mốc mới', async () => {
    await navItem(page, 'Đảng viên').click();

    // Người tròn 34 tuổi đảng: mốc kế tiếp 35 khi Bước = 5, thành 40 khi Bước = 10.
    const shifted = coreMember('S01', 'T0_step10');
    await searchMember(page, shifted.fullName, shifted.fullName);
    let row = (await readTable(page))[0];
    expect(row[MEMBER_COLUMN.nextMilestone]).toBe(`${shifted.nextMilestone} năm`);
    expect(row[MEMBER_COLUMN.nextMilestoneDate]).toBe(shifted.nextAnniversaryDisplay);
    expect(coreMember('S01').nextMilestone).toBe(35);
    expect(shifted.nextMilestone).toBe(40);

    // Người đã vượt mốc lớn nhất vẫn là dấu gạch ngang ở cả hai dãy mốc (QT3a).
    const beyond = coreMember('M02', 'T0_step10');
    await searchMember(page, beyond.fullName, beyond.fullName);
    row = (await readTable(page))[0];
    expect(row[MEMBER_COLUMN.nextMilestone]).toBe('—');
  });

  await test.step('E3-10 · Khôi phục mặc định 30 / 90 / 5 đưa mọi con số về đúng E3-01', async () => {
    await navItem(page, 'Cài đặt').click();
    await page.getByRole('button', { name: 'Khôi phục mặc định 30 / 90 / 5' }).click();
    await expect(page.getByText('Đã khôi phục mốc mặc định 30 / 90 / 5')).toBeVisible();

    await expect(page.getByLabel('Bắt đầu (năm)', { exact: true })).toHaveValue('30');
    await expect(page.getByLabel('Kết thúc (năm)', { exact: true })).toHaveValue('90');
    await expect(page.getByLabel('Bước (năm)', { exact: true })).toHaveValue('5');
    await expect(preview).toContainText(`${BEFORE.milestoneCount} mốc`);
    await dismissToasts(page);

    await navItem(page, 'Dashboard').click();
    await expect(page.locator('.hhd-dashboard__count-value')).toHaveText(String(before.total));
    await expect(uncoveredBadge(page)).toHaveText(String(BEFORE.badgeCurrentYear));

    await openEligibilityTab(page, upcomingId);
    await expect(tableRows(page)).toHaveCount(before.total);
  });
});
