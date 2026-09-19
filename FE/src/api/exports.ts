import { apiClient, type DownloadedFile } from './httpClient';

/**
 * Ba endpoint xuất Excel. Tên file do Backend đặt, Frontend chỉ dùng lại
 * (đọc từ Content-Disposition) — tên truyền vào đây chỉ là phương án dự phòng.
 */

/** UC-34 — xuất danh sách đủ điều kiện của một đợt. */
export function exportEligibility(awardPeriodId: string, year?: number): Promise<DownloadedFile> {
  return apiClient.download('/Exports/Eligibility', 'DuDieuKien.xlsx', {
    params: { awardPeriodId, year },
  });
}

/** UC-11 — xuất đúng danh sách đang hiện trên Dashboard (đợt sắp tới theo QT8). */
export function exportDashboard(): Promise<DownloadedFile> {
  return apiClient.download('/Exports/Dashboard', 'DuDieuKien.xlsx');
}

/** UC-40 — xuất danh sách chưa thuộc đợt nào. */
export function exportUnassigned(year?: number): Promise<DownloadedFile> {
  const suffix = year ?? new Date().getFullYear();
  return apiClient.download('/Exports/Unassigned', `ChuaThuocDot_${suffix}.xlsx`, {
    params: { year },
  });
}
