import { ConfigProvider, Spin } from 'antd';
import { Suspense } from 'react';
import { Outlet } from 'react-router-dom';

import { useAuth } from '../auth/useAuth';
import { useMediaQuery } from '../hooks/useMediaQuery';
import { useSiderCollapsed } from '../hooks/useSiderCollapsed';
import { compactAntdTheme } from '../theme/antdTheme';
import { COMPACT_VIEWPORT_QUERY } from '../theme/breakpoints';
import { AppHeader } from './AppHeader';
import './AppLayout.css';
import { AppSider } from './AppSider';

/** Khung chung của mọi màn từ M1 đến M5. */
export function AppLayout() {
  const { session, uncoveredCount, logout } = useAuth();
  const { collapsed, toggle: toggleCollapsed } = useSiderCollapsed();

  /**
   * Cửa sổ thấp thì thay token Ant Design bằng thang gọn. Phải làm bằng
   * ConfigProvider chứ không bằng biến CSS: cỡ chữ, chiều cao nút và đệm ô bảng
   * được Ant Design sinh thành style lúc chạy từ giá trị JavaScript.
   * `undefined` nghĩa là dùng nguyên chủ đề của `App.tsx` — màn cao không đổi
   * một pixel nào.
   */
  const compact = useMediaQuery(COMPACT_VIEWPORT_QUERY);

  return (
    <ConfigProvider theme={compact ? compactAntdTheme : undefined}>
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
    </ConfigProvider>
  );
}
