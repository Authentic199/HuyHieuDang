import fs from 'node:fs';
import path from 'node:path';

import { chromium } from '@playwright/test';

/**
 * Chụp bảy ảnh bằng chứng của luồng E2E-8 (T75 — ghi chú đảng viên, QT12).
 *
 * Chạy trên đúng stack của `docker-compose.e2e.yml` như bộ kiểm thử, nhưng tách
 * riêng khỏi spec: ảnh là bằng chứng bàn giao cho người đọc, không phải một ca
 * kiểm thử. Dữ liệu tự dọn và tự dựng lại nên chạy lại bao nhiêu lần cũng ra
 * cùng một ảnh — với điều kiện dịch vụ `be` đang chạy ở mốc T0 (19/09/2026).
 *
 *   node e2e/scripts/chup-anh-e8.mjs
 *
 * Ảnh lưu vào `FE/e2e/.artifacts/anh-t75/`.
 */

const BASE_URL = process.env.E2E_BASE_URL ?? 'http://localhost:4174';
const API_URL = process.env.E2E_API_URL ?? `${BASE_URL}/api`;
const HERE = path.dirname(new URL(import.meta.url).pathname.slice(1));
const OUT_DIR = path.join(HERE, '..', '.artifacts', 'anh-t75');
const FIXTURES_DIR = path.join(HERE, '..', '..', '..', 'tests', 'fixtures');
const TOKEN_KEY = 'hhd.accessToken';
const UNIT_NAME = 'Đảng ủy Phường Kiểm Thử';
const NOTE = 'Hồ sơ còn thiếu bản sao quyết định kết nạp.\nGia đình xin nhận thay.';

async function call(token, method, route, body) {
  const response = await fetch(`${API_URL}${route}`, {
    method,
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(body === undefined ? {} : { 'Content-Type': 'application/json' }),
    },
    ...(body === undefined ? {} : { body: JSON.stringify(body) }),
  });
  const text = await response.text();
  if (!response.ok) throw new Error(`API ${response.status} ${route}: ${text.slice(0, 300)}`);
  return text ? JSON.parse(text).data : null;
}

async function upload(token, route, filePath) {
  const form = new FormData();
  const bytes = fs.readFileSync(filePath);
  form.append('file', new Blob([new Uint8Array(bytes)]), path.basename(filePath));
  const response = await fetch(`${API_URL}${route}`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${token}` },
    body: form,
  });
  const text = await response.text();
  if (!response.ok) throw new Error(`API ${response.status} ${route}: ${text.slice(0, 300)}`);
  return JSON.parse(text).data;
}

/** Kho sạch rồi dựng lại đúng trạng thái đầu của E2E-8. */
async function seedBaseline(token) {
  for (;;) {
    const page = await call(token, 'GET', '/PartyMembers?current=1&pageSize=500');
    if (page.pagedData.length === 0) break;
    await call(token, 'POST', '/PartyMembers/DeleteMany', {
      ids: page.pagedData.map((row) => row.id),
    });
  }
  for (const period of (await call(token, 'GET', '/AwardPeriods')).periods) {
    await call(token, 'DELETE', `/AwardPeriods/${period.id}`);
  }

  const expected = JSON.parse(
    fs.readFileSync(path.join(FIXTURES_DIR, 'data', 'expected.json'), 'utf8'),
  );
  for (const period of expected.periods.main) {
    await call(token, 'POST', '/AwardPeriods', {
      name: period.name,
      fromDay: period.fromDay,
      fromMonth: period.fromMonth,
      toDay: period.toDay,
      toMonth: period.toMonth,
    });
  }

  await call(token, 'PUT', '/Settings', {
    startYears: 30,
    endYears: 90,
    stepYears: 5,
    unitName: UNIT_NAME,
  });

  await upload(
    token,
    '/PartyMembers/Import/Commit',
    path.join(FIXTURES_DIR, 'excel', 'core-hop-le.xlsx'),
  );
}

/**
 * Chụp một ảnh sau khi mọi lớp phủ đang mở đã hết hoạt ảnh.
 *
 * Playwright coi một phần tử `opacity: 0` là ĐÃ HIỆN, nên `waitFor()` trả về
 * ngay ở khung hình đầu tiên — lúc hộp thoại của Ant Design còn trong suốt và
 * đang phóng to dần. Ảnh chụp lúc đó trông y hệt ảnh trước khi mở hộp (ảnh 3 và
 * ảnh 4 từng trùng khít từng điểm ảnh). Vì vậy phải chờ theo ĐIỀU KIỆN: mọi hộp
 * thoại, mặt nạ và tooltip đang hiện phải đạt `opacity: 1` và hết biến hình.
 */
async function shoot(page, file) {
  await page.waitForFunction(() => {
    const layers = document.querySelectorAll('.ant-modal, .ant-modal-mask, .ant-tooltip');
    return Array.from(layers).every((layer) => {
      const style = getComputedStyle(layer);
      if (style.display === 'none' || style.visibility === 'hidden') return true;
      const settled = style.transform === 'none' || style.transform === 'matrix(1, 0, 0, 1, 0, 0)';
      return Number(style.opacity) === 1 && settled;
    });
  });
  await page.screenshot({ path: file, animations: 'disabled' });
}

async function main() {
  const login = await call(null, 'POST', '/Auth/Login', {
    username: 'admin',
    password: 'Admin@123',
  });
  const token = login.accessToken;

  await seedBaseline(token);

  const dashboard = await call(token, 'GET', '/Dashboard');
  const subject = dashboard.eligibleMembers[0].fullName;
  const members = await call(token, 'GET', '/PartyMembers?current=1&pageSize=500');
  const subjectId = members.pagedData.find((row) => row.fullName === subject).id;

  await call(token, 'PUT', `/PartyMembers/${subjectId}/Note`, { note: NOTE });

  fs.mkdirSync(OUT_DIR, { recursive: true });

  const browser = await chromium.launch();
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    locale: 'vi-VN',
    timezoneId: 'Asia/Ho_Chi_Minh',
  });
  const page = await context.newPage();
  await page.addInitScript(
    ([key, value]) => window.localStorage.setItem(key, value),
    [TOKEN_KEY, token],
  );

  // 1 — nút ghi chú ở cột Thao tác, tooltip mang đúng ngày ghi.
  await page.goto(`${BASE_URL}/dang-vien`);
  const noteButton = page.getByLabel(`Ghi chú ${subject}`, { exact: true });
  await noteButton.waitFor();
  await noteButton.hover();
  await page.getByRole('tooltip').first().waitFor();
  await shoot(page, path.join(OUT_DIR, '1-nut-ghi-chu-va-tooltip.png'));

  // 2 — hộp ghi chú của một người.
  await noteButton.click();
  const memberDialog = page.locator('.ant-modal').filter({ hasText: `Ghi chú — ${subject}` });
  await memberDialog.getByTestId('note-date-pill').waitFor();
  await shoot(page, path.join(OUT_DIR, '2-hop-ghi-chu-mot-nguoi.png'));
  await memberDialog.getByRole('button', { name: 'Đóng' }).click();

  // 3 — Dashboard: khối gọn `Ghi chú · 1 người`.
  await page.goto(`${BASE_URL}/`);
  const summary = page.getByTestId('note-summary');
  await summary.waitFor();
  await shoot(page, path.join(OUT_DIR, '3-dashboard-khoi-ghi-chu.png'));

  // 4 — hộp xem đầy đủ.
  await summary.click();
  const notesDialog = page.locator('.hhd-note-dialog');
  await notesDialog.getByTestId('note-date-pill').first().waitFor();
  await shoot(page, path.join(OUT_DIR, '4-hop-xem-day-du.png'));

  // 5 — ô tìm trong hộp, tô sáng phần chữ khớp.
  await notesDialog.getByLabel('Tìm trong ghi chú', { exact: true }).fill('quyết định');
  await notesDialog.locator('mark').first().waitFor();
  await shoot(page, path.join(OUT_DIR, '5-o-tim-trong-hop.png'));
  await notesDialog.getByRole('button', { name: 'Đóng' }).click();

  // 6 — form Sửa: ô Ghi chú ở cuối form, có bộ đếm và viên ngày ghi.
  await page.goto(`${BASE_URL}/dang-vien`);
  await page.getByLabel(`Sửa ${subject}`, { exact: true }).click();
  const form = page.locator('.ant-modal-content').last();
  await form.getByTestId('note-date-pill').waitFor();
  await shoot(page, path.join(OUT_DIR, '6-form-sua-o-ghi-chu.png'));
  await form.getByRole('button', { name: 'Đóng' }).click();

  // 7 — xóa ghi chú thì khối gọn biến mất.
  await call(token, 'PUT', `/PartyMembers/${subjectId}/Note`, { note: null });
  await page.goto(`${BASE_URL}/`);
  await page.getByRole('button', { name: 'Xuất Excel' }).first().waitFor();
  await page.waitForFunction(() => !document.querySelector('[data-testid="note-summary"]'));
  await shoot(page, path.join(OUT_DIR, '7-dashboard-sau-khi-xoa.png'));

  await browser.close();

  // Trả kho về trạng thái đầu của luồng để lần chạy sau không bị lệch.
  await call(token, 'PUT', `/PartyMembers/${subjectId}/Note`, { note: null });

  console.log(`Đã chụp 7 ảnh vào ${OUT_DIR}`);
}

main().catch((reason) => {
  console.error(reason);
  process.exit(1);
});
