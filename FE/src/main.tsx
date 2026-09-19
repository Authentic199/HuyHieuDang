import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';

import App from './App';
import './theme/global.css';

// ĐIỂM BẬT/TẮT DỮ LIỆU GIẢ — chỗ duy nhất trong toàn bộ mã nguồn.
// Đặt VITE_USE_MOCK=true khi chưa có Backend. T24 nối Backend thật thì xóa
// khối if này cùng cả thư mục src/mocks; không phải sửa gì thêm.
if (import.meta.env.VITE_USE_MOCK === 'true') {
  const { installMockAdapter } = await import('./mocks/adapter');
  installMockAdapter();
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
