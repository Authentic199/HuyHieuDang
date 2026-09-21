/**
 * Kiểu dữ liệu nghiệp vụ, theo docs/api-contract.md v1 (mục 1.10 và các mục
 * mô tả `data` của từng endpoint). Tên trường dùng camelCase đúng như API trả.
 */

/** Ngày thuần dạng `yyyy-MM-dd`. Hiển thị ra màn hình là dd/MM/yyyy. */
export type IsoDate = string;

/** Mốc thời gian ISO UTC, thường không hiển thị. */
export type IsoDateTime = string;

/** Giới tính trên API. Hiển thị: Male → Nam, Female → Nữ, null → — */
export type Gender = 'Male' | 'Female';

/** Trạng thái của đợt trong năm đang xét (QT11). */
export type AwardPeriodStatus = 'Past' | 'Ongoing' | 'Upcoming';

/** Một đảng viên trong danh sách M2. */
export interface PartyMemberResponse {
  id: string;
  fullName: string;
  dateOfBirth: IsoDate | null;
  gender: Gender | null;
  officialAdmissionDate: IsoDate;
  /** QT3 — tuổi đảng tính đến hôm nay, tính lại mỗi lần gọi */
  partyAgeYears: number;
  /** QT3a — null khi đã vượt mốc lớn nhất */
  nextMilestone: number | null;
  nextMilestoneDate: IsoDate | null;
  createdAt: IsoDateTime;
  updatedAt: IsoDateTime;
}

/**
 * Một dòng trong danh sách đủ điều kiện.
 * Không có `id`: danh sách này không được lưu vào bảng nào (QT5), mỗi lời gọi
 * là một lần tính lại. `partyMemberId` chỉ dùng làm rowKey và để mở màn sửa.
 */
export interface EligibleMemberResponse {
  partyMemberId: string;
  fullName: string;
  gender: Gender | null;
  dateOfBirth: IsoDate | null;
  officialAdmissionDate: IsoDate;
  milestoneDate: IsoDate;
  milestone: number;
}

/** Vị trí khoảng trống mà người bị sót rơi vào (UC-40). */
export type GapType = 'Between' | 'BeforeFirst' | 'AfterLast';

export interface UnassignedGap {
  type: GapType;
  previousPeriodName: string | null;
  nextPeriodName: string | null;
}

export interface UnassignedMemberResponse extends EligibleMemberResponse {
  gap: UnassignedGap;
}

/** Một đợt trao huy hiệu, đã gắn năm đang xét. */
export interface AwardPeriodResponse {
  id: string;
  name: string;
  /** Dữ liệu lưu thật — không có năm (QT6) */
  fromDay: number;
  fromMonth: number;
  toDay: number;
  toMonth: number;
  /** Dạng dd/MM, tiện cho bảng */
  fromDisplay: string;
  toDisplay: string;
  /** Năm đang xét */
  year: number;
  /** Đã gắn year; 29/02 ở năm không nhuận lùi về 28/02 */
  fromDate: IsoDate;
  /** Thuộc year + 1 khi đợt vắt qua 31/12 */
  toDate: IsoDate;
  /** Đợt vắt qua 31/12, Đến ngày rơi vào năm sau (QT6) */
  spansNextYear: boolean;
  status: AwardPeriodStatus;
  /** Chỉ khác null khi status = 'Upcoming' */
  daysRemaining: number | null;
  /** Số người đủ điều kiện của đợt trong year (QT4) */
  eligibleCount: number;
}

/** Hai đợt chồng lấn nhau (QT6). */
export interface CoverageOverlap {
  firstPeriodId: string;
  firstPeriodName: string;
  secondPeriodId: string;
  secondPeriodName: string;
  fromDate: IsoDate;
  toDate: IsoDate;
  fromDisplay: string;
  toDisplay: string;
}

/** Khoảng trong năm chưa đợt nào phủ (QT6). */
export interface CoverageGap {
  fromDate: IsoDate;
  toDate: IsoDate;
  fromDisplay: string;
  toDisplay: string;
  previousPeriodName: string | null;
  nextPeriodName: string | null;
}

/** Cảnh báo chồng lấn và chưa phủ kín. Không bao giờ chặn lưu. */
export interface CoverageWarnings {
  overlaps: CoverageOverlap[];
  gaps: CoverageGap[];
}

/** Số người đủ điều kiện theo từng mốc huy hiệu. */
export interface MilestoneBreakdown {
  milestone: number;
  count: number;
}
