import axios, { AxiosError, type AxiosRequestConfig, type AxiosResponse } from 'axios';

import { ApiError, type SuccessEnvelope } from '../types/api';
import { API_BASE_URL } from './config';
import { FALLBACK_MESSAGE, NETWORK_MESSAGE, SERVER_MESSAGE, messageText } from './messages';
import { clearToken, readToken } from './token';

/**
 * Một instance axios duy nhất cho toàn ứng dụng.
 * Component không bao giờ gọi trực tiếp instance này — chỉ gọi qua các hàm
 * trong src/api/<module>.ts.
 */
const http = axios.create({
  baseURL: API_BASE_URL,
  timeout: 60_000,
  headers: { 'Content-Type': 'application/json' },
});

/** Gắn thẻ đăng nhập vào mọi lời gọi (mục 1.2). */
http.interceptors.request.use((config) => {
  const token = readToken();
  if (token) {
    config.headers.set('Authorization', `Bearer ${token}`);
  }
  return config;
});

/** Sự kiện phát ra khi thẻ hết hạn, để lớp auth đẩy người dùng về trang Đăng nhập. */
export const UNAUTHORIZED_EVENT = 'hhd:unauthorized';

/**
 * Sự kiện phát ra sau MỌI lời gọi làm đổi dữ liệu (thêm/sửa/xóa đảng viên, đổi
 * đợt, nạp Excel, đổi cài đặt). Badge "Chưa thuộc đợt nào" trên menu trái nghe
 * sự kiện này để lấy lại số thật, theo mục 6.4 của hợp đồng API — nhờ vậy màn
 * hình không phải nhớ gọi lại sau từng thao tác.
 */
export const DATA_CHANGED_EVENT = 'hhd:data-changed';

/**
 * Lời gọi không đổi dữ liệu nghiệp vụ nên không cần đánh thức badge:
 * đăng nhập / đăng xuất và chính endpoint đếm badge.
 */
const NON_MUTATING_PATHS = ['/Auth/Login', '/Auth/Logout', '/Eligibility/UnassignedCount'];

function isMutation(method: string | undefined, url: string | undefined): boolean {
  const verb = (method ?? 'get').toLowerCase();
  if (verb === 'get') return false;
  const path = url ?? '';
  return !NON_MUTATING_PATHS.some((skipped) => path.endsWith(skipped));
}

/**
 * Màn Đăng nhập cũng trả 401 khi sai tài khoản — đó không phải hết phiên.
 * Chỉ 401 của các lời gọi khác mới đẩy người dùng ra ngoài.
 */
const LOGIN_PATH = '/Auth/Login';

/** Hai hình dạng lỗi của hợp đồng (mục 1.4) gộp lại. */
interface ServerErrorBody {
  statusCode?: number;
  message?: string;
  traceId?: string;
  supportMessage?: string;
}

function toApiError(status: number, body: ServerErrorBody | undefined): ApiError {
  const key = body?.message ?? null;
  // 401 luôn là "sai tài khoản hoặc mật khẩu" hoặc hết phiên; 500 dùng câu chung.
  const fallback =
    status >= 500 ? SERVER_MESSAGE : status === 0 ? NETWORK_MESSAGE : FALLBACK_MESSAGE;
  return new ApiError(messageText(key, fallback), status, {
    messageKey: key,
    traceId: body?.traceId ?? null,
  });
}

/** Endpoint trả file trả JSON khi lỗi — phải đọc blob thành chữ rồi mới hiểu (mục 1.9). */
async function readBlobError(blob: Blob): Promise<ServerErrorBody | undefined> {
  try {
    return JSON.parse(await blob.text()) as ServerErrorBody;
  } catch {
    return undefined;
  }
}

http.interceptors.response.use(
  (response) => {
    if (isMutation(response.config.method, response.config.url)) {
      window.dispatchEvent(new CustomEvent(DATA_CHANGED_EVENT));
    }
    return response;
  },
  async (error: unknown) => {
    if (!axios.isAxiosError(error)) {
      return Promise.reject(new ApiError(FALLBACK_MESSAGE, 0));
    }

    const axiosError = error as AxiosError<ServerErrorBody | Blob>;
    const status = axiosError.response?.status ?? 0;
    const raw = axiosError.response?.data;
    const body =
      raw instanceof Blob ? await readBlobError(raw) : (raw as ServerErrorBody | undefined);

    const apiError = toApiError(status, body);
    const isLoginCall = (axiosError.config?.url ?? '').endsWith(LOGIN_PATH);
    if (apiError.isUnauthorized && !isLoginCall) {
      clearToken();
      window.dispatchEvent(new CustomEvent(UNAUTHORIZED_EVENT));
    }
    return Promise.reject(apiError);
  },
);

/** Bóc lớp vỏ `{ message, data }`, chỉ trả phần `data` cho phía gọi. */
function unwrap<T>(response: AxiosResponse<SuccessEnvelope<T>>): T {
  return response.data.data;
}

/** File tải về kèm tên do Backend đặt (mục 1.9). */
export interface DownloadedFile {
  blob: Blob;
  fileName: string;
}

/** Đọc tên file từ Content-Disposition, ưu tiên dạng `filename*=UTF-8''…`. */
function parseFileName(disposition: string | undefined, fallback: string): string {
  if (!disposition) return fallback;
  const utf8 = /filename\*=UTF-8''([^;]+)/i.exec(disposition);
  if (utf8) return decodeURIComponent(utf8[1].trim());
  const plain = /filename="?([^";]+)"?/i.exec(disposition);
  return plain ? plain[1].trim() : fallback;
}

export const apiClient = {
  get: <T>(url: string, config?: AxiosRequestConfig) =>
    http.get<SuccessEnvelope<T>>(url, config).then(unwrap),
  post: <T>(url: string, body?: unknown, config?: AxiosRequestConfig) =>
    http.post<SuccessEnvelope<T>>(url, body, config).then(unwrap),
  put: <T>(url: string, body?: unknown, config?: AxiosRequestConfig) =>
    http.put<SuccessEnvelope<T>>(url, body, config).then(unwrap),
  delete: <T>(url: string, config?: AxiosRequestConfig) =>
    http.delete<SuccessEnvelope<T>>(url, config).then(unwrap),

  /**
   * Tải file Excel. Phải đi qua XHR chứ không dùng thẻ <a href> vì cần gửi
   * header Authorization (mục 1.9).
   */
  download: (url: string, fallbackFileName: string, config?: AxiosRequestConfig) =>
    http.get<Blob>(url, { ...config, responseType: 'blob' }).then((response): DownloadedFile => ({
      blob: response.data,
      fileName: parseFileName(response.headers['content-disposition'] as string, fallbackFileName),
    })),

  /** Gửi file lên (import Excel). */
  upload: <T>(url: string, file: File, config?: AxiosRequestConfig) => {
    const form = new FormData();
    form.append('file', file);
    return http
      .post<SuccessEnvelope<T>>(url, form, {
        ...config,
        headers: { ...config?.headers, 'Content-Type': 'multipart/form-data' },
      })
      .then(unwrap);
  },
};

export default http;
