import { useCallback, useEffect, useState } from 'react';

import { uncoveredApi } from '../../api';
import { FALLBACK_MESSAGE } from '../../api/messages';
import type { UnassignedResponse } from '../../api/uncovered';
import { ApiError } from '../../types/api';
import type { UnassignedMemberResponse } from '../../types/domain';

/**
 * Màn "Chưa thuộc đợt nào" (UC-40, QT7).
 *
 * Lần gọi ĐẦU TIÊN cố ý KHÔNG truyền `year`: năm đang xét do máy chủ ấn định và
 * đó chính là năm giữa của bộ chọn năm — giao diện không được lấy từ đồng hồ
 * trình duyệt (mục 1.6 hợp đồng API). Đổi năm chỉ là gọi lại endpoint này với
 * `year` khác; khối gợi ý, số người và bảng đều lấy từ một lời gọi.
 *
 * Danh sách máy chủ đã sắp theo Ngày tròn mốc tăng dần rồi Họ tên (mục 6.3),
 * giao diện giữ nguyên thứ tự đó.
 */

/** Một lần trả lời của máy chủ, gắn với đúng lần hỏi đã sinh ra nó. */
interface UnassignedState {
  /** Dấu nhận dạng của lần hỏi; khác dấu hiện tại nghĩa là đang tải lại */
  token: string;
  data: UnassignedResponse | null;
  error: string | null;
}

export interface UseUnassignedResult {
  /** Năm hiện tại theo lịch MÁY CHỦ, dùng làm mốc giữa của bộ chọn năm */
  serverYear: number | null;
  /** Năm đang xem; null khi chưa biết năm của máy chủ */
  year: number | null;
  selectYear: (year: number) => void;
  members: UnassignedMemberResponse[];
  totalCount: number;
  loading: boolean;
  /** Câu tiếng Việt đã dịch, null khi không lỗi */
  error: string | null;
  reload: () => void;
}

export function useUnassigned(): UseUnassignedResult {
  const [reloadCount, setReloadCount] = useState(0);
  /** null nghĩa là chưa tự chọn năm nào — cứ để máy chủ quyết năm đang xét. */
  const [selectedYear, setSelectedYear] = useState<number | null>(null);
  const [serverYear, setServerYear] = useState<number | null>(null);
  const [state, setState] = useState<UnassignedState | null>(null);

  const token = `${selectedYear ?? ''}#${reloadCount}`;
  const loading = state?.token !== token;
  const data = loading ? null : (state?.data ?? null);
  const error = loading ? null : (state?.error ?? null);

  useEffect(() => {
    let cancelled = false;

    uncoveredApi
      .getUnassigned(selectedYear ?? undefined)
      .then((response) => {
        if (cancelled) return;
        // Chỉ lần gọi không truyền năm mới cho biết năm hiện tại của máy chủ.
        if (selectedYear === null) setServerYear(response.year);
        setState({ token, data: response, error: null });
      })
      .catch((reason: unknown) => {
        if (cancelled) return;
        setState({
          token,
          data: null,
          error: reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE,
        });
      });

    return () => {
      cancelled = true;
    };
    // `token` đã gói đủ mọi tham số của lần hỏi này.
  }, [selectedYear, token]);

  const reload = useCallback(() => {
    setReloadCount((count) => count + 1);
  }, []);

  return {
    serverYear,
    year: selectedYear ?? serverYear,
    selectYear: setSelectedYear,
    members: data?.members ?? [],
    totalCount: data?.totalCount ?? 0,
    loading,
    error,
    reload,
  };
}
