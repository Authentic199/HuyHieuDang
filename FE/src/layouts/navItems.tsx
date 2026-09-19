import {
  AppstoreOutlined,
  CalendarOutlined,
  ControlOutlined,
  TeamOutlined,
  WarningOutlined,
} from '@ant-design/icons';
import type { ReactNode } from 'react';

import { paths } from '../routes/paths';

export interface NavItem {
  key: string;
  label: string;
  icon: ReactNode;
  path: string;
  /** Mục này hiện badge số người chưa thuộc đợt nào */
  showsUncoveredBadge?: boolean;
}

/** 5 mục của sider, đúng thứ tự trong artboard. */
export const navItems: NavItem[] = [
  { key: 'dashboard', label: 'Dashboard', icon: <AppstoreOutlined />, path: paths.dashboard },
  { key: 'members', label: 'Đảng viên', icon: <TeamOutlined />, path: paths.members },
  { key: 'periods', label: 'Đợt trao huy hiệu', icon: <CalendarOutlined />, path: paths.periods },
  {
    key: 'uncovered',
    label: 'Chưa thuộc đợt nào',
    icon: <WarningOutlined />,
    path: paths.uncovered,
    showsUncoveredBadge: true,
  },
  { key: 'settings', label: 'Cài đặt', icon: <ControlOutlined />, path: paths.settings },
];
