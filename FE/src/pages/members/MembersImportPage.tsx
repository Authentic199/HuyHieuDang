import { useNavigate } from 'react-router-dom';

import { PageHeading } from '../../components/PageHeading';
import { paths } from '../../routes/paths';
import { ImportSteps } from './import/ImportSteps';
import { StepChooseFile } from './import/StepChooseFile';
import { StepPreview } from './import/StepPreview';
import { StepResult } from './import/StepResult';
import { useImportWizard } from './import/useImportWizard';
import './import/ImportWizard.css';

/**
 * Màn 4 — Nạp danh sách đảng viên từ file Excel (UC-24, UC-25).
 *
 * Ba bước nối nhau: chọn file → xem trước → kết quả. Máy chủ không giữ trạng
 * thái giữa bước 2 và bước 3 nên đối tượng File chọn ở bước 1 được giữ lại
 * trong `useImportWizard` để gửi lại khi nạp.
 */
export default function MembersImportPage() {
  const navigate = useNavigate();
  const wizard = useImportWizard();

  const backToList = () => navigate(paths.members);

  /**
   * "Hủy" ở bước 2 là bỏ hẳn việc import: xóa file và kết quả xem trước rồi
   * về thẳng danh sách đảng viên, để lần vào sau wizard bắt đầu từ bước 1
   * trắng. Đường lùi về bước 1 mà vẫn giữ file là nút "‹ Quay lại".
   */
  const cancelImport = () => {
    wizard.restart();
    backToList();
  };

  return (
    <div className="hhd-import">
      <div>
        <div className="hhd-import__crumbs">
          Đảng viên / <b>Import Excel</b>
        </div>
        <PageHeading title="Import danh sách đảng viên" />
      </div>

      <ImportSteps
        current={wizard.stepIndex}
        fileName={wizard.file?.name ?? null}
        preview={wizard.preview}
        result={wizard.result}
      />

      {wizard.step === 'choose' ? (
        <StepChooseFile
          file={wizard.file}
          fileError={wizard.fileError}
          previewing={wizard.previewing}
          onChoose={wizard.chooseFile}
          onContinue={wizard.goPreview}
          onCancel={backToList}
        />
      ) : null}

      {wizard.step === 'preview' && wizard.preview ? (
        <StepPreview
          preview={wizard.preview}
          committing={wizard.committing}
          error={wizard.fileError}
          onCommit={wizard.goCommit}
          onBack={wizard.backToChoose}
          onCancel={cancelImport}
        />
      ) : null}

      {wizard.step === 'result' && wizard.result ? (
        <StepResult result={wizard.result} onRestart={wizard.restart} onBackToList={backToList} />
      ) : null}
    </div>
  );
}
