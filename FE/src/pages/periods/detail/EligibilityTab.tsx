import { DownloadOutlined } from '@ant-design/icons';
import { App as AntApp, Button, Segmented, Skeleton, Table } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useState } from 'react';

import { exportsApi } from '../../../api';
import { FALLBACK_MESSAGE } from '../../../api/messages';
import { MilestoneFilterSelect, TableSearchInput } from '../../../components/TableFilters';
import { TableStates } from '../../../components/TableStates';
import { useClientTable } from '../../../hooks/useClientTable';
import { ApiError } from '../../../types/api';
import type { EligibleMemberResponse, Gender, IsoDate } from '../../../types/domain';
import { saveFile } from '../../../utils/download';
import { ELIGIBLE_COMPARATORS, milestoneOfRow, nameOfRow } from '../../../utils/eligibleTable';
import { formatDate, formatGender, formatNumber } from '../../../utils/format';
import { EligibleMilestoneTag } from './EligibleMilestoneTag';
import { useEligibility } from './useEligibility';
import { yearContextOf, yearsAround } from './yearContext';
import { YearContextTag } from './YearContextTag';

/**
 * Tab "Danh sách đủ điều kiện" của trang chi tiết đợt (UC-34), theo artboard
 * "Màn 5 — Chi tiết đợt": thanh công cụ (bộ chọn năm, khoảng ngày đã gắn năm,
 * nhãn ngữ cảnh, nút Xuất Excel), thanh tìm — lọc, rồi bảng và dòng chân thẻ.
 *
 * Máy chủ trả trọn danh sách đã sắp theo Mốc rồi Họ tên; đó là thứ tự mặc định.
 * Tìm, lọc theo mốc, sắp xếp và phân trang làm ngay ở máy khách.
 */

interface EligibilityTabProps {
  awardPeriodId: string;
  /** Năm hiện tại của MÁY CHỦ — mốc giữa của bộ chọn năm */
  serverYear: number | null;
  year: number | null;
  onYearChange: (year: number) => void;
  /** Đổi giá trị khi đợt vừa được sửa — danh sách tải lại theo khoảng ngày mới */
  refreshToken: number;
}

export function EligibilityTab({
  awardPeriodId,
  serverYear,
  year,
  onYearChange,
  refreshToken,
}: EligibilityTabProps) {
  const { message } = AntApp.useApp();
  const { awardPeriod, members, totalCount, loading, error, reload } = useEligibility(
    awardPeriodId,
    year,
    refreshToken,
  );
  const [exporting, setExporting] = useState(false);
  /** Tên file của lần xuất gần nhất, hiện ở chân thẻ như artboard. */
  const [lastFileName, setLastFileName] = useState<string | null>(null);

  const table = useClientTable<EligibleMemberResponse>({
    rows: members,
    searchTextOf: nameOfRow,
    milestoneOf: milestoneOfRow,
    comparators: ELIGIBLE_COMPARATORS,
  });

  const context = year !== null && serverYear !== null ? yearContextOf(year, serverYear) : null;

  /**
   * Xuất TRỌN danh sách của đúng năm đang chọn; tên file do máy chủ đặt (mục 4).
   * Cố ý KHÔNG nối vào ô tìm hay ô lọc mốc — file Excel luôn đủ người.
   */
  async function handleExport() {
    if (year === null || exporting) return;
    setExporting(true);
    try {
      const file = await exportsApi.exportEligibility(awardPeriodId, year);
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
      className: 'hhd-eligibility__index',
      // Số thứ tự chạy tiếp qua từng trang và theo đúng thứ tự đang hiện — người
      // dùng tự bấm sắp xếp thì thứ tự máy chủ không còn là thứ tự trên màn hình.
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
  /** Năm này không ai đủ điều kiện thì hai ô tìm — lọc chưa có gì để làm. */
  const noData = members.length === 0;

  return (
    <div className="hhd-eligibility">
      <div className="hhd-eligibility__toolbar">
        <Segmented<number>
          size="large"
          value={year ?? undefined}
          options={serverYear === null ? [] : yearsAround(serverYear)}
          onChange={onYearChange}
          disabled={serverYear === null}
        />

        <div className="hhd-eligibility__range">
          {/* Khoảng ngày đã gắn năm do máy chủ trả, không tự ghép ở giao diện. */}
          {awardPeriod ? (
            <span className="hhd-eligibility__dates">
              {formatDate(awardPeriod.fromDate)} – {formatDate(awardPeriod.toDate)}
            </span>
          ) : (
            <Skeleton.Input active size="small" style={{ width: 190, minWidth: 190 }} />
          )}
          {context ? <YearContextTag context={context} /> : null}
          <span className="hhd-eligibility__count">
            {loading || error
              ? ''
              : context === 'Next'
                ? `${formatNumber(totalCount)} người · chuẩn bị trước`
                : `${formatNumber(totalCount)} người`}
          </span>
        </div>

        <Button
          type="primary"
          icon={<DownloadOutlined />}
          loading={exporting}
          disabled={year === null}
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
        onRetry={reload}
        isEmpty={table.filteredCount === 0}
        skeletonRows={5}
        description={
          <span style={{ font: "700 22px/30px 'Noto Serif', Georgia, serif" }}>
            {/* Rỗng do tìm / lọc là chuyện khác hẳn rỗng do năm đó không ai tròn mốc. */}
            {table.isFiltered
              ? 'Không tìm thấy đảng viên nào khớp.'
              : `Không có đảng viên nào tròn mốc trong đợt này năm ${year ?? ''}.`}
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
            {table.isFiltered
              ? 'Bác thử xóa bớt chữ trong ô tìm, hoặc chọn lại Mốc: Tất cả.'
              : 'Bác thử chọn năm khác ở trên, hoặc xem mục “Chưa thuộc đợt nào” để biết ai đang bị sót.'}
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

      <div className="hhd-eligibility__footnote">
        <span>{loading || error ? '' : `${formatNumber(totalCount)} người`}</span>
        {lastFileName ? (
          <span className="hhd-eligibility__filename">{lastFileName}</span>
        ) : (
          <span>Sắp theo Mốc huy hiệu rồi Họ tên</span>
        )}
      </div>
    </div>
  );
}
