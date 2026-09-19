import { Button, Result } from 'antd';
import { useNavigate } from 'react-router-dom';

import { paths } from '../routes/paths';

/** Đường dẫn không tồn tại — nói bằng câu chữ thường ngày. */
export default function NotFoundPage() {
  const navigate = useNavigate();

  return (
    <Result
      status="404"
      title="Không tìm thấy trang này"
      subTitle="Đường dẫn có thể đã cũ. Bấm nút dưới để về trang chính."
      extra={
        <Button type="primary" onClick={() => navigate(paths.dashboard)}>
          Về Dashboard
        </Button>
      }
    />
  );
}
