import { App as AntApp, ConfigProvider } from 'antd';
import type { Locale } from 'antd/es/locale';
import viVNModule from 'antd/locale/vi_VN';
import { Suspense } from 'react';
import { BrowserRouter } from 'react-router-dom';

import { AuthProvider } from './auth/AuthProvider';
import { AppRoutes } from './routes/AppRoutes';
import { antdTheme } from './theme/antdTheme';

/**
 * Bộ chữ tiếng Việt của Ant Design. `antd/locale/vi_VN` là mô-đun CommonJS gói
 * giá trị trong `.default`; bản dựng sản xuất giao lại nguyên lớp vỏ đó nên
 * ConfigProvider không thấy khóa nào và cả phần chữ của Ant Design rơi về tiếng
 * Anh ("20 / page", "Previous Page"). Mở vỏ ra để dev và bản dựng giống nhau.
 */
const viVN = (viVNModule as Locale & { default?: Locale }).default ?? viVNModule;

export default function App() {
  return (
    <ConfigProvider theme={antdTheme} locale={viVN}>
      <AntApp>
        <BrowserRouter>
          <AuthProvider>
            <Suspense fallback={null}>
              <AppRoutes />
            </Suspense>
          </AuthProvider>
        </BrowserRouter>
      </AntApp>
    </ConfigProvider>
  );
}
