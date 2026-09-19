import { Spin } from 'antd';
import { Navigate, Outlet, useLocation } from 'react-router-dom';

import { useAuth } from '../auth/useAuth';
import { paths } from './paths';

/** Chặn mọi route trong khung khi chưa đăng nhập, đẩy về trang Đăng nhập. */
export function RequireAuth() {
  const { isInitializing, isAuthenticated } = useAuth();
  const location = useLocation();

  // Chưa kiểm tra xong thẻ đăng nhập — chưa vội đẩy đi đâu.
  if (isInitializing) {
    return (
      <div className="hhd-shell__loading">
        <Spin size="large" tip="Đang mở hệ thống…" />
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to={paths.login} replace state={{ from: location.pathname }} />;
  }

  return <Outlet />;
}
