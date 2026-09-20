import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

import { expect, test } from '../fixtures/app';
import { T1, T2 } from '../fixtures/expected';

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

test.describe('E9 · Chạy lại được và độc lập', () => {
  /**
   * E-903 · Chạy lại toàn bộ với mốc T1 = 15/10/2026 để thẻ Dashboard hiện
   * "Đang diễn ra" (QT11).
   *
   * ĐANG CHẶN — lỗi QC-T28-01: Backend không đọc `HUYHIEUDANG_TEST_TODAY` nên
   * không có cách nào đẩy máy chủ tới 15/10/2026. Bỏ `skip` khi T-FIX-4 xong.
   */
  test.skip(`E-903 · Chạy lại ở mốc T1 = ${T1}: Dashboard báo "Đang diễn ra" [QC-T28-01]`, () => {});

  /**
   * E-904 · Chạy lại toàn bộ với mốc T2 = 01/12/2026: mọi đợt 2026 đã qua nên
   * đợt sắp tới là Đợt 3/2 của năm 2027 (QT8 nhánh năm sau).
   *
   * ĐANG CHẶN vì cùng lý do với E-903.
   */
  test.skip(`E-904 · Chạy lại ở mốc T2 = ${T2}: Dashboard nhảy sang Đợt 3/2 năm 2027 [QC-T28-01]`, () => {});

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
