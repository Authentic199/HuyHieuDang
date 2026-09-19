import { Tooltip } from 'antd';
import { useLocation, useNavigate } from 'react-router-dom';

import { paths } from '../routes/paths';
import { formatNumber } from '../utils/format';
import './AppSider.css';
import { navItems, type NavItem } from './navItems';

/** Mục nào đang sáng: khớp tiền tố đường dẫn, riêng Dashboard phải khớp tuyệt đối. */
function isActive(item: NavItem, pathname: string): boolean {
  if (item.path === paths.dashboard) return pathname === paths.dashboard;
  return pathname === item.path || pathname.startsWith(`${item.path}/`);
}

/**
 * Sider biểu tượng 5 mục theo artboard.
 * Dùng phần tử điều hướng riêng thay vì Menu của Ant Design vì mỗi mục là
 * khối dọc biểu tượng-trên-chữ-dưới, Menu không dựng được dạng này.
 */
export function AppSider({ uncoveredCount }: { uncoveredCount: number }) {
  const navigate = useNavigate();
  const { pathname } = useLocation();

  return (
    <aside className="hhd-sider">
      <nav className="hhd-sider__nav" aria-label="Điều hướng chính">
        {navItems.map((item) => {
          const active = isActive(item, pathname);
          const showBadge = item.showsUncoveredBadge && uncoveredCount > 0;
          return (
            <button
              key={item.key}
              type="button"
              className={`hhd-sider__item${active ? ' hhd-sider__item--active' : ''}`}
              aria-current={active ? 'page' : undefined}
              onClick={() => navigate(item.path)}
            >
              <span className="hhd-sider__icon" aria-hidden>
                {item.icon}
              </span>
              <span className="hhd-sider__label">{item.label}</span>
              {showBadge ? (
                <Tooltip title={`${formatNumber(uncoveredCount)} người chưa thuộc đợt nào`}>
                  <span className="hhd-sider__badge">{formatNumber(uncoveredCount)}</span>
                </Tooltip>
              ) : null}
            </button>
          );
        })}
      </nav>
    </aside>
  );
}
