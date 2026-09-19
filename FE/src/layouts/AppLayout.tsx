import { Spin } from 'antd';
import { Suspense } from 'react';
import { Outlet } from 'react-router-dom';

import { useAuth } from '../auth/useAuth';
import { AppHeader } from './AppHeader';
import './AppLayout.css';
import { AppSider } from './AppSider';

/** Khung chung của mọi màn từ M1 đến M5. */
export function AppLayout() {
  const { session, uncoveredCount, logout } = useAuth();

  return (
    <div className="hhd-shell">
      <AppHeader session={session} onLogout={logout} />
      <AppSider uncoveredCount={uncoveredCount} />
      <main className="hhd-main">
        <Suspense fallback={<Spin size="large" tip="Đang tải…" />}>
          <Outlet />
        </Suspense>
      </main>
    </div>
  );
}
