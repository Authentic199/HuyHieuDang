import { statusBadge } from '../../theme/tokens';
import type { AwardPeriodResponse } from '../../types/domain';
import { formatNumber } from '../../utils/format';

/**
 * Trạng thái của đợt trong năm đang xét (QT11) — viên thuốc có chấm màu, đúng
 * artboard 5. Chữ lấy theo `status`, riêng "Sắp tới" kèm số ngày còn lại.
 */
export function PeriodStatusTag({ period }: { period: AwardPeriodResponse }) {
  const tone =
    period.status === 'Ongoing'
      ? statusBadge.ongoing
      : period.status === 'Upcoming'
        ? statusBadge.upcoming
        : statusBadge.past;

  const label =
    period.status === 'Past'
      ? 'Đã qua'
      : period.status === 'Ongoing'
        ? 'Đang diễn ra'
        : period.daysRemaining === null
          ? 'Sắp tới'
          : `Sắp tới · ${formatNumber(period.daysRemaining)} ngày`;

  return (
    <span
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: 6,
        height: 'var(--hhd-tag-height)',
        padding: '0 10px',
        borderRadius: 9999,
        whiteSpace: 'nowrap',
        background: tone.background,
        color: tone.text,
        font: "600 var(--hhd-fs-14)/var(--hhd-lh-14) 'Noto Sans', sans-serif",
      }}
    >
      {tone.dot ? (
        <i
          style={{
            width: 6,
            height: 6,
            borderRadius: '50%',
            background: tone.dot,
            flex: 'none',
          }}
        />
      ) : null}
      {label}
    </span>
  );
}
