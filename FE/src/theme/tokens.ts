/**
 * Bảng token lấy trực tiếp từ artboard "Màn 0 — Design system nhỏ"
 * (docs/design-system/Huy Hieu Dang - 9 man hinh.html).
 *
 * Đây là NGUỒN MÀU DUY NHẤT của giao diện. Component không được viết mã màu
 * trực tiếp — luôn lấy qua token ở đây hoặc qua biến CSS trong tokens.css.
 */

/** Màu nền tảng */
export const color = {
  /** Đỏ thẫm — nền header, nền sider, cột mốc đậm nhất */
  redDeep: '#6c0f12',
  /** Đỏ chủ đạo — nút chính, tag "Đang diễn ra" */
  red: '#991a14',
  /** Đỏ sáng — điểm đầu gradient nút chính */
  redBright: '#bd2a1c',
  /** Đỏ chữ trên nền sáng — nút chữ, liên kết */
  redLink: '#a3161b',
  /** Vàng sao — chỉ dùng trên nền đỏ (mục sider đang chọn, badge) */
  gold: '#ffcd00',
  /** Vàng chữ cho tag trên nền sáng */
  tagGold: '#8a6a1e',
  /** Nền tag đặc */
  tagFill: '#2a2b2f',
  /** Chữ chính */
  ink: '#1c1c1e',
  /** Nền toàn trang */
  canvas: '#f7f8fa',
  /** Vàng nhạt — nền nhấn nhẹ, số thứ tự bước */
  goldSoft: '#fdf5d8',
  success: '#1f6b4a',
  warning: '#9a6b1f',
  danger: '#8e1d3a',
  info: '#2f4f7a',
} as const;

/** Màu chữ và đường kẻ */
export const neutral = {
  /** Chữ phụ, nhãn trường */
  textSecondary: '#55575c',
  /** Chữ mờ, chú thích mono */
  textTertiary: '#6b6d73',
  /** Chữ bị vô hiệu hóa */
  textDisabled: '#a1a3a8',
  /** Viền thẻ, viền bảng */
  border: '#e6e8ec',
  /** Viền ô nhập (đậm hơn để cán bộ lớn tuổi nhìn rõ) */
  borderInput: '#8a8e96',
  /** Nền vùng chìm, nền nút disabled */
  fill: '#f5f6f8',
  white: '#ffffff',
} as const;

/** Chữ */
export const font = {
  /** Tiêu đề, số liệu lớn */
  serif: "'Noto Serif', Georgia, serif",
  /** Nội dung, nhãn, nút */
  sans: "'Noto Sans', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
  /** Tên file, mã, định dạng ngày mẫu */
  mono: "'JetBrains Mono', ui-monospace, monospace",
} as const;

/** Thang cỡ chữ — nhỏ nhất trong bảng là 14px theo yêu cầu người dùng lớn tuổi */
export const typography = {
  displayLg: { size: 34, line: 42, weight: 700 },
  headingXl: { size: 32, line: 42, weight: 700 },
  headingLg: { size: 22, line: 30, weight: 700 },
  title: { size: 16, line: 24, weight: 700 },
  body: { size: 14, line: 20, weight: 400 },
  label: { size: 13, line: 18, weight: 500 },
  overline: { size: 11, line: 16, weight: 600 },
} as const;

/** Bo góc */
export const radius = {
  /** Ô nhập, nút */
  control: 6,
  /** Thẻ nội dung, mục sider */
  card: 10,
  /** Header, sider, hộp lớn */
  shell: 12,
  /** Hộp đăng nhập */
  panel: 14,
  pill: 9999,
} as const;

/** Khoảng cách — bội số 4 như trong artboard */
export const spacing = {
  xs: 4,
  sm: 8,
  md: 12,
  lg: 16,
  xl: 24,
  xxl: 40,
} as const;

/** Chiều cao cố định lấy từ artboard 1440x900 */
export const size = {
  headerHeight: 56,
  siderWidth: 104,
  siderItemHeight: 64,
  controlHeight: 40,
  controlHeightSm: 32,
  controlHeightLg: 48,
  shellPadding: 16,
} as const;

/** Gradient — ngôn ngữ chung: sáng ở trên, đậm ở dưới, highlight trắng 1px */
export const gradient = {
  header: 'linear-gradient(135deg, #8f1619 0%, #6c0f12 55%, #530b0e 100%)',
  sider: 'linear-gradient(160deg, #86141a 0%, #6c0f12 45%, #560c0f 100%)',
  siderItemActive: 'linear-gradient(180deg, #9d2220 0%, #7d1517 100%)',
  primaryButton: 'linear-gradient(135deg, #bd2a1c 0%, #991a14 50%, #6c0f12 100%)',
} as const;

export const shadow = {
  shell: 'inset 0 1px 0 #ffffff33, 0 2px 8px -3px #6c0f1259',
  sider: 'inset 0 1px 0 #ffffff2e, 0 2px 8px -3px #6c0f1259',
  siderItemActive: 'inset 0 1px 0 #ffffff40',
  primaryButton: 'inset 0 1px 0 #ffffff4d, 0 2px 6px -2px #6c0f1259',
  card: '0 1px 2px #1c1c1e0a',
  pill: 'inset 0 1px 0 #ffffff8c, 0 1px 2px #0a332b26',
} as const;

/**
 * Thang màu mốc huy hiệu "Cool Waters" — mỗi 10 năm lên một bậc.
 * Mốc lẻ dùng bậc của chục liền dưới: 35 năm dùng bậc 30, 45 năm dùng bậc 40.
 * Mọi bậc đạt tương phản chữ ≥ 4.6:1.
 */
export const milestoneScale = [
  {
    from: 30,
    label: 'Mint nhạt',
    background: 'linear-gradient(180deg,#e2fde5 0%,#c7f9cc 55%,#a5ecad 100%)',
    text: '#123f2c',
  },
  {
    from: 40,
    label: 'Mint',
    background: 'linear-gradient(180deg,#c4f8d0 0%,#9ef2b6 55%,#6fdc90 100%)',
    text: '#0f3f2b',
  },
  {
    from: 50,
    label: 'Lục tươi',
    background: 'linear-gradient(180deg,#a9f5b9 0%,#80ed99 55%,#4dd477 100%)',
    text: '#0e3c2a',
  },
  {
    from: 60,
    label: 'Lục ngọc',
    background: 'linear-gradient(180deg,#86e0b5 0%,#57cc99 55%,#2fae7c 100%)',
    text: '#0c3a2d',
  },
  {
    from: 70,
    label: 'Ngọc lam',
    background: 'linear-gradient(180deg,#74d2bd 0%,#48b89f 55%,#2d9a84 100%)',
    text: '#08332f',
  },
  {
    from: 80,
    label: 'Xanh lam ngọc',
    background: 'linear-gradient(180deg,#66c4c6 0%,#38a3a5 55%,#218688 100%)',
    text: '#06302f',
  },
  {
    from: 90,
    label: 'Lam sâu',
    background: 'linear-gradient(180deg,#3b7ba3 0%,#22577a 55%,#163c56 100%)',
    text: '#ffffff',
  },
] as const;

/** Badge trạng thái đợt / dòng import */
export const statusBadge = {
  ongoing: { background: '#991a14', text: '#ffffff', dot: null },
  upcoming: { background: '#fdf1cd', text: '#6e4e00', dot: '#c08a00' },
  past: { background: '#ecedf0', text: '#4e5158', dot: '#9aa0a8' },
  valid: { background: '#dcf3e6', text: '#14603f', dot: '#2fae7c' },
  invalid: { background: '#fbe3e3', text: '#8e1519', dot: '#d13c3c' },
} as const;
