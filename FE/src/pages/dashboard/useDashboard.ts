import { useCallback, useEffect, useState } from 'react';

import { dashboardApi } from '../../api';
import type { DashboardResponse } from '../../api/dashboard';
import { FALLBACK_MESSAGE } from '../../api/messages';
import { ApiError } from '../../types/api';

/**
 * Màn 2 — Dashboard (UC-10, UC-11, UC-12, UC-13).
 *
 * MỘT lời gọi `GET /api/Dashboard` dựng cả màn, kể cả trạng thái trống (mục 6.1
 * hợp đồng API): ngày hôm nay, năm đang xét, bốn loại cảnh báo, thẻ đợt sắp tới,
 * phân bổ theo mốc và danh sách đủ điều kiện. Giao diện không tự tính gì thêm và
 * KHÔNG lấy ngày từ đồng hồ trình duyệt — mọi con số đều do máy chủ trả.
 */

interface DashboardState {
  data: DashboardResponse | null;
  /** Câu tiếng Việt đã dịch, null khi không lỗi */
  error: string | null;
}

export interface UseDashboardResult {
  data: DashboardResponse | null;
  loading: boolean;
  error: string | null;
  reload: () => void;
}

export function useDashboard(): UseDashboardResult {
  const [reloadCount, setReloadCount] = useState(0);
  const [state, setState] = useState<DashboardState | null>(null);
  /** Số lần tải đã hoàn tất; khác `reloadCount` nghĩa là đang tải. */
  const [loadedCount, setLoadedCount] = useState<number | null>(null);

  const loading = loadedCount !== reloadCount;

  useEffect(() => {
    let cancelled = false;

    dashboardApi
      .getDashboard()
      .then((response) => {
        if (cancelled) return;
        setState({ data: response, error: null });
        setLoadedCount(reloadCount);
      })
      .catch((reason: unknown) => {
        if (cancelled) return;
        setState({
          data: null,
          error: reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE,
        });
        setLoadedCount(reloadCount);
      });

    return () => {
      cancelled = true;
    };
  }, [reloadCount]);

  const reload = useCallback(() => {
    setReloadCount((count) => count + 1);
  }, []);

  return {
    data: loading ? null : (state?.data ?? null),
    loading,
    error: loading ? null : (state?.error ?? null),
    reload,
  };
}
