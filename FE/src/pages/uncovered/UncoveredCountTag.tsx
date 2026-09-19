import { statusBadge } from '../../theme/tokens';
import { formatNumber } from '../../utils/format';

/**
 * Viên thuốc "N người" cạnh tiêu đề thẻ bảng, tông đỏ nhạt như artboard
 * "Màn 6 — Chưa thuộc đợt nào": số này là số người đang bị sót, cần nổi lên.
 *
 * Năm nào không sót ai thì đổi sang tông xám: "0 người" là tin mừng, không
 * phải cảnh báo.
 */
export function UncoveredCountTag({ count }: { count: number }) {
  const tone = count > 0 ? statusBadge.invalid : statusBadge.past;

  return (
    <span
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: 6,
        height: 24,
        padding: '0 10px',
        borderRadius: 9999,
        whiteSpace: 'nowrap',
        background: tone.background,
        color: tone.text,
        // Artboard dùng 12px; để 14px theo quy tắc chữ tối thiểu của CLAUDE.md.
        font: "600 14px/20px 'Noto Sans', sans-serif",
      }}
    >
      <i
        style={{
          width: 6,
          height: 6,
          borderRadius: '50%',
          background: tone.dot ?? 'transparent',
          flex: 'none',
        }}
      />
      {formatNumber(count)} người
    </span>
  );
}
