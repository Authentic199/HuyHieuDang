import type {
  AwardPeriodResponse,
  CoverageWarnings,
  IsoDate,
  MilestoneBreakdown,
  EligibleMemberResponse,
} from '../types/domain';
import { apiClient } from './httpClient';

/** Một đoạn của dải độ phủ 12 tháng (UC-36). */
export interface CoverageSegment {
  type: 'Period' | 'Gap';
  periodId: string | null;
  name: string | null;
  fromDate: IsoDate;
  toDate: IsoDate;
}

/** UC-30, UC-36 — một lời gọi đủ cho bảng, dải độ phủ và banner cảnh báo. */
export interface AwardPeriodListResponse {
  year: number;
  today: IsoDate;
  totalCount: number;
  /** Sắp theo fromDate tăng dần */
  periods: AwardPeriodResponse[];
  warnings: CoverageWarnings;
  coverage: { segments: CoverageSegment[] };
}

/** UC-31, UC-32 — modal thêm/sửa đợt, chỉ ngày và tháng (QT6). */
export interface AwardPeriodPayload {
  name: string;
  fromDay: number;
  fromMonth: number;
  toDay: number;
  toMonth: number;
}

/** Thêm/sửa trả kèm cảnh báo để giao diện cập nhật banner ngay. */
export interface AwardPeriodMutationResponse {
  period: AwardPeriodResponse;
  warnings: CoverageWarnings;
}

/** UC-34 tab Danh sách đủ điều kiện. */
export interface EligibilityResponse {
  awardPeriod: AwardPeriodResponse;
  year: number;
  totalCount: number;
  milestoneBreakdown: MilestoneBreakdown[];
  members: EligibleMemberResponse[];
}

/** Bỏ trống `year` thì máy chủ dùng năm hiện tại theo lịch của nó. */
export function getPeriods(year?: number): Promise<AwardPeriodListResponse> {
  return apiClient.get<AwardPeriodListResponse>('/AwardPeriods', { params: { year } });
}

export function getPeriod(id: string, year?: number): Promise<AwardPeriodResponse> {
  return apiClient.get<AwardPeriodResponse>(`/AwardPeriods/${id}`, { params: { year } });
}

export function createPeriod(payload: AwardPeriodPayload): Promise<AwardPeriodMutationResponse> {
  return apiClient.post<AwardPeriodMutationResponse>('/AwardPeriods', payload);
}

export function updatePeriod(
  id: string,
  payload: AwardPeriodPayload,
): Promise<AwardPeriodMutationResponse> {
  return apiClient.put<AwardPeriodMutationResponse>(`/AwardPeriods/${id}`, payload);
}

/** Xóa đợt không xóa đảng viên nào — danh sách đủ điều kiện vốn không được lưu (QT5). */
export function deletePeriod(id: string): Promise<{ id: string; warnings: CoverageWarnings }> {
  return apiClient.delete<{ id: string; warnings: CoverageWarnings }>(`/AwardPeriods/${id}`);
}

/** UC-34 — bộ chọn năm segmented chỉ là ba lần gọi với `year` khác nhau. */
export function getEligibility(awardPeriodId: string, year?: number): Promise<EligibilityResponse> {
  return apiClient.get<EligibilityResponse>('/Eligibility', { params: { awardPeriodId, year } });
}
