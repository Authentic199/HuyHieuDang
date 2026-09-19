import type { ThemeConfig } from 'antd';

import { color, font, neutral, radius, size, typography } from './tokens';

/**
 * Cấu hình ConfigProvider của Ant Design 5, dịch từ token thiết kế.
 * Mọi thay đổi màu/chữ đi qua đây, không đặt style màu trong component.
 */
export const antdTheme: ThemeConfig = {
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
    fontSize: typography.body.size,
    lineHeight: typography.body.line / typography.body.size,
    fontSizeHeading1: typography.headingXl.size,
    fontSizeHeading2: typography.headingLg.size,
    fontSizeHeading3: typography.title.size,

    borderRadius: radius.control,
    borderRadiusLG: radius.card,
    borderRadiusSM: radius.control,

    controlHeight: size.controlHeight,
    controlHeightSM: size.controlHeightSm,
    controlHeightLG: size.controlHeightLg,

    wireframe: false,
  },
  components: {
    Layout: {
      headerBg: 'transparent',
      headerHeight: size.headerHeight,
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
      // Chữ trong bảng không nhỏ hơn 14px — người dùng là cán bộ lớn tuổi.
      fontSize: typography.body.size,
      headerBg: neutral.fill,
      headerColor: neutral.textSecondary,
      headerSplitColor: neutral.border,
      borderColor: neutral.border,
      rowHoverBg: neutral.fill,
      cellPaddingBlock: 12,
      cellPaddingInline: 16,
    },
    Button: {
      fontWeight: 600,
      primaryShadow: 'none',
      defaultShadow: 'none',
      defaultBorderColor: neutral.border,
    },
    Input: { paddingBlock: 8, activeShadow: 'none' },
    InputNumber: { paddingBlock: 8 },
    Select: { optionSelectedBg: neutral.fill },
    DatePicker: { activeShadow: 'none' },
    Card: { borderRadiusLG: radius.card, paddingLG: 20 },
    Modal: { borderRadiusLG: radius.shell, titleFontSize: typography.headingLg.size },
    Alert: { borderRadiusLG: radius.card, defaultPadding: '14px 20px' },
    Tag: { borderRadiusSM: radius.pill, defaultBg: neutral.fill },
    Segmented: { itemSelectedBg: neutral.white, trackBg: neutral.fill },
    Steps: { iconSize: 28 },
    Tabs: { titleFontSize: typography.body.size, horizontalItemPadding: '12px 0' },
    Statistic: { contentFontSize: typography.displayLg.size },
    Empty: { colorTextDescription: neutral.textSecondary },
  },
};
