import { useCallback, useEffect, useState } from 'react';

import { periodsApi } from '../../../api';
import { FALLBACK_MESSAGE } from '../../../api/messages';
import type { EligibilityResponse } from '../../../api/periods';
import { ApiError } from '../../../types/api';
import type { AwardPeriodResponse, EligibleMemberResponse } from '../../../types/domain';

/**
 * Tab "Danh sách đủ điều kiện" (UC-34). Đổi năm chỉ là gọi lại đúng endpoint
 * này với `year` khác — khoảng ngày, số người và bảng đều lấy từ một lời gọi.
 *
 * Danh sách máy chủ đã sắp theo Mốc rồi Họ tên, giao diện giữ nguyên thứ tự đó.
 */

interface EligibilityState {
  /** Dấu nhận dạng của lần hỏi; khác dấu hiện tại nghĩa là đang tải lại */
  token: string;
  data: EligibilityResponse | null;
  error: string | null;
}

export interface UseEligibilityResult {
  /** Đợt đã gắn năm đang chọn: `fromDate`/`toDate` là ngày thật của năm đó */
  awardPeriod: AwardPeriodResponse | null;
  members: EligibleMemberResponse[];
  totalCount: number;
  loading: boolean;
  /** Câu tiếng Việt đã dịch, null khi không lỗi */
  error: string | null;
  reload: () => void;
}

/**
 * `year` null nghĩa là chưa biết năm của máy chủ — cứ để bảng ở trạng thái tải.
 * `refreshToken` đổi giá trị thì tải lại: dùng khi đợt vừa được sửa, khoảng ngày
 * đổi nên danh sách cũng đổi theo.
 */
export function useEligibility(
  awardPeriodId: string,
  year: number | null,
  refreshToken = 0,
): UseEligibilityResult {
  const [reloadCount, setReloadCount] = useState(0);
  const [state, setState] = useState<EligibilityState | null>(null);

  const token = `${awardPeriodId}#${year ?? ''}#${refreshToken}#${reloadCount}`;
  const ready = year !== null;
  const loading = !ready || state?.token !== token;
  const data = loading ? null : (state?.data ?? null);
  const error = loading ? null : (state?.error ?? null);

  useEffect(() => {
    if (!ready) return;
    let cancelled = false;

    periodsApi
      .getEligibility(awardPeriodId, year)
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
  }, [awardPeriodId, ready, token, year]);

  const reload = useCallback(() => {
    setReloadCount((count) => count + 1);
  }, []);

  return {
    awardPeriod: data?.awardPeriod ?? null,
    members: data?.members ?? [],
    totalCount: data?.totalCount ?? 0,
    loading,
    error,
    reload,
  };
}
