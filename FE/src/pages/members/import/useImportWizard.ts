import { useCallback, useState } from 'react';

import { importsApi } from '../../../api';
import type { ImportPreview, ImportResult } from '../../../api/imports';
import { FALLBACK_MESSAGE, messageText } from '../../../api/messages';
import { ApiError } from '../../../types/api';

/**
 * Toàn bộ việc gọi API của màn Nạp danh sách nằm ở đây, ba bước giao diện chỉ
 * dựng hình (quy tắc 6 trong CLAUDE.md).
 */

/** Ba bước của UC-24, cũng là chỉ số của thanh Steps. */
export const IMPORT_STEPS = ['choose', 'preview', 'result'] as const;
export type ImportStep = (typeof IMPORT_STEPS)[number];

/** Chỉ nhận đúng một đuôi file; chữ báo lỗi lấy từ bảng khóa thông điệp. */
const ACCEPTED_EXTENSION = '.xlsx';

/** Giới hạn 10 MB theo mục 4.2 của tài liệu nghiệp vụ. */
export const MAX_FILE_BYTES = 10 * 1024 * 1024;

export interface UseImportWizardResult {
  step: ImportStep;
  /** Chỉ số bước cho thanh Steps */
  stepIndex: number;
  /**
   * File người dùng đã chọn. Máy chủ không giữ trạng thái giữa bước 2 và bước
   * 3 nên phải giữ nguyên đối tượng File này để gửi lại khi nạp (mục 4.2).
   */
  file: File | null;
  preview: ImportPreview | null;
  result: ImportResult | null;
  /** Đang hỏi máy chủ xem trước (bước 1 → bước 2) */
  previewing: boolean;
  /** Đang nạp các dòng hợp lệ (bước 2 → bước 3) */
  committing: boolean;
  /** Lỗi cấp file — câu tiếng Việt đã dịch, null khi không lỗi */
  fileError: string | null;
  /** Nhận file từ vùng kéo-thả; trả false khi file không hợp lệ */
  chooseFile: (file: File) => boolean;
  /** Bỏ file đang chọn và mọi lỗi kèm theo */
  clearFile: () => void;
  /** Bước 1 → bước 2 */
  goPreview: () => Promise<void>;
  /** Bước 2 → bước 3 */
  goCommit: () => Promise<void>;
  /** Bước 2 → bước 1, giữ nguyên file để chọn lại cho nhanh */
  backToChoose: () => void;
  /** "Hủy" ở bước 2: về bước 1 và bỏ file */
  cancelToChoose: () => void;
  /** "Import file khác" ở bước 3: làm lại từ đầu */
  restart: () => void;
}

/** Dịch lỗi của tầng API thành câu tiếng Việt, không để lọt khóa kỹ thuật. */
function errorText(reason: unknown): string {
  return reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE;
}

export function useImportWizard(): UseImportWizardResult {
  const [step, setStep] = useState<ImportStep>('choose');
  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<ImportPreview | null>(null);
  const [result, setResult] = useState<ImportResult | null>(null);
  const [previewing, setPreviewing] = useState(false);
  const [committing, setCommitting] = useState(false);
  const [fileError, setFileError] = useState<string | null>(null);

  /**
   * Chặn ngay tại trình duyệt hai lỗi thấy được mà không cần hỏi máy chủ.
   * Câu chữ dùng đúng khóa `Mes.Import.Invalid.*` của hợp đồng API.
   */
  const chooseFile = useCallback((next: File): boolean => {
    if (!next.name.toLowerCase().endsWith(ACCEPTED_EXTENSION)) {
      setFile(null);
      setFileError(messageText('Mes.Import.Invalid.Extension'));
      return false;
    }
    if (next.size > MAX_FILE_BYTES) {
      setFile(null);
      setFileError(messageText('Mes.Import.Invalid.FileSize'));
      return false;
    }
    setFile(next);
    setFileError(null);
    setPreview(null);
    return true;
  }, []);

  const clearFile = useCallback(() => {
    setFile(null);
    setFileError(null);
    setPreview(null);
  }, []);

  const goPreview = useCallback(async () => {
    if (!file) return;
    setPreviewing(true);
    setFileError(null);
    try {
      setPreview(await importsApi.previewImport(file));
      setStep('preview');
    } catch (reason) {
      // Lỗi cấp file của máy chủ (thiếu cột, file rỗng…) hiện ngay ở bước 1.
      setFileError(errorText(reason));
    } finally {
      setPreviewing(false);
    }
  }, [file]);

  const goCommit = useCallback(async () => {
    if (!file) return;
    setCommitting(true);
    try {
      setResult(await importsApi.commitImport(file));
      setStep('result');
    } catch (reason) {
      setFileError(errorText(reason));
    } finally {
      setCommitting(false);
    }
  }, [file]);

  const backToChoose = useCallback(() => {
    setStep('choose');
  }, []);

  const cancelToChoose = useCallback(() => {
    setStep('choose');
    clearFile();
  }, [clearFile]);

  const restart = useCallback(() => {
    setStep('choose');
    setResult(null);
    clearFile();
  }, [clearFile]);

  return {
    step,
    stepIndex: IMPORT_STEPS.indexOf(step),
    file,
    preview,
    result,
    previewing,
    committing,
    fileError,
    chooseFile,
    clearFile,
    goPreview,
    goCommit,
    backToChoose,
    cancelToChoose,
    restart,
  };
}
