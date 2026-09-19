import { MenuFoldOutlined, MenuUnfoldOutlined } from '@ant-design/icons';
import { Tooltip } from 'antd';
import { useLocation, useNavigate } from 'react-router-dom';

import { paths } from '../routes/paths';
import { formatNumber } from '../utils/format';
import './AppSider.css';
import { navItems, type NavItem } from './navItems';

interface AppSiderProps {
  uncoveredCount: number;
  collapsed: boolean;
  onToggleCollapsed: () => void;
}

/** Mục nào đang sáng: khớp tiền tố đường dẫn, riêng Dashboard phải khớp tuyệt đối. */
function isActive(item: NavItem, pathname: string): boolean {
  if (item.path === paths.dashboard) return pathname === paths.dashboard;
  return pathname === item.path || pathname.startsWith(`${item.path}/`);
}

/**
 * Sider biểu tượng 5 mục theo artboard, thu gọn được về dải biểu tượng.
 * Dùng phần tử điều hướng riêng thay vì Menu của Ant Design vì mỗi mục là
 * khối dọc biểu tượng-trên-chữ-dưới, Menu không dựng được dạng này.
 */
export function AppSider({ uncoveredCount, collapsed, onToggleCollapsed }: AppSiderProps) {
  const navigate = useNavigate();
  const { pathname } = useLocation();

  return (
    <aside className={`hhd-sider${collapsed ? ' hhd-sider--collapsed' : ''}`}>
      <nav className="hhd-sider__nav" aria-label="Điều hướng chính">
        {navItems.map((item) => {
          const active = isActive(item, pathname);
          const showBadge = item.showsUncoveredBadge && uncoveredCount > 0;
          const badgeHint = `${formatNumber(uncoveredCount)} người chưa thuộc đợt nào`;
          return (
            <Tooltip
              key={item.key}
              title={collapsed ? item.label : showBadge ? badgeHint : ''}
              placement="right"
            >
              <button
                type="button"
                className={`hhd-sider__item${active ? ' hhd-sider__item--active' : ''}`}
                aria-current={active ? 'page' : undefined}
                aria-label={item.label}
                onClick={() => navigate(item.path)}
              >
                <span className="hhd-sider__icon" aria-hidden>
                  {item.icon}
                </span>
                {collapsed ? null : <span className="hhd-sider__label">{item.label}</span>}
                {showBadge ? (
                  <span className="hhd-sider__badge" aria-label={badgeHint}>
                    {formatNumber(uncoveredCount)}
                  </span>
                ) : null}
              </button>
            </Tooltip>
          );
        })}
      </nav>
      <button
        type="button"
        className="hhd-sider__collapse"
        onClick={onToggleCollapsed}
        aria-label={collapsed ? 'Mở rộng menu' : 'Thu gọn menu'}
        title={collapsed ? 'Mở rộng menu' : 'Thu gọn menu'}
      >
        {collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
      </button>
    </aside>
  );
}
