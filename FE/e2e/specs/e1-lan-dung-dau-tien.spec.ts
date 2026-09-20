import path from 'node:path';

import {
  dismissToasts,
  ELIGIBLE_COLUMN,
  expect,
  headerToday,
  headerUnit,
  loginThroughUi,
  MEMBER_COLUMN,
  modal,
  searchMember,
  navItem,
  PERIOD_COLUMN,
  readTable,
  tableRows,
  test,
  uncoveredBadge,
} from '../fixtures/app';
import { daysRemainingToUpcoming, displayDate } from '../fixtures/clock';
import { downloadExcel } from '../fixtures/download';
import { ADMIN, EXCEL_DIR, UNIT_NAME } from '../fixtures/env';
import { coreMember, eligibility, MAIN_PERIODS, missed, scenario } from '../fixtures/expected';
import { pickDayMonth } from '../fixtures/pickers';

/**
 * E2E-1 · Lần dùng đầu tiên (UC-00, UC-13, UC-50, UC-31, UC-24, UC-10, UC-11).
 *
 * Trạng thái đầu: kho rỗng hoàn toàn — không đảng viên, không đợt, cài đặt
 * mặc định. Luồng đi trọn từ màn Đăng nhập tới lúc Dashboard hiện đúng đợt sắp
 * tới và đúng danh sách, đúng cách một cán bộ dùng hệ thống lần đầu.
 */

const CASE = scenario('core_default_T0');

test('E2E-1 · Lần dùng đầu tiên: đăng nhập → cài đặt → tạo đợt → import → Dashboard', async ({
  page,
  api,
}) => {
  await api.resetAll();

  await test.step('E1-01 · Mở ứng dụng khi chưa đăng nhập thì bị đưa về màn Đăng nhập', async () => {
    await page.goto('/');
    await expect(page).toHaveURL(/\/dang-nhap$/);
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toHaveCount(0);
  });

  await test.step('E1-02 · Đăng nhập sai báo một câu chung, vẫn ở màn Đăng nhập', async () => {
    await loginThroughUi(page, ADMIN.username, 'mat_khau_sai_hoan_toan');

    const error = page.getByTestId('login-error');
    await expect(error).toBeVisible();
    await expect(error).toContainText('Sai tài khoản hoặc mật khẩu');
    // Câu chung, không chỉ ra ô nào sai.
    await expect(error).not.toContainText('mật khẩu sai');
    await expect(page).toHaveURL(/\/dang-nhap$/);
  });

  await test.step('E1-03 · Đăng nhập đúng thì vào Dashboard', async () => {
    await page.getByLabel('Mật khẩu').fill(ADMIN.password);
    await page.getByRole('button', { name: 'Đăng nhập' }).click();

    await expect(page).not.toHaveURL(/\/dang-nhap/);
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible();
  });

  await test.step('E1-04 · Dashboard trống hiện khối hướng dẫn 3 bước và hai cảnh báo', async () => {
    await expect(page.getByText('Bắt đầu với ba bước')).toBeVisible();
    await expect(page.getByText('Kiểm tra cài đặt mốc')).toBeVisible();
    await expect(page.getByText('Tạo các đợt trong năm')).toBeVisible();
    await expect(page.getByText('Nạp danh sách đảng viên')).toBeVisible();

    await expect(page.getByText('Chưa có đảng viên nào')).toBeVisible();
    await expect(page.getByText('Chưa cài đợt trao huy hiệu')).toBeVisible();

    // Chưa có ai thì không có ai bị sót — menu không được hiện badge.
    await expect(uncoveredBadge(page)).toHaveCount(0);
    // Khối hướng dẫn thay chỗ cho bảng đủ điều kiện.
    await expect(page.getByText('Danh sách đủ điều kiện')).toHaveCount(0);
  });

  await test.step('E1-05 · Bấm bước 1 của khối hướng dẫn thì sang màn Cài đặt', async () => {
    await page.getByRole('button', { name: 'Mở Cài đặt' }).click();
    await expect(page).toHaveURL(/\/cai-dat$/);
    await expect(page.getByRole('heading', { name: 'Cài đặt' })).toBeVisible();
  });

  const preview = page.locator('.hhd-settings__preview');

  await test.step('E1-06 · Cài đặt hiện 30 / 90 / 5 và xem trước 13 mốc', async () => {
    await expect(page.getByLabel('Bắt đầu (năm)', { exact: true })).toHaveValue('30');
    await expect(page.getByLabel('Kết thúc (năm)', { exact: true })).toHaveValue('90');
    await expect(page.getByLabel('Bước (năm)', { exact: true })).toHaveValue('5');

    await expect(preview).toContainText(`${CASE.milestoneCount} mốc`);
    for (const milestone of [30, 35, 90]) {
      await expect(preview.getByText(`${milestone} năm`, { exact: true })).toBeVisible();
    }
    expect(CASE.milestones).toEqual([30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90]);
  });

  await test.step('E1-07 · Gõ Bước = 10 thì xem trước đổi ngay, nhưng chưa lưu nên dữ liệu chưa đổi', async () => {
    await page.getByLabel('Bước (năm)', { exact: true }).fill('10');
    await expect(preview).toContainText('7 mốc');
    await expect(preview.getByText('35 năm', { exact: true })).toHaveCount(0);

    // Chưa bấm Lưu thì máy chủ vẫn giữ nguyên cài đặt cũ.
    expect((await api.settings()).stepYears).toBe(5);
  });

  await test.step('E1-08 · Đổi lại Bước = 5, nhập Tên đơn vị rồi Lưu; header hiện tên đơn vị', async () => {
    await page.getByLabel('Bước (năm)', { exact: true }).fill('5');
    await page.getByLabel('Tên đơn vị', { exact: true }).fill(UNIT_NAME);
    await page.getByRole('button', { name: 'Lưu', exact: true }).click();

    await expect(page.getByText('Đã lưu cài đặt')).toBeVisible();
    await expect(headerUnit(page)).toHaveText(UNIT_NAME);

    const saved = await api.settings();
    expect([saved.startYears, saved.endYears, saved.stepYears]).toEqual([30, 90, 5]);
    expect(saved.unitName).toBe(UNIT_NAME);
  });

  await test.step('E1-09 · Tạo 4 đợt; bảng sắp theo Từ ngày, trạng thái đúng QT11', async () => {
    await dismissToasts(page);
    await navItem(page, 'Đợt trao huy hiệu').click();
    await expect(page).toHaveURL(/\/dot-trao-huy-hieu$/);

    // Cố ý tạo lệch thứ tự để chứng minh bảng tự sắp theo Từ ngày.
    const creationOrder = [...MAIN_PERIODS].reverse();
    for (const period of creationOrder) {
      const addButton = page.getByRole('button', { name: /^\+ Thêm đợt( đầu tiên)?$/ }).first();
      await addButton.click();

      const form = modal(page);
      await form.getByLabel('Tên đợt').fill(period.name);
      await pickDayMonth(page, form.getByLabel('Từ ngày'), period.fromDay, period.fromMonth);
      await pickDayMonth(page, form.getByLabel('Đến ngày'), period.toDay, period.toMonth);
      await expect(form.getByText('Chỉ lưu ngày/tháng.')).toBeVisible();
      await form.getByRole('button', { name: 'Thêm đợt', exact: true }).click();
      await expect(form).toBeHidden();
      await dismissToasts(page);
    }

    await expect(tableRows(page)).toHaveCount(4);
    const rows = await readTable(page);
    expect(rows.map((row) => row[PERIOD_COLUMN.name])).toEqual(
      MAIN_PERIODS.map((period) => period.name),
    );
    expect(
      rows.map((row) => `${row[PERIOD_COLUMN.fromDisplay]} – ${row[PERIOD_COLUMN.toDisplay]}`),
    ).toEqual(MAIN_PERIODS.map((period) => period.label));

    // Ba đợt đã qua, đợt cuối sắp tới kèm số ngày đếm ngược (QT11).
    const serverToday = (await api.dashboard()).today;
    const daysLeft = daysRemainingToUpcoming(serverToday);
    expect(rows.map((row) => row[PERIOD_COLUMN.status])).toEqual([
      'Đã qua',
      'Đã qua',
      'Đã qua',
      `Sắp tới · ${daysLeft} ngày`,
    ]);
    expect(CASE.periodStatuses.map((item) => item.status)).toEqual([
      'Đã qua',
      'Đã qua',
      'Đã qua',
      'Sắp tới',
    ]);
  });

  await test.step('E1-10 · Banner liệt kê đúng 5 khoảng trống, dải độ phủ có vạch Hôm nay', async () => {
    const banner = page.locator('.hhd-periods__banner');
    await expect(banner).toContainText('Các đợt chưa phủ kín cả năm');

    const gaps = CASE.gapsByYear['2026'];
    expect(gaps).toHaveLength(5);
    for (const gap of gaps) {
      await expect(banner).toContainText(
        `${gap.fromDisplay.slice(0, 5)}–${gap.toDisplay.slice(0, 5)}`,
      );
    }

    const serverToday = (await api.dashboard()).today;
    const strip = page.getByLabel('Độ phủ trong năm 2026');
    await expect(strip).toBeVisible();
    await expect(strip.locator('.hhd-coverage__today-chip')).toHaveText(
      `Hôm nay ${displayDate(serverToday).slice(0, 5)}`,
    );
  });

  await test.step('E1-11 · Tải file mẫu: đúng 4 cột đúng thứ tự, có dòng ví dụ', async () => {
    await navItem(page, 'Đảng viên').click();
    await page.getByRole('button', { name: 'Import Excel' }).click();
    await expect(page).toHaveURL(/\/dang-vien\/import$/);

    const template = await downloadExcel(page, () =>
      page.getByRole('button', { name: 'Tải file mẫu' }).click(),
    );
    expect(template.rows[0]).toEqual([
      'Họ tên',
      'Ngày sinh',
      'Giới tính',
      'Ngày vào Đảng chính thức',
    ]);
    // Có ít nhất một dòng ví dụ, ngày ghi kiểu dd/MM/yyyy.
    expect(template.rows.length).toBeGreaterThan(1);
    expect(template.rows[1][3]).toMatch(/^\d{2}\/\d{2}\/\d{4}$/);
  });

  await test.step('E1-12 · Xem trước file lõi: 32 người mới, 0 dòng lỗi, có cảnh báo không kiểm trùng', async () => {
    await page.setInputFiles('input[type="file"]', path.join(EXCEL_DIR, 'core-hop-le.xlsx'));
    await page.getByRole('button', { name: 'Tiếp tục ›' }).click();

    const summary = page.locator('.hhd-import__summary');
    await expect(summary).toContainText('Sẽ thêm 32 người mới · 0 dòng lỗi bị bỏ qua');
    await expect(summary).toContainText('Hệ thống không kiểm tra trùng');

    // Xem trước không được ghi gì vào kho (QT9).
    expect((await api.dashboard()).memberCount).toBe(0);
  });

  await test.step('E1-13 · Nạp: đã thêm 32 người, bỏ qua 0 dòng lỗi', async () => {
    await page.getByRole('button', { name: 'Nạp 32 dòng hợp lệ' }).click();
    const result = page.locator('.hhd-import__result');
    await expect(result).toContainText('Đã thêm 32 người');
    await expect(result).toContainText('Bỏ qua 0 dòng lỗi');
  });

  await test.step('E1-14 · Danh sách đảng viên: 32 người, ô trống hiện —, người vượt mốc lớn nhất có Mốc kế tiếp —', async () => {
    await page.getByRole('button', { name: 'Về danh sách đảng viên' }).click();
    await expect(page).toHaveURL(/\/dang-vien$/);
    await expect(page.getByText('32 người · Tuổi đảng tính đến hôm nay')).toBeVisible();

    // Ngày sinh và Giới tính trống hiện dấu gạch ngang, không để ô trắng.
    const blank = coreMember('E01');
    await searchMember(page, blank.fullName, blank.fullName);
    await expect(tableRows(page)).toHaveCount(1);
    let row = (await readTable(page))[0];
    expect(row[MEMBER_COLUMN.fullName]).toBe(blank.fullName);
    expect(row[MEMBER_COLUMN.gender], 'Giới tính trống phải hiện dấu gạch ngang').toBe('—');
    expect(row[MEMBER_COLUMN.dateOfBirth], 'Ngày sinh trống phải hiện dấu gạch ngang').toBe('—');

    for (const code of ['M01', 'M02']) {
      const member = coreMember(code);
      await searchMember(page, member.fullName, member.fullName);
      await expect(tableRows(page)).toHaveCount(1);
      row = (await readTable(page))[0];
      expect(row[MEMBER_COLUMN.fullName]).toBe(member.fullName);
      expect(row[MEMBER_COLUMN.partyAge]).toBe(String(member.partyAge));
      expect(
        row[MEMBER_COLUMN.nextMilestone],
        `${member.fullName} đã vượt mốc lớn nhất nên Mốc kế tiếp phải là — (QT3a)`,
      ).toBe('—');
      expect(
        row[MEMBER_COLUMN.nextMilestoneDate],
        `${member.fullName} không có ngày tròn mốc kế tiếp`,
      ).toBe('—');
    }

    await page.getByLabel('Tìm theo họ tên').fill('');
    await expect(page.getByText('32 người · Tuổi đảng tính đến hôm nay')).toBeVisible();
    await expect(tableRows(page)).toHaveCount(20);
  });

  await test.step('E1-15 · Dashboard hiện đúng đợt sắp tới, đúng khoảng ngày, đúng số người và phân bổ mốc', async () => {
    await navItem(page, 'Dashboard').click();
    const upcoming = CASE.upcomingPeriod!;
    const list = eligibility('core_default_T0', upcoming.code, 2026);

    const card = page.locator('.hhd-dashboard__card').first();
    await expect(card.locator('.hhd-dashboard__period-name')).toHaveText(upcoming.name);
    await expect(card.locator('.hhd-dashboard__period-range')).toHaveText(
      `${upcoming.boundFromDisplay} – ${upcoming.boundToDisplay}`,
    );
    await expect(card.locator('.hhd-dashboard__count-value')).toHaveText(String(list.total));

    const serverToday = (await api.dashboard()).today;
    await expect(card.locator('.hhd-dashboard__status')).toHaveText(
      `Còn ${daysRemainingToUpcoming(serverToday)} ngày`,
    );

    for (const [milestone, count] of Object.entries(list.byMilestone)) {
      await expect(card.locator('.hhd-dashboard__pills')).toContainText(`${milestone} năm${count}`);
    }
  });

  await test.step('E1-16 · Bảng Dashboard đúng 6 dòng, đúng thứ tự Mốc rồi Họ tên, đúng ngày tròn mốc', async () => {
    const list = eligibility('core_default_T0', CASE.upcomingPeriod!.code, 2026);
    const panel = page.locator('.hhd-dashboard__panel');

    await expect(tableRows(panel)).toHaveCount(list.total);
    const rows = await readTable(panel);
    expect(rows.map((row) => row[ELIGIBLE_COLUMN.fullName])).toEqual(
      list.rows.map((item) => item.fullName),
    );
    expect(rows.map((row) => row[ELIGIBLE_COLUMN.milestoneDate])).toEqual(
      list.rows.map((item) => item.anniversaryDisplay),
    );
    expect(rows.map((row) => row[ELIGIBLE_COLUMN.milestone])).toEqual(
      list.rows.map((item) => `${item.milestone} năm`),
    );
  });

  await test.step('E1-17 · Badge "Chưa thuộc đợt nào" trên menu bằng 7', async () => {
    await expect(uncoveredBadge(page)).toHaveText(String(CASE.badgeCurrentYear));
    expect((await api.unassignedCount()).count).toBe(CASE.badgeCurrentYear);
  });

  await test.step('E1-18 · Cảnh báo Dashboard nói đúng số người bị sót và dẫn sang màn M4', async () => {
    const alert = page.locator('.hhd-dashboard__alert', {
      hasText: 'người chưa thuộc đợt nào',
    });
    await expect(alert).toContainText(`${CASE.badgeCurrentYear} người chưa thuộc đợt nào`);
    await alert.getByRole('button', { name: 'Xem →' }).click();

    await expect(page).toHaveURL(/\/chua-thuoc-dot-nao$/);
    await expect(tableRows(page)).toHaveCount(missed('core_default_T0', 2026).total);
  });

  await test.step('E1-19 · Đăng xuất rồi mở lại URL Dashboard thì bị đưa về Đăng nhập', async () => {
    await page.getByRole('button', { name: 'Đăng xuất' }).click();
    await expect(page).toHaveURL(/\/dang-nhap$/);

    await page.goto('/');
    await expect(page).toHaveURL(/\/dang-nhap$/);
    await expect(page.getByRole('heading', { name: 'Dashboard' })).toHaveCount(0);
    await expect(headerToday(page)).toHaveCount(0);
  });
});
