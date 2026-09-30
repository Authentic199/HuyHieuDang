import { Breadcrumb, Button, Skeleton } from 'antd';
import { Link } from 'react-router-dom';

import '../../../components/PageHeading.css';
import { paths } from '../../../routes/paths';
import type { AwardPeriodResponse } from '../../../types/domain';

/**
 * Đầu trang chi tiết đợt theo artboard "Màn 5 — Chi tiết đợt": breadcrumb, tên
 * đợt cỡ lớn, nút Sửa đợt và Xóa ở bên phải.
 *
 * Tiêu đề chỉ có tên đợt. Khoảng ngày có năm nằm ở thanh công cụ của thẻ danh
 * sách (`.hhd-eligibility__dates`) — đó là chỗ duy nhất trên trang cho biết
 * khoảng ngày của đợt, kể cả đợt vắt qua 31/12 (QT6, UC-34).
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

      {/* Dùng lớp của PageHeading để tên đợt và tiêu đề các màn khác cùng một
          thang chữ, kể cả khi thang gọn bật lên ở khung nhìn thấp. */}
      <div className="hhd-page-heading" style={{ marginTop: 2 }}>
        <h1 className="hhd-page-heading__title">
          {period ? period.name : <Skeleton.Input active size="large" style={{ width: 320 }} />}
        </h1>

        <div className="hhd-page-heading__extra">
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
