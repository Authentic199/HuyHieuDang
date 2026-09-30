import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

import { expect, test } from '@playwright/test';

/**
 * T63 — giao diện không xưng hô với người dùng.
 *
 * Bản đầu gọi người dùng là "bác" ở 14 câu vì tưởng người dùng là cán bộ lớn
 * tuổi. Chủ dự án chốt ngày 30/09: bỏ hẳn đại từ, không thay bằng "bạn",
 * "anh chị" hay "quý vị", cũng không thêm "vui lòng" hay "xin mời" cho lễ phép.
 * Câu giữ nguyên ý, chỉ ngắn hơn.
 *
 * Ca này quét tĩnh toàn bộ mã nguồn giao diện, theo đúng cách ca `E-905` quét
 * mã tìm `waitForTimeout`. Không cần trình duyệt: một câu xưng hô mới lọt vào
 * sẽ bị chặn ngay cả khi không màn nào hiển thị nó trong lần chạy này.
 *
 * Chỉ quét `FE/src`, nên chính tệp này — cái thước đo, đương nhiên chứa đủ các
 * từ bị cấm — nằm ngoài vùng quét.
 */

const SRC_DIR = path.join(path.dirname(fileURLToPath(import.meta.url)), '..', '..', 'src');

/**
 * Chữ cái tiếng Việt, dùng để dựng ranh giới từ. `\b` của JavaScript chỉ biết
 * chữ cái ASCII nên "bác" sẽ khớp cả bên trong một từ dài hơn; chặn hai đầu
 * bằng lớp chữ cái này thì "bàn", "bán", "bạc" không bị bắt oan.
 */
const LETTER = 'A-Za-zÀ-ÖØ-öø-ÿĂăĐđĨĩŨũƠơƯưẠ-ỹ';

/** Đại từ xưng hô bị cấm, và hai cụm rào đón mà quyết định 30/09 bỏ luôn. */
const FORBIDDEN = ['bác', 'bạn', 'anh chị', 'quý vị', 'vui lòng', 'xin mời'];

/** Mọi tệp `.ts` và `.tsx` dưới `FE/src`, kể cả trong thư mục con. */
function sourceFiles(dir: string): string[] {
  return fs.readdirSync(dir, { withFileTypes: true }).flatMap((entry) => {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) return sourceFiles(full);
    return /\.tsx?$/.test(entry.name) ? [full] : [];
  });
}

test('T63 · Mã giao diện không còn chữ xưng hô nào', () => {
  const files = sourceFiles(SRC_DIR);
  expect(files.length, 'Phải quét được mã nguồn trong FE/src').toBeGreaterThan(20);

  const offenders: string[] = [];
  for (const file of files) {
    const source = fs.readFileSync(file, 'utf8');
    const lines = source.split('\n');
    for (const word of FORBIDDEN) {
      const pattern = new RegExp(`(?<![${LETTER}])${word}(?![${LETTER}])`, 'i');
      lines.forEach((line, index) => {
        if (pattern.test(line)) {
          offenders.push(`${path.relative(SRC_DIR, file)}:${index + 1} — "${word}"`);
        }
      });
    }
  }

  expect(
    offenders,
    'Giao diện không xưng hô với người dùng và không rào đón: bỏ hẳn "bác", "bạn", ' +
      '"anh chị", "quý vị", "vui lòng", "xin mời" — kể cả trong chú thích',
  ).toEqual([]);
});
