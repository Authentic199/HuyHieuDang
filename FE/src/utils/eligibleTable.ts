import {
  compareDate,
  compareNumber,
  compareText,
  type RowComparators,
} from '../hooks/useClientTable';
import type { EligibleMemberResponse } from '../types/domain';
import { formatGender } from './format';

/**
 * Cách tìm và cách sắp xếp của bảng "đủ điều kiện", dùng chung cho ba màn có
 * cùng bộ cột: Dashboard (UC-11), tab Đủ điều kiện của chi tiết đợt (UC-34) và
 * Chưa thuộc đợt nào (UC-40). Một chỗ duy nhất để ba màn không lệch nhau.
 */

/** Ô tìm của ba màn này đều tìm theo họ tên. */
export function nameOfRow(row: EligibleMemberResponse): string {
  return row.fullName;
}

/** Mốc huy hiệu của dòng, cho ô lọc "Mốc: Tất cả". */
export function milestoneOfRow(row: EligibleMemberResponse): number {
  return row.milestone;
}

/** Khóa trùng `key` của từng cột trong bảng. */
export const ELIGIBLE_COMPARATORS: RowComparators<EligibleMemberResponse> = {
  fullName: (a, b) => compareText(a.fullName, b.fullName),
  // Sắp theo chữ đang hiện trên màn hình (Nam trước Nữ), ô trống xuống cuối.
  gender: (a, b) =>
    compareText(a.gender ? formatGender(a.gender) : null, b.gender ? formatGender(b.gender) : null),
  dateOfBirth: (a, b) => compareDate(a.dateOfBirth, b.dateOfBirth),
  officialAdmissionDate: (a, b) => compareDate(a.officialAdmissionDate, b.officialAdmissionDate),
  milestoneDate: (a, b) => compareDate(a.milestoneDate, b.milestoneDate),
  milestone: (a, b) => compareNumber(a.milestone, b.milestone),
};
