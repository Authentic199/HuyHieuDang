import fs from 'node:fs';
import path from 'node:path';

import { chromium } from '@playwright/test';

/**
 * Chụp bốn ảnh bằng chứng của luồng E2E-7 (T54 — đợt trao huy hiệu vắt qua 31/12).
 *
 * Chạy trên đúng stack của `docker-compose.e2e.yml` như bộ kiểm thử, nhưng tách
 * riêng khỏi spec: ảnh là bằng chứng bàn giao cho người đọc, không phải một ca
 * kiểm thử. Dữ liệu tự dọn và tự dựng lại nên chạy lại bao nhiêu lần cũng ra
 * cùng một ảnh.
 *
 *   node e2e/scripts/chup-anh-e7.mjs
 *
 * Ảnh lưu vào `FE/e2e/.artifacts/anh-t54/`.
 */

const BASE_URL = process.env.E2E_BASE_URL ?? 'http://localhost:4174';
const API_URL = process.env.E2E_API_URL ?? `${BASE_URL}/api`;
const OUT_DIR = path.join(
  path.dirname(new URL(import.meta.url).pathname.slice(1)),
  '..',
  '.artifacts',
  'anh-t54',
);
const PERIOD = 'Đợt Giao thừa';

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

async function main() {
  const login = await call(null, 'POST', '/Auth/Login', {
    username: 'admin',
    password: 'Admin@123',
  });
  const token = login.accessToken;

  // Kho sạch: chỉ còn đúng đợt vắt năm và vài đảng viên hai đầu đợt.
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
  await call(token, 'PUT', '/Settings', {
    startYears: 30,
    endYears: 90,
    stepYears: 5,
    unitName: 'Đảng ủy Phường Kiểm Thử',
  });

  const year = Number((await call(token, 'GET', '/Dashboard')).today.slice(0, 4));
  for (const [fullName, gender, dayMonth, offset] of [
    ['Lý Văn Biên', 'Male', '12-01', 0],
    ['Đinh Văn Chạp', 'Male', '12-15', 0],
    ['Cao Thị Hai', 'Female', '02-10', 1],
    ['Mai Thị Cuối', 'Female', '02-28', 1],
    ['Hoàng Thị Hè', 'Female', '06-15', 0],
  ]) {
    await call(token, 'POST', '/PartyMembers', {
      fullName,
      gender,
      dateOfBirth: null,
      officialAdmissionDate: `${year + offset - 30}-${dayMonth}`,
    });
  }

  fs.mkdirSync(OUT_DIR, { recursive: true });
  const browser = await chromium.launch();
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    locale: 'vi-VN',
    timezoneId: 'Asia/Ho_Chi_Minh',
  });
  await context.addInitScript(
    ([key, value]) => window.localStorage.setItem(key, value),
    ['hhd.accessToken', token],
  );
  const page = await context.newPage();

  // Ảnh 1 — modal đang mở với dòng nhắc vắt năm.
  await page.goto(`${BASE_URL}/dot-trao-huy-hieu`);
  await page
    .getByRole('button', { name: /^\+ Thêm đợt( đầu tiên)?$/ })
    .first()
    .click();
  const form = page.locator('.ant-modal-content').last();
  await form.getByLabel('Tên đợt').fill(PERIOD);
  await pick(page, form.getByLabel('Từ ngày'), 1, 12);
  await pick(page, form.getByLabel('Đến ngày'), 28, 2);
  await form.locator('.hhd-period-form__note').waitFor();
  await page.screenshot({ path: path.join(OUT_DIR, '1-modal-vat-nam.png') });

  await form.getByRole('button', { name: 'Thêm đợt', exact: true }).click();
  await form.waitFor({ state: 'hidden' });
  await page
    .locator('.ant-message-notice')
    .first()
    .waitFor({ state: 'detached' })
    .catch(() => {});

  // Ảnh 2 — bảng đợt có chữ "năm sau"; ảnh 3 — dải độ phủ hai vạch hai đầu.
  await page.locator('.hhd-periods__nextyear').waitFor();
  await page.screenshot({ path: path.join(OUT_DIR, '2-bang-dot-nam-sau.png') });
  await page
    .locator('.hhd-coverage')
    .screenshot({ path: path.join(OUT_DIR, '3-dai-do-phu-hai-vach.png') });

  // Ảnh 4 — tab Thông tin của trang chi tiết đợt.
  const periodId = (await call(token, 'GET', '/AwardPeriods')).periods[0].id;
  await page.goto(`${BASE_URL}/dot-trao-huy-hieu/${periodId}`);
  await page.getByRole('tab', { name: 'Thông tin' }).click();
  await page.getByText('năm sau, hằng năm').waitFor();
  await page.screenshot({ path: path.join(OUT_DIR, '4-tab-thong-tin.png') });

  await browser.close();
  console.log(`Đã chụp 4 ảnh vào ${OUT_DIR}`);
}

/** Chọn ngày/tháng trong lịch khóa năm nhuận mẫu 2024 của modal Đợt. */
async function pick(page, field, day, month) {
  const labels = [
    'Jan',
    'Feb',
    'Mar',
    'Apr',
    'May',
    'Jun',
    'Jul',
    'Aug',
    'Sep',
    'Oct',
    'Nov',
    'Dec',
  ];
  await field.click();
  const panel = page.locator('.ant-picker-dropdown').last();
  await panel.waitFor();
  const header = panel.locator('.ant-picker-header-view');
  const current = (await header.innerText()).trim();
  const currentMonth = labels.findIndex((label) => current.startsWith(label)) + 1;
  const steps = currentMonth > 0 ? month - currentMonth : 0;
  const button = steps >= 0 ? '.ant-picker-header-next-btn' : '.ant-picker-header-prev-btn';
  for (let index = 0; index < Math.abs(steps); index += 1) await panel.locator(button).click();
  const pad = (value) => String(value).padStart(2, '0');
  await panel.locator(`td.ant-picker-cell-in-view[title="2024-${pad(month)}-${pad(day)}"]`).click();
  await panel.waitFor({ state: 'hidden' });
}

await main();
