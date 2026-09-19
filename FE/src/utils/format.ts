import dayjs from 'dayjs';
import 'dayjs/locale/vi';

import type { Gender, IsoDate } from '../types/domain';

dayjs.locale('vi');

/** Dấu hiển thị cho ô không có dữ liệu (quy tắc cứng số 7). */
export const EMPTY_MARK = '—';

/** Ngày đầy đủ: 15/10/2026 */
export function formatDate(value: IsoDate | null | undefined): string {
  if (!value) return EMPTY_MARK;
  const parsed = dayjs(value);
  return parsed.isValid() ? parsed.format('DD/MM/YYYY') : EMPTY_MARK;
}

/** Ngày/tháng của đợt, không có năm: 07/11 */
export function formatDayMonth(day: number, month: number): string {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${pad(day)}/${pad(month)}`;
}

/** Số theo kiểu Việt Nam, dấu chấm ngăn nghìn: 1.248 */
export function formatNumber(value: number | null | undefined): string {
  if (value === null || value === undefined || Number.isNaN(value)) return EMPTY_MARK;
  return new Intl.NumberFormat('vi-VN').format(value);
}

/** Male → Nam, Female → Nữ, null → — */
export function formatGender(value: Gender | null | undefined): string {
  if (value === 'Male') return 'Nam';
  if (value === 'Female') return 'Nữ';
  return EMPTY_MARK;
}

/** Chuỗi bất kỳ; rỗng thì trả dấu gạch ngang. */
export function orEmptyMark(value: string | null | undefined): string {
  const text = value?.trim();
  return text ? text : EMPTY_MARK;
}

/**
 * Dòng "Hôm nay 19/09/2026" trên thanh đầu trang.
 *
 * Ngày hôm nay lấy từ MÁY CHỦ (`serverDate`), không lấy từ đồng hồ trình duyệt
 * — mọi phép tính mốc tuổi đảng đều theo lịch máy chủ (mục 1.6 hợp đồng API).
 * Chỉ khi chưa có ngày máy chủ mới tạm dùng đồng hồ máy để header không trống.
 */
export function todayLabel(serverDate: IsoDate | null | undefined): string {
  const source = serverDate ? dayjs(serverDate) : dayjs();
  const day = source.isValid() ? source : dayjs();
  return `Hôm nay ${day.format('DD/MM/YYYY')}`;
}

/** Chuyển giá trị của DatePicker về `yyyy-MM-dd` cho API. */
export function toIsoDate(value: dayjs.Dayjs | null | undefined): IsoDate | null {
  return value ? value.format('YYYY-MM-DD') : null;
}

/** Chuyển `yyyy-MM-dd` của API về giá trị cho DatePicker. */
export function fromIsoDate(value: IsoDate | null | undefined): dayjs.Dayjs | null {
  if (!value) return null;
  const parsed = dayjs(value);
  return parsed.isValid() ? parsed : null;
}
