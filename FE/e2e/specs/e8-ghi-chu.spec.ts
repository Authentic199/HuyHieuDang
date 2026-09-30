import type { Locator, Page } from '@playwright/test';

import type { Api } from '../fixtures/api';
import {
  dismissToasts,
  expect,
  modal,
  signIn,
  tableRows,
  test,
  uncoveredBadge,
} from '../fixtures/app';
import { displayDate } from '../fixtures/clock';
import { eligibility, scenario } from '../fixtures/expected';

/**
 * E2E-8 · Ghi chú đảng viên (QT12; UC-20, UC-21, UC-22, UC-26, UC-11, UC-34).
 *
 * Trạng thái đầu: 4 đợt, cài đặt 30/90/5 kèm tên đơn vị, bộ lõi 32 người, mốc T0.
 *
 * Một người đủ điều kiện của đợt sắp tới đi trọn vòng đời một ghi chú: ghi từ
 * nút ở bảng Đảng viên → khối gọn và hộp đầy đủ ở Dashboard → sửa nội dung
 * trong form Sửa → sửa họ tên mà không đụng ghi chú → xóa ghi chú. Cuối luồng
 * mọi con số nghiệp vụ phải y như lúc đầu: ghi chú chỉ để đọc, không tham gia
 * QT1–QT11.
 *
 * **Không phụ thuộc ngày chạy.** Mốc T0 ép cả hai đồng hồ, nên `Ghi ngày` luôn
 * đọc ra ngày máy chủ đang báo. Phần NGÀY của ngày ghi vì vậy đứng yên suốt lần
 * chạy — "ngày ghi có đổi hay không" là điều màn hình không nói được, nên hai
 * bước QT12.3 đối chiếu giá trị đầy đủ lấy từ máy chủ.
 */

const CASE = scenario('core_default_T0');
const UPCOMING = CASE.upcomingPeriod!;
const UPCOMING_ELIGIBLE = eligibility('core_default_T0', UPCOMING.code, 2026);

/** Người mang ghi chú suốt luồng: dòng đầu danh sách đủ điều kiện của đợt sắp tới. */
const SUBJECT = UPCOMING_ELIGIBLE.rows[0].fullName;

/** Họ tên mới của bước E8-05 — đổi tên mà ghi chú phải đứng yên. */
const SUBJECT_RENAMED = `${SUBJECT} (đã sửa tên)`;

const NOTE_FIRST = 'Hồ sơ còn thiếu bản sao quyết định kết nạp.\nGia đình xin nhận thay.';
const NOTE_SECOND = 'Đã bổ sung đủ hồ sơ ngày 30/09, chờ ký.';

/** Một dòng chữ như khối gọn dựng: xuống dòng thu về dấu cách. */
function summaryLineFor(fullName: string, note: string): string {
  return `${fullName}: ${note.replace(/\s+/g, ' ')}`;
}

/** Nút ghi chú ở cột Thao tác của một dòng. */
function noteButton(page: Page, fullName: string): Locator {
  return page.getByLabel(`Ghi chú ${fullName}`, { exact: true });
}

/** Khối gọn `Ghi chú · N người` ở hàng đầu thẻ danh sách. */
function noteSummary(page: Page): Locator {
  return page.getByTestId('note-summary');
}

/** Hộp `Ghi chú — <Tên đợt> năm <năm>` đang mở. */
function notesDialog(page: Page): Locator {
  return page.locator('.hhd-note-dialog');
}

/** Ghi hoặc xóa ghi chú qua hộp của một người, mở từ nút ở cột Thao tác (UC-26). */
async function saveNoteThroughTable(page: Page, fullName: string, note: string): Promise<void> {
  await noteButton(page, fullName).click();
  const dialog = page.locator('.ant-modal').filter({ hasText: `Ghi chú — ${fullName}` });
  await dialog.getByLabel('Ghi chú', { exact: true }).fill(note);
  await dialog.getByRole('button', { name: 'Lưu ghi chú' }).click();
  await expect(dialog).toBeHidden();
  await expect(page.getByText('Đã lưu thay đổi')).toBeVisible();
  await dismissToasts(page);
}

/**
 * Đợi đồng hồ máy chủ sang **giây** mới, bằng cách ghi thử lên một người đứng
 * ngoài danh sách đủ điều kiện cho tới khi máy chủ đóng một mốc khác mốc đã cho.
 *
 * Cần bước này vì hợp đồng API trả ngày ghi ở độ chính xác giây: hai lần lưu
 * rơi vào cùng một giây sẽ cho cùng một chuỗi, và lúc đó ca "ngày ghi phải đổi"
 * xanh hay đỏ là chuyện may rủi. Đây là chờ theo ĐIỀU KIỆN trên trạng thái thật
 * của máy chủ, không phải chờ một quãng cố định (ca E-905), và luôn xong trong
 * khoảng một giây.
 */
async function waitForNextServerSecond(
  api: Api,
  probeMemberId: string,
  previous: string,
): Promise<void> {
  let round = 0;
  await expect(async () => {
    round += 1;
    const probe = await api.saveNote(probeMemberId, `moc-thoi-gian-${round}`);
    expect(probe.noteUpdatedAt).not.toBe(previous);
  }).toPass({ timeout: 15_000, intervals: [50, 100, 200] });

  await api.saveNote(probeMemberId, null);
}

test('E2E-8 · Ghi chú đảng viên: ghi → Dashboard → hộp đầy đủ → sửa → xóa', async ({
  page,
  api,
}) => {
  await api.seedBaseline();
  await signIn(page, api);

  const before = await api.dashboard();
  const serverToday = before.today;
  const writtenOn = `Ghi ngày ${displayDate(serverToday)}`;
  const baselineBadge = (await api.unassignedCount()).count;

  // Người "mồi" của hàm đợi giây: phải đứng NGOÀI danh sách đủ điều kiện của đợt
  // sắp tới, để ghi chú tạm của nó không bao giờ lọt vào khối gọn đang kiểm.
  const eligibleNames = new Set(UPCOMING_ELIGIBLE.rows.map((row) => row.fullName));
  const probe = (await api.notedMembers()).pagedData.find(
    (row) => !eligibleNames.has(row.fullName),
  );
  expect(probe, 'bộ lõi phải có người đứng ngoài danh sách đủ điều kiện để làm mồi').toBeTruthy();

  let writtenAt = '';

  await test.step('E8-01 · Ghi chú từ nút ở bảng Đảng viên, tooltip nêu đúng ngày ghi', async () => {
    await page.goto('/dang-vien');

    // Chưa ghi gì: nút để viền và tooltip mời thêm ghi chú.
    const button = noteButton(page, SUBJECT);
    await expect(button).toBeVisible();
    await expect(button).not.toHaveClass(/hhd-members__note--has/);
    await button.hover();
    await expect(page.getByRole('tooltip', { name: 'Thêm ghi chú' })).toBeVisible();
    await page.getByRole('heading', { name: 'Đảng viên' }).hover();

    await saveNoteThroughTable(page, SUBJECT, NOTE_FIRST);

    // Ghi xong: nút chuyển sang nền vàng, tooltip mang đúng ngày ghi máy chủ đóng.
    await expect(button).toHaveClass(/hhd-members__note--has/);
    await button.hover();
    await expect(
      page.getByRole('tooltip', {
        name: `Ghi chú · ${writtenOn.replace('Ghi ngày', 'ghi ngày')}`,
      }),
    ).toBeVisible();

    const saved = await api.memberByName(SUBJECT);
    expect(saved.note).toBe(NOTE_FIRST);
    expect(saved.noteUpdatedAt).not.toBeNull();
    expect(saved.noteUpdatedAt!.slice(0, 10)).toBe(serverToday);
    writtenAt = saved.noteUpdatedAt!;
  });

  await test.step('E8-02 · Dashboard hiện khối `Ghi chú · 1 người` đúng chỗ', async () => {
    await page.goto('/');

    const summary = noteSummary(page);
    await expect(summary).toBeVisible();
    await expect(summary.locator('.hhd-note-summary__count')).toHaveText('Ghi chú · 1 người');
    await expect(summary.locator('.hhd-note-summary__line')).toHaveText(
      summaryLineFor(SUBJECT, NOTE_FIRST),
    );

    // Khối đứng sát bên trái nút Xuất Excel, cùng một hàng với nó.
    const exportButton = page.getByRole('button', { name: 'Xuất Excel' }).first();
    const summaryBox = (await summary.boundingBox())!;
    const exportBox = (await exportButton.boundingBox())!;
    expect(summaryBox.x + summaryBox.width).toBeLessThanOrEqual(exportBox.x + 1);
    expect(Math.abs(summaryBox.y - exportBox.y)).toBeLessThanOrEqual(summaryBox.height);

    // Khối không lấy đi dòng nào của bảng: cả sáu người đủ điều kiện vẫn thấy.
    await expect(tableRows(page)).toHaveCount(UPCOMING_ELIGIBLE.total);
  });

  await test.step('E8-03 · Hộp đầy đủ mở ra, tìm thấy ghi chú bằng ô tìm', async () => {
    await noteSummary(page).click();

    const dialog = notesDialog(page);
    await expect(dialog.locator('.ant-modal-title')).toHaveText(
      `Ghi chú — ${UPCOMING.name} năm ${UPCOMING.year}`,
    );
    await expect(dialog.getByTestId('note-date-pill')).toHaveText(writtenOn);
    await expect(dialog.locator('.hhd-note-dialog__name')).toHaveText(SUBJECT);

    // Toàn văn giữ đúng chỗ xuống dòng của người viết.
    const fullText = await dialog.locator('.hhd-note-text').innerText();
    for (const line of NOTE_FIRST.split('\n')) {
      expect(fullText).toContain(line);
    }

    // Ô tìm: gõ chữ trong nội dung ghi chú thì vẫn thấy mục đó.
    const search = dialog.getByLabel('Tìm trong ghi chú', { exact: true });
    await search.fill('quyết định');
    await expect(dialog.locator('.hhd-note-dialog__item')).toHaveCount(1);

    // Gõ chuỗi không có thì hộp nói rõ, không để trống lửng lơ.
    await search.fill('khong-he-co-trong-ghi-chu');
    await expect(dialog.locator('.hhd-note-dialog__item')).toHaveCount(0);
    await expect(dialog.getByText('Không tìm thấy ghi chú nào khớp.')).toBeVisible();

    await dialog.getByRole('button', { name: 'Đóng' }).click();
    await expect(dialog).toBeHidden();

    // Mở lại thì ô tìm trống.
    await noteSummary(page).click();
    await expect(notesDialog(page).getByLabel('Tìm trong ghi chú', { exact: true })).toHaveValue(
      '',
    );
    await notesDialog(page).getByRole('button', { name: 'Đóng' }).click();
  });

  await test.step('E8-04 · Sửa nội dung ghi chú trong form Sửa thì ngày ghi đổi', async () => {
    await waitForNextServerSecond(api, probe!.id, writtenAt);

    await page.goto('/dang-vien');
    await page.getByLabel(`Sửa ${SUBJECT}`, { exact: true }).click();

    const form = modal(page);
    await expect(form.getByTestId('note-date-pill')).toHaveText(writtenOn);
    await expect(form.getByTestId('note-counter')).toHaveText(`${NOTE_FIRST.length} / 500`);

    await form.getByLabel('Ghi chú', { exact: true }).fill(NOTE_SECOND);
    await expect(form.getByTestId('note-counter')).toHaveText(`${NOTE_SECOND.length} / 500`);
    await form.getByRole('button', { name: 'Lưu thay đổi' }).click();
    await expect(form).toBeHidden();
    await expect(page.getByText('Đã lưu thay đổi')).toBeVisible();
    await dismissToasts(page);

    const saved = await api.memberByName(SUBJECT);
    expect(saved.note).toBe(NOTE_SECOND);
    expect(saved.noteUpdatedAt).not.toBe(writtenAt);
    expect(saved.noteUpdatedAt!.slice(0, 10)).toBe(serverToday);
    writtenAt = saved.noteUpdatedAt!;
  });

  await test.step('E8-05 · Sửa họ tên mà giữ nguyên ghi chú thì ngày ghi không đổi', async () => {
    await waitForNextServerSecond(api, probe!.id, writtenAt);

    await page.getByLabel(`Sửa ${SUBJECT}`, { exact: true }).click();

    const form = modal(page);
    await form.getByLabel('Họ tên').fill(SUBJECT_RENAMED);
    await form.getByRole('button', { name: 'Lưu thay đổi' }).click();
    await expect(form).toBeHidden();
    await expect(page.getByText('Đã lưu thay đổi')).toBeVisible();
    await dismissToasts(page);

    const saved = await api.memberByName(SUBJECT_RENAMED);
    expect(saved.note).toBe(NOTE_SECOND);
    expect(saved.noteUpdatedAt).toBe(writtenAt);

    // Khối gọn theo tên mới, nội dung ghi chú và ngày ghi y nguyên.
    await page.goto('/');
    await expect(noteSummary(page).locator('.hhd-note-summary__line')).toHaveText(
      summaryLineFor(SUBJECT_RENAMED, NOTE_SECOND),
    );
  });

  await test.step('E8-06 · Xóa ghi chú thì khối gọn biến mất', async () => {
    await page.goto('/dang-vien');
    await saveNoteThroughTable(page, SUBJECT_RENAMED, '');

    await expect(noteButton(page, SUBJECT_RENAMED)).not.toHaveClass(/hhd-members__note--has/);

    const cleared = await api.memberByName(SUBJECT_RENAMED);
    expect(cleared.note).toBeNull();
    expect(cleared.noteUpdatedAt).toBeNull();

    await page.goto('/');
    await expect(noteSummary(page)).toHaveCount(0);
  });

  await test.step('E8-07 · Cả vòng ghi chú không đổi con số nghiệp vụ nào (QT12)', async () => {
    const after = await api.dashboard();

    expect(after.memberCount).toBe(before.memberCount);
    expect(after.upcomingPeriod!.name).toBe(before.upcomingPeriod!.name);
    expect(after.upcomingPeriod!.year).toBe(before.upcomingPeriod!.year);
    expect(after.upcomingPeriod!.eligibleCount).toBe(before.upcomingPeriod!.eligibleCount);
    expect(after.upcomingPeriod!.milestoneBreakdown).toEqual(
      before.upcomingPeriod!.milestoneBreakdown,
    );
    expect(after.warnings.unassignedCount).toBe(before.warnings.unassignedCount);
    expect((await api.unassignedCount()).count).toBe(baselineBadge);
    await expect(uncoveredBadge(page)).toHaveText(String(baselineBadge));
  });
});
