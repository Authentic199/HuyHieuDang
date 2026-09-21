import type { PagedQuery, PagedResult } from '../types/api';
import type { Gender, IsoDate, PartyMemberResponse } from '../types/domain';
import { apiClient } from './httpClient';

/**
 * Giá trị lọc của cột Mốc kế tiếp: một mốc của dãy QT1, hoặc `'None'` cho nhóm
 * đã vượt mốc lớn nhất (cột hiển thị dấu "—").
 */
export type NextMilestoneValue = number | 'None';

/** UC-20 — tìm theo họ tên, lọc theo giới tính và mốc kế tiếp, sắp xếp, phân trang. */
export interface MemberSearchQuery extends PagedQuery {
  /** Bỏ trống nghĩa là "Tất cả" */
  gender?: Gender;
  /** Bỏ trống nghĩa là "Tất cả" (T51) */
  nextMilestone?: NextMilestoneValue;
}

/** UC-21, UC-22 — 4 trường của modal thêm/sửa. Sửa phải gửi đủ cả 4. */
export interface MemberPayload {
  fullName: string;
  dateOfBirth: IsoDate | null;
  gender: Gender | null;
  officialAdmissionDate: IsoDate;
}

/**
 * Cột sắp xếp được (mục 1.7 hợp đồng API v1.5).
 *
 * Bốn cột đầu là cột thật của bảng. `PartyAge` và `NextMilestone` là giá trị tính ra;
 * Backend nhận chúng rồi quy đổi về `OfficialAdmissionDate` theo chiều ngược lại (T51).
 * Riêng `nextMilestoneDate` vẫn không sắp được — ngày tròn mốc không cùng thứ tự với
 * ngày vào Đảng nên không quy đổi được.
 */
export const SORTABLE_MEMBER_FIELDS = [
  'FullName',
  'DateOfBirth',
  'Gender',
  'OfficialAdmissionDate',
  'PartyAge',
  'NextMilestone',
] as const;

export function searchMembers(query: MemberSearchQuery): Promise<PagedResult<PartyMemberResponse>> {
  const { gender, nextMilestone, searchKeyword, searchFields, ...rest } = query;
  return apiClient.get<PagedResult<PartyMemberResponse>>('/PartyMembers', {
    params: {
      ...rest,
      // Chỉ gửi từ khóa khi có chữ, tránh gọi thừa tham số rỗng.
      ...(searchKeyword?.trim()
        ? { searchKeyword: searchKeyword.trim(), searchFields: searchFields ?? ['FullName'] }
        : {}),
      // "Tất cả" thì bỏ hẳn tham số lọc.
      ...(gender ? { 'filter.Gender': `$eq:${gender}` } : {}),
      ...(nextMilestone === undefined ? {} : { 'filter.NextMilestone': `$eq:${nextMilestone}` }),
    },
  });
}

export function getMember(id: string): Promise<PartyMemberResponse> {
  return apiClient.get<PartyMemberResponse>(`/PartyMembers/${id}`);
}

export function createMember(payload: MemberPayload): Promise<PartyMemberResponse> {
  return apiClient.post<PartyMemberResponse>('/PartyMembers', payload);
}

export function updateMember(id: string, payload: MemberPayload): Promise<PartyMemberResponse> {
  return apiClient.put<PartyMemberResponse>(`/PartyMembers/${id}`, payload);
}

/**
 * UC-23 — xóa các dòng đã chọn. Xóa hẳn, không có thùng rác (QT10).
 * Đây là đường xóa duy nhất: xóa một người thì truyền mảng một phần tử.
 * Trả về đúng các id đã xóa được; id không còn tồn tại bị bỏ qua lặng lẽ.
 */
export function deleteManyMembers(ids: string[]): Promise<{ ids: string[] }> {
  return apiClient.post<{ ids: string[] }>('/PartyMembers/DeleteMany', { ids });
}
