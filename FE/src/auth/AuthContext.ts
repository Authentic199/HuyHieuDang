import { createContext } from 'react';

import type { SessionInfo } from '../api/auth';

export interface AuthContextValue {
  /** Đang kiểm tra thẻ đăng nhập lúc mở ứng dụng */
  isInitializing: boolean;
  isAuthenticated: boolean;
  session: SessionInfo | null;
  /** Số người chưa thuộc đợt nào trong năm hiện tại — badge trên sider */
  uncoveredCount: number;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
  /** Gọi lại sau mỗi thao tác đổi dữ liệu để badge luôn đúng */
  refreshUncoveredCount: () => Promise<void>;
  /** Cập nhật tên đơn vị sau khi lưu ở màn Cài đặt (UC-51) */
  setUnitName: (unitName: string | null) => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);
