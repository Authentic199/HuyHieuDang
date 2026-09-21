import type { Locator, Page } from '@playwright/test';

import {
  confirmDialog,
  dismissToasts,
  expect,
  MEMBER_COLUMN,
  modal,
  navItem,
  readTable,
  searchMember,
  signIn,
  tableRows,
  test,
  uncoveredBadge,
} from '../fixtures/app';
import { displayDate, tomorrowOf } from '../fixtures/clock';
import { eligibility, expected, scenario } from '../fixtures/expected';
import { typeFullDate } from '../fixtures/pickers';

/**
 * E2E-6 · Vòng đời đảng viên (UC-20 → UC-23, QT3, QT3a, QT5).
 *
 * Trạng thái đầu: 4 đợt, cài đặt 30/90/5, bộ lõi 32 người.
 * Thêm tay → tìm → sửa ngày vào Đảng → mốc kế tiếp đổi theo → xóa nhiều dòng,
 * và mọi lần đổi đều lan ngay sang Dashboard vì danh sách không được lưu (QT5).
 */

const CASE = scenario('core_default_T0');
const UPCOMING = CASE.upcomingPeriod!;
const BASE_TOTAL = expected.counts.core;

/**
 * Nút xóa nhiều của màn Đảng viên là nút biểu tượng thùng rác (T33): chỉ
 * `aria-label` là chỗ bám ổn định. Tooltip và hộp xác nhận mới mang con số, nên
 * con số được khẳng định riêng ở đó chứ không nhét vào locator của nút.
 */
function deleteSelectedButton(page: Page): Locator {
  return page.getByRole('button', { name: 'Xóa người đã chọn', exact: true });
}

/** Ô đánh dấu chọn của một dòng trong bảng Đảng viên. */
function rowCheckbox(row: Locator): Locator {
  return row.locator('input[type="checkbox"]');
}

/** Tiền tố dùng chung để lọc riêng những người do luồng này tạo ra. */
const PREFIX = 'Kiểm Thử';
const SUBJECT = `${PREFIX} Vòng Đời`;

interface NewMember {
  fullName: string;
  admission: string;
  dateOfBirth?: string;
  gender?: 'Nam' | 'Nữ';
}

async function openAddForm(page: Page) {
  await page.getByRole('button', { name: '+ Thêm', exact: true }).click();
  return modal(page);
}

async function addMember(page: Page, member: NewMember) {
  const form = await openAddForm(page);
  await form.getByLabel('Họ tên').fill(member.fullName);
  await typeFullDate(page, form.getByLabel('Ngày vào Đảng chính thức'), member.admission);
  if (member.dateOfBirth) {
    await typeFullDate(page, form.getByLabel('Ngày sinh'), member.dateOfBirth);
  }
  if (member.gender) await form.getByText(member.gender, { exact: true }).click();

  await form.getByRole('button', { name: 'Thêm vào danh sách' }).click();
  await expect(form).toBeHidden();
  await expect(page.getByText('Đã thêm đảng viên')).toBeVisible();
  await dismissToasts(page);
}

/** Sửa Ngày vào Đảng chính thức của một người đang hiện trên bảng. */
async function editAdmissionDate(
  page: Page,
  fullName: string,
  admission: string,
  dateOfBirth?: string,
) {
  await page.getByRole('button', { name: `Sửa ${fullName}` }).click();
  const form = modal(page);
  // Ngày sinh phải trước Ngày chính thức, nên khi lùi ngày chính thức rất xa
  // thì phải lùi ngày sinh theo — nếu không biểu mẫu chặn đúng theo quy tắc.
  if (dateOfBirth) await typeFullDate(page, form.getByLabel('Ngày sinh'), dateOfBirth);
  await typeFullDate(page, form.getByLabel('Ngày vào Đảng chính thức'), admission);
  await form.getByRole('button', { name: 'Lưu thay đổi' }).click();
  await expect(form).toBeHidden();
  await expect(page.getByText('Đã lưu thay đổi')).toBeVisible();
  await dismissToasts(page);
}

/** Chọn một giá trị trong ô lọc Giới tính (Select của Ant Design). */
async function selectGenderFilter(page: Page, label: string) {
  await page.locator('.hhd-members__gender').click();
  const option = page.locator(`.ant-select-dropdown .ant-select-item-option[title="${label}"]`);
  await option.waitFor({ state: 'visible' });
  await option.click();
}

function headingFor(total: number): string {
  return `${total.toLocaleString('vi-VN')} người · Tuổi đảng tính đến hôm nay`;
}

test('E2E-6 · Vòng đời đảng viên: thêm tay → tìm → sửa → xóa nhiều dòng', async ({ page, api }) => {
  await api.seedBaseline();
  await signIn(page, api);
  const serverToday = (await api.dashboard()).today;
  const upcomingList = eligibility('core_default_T0', UPCOMING.code, 2026);

  await test.step('E6-01 · Danh sách Đảng viên mở đầu với 32 người', async () => {
    await page.goto('/dang-vien');
    await expect(page.getByText(headingFor(BASE_TOTAL))).toBeVisible();
  });

  await test.step('E6-02 · Thêm tay một người thì tổng lên 33', async () => {
    await addMember(page, {
      fullName: SUBJECT,
      dateOfBirth: '10/10/1970',
      gender: 'Nam',
      admission: '01/10/1996',
    });
    await expect(page.getByText(headingFor(BASE_TOTAL + 1))).toBeVisible();
  });

  await test.step('E6-03 · Tìm thấy đúng một dòng, tuổi đảng 29, mốc kế tiếp 30 ngày 01/10/2026', async () => {
    await searchMember(page, SUBJECT, SUBJECT);
    await expect(tableRows(page)).toHaveCount(1);

    const row = (await readTable(page))[0];
    expect(row[MEMBER_COLUMN.admission]).toBe('01/10/1996');
    expect(row[MEMBER_COLUMN.partyAge]).toBe('29');
    expect(row[MEMBER_COLUMN.nextMilestone]).toBe('30 năm');
    expect(row[MEMBER_COLUMN.nextMilestoneDate]).toBe('01/10/2026');
  });

  await test.step('E6-04 · Dashboard lên 7 người — người mới tròn 30 đúng Từ ngày Đợt 7/11 (QT5)', async () => {
    await navItem(page, 'Dashboard').click();
    await expect(page.locator('.hhd-dashboard__count-value')).toHaveText(
      String(upcomingList.total + 1),
    );
    await expect(page.locator('.hhd-dashboard__panel')).toContainText(SUBJECT);
  });

  await test.step('E6-05 · Ngày chính thức ở tương lai bị chặn ở cả giao diện lẫn máy chủ', async () => {
    const tomorrow = tomorrowOf(serverToday);
    await navItem(page, 'Đảng viên').click();

    const form = await openAddForm(page);
    await form.getByLabel('Họ tên').fill(`${PREFIX} Ngày Tương Lai`);
    await form.getByLabel('Ngày vào Đảng chính thức').click();

    const panel = page.locator('.ant-picker-dropdown').last();
    const cell = panel.locator(`td[title="${tomorrow}"]`);
    await expect(cell).toHaveCount(1);
    await expect(cell).toHaveClass(/ant-picker-cell-disabled/);
    // Escape đóng lịch, và nếu modal đóng luôn theo thì cũng không sao.
    await page.keyboard.press('Escape');
    if (await form.isVisible()) await form.getByRole('button', { name: 'Đóng' }).click();
    await expect(form).toBeHidden();

    // Máy chủ cũng phải từ chối, không chỉ dựa vào giao diện.
    const rejected = await api
      .post('/PartyMembers', {
        fullName: `${PREFIX} Ngày Tương Lai`,
        dateOfBirth: null,
        gender: null,
        officialAdmissionDate: tomorrow,
      })
      .then(() => null)
      .catch((reason: { status: number; body: string }) => reason);
    expect(rejected, 'Máy chủ phải từ chối ngày chính thức ở tương lai').not.toBeNull();
    expect(rejected!.status).toBe(400);
    expect(rejected!.body).toContain('Mes.PartyMember.Invalid.OfficialAdmissionDate');
    expect((await api.dashboard()).memberCount).toBe(BASE_TOTAL + 1);
  });

  await test.step('E6-06 · Bỏ trống Họ tên thì bị chặn ngay tại biểu mẫu', async () => {
    const form = await openAddForm(page);
    await typeFullDate(page, form.getByLabel('Ngày vào Đảng chính thức'), '01/10/1996');
    await form.getByRole('button', { name: 'Thêm vào danh sách' }).click();

    await expect(form.getByText('Chưa nhập Họ tên')).toBeVisible();
    await expect(form).toBeVisible();
    await form.getByRole('button', { name: 'Đóng' }).click();
    await expect(form).toBeHidden();
    expect((await api.dashboard()).memberCount).toBe(BASE_TOTAL + 1);
  });

  await test.step('E6-07 · Thêm người chỉ có Họ tên và Ngày chính thức: hai ô còn lại hiện —', async () => {
    // Thêm hai người tối giản: một người cho chính ca này, một người để ca
    // E6-14 có đủ ba dòng do luồng tạo ra mà chọn.
    await addMember(page, { fullName: `${PREFIX} Thiếu Thông Tin`, admission: '15/01/1996' });
    await addMember(page, { fullName: `${PREFIX} Xóa Nhiều`, admission: '31/05/1996' });

    await searchMember(page, `${PREFIX} Thiếu Thông Tin`, `${PREFIX} Thiếu Thông Tin`);
    const row = (await readTable(page))[0];
    expect(row[MEMBER_COLUMN.gender]).toBe('—');
    expect(row[MEMBER_COLUMN.dateOfBirth]).toBe('—');
  });

  await test.step('E6-08 · Sửa Ngày chính thức của người đang theo dõi sang 05/10/1991', async () => {
    await searchMember(page, SUBJECT, SUBJECT);
    await editAdmissionDate(page, SUBJECT, '05/10/1991');
  });

  await test.step('E6-09 · Tuổi đảng thành 34, mốc kế tiếp 35 ngày 05/10/2026', async () => {
    await searchMember(page, SUBJECT, SUBJECT);
    const row = (await readTable(page))[0];
    expect(row[MEMBER_COLUMN.admission]).toBe('05/10/1991');
    expect(row[MEMBER_COLUMN.partyAge]).toBe('34');
    expect(row[MEMBER_COLUMN.nextMilestone]).toBe('35 năm');
    expect(row[MEMBER_COLUMN.nextMilestoneDate]).toBe('05/10/2026');
  });

  await test.step('E6-10 · Dashboard xếp người đó vào mốc 35 của Đợt 7/11, phân bổ đổi đúng', async () => {
    await navItem(page, 'Dashboard').click();
    const panel = page.locator('.hhd-dashboard__panel');
    await expect(panel).toContainText(SUBJECT);
    const rows = await readTable(panel);
    const target = rows.find((row) => row[1] === SUBJECT)!;
    expect(target[6]).toBe('35 năm');
    expect(target[5]).toBe('05/10/2026');

    // Mốc 35 tăng từ 1 lên 2, các mốc khác giữ nguyên.
    const pills = page.locator('.hhd-dashboard__pills');
    await expect(pills).toContainText(`35 năm${upcomingList.byMilestone['35'] + 1}`);
    await expect(pills).toContainText(`30 năm${upcomingList.byMilestone['30']}`);
  });

  await test.step('E6-11 · Sửa Ngày chính thức sang 01/05/1935: tuổi đảng 91, mốc kế tiếp —, rời Đợt 7/11', async () => {
    await navItem(page, 'Đảng viên').click();
    await searchMember(page, SUBJECT, SUBJECT);
    /**
     * Kế hoạch kiểm thử (ca E6-11) chỉ nói đổi Ngày chính thức sang 01/05/1935,
     * nhưng người này có Ngày sinh 10/10/1970 từ ca E6-02. Quy tắc "Ngày sinh
     * phải trước Ngày vào Đảng chính thức" — có ở cả biểu mẫu lẫn máy chủ — sẽ
     * chặn đúng, nên phải lùi Ngày sinh cùng lúc. Đây là chỗ thiếu của kế hoạch,
     * không phải lỗi sản phẩm (QC-T28-03).
     */
    await editAdmissionDate(page, SUBJECT, '01/05/1935', '12/03/1915');
    await searchMember(page, SUBJECT, SUBJECT);

    const row = (await readTable(page))[0];
    expect(row[MEMBER_COLUMN.partyAge]).toBe('91');
    expect(row[MEMBER_COLUMN.nextMilestone]).toBe('—');
    expect(row[MEMBER_COLUMN.nextMilestoneDate]).toBe('—');

    await navItem(page, 'Dashboard').click();
    await expect(page.locator('.hhd-dashboard__count-value')).toHaveText(
      String(upcomingList.total),
    );
    await expect(page.locator('.hhd-dashboard__panel')).not.toContainText(SUBJECT);
  });

  const workingTotal = BASE_TOTAL + 3;

  await test.step('E6-12 · Lọc Giới tính = Nữ rồi bỏ lọc, số dòng khớp bộ dữ liệu', async () => {
    await navItem(page, 'Đảng viên').click();
    await expect(page.getByText(headingFor(workingTotal))).toBeVisible();

    await selectGenderFilter(page, 'Nữ');

    // Ba người luồng này thêm đều là Nam hoặc để trống, nên số Nữ đúng bằng bộ lõi.
    const female = expected.counts.coreGenderNu;
    await expect(page.getByText(headingFor(female))).toBeVisible();
    const genders = (await readTable(page)).map((row) => row[MEMBER_COLUMN.gender]);
    expect(new Set(genders)).toEqual(new Set(['Nữ']));

    await selectGenderFilter(page, 'Tất cả');
    await expect(page.getByText(headingFor(workingTotal))).toBeVisible();
  });

  await test.step('E6-13 · Đổi số dòng/trang và sang trang 2 rồi quay lại: không mất, không trùng người', async () => {
    const pageOne = (await readTable(page)).map((row) => row[MEMBER_COLUMN.fullName]);
    expect(pageOne).toHaveLength(20);

    await page.getByRole('listitem', { name: '2' }).click();
    await expect(page.locator('.ant-pagination-item-active')).toHaveText('2');
    const pageTwo = (await readTable(page)).map((row) => row[MEMBER_COLUMN.fullName]);
    expect(pageTwo).toHaveLength(workingTotal - 20);

    const all = [...pageOne, ...pageTwo];
    expect(new Set(all).size, 'Không được trùng người giữa hai trang').toBe(workingTotal);

    await page.getByRole('listitem', { name: '1' }).click();
    await expect(page.locator('.ant-pagination-item-active')).toHaveText('1');
    expect((await readTable(page)).map((row) => row[MEMBER_COLUMN.fullName])).toEqual(pageOne);
  });

  await test.step('E6-14 · Chọn 3 dòng thì thanh công cụ báo "Đang chọn 3 dòng"', async () => {
    await searchMember(page, PREFIX, PREFIX);
    await expect(tableRows(page)).toHaveCount(3);

    await page.locator('.ant-table-thead input[type="checkbox"]').check();
    await expect(page.locator('.hhd-members__selected')).toContainText('Đang chọn 3 dòng');
  });

  await test.step('E6-15 · Xóa 3 dòng đã chọn: hộp xác nhận nêu rõ 3 người, tổng giảm đúng 3', async () => {
    await deleteSelectedButton(page).click();
    const dialog = confirmDialog(page);
    await expect(dialog).toContainText('Xóa 3 người khỏi danh sách?');
    await expect(dialog).toContainText('Cả 3 người đang chọn sẽ bị xóa hẳn');
    await dialog.getByRole('button', { name: 'Xóa 3 người' }).click();

    await expect(page.getByText('Không tìm thấy ai như vậy')).toBeVisible();
    await dismissToasts(page);
    await page.getByLabel('Tìm theo họ tên').fill('');
    await expect(page.getByText(headingFor(BASE_TOTAL))).toBeVisible();
  });

  await test.step('E6-16 · Bấm Xóa rồi Hủy trong hộp xác nhận thì tổng không đổi', async () => {
    const victim = (await readTable(page))[0][MEMBER_COLUMN.fullName];
    // T33 đã bỏ nút xóa từng dòng: xóa một người cũng đi qua nút thùng rác,
    // và đánh dấu đúng một dòng thì hộp xác nhận gọi thẳng tên người đó.
    const firstRow = tableRows(page).first();
    await rowCheckbox(firstRow).check();
    await expect(page.locator('.hhd-members__selected')).toContainText('Đang chọn 1 dòng');
    await deleteSelectedButton(page).click();

    const dialog = confirmDialog(page);
    await expect(dialog).toContainText(`Xóa ${victim} khỏi danh sách?`);
    await dialog.getByRole('button', { name: 'Để lại' }).click();
    await expect(dialog).toBeHidden();

    // Bấm Hủy thì dòng vẫn đang đánh dấu — bỏ đánh dấu để E6-17 bắt đầu sạch.
    await rowCheckbox(firstRow).uncheck();
    await expect(page.getByText(headingFor(BASE_TOTAL))).toBeVisible();
    expect((await api.dashboard()).memberCount).toBe(BASE_TOTAL);
  });

  await test.step('E6-17 · Kho trở về đúng 32 người, Dashboard và badge về đúng trạng thái đầu', async () => {
    await navItem(page, 'Dashboard').click();
    await expect(page.locator('.hhd-dashboard__count-value')).toHaveText(
      String(upcomingList.total),
    );
    await expect(page.locator('.hhd-dashboard__period-name')).toHaveText(UPCOMING.name);
    await expect(uncoveredBadge(page)).toHaveText(String(CASE.badgeCurrentYear));

    // Header vẫn hiện ngày của MÁY CHỦ, không phải đồng hồ trình duyệt (T-FIX-5).
    await expect(page.locator('.hhd-header__today')).toHaveText(
      `Hôm nay ${displayDate(serverToday)}`,
    );
  });
});
