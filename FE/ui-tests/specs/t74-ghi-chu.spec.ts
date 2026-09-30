import type { Locator, Page } from '@playwright/test';

import { DETAIL_PERIOD, expect, mockData, test, UPCOMING_PERIOD } from '../fixtures/app';
import { RESPONSIVE_VIEWPORTS } from '../playwright.ui.config';
import {
  NOTED_COUNT,
  noteDateTextOf,
  notedRowsOf,
  SERVER_TODAY,
  SERVER_YEAR,
} from '../fixtures/data';

/**
 * T74 — ghi chú đảng viên (QT12): nút ghi chú ở cột Thao tác, hộp ghi chú của
 * một người, form Thêm/Sửa cân lại, khối gọn và hộp xem đầy đủ ở Dashboard và
 * Chi tiết đợt.
 *
 * Bộ dữ liệu giả ở `fixtures/data.ts` đã có bốn ghi chú: một nhiều dòng, một là
 * chuỗi dài không dấu cách, một của năm trước và một ngắn. Mọi con số mong đợi
 * đều tính ra từ bộ đó, không viết cứng.
 */

/** Vỏ `{ message, data }` của mọi phản hồi thành công (mục 1.3 hợp đồng API). */
function envelope(data: unknown) {
  return {
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify({ message: null, data }),
  };
}

/** Trần ký tự của một ghi chú (QT12). */
const NOTE_MAX = 500;

/** Hai người cho màn Đảng viên: một đã có ghi chú, một chưa — dựng ngay ở đây
    để bảng luôn hiện đúng hai dòng này bất kể bộ dữ liệu chung đổi thế nào. */
const WITH_NOTE = {
  id: 't74-co-ghi-chu',
  fullName: 'Lê Hữu Cường',
  note: 'Hồ sơ gốc thiếu bản sao quyết định kết nạp.',
  noteUpdatedAt: `${SERVER_YEAR}-09-12T14:05:00+07:00`,
};
const WITHOUT_NOTE = { id: 't74-chua-ghi-chu', fullName: 'Trần Thị Bình' };

function memberRow(
  id: string,
  fullName: string,
  note: string | null,
  noteUpdatedAt: string | null,
) {
  return {
    id,
    fullName,
    dateOfBirth: '1955-07-15',
    gender: 'Male' as const,
    officialAdmissionDate: '1986-10-18',
    partyAgeYears: 40,
    nextMilestone: 45,
    nextMilestoneDate: `${SERVER_YEAR}-10-18`,
    createdAt: `${SERVER_YEAR}-01-01T00:00:00Z`,
    updatedAt: `${SERVER_YEAR}-01-01T00:00:00Z`,
    note,
    noteUpdatedAt,
  };
}

/** Chặn `/PartyMembers` bằng đúng hai người trên; endpoint khác để fixture lo. */
async function installTwoMembers(page: Page): Promise<void> {
  const rows = [
    memberRow(WITH_NOTE.id, WITH_NOTE.fullName, WITH_NOTE.note, WITH_NOTE.noteUpdatedAt),
    memberRow(WITHOUT_NOTE.id, WITHOUT_NOTE.fullName, null, null),
  ];
  await page.route('**/api/**', async (route) => {
    const path = new URL(route.request().url()).pathname.replace(/^.*\/api/, '');
    if (path === '/PartyMembers') {
      return route.fulfill(
        envelope({
          pagedData: rows,
          pageInfo: { current: 1, pageSize: 20, totalCount: rows.length, totalPages: 1 },
        }),
      );
    }
    return route.fallback();
  });
}

/** Dashboard và Chi tiết đợt mà KHÔNG ai có ghi chú — khối gọn phải biến mất. */
async function installNoNotes(page: Page): Promise<void> {
  const bare = mockData.eligible.map((row) => ({ ...row, note: null, noteUpdatedAt: null }));
  await page.route('**/api/**', async (route) => {
    const url = new URL(route.request().url());
    const path = url.pathname.replace(/^.*\/api/, '');

    if (path === '/Eligibility') {
      const year = Number(url.searchParams.get('year') ?? SERVER_YEAR);
      return route.fulfill(
        envelope({
          awardPeriod: { ...DETAIL_PERIOD, year },
          year,
          totalCount: bare.length,
          milestoneBreakdown: [],
          members: bare,
        }),
      );
    }

    if (path !== '/Dashboard') return route.fallback();

    return route.fulfill(
      envelope({
        today: SERVER_TODAY,
        currentYear: SERVER_YEAR,
        unitName: 'Đảng ủy Phường Kiểm Thử',
        memberCount: bare.length,
        periodCount: mockData.periods.length,
        upcomingPeriod: UPCOMING_PERIOD,
        eligibleMembers: bare,
        warnings: {
          noMembers: false,
          noPeriods: false,
          unassignedYear: SERVER_YEAR,
          unassignedCount: 0,
          overlaps: [],
          gaps: [],
        },
      }),
    );
  });
}

/** Số người có ghi chú trong bộ "nhiều ghi chú" — đủ dài để hộp phải cuộn. */
const MANY_NOTES = 30;

/** Dashboard với nhiều ghi chú, để kiểm danh sách trong hộp cuộn được. */
async function installManyNotes(page: Page): Promise<void> {
  const rows = mockData.eligible.map((row, index) => ({
    ...row,
    note: index < MANY_NOTES ? `Ghi chú số ${index + 1} của ${row.fullName}.` : null,
    noteUpdatedAt: index < MANY_NOTES ? `${SERVER_YEAR}-09-12T14:05:00+07:00` : null,
  }));
  await page.route('**/api/**', async (route) => {
    const path = new URL(route.request().url()).pathname.replace(/^.*\/api/, '');
    if (path !== '/Dashboard') return route.fallback();
    return route.fulfill(
      envelope({
        today: SERVER_TODAY,
        currentYear: SERVER_YEAR,
        unitName: 'Đảng ủy Phường Kiểm Thử',
        memberCount: rows.length,
        periodCount: mockData.periods.length,
        upcomingPeriod: UPCOMING_PERIOD,
        eligibleMembers: rows,
        warnings: {
          noMembers: false,
          noPeriods: false,
          unassignedYear: SERVER_YEAR,
          unassignedCount: 0,
          overlaps: [],
          gaps: [],
        },
      }),
    );
  });
}

const NOTED_ROWS = notedRowsOf(mockData.eligible);

/**
 * Chờ một hộp thoại của Ant Design mở XONG rồi mới đo.
 *
 * Hộp thoại mở bằng hoạt ảnh phóng to: trong lúc hoạt ảnh còn chạy, hộp vẫn
 * đang mờ và đang bị thu nhỏ, nên `boundingBox()` trả về một hình chữ nhật nhỏ
 * hơn hình thật và mọi phép đo theo tọa độ màn đều lệch vài điểm ảnh — đỏ chập
 * chờn chứ không phải sản phẩm sai.
 *
 * Chờ theo điều kiện, không theo quãng thời gian cố định: hộp phải đục hẳn
 * (`opacity: 1`), hết biến hình (`transform` là ma trận đơn vị), không còn lớp
 * hoạt ảnh của Ant Design, và hình chữ nhật phải đứng yên qua hai khung hình
 * liên tiếp.
 */
async function waitForModalOpen(modal: Locator): Promise<void> {
  await expect(modal).toBeVisible();
  await modal.evaluate(
    (node: HTMLElement) =>
      new Promise<void>((resolve) => {
        let previous: DOMRect | null = null;
        const tick = () => {
          const style = getComputedStyle(node);
          const settled =
            style.opacity === '1' &&
            (style.transform === 'none' || style.transform === 'matrix(1, 0, 0, 1, 0, 0)') &&
            ![...node.classList].some((name) => /-(enter|appear|leave)(-|$)/.test(name));
          const rect = node.getBoundingClientRect();
          const same =
            previous !== null &&
            Math.abs(previous.x - rect.x) < 0.01 &&
            Math.abs(previous.y - rect.y) < 0.01 &&
            Math.abs(previous.width - rect.width) < 0.01 &&
            Math.abs(previous.height - rect.height) < 0.01;
          if (settled && same) {
            resolve();
            return;
          }
          previous = settled ? rect : null;
          requestAnimationFrame(tick);
        };
        requestAnimationFrame(tick);
      }),
  );
}

test('ca 1 · nút ghi chú ở cột Thao tác, hai trạng thái hai tooltip', async ({ app }) => {
  await installTwoMembers(app);
  await app.goto('/dang-vien');

  const noted = app.getByLabel(`Ghi chú ${WITH_NOTE.fullName}`, { exact: true });
  const blank = app.getByLabel(`Ghi chú ${WITHOUT_NOTE.fullName}`, { exact: true });
  await expect(noted).toBeVisible();
  await expect(blank).toBeVisible();

  // Người đã có ghi chú: nút tô nền vàng nhạt và dùng biểu tượng đặc.
  await expect(noted).toHaveClass(/hhd-members__note--has/);
  await expect(noted.locator('.anticon-file-text')).toBeVisible();
  await expect(blank).not.toHaveClass(/hhd-members__note--has/);

  // Nút ghi chú đứng BÊN TRÁI nút Sửa trong cùng một ô.
  const box = await noted.boundingBox();
  const editBox = await app.getByLabel(`Sửa ${WITH_NOTE.fullName}`, { exact: true }).boundingBox();
  expect(box!.x).toBeLessThan(editBox!.x);

  await noted.hover();
  const writtenOn = noteDateTextOf(WITH_NOTE.noteUpdatedAt).replace('Ghi ngày ', '');
  await expect(app.getByRole('tooltip', { name: `Ghi chú · ghi ngày ${writtenOn}` })).toBeVisible();

  // Rời chuột khỏi nút cũ rồi mới hỏi tooltip của nút kia. Ant Design giữ lại
  // lớp tooltip cũ một lúc nên phải chọn theo nội dung, không theo lớp chung.
  await app.getByRole('heading', { name: 'Đảng viên' }).hover();
  await blank.hover();
  await expect(app.getByRole('tooltip', { name: 'Thêm ghi chú' })).toBeVisible();
});

test('ca 2 · hộp ghi chú của một người: đếm tới 500, không gõ được ký tự 501', async ({ app }) => {
  await installTwoMembers(app);
  await app.goto('/dang-vien');
  await app.getByLabel(`Ghi chú ${WITH_NOTE.fullName}`, { exact: true }).click();

  const dialog = app.locator('.ant-modal').filter({ hasText: `Ghi chú — ${WITH_NOTE.fullName}` });
  await expect(dialog.locator('.ant-modal-title')).toHaveText(`Ghi chú — ${WITH_NOTE.fullName}`);

  const field = dialog.getByLabel('Ghi chú', { exact: true });
  await expect(field).toHaveValue(WITH_NOTE.note);
  await expect(dialog.getByTestId('note-counter')).toHaveText(
    `${WITH_NOTE.note.length} / ${NOTE_MAX}`,
  );
  // Viên ngày ghi màu vàng nằm ngay dưới ô nhập.
  await expect(dialog.getByTestId('note-date-pill')).toHaveText(
    noteDateTextOf(WITH_NOTE.noteUpdatedAt),
  );

  await field.fill('a'.repeat(NOTE_MAX));
  await expect(dialog.getByTestId('note-counter')).toHaveText(`${NOTE_MAX} / ${NOTE_MAX}`);

  // Ký tự thứ 501 không vào được ô, bộ đếm đứng yên.
  await field.pressSequentially('b');
  await expect(field).toHaveValue('a'.repeat(NOTE_MAX));
  await expect(dialog.getByTestId('note-counter')).toHaveText(`${NOTE_MAX} / ${NOTE_MAX}`);

  await expect(dialog.getByRole('button', { name: 'Đóng' })).toBeVisible();
  await expect(dialog.getByRole('button', { name: 'Lưu ghi chú' })).toBeVisible();
});

test('ca 3 · lưu ghi chú gọi đúng endpoint riêng rồi tải lại bảng', async ({ app }) => {
  await installTwoMembers(app);
  await app.goto('/dang-vien');
  await app.getByLabel(`Ghi chú ${WITHOUT_NOTE.fullName}`, { exact: true }).click();

  const dialog = app
    .locator('.ant-modal')
    .filter({ hasText: `Ghi chú — ${WITHOUT_NOTE.fullName}` });
  await dialog.getByLabel('Ghi chú', { exact: true }).fill('  Cần bổ sung lý lịch.  ');

  const [request] = await Promise.all([
    app.waitForRequest(
      (candidate) =>
        candidate.method() === 'PUT' &&
        candidate.url().includes(`/PartyMembers/${WITHOUT_NOTE.id}/Note`),
    ),
    dialog.getByRole('button', { name: 'Lưu ghi chú' }).click(),
  ]);

  // Khoảng trắng hai đầu cắt ngay ở giao diện, máy chủ vẫn cắt lại lần nữa.
  expect(request.postDataJSON()).toEqual({ note: 'Cần bổ sung lý lịch.' });
  await expect(app.locator('.ant-message-success')).toContainText('Đã lưu thay đổi');
  await expect(dialog).toBeHidden();
});

test('ca 4 · xóa hết chữ rồi Lưu là xóa ghi chú', async ({ app }) => {
  await installTwoMembers(app);
  await app.goto('/dang-vien');
  await app.getByLabel(`Ghi chú ${WITH_NOTE.fullName}`, { exact: true }).click();

  const dialog = app.locator('.ant-modal').filter({ hasText: `Ghi chú — ${WITH_NOTE.fullName}` });
  await dialog.getByLabel('Ghi chú', { exact: true }).fill('');

  const [request] = await Promise.all([
    app.waitForRequest(
      (candidate) => candidate.method() === 'PUT' && candidate.url().includes('/Note'),
    ),
    dialog.getByRole('button', { name: 'Lưu ghi chú' }).click(),
  ]);
  expect(request.postDataJSON()).toEqual({ note: null });
});

test.describe('form Thêm/Sửa ở khung laptop 1366x650', () => {
  test.use({ viewport: { width: 1366, height: 650 } });

  test('ca 5 · bỏ hai dòng chữ dưới hai ô ngày, Ngày sinh và Giới tính cùng hàng, không cuộn', async ({
    app,
  }) => {
    await installTwoMembers(app);
    await app.goto('/dang-vien');
    await app.getByLabel(`Sửa ${WITH_NOTE.fullName}`, { exact: true }).click();

    const modal = app.locator('.ant-modal').filter({ hasText: 'Sửa đảng viên' });
    await expect(modal.locator('.ant-modal-title')).toHaveText('Sửa đảng viên');
    await waitForModalOpen(modal);

    // Hai dòng chữ cũ dưới ô ngày đã bỏ hẳn.
    await expect(modal).not.toContainText('Ngày ghi trong quyết định kết nạp');
    await expect(modal).not.toContainText('Để trống cũng được nếu chưa có trong hồ sơ');
    await expect(modal.locator('.ant-form-item-extra')).toHaveCount(0);

    // Ngày sinh và Giới tính nằm cùng một hàng: hai nhãn thẳng hàng, hai ô cao
    // bằng nhau, ô Giới tính nằm bên phải ô Ngày sinh.
    const birthLabel = modal.locator('label[for="dateOfBirth"]');
    const genderLabel = modal.locator('label[for="gender"]');
    const birthBox = (await birthLabel.boundingBox())!;
    const genderBox = (await genderLabel.boundingBox())!;
    expect(Math.abs(birthBox.y - genderBox.y)).toBeLessThanOrEqual(1);
    expect(genderBox.x).toBeGreaterThan(birthBox.x);

    const picker = (await modal.locator('.ant-picker').nth(1).boundingBox())!;
    const segmented = (await modal.locator('.ant-segmented').boundingBox())!;
    expect(Math.abs(picker.height - segmented.height)).toBeLessThanOrEqual(1);
    expect(Math.abs(picker.y - segmented.y)).toBeLessThanOrEqual(1);

    // Ba lựa chọn chia đều bề ngang ô Giới tính.
    const widths = await modal
      .locator('.ant-segmented-item')
      .evaluateAll((items) => items.map((item) => Math.round(item.getBoundingClientRect().width)));
    expect(widths).toHaveLength(3);
    expect(Math.max(...widths) - Math.min(...widths)).toBeLessThanOrEqual(1);

    // Ô Ghi chú ở cuối form, có bộ đếm và viên ngày ghi của người đang sửa.
    await expect(modal.getByTestId('note-counter')).toHaveText(
      `${WITH_NOTE.note.length} / ${NOTE_MAX}`,
    );
    await expect(modal.getByTestId('note-date-pill')).toHaveText(
      noteDateTextOf(WITH_NOTE.noteUpdatedAt),
    );

    // Form hiện TRỌN, không chỗ nào phải cuộn.
    const overflow = await modal.evaluate((node) => {
      const body = node.querySelector('.ant-modal-body') as HTMLElement;
      const wrap = node.closest('.ant-modal-wrap') as HTMLElement;
      return {
        body: body.scrollHeight - body.clientHeight,
        wrap: wrap.scrollHeight - wrap.clientHeight,
      };
    });
    expect(overflow.body).toBeLessThanOrEqual(1);
    expect(overflow.wrap).toBeLessThanOrEqual(1);
  });

  test('ca 6 · form Thêm cũng hiện trọn và ô Ghi chú bắt đầu trống', async ({ app }) => {
    await installTwoMembers(app);
    await app.goto('/dang-vien');
    await app.getByRole('button', { name: '+ Thêm', exact: true }).click();

    const modal = app.locator('.ant-modal').filter({ hasText: 'Thêm đảng viên' });
    await waitForModalOpen(modal);
    await expect(modal.getByTestId('note-counter')).toHaveText(`0 / ${NOTE_MAX}`);
    // Chưa ghi gì thì chưa có ngày ghi.
    await expect(modal.getByTestId('note-date-pill')).toHaveCount(0);

    const overflow = await modal.evaluate((node) => {
      const body = node.querySelector('.ant-modal-body') as HTMLElement;
      return body.scrollHeight - body.clientHeight;
    });
    expect(overflow).toBeLessThanOrEqual(1);
  });
});

test('ca 7 · Dashboard: khối gọn hiện đúng số người và đúng chỗ', async ({ app }) => {
  await app.goto('/');

  const summary = app.getByTestId('note-summary');
  await expect(summary).toBeVisible();
  await expect(summary.locator('.hhd-note-summary__count')).toHaveText(
    `Ghi chú · ${NOTED_COUNT} người`,
  );

  // Dòng 2 nối các ghi chú dạng `Họ tên: nội dung`, xuống dòng thành dấu cách.
  const line = await summary.locator('.hhd-note-summary__line').innerText();
  expect(line.startsWith(`${NOTED_ROWS[0].fullName}: `)).toBe(true);
  expect(line).not.toContain('\n');

  // Khối nằm sát BÊN TRÁI nút Xuất Excel, trên cùng hàng đầu thẻ.
  const summaryBox = (await summary.boundingBox())!;
  const exportBox = (await app.getByRole('button', { name: 'Xuất Excel' }).boundingBox())!;
  expect(summaryBox.x + summaryBox.width).toBeLessThanOrEqual(exportBox.x + 1);
  expect(Math.abs(summaryBox.y - exportBox.y)).toBeLessThanOrEqual(6);

  // Hàng đầu thẻ không cao thêm: khối không cao hơn nút bên cạnh.
  expect(summaryBox.height).toBeLessThanOrEqual(exportBox.height);
});

test('ca 8 · không ai có ghi chú thì không hiện khối', async ({ app }) => {
  await installNoNotes(app);
  await app.goto('/');

  await expect(app.getByRole('button', { name: 'Xuất Excel' })).toBeVisible();
  await expect(app.getByTestId('note-summary')).toHaveCount(0);
});

test('ca 9 · Chi tiết đợt dùng đúng khối gọn đó, cùng vị trí', async ({ app }) => {
  await app.goto(`/dot-trao-huy-hieu/${DETAIL_PERIOD.id}`);

  const summary = app.getByTestId('note-summary');
  await expect(summary.locator('.hhd-note-summary__count')).toHaveText(
    `Ghi chú · ${NOTED_COUNT} người`,
  );

  const summaryBox = (await summary.boundingBox())!;
  const exportBox = (await app.getByRole('button', { name: 'Xuất Excel' }).boundingBox())!;
  expect(summaryBox.x + summaryBox.width).toBeLessThanOrEqual(exportBox.x + 1);
  expect(Math.abs(summaryBox.y - exportBox.y)).toBeLessThanOrEqual(6);
});

test('ca 10 · hộp đầy đủ: không có dòng đếm người, mọi mục có viên vàng', async ({ app }) => {
  await app.goto('/');
  await app.getByTestId('note-summary').click();

  const dialog = app.locator('.hhd-note-dialog');
  await expect(dialog.locator('.ant-modal-title')).toHaveText(
    `Ghi chú — ${UPCOMING_PERIOD.name} năm ${UPCOMING_PERIOD.year}`,
  );

  // Dòng "x / y người đủ điều kiện có ghi chú" đã bỏ theo yêu cầu chủ dự án.
  await expect(dialog).not.toContainText('người đủ điều kiện có ghi chú');

  const items = dialog.locator('.hhd-note-dialog__item');
  await expect(items).toHaveCount(NOTED_COUNT);
  // Mọi mục đều có viên vàng, kể cả ghi chú của năm trước.
  await expect(dialog.getByTestId('note-date-pill')).toHaveCount(NOTED_COUNT);

  // Thứ tự Mốc rồi Họ tên, giống bảng.
  const names = await dialog.locator('.hhd-note-dialog__name').allInnerTexts();
  expect(names).toEqual(NOTED_ROWS.map((row) => row.fullName));

  // Ghi chú của năm trước vẫn hiện đúng năm đó trong viên vàng.
  const lastYear = NOTED_ROWS.find((row) => !row.noteUpdatedAt!.startsWith(`${SERVER_YEAR}-`))!;
  await expect(dialog.getByTestId('note-date-pill')).toContainText([
    ...NOTED_ROWS.map((row) => noteDateTextOf(row.noteUpdatedAt!)),
  ]);
  expect(noteDateTextOf(lastYear.noteUpdatedAt!)).toContain(`${SERVER_YEAR - 1}`);
});

test('ca 11 · ô tìm của hộp đầy đủ lọc đúng, tô sáng, và báo khi không khớp', async ({ app }) => {
  await app.goto('/');
  await app.getByTestId('note-summary').click();

  const dialog = app.locator('.hhd-note-dialog');
  const search = dialog.getByLabel('Tìm trong ghi chú', { exact: true });
  const items = dialog.locator('.hhd-note-dialog__item');

  // Tìm theo HỌ TÊN.
  const target = NOTED_ROWS[0];
  const byName = NOTED_ROWS.filter(
    (row) => row.fullName.includes(target.fullName) || row.note!.includes(target.fullName),
  ).length;
  await search.fill(target.fullName);
  await expect(items).toHaveCount(byName);
  await expect(items.first().locator('.hhd-note-dialog__name mark')).toHaveText(target.fullName);

  // Tìm theo NỘI DUNG ghi chú — chữ chỉ có trong một ghi chú duy nhất.
  const contentNeedle = 'Khu phố 3';
  const expectedCount = NOTED_ROWS.filter((row) => row.note!.includes(contentNeedle)).length;
  await search.fill(contentNeedle);
  await expect(items).toHaveCount(expectedCount);
  await expect(items.first().locator('.hhd-note-text mark').first()).toHaveText(contentNeedle);

  // Không phân biệt hoa thường, đúng như ô tìm của bảng.
  await search.fill(contentNeedle.toUpperCase());
  await expect(items).toHaveCount(expectedCount);

  // Không khớp gì.
  await search.fill('xyz không có thật');
  await expect(items).toHaveCount(0);
  await expect(dialog).toContainText('Không tìm thấy ghi chú nào khớp.');

  // Đóng rồi mở lại: ô tìm trống, danh sách đủ trở lại.
  await dialog.getByRole('button', { name: 'Đóng' }).click();
  await expect(dialog).toBeHidden();
  await app.getByTestId('note-summary').click();
  await expect(app.locator('.hhd-note-dialog').getByLabel('Tìm trong ghi chú')).toHaveValue('');
  await expect(app.locator('.hhd-note-dialog .hhd-note-dialog__item')).toHaveCount(NOTED_COUNT);
});

test('ca 12 · ô tìm đứng yên khi danh sách trong hộp cuộn', async ({ app }) => {
  await installManyNotes(app);
  await app.goto('/');
  await app.getByTestId('note-summary').click();

  const dialog = app.locator('.hhd-note-dialog');
  const list = dialog.getByTestId('note-dialog-list');
  await expect(dialog.locator('.hhd-note-dialog__item')).toHaveCount(MANY_NOTES);
  await waitForModalOpen(dialog);

  // Ô tìm nằm NGOÀI vùng cuộn — đó mới là lý do nó đứng yên.
  await expect(list.getByLabel('Tìm trong ghi chú')).toHaveCount(0);

  // Đo trong CÙNG một lần chạy trên trình duyệt: cuộn danh sách tới đáy rồi so
  // vị trí ô tìm trước và sau. Đo bằng hai lời gọi rời nhau thì lớp nổi của Ant
  // Design có thể đã dựng lại giữa chừng, con số không còn so được với nhau.
  const moved = await dialog.evaluate((node) => {
    const scroller = node.querySelector('[data-testid="note-dialog-list"]') as HTMLElement;
    const box = node.querySelector('.hhd-note-dialog__search') as HTMLElement;
    const before = box.getBoundingClientRect().y;
    scroller.scrollTop = scroller.scrollHeight;
    return {
      scrolled: scroller.scrollTop,
      shift: Math.abs(box.getBoundingClientRect().y - before),
    };
  });

  // Danh sách thật sự có cuộn, mà ô tìm không nhúc nhích.
  expect(moved.scrolled).toBeGreaterThan(0);
  expect(moved.shift).toBeLessThanOrEqual(1);
});

test('ca 13 · chống tràn ngang: chuỗi dài không dấu cách vẫn tự ngắt dòng', async ({ app }) => {
  await app.goto('/');
  await app.getByTestId('note-summary').click();

  const dialog = app.locator('.hhd-note-dialog');
  await waitForModalOpen(dialog);
  const overflow = await dialog
    .locator('.hhd-note-text')
    .evaluateAll((nodes) => nodes.map((node) => node.scrollWidth - node.clientWidth));
  expect(Math.max(...overflow)).toBeLessThanOrEqual(0);

  // Vùng cuộn của hộp chỉ cuộn dọc, không sinh thanh cuộn ngang.
  const list = await dialog
    .getByTestId('note-dialog-list')
    .evaluate((node) => node.scrollWidth - node.clientWidth);
  expect(list).toBeLessThanOrEqual(0);

  // Toàn văn giữ đúng xuống dòng của người viết.
  await expect(dialog.locator('.hhd-note-text').first()).toHaveCSS('white-space', 'pre-wrap');
});

test('ca 14 · khối gọn và hộp đầy đủ không đổi theo ô tìm của bảng', async ({ app }) => {
  await app.goto('/');

  const summary = app.getByTestId('note-summary');
  await expect(summary.locator('.hhd-note-summary__count')).toHaveText(
    `Ghi chú · ${NOTED_COUNT} người`,
  );

  // Lọc bảng về một người duy nhất; khối gọn vẫn nói về cả đợt.
  await app.getByLabel('Tìm theo họ tên', { exact: true }).first().fill(NOTED_ROWS[0].fullName);
  await expect(summary.locator('.hhd-note-summary__count')).toHaveText(
    `Ghi chú · ${NOTED_COUNT} người`,
  );

  await summary.getByRole('button', { name: 'Xem đầy đủ ghi chú' }).click();
  await expect(app.locator('.hhd-note-dialog .hhd-note-dialog__item')).toHaveCount(NOTED_COUNT);
});

test('ca 15 · ngày ghi lấy nguyên ngày máy chủ, không lệch theo múi giờ', async ({ app }) => {
  // Ghi chú viết lúc 01:00 giờ Việt Nam: quy về UTC sẽ lùi một ngày. Giao diện
  // phải hiện đúng ngày máy chủ đã đóng (mục 1.6 hợp đồng v1.6).
  await app.route('**/api/**', async (route) => {
    const path = new URL(route.request().url()).pathname.replace(/^.*\/api/, '');
    if (path !== '/PartyMembers') return route.fallback();
    const rows = [
      memberRow(
        't74-sang-som',
        'Hoàng Thị Hà',
        'Ghi lúc rạng sáng.',
        `${SERVER_TODAY}T01:00:00+07:00`,
      ),
    ];
    return route.fulfill(
      envelope({
        pagedData: rows,
        pageInfo: { current: 1, pageSize: 20, totalCount: 1, totalPages: 1 },
      }),
    );
  });
  await app.goto('/dang-vien');

  const [year, month, day] = SERVER_TODAY.split('-');
  await app.getByLabel('Ghi chú Hoàng Thị Hà', { exact: true }).click();
  await expect(app.getByTestId('note-date-pill')).toHaveText(`Ghi ngày ${day}/${month}/${year}`);
});

test.describe('hàng đầu thẻ ở khung laptop 1366x650', () => {
  test.use({ viewport: { width: 1366, height: 650 } });

  /** Số dòng bảng nằm TRỌN trong vùng nhìn thấy của thẻ danh sách. */
  async function visibleRowCount(page: Page): Promise<number> {
    return page.locator('.hhd-dashboard__panel').evaluate((panel) => {
      const scroller = panel.querySelector('.ant-table-body, .ant-table-content') as HTMLElement;
      const view = scroller.getBoundingClientRect();
      return [...panel.querySelectorAll('.ant-table-tbody tr.ant-table-row')].filter((row) => {
        const box = row.getBoundingClientRect();
        return box.top >= view.top - 1 && box.bottom <= view.bottom + 1;
      }).length;
    });
  }

  test('ca 16 · khối gọn không lấy mất dòng nào của bảng', async ({ app }) => {
    // Có khối ghi chú.
    await app.goto('/');
    await expect(app.getByTestId('note-summary')).toBeVisible();
    const headWithNotes = await app
      .locator('.hhd-dashboard__panel-head')
      .evaluate((node) => Math.round(node.getBoundingClientRect().height));
    const rowsWithNotes = await visibleRowCount(app);

    // Không có khối ghi chú — đúng dáng của `main` trước việc này.
    await installNoNotes(app);
    await app.reload();
    await expect(app.getByTestId('note-summary')).toHaveCount(0);
    const headWithout = await app
      .locator('.hhd-dashboard__panel-head')
      .evaluate((node) => Math.round(node.getBoundingClientRect().height));
    const rowsWithout = await visibleRowCount(app);

    expect(headWithNotes).toBe(headWithout);
    expect(rowsWithNotes).toBe(rowsWithout);
  });
});

/**
 * Hai ca cuối chạy ở CẢ BỐN khung nhìn của T60, lấy thẳng danh sách khung từ
 * `playwright.ui.config.ts` để không bao giờ lệch với bộ T60.
 *
 * Ca 5 và ca 16 chỉ chạy ở 1366x650 nên đã để lọt hai lỗi CEO bắt được ở lượt
 * gác cổng đầu: nhãn "Để trống" bị cắt ở thang chữ 16px (1440x900 trở lên), và
 * khối gọn đẩy nút Xuất Excel xuống dòng thứ hai ở cận dưới 1280x600.
 */
for (const viewport of RESPONSIVE_VIEWPORTS) {
  const frame = `${viewport.width}x${viewport.height}`;

  test.describe(`bốn khung T60 — ${frame}`, () => {
    test.use({ viewport });

    test(`ca 17 · ${frame} · ba nhãn Giới tính hiện trọn ở cả form Thêm và form Sửa`, async ({
      app,
    }) => {
      await installTwoMembers(app);
      await app.goto('/dang-vien');

      /** Nhãn nào có `scrollWidth` vượt `clientWidth` là đang bị cắt bằng "…". */
      async function expectLabelsFit(title: string) {
        const modal = app.locator('.ant-modal').filter({ hasText: title });
        await expect(modal.locator('.ant-segmented')).toBeVisible();
        await waitForModalOpen(modal);

        const labels = await modal.locator('.ant-segmented-item-label').evaluateAll((nodes) =>
          nodes.map((node) => ({
            text: node.textContent ?? '',
            clientWidth: node.clientWidth,
            scrollWidth: node.scrollWidth,
          })),
        );

        expect(labels.map((label) => label.text)).toEqual(['Nam', 'Nữ', 'Để trống']);
        for (const label of labels) {
          expect(
            label.scrollWidth,
            `${title} · nhãn "${label.text}" bị cắt ở ${frame}`,
          ).toBeLessThanOrEqual(label.clientWidth);
        }

        // Những điều đã duyệt vẫn giữ: ba ô chia đều, ô Giới tính cao bằng ô
        // ngày và nằm cùng hàng với nó.
        //
        // Đo bằng `offsetWidth`/`offsetTop` chứ không bằng `boundingBox()`: hộp
        // của Ant Design mở bằng hiệu ứng phóng to, nên hình chữ nhật trên màn
        // trong lúc hiệu ứng chạy còn đang bị thu nhỏ. Hai số này là số của bố
        // cục nên không dính hiệu ứng.
        const shape = await modal.evaluate((node) => {
          const items = [...node.querySelectorAll('.ant-segmented-item')] as HTMLElement[];
          const birthPicker = node.querySelectorAll('.ant-picker')[1] as HTMLElement;
          const segmented = node.querySelector('.ant-segmented') as HTMLElement;
          return {
            itemWidths: items.map((item) => item.offsetWidth),
            pickerHeight: birthPicker.offsetHeight,
            segmentedHeight: segmented.offsetHeight,
            pickerTop: birthPicker.getBoundingClientRect().top - node.getBoundingClientRect().top,
            segmentedTop: segmented.getBoundingClientRect().top - node.getBoundingClientRect().top,
          };
        });

        expect(Math.max(...shape.itemWidths) - Math.min(...shape.itemWidths)).toBeLessThanOrEqual(
          1,
        );
        expect(shape.segmentedHeight).toBe(shape.pickerHeight);
        expect(Math.abs(shape.segmentedTop - shape.pickerTop)).toBeLessThanOrEqual(1);
      }

      await app.getByRole('button', { name: '+ Thêm', exact: true }).click();
      await expectLabelsFit('Thêm đảng viên');
      await app
        .locator('.ant-modal')
        .filter({ hasText: 'Thêm đảng viên' })
        .getByRole('button', { name: 'Đóng' })
        .click();

      await app.getByLabel(`Sửa ${WITH_NOTE.fullName}`, { exact: true }).click();
      await expectLabelsFit('Sửa đảng viên');
    });

    test(`ca 18 · ${frame} · khối gọn không làm hàng đầu thẻ cao thêm`, async ({ app }) => {
      const SCREENS = [
        { ten: 'Dashboard', url: '/', head: '.hhd-dashboard__panel-head' },
        {
          ten: 'Chi tiết đợt',
          url: `/dot-trao-huy-hieu/${DETAIL_PERIOD.id}`,
          head: '.hhd-eligibility__toolbar',
        },
      ];

      /** Chiều cao hàng đầu thẻ, đo theo bố cục chứ không theo hình trên màn. */
      function headHeight(head: string) {
        return app.locator(head).evaluate((node: HTMLElement) => node.offsetHeight);
      }

      // Lượt 1 — CÓ khối ghi chú. Đo cả hai màn trước, rồi mới thay tầng dữ
      // liệu giả: `page.unrouteAll` sẽ gỡ luôn route của fixture nên không dùng.
      const withNotes = new Map<string, number>();
      for (const screen of SCREENS) {
        await app.goto(screen.url);
        const summary = app.getByTestId('note-summary');
        await expect(summary).toBeVisible();
        withNotes.set(screen.ten, await headHeight(screen.head));

        const summaryBox = (await summary.boundingBox())!;
        const exportBox = (await app.getByRole('button', { name: 'Xuất Excel' }).boundingBox())!;
        // Khối kết thúc trước khi nút bắt đầu — tức nằm sát bên trái nút.
        expect(
          summaryBox.x + summaryBox.width,
          `${screen.ten} ở ${frame}: khối không nằm bên trái nút Xuất Excel`,
        ).toBeLessThanOrEqual(exportBox.x + 1);
        // Cùng một hàng: hai ô chồng nhau theo chiều dọc.
        expect(
          summaryBox.y < exportBox.y + exportBox.height &&
            exportBox.y < summaryBox.y + summaryBox.height,
          `${screen.ten} ở ${frame}: nút Xuất Excel rơi xuống dòng khác`,
        ).toBe(true);
      }

      // Lượt 2 — KHÔNG ai có ghi chú, tức đúng dáng của `main` trước việc này.
      await installNoNotes(app);
      for (const screen of SCREENS) {
        await app.goto(screen.url);
        await expect(app.getByRole('button', { name: 'Xuất Excel' })).toBeVisible();
        await expect(app.getByTestId('note-summary')).toHaveCount(0);

        expect(
          withNotes.get(screen.ten),
          `${screen.ten} ở ${frame}: hàng đầu thẻ cao thêm khi có khối ghi chú`,
        ).toBe(await headHeight(screen.head));
      }
    });
  });
}
