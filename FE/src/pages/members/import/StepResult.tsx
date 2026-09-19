import { CheckOutlined } from '@ant-design/icons';
import { Button } from 'antd';

import type { ImportResult } from '../../../api/imports';
import { formatNumber } from '../../../utils/format';

interface StepResultProps {
  result: ImportResult;
  onRestart: () => void;
  onBackToList: () => void;
}

/** Bước 3 — Kết quả (UC-24). */
export function StepResult({ result, onRestart, onBackToList }: StepResultProps) {
  return (
    <div className="hhd-import__result">
      <div className="hhd-import__result-inner">
        <div className="hhd-import__result-check">
          <CheckOutlined />
        </div>
        <div className="hhd-import__result-title">
          Đã thêm {formatNumber(result.importedCount)} người
        </div>
        <div className="hhd-import__result-note">
          Bỏ qua {formatNumber(result.skippedCount)} dòng lỗi. Danh sách đủ điều kiện ở Dashboard đã
          được tính lại.
        </div>
        <div className="hhd-import__result-actions">
          <Button onClick={onRestart}>Import file khác</Button>
          <Button type="primary" onClick={onBackToList}>
            Về danh sách đảng viên
          </Button>
        </div>
      </div>
    </div>
  );
}
