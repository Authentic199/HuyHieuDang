import type { Page } from '@playwright/test';

import { T0 } from './expected';

/**
 * Đóng băng thời gian (T-FIX của docs/test-plan.md mục 2).
 *
 * Hai đồng hồ phải cùng một mốc trong một lần chạy:
 *  - Backend: biến môi trường `HUYHIEUDANG_TEST_TODAY` (T-FIX-4), đặt trong
 *    `docker-compose.e2e.yml`.
 *  - Trình duyệt: `page.clock.install` (T-FIX-5), đặt trong fixture `app.ts`.
 *
 * Backend hiện CHƯA đọc biến môi trường đó — lỗi QC-T27-01, ca A-903b của
 * `BE/tests/HuyHieuDang.Web.QcIntegrationTests/A9TechnicalTests.cs` đang để
 * `Skip` vì đúng lý do này. Ca E0-01 của bộ này phát hiện lại ở tầng E2E.
 */

/** Mốc thời gian bộ kiểm thử muốn ép. Mặc định T0 = 19/09/2026. */
export const FIXED_TODAY = process.env.E2E_TODAY ?? T0;

/** 08:00 giờ Việt Nam của ngày đang ép — đủ xa nửa đêm để không trôi sang ngày khác. */
export const FIXED_INSTANT = `${FIXED_TODAY}T08:00:00+07:00`;

/**
 * Khoảng ngày mà MỌI con số của kịch bản `core_default_T0` vẫn đúng nguyên.
 *
 * Suy ra từ bộ dữ liệu biên: ngày tròn mốc gần T0 nhất về trước là 10/09/2026
 * (Trương Thị Nhàn) và gần nhất về sau là 30/09/2026 (Lê Văn Cường); Đợt 2/9
 * kết thúc 10/09 và Đợt 7/11 mở 01/10. Trong khoảng này không ai đổi tuổi đảng,
 * không đợt nào đổi trạng thái, danh sách đủ điều kiện và badge giữ nguyên —
 * chỉ số ngày đếm ngược tới Đợt 7/11 là thay đổi.
 *
 * Nhờ vậy bộ kiểm thử vẫn chạy được khi Backend chưa đọc được biến ép ngày,
 * và chỉ đúng một giá trị duy nhất phải tính theo ngày máy chủ báo về.
 */
export const SAFE_WINDOW = { from: '2026-09-11', to: '2026-09-29' } as const;

/** Ngày Đợt 7/11 mở màn — mốc để tính số ngày đếm ngược của QT11. */
export const UPCOMING_PERIOD_START = '2026-10-01';

/** Số ngày nguyên giữa hai ngày `yyyy-MM-dd`, không phụ thuộc múi giờ máy chạy. */
export function daysBetween(fromIso: string, toIso: string): number {
  const from = Date.UTC(
    Number(fromIso.slice(0, 4)),
    Number(fromIso.slice(5, 7)) - 1,
    Number(fromIso.slice(8, 10)),
  );
  const to = Date.UTC(
    Number(toIso.slice(0, 4)),
    Number(toIso.slice(5, 7)) - 1,
    Number(toIso.slice(8, 10)),
  );
  return Math.round((to - from) / 86_400_000);
}

/** `2026-09-19` → `19/09/2026`, đúng cách giao diện hiển thị. */
export function displayDate(iso: string): string {
  return `${iso.slice(8, 10)}/${iso.slice(5, 7)}/${iso.slice(0, 4)}`;
}

/** Số ngày còn lại tới Đợt 7/11 mà QT11 phải hiện, tính theo ngày máy chủ báo. */
export function daysRemainingToUpcoming(serverToday: string): number {
  return daysBetween(serverToday, UPCOMING_PERIOD_START);
}

/** Ngày máy chủ có nằm trong khoảng an toàn của bộ dữ liệu không. */
export function isInSafeWindow(serverToday: string): boolean {
  return serverToday >= SAFE_WINDOW.from && serverToday <= SAFE_WINDOW.to;
}

/** Đồng hồ máy chủ có đúng bằng mốc bộ kiểm thử đang ép không (T-FIX-4). */
export function isFrozen(serverToday: string): boolean {
  return serverToday === FIXED_TODAY;
}

/**
 * Ngày chính thức của dòng Excel 11 trong `loi-4-dong.xlsx` — cố ý đặt là
 * T0 + 1 ngày để thành lỗi "ngày ở tương lai" (QT9).
 *
 * Đây là chỗ DUY NHẤT trong sáu luồng mà việc chưa đóng băng được đồng hồ máy
 * chủ làm đổi kết quả mong đợi: ngày 20/09/2026 chỉ là tương lai khi hôm nay
 * còn là 19/09/2026. Bộ kiểm thử vì vậy suy số dòng lỗi từ chính quy tắc QT9
 * ("Ngày chính thức ≤ hôm nay") thay vì viết cứng, và vẫn chốt lại đúng con số
 * của kế hoạch (6 hợp lệ / 4 lỗi) khi đồng hồ đã đóng băng.
 */
export const FUTURE_ROW_ADMISSION_DATE = '2026-09-20';

/** Ngày chính thức đó có còn là "tương lai" so với ngày máy chủ đang dùng không. */
export function futureRowStillInvalid(serverToday: string): boolean {
  return FUTURE_ROW_ADMISSION_DATE > serverToday;
}

/** Ngày hôm sau ngày máy chủ — luôn bị từ chối theo ràng buộc "≤ hôm nay". */
export function tomorrowOf(serverToday: string): string {
  const date = new Date(`${serverToday}T00:00:00Z`);
  date.setUTCDate(date.getUTCDate() + 1);
  return date.toISOString().slice(0, 10);
}

/**
 * Đóng băng đồng hồ trình duyệt trước khi mở trang đầu tiên (T-FIX-5).
 *
 * Giao diện lấy "hôm nay" từ máy chủ (`serverDate`), nên đồng hồ trình duyệt
 * chỉ ảnh hưởng vài chỗ dùng `new Date()` trực tiếp — nhưng vẫn phải đóng băng
 * để lịch của ô chọn ngày và mọi so sánh phía client không đổi theo ngày chạy.
 *
 * Gọi `resume()` ngay sau `install()`: `install()` một mình dựng đồng hồ giả
 * và DỪNG hẳn, khiến `setTimeout` không bao giờ nổ — ô tìm kiếm hoãn 400 ms và
 * ô xem trước dãy mốc hoãn 300 ms sẽ đứng im. `resume()` cho thời gian chảy
 * tiếp từ đúng mốc đã ép, nên đồng hồ vẫn cố định về NGÀY mà hẹn giờ vẫn chạy.
 */
export async function installClock(page: Page, instant: string = FIXED_INSTANT): Promise<void> {
  await page.clock.install({ time: new Date(instant) });
  await page.clock.resume();
}
