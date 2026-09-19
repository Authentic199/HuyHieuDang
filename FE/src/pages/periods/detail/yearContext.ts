import { statusBadge } from '../../../theme/tokens';

/**
 * Ba năm của bộ chọn năm ở tab Danh sách đủ điều kiện (UC-34) và nhãn ngữ cảnh
 * đi kèm. Năm giữa là năm hiện tại của MÁY CHỦ, không lấy từ trình duyệt.
 */

export type YearContext = 'Previous' | 'Current' | 'Next';

/** Ba năm liền kề, luôn theo thứ tự tăng dần như artboard: 2025 · 2026 · 2027. */
export function yearsAround(serverYear: number): number[] {
  return [serverYear - 1, serverYear, serverYear + 1];
}

export function yearContextOf(year: number, serverYear: number): YearContext {
  if (year < serverYear) return 'Previous';
  if (year > serverYear) return 'Next';
  return 'Current';
}

/** Màu viên thuốc "Năm sau" lấy nguyên từ artboard "Màn 5 — Chi tiết đợt". */
const nextYearBadge = { background: '#e3ecfb', text: '#1d4b8f', dot: '#5b8fd8' } as const;

/** Nhãn ngữ cảnh: chữ và màu của viên thuốc cạnh khoảng ngày. */
export function yearContextBadge(context: YearContext) {
  if (context === 'Next') return { label: 'Năm sau', ...nextYearBadge };
  // Năm nay dùng tông "Đang diễn ra", năm trước dùng tông "Đã qua" của artboard 5
  // để ba năm đọc cùng một ngôn ngữ với cột Trạng thái bên danh sách đợt.
  if (context === 'Current') return { label: 'Năm nay', ...statusBadge.ongoing };
  return { label: 'Năm trước', ...statusBadge.past };
}
