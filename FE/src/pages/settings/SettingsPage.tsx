import { App as AntApp, Alert, Button, Form, Input, Skeleton } from 'antd';
import { useEffect, useMemo } from 'react';

import { FALLBACK_MESSAGE } from '../../api/messages';
import type { SettingsPayload } from '../../api/settings';
import { useAuth } from '../../auth/useAuth';
import { PageHeading } from '../../components/PageHeading';
import { TableError } from '../../components/TableStates';
import { ApiError } from '../../types/api';
import { MilestonePreview } from './MilestonePreview';
import { NumberStepper } from './NumberStepper';
import { buildMilestones, milestoneMessages } from './milestones';
import './SettingsPage.css';
import { useSettings } from './useSettings';

/** Ba số mốc + tên đơn vị; ô số để trống mang giá trị null. */
interface SettingsFormValues {
  startYears: number | null;
  endYears: number | null;
  stepYears: number | null;
  unitName: string;
}

/** Khóa lỗi của máy chủ → ô nhập tương ứng, để câu lỗi nằm ngay dưới ô. */
const ERROR_FIELDS: Record<string, keyof SettingsFormValues> = {
  'Mes.AppSetting.Invalid.StartYears': 'startYears',
  'Mes.AppSetting.Invalid.EndYears': 'endYears',
  'Mes.AppSetting.Invalid.StepYears': 'stepYears',
  'Mes.AppSetting.Invalid.Range': 'endYears',
};

/**
 * Màn 7 — Cài đặt (UC-50, UC-51), theo artboard "Màn 7 — Cài đặt mốc tuổi đảng".
 *
 * Ba ô số sinh ra dãy mốc theo QT1; ô xem trước đổi ngay khi gõ (tính tại chỗ,
 * không gọi mạng) còn dãy chính thức là dãy máy chủ trả về sau khi Lưu. Lưu
 * xong phải làm mới tên đơn vị trên header (UC-51) và badge menu trái, vì đổi
 * mốc là đổi luôn mọi danh sách đủ điều kiện (QT5).
 */
export default function SettingsPage() {
  const { message } = AntApp.useApp();
  const { setUnitName, refreshUncoveredCount } = useAuth();
  const { settings, loading, error, saving, restoring, reload, save, restoreDefaults } =
    useSettings();

  const [form] = Form.useForm<SettingsFormValues>();
  const values = Form.useWatch<SettingsFormValues>([], form);

  // Máy chủ trả cài đặt về lúc nào thì đổ vào form lúc đó (kể cả sau khi khôi
  // phục mặc định) — form luôn soi đúng thứ đang lưu.
  useEffect(() => {
    if (!settings) return;
    form.setFieldsValue({
      startYears: settings.startYears,
      endYears: settings.endYears,
      stepYears: settings.stepYears,
      unitName: settings.unitName ?? '',
    });
  }, [form, settings]);

  /** Dãy xem trước theo đúng ba số đang gõ; chưa gõ gì thì lấy dãy máy chủ. */
  const preview = useMemo(() => {
    if (!values) return settings?.milestones ?? [];
    return buildMilestones(values);
  }, [settings, values]);

  /** Vì sao chưa sinh được mốc nào — nói thẳng ra trong ô xem trước. */
  const invalidHint = useMemo(() => {
    if (!values) return null;
    const { startYears, endYears, stepYears } = values;
    if (startYears === null || endYears === null || stepYears === null) {
      return 'Hãy điền đủ ba ô Bắt đầu, Kết thúc và Bước để xem dãy mốc.';
    }
    if (endYears < startYears) return milestoneMessages.invalidRange;
    return null;
  }, [values]);

  async function handleSave() {
    let formValues: SettingsFormValues;
    try {
      formValues = await form.validateFields();
    } catch {
      // Ant Design đã hiện câu lỗi dưới từng ô, không cần nói thêm.
      return;
    }

    const unitName = formValues.unitName.trim();
    const payload: SettingsPayload = {
      startYears: formValues.startYears as number,
      endYears: formValues.endYears as number,
      stepYears: formValues.stepYears as number,
      unitName: unitName ? unitName : null,
    };

    try {
      const saved = await save(payload);
      // Header lấy tên đơn vị từ phiên làm việc, phải bảo nó đổi theo (UC-51).
      setUnitName(saved.unitName);
      // Đổi mốc là đổi danh sách "chưa thuộc đợt nào" → badge phải tính lại.
      void refreshUncoveredCount();
      message.success('Đã lưu cài đặt');
    } catch (reason) {
      if (reason instanceof ApiError) {
        const field = reason.messageKey ? ERROR_FIELDS[reason.messageKey] : undefined;
        if (field) {
          form.setFields([{ name: field, errors: [reason.message] }]);
          return;
        }
        message.error(reason.message);
        return;
      }
      message.error(FALLBACK_MESSAGE);
    }
  }

  async function handleRestoreDefaults() {
    try {
      await restoreDefaults();
      // Không đụng tới tên đơn vị (mục 7.3 hợp đồng API).
      void refreshUncoveredCount();
      message.success('Đã khôi phục mốc mặc định 30 / 90 / 5');
    } catch (reason) {
      message.error(reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE);
    }
  }

  const busy = saving || restoring;

  return (
    <div className="hhd-settings">
      <PageHeading title="Cài đặt" />

      <div className="hhd-settings__card">
        {loading ? (
          <Skeleton active paragraph={{ rows: 6 }} />
        ) : error ? (
          <TableError message={error} onRetry={reload} />
        ) : (
          <Form<SettingsFormValues>
            form={form}
            layout="vertical"
            requiredMark={false}
            disabled={busy}
            initialValues={{ startYears: null, endYears: null, stepYears: null, unitName: '' }}
          >
            <section className="hhd-settings__section">
              <div>
                <div className="hhd-settings__section-title">Mốc tuổi đảng</div>
                <div className="hhd-settings__section-note">
                  Dãy số năm được trao huy hiệu. Hệ thống xét từ Bắt đầu, cộng thêm Bước, dừng khi
                  vượt Kết thúc.
                </div>
              </div>

              <div className="hhd-settings__grid">
                <Form.Item
                  name="startYears"
                  label="Bắt đầu (năm)"
                  rules={[
                    { required: true, message: milestoneMessages.requiredStart },
                    { type: 'integer', min: 1, message: milestoneMessages.invalidStart },
                  ]}
                >
                  <NumberStepper label="Bắt đầu (năm)" />
                </Form.Item>

                <Form.Item
                  name="endYears"
                  label="Kết thúc (năm)"
                  dependencies={['startYears']}
                  rules={[
                    { required: true, message: milestoneMessages.requiredEnd },
                    { type: 'integer', min: 1, message: milestoneMessages.invalidEnd },
                    ({ getFieldValue }) => ({
                      validator(_rule, value: number | null) {
                        const start = getFieldValue('startYears') as number | null;
                        if (value === null || start === null || value >= start) {
                          return Promise.resolve();
                        }
                        return Promise.reject(new Error(milestoneMessages.invalidRange));
                      },
                    }),
                  ]}
                >
                  <NumberStepper label="Kết thúc (năm)" />
                </Form.Item>

                <Form.Item
                  name="stepYears"
                  label="Bước (năm)"
                  rules={[
                    { required: true, message: milestoneMessages.requiredStep },
                    { type: 'integer', min: 1, message: milestoneMessages.invalidStep },
                  ]}
                >
                  <NumberStepper label="Bước (năm)" />
                </Form.Item>
              </div>

              <MilestonePreview milestones={preview} invalidHint={invalidHint} />

              <Alert
                type="warning"
                showIcon
                className="hhd-settings__impact"
                message="Thay đổi ảnh hưởng ngay đến mọi danh sách đủ điều kiện"
                description="Áp dụng cho mọi năm và cho Dashboard."
              />
            </section>

            <section className="hhd-settings__section">
              <div>
                <div className="hhd-settings__section-title">Tên đơn vị</div>
                <div className="hhd-settings__section-note">
                  Hiện trên đầu mọi màn hình và trên dòng tiêu đề của file Excel xuất ra. Để trống
                  thì chỉ hiện tên hệ thống.
                </div>
              </div>

              <Form.Item
                name="unitName"
                rules={[{ max: 200, message: milestoneMessages.unitNameTooLong }]}
              >
                <Input
                  size="large"
                  placeholder="Ví dụ: Đảng ủy Phường Nguyễn Du"
                  allowClear
                  aria-label="Tên đơn vị"
                />
              </Form.Item>
            </section>

            <div className="hhd-settings__actions">
              <Button type="text" loading={restoring} onClick={handleRestoreDefaults}>
                Khôi phục mặc định 30 / 90 / 5
              </Button>
              <Button type="primary" loading={saving} onClick={handleSave}>
                Lưu
              </Button>
            </div>
          </Form>
        )}
      </div>
    </div>
  );
}
