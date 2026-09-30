import type { IsoDateTime } from '../../types/domain';
import { noteDateLabel } from '../../utils/note';
import './notes.css';

/**
 * Viên vàng `Ghi ngày dd/MM/yyyy` — MỘT kiểu duy nhất cho mọi chỗ hiện ngày ghi:
 * hộp ghi chú của một người, form Thêm/Sửa và từng mục trong hộp đầy đủ.
 *
 * Không tô khác màu cho ghi chú của năm trước: năm đã nằm ngay trong ngày, tô
 * thêm một màu nữa chỉ khiến người đọc phải học thêm một quy ước (quyết định
 * của CEO ở HUYH-82).
 */
export function NoteDatePill({ value }: { value: IsoDateTime | null | undefined }) {
  const label = noteDateLabel(value);
  if (!label) return null;
  return (
    <span className="hhd-note-pill" data-testid="note-date-pill">
      {label}
    </span>
  );
}
