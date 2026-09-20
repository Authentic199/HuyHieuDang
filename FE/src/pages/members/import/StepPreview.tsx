import {
  CheckCircleFilled,
  CloseCircleFilled,
  ExclamationCircleOutlined,
  SearchOutlined,
} from '@ant-design/icons';
import { Alert, Button, Input, Table, Tabs } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useMemo, useState } from 'react';

import {
  importErrorFields,
  importErrorText,
  type ImportErrorField,
  type ImportErrorRow,
  type ImportPreview,
  type ImportValidRow,
} from '../../../api/imports';
import { TableStates } from '../../../components/TableStates';
import { formatDate, formatGender, formatNumber, orEmptyMark } from '../../../utils/format';

/** Số dòng của một trang bảng xem trước — file thật có thể vài trăm dòng. */
const PREVIEW_PAGE_SIZE = 20;

/** Cột số dòng trong file Excel, dùng chung cho cả hai bảng. */
function rowNumberColumn<T extends { rowNumber: number }>(): ColumnsType<T>[number] {
  return {
    key: 'rowNumber',
    title: 'Dòng',
    dataIndex: 'rowNumber',
    width: 80,
    onCell: () => ({ className: 'hhd-import__mono-cell' }),
  };
}

/** Bảng dòng hợp lệ — giá trị đã được máy chủ chuẩn hóa nên hiện kiểu Việt. */
const VALID_COLUMNS: ColumnsType<ImportValidRow> = [
  rowNumberColumn<ImportValidRow>(),
  {
    key: 'fullName',
    title: 'Họ tên',
    dataIndex: 'fullName',
    width: 260,
    onCell: () => ({ style: { whiteSpace: 'nowrap', fontWeight: 600 } }),
  },
  {
    key: 'dateOfBirth',
    title: 'Ngày sinh',
    dataIndex: 'dateOfBirth',
    width: 140,
    render: (value: string | null) => formatDate(value),
  },
  {
    key: 'gender',
    title: 'Giới tính',
    dataIndex: 'gender',
    width: 120,
    render: (value: ImportValidRow['gender']) => formatGender(value),
  },
  {
    key: 'officialAdmissionDate',
    title: 'Ngày chính thức',
    dataIndex: 'officialAdmissionDate',
    width: 200,
    render: (value: string) => formatDate(value),
  },
];

/**
 * Bảng dòng lỗi — giữ nguyên chữ thô đọc từ ô Excel vì chính chúng đang sai,
 * chuẩn hóa lại là giấu mất chỗ bác cần sửa. Ô gây lỗi tô đỏ, cột "Lý do" ghép
 * mọi lý do của dòng đó.
 */
function errorColumns(): ColumnsType<ImportErrorRow> {
  const rawCell = (field: ImportErrorField) => (row: ImportErrorRow) => ({
    className: importErrorFields(row).includes(field) ? 'hhd-import__cell-bad' : undefined,
  });

  return [
    rowNumberColumn<ImportErrorRow>(),
    {
      key: 'fullName',
      title: 'Họ tên',
      dataIndex: 'fullName',
      width: 220,
      onCell: rawCell('FullName'),
      render: (value: string) => orEmptyMark(value),
    },
    {
      key: 'dateOfBirth',
      title: 'Ngày sinh',
      dataIndex: 'dateOfBirth',
      width: 140,
      onCell: rawCell('DateOfBirth'),
      render: (value: string) => orEmptyMark(value),
    },
    {
      key: 'gender',
      title: 'Giới tính',
      dataIndex: 'gender',
      width: 110,
      onCell: rawCell('Gender'),
      render: (value: string) => orEmptyMark(value),
    },
    {
      key: 'officialAdmissionDate',
      title: 'Ngày chính thức',
      dataIndex: 'officialAdmissionDate',
      width: 180,
      onCell: rawCell('OfficialAdmissionDate'),
      render: (value: string) => orEmptyMark(value),
    },
    {
      key: 'reason',
      title: 'Lý do',
      render: (_value, row) => (
        <span className="hhd-import__reason">
          <i />
          <span>{importErrorText(row)}</span>
        </span>
      ),
    },
  ];
}

/**
 * Chuẩn hóa chữ trước khi so khớp: bỏ phân biệt hoa thường, GIỮ dấu — đúng như
 * ô tìm màn Đảng viên đang chạy trên citext của Postgres, để cùng một từ khóa
 * cho ra cùng kết quả ở hai màn. Tìm bỏ dấu là chuyện của v2.
 */
function normalize(text: string): string {
  return text.toLocaleLowerCase('vi');
}

/** Từ khóa toàn chữ số thì tìm thêm theo số dòng trong file Excel. */
function matchesRowNumber(rowNumber: number, keyword: string): boolean {
  return /^\d+$/.test(keyword) && String(rowNumber).includes(keyword);
}

/** Nhãn tab kèm viên đếm, đúng hình của artboard 4. */
function tabLabel(text: string, count: number, tone: 'valid' | 'invalid') {
  return (
    <span className="hhd-import__tab">
      {text}
      <span className={`hhd-import__count hhd-import__count--${tone}`}>{formatNumber(count)}</span>
    </span>
  );
}

interface StepPreviewProps {
  preview: ImportPreview;
  committing: boolean;
  /** Lỗi khi nạp — câu tiếng Việt đã dịch, null khi không lỗi */
  error: string | null;
  onCommit: () => void;
  onBack: () => void;
  onCancel: () => void;
}

/** Bước 2 — Xem trước (UC-24). */
export function StepPreview({
  preview,
  committing,
  error,
  onCommit,
  onBack,
  onCancel,
}: StepPreviewProps) {
  const [tab, setTab] = useState(preview.errorCount > 0 ? 'error' : 'valid');
  // Một từ khóa dùng chung cho cả hai tab, đổi tab thì giữ nguyên từ khóa.
  const [keyword, setKeyword] = useState('');
  // Giữ trang hiện tại để đổi từ khóa là kéo bảng về trang 1 — lọc còn 3 dòng
  // mà bảng vẫn đứng ở trang 12 thì nhìn như mất dữ liệu.
  const [validPage, setValidPage] = useState(1);
  const [errorPage, setErrorPage] = useState(1);

  const needle = normalize(keyword.trim());
  const filtering = needle.length > 0;

  // Lọc ngay tại máy khách trên mảng đã có, không gọi lại máy chủ.
  const validRows = useMemo(
    () =>
      filtering
        ? preview.validRows.filter(
            (row) =>
              normalize(row.fullName).includes(needle) || matchesRowNumber(row.rowNumber, needle),
          )
        : preview.validRows,
    [preview.validRows, needle, filtering],
  );

  // Tab Lỗi tìm thêm trong cột "Lý do" để bác lọc ra mọi dòng "Thiếu họ tên".
  const errorRows = useMemo(
    () =>
      filtering
        ? preview.errorRows.filter(
            (row) =>
              normalize(row.fullName).includes(needle) ||
              normalize(importErrorText(row)).includes(needle) ||
              matchesRowNumber(row.rowNumber, needle),
          )
        : preview.errorRows,
    [preview.errorRows, needle, filtering],
  );

  const changeKeyword = (value: string) => {
    setKeyword(value);
    setValidPage(1);
    setErrorPage(1);
  };

  const pagination = (total: number, current: number, onChange: (page: number) => void) =>
    total > PREVIEW_PAGE_SIZE
      ? {
          current,
          onChange,
          pageSize: PREVIEW_PAGE_SIZE,
          showSizeChanger: false,
          showTotal: (count: number, range: [number, number]) =>
            `${formatNumber(range[0])}–${formatNumber(range[1])} / ${formatNumber(count)}`,
        }
      : (false as const);

  /** Câu chữ bảng trống: lọc không ra gì khác hẳn tab vốn không có dòng nào. */
  const emptyState = (description: string, hint: string) =>
    filtering
      ? {
          description: 'Không tìm thấy dòng nào khớp',
          hint: 'Bác thử bớt chữ, hoặc xóa ô tìm để xem lại cả danh sách.',
        }
      : { description, hint };

  const hasValid = preview.validCount > 0;
  const hasError = preview.errorCount > 0;

  return (
    <>
      {error ? (
        <Alert type="error" showIcon message="Chưa nạp được danh sách" description={error} />
      ) : null}

      {/* role="status" để trình đọc màn hình đọc kết quả ngay khi dải hiện ra.
          Cả file đều lỗi thì dải phải đỏ: lúc đó không có gì để nạp. */}
      <div
        className={`hhd-import__summary hhd-import__summary--${hasValid ? 'ok' : 'bad'}`}
        role="status"
      >
        <div className="hhd-import__summary-stats">
          <span
            className={`hhd-import__stat hhd-import__stat--ok${
              hasValid ? '' : ' hhd-import__stat--muted'
            }`}
          >
            <CheckCircleFilled /> <b>{formatNumber(preview.validCount)}</b> dòng hợp lệ
          </span>
          {/* Không lỗi nào thì dấu ✕ để xám — bôi đỏ số 0 là báo động giả. */}
          <span
            className={`hhd-import__stat hhd-import__stat--err${
              hasError ? '' : ' hhd-import__stat--off'
            }`}
          >
            <CloseCircleFilled /> <b>{formatNumber(preview.errorCount)}</b> dòng bị lỗi, bỏ qua
          </span>
        </div>
        <div className="hhd-import__summary-note">
          <ExclamationCircleOutlined /> Hệ thống không kiểm tra trùng — nếu đã nạp file này trước
          đó, hãy Hủy.
        </div>
      </div>

      <div className="hhd-import__panel">
        <Tabs
          className="hhd-import__tabs"
          activeKey={tab}
          onChange={setTab}
          tabBarExtraContent={{
            right: (
              <Input
                className="hhd-import__search"
                allowClear
                prefix={<SearchOutlined />}
                placeholder="Tìm theo họ tên…"
                aria-label="Tìm trong danh sách xem trước"
                value={keyword}
                onChange={(event) => changeKeyword(event.target.value)}
              />
            ),
          }}
          items={[
            {
              key: 'valid',
              // Viên đếm là con số của cả file, không đổi theo bộ lọc.
              label: tabLabel('Hợp lệ', preview.validCount, 'valid'),
              children: (
                <TableStates
                  loading={false}
                  error={null}
                  isEmpty={validRows.length === 0}
                  {...emptyState(
                    'Không có dòng nào hợp lệ',
                    'Cả file đều vướng lỗi. Bác sửa trong file gốc rồi nạp lại giúp.',
                  )}
                >
                  <Table<ImportValidRow>
                    rowKey="rowNumber"
                    columns={VALID_COLUMNS}
                    dataSource={validRows}
                    pagination={pagination(validRows.length, validPage, setValidPage)}
                  />
                </TableStates>
              ),
            },
            {
              key: 'error',
              label: tabLabel('Lỗi', preview.errorCount, 'invalid'),
              children: (
                <TableStates
                  loading={false}
                  error={null}
                  isEmpty={errorRows.length === 0}
                  {...emptyState(
                    'Không có dòng lỗi nào',
                    'Cả file đều đọc được. Bác bấm nạp là xong.',
                  )}
                >
                  <Table<ImportErrorRow>
                    rowKey="rowNumber"
                    columns={errorColumns()}
                    dataSource={errorRows}
                    pagination={pagination(errorRows.length, errorPage, setErrorPage)}
                  />
                </TableStates>
              ),
            },
          ]}
        />

        <div className="hhd-import__footer hhd-import__footer--panel">
          <span className="hhd-import__footer-note">
            Các dòng lỗi sẽ không được nạp. Sửa trong file gốc rồi import lại nếu cần.
          </span>
          {/* "Hủy" bỏ luôn file đang xem và quay về bước 1; "Quay lại" giữ file
              để bác xem lại phần mô tả 4 cột rồi tiếp tục. */}
          <Button type="text" onClick={onCancel}>
            Hủy
          </Button>
          <Button onClick={onBack}>‹ Quay lại</Button>
          <Button
            type="primary"
            disabled={preview.validCount === 0}
            loading={committing}
            onClick={onCommit}
          >
            Nạp {formatNumber(preview.validCount)} dòng hợp lệ
          </Button>
        </div>
      </div>
    </>
  );
}
