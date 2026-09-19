import { Alert, Button, Form, Input } from 'antd';
import { useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';

import { BrandMark } from '../../components/BrandMark';
import { Logo } from '../../components/Logo';
import { useAuth } from '../../auth/useAuth';
import { paths } from '../../routes/paths';
import { ApiError } from '../../types/api';
import './LoginPage.css';

interface LoginFormValues {
  username: string;
  password: string;
}

/** Màn 1 — Đăng nhập (UC-00). */
export default function LoginPage() {
  const { isAuthenticated, login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [submitting, setSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Đã đăng nhập rồi thì không cho quay lại trang này.
  if (isAuthenticated) {
    return <Navigate to={paths.dashboard} replace />;
  }

  // Quay lại đúng trang người dùng định vào trước khi bị chặn.
  const redirectTo = (location.state as { from?: string } | null)?.from ?? paths.dashboard;

  async function handleSubmit(values: LoginFormValues) {
    setSubmitting(true);
    setErrorMessage(null);
    try {
      await login(values.username.trim(), values.password);
      navigate(redirectTo, { replace: true });
    } catch (error) {
      // Sai tài khoản hay sai mật khẩu đều báo chung một câu, không chỉ rõ ô nào sai.
      // Lớp HTTP đã dịch khóa Mes.User.Login.Failed thành "Sai tài khoản hoặc mật khẩu".
      setErrorMessage(
        error instanceof ApiError ? error.message : 'Không đăng nhập được. Vui lòng thử lại.',
      );
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="hhd-login">
      <div className="hhd-login__brand">
        <div className="hhd-login__watermark">
          <Logo size={560} decorative />
        </div>
        <div style={{ position: 'relative' }}>
          <BrandMark logoSize={36} fontSize={20} />
        </div>
        <div style={{ position: 'relative' }}>
          <div className="hhd-login__headline">Hệ thống hỗ trợ xét trao Huy hiệu Đảng</div>
          <div className="hhd-login__subline">
            Tự động tính đảng viên tròn mốc tuổi Đảng theo từng đợt, xuất Excel làm tờ trình.
          </div>
        </div>
        <div className="hhd-login__version">v1.0 · chạy cục bộ</div>
      </div>

      <div className="hhd-login__form-side">
        <div className="hhd-login__card">
          <div className="hhd-login__title">Đăng nhập</div>
          <div className="hhd-login__hint">Tài khoản quản trị của đơn vị.</div>

          {errorMessage ? (
            <Alert
              type="error"
              showIcon
              message={errorMessage}
              style={{ marginTop: 24 }}
              data-testid="login-error"
            />
          ) : null}

          <Form<LoginFormValues>
            layout="vertical"
            requiredMark={false}
            onFinish={handleSubmit}
            style={{ marginTop: 20 }}
            autoComplete="on"
          >
            <Form.Item
              name="username"
              label="Tài khoản"
              rules={[{ required: true, message: 'Vui lòng nhập tài khoản.' }]}
            >
              <Input size="large" autoFocus autoComplete="username" />
            </Form.Item>
            <Form.Item
              name="password"
              label="Mật khẩu"
              rules={[{ required: true, message: 'Vui lòng nhập mật khẩu.' }]}
            >
              <Input.Password size="large" autoComplete="current-password" />
            </Form.Item>
            <Button
              className="hhd-login__submit"
              type="primary"
              htmlType="submit"
              block
              loading={submitting}
            >
              Đăng nhập
            </Button>
          </Form>
        </div>
      </div>
    </div>
  );
}
