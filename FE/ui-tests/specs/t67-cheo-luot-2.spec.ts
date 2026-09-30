import fs from 'node:fs';
import path from 'node:path';

import type { Locator, Page } from '@playwright/test';

import { DETAIL_PERIOD, expect, mockData, test } from '../fixtures/app';
import { SERVER_TODAY, SERVER_YEAR } from '../fixtures/data';

/**
 * T67 — nghiệm thu bản GỘP ba PR của lượt 2 (T63, T64, T66).
 *
 * Ba PR cố ý chia tệp, nhưng lượt này có hai NGOẠI LỆ CÓ KIỂM SOÁT: ba tệp bị
 * hai việc cùng sửa. `PeriodsPage.tsx` do T63 và T64 cùng chạm;
 * `EligibilityTab.tsx` và `UncoveredPage.tsx` do T63 và T66 cùng chạm. Ở cả ba
 * chỗ, phần T63 sửa là CÂU GỢI Ý khi bảng trống — thứ chỉ hiện ra khi tìm không
 * thấy gì hoặc khi năm đó không ai tròn mốc. Bộ ca của T63 quét mã tĩnh nên
 * không bao giờ dựng những câu đó lên màn hình, còn bộ ca của T64 và T66 không
 * biết câu đó phải đọc ra sao. Nói cách khác: nếu phép gộp của git lấy bản cũ
 * của một câu, cả ba bộ ca kia vẫn xanh.
 *
 * Tệp này kiểm đúng chỗ ba việc gặp nhau:
 *
 * 1. T63 x T66 và T63 x T64 — DỰNG THẬT ba câu gợi ý ấy lên màn hình và đọc
 *    từng chữ, cộng thêm một lượt quét tĩnh `FE/src` bằng thước đo độc lập với
 *    thước của T63 (quét cả `.css`, và chặn thêm cả dạng có dấu cách lẫn dấu
 *    ngoặc quanh từ).
 * 2. T66 x T66 — Chi tiết đợt và Chưa thuộc đợt nào phải dùng CÙNG MỘT bộ chọn
 *    năm. Bộ ca của T66 kiểm hành vi trên từng màn một; ở đây so thẳng hình
 *    dáng thật của hai bộ chọn với nhau, và đối chiếu với kiểu B mà chủ dự án
 *    đã duyệt.
 * 3. Giới hạn năm máy chủ ± 100 — kiểm CẢ HAI phía, CẢ HAI cơ chế (`‹ ›` và
 *    bảng chọn), trên CẢ HAI màn. T66 chỉ kiểm đủ bốn góc ở màn Chi tiết đợt.
 * 4. T64 — màn Đợt không còn dòng tóm tắt, và KHÔNG ô nào còn nền vàng
 *    `--hhd-gold-soft`. Đo theo mã màu thật thay vì so ba dòng với nhau: nếu
 *    một lần gộp sau này tô vàng CẢ BẢNG thì phép so của T64 vẫn xanh.
 *
 * Mọi con số suy từ năm máy chủ giả `SERVER_YEAR`, không viết cứng theo năm
 * chạy thật.
 */

/** Nơi để ảnh soi tay bàn giao cho chủ dự án. */
const SHOT_DIR = path.join(import.meta.dirname, '..', '.artifacts', 'anh-t67');

/** Đi xa nhất 100 năm mỗi phía — khớp `REACH_YEARS` trong `YearPicker.tsx`. */
const REACH_YEARS = 100;
const MIN_YEAR = SERVER_YEAR - REACH_YEARS;
const MAX_YEAR = SERVER_YEAR + REACH_YEARS;

const DETAIL_URL = `/dot-trao-huy-hieu/${DETAIL_PERIOD.id}`;
const UNCOVERED_URL = '/chua-thuoc-dot-nao';
const PERIODS_URL = '/dot-trao-huy-hieu';

/** Hai màn dùng chung bộ chọn năm — mọi ca so sánh chạy trên cả hai. */
const YEAR_PICKER_SCREENS = [
  { label: 'Chi tiết đợt', url: DETAIL_URL, endpoint: '/Eligibility' },
  { label: 'Chưa thuộc đợt nào', url: UNCOVERED_URL, endpoint: '/Eligibility/Unassigned' },
] as const;

/* ------------------------------------------------------------------ */
/* 1. Xưng hô: quét tĩnh và dựng thật câu gợi ý lên màn hình           */
/* ------------------------------------------------------------------ */

const SRC_DIR = path.join(import.meta.dirname, '..', '..', 'src');

/**
 * Chữ cái tiếng Việt, dùng dựng ranh giới từ. `\b` của JavaScript chỉ biết chữ
 * ASCII nên "bác" sẽ khớp cả trong "cấp bách"; chặn hai đầu bằng lớp chữ này
 * thì "bách", "bàn", "bạc" không bị bắt oan.
 */
const LETTER = 'A-Za-zÀ-ÖØ-öø-ÿĂăĐđĨĩŨũƠơƯưẠ-ỹ';

/** Đại từ xưng hô bị cấm, và hai cụm rào đón mà quyết định 30/09 bỏ luôn. */
const FORBIDDEN = ['bác', 'bạn', 'anh chị', 'quý vị', 'vui lòng', 'xin mời'];

/**
 * Ba tệp của hai ngoại lệ có kiểm soát — chỗ duy nhất phép gộp của git có thể
 * âm thầm lấy lại bản cũ.
 */
const SHARED_FILES = [
  'pages/periods/PeriodsPage.tsx',
  'pages/periods/detail/EligibilityTab.tsx',
  'pages/uncovered/UncoveredPage.tsx',
];

/** Mọi tệp mã và tệp kiểu dáng dưới `FE/src`, kể cả thư mục con. */
function sourceFiles(dir: string): string[] {
  return fs.readdirSync(dir, { withFileTypes: true }).flatMap((entry) => {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) return sourceFiles(full);
    return /\.(tsx?|css)$/.test(entry.name) ? [full] : [];
  });
}

/** Những dòng có xưng hô trong một tệp, kèm số dòng và từ bắt được. */
function offendersIn(file: string): string[] {
  const lines = fs.readFileSync(file, 'utf8').split('\n');
  const found: string[] = [];
  for (const word of FORBIDDEN) {
    const pattern = new RegExp(`(?<![${LETTER}])${word}(?![${LETTER}])`, 'i');
    lines.forEach((line, index) => {
      if (pattern.test(line)) {
        found.push(`${path.relative(SRC_DIR, file)}:${index + 1} — "${word}"`);
      }
    });
  }
  return found;
}

test.describe('T63 x T64 x T66 · bản gộp không còn xưng hô', () => {
  test('quét tĩnh FE/src sau khi gộp, gồm cả tệp .css', () => {
    const files = sourceFiles(SRC_DIR);
    expect(files.length, 'phải quét được mã nguồn trong FE/src').toBeGreaterThan(20);

    const offenders = files.flatMap(offendersIn);
    expect(
      offenders,
      'bản gộp không được có "bác", "bạn", "anh chị", "quý vị", "vui lòng", "xin mời"',
    ).toEqual([]);
  });

  test('ba tệp của hai ngoại lệ có kiểm soát vẫn sạch sau khi gộp', () => {
    for (const relative of SHARED_FILES) {
      const file = path.join(SRC_DIR, relative);
      expect(fs.existsSync(file), `${relative} phải còn trong bản gộp`).toBe(true);
      expect(offendersIn(file), `${relative} — phép gộp đã lấy lại bản có xưng hô`).toEqual([]);
    }
  });
});

/** Vỏ `{ message, data }` của mọi phản hồi thành công (mục 1.3 hợp đồng API). */
function envelope(data: unknown) {
  return {
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify({ message: null, data }),
  };
}

/** Câu gợi ý dưới câu chính của bảng trống — khối `hint` của `TableEmpty`. */
function emptyHint(app: Page): Locator {
  return app.locator('.ant-empty-description > div > div').nth(1);
}

/** Câu chính của bảng trống. */
function emptyDescription(app: Page): Locator {
  return app.locator('.ant-empty-description > div > div').first();
}

/** Chữ tìm không khớp bất kỳ họ tên hay tên đợt nào trong bộ dữ liệu giả. */
const NO_MATCH_KEYWORD = 'zzz-không-có-ai-tên-thế-này';

/** Bảng trống mà còn xưng hô thì cả câu chính lẫn câu gợi ý đều phải bị bắt. */
async function expectEmptyStateClean(app: Page): Promise<void> {
  const text = await app.locator('.ant-empty-description').innerText();
  for (const word of FORBIDDEN) {
    const pattern = new RegExp(`(?<![${LETTER}])${word}(?![${LETTER}])`, 'i');
    expect(pattern.test(text), `câu của bảng trống còn "${word}": ${text}`).toBe(false);
  }
}

test.describe('T63 x T66 · câu gợi ý trong hai tệp dùng chung, dựng thật lên màn hình', () => {
  test('Chi tiết đợt — tìm không thấy gì', async ({ app }) => {
    await app.goto(DETAIL_URL);
    await expect(app.locator('.hhd-eligibility .ant-table-row').first()).toBeVisible();

    await app.locator('.hhd-table-filters__search input').fill(NO_MATCH_KEYWORD);

    await expect(emptyDescription(app)).toHaveText('Không tìm thấy đảng viên nào khớp.');
    await expect(emptyHint(app)).toHaveText(
      'Thử xóa bớt chữ trong ô tìm, hoặc chọn lại Mốc: Tất cả.',
    );
    await expectEmptyStateClean(app);
  });

  test('Chi tiết đợt — năm đó không ai tròn mốc', async ({ app }) => {
    await app.route('**/api/Eligibility?*', (route) => {
      const year = Number(new URL(route.request().url()).searchParams.get('year'));
      return route.fulfill(
        envelope({
          awardPeriod: { ...DETAIL_PERIOD, year },
          year,
          totalCount: 0,
          milestoneBreakdown: [],
          members: [],
        }),
      );
    });

    await app.goto(DETAIL_URL);

    await expect(emptyDescription(app)).toHaveText(
      `Không có đảng viên nào tròn mốc trong đợt này năm ${SERVER_YEAR}.`,
    );
    await expect(emptyHint(app)).toHaveText(
      'Thử chọn năm khác ở trên, hoặc xem mục “Chưa thuộc đợt nào” để biết ai đang bị sót.',
    );
    await expectEmptyStateClean(app);
  });

  test('Chưa thuộc đợt nào — tìm không thấy gì', async ({ app }) => {
    await app.goto(UNCOVERED_URL);
    await expect(app.locator('.hhd-uncovered .ant-table-row').first()).toBeVisible();

    await app.locator('.hhd-table-filters__search input').fill(NO_MATCH_KEYWORD);

    await expect(emptyDescription(app)).toHaveText('Không tìm thấy đảng viên nào khớp.');
    await expect(emptyHint(app)).toHaveText(
      'Thử xóa bớt chữ trong ô tìm, hoặc chọn lại Mốc: Tất cả.',
    );
    await expectEmptyStateClean(app);
  });
});

test.describe('T63 x T64 · câu gợi ý của màn Đợt, dựng thật lên màn hình', () => {
  test('tìm không thấy đợt nào', async ({ app }) => {
    await app.goto(PERIODS_URL);
    await expect(app.locator('.hhd-periods__panel .ant-table-row').first()).toBeVisible();

    await app.locator('.hhd-table-filters__search input').fill(NO_MATCH_KEYWORD);

    await expect(emptyDescription(app)).toHaveText('Không tìm thấy đợt nào khớp.');
    await expect(emptyHint(app)).toHaveText('Thử xóa bớt chữ trong ô tìm.');
    await expectEmptyStateClean(app);
  });

  test('chưa cài đợt nào — vẫn không có dòng tóm tắt dưới tiêu đề', async ({ app }) => {
    await app.route('**/api/AwardPeriods', (route) =>
      route.fulfill(
        envelope({
          year: SERVER_YEAR,
          today: SERVER_TODAY,
          totalCount: 0,
          periods: [],
          warnings: { overlaps: [], gaps: [] },
          coverage: { segments: [] },
        }),
      ),
    );

    await app.goto(PERIODS_URL);

    await expect(emptyDescription(app)).toHaveText('Chưa có đợt trao huy hiệu nào');
    await expect(app.locator('.hhd-page-heading__description')).toHaveCount(0);
    await expectEmptyStateClean(app);
  });
});

/* ------------------------------------------------------------------ */
/* 2. Hai màn dùng CÙNG MỘT bộ chọn năm, đúng kiểu B                   */
/* ------------------------------------------------------------------ */

function yearCell(app: Page): Locator {
  return app.getByRole('button', { name: /^Chọn năm khác, đang xem/ });
}

function backButton(app: Page): Locator {
  return app.getByRole('button', { name: 'Lùi một năm' });
}

function forwardButton(app: Page): Locator {
  return app.getByRole('button', { name: 'Tiến một năm' });
}

function panel(app: Page): Locator {
  return app.locator('.hhd-year-panel');
}

/** Một ô năm trong bảng chọn — khớp đúng con số, không khớp một phần. */
function panelYear(app: Page, year: number): Locator {
  return panel(app).getByRole('button', { name: String(year), exact: true });
}

async function openPanel(app: Page): Promise<void> {
  await yearCell(app).click();
  await expect(panel(app)).toBeVisible();
}

/** Chờ chữ trong ô năm đổi — danh sách năm mới đã dựng xong. */
async function expectYear(app: Page, year: number): Promise<void> {
  await expect(yearCell(app)).toHaveText(String(year));
}

/**
 * Hình dáng thật của bộ chọn năm, lấy từ trình duyệt. Số năm trong `aria-label`
 * bị thay bằng chữ `NĂM` để hai màn so được với nhau, còn lại giữ nguyên: lớp
 * CSS, màu nền, bóng, kiểu chữ và số đo. Hai màn ra cùng một kết quả nghĩa là
 * chúng thật sự dùng chung một component, không phải hai bản dựng giống nhau.
 */
function yearPickerShape(app: Page) {
  return app.evaluate(() => {
    const roots = document.querySelectorAll('.hhd-year-picker');
    const root = roots[0] as HTMLElement;
    const box = (element: Element) => {
      const rect = element.getBoundingClientRect();
      return { width: Math.round(rect.width), height: Math.round(rect.height) };
    };
    const rootStyle = window.getComputedStyle(root);

    return {
      count: roots.length,
      role: root.getAttribute('role'),
      ariaLabel: root.getAttribute('aria-label'),
      className: root.className,
      background: rootStyle.backgroundColor,
      borderRadius: rootStyle.borderRadius,
      ...box(root),
      buttons: [...root.querySelectorAll('button')].map((button) => {
        const style = window.getComputedStyle(button);
        return {
          className: button.className,
          // Năm hiện trên màn khác nhau giữa hai lần đo thì vẫn phải so được.
          ariaLabel: button.getAttribute('aria-label')!.replace(/\d{4}/, 'NĂM'),
          ariaHasPopup: button.getAttribute('aria-haspopup'),
          background: style.backgroundColor,
          backgroundImage: style.backgroundImage,
          color: style.color,
          fontFamily: style.fontFamily,
          fontWeight: style.fontWeight,
          borderTopWidth: style.borderTopWidth,
          boxShadow: style.boxShadow,
          ...box(button),
        };
      }),
    };
  });
}

/**
 * Hình dáng bộ chọn năm ĐÃ ĐỨNG YÊN.
 *
 * `YearPicker.css` cho mũi tên `‹ ›` một hiệu ứng `transition: color 0.15s`.
 * Ngay sau khi lớp kiểu dáng được nạp, màu chữ đang trên đường đi từ màu mặc
 * định của trình duyệt về `--hhd-text-secondary`, nên đọc `getComputedStyle`
 * một lần sẽ bắt được màu giữa đường — mỗi lần chạy một con số khác. Đọc lại
 * tới khi hai lần liền nhau giống hệt thì mới là màu thật.
 */
async function settled<T>(app: Page, read: () => Promise<T>, what: string): Promise<T> {
  let previous = JSON.stringify(await read());
  for (let attempt = 0; attempt < 60; attempt += 1) {
    await app.waitForFunction(
      () => new Promise((resolve) => window.requestAnimationFrame(() => resolve(true))),
    );
    const current = await read();
    const serialized = JSON.stringify(current);
    if (serialized === previous) return current;
    previous = serialized;
  }
  throw new Error(`${what} không đứng yên sau 60 lần đo`);
}

function settledYearPickerShape(app: Page) {
  return settled(app, () => yearPickerShape(app), 'bộ chọn năm');
}

/** Hình dáng thật của bảng chọn năm đang mở. */
function yearPanelShape(app: Page) {
  return app.evaluate(() => {
    const root = document.querySelector('.hhd-year-panel') as HTMLElement;
    const grid = root.querySelector('.hhd-year-panel__grid') as HTMLElement;
    const today = root.querySelector('.hhd-year-panel__today') as HTMLElement;
    const range = root.querySelector('.hhd-year-panel__range') as HTMLElement;
    const style = window.getComputedStyle(root);

    return {
      role: root.getAttribute('role'),
      ariaLabel: root.getAttribute('aria-label'),
      width: Math.round(root.getBoundingClientRect().width),
      padding: style.padding,
      // Nút "Năm nay" phải ở GÓC TRÁI đầu bảng, câu khoảng chọn ở bên phải.
      todayText: today.textContent,
      todayLeftOfRange:
        today.getBoundingClientRect().right <= range.getBoundingClientRect().left + 1,
      rangeText: range.textContent,
      columns: window.getComputedStyle(grid).gridTemplateColumns.split(' ').length,
      gridRole: grid.getAttribute('role'),
    };
  });
}

test.describe('T66 · Chi tiết đợt và Chưa thuộc đợt nào dùng cùng một bộ chọn năm', () => {
  test('hình dáng bộ chọn năm giống nhau từng chi tiết trên hai màn', async ({ app }) => {
    await app.goto(DETAIL_URL);
    await expectYear(app, SERVER_YEAR);
    const detail = await settledYearPickerShape(app);

    await app.goto(UNCOVERED_URL);
    await expectYear(app, SERVER_YEAR);
    const uncovered = await settledYearPickerShape(app);

    expect(detail.count, 'mỗi màn chỉ được có MỘT bộ chọn năm').toBe(1);
    expect(uncovered, 'hai màn phải dùng chung một bộ chọn năm').toEqual(detail);
  });

  test('bảng chọn năm giống nhau trên hai màn', async ({ app }) => {
    await app.goto(DETAIL_URL);
    await openPanel(app);
    const detail = await yearPanelShape(app);

    await app.goto(UNCOVERED_URL);
    await openPanel(app);
    const uncovered = await yearPanelShape(app);

    expect(uncovered, 'hai màn phải dùng chung một bảng chọn năm').toEqual(detail);
    expect(detail.rangeText, 'đầu bảng phải ghi đúng khoảng chọn thật').toBe(
      `Chọn trong ${MIN_YEAR} – ${MAX_YEAR}`,
    );
    expect(detail.todayText).toBe('Năm nay');
    expect(detail.todayLeftOfRange, '"Năm nay" phải nằm ở góc trái đầu bảng').toBe(true);
    expect(detail.columns, 'lưới năm phải có 5 cột').toBe(5);
  });

  for (const { label, url } of YEAR_PICKER_SCREENS) {
    test(`${label}: không còn Segmented năm, không còn tab`, async ({ app }) => {
      await app.goto(url);
      await expectYear(app, SERVER_YEAR);

      await expect(app.locator('.ant-segmented')).toHaveCount(0);
      await expect(app.getByRole('tablist')).toHaveCount(0);
      await expect(app.getByRole('tab')).toHaveCount(0);
      await expect(app.locator('.hhd-period-info')).toHaveCount(0);
    });
  }

  test('màn Đợt trao huy hiệu KHÔNG có bộ chọn năm', async ({ app }) => {
    await app.goto(PERIODS_URL);
    await expect(app.locator('.hhd-periods__panel .ant-table-row').first()).toBeVisible();

    await expect(app.locator('.hhd-year-picker')).toHaveCount(0);
    await expect(app.locator('.ant-segmented')).toHaveCount(0);
  });
});

/**
 * Bốn màu của bộ thiết kế, đã quy về dạng `rgb(...)` — cùng dạng
 * `getComputedStyle` trả về, nên so trực tiếp được.
 *
 * Việc quy đổi để chính trình duyệt làm: gán token vào một thẻ tạm rồi đọc lại
 * màu đã tính. Tự đổi mã hex bằng tay thì `#fff` viết tắt sẽ ra số vô nghĩa.
 */
function tokenColors(app: Page) {
  return app.evaluate(() => {
    const probe = document.createElement('span');
    probe.style.display = 'none';
    document.body.append(probe);
    const resolve = (name: string) => {
      probe.style.color = `var(${name})`;
      return window.getComputedStyle(probe).color;
    };
    const colors = {
      red: resolve('--hhd-red'),
      fill: resolve('--hhd-fill'),
      white: resolve('--hhd-white'),
      goldSoft: resolve('--hhd-gold-soft'),
    };
    probe.remove();
    return colors;
  });
}

test.describe('T66 · bộ chọn năm đúng kiểu B mà chủ dự án đã duyệt', () => {
  test('rãnh xám, ô năm trắng nổi bóng mềm, số năm chữ có chân màu đỏ', async ({ app }) => {
    await app.goto(DETAIL_URL);
    await expectYear(app, SERVER_YEAR);

    const token = await tokenColors(app);
    const shape = await settledYearPickerShape(app);
    const [back, year, forward] = shape.buttons;

    // Rãnh ngoài: nền xám nhạt `--hhd-fill`.
    expect(shape.background, 'rãnh của bộ chọn phải là nền xám nhạt').toBe(token.fill);

    // Ô năm giữa: kiểu B là nền TRẮNG, số ĐỎ, có bóng, KHÔNG viền. Kiểu A có
    // viền đỏ, kiểu C tô đỏ đặc — hai điều kiện dưới đây loại cả hai kiểu kia.
    expect(year.ariaLabel).toBe('Chọn năm khác, đang xem NĂM');
    expect(year.background, 'ô năm phải nền trắng (kiểu C tô đỏ đặc)').toBe(token.white);
    expect(year.backgroundImage, 'ô năm không được tô gradient').toBe('none');
    expect(year.color, 'số năm phải màu đỏ chủ đạo').toBe(token.red);
    expect(year.borderTopWidth, 'ô năm không có viền (kiểu A có viền đỏ)').toBe('0px');
    expect(year.boxShadow, 'ô năm phải có bóng mềm').not.toBe('none');
    expect(year.fontFamily, 'số năm phải là chữ có chân Noto Serif').toContain('Noto Serif');
    expect(Number(year.fontWeight)).toBeGreaterThanOrEqual(600);

    // Hai nút mũi tên: nền trong suốt, mũi tên màu chữ phụ, không đỏ.
    for (const [step, name] of [
      [back, 'nút lùi'],
      [forward, 'nút tiến'],
    ] as const) {
      expect(step.background, `${name} phải nền trong suốt`).toBe('rgba(0, 0, 0, 0)');
      expect(step.color, `${name} không dùng màu đỏ`).not.toBe(token.red);
    }
  });

  test('bảng chọn: năm đang xem đỏ đặc, năm nay chữ đỏ có chấm', async ({ app }) => {
    await app.goto(DETAIL_URL);
    // Xem một năm KHÁC năm máy chủ thì hai dấu mới hiện cùng lúc.
    await openPanel(app);
    await panelYear(app, SERVER_YEAR - 12).click();
    await expectYear(app, SERVER_YEAR - 12);
    await openPanel(app);

    const token = await tokenColors(app);
    const readMarks = () =>
      app.evaluate(() => {
        const root = document.querySelector('.hhd-year-panel')!;
        const selected = root.querySelector('.hhd-year-panel__cell--selected') as HTMLElement;
        const today = root.querySelector('.hhd-year-panel__cell--today') as HTMLElement;
        const selectedStyle = window.getComputedStyle(selected);
        const todayStyle = window.getComputedStyle(today);
        const dot = window.getComputedStyle(today, '::after');
        return {
          selectedYear: selected.textContent,
          selectedBackgroundImage: selectedStyle.backgroundImage,
          selectedColor: selectedStyle.color,
          todayYear: today.textContent,
          todayColor: todayStyle.color,
          dotContent: dot.content,
          dotWidth: dot.width,
          dotBackground: dot.backgroundColor,
          dotRadius: dot.borderRadius,
        };
      });
    // Ô năm cũng có `transition: color`, nên đọc tới khi màu đứng yên.
    const marks = await settled(app, readMarks, 'dấu trong bảng chọn năm');

    expect(marks.selectedYear, 'năm đang xem phải là năm vừa chọn').toBe(String(SERVER_YEAR - 12));
    expect(marks.selectedBackgroundImage, 'năm đang xem phải là viên đỏ đặc').toContain(
      'linear-gradient',
    );
    expect(marks.selectedColor, 'số của năm đang xem phải trắng').toBe(token.white);

    expect(marks.todayYear, 'dấu "năm nay" phải ở đúng năm máy chủ').toBe(String(SERVER_YEAR));
    expect(marks.todayColor, 'số của năm nay phải màu đỏ').toBe(token.red);
    expect(marks.dotContent, 'năm nay phải có chấm dưới số').not.toBe('none');
    expect(marks.dotWidth).toBe('4px');
    expect(marks.dotBackground, 'chấm phải màu đỏ').toBe(token.red);
    expect(marks.dotRadius, 'chấm phải tròn').toBe('50%');
  });

  test('vừa mở bảng bấm Enter rồi bấm Space: không lần nào đổi năm ngoài ý muốn', async ({
    app,
  }) => {
    // Ca bàn phím từng chập chờn khi chạy lặp, đã sửa ở `9c82061`. Ở đây bấm
    // hai lần liền trong MỘT ca, đúng cảnh chủ dự án gặp.
    await app.goto(DETAIL_URL);
    await expectYear(app, SERVER_YEAR);

    for (const key of ['Enter', 'Space'] as const) {
      await openPanel(app);
      await app.keyboard.press(key);
      await expect(panel(app), `${key} phải đóng bảng`).toBeHidden();
      await expectYear(app, SERVER_YEAR);
    }

    await expect(app.locator('.hhd-eligibility__range')).toContainText('Năm nay');
  });
});

/* ------------------------------------------------------------------ */
/* 3. Giới hạn năm máy chủ ± 100 — hai phía, hai cơ chế, hai màn        */
/* ------------------------------------------------------------------ */

test.describe('T66 · giới hạn năm máy chủ ± 100', () => {
  for (const { label, url, endpoint } of YEAR_PICKER_SCREENS) {
    test(`${label}: nút ‹ › dừng đúng ở ${MIN_YEAR} và ${MAX_YEAR}`, async ({ app }) => {
      await app.goto(url);
      await expectYear(app, SERVER_YEAR);

      // Tới sát trần bằng bảng chọn, rồi thử bước thêm một năm bằng nút.
      await openPanel(app);
      await panelYear(app, MAX_YEAR - 1).click();
      await expectYear(app, MAX_YEAR - 1);

      await expect(forwardButton(app), `${label}: còn một năm nữa mới tới trần`).toBeEnabled();
      await Promise.all([
        app.waitForRequest((request) => {
          const parsed = new URL(request.url());
          return (
            parsed.pathname.endsWith(endpoint) &&
            parsed.searchParams.get('year') === String(MAX_YEAR)
          );
        }),
        forwardButton(app).click(),
      ]);
      await expectYear(app, MAX_YEAR);
      await expect(forwardButton(app), `${label}: ở trần thì › phải khóa`).toBeDisabled();
      await expect(backButton(app)).toBeEnabled();

      // Xuống sát sàn rồi thử lùi thêm.
      await openPanel(app);
      await panelYear(app, MIN_YEAR + 1).click();
      await expectYear(app, MIN_YEAR + 1);

      await expect(backButton(app)).toBeEnabled();
      await backButton(app).click();
      await expectYear(app, MIN_YEAR);
      await expect(backButton(app), `${label}: ở sàn thì ‹ phải khóa`).toBeDisabled();
      await expect(forwardButton(app)).toBeEnabled();
    });

    test(`${label}: bảng chọn khóa đúng mọi ô ngoài ${MIN_YEAR} – ${MAX_YEAR}`, async ({ app }) => {
      await app.goto(url);
      await expectYear(app, SERVER_YEAR);
      await openPanel(app);

      await expect(panel(app)).toContainText(`Chọn trong ${MIN_YEAR} – ${MAX_YEAR}`);

      // Lưới căn theo bội số của 5 nên hàng đầu và hàng cuối thừa ra vài ô.
      // Mọi ô thừa đó phải khóa, và hai ô sát mép trong khoảng phải bấm được.
      const disabled = await panel(app)
        .locator('.hhd-year-panel__cell:disabled')
        .evaluateAll((cells) => cells.map((cell) => Number(cell.textContent)));

      expect(disabled.length, `${label}: phải có ô ngoài khoảng bị khóa`).toBeGreaterThan(0);
      for (const year of disabled) {
        expect(
          year < MIN_YEAR || year > MAX_YEAR,
          `${label}: năm ${year} nằm trong khoảng nhưng bị khóa`,
        ).toBe(true);
      }
      await expect(panelYear(app, MIN_YEAR)).toBeEnabled();
      await expect(panelYear(app, MAX_YEAR)).toBeEnabled();
      await expect(panelYear(app, MIN_YEAR - 1)).toBeDisabled();
      await expect(panelYear(app, MAX_YEAR + 1)).toBeDisabled();
    });

    test(`${label}: bàn phím trong lưới cũng không vượt được ${MAX_YEAR}`, async ({ app }) => {
      await app.goto(url);
      await expectYear(app, SERVER_YEAR);
      await openPanel(app);
      await panelYear(app, MAX_YEAR).click();
      await expectYear(app, MAX_YEAR);

      await openPanel(app);
      // Năm ô liền nhau về phía trước, rồi chọn: vẫn phải là năm sát trần.
      for (let press = 0; press < 5; press += 1) await app.keyboard.press('ArrowRight');
      await app.keyboard.press('ArrowDown');
      await app.keyboard.press('Enter');

      await expect(panel(app)).toBeHidden();
      await expectYear(app, MAX_YEAR);
    });
  }
});

/* ------------------------------------------------------------------ */
/* 4. T64 — màn Đợt: không dòng tóm tắt, không nền vàng                */
/* ------------------------------------------------------------------ */

/** Cỡ trang đủ chứa cả 36 đợt mẫu, để một trang có đủ ba trạng thái. */
const ALL_ON_ONE_PAGE = 100;

test.describe('T64 · màn Đợt trao huy hiệu trên bản gộp', () => {
  test('tiêu đề đứng một mình ở mọi trạng thái của màn', async ({ app }) => {
    await app.goto(PERIODS_URL);
    await expect(app.getByRole('heading', { name: 'Đợt trao huy hiệu' })).toBeVisible();
    await expect(app.locator('.hhd-page-heading__description')).toHaveCount(0);
    await expect(app.locator('.hhd-periods__panel .ant-table-row').first()).toBeVisible();
    await expect(app.locator('.hhd-page-heading__description')).toHaveCount(0);
    await expect(app.getByText('dùng chung cho mọi năm')).toHaveCount(0);
  });

  test('không ô nào trong bảng còn nền vàng của token gold-soft', async ({ app }) => {
    await app.goto(PERIODS_URL);
    const rows = app.locator('.hhd-periods__panel .ant-table-tbody tr.ant-table-row');
    await expect(rows.first()).toBeVisible();

    // Dàn cả 36 đợt lên một trang để đủ ba trạng thái, rồi rời chuột khỏi bảng:
    // hover của Ant Design cũng đổi nền dòng.
    await app.locator('.ant-pagination .ant-select').click();
    await app
      .locator(
        `.ant-select-dropdown:not(.ant-select-dropdown-hidden) .ant-select-item-option[title="${ALL_ON_ONE_PAGE} / trang"]`,
      )
      .click();
    await app.mouse.move(0, 0);
    await expect(rows).toHaveCount(mockData.periods.length);

    const token = await tokenColors(app);
    const gold = token.goldSoft;

    const painted = await rows.locator('td').evaluateAll((cells, wanted) => {
      return cells
        .map((cell, index) => ({ index, color: window.getComputedStyle(cell).backgroundColor }))
        .filter((cell) => cell.color === wanted);
    }, gold);

    expect(painted, `không ô nào được tô nền vàng ${gold}`).toEqual([]);
    await expect(app.locator('.hhd-periods__row--next')).toHaveCount(0);

    // Bỏ nền dòng chứ không bỏ nhãn: ba nhãn trạng thái vẫn phải còn.
    const statuses = await rows.locator('td:nth-child(5)').allInnerTexts();
    expect(statuses.some((text) => text === 'Đã qua')).toBe(true);
    expect(statuses.some((text) => text === 'Đang diễn ra')).toBe(true);
    expect(statuses.some((text) => text.startsWith('Sắp tới'))).toBe(true);
  });
});

/* ------------------------------------------------------------------ */
/* 5. Ảnh soi tay bàn giao — 1440x900 và 1280x600                      */
/* ------------------------------------------------------------------ */

const SHOT_VIEWPORTS = [
  { width: 1440, height: 900 },
  { width: 1280, height: 600 },
] as const;

for (const viewport of SHOT_VIEWPORTS) {
  const label = `${viewport.width}x${viewport.height}`;

  test.describe(`Ảnh soi tay ${label}`, () => {
    test.use({ viewport });

    test(`Chi tiết đợt và bộ chọn năm ở ${label}`, async ({ app }) => {
      await app.goto(DETAIL_URL);
      await expect(app.locator('.hhd-eligibility .ant-table-row').first()).toBeVisible();
      await app.mouse.move(0, 0);
      await app.screenshot({ path: path.join(SHOT_DIR, `t67-chi-tiet-dot-${label}.png`) });

      // Ảnh cận bộ chọn năm, để đối chiếu kiểu B với ảnh dựng thử của CEO.
      await app
        .locator('.hhd-eligibility__toolbar')
        .screenshot({ path: path.join(SHOT_DIR, `t67-bo-chon-nam-${label}.png`) });
    });

    test(`bảng chọn năm mở tại năm khác năm nay ở ${label}`, async ({ app }) => {
      await app.goto(DETAIL_URL);
      await openPanel(app);
      // Xem năm liền sau năm máy chủ, đúng cảnh trong ảnh dựng thử của CEO:
      // hai dấu nằm cạnh nhau nên một khung ảnh thấy được cả hai. Năm xa hơn
      // thì lưới cuộn năm đang xem vào giữa và đẩy năm nay ra ngoài tầm nhìn.
      await panelYear(app, SERVER_YEAR + 1).click();
      await expectYear(app, SERVER_YEAR + 1);
      await openPanel(app);

      // Cả hai dấu cùng hiện: năm đang xem đỏ đặc, năm nay chữ đỏ có chấm.
      const selected = panel(app).locator('.hhd-year-panel__cell--selected');
      const today = panel(app).locator('.hhd-year-panel__cell--today');
      await expect(selected).toHaveText(String(SERVER_YEAR + 1));
      await expect(today).toHaveText(String(SERVER_YEAR));
      await expect(selected).toBeInViewport();
      await expect(today).toBeInViewport();
      await app.mouse.move(0, 0);
      await app.screenshot({ path: path.join(SHOT_DIR, `t67-bang-chon-nam-${label}.png`) });
    });

    test(`giới hạn trần khóa nút › ở ${label}`, async ({ app }) => {
      await app.goto(DETAIL_URL);
      await openPanel(app);
      await panelYear(app, MAX_YEAR).click();
      await expectYear(app, MAX_YEAR);
      await expect(forwardButton(app)).toBeDisabled();
      await app.mouse.move(0, 0);
      await app.screenshot({ path: path.join(SHOT_DIR, `t67-gioi-han-tran-${label}.png`) });
    });

    test(`Chưa thuộc đợt nào ở ${label}`, async ({ app }) => {
      await app.goto(UNCOVERED_URL);
      await expect(app.locator('.hhd-uncovered .ant-table-row').first()).toBeVisible();
      await app.mouse.move(0, 0);
      await app.screenshot({ path: path.join(SHOT_DIR, `t67-chua-thuoc-dot-nao-${label}.png`) });
    });

    test(`màn Đợt trao huy hiệu ở ${label}`, async ({ app }) => {
      await app.goto(PERIODS_URL);
      await expect(app.locator('.hhd-periods__panel .ant-table-row').first()).toBeVisible();
      await app.mouse.move(0, 0);
      await app.screenshot({ path: path.join(SHOT_DIR, `t67-man-dot-${label}.png`) });
    });
  });
}
