import type {
  AwardPeriodStatus,
  CoverageGap,
  CoverageOverlap,
  EligibleMemberResponse,
  IsoDate,
  MilestoneBreakdown,
} from '../types/domain';
import { apiClient } from './httpClient';

/** UC-10 — thẻ đợt sắp tới (QT8). */
export interface UpcomingPeriod {
  id: string;
  name: string;
  year: number;
  /** true khi mọi đợt năm nay đã qua và đợt sắp tới thuộc năm sau */
  isNextYear: boolean;
  fromDate: IsoDate;
  toDate: IsoDate;
  fromDisplay: string;
  toDisplay: string;
  status: AwardPeriodStatus;
  /** null khi đang diễn ra → giao diện hiện "Đang diễn ra" */
  daysRemaining: number | null;
  eligibleCount: number;
  /** Sắp theo mốc tăng dần, chỉ liệt kê mốc có người */
  milestoneBreakdown: MilestoneBreakdown[];
}

/** UC-12, UC-13 — cảnh báo nhanh và điều kiện hiện khối hướng dẫn 3 bước. */
export interface DashboardWarnings {
  /** true khi chưa có đảng viên nào → hiện khối hướng dẫn 3 bước */
  noMembers: boolean;
  noPeriods: boolean;
  unassignedYear: number;
  /** Số người bị sót trong unassignedYear (QT7) */
  unassignedCount: number;
  overlaps: CoverageOverlap[];
  gaps: CoverageGap[];
}

/** Một lời gọi đủ dựng cả màn Dashboard, kể cả trạng thái trống. */
export interface DashboardResponse {
  /** Ngày hôm nay theo lịch máy chủ */
  today: IsoDate;
  currentYear: number;
  unitName: string | null;
  memberCount: number;
  periodCount: number;
  /** null khi chưa có đợt nào */
  upcomingPeriod: UpcomingPeriod | null;
  /** Đủ điều kiện của đợt sắp tới; rỗng khi upcomingPeriod = null */
  eligibleMembers: EligibleMemberResponse[];
  warnings: DashboardWarnings;
}

export function getDashboard(): Promise<DashboardResponse> {
  return apiClient.get<DashboardResponse>('/Dashboard');
}
