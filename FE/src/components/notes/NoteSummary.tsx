import { ExpandAltOutlined, FileTextOutlined } from '@ant-design/icons';
import { Button, Tooltip } from 'antd';
import { useMemo, useState } from 'react';

import type { EligibleMemberResponse } from '../../types/domain';
import { formatNumber } from '../../utils/format';
import { noteSummaryText, notedMembersOf } from '../../utils/note';
import { NotesDialog } from './NotesDialog';
import './notes.css';

/**
 * Khối gọn `Ghi chú · N người` ở hàng đầu thẻ danh sách, sát bên trái nút Xuất
 * Excel — dùng chung cho Dashboard (UC-11) và Chi tiết đợt (UC-34) để hai màn
 * không bao giờ lệch nhau.
 *
 * Khối tính trên TRỌN danh sách của đợt và năm đang xem: `members` phải là mảng
 * máy chủ trả, không phải `pageRows` đã qua ô tìm và ô lọc của bảng.
 *
 * Khối cao đúng bằng một nút (`--hhd-control-height`) nên hàng đầu thẻ không cao
 * thêm dòng nào — bảng vẫn thấy đủ số dòng như trước.
 */

interface NoteSummaryProps {
  /** Trọn danh sách đủ điều kiện của đợt và năm đang xem */
  members: EligibleMemberResponse[];
  /** Ví dụ `Đợt 7/11 năm 2026` — đi vào tiêu đề hộp đầy đủ */
  subject: string;
}

export function NoteSummary({ members, subject }: NoteSummaryProps) {
  const [open, setOpen] = useState(false);
  const noted = useMemo(() => notedMembersOf(members), [members]);
  const summary = useMemo(() => noteSummaryText(noted), [noted]);

  // Không ai có ghi chú thì không hiện khối — hàng đầu thẻ về đúng như cũ.
  if (noted.length === 0) return null;

  return (
    <>
      <div
        className="hhd-note-summary"
        role="button"
        tabIndex={0}
        data-testid="note-summary"
        onClick={() => setOpen(true)}
        onKeyDown={(event) => {
          if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault();
            setOpen(true);
          }
        }}
      >
        <FileTextOutlined className="hhd-note-summary__icon" />
        <span className="hhd-note-summary__text">
          <span className="hhd-note-summary__count">
            Ghi chú · {formatNumber(noted.length)} người
          </span>
          <span className="hhd-note-summary__line">{summary}</span>
        </span>
        <Tooltip title="Xem đầy đủ ghi chú">
          <Button
            size="small"
            className="hhd-note-summary__expand"
            icon={<ExpandAltOutlined />}
            aria-label="Xem đầy đủ ghi chú"
            onClick={(event) => {
              // Nút nằm trong khối cũng mở hộp — chặn để không mở hai lần.
              event.stopPropagation();
              setOpen(true);
            }}
          />
        </Tooltip>
      </div>

      {open ? (
        <NotesDialog subject={subject} members={noted} onClose={() => setOpen(false)} />
      ) : null}
    </>
  );
}
