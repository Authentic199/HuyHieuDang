import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

import { expect, loginThroughUi, navItem, test } from '../fixtures/app';
import { FIXED_TODAY } from '../fixtures/clock';
import { ADMIN } from '../fixtures/env';
import { eligibility, scenario, T1, T2 } from '../fixtures/expected';

/**
 * Các ca về tính chạy-lại-được của chính bộ kiểm thử (docs/test-plan.md mục 6,
 * bảng "E2E — ca chạy lại và độc lập").
 *
 * E-901 và E-902 không kiểm được bằng một ca riêng — chúng là tính chất của cả
 * lần chạy. Cách kiểm: chạy `npx playwright test` hai lần liên tiếp không reset
 * tay, rồi chạy lại với `--shard` đảo thứ tự. Kết quả hai lần được dán trong
 * báo cáo `docs/test-report/`. Điều kiện để chúng đúng đã được cài sẵn: mỗi
 * luồng gọi `api.seedBaseline()` hoặc `api.resetAll()` ở dòng đầu tiên.
 */

const SPEC_DIR = path.dirname(fileURLToPath(import.meta.url));
const FIXTURE_DIR = path.join(SPEC_DIR, '..', 'fixtures');

/**
 * Thẻ đợt sắp tới trên Dashboard phải khớp oracle của kịch bản tương ứng.
 *
 * Dùng chung cho E-903 và E-904 vì hai ca chỉ khác nhau ở mốc thời gian; mọi
 * con số đều lấy từ `expected.json`, không viết cứng ở đây.
 */
async function assertUpcomingCard(
  page: Parameters<typeof loginThroughUi>[0],
  scenarioName: 'core_default_T1' | 'core_default_T2',
  eligibilityYear: number,
): Promise<void> {
  const upcoming = scenario(scenarioName).upcomingPeriod!;
  const list = eligibility(scenarioName, upcoming.code, eligibilityYear);

  await page.goto('/');
  await loginThroughUi(page, ADMIN.username, ADMIN.password);
  await navItem(page, 'Dashboard').click();

  const card = page.locator('.hhd-dashboard__card').first();
  await expect(card.locator('.hhd-dashboard__period-name')).toHaveText(upcoming.name);

  // `.first()` là cố ý: khi đợt chưa có ai tròn mốc, thẻ dùng lại đúng lớp này
  // cho dòng "Đợt này chưa có ai tròn mốc…" ở dưới, nên có hai phần tử khớp.
  await expect(card.locator('.hhd-dashboard__period-range').first()).toHaveText(
    `${upcoming.boundFromDisplay} – ${upcoming.boundToDisplay}`,
  );
  await expect(card.locator('.hhd-dashboard__status')).toHaveText(
    upcoming.status === 'Đang diễn ra' ? 'Đang diễn ra' : `Còn ${upcoming.daysLeft} ngày`,
  );
  await expect(card.locator('.hhd-dashboard__count-value')).toHaveText(String(list.total));

  if (list.total === 0) {
    await expect(card).toContainText(`Đợt này chưa có ai tròn mốc trong năm ${upcoming.year}.`);
  }
}

test.describe('E9 · Chạy lại được và độc lập', () => {
  /**
   * E-903 · Chạy lại toàn bộ với mốc T1 = 15/10/2026 để thẻ Dashboard hiện
   * "Đang diễn ra" (QT11).
   *
   * Hai ca E-903 và E-904 từng là chỗ trống để `skip`, vì lỗi QC-T28-01 làm
   * không có cách nào đẩy máy chủ tới một ngày khác. Lỗi đã sửa: `E2E_TODAY`
   * nay ép được cả hai đồng hồ, nên hai ca này có thân thật.
   *
   * Chúng chỉ chạy khi stack được dựng ở đúng mốc — mốc của Backend nằm trong
   * biến môi trường của container nên một lần chạy chỉ kiểm được một mốc:
   *
   * ```bash
   * E2E_TODAY=2026-10-15 docker compose -f e2e/docker-compose.e2e.yml up -d
   * E2E_TODAY=2026-10-15 npx playwright test -c e2e/playwright.e2e.config.ts
   * ```
   *
   * Chạy ở mốc khác thì ca tự bỏ qua kèm câu nhắc, thay vì đỏ oan.
   */
  test(`E-903 · Chạy lại ở mốc T1 = ${T1}: Dashboard báo "Đang diễn ra"`, async ({ api, page }) => {
    test.skip(FIXED_TODAY !== T1, `Chỉ chạy khi stack dựng ở mốc T1 — đặt E2E_TODAY=${T1}.`);

    await api.seedBaseline();
    expect((await api.dashboard()).today, 'máy chủ phải đang ở đúng mốc T1').toBe(T1);

    await assertUpcomingCard(page, 'core_default_T1', 2026);
  });

  /**
   * E-904 · Chạy lại toàn bộ với mốc T2 = 01/12/2026: mọi đợt 2026 đã qua nên
   * đợt sắp tới là Đợt 3/2 của năm 2027 (QT8 nhánh năm sau).
   *
   * Cách chạy giống E-903, đổi mốc thành `2026-12-01`.
   */
  test(`E-904 · Chạy lại ở mốc T2 = ${T2}: Dashboard nhảy sang Đợt 3/2 năm 2027`, async ({
    api,
    page,
  }) => {
    test.skip(FIXED_TODAY !== T2, `Chỉ chạy khi stack dựng ở mốc T2 — đặt E2E_TODAY=${T2}.`);

    await api.seedBaseline();
    expect((await api.dashboard()).today, 'máy chủ phải đang ở đúng mốc T2').toBe(T2);

    // Năm của đợt sắp tới là 2027, không phải năm đang xem: đây chính là nhánh
    // "mọi đợt trong năm đã qua thì nhìn sang năm sau" của QT8.
    await assertUpcomingCard(page, 'core_default_T2', 2027);
  });

  /**
   * E-905 · Không ca nào được chờ một quãng cố định "cho chắc". Chờ cố định làm
   * bộ kiểm thử vừa chậm vừa chập chờn; mọi chỗ chờ phải là chờ theo điều kiện.
   */
  test('E-905 · Không ca nào dùng waitForTimeout', async () => {
    // Quét sáu luồng, ca tiền đề và lớp fixture. Không quét chính tệp này: nó
    // là cái thước đo nên đương nhiên có chữ đó.
    const scanned = [
      ...fs
        .readdirSync(SPEC_DIR)
        .filter((name) => /^e[0-6]-.*\.spec\.ts$/.test(name))
        .map((name) => path.join(SPEC_DIR, name)),
      ...fs
        .readdirSync(FIXTURE_DIR)
        .filter((name) => name.endsWith('.ts'))
        .map((name) => path.join(FIXTURE_DIR, name)),
    ];
    expect(scanned.length, 'Phải quét được ít nhất bảy tệp luồng và fixture').toBeGreaterThan(7);

    // Bắt đúng LỜI GỌI `page.waitForTimeout(...)`, không bắt chữ trong chú thích.
    const offenders = scanned.filter((file) =>
      /\.waitForTimeout\s*\(/.test(fs.readFileSync(file, 'utf8')),
    );
    expect(
      offenders.map((file) => path.basename(file)),
      'Chỉ được chờ theo điều kiện, không chờ một quãng cố định',
    ).toEqual([]);
  });

  /**
   * Kiểm tra chính cách dựng dữ liệu: mọi luồng phải tự dọn kho ở đầu, nếu
   * không thì thứ tự chạy sẽ quyết định kết quả (E-901, E-902).
   */
  test('E-906 · Mỗi luồng tự dựng trạng thái đầu ngay ở dòng đầu tiên', async () => {
    const flows = fs
      .readdirSync(SPEC_DIR)
      .filter((name) => /^e[1-6]-/.test(name) && name.endsWith('.spec.ts'));
    expect(flows, 'Phải có đủ sáu tệp luồng E2E-1 đến E2E-6').toHaveLength(6);

    for (const file of flows) {
      const source = fs.readFileSync(path.join(SPEC_DIR, file), 'utf8');
      expect(
        /await api\.(seedBaseline|resetAll)\(\)/.test(source),
        `${file} phải gọi api.seedBaseline() hoặc api.resetAll() trước khi thao tác`,
      ).toBe(true);
    }
  });
});
