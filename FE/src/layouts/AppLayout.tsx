import { Spin } from 'antd';
import { Suspense, useCallback, useState } from 'react';
import { Outlet } from 'react-router-dom';

import { useAuth } from '../auth/useAuth';
import { AppHeader } from './AppHeader';
import './AppLayout.css';
import { AppSider } from './AppSider';

/** Nhớ lựa chọn thu gọn menu để lần mở sau vẫn như cũ. */
const COLLAPSED_STORAGE_KEY = 'hhd.siderCollapsed';

function readCollapsed(): boolean {
  try {
    return window.localStorage.getItem(COLLAPSED_STORAGE_KEY) === '1';
  } catch {
    return false;
  }
}

/** Khung chung của mọi màn từ M1 đến M5. */
export function AppLayout() {
  const { session, uncoveredCount, logout } = useAuth();
  const [collapsed, setCollapsed] = useState(readCollapsed);

  const toggleCollapsed = useCallback(() => {
    setCollapsed((current) => {
      const next = !current;
      try {
        window.localStorage.setItem(COLLAPSED_STORAGE_KEY, next ? '1' : '0');
      } catch {
        /* Trình duyệt chặn localStorage — chỉ mất phần ghi nhớ, không sao. */
      }
      return next;
    });
  }, []);

  return (
    <div className={`hhd-shell${collapsed ? ' hhd-shell--collapsed' : ''}`}>
      <AppHeader session={session} onLogout={logout} />
      <AppSider
        uncoveredCount={uncoveredCount}
        collapsed={collapsed}
        onToggleCollapsed={toggleCollapsed}
      />
      <main className="hhd-main">
        <Suspense fallback={<Spin size="large" tip="Đang tải…" />}>
          <Outlet />
        </Suspense>
      </main>
    </div>
  );
}
