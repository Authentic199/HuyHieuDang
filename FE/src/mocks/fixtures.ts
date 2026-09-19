/**
 * Dữ liệu giả cho tầng mock — LẤY TỪ bộ dữ liệu kiểm thử của QC
 * (`tests/fixtures/data/expected.json`, `members-core.json`, kịch bản
 * `core_default_T0`, ngày chuẩn T0 = 2026-09-19). Không con số nào bịa ra.
 *
 * Chỉ dùng khi `VITE_USE_MOCK=true`. Khi nối Backend thật (T24) thì xóa cả thư
 * mục `src/mocks` và khối bật mock trong `src/main.tsx`.
 */

/** Ngày "hôm nay" của bộ dữ liệu giả (T0 trong tài liệu kiểm thử). */
export const MOCK_TODAY = '2026-09-19';
export const MOCK_CURRENT_YEAR = 2026;

export const MOCK_ACCOUNT = {
  username: 'admin',
  password: 'admin',
  displayName: 'Quản trị viên',
};

export const MOCK_SETTINGS = {
  startYears: 30,
  endYears: 90,
  stepYears: 5,
  unitName: 'Đảng ủy Phường Kiểm Thử' as string | null,
  milestones: [30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90],
};

/** Một đảng viên trong bộ dữ liệu giả; `code` là mã dòng của bộ kiểm thử. */
export interface MockMember {
  code: string;
  id: string;
  fullName: string;
  dateOfBirth: string | null;
  gender: 'Male' | 'Female' | null;
  officialAdmissionDate: string;
  partyAgeYears: number;
  nextMilestone: number | null;
  nextMilestoneDate: string | null;
}

export const MOCK_MEMBERS: MockMember[] = [
  {
    code: 'B01',
    id: '11111111-0000-4000-8000-000000000001',
    fullName: 'Nguyễn Văn An',
    dateOfBirth: '1974-03-12',
    gender: 'Male',
    officialAdmissionDate: '1996-10-01',
    partyAgeYears: 29,
    nextMilestone: 30,
    nextMilestoneDate: '2026-10-01',
  },
  {
    code: 'B02',
    id: '11111111-0000-4000-8000-000000000002',
    fullName: 'Trần Thị Bình',
    dateOfBirth: '1973-07-05',
    gender: 'Female',
    officialAdmissionDate: '1996-11-07',
    partyAgeYears: 29,
    nextMilestone: 30,
    nextMilestoneDate: '2026-11-07',
  },
  {
    code: 'B03',
    id: '11111111-0000-4000-8000-000000000003',
    fullName: 'Lê Văn Cường',
    dateOfBirth: '1972-11-21',
    gender: 'Male',
    officialAdmissionDate: '1996-09-30',
    partyAgeYears: 29,
    nextMilestone: 30,
    nextMilestoneDate: '2026-09-30',
  },
  {
    code: 'B04',
    id: '11111111-0000-4000-8000-000000000004',
    fullName: 'Phạm Thị Dung',
    dateOfBirth: '1975-02-09',
    gender: 'Female',
    officialAdmissionDate: '1996-11-08',
    partyAgeYears: 29,
    nextMilestone: 30,
    nextMilestoneDate: '2026-11-08',
  },
  {
    code: 'B05',
    id: '11111111-0000-4000-8000-000000000005',
    fullName: 'Hoàng Văn Em',
    dateOfBirth: '1971-06-30',
    gender: 'Male',
    officialAdmissionDate: '1996-01-15',
    partyAgeYears: 30,
    nextMilestone: 35,
    nextMilestoneDate: '2031-01-15',
  },
  {
    code: 'B06',
    id: '11111111-0000-4000-8000-000000000006',
    fullName: 'Vũ Thị Giang',
    dateOfBirth: '1972-09-14',
    gender: 'Female',
    officialAdmissionDate: '1996-03-05',
    partyAgeYears: 30,
    nextMilestone: 35,
    nextMilestoneDate: '2031-03-05',
  },
  {
    code: 'B07',
    id: '11111111-0000-4000-8000-000000000007',
    fullName: 'Đỗ Văn Hải',
    dateOfBirth: '1970-03-03',
    gender: 'Male',
    officialAdmissionDate: '1996-01-14',
    partyAgeYears: 30,
    nextMilestone: 35,
    nextMilestoneDate: '2031-01-14',
  },
  {
    code: 'B08',
    id: '11111111-0000-4000-8000-000000000008',
    fullName: 'Bùi Thị Hoa',
    dateOfBirth: '1973-12-28',
    gender: 'Female',
    officialAdmissionDate: '1996-03-06',
    partyAgeYears: 30,
    nextMilestone: 35,
    nextMilestoneDate: '2031-03-06',
  },
  {
    code: 'B09',
    id: '11111111-0000-4000-8000-000000000009',
    fullName: 'Đinh Văn Khoa',
    dateOfBirth: '1970-04-17',
    gender: 'Male',
    officialAdmissionDate: '1996-05-01',
    partyAgeYears: 30,
    nextMilestone: 35,
    nextMilestoneDate: '2031-05-01',
  },
  {
    code: 'B10',
    id: '11111111-0000-4000-8000-000000000010',
    fullName: 'Hà Thị Liên',
    dateOfBirth: '1971-08-02',
    gender: 'Female',
    officialAdmissionDate: '1996-05-31',
    partyAgeYears: 30,
    nextMilestone: 35,
    nextMilestoneDate: '2031-05-31',
  },
  {
    code: 'B11',
    id: '11111111-0000-4000-8000-000000000011',
    fullName: 'Chu Văn Mạnh',
    dateOfBirth: '1969-09-19',
    gender: 'Male',
    officialAdmissionDate: '1996-08-15',
    partyAgeYears: 30,
    nextMilestone: 35,
    nextMilestoneDate: '2031-08-15',
  },
  {
    code: 'B12',
    id: '11111111-0000-4000-8000-000000000012',
    fullName: 'Trương Thị Nhàn',
    dateOfBirth: '1974-10-25',
    gender: 'Female',
    officialAdmissionDate: '1996-09-10',
    partyAgeYears: 30,
    nextMilestone: 35,
    nextMilestoneDate: '2031-09-10',
  },
  {
    code: 'L01',
    id: '11111111-0000-4000-8000-000000000013',
    fullName: 'Ngô Văn Khánh',
    dateOfBirth: '1976-02-29',
    gender: 'Male',
    officialAdmissionDate: '1996-02-29',
    partyAgeYears: 30,
    nextMilestone: 35,
    nextMilestoneDate: '2031-02-28',
  },
  {
    code: 'L02',
    id: '11111111-0000-4000-8000-000000000014',
    fullName: 'Dương Thị Lan',
    dateOfBirth: '1966-06-11',
    gender: 'Female',
    officialAdmissionDate: '1988-02-29',
    partyAgeYears: 38,
    nextMilestone: 40,
    nextMilestoneDate: '2028-02-29',
  },
  {
    code: 'L03',
    id: '11111111-0000-4000-8000-000000000015',
    fullName: 'Lâm Văn Lộc',
    dateOfBirth: '1968-02-29',
    gender: 'Male',
    officialAdmissionDate: '1991-05-20',
    partyAgeYears: 35,
    nextMilestone: 40,
    nextMilestoneDate: '2031-05-20',
  },
  {
    code: 'M01',
    id: '11111111-0000-4000-8000-000000000016',
    fullName: 'Trịnh Văn Minh',
    dateOfBirth: '1917-01-01',
    gender: 'Male',
    officialAdmissionDate: '1935-05-01',
    partyAgeYears: 91,
    nextMilestone: null,
    nextMilestoneDate: null,
  },
  {
    code: 'M02',
    id: '11111111-0000-4000-8000-000000000017',
    fullName: 'Lý Thị Nga',
    dateOfBirth: '1918-03-20',
    gender: 'Female',
    officialAdmissionDate: '1936-05-01',
    partyAgeYears: 90,
    nextMilestone: null,
    nextMilestoneDate: null,
  },
  {
    code: 'M03',
    id: '11111111-0000-4000-8000-000000000018',
    fullName: 'Phan Văn Phúc',
    dateOfBirth: '1918-08-08',
    gender: 'Male',
    officialAdmissionDate: '1936-06-10',
    partyAgeYears: 90,
    nextMilestone: null,
    nextMilestoneDate: null,
  },
  {
    code: 'N01',
    id: '11111111-0000-4000-8000-000000000019',
    fullName: 'Đặng Thị Quỳnh',
    dateOfBirth: '1978-02-14',
    gender: 'Female',
    officialAdmissionDate: '1997-10-01',
    partyAgeYears: 28,
    nextMilestone: 30,
    nextMilestoneDate: '2027-10-01',
  },
  {
    code: 'N02',
    id: '11111111-0000-4000-8000-000000000020',
    fullName: 'Tạ Văn Sơn',
    dateOfBirth: '1980-05-06',
    gender: 'Male',
    officialAdmissionDate: '2000-01-15',
    partyAgeYears: 26,
    nextMilestone: 30,
    nextMilestoneDate: '2030-01-15',
  },
  {
    code: 'E01',
    id: '11111111-0000-4000-8000-000000000021',
    fullName: 'Cao Thị Thu',
    dateOfBirth: null,
    gender: null,
    officialAdmissionDate: '1996-05-20',
    partyAgeYears: 30,
    nextMilestone: 35,
    nextMilestoneDate: '2031-05-20',
  },
  {
    code: 'E02',
    id: '11111111-0000-4000-8000-000000000022',
    fullName: 'Mai Văn Tuấn',
    dateOfBirth: null,
    gender: 'Male',
    officialAdmissionDate: '1996-08-25',
    partyAgeYears: 30,
    nextMilestone: 35,
    nextMilestoneDate: '2031-08-25',
  },
  {
    code: 'E03',
    id: '11111111-0000-4000-8000-000000000023',
    fullName: 'Võ Thị Út',
    dateOfBirth: '1975-07-07',
    gender: null,
    officialAdmissionDate: '1996-08-22',
    partyAgeYears: 30,
    nextMilestone: 35,
    nextMilestoneDate: '2031-08-22',
  },
  {
    code: 'S01',
    id: '11111111-0000-4000-8000-000000000024',
    fullName: 'Hồ Thị Vân',
    dateOfBirth: '1969-01-19',
    gender: 'Female',
    officialAdmissionDate: '1991-10-05',
    partyAgeYears: 34,
    nextMilestone: 35,
    nextMilestoneDate: '2026-10-05',
  },
  {
    code: 'S02',
    id: '11111111-0000-4000-8000-000000000025',
    fullName: 'Nguyễn Văn Xuân',
    dateOfBirth: '1962-04-23',
    gender: 'Male',
    officialAdmissionDate: '1986-10-20',
    partyAgeYears: 39,
    nextMilestone: 40,
    nextMilestoneDate: '2026-10-20',
  },
  {
    code: 'S03',
    id: '11111111-0000-4000-8000-000000000026',
    fullName: 'Trần Thị Yến',
    dateOfBirth: '1957-11-16',
    gender: 'Female',
    officialAdmissionDate: '1981-11-03',
    partyAgeYears: 44,
    nextMilestone: 45,
    nextMilestoneDate: '2026-11-03',
  },
  {
    code: 'S04',
    id: '11111111-0000-4000-8000-000000000027',
    fullName: 'Ngô Thị Cẩm',
    dateOfBirth: '1966-03-08',
    gender: 'Female',
    officialAdmissionDate: '1991-07-15',
    partyAgeYears: 35,
    nextMilestone: 40,
    nextMilestoneDate: '2031-07-15',
  },
  {
    code: 'G05',
    id: '11111111-0000-4000-8000-000000000028',
    fullName: 'Phùng Văn Bảo',
    dateOfBirth: '1970-12-12',
    gender: 'Male',
    officialAdmissionDate: '1996-07-01',
    partyAgeYears: 30,
    nextMilestone: 35,
    nextMilestoneDate: '2031-07-01',
  },
  {
    code: 'C01',
    id: '11111111-0000-4000-8000-000000000029',
    fullName: 'Đào Văn Ân',
    dateOfBirth: '1975-01-01',
    gender: 'Male',
    officialAdmissionDate: '1996-10-03',
    partyAgeYears: 29,
    nextMilestone: 30,
    nextMilestoneDate: '2026-10-03',
  },
  {
    code: 'X01',
    id: '11111111-0000-4000-8000-000000000030',
    fullName: 'Nguyễn Thị Ánh',
    dateOfBirth: '1971-02-02',
    gender: 'Female',
    officialAdmissionDate: '1996-02-10',
    partyAgeYears: 30,
    nextMilestone: 35,
    nextMilestoneDate: '2031-02-10',
  },
  {
    code: 'X02',
    id: '11111111-0000-4000-8000-000000000031',
    fullName: 'Nguyễn Văn Ẩn',
    dateOfBirth: '1968-09-09',
    gender: 'Male',
    officialAdmissionDate: '1991-02-20',
    partyAgeYears: 35,
    nextMilestone: 40,
    nextMilestoneDate: '2031-02-20',
  },
  {
    code: 'V01',
    id: '11111111-0000-4000-8000-000000000032',
    fullName: 'Lưu Thị Diễm',
    dateOfBirth: '2000-04-04',
    gender: 'Female',
    officialAdmissionDate: '2026-09-19',
    partyAgeYears: 0,
    nextMilestone: 30,
    nextMilestoneDate: '2056-09-19',
  },
];

export interface MockPeriod {
  code: string;
  id: string;
  name: string;
  fromDay: number;
  fromMonth: number;
  toDay: number;
  toMonth: number;
  status: 'Past' | 'Ongoing' | 'Upcoming';
  daysRemaining: number | null;
}

export const MOCK_PERIODS: MockPeriod[] = [
  {
    code: 'P1',
    id: '22222222-0000-4000-8000-000000000001',
    name: 'Đợt 3/2',
    fromDay: 15,
    fromMonth: 1,
    toDay: 5,
    toMonth: 3,
    status: 'Past',
    daysRemaining: null,
  },
  {
    code: 'P2',
    id: '22222222-0000-4000-8000-000000000002',
    name: 'Đợt 19/5',
    fromDay: 1,
    fromMonth: 5,
    toDay: 31,
    toMonth: 5,
    status: 'Past',
    daysRemaining: null,
  },
  {
    code: 'P3',
    id: '22222222-0000-4000-8000-000000000003',
    name: 'Đợt 2/9',
    fromDay: 15,
    fromMonth: 8,
    toDay: 10,
    toMonth: 9,
    status: 'Past',
    daysRemaining: null,
  },
  {
    code: 'P4',
    id: '22222222-0000-4000-8000-000000000004',
    name: 'Đợt 7/11',
    fromDay: 1,
    fromMonth: 10,
    toDay: 7,
    toMonth: 11,
    status: 'Upcoming',
    daysRemaining: 12,
  },
  /**
   * Đợt chồng lấn, thêm cho T19 để xem được banner cảnh báo và đoạn gạch chéo
   * trên dải độ phủ. Nằm TRỌN trong "Đợt 3/2" (15/01–05/03) nên không đụng vào
   * khoảng trống nào của MOCK_GAPS, cũng không đổi số người "Chưa thuộc đợt
   * nào". Người đủ điều kiện của đợt này là đúng ba người của P1 rơi vào
   * 01/02–28/02, không thêm ai mới.
   */
  {
    code: 'P5',
    id: '22222222-0000-4000-8000-000000000005',
    name: 'Đợt tháng 2',
    fromDay: 1,
    fromMonth: 2,
    toDay: 28,
    toMonth: 2,
    status: 'Past',
    daysRemaining: null,
  },
];

export interface MockEligibleRow {
  code: string;
  milestone: number;
  milestoneDate: string;
}

export interface MockEligibleYear {
  total: number;
  byMilestone: { milestone: number; count: number }[];
  rows: MockEligibleRow[];
}

/** Đủ điều kiện theo mã đợt rồi theo năm (QT4). */
export const MOCK_ELIGIBLE: Record<string, Record<string, MockEligibleYear>> = {
  P1: {
    '2025': {
      total: 0,
      byMilestone: [],
      rows: [],
    },
    '2026': {
      total: 5,
      byMilestone: [
        {
          milestone: 30,
          count: 4,
        },
        {
          milestone: 35,
          count: 1,
        },
      ],
      rows: [
        {
          code: 'B05',
          milestone: 30,
          milestoneDate: '2026-01-15',
        },
        {
          code: 'L01',
          milestone: 30,
          milestoneDate: '2026-02-28',
        },
        {
          code: 'X01',
          milestone: 30,
          milestoneDate: '2026-02-10',
        },
        {
          code: 'B06',
          milestone: 30,
          milestoneDate: '2026-03-05',
        },
        {
          code: 'X02',
          milestone: 35,
          milestoneDate: '2026-02-20',
        },
      ],
    },
    '2027': {
      total: 0,
      byMilestone: [],
      rows: [],
    },
    '2028': {
      total: 1,
      byMilestone: [
        {
          milestone: 40,
          count: 1,
        },
      ],
      rows: [
        {
          code: 'L02',
          milestone: 40,
          milestoneDate: '2028-02-29',
        },
      ],
    },
  },
  P2: {
    '2025': {
      total: 1,
      byMilestone: [
        {
          milestone: 90,
          count: 1,
        },
      ],
      rows: [
        {
          code: 'M01',
          milestone: 90,
          milestoneDate: '2025-05-01',
        },
      ],
    },
    '2026': {
      total: 5,
      byMilestone: [
        {
          milestone: 30,
          count: 3,
        },
        {
          milestone: 35,
          count: 1,
        },
        {
          milestone: 90,
          count: 1,
        },
      ],
      rows: [
        {
          code: 'E01',
          milestone: 30,
          milestoneDate: '2026-05-20',
        },
        {
          code: 'B09',
          milestone: 30,
          milestoneDate: '2026-05-01',
        },
        {
          code: 'B10',
          milestone: 30,
          milestoneDate: '2026-05-31',
        },
        {
          code: 'L03',
          milestone: 35,
          milestoneDate: '2026-05-20',
        },
        {
          code: 'M02',
          milestone: 90,
          milestoneDate: '2026-05-01',
        },
      ],
    },
    '2027': {
      total: 0,
      byMilestone: [],
      rows: [],
    },
    '2028': {
      total: 0,
      byMilestone: [],
      rows: [],
    },
  },
  P3: {
    '2025': {
      total: 0,
      byMilestone: [],
      rows: [],
    },
    '2026': {
      total: 4,
      byMilestone: [
        {
          milestone: 30,
          count: 4,
        },
      ],
      rows: [
        {
          code: 'B11',
          milestone: 30,
          milestoneDate: '2026-08-15',
        },
        {
          code: 'E02',
          milestone: 30,
          milestoneDate: '2026-08-25',
        },
        {
          code: 'B12',
          milestone: 30,
          milestoneDate: '2026-09-10',
        },
        {
          code: 'E03',
          milestone: 30,
          milestoneDate: '2026-08-22',
        },
      ],
    },
    '2027': {
      total: 0,
      byMilestone: [],
      rows: [],
    },
    '2028': {
      total: 0,
      byMilestone: [],
      rows: [],
    },
  },
  P4: {
    '2025': {
      total: 0,
      byMilestone: [],
      rows: [],
    },
    '2026': {
      total: 6,
      byMilestone: [
        {
          milestone: 30,
          count: 3,
        },
        {
          milestone: 35,
          count: 1,
        },
        {
          milestone: 40,
          count: 1,
        },
        {
          milestone: 45,
          count: 1,
        },
      ],
      rows: [
        {
          code: 'C01',
          milestone: 30,
          milestoneDate: '2026-10-03',
        },
        {
          code: 'B01',
          milestone: 30,
          milestoneDate: '2026-10-01',
        },
        {
          code: 'B02',
          milestone: 30,
          milestoneDate: '2026-11-07',
        },
        {
          code: 'S01',
          milestone: 35,
          milestoneDate: '2026-10-05',
        },
        {
          code: 'S02',
          milestone: 40,
          milestoneDate: '2026-10-20',
        },
        {
          code: 'S03',
          milestone: 45,
          milestoneDate: '2026-11-03',
        },
      ],
    },
    '2027': {
      total: 1,
      byMilestone: [
        {
          milestone: 30,
          count: 1,
        },
      ],
      rows: [
        {
          code: 'N01',
          milestone: 30,
          milestoneDate: '2027-10-01',
        },
      ],
    },
    '2028': {
      total: 0,
      byMilestone: [],
      rows: [],
    },
  },
  /**
   * Đợt chồng lấn P5 (01/02–28/02) không có người nào của riêng nó: đây đúng
   * ba người của P1 tròn mốc trong tháng 2 — chính là hệ quả mà banner "có đợt
   * chồng lấn" muốn cảnh báo.
   */
  P5: {
    '2025': {
      total: 0,
      byMilestone: [],
      rows: [],
    },
    '2026': {
      total: 3,
      byMilestone: [
        {
          milestone: 30,
          count: 2,
        },
        {
          milestone: 35,
          count: 1,
        },
      ],
      rows: [
        {
          code: 'X01',
          milestone: 30,
          milestoneDate: '2026-02-10',
        },
        {
          code: 'L01',
          milestone: 30,
          milestoneDate: '2026-02-28',
        },
        {
          code: 'X02',
          milestone: 35,
          milestoneDate: '2026-02-20',
        },
      ],
    },
    '2027': {
      total: 0,
      byMilestone: [],
      rows: [],
    },
    '2028': {
      total: 0,
      byMilestone: [],
      rows: [],
    },
  },
};

export interface MockUnassignedRow extends MockEligibleRow {
  gap: {
    type: 'Between' | 'BeforeFirst' | 'AfterLast';
    previousPeriodName: string | null;
    nextPeriodName: string | null;
  };
}

/** Chưa thuộc đợt nào theo năm (QT7). */
export const MOCK_UNASSIGNED: Record<string, { total: number; rows: MockUnassignedRow[] }> = {
  '2025': {
    total: 0,
    rows: [],
  },
  '2026': {
    total: 7,
    rows: [
      {
        code: 'B08',
        milestone: 30,
        milestoneDate: '2026-03-06',
        gap: {
          type: 'Between',
          previousPeriodName: 'Đợt 3/2',
          nextPeriodName: 'Đợt 19/5',
        },
      },
      {
        code: 'B07',
        milestone: 30,
        milestoneDate: '2026-01-14',
        gap: {
          type: 'BeforeFirst',
          previousPeriodName: null,
          nextPeriodName: null,
        },
      },
      {
        code: 'B03',
        milestone: 30,
        milestoneDate: '2026-09-30',
        gap: {
          type: 'Between',
          previousPeriodName: 'Đợt 2/9',
          nextPeriodName: 'Đợt 7/11',
        },
      },
      {
        code: 'B04',
        milestone: 30,
        milestoneDate: '2026-11-08',
        gap: {
          type: 'AfterLast',
          previousPeriodName: null,
          nextPeriodName: null,
        },
      },
      {
        code: 'G05',
        milestone: 30,
        milestoneDate: '2026-07-01',
        gap: {
          type: 'Between',
          previousPeriodName: 'Đợt 19/5',
          nextPeriodName: 'Đợt 2/9',
        },
      },
      {
        code: 'S04',
        milestone: 35,
        milestoneDate: '2026-07-15',
        gap: {
          type: 'Between',
          previousPeriodName: 'Đợt 19/5',
          nextPeriodName: 'Đợt 2/9',
        },
      },
      {
        code: 'M03',
        milestone: 90,
        milestoneDate: '2026-06-10',
        gap: {
          type: 'Between',
          previousPeriodName: 'Đợt 19/5',
          nextPeriodName: 'Đợt 2/9',
        },
      },
    ],
  },
  '2027': {
    total: 0,
    rows: [],
  },
  '2028': {
    total: 0,
    rows: [],
  },
};

export interface MockGap {
  fromDate: string;
  toDate: string;
  fromDisplay: string;
  toDisplay: string;
  previousPeriodName: string | null;
  nextPeriodName: string | null;
}

/** Khoảng trống chưa đợt nào phủ, theo năm (QT6). */
export const MOCK_GAPS: Record<string, MockGap[]> = {
  '2025': [
    {
      fromDate: '2025-01-01',
      toDate: '2025-01-14',
      fromDisplay: '01/01',
      toDisplay: '14/01',
      previousPeriodName: null,
      nextPeriodName: null,
    },
    {
      fromDate: '2025-03-06',
      toDate: '2025-04-30',
      fromDisplay: '06/03',
      toDisplay: '30/04',
      previousPeriodName: 'Đợt 3/2',
      nextPeriodName: 'Đợt 19/5',
    },
    {
      fromDate: '2025-06-01',
      toDate: '2025-08-14',
      fromDisplay: '01/06',
      toDisplay: '14/08',
      previousPeriodName: 'Đợt 19/5',
      nextPeriodName: 'Đợt 2/9',
    },
    {
      fromDate: '2025-09-11',
      toDate: '2025-09-30',
      fromDisplay: '11/09',
      toDisplay: '30/09',
      previousPeriodName: 'Đợt 2/9',
      nextPeriodName: 'Đợt 7/11',
    },
    {
      fromDate: '2025-11-08',
      toDate: '2025-12-31',
      fromDisplay: '08/11',
      toDisplay: '31/12',
      previousPeriodName: null,
      nextPeriodName: null,
    },
  ],
  '2026': [
    {
      fromDate: '2026-01-01',
      toDate: '2026-01-14',
      fromDisplay: '01/01',
      toDisplay: '14/01',
      previousPeriodName: null,
      nextPeriodName: null,
    },
    {
      fromDate: '2026-03-06',
      toDate: '2026-04-30',
      fromDisplay: '06/03',
      toDisplay: '30/04',
      previousPeriodName: 'Đợt 3/2',
      nextPeriodName: 'Đợt 19/5',
    },
    {
      fromDate: '2026-06-01',
      toDate: '2026-08-14',
      fromDisplay: '01/06',
      toDisplay: '14/08',
      previousPeriodName: 'Đợt 19/5',
      nextPeriodName: 'Đợt 2/9',
    },
    {
      fromDate: '2026-09-11',
      toDate: '2026-09-30',
      fromDisplay: '11/09',
      toDisplay: '30/09',
      previousPeriodName: 'Đợt 2/9',
      nextPeriodName: 'Đợt 7/11',
    },
    {
      fromDate: '2026-11-08',
      toDate: '2026-12-31',
      fromDisplay: '08/11',
      toDisplay: '31/12',
      previousPeriodName: null,
      nextPeriodName: null,
    },
  ],
  '2027': [
    {
      fromDate: '2027-01-01',
      toDate: '2027-01-14',
      fromDisplay: '01/01',
      toDisplay: '14/01',
      previousPeriodName: null,
      nextPeriodName: null,
    },
    {
      fromDate: '2027-03-06',
      toDate: '2027-04-30',
      fromDisplay: '06/03',
      toDisplay: '30/04',
      previousPeriodName: 'Đợt 3/2',
      nextPeriodName: 'Đợt 19/5',
    },
    {
      fromDate: '2027-06-01',
      toDate: '2027-08-14',
      fromDisplay: '01/06',
      toDisplay: '14/08',
      previousPeriodName: 'Đợt 19/5',
      nextPeriodName: 'Đợt 2/9',
    },
    {
      fromDate: '2027-09-11',
      toDate: '2027-09-30',
      fromDisplay: '11/09',
      toDisplay: '30/09',
      previousPeriodName: 'Đợt 2/9',
      nextPeriodName: 'Đợt 7/11',
    },
    {
      fromDate: '2027-11-08',
      toDate: '2027-12-31',
      fromDisplay: '08/11',
      toDisplay: '31/12',
      previousPeriodName: null,
      nextPeriodName: null,
    },
  ],
  '2028': [
    {
      fromDate: '2028-01-01',
      toDate: '2028-01-14',
      fromDisplay: '01/01',
      toDisplay: '14/01',
      previousPeriodName: null,
      nextPeriodName: null,
    },
    {
      fromDate: '2028-03-06',
      toDate: '2028-04-30',
      fromDisplay: '06/03',
      toDisplay: '30/04',
      previousPeriodName: 'Đợt 3/2',
      nextPeriodName: 'Đợt 19/5',
    },
    {
      fromDate: '2028-06-01',
      toDate: '2028-08-14',
      fromDisplay: '01/06',
      toDisplay: '14/08',
      previousPeriodName: 'Đợt 19/5',
      nextPeriodName: 'Đợt 2/9',
    },
    {
      fromDate: '2028-09-11',
      toDate: '2028-09-30',
      fromDisplay: '11/09',
      toDisplay: '30/09',
      previousPeriodName: 'Đợt 2/9',
      nextPeriodName: 'Đợt 7/11',
    },
    {
      fromDate: '2028-11-08',
      toDate: '2028-12-31',
      fromDisplay: '08/11',
      toDisplay: '31/12',
      previousPeriodName: null,
      nextPeriodName: null,
    },
  ],
};
