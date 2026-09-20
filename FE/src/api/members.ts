import type { PagedQuery, PagedResult } from '../types/api';
import type { Gender, IsoDate, PartyMemberResponse } from '../types/domain';
import { apiClient } from './httpClient';

/** UC-20 — tìm theo họ tên, lọc theo giới tính, sắp xếp, phân trang. */
export interface MemberSearchQuery extends PagedQuery {
  /** Bỏ trống nghĩa là "Tất cả" */
  gender?: Gender;
}

/** UC-21, UC-22 — 4 trường của modal thêm/sửa. Sửa phải gửi đủ cả 4. */
export interface MemberPayload {
  fullName: string;
  dateOfBirth: IsoDate | null;
  gender: Gender | null;
  officialAdmissionDate: IsoDate;
}

/**
 * Cột sắp xếp được: FullName, DateOfBirth, Gender, OfficialAdmissionDate.
 * Tuổi đảng và mốc kế tiếp là giá trị tính ra nên không sắp xếp được (mục 1.7).
 */
export const SORTABLE_MEMBER_FIELDS = [
  'FullName',
  'DateOfBirth',
  'Gender',
  'OfficialAdmissionDate',
] as const;

export function searchMembers(query: MemberSearchQuery): Promise<PagedResult<PartyMemberResponse>> {
  const { gender, searchKeyword, searchFields, ...rest } = query;
  return apiClient.get<PagedResult<PartyMemberResponse>>('/PartyMembers', {
    params: {
      ...rest,
      // Chỉ gửi từ khóa khi có chữ, tránh gọi thừa tham số rỗng.
      ...(searchKeyword?.trim()
        ? { searchKeyword: searchKeyword.trim(), searchFields: searchFields ?? ['FullName'] }
        : {}),
      // "Tất cả" thì bỏ hẳn tham số lọc.
      ...(gender ? { 'filter.Gender': `$eq:${gender}` } : {}),
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
