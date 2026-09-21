# QC nghiệm thu đợt trao huy hiệu vắt qua 31/12 (T54)

Nhánh `test/T54-qc-dot-vat-qua-nam`, tách từ `agent/ceo/28ebdc3754dd`. Không sửa dòng nào
trong `BE/src/` và `FE/src/`.

Lệnh đã chạy:

```
cd BE && dotnet test HuyHieuDang.sln
cd FE && docker compose -f e2e/docker-compose.e2e.yml up -d --build
cd FE && npx playwright test -c e2e/playwright.e2e.config.ts
cd FE && npx tsc -p tsconfig.e2e.json --noEmit
cd FE && node e2e/scripts/chup-anh-e7.mjs
```

Kết quả `dotnet test`: **634 ĐẠT · 0 HỎNG · 8 bỏ qua**, trên sáu dự án test.

| Dự án | Đạt | Hỏng | Bỏ qua |
|---|---|---|---|
| HuyHieuDang.Core.UnitTests | 141 | 0 | 0 |
| HuyHieuDang.Core.QcTests | 129 | 0 | 1 |
| HuyHieuDang.Infrastructure.UnitTests | 46 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |
| HuyHieuDang.Web.IntegrationTests | 181 | 0 | 0 |
| HuyHieuDang.Web.QcIntegrationTests | 136 | 0 | 7 |

Kết quả `npx playwright test`: **10 ĐẠT · 2 HỎNG · 3 bỏ qua** trên 8 tệp luồng. Luồng mới
`e7-dot-vat-qua-nam` ĐẠT trọn vẹn. Hai luồng hỏng là `e2` và `e6`, hỏng vì một lý do
**không liên quan** đến đợt vắt năm — xem lỗi QC-T54-02 ở cuối báo cáo.

Kiểm kiểu bộ E2E (`tsc -p tsconfig.e2e.json`) chạy sạch.

## Mã ca: bảng quy đổi so với đề bài T54

Bảy mã mà T54 đặt ra đã có chủ trong `docs/test-plan.md` với nghĩa khác. Để không có hai ca
khác nghĩa cùng một mã, khối mới nhận mã liền sau khối cũ:

| T54 đặt | Mã đã dùng | Mã đang có chủ trong test-plan |
|---|---|---|
| U-610 | **U-620** | U-610 · Bộ 4 đợt chính để hở đúng 5 khoảng trống |
| U-611 | **U-621** | U-611 · Hai đợt phủ kín 01/01–31/12 |
| U-612 | **U-622** | U-612 · Hai đợt liền kề không phải chồng lấn |
| U-613 | **U-623** | (trống) |
| U-614 | **U-624a**, **U-624b** | (trống) |
| U-615 | **U-625** | (trống) |
| A-210 | **A-221** | A-210 · 4 đợt chính, 5 khoảng trống |
| A-211 | **A-222** | A-211 · Bộ đợt phủ kín |
| A-212 | **A-223** | A-212 · Nới Đến ngày lan truyền |
| A-213 | **A-224** | A-213 · Xóa đợt không mất đảng viên |

Thêm U-626 — ca đối chiếu diện rộng giữa service và oracle của QC.

## Oracle của QC đã viết lại, không còn là bản chép của service

`QcOracle` bản CEO gửi sang cắt mỗi lần diễn ra thành "phần trong năm" rồi làm toán trên
khoảng — đúng từng bước một với `PartyMilestoneCalculator`, kể cả mẹo bỏ qua cặp trùng tên.
Hai bản sai cùng kiểu thì khớp nhau vẫn cùng sai, nên phần đợt vắt năm được viết lại theo
một lối khác hẳn: oracle **đi từng ngày** của năm và chỉ hỏi một câu "ngày này có nằm trong
đợt không?" — so cặp (tháng, ngày) của chính ngày đó với hai cặp của đợt, đợt vắt năm là
phép *hoặc*. Độ phủ, khoảng trống, chồng lấn, đợt sắp tới và trạng thái đều suy ra từ tập
ngày ấy; oracle không mượn khái niệm lần diễn ra, năm neo hay phần cắt nào của service.
`GetUpcomingPeriod` thì oracle dò lùi rồi dò tới từng ngày thay vì gắn sẵn ba năm neo.

Một khác biệt có thật giữa hai cách, đã xem xét và kết luận **không phải lỗi**: đợt phủ trọn
năm (`01/12 – 30/11`, `02/01 – 01/01`, `01/03 – 28/02`) để lại hai phần **dính nhau** trong
cách cắt của service (`01/01–30/11` và `01/12–31/12`) nhưng một dải liền trong cách đi từng
ngày của oracle. Hai cách chia khác nhau, số ngày được phủ giống hệt nhau, nên ca U-626 so
sau khi nối các đoạn liền nhau của cùng một đợt.

## Qc12Qt6SpanningYearTests (mới)

U-620 · Đợt 01/12 – 30/11 dài 365 ngày vẫn chỉ trao tối đa 1 mốc/người/đợt, quét 366 ngày vào Đảng × 7 năm với Bước = 1 — ĐẠT
U-621 · Đợt 02/01 – 01/01 phủ trọn năm: không khoảng trống, không tự chồng lấn, không ai bị sót, mọi lời gọi dừng dưới 30 giây — ĐẠT
U-622 · Đợt vắt năm kết thúc 29/02 gắn năm 2026 cho 28/02/2027, năm 2027 cho 29/02/2028, năm 2028 cho 28/02/2029 — ĐẠT
U-623 · Người tròn mốc đúng 01/12 và đúng 28/02 đều đủ điều kiện, đều không bị xếp vào "chưa thuộc đợt nào"; hai ngày liền ngoài biên thì ngược lại — ĐẠT
U-624a · Đợt vắt năm chồng lấn một đợt thường ở đầu năm báo đúng một cặp, khoảng dùng chung 15/01–28/02 — ĐẠT
U-624b · Hai đợt vắt năm gặp nhau ở hai đoạn rời nhau, 01/01–20/01 và 01/12–31/12, khớp oracle — ĐẠT
U-625 · Xóa đợt vắt năm thì khoảng trống mới phủ đúng hai đầu năm, người tròn mốc 20/01 mất chỗ đúng chiều — ĐẠT
U-626 · Service và oracle độc lập khớp nhau trên 9 bộ đợt vắt năm × 7 năm, cộng 731 ngày quét đợt sắp tới và trạng thái — ĐẠT

## Qc06Qt6PeriodTests (sửa một chỗ)

U-602b · Đối chiếu oracle nay so cả khoảng ngày dùng chung, không chỉ tên cặp đợt — ĐẠT
Mười ca còn lại của lớp giữ nguyên, vẫn đạt trên oracle đã viết lại — ĐẠT

## A2bSpanningYearTests (mới, chạy trên PostgreSQL thật)

A-221 · `GET /AwardPeriods?year=2026`: toDate 2027-02-28, spansNextYear = true, dải độ phủ hai đoạn cùng periodId ở hai đầu năm, không cảnh báo chồng lấn, khoảng trống đúng 01/03–30/11 — ĐẠT
A-222 · Người tròn mốc 20/01 và 10/02 nằm trong đuôi đợt nên không có mặt ở "chưa thuộc đợt nào" năm 2027; badge bằng đúng số dòng của danh sách — ĐẠT
A-223 · `GET /Dashboard` ngày 15/01/2027: đợt sắp tới là lần diễn ra neo ở 2026, trạng thái Ongoing, không có số ngày đếm ngược; bảng đợt năm 2027 cũng đọc Ongoing; tổng người trên Dashboard bằng danh sách đủ điều kiện — ĐẠT
A-224 · Xuất Excel đợt vắt năm: tên file `DuDieuKien_DotGiaothua_2026.xlsx`, dòng tiêu đề ghi 01/12/2026 – 28/02/2027, số dòng bằng đúng bảng trên màn hình — ĐẠT

Ghi chú: ca ép ngày ở A-223 chạy được vì host kiểm thử tích hợp thay thẳng `IDateTimeProvider`.
Ở tầng E2E thì chưa — xem phần T53 bên dưới.

## e7-dot-vat-qua-nam.spec.ts (mới, trình duyệt thật)

E7-01 · Tạo đợt Từ 01/12 Đến 28/02 qua modal, lưu được, không dòng lỗi đỏ nào — ĐẠT
E7-02 · Dòng nhắc trong modal đổi thành "Đợt vắt qua 31/12: 01/12 năm nay đến 28/02 năm sau" — ĐẠT
E7-03 · Bảng đợt: cột Đến ngày hiện `28/02 năm sau`, số người đủ điều kiện khớp máy chủ — ĐẠT
E7-04 · Dải độ phủ cho đúng hai vạch, một sát mép trái, một sát mép phải, không vạch chồng lấn nào — ĐẠT
E7-05 · Banner cảnh báo nêu đúng khoảng trống giữa năm 01/03–30/11 — ĐẠT
E7-06 · Tab Thông tin đọc "01/12 – 28/02 năm sau, hằng năm" — ĐẠT
E7-07 · Tab Đủ điều kiện năm giữa: khoảng ngày 01/12/2026 – 28/02/2027, đủ bốn người kể cả người tròn mốc tháng 02 năm sau và hai người đúng biên; chọn năm trước thì gom đúng người tròn mốc 20/01 — ĐẠT
E7-08 · Người tròn mốc 20/01 không có mặt ở "Chưa thuộc đợt nào", badge bằng 1 và bằng đúng số máy chủ trả — ĐẠT
E7-09 · Xuất Excel từ tab Đủ điều kiện: tải về được, tên file đúng quy ước, ba dòng tiêu đề đúng, số dòng và thứ tự khớp bảng — ĐẠT
E7-10 · Sửa về 01/10 – 07/11: chữ "năm sau" biến mất, còn một vạch độ phủ, hai khoảng trống mới, bốn người quay lại "chưa thuộc đợt nào" — ĐẠT
E7-11 · Sửa ngược lại 01/12 – 28/02: mọi con số trở về đúng trạng thái ban đầu, Dashboard đọc đúng lần diễn ra neo ở năm nay — ĐẠT
QC-T54-01 · Tiêu đề trang chi tiết đợt phải nói rõ Đến ngày thuộc năm sau — HỎNG đúng như mô tả (ca gắn `test.fail()`, xem lỗi bên dưới)

## Năm luồng E2E cũ

E2E-1 Lần dùng đầu tiên — ĐẠT
E2E-3 Đổi cài đặt lan truyền — ĐẠT
E2E-4 Sửa đợt lan truyền — ĐẠT
E2E-5 Xuất Excel ba nơi — ĐẠT
E2E-2 Import có lỗi — HỎNG ở bước E2-04, không tìm thấy nút "Xóa 2 đã chọn" (QC-T54-02)
E2E-6 Vòng đời đảng viên — HỎNG ở bước E6-15, không tìm thấy nút "Xóa 3 đã chọn" (QC-T54-02)

## Lỗi phát hiện

### QC-T54-01 · Tiêu đề trang chi tiết đợt bỏ mất chữ "năm sau" — mức Thấp

Bước tái hiện: tạo đợt `01/12 – 28/02` → mở trang chi tiết đợt.

Kết quả mong đợi: dòng tiêu đề lớn nói rõ Đến ngày thuộc năm sau, cùng một giọng với bảng
đợt và tab Thông tin.

Kết quả thực tế: tiêu đề đọc **"Đợt Giao thừa 01/12 – 28/02 hằng năm"** — nghe như hai đầu
nằm trong cùng một năm — trong khi tab Thông tin ngay bên dưới đọc **"01/12 – 28/02 năm sau,
hằng năm"**. Cùng một màn hình đang nói hai kiểu. Ảnh `4-tab-thong-tin.png` thấy rõ cả hai
dòng.

Quy tắc bị vi phạm: QT6 (Đến ngày của đợt vắt năm thuộc năm kế tiếp), UC-34.

Nơi sửa: `FE/src/pages/periods/detail/PeriodDetailHeader.tsx` — chỗ duy nhất còn ghép chuỗi
`{fromDisplay} – {toDisplay} hằng năm` mà không xét `period.spansNextYear`.

Ca kiểm thử đã có sẵn, đang gắn `test.fail()` ở cuối `e7-dot-vat-qua-nam.spec.ts`: khi sửa
xong, Playwright sẽ báo "đáng lẽ hỏng mà lại đạt" — lúc đó bỏ dòng `test.fail()` đi.

### QC-T54-02 · Hai luồng E2E cũ đỏ vì nút xóa nhiều đổi tên, không liên quan đợt vắt năm — mức Trung bình

Bước tái hiện: `npx playwright test -c e2e/playwright.e2e.config.ts` → `e2` hỏng ở E2-04,
`e6` hỏng ở E6-15.

Kết quả mong đợi: bấm được nút xóa nhiều dòng sau khi đánh dấu chọn.

Kết quả thực tế: cả hai luồng chờ nút tên `Xóa 2 đã chọn` / `Xóa 3 đã chọn` mà không thấy.
Màn Đảng viên nay dùng nút thùng rác đỏ mang `aria-label="Xóa người đã chọn"` — đổi từ T33
(`27239da`), và commit ấy không cập nhật hai tệp luồng. Hai luồng đã đỏ từ trước khi có thay
đổi đợt vắt năm: commit `e811735` không đụng `FE/src/pages/members/` lẫn `FE/e2e/specs/`, còn
hai tệp luồng thì chưa đổi dòng nào kể từ `36b8af6`.

Chưa sửa theo đúng dặn dò của T54: `e1…e6` chỉ sửa khi đỏ vì thay đổi này, và phải báo trước.
Sửa thì chỉ cần đổi cách định vị nút trong hai tệp luồng, mã sản phẩm không sai.

## Bốn dấu hiệu "báo ngay" của T54 — đều không thấy

| Dấu hiệu | Ca đã phủ | Kết quả |
|---|---|---|
| Người tròn mốc đúng 01/12 hoặc 28/02 bị lọt ra ngoài | U-623, E7-07 | Không xảy ra |
| Một người vừa đủ điều kiện vừa chưa thuộc đợt nào trong một năm | U-623, A-222, E7-08 | Không xảy ra |
| Một đợt tự báo chồng lấn với chính nó | U-621, U-626, A-221, E7-04 | Không xảy ra |
| Tổng người của đợt vắt năm khác tổng trên Dashboard | A-223, E7-11 | Không xảy ra |

## Còn nợ: ca ép ngày ở tầng E2E, chờ T53

Backend vẫn **chưa đọc** `HUYHIEUDANG_TEST_TODAY`: `grep` trong `BE/src` không có dòng nào,
và stack e2e chạy với `E2E_TODAY=2026-09-19` vẫn trả `today = 2026-09-20`. Vì vậy ca
`E2E_TODAY=2027-01-15` (bảng đợt đọc "Đang diễn ra", thẻ Dashboard chọn lần diễn ra khởi đầu
từ 01/12/2026) chưa chạy được ở trình duyệt — đúng như T54 đã lường trước, để lại cho nhịp
bàn giao thứ hai.

Phần nghiệp vụ của ca ấy đã được nghiệm thu ở tầng API bằng A-223, nơi host kiểm thử ép được
đồng hồ: ngày 15/01/2027, Dashboard đọc lần diễn ra neo ở 2026 với trạng thái Ongoing và bảng
đợt năm 2027 cũng đọc Ongoing.

## Ảnh bằng chứng

Chụp bằng `node e2e/scripts/chup-anh-e7.mjs`, lưu ở `FE/e2e/.artifacts/anh-t54/`
(không commit):

1. `1-modal-vat-nam.png` — modal đang mở, dòng nhắc "Đợt vắt qua 31/12: 01/12 năm nay đến 28/02 năm sau".
2. `2-bang-dot-nam-sau.png` — bảng đợt với `28/02 năm sau`, banner khoảng trống 01/03–30/11.
3. `3-dai-do-phu-hai-vach.png` — dải độ phủ hai vạch, hai đầu năm.
4. `4-tab-thong-tin.png` — tab Thông tin đọc "01/12 – 28/02 năm sau, hằng năm" (và thấy luôn lỗi QC-T54-01 ở dòng tiêu đề).
