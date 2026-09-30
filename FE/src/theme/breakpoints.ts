/**
 * Hai ngưỡng khung nhìn của responsive mức 1 (chỉ máy tính, từ 1280x600).
 *
 * Giữ ở một nơi vì mỗi ngưỡng được dùng ở hai chỗ: khối `@media` trong
 * `theme/responsive.css` và đoạn JavaScript đọc `matchMedia` — sửa lệch nhau
 * thì CSS và token Ant Design rơi vào hai trạng thái khác nhau.
 */

/**
 * Cửa sổ thấp hơn chiều cao khung của bộ thiết kế (900 px) thì bật thang gọn:
 * cỡ chữ, chiều cao nút và khoảng đệm lùi một nấc.
 */
export const COMPACT_VIEWPORT_QUERY = '(max-height: 899px)';

/**
 * Cửa sổ hẹp hơn bề ngang khung của bộ thiết kế (1440 px) thì menu trái mặc
 * định thu về dải biểu tượng để bảng còn chỗ hiện hết cột.
 */
export const NARROW_VIEWPORT_QUERY = '(max-width: 1439px)';
