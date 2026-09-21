import { Descriptions, Skeleton } from 'antd';

import type { AwardPeriodResponse } from '../../../types/domain';

/**
 * Tab "Thông tin" của trang chi tiết đợt (UC-34). Đợt chỉ lưu ngày và tháng
 * (QT6) nên ở đây chỉ có đúng hai dòng: tên đợt và khoảng ngày hằng năm.
 *
 * Đợt vắt qua 31/12 đọc là "01/12 – 28/02 năm sau, hằng năm" để cán bộ không
 * nhầm hai đầu khoảng ngày nằm trong cùng một năm.
 */
export function PeriodInfoTab({ period }: { period: AwardPeriodResponse | null }) {
  return (
    <div className="hhd-period-info">
      {period ? (
        <Descriptions
          column={1}
          size="middle"
          styles={{ label: { width: 200 } }}
          items={[
            { key: 'name', label: 'Tên đợt', children: period.name },
            {
              key: 'range',
              label: 'Khoảng ngày',
              children: period.spansNextYear
                ? `${period.fromDisplay} – ${period.toDisplay} năm sau, hằng năm`
                : `${period.fromDisplay} – ${period.toDisplay} hằng năm`,
            },
          ]}
        />
      ) : (
        <Skeleton active title={false} paragraph={{ rows: 2, width: ['60%', '45%'] }} />
      )}
    </div>
  );
}
