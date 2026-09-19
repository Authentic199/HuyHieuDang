import { DownloadOutlined, InboxOutlined } from '@ant-design/icons';
import { Alert, App as AntApp, Button, Upload } from 'antd';
import { useState } from 'react';

import { importsApi } from '../../../api';
import { FALLBACK_MESSAGE } from '../../../api/messages';
import { ApiError } from '../../../types/api';
import { saveFile } from '../../../utils/download';
import { MAX_FILE_BYTES } from './useImportWizard';

/** Bốn cột bắt buộc của file, đúng thứ tự — kèm một dòng ví dụ như artboard. */
const TEMPLATE_COLUMNS = [
  { header: 'Họ tên', required: true, sample: 'Nguyễn Văn An', mono: false },
  { header: 'Ngày sinh', required: false, sample: '12/03/1958', mono: true },
  { header: 'Giới tính', required: false, sample: 'Nam', mono: false },
  { header: 'Ngày vào Đảng chính thức', required: true, sample: '15/10/1996', mono: true },
] as const;

/** Dung lượng file cho người đọc: 2,4 MB. */
function fileSizeText(bytes: number): string {
  const megabytes = bytes / (1024 * 1024);
  if (megabytes >= 1) return `${megabytes.toFixed(1).replace('.', ',')} MB`;
  return `${Math.max(1, Math.round(bytes / 1024))} KB`;
}

interface StepChooseFileProps {
  file: File | null;
  fileError: string | null;
  previewing: boolean;
  onChoose: (file: File) => boolean;
  onContinue: () => void;
  onCancel: () => void;
}

/** Bước 1 — Chọn file (UC-24, UC-25). */
export function StepChooseFile({
  file,
  fileError,
  previewing,
  onChoose,
  onContinue,
  onCancel,
}: StepChooseFileProps) {
  const { message } = AntApp.useApp();
  const [downloadingTemplate, setDownloadingTemplate] = useState(false);

  /** UC-25 — tải file mẫu 4 cột, cùng cách làm với nút bên màn Đảng viên. */
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

  return (
    <div className="hhd-import__card">
      {fileError ? (
        <Alert
          type="error"
          showIcon
          message="Chưa đọc được file này"
          description={fileError}
          className="hhd-import__file-error"
        />
      ) : null}

      <Upload.Dragger
        className="hhd-import__dropzone"
        name="file"
        accept=".xlsx"
        multiple={false}
        showUploadList={false}
        // Không để Ant Design tự gửi file đi — file chỉ được gửi khi bấm
        // "Tiếp tục", và gửi qua tầng src/api.
        beforeUpload={(candidate) => {
          onChoose(candidate as unknown as File);
          return Upload.LIST_IGNORE;
        }}
      >
        <div className="hhd-import__dropzone-icon">
          <InboxOutlined />
        </div>
        {file ? (
          <>
            <div className="hhd-import__dropzone-title">{file.name}</div>
            <div className="hhd-import__dropzone-hint">
              {fileSizeText(file.size)} · Bấm vào đây hoặc kéo file khác vào để chọn lại
            </div>
          </>
        ) : (
          <>
            <div className="hhd-import__dropzone-title">Kéo thả file Excel vào đây</div>
            <div className="hhd-import__dropzone-hint">
              hoặc <span className="hhd-import__dropzone-link">chọn file từ máy</span> · .xlsx, tối
              đa {MAX_FILE_BYTES / (1024 * 1024)} MB
            </div>
          </>
        )}
      </Upload.Dragger>

      <div className="hhd-import__template">
        <div>
          <div className="hhd-import__overline">File cần có đúng 4 cột, theo thứ tự</div>
          <div className="hhd-import__columns">
            {TEMPLATE_COLUMNS.map((column) => (
              <div key={column.header} className="hhd-import__columns-head">
                {column.header}
                {column.required ? <span className="hhd-import__required"> *</span> : null}
              </div>
            ))}
            {TEMPLATE_COLUMNS.map((column) => (
              <div
                key={`${column.header}-sample`}
                className={`hhd-import__columns-cell${column.mono ? ' hhd-import__mono' : ''}`}
              >
                {column.sample}
              </div>
            ))}
          </div>
          <div className="hhd-import__template-note">
            Ngày theo định dạng <span className="hhd-import__mono">dd/MM/yyyy</span>. Giới tính: Nam
            / Nữ hoặc để trống.
          </div>
        </div>
        <Button
          icon={<DownloadOutlined />}
          loading={downloadingTemplate}
          onClick={handleDownloadTemplate}
        >
          Tải file mẫu
        </Button>
      </div>

      <div className="hhd-import__footer">
        {/* Bước 1 là cửa vào màn này nên "Hủy" đưa bác trở ra danh sách đảng
            viên; chọn nhầm file thì chỉ cần thả file khác vào ô bên trên. */}
        <Button type="text" onClick={onCancel}>
          Hủy
        </Button>
        <Button type="primary" disabled={!file} loading={previewing} onClick={onContinue}>
          Tiếp tục ›
        </Button>
      </div>
    </div>
  );
}
