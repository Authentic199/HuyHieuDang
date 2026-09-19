import { DownloadOutlined } from '@ant-design/icons';
import { App as AntApp, Button, Segmented, Skeleton, Table } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useState } from 'react';

import { exportsApi } from '../../../api';
import { FALLBACK_MESSAGE } from '../../../api/messages';
import { TableStates } from '../../../components/TableStates';
import { ApiError } from '../../../types/api';
import type { EligibleMemberResponse, Gender, IsoDate } from '../../../types/domain';
import { saveFile } from '../../../utils/download';
import { formatDate, formatGender, formatNumber } from '../../../utils/format';
import { EligibleMilestoneTag } from './EligibleMilestoneTag';
import { useEligibility } from './useEligibility';
import { yearContextOf, yearsAround } from './yearContext';
import { YearContextTag } from './YearContextTag';

/**
 * Tab "Danh sách đủ điều kiện" của trang chi tiết đợt (UC-34), theo artboard
 * "Màn 5 — Chi tiết đợt": thanh công cụ (bộ chọn năm, khoảng ngày đã gắn năm,
 * nhãn ngữ cảnh, nút Xuất Excel) rồi bảng, cuối cùng là dòng chân thẻ.
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

  const context = year !== null && serverYear !== null ? yearContextOf(year, serverYear) : null;

  /** Xuất Excel theo đúng năm đang chọn; tên file do máy chủ đặt (mục 4). */
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
      // Máy chủ đã sắp theo Mốc rồi Họ tên; số thứ tự chỉ đếm theo thứ tự đó.
      render: (_value, _record, index) => index + 1,
    },
    {
      key: 'fullName',
      title: 'Họ tên',
      dataIndex: 'fullName',
      onCell: () => ({ style: { whiteSpace: 'nowrap', fontWeight: 600 } }),
    },
    {
      key: 'gender',
      title: 'Giới tính',
      dataIndex: 'gender',
      width: 110,
      render: (value: Gender | null) => formatGender(value),
    },
    {
      key: 'dateOfBirth',
      title: 'Ngày sinh',
      dataIndex: 'dateOfBirth',
      width: 140,
      render: (value: IsoDate | null) => formatDate(value),
    },
    {
      key: 'officialAdmissionDate',
      title: 'Ngày vào Đảng chính thức',
      dataIndex: 'officialAdmissionDate',
      width: 220,
      render: (value: IsoDate) => formatDate(value),
    },
    {
      key: 'milestoneDate',
      title: 'Ngày tròn mốc',
      dataIndex: 'milestoneDate',
      width: 160,
      render: (value: IsoDate) => formatDate(value),
    },
    {
      key: 'milestone',
      title: 'Mốc huy hiệu',
      dataIndex: 'milestone',
      width: 150,
      render: (value: number) => <EligibleMilestoneTag milestone={value} />,
    },
  ];

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

      <TableStates
        loading={loading}
        error={error}
        onRetry={reload}
        isEmpty={members.length === 0}
        skeletonRows={5}
        description={
          <span style={{ font: "700 22px/30px 'Noto Serif', Georgia, serif" }}>
            Không có đảng viên nào tròn mốc trong đợt này năm {year ?? ''}.
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
            Bác thử chọn năm khác ở trên, hoặc xem mục “Chưa thuộc đợt nào” để biết ai đang bị sót.
          </span>
        }
      >
        <Table<EligibleMemberResponse>
          rowKey="partyMemberId"
          columns={columns}
          dataSource={members}
          pagination={false}
        />
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
