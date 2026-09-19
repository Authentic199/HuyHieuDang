/**
 * Tầng dữ liệu giả — thay bộ chuyển tiếp HTTP của axios bằng một hàm trả lời
 * ngay tại trình duyệt, đúng hình dạng của `docs/api-contract.md`.
 *
 * Bật bằng `VITE_USE_MOCK=true` (xem `src/main.tsx`). Đây là nơi DUY NHẤT biết
 * đến dữ liệu giả: tầng `src/api` và mọi màn hình không đổi một dòng nào khi
 * chuyển sang Backend thật ở T24 — chỉ cần tắt biến môi trường.
 *
 * Dữ liệu lấy từ bộ kiểm thử của QC (xem `fixtures.ts`). Các phép tính nghiệp
 * vụ (tuổi đảng, mốc kế tiếp, đủ điều kiện) đã có sẵn trong bộ dữ liệu đó —
 * màn hình không tự tính lại ở bất cứ đâu.
 */
import {
  AxiosError,
  AxiosHeaders,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from 'axios';
import dayjs from 'dayjs';

import type { LoginResponse, SessionInfo } from '../api/auth';
import type { DashboardResponse } from '../api/dashboard';
import http from '../api/httpClient';
import type { ImportResult } from '../api/imports';
import type {
  AwardPeriodListResponse,
  AwardPeriodMutationResponse,
  AwardPeriodPayload,
  CoverageSegment,
  EligibilityResponse,
} from '../api/periods';
import type { SettingsPayload, SettingsResponse } from '../api/settings';
import type { UnassignedResponse } from '../api/uncovered';
import type { PagedResult } from '../types/api';
import type {
  AwardPeriodResponse,
  AwardPeriodStatus,
  CoverageGap,
  CoverageOverlap,
  EligibleMemberResponse,
  Gender,
  PartyMemberResponse,
  UnassignedMemberResponse,
} from '../types/domain';
import {
  MOCK_ACCOUNT,
  MOCK_CURRENT_YEAR,
  MOCK_ELIGIBLE,
  MOCK_GAPS,
  MOCK_MEMBERS,
  MOCK_PERIODS,
  MOCK_SETTINGS,
  MOCK_TODAY,
  MOCK_UNASSIGNED,
  type MockMember,
  type MockPeriod,
} from './fixtures';
import { MOCK_IMPORT_PREVIEW } from './importFixtures';

/** Thẻ đăng nhập giả. Thẻ khác giá trị này bị coi là hết hạn → 401. */
const MOCK_TOKEN = 'mock.access.token';

/** Độ trễ giả để nhìn thấy được trạng thái "đang tải" của bảng. */
const LATENCY_MS = 350;

/* ------------------------------------------------------------------ */
/* Kho dữ liệu trong bộ nhớ — mất khi tải lại trang, đúng tính chất tạm */
/* ------------------------------------------------------------------ */

let members: MockMember[] = MOCK_MEMBERS.map((item) => ({ ...item }));
let periods: MockPeriod[] = MOCK_PERIODS.map((item) => ({ ...item }));
const settings = { ...MOCK_SETTINGS, milestones: [...MOCK_SETTINGS.milestones] };
let nextId = 1000;

function newId(prefix: string): string {
  nextId += 1;
  return `${prefix}-0000-4000-8000-${String(nextId).padStart(12, '0')}`;
}

/* ------------------------------------------------------------------ */
/* Chuyển dữ liệu giả sang đúng hình dạng hợp đồng                      */
/* ------------------------------------------------------------------ */

function toMemberResponse(item: MockMember): PartyMemberResponse {
  return {
    id: item.id,
    fullName: item.fullName,
    dateOfBirth: item.dateOfBirth,
    gender: item.gender,
    officialAdmissionDate: item.officialAdmissionDate,
    partyAgeYears: item.partyAgeYears,
    nextMilestone: item.nextMilestone,
    nextMilestoneDate: item.nextMilestoneDate,
    createdAt: `${MOCK_TODAY}T01:00:00Z`,
    updatedAt: `${MOCK_TODAY}T01:00:00Z`,
  };
}

const pad = (value: number) => String(value).padStart(2, '0');

/** Gắn năm vào ngày/tháng của đợt; 29/02 ở năm không nhuận lùi về 28/02 (QT2). */
function bindDate(day: number, month: number, year: number): string {
  const candidate = dayjs(`${year}-${pad(month)}-${pad(day)}`);
  const sameDay =
    candidate.isValid() && candidate.date() === day && candidate.month() + 1 === month;
  if (sameDay) return candidate.format('YYYY-MM-DD');
  return dayjs(`${year}-${pad(month)}-01`)
    .endOf('month')
    .format('YYYY-MM-DD');
}

function statusOf(fromDate: string, toDate: string): AwardPeriodStatus {
  const today = dayjs(MOCK_TODAY);
  if (today.isAfter(dayjs(toDate))) return 'Past';
  if (today.isBefore(dayjs(fromDate))) return 'Upcoming';
  return 'Ongoing';
}

function toPeriodResponse(item: MockPeriod, year: number): AwardPeriodResponse {
  const fromDate = bindDate(item.fromDay, item.fromMonth, year);
  const toDate = bindDate(item.toDay, item.toMonth, year);
  const status = statusOf(fromDate, toDate);
  return {
    id: item.id,
    name: item.name,
    fromDay: item.fromDay,
    fromMonth: item.fromMonth,
    toDay: item.toDay,
    toMonth: item.toMonth,
    fromDisplay: `${pad(item.fromDay)}/${pad(item.fromMonth)}`,
    toDisplay: `${pad(item.toDay)}/${pad(item.toMonth)}`,
    year,
    fromDate,
    toDate,
    status,
    daysRemaining: status === 'Upcoming' ? dayjs(fromDate).diff(dayjs(MOCK_TODAY), 'day') : null,
    eligibleCount: MOCK_ELIGIBLE[item.code]?.[String(year)]?.total ?? 0,
  };
}

function memberByCode(code: string): MockMember | undefined {
  return members.find((item) => item.code === code);
}

function toEligible(
  code: string,
  milestone: number,
  milestoneDate: string,
): EligibleMemberResponse {
  const member = memberByCode(code);
  return {
    partyMemberId: member?.id ?? code,
    fullName: member?.fullName ?? code,
    gender: member?.gender ?? null,
    dateOfBirth: member?.dateOfBirth ?? null,
    officialAdmissionDate: member?.officialAdmissionDate ?? milestoneDate,
    milestoneDate,
    milestone,
  };
}

function eligibleOf(periodCode: string, year: number) {
  return MOCK_ELIGIBLE[periodCode]?.[String(year)] ?? { total: 0, byMilestone: [], rows: [] };
}

function gapsOf(year: number): CoverageGap[] {
  return MOCK_GAPS[String(year)] ?? [];
}

function unassignedOf(year: number): UnassignedMemberResponse[] {
  const bucket = MOCK_UNASSIGNED[String(year)];
  if (!bucket) return [];
  return bucket.rows.map((row) => ({
    ...toEligible(row.code, row.milestone, row.milestoneDate),
    gap: row.gap,
  }));
}

/** Các đợt đã gắn năm, sắp theo Từ ngày — dùng chung cho dải độ phủ và cảnh báo. */
function boundPeriods(year: number) {
  return [...periods]
    .map((item) => ({
      item,
      fromDate: bindDate(item.fromDay, item.fromMonth, year),
      toDate: bindDate(item.toDay, item.toMonth, year),
    }))
    .sort((a, b) => a.fromDate.localeCompare(b.fromDate));
}

/**
 * Dải độ phủ 12 tháng (UC-36): mỗi đợt một đoạn `Period` riêng — hai đợt chồng
 * lấn cho ra hai đoạn trùng nhau, giao diện xếp chúng thành hai hàng — cộng các
 * đoạn `Gap` cho phần cả năm chưa đợt nào phủ.
 */
function coverageSegments(year: number): CoverageSegment[] {
  const bound = boundPeriods(year);
  const segments: CoverageSegment[] = bound.map(({ item, fromDate, toDate }) => ({
    type: 'Period',
    periodId: item.id,
    name: item.name,
    fromDate,
    toDate,
  }));

  // Con trỏ chạy theo mốc phủ XA NHẤT đã đạt, nhờ vậy một đợt nằm lọt trong đợt
  // khác không sinh ra khoảng trống ảo.
  let cursor = dayjs(`${year}-01-01`);
  const lastDay = dayjs(`${year}-12-31`);
  for (const { fromDate, toDate } of bound) {
    const from = dayjs(fromDate);
    if (from.isAfter(cursor)) {
      segments.push({
        type: 'Gap',
        periodId: null,
        name: null,
        fromDate: cursor.format('YYYY-MM-DD'),
        toDate: from.subtract(1, 'day').format('YYYY-MM-DD'),
      });
    }
    const next = dayjs(toDate).add(1, 'day');
    if (next.isAfter(cursor)) cursor = next;
  }
  if (!cursor.isAfter(lastDay)) {
    segments.push({
      type: 'Gap',
      periodId: null,
      name: null,
      fromDate: cursor.format('YYYY-MM-DD'),
      toDate: lastDay.format('YYYY-MM-DD'),
    });
  }

  return segments.sort((a, b) => a.fromDate.localeCompare(b.fromDate));
}

/** Từng cặp đợt trùng ngày nhau trong năm (QT6) — cảnh báo, không chặn lưu. */
function overlapsOf(year: number): CoverageOverlap[] {
  const bound = boundPeriods(year);
  const result: CoverageOverlap[] = [];

  for (let i = 0; i < bound.length; i += 1) {
    for (let j = i + 1; j < bound.length; j += 1) {
      const first = bound[i];
      const second = bound[j];
      const from = first.fromDate > second.fromDate ? first.fromDate : second.fromDate;
      const to = first.toDate < second.toDate ? first.toDate : second.toDate;
      if (from > to) continue;
      result.push({
        firstPeriodId: first.item.id,
        firstPeriodName: first.item.name,
        secondPeriodId: second.item.id,
        secondPeriodName: second.item.name,
        fromDate: from,
        toDate: to,
        fromDisplay: dayjs(from).format('DD/MM'),
        toDisplay: dayjs(to).format('DD/MM'),
      });
    }
  }
  return result;
}

/** Gói cảnh báo của một năm, dùng cho cả danh sách lẫn thêm/sửa/xóa. */
function warningsOf(year: number) {
  return { overlaps: overlapsOf(year), gaps: gapsOf(year) };
}

function settingsResponse(): SettingsResponse {
  return {
    startYears: settings.startYears,
    endYears: settings.endYears,
    stepYears: settings.stepYears,
    unitName: settings.unitName,
    milestones: settings.milestones,
    milestoneCount: settings.milestones.length,
    updatedAt: `${MOCK_TODAY}T01:00:00Z`,
  };
}

/** QT1 thuộc về Backend; bản giả chỉ dựng lại dãy để đi hết luồng xem trước. */
function buildMilestones(start: number, end: number, step: number): number[] {
  const list: number[] = [];
  for (let value = start; value <= end; value += step) list.push(value);
  return list;
}

/* ------------------------------------------------------------------ */
/* Bộ định tuyến giả                                                    */
/* ------------------------------------------------------------------ */

interface MockContext {
  path: string;
  method: string;
  params: Record<string, unknown>;
  body: Record<string, unknown>;
  config: InternalAxiosRequestConfig;
}

/** Lỗi theo hình dạng B của hợp đồng (mục 1.4). */
class MockFailure extends Error {
  readonly status: number;
  readonly key: string;

  constructor(status: number, key: string) {
    super(key);
    this.status = status;
    this.key = key;
  }
}

function requireSession(context: MockContext): void {
  const header = String(context.config.headers?.Authorization ?? '');
  if (header !== `Bearer ${MOCK_TOKEN}`) {
    throw new MockFailure(401, 'Mes.User.Login.Failed');
  }
}

function numberParam(value: unknown, fallback: number): number {
  const parsed = Number(value);
  return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback;
}

function yearParam(context: MockContext): number {
  return numberParam(context.params.year, MOCK_CURRENT_YEAR);
}

function session(): SessionInfo {
  return {
    username: MOCK_ACCOUNT.username,
    displayName: MOCK_ACCOUNT.displayName,
    unitName: settings.unitName,
    serverDate: MOCK_TODAY,
    expiresAt: `${MOCK_TODAY}T16:30:00Z`,
  };
}

function searchMembers(context: MockContext): PagedResult<PartyMemberResponse> {
  const keyword = String(context.params.searchKeyword ?? '').trim();
  const genderFilter = String(context.params['filter.Gender'] ?? '').replace('$eq:', '');
  const sortQuery = String(context.params.sortQuery ?? 'FullName asc');
  const current = numberParam(context.params.current, 1);
  const pageSize = numberParam(context.params.pageSize, 20);

  // Không phân biệt hoa thường, CÓ phân biệt dấu (OQ-8).
  const lowered = keyword.toLocaleLowerCase('vi');
  let rows = members.filter((item) =>
    keyword ? item.fullName.toLocaleLowerCase('vi').includes(lowered) : true,
  );
  if (genderFilter) rows = rows.filter((item) => item.gender === genderFilter);

  const [field, direction] = sortQuery.split(' ');
  const collator = new Intl.Collator('vi');
  const pick = (item: MockMember): string => {
    if (field === 'DateOfBirth') return item.dateOfBirth ?? '';
    if (field === 'Gender') return item.gender ?? '';
    if (field === 'OfficialAdmissionDate') return item.officialAdmissionDate;
    return item.fullName;
  };
  rows = [...rows].sort((a, b) => {
    const result =
      field === 'FullName' ? collator.compare(pick(a), pick(b)) : pick(a).localeCompare(pick(b));
    return direction === 'desc' ? -result : result;
  });

  const totalCount = rows.length;
  const start = (current - 1) * pageSize;
  return {
    pagedData: rows.slice(start, start + pageSize).map(toMemberResponse),
    pageInfo: {
      totalCount,
      pageSize,
      current,
      totalPages: Math.max(1, Math.ceil(totalCount / pageSize)),
      hasNext: start + pageSize < totalCount,
      hasPrevious: current > 1,
    },
  };
}

function dashboard(): DashboardResponse {
  const year = MOCK_CURRENT_YEAR;
  const bound = periods.map((item) => ({ item, response: toPeriodResponse(item, year) }));
  const upcoming =
    bound.find((entry) => entry.response.status === 'Ongoing') ??
    bound.find((entry) => entry.response.status === 'Upcoming') ??
    null;
  const eligible = upcoming ? eligibleOf(upcoming.item.code, year) : null;

  return {
    today: MOCK_TODAY,
    currentYear: year,
    unitName: settings.unitName,
    memberCount: members.length,
    periodCount: periods.length,
    upcomingPeriod: upcoming
      ? {
          id: upcoming.response.id,
          name: upcoming.response.name,
          year,
          isNextYear: false,
          fromDate: upcoming.response.fromDate,
          toDate: upcoming.response.toDate,
          fromDisplay: upcoming.response.fromDisplay,
          toDisplay: upcoming.response.toDisplay,
          status: upcoming.response.status,
          daysRemaining: upcoming.response.daysRemaining,
          eligibleCount: eligible?.total ?? 0,
          milestoneBreakdown: eligible?.byMilestone ?? [],
        }
      : null,
    eligibleMembers: (eligible?.rows ?? []).map((row) =>
      toEligible(row.code, row.milestone, row.milestoneDate),
    ),
    warnings: {
      noMembers: members.length === 0,
      noPeriods: periods.length === 0,
      unassignedYear: year,
      unassignedCount: MOCK_UNASSIGNED[String(year)]?.total ?? 0,
      overlaps: [],
      gaps: gapsOf(year),
    },
  };
}

function periodList(context: MockContext): AwardPeriodListResponse {
  const year = yearParam(context);
  return {
    year,
    today: MOCK_TODAY,
    totalCount: periods.length,
    periods: periods
      .map((item) => toPeriodResponse(item, year))
      .sort((a, b) => a.fromDate.localeCompare(b.fromDate)),
    warnings: warningsOf(year),
    coverage: { segments: coverageSegments(year) },
  };
}

function findPeriod(id: string): MockPeriod {
  const found = periods.find((item) => item.id === id);
  if (!found) throw new MockFailure(400, 'Mes.AwardPeriod.NotFound');
  return found;
}

function savePeriod(
  payload: AwardPeriodPayload,
  existing?: MockPeriod,
): AwardPeriodMutationResponse {
  const name = payload.name?.trim();
  if (!name) throw new MockFailure(400, 'Mes.AwardPeriod.Required.Name');
  const duplicated = periods.some(
    (item) =>
      item.id !== existing?.id &&
      item.name.toLocaleLowerCase('vi') === name.toLocaleLowerCase('vi'),
  );
  if (duplicated) throw new MockFailure(400, 'Mes.AwardPeriod.Repeated.Name');
  if (
    payload.fromMonth > payload.toMonth ||
    (payload.fromMonth === payload.toMonth && payload.fromDay > payload.toDay)
  ) {
    throw new MockFailure(400, 'Mes.AwardPeriod.Invalid.Range');
  }

  const saved: MockPeriod = {
    code: existing?.code ?? `NEW-${periods.length + 1}`,
    id: existing?.id ?? newId('22222222'),
    name,
    fromDay: payload.fromDay,
    fromMonth: payload.fromMonth,
    toDay: payload.toDay,
    toMonth: payload.toMonth,
    status: 'Upcoming',
    daysRemaining: null,
  };
  periods = existing
    ? periods.map((item) => (item.id === existing.id ? saved : item))
    : [...periods, saved];

  return {
    period: toPeriodResponse(saved, MOCK_CURRENT_YEAR),
    warnings: warningsOf(MOCK_CURRENT_YEAR),
  };
}

function memberFromPayload(body: Record<string, unknown>): MockMember {
  const fullName = String(body.fullName ?? '').trim();
  if (!fullName) throw new MockFailure(400, 'Mes.PartyMember.Required.FullName');
  const officialAdmissionDate = String(body.officialAdmissionDate ?? '');
  if (!officialAdmissionDate) {
    throw new MockFailure(400, 'Mes.PartyMember.Required.OfficialAdmissionDate');
  }
  if (dayjs(officialAdmissionDate).isAfter(dayjs(MOCK_TODAY))) {
    throw new MockFailure(400, 'Mes.PartyMember.Invalid.OfficialAdmissionDate');
  }
  const dateOfBirth = (body.dateOfBirth as string | null) ?? null;
  if (dateOfBirth && !dayjs(dateOfBirth).isBefore(dayjs(officialAdmissionDate))) {
    throw new MockFailure(400, 'Mes.PartyMember.Invalid.DateOfBirth');
  }

  return {
    code: `NEW-${members.length + 1}`,
    id: newId('11111111'),
    fullName,
    dateOfBirth,
    gender: (body.gender as Gender | null) ?? null,
    officialAdmissionDate,
    partyAgeYears: dayjs(MOCK_TODAY).diff(dayjs(officialAdmissionDate), 'year'),
    // Mốc kế tiếp là việc của Backend — bản giả để trống thay vì tự suy ra.
    nextMilestone: null,
    nextMilestoneDate: null,
  };
}

function excelBlob(lines: string[]): Blob {
  // Bản giả chỉ cần một file tải về được để đi hết luồng; nội dung thật do
  // Backend sinh (mục 1.9 và mục 8 của hợp đồng).
  return new Blob([`\uFEFF${lines.join('\n')}`], {
    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  });
}

/** Tên đợt rút gọn cho tên file, theo quy ước mục 1.9. */
function shortName(name: string): string {
  return name
    .replace(/Đ/g, 'D')
    .replace(/đ/g, 'd')
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '')
    .replace(/\s+/g, '')
    .replace(/[^a-zA-Z0-9]+/g, '-')
    .replace(/^-|-$/g, '');
}

interface MockFile {
  blob: Blob;
  fileName: string;
}

function handleMembersById(context: MockContext): unknown {
  const id = context.path.slice('/PartyMembers/'.length);
  const existing = members.find((item) => item.id === id);
  if (!existing) throw new MockFailure(400, 'Mes.PartyMember.NotFound');
  if (context.method === 'put') {
    const updated = { ...memberFromPayload(context.body), code: existing.code, id: existing.id };
    members = members.map((item) => (item.id === id ? updated : item));
    return toMemberResponse(updated);
  }
  if (context.method === 'delete') {
    members = members.filter((item) => item.id !== id);
    return { id };
  }
  return toMemberResponse(existing);
}

function handleSettings(context: MockContext): unknown {
  if (context.method === 'get') return settingsResponse();
  const payload = context.body as unknown as SettingsPayload;
  if (payload.startYears < 1) throw new MockFailure(400, 'Mes.AppSetting.Invalid.StartYears');
  if (payload.endYears < 1) throw new MockFailure(400, 'Mes.AppSetting.Invalid.EndYears');
  if (payload.stepYears < 1) throw new MockFailure(400, 'Mes.AppSetting.Invalid.StepYears');
  if (payload.endYears < payload.startYears) {
    throw new MockFailure(400, 'Mes.AppSetting.Invalid.Range');
  }
  settings.startYears = payload.startYears;
  settings.endYears = payload.endYears;
  settings.stepYears = payload.stepYears;
  settings.unitName = payload.unitName?.trim() ? payload.unitName.trim() : null;
  settings.milestones = buildMilestones(payload.startYears, payload.endYears, payload.stepYears);
  return settingsResponse();
}

function handleJson(context: MockContext): unknown {
  const { path, method } = context;

  if (method === 'post' && path === '/Auth/Login') {
    const username = String(context.body.username ?? '');
    const password = String(context.body.password ?? '');
    if (username !== MOCK_ACCOUNT.username || password !== MOCK_ACCOUNT.password) {
      throw new MockFailure(401, 'Mes.User.Login.Failed');
    }
    const response: LoginResponse = { ...session(), accessToken: MOCK_TOKEN, tokenType: 'Bearer' };
    return response;
  }
  if (method === 'post' && path === '/Auth/Logout') return null;

  requireSession(context);

  if (method === 'get' && path === '/Auth/Me') return session();

  if (path === '/PartyMembers') {
    if (method === 'get') return searchMembers(context);
    const created = memberFromPayload(context.body);
    members = [...members, created];
    return toMemberResponse(created);
  }
  if (method === 'post' && path === '/PartyMembers/DeleteMany') {
    const ids = (context.body.ids as string[] | undefined) ?? [];
    const removed = members.filter((item) => ids.includes(item.id)).map((item) => item.id);
    members = members.filter((item) => !removed.includes(item.id));
    return { ids: removed };
  }
  if (method === 'post' && path === '/PartyMembers/Import/Preview') return MOCK_IMPORT_PREVIEW;
  if (method === 'post' && path === '/PartyMembers/Import/Commit') {
    const result: ImportResult = {
      importedCount: MOCK_IMPORT_PREVIEW.validCount,
      skippedCount: MOCK_IMPORT_PREVIEW.errorCount,
    };
    return result;
  }
  if (path.startsWith('/PartyMembers/')) return handleMembersById(context);

  if (path === '/AwardPeriods') {
    if (method === 'get') return periodList(context);
    return savePeriod(context.body as unknown as AwardPeriodPayload);
  }
  if (path.startsWith('/AwardPeriods/')) {
    const existing = findPeriod(path.slice('/AwardPeriods/'.length));
    if (method === 'put')
      return savePeriod(context.body as unknown as AwardPeriodPayload, existing);
    if (method === 'delete') {
      periods = periods.filter((item) => item.id !== existing.id);
      return { id: existing.id, warnings: warningsOf(MOCK_CURRENT_YEAR) };
    }
    return toPeriodResponse(existing, yearParam(context));
  }

  if (method === 'get' && path === '/Dashboard') return dashboard();

  if (method === 'get' && path === '/Eligibility') {
    const year = yearParam(context);
    const period = findPeriod(String(context.params.awardPeriodId ?? ''));
    const bucket = eligibleOf(period.code, year);
    const response: EligibilityResponse = {
      awardPeriod: toPeriodResponse(period, year),
      year,
      totalCount: bucket.total,
      milestoneBreakdown: bucket.byMilestone,
      members: bucket.rows.map((row) => toEligible(row.code, row.milestone, row.milestoneDate)),
    };
    return response;
  }
  if (method === 'get' && path === '/Eligibility/Unassigned') {
    const year = yearParam(context);
    const rows = unassignedOf(year);
    const response: UnassignedResponse = { year, totalCount: rows.length, members: rows };
    return response;
  }
  if (method === 'get' && path === '/Eligibility/UnassignedCount') {
    const year = yearParam(context);
    return { year, count: MOCK_UNASSIGNED[String(year)]?.total ?? 0 };
  }

  if (path === '/Settings') return handleSettings(context);
  if (method === 'post' && path === '/Settings/RestoreDefaults') {
    settings.startYears = MOCK_SETTINGS.startYears;
    settings.endYears = MOCK_SETTINGS.endYears;
    settings.stepYears = MOCK_SETTINGS.stepYears;
    settings.milestones = [...MOCK_SETTINGS.milestones];
    return settingsResponse();
  }
  if (method === 'get' && path === '/Settings/Milestones') {
    const start = numberParam(context.params.start, settings.startYears);
    const end = numberParam(context.params.end, settings.endYears);
    const step = numberParam(context.params.step, settings.stepYears);
    if (end < start) throw new MockFailure(400, 'Mes.AppSetting.Invalid.Range');
    const milestones = buildMilestones(start, end, step);
    return { milestones, milestoneCount: milestones.length };
  }

  throw new MockFailure(400, 'Mes.Query.Invalid.Year');
}

function handleFile(context: MockContext): MockFile {
  const { path } = context;
  if (path === '/PartyMembers/Import/Template') {
    return {
      blob: excelBlob(['Họ tên,Ngày sinh,Giới tính,Ngày vào Đảng chính thức']),
      fileName: 'MauDanhSachDangVien.xlsx',
    };
  }

  requireSession(context);
  const year = yearParam(context);

  if (path === '/Exports/Unassigned') {
    const rows = unassignedOf(year);
    return {
      blob: excelBlob(['Chưa thuộc đợt nào', ...rows.map((row) => row.fullName)]),
      fileName: `ChuaThuocDot_${year}.xlsx`,
    };
  }

  const period =
    path === '/Exports/Dashboard'
      ? periods.find((item) => toPeriodResponse(item, year).status !== 'Past')
      : findPeriod(String(context.params.awardPeriodId ?? ''));
  if (!period) throw new MockFailure(400, 'Mes.Dashboard.NotFound.UpcomingPeriod');

  const bucket = eligibleOf(period.code, year);
  const names = bucket.rows.map(
    (row) => toEligible(row.code, row.milestone, row.milestoneDate).fullName,
  );
  return {
    blob: excelBlob([period.name, ...names]),
    fileName: `DuDieuKien_${shortName(period.name)}_${year}.xlsx`,
  };
}

/* ------------------------------------------------------------------ */
/* Nối vào axios                                                        */
/* ------------------------------------------------------------------ */

function contextOf(config: InternalAxiosRequestConfig): MockContext {
  const base = config.baseURL ?? '';
  const url = config.url ?? '';
  const path = (url.startsWith(base) ? url.slice(base.length) : url).split('?')[0];
  let body: Record<string, unknown> = {};
  if (typeof config.data === 'string') {
    try {
      body = JSON.parse(config.data) as Record<string, unknown>;
    } catch {
      body = {};
    }
  }
  return {
    path,
    method: (config.method ?? 'get').toLowerCase(),
    params: (config.params ?? {}) as Record<string, unknown>,
    body,
    config,
  };
}

function reply(
  config: InternalAxiosRequestConfig,
  data: unknown,
  headers: Record<string, string> = {},
): AxiosResponse {
  return {
    data,
    status: 200,
    statusText: 'OK',
    headers: new AxiosHeaders(headers),
    config,
  };
}

function fail(config: InternalAxiosRequestConfig, failure: MockFailure): AxiosError {
  const response: AxiosResponse = {
    data: { statusCode: failure.status, message: failure.key },
    status: failure.status,
    statusText: 'Error',
    headers: new AxiosHeaders(),
    config,
  };
  return new AxiosError(failure.key, String(failure.status), config, null, response);
}

const wait = (ms: number) => new Promise((resolve) => window.setTimeout(resolve, ms));

/**
 * Bật tầng dữ liệu giả. Gọi đúng một lần, trước khi dựng giao diện.
 * Tắt bằng cách bỏ `VITE_USE_MOCK=true` — không phải sửa dòng mã nào khác.
 */
export function installMockAdapter(): void {
  http.defaults.adapter = async (config) => {
    await wait(LATENCY_MS);
    const context = contextOf(config);
    try {
      if (config.responseType === 'blob') {
        const file = handleFile(context);
        const name = encodeURIComponent(file.fileName);
        return reply(config, file.blob, {
          'content-disposition': `attachment; filename="${file.fileName}"; filename*=UTF-8''${name}`,
        });
      }
      return reply(config, { message: 'Mes.Mock.Successfully', data: handleJson(context) });
    } catch (error) {
      if (error instanceof MockFailure) throw fail(config, error);
      throw error;
    }
  };
}
