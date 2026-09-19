import { messageText } from '../../api/messages';

/**
 * Ràng buộc QT1 phía giao diện cho màn Cài đặt.
 *
 * Ở đây CHỈ kiểm tra ba số có hợp lệ hay không để chặn lời gọi vô ích và bật
 * tắt nút Lưu. Dãy mốc luôn do máy chủ sinh (`GET /api/Settings` →
 * `milestones`, `GET /api/Settings/Milestones` khi xem trước) — QT1 nằm một
 * chỗ duy nhất ở Backend, giao diện không cài lại công thức.
 */

/** Số viên thuốc tối đa vẽ ra màn hình; phần còn lại gộp vào một dòng chữ. */
export const PREVIEW_PILL_LIMIT = 60;

/** Hoãn sau mỗi lần gõ rồi mới hỏi máy chủ dãy mốc (mục 7.4 hợp đồng API). */
export const PREVIEW_DEBOUNCE_MS = 300;

export interface MilestoneInput {
  startYears: number | null;
  endYears: number | null;
  stepYears: number | null;
}

/** Lỗi của từng ô; ô nào không có lỗi thì khuyết. */
export type MilestoneErrors = Partial<Record<keyof MilestoneInput, string>>;

/** Ràng buộc QT1 dưới dạng câu tiếng Việt, dùng chung cho cả ba ô. */
export const milestoneMessages = {
  requiredStart: 'Chưa nhập Mốc bắt đầu',
  requiredEnd: 'Chưa nhập Mốc kết thúc',
  requiredStep: 'Chưa nhập Bước',
  /** Bốn khóa lỗi của máy chủ — giữ nguyên câu chữ để hai bên nói giống nhau. */
  invalidStart: messageText('Mes.AppSetting.Invalid.StartYears'),
  invalidEnd: messageText('Mes.AppSetting.Invalid.EndYears'),
  invalidStep: messageText('Mes.AppSetting.Invalid.StepYears'),
  invalidRange: messageText('Mes.AppSetting.Invalid.Range'),
  saved: messageText('Mes.AppSetting.Update.Successfully'),
  unitNameTooLong: 'Tên đơn vị chỉ được tối đa 200 ký tự',
} as const;

/** Số nguyên dương, không nhận số thập phân hay số âm. */
function isPositiveInteger(value: number | null): value is number {
  return value !== null && Number.isInteger(value) && value >= 1;
}

/**
 * Cả ba là số nguyên ≥ 1 và Bắt đầu ≤ Kết thúc (QT1). Lỗi "Bắt đầu phải nhỏ
 * hơn hoặc bằng Kết thúc" gắn vào ô Kết thúc — đó là ô người dùng vừa gõ sai
 * trong phần lớn trường hợp.
 */
export function validateMilestoneInput(input: MilestoneInput): MilestoneErrors {
  const errors: MilestoneErrors = {};
  const { startYears, endYears, stepYears } = input;

  if (startYears === null) errors.startYears = milestoneMessages.requiredStart;
  else if (!isPositiveInteger(startYears)) errors.startYears = milestoneMessages.invalidStart;

  if (endYears === null) errors.endYears = milestoneMessages.requiredEnd;
  else if (!isPositiveInteger(endYears)) errors.endYears = milestoneMessages.invalidEnd;

  if (stepYears === null) errors.stepYears = milestoneMessages.requiredStep;
  else if (!isPositiveInteger(stepYears)) errors.stepYears = milestoneMessages.invalidStep;

  if (!errors.startYears && !errors.endYears && (endYears as number) < (startYears as number)) {
    errors.endYears = milestoneMessages.invalidRange;
  }

  return errors;
}

/** Câu lỗi đầu tiên theo thứ tự ba ô trên màn — dùng cho ô xem trước. */
export function firstMilestoneError(errors: MilestoneErrors): string | null {
  return errors.startYears ?? errors.endYears ?? errors.stepYears ?? null;
}
