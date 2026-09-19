import { Alert, DatePicker, Form, Input, Modal, Segmented } from 'antd';
import dayjs, { type Dayjs } from 'dayjs';
import { useState } from 'react';

import { membersApi } from '../../api';
import type { MemberPayload } from '../../api/members';
import { FALLBACK_MESSAGE, messageText } from '../../api/messages';
import { ApiError } from '../../types/api';
import type { Gender, IsoDate, PartyMemberResponse } from '../../types/domain';
import { fromIsoDate, toIsoDate } from '../../utils/format';

/** 'Unknown' là lựa chọn "để trống" của UC-21, gửi lên máy chủ thành null. */
type GenderChoice = Gender | 'Unknown';

interface MemberFormValues {
  fullName: string;
  dateOfBirth: Dayjs | null;
  gender: GenderChoice;
  officialAdmissionDate: Dayjs | null;
}

interface MemberFormModalProps {
  /** null nghĩa là đang thêm mới (UC-21); có giá trị là đang sửa (UC-22). */
  member: PartyMemberResponse | null;
  /** Hôm nay theo lịch MÁY CHỦ, dùng để chặn ngày ở tương lai (mục 1.6). */
  today: IsoDate;
  onCancel: () => void;
  /** Lưu xong: màn hình tải lại bảng và hiện câu báo thành công. */
  onSaved: (savedMessage: string) => void;
}

const DATE_FORMAT = 'DD/MM/YYYY';

/**
 * Modal Thêm / Sửa đảng viên (UC-21, UC-22). Bộ artboard chưa có mockup cho
 * modal này nên dựng theo đúng ngôn ngữ thiết kế của 10 artboard: nhãn trên ô,
 * ô nhập cao 40px, chữ 14px, nút chính đỏ ở góc phải.
 *
 * Màn hình chỉ dựng modal này khi thật sự mở, nên mỗi lần mở là một lần dựng
 * mới: dữ liệu ban đầu và câu lỗi cũ tự sạch, không cần hiệu ứng nạp lại.
 */
export function MemberFormModal({ member, today, onCancel, onSaved }: MemberFormModalProps) {
  const [form] = Form.useForm<MemberFormValues>();
  const [submitting, setSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const isEditing = member !== null;

  // Chưa có ngày máy chủ (phiên vừa hết) thì tạm dùng đồng hồ máy để vẫn chặn
  // được ngày tương lai; máy chủ vẫn kiểm tra lại lần nữa khi lưu.
  const parsedToday = dayjs(today);
  const serverToday = parsedToday.isValid() ? parsedToday : dayjs();

  async function handleFinish(values: MemberFormValues) {
    if (submitting) return;
    setSubmitting(true);
    setErrorMessage(null);

    const payload: MemberPayload = {
      fullName: values.fullName.trim(),
      dateOfBirth: toIsoDate(values.dateOfBirth),
      gender: values.gender === 'Unknown' ? null : values.gender,
      // Form đã bắt buộc ô này nên tới đây chắc chắn có ngày.
      officialAdmissionDate: toIsoDate(values.officialAdmissionDate) as IsoDate,
    };

    try {
      if (member) {
        await membersApi.updateMember(member.id, payload);
        onSaved(messageText('Mes.PartyMember.Update.Successfully'));
      } else {
        await membersApi.createMember(payload);
        onSaved(messageText('Mes.PartyMember.Create.Successfully'));
      }
    } catch (reason) {
      // Câu chữ lấy từ src/api/messages.ts, không viết chuỗi lỗi ở màn hình.
      setErrorMessage(reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE);
      setSubmitting(false);
    }
  }

  return (
    <Modal
      open
      title={isEditing ? 'Sửa đảng viên' : 'Thêm đảng viên'}
      okText={isEditing ? 'Lưu thay đổi' : 'Thêm vào danh sách'}
      cancelText="Đóng"
      width={560}
      destroyOnHidden
      maskClosable={!submitting}
      onCancel={submitting ? undefined : onCancel}
      okButtonProps={{ loading: submitting }}
      cancelButtonProps={{ disabled: submitting }}
      onOk={() => form.submit()}
    >
      {errorMessage ? (
        <Alert
          type="error"
          showIcon
          message={errorMessage}
          style={{ marginBottom: 16 }}
          data-testid="member-form-error"
        />
      ) : null}

      <Form<MemberFormValues>
        form={form}
        layout="vertical"
        initialValues={{
          fullName: member?.fullName ?? '',
          dateOfBirth: fromIsoDate(member?.dateOfBirth),
          gender: member?.gender ?? 'Unknown',
          officialAdmissionDate: fromIsoDate(member?.officialAdmissionDate),
        }}
        requiredMark={false}
        onFinish={handleFinish}
        disabled={submitting}
      >
        <Form.Item
          name="fullName"
          label="Họ tên"
          rules={[
            {
              required: true,
              whitespace: true,
              message: messageText('Mes.PartyMember.Required.FullName'),
            },
          ]}
        >
          <Input size="large" autoFocus placeholder="Ví dụ: Nguyễn Văn An" maxLength={200} />
        </Form.Item>

        <Form.Item
          name="officialAdmissionDate"
          label="Ngày vào Đảng chính thức"
          rules={[
            {
              required: true,
              message: messageText('Mes.PartyMember.Required.OfficialAdmissionDate'),
            },
            {
              validator: (_rule, value: Dayjs | null) =>
                !value || !value.isAfter(serverToday, 'day')
                  ? Promise.resolve()
                  : Promise.reject(
                      new Error(messageText('Mes.PartyMember.Invalid.OfficialAdmissionDate')),
                    ),
            },
          ]}
          extra="Ngày ghi trong quyết định công nhận đảng viên chính thức."
        >
          <DatePicker
            size="large"
            style={{ width: '100%' }}
            format={DATE_FORMAT}
            placeholder="dd/mm/yyyy"
            // Không ai vào Đảng ở ngày mai — chặn luôn cho khỏi gõ nhầm.
            disabledDate={(current) => current.isAfter(serverToday, 'day')}
            // Cán bộ lớn tuổi thường nhập năm trước cho nhanh.
            showNow={false}
          />
        </Form.Item>

        <Form.Item
          name="dateOfBirth"
          label="Ngày sinh"
          dependencies={['officialAdmissionDate']}
          rules={[
            ({ getFieldValue }) => ({
              validator: (_rule, value: Dayjs | null) => {
                const admission = getFieldValue('officialAdmissionDate') as Dayjs | null;
                if (!value || !admission || value.isBefore(admission, 'day')) {
                  return Promise.resolve();
                }
                return Promise.reject(
                  new Error(messageText('Mes.PartyMember.Invalid.DateOfBirth')),
                );
              },
            }),
          ]}
          extra="Để trống cũng được nếu chưa có trong hồ sơ."
        >
          <DatePicker
            size="large"
            style={{ width: '100%' }}
            format={DATE_FORMAT}
            placeholder="dd/mm/yyyy"
            disabledDate={(current) => current.isAfter(serverToday, 'day')}
            showNow={false}
          />
        </Form.Item>

        <Form.Item name="gender" label="Giới tính">
          <Segmented<GenderChoice>
            size="large"
            options={[
              { label: 'Nam', value: 'Male' },
              { label: 'Nữ', value: 'Female' },
              { label: 'Để trống', value: 'Unknown' },
            ]}
          />
        </Form.Item>
      </Form>
    </Modal>
  );
}
