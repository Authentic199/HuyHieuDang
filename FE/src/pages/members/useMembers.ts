import { useCallback, useEffect, useRef, useState } from 'react';

import { membersApi } from '../../api';
import type { MemberSearchQuery } from '../../api/members';
import { FALLBACK_MESSAGE } from '../../api/messages';
import { ApiError, type PageInfo } from '../../types/api';
import type { Gender, PartyMemberResponse } from '../../types/domain';

/**
 * Toàn bộ việc gọi API của màn Đảng viên nằm ở đây, màn hình chỉ dựng giao diện
 * (quy tắc 6 trong CLAUDE.md).
 */

/** 'All' là lựa chọn "Tất cả" của ô lọc giới tính, không gửi lên máy chủ. */
export type GenderFilter = Gender | 'All';

/** Trạng thái tìm — lọc — sắp xếp — phân trang mà người dùng đang chọn. */
export interface MembersQuery {
  keyword: string;
  gender: GenderFilter;
  current: number;
  pageSize: number;
  /**
   * Ví dụ 'FullName asc'. Chỉ nhận 4 cột trong SORTABLE_MEMBER_FIELDS.
   * Rỗng nghĩa là người dùng đã bỏ sắp xếp — xem NO_SORT_QUERY.
   */
  sortQuery: string;
}

/**
 * Trạng thái "bỏ sắp xếp": nhấn lần thứ ba vào tên cột thì về đây. Không gửi
 * tham số nào lên máy chủ, để máy chủ tự dùng thứ tự mặc định `FullName asc`
 * (mục 1.7 hợp đồng API) — thứ tự luôn xác định nên phân trang không nhảy dòng.
 */
export const NO_SORT_QUERY = '';

/** Số dòng mỗi trang mặc định lấy theo mục 1.7 của hợp đồng API. */
export const DEFAULT_MEMBERS_QUERY: MembersQuery = {
  keyword: '',
  gender: 'All',
  current: 1,
  pageSize: 20,
  sortQuery: NO_SORT_QUERY,
};

/** Các lựa chọn số dòng mỗi trang cho ô "… / trang" ở chân bảng. */
export const MEMBERS_PAGE_SIZES = [10, 20, 50, 100];

/** Một lần trả lời của máy chủ, gắn với đúng lần hỏi đã sinh ra nó. */
interface MembersResult {
  /** Dấu nhận dạng của lần hỏi; khác dấu hiện tại nghĩa là đang tải lại */
  token: string;
  rows: PartyMemberResponse[];
  pageInfo: PageInfo | null;
  error: string | null;
}

export interface UseMembersResult {
  query: MembersQuery;
  rows: PartyMemberResponse[];
  pageInfo: PageInfo | null;
  loading: boolean;
  /** Câu tiếng Việt đã dịch, null khi không lỗi */
  error: string | null;
  /** Đang lọc hoặc đang tìm — dùng để chọn đúng câu chữ cho bảng trống */
  isFiltered: boolean;
  /** Đổi vài tham số; đổi bộ lọc thì tự quay về trang 1 */
  setQuery: (patch: Partial<MembersQuery>) => void;
  /** Tải lại đúng trang đang xem */
  reload: () => void;
  /**
   * Tải lại sau khi xóa. Nếu trang đang xem vừa bị xóa hết thì lùi về trang
   * trước, tránh để người dùng nhìn một trang trắng.
   */
  reloadAfterDelete: (removedCount: number) => void;
}

export function useMembers(): UseMembersResult {
  const [query, setQueryState] = useState<MembersQuery>(DEFAULT_MEMBERS_QUERY);
  const [reloadCount, setReloadCount] = useState(0);
  const [result, setResult] = useState<MembersResult | null>(null);
  // Số dòng đang hiển thị, đọc trong reloadAfterDelete mà không phải phụ thuộc state.
  const rowCountRef = useRef(0);

  const token = JSON.stringify([
    query.keyword,
    query.gender,
    query.current,
    query.pageSize,
    query.sortQuery,
    reloadCount,
  ]);

  // Chưa có kết quả của đúng lần hỏi hiện tại thì bảng đang tải. Cách này giữ
  // cờ "đang tải" mà không phải gọi setState ngay trong thân effect.
  const loading = result?.token !== token;
  const rows = loading ? [] : (result?.rows ?? []);
  const pageInfo = loading ? null : (result?.pageInfo ?? null);
  const error = loading ? null : (result?.error ?? null);

  // Ghi ngoài lượt dựng để reloadAfterDelete biết trang đang có bao nhiêu dòng.
  useEffect(() => {
    rowCountRef.current = rows.length;
  }, [rows.length]);

  useEffect(() => {
    let cancelled = false;

    const payload: MemberSearchQuery = {
      current: query.current,
      pageSize: query.pageSize,
      searchKeyword: query.keyword,
      // Bỏ sắp xếp thì bỏ luôn tham số, không gửi chuỗi rỗng.
      ...(query.sortQuery === NO_SORT_QUERY ? {} : { sortQuery: query.sortQuery }),
      ...(query.gender === 'All' ? {} : { gender: query.gender }),
    };

    membersApi
      .searchMembers(payload)
      .then((paged) => {
        if (cancelled) return;
        setResult({ token, rows: paged.pagedData, pageInfo: paged.pageInfo, error: null });
      })
      .catch((reason: unknown) => {
        if (cancelled) return;
        setResult({
          token,
          rows: [],
          pageInfo: null,
          error: reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE,
        });
      });

    return () => {
      cancelled = true;
    };
    // `token` đã gói đủ mọi tham số của lần hỏi này.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  const setQuery = useCallback((patch: Partial<MembersQuery>) => {
    setQueryState((previous) => {
      // Đổi từ khóa, bộ lọc hay cách sắp xếp thì trang cũ không còn đúng nữa —
      // luôn về trang 1 trừ khi chính trang được đổi.
      const changesResultSet =
        ('keyword' in patch && patch.keyword !== previous.keyword) ||
        ('gender' in patch && patch.gender !== previous.gender) ||
        ('pageSize' in patch && patch.pageSize !== previous.pageSize) ||
        ('sortQuery' in patch && patch.sortQuery !== previous.sortQuery);
      const next = { ...previous, ...patch };
      if (changesResultSet && patch.current === undefined) next.current = 1;
      // Không có gì đổi thì giữ nguyên đối tượng cũ, nếu không màn hình sẽ gọi
      // lại máy chủ mỗi lần ô tìm nhả nhịp chờ mà chữ vẫn y như trước.
      const unchanged = (Object.keys(next) as (keyof MembersQuery)[]).every(
        (key) => next[key] === previous[key],
      );
      return unchanged ? previous : next;
    });
  }, []);

  const reload = useCallback(() => {
    setReloadCount((count) => count + 1);
  }, []);

  const reloadAfterDelete = useCallback((removedCount: number) => {
    setQueryState((previous) =>
      removedCount >= rowCountRef.current && previous.current > 1
        ? { ...previous, current: previous.current - 1 }
        : previous,
    );
    setReloadCount((count) => count + 1);
  }, []);

  return {
    query,
    rows,
    pageInfo,
    loading,
    error,
    isFiltered: query.keyword.trim() !== '' || query.gender !== 'All',
    setQuery,
    reload,
    reloadAfterDelete,
  };
}
