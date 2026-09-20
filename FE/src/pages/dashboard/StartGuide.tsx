import { Button } from 'antd';
import { Fragment } from 'react';
import { useNavigate } from 'react-router-dom';

import { paths } from '../../routes/paths';

/**
 * Khối hướng dẫn 3 bước của trạng thái trống (UC-13) — artboard "Màn 2 —
 * Dashboard trạng thái trống (lần dùng đầu)".
 *
 * Chỉ dựng khi máy chủ báo chưa có đảng viên hoặc chưa có đợt
 * (`warnings.noMembers`, `warnings.noPeriods`). Bước còn thiếu được tô đậm để
 * cán bộ biết bấm vào đâu trước; các bước khác vẫn bấm được.
 */

interface StartGuideProps {
  /** Bước đang cần làm: 2 = tạo đợt, 3 = nạp danh sách đảng viên */
  currentStep: 2 | 3;
}

interface Step {
  order: 1 | 2 | 3;
  title: string;
  detail: string;
  actionLabel: string;
  to: string;
}

const STEPS: Step[] = [
  {
    order: 1,
    title: 'Kiểm tra cài đặt mốc',
    detail: 'Mặc định 30 / 90 / 5 → 30, 35 … 90 năm. Thường giữ nguyên.',
    actionLabel: 'Mở Cài đặt',
    to: paths.settings,
  },
  {
    order: 2,
    title: 'Tạo các đợt trong năm',
    detail: 'Ví dụ 4 đợt quanh 3/2, 19/5, 2/9, 7/11. Chỉ lưu ngày/tháng.',
    actionLabel: 'Thêm đợt',
    to: paths.periods,
  },
  {
    order: 3,
    title: 'Nạp danh sách đảng viên',
    detail: 'Tải file mẫu 4 cột, điền rồi import. Dòng lỗi được báo trước khi nạp.',
    actionLabel: 'Import Excel',
    to: paths.membersImport,
  },
];

export function StartGuide({ currentStep }: StartGuideProps) {
  const navigate = useNavigate();

  return (
    <div className="hhd-dashboard__guide">
      <div className="hhd-dashboard__guide-inner">
        <div className="hhd-dashboard__guide-title">Bắt đầu với ba bước</div>

        <div className="hhd-dashboard__steps">
          {STEPS.map((step, index) => {
            const active = step.order === currentStep;
            return (
              <Fragment key={step.order}>
                {index > 0 ? <div className="hhd-dashboard__step-arrow">→</div> : null}
                <div
                  className={`hhd-dashboard__step${active ? ' hhd-dashboard__step--active' : ''}`}
                >
                  <span className="hhd-dashboard__step-order">{step.order}</span>
                  <div className="hhd-dashboard__step-title">{step.title}</div>
                  <div className="hhd-dashboard__step-detail">{step.detail}</div>
                  <Button
                    type={active ? 'primary' : 'default'}
                    onClick={() => navigate(step.to)}
                    style={{ alignSelf: 'flex-start' }}
                  >
                    {step.actionLabel}
                  </Button>
                </div>
              </Fragment>
            );
          })}
        </div>
      </div>
    </div>
  );
}
