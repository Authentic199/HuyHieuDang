import { Input } from 'antd';

import type { IsoDateTime } from '../../types/domain';
import { NOTE_MAX_LENGTH } from '../../utils/note';
import { NoteDatePill } from './NoteDatePill';
import './notes.css';

/**
 * Ô nhập ghi chú: vùng nhập nhiều dòng, dưới nó là viên ngày ghi bên trái và bộ
 * đếm `x / 500` bên phải. Dùng chung cho hộp `Ghi chú — <Họ tên>` (UC-26) và ô
 * Ghi chú ở cuối form Thêm/Sửa (UC-21, UC-22).
 *
 * `value` và `onChange` để trống được — `Form.Item` của Ant Design tự truyền
 * vào khi ô này nằm trong form.
 *
 * Bộ đếm tự dựng chứ không dùng `showCount` của Ant Design: `showCount` đặt số
 * đếm ở một góc riêng, không xếp cùng hàng với viên ngày ghi được.
 */

interface NoteFieldProps {
  value?: string;
  onChange?: (value: string) => void;
  /** Ngày ghi đang lưu; null thì hàng dưới chỉ có bộ đếm */
  noteUpdatedAt?: IsoDateTime | null;
  rows?: number;
  autoFocus?: boolean;
  disabled?: boolean;
}

export function NoteField({
  value = '',
  onChange,
  noteUpdatedAt,
  rows = 4,
  autoFocus,
  disabled,
}: NoteFieldProps) {
  return (
    <>
      <Input.TextArea
        className="hhd-note-input"
        value={value}
        onChange={(event) => onChange?.(event.target.value)}
        // Chặn gõ quá 500 ngay ở ô nhập; máy chủ vẫn kiểm lại lần nữa khi lưu.
        maxLength={NOTE_MAX_LENGTH}
        rows={rows}
        autoFocus={autoFocus}
        disabled={disabled}
        placeholder="Ví dụ: hồ sơ còn thiếu bản sao quyết định kết nạp"
        aria-label="Ghi chú"
      />
      <div className="hhd-note-field">
        <NoteDatePill value={noteUpdatedAt} />
        <span className="hhd-note-field__count" data-testid="note-counter">
          {value.length} / {NOTE_MAX_LENGTH}
        </span>
      </div>
    </>
  );
}
