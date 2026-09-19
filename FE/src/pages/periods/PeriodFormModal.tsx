import { InfoCircleOutlined } from '@ant-design/icons';
import { Alert, DatePicker, Form, Input, Modal } from 'antd';
import dayjs, { type Dayjs } from 'dayjs';
import { useState } from 'react';

import { periodsApi } from '../../api';
import { FALLBACK_MESSAGE, messageText } from '../../api/messages';
import type { AwardPeriodPayload } from '../../api/periods';
import { ApiError } from '../../types/api';
import type { AwardPeriodResponse, CoverageWarnings } from '../../types/domain';

/**
 * Modal Thêm / Sửa đợt trao huy hiệu (UC-31, UC-32) — bám mockup "Thêm đợt trao
 * huy hiệu" đi kèm artboard 5: ô nhập cao 40px, hai ô ngày xếp hai cột, dòng
 * nhắc nền xám, nút chính đỏ ở góc phải.
 *
 * Đợt chỉ lưu NGÀY và THÁNG, không lưu năm (QT6). Để cán bộ vẫn chọn được
 * 29/02, lịch chạy trên một năm nhuận cố định và ô nhập chỉ hiện dd/MM — người
 * dùng không bao giờ nhìn thấy năm này.
 */

/** Năm nhuận cố định, chỉ để lịch có đủ ngày 29/02. Không gửi lên máy chủ. */
const LEAP_REFERENCE_YEAR = 2024;
const DAY_MONTH_FORMAT = 'DD/MM';

interface PeriodFormValues {
  name: string;
  from: Dayjs | null;
  to: Dayjs | null;
}

interface PeriodFormModalProps {
  /** null nghĩa là đang thêm mới (UC-31); có giá trị là đang sửa (UC-32). */
  period: AwardPeriodResponse | null;
  onCancel: () => void;
  /** Lưu xong: màn hình đổi banner theo cảnh báo mới rồi tải lại bảng. */
  onSaved: (savedMessage: string, warnings: CoverageWarnings) => void;
}

/** Ghép ngày/tháng của đợt thành giá trị cho lịch, luôn ở năm nhuận mẫu. */
function toPickerValue(day: number | undefined, month: number | undefined): Dayjs | null {
  if (!day || !month) return null;
  const value = dayjs(
    `${LEAP_REFERENCE_YEAR}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`,
  );
  return value.isValid() ? value : null;
}

export function PeriodFormModal({ period, onCancel, onSaved }: PeriodFormModalProps) {
  const [form] = Form.useForm<PeriodFormValues>();
  const [submitting, setSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const isEditing = period !== null;

  async function handleFinish(values: PeriodFormValues) {
    if (submitting) return;
    setSubmitting(true);
    setErrorMessage(null);

    // Form đã bắt buộc hai ô ngày nên tới đây chắc chắn có giá trị.
    const from = values.from as Dayjs;
    const to = values.to as Dayjs;
    const payload: AwardPeriodPayload = {
      name: values.name.trim(),
      fromDay: from.date(),
      fromMonth: from.month() + 1,
      toDay: to.date(),
      toMonth: to.month() + 1,
    };

    try {
      const result = period
        ? await periodsApi.updatePeriod(period.id, payload)
        : await periodsApi.createPeriod(payload);
      onSaved(
        messageText(
          period ? 'Mes.AwardPeriod.Update.Successfully' : 'Mes.AwardPeriod.Create.Successfully',
        ),
        result.warnings,
      );
    } catch (reason) {
      // Câu chữ lấy từ src/api/messages.ts, không viết chuỗi lỗi ở màn hình.
      setErrorMessage(reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE);
      setSubmitting(false);
    }
  }

  return (
    <Modal
      open
      title={isEditing ? 'Sửa đợt trao huy hiệu' : 'Thêm đợt trao huy hiệu'}
      okText={isEditing ? 'Lưu thay đổi' : 'Thêm đợt'}
      cancelText="Hủy"
      width={480}
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
          data-testid="period-form-error"
        />
      ) : null}

      <Form<PeriodFormValues>
        form={form}
        layout="vertical"
        initialValues={{
          name: period?.name ?? '',
          from: toPickerValue(period?.fromDay, period?.fromMonth),
          to: toPickerValue(period?.toDay, period?.toMonth),
        }}
        // Dấu sao đứng SAU nhãn, đúng mockup "Tên đợt *" của artboard 5.
        requiredMark={(label, { required }) => (
          <>
            {label}
            {required ? <span style={{ color: '#8e1d3a' }}>{' *'}</span> : null}
          </>
        )}
        onFinish={handleFinish}
        disabled={submitting}
      >
        <Form.Item
          name="name"
          label="Tên đợt"
          rules={[
            {
              required: true,
              whitespace: true,
              message: messageText('Mes.AwardPeriod.Required.Name'),
            },
          ]}
        >
          <Input size="large" autoFocus placeholder="Ví dụ: Đợt 7/11" maxLength={200} />
        </Form.Item>

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16 }}>
          <Form.Item
            name="from"
            label="Từ ngày"
            rules={[{ required: true, message: messageText('Mes.AwardPeriod.Invalid.FromDate') }]}
          >
            <DatePicker
              size="large"
              style={{ width: '100%' }}
              format={DAY_MONTH_FORMAT}
              placeholder="dd/mm"
              inputReadOnly
              showNow={false}
              // Lịch khóa trong một năm duy nhất: người dùng chỉ chọn ngày và
              // tháng, không lạc sang năm khác.
              disabledDate={(current) => current.year() !== LEAP_REFERENCE_YEAR}
              defaultPickerValue={dayjs(`${LEAP_REFERENCE_YEAR}-01-01`)}
            />
          </Form.Item>

          <Form.Item
            name="to"
            label="Đến ngày"
            dependencies={['from']}
            rules={[
              { required: true, message: messageText('Mes.AwardPeriod.Invalid.ToDate') },
              ({ getFieldValue }) => ({
                validator: (_rule, value: Dayjs | null) => {
                  const from = getFieldValue('from') as Dayjs | null;
                  // Đợt vắt năm không có trong v1 — Đến ngày phải bằng hoặc sau
                  // Từ ngày trong cùng một năm (QT6).
                  if (!value || !from || !value.isBefore(from, 'day')) return Promise.resolve();
                  return Promise.reject(new Error(messageText('Mes.AwardPeriod.Invalid.Range')));
                },
              }),
            ]}
          >
            <DatePicker
              size="large"
              style={{ width: '100%' }}
              format={DAY_MONTH_FORMAT}
              placeholder="dd/mm"
              inputReadOnly
              showNow={false}
              disabledDate={(current) => current.year() !== LEAP_REFERENCE_YEAR}
              defaultPickerValue={dayjs(`${LEAP_REFERENCE_YEAR}-01-01`)}
            />
          </Form.Item>
        </div>

        <div className="hhd-period-form__note">
          <InfoCircleOutlined style={{ color: '#2b466b', fontSize: 18, flex: 'none' }} />
          <span>Chỉ lưu ngày/tháng. Thay đổi áp dụng ngay cho năm hiện tại và các năm sau.</span>
        </div>
      </Form>
    </Modal>
  );
}
