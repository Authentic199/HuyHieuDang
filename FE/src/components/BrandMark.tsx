import { font } from '../theme/tokens';
import { Logo } from './Logo';

/** Logo kèm tên hệ thống, dùng ở header và trang Đăng nhập. */
export function BrandMark({
  logoSize = 30,
  fontSize = 18,
}: {
  logoSize?: number;
  fontSize?: number;
}) {
  return (
    <span style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
      <Logo size={logoSize} />
      <span style={{ font: `700 ${fontSize}px/${fontSize + 6}px ${font.serif}` }}>
        Huy Hiệu Đảng
      </span>
    </span>
  );
}
