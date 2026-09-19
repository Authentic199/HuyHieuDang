import { Alert, Button, Form, Input } from 'antd';
import { useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';

import { BrandMark } from '../../components/BrandMark';
import { Logo } from '../../components/Logo';
import { useAuth } from '../../auth/useAuth';
import { FALLBACK_MESSAGE, messageText } from '../../api/messages';
import { paths } from '../../routes/paths';
import { ApiError } from '../../types/api';
import './LoginPage.css';

interface LoginFormValues {
  username: string;
  password: string;
}

/** Dấu chấm than trong vòng tròn — đúng hình trong artboard 1. */
function ErrorIcon() {
  return (
    <svg
      width="20"
      height="20"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.75"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <circle cx="12" cy="12" r="8.5" />
      <path d="M12 8v5M12 16v.5" />
    </svg>
  );
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
    // Bấm Đăng nhập hai lần, hoặc gõ Enter khi đang gửi, không gửi thêm lần nữa.
    if (submitting) return;
    setSubmitting(true);
    setErrorMessage(null);
    try {
      await login(values.username.trim(), values.password);
      navigate(redirectTo, { replace: true });
    } catch (error) {
      // Sai tài khoản hay sai mật khẩu đều báo chung một câu, không chỉ rõ ô nào sai.
      // Câu chữ do src/api/messages.ts giữ (khóa Mes.User.Login.Failed), không viết lại ở đây.
      setErrorMessage(error instanceof ApiError ? error.message : FALLBACK_MESSAGE);
      setSubmitting(false);
    }
  }

  return (
    <div className="hhd-login">
      <div className="hhd-login__brand">
        <div className="hhd-login__watermark">
          <Logo size={560} decorative />
        </div>
        <div className="hhd-login__brand-row">
          <BrandMark logoSize={36} fontSize={20} />
        </div>
        <div className="hhd-login__brand-row">
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
              className="hhd-login__error"
              type="error"
              icon={<ErrorIcon />}
              showIcon
              message={errorMessage}
              data-testid="login-error"
            />
          ) : null}

          <Form<LoginFormValues>
            className="hhd-login__form"
            layout="vertical"
            requiredMark={false}
            onFinish={handleSubmit}
            autoComplete="on"
          >
            <Form.Item
              name="username"
              label="Tài khoản"
              rules={[{ required: true, message: messageText('Mes.User.Required.Username') }]}
            >
              <Input size="large" autoFocus autoComplete="username" disabled={submitting} />
            </Form.Item>
            <Form.Item
              name="password"
              label="Mật khẩu"
              rules={[{ required: true, message: messageText('Mes.User.Required.Password') }]}
            >
              <Input.Password
                size="large"
                autoComplete="current-password"
                disabled={submitting}
                // Artboard 1 hiện chữ "Hiện" chứ không phải hình con mắt.
                iconRender={(visible) => (
                  <span className="hhd-login__reveal">{visible ? 'Ẩn' : 'Hiện'}</span>
                )}
              />
            </Form.Item>
            <Button
              className="hhd-login__submit"
              type="primary"
              htmlType="submit"
              block
              loading={submitting}
              disabled={submitting}
            >
              {submitting ? 'Đang đăng nhập…' : 'Đăng nhập'}
            </Button>
          </Form>
        </div>
      </div>
    </div>
  );
}
