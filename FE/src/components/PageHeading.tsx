import type { ReactNode } from 'react';

import { font, neutral, typography } from '../theme/tokens';

interface PageHeadingProps {
  title: string;
  /** Dòng tóm tắt dưới tiêu đề, ví dụ "125 người · Tuổi đảng tính đến hôm nay" */
  description?: ReactNode;
  /** Nút hành động bên phải */
  extra?: ReactNode;
}

/** Tiêu đề màn hình, chữ có chân 32px theo artboard. */
export function PageHeading({ title, description, extra }: PageHeadingProps) {
  return (
    <div
      style={{ display: 'flex', alignItems: 'flex-end', justifyContent: 'space-between', gap: 16 }}
    >
      <div>
        <h1
          style={{
            margin: 0,
            font: `${typography.headingXl.weight} ${typography.headingXl.size}px/${typography.headingXl.line}px ${font.serif}`,
            letterSpacing: '-0.005em',
          }}
        >
          {title}
        </h1>
        {description ? (
          <div style={{ color: neutral.textSecondary, marginTop: 2 }}>{description}</div>
        ) : null}
      </div>
      {extra ? <div style={{ display: 'flex', gap: 8 }}>{extra}</div> : null}
    </div>
  );
}
