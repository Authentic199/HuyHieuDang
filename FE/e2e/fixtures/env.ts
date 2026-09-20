import { fileURLToPath } from 'node:url';
import path from 'node:path';

/**
 * Hằng số môi trường của bộ kiểm thử end-to-end (T28).
 *
 * Mọi giá trị đều đọc được từ biến môi trường để chạy lại được trên máy khác;
 * giá trị mặc định khớp `FE/e2e/docker-compose.e2e.yml`.
 */

const here = path.dirname(fileURLToPath(import.meta.url));

/** Gốc kho mã — dùng để trỏ tới `tests/fixtures/`. */
export const REPO_ROOT = path.resolve(here, '..', '..', '..');

/** Thư mục bộ dữ liệu biên dùng chung (hợp đồng dữ liệu kiểm thử). */
export const FIXTURES_DIR = path.join(REPO_ROOT, 'tests', 'fixtures');

/** Các tệp Excel của bộ dữ liệu. */
export const EXCEL_DIR = path.join(FIXTURES_DIR, 'excel');

/** Nơi chứa tệp tạm do chính bộ kiểm thử sinh ra (không commit). */
export const ARTIFACT_DIR = path.join(here, '..', '.artifacts');

/** Địa chỉ giao diện — nginx của stack e2e, đã chuyển tiếp `/api` sang Backend. */
export const BASE_URL = process.env.E2E_BASE_URL ?? 'http://localhost:4174';

/** Địa chỉ API gọi thẳng, dùng cho việc dựng dữ liệu trước mỗi luồng. */
export const API_URL = process.env.E2E_API_URL ?? `${BASE_URL}/api`;

/** Tài khoản admin seed sẵn của hệ thống. */
export const ADMIN = {
  username: process.env.E2E_ADMIN_USERNAME ?? 'admin',
  password: process.env.E2E_ADMIN_PASSWORD ?? 'Admin@123',
} as const;

/** Khóa localStorage giữ thẻ đăng nhập — phải khớp `FE/src/api/config.ts`. */
export const TOKEN_STORAGE_KEY = 'hhd.accessToken';

/** Tên đơn vị dùng xuyên suốt sáu luồng (UC-51). */
export const UNIT_NAME = 'Đảng ủy Phường Kiểm Thử';
