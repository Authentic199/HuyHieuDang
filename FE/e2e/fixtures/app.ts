import { test as base, expect, type Locator, type Page } from '@playwright/test';

import { Api } from './api';
import { installClock } from './clock';
import { TOKEN_STORAGE_KEY } from './env';

/**
 * Fixture dùng chung cho cả sáu luồng.
 *
 * `api`   — lối vào REST để dọn kho và gieo dữ liệu nền trước mỗi luồng.
 * `page`  — đã đóng băng đồng hồ trình duyệt trước khi mở trang đầu tiên.
 *
 * Mỗi luồng tự dựng trạng thái đầu của mình rồi mới thao tác, nên chạy được
 * độc lập, theo thứ tự bất kỳ, và chạy lại nhiều lần cho cùng kết quả
 * (yêu cầu E-901, E-902 của docs/test-plan.md mục 6).
 */

export const test = base.extend<{ api: Api }>({
  // Hai điều chỉnh so với ví dụ trong tài liệu Playwright, đều vì bộ lint của
  // dự án là bộ lint React: tham số thứ hai đặt tên `runTest` thay vì `use`
  // (nếu không, lint tưởng `use(...)` là một React Hook), và tham số thứ nhất
  // vẫn phải là mẫu rã object rỗng vì Playwright đọc chính chữ ký này để biết
  // fixture phụ thuộc vào đâu.
  // eslint-disable-next-line no-empty-pattern
  api: async ({}, runTest) => {
    await runTest(await Api.login());
  },

  page: async ({ page }, runTest) => {
    await installClock(page);
    await runTest(page);
  },
});

export { expect };

/** Đưa trình duyệt vào trạng thái đã đăng nhập mà không qua màn Đăng nhập. */
export async function signIn(page: Page, api: Api): Promise<void> {
  await page.addInitScript(([key, token]) => window.localStorage.setItem(key, token), [
    TOKEN_STORAGE_KEY,
    api.token,
  ] as const);
}

/** Đăng nhập thật qua giao diện (dùng cho E2E-1, nơi chính màn này là ca kiểm thử). */
export async function loginThroughUi(
  page: Page,
  username: string,
  password: string,
): Promise<void> {
  await page.getByLabel('Tài khoản').fill(username);
  await page.getByLabel('Mật khẩu').fill(password);
  await page.getByRole('button', { name: 'Đăng nhập' }).click();
}

// ----- Định vị phần tử dùng chung -----

/** Mục menu trái. Bám theo `aria-label` để không vỡ khi chữ trên nút đổi. */
export function navItem(page: Page, label: string): Locator {
  return page.getByRole('button', { name: label, exact: true });
}

/** Badge số người chưa thuộc đợt nào trên menu trái. */
export function uncoveredBadge(page: Page): Locator {
  return page.locator('.hhd-sider__badge');
}

/** Dòng "Hôm nay dd/MM/yyyy" trên thanh đầu trang. */
export function headerToday(page: Page): Locator {
  return page.locator('.hhd-header__today');
}

/** Tên đơn vị trên thanh đầu trang (UC-51). */
export function headerUnit(page: Page): Locator {
  return page.locator('.hhd-header__unit');
}

/** Mọi dòng dữ liệu của bảng Ant Design bên trong một vùng. */
export function tableRows(scope: Page | Locator): Locator {
  return scope.locator('.ant-table-tbody > tr.ant-table-row');
}

/** Đọc cả bảng thành mảng hai chiều các ô đã cắt khoảng trắng. */
export async function readTable(scope: Page | Locator): Promise<string[][]> {
  return tableRows(scope).evaluateAll((rows) =>
    rows.map((row) =>
      Array.from(row.querySelectorAll('td')).map((cell) => (cell.textContent ?? '').trim()),
    ),
  );
}

/** Đọc dòng tiêu đề của bảng. */
export async function readHeaders(scope: Page | Locator): Promise<string[]> {
  return scope
    .locator('.ant-table-thead th')
    .evaluateAll((cells) => cells.map((cell) => (cell.textContent ?? '').trim()));
}

/**
 * Chỉ số cột của từng bảng, đếm theo ô `<td>` thật sự có trong DOM.
 *
 * Mọi bảng đều mở đầu bằng cột STT; riêng bảng Đảng viên có thêm ô đánh dấu
 * chọn đứng trước cột STT nên mọi cột lùi một ô. Gom vào đây để ca kiểm thử
 * không phải đếm tay và không đếm nhầm.
 */
export const MEMBER_COLUMN = {
  select: 0,
  index: 1,
  fullName: 2,
  gender: 3,
  dateOfBirth: 4,
  admission: 5,
  partyAge: 6,
  nextMilestone: 7,
  nextMilestoneDate: 8,
} as const;

export const ELIGIBLE_COLUMN = {
  index: 0,
  fullName: 1,
  gender: 2,
  dateOfBirth: 3,
  admission: 4,
  milestoneDate: 5,
  milestone: 6,
} as const;

export const UNCOVERED_COLUMN = { ...ELIGIBLE_COLUMN, gap: 7 } as const;

export const PERIOD_COLUMN = {
  index: 0,
  name: 1,
  fromDisplay: 2,
  toDisplay: 3,
  status: 4,
  eligibleCount: 5,
} as const;

/** Hộp thoại xác nhận của Ant Design đang mở. */
export function confirmDialog(page: Page): Locator {
  return page.locator('.ant-modal-confirm').last();
}

/** Modal thường (Thêm/Sửa) đang mở. */
export function modal(page: Page): Locator {
  return page.locator('.ant-modal-content').last();
}

/**
 * Gõ vào ô tìm của màn Đảng viên rồi CHỜ THEO ĐIỀU KIỆN cho bảng đổi xong.
 *
 * Ô tìm hoãn 400 ms rồi mới gọi máy chủ, nên ngay sau khi gõ bảng vẫn còn kết
 * quả cũ. Chờ đúng dòng mong đợi hiện ra thay vì chờ một quãng cố định — ca
 * E-905 cấm dùng `waitForTimeout`.
 */
export async function searchMember(
  page: Page,
  keyword: string,
  expectedName: string,
): Promise<void> {
  await page.getByLabel('Tìm theo họ tên').fill(keyword);
  await expect(tableRows(page).first()).toContainText(expectedName);
}

/**
 * Bấm một năm trên bộ chọn năm (Segmented của Ant Design).
 *
 * Segmented dựng ô radio ẩn bên dưới nhãn, nên phải bấm vào chính nhãn — bấm
 * vào ô radio ẩn thì Playwright báo phần tử không nhìn thấy.
 */
export async function selectYear(scope: Page | Locator, year: number): Promise<void> {
  await scope
    .locator('.ant-segmented-item-label')
    .filter({ hasText: new RegExp(`^${year}$`) })
    .click();
}

/** Chờ cho lời nhắn nổi của Ant Design biến mất để khỏi che nút bên dưới. */
export async function dismissToasts(page: Page): Promise<void> {
  await page
    .locator('.ant-message-notice')
    .first()
    .waitFor({ state: 'detached' })
    .catch(() => {});
}
