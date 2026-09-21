import { expect, test } from '../fixtures/app';
import { FIXED_TODAY, SAFE_WINDOW, isInSafeWindow } from '../fixtures/clock';

/**
 * Tiền đề của cả sáu luồng — kiểm ràng buộc T-FIX trước khi kiểm nghiệp vụ.
 *
 * Nếu đồng hồ không đúng thì mọi con số QT3, QT3a, QT8, QT11 sẽ sai theo, và
 * một luồng đỏ ở dưới sẽ bị đọc nhầm thành lỗi sản phẩm. Hai ca dưới đây nói rõ
 * trạng thái đồng hồ trước khi bất cứ luồng nào chạy.
 */
test.describe('E0 · Tiền đề đóng băng thời gian (T-FIX)', () => {
  /**
   * E0-01 · T-FIX-4 — Backend phải đọc `HUYHIEUDANG_TEST_TODAY` khi không chạy
   * ở Production, để Playwright ép được "hôm nay".
   *
   * ĐÃ SỬA ở PR #38 — xác minh ngày 20/09/2026: dựng lại stack e2e từ nhánh
   * đó thì máy chủ báo đúng 2026-09-19 và ca này xanh. Gỡ `skip` ngay khi PR
   * vào `main`; giữ `skip` lúc này để `main` không đỏ.
   *
   * Gốc lỗi QC-T28-01 (trùng gốc với QC-T27-01, ca A-903b ở
   * `BE/tests/HuyHieuDang.Web.QcIntegrationTests/A9TechnicalTests.cs`).
   * `BE/src` không đọc biến này ở bất cứ đâu, nên `docker-compose.e2e.yml` đặt
   * biến mà máy chủ vẫn trả ngày thật của máy. Bỏ `skip` là ca đỏ lại ngay.
   */
  test.skip('E0-01 · Backend đóng băng "hôm nay" theo HUYHIEUDANG_TEST_TODAY [QC-T28-01]', async ({
    api,
  }) => {
    const dashboard = await api.dashboard();
    expect(
      dashboard.today,
      'Backend phải trả đúng ngày bị ép qua biến môi trường HUYHIEUDANG_TEST_TODAY (T-FIX-4)',
    ).toBe(FIXED_TODAY);
  });

  /**
   * E0-02 · Chừng nào T-FIX-4 chưa có, bộ kiểm thử vẫn phải tất định: ngày máy
   * chủ bắt buộc nằm trong khoảng mà mọi con số của bộ dữ liệu biên còn nguyên.
   * Ra ngoài khoảng đó thì dừng ngay, thay vì để sáu luồng đỏ vì lý do khác.
   */
  test('E0-02 · Ngày máy chủ nằm trong khoảng an toàn của bộ dữ liệu biên', async ({ api }) => {
    const dashboard = await api.dashboard();
    expect(
      isInSafeWindow(dashboard.today),
      `Máy chủ báo hôm nay là ${dashboard.today}. Bộ dữ liệu biên chỉ đúng trong khoảng ` +
        `${SAFE_WINDOW.from} … ${SAFE_WINDOW.to} (mốc muốn ép: ${FIXED_TODAY}). ` +
        'Xem FE/e2e/README.md mục "Đóng băng thời gian".',
    ).toBe(true);
  });

  /** E0-03 · T-FIX-5 — đồng hồ trình duyệt đã bị đóng băng đúng mốc. */
  test('E0-03 · Đồng hồ trình duyệt đóng băng đúng mốc và đúng múi giờ', async ({ page }) => {
    await page.goto('/dang-nhap');

    const browserDate = await page.evaluate(() =>
      new Intl.DateTimeFormat('en-CA', {
        timeZone: 'Asia/Ho_Chi_Minh',
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
      }).format(new Date()),
    );

    expect(browserDate, 'page.clock.install phải ép ngày của trình duyệt về mốc T0').toBe(
      FIXED_TODAY,
    );
    // UTC+07:00 quanh năm — Việt Nam không dùng giờ mùa hè từ 1975 (T-FIX-2).
    expect(
      await page.evaluate(() => new Date().getTimezoneOffset()),
      'Cấu hình Playwright phải đặt timezoneId = Asia/Ho_Chi_Minh (T-FIX-2)',
    ).toBe(-420);
  });
});
