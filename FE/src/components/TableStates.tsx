import { Alert, Button, Empty, Skeleton } from 'antd';
import type { ReactNode } from 'react';

import { neutral, radius } from '../theme/tokens';

/** Khung trắng bao quanh mọi trạng thái, cùng hình dáng với thẻ bảng. */
function StateCard({ children }: { children: ReactNode }) {
  return (
    <div
      style={{
        background: neutral.white,
        border: `1px solid ${neutral.border}`,
        borderRadius: radius.card,
        padding: 32,
      }}
    >
      {children}
    </div>
  );
}

/** Đang tải: khung xám thay cho các dòng, không nhảy giật khi có dữ liệu. */
export function TableLoading({ rows = 5 }: { rows?: number }) {
  return (
    <StateCard>
      <Skeleton active title={false} paragraph={{ rows, width: '100%' }} />
    </StateCard>
  );
}

interface TableEmptyProps {
  /** Câu chính, lấy đúng chữ trong bản thiết kế của từng màn */
  description: ReactNode;
  /** Câu phụ giải thích cách làm tiếp */
  hint?: ReactNode;
  /** Nút gợi ý việc nên làm, ví dụ "Thêm đảng viên" */
  action?: ReactNode;
}

/** Bảng trống — nói rõ vì sao trống và làm gì tiếp, không để màn hình trắng. */
export function TableEmpty({ description, hint, action }: TableEmptyProps) {
  return (
    <StateCard>
      <Empty
        image={Empty.PRESENTED_IMAGE_SIMPLE}
        description={
          <div style={{ color: neutral.textSecondary }}>
            <div style={{ fontSize: 16, lineHeight: '24px', color: '#1c1c1e' }}>{description}</div>
            {hint ? <div style={{ fontSize: 14, marginTop: 4 }}>{hint}</div> : null}
          </div>
        }
      >
        {action}
      </Empty>
    </StateCard>
  );
}

interface TableErrorProps {
  /** Câu tiếng Việt đã dịch sẵn từ tầng API */
  message: string;
  onRetry?: () => void;
}

/** Lỗi khi tải bảng — câu tiếng Việt đời thường kèm nút thử lại. */
export function TableError({ message, onRetry }: TableErrorProps) {
  return (
    <Alert
      type="error"
      showIcon
      message="Chưa tải được danh sách"
      description={message}
      action={
        onRetry ? (
          <Button size="small" danger onClick={onRetry}>
            Thử lại
          </Button>
        ) : null
      }
    />
  );
}

interface TableStatesProps extends TableEmptyProps {
  loading: boolean;
  /** Câu lỗi đã dịch, null khi không lỗi */
  error: string | null;
  onRetry?: () => void;
  isEmpty: boolean;
  skeletonRows?: number;
  /** Bảng thật, chỉ dựng khi có dữ liệu */
  children: ReactNode;
}

/**
 * Ba trạng thái bắt buộc của mọi bảng (quy tắc 3 trong CLAUDE.md): đang tải,
 * lỗi, trống. Màn hình chỉ cần truyền cờ và câu chữ của riêng nó.
 */
export function TableStates({
  loading,
  error,
  onRetry,
  isEmpty,
  skeletonRows,
  description,
  hint,
  action,
  children,
}: TableStatesProps) {
  if (loading) return <TableLoading rows={skeletonRows} />;
  if (error) return <TableError message={error} onRetry={onRetry} />;
  if (isEmpty) return <TableEmpty description={description} hint={hint} action={action} />;
  return <>{children}</>;
}
