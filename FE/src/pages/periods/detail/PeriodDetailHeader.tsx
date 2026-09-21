import { Breadcrumb, Button, Skeleton } from 'antd';
import { Link } from 'react-router-dom';

import { paths } from '../../../routes/paths';
import { font, typography } from '../../../theme/tokens';
import type { AwardPeriodResponse } from '../../../types/domain';

/**
 * Đầu trang chi tiết đợt theo artboard "Màn 5 — Chi tiết đợt": breadcrumb, tên
 * đợt cỡ lớn kèm khoảng ngày hằng năm, nút Sửa đợt và Xóa ở bên phải.
 *
 * Đợt vắt qua 31/12 phải đọc "năm sau" đúng như tab Thông tin ngay bên dưới
 * (QT6, UC-34) — một màn hình không được nói hai kiểu về cùng một khoảng ngày.
 */

interface PeriodDetailHeaderProps {
  period: AwardPeriodResponse | null;
  onEdit: () => void;
  onDelete: () => void;
}

export function PeriodDetailHeader({ period, onEdit, onDelete }: PeriodDetailHeaderProps) {
  return (
    <div>
      <Breadcrumb
        items={[
          { title: <Link to={paths.periods}>Đợt trao huy hiệu</Link> },
          { title: period ? period.name : '…' },
        ]}
      />

      <div
        style={{
          display: 'flex',
          alignItems: 'flex-end',
          justifyContent: 'space-between',
          gap: 16,
          marginTop: 2,
        }}
      >
        <h1
          style={{
            margin: 0,
            font: `${typography.headingXl.weight} ${typography.headingXl.size}px/${typography.headingXl.line}px ${font.serif}`,
            letterSpacing: '-0.005em',
          }}
        >
          {period ? (
            <>
              {period.name}{' '}
              <span className="hhd-period-detail__range">
                {period.fromDisplay} – {period.toDisplay}
                {period.spansNextYear ? ' năm sau, hằng năm' : ' hằng năm'}
              </span>
            </>
          ) : (
            <Skeleton.Input active size="large" style={{ width: 320 }} />
          )}
        </h1>

        <div style={{ display: 'flex', gap: 8, flex: 'none' }}>
          <Button disabled={!period} onClick={onEdit}>
            Sửa đợt
          </Button>
          <Button danger disabled={!period} onClick={onDelete}>
            Xóa
          </Button>
        </div>
      </div>
    </div>
  );
}
