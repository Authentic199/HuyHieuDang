import { font, neutral, radius } from '../theme/tokens';
import { Logo } from './Logo';

/**
 * Cờ kèm tên hệ thống, dùng ở header và trang Đăng nhập.
 *
 * Cờ luôn nằm trong một ô nền trắng bo góc: cả hai nơi dùng đều là nền đỏ, mà
 * cờ cũng đỏ — đặt thẳng lên thì thân cờ chìm mất, chỉ còn ngôi sao và búa liềm.
 */
export function BrandMark({
  logoSize = 28,
  fontSize = 18,
}: {
  logoSize?: number;
  fontSize?: number;
}) {
  return (
    <span style={{ display: 'flex', alignItems: 'center', gap: 10, minWidth: 0 }}>
      <span
        style={{
          display: 'flex',
          alignItems: 'center',
          background: neutral.white,
          borderRadius: radius.control,
          padding: '3px 5px',
          flex: 'none',
        }}
      >
        <Logo size={logoSize} />
      </span>
      <span
        style={{
          font: `700 ${fontSize}px/${fontSize + 6}px ${font.serif}`,
          whiteSpace: 'nowrap',
        }}
      >
        Huy Hiệu Đảng
      </span>
    </span>
  );
}
