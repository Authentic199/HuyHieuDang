import { useCallback, useEffect, useState } from 'react';

import { periodsApi } from '../../../api';
import { FALLBACK_MESSAGE } from '../../../api/messages';
import { ApiError } from '../../../types/api';
import type { AwardPeriodResponse } from '../../../types/domain';

/**
 * Đầu trang chi tiết đợt (UC-34): tên đợt, khoảng ngày và năm hiện tại của máy
 * chủ. Gọi `getPeriod` KHÔNG truyền `year` — năm đang xét do máy chủ ấn định,
 * giao diện không được lấy từ đồng hồ trình duyệt (mục 1.6 hợp đồng API).
 */

/** Một lần trả lời của máy chủ, gắn với đúng lần hỏi đã sinh ra nó. */
interface PeriodDetailState {
  /** Dấu nhận dạng của lần hỏi; khác dấu hiện tại nghĩa là đang tải lại */
  token: string;
  data: AwardPeriodResponse | null;
  error: string | null;
}

export interface UsePeriodDetailResult {
  period: AwardPeriodResponse | null;
  /** Năm hiện tại theo lịch MÁY CHỦ, dùng làm mốc giữa của bộ chọn năm */
  serverYear: number | null;
  loading: boolean;
  /** Câu tiếng Việt đã dịch, null khi không lỗi */
  error: string | null;
  reload: () => void;
}

export function usePeriodDetail(id: string): UsePeriodDetailResult {
  const [reloadCount, setReloadCount] = useState(0);
  const [state, setState] = useState<PeriodDetailState | null>(null);

  const token = `${id}#${reloadCount}`;
  const loading = state?.token !== token;
  const data = loading ? null : (state?.data ?? null);
  const error = loading ? null : (state?.error ?? null);

  useEffect(() => {
    let cancelled = false;

    periodsApi
      .getPeriod(id)
      .then((response) => {
        if (cancelled) return;
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
  }, [id, token]);

  const reload = useCallback(() => {
    setReloadCount((count) => count + 1);
  }, []);

  return {
    period: data,
    serverYear: data?.year ?? null,
    loading,
    error,
    reload,
  };
}
