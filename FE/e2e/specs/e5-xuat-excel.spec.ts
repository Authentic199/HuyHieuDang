import type { Page } from '@playwright/test';

import {
  dismissToasts,
  ELIGIBLE_COLUMN,
  expect,
  navItem,
  readTable,
  selectYear,
  signIn,
  tableRows,
  test,
} from '../fixtures/app';
import { displayDate } from '../fixtures/clock';
import { downloadExcel, titleLines, type ExcelDownload } from '../fixtures/download';
import { UNIT_NAME } from '../fixtures/env';
import { coreMember, eligibility, expected, missed, scenario } from '../fixtures/expected';

/**
 * E2E-5 · Xuất Excel từ cả ba nơi (UC-11, UC-34, UC-40, UC-51).
 *
 * Trạng thái đầu: 4 đợt, cài đặt 30/90/5, bộ lõi 32 người.
 * Mỗi tệp tải về đều được MỞ RA ĐỌC: tên tệp, dòng tiêu đề, số dòng và từng ô
 * phải khớp bảng đang hiện trên màn hình.
 */

const CASE = scenario('core_default_T0');
const UPCOMING = CASE.upcomingPeriod!;

/** Bảy cột của tệp xuất, đúng thứ tự Backend ghi ra. */
const FILE_HEADERS = [
  'STT',
  'Họ tên',
  'Giới tính',
  'Ngày sinh',
  'Ngày chính thức',
  'Ngày tròn mốc',
  'Mốc huy hiệu',
];

async function openEligibilityTab(page: Page, periodId: string) {
  await page.goto(`/dot-trao-huy-hieu/${periodId}`);
  await page.getByRole('tab', { name: 'Danh sách đủ điều kiện' }).click();
}

/** Viên thuốc "30 năm" trên màn hình ứng với ô "30" trong tệp. */
function stripMilestoneSuffix(cell: string): string {
  return cell.replace(/\s*năm$/, '');
}

/** So từng ô của tệp với từng ô của bảng đang hiện. */
function expectFileMatchesScreen(file: ExcelDownload, screenRows: string[][]) {
  expect(file.dataRows.length).toBe(screenRows.length);
  screenRows.forEach((screenRow, index) => {
    const fileRow = file.dataRows[index];
    expect(fileRow[ELIGIBLE_COLUMN.index]).toBe(screenRow[ELIGIBLE_COLUMN.index]);
    expect(fileRow[ELIGIBLE_COLUMN.fullName]).toBe(screenRow[ELIGIBLE_COLUMN.fullName]);
    expect(fileRow[ELIGIBLE_COLUMN.admission]).toBe(screenRow[ELIGIBLE_COLUMN.admission]);
    expect(fileRow[ELIGIBLE_COLUMN.milestoneDate]).toBe(screenRow[ELIGIBLE_COLUMN.milestoneDate]);
    expect(fileRow[ELIGIBLE_COLUMN.milestone]).toBe(
      stripMilestoneSuffix(screenRow[ELIGIBLE_COLUMN.milestone]),
    );
    // Ô trống: màn hình hiện dấu gạch ngang, tệp để RỖNG (A-707).
    for (const column of [ELIGIBLE_COLUMN.gender, ELIGIBLE_COLUMN.dateOfBirth]) {
      expect(fileRow[column]).toBe(screenRow[column] === '—' ? '' : screenRow[column]);
    }
  });
}

test('E2E-5 · Xuất Excel từ Dashboard, chi tiết đợt và Chưa thuộc đợt nào', async ({
  page,
  api,
}) => {
  await api.seedBaseline();
  await signIn(page, api);
  const serverToday = (await api.dashboard()).today;
  const exportDateLine = `Ngày xuất: ${displayDate(serverToday)}`;

  let dashboardFile!: ExcelDownload;

  await test.step('E5-01 · Dashboard → Xuất Excel cho đúng tên tệp theo quy ước', async () => {
    await page.goto('/');
    dashboardFile = await downloadExcel(page, () =>
      page.locator('.hhd-dashboard__panel').getByRole('button', { name: 'Xuất Excel' }).click(),
    );
    expect(dashboardFile.fileName).toBe(expected.exportFileNames[`${UPCOMING.name} năm 2026`]);
    expect(dashboardFile.fileName).toBe('DuDieuKien_Dot7-11_2026.xlsx');
  });

  await test.step('E5-02 · Mở tệp: dòng tiêu đề có tên đơn vị, tên đợt kèm khoảng ngày đã gắn năm, ngày xuất', async () => {
    expect(titleLines(dashboardFile)).toEqual([
      UNIT_NAME,
      `${UPCOMING.name} · ${UPCOMING.boundFromDisplay} – ${UPCOMING.boundToDisplay}`,
      exportDateLine,
    ]);
    expect(dashboardFile.headers).toEqual(FILE_HEADERS);
    expect(dashboardFile.workbook.sheets).toHaveLength(1);
  });

  await test.step('E5-03 · Nội dung tệp khớp từng ô với bảng đang hiện trên Dashboard', async () => {
    const list = eligibility('core_default_T0', UPCOMING.code, 2026);
    const panel = page.locator('.hhd-dashboard__panel');
    await expect(tableRows(panel)).toHaveCount(list.total);

    const screenRows = await readTable(panel);
    expect(dashboardFile.dataRows.length).toBe(list.total);
    expectFileMatchesScreen(dashboardFile, screenRows);
    expect(dashboardFile.dataRows.map((row) => row[ELIGIBLE_COLUMN.fullName])).toEqual(
      list.rows.map((item) => item.fullName),
    );
    await dismissToasts(page);
  });

  await test.step('E5-04 · Chi tiết Đợt 3/2 · 2026: đúng tên tệp, 5 dòng, Ngô Văn Khánh tròn mốc 28/02/2026', async () => {
    const list = eligibility('core_default_T0', 'P1', 2026);
    await openEligibilityTab(page, await api.periodIdByName('Đợt 3/2'));
    await expect(tableRows(page)).toHaveCount(list.total);

    const file = await downloadExcel(page, () =>
      page.getByRole('button', { name: 'Xuất Excel' }).click(),
    );
    expect(file.fileName).toBe(expected.exportFileNames['Đợt 3/2 năm 2026']);
    expect(file.dataRows.length).toBe(list.total);
    expect(list.total).toBe(5);

    // QT2: vào Đảng 29/02/1996, tròn 30 năm ở năm KHÔNG nhuận thu về 28/02.
    const leap = coreMember('L01');
    const row = file.dataRows.find((item) => item[ELIGIBLE_COLUMN.fullName] === leap.fullName)!;
    expect(row[ELIGIBLE_COLUMN.milestoneDate]).toBe('28/02/2026');
    expectFileMatchesScreen(file, await readTable(page));
    await dismissToasts(page);
  });

  await test.step('E5-05 · Đổi bộ chọn năm sang 2027 rồi xuất: tên tệp và khoảng ngày gắn năm 2027', async () => {
    const list = eligibility('core_default_T0', 'P1', 2027);
    await selectYear(page, 2027);
    await expect(page.locator('.hhd-eligibility__dates')).toHaveText('15/01/2027 – 05/03/2027');
    await expect(tableRows(page)).toHaveCount(list.total);

    const file = await downloadExcel(page, () =>
      page.getByRole('button', { name: 'Xuất Excel' }).click(),
    );
    expect(file.fileName).toBe('DuDieuKien_Dot3-2_2027.xlsx');
    expect(titleLines(file)).toEqual([
      UNIT_NAME,
      'Đợt 3/2 · 15/01/2027 – 05/03/2027',
      exportDateLine,
    ]);
    expect(file.dataRows.length).toBe(list.total);
    expectFileMatchesScreen(file, await readTable(page));
    await dismissToasts(page);
  });

  await test.step('E5-06 · M4 năm 2026 → Xuất Excel: đúng tên tệp, 7 dòng, có cột Khoảng trống', async () => {
    const list = missed('core_default_T0', 2026);
    await navItem(page, 'Chưa thuộc đợt nào').click();
    await expect(tableRows(page)).toHaveCount(list.total);

    const file = await downloadExcel(page, () =>
      page.getByRole('button', { name: 'Xuất Excel' }).click(),
    );
    expect(file.fileName).toBe(expected.exportFileNames['Chưa thuộc đợt nào năm 2026']);
    expect(file.headers).toEqual([...FILE_HEADERS, 'Khoảng trống']);
    expect(titleLines(file)).toEqual([UNIT_NAME, 'Chưa thuộc đợt nào · năm 2026', exportDateLine]);
    expect(file.dataRows.length).toBe(list.total);
    expect(file.dataRows.map((row) => row[1])).toEqual(list.rows.map((item) => item.fullName));
    expect(file.dataRows.map((row) => row[7])).toEqual(list.rows.map((item) => item.gapLabel));
    expectFileMatchesScreen(file, await readTable(page));
    await dismissToasts(page);
  });

  await test.step('E5-07 · Ô Ngày sinh / Giới tính của Cao Thị Thu là RỖNG trong tệp, không phải dấu gạch ngang', async () => {
    const blank = coreMember('E01');
    await openEligibilityTab(page, await api.periodIdByName('Đợt 19/5'));
    await expect(tableRows(page)).toHaveCount(eligibility('core_default_T0', 'P2', 2026).total);

    const file = await downloadExcel(page, () =>
      page.getByRole('button', { name: 'Xuất Excel' }).click(),
    );
    const row = file.dataRows.find((item) => item[ELIGIBLE_COLUMN.fullName] === blank.fullName)!;
    expect(row[ELIGIBLE_COLUMN.gender]).toBe('');
    expect(row[ELIGIBLE_COLUMN.dateOfBirth]).toBe('');
    expect(row.join('|')).not.toContain('—');
    await dismissToasts(page);
  });

  await test.step('E5-08 · Xóa Tên đơn vị rồi xuất lại: tiêu đề bỏ hẳn dòng đó, không để dòng trắng lạ', async () => {
    await navItem(page, 'Cài đặt').click();
    await page.getByLabel('Tên đơn vị', { exact: true }).fill('');
    await page.getByRole('button', { name: 'Lưu', exact: true }).click();
    await expect(page.getByText('Đã lưu cài đặt')).toBeVisible();
    await dismissToasts(page);

    await page.goto('/');
    const file = await downloadExcel(page, () =>
      page.locator('.hhd-dashboard__panel').getByRole('button', { name: 'Xuất Excel' }).click(),
    );
    expect(titleLines(file)).toEqual([
      `${UPCOMING.name} · ${UPCOMING.boundFromDisplay} – ${UPCOMING.boundToDisplay}`,
      exportDateLine,
    ]);
    // Dòng tiêu đề lùi lên một dòng, không để lại dòng trắng ở đầu tệp.
    expect(file.rows[0][0]).not.toBe('');

    await navItem(page, 'Cài đặt').click();
    await page.getByLabel('Tên đơn vị', { exact: true }).fill(UNIT_NAME);
    await page.getByRole('button', { name: 'Lưu', exact: true }).click();
    await expect(page.getByText('Đã lưu cài đặt')).toBeVisible();
    await dismissToasts(page);
  });

  await test.step('E5-09 · Xuất từ một đợt không có ai đủ điều kiện vẫn ra tệp có tiêu đề, 0 dòng dữ liệu', async () => {
    const empty = eligibility('core_default_T0', 'P1', 2025);
    expect(empty.total).toBe(0);

    await openEligibilityTab(page, await api.periodIdByName('Đợt 3/2'));
    await selectYear(page, 2025);
    await expect(
      page.getByText('Không có đảng viên nào tròn mốc trong đợt này năm 2025.'),
    ).toBeVisible();

    const file = await downloadExcel(page, () =>
      page.getByRole('button', { name: 'Xuất Excel' }).click(),
    );
    expect(file.fileName).toBe('DuDieuKien_Dot3-2_2025.xlsx');
    expect(file.headers).toEqual(FILE_HEADERS);
    expect(file.dataRows).toHaveLength(0);
    await dismissToasts(page);
  });

  await test.step('E5-10 · Nạp thêm bộ lớn: số trên giao diện là "1.232 người", dùng dấu chấm ngăn nghìn', async () => {
    const loaded = await api.seedExcel('bulk-1200.xlsx');
    expect(loaded.importedCount).toBe(expected.counts.bulk);

    await navItem(page, 'Đảng viên').click();
    await expect(
      page.getByText(
        `${expected.counts.total.toLocaleString('vi-VN')} người · Tuổi đảng tính đến hôm nay`,
      ),
    ).toBeVisible();
    await expect(page.getByText('1.232 người · Tuổi đảng tính đến hôm nay')).toBeVisible();

    // Bộ lớn không làm lệch con số của bộ lõi (ngày chính thức 2015–2020).
    await navItem(page, 'Dashboard').click();
    await expect(page.locator('.hhd-dashboard__count-value')).toHaveText(
      String(eligibility('core_default_T0', UPCOMING.code, 2026).total),
    );
  });
});
