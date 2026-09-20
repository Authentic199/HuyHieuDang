import fs from 'node:fs';
import path from 'node:path';

import type { Page } from '@playwright/test';

import { ARTIFACT_DIR } from './env';
import { readWorkbook, trimTrailingEmptyRows, type Workbook } from './xlsx';

/**
 * Bắt tệp tải về từ trình duyệt rồi MỞ RA ĐỌC (yêu cầu của E2E-5).
 *
 * Giao diện tải tệp bằng thẻ `<a download>` dựng từ blob, nên Playwright vẫn
 * bắt được qua sự kiện `download`; tên tệp lấy đúng tên Backend đặt.
 */

export interface ExcelDownload {
  fileName: string;
  workbook: Workbook;
  /** Toàn bộ lưới ô, đã bỏ các dòng trống thừa ở cuối. */
  rows: string[][];
  /** Ba dòng tiêu đề + dòng trắng phía trên bảng. */
  titleRows: string[][];
  /** Dòng tiêu đề cột. */
  headers: string[];
  /** Các dòng dữ liệu, không kể tiêu đề. */
  dataRows: string[][];
  /** Đường dẫn bản sao đã lưu lại để đính kèm báo cáo khi cần. */
  savedPath: string;
}

/** Dòng tiêu đề cột luôn là dòng đầu tiên có ô thứ nhất bằng "STT". */
function splitSections(rows: string[][]) {
  const headerIndex = rows.findIndex((row) => row[0] === 'STT');
  if (headerIndex < 0) {
    return { titleRows: rows, headers: [] as string[], dataRows: [] as string[][] };
  }
  return {
    titleRows: rows.slice(0, headerIndex),
    headers: rows[headerIndex],
    dataRows: rows.slice(headerIndex + 1),
  };
}

/** Bấm nút xuất, chờ tệp về, đọc nội dung. */
export async function downloadExcel(
  page: Page,
  trigger: () => Promise<void>,
): Promise<ExcelDownload> {
  const [download] = await Promise.all([page.waitForEvent('download'), trigger()]);

  fs.mkdirSync(ARTIFACT_DIR, { recursive: true });
  const savedPath = path.join(ARTIFACT_DIR, `${Date.now()}-${download.suggestedFilename()}`);
  await download.saveAs(savedPath);

  const workbook = readWorkbook(fs.readFileSync(savedPath));
  const rows = trimTrailingEmptyRows(workbook.first.rows);

  return {
    fileName: download.suggestedFilename(),
    workbook,
    rows,
    savedPath,
    ...splitSections(rows),
  };
}

/** Ô đầu tiên của các dòng tiêu đề, bỏ những dòng trống. */
export function titleLines(download: ExcelDownload): string[] {
  return download.titleRows.map((row) => row[0] ?? '').filter((cell) => cell !== '');
}
