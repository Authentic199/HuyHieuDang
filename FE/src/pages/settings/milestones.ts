import { messageText } from '../../api/messages';

/**
 * Xem trước dãy mốc (QT1) cho màn Cài đặt.
 *
 * Dãy mốc CHÍNH THỨC luôn do máy chủ trả về (`GET/PUT /api/Settings` →
 * `milestones`). Những hàm dưới đây chỉ phục vụ ô "Xem trước dãy mốc": người
 * dùng gõ tới đâu nhìn thấy dãy tới đó, không chờ mạng và không ghi gì.
 * Sau khi bấm Lưu, giao diện bỏ bản xem trước và hiện lại dãy của máy chủ.
 */

/** Giá trị mặc định ghi trên nút Khôi phục (mục 7.3 hợp đồng API). */
export const DEFAULT_MILESTONE_SETTINGS = {
  startYears: 30,
  endYears: 90,
  stepYears: 5,
} as const;

/** Số viên thuốc tối đa vẽ ra màn hình; phần còn lại gộp vào một dòng chữ. */
export const PREVIEW_PILL_LIMIT = 60;

export interface MilestoneInput {
  startYears: number | null;
  endYears: number | null;
  stepYears: number | null;
}

/** Số nguyên dương, không nhận số thập phân hay số âm. */
function isPositiveInteger(value: number | null): value is number {
  return value !== null && Number.isInteger(value) && value >= 1;
}

/**
 * Sinh dãy `{Bắt đầu, Bắt đầu + Bước, …}` cho tới khi vượt Kết thúc (QT1).
 * Trả về mảng rỗng khi bộ số chưa hợp lệ — lúc đó ô xem trước hiện lời nhắc.
 */
export function buildMilestones(input: MilestoneInput): number[] {
  const { startYears, endYears, stepYears } = input;
  if (!isPositiveInteger(startYears) || !isPositiveInteger(endYears)) return [];
  if (!isPositiveInteger(stepYears)) return [];
  if (endYears < startYears) return [];

  const list: number[] = [];
  for (let value = startYears; value <= endYears; value += stepYears) list.push(value);
  return list;
}

/** Ràng buộc QT1 dưới dạng câu tiếng Việt, dùng chung cho cả 3 ô. */
export const milestoneMessages = {
  requiredStart: 'Chưa nhập Mốc bắt đầu',
  requiredEnd: 'Chưa nhập Mốc kết thúc',
  requiredStep: 'Chưa nhập Bước',
  /** Khóa lỗi của máy chủ — giữ nguyên câu chữ để hai bên nói giống nhau. */
  invalidStart: messageText('Mes.AppSetting.Invalid.StartYears'),
  invalidEnd: messageText('Mes.AppSetting.Invalid.EndYears'),
  invalidStep: messageText('Mes.AppSetting.Invalid.StepYears'),
  invalidRange: messageText('Mes.AppSetting.Invalid.Range'),
  unitNameTooLong: 'Tên đơn vị chỉ được tối đa 200 ký tự',
} as const;

/** Một số nguyên dương hay không — dùng cho luật của từng ô. */
export function checkPositiveInteger(value: number | null | undefined, message: string) {
  if (value === null || value === undefined) return null;
  return isPositiveInteger(value) ? null : message;
}
