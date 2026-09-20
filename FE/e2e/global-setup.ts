import fs from 'node:fs';
import path from 'node:path';

import { Api } from './fixtures/api';
import { FIXED_TODAY, isFrozen, isInSafeWindow, SAFE_WINDOW } from './fixtures/clock';
import { API_URL, ARTIFACT_DIR, BASE_URL, EXCEL_DIR } from './fixtures/env';

/**
 * Chuẩn bị trước khi chạy sáu luồng:
 *
 *  1. Chờ giao diện và API của stack e2e lên hẳn — không dùng `waitForTimeout`
 *     cố định (ca E-905), chỉ chờ theo điều kiện.
 *  2. Đọc "hôm nay" của máy chủ và in ra rõ ràng. Ngày máy chủ quyết định mọi
 *     con số QT3, QT3a, QT8, QT11 nên phải biết trước khi đọc kết quả.
 *  3. Dựng tệp Excel 11 MB mà bộ dữ liệu cố ý không commit (ca E2-14).
 */

const MAX_WAIT_MS = 180_000;

async function waitForHttp(url: string, accept: (status: number) => boolean): Promise<void> {
  const deadline = Date.now() + MAX_WAIT_MS;
  let lastError = '';
  while (Date.now() < deadline) {
    try {
      const response = await fetch(url);
      if (accept(response.status)) return;
      lastError = `mã trả về ${response.status}`;
    } catch (reason) {
      lastError = reason instanceof Error ? reason.message : String(reason);
    }
    await new Promise((resolve) => setTimeout(resolve, 1_000));
  }
  throw new Error(
    `Chờ quá ${MAX_WAIT_MS / 1000}s mà ${url} chưa sẵn sàng (${lastError}).\n` +
      'Dựng môi trường trước khi chạy: xem FE/e2e/README.md.',
  );
}

/**
 * Tệp 11 MB dùng cho ca "vượt 10 MB". Bộ dữ liệu cố ý không commit nó
 * (`tests/fixtures/.gitignore`), nên dựng tại chỗ bằng cách nhồi thêm byte vào
 * cuối một tệp `.xlsx` thật — giao diện chặn theo DUNG LƯỢNG trước khi đọc nội
 * dung, đúng thứ tự mà ca kiểm thử muốn kiểm.
 */
function buildOversizeFile(): void {
  fs.mkdirSync(ARTIFACT_DIR, { recursive: true });
  const target = path.join(ARTIFACT_DIR, 'loi-qua-10mb.xlsx');
  const wanted = 11 * 1024 * 1024;
  if (fs.existsSync(target) && fs.statSync(target).size >= wanted) return;

  const base = fs.readFileSync(path.join(EXCEL_DIR, 'core-hop-le.xlsx'));
  fs.writeFileSync(target, Buffer.concat([base, Buffer.alloc(wanted - base.length, 0x20)]));
}

export default async function globalSetup(): Promise<void> {
  await waitForHttp(BASE_URL, (status) => status === 200);
  await waitForHttp(`${API_URL}/Auth/Me`, (status) => status === 401 || status === 200);

  const api = await Api.login();
  const dashboard = await api.dashboard();
  const serverToday = dashboard.today;

  buildOversizeFile();

  const lines = [
    '',
    '───── Môi trường kiểm thử end-to-end (T28) ─────',
    `  Giao diện      : ${BASE_URL}`,
    `  API            : ${API_URL}`,
    `  Mốc đang ép    : ${FIXED_TODAY}`,
    `  Máy chủ báo    : ${serverToday}`,
  ];

  if (isFrozen(serverToday)) {
    lines.push('  Đồng hồ        : ĐÃ ĐÓNG BĂNG đúng mốc (T-FIX-4 hoạt động)');
  } else if (isInSafeWindow(serverToday)) {
    lines.push(
      '  Đồng hồ        : CHƯA đóng băng được — Backend bỏ qua HUYHIEUDANG_TEST_TODAY',
      '                   (lỗi QC-T27-01 / QC-T28-01, ca E0-01 báo chi tiết).',
      `                   Ngày máy chủ vẫn nằm trong khoảng an toàn ${SAFE_WINDOW.from} … ${SAFE_WINDOW.to}`,
      '                   nên mọi con số của bộ dữ liệu biên còn nguyên giá trị;',
      '                   riêng số ngày đếm ngược tính theo ngày máy chủ báo về.',
    );
  } else {
    lines.push(
      '  Đồng hồ        : NGOÀI KHOẢNG AN TOÀN — kết quả sẽ sai hàng loạt.',
      `                   Cần ${FIXED_TODAY}, hoặc ít nhất trong khoảng`,
      `                   ${SAFE_WINDOW.from} … ${SAFE_WINDOW.to}.`,
    );
  }
  lines.push('────────────────────────────────────────────────', '');
  console.log(lines.join('\n'));
}
