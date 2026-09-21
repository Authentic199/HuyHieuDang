# Kiểm thử đầu-cuối (T28)

Bảy luồng nghiệp vụ **E2E-1 đến E2E-7** của `docs/test-plan.md` mục 6, chạy trên
trình duyệt thật với Backend và PostgreSQL thật. Không có tầng dữ liệu giả nào.

Mỗi luồng là **một ca Playwright**, bên trong chia thành các bước mang đúng mã ca
của kế hoạch (`E1-01`, `E3-05`, `E6-15`…), nên đọc kết quả là biết ngay bước nào đỏ.

| Tệp | Luồng | Phủ |
|---|---|---|
| `specs/e0-tien-de.spec.ts` | Tiền đề đóng băng thời gian | T-FIX-2, T-FIX-4, T-FIX-5 |
| `specs/e1-lan-dung-dau-tien.spec.ts` | E2E-1 Lần dùng đầu tiên | UC-00, UC-13, UC-50, UC-31, UC-24, UC-10, UC-11 |
| `specs/e2-import-co-loi.spec.ts` | E2E-2 Import có lỗi | UC-24, UC-25, QT9 |
| `specs/e3-doi-cai-dat-lan-truyen.spec.ts` | E2E-3 Đổi cài đặt lan truyền | UC-50, QT1, QT5 |
| `specs/e4-sua-dot-lan-truyen.spec.ts` | E2E-4 Sửa đợt lan truyền | UC-32, QT5, QT6, QT7 |
| `specs/e5-xuat-excel.spec.ts` | E2E-5 Xuất Excel ba nơi | UC-11, UC-34, UC-40, UC-51 |
| `specs/e6-vong-doi-dang-vien.spec.ts` | E2E-6 Vòng đời đảng viên | UC-20 → UC-23, QT3, QT3a, QT5 |
| `specs/e7-dot-vat-qua-nam.spec.ts` | E2E-7 Đợt vắt qua 31/12 (T54) | UC-31, UC-34, UC-36, UC-40, QT6, QT7 |
| `specs/e9-chay-lai-va-doc-lap.spec.ts` | Ca về chính bộ kiểm thử | E-903 → E-906 |

## Dựng môi trường rồi chạy

Cần: Docker, Node 22 trở lên. Mọi lệnh chạy từ thư mục `FE/`.

```bash
# 1. Dựng hệ thống thật cho kiểm thử (PostgreSQL + Backend + nginx phục vụ giao diện).
#    Đây là stack RIÊNG, cổng lệch hẳn stack phát triển ở docker-compose.yml gốc kho,
#    vì bộ kiểm thử xóa sạch dữ liệu trước mỗi luồng.
docker compose -f e2e/docker-compose.e2e.yml up -d --build

# 2. Cài phụ thuộc và trình duyệt (chỉ lần đầu).
npm ci
npx playwright install chromium

# 3. Chạy bảy luồng.
npx playwright test -c e2e/playwright.e2e.config.ts

# 4. Dọn.
docker compose -f e2e/docker-compose.e2e.yml down -v
```

Cổng của stack kiểm thử: giao diện `4174`, API `18080`, PostgreSQL `55433`.
Đổi được bằng biến môi trường `E2E_BASE_URL`, `E2E_API_URL`.

Xem báo cáo HTML của lần chạy gần nhất:

```bash
npx playwright show-report e2e/.artifacts/bao-cao
```

## Đóng băng thời gian

Gần như mọi quy tắc trong hệ thống phụ thuộc "hôm nay" (QT3, QT3a, QT8, QT11).
Kế hoạch kiểm thử vì vậy bắt buộc ép ngày ở **cả hai đầu**, cùng một mốc:

| Đầu | Cách ép | Nơi đặt |
|---|---|---|
| Backend | biến môi trường `HUYHIEUDANG_TEST_TODAY` (T-FIX-4) | `docker-compose.e2e.yml` |
| Trình duyệt | `page.clock.install` + `resume()` (T-FIX-5) | `fixtures/clock.ts` |

Mốc mặc định là **T0 = 19/09/2026**, đổi được bằng `E2E_TODAY`:

```bash
E2E_TODAY=2026-10-15 npx playwright test -c e2e/playwright.e2e.config.ts
```

> **Backend hiện CHƯA đọc `HUYHIEUDANG_TEST_TODAY`** — lỗi `QC-T28-01`
> (cùng gốc với `QC-T27-01`, ca `A903b` trong `BE/tests/HuyHieuDang.Web.QcIntegrationTests`).
> Vì vậy ca `E0-01` đang để `Skip`, và hai ca `E-903` / `E-904` (chạy lại ở mốc
> T1, T2) chưa chạy được.
>
> Trong lúc chờ, bộ kiểm thử vẫn tất định nhờ hai điều:
>
> 1. `global-setup.ts` in rõ ngày máy chủ đang dùng ở đầu mỗi lần chạy.
> 2. Ca `E0-02` bắt buộc ngày máy chủ nằm trong **khoảng an toàn
>    11/09/2026 – 29/09/2026** — khoảng mà mọi con số của bộ dữ liệu biên còn
>    nguyên (ngày tròn mốc gần nhất hai bên là 10/09 và 30/09). Ra ngoài khoảng
>    đó thì dừng ngay với một câu giải thích, thay vì để sáu luồng đỏ vì lý do khác.
>
> Chỉ đúng **hai** giá trị phải suy theo ngày máy chủ báo về thay vì viết cứng:
> số ngày đếm ngược tới Đợt 7/11, và dòng lỗi "ngày ở tương lai" của
> `loi-4-dong.xlsx` (ngày 20/09/2026). Cả hai được chú thích tại chỗ trong
> `fixtures/clock.ts`.

> **Hai luồng `e2` và `e6` đang đỏ** — lỗi `QC-T54-02`, không liên quan đợt vắt
> năm. Cả hai chờ nút tên `Xóa N đã chọn`, trong khi màn Đảng viên từ T33
> (`27239da`) dùng nút thùng rác đỏ mang `aria-label="Xóa người đã chọn"`. Mã sản
> phẩm không sai; sửa thì chỉ đổi cách định vị nút trong hai tệp luồng, và đang
> chờ CEO phân người.

## Nguồn số liệu mong đợi

Bộ kiểm thử **không tự tính lại con số nghiệp vụ nào**. Mọi khẳng định đều so với
`tests/fixtures/data/expected.json` — kết quả do `tests/fixtures/qt_reference.py`
tính ra từ QT1–QT11. Mã nguồn cho ra số khác nghĩa là một trong hai bên sai, và
không được sửa fixture cho khớp mã nguồn (xem `tests/fixtures/README.md`).

## Cấu trúc

```
FE/e2e/
├── docker-compose.e2e.yml      Stack riêng: postgres + be + fe
├── playwright.e2e.config.ts    Cấu hình sáu luồng (1440x900, vi-VN, Asia/Ho_Chi_Minh)
├── global-setup.ts             Chờ hệ thống lên, in mốc thời gian, dựng tệp 11 MB
├── fixtures/
│   ├── env.ts        Cổng, địa chỉ, tài khoản, đường dẫn bộ dữ liệu
│   ├── expected.ts   Cửa đọc duy nhất vào expected.json
│   ├── clock.ts      Đóng băng thời gian, khoảng an toàn, tính ngày
│   ├── api.ts        Dọn kho và gieo dữ liệu nền qua REST
│   ├── app.ts        Fixture Playwright, định vị phần tử, chỉ số cột bảng
│   ├── pickers.ts    Thao tác với ô chọn ngày của Ant Design
│   ├── download.ts   Bắt tệp tải về rồi mở ra đọc
│   └── xlsx.ts       Trình đọc .xlsx tối giản, chỉ dùng thư viện chuẩn Node
├── scripts/
│   └── chup-anh-e7.mjs   Chụp bốn ảnh bằng chứng của luồng E2E-7 (T54)
└── specs/            Bảy luồng + ca tiền đề + ca về chính bộ kiểm thử
```

`fixtures/xlsx.ts` tự giải nén ZIP và đọc XML vì `FE/package.json` không có gói đọc
Excel nào, và T28 không được phép thêm phụ thuộc vào đó.

## Quy ước khi viết thêm ca

- **Chờ theo điều kiện, không chờ một quãng cố định.** Ca `E-905` quét mã và chặn
  `waitForTimeout`.
- **Mỗi luồng tự dựng trạng thái đầu** bằng `api.seedBaseline()` hoặc
  `api.resetAll()` ở dòng đầu tiên. Ca `E-906` kiểm điều này, nhờ vậy sáu luồng
  chạy được theo thứ tự bất kỳ và chạy lại nhiều lần cho cùng kết quả.
- **Việc nghiệp vụ làm qua giao diện**, lớp `api.ts` chỉ lo dọn dẹp, gieo nền và
  đối chiếu số liệu máy chủ với số trên màn hình.
- **Định vị theo `aria-label` hoặc vai trò**, không theo chữ in trên nút, để màn
  hình đổi câu chữ thì ca kiểm thử không vỡ.
