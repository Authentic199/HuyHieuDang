import type { Locator, Page } from '@playwright/test';

import type { Api } from '../fixtures/api';
import {
  dismissToasts,
  ELIGIBLE_COLUMN,
  expect,
  modal,
  navItem,
  PERIOD_COLUMN,
  readTable,
  selectYear,
  signIn,
  tableRows,
  test,
  uncoveredBadge,
  UNCOVERED_COLUMN,
} from '../fixtures/app';
import { downloadExcel, titleLines } from '../fixtures/download';
import { UNIT_NAME } from '../fixtures/env';
import { pickDayMonth } from '../fixtures/pickers';

/**
 * E2E-7 · Đợt trao huy hiệu vắt qua 31/12 (T54 — QT4, QT6, QT7, QT8, QT11).
 *
 * Luồng chạy trên stack thật: tạo đợt `01/12 – 28/02` qua modal rồi đi hết các
 * màn mà đợt vắt năm chạm tới — bảng đợt, dải độ phủ, banner khoảng trống, tab
 * Thông tin, tab Đủ điều kiện, màn Chưa thuộc đợt nào, tệp Excel — và cuối cùng
 * sửa qua lại giữa đợt vắt năm và đợt thường để chứng minh đổi chiều được.
 *
 * Không con số nào viết cứng theo năm chạy: mọi ngày suy từ NĂM MÁY CHỦ đang
 * báo về, còn các mốc tuổi đảng thì suy ngược từ ngày tròn mốc cần dựng. Luồng
 * chỉ đòi hỏi hôm nay nằm ngoài khoảng `01/12 – 28/02` và sau `28/02`, đúng
 * khoảng an toàn mà `fixtures/clock.ts` đã chốt cho cả bộ.
 */

const PERIOD = 'Đợt Giao thừa';
const SPANNING = { fromDay: 1, fromMonth: 12, toDay: 28, toMonth: 2 } as const;
const PLAIN = { fromDay: 1, fromMonth: 10, toDay: 7, toMonth: 11 } as const;

/** Mốc huy hiệu nhỏ nhất của cài đặt mặc định — dùng để suy ngược ngày vào Đảng. */
const MILESTONE = 30;

interface Seeded {
  fullName: string;
  gender: 'Male' | 'Female';
  /** Ngày tròn mốc cần dựng, dạng `MM-dd`. */
  anniversary: string;
  /** Ngày tròn mốc rơi vào năm máy chủ (0) hay năm sau (1). */
  yearOffset: 0 | 1;
  /** Vai trò của người này trong luồng, để đọc báo cáo là hiểu ngay. */
  intent: string;
}

const PEOPLE: Seeded[] = [
  {
    fullName: 'Bùi Văn Giêng',
    gender: 'Male',
    anniversary: '01-20',
    yearOffset: 0,
    intent: 'Tròn mốc 20/01 — thuộc đuôi đầu năm của lần diễn ra neo ở năm trước',
  },
  {
    fullName: 'Hoàng Thị Hè',
    gender: 'Female',
    anniversary: '06-15',
    yearOffset: 0,
    intent: 'Tròn mốc giữa năm — người duy nhất rơi vào khoảng trống',
  },
  {
    fullName: 'Lý Văn Biên',
    gender: 'Male',
    anniversary: '12-01',
    yearOffset: 0,
    intent: 'Tròn mốc đúng Từ ngày 01/12 — biên mở của đợt',
  },
  {
    fullName: 'Đinh Văn Chạp',
    gender: 'Male',
    anniversary: '12-15',
    yearOffset: 0,
    intent: 'Tròn mốc 15/12 — nửa đầu của lần diễn ra neo ở năm nay',
  },
  {
    fullName: 'Cao Thị Hai',
    gender: 'Female',
    anniversary: '02-10',
    yearOffset: 1,
    intent: 'Tròn mốc 10/02 năm sau — nửa sau của chính lần diễn ra ấy',
  },
  {
    fullName: 'Mai Thị Cuối',
    gender: 'Female',
    anniversary: '02-28',
    yearOffset: 1,
    intent: 'Tròn mốc đúng Đến ngày 28/02 năm sau — biên đóng của đợt',
  },
];

/** `2026-12-01` → `01/12/2026`. */
function display(iso: string): string {
  return `${iso.slice(8, 10)}/${iso.slice(5, 7)}/${iso.slice(0, 4)}`;
}

/** Ngày vào Đảng chính thức để tròn mốc 30 đúng ngày mong muốn. */
function admissionFor(person: Seeded, serverYear: number): string {
  return `${serverYear + person.yearOffset - MILESTONE}-${person.anniversary}`;
}

function anniversaryOf(person: Seeded, serverYear: number): string {
  return `${serverYear + person.yearOffset}-${person.anniversary}`;
}

/** Tọa độ của một vạch trên dải độ phủ, đọc thẳng từ thuộc tính `style`. */
async function barBox(bar: Locator): Promise<{ left: number; width: number }> {
  const style = (await bar.getAttribute('style')) ?? '';
  const left = Number(/left:\s*([\d.]+)%/.exec(style)?.[1] ?? NaN);
  const width = Number(/width:\s*([\d.]+)%/.exec(style)?.[1] ?? NaN);
  return { left, width };
}

/** Mở modal Sửa của đợt rồi đặt lại cả hai ngày. */
async function editPeriod(
  page: Page,
  dates: { fromDay: number; fromMonth: number; toDay: number; toMonth: number },
): Promise<void> {
  await navItem(page, 'Đợt trao huy hiệu').click();
  await page.getByRole('button', { name: `Sửa ${PERIOD}` }).click();

  const form = modal(page);
  await pickDayMonth(page, form.getByLabel('Từ ngày'), dates.fromDay, dates.fromMonth);
  await pickDayMonth(page, form.getByLabel('Đến ngày'), dates.toDay, dates.toMonth);
  await form.getByRole('button', { name: 'Lưu thay đổi' }).click();
  await expect(form).toBeHidden();
  await dismissToasts(page);
}

async function openTab(page: Page, api: Api, tab: string): Promise<void> {
  await page.goto(`/dot-trao-huy-hieu/${await api.periodIdByName(PERIOD)}`);
  await page.getByRole('tab', { name: tab }).click();
}

/** Vùng tab "Thông tin" của trang chi tiết đợt — tiêu đề trang không tính. */
function infoTab(page: Page): Locator {
  return page.getByRole('tabpanel', { name: 'Thông tin' });
}

/**
 * Mở màn "Chưa thuộc đợt nào" và CHỜ ĐÚNG MÀN ẤY dựng xong.
 *
 * Bảng của màn trước còn nằm trong DOM một nhịp sau khi bấm menu, nên chỉ đếm
 * số dòng thôi thì có lúc đếm nhầm bảng cũ. Chờ tiêu đề "Bị sót trong năm ..."
 * hiện ra mới là chờ theo điều kiện đúng (quy ước của E-905).
 */
async function openUncovered(page: Page, year: number): Promise<void> {
  await navItem(page, 'Chưa thuộc đợt nào').click();
  await expect(page).toHaveURL(/\/chua-thuoc-dot-nao$/);
  await expect(page.locator('.hhd-uncovered__title')).toContainText(`Bị sót trong năm ${year}`);
}

test('E2E-7 · Đợt trao huy hiệu vắt qua 31/12 chạy đúng trên mọi màn', async ({ page, api }) => {
  await api.resetAll();
  await api.saveSettings({
    startYears: 30,
    endYears: 90,
    stepYears: 5,
    unitName: UNIT_NAME,
  });

  const serverToday = (await api.dashboard()).today;
  const year = Number(serverToday.slice(0, 4));

  // Luồng dựng sẵn một đợt 01/12 – 28/02: nếu hôm nay rơi vào chính khoảng ấy
  // thì trạng thái và đợt sắp tới đọc khác hẳn, nên dừng ngay với lời giải
  // thích thay vì để các bước sau đỏ vì lý do khác.
  expect(
    serverToday >= `${year}-03-01` && serverToday <= `${year}-11-30`,
    `Luồng E7 cần hôm nay nằm trong 01/03 – 30/11; máy chủ đang báo ${serverToday}`,
  ).toBe(true);

  for (const person of PEOPLE) {
    await api.post('/PartyMembers', {
      fullName: person.fullName,
      gender: person.gender,
      dateOfBirth: null,
      officialAdmissionDate: admissionFor(person, year),
    });
  }

  await signIn(page, api);

  await test.step('E7-01 · Tạo đợt Từ 01/12, Đến 28/02 qua modal: lưu được, không lỗi đỏ', async () => {
    await page.goto('/dot-trao-huy-hieu');
    await page
      .getByRole('button', { name: /^\+ Thêm đợt( đầu tiên)?$/ })
      .first()
      .click();

    const form = modal(page);
    await form.getByLabel('Tên đợt').fill(PERIOD);
    await pickDayMonth(page, form.getByLabel('Từ ngày'), SPANNING.fromDay, SPANNING.fromMonth);
    await pickDayMonth(page, form.getByLabel('Đến ngày'), SPANNING.toDay, SPANNING.toMonth);

    // Không được có lời báo lỗi nào dưới ô Đến ngày (khóa Mes.AwardPeriod.Invalid.Range đã bỏ).
    await expect(form.locator('.ant-form-item-explain-error')).toHaveCount(0);
    await expect(form.getByText('Đến ngày phải bằng hoặc sau Từ ngày')).toBeHidden();

    await test.step('E7-02 · Dòng nhắc trong modal đổi thành câu "vắt qua 31/12" kèm đúng hai ngày', async () => {
      const note = form.locator('.hhd-period-form__note');
      await expect(note).toContainText('vắt qua 31/12');
      await expect(note).toContainText('01/12');
      await expect(note).toContainText('28/02');
      await expect(note).toContainText('năm sau');
    });

    await form.getByRole('button', { name: 'Thêm đợt', exact: true }).click();
    await expect(form).toBeHidden();
    await dismissToasts(page);
    await expect(tableRows(page)).toHaveCount(1);
  });

  await test.step('E7-03 · Bảng đợt: cột Đến ngày hiện 28/02 kèm chữ "năm sau"', async () => {
    const row = (await readTable(page))[0];
    expect(row[PERIOD_COLUMN.name]).toBe(PERIOD);
    expect(row[PERIOD_COLUMN.fromDisplay]).toBe('01/12');
    expect(row[PERIOD_COLUMN.toDisplay]).toBe('28/02 năm sau');

    const server = await api.periods(year);
    expect(server.periods[0].toDisplay).toBe('28/02');
    expect(row[PERIOD_COLUMN.eligibleCount]).toBe(`${server.periods[0].eligibleCount} người`);
    expect(server.periods[0].eligibleCount).toBe(4);
  });

  await test.step('E7-04 · Dải độ phủ cho đúng hai vạch, sát hai mép, không tự báo chồng lấn', async () => {
    const strip = page.locator('.hhd-coverage');
    await expect(strip).toBeVisible();

    const bars = strip.locator('.hhd-coverage__bar');
    await expect(bars).toHaveCount(2);

    const first = await barBox(bars.nth(0));
    const second = await barBox(bars.nth(1));
    expect(first.left).toBeLessThan(1);
    expect(second.left + second.width).toBeGreaterThan(99);
    expect(first.left + first.width).toBeLessThan(second.left);

    // Hai phần của cùng một đợt không phải chồng lấn (QT6).
    await expect(strip.locator('.hhd-coverage__overlap')).toHaveCount(0);
    expect((await api.periods(year)).warnings.overlaps).toHaveLength(0);
    await expect(page.locator('.hhd-periods__banner')).not.toContainText('chồng lấn');
  });

  await test.step('E7-05 · Banner khoảng trống nêu đúng khoảng giữa năm 01/03–30/11', async () => {
    const banner = page.locator('.hhd-periods__banner');
    await expect(banner).toContainText('chưa phủ kín');
    await expect(banner).toContainText('01/03–30/11');

    const gaps = (await api.periods(year)).warnings.gaps;
    expect(gaps).toHaveLength(1);
    expect(`${gaps[0].fromDisplay}–${gaps[0].toDisplay}`).toBe('01/03–30/11');
  });

  await test.step('E7-06 · Tab Thông tin đọc "01/12 – 28/02 năm sau, hằng năm"', async () => {
    await openTab(page, api, 'Thông tin');
    await expect(infoTab(page).getByText('01/12 – 28/02 năm sau, hằng năm')).toBeVisible();
  });

  await test.step('E7-07 · Tab Đủ điều kiện, năm giữa: khoảng ngày gắn đúng hai năm và có người tròn mốc tháng 02 năm sau', async () => {
    await openTab(page, api, 'Danh sách đủ điều kiện');
    await selectYear(page, year);

    await expect(page.locator('.hhd-eligibility__dates')).toHaveText(
      `${display(`${year}-12-01`)} – ${display(`${year + 1}-02-28`)}`,
    );

    const expectedNames = PEOPLE.filter((person) => {
      const date = anniversaryOf(person, year);
      return date >= `${year}-12-01` && date <= `${year + 1}-02-28`;
    }).map((person) => person.fullName);
    expect(expectedNames).toHaveLength(4);

    await expect(tableRows(page)).toHaveCount(expectedNames.length);
    const names = (await readTable(page)).map((row) => row[ELIGIBLE_COLUMN.fullName]);
    expect(names.slice().sort()).toEqual(expectedNames.slice().sort());
    expect(names).toContain('Cao Thị Hai');

    // Hai biên phải nằm trong danh sách, kèm đúng ngày tròn mốc đã gắn năm.
    const rows = await readTable(page);
    const opening = rows.find((row) => row[ELIGIBLE_COLUMN.fullName] === 'Lý Văn Biên')!;
    const closing = rows.find((row) => row[ELIGIBLE_COLUMN.fullName] === 'Mai Thị Cuối')!;
    expect(opening[ELIGIBLE_COLUMN.milestoneDate]).toBe(display(`${year}-12-01`));
    expect(closing[ELIGIBLE_COLUMN.milestoneDate]).toBe(display(`${year + 1}-02-28`));

    // Năm trước gom đúng người tròn mốc 20/01 của năm nay — cùng một lần diễn ra.
    await selectYear(page, year - 1);
    await expect(page.locator('.hhd-eligibility__dates')).toHaveText(
      `${display(`${year - 1}-12-01`)} – ${display(`${year}-02-28`)}`,
    );
    await expect(tableRows(page)).toHaveCount(1);
    expect((await readTable(page))[0][ELIGIBLE_COLUMN.fullName]).toBe('Bùi Văn Giêng');
  });

  await test.step('E7-08 · Người tròn mốc 20/01 không nằm ở "Chưa thuộc đợt nào", badge không tăng vì người đó', async () => {
    await openUncovered(page, year);
    await expect(tableRows(page)).toHaveCount(1);

    const rows = await readTable(page);
    const names = rows.map((row) => row[UNCOVERED_COLUMN.fullName]);
    expect(names).not.toContain('Bùi Văn Giêng');
    expect(names).toEqual(['Hoàng Thị Hè']);
    expect(rows[0][UNCOVERED_COLUMN.milestoneDate]).toBe(display(`${year}-06-15`));

    await expect(uncoveredBadge(page)).toHaveText('1');
    expect((await api.unassignedCount(year)).count).toBe(1);
  });

  await test.step('E7-09 · Xuất Excel từ tab Đủ điều kiện: tải về được, đủ dòng, tiêu đề ghi đúng khoảng ngày', async () => {
    await openTab(page, api, 'Danh sách đủ điều kiện');
    await selectYear(page, year);
    await expect(tableRows(page)).toHaveCount(4);

    const file = await downloadExcel(page, () =>
      page.getByRole('button', { name: 'Xuất Excel' }).click(),
    );

    expect(file.fileName).toBe(`DuDieuKien_DotGiaothua_${year}.xlsx`);
    expect(titleLines(file)).toEqual([
      UNIT_NAME,
      `${PERIOD} · ${display(`${year}-12-01`)} – ${display(`${year + 1}-02-28`)}`,
      `Ngày xuất: ${display(serverToday)}`,
    ]);

    const screen = (await readTable(page)).map((row) => row[ELIGIBLE_COLUMN.fullName]);
    expect(file.dataRows).toHaveLength(screen.length);
    expect(file.dataRows.map((row) => row[ELIGIBLE_COLUMN.fullName])).toEqual(screen);
  });

  await test.step('E7-10 · Sửa về 01/10 – 07/11 thì mọi dấu hiệu vắt năm biến mất và các con số đổi theo', async () => {
    await editPeriod(page, PLAIN);

    const row = (await readTable(page))[0];
    expect(row[PERIOD_COLUMN.fromDisplay]).toBe('01/10');
    expect(row[PERIOD_COLUMN.toDisplay]).toBe('07/11');
    expect(row[PERIOD_COLUMN.toDisplay]).not.toContain('năm sau');
    expect(row[PERIOD_COLUMN.eligibleCount]).toBe('0 người');

    await expect(page.locator('.hhd-coverage__bar')).toHaveCount(1);
    const banner = page.locator('.hhd-periods__banner');
    await expect(banner).toContainText('01/01–30/09');
    await expect(banner).toContainText('08/11–31/12');

    await openTab(page, api, 'Thông tin');
    await expect(infoTab(page).getByText('01/10 – 07/11 hằng năm')).toBeVisible();
    await expect(infoTab(page).getByText('năm sau, hằng năm')).toBeHidden();

    // Bốn người tròn mốc trong năm nay đều rơi ra ngoài đợt, kể cả người 20/01.
    await openUncovered(page, year);
    await expect(tableRows(page)).toHaveCount(4);
    expect((await readTable(page)).map((item) => item[UNCOVERED_COLUMN.fullName])).toContain(
      'Bùi Văn Giêng',
    );
    await expect(uncoveredBadge(page)).toHaveText('4');
  });

  await test.step('E7-11 · Sửa ngược lại 01/12 – 28/02 thì mọi con số trở về đúng trạng thái ban đầu', async () => {
    await editPeriod(page, SPANNING);

    const row = (await readTable(page))[0];
    expect(row[PERIOD_COLUMN.toDisplay]).toBe('28/02 năm sau');
    expect(row[PERIOD_COLUMN.eligibleCount]).toBe('4 người');

    await expect(page.locator('.hhd-coverage__bar')).toHaveCount(2);
    await expect(page.locator('.hhd-periods__banner')).toContainText('01/03–30/11');

    await openUncovered(page, year);
    await expect(tableRows(page)).toHaveCount(1);
    await expect(uncoveredBadge(page)).toHaveText('1');

    // Dashboard đọc đúng lần diễn ra neo ở năm nay và cùng số người với chi tiết đợt.
    await navItem(page, 'Dashboard').click();
    const card = page.locator('.hhd-dashboard__card').first();
    await expect(card.locator('.hhd-dashboard__period-name')).toHaveText(PERIOD);
    await expect(card.locator('.hhd-dashboard__count-value')).toHaveText('4');

    const dashboard = await api.dashboard();
    expect(dashboard.upcomingPeriod?.year).toBe(year);
    expect(dashboard.upcomingPeriod?.fromDate).toBe(`${year}-12-01`);
    expect(dashboard.upcomingPeriod?.toDate).toBe(`${year + 1}-02-28`);
    expect(dashboard.upcomingPeriod?.eligibleCount).toBe(4);
  });
});

/**
 * QC-T54-01 · Tiêu đề trang chi tiết đợt bỏ mất chữ "năm sau" của đợt vắt năm.
 *
 * Cùng một màn hình đang nói hai kiểu: dòng tiêu đề lớn đọc
 * "Đợt Giao thừa 01/12 – 28/02 hằng năm" — nghe như hai đầu nằm trong cùng một
 * năm — trong khi tab Thông tin ngay bên dưới đọc "01/12 – 28/02 năm sau,
 * hằng năm". Bảng đợt và tab Thông tin đều đã gắn chữ "năm sau" theo QT6, chỉ
 * `PeriodDetailHeader` còn sót.
 *
 * Đã sửa ở T55 (`PeriodDetailHeader.tsx` xét `spansNextYear`), nên ca này bỏ
 * `test.fail()` và trở thành ca canh hồi quy bình thường.
 */
test('QC-T54-01 · Tiêu đề trang chi tiết đợt phải nói rõ Đến ngày thuộc năm sau', async ({
  page,
  api,
}) => {
  await api.resetAll();
  await api.post('/AwardPeriods', { name: PERIOD, ...SPANNING });
  await signIn(page, api);

  await page.goto(`/dot-trao-huy-hieu/${await api.periodIdByName(PERIOD)}`);
  await expect(page.locator('.hhd-period-detail__range')).toContainText('năm sau');
});
