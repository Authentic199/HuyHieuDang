import { statusBadge } from '../../../theme/tokens';

/**
 * Nhãn ngữ cảnh cạnh khoảng ngày ở màn Chi tiết đợt (UC-34).
 *
 * Từ quyết định 30/09, bộ chọn năm đi được 100 năm mỗi phía nên nhãn phải ghi
 * đúng khoảng cách chứ không chỉ ba chữ năm trước / năm nay / năm sau. Gốc so
 * sánh luôn là năm hiện tại của MÁY CHỦ, không lấy từ trình duyệt.
 */

export type YearContext = 'Previous' | 'Current' | 'Next';

export function yearContextOf(year: number, serverYear: number): YearContext {
  if (year < serverYear) return 'Previous';
  if (year > serverYear) return 'Next';
  return 'Current';
}

/** Số năm cách năm máy chủ — 1 là liền kề, từ 2 trở lên thì nhãn ghi rõ số. */
export function yearDistanceOf(year: number, serverYear: number): number {
  return Math.abs(year - serverYear);
}

/** Màu viên thuốc "Năm sau" lấy nguyên từ artboard "Màn 5 — Chi tiết đợt". */
const nextYearBadge = { background: '#e3ecfb', text: '#1d4b8f', dot: '#5b8fd8' } as const;

/**
 * Chữ và màu của viên thuốc cạnh khoảng ngày. `distance` là số năm cách năm
 * máy chủ; mọi năm tương lai giữ tông xanh của "Năm sau", mọi năm quá khứ giữ
 * tông "Đã qua", nên đọc xa hay gần vẫn cùng một ngôn ngữ màu.
 */
export function yearContextBadge(context: YearContext, distance = 1) {
  if (context === 'Current') return { label: 'Năm nay', ...statusBadge.ongoing };
  if (context === 'Next') {
    return { label: distance <= 1 ? 'Năm sau' : `${distance} năm nữa`, ...nextYearBadge };
  }
  return { label: distance <= 1 ? 'Năm trước' : `${distance} năm trước`, ...statusBadge.past };
}
