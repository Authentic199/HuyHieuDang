import { App as AntApp, ConfigProvider } from 'antd';
import type { Locale } from 'antd/es/locale';
import viVNModule from 'antd/locale/vi_VN';
import { Suspense } from 'react';
import { BrowserRouter } from 'react-router-dom';

import { AuthProvider } from './auth/AuthProvider';
import { AppRoutes } from './routes/AppRoutes';
import { antdTheme } from './theme/antdTheme';

/**
 * Gói tiếng Việt của Ant Design phát ra dạng CommonJS; tùy cách gộp mô-đun,
 * `import` có khi trả về cả gói thay vì phần locale bên trong. Gỡ thêm một lớp
 * `default` để chữ của bảng, phân trang và ô chọn luôn là tiếng Việt
 * ("20 / trang", "Trang Kế") chứ không rơi về mặc định tiếng Anh.
 */
const viVN = ((viVNModule as Locale & { default?: Locale }).default ?? viVNModule) as Locale;

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
