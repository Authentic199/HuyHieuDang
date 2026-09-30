import { Alert, Modal } from 'antd';
import { useState } from 'react';

import { membersApi } from '../../api';
import { FALLBACK_MESSAGE, messageText } from '../../api/messages';
import { NoteField } from '../../components/notes/NoteField';
import { ApiError } from '../../types/api';
import type { PartyMemberResponse } from '../../types/domain';

/**
 * Hộp `Ghi chú — <Họ tên>` mở từ nút ghi chú ở cột Thao tác (UC-26).
 *
 * Gọi `PUT /PartyMembers/{id}/Note` chứ không gọi `PUT /PartyMembers/{id}`: hộp
 * này không bày Họ tên, Ngày sinh, Giới tính, Ngày vào Đảng nên không được gửi
 * lại bốn trường người dùng không nhìn thấy (mục 3.6 hợp đồng v1.6).
 *
 * Xóa hết chữ rồi bấm Lưu nghĩa là xóa ghi chú — gửi `null`, máy chủ đưa cả ghi
 * chú lẫn ngày ghi về null (QT12).
 */

interface MemberNoteModalProps {
  member: PartyMemberResponse;
  onCancel: () => void;
  /** Lưu xong: màn hình báo thành công rồi tải lại bảng */
  onSaved: (savedMessage: string) => void;
}

export function MemberNoteModal({ member, onCancel, onSaved }: MemberNoteModalProps) {
  const [note, setNote] = useState(member.note ?? '');
  const [submitting, setSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  async function handleSave() {
    if (submitting) return;
    setSubmitting(true);
    setErrorMessage(null);
    try {
      const trimmed = note.trim();
      await membersApi.updateMemberNote(member.id, trimmed === '' ? null : trimmed);
      onSaved(messageText('Mes.PartyMember.Update.Successfully'));
    } catch (reason) {
      // Lỗi hiện ngay trong hộp, giống form Thêm/Sửa — không bắn ra góc màn hình.
      setErrorMessage(reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE);
      setSubmitting(false);
    }
  }

  return (
    <Modal
      open
      title={`Ghi chú — ${member.fullName}`}
      width={560}
      destroyOnHidden
      okText="Lưu ghi chú"
      cancelText="Đóng"
      maskClosable={!submitting}
      onCancel={submitting ? undefined : onCancel}
      okButtonProps={{ loading: submitting }}
      cancelButtonProps={{ disabled: submitting }}
      onOk={handleSave}
    >
      {errorMessage ? (
        <Alert
          type="error"
          showIcon
          message={errorMessage}
          style={{ marginBottom: 16 }}
          data-testid="member-note-error"
        />
      ) : null}

      <NoteField
        value={note}
        onChange={setNote}
        noteUpdatedAt={member.noteUpdatedAt}
        rows={6}
        autoFocus
        disabled={submitting}
      />
    </Modal>
  );
}
