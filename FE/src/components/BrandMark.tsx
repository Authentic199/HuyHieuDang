import { font } from '../theme/tokens';
import { Logo } from './Logo';

/**
 * Cờ kèm tên hệ thống, dùng ở header và trang Đăng nhập.
 *
 * Cờ đặt THẲNG lên nền đỏ, không lót ô nền trắng: nền là đỏ sẫm (#8f1619 →
 * #530b0e) còn cờ là đỏ tươi (#D9251C) nên thân cờ vẫn nổi rõ, trong khi ô
 * trắng trông như miếng dán. Quyết định của CEO ngày 30/09 sau khi dựng thử
 * trên nền thật.
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
      <Logo size={logoSize} />
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
