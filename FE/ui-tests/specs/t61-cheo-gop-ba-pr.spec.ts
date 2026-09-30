import path from 'node:path';

import type { Locator } from '@playwright/test';

import { expect, test } from '../fixtures/app';
import { TableView } from '../fixtures/table';

/**
 * T61 — nghiệm thu bản GỘP ba PR của đợt 30/09 (T58, T59, T60).
 *
 * Ba PR cố ý chia tệp để không đụng nhau, nên từng PR đều xanh khi chạy riêng.
 * Tệp này kiểm đúng phần không PR nào kiểm được một mình: chỗ ba việc GẶP NHAU.
 *
 * 1. T58 x T60 — cỡ trang 10 của bảng Dashboard sống trong khung responsive.
 *    `t58-quyet-dinh-giao-dien.spec.ts` chỉ chạy ở khung mặc định 1440x900;
 *    `t60-responsive.spec.ts` khẳng định "thấy được >= 8 dòng" nhưng không biết
 *    bảng này có đúng 10 dòng. Nếu thang gọn hay lớp cuộn vùng nội dung của T60
 *    kéo cỡ trang về 20 hay nuốt mất thanh phân trang, cả hai bộ ca kia vẫn xanh.
 *
 * 2. T59 x T60 — cờ mới nằm trong thanh đầu trang đã bị T60 thu thấp.
 *    T60 hạ `--hhd-header-height` từ 56px xuống 48px ở khung nhìn thấp
 *    (`theme/responsive.css`), còn T59 đặt vào đó ảnh cờ cao 28px và rộng hơn
 *    hẳn logo vuông cũ (`components/Logo.tsx`). T59 có kiểm header ở 1280x600,
 *    nhưng trên nhánh của nó header vẫn cao 56px — chưa ai thấy cờ trong 48px.
 *
 * Chạy ở cả bốn khung nhìn của T60: hai khung thấp là chỗ hai lớp thay đổi
 * chồng nhau, hai khung cao đóng vai mốc đối chiếu.
 */

const SHOT_DIR = path.join(import.meta.dirname, '..', '.artifacts', 'anh-t61');

/**
 * Bốn khung nhìn của T60, khai lại tại chỗ thay vì nhập từ
 * `playwright.ui.config.ts`: tệp cấu hình thuộc HUYH-68, còn tệp này phải tự
 * đứng được để gộp sau cả ba PR mà không kéo theo phụ thuộc biên dịch.
 * Đổi danh sách ở cấu hình thì đổi cả ở đây.
 */
const VIEWPORTS = [
  { width: 1280, height: 600 },
  { width: 1366, height: 650 },
  { width: 1440, height: 900 },
  { width: 3440, height: 1440 },
] as const;

/** Cỡ trang riêng của thẻ đủ điều kiện trên Dashboard — quyết định của T58. */
const DASHBOARD_PAGE_SIZE = 10;

/** Ngưỡng bật thang gọn, khớp `COMPACT_VIEWPORT_QUERY` của `theme/breakpoints.ts`. */
const COMPACT_MAX_HEIGHT = 899;

/** Chiều cao thanh đầu trang: `tokens.css` 56px, `responsive.css` hạ còn 48px. */
const HEADER_HEIGHT_TALL = 56;
const HEADER_HEIGHT_COMPACT = 48;

/**
 * Cuộn một phần tử vào tầm nhìn rồi hỏi nó có thật sự nằm trong khung nhìn
 * không. Dùng cho từng dòng bảng: "tới được" mới là điều người dùng cần, còn
 * đòi "thấy ngay không cần cuộn" thì bảng nào cũng trượt.
 */
async function expectReachable(target: Locator, what: string): Promise<void> {
  await target.scrollIntoViewIfNeeded();
  await expect(target, `${what} không tới được`).toBeInViewport();
}

/** Hai hình chữ nhật có chồng lên nhau theo chiều ngang không. */
function overlapsHorizontally(
  a: { x: number; width: number },
  b: { x: number; width: number },
): boolean {
  return a.x < b.x + b.width - 1 && b.x < a.x + a.width - 1;
}

for (const viewport of VIEWPORTS) {
  const label = `${viewport.width}x${viewport.height}`;
  const compact = viewport.height <= COMPACT_MAX_HEIGHT;

  test.describe(`khung nhìn ${label}`, () => {
    test.use({ viewport });

    test(`T58 x T60 · bảng Dashboard giữ đúng 10 dòng và tới được hết ở ${label}`, async ({
      app,
    }) => {
      await app.goto('/');
      const panel = app.locator('.hhd-dashboard__panel');
      const table = new TableView(app, panel);
      await expect(panel).toBeVisible();

      // 1. Cỡ trang của T58 không bị thang gọn của T60 kéo về mặc định chung 20.
      await expect(table.rows, `${label}: bảng Dashboard phải có đúng 10 dòng`).toHaveCount(
        DASHBOARD_PAGE_SIZE,
      );
      await table.expectPageSize(DASHBOARD_PAGE_SIZE);

      // 2. Quyết định còn lại của T58 cũng phải sống sót qua khung responsive.
      await table.expectNoTotalText();
      await expect(table.pagination).not.toHaveText(/\d[–-]\d+\s*\/\s*\d/);

      // 3. Cả mười dòng đều tới được — điều `t60-responsive` không kiểm nổi vì
      //    sàn của nó là 8 dòng, thấp hơn cỡ trang thật của bảng này.
      for (let index = 0; index < DASHBOARD_PAGE_SIZE; index += 1) {
        await expectReachable(table.rows.nth(index), `${label}: dòng ${index + 1}`);
      }

      // 4. Cuộn hết mười dòng rồi thanh phân trang vẫn phải tới được.
      await expectReachable(table.pagination, `${label}: thanh phân trang`);

      // 5. Không khung nhìn nào được sinh thanh cuộn ngang của cả cửa sổ.
      const horizontal = await app.evaluate(
        () => document.documentElement.scrollWidth > window.innerWidth + 1,
      );
      expect(horizontal, `${label}: cửa sổ mọc thanh cuộn ngang`).toBe(false);

      await app.evaluate(() => {
        window.scrollTo(0, 0);
        for (const node of document.querySelectorAll('.hhd-main, .ant-table-content')) {
          node.scrollTop = 0;
        }
      });
      await app.screenshot({ path: path.join(SHOT_DIR, `t61-dashboard-10-dong-${label}.png`) });
    });

    test(`T59 x T60 · cờ vừa trong thanh đầu trang và không đè chữ ở ${label}`, async ({ app }) => {
      await app.goto('/');

      const header = app.locator('.hhd-header');
      const flag = header.locator('img');
      await expect(header).toBeVisible();
      await expect(flag).toBeVisible();

      // 1. Thang gọn của T60 phải đúng là thứ đang quyết chiều cao header.
      const headerBox = (await header.boundingBox())!;
      const expectedHeight = compact ? HEADER_HEIGHT_COMPACT : HEADER_HEIGHT_TALL;
      expect(Math.round(headerBox.height), `${label}: header phải cao ${expectedHeight}px`).toBe(
        expectedHeight,
      );

      // 2. Ảnh cờ phải tải được thật, không phải thẻ <img> hỏng. Phải CHỜ chứ
      //    không hỏi một lần: lúc phần tử vừa hiện ra thì tệp ảnh thường còn
      //    đang tải, hỏi ngay sẽ đỏ oan.
      await expect
        .poll(
          () =>
            flag.evaluate(
              (element: HTMLImageElement) => element.complete && element.naturalWidth > 0,
            ),
          { message: `${label}: ảnh cờ trên header phải tải được` },
        )
        .toBe(true);

      // 3. Cờ nằm TRỌN trong header — chỗ T59 và T60 gặp nhau: cờ cao 28px
      //    trong header 48px chỉ còn 10px đệm mỗi bên, hụt một chút là cờ tràn
      //    ra ngoài nền đỏ và lòi sang vùng nội dung.
      const flagBox = (await flag.boundingBox())!;
      expect(flagBox.y, `${label}: cờ thò lên trên header`).toBeGreaterThanOrEqual(headerBox.y - 1);
      expect(flagBox.y + flagBox.height, `${label}: cờ thò xuống dưới header`).toBeLessThanOrEqual(
        headerBox.y + headerBox.height + 1,
      );

      // 4. Cờ rộng hơn logo vuông cũ: hai cụm chữ bên phải không được bị đè.
      const brandBox = (await header.locator('> span').first().boundingBox())!;
      const unitBox = (await app.locator('.hhd-header__unit').boundingBox())!;
      const metaBox = (await app.locator('.hhd-header__meta').boundingBox())!;

      expect(
        overlapsHorizontally(brandBox, unitBox),
        `${label}: cờ và tên hệ thống đè tên đơn vị`,
      ).toBe(false);
      expect(overlapsHorizontally(unitBox, metaBox), `${label}: tên đơn vị đè ngày và nút`).toBe(
        false,
      );

      // 5. Header không tràn ngang, nút Đăng xuất ở tận cùng phải vẫn thấy được.
      const overflow = await header.evaluate(
        (element) => element.scrollWidth > element.clientWidth + 1,
      );
      expect(overflow, `${label}: header tràn ngang`).toBe(false);
      await expect(app.locator('.hhd-header__logout')).toBeInViewport();

      await app.screenshot({ path: path.join(SHOT_DIR, `t61-header-${label}.png`) });
    });
  });
}
