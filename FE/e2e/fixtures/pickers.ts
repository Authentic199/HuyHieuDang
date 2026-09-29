import { expect, type Locator, type Page } from '@playwright/test';

/**
 * Thao tác với ô chọn ngày của Ant Design.
 *
 * Ô chọn ngày của modal Đợt đặt `inputReadOnly` nên KHÔNG gõ thẳng được — phải
 * mở lịch rồi bấm đúng ô ngày, đúng như cán bộ thao tác thật. Lịch của modal
 * Đợt bị khóa trong năm nhuận mẫu 2024 (chỉ lưu ngày/tháng theo QT6), nên mọi
 * ô ngày ở đó mang thuộc tính `title` dạng `2024-MM-dd`.
 *
 * Mọi thứ ở đây bám vào `title`, không bám vào chữ hiển thị: giao diện chạy
 * locale `vi_VN`, đầu lịch in "Th 01" chứ không phải "Jan".
 */

function pad(value: number): string {
  return String(value).padStart(2, '0');
}

/** Bảng lịch đang mở. */
function openPanel(page: Page): Locator {
  return page.locator('.ant-picker-dropdown').last();
}

/**
 * Tháng đang hiển thị, đọc từ thuộc tính `title` của một ô ngày bất kỳ trong bảng.
 *
 * Cố ý KHÔNG đọc chữ ở đầu lịch: giao diện chạy locale `vi_VN` nên chỗ đó in
 * "Th 01", không phải "Jan". `title` luôn là `yyyy-MM-dd` bất kể ngôn ngữ.
 */
async function currentMonthOf(panel: Locator): Promise<number> {
  const title = await panel.locator('td.ant-picker-cell-in-view').first().getAttribute('title');

  return title ? Number(title.slice(5, 7)) : 0;
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

  // Bấm từng nhịp một rồi đọc lại tháng, thay vì tính một lần rồi bấm mù: lịch
  // bị khóa trong năm 2024 nên nhịp ở hai đầu năm có thể không nhúc nhích.
  for (let guard = 0; guard < 12; guard += 1) {
    const current = await currentMonthOf(panel);
    if (current === month) {
      break;
    }

    const button = current < month ? '.ant-picker-header-next-btn' : '.ant-picker-header-prev-btn';
    await panel.locator(button).click();
    await expect.poll(async () => currentMonthOf(panel), { timeout: 5_000 }).not.toBe(current);
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
