import { Empty } from 'antd';

import { neutral, radius } from '../theme/tokens';
import { Logo } from './Logo';

/**
 * Khối giữ chỗ cho các màn chưa dựng nội dung (task T00B chỉ dựng khung).
 * Mỗi màn sẽ được thay bằng nội dung thật ở task riêng của nó.
 */
export function PagePlaceholder({ screen }: { screen: string }) {
  return (
    <div
      style={{
        flex: 1,
        background: neutral.white,
        border: `1px solid ${neutral.border}`,
        borderRadius: radius.card,
        display: 'grid',
        placeItems: 'center',
        padding: 64,
      }}
    >
      <Empty
        image={<Logo size={64} />}
        imageStyle={{ height: 64, display: 'grid', placeItems: 'center' }}
        description={
          <span style={{ color: neutral.textSecondary, fontSize: 16, lineHeight: '24px' }}>
            Màn {screen} sẽ có nội dung ở bước tiếp theo.
          </span>
        }
      />
    </div>
  );
}
