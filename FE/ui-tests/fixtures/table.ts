import { expect, type Locator, type Page } from '@playwright/test';

/**
 * Cách đọc một bảng trên màn hình: dòng, ô, thanh phân trang, ô tìm, ô lọc mốc.
 * Định vị theo `aria-label` và vai trò, không theo chữ in trên nút — màn hình
 * đổi câu chữ thì ca kiểm thử không vỡ.
 */
export class TableView {
  readonly page: Page;
  /** Thẻ trắng bao bảng, ví dụ `.hhd-dashboard__panel` */
  readonly panel: Locator;

  constructor(page: Page, panel: Locator) {
    this.page = page;
    this.panel = panel;
  }

  get rows(): Locator {
    return this.panel.locator('.ant-table-tbody tr.ant-table-row');
  }

  /** Vùng cuộn của thân bảng — chính chỗ trước đây cắt mất dòng. */
  get scroller(): Locator {
    return this.panel.locator('.hhd-table-scroll .ant-table-content');
  }

  get pagination(): Locator {
    return this.panel.locator('.ant-pagination');
  }

  /**
   * Cụm đếm "1–20 / 320" bên trái thanh phân trang. Chủ dự án đã cho bỏ ở T49,
   * nên ở mọi bảng nó phải KHÔNG tồn tại — xem `expectNoTotalText`.
   */
  get totalText(): Locator {
    return this.panel.locator('.ant-pagination-total-text');
  }

  /** Chữ đang hiện trong ô "… / trang", ví dụ "20 / trang". */
  get pageSizeText(): Locator {
    return this.pagination.locator('.ant-pagination-options .ant-select-selection-item');
  }

  /** Số trang cuối trên thanh phân trang — Ant Design luôn in trang cuối. */
  get lastPageItem(): Locator {
    return this.pagination.locator('.ant-pagination-item').last();
  }

  get search(): Locator {
    return this.panel.locator('.hhd-table-filters__search input');
  }

  get milestoneSelect(): Locator {
    return this.panel.locator('.hhd-table-filters__milestone');
  }

  /** Ô thứ `column` (đếm từ 0) của dòng thứ `row`. */
  cell(row: number, column: number): Locator {
    return this.rows.nth(row).locator('td').nth(column);
  }

  /** Chữ của cả một cột trên trang đang xem. */
  columnTexts(column: number): Promise<string[]> {
    return this.rows.locator(`td:nth-child(${column + 1})`).allInnerTexts();
  }

  /**
   * Mọi tooltip đang hiện của Ant Design — cả bảng lẫn nơi khác. Tooltip nằm ở
   * lớp phủ gắn vào `body` nên phải tìm từ trang, không tìm trong thẻ bảng.
   */
  private get openTooltips(): Locator {
    return this.page.locator('.ant-tooltip:not(.ant-tooltip-hidden)');
  }

  /**
   * Đưa chuột ra khỏi bảng rồi chờ tooltip ẩn hẳn.
   *
   * Danh sách thả xuống của ô lọc mốc đè lên hàng tiêu đề bảng, nên bấm chọn
   * một mục xong là chuột nằm lại ngay trên một tiêu đề cột có sắp xếp. Ant
   * Design hiện tooltip "Nhấp để sắp xếp tăng dần" đúng chỗ ô lọc, và vì chuột
   * không tự rời đi nên tooltip đó chặn cú bấm tiếp theo cho tới khi hết giờ.
   * Tooltip hiện kịp hay không tùy thời điểm — đó là lý do ca kiểm thử chập
   * chờn. Gọi hàm này sau mỗi thao tác có thể để chuột nằm lại trên bảng.
   */
  private async leaveTable(): Promise<void> {
    await this.page.mouse.move(0, 0);
    await expect(this.openTooltips).toHaveCount(0);
  }

  async goToPage(pageNumber: number): Promise<void> {
    await this.pagination.locator(`.ant-pagination-item-${pageNumber}`).click();
    await this.leaveTable();
  }

  /**
   * Một lựa chọn đang mở của Select. Danh sách dài thì Ant Design dùng danh sách ảo:
   * các phần tử mang `role="option"` nằm trong một lớp đo kích thước 0x0 bị ẩn, còn
   * dòng bấm được thật lại không có vai trò nào — nên định vị theo `title`.
   */
  private openOption(title: string): Locator {
    return this.page.locator(
      `.ant-select-dropdown:not(.ant-select-dropdown-hidden) .ant-select-item-option[title="${title}"]`,
    );
  }

  async choosePageSize(size: number): Promise<void> {
    await this.pagination.locator('.ant-select').click();
    await this.openOption(`${size} / trang`).click();
    await this.leaveTable();
  }

  async chooseMilestone(label: string): Promise<void> {
    await this.milestoneSelect.click();
    await this.openOption(label).click();
    await this.leaveTable();
  }

  async clickSort(columnTitle: string): Promise<void> {
    await this.panel.getByRole('columnheader', { name: columnTitle }).click();
    await this.leaveTable();
  }

  /**
   * Thay cho cụm đếm đã bỏ: trang đang xem có đúng số dòng, và trang cuối đúng
   * bằng tổng chia cỡ trang. Hai điều đó cùng nói lên tổng vẫn được đếm đúng mà
   * không cần in con số nào ra chân bảng.
   */
  async expectPage(rowsOnPage: number, total: number, pageSize: number): Promise<void> {
    await expect(this.rows).toHaveCount(rowsOnPage);
    await expect(this.lastPageItem).toHaveText(String(Math.ceil(total / pageSize)));
  }

  /** Chân bảng KHÔNG được có cụm "1–20 / 1.342" (T49, hồi quy ở T58). */
  async expectNoTotalText(): Promise<void> {
    await expect(this.totalText).toHaveCount(0);
  }

  /** Ô "… / trang" đang ở cỡ nào — cỡ trang mặc định của bảng. */
  async expectPageSize(size: number): Promise<void> {
    await expect(this.pageSizeText).toHaveText(`${size} / trang`);
  }
}
