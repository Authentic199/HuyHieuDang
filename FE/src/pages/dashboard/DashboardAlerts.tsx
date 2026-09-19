import { WarningFilled } from '@ant-design/icons';
import { Button } from 'antd';
import type { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';

import type { DashboardWarnings } from '../../api/dashboard';
import { paths } from '../../routes/paths';
import { formatNumber } from '../../utils/format';

/**
 * Cảnh báo nhanh (UC-12) — artboard 2 và 2 trống: lưới hai cột, mỗi cảnh báo là
 * một thẻ trắng viền xám, dấu tam giác vàng bên trái và một nút chữ dẫn thẳng
 * tới màn xử lý được việc đó.
 *
 * Bốn loại cảnh báo, xếp theo mức cấp bách: chưa có đảng viên, chưa có đợt,
 * người chưa thuộc đợt nào, rồi các đợt chồng lấn / chưa phủ kín. Không có
 * cảnh báo nào thì không dựng gì cả.
 */

interface DashboardAlertsProps {
  warnings: DashboardWarnings;
}

interface AlertItem {
  key: string;
  title: ReactNode;
  /** Câu giải thích dưới tiêu đề; có cảnh báo chỉ một dòng nên để trống */
  detail?: ReactNode;
  actionLabel: string;
  to: string;
}

/** Liệt kê khoảng theo lối viết "04/02–28/02, 20/05–30/06" của artboard. */
function joinRanges(ranges: { fromDisplay: string; toDisplay: string }[]): string {
  return ranges.map((range) => `${range.fromDisplay}–${range.toDisplay}`).join(', ');
}

export function DashboardAlerts({ warnings }: DashboardAlertsProps) {
  const navigate = useNavigate();
  const items: AlertItem[] = [];

  if (warnings.noMembers) {
    items.push({
      key: 'no-members',
      title: <b>Chưa có đảng viên nào</b>,
      detail: 'Nạp danh sách để bắt đầu tính.',
      actionLabel: 'Import Excel →',
      to: paths.membersImport,
    });
  }

  if (warnings.noPeriods) {
    items.push({
      key: 'no-periods',
      title: <b>Chưa cài đợt trao huy hiệu</b>,
      detail: 'Hệ thống chưa biết gom người theo khoảng ngày nào.',
      actionLabel: 'Tạo đợt →',
      to: paths.periods,
    });
  }

  if (warnings.unassignedCount > 0) {
    items.push({
      key: 'unassigned',
      title: (
        <>
          Có <b>{formatNumber(warnings.unassignedCount)} người chưa thuộc đợt nào</b> trong năm{' '}
          {warnings.unassignedYear}
        </>
      ),
      actionLabel: 'Xem →',
      to: paths.uncovered,
    });
  }

  if (warnings.overlaps.length > 0) {
    items.push({
      key: 'overlaps',
      title: (
        <>
          <b>Có đợt chồng lấn nhau</b> — {joinRanges(warnings.overlaps)}. Người tròn mốc trong các
          khoảng này nằm trong cả hai đợt.
        </>
      ),
      actionLabel: 'Điều chỉnh →',
      to: paths.periods,
    });
  }

  if (warnings.gaps.length > 0) {
    items.push({
      key: 'gaps',
      title: (
        <>
          <b>Các đợt chưa phủ kín cả năm</b> — trống {joinRanges(warnings.gaps)}.
        </>
      ),
      actionLabel: 'Điều chỉnh →',
      to: paths.periods,
    });
  }

  if (items.length === 0) return null;

  return (
    <div className="hhd-dashboard__alerts">
      {items.map((item) => (
        <div key={item.key} className="hhd-dashboard__alert">
          <WarningFilled className="hhd-dashboard__alert-icon" />
          <div className="hhd-dashboard__alert-text">
            <div>{item.title}</div>
            {item.detail ? <div className="hhd-dashboard__alert-detail">{item.detail}</div> : null}
          </div>
          <Button
            type="link"
            className="hhd-dashboard__alert-action"
            onClick={() => navigate(item.to)}
          >
            {item.actionLabel}
          </Button>
        </div>
      ))}
    </div>
  );
}
