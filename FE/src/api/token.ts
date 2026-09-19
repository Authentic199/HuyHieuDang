import { TOKEN_STORAGE_KEY } from './config';

/** Đọc/ghi thẻ đăng nhập. Tách riêng để tầng HTTP và tầng auth dùng chung. */

export function readToken(): string | null {
  try {
    return window.localStorage.getItem(TOKEN_STORAGE_KEY);
  } catch {
    return null;
  }
}

export function writeToken(token: string): void {
  try {
    window.localStorage.setItem(TOKEN_STORAGE_KEY, token);
  } catch {
    /* Trình duyệt chặn localStorage — bỏ qua, phiên chỉ sống trong tab hiện tại. */
  }
}

export function clearToken(): void {
  try {
    window.localStorage.removeItem(TOKEN_STORAGE_KEY);
  } catch {
    /* như trên */
  }
}
