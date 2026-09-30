import type { ThemeConfig } from 'antd';

import {
  color,
  compactSize,
  compactTypography,
  font,
  neutral,
  radius,
  size,
  typography,
} from './tokens';

/**
 * Cấu hình ConfigProvider của Ant Design 5, dịch từ token thiết kế.
 * Mọi thay đổi màu/chữ đi qua đây, không đặt style màu trong component.
 *
 * Hai bản: `antdTheme` cho màn cao (đúng bộ thiết kế 1440x900) và
 * `compactAntdTheme` cho khung nhìn thấp. Dựng từ cùng một hàm để hai bản
 * không bao giờ lệch nhau về màu hay bo góc — chỉ khác cỡ chữ, chiều cao và
 * khoảng đệm. Xem `theme/responsive.css` để biết vì sao cần thang gọn.
 */

/** Phần đo được của một bản chủ đề — cỡ chữ, chiều cao, khoảng đệm. */
interface ThemeScale {
  typography: typeof typography | typeof compactTypography;
  size: typeof size | typeof compactSize;
  /** Đệm dọc / ngang trong ô bảng */
  tableCellPadding: { block: number; inline: number };
  /** Đệm trong thẻ Card */
  cardPadding: number;
  /** Đệm dọc của ô nhập — giữ chữ nằm giữa ô khi ô thấp đi */
  inputPaddingBlock: number;
  /** Đệm của Alert, dạng chuỗi CSS */
  alertPadding: string;
  /** Đệm của đầu tab, dạng chuỗi CSS */
  tabItemPadding: string;
  /** Đường kính vòng số của thanh Steps */
  stepsIconSize: number;
}

const fullScale: ThemeScale = {
  typography,
  size,
  tableCellPadding: { block: 12, inline: 16 },
  cardPadding: 20,
  inputPaddingBlock: 8,
  alertPadding: '14px 20px',
  tabItemPadding: '12px 0',
  stepsIconSize: 28,
};

const compactScale: ThemeScale = {
  typography: compactTypography,
  size: compactSize,
  tableCellPadding: { block: 8, inline: 12 },
  cardPadding: 16,
  inputPaddingBlock: 5,
  alertPadding: '10px 16px',
  tabItemPadding: '8px 0',
  stepsIconSize: 24,
};

function buildTheme(scale: ThemeScale): ThemeConfig {
  const type = scale.typography;
  return {
    token: {
      colorPrimary: color.red,
      colorLink: color.redLink,
      colorInfo: color.info,
      colorSuccess: color.success,
      colorWarning: color.warning,
      colorError: color.danger,

      colorText: color.ink,
      colorTextSecondary: neutral.textSecondary,
      colorTextTertiary: neutral.textTertiary,
      colorTextDisabled: neutral.textDisabled,
      colorBorder: neutral.borderInput,
      colorBorderSecondary: neutral.border,
      colorBgLayout: color.canvas,
      colorBgContainer: neutral.white,
      colorFillTertiary: neutral.fill,

      fontFamily: font.sans,
      fontSize: type.body.size,
      lineHeight: type.body.line / type.body.size,
      fontSizeHeading1: type.headingXl.size,
      fontSizeHeading2: type.headingLg.size,
      fontSizeHeading3: type.title.size,

      borderRadius: radius.control,
      borderRadiusLG: radius.card,
      borderRadiusSM: radius.control,

      controlHeight: scale.size.controlHeight,
      controlHeightSM: scale.size.controlHeightSm,
      controlHeightLG: scale.size.controlHeightLg,

      wireframe: false,
    },
    components: {
      Layout: {
        headerBg: 'transparent',
        headerHeight: scale.size.headerHeight,
        headerPadding: 0,
        bodyBg: color.canvas,
        siderBg: 'transparent',
      },
      Menu: {
        // Sider dùng nền gradient riêng nên Menu để trong suốt.
        itemBg: 'transparent',
        subMenuItemBg: 'transparent',
        itemColor: neutral.white,
        itemHoverColor: color.gold,
        itemSelectedColor: color.gold,
        itemSelectedBg: 'transparent',
        itemHoverBg: 'transparent',
        itemBorderRadius: radius.card,
        itemMarginInline: 0,
        itemMarginBlock: 2,
      },
      Table: {
        // Chữ trong bảng không nhỏ hơn 13px, kể cả ở thang gọn.
        fontSize: type.body.size,
        headerBg: neutral.fill,
        headerColor: neutral.textSecondary,
        headerSplitColor: neutral.border,
        borderColor: neutral.border,
        rowHoverBg: neutral.fill,
        cellPaddingBlock: scale.tableCellPadding.block,
        cellPaddingInline: scale.tableCellPadding.inline,
      },
      Button: {
        fontWeight: 600,
        primaryShadow: 'none',
        defaultShadow: 'none',
        defaultBorderColor: neutral.border,
      },
      Input: { paddingBlock: scale.inputPaddingBlock, activeShadow: 'none' },
      InputNumber: { paddingBlock: scale.inputPaddingBlock },
      Select: { optionSelectedBg: neutral.fill },
      DatePicker: { activeShadow: 'none' },
      Card: { borderRadiusLG: radius.card, paddingLG: scale.cardPadding },
      Modal: { borderRadiusLG: radius.shell, titleFontSize: type.headingLg.size },
      Alert: { borderRadiusLG: radius.card, defaultPadding: scale.alertPadding },
      Tag: { borderRadiusSM: radius.pill, defaultBg: neutral.fill },
      Segmented: { itemSelectedBg: neutral.white, trackBg: neutral.fill },
      Steps: { iconSize: scale.stepsIconSize },
      Tabs: { titleFontSize: type.body.size, horizontalItemPadding: scale.tabItemPadding },
      Statistic: { contentFontSize: type.displayLg.size },
      Empty: { colorTextDescription: neutral.textSecondary },
    },
  };
}

export const antdTheme: ThemeConfig = buildTheme(fullScale);

export const compactAntdTheme: ThemeConfig = buildTheme(compactScale);
