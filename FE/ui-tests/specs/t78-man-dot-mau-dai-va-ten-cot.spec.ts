import path from 'node:path';

import type { Locator, Page } from '@playwright/test';

import { expect, test } from '../fixtures/app';
import { TableView } from '../fixtures/table';

/**
 * T78 — ba sửa đổi giao diện của chủ dự án trên màn Đợt trao huy hiệu.
 *
 * 1. Dải "Độ phủ trong năm" trước đây tô NGƯỢC NGHĨA: đợt đã qua màu đỏ, đợt
 *    đang diễn ra lại vàng giống hệt đợt sắp tới. Giờ mỗi bar mang đúng màu của
 *    nhãn tương ứng ở cột Trạng thái, và chú giải liệt kê đủ ba trạng thái.
 * 2. Cột "Trạng thái <năm>" còn "Trạng thái", cột "Đủ điều kiện năm nay" còn
 *    "Đủ điều kiện" — con số vẫn là của năm hiện tại, chỉ bỏ chữ.
 * 3. Không còn dòng "Sắp theo Từ ngày…" dưới chân bảng.
 *
 * Dữ liệu giả dựng NGAY TRONG TỆP NÀY bằng `app.route` đăng ký sau fixture —
 * Playwright ưu tiên route đăng ký sau — để không chạm `fixtures/**` mà các bộ
 * ca khác đang dùng chung. Bộ đợt lấy đúng theo ảnh chủ dự án gửi, thêm một đợt
 * vắt qua 31/12 vì đó là trường hợp duy nhất một đợt cho HAI bar trong một năm.
 */

/** Nơi để ảnh chụp bàn giao. */
const SHOT_DIR = path.join(import.meta.dirname, '..', '.artifacts', 'anh');

/** Thẻ trắng bao bảng đợt. */
const PANEL = '.hhd-periods__panel';

/** Ô "Tên đợt", "Trạng thái" và "Đủ điều kiện" — đếm từ 0, sau cột STT. */
const NAME_COLUMN = 1;
const STATUS_COLUMN = 4;
const ELIGIBLE_COLUMN = 5;

/** Năm của bộ dữ liệu giả trong tệp này. */
const YEAR = 2026;

/** Ngày hôm nay trong ảnh chủ dự án gửi. */
const TODAY_IN_SHOT = '2026-09-30';

/** Vỏ `{ message, data }` của mọi phản hồi thành công (mục 1.3 hợp đồng API). */
function envelope(data: unknown) {
  return {
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify({ message: null, data }),
  };
}

function pad(value: number): string {
  return String(value).padStart(2, '0');
}

/** Số ngày giữa hai ngày ISO — đếm theo UTC nên không lệch vì múi giờ. */
function daysBetween(fromIso: string, toIso: string): number {
  const millis = Date.parse(`${toIso}T00:00:00Z`) - Date.parse(`${fromIso}T00:00:00Z`);
  return Math.round(millis / 86_400_000);
}

/** Một đợt ở dạng chủ dự án đọc: chỉ ngày/tháng, đúng QT6. */
interface PeriodSpec {
  id: string;
  from: readonly [day: number, month: number];
  to: readonly [day: number, month: number];
}

/**
 * Bốn đợt trong ảnh chủ dự án. Đợt cuối vắt qua 31/12 nên Đến ngày thuộc năm
 * sau, và máy chủ cắt nó thành hai đoạn trong năm đang vẽ.
 */
const PERIOD_SPECS: readonly PeriodSpec[] = [
  { id: 't78-dot-01-03', from: [1, 3], to: [31, 5] },
  { id: 't78-dot-01-06', from: [1, 6], to: [30, 9] },
  { id: 't78-dot-01-10', from: [1, 10], to: [30, 11] },
  { id: 't78-dot-01-12', from: [1, 12], to: [28, 2] },
];

function nameOf(spec: PeriodSpec): string {
  return `Đợt ${pad(spec.from[0])}/${pad(spec.from[1])}`;
}

/** Đợt có Đến ngày trước Từ ngày trong cùng năm thì vắt qua 31/12 (QT6). */
function spansNextYear(spec: PeriodSpec): boolean {
  return spec.to[1] * 100 + spec.to[0] < spec.from[1] * 100 + spec.from[0];
}

/**
 * Lần diễn ra neo vào `YEAR` của một đợt: Từ ngày luôn trong `YEAR`, Đến ngày
 * sang năm sau nếu đợt vắt 31/12.
 */
function occurrenceOf(spec: PeriodSpec) {
  return {
    fromDate: `${YEAR}-${pad(spec.from[1])}-${pad(spec.from[0])}`,
    toDate: `${YEAR + (spansNextYear(spec) ? 1 : 0)}-${pad(spec.to[1])}-${pad(spec.to[0])}`,
  };
}

/**
 * Trạng thái và số ngày còn lại của đợt theo QT11 — xét đúng lần diễn ra neo
 * năm nay, y như máy chủ.
 */
function statusOf(spec: PeriodSpec, today: string) {
  const { fromDate, toDate } = occurrenceOf(spec);
  if (toDate < today) return { status: 'Past' as const, daysRemaining: null };
  if (fromDate > today) {
    return { status: 'Upcoming' as const, daysRemaining: daysBetween(today, fromDate) };
  }
  return { status: 'Ongoing' as const, daysRemaining: null };
}

/** Một đợt ở dạng đầy đủ mà bảng và dải độ phủ cần. */
function periodRowOf(spec: PeriodSpec, today: string) {
  return {
    id: spec.id,
    name: nameOf(spec),
    fromDay: spec.from[0],
    fromMonth: spec.from[1],
    toDay: spec.to[0],
    toMonth: spec.to[1],
    fromDisplay: `${pad(spec.from[0])}/${pad(spec.from[1])}`,
    toDisplay: `${pad(spec.to[0])}/${pad(spec.to[1])}`,
    year: YEAR,
    ...occurrenceOf(spec),
    spansNextYear: spansNextYear(spec),
    ...statusOf(spec, today),
    // Số khác nhau đôi một để bấm sắp xếp cột "Đủ điều kiện" thấy thứ tự đổi thật.
    eligibleCount: spec.from[1] * 7 + 3,
  };
}

/**
 * Các đoạn của dải, cắt như máy chủ: đợt thường một đoạn, đợt vắt 31/12 thành
 * đuôi ở đầu năm và đầu ở cuối năm. Sắp theo ngày bắt đầu, tức đúng thứ tự từ
 * trái sang phải trên dải.
 */
function segmentsOf(specs: readonly PeriodSpec[]) {
  const segments = specs.flatMap((spec) => {
    const base = { type: 'Period' as const, periodId: spec.id, name: nameOf(spec) };
    const from = `${YEAR}-${pad(spec.from[1])}-${pad(spec.from[0])}`;
    const to = `${YEAR}-${pad(spec.to[1])}-${pad(spec.to[0])}`;
    return spansNextYear(spec)
      ? [
          { ...base, fromDate: `${YEAR}-01-01`, toDate: to },
          { ...base, fromDate: from, toDate: `${YEAR}-12-31` },
        ]
      : [{ ...base, fromDate: from, toDate: to }];
  });
  return segments.sort((a, b) => a.fromDate.localeCompare(b.fromDate));
}

/** Năm đoạn đợt: bốn đợt, trong đó đợt vắt năm góp hai đoạn. */
const SEGMENTS = segmentsOf(PERIOD_SPECS);

/** Một cặp chồng lấn giả, chỉ để kiểm chú giải có thêm mục "Chồng lấn". */
function overlapBetween(first: PeriodSpec, second: PeriodSpec) {
  const day = `${pad(first.to[0])}/${pad(first.to[1])}`;
  return {
    firstPeriodId: first.id,
    firstPeriodName: nameOf(first),
    secondPeriodId: second.id,
    secondPeriodName: nameOf(second),
    fromDate: occurrenceOf(first).toDate,
    toDate: occurrenceOf(first).toDate,
    fromDisplay: day,
    toDisplay: day,
  };
}

interface Scenario {
  today: string;
  overlaps?: ReturnType<typeof overlapBetween>[];
}

/** Mở màn Đợt với bộ dữ liệu giả của tệp này, chờ dải và bảng dựng xong. */
async function openPeriods(app: Page, scenario: Scenario): Promise<TableView> {
  const periods = PERIOD_SPECS.map((spec) => periodRowOf(spec, scenario.today));
  const overlaps = scenario.overlaps ?? [];

  // Thanh trên cùng cũng in ngày máy chủ; để ảnh bàn giao không có hai ngày
  // khác nhau trên cùng một màn, kịch bản nào thì cả hai chỗ cùng ngày đó.
  await app.route('**/api/Auth/Me', (route) =>
    route.fulfill(
      envelope({
        username: 'admin',
        displayName: 'Cán bộ văn phòng',
        unitName: 'Đảng ủy Phường Kiểm Thử',
        serverDate: scenario.today,
        expiresAt: `${YEAR}-12-31T00:00:00Z`,
      }),
    ),
  );

  await app.route('**/api/AwardPeriods', (route) =>
    route.fulfill(
      envelope({
        year: YEAR,
        today: scenario.today,
        totalCount: periods.length,
        periods,
        warnings: { overlaps, gaps: [] },
        coverage: { segments: SEGMENTS },
      }),
    ),
  );

  await app.goto('/dot-trao-huy-hieu');
  const table = new TableView(app, app.locator(PANEL));
  await expect(table.rows).toHaveCount(periods.length);
  await expect(app.locator('.hhd-coverage__bar')).toHaveCount(SEGMENTS.length);
  // Hover mặc định của Ant Design đổi nền dòng và mở tooltip của bar.
  await app.mouse.move(0, 0);
  return table;
}

/** Ba loại màu bar, đúng ba nhãn của cột Trạng thái. */
const TONES = ['past', 'ongoing', 'upcoming'] as const;
type Tone = (typeof TONES)[number];

/** Nhãn ở cột Trạng thái nói lên bar của đợt đó phải mang màu nào. */
function toneOfLabel(label: string): Tone {
  if (label === 'Đã qua') return 'past';
  if (label === 'Đang diễn ra') return 'ongoing';
  expect(label, 'nhãn trạng thái chỉ có ba dạng').toContain('Sắp tới');
  return 'upcoming';
}

/** Màu của từng bar, đọc từ trái sang phải trên dải. */
function tonesLeftToRight(page: Page): Promise<string[]> {
  return page.locator('.hhd-coverage__bar').evaluateAll((nodes) =>
    nodes
      .map((node) => ({
        left: node.getBoundingClientRect().left,
        tone:
          ['past', 'ongoing', 'upcoming'].find((name) =>
            node.classList.contains(`hhd-coverage__bar--${name}`),
          ) ?? 'không-rõ',
      }))
      .sort((a, b) => a.left - b.left)
      .map((item) => item.tone),
  );
}

/** Màu thật của một phần tử sau khi tính hết các luật CSS. */
function paintOf(target: Locator) {
  return target.evaluate((element) => {
    const style = window.getComputedStyle(element);
    return {
      backgroundColor: style.backgroundColor,
      backgroundImage: style.backgroundImage,
      color: style.color,
      borderColor: style.borderTopColor,
      borderWidth: style.borderTopWidth,
    };
  });
}

/** Bar mang tên đợt `name` — đợt không vắt năm chỉ có đúng một bar như vậy. */
function barNamed(page: Page, name: string): Locator {
  return page.locator('.hhd-coverage__bar').filter({ hasText: name });
}

test.describe('Tiêu đề hai cột bỏ hết chữ về năm', () => {
  test('bảy tiêu đề đúng chữ, không ô nào nhắc năm', async ({ app }) => {
    const table = await openPeriods(app, { today: TODAY_IN_SHOT });

    const headers = app.locator(`${PANEL} .ant-table-thead th`);
    await expect(headers).toHaveText([
      'STT',
      'Tên đợt',
      'Từ ngày',
      'Đến ngày',
      'Trạng thái',
      'Đủ điều kiện',
      'Thao tác',
    ]);

    for (const text of await headers.allInnerTexts()) {
      expect(text, 'tiêu đề cột không được nhắc năm').not.toMatch(/\d{4}|năm nay/);
    }

    // Bỏ chữ ở tiêu đề nhưng vẫn sắp xếp được như cũ.
    await table.clickSort('Trạng thái');
    expect(await table.columnTexts(STATUS_COLUMN)).toEqual([
      'Đã qua',
      'Đang diễn ra',
      'Sắp tới · 1 ngày',
      'Sắp tới · 62 ngày',
    ]);

    await table.clickSort('Đủ điều kiện');
    const counts = (await table.columnTexts(ELIGIBLE_COLUMN)).map((text) =>
      Number(text.replace(/\D/g, '')),
    );
    expect(counts, 'bấm tiêu đề "Đủ điều kiện" phải sắp được').toEqual(
      [...counts].sort((a, b) => a - b),
    );
  });
});

test.describe('Chân bảng không còn dòng chú thích', () => {
  test('không có khối chú thích, không còn câu nào của nó', async ({ app }) => {
    await openPeriods(app, { today: TODAY_IN_SHOT });

    await expect(app.locator('.hhd-periods__footnote')).toHaveCount(0);
    const pageText = await app.locator('body').innerText();
    expect(pageText).not.toContain('Sắp theo Từ ngày');
    expect(pageText).not.toContain('hiệu lực ngay cho mọi năm');
  });
});

test.describe('Dải độ phủ tô theo trạng thái', () => {
  test('năm bar đúng màu, khớp nhãn ở cột Trạng thái', async ({ app }) => {
    const table = await openPeriods(app, { today: TODAY_IN_SHOT });

    // Hôm nay 30/09 đúng bằng Đến ngày của đợt 01/06 – 30/09: ca biên, bar đó
    // vẫn phải là "đang diễn ra".
    expect(await tonesLeftToRight(app)).toEqual([
      'past',
      'past',
      'ongoing',
      'upcoming',
      'upcoming',
    ]);

    // Ba đợt không vắt năm: một đoạn nên màu bar luôn trùng nhãn trong bảng.
    const names = await table.columnTexts(NAME_COLUMN);
    const labels = await table.columnTexts(STATUS_COLUMN);
    for (const spec of PERIOD_SPECS.filter((item) => !spansNextYear(item))) {
      const row = names.indexOf(nameOf(spec));
      expect(row, `phải có dòng "${nameOf(spec)}"`).toBeGreaterThanOrEqual(0);
      const bar = barNamed(app, nameOf(spec));
      await expect(bar).toHaveCount(1);
      await expect(bar).toHaveClass(
        new RegExp(`hhd-coverage__bar--${toneOfLabel(labels[row])}(\\s|$)`),
      );
    }

    // Class gốc phải còn trên mọi bar: bài e2e `e7` đếm bar bằng class này.
    await expect(app.locator('.hhd-coverage__bar')).toHaveCount(SEGMENTS.length);
    await expect(app.locator('.hhd-coverage__bar--next')).toHaveCount(0);
  });

  test('ba loại bar đúng bộ màu của cột Trạng thái', async ({ app }) => {
    await openPeriods(app, { today: TODAY_IN_SHOT });

    const past = await paintOf(app.locator('.hhd-coverage__bar--past').first());
    expect(past.backgroundColor).toBe('rgb(236, 237, 240)');
    expect(past.color).toBe('rgb(78, 81, 88)');
    expect(past.borderColor).toBe('rgb(154, 160, 168)');
    expect(past.borderWidth).toBe('1px');

    const ongoing = await paintOf(app.locator('.hhd-coverage__bar--ongoing').first());
    expect(ongoing.backgroundImage).toContain('rgb(153, 26, 20)');
    expect(ongoing.color).toBe('rgb(255, 255, 255)');

    const upcoming = await paintOf(app.locator('.hhd-coverage__bar--upcoming').first());
    expect(upcoming.backgroundImage).toContain('rgb(255, 205, 0)');
    expect(upcoming.color).toBe('rgb(61, 44, 0)');
  });

  test('đổi hôm nay thì màu bar đổi theo', async ({ app }) => {
    // Qua đúng một ngày: đợt 01/06 – 30/09 hết, đợt 01/10 – 30/11 bắt đầu.
    await openPeriods(app, { today: '2026-10-01' });
    expect(await tonesLeftToRight(app)).toEqual(['past', 'past', 'past', 'ongoing', 'upcoming']);
  });

  test('hai đoạn của đợt vắt năm tô theo khoảng ngày riêng', async ({ app }) => {
    // Giữa tháng 1: đoạn đuôi 01/01 – 28/02 đang chạy thật, đoạn đầu 01/12 –
    // 31/12 còn ở phía trước.
    await openPeriods(app, { today: '2026-01-15' });
    expect(await tonesLeftToRight(app)).toEqual([
      'ongoing',
      'upcoming',
      'upcoming',
      'upcoming',
      'upcoming',
    ]);
  });

  test('giữa tháng 12 thì đoạn đầu năm đã qua, đoạn cuối năm đang chạy', async ({ app }) => {
    await openPeriods(app, { today: '2026-12-15' });
    expect(await tonesLeftToRight(app)).toEqual(['past', 'past', 'past', 'past', 'ongoing']);
  });
});

test.describe('Chú giải của dải độ phủ', () => {
  const LEGEND = '.hhd-coverage__legend > span';

  test('không chồng lấn: năm mục, không còn mục "Đợt"', async ({ app }) => {
    await openPeriods(app, { today: TODAY_IN_SHOT });

    await expect(app.locator(LEGEND)).toHaveText([
      'Đã qua',
      'Đang diễn ra',
      'Sắp tới',
      'Khoảng trống',
      'Hôm nay',
    ]);
  });

  test('có chồng lấn: mục "Chồng lấn" chen vào trước "Hôm nay"', async ({ app }) => {
    await openPeriods(app, {
      today: TODAY_IN_SHOT,
      overlaps: [overlapBetween(PERIOD_SPECS[1], PERIOD_SPECS[2])],
    });

    await expect(app.locator(LEGEND)).toHaveText([
      'Đã qua',
      'Đang diễn ra',
      'Sắp tới',
      'Khoảng trống',
      'Chồng lấn',
      'Hôm nay',
    ]);
  });

  test('ô màu ba mục đầu trùng màu ba loại bar', async ({ app }) => {
    await openPeriods(app, { today: TODAY_IN_SHOT });

    for (const tone of TONES) {
      const swatch = await paintOf(app.locator(`.hhd-coverage__swatch--${tone}`));
      const bar = await paintOf(app.locator(`.hhd-coverage__bar--${tone}`).first());
      expect(swatch.backgroundColor, `ô "${tone}"`).toBe(bar.backgroundColor);
      expect(swatch.backgroundImage, `ô "${tone}"`).toBe(bar.backgroundImage);
    }

    await expect(app.locator('.hhd-coverage__swatch--period')).toHaveCount(0);
  });

  for (const { width, height } of [
    { width: 1440, height: 900 },
    { width: 1280, height: 600 },
  ] as const) {
    test(`sáu mục đứng trên một hàng ở ${width}x${height}`, async ({ app }) => {
      await app.setViewportSize({ width, height });
      await openPeriods(app, {
        today: TODAY_IN_SHOT,
        overlaps: [overlapBetween(PERIOD_SPECS[1], PERIOD_SPECS[2])],
      });

      const tops = await app
        .locator(LEGEND)
        .evaluateAll((nodes) => nodes.map((node) => node.getBoundingClientRect().top));
      expect(tops).toHaveLength(6);
      expect(new Set(tops.map((top) => Math.round(top))).size, 'chú giải phải trên một hàng').toBe(
        1,
      );
    });
  }
});

test.describe('Ảnh bàn giao', () => {
  for (const { width, height } of [
    { width: 1440, height: 900 },
    { width: 1280, height: 600 },
  ] as const) {
    test(`chụp màn Đợt hôm nay 30/09 ở ${width}x${height}`, async ({ app }) => {
      await app.setViewportSize({ width, height });
      await openPeriods(app, { today: TODAY_IN_SHOT });
      await app.screenshot({ path: path.join(SHOT_DIR, `t78-man-dot-${width}x${height}.png`) });
    });
  }

  test('chụp riêng dải độ phủ hôm nay 15/01', async ({ app }) => {
    await openPeriods(app, { today: '2026-01-15' });
    await app
      .locator('.hhd-coverage')
      .screenshot({ path: path.join(SHOT_DIR, 't78-dai-do-phu-15-01.png') });
  });
});
