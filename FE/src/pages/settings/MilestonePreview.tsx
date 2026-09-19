import { neutral, radius } from '../../theme/tokens';
import { formatNumber } from '../../utils/format';
import { EligibleMilestoneTag } from '../periods/detail/EligibleMilestoneTag';
import { PREVIEW_PILL_LIMIT } from './milestones';

interface MilestonePreviewProps {
  milestones: number[];
  /** Câu nhắc khi bộ số chưa hợp lệ nên chưa sinh được dãy nào */
  invalidHint: string | null;
}

/**
 * Ô "Xem trước dãy mốc · N mốc" của artboard 7 — nền chìm, viên thuốc mốc
 * dùng chung thang màu với bảng đủ điều kiện.
 *
 * Dãy này cập nhật ngay khi gõ, chỉ để xem trước. Dãy chính thức là dãy máy
 * chủ trả về sau khi bấm Lưu.
 */
export function MilestonePreview({ milestones, invalidHint }: MilestonePreviewProps) {
  const shown = milestones.slice(0, PREVIEW_PILL_LIMIT);
  const hidden = milestones.length - shown.length;

  return (
    <div
      style={{
        padding: 16,
        background: neutral.fill,
        borderRadius: radius.card,
      }}
    >
      <div
        style={{
          font: "600 12px/16px 'Noto Sans', sans-serif",
          letterSpacing: '.08em',
          textTransform: 'uppercase',
          color: neutral.textSecondary,
          marginBottom: 10,
        }}
      >
        Xem trước dãy mốc
        {milestones.length > 0 ? ` · ${formatNumber(milestones.length)} mốc` : ''}
      </div>

      {milestones.length > 0 ? (
        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8, alignItems: 'center' }}>
          {shown.map((milestone) => (
            <EligibleMilestoneTag key={milestone} milestone={milestone} />
          ))}
          {hidden > 0 ? (
            <span style={{ color: neutral.textSecondary, fontSize: 14 }}>
              … và {formatNumber(hidden)} mốc nữa
            </span>
          ) : null}
        </div>
      ) : (
        <div style={{ color: neutral.textSecondary, fontSize: 14, lineHeight: '20px' }}>
          {invalidHint ?? 'Chưa sinh được mốc nào từ ba số đang nhập.'}
        </div>
      )}
    </div>
  );
}
