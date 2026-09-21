/**
 * Số theo kiểu Việt Nam, dấu chấm ngăn nghìn — cùng cách với
 * `FE/src/utils/format.ts`. Ca kiểm thử cố ý KHÔNG import từ `src/` để không
 * kéo theo cả cây phụ thuộc của ứng dụng vào tiến trình Node của Playwright.
 */
export function formatNumber(value: number): string {
  return new Intl.NumberFormat('vi-VN').format(value);
}
