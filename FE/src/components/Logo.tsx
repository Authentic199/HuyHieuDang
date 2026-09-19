import logoUrl from '../assets/logo-huyhieudang.png';

/**
 * Logo dùng đúng tệp gốc của bộ thiết kế, không vẽ lại.
 * Thanh đầu trang 30px, trang đăng nhập 44px, trạng thái trống 64px, tối thiểu 20px.
 */
export function Logo({ size = 30, decorative = false }: { size?: number; decorative?: boolean }) {
  return (
    <img
      src={logoUrl}
      alt={decorative ? '' : 'Huy Hiệu Đảng'}
      aria-hidden={decorative || undefined}
      width={size}
      height={size}
      style={{ width: size, height: size, objectFit: 'contain', flex: 'none', display: 'block' }}
    />
  );
}
