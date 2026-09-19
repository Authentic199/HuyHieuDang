import { Alert, Button, Spin } from 'antd';
import { Navigate, Outlet, useLocation } from 'react-router-dom';

import { useAuth } from '../auth/useAuth';
import { paths } from './paths';

/** Chặn mọi route trong khung khi chưa đăng nhập, đẩy về trang Đăng nhập. */
export function RequireAuth() {
  const { isInitializing, isAuthenticated, startupError, retryStartup } = useAuth();
  const location = useLocation();

  // Chưa kiểm tra xong thẻ đăng nhập — chưa vội đẩy đi đâu.
  if (isInitializing) {
    return (
      <div className="hhd-shell__loading">
        <Spin size="large" tip="Đang mở hệ thống…" />
      </div>
    );
  }

  // Không hỏi được máy chủ (máy chủ chưa lên, mất mạng). Phiên vẫn còn nguyên
  // nên không bắt đăng nhập lại — chỉ chờ bấm "Thử lại".
  if (startupError) {
    return (
      <div className="hhd-shell__loading">
        <Alert
          type="error"
          showIcon
          style={{ maxWidth: 560 }}
          message="Chưa mở được hệ thống"
          description={startupError}
          action={
            <Button size="small" danger onClick={retryStartup}>
              Thử lại
            </Button>
          }
        />
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to={paths.login} replace state={{ from: location.pathname }} />;
  }

  return <Outlet />;
}
