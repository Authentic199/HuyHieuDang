import { DownloadOutlined, SearchOutlined } from '@ant-design/icons';
import { App as AntApp, Button, Input, Table } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import type { TablePaginationConfig } from 'antd/es/table/interface';
import { useMemo, useState } from 'react';

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
 * Bảng đủ điều kiện của đợt sắp tới (UC-11) — artboard 2: đầu thẻ có tên đợt,
 * ô tìm và nút Xuất Excel, giữa là bảng, chân thẻ là số người và tên file vừa tải về.
 *
 * Máy chủ đã sắp theo Mốc huy hiệu rồi Họ tên (mục 1.8 hợp đồng API) nên đó là
 * thứ tự mặc định. Cán bộ có thể tìm, sắp lại và lật trang ngay tại đây giống
 * màn Đảng viên; khác một điểm: cả danh sách đã nằm sẵn trong lời gọi
 * `GET /api/Dashboard` (mục 6.1) nên tìm — sắp — phân trang làm ngay ở giao
 * diện, không gọi thêm máy chủ. Bỏ sắp xếp (bấm lần thứ ba) thì về đúng thứ tự
 * máy chủ đã trả.
 */

/** Các lựa chọn số dòng mỗi trang, lấy đúng bộ của màn Đảng viên. */
const PAGE_SIZES = [10, 20, 50, 100];

/** Số dòng mỗi trang mặc định, bằng màn Đảng viên (mục 1.7 hợp đồng API). */
const DEFAULT_PAGE_SIZE = 20;

/**
 * Bỏ dấu và hạ chữ thường để gõ "nguyen van" vẫn tìm ra "Nguyễn Văn" — cán bộ
 * lớn tuổi thường gõ không dấu.
 */
function searchKey(text: string): string {
  return text
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/đ/g, 'd');
}

/** Chữ dùng để sắp cột Giới tính; chưa ghi thì để trống cho xuống cuối bảng. */
function genderText(value: Gender | null): string | null {
  return value ? formatGender(value) : null;
}

/** So chữ theo tiếng Việt; ô trống luôn xuống cuối bảng khi sắp tăng dần. */
function compareText(a: string | null, b: string | null): number {
  if (!a) return b ? 1 : 0;
  if (!b) return -1;
  return a.localeCompare(b, 'vi');
}

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
  const [keyword, setKeyword] = useState('');
  const [page, setPage] = useState({ current: 1, pageSize: DEFAULT_PAGE_SIZE });

  /**
   * Xuất đúng danh sách đang hiện (mục 8.2): endpoint không có tham số, máy chủ
   * tự lấy lại đợt sắp tới theo QT8 và tự đặt tên file. Ô tìm chỉ lọc trên màn
   * hình nên file Excel vẫn là CẢ danh sách đủ điều kiện của đợt.
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

  const searching = keyword.trim() !== '';

  const rows = useMemo(() => {
    const needle = searchKey(keyword.trim());
    if (!needle) return members;
    return members.filter((member) => searchKey(member.fullName).includes(needle));
  }, [keyword, members]);

  // Danh sách vừa tải lại mà ngắn đi thì trang đang xem có thể không còn nữa —
  // kẹp lại ngay khi dựng, tránh để cán bộ nhìn một trang trắng.
  const pageCount = Math.max(1, Math.ceil(rows.length / page.pageSize));
  const current = Math.min(page.current, pageCount);

  const columns: ColumnsType<EligibleMemberResponse> = [
    {
      key: 'index',
      title: 'STT',
      width: 72,
      className: 'hhd-dashboard__index',
      // Đếm theo đúng thứ tự đang hiện, cộng thêm số dòng của các trang trước.
      render: (_value, _record, index) => (current - 1) * page.pageSize + index + 1,
    },
    {
      key: 'fullName',
      title: 'Họ tên',
      dataIndex: 'fullName',
      sorter: (a, b) => compareText(a.fullName, b.fullName),
      onCell: () => ({ style: { whiteSpace: 'nowrap', fontWeight: 600 } }),
    },
    {
      key: 'gender',
      title: 'Giới tính',
      dataIndex: 'gender',
      width: 110,
      // So theo chữ đang hiện (Nam / Nữ), người chưa ghi giới tính xuống cuối.
      sorter: (a, b) => compareText(genderText(a.gender), genderText(b.gender)),
      render: (value: Gender | null) => formatGender(value),
    },
    {
      key: 'dateOfBirth',
      title: 'Ngày sinh',
      dataIndex: 'dateOfBirth',
      width: 140,
      // Ngày ở dạng yyyy-MM-dd nên so chuỗi cũng ra đúng thứ tự thời gian.
      sorter: (a, b) => compareText(a.dateOfBirth, b.dateOfBirth),
      render: (value: IsoDate | null) => formatDate(value),
    },
    {
      key: 'officialAdmissionDate',
      title: 'Ngày vào Đảng chính thức',
      dataIndex: 'officialAdmissionDate',
      width: 220,
      sorter: (a, b) => compareText(a.officialAdmissionDate, b.officialAdmissionDate),
      render: (value: IsoDate) => formatDate(value),
    },
    {
      key: 'milestoneDate',
      title: 'Ngày tròn mốc',
      dataIndex: 'milestoneDate',
      width: 160,
      sorter: (a, b) => compareText(a.milestoneDate, b.milestoneDate),
      render: (value: IsoDate) => formatDate(value),
    },
    {
      key: 'milestone',
      title: 'Mốc huy hiệu',
      dataIndex: 'milestone',
      width: 150,
      // Mốc lớn hơn thì xếp sau; bằng nhau thì giữ thứ tự họ tên của máy chủ.
      sorter: (a, b) => a.milestone - b.milestone || compareText(a.fullName, b.fullName),
      render: (value: number) => <EligibleMilestoneTag milestone={value} />,
    },
  ];

  function handleTableChange(
    pagination: TablePaginationConfig,
    action: 'paginate' | 'sort' | 'filter',
  ) {
    setPage({
      pageSize: pagination.pageSize ?? DEFAULT_PAGE_SIZE,
      // Đổi cách sắp xếp thì thứ tự cả danh sách đổi theo — xem lại từ trang 1.
      current: action === 'sort' ? 1 : (pagination.current ?? 1),
    });
  }

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
        </div>
        <Input
          className="hhd-dashboard__search"
          allowClear
          prefix={<SearchOutlined />}
          placeholder="Tìm theo họ tên…"
          aria-label="Tìm theo họ tên trong danh sách đủ điều kiện"
          value={keyword}
          disabled={!ready || members.length === 0}
          onChange={(event) => {
            // Chữ tìm đổi thì danh sách đổi theo — xem lại từ trang 1.
            setKeyword(event.target.value);
            setPage((state) => ({ ...state, current: 1 }));
          }}
        />
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
        isEmpty={rows.length === 0}
        skeletonRows={7}
        description={
          <span style={{ font: "700 22px/30px 'Noto Serif', Georgia, serif" }}>
            {searching
              ? 'Không tìm thấy ai như vậy'
              : period === null
                ? 'Chưa cài đợt trao huy hiệu.'
                : `Không có đảng viên nào tròn mốc trong ${period.name} năm ${period.year}.`}
          </span>
        }
        hint={
          <span className="hhd-dashboard__empty-hint">
            {searching
              ? 'Bác thử xóa bớt chữ trong ô tìm ở đầu bảng.'
              : period === null
                ? 'Tạo đợt trao huy hiệu trước, danh sách sẽ tự hiện ra.'
                : 'Bác xem mục “Chưa thuộc đợt nào” để biết ai đang bị sót ngoài các đợt.'}
          </span>
        }
        action={
          searching ? (
            <Button
              onClick={() => {
                setKeyword('');
                setPage((state) => ({ ...state, current: 1 }));
              }}
            >
              Xem lại tất cả
            </Button>
          ) : undefined
        }
      >
        <Table<EligibleMemberResponse>
          rowKey="partyMemberId"
          columns={columns}
          dataSource={rows}
          onChange={(pagination, _filters, _sorter, extra) =>
            handleTableChange(pagination, extra.action)
          }
          pagination={{
            current,
            pageSize: page.pageSize,
            total: rows.length,
            showSizeChanger: true,
            pageSizeOptions: PAGE_SIZES,
            showTotal: (total, range) =>
              `${formatNumber(range[0])}–${formatNumber(range[1])} / ${formatNumber(total)}`,
          }}
        />
      </TableStates>

      <div className="hhd-dashboard__footnote">
        <span>
          {!ready
            ? ''
            : searching
              ? `${formatNumber(rows.length)} / ${formatNumber(members.length)} người`
              : `${formatNumber(members.length)} người`}
        </span>
        {lastFileName ? (
          <span className="hhd-dashboard__filename">{lastFileName}</span>
        ) : (
          <span>Danh sách được tính lại mỗi lần xem</span>
        )}
      </div>
    </div>
  );
}
