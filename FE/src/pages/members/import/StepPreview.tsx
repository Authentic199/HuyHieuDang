import { Alert, Button, Table, Tabs } from 'antd';
import type { ColumnsType } from 'antd/es/table';
import { useState } from 'react';

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
  /** Về bước 1, giữ nguyên file đã chọn */
  onBack: () => void;
  /** Bỏ hẳn việc import và rời màn Import */
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

  const pagination = (total: number) =>
    total > PREVIEW_PAGE_SIZE
      ? {
          pageSize: PREVIEW_PAGE_SIZE,
          showSizeChanger: false,
          showTotal: (count: number, range: [number, number]) =>
            `${formatNumber(range[0])}–${formatNumber(range[1])} / ${formatNumber(count)}`,
        }
      : (false as const);

  return (
    <>
      {error ? (
        <Alert type="error" showIcon message="Chưa nạp được danh sách" description={error} />
      ) : null}

      <div className="hhd-import__summary">
        <div className="hhd-import__summary-number">{formatNumber(preview.validCount)}</div>
        <div>
          <b>
            Sẽ thêm {formatNumber(preview.validCount)} người mới ·{' '}
            {formatNumber(preview.errorCount)} dòng lỗi bị bỏ qua
          </b>
          <div className="hhd-import__summary-note">
            Hệ thống không kiểm tra trùng — nếu đã nạp file này trước đó, hãy Hủy.
          </div>
        </div>
      </div>

      <div className="hhd-import__panel">
        <Tabs
          className="hhd-import__tabs"
          activeKey={tab}
          onChange={setTab}
          items={[
            {
              key: 'valid',
              label: tabLabel('Hợp lệ', preview.validCount, 'valid'),
              children: (
                <TableStates
                  loading={false}
                  error={null}
                  isEmpty={preview.validRows.length === 0}
                  description="Không có dòng nào hợp lệ"
                  hint="Cả file đều vướng lỗi. Bác sửa trong file gốc rồi nạp lại giúp."
                >
                  <Table<ImportValidRow>
                    rowKey="rowNumber"
                    columns={VALID_COLUMNS}
                    dataSource={preview.validRows}
                    pagination={pagination(preview.validRows.length)}
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
                  isEmpty={preview.errorRows.length === 0}
                  description="Không có dòng lỗi nào"
                  hint="Cả file đều đọc được. Bác bấm nạp là xong."
                >
                  <Table<ImportErrorRow>
                    rowKey="rowNumber"
                    columns={errorColumns()}
                    dataSource={preview.errorRows}
                    pagination={pagination(preview.errorRows.length)}
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
          {/* "Hủy" bỏ hẳn việc import và về thẳng danh sách đảng viên; "Quay
              lại" giữ file để bác xem lại phần mô tả 4 cột rồi tiếp tục. */}
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
