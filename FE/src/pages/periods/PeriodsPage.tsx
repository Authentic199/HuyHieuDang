import { DeleteOutlined, EditOutlined, UnorderedListOutlined } from '@ant-design/icons';
import { App as AntApp, Button, Table, Tooltip } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { periodsApi } from '../../api';
import { FALLBACK_MESSAGE, messageText } from '../../api/messages';
import { PageHeading } from '../../components/PageHeading';
import { TableSearchInput } from '../../components/TableFilters';
import { TableStates } from '../../components/TableStates';
import {
  compareNumber,
  compareText,
  useClientTable,
  type RowComparators,
} from '../../hooks/useClientTable';
import { periodDetailPath } from '../../routes/paths';
import { ApiError } from '../../types/api';
import type { AwardPeriodResponse, AwardPeriodStatus, CoverageWarnings } from '../../types/domain';
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
 *
 * Máy chủ trả trọn danh sách đã sắp theo Từ ngày tăng dần và đó là thứ tự mặc
 * định; tìm theo tên đợt, sắp xếp và phân trang làm ngay ở máy khách. Màn này
 * không có cột mốc huy hiệu nên KHÔNG có ô lọc mốc.
 */

/** Ngày/tháng của đợt quy về một số so được: 07/11 → 1107. */
function dayMonthKey(day: number, month: number): number {
  return month * 100 + day;
}

/** Thứ tự thời gian trong năm đang xét: đã qua → đang diễn ra → sắp tới. */
const STATUS_RANK: Record<AwardPeriodStatus, number> = { Past: 0, Ongoing: 1, Upcoming: 2 };

const PERIOD_COMPARATORS: RowComparators<AwardPeriodResponse> = {
  name: (a, b) => compareText(a.name, b.name),
  // So theo ngày/tháng thật, không so chuỗi "07/11" — nếu không thì 10/01 lại
  // đứng trước 07/11 chỉ vì chữ "1" nhỏ hơn chữ "7".
  fromDisplay: (a, b) =>
    compareNumber(dayMonthKey(a.fromDay, a.fromMonth), dayMonthKey(b.fromDay, b.fromMonth)),
  toDisplay: (a, b) =>
    compareNumber(dayMonthKey(a.toDay, a.toMonth), dayMonthKey(b.toDay, b.toMonth)),
  status: (a, b) => compareNumber(STATUS_RANK[a.status], STATUS_RANK[b.status]),
  eligibleCount: (a, b) => compareNumber(a.eligibleCount, b.eligibleCount),
};

function nameOfPeriod(period: AwardPeriodResponse): string {
  return period.name;
}

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

  const table = useClientTable<AwardPeriodResponse>({
    rows: periods,
    searchTextOf: nameOfPeriod,
    comparators: PERIOD_COMPARATORS,
  });

  /** Chưa có đợt nào thì ô tìm chưa có gì để làm. */
  const noData = periods.length === 0;
  const ready = !loading && !error;
  const showCoverage = ready && !noData;

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
      sorter: true,
      sortOrder: table.sortOrderOf('name'),
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
      width: 140,
      className: 'hhd-periods__daycell',
      sorter: true,
      sortOrder: table.sortOrderOf('fromDisplay'),
    },
    {
      key: 'toDisplay',
      title: 'Đến ngày',
      dataIndex: 'toDisplay',
      width: 150,
      className: 'hhd-periods__daycell',
      sorter: true,
      sortOrder: table.sortOrderOf('toDisplay'),
    },
    {
      key: 'status',
      title: `Trạng thái ${year}`,
      width: 190,
      sorter: true,
      sortOrder: table.sortOrderOf('status'),
      render: (_value, record) => <PeriodStatusTag period={record} />,
    },
    {
      key: 'eligibleCount',
      title: 'Đủ điều kiện năm nay',
      dataIndex: 'eligibleCount',
      width: 215,
      align: 'right',
      sorter: true,
      sortOrder: table.sortOrderOf('eligibleCount'),
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
    : error || noData
      ? null
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
        <div className={`hhd-table-filters${noData ? ' hhd-table-filters--muted' : ''}`}>
          <TableSearchInput
            value={table.keyword}
            onChange={table.setKeyword}
            placeholder="Tìm theo tên đợt…"
            ariaLabel="Tìm theo tên đợt"
            disabled={noData}
          />
          {ready && table.isFiltered ? (
            <span className="hhd-table-filters__count">
              Còn <b>{formatNumber(table.filteredCount)}</b> / {formatNumber(table.totalCount)} đợt
            </span>
          ) : null}
        </div>

        <TableStates
          loading={loading}
          error={error}
          onRetry={reload}
          isEmpty={table.filteredCount === 0}
          skeletonRows={5}
          description={
            // Cỡ chữ và kiểu chữ lấy theo khối giữa của các artboard trống.
            <span style={{ font: "700 22px/30px 'Noto Serif', Georgia, serif" }}>
              {/* Rỗng do tìm là chuyện khác hẳn rỗng do chưa cài đợt nào. */}
              {table.isFiltered ? 'Không tìm thấy đợt nào khớp.' : 'Chưa có đợt trao huy hiệu nào'}
            </span>
          }
          hint={
            table.isFiltered ? (
              <span
                style={{
                  display: 'block',
                  maxWidth: 560,
                  margin: '8px auto 0',
                  fontSize: 16,
                  lineHeight: '24px',
                }}
              >
                Bác thử xóa bớt chữ trong ô tìm.
              </span>
            ) : null
          }
          action={
            table.isFiltered ? (
              <Button onClick={table.clearFilters}>Xóa bộ lọc</Button>
            ) : (
              <Button onClick={openCreate}>+ Thêm đợt đầu tiên</Button>
            )
          }
        >
          <div className="hhd-table-scroll">
            <Table<AwardPeriodResponse>
              rowKey="id"
              columns={columns}
              dataSource={table.pageRows}
              pagination={table.pagination}
              onChange={table.onTableChange}
              // Đợt đang diễn ra hoặc sắp tới được tô nền vàng nhạt như artboard 5.
              rowClassName={(record) => (record.status === 'Past' ? '' : 'hhd-periods__row--next')}
            />
          </div>
        </TableStates>

        <div className="hhd-periods__footnote">
          Sắp theo Từ ngày. Sửa hoặc xóa đợt có hiệu lực ngay cho mọi năm.
        </div>
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
