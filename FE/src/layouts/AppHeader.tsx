import { Button } from 'antd';

import type { SessionInfo } from '../api/auth';
import { BrandMark } from '../components/BrandMark';
import { todayLabel } from '../utils/format';
import './AppHeader.css';

interface AppHeaderProps {
  session: SessionInfo | null;
  onLogout: () => void;
}

/** Thanh đầu trang: tên hệ thống, tên đơn vị, ngày hôm nay, tài khoản, nút Đăng xuất. */
export function AppHeader({ session, onLogout }: AppHeaderProps) {
  // Tên đơn vị chưa đặt thì header chỉ hiện tên hệ thống (UC-51).
  const unitName = session?.unitName?.trim();

  return (
    <header className="hhd-header">
      <BrandMark />
      {unitName ? (
        <>
          <span className="hhd-header__divider" />
          <span className="hhd-header__unit">{unitName}</span>
        </>
      ) : null}
      <div className="hhd-header__meta">
        <span className="hhd-header__today">{todayLabel(session?.serverDate)}</span>
        <span className="hhd-header__divider" />
        <span>{session?.displayName ?? session?.username ?? ''}</span>
        <Button className="hhd-header__logout" onClick={onLogout}>
          Đăng xuất
        </Button>
      </div>
    </header>
  );
}
