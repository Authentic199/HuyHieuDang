import { App as AntApp, Alert, Button, Form, Input, Skeleton } from 'antd';
import { useEffect, useMemo, useRef, useState } from 'react';

import { FALLBACK_MESSAGE } from '../../api/messages';
import type { SettingsPayload, SettingsResponse } from '../../api/settings';
import { useAuth } from '../../auth/useAuth';
import { PageHeading } from '../../components/PageHeading';
import { TableError } from '../../components/TableStates';
import { ApiError } from '../../types/api';
import { MilestonePreview } from './MilestonePreview';
import { NumberStepper } from './NumberStepper';
import {
  DEFAULT_MILESTONE_SETTINGS,
  type MilestoneErrors,
  milestoneMessages,
  validateMilestoneInput,
} from './milestones';
import './SettingsPage.css';
import { useMilestonePreview } from './useMilestonePreview';
import { useSettings } from './useSettings';

/** Ba số mốc + tên đơn vị; ô số để trống mang giá trị null. */
interface SettingsFormValues {
  startYears: number | null;
  endYears: number | null;
  stepYears: number | null;
  unitName: string;
}

const EMPTY_FORM: SettingsFormValues = {
  startYears: null,
  endYears: null,
  stepYears: null,
  unitName: '',
};

/** Khóa lỗi của máy chủ → ô nhập tương ứng, để câu lỗi nằm ngay dưới ô. */
const ERROR_FIELDS: Record<string, keyof MilestoneErrors> = {
  'Mes.AppSetting.Invalid.StartYears': 'startYears',
  'Mes.AppSetting.Invalid.EndYears': 'endYears',
  'Mes.AppSetting.Invalid.StepYears': 'stepYears',
  'Mes.AppSetting.Invalid.Range': 'endYears',
};

/** Tên đơn vị đã lưu, quy về chuỗi để so với ô nhập. */
function savedUnitName(settings: SettingsResponse | null): string {
  return settings?.unitName ?? '';
}

/**
 * Màn 7 — Cài đặt (UC-50, UC-51), theo artboard "Màn 7 — Cài đặt mốc tuổi đảng".
 *
 * Dãy mốc luôn do máy chủ sinh: mở màn lấy theo `GET /api/Settings`, đang gõ
 * thì hỏi `GET /api/Settings/Milestones` sau khi ngừng 300 ms (QT1 chỉ nằm ở
 * Backend). Lưu xong phải đổi tên đơn vị trên header (UC-51) và tính lại badge
 * menu trái, vì đổi mốc là đổi mọi danh sách đủ điều kiện (QT5).
 */
export default function SettingsPage() {
  const { message } = AntApp.useApp();
  const { setUnitName, refreshUncoveredCount } = useAuth();
  const { settings, loading, error, saving, restoring, reload, save, restoreDefaults } =
    useSettings();

  const [form] = Form.useForm<SettingsFormValues>();
  // useWatch trả về kho giá trị thô: ô chưa gõ có thể khuyết hẳn, nên phải
  // quy về đúng kiểu trước khi dùng.
  const watched = Form.useWatch<Partial<SettingsFormValues>>([], form);
  const values: SettingsFormValues = {
    startYears: watched?.startYears ?? null,
    endYears: watched?.endYears ?? null,
    stepYears: watched?.stepYears ?? null,
    unitName: watched?.unitName ?? '',
  };

  /** Lỗi máy chủ vừa trả về, xóa ngay khi người dùng sửa lại ô. */
  const [serverErrors, setServerErrors] = useState<MilestoneErrors>({});

  /**
   * Đổ giá trị máy chủ vào form. Tên đơn vị chỉ đổ lại khi chính máy chủ đổi
   * nó — Khôi phục mặc định không được cướp chữ bác đang gõ dở (mục 7.3).
   */
  const syncedRef = useRef<SettingsResponse | null>(null);
  useEffect(() => {
    if (!settings || syncedRef.current === settings) return;
    const previous = syncedRef.current;
    syncedRef.current = settings;

    form.setFieldsValue({
      startYears: settings.startYears,
      endYears: settings.endYears,
      stepYears: settings.stepYears,
    });
    if (!previous || previous.unitName !== settings.unitName) {
      form.setFieldValue('unitName', savedUnitName(settings));
    }
  }, [form, settings]);

  const milestoneInput = useMemo(
    () => ({
      startYears: values.startYears,
      endYears: values.endYears,
      stepYears: values.stepYears,
    }),
    [values.startYears, values.endYears, values.stepYears],
  );

  /** Ràng buộc QT1 kiểm tại chỗ: chặn lời gọi vô ích và khóa nút Lưu. */
  const clientErrors = useMemo(() => validateMilestoneInput(milestoneInput), [milestoneInput]);
  const fieldErrors: MilestoneErrors = { ...serverErrors, ...clientErrors };

  const unitNameError =
    values.unitName.trim().length > 200 ? milestoneMessages.unitNameTooLong : null;

  const preview = useMilestonePreview(milestoneInput, fieldErrors, settings);

  /** Chưa đổi gì thì không có gì để lưu. */
  const dirty =
    settings !== null &&
    (values.startYears !== settings.startYears ||
      values.endYears !== settings.endYears ||
      values.stepYears !== settings.stepYears ||
      values.unitName.trim() !== savedUnitName(settings));

  const busy = saving || restoring;
  const hasError = Object.keys(fieldErrors).length > 0 || unitNameError !== null;
  const canSave = dirty && !hasError && !busy;

  async function handleSave() {
    if (!canSave) return;

    const unitName = values.unitName.trim();
    const payload: SettingsPayload = {
      startYears: values.startYears as number,
      endYears: values.endYears as number,
      stepYears: values.stepYears as number,
      unitName: unitName ? unitName : null,
    };

    try {
      const saved = await save(payload);
      // Header đọc tên đơn vị từ phiên làm việc, phải bảo nó đổi theo (UC-51).
      setUnitName(saved.unitName);
      // Đổi mốc là đổi danh sách "chưa thuộc đợt nào" → badge phải tính lại.
      void refreshUncoveredCount();
      message.success(milestoneMessages.saved);
    } catch (reason) {
      if (reason instanceof ApiError) {
        const field = reason.messageKey ? ERROR_FIELDS[reason.messageKey] : undefined;
        if (field) {
          setServerErrors({ [field]: reason.message });
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
      setServerErrors({});
      const { startYears, endYears, stepYears } = DEFAULT_MILESTONE_SETTINGS;
      message.success(`Đã khôi phục mốc mặc định ${startYears} / ${endYears} / ${stepYears}`);
    } catch (reason) {
      message.error(reason instanceof ApiError ? reason.message : FALLBACK_MESSAGE);
    }
  }

  if (loading) {
    return (
      <div className="hhd-settings">
        <PageHeading title="Cài đặt" />
        <div className="hhd-settings__card">
          <Skeleton active paragraph={{ rows: 6 }} />
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="hhd-settings">
        <PageHeading title="Cài đặt" />
        <div className="hhd-settings__card">
          <TableError message={error} onRetry={reload} />
        </div>
      </div>
    );
  }

  return (
    <div className="hhd-settings">
      <PageHeading title="Cài đặt" />

      <Form<SettingsFormValues>
        form={form}
        layout="vertical"
        requiredMark={false}
        disabled={busy}
        initialValues={EMPTY_FORM}
        // Bác sửa lại ô nào thì bỏ câu lỗi máy chủ đang treo ở đó.
        onValuesChange={() => setServerErrors({})}
      >
        <div className="hhd-settings__card">
          <div>
            <div className="hhd-settings__card-title">Mốc tuổi đảng</div>
            <div className="hhd-settings__card-note">
              Dãy số năm được trao huy hiệu. Hệ thống xét từ Bắt đầu, cộng thêm Bước, dừng khi vượt
              Kết thúc.
            </div>
          </div>

          <div className="hhd-settings__grid">
            <Form.Item
              name="startYears"
              label="Bắt đầu (năm)"
              validateStatus={fieldErrors.startYears ? 'error' : ''}
              help={fieldErrors.startYears}
            >
              <NumberStepper label="Bắt đầu (năm)" />
            </Form.Item>

            <Form.Item
              name="endYears"
              label="Kết thúc (năm)"
              validateStatus={fieldErrors.endYears ? 'error' : ''}
              help={fieldErrors.endYears}
            >
              <NumberStepper label="Kết thúc (năm)" />
            </Form.Item>

            <Form.Item
              name="stepYears"
              label="Bước (năm)"
              validateStatus={fieldErrors.stepYears ? 'error' : ''}
              help={fieldErrors.stepYears}
            >
              <NumberStepper label="Bước (năm)" />
            </Form.Item>
          </div>

          <MilestonePreview preview={preview} />

          <Alert
            type="warning"
            showIcon
            className="hhd-settings__impact"
            message="Thay đổi ảnh hưởng ngay đến mọi danh sách đủ điều kiện"
            description="Áp dụng cho mọi năm và cho Dashboard."
          />

          <div className="hhd-settings__card-foot">
            <Button type="text" loading={restoring} onClick={handleRestoreDefaults}>
              Khôi phục mặc định {DEFAULT_MILESTONE_SETTINGS.startYears} /{' '}
              {DEFAULT_MILESTONE_SETTINGS.endYears} / {DEFAULT_MILESTONE_SETTINGS.stepYears}
            </Button>
          </div>
        </div>

        <div className="hhd-settings__card">
          <div>
            <div className="hhd-settings__card-title">Tên đơn vị</div>
            <div className="hhd-settings__card-note">
              Hiện trên header mọi màn và dòng tiêu đề file Excel. Để trống thì header chỉ hiện tên
              hệ thống.
            </div>
          </div>

          <Form.Item
            name="unitName"
            label="Tên đơn vị"
            validateStatus={unitNameError ? 'error' : ''}
            help={unitNameError}
          >
            <Input size="large" placeholder="Đảng ủy Phường X" maxLength={200} allowClear />
          </Form.Item>

          <div className="hhd-settings__card-foot hhd-settings__card-foot--save">
            <span className="hhd-settings__save-note">
              Nút Lưu lưu cả mốc tuổi đảng và tên đơn vị.
            </span>
            <Button type="primary" loading={saving} disabled={!canSave} onClick={handleSave}>
              Lưu
            </Button>
          </div>
        </div>
      </Form>
    </div>
  );
}
