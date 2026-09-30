import type { ReactNode } from 'react';

import './PageHeading.css';

interface PageHeadingProps {
  title: string;
  /** Dòng tóm tắt dưới tiêu đề, ví dụ "125 người · Tuổi đảng tính đến hôm nay" */
  description?: ReactNode;
  /** Nút hành động bên phải */
  extra?: ReactNode;
}

/** Tiêu đề màn hình, chữ có chân 32px theo artboard (24px ở thang gọn). */
export function PageHeading({ title, description, extra }: PageHeadingProps) {
  return (
    <div className="hhd-page-heading">
      <div>
        <h1 className="hhd-page-heading__title">{title}</h1>
        {description ? <div className="hhd-page-heading__description">{description}</div> : null}
      </div>
      {extra ? <div className="hhd-page-heading__extra">{extra}</div> : null}
    </div>
  );
}
