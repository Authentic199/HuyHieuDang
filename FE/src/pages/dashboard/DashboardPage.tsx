import { Alert, Button, Skeleton } from 'antd';

import { PageHeading } from '../../components/PageHeading';
import { DashboardAlerts } from './DashboardAlerts';
import './DashboardPage.css';
import { EligibleTableCard } from './EligibleTableCard';
import { StartGuide } from './StartGuide';
import { UpcomingPeriodCard } from './UpcomingPeriodCard';
import { useDashboard } from './useDashboard';

/**
 * Màn 2 — Dashboard (UC-10, UC-11, UC-12, UC-13), theo artboard "Màn 2 —
 * Dashboard (đợt sắp tới, có cảnh báo)" và "Màn 2 — Dashboard trạng thái trống".
 *
 * Đây là màn đầu tiên cán bộ nhìn thấy sau khi đăng nhập, nên phải trả lời ngay
 * ba câu: có gì cần sửa không (cảnh báo nhanh), đợt nào sắp tới và bao nhiêu
 * người đủ điều kiện, danh sách cụ thể là ai.
 *
 * Lần dùng đầu — chưa có đảng viên hoặc chưa có đợt — thì thay thẻ đợt và bảng
 * bằng khối hướng dẫn 3 bước, vì lúc đó không có số liệu nào để xem.
 */
export default function DashboardPage() {
  const { data, loading, error, reload } = useDashboard();

  const warnings = data?.warnings ?? null;
  /** Chưa đủ dữ liệu nền để tính gì cả → dẫn cán bộ đi ba bước cài đặt ban đầu. */
  const firstUse = warnings !== null && (warnings.noMembers || warnings.noPeriods);

  return (
    <div className="hhd-dashboard">
      <PageHeading title="Dashboard" />

      {error ? (
        <Alert
          type="error"
          showIcon
          message="Chưa tải được Dashboard"
          description={error}
          action={
            <Button size="small" danger onClick={reload}>
              Thử lại
            </Button>
          }
        />
      ) : null}

      {warnings ? <DashboardAlerts warnings={warnings} /> : null}

      {loading ? (
        <div className="hhd-dashboard__card">
          <div className="hhd-dashboard__card-main">
            <Skeleton active title={false} paragraph={{ rows: 3, width: ['30%', '60%', '45%'] }} />
          </div>
        </div>
      ) : null}

      {firstUse && warnings ? (
        // Thiếu đợt thì việc gấp là tạo đợt; thiếu người thì là nạp danh sách.
        <StartGuide currentStep={warnings.noMembers ? 3 : 2} />
      ) : null}

      {!firstUse && !error ? (
        <>
          {data ? <UpcomingPeriodCard period={data.upcomingPeriod} /> : null}
          <EligibleTableCard
            period={data?.upcomingPeriod ?? null}
            members={data?.eligibleMembers ?? []}
            loading={loading}
            error={error}
            onRetry={reload}
          />
        </>
      ) : null}
    </div>
  );
}
