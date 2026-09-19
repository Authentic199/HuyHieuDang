import type { IsoDateTime } from '../types/domain';
import { apiClient } from './httpClient';

/** UC-50, UC-51 — cài mốc tuổi đảng và tên đơn vị. */
export interface SettingsResponse {
  startYears: number;
  endYears: number;
  stepYears: number;
  unitName: string | null;
  /** Dãy mốc đã sinh theo QT1 — nguồn sự thật duy nhất, Frontend không tự suy ra */
  milestones: number[];
  milestoneCount: number;
  updatedAt: IsoDateTime;
}

export interface SettingsPayload {
  startYears: number;
  endYears: number;
  stepYears: number;
  unitName: string | null;
}

export function getSettings(): Promise<SettingsResponse> {
  return apiClient.get<SettingsResponse>('/Settings');
}

/** Đổi cài đặt làm mọi danh sách đủ điều kiện thay đổi ngay (QT5). */
export function updateSettings(payload: SettingsPayload): Promise<SettingsResponse> {
  return apiClient.put<SettingsResponse>('/Settings', payload);
}

/** Đặt lại 30 / 90 / 5. Không đụng tới tên đơn vị. */
export function restoreDefaults(): Promise<SettingsResponse> {
  return apiClient.post<SettingsResponse>('/Settings/RestoreDefaults');
}

/**
 * Xem trước dãy mốc ngay khi người dùng gõ, trước khi bấm Lưu.
 * Nên hoãn 300 ms sau mỗi lần gõ rồi mới gọi.
 */
export function previewMilestones(params: {
  start?: number;
  end?: number;
  step?: number;
}): Promise<{ milestones: number[]; milestoneCount: number }> {
  return apiClient.get<{ milestones: number[]; milestoneCount: number }>('/Settings/Milestones', {
    params,
  });
}
