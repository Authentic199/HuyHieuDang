| | |
|---|---|
| Dự án | HuyHieuDang — Hệ thống hỗ trợ xét trao Huy hiệu Đảng |
| Tài liệu | Kế hoạch kiểm thử (T25) |
| Phiên bản | 1.0 — 19/09/2026 |
| Chủ sở hữu | QC |
| Nguồn sự thật | `docs/2026-09-17-huyhieudang-business-design.md` v1.1 — mục 3 (QT1–QT11), mục 4 (UC-xx) |
| Bộ dữ liệu | `tests/fixtures/` — xem `tests/fixtures/README.md` |

Tài liệu này viết **trước khi có mã nguồn**. Backend và Frontend đọc nó để biết mình
sẽ bị kiểm thử bằng gì, và biết những ràng buộc kỹ thuật bắt buộc (mục 2 và mục 3).

Mỗi ca kiểm thử đều truy vết về một quy tắc **QT** hoặc một use case **UC**. Ca nào
không truy vết được thì không nằm trong kế hoạch này.

---

## 1. Phạm vi và ba tầng

| Tầng | Mã ca | Chạy bằng | Cần gì | Task |
|---|---|---|---|---|
| Logic thuần | `U-xxx` | xUnit, không DB, không HTTP | service tính mốc | T26 |
| Tích hợp API | `A-xxx` | xUnit + `WebApplicationFactory` + PostgreSQL thật | API đã lên | T27 |
| Giao diện & E2E | `E1`–`E6` | Playwright, trình duyệt thật | FE nối API thật | T28 |

Ba tầng không thay thế nhau. Một quy tắc QT được phủ ở tầng logic **không** miễn cho
nó khỏi tầng API: tính đúng trong service mà truy vấn sai điều kiện hoặc phân trang
cắt mất người thì kết quả người dùng thấy vẫn sai.

**Ngoài phạm vi kiểm thử** (vì ngoài phạm vi v1): quản lý user/role, đánh dấu đã trao,
chốt đợt, trừ tuổi đảng gián đoạn, trao sớm, truy tặng, in tờ trình, chống trùng khi import.

---

## 2. Cố định thời gian (T-FIX) — ràng buộc bắt buộc với Backend và Frontend

Gần như mọi quy tắc trong hệ thống này phụ thuộc "hôm nay": QT3 tuổi đảng, QT3a mốc kế
tiếp, QT8 đợt sắp tới, QT11 trạng thái đợt, ràng buộc "Ngày chính thức ≤ hôm nay". Nếu
kiểm thử lấy ngày thật của máy thì bộ kiểm thử sẽ tự đổi màu theo ngày chạy và mất giá
trị. Quyết định dưới đây là **bắt buộc**, không phải gợi ý.

### T-FIX-1 · Backend: không gọi `DateTime.Now` ở bất cứ đâu trong logic nghiệp vụ

Thêm vào `Core` một abstraction thời gian, ví dụ:

```csharp
public interface IDateTimeProvider
{
    DateTimeOffset Now { get; }   // đã quy về múi giờ Asia/Ho_Chi_Minh
    DateOnly Today { get; }
}
```

- Service tính mốc tuổi đảng là **service thuần**: nhận `DateOnly today` qua **tham số**,
  không tiêm `IDateTimeProvider` vào nó. Tầng trên (handler/controller) lấy `Today` từ
  provider rồi truyền xuống. Nhờ vậy unit test không cần mock gì cả.
- Mọi chỗ khác cần "hôm nay" thì tiêm `IDateTimeProvider`, không dùng `DateTime.Now`,
  `DateTime.UtcNow`, `DateTimeOffset.Now`.
- QC sẽ chạy một ca kiểm thử tĩnh (`A-901`) grep toàn bộ `src/` để chặn `DateTime.Now`
  và `DateTime.Today` ngoài lớp provider. Vi phạm là **lỗi chặn**.

### T-FIX-2 · Múi giờ

`Today` phải tính theo `Asia/Ho_Chi_Minh`, không theo múi giờ mặc định của tiến trình.
Container mặc định chạy UTC; lúc 00:30 giờ Việt Nam thì UTC vẫn là **ngày hôm trước**,
và mọi ca biên "tròn mốc đúng ngày Từ/Đến" sẽ sai một ngày.

- `docker-compose.yml` phải đặt `TZ=Asia/Ho_Chi_Minh` cho service BE.
- Provider vẫn phải tự quy đổi, không dựa vào `TZ` của môi trường — ca `A-902` kiểm
  bằng cách chạy tiến trình ở `TZ=UTC` và yêu cầu kết quả không đổi.

### T-FIX-3 · Kiểm thử tích hợp: thay provider trong host kiểm thử

`WebApplicationFactory` thay `IDateTimeProvider` bằng bản cố định:

```csharp
builder.ConfigureTestServices(s =>
    s.AddSingleton<IDateTimeProvider>(new FixedDateTimeProvider(new DateOnly(2026, 9, 19))));
```

Ca cần T1 hoặc T2 thì dựng factory riêng với giá trị tương ứng. Không dùng biến môi
trường cho tầng này.

### T-FIX-4 · Chạy thật cho E2E: biến môi trường, chỉ ngoài Production

Playwright cần một tiến trình BE thật. BE đọc biến môi trường:

```
HUYHIEUDANG_TEST_TODAY=2026-09-19
```

Ràng buộc bắt buộc:
- Chỉ đọc khi `ASPNETCORE_ENVIRONMENT != "Production"`.
- Nếu biến được đặt trong Production: **bỏ qua** và ghi log cảnh báo mức `Warning`.
  Ca `A-903` kiểm điều này. Một hệ thống sản phẩm cho phép bên ngoài dịch chuyển "hôm
  nay" là lỗi bảo mật, không phải tiện ích kiểm thử.
- Khi biến có hiệu lực, BE ghi một dòng log mức `Warning` ở lúc khởi động nêu rõ ngày
  đang bị ép — để không ai lặng lẽ chạy demo với ngày giả.

### T-FIX-5 · Frontend: đóng băng cả đồng hồ trình duyệt

Header hiện "hôm nay", QT11 tính "còn N ngày", một số nhãn ngữ cảnh tính ở phía client.
Ép ngày ở BE là chưa đủ.

```ts
// playwright.config.ts
use: { timezoneId: 'Asia/Ho_Chi_Minh', locale: 'vi-VN' }

// trong fixture, trước mỗi test
await page.clock.install({ time: new Date('2026-09-19T08:00:00+07:00') });
```

BE và FE phải cùng một mốc trong một lần chạy: `HUYHIEUDANG_TEST_TODAY` và
`page.clock.install` luôn đi cặp. Lệch nhau sẽ sinh ra lỗi giả rất khó truy.

Frontend **không** được tự tính lại tuổi đảng, mốc kế tiếp, trạng thái đợt hay số ngày
còn lại từ `new Date()`. Các giá trị đó do API trả về. FE chỉ dùng ngày của trình duyệt
để hiển thị "Hôm nay dd/MM/yyyy" trên header. Ca `E1-12` và `E3-05` kiểm điều này bằng
cách so số liệu trên UI với số liệu API.

### T-FIX-6 · Dữ liệu là ngày tuyệt đối

Mọi ngày trong `tests/fixtures/` là ngày dương lịch cố định, không phải "hôm nay − N năm".
Không được sinh fixture theo ngày chạy.

### Ba mốc thời gian dùng trong kế hoạch

| Mốc | Giá trị | Vì sao cần |
|---|---|---|
| **T0** | `2026-09-19` | Mặc định. Đợt sắp tới = Đợt 7/11, "Sắp tới · 12 ngày" |
| **T1** | `2026-10-15` | Hôm nay nằm trong Đợt 7/11 → "Đang diễn ra" (QT11) |
| **T2** | `2026-12-01` | Mọi đợt 2026 đã qua → đợt sắp tới là Đợt 3/2 của **2027** (QT8) |

---

## 3. Bộ dữ liệu biên

Chi tiết trong `tests/fixtures/README.md`. Tóm tắt những gì kế hoạch này dựa vào:

- **Bộ lõi**: 32 đảng viên, mỗi người phục vụ một ca biên cụ thể, mã `B01`–`V01`.
- **Bộ lớn**: 1200 đảng viên chỉ dùng cho phân trang/tìm kiếm/hiệu năng. Ngày chính thức
  2015–2020 nên **không** làm lệch con số đủ điều kiện của bộ lõi ở các năm 2025–2030.
- **Đợt chính**: Đợt 3/2 (15/01–05/03) · Đợt 19/5 (01/05–31/05) · Đợt 2/9 (15/08–10/09) ·
  Đợt 7/11 (01/10–07/11). Cố ý chừa 5 khoảng trống.
- **Kết quả mong đợi**: `tests/fixtures/data/expected.json`, do
  `tests/fixtures/qt_reference.py` tính ra và được tự kiểm định bằng assert.

Con số chốt tại T0, cài đặt 30/90/5, chỉ nạp bộ lõi:

| | |
|---|---|
| Đợt 3/2 · 2026 | 5 người (mốc 30: 4, mốc 35: 1) |
| Đợt 19/5 · 2026 | 5 người (mốc 30: 3, mốc 35: 1, mốc 90: 1) |
| Đợt 2/9 · 2026 | 4 người (mốc 30: 4) |
| Đợt 7/11 · 2026 | 6 người (mốc 30: 3, 35: 1, 40: 1, 45: 1) |
| Chưa thuộc đợt nào · 2026 | 7 người (badge = 7) |

---

## 4. Tầng 1 — Kiểm thử logic (T26, `U-xxx`)

Chạy `cd BE && dotnet test`. Không DB, không HTTP. Mỗi ca ghi rõ dữ liệu vào và kết
quả mong đợi; dữ liệu lấy từ `tests/fixtures/data/members-core.json`.

### 4.1 QT1 — Dãy mốc huy hiệu

| Ca | Vào | Mong đợi |
|---|---|---|
| U-101 | 30 / 90 / 5 | `[30,35,40,45,50,55,60,65,70,75,80,85,90]`, 13 mốc |
| U-102 | 30 / 90 / 10 | `[30,40,50,60,70,80,90]`, 7 mốc |
| U-103 | 30 / 30 / 5 | `[30]` — Bắt đầu bằng Kết thúc |
| U-104 | 30 / 90 / 61 | `[30]` — bước nhảy vượt khoảng, chỉ còn mốc đầu |
| U-105 | 30 / 92 / 5 | `[30…90]` — dừng khi **vượt** Kết thúc, không lấy 95 |
| U-106 | 30 / 95 / 5 | `[30…95]` — 95 đúng bằng Kết thúc thì phải có |
| U-107 | Bắt đầu 0, hoặc âm | Từ chối, không trả dãy rỗng lặng lẽ |
| U-108 | Bước 0 hoặc âm | Từ chối (QT1: Bước ≥ 1) |
| U-109 | Bắt đầu 90, Kết thúc 30 | Từ chối (QT1: Bắt đầu ≤ Kết thúc) |
| U-110 | 1 / 100 / 1 | 100 mốc, không tràn, không vòng lặp vô hạn |

### 4.2 QT2 — Ngày tròn mốc, đặc biệt 29/02

| Ca | Vào | Mong đợi |
|---|---|---|
| U-201 | D = 01/10/1996, N = 30 | 01/10/2026 |
| U-202 | D = 29/02/1996, N = 30 → 2026 **không nhuận** | **28/02/2026** |
| U-203 | D = 29/02/1988, N = 40 → 2028 **nhuận** | **29/02/2028**, giữ nguyên 29/02 |
| U-204 | D = 29/02/1996, N = 4 → 2000 (nhuận, chia hết 400) | 29/02/2000 |
| U-205 | D = 29/02/**1896**, N = 4 → 1900 (chia hết 100, **không** nhuận) | **28/02/1900** — kiểm quy tắc nhuận đúng chuẩn Gregorian, không chỉ "chia hết 4" |
| U-206 | D = 28/02/1996, N = 32 → 2028 nhuận | **28/02/2028**, không được nhảy sang 29/02 |
| U-207 | D = 31/01/1996, N = 30 | 31/01/2026 — không bị lỗi "tháng 2 không có ngày 31" |
| U-208 | D = 29/02/1996, N = 0 | 29/02/1996 |

### 4.3 QT3 — Tuổi đảng

| Ca | Vào (today = T0 = 19/09/2026) | Mong đợi |
|---|---|---|
| U-301 | D = 19/09/1996 | **30** — đúng ngày kỷ niệm đã tính |
| U-302 | D = 20/09/1996 | **29** — chưa tới kỷ niệm |
| U-303 | D = 18/09/1996 | 30 |
| U-304 | D = 19/09/2026 (V01) | **0** — vào Đảng đúng hôm nay |
| U-305 | D = 01/05/1935 (M01) | **91** |
| U-306 | D = 01/05/1936 (M02) | **90** |
| U-307 | D = 29/02/1996, today = 27/02/2026 | 29 |
| U-308 | D = 29/02/1996, today = **28/02/2026** | **30** — ngày kỷ niệm đã thu về 28/02 |
| U-309 | D = 29/02/1996, today = 01/03/2026 | 30 |

### 4.4 QT3a — Mốc kế tiếp

| Ca | Vào (today = T0) | Mong đợi |
|---|---|---|
| U-351 | D = 15/01/2000 (N02), tuổi đảng 26 | mốc 30, ngày 15/01/2030 |
| U-352 | D = 01/10/1997 (N01), tuổi đảng 28 | mốc 30, ngày 01/10/2027 |
| U-353 | D = 01/05/1936 (M02), tuổi đảng **đúng 90** | **`—`** ở cả hai cột (không còn mốc nào > 90) |
| U-354 | D = 01/05/1935 (M01), tuổi đảng 91 | **`—`** ở cả hai cột |
| U-355 | D = 29/02/1988 (L02), tuổi đảng 38 | mốc 40, ngày **29/02/2028** |
| U-356 | D = 19/09/1996, tuổi đảng đúng 30 | mốc **35** — mốc kế tiếp phải **lớn hơn**, không bằng |
| U-357 | M02 với Bước = 10 | vẫn `—` — đổi cài đặt không sinh ra mốc > 90 |

### 4.5 QT4 — Đủ điều kiện trong đợt, theo năm

| Ca | Vào (Đợt 7/11 = 01/10–07/11, năm 2026) | Mong đợi |
|---|---|---|
| U-401 | B01, D = 01/10/1996 | Đủ điều kiện, mốc **30** — tròn mốc **đúng Từ ngày** |
| U-402 | B02, D = 07/11/1996 | Đủ điều kiện, mốc **30** — tròn mốc **đúng Đến ngày** |
| U-403 | B03, D = 30/09/1996 | **Không** đủ điều kiện — lệch 1 ngày trước Từ ngày |
| U-404 | B04, D = 08/11/1996 | **Không** đủ điều kiện — lệch 1 ngày sau Đến ngày |
| U-405 | B05, D = 15/01/1996, Đợt 3/2 | Đủ điều kiện, mốc 30 — biên dưới đợt khác |
| U-406 | B06, D = 05/03/1996, Đợt 3/2 | Đủ điều kiện, mốc 30 — biên trên đợt khác |
| U-407 | L01, D = 29/02/1996, Đợt 3/2 | Đủ điều kiện, mốc 30, ngày tròn mốc **28/02/2026** |
| U-408 | L02, D = 29/02/1988, Đợt 3/2, năm **2028** | Đủ điều kiện, mốc 40, ngày **29/02/2028** |
| U-409 | L02, Đợt 3/2, năm 2026 | Không đủ điều kiện — không mốc nào rơi vào 2026 |
| U-410 | M02, D = 01/05/1936, Đợt 19/5 | Đủ điều kiện, mốc **90** — mốc lớn nhất vẫn được trao |
| U-411 | N01, D = 01/10/1997, Đợt 7/11, năm 2027 | Đủ điều kiện, mốc 30 — bộ chọn năm "năm sau" |
| U-412 | N01, Đợt 7/11, năm 2026 | Không đủ điều kiện |
| U-413 | Đợt có Từ = **29/02**, năm 2026 | Ràng buộc đợt thu về **28/02/2026**; L01 đủ điều kiện (tròn mốc đúng Từ ngày đã thu) |
| U-414 | Đợt có Từ = 29/02, năm 2028 | Ràng buộc đợt giữ **29/02/2028** |
| U-415 | Đợt 1 ngày (Từ = Đến = 01/10) | B01 đủ điều kiện; B03, B04 không |
| U-416 | Cả bộ lõi, Đợt 7/11, 2026, Bước 5 | **6 người**, phân bổ mốc 30:3 · 35:1 · 40:1 · 45:1 |
| U-417 | Cả bộ lõi, Đợt 7/11, 2026, Bước **10** | **4 người**, phân bổ mốc 30:3 · 40:1 |
| U-418 | Cả bộ lõi, mọi đợt, 2026 | Không ai xuất hiện ở hai đợt cùng lúc (QT4: tối đa 1 mốc / đợt) |
| U-419 | Sắp xếp kết quả Đợt 7/11 | Theo **Mốc rồi Họ tên**: mốc 30 = Đào Văn Ân → Nguyễn Văn An → Trần Thị Bình (xem OQ-3) |

### 4.6 QT5 — Không lưu kết quả

| Ca | Mong đợi |
|---|---|
| U-501 | Gọi hàm tính hai lần với cùng dữ liệu → kết quả giống nhau, không side effect |
| U-502 | Đổi cài đặt giữa hai lần gọi → lần thứ hai đổi theo ngay, không cần "làm mới" |
| U-503 | Rà soát mô hình dữ liệu: **không tồn tại** bảng/entity lưu danh sách đủ điều kiện. Ca kiểm tra kiến trúc, không phải hành vi |

### 4.7 QT6 — Đợt trao huy hiệu

| Ca | Vào | Mong đợi |
|---|---|---|
| U-601 | Từ 01/10, Đến 07/11 | Hợp lệ |
| U-602 | Từ 01/12, Đến 28/02 | Chấp nhận — đợt vắt qua 31/12, Đến ngày thuộc năm sau |
| U-603 | Từ = Đến = 01/10 | Hợp lệ — đợt một ngày |
| U-604 | Từ 01/01, Đến 31/12 | Hợp lệ — trọn năm |
| U-605 | Từ 29/02, Đến 05/03 | Hợp lệ — 29/02 là cặp ngày/tháng hợp lệ dù năm xét không nhuận |
| U-606 | Từ 31/02, Đến 05/03 | Từ chối — ngày/tháng không tồn tại ở bất kỳ năm nào |
| U-607 | Từ 31/04 | Từ chối |
| U-608 | Tên trùng đợt đã có | Từ chối (Name duy nhất) |
| U-609 | Hai đợt chồng lấn: 01/10–07/11 và 01/11–30/11 | **Cảnh báo** nhưng **vẫn lưu**, liệt kê đúng cặp đợt |
| U-610 | Bộ 4 đợt chính | Cảnh báo chưa phủ kín, liệt kê đúng **5** khoảng trống |
| U-611 | Hai đợt phủ 01/01–30/06 và 01/07–31/12 | **Không** cảnh báo phủ kín |
| U-612 | Hai đợt liền kề 01/10–07/11 và 08/11–31/12 | Không coi là chồng lấn (kề nhau ≠ chồng lấn) |

Khối T54 — đợt vắt qua 31/12 (thêm ngày 20/09/2026). Mã bắt đầu từ U-620 vì U-610 → U-612 đã
có chủ ở trên.

| Ca | Vào | Mong đợi |
|---|---|---|
| U-620 | Đợt 01/12 – 30/11, Bước = 1 | Vẫn tối đa 1 mốc / người / đợt |
| U-621 | Đợt 02/01 – 01/01 | Phủ trọn năm: không khoảng trống, không tự chồng lấn, không vòng lặp vô hạn |
| U-622 | Đợt 01/12 – 29/02, năm kết thúc không nhuận | Đến ngày lùi về 28/02 |
| U-623 | Tròn mốc đúng 01/12 và đúng 28/02 của đợt 01/12 – 28/02 | Đủ điều kiện cả hai biên, không ai bị xếp vào "chưa thuộc đợt nào" |
| U-624a | Đợt vắt năm + đợt thường chồng lấn đầu năm | Đúng một cặp, đúng khoảng ngày dùng chung |
| U-624b | Hai đợt vắt năm | Hai đoạn dùng chung rời nhau: đầu năm và cuối năm |
| U-625 | Xóa đợt vắt năm | Khoảng trống mới phủ đúng hai đầu năm |
| U-626 | 9 bộ đợt vắt năm × 7 năm, quét từng ngày 2026–2027 | Service khớp oracle độc lập của QC về độ phủ, khoảng trống, chồng lấn, đợt sắp tới, trạng thái |

### 4.8 QT7 — Chưa thuộc đợt nào

| Ca | Vào (bộ 4 đợt chính, năm 2026) | Mong đợi |
|---|---|---|
| U-701 | B07, tròn mốc 14/01/2026 | Bị sót, nhãn **"Trước đợt đầu tiên"** |
| U-702 | B08, tròn mốc 06/03/2026 | Bị sót, nhãn **"Giữa Đợt 3/2 và Đợt 19/5"** |
| U-703 | G05, tròn mốc 01/07/2026 | Bị sót, nhãn "Giữa Đợt 19/5 và Đợt 2/9" |
| U-704 | B03, tròn mốc 30/09/2026 | Bị sót, nhãn "Giữa Đợt 2/9 và Đợt 7/11" |
| U-705 | B04, tròn mốc 08/11/2026 | Bị sót, nhãn **"Sau đợt cuối cùng"** |
| U-706 | M03, tròn mốc **90** ngày 10/06/2026 | Bị sót — người vượt mốc vẫn phải xuất hiện nếu còn mốc rơi trong năm |
| U-707 | L02 (không mốc nào trong 2026) | **Không** bị sót — không có mốc trong năm thì không tính là sót |
| U-708 | B01 (đủ điều kiện Đợt 7/11) | Không bị sót |
| U-709 | Cả bộ lõi, 2026 | **7 người** bị sót |
| U-710 | Cả bộ lõi, 2026, Bước = **10** | **6 người** — S04 (mốc 35) rời khỏi danh sách |
| U-711 | Bộ đợt phủ kín cả năm | **0 người** bị sót |
| U-712 | **Không có đợt nào** | **27** người — mọi người có mốc rơi trong 2026 đều bị sót (20 đủ điều kiện + 7 bị sót; 5 người còn lại không có mốc nào trong 2026) |

### 4.9 QT8 — Đợt sắp tới

| Ca | Vào | Mong đợi |
|---|---|---|
| U-801 | T0 = 19/09/2026, bộ 4 đợt | **Đợt 7/11 / 2026**, `Đến ≥ hôm nay` và `Từ` nhỏ nhất |
| U-802 | T1 = 15/10/2026 | **Đợt 7/11 / 2026** — hôm nay nằm trong đợt thì chính là đợt đó |
| U-803 | T2 = 01/12/2026 | **Đợt 3/2 / 2027** — mọi đợt trong năm đã qua → đợt sớm nhất năm sau |
| U-804 | hôm nay = 07/11/2026 (đúng Đến ngày) | Đợt 7/11 / 2026 — `Đến ≥ hôm nay` là **lớn hơn hoặc bằng** |
| U-805 | hôm nay = 08/11/2026 | Đợt 3/2 / 2027 |
| U-806 | hôm nay = 01/10/2026 (đúng Từ ngày) | Đợt 7/11 / 2026 |
| U-807 | Không có đợt nào | Trả **không có** — tầng trên hiện "Chưa cài đợt trao huy hiệu" |
| U-808 | Hai đợt cùng Từ ngày | Chọn tất định (không phụ thuộc thứ tự nạp) — xem OQ-9 |
| U-809 | Đợt sắp tới là 29/02, hôm nay 01/01/2027 (không nhuận) | Ràng buộc thu về 28/02/2027, số ngày còn lại tính theo ngày đã thu |

### 4.10 QT9 — Import Excel (phần thuần logic: đọc và kiểm dòng)

| Ca | Vào | Mong đợi |
|---|---|---|
| U-901 | `core-hop-le.xlsx` | 32 dòng hợp lệ, 0 lỗi |
| U-902 | `core-hop-le-ngay-kieu-date.xlsx` | 32 dòng hợp lệ — ô ngày **kiểu ngày** của Excel phải đọc được như ô chuỗi |
| U-903 | `loi-4-dong.xlsx` | 6 hợp lệ, 4 lỗi ở dòng Excel **8, 9, 10, 11**, lý do đúng từng dòng |
| U-904 | Dòng thiếu Họ tên | Lỗi "Thiếu Họ tên" |
| U-905 | Dòng thiếu Ngày chính thức | Lỗi "Thiếu Ngày vào Đảng chính thức" |
| U-906 | Ngày `1996-10-01` | Lỗi sai định dạng (cần `dd/MM/yyyy`) |
| U-907 | Ngày chính thức 20/09/2026 (T0 + 1) | Lỗi "ở tương lai" — biên sát nhất |
| U-908 | Ngày chính thức 19/09/2026 (đúng T0) | **Hợp lệ** — biên trên của `≤ hôm nay` |
| U-909 | Giới tính `Khác` | Lỗi giới tính |
| U-910 | Giới tính để trống | **Hợp lệ** — giới tính không bắt buộc |
| U-911 | Ngày sinh sau Ngày chính thức | Lỗi |
| U-912 | Ngày sinh = Ngày chính thức | Xem OQ-10 |
| U-913 | Ngày sinh để trống | Hợp lệ |
| U-914 | Ngày sinh `31/02/1974` | Xem OQ-1 |
| U-915 | Dòng nhiều lỗi cùng lúc | Xem OQ-2 |
| U-916 | Dòng trắng hoàn toàn ở giữa file | Bỏ qua, không tính là lỗi, không tính là người |
| U-917 | `loi-sai-cot.xlsx` | Lỗi **cấp file**, dừng ngay, không trả danh sách dòng |
| U-918 | `loi-rong.xlsx` | Lỗi cấp file "file rỗng" |
| U-919 | `loi-chi-co-tieu-de.xlsx` | Lỗi cấp file — thông báo phải khác `loi-rong.xlsx` (OQ-7) |
| U-920 | `loi-khong-phai-xlsx.xlsx` | Lỗi định dạng, **không** ném exception chưa bắt |
| U-921 | `bien-chuan-hoa.xlsx` | Kết quả chỉ chốt sau OQ-4, OQ-5, OQ-6 |

### 4.11 QT10, QT11

| Ca | Vào | Mong đợi |
|---|---|---|
| U-1001 | Xóa 1 đảng viên | Xóa hẳn khỏi kho, không cờ soft-delete |
| U-1002 | Xóa 1 đợt | Xóa hẳn; đảng viên **không** bị ảnh hưởng |
| U-1101 | Đợt 3/2 tại T0 | **Đã qua**, không có số ngày |
| U-1102 | Đợt 7/11 tại T0 | **Sắp tới**, còn **12** ngày |
| U-1103 | Đợt 7/11 tại T1 (15/10) | **Đang diễn ra**, không có số ngày |
| U-1104 | Đợt 7/11 tại 01/10/2026 (đúng Từ) | **Đang diễn ra** — biên `Từ ≤ hôm nay` |
| U-1105 | Đợt 7/11 tại 07/11/2026 (đúng Đến) | **Đang diễn ra** — biên `hôm nay ≤ Đến` |
| U-1106 | Đợt 7/11 tại 08/11/2026 | **Đã qua** |
| U-1107 | Đợt 7/11 tại 30/09/2026 | Sắp tới, còn **1** ngày |

### 4.12 Hiệu năng (yêu cầu phi chức năng mục 7)

| Ca | Mong đợi |
|---|---|
| U-1201 | Tính đủ điều kiện cho 1 đợt / 1 năm với **10.000** đảng viên: **< 1 giây** ở tầng logic thuần |

---

## 5. Tầng 2 — Kiểm thử tích hợp API (T27, `A-xxx`)

Chạy trên PostgreSQL thật qua `WebApplicationFactory` + Testcontainers. Mỗi ca tự nạp
dữ liệu và tự dọn (Respawn). `IDateTimeProvider` bị thay theo T-FIX-3.

Mục tiêu bao phủ: **mọi endpoint trong `docs/api-contract.md` đều được chạm tới**. Bảng
dưới viết theo chức năng; khi contract chốt (T05), QC rà lại và bổ sung ca cho endpoint
nào chưa có.

### 5.1 Phân quyền — gọi API khi chưa đăng nhập

| Ca | Vào | Mong đợi |
|---|---|---|
| A-001 | Gọi **mọi** endpoint nghiệp vụ không kèm JWT | **401**, thân phản hồi không lộ dữ liệu |
| A-002 | JWT sai chữ ký | 401 |
| A-003 | JWT đã hết hạn | 401 |
| A-004 | JWT thiếu `Bearer` | 401 |
| A-005 | Đăng nhập sai mật khẩu | 401, thông báo **chung** "Sai tài khoản hoặc mật khẩu" — không được phân biệt "sai mật khẩu" với "không có tài khoản" (UC-00) |
| A-006 | Đăng nhập đúng | 200 + token; token dùng được ngay cho endpoint khác |
| A-007 | Liệt kê route: không endpoint nghiệp vụ nào `[AllowAnonymous]` ngoài `login` | Ca kiểm tra tĩnh |

### 5.2 Đảng viên (UC-20 → UC-23)

| Ca | Vào | Mong đợi |
|---|---|---|
| A-101 | Nạp bộ lõi + bộ lớn, lấy danh sách | Tổng **1232** |
| A-102 | `pageSize=20, page=1` | 20 dòng, tổng 1232, **62** trang |
| A-103 | `pageSize=20, page=62` | **12** dòng (trang cuối) |
| A-104 | `pageSize=20, page=63` | Trang rỗng, **không** lỗi 500 |
| A-105 | `pageSize=50` / `100` | 25 / 13 trang; dòng trang cuối 32 / 32 |
| A-106 | `pageSize=0`, âm, hoặc rất lớn (1e9) | Bị chặn hoặc kẹp về mức trần; không treo, không OOM |
| A-107 | Ghép mọi trang lại | Đúng 1232 người **không trùng, không thiếu** — bắt lỗi phân trang thiếu sắp xếp tất định |
| A-108 | Tìm `Nguyễn` | **79** người |
| A-109 | Tìm `nguyễn` viết thường | 79 — xem OQ-8 |
| A-110 | Tìm `Đào Văn Ân` | 1 người |
| A-111 | Tìm chuỗi không tồn tại | 0, danh sách rỗng, không lỗi |
| A-112 | Tìm với `%`, `_`, `'`, `\` | Trả kết quả bình thường — chuỗi được tham số hoá, không phải nối SQL |
| A-113 | Lọc Giới tính = Nam / Nữ / trống | 592 / 590 / 50 |
| A-114 | Sắp xếp theo từng cột, hai chiều | Đúng chiều; ô trống không làm sai thứ tự |
| A-115 | Sắp xếp theo Họ tên | Xem OQ-3 |
| A-116 | Thêm tay hợp lệ | 201/200, tổng tăng đúng 1 |
| A-117 | Thêm tay thiếu Họ tên | 400, thông báo tiếng Việt |
| A-118 | Thêm tay Ngày chính thức = hôm nay (T0) | Hợp lệ |
| A-119 | Thêm tay Ngày chính thức = T0 + 1 ngày | 400 |
| A-120 | Sửa Ngày chính thức | Tuổi đảng, Mốc kế tiếp, các danh sách đủ điều kiện đổi theo **ngay** (QT5) |
| A-121 | Xóa 1 người | Tổng giảm đúng 1 |
| A-122 | Xóa nhiều người một lần | Tổng giảm đúng số lượng; xóa hẳn |
| A-123 | Xóa id không tồn tại | Lỗi nghiệp vụ rõ ràng, không 500 |
| A-124 | Xóa danh sách có id trùng nhau | Không đếm trùng, không lỗi |
| A-125 | Tổng số + "tuổi đảng tính đến hôm nay" trên dòng tóm tắt | Khớp T0 |

### 5.3 Đợt trao huy hiệu (UC-30 → UC-34, QT6, QT11)

| Ca | Vào | Mong đợi |
|---|---|---|
| A-201 | Nạp 4 đợt chính, lấy danh sách | 4 đợt, **sắp theo Từ ngày** |
| A-202 | Trạng thái năm nay tại T0 | Đã qua · Đã qua · Đã qua · Sắp tới 12 ngày |
| A-203 | Cùng dữ liệu tại T1 | Đợt 7/11 = Đang diễn ra |
| A-204 | Số người đủ điều kiện năm nay trên từng dòng | 5 · 5 · 4 · 6 |
| A-205 | Tạo đợt Từ > Đến | 200 — đợt vắt qua 31/12, spansNextYear = true |
| A-206 | Tạo đợt trùng tên | 400 |
| A-207 | Tạo đợt ngày/tháng không tồn tại (31/02) | 400 |
| A-208 | Tạo đợt 29/02–05/03 | Hợp lệ |
| A-209 | Tạo hai đợt chồng lấn | **Lưu được** + cảnh báo nêu đúng cặp đợt (QT6) |
| A-210 | 4 đợt chính | Cảnh báo chưa phủ kín, liệt kê đúng 5 khoảng trống |
| A-211 | Bộ đợt phủ kín | Không cảnh báo phủ kín |
| A-212 | Sửa Đến ngày Đợt 2/9 10/09 → 30/09 | Đợt 2/9 · 2026 lên **5** người, "chưa thuộc đợt nào" còn **6** — không cần thao tác nào khác (QT5, QT6) |
| A-213 | Xóa đợt | Đợt mất; số đảng viên **không** đổi |
| A-214 | Đủ điều kiện Đợt 7/11 · 2026 | 6 người, phân bổ 30:3 · 35:1 · 40:1 · 45:1, sắp theo Mốc rồi Họ tên |
| A-215 | Đủ điều kiện Đợt 7/11 · **2027** | Có Đặng Thị Quỳnh (N01) mốc 30 |
| A-216 | Đủ điều kiện Đợt 3/2 · **2028** | Có Dương Thị Lan (L02) mốc 40, ngày tròn mốc **29/02/2028** |
| A-217 | Đủ điều kiện Đợt 3/2 · 2026 | Có Ngô Văn Khánh (L01), ngày tròn mốc **28/02/2026** |
| A-218 | Năm = 1900, 2100, 0, âm, chữ | Xử lý tất định (kẹp hoặc 400), không 500 |
| A-219 | Đủ điều kiện của đợt không tồn tại | Lỗi nghiệp vụ rõ ràng |
| A-220 | Nạp thêm bộ lớn rồi lấy lại A-214 | **Vẫn 6 người** — bộ lớn không gây nhiễu |

Khối T54 — đợt vắt qua 31/12. Mã bắt đầu từ A-221 vì A-210 → A-213 đã có chủ ở trên.

| Ca | Vào | Mong đợi |
|---|---|---|
| A-221 | `GET /AwardPeriods?year=Y` với đợt 01/12 – 28/02 | `toDate` thuộc `Y+1`, `spansNextYear = true`, `coverage.segments` có hai đoạn cùng `periodId`, không cảnh báo chồng lấn |
| A-222 | `GET /Eligibility/Unassigned?year=Y` | Người tròn mốc 20/01 **không** bị xếp vào "chưa thuộc đợt nào"; badge bằng số dòng danh sách |
| A-223 | `GET /Dashboard` ngày 15/01/2027 | `upcomingPeriod.year = 2026`, `status = Ongoing`, tổng người khớp danh sách đủ điều kiện |
| A-224 | Xuất Excel đợt vắt năm | Tên file đúng quy ước, dòng tiêu đề ghi 01/12/Y – 28/02/(Y+1), số dòng khớp màn hình |

### 5.4 Dashboard (UC-10 → UC-13, QT8)

| Ca | Vào | Mong đợi |
|---|---|---|
| A-301 | Dashboard tại T0 | Đợt sắp tới = Đợt 7/11 / **2026**, khoảng ngày 01/10/2026 – 07/11/2026, còn 12 ngày, 6 người, phân bổ 30:3 · 35:1 · 40:1 · 45:1 |
| A-302 | Dashboard tại T1 | Đợt 7/11, "đang diễn ra" |
| A-303 | Dashboard tại T2 | Đợt **3/2 / 2027**, khoảng 15/01/2027 – 05/03/2027 |
| A-304 | Không có đợt nào | Báo "chưa cài đợt", **không** 500, không trả đợt rỗng giả |
| A-305 | Không có đảng viên nào | Cảnh báo "chưa có đảng viên", bảng rỗng |
| A-306 | Kho trống hoàn toàn | Cả hai cảnh báo; dữ liệu cho khối hướng dẫn 3 bước (UC-13) |
| A-307 | Số người "chưa thuộc đợt nào" năm nay (badge) | **7** |
| A-308 | Cảnh báo chồng lấn / chưa phủ kín trên Dashboard | Khớp với cảnh báo ở màn Đợt |

### 5.5 Chưa thuộc đợt nào (UC-40, QT7)

| Ca | Vào | Mong đợi |
|---|---|---|
| A-401 | Năm 2026 | 7 người, mỗi người đúng nhãn khoảng trống (xem U-701…U-706) |
| A-402 | Năm 2026 sau khi đổi Bước = 10 | 6 người |
| A-403 | Năm 2027, 2025 | Khớp `expected.json` |
| A-404 | Bộ đợt phủ kín | 0 người, trạng thái trống |
| A-405 | Sắp xếp | Theo Mốc rồi Họ tên |
| A-406 | Badge = số dòng của màn này | Hai con số **luôn** khớp — cùng một nguồn tính |

### 5.6 Cài đặt (UC-50, UC-51, QT1)

| Ca | Vào | Mong đợi |
|---|---|---|
| A-501 | Kho trống, đọc cài đặt | Trả mặc định **30 / 90 / 5** |
| A-502 | Lưu 30/90/10 | Lưu được; dãy mốc đổi; **mọi** danh sách đủ điều kiện đổi theo ngay (QT5) |
| A-503 | Lưu Bắt đầu > Kết thúc | 400 |
| A-504 | Lưu Bước 0 hoặc âm | 400 |
| A-505 | Lưu giá trị không phải số / rỗng | 400 |
| A-506 | Khôi phục mặc định | Về 30/90/5 |
| A-507 | Lưu Tên đơn vị | Hiện ở header và ở dòng tiêu đề file Excel xuất ra |
| A-508 | Tên đơn vị để trống | Hợp lệ; tiêu đề Excel bỏ dòng đó |
| A-509 | Gọi lưu cài đặt hai lần liên tiếp | Vẫn đúng **một** bản ghi cài đặt |

### 5.7 Import Excel qua API (UC-24, QT9)

| Ca | Vào | Mong đợi |
|---|---|---|
| A-601 | Xem trước `core-hop-le.xlsx` | 32 hợp lệ, 0 lỗi; **chưa** ghi vào DB |
| A-602 | Nạp sau xem trước | Tổng tăng đúng 32 |
| A-603 | Xem trước `loi-4-dong.xlsx` | 6 hợp lệ · 4 lỗi, số dòng 8/9/10/11 và lý do đúng |
| A-604 | Nạp `loi-4-dong.xlsx` | Tổng tăng đúng **6** |
| A-605 | Chỉ gọi xem trước rồi bỏ | Tổng **không** đổi |
| A-606 | Nạp cùng một file **hai lần** | Tổng tăng 2× — QT9 không kiểm trùng, đây là hành vi **đúng** |
| A-607 | `loi-sai-cot.xlsx` | 400 ở bước 1, không sang xem trước |
| A-608 | `loi-rong.xlsx` | 400 "file rỗng" |
| A-609 | `loi-chi-co-tieu-de.xlsx` | 400, thông báo khác A-608 (OQ-7) |
| A-610 | `loi-khong-phai-xlsx.xlsx` | **400**, không 500, thông báo tiếng Việt |
| A-611 | `loi-dinh-dang-csv.csv` | 400 |
| A-612 | `loi-qua-10mb.xlsx` (11 MB) | 400 "vượt 10 MB"; phải bị chặn **trước khi** đọc nội dung |
| A-613 | File 9,9 MB hợp lệ | Chấp nhận — biên dưới của ràng buộc |
| A-614 | Không gửi file | 400 |
| A-615 | Gửi 2 file | Xử lý tất định, không 500 |
| A-616 | Cắt kết nối giữa lúc nạp | Không còn dữ liệu nửa vời (QT9: nạp là **một giao dịch**) |
| A-617 | Lỗi kỹ thuật giữa lúc nạp (ép lỗi ở dòng cuối) | **Rollback toàn bộ**, tổng không đổi |
| A-618 | `bulk-1200.xlsx` | 1200 dòng hợp lệ, nạp xong trong thời gian hợp lý |
| A-619 | Tải file mẫu (UC-25) | Đúng 4 cột đúng thứ tự, có dòng ví dụ, ngày `dd/MM/yyyy`; **nạp lại chính file mẫu đó phải hợp lệ** |
| A-620 | `core-hop-le-ngay-kieu-date.xlsx` | 32 hợp lệ |

### 5.8 Xuất Excel (UC-11, UC-34, UC-40)

| Ca | Vào | Mong đợi |
|---|---|---|
| A-701 | Xuất đủ điều kiện Đợt 7/11 · 2026 | Tên tệp `DuDieuKien_Dot7-11_2026.xlsx` — bỏ dấu, `/` → `-` |
| A-702 | Xuất Đợt 19/5 · 2026 | `DuDieuKien_Dot19-5_2026.xlsx` |
| A-703 | Xuất chưa thuộc đợt nào · 2026 | `ChuaThuocDot_2026.xlsx` |
| A-704 | Dòng tiêu đề | Tên đơn vị + tên đợt + khoảng ngày **đã gắn năm** + ngày xuất (= T0) |
| A-705 | Tên đơn vị để trống | Tiêu đề bỏ phần tên đơn vị, không để dòng trắng lạ |
| A-706 | Số dòng dữ liệu | Đúng **6** cho Đợt 7/11 · 2026 |
| A-707 | Ô trống (E01 không có Ngày sinh, Giới tính) | Ô **rỗng**, **không** ghi `—` |
| A-708 | Cột và thứ tự sắp | Khớp bảng đang xem |
| A-709 | Xuất khi danh sách rỗng | Vẫn ra file có tiêu đề, 0 dòng dữ liệu — không 500 |
| A-710 | Mở file bằng thư viện đọc Excel | Đọc được, đúng 1 sheet, không cảnh báo hỏng |

### 5.9 Ca kiểm tra ràng buộc kỹ thuật

| Ca | Mong đợi |
|---|---|
| A-901 | Grep `src/`: không còn `DateTime.Now` / `DateTime.Today` / `DateTimeOffset.Now` ngoài lớp provider (T-FIX-1) |
| A-902 | Chạy tiến trình với `TZ=UTC`: kết quả **không đổi** so với `TZ=Asia/Ho_Chi_Minh` (T-FIX-2) |
| A-903 | Đặt `HUYHIEUDANG_TEST_TODAY` khi `ASPNETCORE_ENVIRONMENT=Production`: **bị bỏ qua** + log `Warning` (T-FIX-4) |
| A-904 | Đủ điều kiện 1 đợt / 1 năm với 10.000 đảng viên trong DB: **< 1 giây** (mục 7 tài liệu nghiệp vụ) |
| A-905 | Mọi thông báo lỗi trả cho người dùng là **tiếng Việt**, không có stack trace, không có tên bảng/cột |

---

## 6. Tầng 3 — Kiểm thử end-to-end (T28, `E1`–`E6`; T54, `E7`)

Playwright, trình duyệt thật, BE + DB thật. Cấu hình bắt buộc theo T-FIX-5:
`timezoneId: 'Asia/Ho_Chi_Minh'`, `locale: 'vi-VN'`, `page.clock.install` ở mốc T0, và
BE chạy với `HUYHIEUDANG_TEST_TODAY` cùng mốc.

Mỗi luồng **tự dựng trạng thái đầu** (reset DB, seed đúng những gì nó cần) và chạy được
độc lập, theo thứ tự bất kỳ, chạy lại nhiều lần cho cùng kết quả.

### E2E-1 · Lần dùng đầu tiên (UC-00, UC-13, UC-50, UC-31, UC-24, UC-10, UC-11)

Trạng thái đầu: DB rỗng hoàn toàn (không đảng viên, không đợt, cài đặt chưa lưu).

| Bước | Thao tác | Kết quả mong đợi |
|---|---|---|
| E1-01 | Mở ứng dụng khi chưa đăng nhập | Chuyển về màn Đăng nhập, không lọt vào Dashboard |
| E1-02 | Đăng nhập sai | Thông báo **chung** "Sai tài khoản hoặc mật khẩu", vẫn ở màn đăng nhập |
| E1-03 | Đăng nhập đúng | Vào Dashboard |
| E1-04 | Xem Dashboard trống | Hiện khối **hướng dẫn 3 bước** (UC-13) thay cho bảng; cảnh báo "chưa có đảng viên" và "chưa có đợt"; **không** có badge trên menu "Chưa thuộc đợt nào" |
| E1-05 | Bấm bước 1 của khối hướng dẫn | Đi tới màn Cài đặt |
| E1-06 | Xem Cài đặt | Ba ô hiện **30 / 90 / 5**; xem trước dãy mốc `30, 35, … 90` và nhãn "**13 mốc**" |
| E1-07 | Gõ Bước = 10 (chưa lưu) | Xem trước đổi ngay thành 7 mốc; **Dashboard chưa đổi** vì chưa lưu |
| E1-08 | Đổi lại Bước = 5, nhập Tên đơn vị "Đảng ủy Phường Kiểm Thử", Lưu | Lưu thành công; header mọi màn hiện tên đơn vị |
| E1-09 | Sang màn Đợt, tạo 4 đợt: Đợt 3/2 (15/01–05/03), Đợt 19/5 (01/05–31/05), Đợt 2/9 (15/08–10/09), Đợt 7/11 (01/10–07/11) | Bảng 4 dòng **sắp theo Từ ngày**; trạng thái Đã qua · Đã qua · Đã qua · **Sắp tới · 12 ngày** |
| E1-10 | Xem banner phủ kín và dải độ phủ 12 tháng (UC-36) | Banner liệt kê **5** khoảng trống; dải có vạch "Hôm nay" ở **19/09** |
| E1-11 | Sang Đảng viên → Import Excel → Tải file mẫu | Tải về file 4 cột đúng thứ tự, có dòng ví dụ |
| E1-12 | Chọn `tests/fixtures/excel/core-hop-le.xlsx` → Xem trước | "Sẽ thêm **32 người mới** · 0 dòng lỗi"; có cảnh báo "hệ thống không kiểm tra trùng" |
| E1-13 | Nạp | "Đã thêm 32 người, bỏ qua 0 dòng lỗi" |
| E1-14 | Về danh sách Đảng viên | Dòng tóm tắt "**32 người**"; ô trống hiện `—`; Trịnh Văn Minh và Lý Thị Nga có Mốc kế tiếp = `—` |
| E1-15 | Về Dashboard | Thẻ đợt sắp tới = **Đợt 7/11**, khoảng **01/10/2026 – 07/11/2026**, **còn 12 ngày**, **6 người**, phân bổ "30 năm: 3 · 35 năm: 1 · 40 năm: 1 · 45 năm: 1" |
| E1-16 | Đối chiếu bảng Dashboard | Đúng **6 dòng**, sắp theo Mốc rồi Họ tên; cột Ngày tròn mốc đúng `expected.json` |
| E1-17 | Xem menu trái | Badge "Chưa thuộc đợt nào" = **7** |
| E1-18 | Xem cảnh báo Dashboard | Cảnh báo "có 7 người chưa thuộc đợt nào", bấm vào đi tới M4 |
| E1-19 | Đăng xuất rồi mở lại URL Dashboard | Bị đưa về Đăng nhập |

### E2E-2 · Import có lỗi (UC-24, UC-25, QT9)

Trạng thái đầu: đã đăng nhập, đã có 4 đợt và cài đặt; **0 đảng viên**.

| Bước | Thao tác | Kết quả mong đợi |
|---|---|---|
| E2-01 | Import Excel → bước 1 | Vùng kéo-thả; mô tả 4 cột + dòng ví dụ; ghi rõ `.xlsx` ≤ 10 MB và định dạng ngày |
| E2-02 | Tải file mẫu | File tải về khớp `mau-dang-vien.xlsx` về tiêu đề và thứ tự cột |
| E2-03 | Nạp lại đúng file mẫu vừa tải | 2 dòng hợp lệ, 0 lỗi — file mẫu của hệ thống phải tự hợp lệ |
| E2-04 | Xóa 2 người vừa nạp | Còn 0 người |
| E2-05 | Chọn `loi-4-dong.xlsx` → Xem trước | Tóm tắt "Sẽ thêm **6 người mới** · **4 dòng lỗi** bị bỏ qua"; cảnh báo không kiểm trùng |
| E2-06 | Xem tab Hợp lệ (6) / Lỗi (4) | Số trên tab khớp; chuyển tab được |
| E2-07 | Xem bảng lỗi | 4 dòng, cột Dòng · Họ tên · Ngày sinh · Giới tính · Ngày chính thức · Lý do. Dòng **8** thiếu Họ tên · **9** thiếu Ngày chính thức · **10** sai định dạng ngày · **11** ngày ở tương lai |
| E2-08 | Bấm Hủy | Về danh sách, vẫn **0 người** |
| E2-09 | Làm lại tới bước 2 rồi bấm Nạp | Bước 3: "Đã thêm **6** người, bỏ qua **4** dòng lỗi" |
| E2-10 | Về danh sách | Đúng **6 người** — tăng đúng bằng số dòng hợp lệ |
| E2-11 | Nạp lại **cùng file** lần nữa | Thành **12 người** — QT9 không kiểm trùng; đây là hành vi đúng và cảnh báo phải nói trước |
| E2-12 | Chọn `loi-sai-cot.xlsx` | Bị chặn **ngay bước 1**, thông báo tiếng Việt, không sang bước 2 |
| E2-13 | Chọn `loi-khong-phai-xlsx.xlsx` | Bị chặn, thông báo tiếng Việt, **không** hiện lỗi máy chủ |
| E2-14 | Chọn `loi-qua-10mb.xlsx` | Bị chặn với thông báo vượt 10 MB |
| E2-15 | Chọn `loi-rong.xlsx` | Bị chặn với thông báo file rỗng |

### E2E-3 · Đổi cài đặt lan truyền (UC-50, QT1, QT5)

Trạng thái đầu: 4 đợt, cài đặt 30/90/5, đã nạp **bộ lõi 32 người**, đang ở T0.

| Bước | Thao tác | Kết quả mong đợi |
|---|---|---|
| E3-01 | Ghi lại trạng thái đầu | Dashboard **6 người**; chi tiết Đợt 7/11 · 2026 **6 người**; badge **7** |
| E3-02 | Cài đặt → Bước 5 → **10** (chưa lưu) | Xem trước `30, 40, 50, 60, 70, 80, 90` và "**7 mốc**"; dòng nhắc về ảnh hưởng tức thì |
| E3-03 | Chưa lưu, quay lại Dashboard | **Vẫn 6 người** — chưa lưu thì chưa đổi |
| E3-04 | Về Cài đặt, bấm Lưu | Lưu thành công |
| E3-05 | Mở chi tiết Đợt 7/11 · 2026 | Còn **4 người**; **không còn** Hồ Thị Vân (mốc 35) và Trần Thị Yến (mốc 45); vẫn có Nguyễn Văn Xuân (mốc 40) |
| E3-06 | Về Dashboard **mà không làm gì thêm** | **4 người**, phân bổ "30 năm: 3 · 40 năm: 1" |
| E3-07 | Xem badge menu | Còn **6** — Ngô Thị Cẩm (mốc 35) rời khỏi màn M4 |
| E3-08 | Mở M4 năm 2026 | **6 dòng**, không còn Ngô Thị Cẩm |
| E3-09 | Danh sách Đảng viên | Cột Mốc kế tiếp đổi theo dãy mốc mới; Lý Thị Nga vẫn `—` |
| E3-10 | Bấm "Khôi phục mặc định 30 / 90 / 5", Lưu | Mọi con số trở về đúng E3-01 |

### E2E-4 · Sửa đợt lan truyền (UC-32, QT5, QT6)

Trạng thái đầu: giống E2E-3 (4 đợt, 30/90/5, bộ lõi 32 người, T0).

| Bước | Thao tác | Kết quả mong đợi |
|---|---|---|
| E4-01 | Mở M4 năm 2026 | **7 người**; Lê Văn Cường có Khoảng trống = "Giữa Đợt 2/9 và Đợt 7/11", Ngày tròn mốc **30/09/2026** |
| E4-02 | Badge menu | **7** |
| E4-03 | Bấm khối gợi ý "Nới Đến ngày của đợt trước…" | Đi tới màn Đợt |
| E4-04 | Sửa Đợt 2/9: Đến ngày 10/09 → **30/09** | Modal có dòng nhắc "Thay đổi áp dụng ngay cho năm hiện tại và các năm sau"; lưu thành công |
| E4-05 | Xem bảng đợt | Đợt 2/9 hiện **15/08 – 30/09**; Số người đủ điều kiện năm nay = **5** (trước là 4) |
| E4-06 | Mở chi tiết Đợt 2/9 · 2026 | **5 người**, có **Lê Văn Cường** mốc 30 ngày 30/09/2026 |
| E4-07 | Badge menu | Còn **6** |
| E4-08 | Mở M4 năm 2026 | **6 dòng**, không còn Lê Văn Cường; khoảng trống "Giữa Đợt 2/9 và Đợt 7/11" **biến mất** khỏi banner |
| E4-09 | Banner phủ kín ở màn Đợt | Còn **4** khoảng trống |
| E4-10 | Dashboard | Đợt sắp tới **vẫn** là Đợt 7/11, vẫn 6 người |
| E4-11 | Sửa Đợt 2/9 Đến ngày → **07/11** (chồng lấn Đợt 7/11) | **Lưu được** + cảnh báo chồng lấn nêu đúng cặp đợt (QT6) |
| E4-12 | Hoàn nguyên Đến ngày về 10/09 | Mọi con số trở về đúng E4-01 |

### E2E-5 · Xuất Excel từ cả ba nơi (UC-11, UC-34, UC-40, UC-51)

Trạng thái đầu: giống E2E-3. Mỗi file tải về phải được **mở ra đọc**, không chỉ kiểm tên.

| Bước | Thao tác | Kết quả mong đợi |
|---|---|---|
| E5-01 | Dashboard → Xuất Excel | Tên tệp `DuDieuKien_Dot7-11_2026.xlsx` |
| E5-02 | Mở file E5-01 | Dòng tiêu đề có "Đảng ủy Phường Kiểm Thử", tên đợt, khoảng **01/10/2026 – 07/11/2026**, ngày xuất **19/09/2026** |
| E5-03 | Đối chiếu nội dung E5-01 với bảng trên màn hình | **6 dòng**, cùng cột, cùng thứ tự sắp; giá trị từng ô khớp |
| E5-04 | Chi tiết Đợt 3/2 · 2026 → Xuất Excel | `DuDieuKien_Dot3-2_2026.xlsx`, **5 dòng**, Ngô Văn Khánh có Ngày tròn mốc **28/02/2026** |
| E5-05 | Đổi bộ chọn năm sang **2027** rồi xuất | Tên tệp chứa **2027**, khoảng ngày gắn năm 2027, nội dung khớp bảng năm 2027 |
| E5-06 | M4 năm 2026 → Xuất Excel | `ChuaThuocDot_2026.xlsx`, **7 dòng**, có cột Khoảng trống |
| E5-07 | Kiểm ô trống trong mọi file trên | Ô Ngày sinh / Giới tính của Cao Thị Thu là **rỗng**, **không** phải `—` |
| E5-08 | Xóa Tên đơn vị trong Cài đặt rồi xuất lại | Tiêu đề không còn phần tên đơn vị, không để lại dòng trắng lạ |
| E5-09 | Xuất từ một đợt không có ai đủ điều kiện | Vẫn ra file, có tiêu đề, 0 dòng dữ liệu, không lỗi |
| E5-10 | Kiểm số trên giao diện | "1.232 người" dùng **dấu chấm** ngăn nghìn (sau khi nạp cả bộ lớn) |

### E2E-6 · Vòng đời đảng viên (UC-20 → UC-23, QT3, QT3a, QT5)

Trạng thái đầu: đã đăng nhập, 4 đợt, cài đặt 30/90/5, bộ lõi 32 người, T0.

| Bước | Thao tác | Kết quả mong đợi |
|---|---|---|
| E6-01 | Danh sách Đảng viên | "32 người · Tuổi đảng tính đến hôm nay" |
| E6-02 | Thêm tay: Họ tên "Kiểm Thử Vòng Đời", Ngày sinh 10/10/1970, Nam, Ngày chính thức **01/10/1996** | Thêm thành công, tổng **33** |
| E6-03 | Tìm "Kiểm Thử Vòng Đời" | Thấy đúng 1 dòng; Tuổi đảng **29**, Mốc kế tiếp **30**, Ngày tròn mốc kế tiếp **01/10/2026** |
| E6-04 | Mở Dashboard | Đợt 7/11 lên **7 người** — người mới tròn 30 đúng Từ ngày (QT5) |
| E6-05 | Thử thêm tay với Ngày chính thức **20/09/2026** | Bị chặn, thông báo tiếng Việt (ràng buộc ≤ hôm nay) |
| E6-06 | Thử thêm tay bỏ trống Họ tên | Bị chặn |
| E6-07 | Thêm tay chỉ có Họ tên + Ngày chính thức (bỏ Ngày sinh, Giới tính) | Thành công; hai ô hiện `—` |
| E6-08 | Sửa "Kiểm Thử Vòng Đời": Ngày chính thức → **05/10/1991** | Lưu thành công |
| E6-09 | Xem lại dòng đó | Tuổi đảng **34**, Mốc kế tiếp **35**, Ngày tròn mốc kế tiếp **05/10/2026** |
| E6-10 | Dashboard | Người đó giờ thuộc mốc **35** của Đợt 7/11; phân bổ theo mốc đổi đúng |
| E6-11 | Sửa Ngày chính thức → **01/05/1935** | Tuổi đảng 91, Mốc kế tiếp `—`, và người đó **rời** danh sách Đợt 7/11 |
| E6-12 | Lọc Giới tính = Nữ, rồi bỏ lọc | Số dòng khớp `expected.json`; bỏ lọc trở về đủ |
| E6-13 | Đổi số dòng/trang, sang trang 2, quay lại | Không mất người, không trùng người |
| E6-14 | Chọn 3 dòng | Hiện "**Đang chọn 3 dòng**" |
| E6-15 | Xóa 3 dòng đã chọn | Hộp xác nhận nêu rõ "**3 người**"; sau khi xóa tổng giảm đúng 3 |
| E6-16 | Bấm Xóa rồi **Hủy** trong hộp xác nhận | Tổng **không** đổi |
| E6-17 | Xóa hết người vừa thêm, về lại 32 | Dashboard và badge trở về đúng E3-01 |

### E2E-7 · Đợt trao huy hiệu vắt qua 31/12 (T54 — UC-31, UC-34, UC-36, UC-40, QT6, QT7)

Trạng thái đầu: đã đăng nhập, kho sạch, cài đặt 30/90/5 kèm tên đơn vị, sáu đảng viên dựng
riêng quanh hai đầu đợt. Mọi ngày suy từ năm máy chủ đang báo, không viết cứng.

| Bước | Thao tác | Kết quả mong đợi |
|---|---|---|
| E7-01 | Tạo đợt Từ 01/12, Đến 28/02 qua modal | Lưu được, không dòng lỗi đỏ nào |
| E7-02 | Đọc dòng nhắc lúc modal còn mở | Câu có chữ "vắt qua 31/12" và nêu đúng hai ngày |
| E7-03 | Bảng đợt | Cột Đến ngày hiện `28/02` kèm chữ **năm sau** |
| E7-04 | Dải độ phủ | Hai vạch: một sát mép trái, một sát mép phải; không cảnh báo chồng lấn |
| E7-05 | Banner cảnh báo | Nêu đúng khoảng trống giữa năm 01/03–30/11 |
| E7-06 | Chi tiết đợt, tab Thông tin | "01/12 – 28/02 năm sau, hằng năm" |
| E7-07 | Tab Đủ điều kiện, năm giữa | Khoảng ngày 01/12/Y – 28/02/(Y+1); người tròn mốc tháng 02 năm sau có trong danh sách |
| E7-08 | Màn Chưa thuộc đợt nào | Người tròn mốc 20/01 không xuất hiện; badge không tăng vì người đó |
| E7-09 | Xuất Excel từ tab Đủ điều kiện | Tải về được, đủ số dòng, dòng tiêu đề ghi đúng khoảng ngày |
| E7-10 | Sửa đợt về 01/10 – 07/11 | Mọi dấu hiệu vắt năm biến mất, các con số đổi theo |
| E7-11 | Sửa ngược lại 01/12 – 28/02 | Mọi con số trở về đúng trạng thái ban đầu — đổi chiều được |

Ca ép ngày `E2E_TODAY=2027-01-15` (bảng đợt đọc "Đang diễn ra", Dashboard chọn lần diễn ra
khởi đầu từ 01/12/2026) chờ T53 gỡ nút `HUYHIEUDANG_TEST_TODAY`; phần nghiệp vụ đã nghiệm thu
ở tầng API bằng A-223.

### E2E — ca chạy lại và độc lập

| Ca | Mong đợi |
|---|---|
| E-901 | Chạy cả 6 luồng **hai lần liên tiếp** không reset tay giữa hai lần: cùng kết quả |
| E-902 | Chạy 6 luồng theo thứ tự đảo: cùng kết quả — không luồng nào phụ thuộc luồng khác |
| E-903 | Chạy lại toàn bộ với mốc **T1** (15/10/2026): thẻ Dashboard hiện "đang diễn ra" |
| E-904 | Chạy lại toàn bộ với mốc **T2** (01/12/2026): Dashboard hiện Đợt 3/2 của **2027** |
| E-905 | Không ca nào dùng `waitForTimeout` cố định để "chờ cho chắc" — chỉ chờ theo điều kiện |

---

## 7. Bảng truy vết QT1–QT11

| Quy tắc | Logic | API | E2E |
|---|---|---|---|
| **QT1** Dãy mốc | U-101 → U-110 | A-501 → A-506 | E1-06, E1-07, E3-02 |
| **QT2** Ngày tròn mốc (29/02) | U-201 → U-208 | A-216, A-217 | E5-04 |
| **QT3** Tuổi đảng | U-301 → U-309 | A-125 | E1-14, E6-03, E6-09 |
| **QT3a** Mốc kế tiếp (`—`) | U-351 → U-357 | A-120 | E1-14, E6-09, E6-11, E3-09 |
| **QT4** Đủ điều kiện theo đợt/năm | U-401 → U-419 | A-214 → A-220 | E1-15, E1-16, E3-05, E5-03 |
| **QT5** Không lưu kết quả | U-501 → U-503 | A-120, A-212, A-502 | E3-05, E3-06, E4-06, E6-04 |
| **QT6** Đợt trao huy hiệu | U-601 → U-612, U-620 → U-626 | A-205 → A-213, A-221 → A-224 | E1-09, E1-10, E4-04, E4-09, E4-11, E7-01 → E7-11 |
| **QT7** Chưa thuộc đợt nào | U-701 → U-712, U-623, U-625 | A-401 → A-406, A-222 | E1-17, E3-07, E3-08, E4-01, E4-08, E7-08 |
| **QT8** Đợt sắp tới | U-801 → U-809, U-626 | A-301 → A-304, A-223 | E1-15, E-903, E-904, E7-11 |
| **QT9** Import Excel | U-901 → U-921 | A-601 → A-620 | E2-01 → E2-15 |
| **QT10** Xóa hẳn | U-1001, U-1002 | A-121 → A-124, A-213 | E6-15, E6-16, E4-12 |
| **QT11** Trạng thái đợt | U-1101 → U-1107, U-626 | A-202, A-203, A-223 | E1-09, E-903, E7-03 |

Use case: UC-00/01 → A-005, A-006, E1-01 → E1-03, E1-19 · UC-10 → A-301, E1-15 ·
UC-11 → A-214, E1-16, E5-01 · UC-12 → A-305 → A-308, E1-04, E1-18 · UC-13 → A-306, E1-04 ·
UC-20 → A-101 → A-115, E6-01, E6-12 → E6-14 · UC-21/22 → A-116 → A-120, E6-02, E6-05 → E6-09 ·
UC-23 → A-121 → A-124, E6-15, E6-16 · UC-24 → A-601 → A-618, E2-01 → E2-15 ·
UC-25 → A-619, E2-02, E2-03 · UC-30 → A-201 → A-204, E1-09 · UC-31/32/33 → A-205 → A-213, E4-04 ·
UC-34 → A-214 → A-219, E5-04, E5-05 · UC-36 → E1-10 · UC-40 → A-401 → A-406, E4-01, E5-06 ·
UC-50 → A-501 → A-506, E1-06, E3-02 · UC-51 → A-507, A-508, E1-08, E5-02, E5-08.

---

## 8. Câu hỏi còn treo (OQ)

Những điểm tài liệu nghiệp vụ **chưa quy định**. QC không tự quyết. Ca kiểm thử liên
quan được viết sẵn nhưng để trạng thái *chờ chốt*; khi có câu trả lời, QC cập nhật
`tests/fixtures/dataset.py` rồi sinh lại fixture.

| Mã | Câu hỏi | Đề xuất của QC | Ảnh hưởng |
|---|---|---|---|
| **OQ-1** | Ngày sinh sai định dạng hoặc không tồn tại (`31/02/1974`) — lỗi cấp dòng, hay bỏ qua và để trống? | Lỗi cấp dòng, để người dùng thấy và sửa | U-914, `bien-chuan-hoa.xlsx` |
| **OQ-2** | Một dòng có nhiều lỗi — liệt kê **hết** lý do hay chỉ lỗi đầu tiên? | Liệt kê hết, ngăn cách bằng `;` — sửa một lần cho xong | U-915, E2-07 |
| **OQ-3** | Sắp xếp Họ tên dùng collation nào? `Đào Văn Ân` đứng **trước** `Nguyễn Văn An` theo chữ cái tiếng Việt, nhưng **sau** theo mã Unicode | Collation tiếng Việt (ICU `vi`), vì người dùng là cán bộ Việt Nam | U-419, A-115, E1-16, E5-03 |
| **OQ-4** | Họ tên có khoảng trắng đầu/cuối khi import — có cắt bỏ không? | Cắt bỏ | U-921 |
| **OQ-5** | Giới tính `nam` / `NAM` — có nhận không? | Nhận, so sánh không phân biệt hoa/thường | U-921 |
| **OQ-6** | Ngày `1/10/1996` (không đủ 2 chữ số) — có nhận không? | Nhận, vì Excel hay tự bỏ số 0 | U-921 |
| **OQ-7** | File chỉ có tiêu đề, 0 dòng dữ liệu — thông báo giống hay khác "file rỗng"? | Khác: "File không có dòng dữ liệu nào" | U-919, A-609 |
| **OQ-8** | Tìm theo tên — phân biệt hoa/thường? phân biệt dấu? | Không phân biệt hoa/thường (citext). **Có** phân biệt dấu, vì tài liệu chỉ nói "chứa chuỗi" | A-108, A-109 |
| **OQ-9** | Hai đợt có cùng Từ ngày — QT8 chọn đợt nào làm "sắp tới"? | Đợt có Đến ngày sớm hơn; bằng nhau nữa thì theo Tên | U-808 |
| **OQ-10** | Ngày sinh **bằng** Ngày chính thức — hợp lệ hay lỗi? | Lỗi (không ai vào Đảng ngày mình sinh ra) | U-912 |

---

## 9. Cách báo lỗi

Mỗi lỗi là một issue Multica, gán cho agent sở hữu vùng mã, và **phải** có đủ sáu phần:

```
Tiêu đề: [LỖI][<mức>] <một câu tả hiện tượng>

1. Quy tắc bị vi phạm: QT<x> hoặc UC-<xx> — trích đúng câu trong tài liệu
2. Ca kiểm thử: <mã ca, ví dụ U-402 / A-214 / E3-05>
3. Bước tái hiện: đánh số, kèm mốc thời gian đang ép (T0/T1/T2) và fixture đã nạp
4. Kết quả mong đợi: kèm số cụ thể, dẫn tests/fixtures/data/expected.json
5. Kết quả thực tế: kèm số cụ thể, ảnh chụp hoặc log
6. Mức độ: Chặn / Nặng / Vừa / Nhẹ
```

| Mức | Nghĩa | Xử lý |
|---|---|---|
| **Chặn** | Một quy tắc QT cho ra số sai, hoặc không dùng được chức năng chính | Báo CEO **ngay**, không chờ hết đợt kiểm thử. QC chặn phát hành |
| **Nặng** | Sai ở ca biên, hoặc lỗi 500 lộ ra người dùng | Sửa trước khi đóng task |
| **Vừa** | Sai hiển thị, sai định dạng, thông báo không phải tiếng Việt | Đưa vào vòng T29 |
| **Nhẹ** | Khác biệt nhỏ về giao diện, câu chữ | Ghi nhận, sửa nếu còn thời gian |

**Một lỗi chỉ được coi là đã sửa khi QC chạy lại đúng ca kiểm thử đó và nó chuyển xanh.**
Người sửa tự xác nhận không tính. Kèm lệnh đã chạy và kết quả dán nguyên văn.

---

## 10. Việc còn phải làm của chính kế hoạch này

| Việc | Khi nào | Của ai |
|---|---|---|
| Rà lại bảng mục 5 theo `docs/api-contract.md`, bổ sung ca cho endpoint chưa chạm | Sau T05 | QC |
| Sửa tên bảng/cột trong `tests/fixtures/generate.py` (biến `DB`) theo migration thật, sinh lại `sql/` | Sau T06 | QC |
| Trả lời OQ-1 → OQ-10 | Trước T26 | CEO, với ý kiến Backend |
| Dựng abstraction thời gian theo T-FIX-1 → T-FIX-4 | Cùng T07 | Backend |
| Dựng cấu hình Playwright theo T-FIX-5 | Cùng T24 | Frontend, QC |
