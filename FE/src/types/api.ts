/** Kiểu dùng chung cho tầng gọi API, theo mục 1.3–1.7 của docs/api-contract.md. */

/** Lớp vỏ chung của mọi phản hồi thành công. */
export interface SuccessEnvelope<T> {
  /** Khóa thông điệp dạng Mes.<ThựcThể>.<HànhĐộng>.<KếtQuả>, không phải chữ hiển thị */
  message: string;
  data: T;
}

/** Thông tin phân trang do Backend trả về (mục 1.7). */
export interface PageInfo {
  totalCount: number;
  pageSize: number;
  current: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

/** Hình dạng phản hồi của endpoint có phân trang. */
export interface PagedResult<T> {
  pagedData: T[];
  pageInfo: PageInfo;
}

/** Tham số truy vấn chuẩn của endpoint có phân trang. */
export interface PagedQuery {
  current: number;
  pageSize: number;
  searchKeyword?: string;
  searchFields?: string[];
  /** Ví dụ 'FullName asc', 'OfficialAdmissionDate desc' */
  sortQuery?: string;
}

/**
 * Lỗi đã được dịch sang tiếng Việt để hiển thị thẳng cho người dùng.
 * Component đọc `message`; `messageKey` dành cho chỗ cần phân biệt loại lỗi.
 *
 * Lưu ý: theo mục 1.4 của hợp đồng, "không tìm thấy" trả 400 chứ không phải 404.
 */
export class ApiError extends Error {
  readonly status: number;
  /** Khóa gốc Backend trả về, ví dụ 'Mes.AwardPeriod.Repeated.Name' */
  readonly messageKey: string | null;
  readonly traceId: string | null;

  constructor(
    message: string,
    status: number,
    options?: { messageKey?: string | null; traceId?: string | null },
  ) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.messageKey = options?.messageKey ?? null;
    this.traceId = options?.traceId ?? null;
  }

  /** Token thiếu hoặc hết hạn — lớp auth dùng để đẩy về trang Đăng nhập. */
  get isUnauthorized(): boolean {
    return this.status === 401;
  }

  /** Bản ghi không còn tồn tại. */
  get isNotFound(): boolean {
    return this.messageKey?.endsWith('.NotFound') ?? false;
  }
}
