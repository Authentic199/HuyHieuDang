import flagUrl from '../assets/co-dang-co-to-quoc.png';

/**
 * Ảnh cờ Đảng và cờ Tổ quốc, dùng đúng tệp chủ dự án gửi, không vẽ lại.
 * Sinh từ `docs/logo/co-dang-co-to-quoc-goc.png` — xem `docs/logo/tao-anh-co.py`.
 */

/** Bề ngang chia bề cao của tệp ảnh (498x276). Cờ nằm ngang chứ không vuông. */
const ASPECT_RATIO = 498 / 276;

/**
 * `size` là CHIỀU CAO tính bằng px; bề ngang tự suy theo tỷ lệ ảnh.
 * Thanh đầu trang 28px, trang đăng nhập 32px, trạng thái trống 64px.
 */
export function Logo({ size = 28, decorative = false }: { size?: number; decorative?: boolean }) {
  const width = Math.round(size * ASPECT_RATIO);

  return (
    <img
      src={flagUrl}
      alt={decorative ? '' : 'Cờ Đảng và cờ Tổ quốc'}
      aria-hidden={decorative || undefined}
      width={width}
      height={size}
      style={{ width, height: size, objectFit: 'contain', flex: 'none', display: 'block' }}
    />
  );
}
