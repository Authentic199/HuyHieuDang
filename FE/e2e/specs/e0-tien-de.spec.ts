import { expect, test } from '../fixtures/app';
import { FIXED_TODAY, SAFE_WINDOW, isInSafeWindow } from '../fixtures/clock';
import { T0 } from '../fixtures/expected';

/**
 * Tiền đề của cả sáu luồng — kiểm ràng buộc T-FIX trước khi kiểm nghiệp vụ.
 *
 * Nếu đồng hồ không đúng thì mọi con số QT3, QT3a, QT8, QT11 sẽ sai theo, và
 * một luồng đỏ ở dưới sẽ bị đọc nhầm thành lỗi sản phẩm. Hai ca dưới đây nói rõ
 * trạng thái đồng hồ trước khi bất cứ luồng nào chạy.
 */
test.describe('E0 · Tiền đề đóng băng thời gian (T-FIX)', () => {
  /**
   * E0-01 · T-FIX-4 — Backend đọc `HUYHIEUDANG_TEST_TODAY` khi không chạy ở
   * Production, để Playwright ép được "hôm nay".
   *
   * Ca này từng để `skip` vì lỗi QC-T28-01 (cùng gốc với QC-T27-01). Lỗi đã sửa
   * ở PR #38 và mặt Backend có ca A-903b canh; `skip` giữ lại sau đó chỉ là nợ
   * chưa dọn. Đã xác minh trên stack e2e ngày 21/09/2026: máy chủ báo đúng
   * 2026-09-19. Nay ca chạy thật.
   */
  test('E0-01 · Backend đóng băng "hôm nay" theo HUYHIEUDANG_TEST_TODAY', async ({ api }) => {
    const dashboard = await api.dashboard();
    expect(
      dashboard.today,
      'Backend phải trả đúng ngày bị ép qua biến môi trường HUYHIEUDANG_TEST_TODAY (T-FIX-4)',
    ).toBe(FIXED_TODAY);
  });

  /**
   * E0-02 · Lưới an toàn cho mốc mặc định: mọi con số của bộ dữ liệu biên chỉ
   * đúng nguyên khi ngày máy chủ nằm trong một khoảng hẹp quanh T0. Ra ngoài
   * khoảng đó thì dừng ngay ở đây, thay vì để sáu luồng đỏ vì lý do khác.
   *
   * Chạy có chủ đích ở mốc khác (`E2E_TODAY=2026-10-15` cho E-903,
   * `2026-12-01` cho E-904) thì ca này tự bỏ qua: lúc đó bộ số ĐƯỢC PHÉP đổi,
   * và oracle `core_default_T1` / `core_default_T2` mới là thước đo.
   */
  test('E0-02 · Ngày máy chủ nằm trong khoảng an toàn của bộ dữ liệu biên', async ({ api }) => {
    test.skip(
      FIXED_TODAY !== T0,
      `Đang chạy có chủ đích ở mốc ${FIXED_TODAY}, không phải mốc mặc định ${T0}.`,
    );

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
