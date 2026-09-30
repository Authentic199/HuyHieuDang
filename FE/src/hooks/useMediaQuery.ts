import { useEffect, useState } from 'react';

/**
 * Theo dõi một media query của trình duyệt.
 *
 * Dùng cho phần responsive nào CSS không làm được một mình — cụ thể là token
 * của Ant Design (cỡ chữ, chiều cao nút, đệm ô bảng): Ant Design sinh style
 * lúc chạy từ giá trị JavaScript nên phải đổi bằng JavaScript, không phải bằng
 * biến CSS. Ngưỡng lấy từ `theme/breakpoints.ts` để CSS và JavaScript luôn đổi
 * cùng lúc.
 */
export function useMediaQuery(query: string): boolean {
  const [matches, setMatches] = useState(() => window.matchMedia(query).matches);

  useEffect(() => {
    const media = window.matchMedia(query);
    const sync = () => setMatches(media.matches);
    // Gọi một lần: cửa sổ có thể đã đổi kích thước giữa lần dựng đầu và lúc này.
    sync();
    media.addEventListener('change', sync);
    return () => media.removeEventListener('change', sync);
  }, [query]);

  return matches;
}
