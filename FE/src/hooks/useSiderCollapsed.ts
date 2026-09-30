import { useCallback, useState } from 'react';

import { NARROW_VIEWPORT_QUERY } from '../theme/breakpoints';
import { useMediaQuery } from './useMediaQuery';

/**
 * Trạng thái thu gọn của menu trái.
 *
 * Hai điều phải cùng đúng:
 * - Cửa sổ hẹp thì menu tự thu về dải biểu tượng để bảng còn chỗ mà hiện cột.
 * - Người dùng bấm nút thu gọn / mở rộng thì lựa chọn của họ thắng, và còn đó ở
 *   lần mở sau — kể cả khi cửa sổ thay đổi bề ngang.
 */

/** Nhớ lựa chọn thu gọn menu để lần mở sau vẫn như cũ. */
const COLLAPSED_STORAGE_KEY = 'hhd.siderCollapsed';

/** Lựa chọn đã ghi nhớ; `null` khi người dùng chưa bấm nút lần nào. */
function readStoredChoice(): boolean | null {
  try {
    const raw = window.localStorage.getItem(COLLAPSED_STORAGE_KEY);
    return raw === null ? null : raw === '1';
  } catch {
    /* Trình duyệt chặn localStorage — coi như chưa có lựa chọn nào. */
    return null;
  }
}

export interface SiderCollapsedState {
  collapsed: boolean;
  toggle: () => void;
}

export function useSiderCollapsed(): SiderCollapsedState {
  const [chosen, setChosen] = useState(readStoredChoice);
  const narrow = useMediaQuery(NARROW_VIEWPORT_QUERY);

  // Cửa sổ hẹp chỉ đổi giá trị MẶC ĐỊNH, không ghi đè lựa chọn của người dùng.
  const collapsed = chosen ?? narrow;

  const toggle = useCallback(() => {
    const next = !collapsed;
    setChosen(next);
    try {
      window.localStorage.setItem(COLLAPSED_STORAGE_KEY, next ? '1' : '0');
    } catch {
      /* Trình duyệt chặn localStorage — chỉ mất phần ghi nhớ, không sao. */
    }
  }, [collapsed]);

  return { collapsed, toggle };
}
