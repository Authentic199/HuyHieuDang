/**
 * Quy ước đánh dấu trường bắt buộc, dùng chung cho mọi form nhập.
 *
 * - Dấu `*` đỏ đứng SAU nhãn, đúng bản vẽ "Tên đợt *" của artboard 5.
 * - Chân form có một dòng chú thích ở góc trái giải thích dấu sao đó.
 *
 * Màu lấy từ biến CSS `--hhd-danger` (src/theme/tokens.css), không viết mã màu
 * trực tiếp tại đây.
 */
import type { FormProps } from 'antd';

/**
 * Bộ dựng nhãn cho `<Form requiredMark={...}>`.
 *
 * Dùng: `<Form requiredMark={requiredMark}>`.
 */
// Bộ dựng nhãn không phải component React nên phải tắt cảnh báo fast-refresh;
// giữ chung tệp với RequiredFieldNote để một quy ước chỉ nằm ở một nơi.
// eslint-disable-next-line react-refresh/only-export-components
export const requiredMark: Exclude<FormProps['requiredMark'], boolean | undefined> = (
  label,
  { required },
) => (
  <>
    {label}
    {required ? <span style={{ color: 'var(--hhd-danger)' }}>{' *'}</span> : null}
  </>
);

/**
 * Dòng chú thích đặt ở góc TRÁI chân form, cùng hàng với cụm nút.
 */
export function RequiredFieldNote() {
  return (
    <span
      style={{
        color: 'var(--hhd-text-secondary)',
        fontSize: 13,
        lineHeight: '20px',
      }}
      data-testid="required-field-note"
    >
      <span style={{ color: 'var(--hhd-danger)' }}>*</span> là các trường dữ liệu bắt buộc
    </span>
  );
}
