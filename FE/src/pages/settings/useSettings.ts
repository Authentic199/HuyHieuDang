import { useCallback, useEffect, useState } from 'react';

import { settingsApi } from '../../api';
import { FALLBACK_MESSAGE } from '../../api/messages';
import type { SettingsPayload, SettingsResponse } from '../../api/settings';
import { ApiError } from '../../types/api';

/**
 * Tầng dữ liệu của màn Cài đặt (UC-50, UC-51).
 *
 * Component chỉ nhận dữ liệu đã sẵn sàng và ba việc: lưu, khôi phục mặc định,
 * tải lại. Mọi lời gọi mạng nằm ở đây theo quy tắc 6 trong CLAUDE.md.
 */
export interface UseSettingsResult {
  settings: SettingsResponse | null;
  loading: boolean;
  /** Câu tiếng Việt đã dịch khi không đọc được cài đặt, null khi bình thường */
  error: string | null;
  saving: boolean;
  restoring: boolean;
  reload: () => void;
  /** Ném ApiError để màn hình gắn lỗi vào đúng ô nhập */
  save: (payload: SettingsPayload) => Promise<SettingsResponse>;
  restoreDefaults: () => Promise<SettingsResponse>;
}

/** Một lần trả lời của máy chủ, gắn với đúng lần hỏi đã sinh ra nó. */
interface SettingsState {
  /** Dấu nhận dạng của lần hỏi; khác dấu hiện tại nghĩa là đang tải lại */
  token: string;
  data: SettingsResponse | null;
  error: string | null;
}

export function useSettings(): UseSettingsResult {
  const [reloadCount, setReloadCount] = useState(0);
  const [state, setState] = useState<SettingsState | null>(null);
  const [saving, setSaving] = useState(false);
  const [restoring, setRestoring] = useState(false);

  const token = String(reloadCount);
  const loading = state?.token !== token;

  useEffect(() => {
    let cancelled = false;

    settingsApi
      .getSettings()
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
  }, [token]);

  const reload = useCallback(() => {
    setReloadCount((count) => count + 1);
  }, []);

  /** Ghi đè cài đặt đang giữ bằng bản máy chủ vừa trả về. */
  const accept = useCallback((response: SettingsResponse) => {
    setState((current) => (current ? { ...current, data: response, error: null } : current));
  }, []);

  const save = useCallback(
    async (payload: SettingsPayload) => {
      setSaving(true);
      try {
        const response = await settingsApi.updateSettings(payload);
        accept(response);
        return response;
      } finally {
        setSaving(false);
      }
    },
    [accept],
  );

  /** Chỉ đặt lại 30 / 90 / 5; tên đơn vị giữ nguyên (mục 7.3 hợp đồng API). */
  const restoreDefaults = useCallback(async () => {
    setRestoring(true);
    try {
      const response = await settingsApi.restoreDefaults();
      accept(response);
      return response;
    } finally {
      setRestoring(false);
    }
  }, [accept]);

  return {
    settings: loading ? null : (state?.data ?? null),
    loading,
    error: loading ? null : (state?.error ?? null),
    saving,
    restoring,
    reload,
    save,
    restoreDefaults,
  };
}
