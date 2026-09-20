import fs from 'node:fs';
import path from 'node:path';

import { FIXTURES_DIR } from './env';

/**
 * Cửa đọc duy nhất vào `tests/fixtures/data/expected.json` — kết quả mong đợi
 * do `tests/fixtures/qt_reference.py` tính ra từ QT1–QT11.
 *
 * Bộ kiểm thử KHÔNG tự tính lại con số nghiệp vụ nào: mọi khẳng định đều so
 * với tệp này. Mã nguồn cho ra số khác nghĩa là một trong hai bên sai, và
 * không được sửa fixture cho khớp mã nguồn (xem tests/fixtures/README.md).
 */

function readJson<T>(...segments: string[]): T {
  return JSON.parse(fs.readFileSync(path.join(FIXTURES_DIR, ...segments), 'utf8')) as T;
}

export interface EligibleRow {
  code: string;
  fullName: string;
  milestone: number;
  anniversary: string;
  anniversaryDisplay: string;
}

export interface MissedRow extends EligibleRow {
  gapLabel: string;
}

export interface MemberRow {
  code: string;
  fullName: string;
  dateOfBirth: string | null;
  dateOfBirthDisplay: string;
  gender: string | null;
  genderDisplay: string;
  officialAdmissionDate: string;
  officialAdmissionDateDisplay: string;
  partyAge: number;
  nextMilestone: number | null;
  nextMilestoneDisplay: string;
  nextAnniversary: string | null;
  nextAnniversaryDisplay: string;
  intent: string;
}

export interface PeriodFixture {
  code: string;
  name: string;
  fromDay: number;
  fromMonth: number;
  toDay: number;
  toMonth: number;
  label: string;
}

export interface YearEligibility {
  boundFrom: string;
  boundTo: string;
  total: number;
  byMilestone: Record<string, number>;
  rows: EligibleRow[];
}

export interface Scenario {
  today: string;
  settings: {
    start_years: number;
    end_years: number;
    step_years: number;
    unit_name: string | null;
  };
  milestones: number[];
  milestoneCount: number;
  upcomingPeriod: {
    code: string;
    name: string;
    year: number;
    boundFromDisplay: string;
    boundToDisplay: string;
    status: string;
    daysLeft: number | null;
  } | null;
  periodStatuses: { code: string; name: string; status: string; daysLeft: number | null }[];
  eligibleByPeriod: Record<string, { name: string; byYear: Record<string, YearEligibility> }>;
  missedByYear: Record<string, { total: number; rows: MissedRow[] }>;
  gapsByYear: Record<string, { fromDisplay: string; toDisplay: string; label: string }[]>;
  badgeCurrentYear: number;
  overlapWarnings: unknown[];
}

interface ExpectedFile {
  fixedDates: { T0: string; T1: string; T2: string; timezone: string };
  counts: Record<string, number>;
  periods: { main: PeriodFixture[]; leapEdge: PeriodFixture[] };
  exportFileNames: Record<string, string>;
  memberList: Record<string, MemberRow[]>;
  scenarios: Record<string, Scenario>;
}

export const expected = readJson<ExpectedFile>('data', 'expected.json');

export const excelManifest = readJson<
  { file: string; validRows: number; errorRows: number; purpose: string }[]
>('data', 'excel-manifest.json');

/** Ba mốc thời gian cố định của kế hoạch kiểm thử. */
export const T0 = expected.fixedDates.T0;
export const T1 = expected.fixedDates.T1;
export const T2 = expected.fixedDates.T2;

/** Bốn đợt chính, đúng thứ tự Từ ngày. */
export const MAIN_PERIODS = expected.periods.main;

/** Lấy một kịch bản theo tên, ném lỗi nói rõ khi gõ sai tên. */
export function scenario(name: string): Scenario {
  const found = expected.scenarios[name];
  if (!found) {
    throw new Error(
      `Không có kịch bản "${name}" trong expected.json. Các kịch bản hiện có: ${Object.keys(
        expected.scenarios,
      ).join(', ')}`,
    );
  }
  return found;
}

/** Danh sách đủ điều kiện của một đợt trong một năm. */
export function eligibility(name: string, periodCode: string, year: number): YearEligibility {
  const period = scenario(name).eligibleByPeriod[periodCode];
  if (!period) throw new Error(`Kịch bản "${name}" không có đợt ${periodCode}`);
  const byYear = period.byYear[String(year)];
  if (!byYear) throw new Error(`Đợt ${periodCode} của "${name}" không có năm ${year}`);
  return byYear;
}

/** Danh sách "Chưa thuộc đợt nào" của một năm. */
export function missed(name: string, year: number) {
  const found = scenario(name).missedByYear[String(year)];
  if (!found) throw new Error(`Kịch bản "${name}" không có năm ${year} ở missedByYear`);
  return found;
}

/** Số dòng hợp lệ / dòng lỗi mong đợi của một tệp Excel trong bộ dữ liệu. */
export function excelCounts(file: string) {
  const found = excelManifest.find((item) => item.file === file);
  if (!found) throw new Error(`excel-manifest.json không mô tả tệp ${file}`);
  return found;
}

/** Một người của bộ lõi, tra theo mã ca biên (B01, S01, V01…). */
export function coreMember(
  code: string,
  list: 'T0_default' | 'T0_step10' = 'T0_default',
): MemberRow {
  const found = expected.memberList[list].find((item) => item.code === code);
  if (!found) throw new Error(`Bộ lõi không có ca biên ${code}`);
  return found;
}
