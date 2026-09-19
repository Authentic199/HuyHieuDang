import type { IsoDate, IsoDateTime } from '../types/domain';
import { apiClient } from './httpClient';

export interface LoginRequest {
  username: string;
  password: string;
}

/** Phiên làm việc hiện tại. Dùng dựng header: tên đơn vị + ngày hôm nay. */
export interface SessionInfo {
  username: string;
  displayName: string;
  /** null khi chưa đặt tên đơn vị (UC-51) — header chỉ hiện tên hệ thống */
  unitName: string | null;
  /** Ngày hôm nay theo lịch MÁY CHỦ, không lấy từ đồng hồ trình duyệt (mục 1.6) */
  serverDate: IsoDate;
  expiresAt: IsoDateTime;
}

export interface LoginResponse extends SessionInfo {
  accessToken: string;
  tokenType: string;
}

/** UC-00 — Đăng nhập. Sai tài khoản hoặc mật khẩu đều trả 401 với một khóa chung. */
export function login(payload: LoginRequest): Promise<LoginResponse> {
  return apiClient.post<LoginResponse>('/Auth/Login', payload);
}

/**
 * UC-01 — Đăng xuất. Máy chủ không huỷ token (JWT không trạng thái);
 * phía client vẫn xóa token dù lời gọi này có lỗi.
 */
export function logout(): Promise<null> {
  return apiClient.post<null>('/Auth/Logout');
}

/** Tải lại trang: 200 thì token còn hiệu lực, 401 thì về màn đăng nhập. */
export function getSession(): Promise<SessionInfo> {
  return apiClient.get<SessionInfo>('/Auth/Me');
}
