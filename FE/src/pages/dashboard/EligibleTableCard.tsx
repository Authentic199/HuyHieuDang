import { DownloadOutlined } from '@ant-design/icons';
import { App as AntApp, Button, Table } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useState } from 'react';

import { exportsApi } from '../../api';
import type { UpcomingPeriod } from '../../api/dashboard';
import { FALLBACK_MESSAGE } from '../../api/messages';
import { TableStates } from '../../components/TableStates';
import { ApiError } from '../../types/api';
import type { EligibleMemberResponse, Gender, IsoDate } from '../../types/domain';
import { saveFile } from '../../utils/download';
import { formatDate, formatGender, formatNumber } from '../../utils/format';
import { EligibleMilestoneTag } from '../periods/detail/EligibleMilestoneTag';

/**
 * Bảng đủ điều kiện của đợt sắp tới (UC-11) — artboard 2: đầu thẻ có tên đợt và
 * nút Xuất Excel, giữa là bảng, chân thẻ là số người và tên file vừa tải về.
 *
 * Máy chủ đã sắp theo Mốc huy hiệu rồi Họ tên (mục 1.8 hợp đồng API); giao diện
 * GIỮ NGUYÊN thứ tự đó, không sắp lại và không phân trang.
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

  /**
   * Xuất đúng danh sách đang hiện (mục 8.2): endpoint không có tham số, máy chủ
   * tự lấy lại đợt sắp tới theo QT8 và tự đặt tên file.
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

  const ready = !loading && !error;

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
          disabled={!ready || period === null || members.length === 0}
          onClick={handleExport}
        >
          Xuất Excel
        </Button>
      </div>

      <TableStates
        loading={loading}
        error={error}
        onRetry={onRetry}
        isEmpty={members.length === 0}
        skeletonRows={7}
        description={
          <span style={{ font: "700 22px/30px 'Noto Serif', Georgia, serif" }}>
            {period === null
              ? 'Chưa cài đợt trao huy hiệu.'
              : `Không có đảng viên nào tròn mốc trong ${period.name} năm ${period.year}.`}
          </span>
        }
        hint={
          <span className="hhd-dashboard__empty-hint">
            {period === null
              ? 'Tạo đợt trao huy hiệu trước, danh sách sẽ tự hiện ra.'
              : 'Bác xem mục “Chưa thuộc đợt nào” để biết ai đang bị sót ngoài các đợt.'}
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

      <div className="hhd-dashboard__footnote">
        <span>{ready ? `${formatNumber(members.length)} người` : ''}</span>
        {lastFileName ? (
          <span className="hhd-dashboard__filename">{lastFileName}</span>
        ) : (
          <span>Danh sách được tính lại mỗi lần xem</span>
        )}
      </div>
    </div>
  );
}
