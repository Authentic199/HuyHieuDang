import { useCallback, useEffect, useState } from 'react';

import { periodsApi } from '../../api';
import type { AwardPeriodListResponse } from '../../api/periods';
import { FALLBACK_MESSAGE } from '../../api/messages';
import { ApiError } from '../../types/api';
import type { AwardPeriodResponse, CoverageWarnings } from '../../types/domain';

/**
 * Toàn bộ việc gọi API của màn Đợt trao huy hiệu nằm ở đây, màn hình chỉ dựng
 * giao diện (quy tắc 6 trong CLAUDE.md).
 *
 * Một lời gọi `getPeriods` trả đủ cho cả ba khối của màn: bảng, dải độ phủ và
 * banner cảnh báo — không tách thành nhiều lượt hỏi.
 */

const EMPTY_WARNINGS: CoverageWarnings = { overlaps: [], gaps: [] };

/** Một lần trả lời của máy chủ, gắn với đúng lần hỏi đã sinh ra nó. */
interface PeriodsResult {
  /** Dấu nhận dạng của lần hỏi; khác dấu hiện tại nghĩa là đang tải lại */
  token: string;
  data: AwardPeriodListResponse | null;
  error: string | null;
}

export interface UsePeriodsResult {
  /** Đã sắp theo Từ ngày, đúng thứ tự máy chủ trả */
  periods: AwardPeriodResponse[];
  /** Năm đang xét — màn này không có bộ chọn năm, luôn là năm hiện tại */
  year: number;
  /** Hôm nay theo lịch MÁY CHỦ, dùng cho vạch "Hôm nay" của dải độ phủ */
  today: string | null;
  totalCount: number;
  /** Các đoạn của dải độ phủ 12 tháng (UC-36) */
  segments: AwardPeriodListResponse['coverage']['segments'];
  /** Cảnh báo chồng lấn và chưa phủ kín, luôn có giá trị kể cả khi đang tải */
  warnings: CoverageWarnings;
  loading: boolean;
  /** Câu tiếng Việt đã dịch, null khi không lỗi */
  error: string | null;
  reload: () => void;
  /**
   * Thêm / sửa / xóa đều trả kèm cảnh báo mới — gọi hàm này để banner đổi ngay
   * trong lúc bảng đang tải lại, người dùng không phải chờ hai nhịp.
   */
  applyWarnings: (warnings: CoverageWarnings) => void;
}

export function usePeriods(): UsePeriodsResult {
  const [reloadCount, setReloadCount] = useState(0);
  const [result, setResult] = useState<PeriodsResult | null>(null);
  // Cảnh báo do thêm/sửa/xóa trả về, dùng tạm cho tới khi danh sách tải xong.
  const [pendingWarnings, setPendingWarnings] = useState<CoverageWarnings | null>(null);

  const token = String(reloadCount);
  const loading = result?.token !== token;
  const data = loading ? null : (result?.data ?? null);
  const error = loading ? null : (result?.error ?? null);

  useEffect(() => {
    let cancelled = false;

    periodsApi
      .getPeriods()
      .then((response) => {
        if (cancelled) return;
        setPendingWarnings(null);
        setResult({ token, data: response, error: null });
      })
      .catch((reason: unknown) => {
        if (cancelled) return;
        setResult({
          token,
          data: null,
          error: reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE,
        });
      });

    return () => {
      cancelled = true;
    };
    // `token` đã gói đủ mọi tham số của lần hỏi này.
  }, [token]);

  const reload = useCallback(() => {
    setReloadCount((count) => count + 1);
  }, []);

  const applyWarnings = useCallback((warnings: CoverageWarnings) => {
    setPendingWarnings(warnings);
  }, []);

  return {
    periods: data?.periods ?? [],
    year: data?.year ?? new Date().getFullYear(),
    today: data?.today ?? null,
    totalCount: data?.totalCount ?? 0,
    segments: data?.coverage.segments ?? [],
    warnings: pendingWarnings ?? data?.warnings ?? EMPTY_WARNINGS,
    loading,
    error,
    reload,
    applyWarnings,
  };
}
