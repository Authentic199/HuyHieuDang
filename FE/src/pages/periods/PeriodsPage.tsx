import { DeleteOutlined, EditOutlined, UnorderedListOutlined } from '@ant-design/icons';
import { App as AntApp, Button, Table, Tooltip } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { periodsApi } from '../../api';
import { FALLBACK_MESSAGE, messageText } from '../../api/messages';
import { PageHeading } from '../../components/PageHeading';
import { TableStates } from '../../components/TableStates';
import { periodDetailPath } from '../../routes/paths';
import { ApiError } from '../../types/api';
import type { AwardPeriodResponse, CoverageWarnings } from '../../types/domain';
import { formatNumber } from '../../utils/format';
import { CoverageStrip } from './CoverageStrip';
import { CoverageWarningBanner } from './CoverageWarningBanner';
import { PeriodFormModal } from './PeriodFormModal';
import { PeriodStatusTag } from './PeriodStatusTag';
import './PeriodsPage.css';
import { usePeriods } from './usePeriods';

/**
 * Màn 5 — Đợt trao huy hiệu (UC-30 đến UC-33, UC-36), theo artboard 5.
 *
 * Đợt dùng chung cho mọi năm nên màn này KHÔNG có bộ chọn năm: mọi con số đều
 * của năm hiện tại do máy chủ ấn định. Chọn năm khác là việc của trang chi tiết.
 */
export default function PeriodsPage() {
  const { message, modal } = AntApp.useApp();
  const navigate = useNavigate();
  const {
    periods,
    year,
    today,
    totalCount,
    segments,
    warnings,
    loading,
    error,
    reload,
    applyWarnings,
  } = usePeriods();

  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<AwardPeriodResponse | null>(null);

  const isEmpty = periods.length === 0;
  const showCoverage = !loading && !error && !isEmpty;

  function openCreate() {
    setEditing(null);
    setFormOpen(true);
  }

  function openEdit(period: AwardPeriodResponse) {
    setEditing(period);
    setFormOpen(true);
  }

  function handleSaved(savedMessage: string, nextWarnings: CoverageWarnings) {
    setFormOpen(false);
    setEditing(null);
    message.success(savedMessage);
    // Banner đổi ngay theo cảnh báo máy chủ vừa trả, bảng và dải độ phủ tải lại.
    applyWarnings(nextWarnings);
    reload();
  }

  /** UC-33 — xóa đợt không xóa đảng viên nào, nhưng vẫn luôn hỏi lại. */
  function confirmDelete(period: AwardPeriodResponse) {
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
          const result = await periodsApi.deletePeriod(period.id);
          message.success(messageText('Mes.AwardPeriod.Delete.Successfully'));
          applyWarnings(result.warnings);
          reload();
        } catch (reason) {
          message.error(reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE);
        }
      },
    });
  }

  const columns: ColumnsType<AwardPeriodResponse> = [
    {
      key: 'name',
      title: 'Tên đợt',
      dataIndex: 'name',
      render: (value: string, record) => (
        <Button
          type="link"
          className="hhd-periods__name"
          onClick={() => navigate(periodDetailPath(record.id))}
        >
          {value}
        </Button>
      ),
    },
    {
      key: 'fromDisplay',
      // Ngày/tháng đã được máy chủ định dạng sẵn (fromDisplay), không tự ghép lại.
      title: 'Từ ngày',
      dataIndex: 'fromDisplay',
      width: 130,
      className: 'hhd-periods__daycell',
    },
    {
      key: 'toDisplay',
      title: 'Đến ngày',
      dataIndex: 'toDisplay',
      width: 130,
      className: 'hhd-periods__daycell',
    },
    {
      key: 'status',
      title: `Trạng thái ${year}`,
      width: 180,
      render: (_value, record) => <PeriodStatusTag period={record} />,
    },
    {
      key: 'eligibleCount',
      title: 'Đủ điều kiện năm nay',
      dataIndex: 'eligibleCount',
      width: 200,
      align: 'right',
      render: (value: number, record) => (
        <span style={{ fontWeight: record.status === 'Past' ? 400 : 700 }}>
          {formatNumber(value)} người
        </span>
      ),
    },
    {
      key: 'actions',
      title: 'Thao tác',
      width: 150,
      align: 'right',
      render: (_value, record) => (
        <span className="hhd-periods__row-actions">
          <Tooltip title="Xem chi tiết">
            <Button
              icon={<UnorderedListOutlined />}
              aria-label={`Xem chi tiết ${record.name}`}
              onClick={() => navigate(periodDetailPath(record.id))}
            />
          </Tooltip>
          <Tooltip title="Sửa">
            <Button
              icon={<EditOutlined />}
              aria-label={`Sửa ${record.name}`}
              onClick={() => openEdit(record)}
            />
          </Tooltip>
          <Tooltip title="Xóa">
            <Button
              danger
              icon={<DeleteOutlined />}
              aria-label={`Xóa ${record.name}`}
              onClick={() => confirmDelete(record)}
            />
          </Tooltip>
        </span>
      ),
    },
  ];

  const headingDescription = loading
    ? 'Đang tải danh sách…'
    : error
      ? null
      : isEmpty
        ? 'Chưa có đợt nào'
        : `${formatNumber(totalCount)} đợt · dùng chung cho mọi năm, chỉ lưu ngày/tháng`;

  return (
    <div className="hhd-periods">
      <PageHeading
        title="Đợt trao huy hiệu"
        description={headingDescription}
        extra={
          <Button type="primary" onClick={openCreate}>
            + Thêm đợt
          </Button>
        }
      />

      {showCoverage ? <CoverageWarningBanner warnings={warnings} /> : null}

      {showCoverage ? (
        <CoverageStrip
          year={year}
          today={today}
          segments={segments}
          overlaps={warnings.overlaps}
          periods={periods}
        />
      ) : null}

      <div className="hhd-periods__panel">
        <TableStates
          loading={loading}
          error={error}
          onRetry={reload}
          isEmpty={isEmpty}
          skeletonRows={5}
          description={
            // Cỡ chữ và kiểu chữ lấy theo khối giữa của các artboard trống.
            <span style={{ font: "700 22px/30px 'Noto Serif', Georgia, serif" }}>
              Chưa có đợt trao huy hiệu nào
            </span>
          }
          hint={
            <span
              style={{
                display: 'block',
                maxWidth: 560,
                margin: '8px auto 0',
                fontSize: 16,
                lineHeight: '24px',
              }}
            >
              Bác thêm đợt đầu tiên để hệ thống biết ai sẽ được trao huy hiệu trong năm nay. Đợt chỉ
              lưu ngày và tháng, dùng chung cho mọi năm.
            </span>
          }
          action={<Button onClick={openCreate}>+ Thêm đợt đầu tiên</Button>}
        >
          <Table<AwardPeriodResponse>
            rowKey="id"
            columns={columns}
            dataSource={periods}
            pagination={false}
            // Đợt đang diễn ra hoặc sắp tới được tô nền vàng nhạt như artboard 5.
            rowClassName={(record) => (record.status === 'Past' ? '' : 'hhd-periods__row--next')}
          />
          <div className="hhd-periods__footnote">
            Sắp theo Từ ngày. Sửa hoặc xóa đợt có hiệu lực ngay cho mọi năm.
          </div>
        </TableStates>
      </div>

      {formOpen ? (
        <PeriodFormModal
          period={editing}
          onCancel={() => setFormOpen(false)}
          onSaved={handleSaved}
        />
      ) : null}
    </div>
  );
}
