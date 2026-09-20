import fs from 'node:fs';
import path from 'node:path';

import { ADMIN, API_URL, EXCEL_DIR, UNIT_NAME as BASELINE_UNIT_NAME } from './env';
import { MAIN_PERIODS, type PeriodFixture } from './expected';

/**
 * Lối vào API dùng để DỰNG trạng thái trước mỗi luồng và để ĐỐI CHIẾU số liệu
 * trên giao diện với số liệu máy chủ (ca E1-12, E3-05 của T-FIX-5).
 *
 * Việc nghiệp vụ mà luồng đang kiểm thử thì luôn làm qua giao diện; lớp này
 * chỉ lo phần dọn dẹp và gieo dữ liệu nền cho nhanh và tất định.
 */

export interface PageInfo {
  totalCount: number;
  pageSize: number;
  current: number;
  totalPages: number;
}

export class ApiError extends Error {
  readonly status: number;
  readonly body: string;

  constructor(status: number, body: string, route: string) {
    super(`API ${status} khi gọi ${route}: ${body.slice(0, 400)}`);
    this.name = 'ApiError';
    this.status = status;
    this.body = body;
  }
}

export class Api {
  readonly token: string;

  private constructor(token: string) {
    this.token = token;
  }

  /** Đăng nhập bằng tài khoản admin seed sẵn. */
  static async login(): Promise<Api> {
    const response = await fetch(`${API_URL}/Auth/Login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username: ADMIN.username, password: ADMIN.password }),
    });
    const text = await response.text();
    if (!response.ok) throw new ApiError(response.status, text, '/Auth/Login');
    return new Api(JSON.parse(text).data.accessToken as string);
  }

  private async call<T>(method: string, route: string, body?: unknown): Promise<T> {
    const response = await fetch(`${API_URL}${route}`, {
      method,
      headers: {
        Authorization: `Bearer ${this.token}`,
        ...(body === undefined ? {} : { 'Content-Type': 'application/json' }),
      },
      ...(body === undefined ? {} : { body: JSON.stringify(body) }),
    });
    const text = await response.text();
    if (!response.ok) throw new ApiError(response.status, text, route);
    return (text ? JSON.parse(text).data : null) as T;
  }

  get<T>(route: string): Promise<T> {
    return this.call<T>('GET', route);
  }

  post<T>(route: string, body?: unknown): Promise<T> {
    return this.call<T>('POST', route, body ?? {});
  }

  put<T>(route: string, body: unknown): Promise<T> {
    return this.call<T>('PUT', route, body);
  }

  del<T>(route: string): Promise<T> {
    return this.call<T>('DELETE', route);
  }

  /** Gửi một tệp Excel lên endpoint nhận multipart. */
  async upload<T>(route: string, filePath: string): Promise<T> {
    const form = new FormData();
    const bytes = fs.readFileSync(filePath);
    form.append('file', new Blob([new Uint8Array(bytes)]), path.basename(filePath));
    const response = await fetch(`${API_URL}${route}`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${this.token}` },
      body: form,
    });
    const text = await response.text();
    if (!response.ok) throw new ApiError(response.status, text, route);
    return JSON.parse(text).data as T;
  }

  // ----- Đọc số liệu để đối chiếu với giao diện -----

  dashboard(): Promise<DashboardData> {
    return this.get<DashboardData>('/Dashboard');
  }

  periods(year?: number): Promise<PeriodListData> {
    return this.get<PeriodListData>(`/AwardPeriods${year ? `?year=${year}` : ''}`);
  }

  settings(): Promise<SettingsData> {
    return this.get<SettingsData>('/Settings');
  }

  unassignedCount(year?: number): Promise<{ year: number; count: number }> {
    return this.get(`/Eligibility/UnassignedCount${year ? `?year=${year}` : ''}`);
  }

  members(
    current = 1,
    pageSize = 500,
  ): Promise<{ pagedData: { id: string }[]; pageInfo: PageInfo }> {
    return this.get(`/PartyMembers?current=${current}&pageSize=${pageSize}`);
  }

  // ----- Dựng trạng thái -----

  /** Xóa hết đảng viên. Lặp theo trang vì một lần xóa chỉ nhận danh sách id. */
  async deleteAllMembers(): Promise<void> {
    for (;;) {
      const page = await this.members(1, 500);
      if (page.pagedData.length === 0) return;
      await this.post('/PartyMembers/DeleteMany', { ids: page.pagedData.map((row) => row.id) });
    }
  }

  /** Xóa hết đợt trao huy hiệu. */
  async deleteAllPeriods(): Promise<void> {
    const list = await this.periods();
    for (const period of list.periods) await this.del(`/AwardPeriods/${period.id}`);
  }

  /** Đưa cài đặt về 30 / 90 / 5 và bỏ tên đơn vị. */
  async resetSettings(): Promise<void> {
    await this.post('/Settings/RestoreDefaults');
    await this.put('/Settings', { startYears: 30, endYears: 90, stepYears: 5, unitName: null });
  }

  /** Kho hoàn toàn trống: không đảng viên, không đợt, cài đặt mặc định (E2E-1). */
  async resetAll(): Promise<void> {
    await this.deleteAllMembers();
    await this.deleteAllPeriods();
    await this.resetSettings();
  }

  /** Tạo bốn đợt chính theo đúng bộ dữ liệu biên. */
  async seedMainPeriods(periods: PeriodFixture[] = MAIN_PERIODS): Promise<void> {
    for (const period of periods) {
      await this.post('/AwardPeriods', {
        name: period.name,
        fromDay: period.fromDay,
        fromMonth: period.fromMonth,
        toDay: period.toDay,
        toMonth: period.toMonth,
      });
    }
  }

  /** Nạp một tệp Excel của bộ dữ liệu vào kho. */
  async seedExcel(fileName: string): Promise<{ importedCount: number; skippedCount: number }> {
    return this.upload('/PartyMembers/Import/Commit', path.join(EXCEL_DIR, fileName));
  }

  /** Lưu cài đặt mốc và tên đơn vị. */
  async saveSettings(payload: {
    startYears: number;
    endYears: number;
    stepYears: number;
    unitName: string | null;
  }): Promise<void> {
    await this.put('/Settings', payload);
  }

  /**
   * Trạng thái đầu dùng chung của E2E-3 đến E2E-6: bốn đợt chính, cài đặt
   * 30 / 90 / 5 kèm tên đơn vị, và bộ lõi 32 người.
   */
  async seedBaseline(): Promise<void> {
    await this.resetAll();
    await this.seedMainPeriods();
    await this.saveSettings({
      startYears: 30,
      endYears: 90,
      stepYears: 5,
      unitName: BASELINE_UNIT_NAME,
    });
    const imported = await this.seedExcel('core-hop-le.xlsx');
    if (imported.importedCount !== 32) {
      throw new Error(`Gieo bộ lõi phải được 32 người, thực tế ${imported.importedCount}`);
    }
  }

  /** Tìm một đợt theo tên — dùng để mở thẳng trang chi tiết. */
  async periodIdByName(name: string): Promise<string> {
    const list = await this.periods();
    const found = list.periods.find((item) => item.name === name);
    if (!found) throw new Error(`Không tìm thấy đợt tên "${name}" trên máy chủ`);
    return found.id;
  }
}

export interface DashboardData {
  today: string;
  currentYear: number;
  unitName: string | null;
  memberCount: number;
  periodCount: number;
  upcomingPeriod: {
    id: string;
    name: string;
    year: number;
    fromDate: string;
    toDate: string;
    daysRemaining: number | null;
    eligibleCount: number;
    isNextYear: boolean;
    milestoneBreakdown: { milestone: number; count: number }[];
  } | null;
  eligibleMembers: { fullName: string; milestone: number; milestoneDate: string }[];
  warnings: {
    noMembers: boolean;
    noPeriods: boolean;
    unassignedYear: number;
    unassignedCount: number;
    gaps: { fromDisplay: string; toDisplay: string }[];
    overlaps: { fromDisplay: string; toDisplay: string }[];
  };
}

export interface PeriodListData {
  year: number;
  today: string;
  totalCount: number;
  periods: {
    id: string;
    name: string;
    fromDisplay: string;
    toDisplay: string;
    status: string;
    daysRemaining: number | null;
    eligibleCount: number;
    fromDay: number;
    fromMonth: number;
    toDay: number;
    toMonth: number;
  }[];
  warnings: {
    gaps: { fromDisplay: string; toDisplay: string }[];
    overlaps: { fromDisplay: string; toDisplay: string }[];
  };
}

export interface SettingsData {
  startYears: number;
  endYears: number;
  stepYears: number;
  unitName: string | null;
  milestones: number[];
  milestoneCount: number;
}
