import { App as AntApp } from 'antd';
import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from 'react';

import { authApi, SESSION_EXPIRED_MESSAGE, uncoveredApi, UNAUTHORIZED_EVENT } from '../api';
import type { SessionInfo } from '../api/auth';
import { clearToken, readToken, writeToken } from '../api/token';
import { AuthContext, type AuthContextValue } from './AuthContext';

/**
 * Giữ trạng thái đăng nhập cho toàn ứng dụng.
 * Không gọi axios trực tiếp — mọi lời gọi đi qua src/api.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const [isInitializing, setIsInitializing] = useState(true);
  const [session, setSession] = useState<SessionInfo | null>(null);
  const [uncoveredCount, setUncoveredCount] = useState(0);
  const { message } = AntApp.useApp();
  // Chỉ báo "hết phiên" khi người dùng đang làm dở; 401 lúc mở ứng dụng thì
  // chỉ lặng lẽ đưa về màn Đăng nhập.
  const hasSessionRef = useRef(false);

  useEffect(() => {
    hasSessionRef.current = session !== null;
  }, [session]);

  const clearSession = useCallback(() => {
    clearToken();
    setSession(null);
    setUncoveredCount(0);
  }, []);

  /** Badge trên menu trái; lỗi thì giữ số cũ chứ không làm hỏng màn đang xem. */
  const refreshUncoveredCount = useCallback(async () => {
    try {
      const { count } = await uncoveredApi.getUnassignedCount();
      setUncoveredCount(count);
    } catch {
      /* Không làm gì — badge không phải thông tin bắt buộc để dùng tiếp. */
    }
  }, []);

  const logout = useCallback(() => {
    // Máy chủ không huỷ token được (JWT không trạng thái) nên lỗi ở đây
    // cũng không ngăn việc đăng xuất phía người dùng.
    void authApi.logout().catch(() => undefined);
    clearSession();
  }, [clearSession]);

  // Thẻ hết hạn giữa chừng thì xóa thẻ, báo một câu và đẩy về trang Đăng nhập.
  useEffect(() => {
    function handleExpired() {
      if (hasSessionRef.current) {
        message.warning(SESSION_EXPIRED_MESSAGE);
      }
      clearSession();
    }

    window.addEventListener(UNAUTHORIZED_EVENT, handleExpired);
    return () => window.removeEventListener(UNAUTHORIZED_EVENT, handleExpired);
  }, [clearSession, message]);

  // Mở lại trình duyệt mà thẻ còn hạn thì vào thẳng, khỏi đăng nhập lại.
  useEffect(() => {
    let cancelled = false;

    async function restoreSession() {
      if (!readToken()) {
        if (!cancelled) setIsInitializing(false);
        return;
      }
      try {
        const current = await authApi.getSession();
        if (cancelled) return;
        setSession(current);
        void refreshUncoveredCount();
      } catch {
        clearToken();
      } finally {
        if (!cancelled) setIsInitializing(false);
      }
    }

    void restoreSession();
    return () => {
      cancelled = true;
    };
  }, [refreshUncoveredCount]);

  const login = useCallback(
    async (username: string, password: string) => {
      const { accessToken, ...info } = await authApi.login({ username, password });
      writeToken(accessToken);
      setSession(info);
      void refreshUncoveredCount();
    },
    [refreshUncoveredCount],
  );

  const setUnitName = useCallback((unitName: string | null) => {
    setSession((current) => (current ? { ...current, unitName } : current));
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      isInitializing,
      isAuthenticated: session !== null,
      session,
      uncoveredCount,
      login,
      logout,
      refreshUncoveredCount,
      setUnitName,
    }),
    [isInitializing, session, uncoveredCount, login, logout, refreshUncoveredCount, setUnitName],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
