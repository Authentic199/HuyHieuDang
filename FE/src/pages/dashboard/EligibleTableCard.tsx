import { DownloadOutlined } from '@ant-design/icons';
import { App as AntApp, Button, Table } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useState } from 'react';

import { exportsApi } from '../../api';
import type { UpcomingPeriod } from '../../api/dashboard';
import { FALLBACK_MESSAGE } from '../../api/messages';
import { MilestoneFilterSelect, TableSearchInput } from '../../components/TableFilters';
import { TableStates } from '../../components/TableStates';
import { useClientTable } from '../../hooks/useClientTable';
import { ApiError } from '../../types/api';
import type { EligibleMemberResponse, Gender, IsoDate } from '../../types/domain';
import { saveFile } from '../../utils/download';
import { ELIGIBLE_COMPARATORS, milestoneOfRow, nameOfRow } from '../../utils/eligibleTable';
import { formatDate, formatGender, formatNumber } from '../../utils/format';
import { EligibleMilestoneTag } from '../periods/detail/EligibleMilestoneTag';

/**
 * Bảng đủ điều kiện của đợt sắp tới (UC-11) — artboard 2: đầu thẻ có tên đợt và
 * nút Xuất Excel, giữa là bảng, chân thẻ là số người và tên file vừa tải về.
 *
 * Máy chủ trả trọn danh sách đã sắp theo Mốc huy hiệu rồi Họ tên (mục 1.8 hợp
 * đồng API) và đó là thứ tự mặc định của bảng. Việc tìm, lọc theo mốc, sắp xếp
 * và phân trang làm ngay ở máy khách trên chính mảng đó, không gọi lại máy chủ.
 */

interface EligibleTableCardProps {
  period: UpcomingPeriod | null;
  members: EligibleMemberResponse[];
  loading: boolean;
  /** Câu lỗi đã dịch, null khi không lỗi */
  error: string | null;
  onRetry: () => void;
}

export function EligibleTableCard({
  period,
  members,
  loading,
  error,
  onRetry,
}: EligibleTableCardProps) {
  const { message } = AntApp.useApp();
  const [exporting, setExporting] = useState(false);
  /** Tên file của lần xuất gần nhất, hiện ở chân thẻ như artboard. */
  const [lastFileName, setLastFileName] = useState<string | null>(null);

  const table = useClientTable<EligibleMemberResponse>({
    rows: members,
    searchTextOf: nameOfRow,
    milestoneOf: milestoneOfRow,
    comparators: ELIGIBLE_COMPARATORS,
  });

  /**
   * Xuất TRỌN danh sách của đợt sắp tới (mục 8.2): endpoint không có tham số,
   * máy chủ tự lấy lại đợt theo QT8 và tự đặt tên file. Cố ý KHÔNG nối vào ô tìm
   * hay ô lọc — file Excel luôn đủ người, không phải chỉ trang đang xem.
   */
  async function handleExport() {
    if (exporting) return;
    setExporting(true);
    try {
      const file = await exportsApi.exportDashboard();
      saveFile(file);
      setLastFileName(file.fileName);
      message.success(`Đã tải về ${file.fileName}`);
    } catch (reason) {
      message.error(reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE);
    } finally {
      setExporting(false);
    }
  }

  const columns: ColumnsType<EligibleMemberResponse> = [
    {
      key: 'index',
      title: 'STT',
      width: 72,
      className: 'hhd-dashboard__index',
      // Số thứ tự chạy tiếp qua từng trang và theo đúng thứ tự đang hiện.
      render: (_value, _record, index) => table.indexOffset + index + 1,
    },
    {
      key: 'fullName',
      title: 'Họ tên',
      dataIndex: 'fullName',
      sorter: true,
      sortOrder: table.sortOrderOf('fullName'),
      onCell: () => ({ style: { whiteSpace: 'nowrap', fontWeight: 600 } }),
    },
    {
      key: 'gender',
      title: 'Giới tính',
      dataIndex: 'gender',
      width: 130,
      sorter: true,
      sortOrder: table.sortOrderOf('gender'),
      render: (value: Gender | null) => formatGender(value),
    },
    {
      key: 'dateOfBirth',
      title: 'Ngày sinh',
      dataIndex: 'dateOfBirth',
      width: 150,
      sorter: true,
      sortOrder: table.sortOrderOf('dateOfBirth'),
      render: (value: IsoDate | null) => formatDate(value),
    },
    {
      key: 'officialAdmissionDate',
      title: 'Ngày vào Đảng chính thức',
      dataIndex: 'officialAdmissionDate',
      width: 230,
      sorter: true,
      sortOrder: table.sortOrderOf('officialAdmissionDate'),
      render: (value: IsoDate) => formatDate(value),
    },
    {
      key: 'milestoneDate',
      title: 'Ngày tròn mốc',
      dataIndex: 'milestoneDate',
      width: 175,
      sorter: true,
      sortOrder: table.sortOrderOf('milestoneDate'),
      render: (value: IsoDate) => formatDate(value),
    },
    {
      key: 'milestone',
      title: 'Mốc huy hiệu',
      dataIndex: 'milestone',
      width: 170,
      sorter: true,
      sortOrder: table.sortOrderOf('milestone'),
      render: (value: number) => <EligibleMilestoneTag milestone={value} />,
    },
  ];

  const ready = !loading && !error;
  /** Máy chủ chưa trả ai thì hai ô tìm — lọc chưa có gì để làm. */
  const noData = members.length === 0;

  return (
    <div className="hhd-dashboard__panel">
      <div className="hhd-dashboard__panel-head">
        <div className="hhd-dashboard__panel-title">
          <span>
            {period
              ? `Danh sách đủ điều kiện — ${period.name} năm ${period.year}`
              : 'Danh sách đủ điều kiện'}
          </span>
          <span className="hhd-dashboard__panel-hint">Sắp theo mốc, rồi họ tên</span>
        </div>
        <Button
          type="primary"
          icon={<DownloadOutlined />}
          loading={exporting}
          // Chưa có đợt hoặc không ai đủ điều kiện thì không có gì để xuất.
          disabled={!ready || period === null || noData}
          onClick={handleExport}
        >
          Xuất Excel
        </Button>
      </div>

      <div className={`hhd-table-filters${noData ? ' hhd-table-filters--muted' : ''}`}>
        <TableSearchInput
          value={table.keyword}
          onChange={table.setKeyword}
          placeholder="Tìm theo họ tên…"
          ariaLabel="Tìm theo họ tên"
          disabled={noData}
        />
        <MilestoneFilterSelect
          value={table.milestone}
          onChange={table.setMilestone}
          options={table.milestoneOptions}
          disabled={noData}
        />
        {ready && table.isFiltered ? (
          <span className="hhd-table-filters__count">
            Còn <b>{formatNumber(table.filteredCount)}</b> / {formatNumber(table.totalCount)} người
          </span>
        ) : null}
      </div>

      <TableStates
        loading={loading}
        error={error}
        onRetry={onRetry}
        isEmpty={table.filteredCount === 0}
        skeletonRows={7}
        description={
          <span style={{ font: "700 22px/30px 'Noto Serif', Georgia, serif" }}>
            {/* Rỗng do tìm / lọc là chuyện khác hẳn rỗng do chưa có dữ liệu. */}
            {table.isFiltered
              ? 'Không tìm thấy đảng viên nào khớp.'
              : period === null
                ? 'Chưa cài đợt trao huy hiệu.'
                : `Không có đảng viên nào tròn mốc trong ${period.name} năm ${period.year}.`}
          </span>
        }
        hint={
          <span className="hhd-dashboard__empty-hint">
            {table.isFiltered
              ? 'Bác thử xóa bớt chữ trong ô tìm, hoặc chọn lại Mốc: Tất cả.'
              : period === null
                ? 'Tạo đợt trao huy hiệu trước, danh sách sẽ tự hiện ra.'
                : 'Bác xem mục “Chưa thuộc đợt nào” để biết ai đang bị sót ngoài các đợt.'}
          </span>
        }
        action={table.isFiltered ? <Button onClick={table.clearFilters}>Xóa bộ lọc</Button> : null}
      >
        <div className="hhd-table-scroll">
          <Table<EligibleMemberResponse>
            rowKey="partyMemberId"
            columns={columns}
            dataSource={table.pageRows}
            pagination={table.pagination}
            onChange={table.onTableChange}
          />
        </div>
      </TableStates>

      <div className="hhd-dashboard__footnote">
        <span>{ready ? `${formatNumber(table.totalCount)} người` : ''}</span>
        {lastFileName ? (
          <span className="hhd-dashboard__filename">{lastFileName}</span>
        ) : (
          <span>Danh sách được tính lại mỗi lần xem</span>
        )}
      </div>
    </div>
  );
}
