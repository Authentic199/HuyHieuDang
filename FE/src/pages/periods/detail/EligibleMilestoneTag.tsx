import { milestoneScale, shadow } from '../../../theme/tokens';

/**
 * Viên thuốc "N năm" của cột Mốc huy hiệu, màu lấy từ thang "Cool Waters"
 * trong artboard 0 — cùng cách dựng với viên thuốc mốc bên màn Đảng viên.
 *
 * Mốc lẻ dùng bậc của chục liền dưới: 35 năm dùng bậc 30, 45 năm dùng bậc 40.
 * Ở màn này mốc luôn có giá trị: mỗi dòng là một người TRÒN mốc trong đợt.
 */
export function EligibleMilestoneTag({ milestone }: { milestone: number }) {
  let step: (typeof milestoneScale)[number] = milestoneScale[0];
  for (const candidate of milestoneScale) {
    if (milestone >= candidate.from) step = candidate;
  }

  return (
    <span
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        height: 24,
        padding: '0 10px',
        borderRadius: 9999,
        whiteSpace: 'nowrap',
        background: step.background,
        boxShadow: shadow.pill,
        color: step.text,
        // Artboard dùng 12px; ở đây để 14px theo quy tắc chữ trong bảng không
        // nhỏ hơn 14px cho cán bộ lớn tuổi (quy tắc 1 trong CLAUDE.md).
        font: "600 14px/20px 'Noto Sans', sans-serif",
      }}
    >
      {milestone} năm
    </span>
  );
}
