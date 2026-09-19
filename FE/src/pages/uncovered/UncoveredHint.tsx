import { WarningFilled } from '@ant-design/icons';
import { Alert, Button } from 'antd';

import { formatNumber } from '../../utils/format';

/**
 * Khối gợi ý của màn "Chưa thuộc đợt nào" (UC-40) — artboard 6: thẻ trắng viền
 * xám, dấu tam giác vàng, một câu nói rõ phải nới đợt nào, kèm nút sang màn Đợt.
 *
 * Chỉ dựng khi còn người bị sót; năm nào phủ kín thì không nhắc gì cả.
 *
 * Artboard viết "giữa hai đợt" vì ví dụ chỉ có loại khoảng trống ở giữa; bản
 * chạy thật còn "Trước đợt đầu tiên" và "Sau đợt cuối cùng" nên CEO chốt đổi
 * thành "giữa các đợt" cho đúng mọi trường hợp.
 */
export function UncoveredHint({
  count,
  onGoToPeriods,
}: {
  count: number;
  onGoToPeriods: () => void;
}) {
  return (
    <Alert
      type="warning"
      className="hhd-uncovered__hint"
      icon={<WarningFilled style={{ color: '#7d5414', fontSize: 20 }} />}
      showIcon
      message={
        <span style={{ fontSize: 14, lineHeight: '22px' }}>
          {formatNumber(count)} người dưới đây rơi vào khoảng trống giữa các đợt. Nới{' '}
          <b>Đến ngày</b> của đợt trước hoặc <b>Từ ngày</b> của đợt sau là đủ.
        </span>
      }
      action={
        <Button type="link" className="hhd-uncovered__hint-action" onClick={onGoToPeriods}>
          Điều chỉnh đợt →
        </Button>
      }
    />
  );
}
