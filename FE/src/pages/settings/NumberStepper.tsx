import { Button, InputNumber, Space } from 'antd';

/**
 * Ô số có nút trừ / cộng của artboard 7: ô cao 48px, số ở giữa bằng chữ có
 * chân 20px, hai nút vuông 44px hai bên.
 *
 * Hai nút to để bác cán bộ chỉnh bằng chuột không cần gõ bàn phím; vẫn gõ
 * thẳng vào ô được. Giá trị `null` nghĩa là ô đang trống.
 */
interface NumberStepperProps {
  value?: number | null;
  onChange?: (value: number | null) => void;
  /** Nhãn đọc cho trình đọc màn hình, ví dụ "Bắt đầu (năm)" */
  label: string;
  min?: number;
  disabled?: boolean;
  /** Ô đang có lỗi — viền đỏ do Ant Design tự gắn, ở đây chỉ đổi nút */
  status?: '' | 'error';
}

export function NumberStepper({
  value = null,
  onChange,
  label,
  min = 1,
  disabled,
  status,
}: NumberStepperProps) {
  /** Bấm − / + khi ô đang trống thì bắt đầu từ chính giá trị nhỏ nhất. */
  function step(delta: number) {
    const current = value ?? min;
    const next = Math.max(min, Math.round(current) + delta);
    onChange?.(next);
  }

  return (
    <Space.Compact className="hhd-settings__stepper" block>
      <Button
        className="hhd-settings__stepper-button"
        disabled={disabled || (value !== null && value <= min)}
        onClick={() => step(-1)}
        aria-label={`Giảm ${label}`}
      >
        −
      </Button>
      <InputNumber
        className="hhd-settings__stepper-input"
        value={value}
        onChange={(next) => onChange?.(next === null ? null : Number(next))}
        min={min}
        precision={0}
        controls={false}
        disabled={disabled}
        status={status}
        aria-label={label}
      />
      <Button
        className="hhd-settings__stepper-button"
        disabled={disabled}
        onClick={() => step(1)}
        aria-label={`Tăng ${label}`}
      >
        +
      </Button>
    </Space.Compact>
  );
}
