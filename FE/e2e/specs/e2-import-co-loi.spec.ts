import path from 'node:path';

import {
  confirmDialog,
  dismissToasts,
  expect,
  readTable,
  signIn,
  tableRows,
  test,
} from '../fixtures/app';
import { futureRowStillInvalid, isFrozen } from '../fixtures/clock';
import { downloadExcel } from '../fixtures/download';
import { EXCEL_DIR, ARTIFACT_DIR, UNIT_NAME } from '../fixtures/env';
import { excelCounts } from '../fixtures/expected';
import { readWorkbook, trimTrailingEmptyRows } from '../fixtures/xlsx';
import fs from 'node:fs';

/**
 * E2E-2 · Import có lỗi (UC-24, UC-25, QT9).
 *
 * Trạng thái đầu: đã đăng nhập, đã có 4 đợt và cài đặt, 0 đảng viên.
 * Luồng chứng minh: xem trước không ghi gì, chỉ dòng hợp lệ được nạp, dòng lỗi
 * nói rõ lý do, và nạp lại cùng một file vẫn thêm mới vì QT9 không kiểm trùng.
 */

const ERROR_FILE = 'loi-4-dong.xlsx';
const importPage = '/dang-vien/import';

test('E2E-2 · Import có lỗi: xem trước đúng, chỉ dòng hợp lệ được nạp', async ({ page, api }) => {
  await api.resetAll();
  await api.seedMainPeriods();
  await api.saveSettings({ startYears: 30, endYears: 90, stepYears: 5, unitName: UNIT_NAME });
  await signIn(page, api);

  const serverToday = (await api.dashboard()).today;
  // Dòng Excel 11 chỉ là lỗi khi ngày 20/09/2026 còn ở tương lai (xem clock.ts).
  const futureRowIsError = futureRowStillInvalid(serverToday);
  const expectedErrors = futureRowIsError ? 4 : 3;
  const expectedValid = excelCounts(ERROR_FILE).validRows + (futureRowIsError ? 0 : 1);

  if (isFrozen(serverToday)) {
    // Đồng hồ đã đóng băng đúng T0 thì phải ra đúng con số của kế hoạch kiểm thử.
    expect([expectedValid, expectedErrors]).toEqual([6, 4]);
  }

  await test.step('E2-01 · Bước 1 mô tả đủ 4 cột, dòng ví dụ và ràng buộc .xlsx ≤ 10 MB', async () => {
    await page.goto(importPage);
    await expect(page.getByText('Kéo thả file Excel vào đây')).toBeVisible();
    await expect(page.getByText('.xlsx, tối đa 10 MB')).toBeVisible();

    const columns = page.locator('.hhd-import__columns');
    await expect(columns).toContainText('Họ tên');
    await expect(columns).toContainText('Ngày sinh');
    await expect(columns).toContainText('Giới tính');
    await expect(columns).toContainText('Ngày vào Đảng chính thức');
    // Dòng ví dụ ngay dưới tiêu đề cột.
    await expect(columns).toContainText('Nguyễn Văn An');
    await expect(columns).toContainText('15/10/1996');
    await expect(page.getByText('Ngày theo định dạng')).toContainText('dd/MM/yyyy');
  });

  let templatePath = '';

  await test.step('E2-02 · File mẫu tải về khớp mau-dang-vien.xlsx về tiêu đề và thứ tự cột', async () => {
    const template = await downloadExcel(page, () =>
      page.getByRole('button', { name: 'Tải file mẫu' }).click(),
    );
    templatePath = template.savedPath;

    const reference = trimTrailingEmptyRows(
      readWorkbook(fs.readFileSync(path.join(EXCEL_DIR, 'mau-dang-vien.xlsx'))).first.rows,
    );
    expect(template.rows[0]).toEqual(reference[0]);
    expect(template.rows.length).toBe(reference.length);
  });

  await test.step('E2-03 · Nạp lại chính file mẫu vừa tải: 2 dòng hợp lệ, 0 lỗi', async () => {
    await page.setInputFiles('input[type="file"]', templatePath);
    await page.getByRole('button', { name: 'Tiếp tục ›' }).click();

    await expect(page.locator('.hhd-import__summary')).toContainText(
      'Sẽ thêm 2 người mới · 0 dòng lỗi bị bỏ qua',
    );
    await page.getByRole('button', { name: 'Nạp 2 dòng hợp lệ' }).click();
    await expect(page.locator('.hhd-import__result')).toContainText('Đã thêm 2 người');
    expect((await api.dashboard()).memberCount).toBe(2);
  });

  await test.step('E2-04 · Xóa 2 người vừa nạp thì còn 0 người', async () => {
    await page.getByRole('button', { name: 'Về danh sách đảng viên' }).click();
    await expect(tableRows(page)).toHaveCount(2);

    await page.locator('.ant-table-thead input[type="checkbox"]').check();
    await expect(page.locator('.hhd-members__selected')).toContainText('Đang chọn 2 dòng');
    await page.getByRole('button', { name: 'Xóa 2 đã chọn' }).click();

    const dialog = confirmDialog(page);
    await expect(dialog).toContainText('Xóa 2 người khỏi danh sách?');
    await dialog.getByRole('button', { name: 'Xóa 2 người' }).click();

    await expect(page.getByText('Chưa có ai trong danh sách')).toBeVisible();
    expect((await api.dashboard()).memberCount).toBe(0);
    await dismissToasts(page);
  });

  await test.step(`E2-05 · Xem trước ${ERROR_FILE}: đúng số dòng hợp lệ và số dòng lỗi`, async () => {
    await page.goto(importPage);
    await page.setInputFiles('input[type="file"]', path.join(EXCEL_DIR, ERROR_FILE));
    await page.getByRole('button', { name: 'Tiếp tục ›' }).click();

    const summary = page.locator('.hhd-import__summary');
    await expect(summary).toContainText(
      `Sẽ thêm ${expectedValid} người mới · ${expectedErrors} dòng lỗi bị bỏ qua`,
    );
    await expect(summary).toContainText('Hệ thống không kiểm tra trùng');
    // Xem trước tuyệt đối không ghi gì vào kho (QT9).
    expect((await api.dashboard()).memberCount).toBe(0);
  });

  await test.step('E2-06 · Hai tab Hợp lệ / Lỗi có số đúng và chuyển qua lại được', async () => {
    const tabs = page.locator('.hhd-import__tabs');
    await expect(tabs.getByRole('tab', { name: `Lỗi ${expectedErrors}` })).toBeVisible();
    await expect(tabs.getByRole('tab', { name: `Hợp lệ ${expectedValid}` })).toBeVisible();

    await tabs.getByRole('tab', { name: `Hợp lệ ${expectedValid}` }).click();
    await expect(tableRows(page.locator('.ant-tabs-tabpane-active'))).toHaveCount(expectedValid);

    await tabs.getByRole('tab', { name: `Lỗi ${expectedErrors}` }).click();
    await expect(tableRows(page.locator('.ant-tabs-tabpane-active'))).toHaveCount(expectedErrors);
  });

  await test.step('E2-07 · Bảng lỗi nêu đúng số dòng Excel và đúng lý do từng dòng', async () => {
    const pane = page.locator('.ant-tabs-tabpane-active');
    const headers = await pane
      .locator('.ant-table-thead th')
      .evaluateAll((cells) => cells.map((cell) => (cell.textContent ?? '').trim()));
    expect(headers).toEqual([
      'Dòng',
      'Họ tên',
      'Ngày sinh',
      'Giới tính',
      'Ngày chính thức',
      'Lý do',
    ]);

    const rows = await readTable(pane);
    const expectedRows: [string, string][] = [
      ['8', 'Thiếu họ tên'],
      ['9', 'Thiếu ngày vào Đảng chính thức'],
      ['10', 'Sai định dạng ngày (cần dd/MM/yyyy)'],
    ];
    if (futureRowIsError) expectedRows.push(['11', 'Ngày chính thức ở tương lai']);

    expect(rows.map((row) => [row[0], row[5]])).toEqual(expectedRows);
    // Ô gây lỗi giữ nguyên chữ thô đọc từ Excel để bác biết chỗ nào phải sửa.
    expect(rows[2][4]).toBe('1996-10-01');
    expect(rows[0][1]).toBe('—');
  });

  await test.step('E2-08 · Bấm Hủy thì không ghi gì vào kho, danh sách vẫn 0 người', async () => {
    await page.getByRole('button', { name: 'Hủy', exact: true }).click();
    await expect(page.getByText('Kéo thả file Excel vào đây')).toBeVisible();

    await page.getByRole('button', { name: 'Hủy', exact: true }).click();
    await expect(page).toHaveURL(/\/dang-vien$/);
    await expect(page.getByText('Chưa có ai trong danh sách')).toBeVisible();
    expect((await api.dashboard()).memberCount).toBe(0);
  });

  await test.step('E2-09 · Làm lại tới bước 2 rồi Nạp: báo đúng số thêm và số bỏ qua', async () => {
    await page.goto(importPage);
    await page.setInputFiles('input[type="file"]', path.join(EXCEL_DIR, ERROR_FILE));
    await page.getByRole('button', { name: 'Tiếp tục ›' }).click();
    await page.getByRole('button', { name: `Nạp ${expectedValid} dòng hợp lệ` }).click();

    const result = page.locator('.hhd-import__result');
    await expect(result).toContainText(`Đã thêm ${expectedValid} người`);
    await expect(result).toContainText(`Bỏ qua ${expectedErrors} dòng lỗi`);
  });

  await test.step('E2-10 · Danh sách tăng đúng bằng số dòng hợp lệ', async () => {
    await page.getByRole('button', { name: 'Về danh sách đảng viên' }).click();
    await expect(
      page.getByText(`${expectedValid} người · Tuổi đảng tính đến hôm nay`),
    ).toBeVisible();
    expect((await api.dashboard()).memberCount).toBe(expectedValid);
  });

  await test.step('E2-11 · Nạp lại cùng file lần nữa thì số người tăng gấp đôi (QT9 không kiểm trùng)', async () => {
    await page.goto(importPage);
    await page.setInputFiles('input[type="file"]', path.join(EXCEL_DIR, ERROR_FILE));
    await page.getByRole('button', { name: 'Tiếp tục ›' }).click();
    await page.getByRole('button', { name: `Nạp ${expectedValid} dòng hợp lệ` }).click();
    await expect(page.locator('.hhd-import__result')).toContainText(
      `Đã thêm ${expectedValid} người`,
    );

    await page.getByRole('button', { name: 'Về danh sách đảng viên' }).click();
    await expect(
      page.getByText(`${expectedValid * 2} người · Tuổi đảng tính đến hôm nay`),
    ).toBeVisible();
  });

  const fileLevelCases: { step: string; file: string; message: string }[] = [
    {
      step: 'E2-12 · File sai cột bị chặn ngay bước 1',
      file: path.join(EXCEL_DIR, 'loi-sai-cot.xlsx'),
      message:
        'File phải có đúng 4 cột theo thứ tự Họ tên · Ngày sinh · Giới tính · Ngày vào Đảng chính thức',
    },
    {
      step: 'E2-13 · File không phải Excel bị chặn, không lộ lỗi máy chủ',
      file: path.join(EXCEL_DIR, 'loi-khong-phai-xlsx.xlsx'),
      message: 'Chỉ nhận file .xlsx',
    },
    {
      step: 'E2-14 · File vượt 10 MB bị chặn với thông báo dung lượng',
      file: path.join(ARTIFACT_DIR, 'loi-qua-10mb.xlsx'),
      message: 'File vượt quá 10 MB',
    },
    {
      step: 'E2-15 · File rỗng bị chặn với thông báo file rỗng',
      file: path.join(EXCEL_DIR, 'loi-rong.xlsx'),
      message: 'File rỗng, không đọc được dữ liệu',
    },
  ];

  const before = (await api.dashboard()).memberCount;
  for (const item of fileLevelCases) {
    await test.step(item.step, async () => {
      await page.goto(importPage);
      await page.setInputFiles('input[type="file"]', item.file);

      // File quá cỡ và sai đuôi bị chặn ngay tại trình duyệt, không cần bấm tiếp.
      const continueButton = page.getByRole('button', { name: 'Tiếp tục ›' });
      if (await continueButton.isEnabled()) await continueButton.click();

      const alert = page.locator('.hhd-import__file-error');
      await expect(alert).toBeVisible();
      await expect(alert).toContainText('Chưa đọc được file này');
      await expect(alert).toContainText(item.message);
      // Vẫn ở bước 1: chưa có khối tóm tắt của bước 2, nút Tiếp tục vẫn còn đó.
      await expect(page.locator('.hhd-import__summary')).toHaveCount(0);
      await expect(page.getByRole('button', { name: 'Tiếp tục ›' })).toBeVisible();
      // Không để lọt chữ kỹ thuật ra màn hình.
      await expect(alert).not.toContainText('Exception');
      await expect(alert).not.toContainText('Mes.');
      await expect(alert).not.toContainText('Hệ thống gặp sự cố');
    });
  }

  expect((await api.dashboard()).memberCount, 'Lỗi cấp file không được ghi gì vào kho').toBe(
    before,
  );
});
