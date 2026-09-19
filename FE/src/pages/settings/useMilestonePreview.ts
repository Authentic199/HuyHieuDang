import { useEffect, useState } from 'react';

import { settingsApi } from '../../api';
import { FALLBACK_MESSAGE } from '../../api/messages';
import type { SettingsResponse } from '../../api/settings';
import { ApiError } from '../../types/api';
import {
  PREVIEW_DEBOUNCE_MS,
  firstMilestoneError,
  type MilestoneErrors,
  type MilestoneInput,
} from './milestones';

/**
 * Ô "Xem trước dãy mốc" của màn Cài đặt (mục 7.4 hợp đồng API).
 *
 * Dãy mốc do MÁY CHỦ sinh, giao diện không tự tính QT1. Người dùng ngừng gõ
 * 300 ms thì mới hỏi, và chỉ hỏi khi cả ba ô hợp lệ. Trong lúc chờ thì giữ
 * nguyên dãy cũ (thẻ gọi vẽ mờ đi) để màn hình không nháy.
 */
export interface MilestonePreview {
  milestones: number[];
  /** null khi chưa biết số mốc — ô đếm hiện dấu gạch ngang */
  count: number | null;
  /** Đang chờ máy chủ trả lời cho bộ số vừa gõ */
  pending: boolean;
  /** Câu lỗi tiếng Việt: lỗi ràng buộc phía giao diện hoặc lỗi máy chủ */
  error: string | null;
}

/** Một lần trả lời, gắn với đúng bộ ba số đã sinh ra nó. */
interface PreviewState {
  key: string;
  milestones: number[];
  count: number | null;
  error: string | null;
}

function inputKey(input: MilestoneInput): string {
  return `${input.startYears ?? ''}/${input.endYears ?? ''}/${input.stepYears ?? ''}`;
}

/** Ba số đang gõ có đúng bằng ba số đã lưu hay không. */
function matchesSaved(input: MilestoneInput, saved: SettingsResponse | null): boolean {
  if (!saved) return false;
  return (
    input.startYears === saved.startYears &&
    input.endYears === saved.endYears &&
    input.stepYears === saved.stepYears
  );
}

export function useMilestonePreview(
  input: MilestoneInput,
  errors: MilestoneErrors,
  saved: SettingsResponse | null,
): MilestonePreview {
  const [state, setState] = useState<PreviewState | null>(null);

  const invalidMessage = firstMilestoneError(errors);
  const key = inputKey(input);
  // Bộ số đúng bằng bộ đang lưu thì đã có sẵn dãy của máy chủ, khỏi hỏi lại.
  const useSaved = matchesSaved(input, saved);
  const shouldAsk = invalidMessage === null && !useSaved;

  useEffect(() => {
    if (!shouldAsk) return;

    let cancelled = false;
    const timer = setTimeout(() => {
      settingsApi
        .previewMilestones({
          start: input.startYears ?? undefined,
          end: input.endYears ?? undefined,
          step: input.stepYears ?? undefined,
        })
        .then((response) => {
          if (cancelled) return;
          setState({
            key,
            milestones: response.milestones,
            count: response.milestoneCount,
            error: null,
          });
        })
        .catch((reason: unknown) => {
          if (cancelled) return;
          setState({
            key,
            milestones: [],
            count: null,
            error: reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE,
          });
        });
    }, PREVIEW_DEBOUNCE_MS);

    return () => {
      cancelled = true;
      clearTimeout(timer);
    };
    // `key` đã gói đủ ba số của lần hỏi này.
  }, [input.startYears, input.endYears, input.stepYears, key, shouldAsk]);

  // Ô nào chưa hợp lệ thì không vẽ dãy nào, chỉ nói vì sao.
  if (invalidMessage !== null) {
    return { milestones: [], count: null, pending: false, error: invalidMessage };
  }

  if (useSaved && saved) {
    return {
      milestones: saved.milestones,
      count: saved.milestoneCount,
      pending: false,
      error: null,
    };
  }

  if (state?.key === key) {
    return { ...state, pending: false };
  }

  // Chưa có trả lời cho bộ số này: giữ dãy gần nhất, thẻ gọi sẽ vẽ mờ.
  return {
    milestones: state?.milestones ?? saved?.milestones ?? [],
    count: state?.count ?? saved?.milestoneCount ?? null,
    pending: true,
    error: null,
  };
}
