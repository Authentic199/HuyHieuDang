import { Steps } from 'antd';

import { formatNumber } from '../../../utils/format';
import type { ImportPreview, ImportResult } from '../../../api/imports';

interface ImportStepsProps {
  /** 0 Chọn file · 1 Xem trước · 2 Kết quả */
  current: number;
  fileName: string | null;
  preview: ImportPreview | null;
  result: ImportResult | null;
}

/**
 * Thanh ba bước của artboard 4 — ba viên pill rời (phương án 4b của thiết kế):
 * bước đã xong tô gradient đỏ, bước đang làm viền đỏ 2px, bước chưa tới nền
 * chìm. Hình dáng đó dựng bằng CSS trong `ImportWizard.css`, còn cấu trúc vẫn
 * là Steps của Ant Design (quy tắc 2 trong CLAUDE.md).
 *
 * Dòng chữ nhỏ dưới mỗi bước luôn nói tình hình thật: tên file đang chọn, số
 * dòng máy chủ vừa đọc được — không có chữ mẫu nào lấy từ artboard.
 */
export function ImportSteps({ current, fileName, preview, result }: ImportStepsProps) {
  const statusOf = (index: number) =>
    index < current ? 'finish' : index === current ? 'process' : 'wait';

  const previewNote = preview
    ? `${formatNumber(preview.validCount)} dòng hợp lệ · ${formatNumber(preview.errorCount)} dòng lỗi`
    : 'Kiểm tra dòng hợp lệ và dòng lỗi';

  return (
    <Steps
      className="hhd-import__steps"
      current={current}
      labelPlacement="horizontal"
      items={[
        {
          title: 'Chọn file',
          description: fileName ?? 'Chưa chọn file',
          status: statusOf(0),
        },
        {
          title: 'Xem trước',
          description: previewNote,
          status: statusOf(1),
        },
        {
          title: 'Kết quả',
          description: result ? 'Đã nạp xong' : 'Nạp dữ liệu',
          status: result ? 'finish' : statusOf(2),
        },
      ]}
    />
  );
}
