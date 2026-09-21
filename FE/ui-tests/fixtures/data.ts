/**
 * Bộ dữ liệu giả cho các ca giao diện của T50.
 *
 * Sáu luồng nghiệp vụ trong `FE/e2e/` chạy trên Backend thật và KHÔNG có tầng
 * dữ liệu giả — đó vẫn là nơi kiểm nghiệp vụ. Bộ này phục vụ một việc khác hẳn:
 * dựng danh sách hơn 300 dòng để kiểm phân trang, tìm, sắp xếp và lọc theo mốc
 * — những thứ chạy trọn vẹn ở máy khách, không phụ thuộc máy chủ. Dựng bằng
 * dữ liệu thật thì phải nhập 300 người qua giao diện trước mỗi lần chạy.
 *
 * Mọi con số mong đợi trong ca kiểm thử đều TÍNH RA từ chính bộ này, không viết
 * cứng — sửa bộ dữ liệu thì ca kiểm thử vẫn đúng.
 */

export interface EligibleRow {
  partyMemberId: string;
  fullName: string;
  gender: 'Male' | 'Female' | null;
  dateOfBirth: string | null;
  officialAdmissionDate: string;
  milestoneDate: string;
  milestone: number;
}

export interface UnassignedRow extends EligibleRow {
  gap: {
    type: 'Between' | 'BeforeFirst' | 'AfterLast';
    previousPeriodName: string | null;
    nextPeriodName: string | null;
  };
}

/** Ngày hôm nay của "máy chủ" giả — mọi màn đều lấy ngày từ đây. */
export const SERVER_TODAY = '2026-09-19';
export const SERVER_YEAR = 2026;

const HO = ['Nguyễn', 'Trần', 'Lê', 'Phạm', 'Hoàng', 'Vũ', 'Đặng', 'Bùi', 'Đỗ', 'Hồ'];
const DEM = ['Văn', 'Thị', 'Hữu', 'Đức', 'Minh', 'Quang', 'Thanh', 'Xuân'];
const TEN = [
  'An',
  'Bình',
  'Cường',
  'Dũng',
  'Hà',
  'Hải',
  'Hùng',
  'Khánh',
  'Lan',
  'Mai',
  'Nam',
  'Oanh',
  'Phúc',
  'Quyên',
  'Sơn',
  'Tâm',
  'Uyên',
  'Việt',
  'Yến',
  'Trung',
];

/** Dãy mốc QT1 mặc định rút gọn — chỉ những mốc có người, đúng tinh thần UC-11. */
const MILESTONES = [30, 40, 45, 50, 55, 60, 70];

function pad(value: number): string {
  return String(value).padStart(2, '0');
}

/** Tên đầy đủ sinh ra từ chỉ số, không dùng ngẫu nhiên để chạy lại là y hệt. */
export function fullNameOf(index: number): string {
  return `${HO[index % HO.length]} ${DEM[(index * 3) % DEM.length]} ${TEN[(index * 7) % TEN.length]}`;
}

/** Danh sách đủ điều kiện, đã sắp theo Mốc rồi Họ tên đúng như máy chủ trả. */
export function makeEligibleRows(count: number): EligibleRow[] {
  const rows: EligibleRow[] = [];
  for (let index = 0; index < count; index += 1) {
    const milestone = MILESTONES[index % MILESTONES.length];
    const month = (index % 12) + 1;
    const day = (index % 27) + 1;
    const admissionYear = SERVER_YEAR - milestone;
    rows.push({
      partyMemberId: `member-${pad(index)}`,
      fullName: fullNameOf(index),
      gender: index % 3 === 0 ? 'Female' : 'Male',
      dateOfBirth: `${admissionYear - 20}-${pad(month)}-${pad(day)}`,
      officialAdmissionDate: `${admissionYear}-${pad(month)}-${pad(day)}`,
      milestoneDate: `${SERVER_YEAR}-${pad(month)}-${pad(day)}`,
      milestone,
    });
  }
  return sortLikeServer(rows);
}

/** Máy chủ sắp theo Mốc huy hiệu tăng dần rồi Họ tên (mục 1.8 hợp đồng API). */
export function sortLikeServer<T extends EligibleRow>(rows: T[]): T[] {
  const collator = new Intl.Collator('vi');
  return [...rows].sort(
    (a, b) => a.milestone - b.milestone || collator.compare(a.fullName, b.fullName),
  );
}

const GAPS: UnassignedRow['gap'][] = [
  { type: 'BeforeFirst', previousPeriodName: null, nextPeriodName: null },
  { type: 'Between', previousPeriodName: 'Đợt 3/2', nextPeriodName: 'Đợt 19/5' },
  { type: 'AfterLast', previousPeriodName: null, nextPeriodName: null },
];

export function makeUnassignedRows(count: number): UnassignedRow[] {
  return makeEligibleRows(count).map((row, index) => ({ ...row, gap: GAPS[index % GAPS.length] }));
}

export interface PeriodRow {
  id: string;
  name: string;
  fromDay: number;
  fromMonth: number;
  toDay: number;
  toMonth: number;
  fromDisplay: string;
  toDisplay: string;
  year: number;
  fromDate: string;
  toDate: string;
  status: 'Past' | 'Ongoing' | 'Upcoming';
  daysRemaining: number | null;
  eligibleCount: number;
}

/**
 * Đợt trao huy hiệu. Bảng này có trần 12 đợt mỗi năm trong nghiệp vụ, nhưng ca
 * kiểm thử vẫn dựng nhiều hơn một trang để thấy thanh phân trang làm việc.
 */
export function makePeriodRows(count: number): PeriodRow[] {
  const rows: PeriodRow[] = [];
  for (let index = 0; index < count; index += 1) {
    const fromMonth = (index % 12) + 1;
    const fromDay = (index % 20) + 1;
    const toDay = fromDay + 5;
    // Đợt trước ngày hôm nay của máy chủ là đã qua, sau đó là sắp tới.
    const status = fromMonth < 9 ? 'Past' : fromMonth === 9 ? 'Ongoing' : 'Upcoming';
    rows.push({
      id: `period-${pad(index)}`,
      name: `Đợt ${pad(fromDay)}/${pad(fromMonth)} nhóm ${index + 1}`,
      fromDay,
      fromMonth,
      toDay,
      toMonth: fromMonth,
      fromDisplay: `${pad(fromDay)}/${pad(fromMonth)}`,
      toDisplay: `${pad(toDay)}/${pad(fromMonth)}`,
      year: SERVER_YEAR,
      fromDate: `${SERVER_YEAR}-${pad(fromMonth)}-${pad(fromDay)}`,
      toDate: `${SERVER_YEAR}-${pad(fromMonth)}-${pad(toDay)}`,
      status,
      daysRemaining: status === 'Upcoming' ? 30 : null,
      eligibleCount: (index * 7) % 53,
    });
  }
  // Máy chủ sắp theo Từ ngày tăng dần (mục 6.2 hợp đồng API).
  return rows.sort((a, b) => a.fromDate.localeCompare(b.fromDate));
}

/** Số dòng của mỗi bảng trong ca kiểm thử — vượt xa một trang 20 dòng. */
export const ELIGIBLE_COUNT = 320;
export const UNASSIGNED_COUNT = 310;
export const PERIOD_COUNT = 36;
