import { DownloadOutlined } from '@ant-design/icons';
import { App as AntApp, Button, Segmented, Table } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { exportsApi } from '../../api';
import { FALLBACK_MESSAGE } from '../../api/messages';
import { gapLabel } from '../../api/uncovered';
import { PageHeading } from '../../components/PageHeading';
import { TableStates } from '../../components/TableStates';
import { paths } from '../../routes/paths';
import { ApiError } from '../../types/api';
import type { Gender, IsoDate, UnassignedGap, UnassignedMemberResponse } from '../../types/domain';
import { saveFile } from '../../utils/download';
import { formatDate, formatGender, formatNumber } from '../../utils/format';
// Viên thuốc mốc dùng chung với tab Danh sách đủ điều kiện của màn chi tiết đợt.
import { EligibleMilestoneTag } from '../periods/detail/EligibleMilestoneTag';
import { yearsAround } from '../periods/detail/yearContext';
import { UncoveredCountTag } from './UncoveredCountTag';
import { UncoveredHint } from './UncoveredHint';
import './UncoveredPage.css';
import { useUnassigned } from './useUnassigned';

/**
 * Màn 6 — Chưa thuộc đợt nào (UC-40, QT7), theo artboard
 * "Màn 6 — Chưa thuộc đợt nào".
 *
 * Đây là những người TRÒN MỐC trong năm nhưng ngày tròn mốc rơi ra ngoài mọi
 * đợt. Màn này chỉ để nhìn ra và xuất Excel; việc sửa khoảng ngày làm bên màn
 * Đợt trao huy hiệu, nên khối gợi ý dẫn thẳng sang đó.
 */
export default function UncoveredPage() {
  const { message } = AntApp.useApp();
  const navigate = useNavigate();
  const { serverYear, year, selectYear, members, totalCount, loading, error, reload } =
    useUnassigned();

  const [exporting, setExporting] = useState(false);
  /** Tên file của lần xuất gần nhất, hiện ở chân thẻ như artboard. */
  const [lastFileName, setLastFileName] = useState<string | null>(null);

  /** Năm nào phủ kín thì không còn gì để xuất và cũng không cần nhắc gì. */
  const isEmpty = members.length === 0;
  const ready = !loading && !error;

  /** Xuất Excel theo đúng năm đang chọn; tên file do máy chủ đặt (mục 4). */
  async function handleExport() {
    if (year === null || exporting) return;
    setExporting(true);
    try {
      const file = await exportsApi.exportUnassigned(year);
      saveFile(file);
      setLastFileName(file.fileName);
      message.success(`Đã tải về ${file.fileName}`);
    } catch (reason) {
      message.error(reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE);
    } finally {
      setExporting(false);
    }
  }

  const columns: ColumnsType<UnassignedMemberResponse> = [
    {
      key: 'index',
      title: 'STT',
      width: 72,
      className: 'hhd-uncovered__index',
      // Máy chủ đã sắp theo Ngày tròn mốc rồi Họ tên; STT chỉ đếm theo thứ tự đó.
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
    {
      key: 'gap',
      title: 'Khoảng trống',
      dataIndex: 'gap',
      width: 260,
      className: 'hhd-uncovered__gap',
      render: (value: UnassignedGap) => gapLabel(value),
    },
  ];

  return (
    <div className="hhd-uncovered">
      <PageHeading
        title="Chưa thuộc đợt nào"
        description={
          <div className="hhd-uncovered__lead">
            Những người tròn mốc trong năm nhưng ngày tròn mốc không rơi vào đợt nào. Hãy điều chỉnh
            khoảng ngày các đợt để phủ kín.
          </div>
        }
        extra={
          <Segmented<number>
            size="large"
            value={year ?? undefined}
            options={serverYear === null ? [] : yearsAround(serverYear)}
            onChange={selectYear}
            disabled={serverYear === null}
          />
        }
      />

      {ready && totalCount > 0 ? (
        <UncoveredHint count={totalCount} onGoToPeriods={() => navigate(paths.periods)} />
      ) : null}

      <div className="hhd-uncovered__panel">
        <div className="hhd-uncovered__head">
          <div className="hhd-uncovered__title">
            <span>Bị sót trong năm {year ?? ''}</span>
            {ready ? <UncoveredCountTag count={totalCount} /> : null}
          </div>
          <Button
            type="primary"
            icon={<DownloadOutlined />}
            loading={exporting}
            disabled={year === null || !ready || isEmpty}
            onClick={handleExport}
          >
            Xuất Excel
          </Button>
        </div>

        <TableStates
          loading={loading}
          error={error}
          onRetry={reload}
          isEmpty={isEmpty}
          skeletonRows={5}
          description={
            <span style={{ font: "700 22px/30px 'Noto Serif', Georgia, serif" }}>
              Không có ai bị sót trong năm {year ?? ''}.
            </span>
          }
        >
          <Table<UnassignedMemberResponse>
            rowKey="partyMemberId"
            columns={columns}
            dataSource={members}
            pagination={false}
          />
        </TableStates>

        <div className="hhd-uncovered__footnote">
          <span>{ready ? `${formatNumber(totalCount)} người` : ''}</span>
          {lastFileName ? (
            <span className="hhd-uncovered__filename">{lastFileName}</span>
          ) : (
            <span>Sắp theo Ngày tròn mốc rồi Họ tên</span>
          )}
        </div>
      </div>
    </div>
  );
}
