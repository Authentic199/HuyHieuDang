import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';

import App from './App';
import './theme/global.css';
// Nạp sau global.css: các khối @media của nó ghi đè cách xếp khung khi cửa sổ
// thấp hoặc hẹp, nên phải đứng cuối chuỗi CSS.
import './theme/responsive.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
