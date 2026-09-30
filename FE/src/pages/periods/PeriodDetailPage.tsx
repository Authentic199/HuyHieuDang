import { App as AntApp, Alert, Button } from 'antd';
import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import { periodsApi } from '../../api';
import { FALLBACK_MESSAGE, messageText } from '../../api/messages';
import { paths } from '../../routes/paths';
import { ApiError } from '../../types/api';
import { EligibilityTab } from './detail/EligibilityTab';
import { PeriodDetailHeader } from './detail/PeriodDetailHeader';
import { usePeriodDetail } from './detail/usePeriodDetail';
import './detail/PeriodDetail.css';
// Modal Sửa đợt dùng lại nguyên của màn danh sách, kể cả dòng nhắc nền xám.
import { PeriodFormModal } from './PeriodFormModal';
import './PeriodsPage.css';

/**
 * Màn 5 — Chi tiết đợt (UC-34), theo artboard "Màn 5 — Chi tiết đợt".
 *
 * Từ quyết định 30/09, trang không còn tab. Tiêu đề đã nói đủ phần Thông tin —
 * tên đợt và khoảng ngày hằng năm (QT6) — nên ngay dưới nó là thẻ danh sách đủ
 * điều kiện: chọn năm, bảng người tròn mốc, xuất Excel. Đợt chỉ lưu ngày/tháng
 * nên khoảng ngày có năm chỉ xuất hiện trong thẻ, theo năm đang chọn.
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
        <EligibilityTab
          awardPeriodId={id}
          serverYear={serverYear}
          year={year}
          onYearChange={setSelectedYear}
          refreshToken={refreshToken}
        />
      )}

      {editing && !loading ? (
        <PeriodFormModal period={period} onCancel={() => setEditing(false)} onSaved={handleSaved} />
      ) : null}
    </div>
  );
}
