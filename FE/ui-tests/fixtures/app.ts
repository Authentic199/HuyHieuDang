import { test as base, type Page } from '@playwright/test';

import {
  ELIGIBLE_COUNT,
  makeEligibleRows,
  makePeriodRows,
  makeUnassignedRows,
  PERIOD_COUNT,
  SERVER_TODAY,
  SERVER_YEAR,
  UNASSIGNED_COUNT,
  type EligibleRow,
  type PeriodRow,
  type UnassignedRow,
} from './data';

/**
 * Fixture cho các ca giao diện của T50: chặn mọi lời gọi `/api/**` và trả bộ dữ
 * liệu giả, đặt sẵn thẻ đăng nhập để vào thẳng màn cần kiểm.
 */

/** Khóa localStorage giữ thẻ đăng nhập — phải khớp `FE/src/api/config.ts`. */
const TOKEN_STORAGE_KEY = 'hhd.accessToken';

/** Vỏ `{ message, data }` của mọi phản hồi thành công (mục 1.3 hợp đồng API). */
function envelope(data: unknown) {
  return {
    status: 200,
    contentType: 'application/json',
    body: JSON.stringify({ message: null, data }),
  };
}

export interface MockData {
  eligible: EligibleRow[];
  unassigned: UnassignedRow[];
  periods: PeriodRow[];
}

export const mockData: MockData = {
  eligible: makeEligibleRows(ELIGIBLE_COUNT),
  unassigned: makeUnassignedRows(UNASSIGNED_COUNT),
  periods: makePeriodRows(PERIOD_COUNT),
};

const UPCOMING_PERIOD = {
  id: 'period-upcoming',
  name: 'Đợt 7/11',
  year: SERVER_YEAR,
  isNextYear: false,
  fromDate: `${SERVER_YEAR}-11-01`,
  toDate: `${SERVER_YEAR}-11-07`,
  fromDisplay: '01/11',
  toDisplay: '07/11',
  status: 'Upcoming' as const,
  daysRemaining: 43,
  eligibleCount: ELIGIBLE_COUNT,
  milestoneBreakdown: [] as { milestone: number; count: number }[],
};

/** Đợt dùng cho trang chi tiết — lấy đợt đầu tiên của danh sách. */
export const DETAIL_PERIOD = mockData.periods[0];

async function installApiMocks(page: Page): Promise<void> {
  await page.route('**/api/**', async (route) => {
    const url = new URL(route.request().url());
    const path = url.pathname.replace(/^.*\/api/, '');

    if (path === '/Auth/Me') {
      return route.fulfill(
        envelope({
          username: 'admin',
          displayName: 'Cán bộ văn phòng',
          unitName: 'Đảng ủy Phường Kiểm Thử',
          serverDate: SERVER_TODAY,
          expiresAt: `${SERVER_YEAR}-12-31T00:00:00Z`,
        }),
      );
    }

    if (path === '/Eligibility/UnassignedCount') {
      return route.fulfill(envelope({ year: SERVER_YEAR, count: mockData.unassigned.length }));
    }

    if (path === '/Dashboard') {
      return route.fulfill(
        envelope({
          today: SERVER_TODAY,
          currentYear: SERVER_YEAR,
          unitName: 'Đảng ủy Phường Kiểm Thử',
          memberCount: 1342,
          periodCount: mockData.periods.length,
          upcomingPeriod: {
            ...UPCOMING_PERIOD,
            milestoneBreakdown: breakdownOf(mockData.eligible),
          },
          eligibleMembers: mockData.eligible,
          warnings: {
            noMembers: false,
            noPeriods: false,
            unassignedYear: SERVER_YEAR,
            unassignedCount: mockData.unassigned.length,
            overlaps: [],
            gaps: [],
          },
        }),
      );
    }

    if (path === '/AwardPeriods') {
      return route.fulfill(
        envelope({
          year: SERVER_YEAR,
          today: SERVER_TODAY,
          totalCount: mockData.periods.length,
          periods: mockData.periods,
          warnings: { overlaps: [], gaps: [] },
          coverage: { segments: [] },
        }),
      );
    }

    if (path.startsWith('/AwardPeriods/')) {
      const id = path.slice('/AwardPeriods/'.length);
      const period = mockData.periods.find((row) => row.id === id) ?? mockData.periods[0];
      return route.fulfill(envelope(period));
    }

    if (path === '/Eligibility') {
      const year = Number(url.searchParams.get('year') ?? SERVER_YEAR);
      return route.fulfill(
        envelope({
          awardPeriod: { ...DETAIL_PERIOD, year },
          year,
          totalCount: mockData.eligible.length,
          milestoneBreakdown: breakdownOf(mockData.eligible),
          members: mockData.eligible,
        }),
      );
    }

    if (path === '/Eligibility/Unassigned') {
      const year = Number(url.searchParams.get('year') ?? SERVER_YEAR);
      return route.fulfill(
        envelope({
          year,
          totalCount: mockData.unassigned.length,
          members: mockData.unassigned,
        }),
      );
    }

    if (path === '/PartyMembers') {
      const current = Number(url.searchParams.get('current') ?? 1);
      const pageSize = Number(url.searchParams.get('pageSize') ?? 20);
      const all = mockData.eligible.map((row, index) => ({
        id: row.partyMemberId,
        fullName: row.fullName,
        dateOfBirth: row.dateOfBirth,
        gender: row.gender,
        officialAdmissionDate: row.officialAdmissionDate,
        partyAgeYears: row.milestone,
        nextMilestone: row.milestone + 5,
        nextMilestoneDate: row.milestoneDate,
        createdAt: `${SERVER_YEAR}-01-01T00:00:00Z`,
        updatedAt: `${SERVER_YEAR}-01-0${(index % 9) + 1}T00:00:00Z`,
      }));
      return route.fulfill(
        envelope({
          pagedData: all.slice((current - 1) * pageSize, current * pageSize),
          pageInfo: {
            current,
            pageSize,
            totalCount: all.length,
            totalPages: Math.ceil(all.length / pageSize),
          },
        }),
      );
    }

    if (path === '/Settings') {
      return route.fulfill(
        envelope({
          startYears: 30,
          endYears: 90,
          stepYears: 5,
          unitName: 'Đảng ủy Phường Kiểm Thử',
          milestones: [30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90],
          milestoneCount: 13,
          updatedAt: `${SERVER_YEAR}-01-01T00:00:00Z`,
        }),
      );
    }

    // Endpoint nào chưa dùng trong các ca này thì trả rỗng, không để treo lời gọi.
    return route.fulfill(envelope(null));
  });
}

function breakdownOf(rows: EligibleRow[]): { milestone: number; count: number }[] {
  const counts = new Map<number, number>();
  for (const row of rows) counts.set(row.milestone, (counts.get(row.milestone) ?? 0) + 1);
  return [...counts.entries()]
    .sort((a, b) => a[0] - b[0])
    .map(([milestone, count]) => ({ milestone, count }));
}

export const test = base.extend<{ app: Page }>({
  // Tham số thứ hai đặt tên `runTest` thay vì `use` như tài liệu Playwright: bộ
  // lint của dự án là bộ lint React nên `use(...)` sẽ bị hiểu là một React Hook.
  app: async ({ page }, runTest) => {
    await installApiMocks(page);
    await page.addInitScript(([key, token]) => window.localStorage.setItem(key, token), [
      TOKEN_STORAGE_KEY,
      'thẻ-giả-cho-ca-kiểm-thử',
    ] as const);
    await runTest(page);
  },
});

export { expect } from '@playwright/test';
