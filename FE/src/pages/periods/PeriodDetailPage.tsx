import { App as AntApp, Alert, Button, Tabs } from 'antd';
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import { periodsApi } from '../../api';
import { FALLBACK_MESSAGE, messageText } from '../../api/messages';
import { paths } from '../../routes/paths';
import { ApiError } from '../../types/api';
import { EligibilityTab } from './detail/EligibilityTab';
import { PeriodDetailHeader } from './detail/PeriodDetailHeader';
import { PeriodInfoTab } from './detail/PeriodInfoTab';
import { usePeriodDetail } from './detail/usePeriodDetail';
import './detail/PeriodDetail.css';
// Modal Sửa đợt dùng lại nguyên của màn danh sách, kể cả dòng nhắc nền xám.
import { PeriodFormModal } from './PeriodFormModal';
import './PeriodsPage.css';

/**
 * Màn 5 — Chi tiết đợt (UC-34), theo artboard "Màn 5 — Chi tiết đợt".
 *
 * Hai tab: "Thông tin" (tên đợt, khoảng ngày hằng năm) và "Danh sách đủ điều
 * kiện" (chọn năm, bảng người tròn mốc, xuất Excel). Đợt chỉ lưu ngày/tháng
 * (QT6) nên khoảng ngày có năm chỉ xuất hiện ở tab thứ hai, theo năm đang chọn.
 */
export default function PeriodDetailPage() {
  const { id = '' } = useParams<{ id: string }>();
  const { message, modal } = AntApp.useApp();
  const navigate = useNavigate();
  const { period, serverYear, loading, error, reload } = usePeriodDetail(id);

  const [editing, setEditing] = useState(false);
  const [selectedYear, setSelectedYear] = useState<number | null>(null);
  // Tăng sau mỗi lần sửa đợt để danh sách đủ điều kiện tải lại theo ngày mới.
  const [refreshToken, setRefreshToken] = useState(0);

  // Chưa chọn năm thì lấy năm hiện tại của máy chủ, không lấy đồng hồ trình duyệt.
  const year = selectedYear ?? serverYear;

  function handleSaved(savedMessage: string) {
    setEditing(false);
    message.success(savedMessage);
    reload();
    setRefreshToken((count) => count + 1);
  }

  /** UC-33 — câu chữ giống hệt hộp xác nhận xóa bên màn danh sách đợt. */
  function confirmDelete() {
    if (!period) return;
    modal.confirm({
      width: 520,
      title: `Xóa đợt “${period.name}”?`,
      content: (
        <>
          “{period.name}” ({period.fromDisplay} – {period.toDisplay}) sẽ bị xóa khỏi mọi năm, không
          lấy lại được. Danh sách đảng viên vẫn giữ nguyên, nhưng người tròn mốc trong khoảng này sẽ
          chuyển sang mục “Chưa thuộc đợt nào”.
        </>
      ),
      okText: 'Xóa đợt này',
      cancelText: 'Để lại',
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          await periodsApi.deletePeriod(period.id);
          message.success(messageText('Mes.AwardPeriod.Delete.Successfully'));
          // Đợt không còn nữa, quay về danh sách thay vì để lại trang trống.
          navigate(paths.periods);
        } catch (reason) {
          message.error(reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE);
        }
      },
    });
  }

  return (
    <div className="hhd-period-detail">
      <PeriodDetailHeader
        period={period}
        onEdit={() => setEditing(true)}
        onDelete={confirmDelete}
      />

      {error ? (
        <Alert
          type="error"
          showIcon
          message="Chưa mở được đợt này"
          description={error}
          action={
            <Button size="small" danger onClick={reload}>
              Thử lại
            </Button>
          }
        />
      ) : (
        <Tabs
          className="hhd-period-detail__tabs"
          defaultActiveKey="info"
          items={[
            {
              key: 'info',
              label: 'Thông tin',
              children: <PeriodInfoTab period={period} />,
            },
            {
              key: 'eligibility',
              label: 'Danh sách đủ điều kiện',
              children: (
                <EligibilityTab
                  awardPeriodId={id}
                  serverYear={serverYear}
                  year={year}
                  onYearChange={setSelectedYear}
                  refreshToken={refreshToken}
                />
              ),
            },
          ]}
        />
      )}

      {editing && !loading ? (
        <PeriodFormModal period={period} onCancel={() => setEditing(false)} onSaved={handleSaved} />
      ) : null}
    </div>
  );
}
