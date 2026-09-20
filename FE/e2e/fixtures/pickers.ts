import { expect, type Locator, type Page } from '@playwright/test';

/**
 * Thao tác với ô chọn ngày của Ant Design.
 *
 * Ô chọn ngày của modal Đợt đặt `inputReadOnly` nên KHÔNG gõ thẳng được — phải
 * mở lịch rồi bấm đúng ô ngày, đúng như cán bộ thao tác thật. Lịch của modal
 * Đợt bị khóa trong năm nhuận mẫu 2024 (chỉ lưu ngày/tháng theo QT6), nên mọi
 * ô ngày ở đó mang thuộc tính `title` dạng `2024-MM-dd`.
 */

/** Chữ viết tắt tháng mà rc-picker in ra ở đầu bảng lịch. */
const MONTH_LABELS = [
  'Jan',
  'Feb',
  'Mar',
  'Apr',
  'May',
  'Jun',
  'Jul',
  'Aug',
  'Sep',
  'Oct',
  'Nov',
  'Dec',
];

function pad(value: number): string {
  return String(value).padStart(2, '0');
}

/** Bảng lịch đang mở. */
function openPanel(page: Page): Locator {
  return page.locator('.ant-picker-dropdown').last();
}

/**
 * Chọn ngày/tháng trong lịch khóa năm 2024 của modal Đợt trao huy hiệu.
 * `year` chỉ là năm mẫu để lịch có đủ ngày 29/02 — không bao giờ gửi lên máy chủ.
 */
export async function pickDayMonth(
  page: Page,
  field: Locator,
  day: number,
  month: number,
  year = 2024,
): Promise<void> {
  await field.click();
  const panel = openPanel(page);
  await expect(panel).toBeVisible();

  const header = panel.locator('.ant-picker-header-view');
  const current = (await header.innerText()).trim();
  const currentMonth = MONTH_LABELS.findIndex((label) => current.startsWith(label)) + 1;

  const steps = currentMonth > 0 ? month - currentMonth : 0;
  const button = steps >= 0 ? '.ant-picker-header-next-btn' : '.ant-picker-header-prev-btn';
  for (let index = 0; index < Math.abs(steps); index += 1) {
    await panel.locator(button).click();
  }

  const cell = panel.locator(
    `td.ant-picker-cell-in-view[title="${year}-${pad(month)}-${pad(day)}"]`,
  );
  await expect(cell).toHaveCount(1);
  await cell.click();
  await expect(panel).toBeHidden();
}

/**
 * Ô chọn ngày đầy đủ dd/MM/yyyy của modal Đảng viên — ô này gõ thẳng được.
 * Gõ xong nhấn Enter để lịch đóng lại và giá trị được nhận.
 */
export async function typeFullDate(page: Page, field: Locator, display: string): Promise<void> {
  await field.click();
  await field.fill(display);
  await field.press('Enter');
  await expect(openPanel(page)).toBeHidden();
}
