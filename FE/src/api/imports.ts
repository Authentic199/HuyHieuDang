import type { Gender, IsoDate } from '../types/domain';
import { apiClient, type DownloadedFile } from './httpClient';

/** Một dòng hợp lệ đã chuẩn hóa (UC-24 bước 2). */
export interface ImportValidRow {
  /** Số dòng trong file Excel; dòng tiêu đề là 1 nên dữ liệu bắt đầu từ 2 */
  rowNumber: number;
  fullName: string;
  dateOfBirth: IsoDate | null;
  gender: Gender | null;
  officialAdmissionDate: IsoDate;
}

/** Mã lý do của dòng bị loại. Một dòng có thể có nhiều lý do (OQ-2). */
export type ImportErrorCode =
  | 'MissingFullName'
  | 'MissingOfficialAdmissionDate'
  | 'InvalidDateFormat'
  | 'FutureOfficialAdmissionDate'
  | 'InvalidGender'
  | 'BirthDateAfterAdmissionDate';

/** Tên cột gây lỗi, dùng để tô đỏ đúng ô trong bảng dòng lỗi. */
export type ImportErrorField = 'FullName' | 'DateOfBirth' | 'Gender' | 'OfficialAdmissionDate';

/** Một lý do của dòng lỗi. */
export interface ImportRowError {
  errorCode: ImportErrorCode;
  field: ImportErrorField;
}

/**
 * Một dòng bị loại. Mọi trường giữ nguyên chữ thô đọc từ ô Excel — chính chúng
 * đang sai nên không chuẩn hóa được.
 */
export interface ImportErrorRow {
  rowNumber: number;
  fullName: string;
  dateOfBirth: string;
  gender: string;
  officialAdmissionDate: string;
  /** Luôn có ít nhất một phần tử, sắp theo thứ tự bảng mã lỗi (OQ-2) */
  errors: ImportRowError[];
}

/** Chữ hiển thị ở cột "Lý do" của bảng dòng lỗi. */
export const IMPORT_ERROR_TEXTS: Record<ImportErrorCode, string> = {
  MissingFullName: 'Thiếu họ tên',
  MissingOfficialAdmissionDate: 'Thiếu ngày vào Đảng chính thức',
  InvalidDateFormat: 'Sai định dạng ngày (cần dd/MM/yyyy)',
  FutureOfficialAdmissionDate: 'Ngày chính thức ở tương lai',
  InvalidGender: 'Giới tính chỉ nhận Nam hoặc Nữ',
  BirthDateAfterAdmissionDate: 'Ngày sinh phải trước ngày vào Đảng chính thức',
};

/** Ghép mọi lý do của một dòng thành chữ cho cột "Lý do", ngăn bằng "; ". */
export function importErrorText(row: ImportErrorRow): string {
  return row.errors.map((item) => IMPORT_ERROR_TEXTS[item.errorCode]).join('; ');
}

/** Những cột bị tô đỏ của một dòng lỗi. */
export function importErrorFields(row: ImportErrorRow): ImportErrorField[] {
  return Array.from(new Set(row.errors.map((item) => item.field)));
}

export interface ImportPreview {
  fileName: string;
  totalRows: number;
  validCount: number;
  errorCount: number;
  validRows: ImportValidRow[];
  errorRows: ImportErrorRow[];
}

export interface ImportResult {
  importedCount: number;
  skippedCount: number;
}

/** UC-25 — tải file mẫu 4 cột. */
export function downloadTemplate(): Promise<DownloadedFile> {
  return apiClient.download('/PartyMembers/Import/Template', 'MauDanhSachDangVien.xlsx');
}

/** Bước 2 — chỉ kiểm tra, không ghi gì vào cơ sở dữ liệu. */
export function previewImport(file: File): Promise<ImportPreview> {
  return apiClient.upload<ImportPreview>('/PartyMembers/Import/Preview', file);
}

/**
 * Bước 3 — gửi lại đúng file vừa xem trước. Máy chủ không giữ trạng thái giữa
 * hai bước nên Frontend phải giữ đối tượng File từ bước 1.
 * Nạp mọi dòng hợp lệ, bỏ qua dòng lỗi, trong một giao dịch (QT9).
 */
export function commitImport(file: File): Promise<ImportResult> {
  return apiClient.upload<ImportResult>('/PartyMembers/Import/Commit', file);
}
