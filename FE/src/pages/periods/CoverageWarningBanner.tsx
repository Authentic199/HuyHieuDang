import { WarningFilled } from '@ant-design/icons';
import { Alert } from 'antd';

import type { CoverageWarnings } from '../../types/domain';

/**
 * Banner cảnh báo chồng lấn / chưa phủ kín (UC-33) — artboard 5.
 *
 * Cảnh báo KHÔNG chặn lưu (QT6): chỉ nói cho cán bộ biết hệ quả, bằng câu chữ
 * đời thường. Không có cảnh báo nào thì không dựng gì cả.
 */

/** Liệt kê các khoảng theo đúng lối viết "04/02–28/02 · 20/05–30/06" của artboard. */
function joinRanges(ranges: { fromDisplay: string; toDisplay: string }[]): string {
  return ranges.map((range) => `${range.fromDisplay}–${range.toDisplay}`).join(' · ');
}

export function CoverageWarningBanner({ warnings }: { warnings: CoverageWarnings }) {
  const { overlaps, gaps } = warnings;
  if (overlaps.length === 0 && gaps.length === 0) return null;

  return (
    <Alert
      type="warning"
      className="hhd-periods__banner"
      icon={<WarningFilled style={{ color: '#7d5414', fontSize: 20 }} />}
      showIcon
      message={
        <div style={{ display: 'grid', gap: 6, fontSize: 14, lineHeight: '22px' }}>
          {overlaps.length > 0 ? (
            <div>
              <b>Có đợt chồng lấn nhau</b> — {joinRanges(overlaps)}
              {' ('}
              {overlaps
                .map((item) => `${item.firstPeriodName} và ${item.secondPeriodName}`)
                .join(' · ')}
              {') '}— người tròn mốc trong các khoảng này nằm trong cả hai đợt.
            </div>
          ) : null}
          {gaps.length > 0 ? (
            <div>
              <b>Các đợt chưa phủ kín cả năm</b> — trống {joinRanges(gaps)} — người tròn mốc trong
              các khoảng này sẽ xuất hiện ở “Chưa thuộc đợt nào”.
            </div>
          ) : null}
        </div>
      }
    />
  );
}
