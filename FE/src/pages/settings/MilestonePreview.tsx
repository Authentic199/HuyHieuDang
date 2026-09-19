import { color, neutral, radius } from '../../theme/tokens';
import { EMPTY_MARK, formatNumber } from '../../utils/format';
import { EligibleMilestoneTag } from '../periods/detail/EligibleMilestoneTag';
import { PREVIEW_PILL_LIMIT } from './milestones';
import type { MilestonePreview as Preview } from './useMilestonePreview';

/**
 * Ô "Xem trước dãy mốc · N mốc" của artboard 7 — nền chìm, viên thuốc mốc
 * dùng chung thang màu với bảng đủ điều kiện.
 *
 * Ba cảnh: có dãy (vẽ viên thuốc), đang chờ máy chủ (giữ dãy cũ, làm mờ để
 * không nháy), và bộ số chưa hợp lệ (một dòng đỏ đúng câu lỗi, số mốc là —).
 */
export function MilestonePreview({ preview }: { preview: Preview }) {
  const { milestones, count, pending, error } = preview;
  const shown = milestones.slice(0, PREVIEW_PILL_LIMIT);
  const hidden = milestones.length - shown.length;

  return (
    <div
      className="hhd-settings__preview"
      style={{ padding: 16, background: neutral.fill, borderRadius: radius.card }}
      aria-busy={pending}
    >
      <div className="hhd-settings__preview-title">
        Xem trước dãy mốc · {count === null ? EMPTY_MARK : `${formatNumber(count)} mốc`}
      </div>

      {error ? (
        <div style={{ color: color.danger, fontSize: 14, lineHeight: '20px' }}>{error}</div>
      ) : (
        <div
          style={{
            display: 'flex',
            flexWrap: 'wrap',
            gap: 8,
            alignItems: 'center',
            // Đang chờ thì mờ đi chứ không xóa — mắt không phải bắt lại từ đầu.
            opacity: pending ? 0.45 : 1,
            transition: 'opacity .15s ease',
          }}
        >
          {shown.map((milestone) => (
            <EligibleMilestoneTag key={milestone} milestone={milestone} />
          ))}
          {hidden > 0 ? (
            <span style={{ color: neutral.textSecondary, fontSize: 14 }}>
              … và {formatNumber(hidden)} mốc nữa
            </span>
          ) : null}
        </div>
      )}
    </div>
  );
}
