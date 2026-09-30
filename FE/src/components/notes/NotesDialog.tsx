import { Button, Empty, Modal } from 'antd';
import { useMemo, useState } from 'react';

import { matchesKeyword } from '../../hooks/useClientTable';
import { EligibleMilestoneTag } from '../../pages/periods/detail/EligibleMilestoneTag';
import type { NotedMember } from '../../utils/note';
import { TableSearchInput } from '../TableFilters';
import { HighlightedText } from './HighlightedText';
import { NoteDatePill } from './NoteDatePill';
import './notes.css';

/**
 * Hộp xem đầy đủ ghi chú của Dashboard (UC-11) và Chi tiết đợt (UC-34).
 *
 * Bám đúng "Giao diện đã duyệt" của HUYH-82 sau các sửa đổi của chủ dự án: KHÔNG
 * có dòng đếm "x / y người đủ điều kiện có ghi chú", mọi ngày ghi dùng chung một
 * viên vàng, và có ô tìm ở đầu hộp.
 *
 * Màn hình chỉ dựng hộp khi thật sự mở (`{open ? <NotesDialog/> : null}`) nên
 * đóng rồi mở lại là một lần dựng mới — ô tìm tự trống, không cần dọn tay.
 */

interface NotesDialogProps {
  /** Ví dụ `Đợt 7/11 năm 2026` — ghép sẵn ở màn gọi, hộp không tự đoán */
  subject: string;
  members: NotedMember[];
  onClose: () => void;
}

export function NotesDialog({ subject, members, onClose }: NotesDialogProps) {
  const [keyword, setKeyword] = useState('');

  const trimmed = keyword.trim();
  const shown = useMemo(
    () =>
      trimmed === ''
        ? members
        : members.filter(
            (member) =>
              matchesKeyword(member.fullName, trimmed) || matchesKeyword(member.note, trimmed),
          ),
    [members, trimmed],
  );

  return (
    <Modal
      open
      title={`Ghi chú — ${subject}`}
      width={720}
      destroyOnHidden
      onCancel={onClose}
      className="hhd-note-dialog"
      footer={<Button onClick={onClose}>Đóng</Button>}
    >
      {/* Ô tìm nằm ngoài vùng cuộn nên đứng yên khi danh sách trôi. */}
      <div className="hhd-note-dialog__search">
        <TableSearchInput
          value={keyword}
          onChange={setKeyword}
          placeholder="Tìm trong ghi chú…"
          ariaLabel="Tìm trong ghi chú"
        />
      </div>

      <div className="hhd-note-dialog__list" data-testid="note-dialog-list">
        {shown.length === 0 ? (
          <Empty
            image={Empty.PRESENTED_IMAGE_SIMPLE}
            description={
              <span className="hhd-note-dialog__empty">Không tìm thấy ghi chú nào khớp.</span>
            }
          />
        ) : (
          shown.map((member) => (
            <div className="hhd-note-dialog__item" key={member.key}>
              <div className="hhd-note-dialog__head">
                <span className="hhd-note-dialog__name">
                  <HighlightedText text={member.fullName} keyword={trimmed} />
                </span>
                <EligibleMilestoneTag milestone={member.milestone} />
                <span className="hhd-note-dialog__date">
                  <NoteDatePill value={member.noteUpdatedAt} />
                </span>
              </div>
              <p className="hhd-note-text">
                <HighlightedText text={member.note} keyword={trimmed} />
              </p>
            </div>
          ))
        )}
      </div>
    </Modal>
  );
}
