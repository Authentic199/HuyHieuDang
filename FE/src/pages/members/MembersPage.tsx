import { DeleteOutlined, EditOutlined, SearchOutlined } from '@ant-design/icons';
import { App as AntApp, Button, ConfigProvider, Input, Select, Table, Tooltip } from 'antd';
import type { ColumnsType, SorterResult, TablePaginationConfig } from 'antd/es/table/interface';
import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { importsApi, membersApi } from '../../api';
import { FALLBACK_MESSAGE, messageText } from '../../api/messages';
import { PageHeading } from '../../components/PageHeading';
import { TableStates } from '../../components/TableStates';
import { useAuth } from '../../auth/useAuth';
import { paths } from '../../routes/paths';
import { color } from '../../theme/tokens';
import { ApiError } from '../../types/api';
import type { Gender, PartyMemberResponse } from '../../types/domain';
import { formatDate, formatGender, formatNumber } from '../../utils/format';
import { saveFile } from '../../utils/download';
import { MemberFormModal } from './MemberFormModal';
import { MilestoneTag } from './MilestoneTag';
import './MembersPage.css';
import {
  DEFAULT_MEMBERS_QUERY,
  MEMBERS_PAGE_SIZES,
  useMembers,
  type GenderFilter,
} from './useMembers';

/** Chờ người dùng gõ xong rồi mới gọi máy chủ, đỡ giật bảng. */
const SEARCH_DEBOUNCE_MS = 400;

type MemberSorter = SorterResult<PartyMemberResponse> | SorterResult<PartyMemberResponse>[];

/** Màn 3 — Đảng viên (UC-20 đến UC-23). */
export default function MembersPage() {
  const { session } = useAuth();
  const { message, modal } = AntApp.useApp();
  const navigate = useNavigate();
  const { query, rows, pageInfo, loading, error, isFiltered, setQuery, reload, reloadAfterDelete } =
    useMembers();

  const [keywordInput, setKeywordInput] = useState('');
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [formOpen, setFormOpen] = useState(false);
  const [editing, setEditing] = useState<PartyMemberResponse | null>(null);
  const [downloadingTemplate, setDownloadingTemplate] = useState(false);

  // Đổi trang hay đổi bộ lọc thì bỏ đánh dấu, tránh xóa nhầm người không còn
  // nhìn thấy trên màn hình.
  const changeQuery = useCallback(
    (patch: Parameters<typeof setQuery>[0]) => {
      setSelectedIds([]);
      setQuery(patch);
    },
    [setQuery],
  );

  useEffect(() => {
    const timer = window.setTimeout(
      () => changeQuery({ keyword: keywordInput.trim() }),
      SEARCH_DEBOUNCE_MS,
    );
    return () => window.clearTimeout(timer);
  }, [keywordInput, changeQuery]);

  const totalCount = pageInfo?.totalCount ?? 0;
  const isEmpty = rows.length === 0;
  const isPristineEmpty = isEmpty && !isFiltered && !loading && !error;
  const selectedRows = rows.filter((row) => selectedIds.includes(row.id));

  const [sortField, sortDirection] = query.sortQuery.split(' ');
  const sortOrderOf = (field: string) =>
    sortField === field ? (sortDirection === 'desc' ? 'descend' : 'ascend') : null;

  function openCreate() {
    setEditing(null);
    setFormOpen(true);
  }

  function openEdit(member: PartyMemberResponse) {
    setEditing(member);
    setFormOpen(true);
  }

  function handleSaved(savedMessage: string) {
    setFormOpen(false);
    setEditing(null);
    message.success(savedMessage);
    reload();
  }

  /** UC-23 — xóa những người đang đánh dấu, luôn hỏi lại trước. */
  function confirmDelete(targets: PartyMemberResponse[]) {
    const count = targets.length;
    if (count === 0) return;

    modal.confirm({
      width: 520,
      title:
        count === 1
          ? `Xóa ${targets[0].fullName} khỏi danh sách?`
          : `Xóa ${formatNumber(count)} người khỏi danh sách?`,
      content:
        count === 1
          ? 'Xóa rồi là mất hẳn, không lấy lại được. Bác xem kỹ giúp trước khi xóa.'
          : `Cả ${formatNumber(count)} người đang chọn sẽ bị xóa hẳn, không lấy lại được. Bác xem kỹ giúp trước khi xóa.`,
      okText: count === 1 ? 'Xóa người này' : `Xóa ${formatNumber(count)} người`,
      cancelText: 'Để lại',
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          // Chỉ còn một đường xóa duy nhất: một người cũng gửi mảng một phần tử.
          await membersApi.deleteManyMembers(targets.map((row) => row.id));
          message.success(messageText('Mes.PartyMember.Delete.Successfully'));
          setSelectedIds([]);
          reloadAfterDelete(count);
        } catch (reason) {
          message.error(reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE);
        }
      },
    });
  }

  /** UC-25 — file mẫu 4 cột, lối vào nhanh nhất khi danh sách còn trống. */
  async function handleDownloadTemplate() {
    setDownloadingTemplate(true);
    try {
      saveFile(await importsApi.downloadTemplate());
    } catch (reason) {
      message.error(reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE);
    } finally {
      setDownloadingTemplate(false);
    }
  }

  function handleTableChange(pagination: TablePaginationConfig, sorter: MemberSorter) {
    const single = Array.isArray(sorter) ? sorter[0] : sorter;
    const field = single?.columnKey as string | undefined;
    const nextSort =
      field && single?.order
        ? `${field} ${single.order === 'descend' ? 'desc' : 'asc'}`
        : DEFAULT_MEMBERS_QUERY.sortQuery;
    const sortChanged = nextSort !== query.sortQuery;
    changeQuery({
      sortQuery: nextSort,
      pageSize: pagination.pageSize ?? query.pageSize,
      // Đổi cách sắp xếp thì thứ tự cả danh sách đổi theo — xem lại từ trang 1.
      current: sortChanged ? 1 : (pagination.current ?? 1),
    });
  }

  const columns: ColumnsType<PartyMemberResponse> = [
    {
      key: 'FullName',
      title: 'Họ tên',
      dataIndex: 'fullName',
      sorter: true,
      sortOrder: sortOrderOf('FullName'),
      onCell: () => ({ style: { whiteSpace: 'nowrap', fontWeight: 600 } }),
    },
    {
      key: 'Gender',
      title: 'Giới tính',
      dataIndex: 'gender',
      width: 120,
      sorter: true,
      sortOrder: sortOrderOf('Gender'),
      render: (value: Gender | null) => formatGender(value),
    },
    {
      key: 'DateOfBirth',
      title: 'Ngày sinh',
      dataIndex: 'dateOfBirth',
      width: 132,
      sorter: true,
      sortOrder: sortOrderOf('DateOfBirth'),
      render: (value: string | null) => formatDate(value),
    },
    {
      key: 'OfficialAdmissionDate',
      title: 'Ngày chính thức',
      dataIndex: 'officialAdmissionDate',
      width: 148,
      sorter: true,
      sortOrder: sortOrderOf('OfficialAdmissionDate'),
      render: (value: string) => formatDate(value),
    },
    {
      // Tuổi đảng là giá trị tính ra nên không sắp xếp được (mục 1.7).
      key: 'partyAgeYears',
      title: 'Tuổi đảng',
      dataIndex: 'partyAgeYears',
      width: 110,
      align: 'right',
      render: (value: number) => formatNumber(value),
    },
    {
      key: 'nextMilestone',
      title: 'Mốc kế tiếp',
      dataIndex: 'nextMilestone',
      width: 140,
      render: (value: number | null) => <MilestoneTag milestone={value} />,
    },
    {
      key: 'nextMilestoneDate',
      title: 'Ngày tròn mốc kế tiếp',
      dataIndex: 'nextMilestoneDate',
      width: 180,
      render: (value: string | null) => formatDate(value),
    },
    {
      key: 'actions',
      title: 'Thao tác',
      // Cột chỉ còn nút Sửa — mọi thao tác xóa đi qua nút thùng rác ở đầu trang.
      width: 88,
      align: 'right',
      render: (_value, record) => (
        <span className="hhd-members__row-actions">
          <Tooltip title="Sửa">
            <Button
              icon={<EditOutlined />}
              aria-label={`Sửa ${record.fullName}`}
              onClick={() => openEdit(record)}
            />
          </Tooltip>
        </span>
      ),
    },
  ];

  const headingDescription = loading
    ? 'Đang tải danh sách…'
    : // Chưa tải được thì không nói "0 người" — dễ khiến người xem tưởng mất dữ liệu.
      error
      ? null
      : isPristineEmpty
        ? 'Chưa có ai trong danh sách'
        : `${formatNumber(totalCount)} người · Tuổi đảng tính đến hôm nay`;

  return (
    <div className="hhd-members">
      <PageHeading
        title="Đảng viên"
        description={headingDescription}
        extra={
          <>
            {/* Luôn hiện để bác thấy trước là có chức năng xóa; chưa đánh dấu ai
                thì nút mờ đi và bấm không được. */}
            <Tooltip
              title={
                selectedRows.length > 0
                  ? `Xóa ${formatNumber(selectedRows.length)} người đã chọn`
                  : 'Xóa người đã chọn — bác đánh dấu vào ô vuông đầu dòng trước'
              }
            >
              <Button
                className="hhd-members__delete"
                danger
                type="primary"
                icon={<DeleteOutlined />}
                aria-label="Xóa người đã chọn"
                disabled={selectedRows.length === 0}
                onClick={() => confirmDelete(selectedRows)}
              />
            </Tooltip>
            <Button onClick={openCreate}>+ Thêm</Button>
            <Button type="primary" onClick={() => navigate(paths.membersImport)}>
              Import Excel
            </Button>
          </>
        }
      />

      <div className="hhd-members__panel">
        <div
          className={`hhd-members__toolbar${isPristineEmpty ? ' hhd-members__toolbar--muted' : ''}`}
        >
          <Input
            className="hhd-members__search"
            size="large"
            allowClear
            prefix={<SearchOutlined />}
            placeholder="Tìm theo họ tên…"
            aria-label="Tìm theo họ tên"
            value={keywordInput}
            disabled={isPristineEmpty}
            onChange={(event) => setKeywordInput(event.target.value)}
          />
          <Select<GenderFilter>
            className="hhd-members__gender"
            size="large"
            value={query.gender}
            disabled={isPristineEmpty}
            aria-label="Lọc theo giới tính"
            onChange={(value) => changeQuery({ gender: value })}
            labelRender={({ label }) => <>Giới tính: {label}</>}
            options={[
              { label: 'Tất cả', value: 'All' },
              { label: 'Nam', value: 'Male' },
              { label: 'Nữ', value: 'Female' },
            ]}
          />
          {selectedRows.length > 0 ? (
            <span className="hhd-members__selected">
              Đang chọn <b>{formatNumber(selectedRows.length)}</b> dòng
            </span>
          ) : null}
        </div>

        <div className="hhd-members__body">
          <TableStates
            loading={loading}
            error={error}
            onRetry={reload}
            isEmpty={isEmpty}
            skeletonRows={8}
            description={
              // Cỡ chữ và kiểu chữ lấy đúng khối giữa của artboard "3 Đảng viên trống".
              <span style={{ font: "700 22px/30px 'Noto Serif', Georgia, serif" }}>
                {isFiltered ? 'Không tìm thấy ai như vậy' : 'Chưa có đảng viên nào'}
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
                {isFiltered
                  ? 'Bác thử xóa bớt chữ trong ô tìm, hoặc chọn lại Giới tính: Tất cả.'
                  : 'Cách nhanh nhất là tải file mẫu 4 cột (Họ tên · Ngày sinh · Giới tính · Ngày vào Đảng chính thức), điền rồi import. Hoặc thêm từng người bằng tay.'}
              </span>
            }
            action={
              isFiltered ? (
                <Button
                  onClick={() => {
                    setKeywordInput('');
                    changeQuery({ keyword: '', gender: 'All' });
                  }}
                >
                  Xem lại tất cả
                </Button>
              ) : (
                <>
                  <Button loading={downloadingTemplate} onClick={handleDownloadTemplate}>
                    Tải file mẫu
                  </Button>
                  <Button style={{ marginLeft: 8 }} onClick={openCreate}>
                    + Thêm một người
                  </Button>
                </>
              )
            }
          >
            {/* Dòng đang tick dùng nền vàng nhạt như artboard 3, không phải nền
                đỏ nhạt mặc định của Ant Design. */}
            <ConfigProvider
              theme={{
                components: {
                  Table: { rowSelectedBg: color.goldSoft, rowSelectedHoverBg: color.goldSoft },
                },
              }}
            >
              <Table<PartyMemberResponse>
                rowKey="id"
                columns={columns}
                dataSource={rows}
                sortDirections={['ascend', 'descend', 'ascend']}
                rowSelection={{
                  selectedRowKeys: selectedIds,
                  onChange: (keys) => setSelectedIds(keys as string[]),
                }}
                onChange={(pagination, _filters, sorter) => handleTableChange(pagination, sorter)}
                pagination={{
                  current: pageInfo?.current ?? query.current,
                  pageSize: pageInfo?.pageSize ?? query.pageSize,
                  total: totalCount,
                  showSizeChanger: true,
                  pageSizeOptions: MEMBERS_PAGE_SIZES,
                  showTotal: (total, range) =>
                    `${formatNumber(range[0])}–${formatNumber(range[1])} / ${formatNumber(total)}`,
                }}
              />
            </ConfigProvider>
          </TableStates>
        </div>
      </div>

      {formOpen ? (
        <MemberFormModal
          member={editing}
          today={session?.serverDate ?? ''}
          onCancel={() => setFormOpen(false)}
          onSaved={handleSaved}
        />
      ) : null}
    </div>
  );
}
