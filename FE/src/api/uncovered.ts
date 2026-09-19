import type { UnassignedGap, UnassignedMemberResponse } from '../types/domain';
import { apiClient } from './httpClient';

/** UC-40 — danh sách người tròn mốc trong năm nhưng không rơi vào đợt nào. */
export interface UnassignedResponse {
  year: number;
  totalCount: number;
  members: UnassignedMemberResponse[];
}

/** Bỏ trống `year` thì máy chủ dùng năm hiện tại. */
export function getUnassigned(year?: number): Promise<UnassignedResponse> {
  return apiClient.get<UnassignedResponse>('/Eligibility/Unassigned', { params: { year } });
}

/**
 * Endpoint nhẹ dành riêng cho badge trên menu trái. Gọi khi vào ứng dụng và
 * gọi lại sau mỗi thao tác đổi dữ liệu. count = 0 thì ẩn badge.
 */
export function getUnassignedCount(year?: number): Promise<{ year: number; count: number }> {
  return apiClient.get<{ year: number; count: number }>('/Eligibility/UnassignedCount', {
    params: { year },
  });
}

/** Chữ cho cột "Khoảng trống" theo đúng khuôn của hợp đồng. */
export function gapLabel(gap: UnassignedGap): string {
  switch (gap.type) {
    case 'Between':
      return `Giữa ${gap.previousPeriodName} và ${gap.nextPeriodName}`;
    case 'BeforeFirst':
      return 'Trước đợt đầu tiên';
    case 'AfterLast':
      return 'Sau đợt cuối cùng';
  }
}
