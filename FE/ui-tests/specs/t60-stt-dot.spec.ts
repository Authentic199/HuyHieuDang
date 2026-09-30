import { expect, test } from '../fixtures/app';

/**
 * Ca chặn cho cột STT của bảng Đợt trao huy hiệu.
 *
 * Bảng này từng tính số thứ tự bằng `index + 1`, tức chỉ số bên trong TRANG
 * đang xem, nên sang trang 2 số lại đếm từ 1 — người dùng đọc thành "đợt số 1"
 * hai lần trong cùng một danh sách. Bốn bảng còn lại đều đánh nối tiếp qua các
 * trang; ca này giữ cho bảng Đợt không tụt lại lần nữa.
 *
 * Không dùng `fixtures/table.ts` (thuộc HUYH-64) để ca này đứng một mình.
 */

/** Cỡ trang mặc định của mọi bảng — xem `DEFAULT_TABLE_PAGE_SIZE`. */
const PAGE_SIZE = 20;

const PANEL = '.hhd-periods__panel';

test('bảng Đợt đánh số thứ tự nối tiếp qua các trang', async ({ app }) => {
  await app.goto('/dot-trao-huy-hieu');

  const rows = app.locator(`${PANEL} .ant-table-tbody > tr.ant-table-row`);
  const firstIndexCell = rows.first().locator('td.hhd-periods__index');

  await expect(rows).toHaveCount(PAGE_SIZE);
  await expect(firstIndexCell).toHaveText('1');

  await app.locator(`${PANEL} .ant-pagination .ant-pagination-item-2`).click();

  // Dòng đầu trang 2 mang số 21, không quay về 1.
  await expect(firstIndexCell).toHaveText(String(PAGE_SIZE + 1));

  // Cả cột chạy liền mạch 21, 22, 23… chứ không chỉ đúng mỗi dòng đầu.
  const indexes = await rows.locator('td.hhd-periods__index').allInnerTexts();
  expect(indexes.map((text) => text.trim())).toEqual(
    indexes.map((_text, position) => String(PAGE_SIZE + 1 + position)),
  );
});
