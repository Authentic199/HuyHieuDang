/** Địa chỉ gốc của API, đặt qua biến môi trường VITE_API_BASE_URL. */
export const API_BASE_URL: string = import.meta.env.VITE_API_BASE_URL ?? '/api';

/** Khóa lưu thẻ đăng nhập trong localStorage. */
export const TOKEN_STORAGE_KEY = 'hhd.accessToken';
