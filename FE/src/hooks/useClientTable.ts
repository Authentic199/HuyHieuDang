import type { SortOrder, SorterResult, TablePaginationConfig } from 'antd/es/table/interface';
import { useCallback, useMemo, useState } from 'react';

import { formatNumber } from '../utils/format';

/**
 * Tìm — lọc theo mốc — sắp xếp — phân trang cho những bảng mà MÁY CHỦ đã trả
 * trọn mảng trong một lời gọi: Dashboard, Đợt trao huy hiệu, tab Đủ điều kiện,
 * Chưa thuộc đợt nào.
 *
 * Bốn danh sách này có trần thật (một đảng ủy, một đợt vài chục người, mỗi năm
 * tối đa 12 đợt) nên làm hoàn toàn ở máy khách; hợp đồng API không phải mở lại.
 * Màn Đảng viên thì khác — nó phân trang ở máy chủ và KHÔNG dùng hook này.
 *
 * Quy ước giữ trùng khít màn Đảng viên: 10/20/50/100 dòng mỗi trang, mặc định
 * 20, `showTotal` dạng `1–20 / 1.342`, và ba trạng thái sắp xếp của Ant Design
 * (tăng dần → giảm dần → bỏ sắp xếp) — lần nhấn thứ ba trả về đúng thứ tự máy
 * chủ đã sắp.
 */

/** Các lựa chọn cho ô "… / trang" ở chân bảng. */
export const TABLE_PAGE_SIZES = [10, 20, 50, 100];

/** Số dòng mỗi trang khi mới vào màn. */
export const DEFAULT_TABLE_PAGE_SIZE = 20;

/** Không sắp xếp theo cột nào — giữ nguyên thứ tự máy chủ trả về. */
export const NO_SORT_KEY = '';

/** Giá trị "Tất cả mốc" của ô lọc mốc tuổi đảng. */
export const ALL_MILESTONES = 'All';

/** Mốc đang lọc: một con số cụ thể, hoặc 'All' khi xem tất cả. */
export type MilestoneFilter = number | typeof ALL_MILESTONES;

/** So sánh tăng dần cho một cột. Khóa của map trùng `key` của cột trong bảng. */
export type RowComparators<T> = Record<string, (a: T, b: T) => number>;

export interface ClientTableOptions<T> {
  /** Trọn danh sách máy chủ trả về, đã ở đúng thứ tự mặc định của nó */
  rows: T[];
  /** Chuỗi đem so với ô tìm — thường là họ tên hoặc tên đợt */
  searchTextOf: (row: T) => string;
  /** Mốc huy hiệu của dòng; bỏ trống khi bảng không có cột mốc */
  milestoneOf?: (row: T) => number;
  /** Các cột sắp xếp được */
  comparators: RowComparators<T>;
}

export interface MilestoneOption {
  label: string;
  value: MilestoneFilter;
}

export interface ClientTableResult<T> {
  keyword: string;
  setKeyword: (keyword: string) => void;
  milestone: MilestoneFilter;
  setMilestone: (milestone: MilestoneFilter) => void;
  /** Chỉ những mốc thật sự có trong dữ liệu đang xem, kèm số lượng */
  milestoneOptions: MilestoneOption[];
  /** Mũi tên sắp xếp của một cột; null nghĩa là cột đó đang không sắp */
  sortOrderOf: (key: string) => SortOrder;
  /** Dòng của đúng trang đang xem, sau khi tìm — lọc — sắp xếp */
  pageRows: T[];
  /** Tổng số dòng còn lại sau khi tìm — lọc */
  filteredCount: number;
  /** Tổng số dòng máy chủ trả về, không phụ thuộc ô tìm hay ô lọc */
  totalCount: number;
  /** Cộng vào `index` của cột STT để số thứ tự chạy tiếp qua từng trang */
  indexOffset: number;
  /** Đang tìm hoặc đang lọc — dùng để chọn đúng câu chữ cho bảng trống */
  isFiltered: boolean;
  /** Xóa ô tìm và ô lọc, về lại trọn danh sách */
  clearFilters: () => void;
  /** Đưa thẳng vào prop `pagination` của Table */
  pagination: TablePaginationConfig;
  /** Đưa thẳng vào prop `onChange` của Table */
  onTableChange: (
    pagination: TablePaginationConfig,
    filters: unknown,
    sorter: SorterResult<T> | SorterResult<T>[],
  ) => void;
}

/** So chuỗi theo thứ tự chữ cái tiếng Việt. */
const collator = new Intl.Collator('vi', { sensitivity: 'base', numeric: true });

/** So chuỗi; ô trống luôn xuống cuối, không lẫn vào đầu danh sách. */
export function compareText(a: string | null | undefined, b: string | null | undefined): number {
  const left = a?.trim() ?? '';
  const right = b?.trim() ?? '';
  if (left === '' || right === '') return left === right ? 0 : left === '' ? 1 : -1;
  return collator.compare(left, right);
}

/** So ngày dạng `yyyy-MM-dd`: so chuỗi là đủ và đúng thứ tự thời gian. */
export function compareDate(a: string | null | undefined, b: string | null | undefined): number {
  return compareText(a, b);
}

/** So số; thiếu số thì xuống cuối. */
export function compareNumber(a: number | null | undefined, b: number | null | undefined): number {
  const hasLeft = a !== null && a !== undefined;
  const hasRight = b !== null && b !== undefined;
  if (!hasLeft || !hasRight) return hasLeft === hasRight ? 0 : hasLeft ? -1 : 1;
  return a - b;
}

/**
 * Tìm là CHỨA CHUỖI, KHÔNG phân biệt hoa thường, CÓ phân biệt dấu — bằng đúng
 * hành vi máy chủ ở màn Đảng viên. Cố ý không bỏ dấu tiếng Việt: hai màn tìm
 * theo hai kiểu còn khó hiểu hơn là cùng một kiểu hơi khắt khe.
 */
export function matchesKeyword(text: string, keyword: string): boolean {
  return text.toLowerCase().includes(keyword.toLowerCase());
}

export function useClientTable<T>({
  rows,
  searchTextOf,
  milestoneOf,
  comparators,
}: ClientTableOptions<T>): ClientTableResult<T> {
  const [keyword, setKeywordState] = useState('');
  const [milestone, setMilestoneState] = useState<MilestoneFilter>(ALL_MILESTONES);
  const [sortKey, setSortKey] = useState<string>(NO_SORT_KEY);
  const [descending, setDescending] = useState(false);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(DEFAULT_TABLE_PAGE_SIZE);

  /**
   * Danh sách mốc đếm trên TRỌN dữ liệu đang xem, không đếm lại theo ô tìm: nếu
   * đếm theo ô tìm thì mốc đang chọn có thể biến mất ngay giữa lúc người dùng
   * đang gõ, và các lựa chọn nhảy liên tục.
   */
  const milestoneOptions = useMemo<MilestoneOption[]>(() => {
    if (!milestoneOf) return [];
    const counts = new Map<number, number>();
    for (const row of rows) {
      const value = milestoneOf(row);
      counts.set(value, (counts.get(value) ?? 0) + 1);
    }
    return [
      { label: 'Tất cả mốc', value: ALL_MILESTONES },
      ...[...counts.entries()]
        .sort((a, b) => a[0] - b[0])
        .map(([value, count]) => ({
          label: `${formatNumber(value)} năm (${formatNumber(count)})`,
          value,
        })),
    ];
  }, [rows, milestoneOf]);

  // Đổi năm hay tải lại thì mốc đang chọn có thể không còn trong dữ liệu mới —
  // tự về "Tất cả mốc" thay vì để bảng trống mà không rõ vì sao.
  const hasSelectedMilestone =
    milestone === ALL_MILESTONES || milestoneOptions.some((option) => option.value === milestone);
  const activeMilestone: MilestoneFilter = hasSelectedMilestone ? milestone : ALL_MILESTONES;

  const trimmedKeyword = keyword.trim();
  const isFiltered = trimmedKeyword !== '' || activeMilestone !== ALL_MILESTONES;

  const filteredRows = useMemo(() => {
    if (!isFiltered) return rows;
    return rows.filter((row) => {
      if (trimmedKeyword !== '' && !matchesKeyword(searchTextOf(row), trimmedKeyword)) return false;
      if (activeMilestone !== ALL_MILESTONES && milestoneOf?.(row) !== activeMilestone) {
        return false;
      }
      return true;
    });
  }, [rows, isFiltered, trimmedKeyword, activeMilestone, searchTextOf, milestoneOf]);

  const sortedRows = useMemo(() => {
    const comparator = comparators[sortKey];
    if (!comparator) return filteredRows;
    // Array.prototype.sort giữ nguyên thứ tự của những dòng bằng nhau, nên trong
    // mỗi nhóm bằng nhau thứ tự máy chủ vẫn được tôn trọng.
    const sorted = [...filteredRows].sort(comparator);
    return descending ? sorted.reverse() : sorted;
  }, [filteredRows, comparators, sortKey, descending]);

  const filteredCount = sortedRows.length;
  const totalPages = Math.max(1, Math.ceil(filteredCount / pageSize));
  // Lọc xong còn ít dòng hơn thì trang đang xem có thể vượt quá số trang mới.
  const current = Math.min(page, totalPages);
  const indexOffset = (current - 1) * pageSize;
  const pageRows = useMemo(
    () => sortedRows.slice(indexOffset, indexOffset + pageSize),
    [sortedRows, indexOffset, pageSize],
  );

  const setKeyword = useCallback((next: string) => {
    setKeywordState(next);
    setPage(1);
  }, []);

  const setMilestone = useCallback((next: MilestoneFilter) => {
    setMilestoneState(next);
    setPage(1);
  }, []);

  const clearFilters = useCallback(() => {
    setKeywordState('');
    setMilestoneState(ALL_MILESTONES);
    setPage(1);
  }, []);

  const sortOrderOf = useCallback(
    (key: string): SortOrder => {
      if (key !== sortKey) return null;
      return descending ? 'descend' : 'ascend';
    },
    [sortKey, descending],
  );

  const onTableChange = useCallback<ClientTableResult<T>['onTableChange']>(
    (nextPagination, _filters, sorter) => {
      const single = Array.isArray(sorter) ? sorter[0] : sorter;
      const field = single?.columnKey as string | undefined;
      // Ant Design cho ba trạng thái: tăng dần → giảm dần → bỏ sắp xếp. Lần
      // nhấn thứ ba trả về order rỗng, nghĩa là quay lại thứ tự máy chủ.
      const nextSortKey = field && single?.order ? field : NO_SORT_KEY;
      const nextDescending = single?.order === 'descend';
      const nextPageSize = nextPagination.pageSize ?? pageSize;
      const resetsPage =
        nextSortKey !== sortKey || nextDescending !== descending || nextPageSize !== pageSize;

      setSortKey(nextSortKey);
      setDescending(nextDescending);
      setPageSize(nextPageSize);
      setPage(resetsPage ? 1 : (nextPagination.current ?? 1));
    },
    [sortKey, descending, pageSize],
  );

  return {
    keyword,
    setKeyword,
    milestone: activeMilestone,
    setMilestone,
    milestoneOptions,
    sortOrderOf,
    pageRows,
    filteredCount,
    totalCount: rows.length,
    indexOffset,
    isFiltered,
    clearFilters,
    pagination: {
      current,
      pageSize,
      total: filteredCount,
      showSizeChanger: true,
      pageSizeOptions: TABLE_PAGE_SIZES,
      showTotal: (total, range) =>
        `${formatNumber(range[0])}–${formatNumber(range[1])} / ${formatNumber(total)}`,
    },
    onTableChange,
  };
}
