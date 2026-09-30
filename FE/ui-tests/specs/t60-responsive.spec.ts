import path from 'node:path';

import type { Page } from '@playwright/test';

import { DETAIL_PERIOD, expect, test } from '../fixtures/app';

/**
 * T60 — responsive mức 1 cho màn máy tính nhỏ.
 *
 * Tệp này chạy ở BỐN khung nhìn, khai báo trong `playwright.ui.config.ts`:
 * 1280x600 (cận dưới), 1366x650 (laptop 1366x768 của chủ dự án), 1440x900
 * (khung của bộ thiết kế) và 3440x1440 (màn 34 inch). Cùng một bộ ca chạy cho
 * cả bốn nên mỗi khẳng định phải đúng ở mọi khung, còn phần riêng của màn cao
 * thì đứng trong nhánh `if (tallViewport)`.
 *
 * Bốn điều được canh:
 * - Ở màn thấp, mỗi bảng hiện ít nhất 8 dòng, hoặc đủ số dòng nếu trang có ít
 *   hơn; ở màn cao thì chỉ cần không mất dòng nào, vì bố cục cũ phải giữ nguyên.
 * - Thanh phân trang tới được; ở màn cao thì thấy ngay không cần cuộn.
 * - Không màn nào sinh thanh cuộn ngang của cả trang.
 * - Thang gọn giữ đúng sàn: chữ thân và chữ bảng >= 13px, nút cao >= 32px; còn
 *   màn cao thì cỡ chữ không đổi một pixel nào.
 */

/** Ngưỡng của thang gọn — khớp `COMPACT_VIEWPORT_QUERY` trong `src/theme`. */
const COMPACT_MAX_HEIGHT = 899;

/** Số dòng tối thiểu phải nhìn thấy được ở mọi bảng. */
const MIN_VISIBLE_ROWS = 8;

const SHOT_DIR = path.join(import.meta.dirname, '..', '.artifacts', 'anh-t60');

interface Screen {
  /** Tên tệp ảnh chụp */
  key: string;
  label: string;
  open: (app: Page) => Promise<void>;
  /** Thẻ bảng của màn, để trống nếu màn không có bảng phân trang */
  table?: string;
}

const SCREENS: Screen[] = [
  {
    key: '1-dashboard',
    label: 'Dashboard',
    table: '.hhd-dashboard__panel',
    open: async (app) => {
      await app.goto('/');
      await expect(app.locator('.hhd-dashboard__panel .ant-table-row').first()).toBeVisible();
    },
  },
  {
    key: '2-dang-vien',
    label: 'Đảng viên',
    table: '.hhd-members__panel',
    open: async (app) => {
      await app.goto('/dang-vien');
      await expect(app.locator('.hhd-members__panel .ant-table-row').first()).toBeVisible();
    },
  },
  {
    key: '3-import-buoc-1',
    label: 'Import bước 1 — chọn file',
    open: async (app) => {
      await app.goto('/dang-vien/import');
      await expect(app.getByRole('button', { name: 'Tiếp tục ›' })).toBeVisible();
    },
  },
  {
    key: '4-import-buoc-2',
    label: 'Import bước 2 — xem trước',
    open: async (app) => {
      await openImportStepTwo(app);
    },
  },
  {
    key: '5-import-buoc-3',
    label: 'Import bước 3 — kết quả',
    open: async (app) => {
      await openImportStepTwo(app);
      await app.getByRole('button', { name: /^Nạp / }).click();
      await expect(app.getByRole('button', { name: 'Import file khác' })).toBeVisible();
    },
  },
  {
    key: '6-dot-trao-huy-hieu',
    label: 'Đợt trao huy hiệu',
    table: '.hhd-periods__panel',
    open: async (app) => {
      await app.goto('/dot-trao-huy-hieu');
      await expect(app.locator('.hhd-periods__panel .ant-table-row').first()).toBeVisible();
    },
  },
  {
    key: '7-chi-tiet-dot-thong-tin',
    label: 'Chi tiết đợt — tab Thông tin',
    open: async (app) => {
      await app.goto(`/dot-trao-huy-hieu/${DETAIL_PERIOD.id}`);
      await expect(app.locator('.hhd-period-info')).toBeVisible();
    },
  },
  {
    key: '8-chi-tiet-dot-du-dieu-kien',
    label: 'Chi tiết đợt — tab Danh sách đủ điều kiện',
    table: '.hhd-eligibility',
    open: async (app) => {
      await app.goto(`/dot-trao-huy-hieu/${DETAIL_PERIOD.id}`);
      await app.getByRole('tab', { name: 'Danh sách đủ điều kiện' }).click();
      await expect(app.locator('.hhd-eligibility .ant-table-row').first()).toBeVisible();
    },
  },
  {
    key: '9-chua-thuoc-dot-nao',
    label: 'Chưa thuộc đợt nào',
    table: '.hhd-uncovered__panel',
    open: async (app) => {
      await app.goto('/chua-thuoc-dot-nao');
      await expect(app.locator('.hhd-uncovered__panel .ant-table-row').first()).toBeVisible();
    },
  },
  {
    key: '10-cai-dat',
    label: 'Cài đặt',
    open: async (app) => {
      await app.goto('/cai-dat');
      await expect(app.locator('.hhd-settings__card').first()).toBeVisible();
    },
  },
  {
    key: '11-khong-tim-thay-trang',
    label: 'Trang 404',
    open: async (app) => {
      await app.goto('/duong-dan-nay-khong-co');
      await expect(app.getByText('Không tìm thấy trang này')).toBeVisible();
    },
  },
];

/** Bước 1 → bước 2 của màn Nạp danh sách: thả một file .xlsx giả rồi Tiếp tục. */
async function openImportStepTwo(app: Page): Promise<void> {
  await app.goto('/dang-vien/import');
  await app.locator('.hhd-import input[type="file"]').setInputFiles({
    name: 'DanhSachDangVien.xlsx',
    mimeType: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    buffer: Buffer.from('file giả cho ca kiểm thử'),
  });
  await app.getByRole('button', { name: 'Tiếp tục ›' }).click();
  await expect(app.locator('.hhd-import__panel')).toBeVisible();
}

/** Khung nhìn hiện tại; dùng để tách khẳng định của màn cao và màn thấp. */
function viewportOf(app: Page): { width: number; height: number } {
  const size = app.viewportSize();
  if (!size) throw new Error('Ca này phải chạy với khung nhìn cố định');
  return size;
}

/**
 * Đếm số dòng NHÌN THẤY ĐƯỢC của một bảng.
 *
 * "Nhìn thấy được" nghĩa là hình chữ nhật của dòng nằm trọn trong khung nhìn VÀ
 * trong mọi lớp cắt bên trên nó — `toBeInViewport` của Playwright không xét lớp
 * cắt, nên một dòng bị thẻ `overflow: hidden` che vẫn bị tính là thấy.
 */
async function countVisibleRows(
  app: Page,
  table: string,
): Promise<{ visible: number; total: number }> {
  return app.evaluate((selector) => {
    const root = document.querySelector(selector);
    if (!root) throw new Error(`Không thấy thẻ bảng ${selector}`);
    const rows = Array.from(root.querySelectorAll('.ant-table-tbody > tr.ant-table-row'));

    /** Phần chiều dọc còn lại sau khi trừ mọi vùng cuộn / vùng cắt bên trên. */
    function clipOf(element: Element): { top: number; bottom: number } {
      let top = 0;
      let bottom = window.innerHeight;
      for (let parent = element.parentElement; parent; parent = parent.parentElement) {
        if (getComputedStyle(parent).overflowY !== 'visible') {
          const box = parent.getBoundingClientRect();
          top = Math.max(top, box.top);
          bottom = Math.min(bottom, box.bottom);
        }
      }
      return { top, bottom };
    }

    let visible = 0;
    for (const row of rows) {
      const box = row.getBoundingClientRect();
      const clip = clipOf(row);
      if (box.height > 0 && box.top >= clip.top - 1 && box.bottom <= clip.bottom + 1) visible += 1;
    }
    return { visible, total: rows.length };
  }, table);
}

/** Cuộn sao cho dòng đầu của bảng lên đầu vùng cuộn, rồi đếm lại. */
async function scrollTableIntoView(app: Page, table: string): Promise<void> {
  await app.evaluate((selector) => {
    const first = document.querySelector(`${selector} .ant-table-tbody > tr.ant-table-row`);
    first?.scrollIntoView({ block: 'start' });
  }, table);
}

/** Cửa sổ có sinh thanh cuộn (ngang / dọc) hay không. */
async function windowScroll(app: Page): Promise<{ horizontal: boolean; vertical: boolean }> {
  return app.evaluate(() => ({
    horizontal: document.documentElement.scrollWidth > window.innerWidth + 1,
    vertical: document.documentElement.scrollHeight > window.innerHeight + 1,
  }));
}

for (const screen of SCREENS) {
  test(`${screen.label} — không mất nội dung, không cuộn ngang`, async ({ app }) => {
    const viewport = viewportOf(app);
    const tallViewport = viewport.height > COMPACT_MAX_HEIGHT;

    await screen.open(app);

    // Cửa sổ không bao giờ được cuộn ngang; phần quá rộng là việc của thẻ bảng.
    const beforeScroll = await windowScroll(app);
    expect(beforeScroll.horizontal, `${screen.label}: cửa sổ mọc thanh cuộn ngang`).toBe(false);

    if (tallViewport) {
      // Hành vi cũ của khung 1440x900 và màn 34 inch: cả trang vừa khít cửa sổ.
      expect(beforeScroll.vertical, `${screen.label}: màn cao không được cuộn cửa sổ`).toBe(false);
    }

    if (screen.table) {
      const pagination = app.locator(`${screen.table} .ant-pagination`).first();

      if (tallViewport) {
        // Màn cao: phân trang nằm trong khung nhìn ngay, không phải cuộn.
        await expect(pagination).toBeInViewport();
      }

      await scrollTableIntoView(app, screen.table);
      const rows = await countVisibleRows(app, screen.table);
      expect(rows.total, `${screen.label}: bảng không dựng được dòng nào`).toBeGreaterThan(0);

      // Sàn 8 dòng là yêu cầu của MÀN THẤP. Màn cao phải giữ nguyên bố cục cũ,
      // mà bố cục cũ vốn chỉ hiện 4-7 dòng ở vài màn (thẻ đợt sắp tới và dải độ
      // phủ chiếm chỗ) — ép 8 dòng ở đó là đổi khung 1440x900, trái yêu cầu 1.
      const minimumRows = tallViewport ? 1 : Math.min(MIN_VISIBLE_ROWS, rows.total);
      expect(
        rows.visible,
        `${screen.label}: chỉ thấy ${rows.visible}/${rows.total} dòng`,
      ).toBeGreaterThanOrEqual(minimumRows);

      // Thanh phân trang phải tới được: cuộn tới rồi nằm trong khung nhìn.
      await pagination.scrollIntoViewIfNeeded();
      await expect(pagination).toBeInViewport();

      // Cuộn dọc không được kéo theo thanh cuộn ngang của cả trang.
      const afterScroll = await windowScroll(app);
      expect(afterScroll.horizontal, `${screen.label}: cuộn xong lại mọc cuộn ngang`).toBe(false);
    }

    await app.evaluate(() => window.scrollTo(0, 0));
    await app.screenshot({
      path: path.join(SHOT_DIR, `${viewport.width}x${viewport.height}`, `${screen.key}.png`),
    });
  });
}

test('thang gọn giữ đúng sàn cỡ chữ và chiều cao nút', async ({ app }) => {
  const viewport = viewportOf(app);
  const tallViewport = viewport.height > COMPACT_MAX_HEIGHT;

  await app.goto('/dang-vien');
  await expect(app.locator('.hhd-members__panel .ant-table-row').first()).toBeVisible();

  const bodyFontSize = await app.evaluate(() =>
    parseFloat(getComputedStyle(document.body).fontSize),
  );
  const cellFontSize = await app
    .locator('.hhd-members__panel .ant-table-tbody > tr.ant-table-row td')
    .first()
    .evaluate((cell) => parseFloat(getComputedStyle(cell).fontSize));
  const buttonHeight = await app
    .getByRole('button', { name: '+ Thêm' })
    .evaluate((node) => node.getBoundingClientRect().height);

  if (tallViewport) {
    // Màn cao: đúng bộ thiết kế, thang gọn không được chạm vào.
    expect(bodyFontSize).toBe(14);
    expect(cellFontSize).toBe(14);
    expect(buttonHeight).toBe(40);
  } else {
    // Màn thấp: nhỏ hơn một nấc nhưng không dưới sàn đã chốt.
    expect(bodyFontSize).toBeLessThan(14);
    expect(bodyFontSize).toBeGreaterThanOrEqual(13);
    expect(cellFontSize).toBeGreaterThanOrEqual(13);
    expect(buttonHeight).toBeGreaterThanOrEqual(32);
  }
});

test('menu trái tự thu gọn khi cửa sổ hẹp, vẫn mở lại được', async ({ app }) => {
  const viewport = viewportOf(app);
  const narrow = viewport.width < 1440;

  await app.goto('/');
  const shell = app.locator('.hhd-shell');
  const label = app.locator('.hhd-sider__label').first();

  if (narrow) {
    await expect(shell).toHaveClass(/hhd-shell--collapsed/);
    // Nút thu gọn có sẵn vẫn mở được menu ra — lựa chọn của người dùng thắng.
    await app.getByRole('button', { name: 'Mở rộng menu' }).click();
    await expect(shell).not.toHaveClass(/hhd-shell--collapsed/);
    await expect(label).toBeVisible();
  } else {
    await expect(shell).not.toHaveClass(/hhd-shell--collapsed/);
    await expect(label).toBeVisible();
  }
});

for (const modal of [
  {
    key: '12-modal-them-dang-vien',
    label: 'modal Thêm đảng viên',
    open: async (app: Page) => {
      await app.goto('/dang-vien');
      await app.getByRole('button', { name: '+ Thêm' }).click();
    },
    save: 'Thêm vào danh sách',
  },
  {
    key: '13-modal-them-dot',
    label: 'modal Thêm đợt trao huy hiệu',
    open: async (app: Page) => {
      await app.goto('/dot-trao-huy-hieu');
      await app.getByRole('button', { name: '+ Thêm đợt' }).click();
    },
    save: 'Thêm đợt',
  },
]) {
  test(`${modal.label} vừa trong khung nhìn, nút Lưu tới được`, async ({ app }) => {
    const viewport = viewportOf(app);
    await modal.open(app);

    const dialog = app.getByRole('dialog');
    await expect(dialog).toBeVisible();

    const save = dialog.getByRole('button', { name: modal.save });
    await expect(save).toBeVisible();
    await expect(save).toBeInViewport();

    // Cả hộp thoại phải nằm trong khung nhìn, không tràn ra ngoài mép trên/dưới.
    const box = await dialog.boundingBox();
    expect(box, `${modal.label}: không đo được hộp thoại`).not.toBeNull();
    expect(box!.y).toBeGreaterThanOrEqual(-1);
    expect(box!.y + box!.height).toBeLessThanOrEqual(viewport.height + 1);

    await app.screenshot({
      path: path.join(SHOT_DIR, `${viewport.width}x${viewport.height}`, `${modal.key}.png`),
    });
  });
}
