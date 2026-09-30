Báo cáo kiểm thử cho việc đổi nhãn "Ngày vào Đảng chính thức" / "Ngày chính thức" thành
"Ngày vào Đảng (dự bị)" (T69, HUYH-79). Chạy ngày 30/09/2026 trên nhánh
`fix/T69-ngay-vao-dang-du-bi`.

## Lệnh đã chạy và tổng kết

```
cd BE && dotnet build && dotnet test
```

| Dự án kiểm thử | Đạt | Hỏng | Bỏ qua |
|---|---:|---:|---:|
| HuyHieuDang.Core.QcTests | 129 | 0 | 1 |
| HuyHieuDang.Core.UnitTests | 157 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |
| HuyHieuDang.Infrastructure.UnitTests | 78 | 0 | 0 |
| HuyHieuDang.Web.IntegrationTests | 226 | 0 | 0 |
| HuyHieuDang.Web.QcIntegrationTests | 143 | 0 | 0 |
| **Tổng** | **734** | **0** | **1** |

```
cd FE && T50_PORT=4185 npm run test:ui
```

98 đạt · 0 hỏng · 0 bỏ qua.

```
cd FE && docker compose -f e2e/docker-compose.e2e.yml up -d --build
cd FE && npx playwright test -c e2e/playwright.e2e.config.ts \
    e0-tien-de e1-lan-dung-dau-tien e2-import-co-loi e5-xuat-excel \
    e6-vong-doi-dang-vien e7-dot-vat-qua-nam
```

9 đạt · 0 hỏng · 0 bỏ qua.

# Backend

## HuyHieuDang.Core.QcTests

### Qc01Qt1MilestoneTests

- QC-04 · Bước = int.MaxValue chỉ được cho ra mốc đầu, không được tràn số — ĐẠT
- QC · Gọi hai lần cùng cài đặt cho hai danh sách bằng nhau và tách rời nhau — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 5, end: 100, step: 7) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 30, end: 95, step: 5) — ĐẠT
- U-110 · 1 / 100 / 1 cho đúng 100 mốc, không treo — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 1, end: 100, step: 1) — ĐẠT
- U-101 · Dãy mốc luôn tăng dần và không trùng — ĐẠT
- U-104 · Bước lớn hơn cả khoảng 30–90 thì chỉ còn mốc đầu — ĐẠT
- U-101/U-102 · Dãy mốc khớp expected.json của QC(scenario: "core_default_T0") — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 1, end: 1, step: 1) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 30, end: 90, step: 61) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 30, end: 92, step: 5) — ĐẠT
- U-107/U-108/U-109 · Cài đặt sai bị từ chối bằng lỗi nghiệp vụ, không trả dãy rỗng — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 30, end: 30, step: 5) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 30, end: 90, step: 5) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt(start: 30, end: 90, step: 10) — ĐẠT
- U-101/U-102 · Dãy mốc khớp expected.json của QC(scenario: "core_step10_T0") — ĐẠT
- U-106 · Mốc cuối đúng bằng Kết thúc thì phải có trong dãy — ĐẠT

### Qc02Qt2AnniversaryTests

- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc(month: 1) — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc(month: 8) — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc(month: 5) — ĐẠT
- U-204 · 29/02/1996 + 4 năm → 2000 chia hết 400 nên vẫn nhuận — ĐẠT
- U-207 · 31/01/1996 + 30 năm → 31/01/2026, ngày cuối tháng không bị đụng — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc(month: 3) — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc(month: 12) — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc(month: 10) — ĐẠT
- U-208 · Mốc 0 trả đúng ngày vào Đảng — ĐẠT
- U-206 · 28/02/1996 + 32 năm → 28/02/2028, không được nhảy sang 29/02 — ĐẠT
- U-205 · 29/02/1896 + 4 năm → 1900 chia hết 100 nhưng KHÔNG nhuận → 28/02 — ĐẠT
- QC · Đối chiếu mọi ngày của 4 năm (nhuận và không nhuận) với oracle của QC — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc(month: 7) — ĐẠT

### Qc03Qt3PartyAgeTests

- U-304/U-305/U-306 · Các mốc tuổi đảng đặc biệt của bộ lõi tại T0(code: "M02", expected: 90) — ĐẠT
- QC · Mốc kế tiếp khớp oracle của QC trên cả bộ lõi ở T0/T1/T2, hai cài đặt — ĐẠT
- U-356 · Mốc kế tiếp phải LỚN HƠN tuổi đảng, không được bằng — ĐẠT
- U-304/U-305/U-306 · Các mốc tuổi đảng đặc biệt của bộ lõi tại T0(code: "V01", expected: 0) — ĐẠT
- U-307/U-308/U-309 · Người vào Đảng 29/02 xét ở năm không nhuận(y: 2026, m: 3, d: 1, expected: 30) — ĐẠT
- U-355 · L02 (29/02/1988) có mốc kế tiếp 40 rơi đúng 29/02/2028 — ĐẠT
- U-353/U-354/U-357 · Người đã vượt mốc lớn nhất hết mốc kế tiếp, kể cả khi đổi Bước(code: "M02", step: 5) — ĐẠT
- U-301/U-302/U-303 · Tuổi đảng quanh đúng ngày kỷ niệm tại T0(year: 1996, month: 9, day: 19, expected: 30) — ĐẠT
- U-301/U-302/U-303 · Tuổi đảng quanh đúng ngày kỷ niệm tại T0(year: 1996, month: 9, day: 20, expected: 29) — ĐẠT
- U-307/U-308/U-309 · Người vào Đảng 29/02 xét ở năm không nhuận(y: 2026, m: 2, d: 27, expected: 29) — ĐẠT
- U-307/U-308/U-309 · Người vào Đảng 29/02 xét ở năm không nhuận(y: 2026, m: 2, d: 28, expected: 30) — ĐẠT
- QC · Tuổi đảng khớp oracle của QC trên mọi ngày 2024–2030 của cả bộ lõi — ĐẠT
- QC · Tuổi đảng chỉ tăng theo thời gian, không bao giờ tụt (quét bộ lõi 2024–2030) — ĐẠT
- U-353/U-354/U-357 · Người đã vượt mốc lớn nhất hết mốc kế tiếp, kể cả khi đổi Bước(code: "M02", step: 10) — ĐẠT
- U-304/U-305/U-306 · Các mốc tuổi đảng đặc biệt của bộ lõi tại T0(code: "M01", expected: 91) — ĐẠT
- U-353/U-354/U-357 · Người đã vượt mốc lớn nhất hết mốc kế tiếp, kể cả khi đổi Bước(code: "M01", step: 5) — ĐẠT
- U-353/U-354/U-357 · Người đã vượt mốc lớn nhất hết mốc kế tiếp, kể cả khi đổi Bước(code: "M01", step: 10) — ĐẠT
- U-301/U-302/U-303 · Tuổi đảng quanh đúng ngày kỷ niệm tại T0(year: 1996, month: 9, day: 18, expected: 30) — ĐẠT

### Qc04Qt4EligibilityTests

- U-411/U-412 · N01 chỉ đủ điều kiện Đợt 7/11 của năm 2027, không phải 2026 — ĐẠT
- QC · Đủ điều kiện khớp oracle của QC trên mọi đợt × mọi năm 2020–2035 — ĐẠT
- U-413/U-414 · Đợt có Từ ngày 29/02: thu về 28/02 ở 2026, giữ 29/02 ở 2028 — ĐẠT
- U-418 · Không ai đủ điều kiện ở hai đợt cùng một năm — ĐẠT
- QC · Danh sách đủ điều kiện từng đợt/từng năm khớp expected.json của QC(scenario: "core_step10_T0") — ĐẠT
- U-415 · Đợt một ngày (Từ = Đến = 01/10) chỉ nhận đúng người tròn mốc hôm đó — ĐẠT
- QC · Danh sách đủ điều kiện từng đợt/từng năm khớp expected.json của QC(scenario: "core_default_T0") — ĐẠT
- U-416 · Phân bổ mốc của Đợt 7/11 năm 2026: 30×3, 35×1, 40×1, 45×1 — ĐẠT
- U-416/U-417 · Số người đủ điều kiện Đợt 7/11 năm 2026 theo Bước 5 và Bước 10(step: 10, expectedCount: 4) — ĐẠT
- U-416/U-417 · Số người đủ điều kiện Đợt 7/11 năm 2026 theo Bước 5 và Bước 10(step: 5, expectedCount: 6) — ĐẠT
- U-408/U-409 · L02 chỉ đủ điều kiện ở năm nhuận 2028 với mốc 40 — ĐẠT

### Qc05Qt5NoStoredResultTests

- U-501 · Service không sửa danh sách đợt và danh sách mốc được truyền vào — ĐẠT
- U-502 · Đổi cài đặt giữa hai lần gọi thì lần thứ hai đổi theo ngay — ĐẠT
- U-502 · Nới Đến ngày của đợt thì người đang bị sót chuyển sang đủ điều kiện ngay — ĐẠT
- U-503 · Không có bảng/entity nào lưu danh sách đủ điều kiện — ĐẠT
- U-501 · Gọi hai lần với cùng dữ liệu cho cùng kết quả, không tác dụng phụ — ĐẠT

### Qc06Qt6PeriodTests

- U-610 · Bộ 4 đợt chính để hở đúng 5 khoảng trống ở mọi năm xét(year: 2028) — ĐẠT
- U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày(fromDay: 29, fromMonth: 2, toDay: 5, toMonth: 3, year: 2028, from: "2028-02-29", to: "2028-03-05") — ĐẠT
- U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày(fromDay: 1, fromMonth: 10, toDay: 1, toMonth: 10, year: 2026, from: "2026-10-01", to: "2026-10-01") — ĐẠT
- QC · Khoảng trống khớp oracle độc lập của QC trên mọi bộ đợt, 2024–2030 — ĐẠT
- U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày(fromDay: 1, fromMonth: 10, toDay: 7, toMonth: 11, year: 2026, from: "2026-10-01", to: "2026-11-07") — ĐẠT
- U-609/U-611/U-612 · Cảnh báo chồng lấn khớp oracle của QC — ĐẠT
- QC · Các khoảng trống không chồng nhau, không thủng, phủ đúng phần còn lại của năm — ĐẠT
- U-602b · Oracle của QC và service của Backend khớp nhau trên đợt vắt năm — ĐẠT
- U-602 · Đợt có Từ ngày > Đến ngày kết thúc ở năm sau — ĐẠT
- U-610 · Bộ 4 đợt chính để hở đúng 5 khoảng trống ở mọi năm xét(year: 2026) — ĐẠT
- U-610 · Bộ 4 đợt chính để hở đúng 5 khoảng trống ở mọi năm xét(year: 2027) — ĐẠT
- U-610 · Bộ 4 đợt chính để hở đúng 5 khoảng trống ở mọi năm xét(year: 2025) — ĐẠT
- U-606/U-607 · Ngày/tháng không tồn tại phải báo lỗi nghiệp vụ — ĐẠT
- U-609 · Hai đợt chồng lấn vẫn được tính bình thường, chỉ là cảnh báo — ĐẠT
- U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày(fromDay: 1, fromMonth: 1, toDay: 31, toMonth: 12, year: 2026, from: "2026-01-01", to: "2026-12-31") — ĐẠT
- U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày(fromDay: 29, fromMonth: 2, toDay: 5, toMonth: 3, year: 2026, from: "2026-02-28", to: "2026-03-05") — ĐẠT

### Qc07Qt7MissedMilestoneTests

- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch(scenario: "core_default_T0_fullCover", periodSet: "fullCover") — ĐẠT
- QC · Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp expected.json(scenario: "core_default_T0_widenedP3", periodSet: "widened") — ĐẠT
- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch(scenario: "core_default_T0_overlap", periodSet: "overlap") — ĐẠT
- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch(scenario: "core_default_T0_widenedP3", periodSet: "widened") — ĐẠT
- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch(scenario: "core_default_T0_noPeriod", periodSet: "none") — ĐẠT
- QC · Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp expected.json(scenario: "core_default_T0", periodSet: "main") — ĐẠT
- QC · Người bị sót và người đủ điều kiện là hai tập rời nhau — ĐẠT
- QC-02 · Nhãn khoảng trống khi chưa cài đợt nào phải khớp expected.json — ĐẠT
- QC · Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp expected.json(scenario: "core_default_T0_leapEdgePeriod", periodSet: "leapEdge") — ĐẠT
- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch(scenario: "core_default_T0_leapEdgePeriod", periodSet: "leapEdge") — ĐẠT
- U-710 · Đổi Bước sang 10 thì S04 rời danh sách bị sót, còn 6 người — ĐẠT
- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch(scenario: "core_default_T0", periodSet: "main") — ĐẠT
- QC · Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp expected.json(scenario: "core_default_T0_overlap", periodSet: "overlap") — ĐẠT
- U-711 · Bộ đợt phủ kín cả năm thì không ai bị sót — ĐẠT
- U-712 · Không cài đợt nào thì 27 người của bộ lõi đều bị sót ở 2026 — ĐẠT

### Qc08Qt8UpcomingPeriodTests

- U-808 · Hai đợt cùng Từ ngày phải cho kết quả tất định — ĐẠT
- U-807 · Chưa cài đợt nào thì không có đợt sắp tới — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch(today: "2026-12-01", name: "Đợt 3/2", year: 2027) — ĐẠT
- QC · Đợt sắp tới khớp oracle của QC ở mọi ngày của 2026 và 2028 — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch(today: "2026-09-19", name: "Đợt 7/11", year: 2026) — ĐẠT
- QC · Đợt sắp tới luôn có Đến ngày không nhỏ hơn hôm nay — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch(today: "2026-11-07", name: "Đợt 7/11", year: 2026) — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch(today: "2026-10-15", name: "Đợt 7/11", year: 2026) — ĐẠT
- U-809 · Đợt 29/02 sang năm không nhuận: thu về 28/02 và đếm ngày theo ngày đã thu — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch(today: "2026-12-31", name: "Đợt 3/2", year: 2027) — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch(today: "2026-01-01", name: "Đợt 3/2", year: 2026) — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch(today: "2026-11-08", name: "Đợt 3/2", year: 2027) — ĐẠT

### Qc09Qt11PeriodStatusTests

- QC · Trạng thái 4 đợt chính khớp expected.json ở T0, T1, T2(scenario: "core_default_T2") — ĐẠT
- QC · Chỉ trạng thái Sắp tới mới có số ngày còn lại, và luôn dương — ĐẠT
- QC · Trạng thái và số ngày còn lại khớp oracle của QC ở mọi ngày 2026–2028 — ĐẠT
- QC · Trạng thái 4 đợt chính khớp expected.json ở T0, T1, T2(scenario: "core_default_T0") — ĐẠT
- QC · Trạng thái 4 đợt chính khớp expected.json ở T0, T1, T2(scenario: "core_default_T1") — ĐẠT
- U-1102/U-1107 · Số ngày còn lại đếm đúng từng ngày trước Từ ngày của Đợt 7/11 — ĐẠT

### Qc10PerformanceTests

- U-1201 · Tính đủ điều kiện 1 đợt / 1 năm cho 10.000 đảng viên dưới 1 giây — ĐẠT
- QC · Quét 'chưa thuộc đợt nào' cho 10.000 đảng viên dưới 1 giây — ĐẠT

### Qc11ClockScanTests

- A-901 · Không nơi nào trong BE/src đọc đồng hồ theo giờ máy (T-FIX-2) — ĐẠT
- A-901 · Module nghiệp vụ trong Infrastructure không đọc đồng hồ (trừ 7 dòng tầng khung) — ĐẠT
- QC-05 · Core không được đọc đồng hồ hệ thống — bỏ qua
- A-901 · Service tính mốc tuổi đảng không đọc đồng hồ, chỉ nhận today qua tham số — ĐẠT

### Qc12Qt6SpanningYearTests

- U-620 · Đợt 01/12 – 30/11 dài 365 ngày vẫn chỉ trao tối đa 1 mốc/người/đợt — ĐẠT
- U-623 · Người tròn mốc đúng 01/12 và đúng 28/02 đều đủ điều kiện — ĐẠT
- U-622 · Đợt vắt năm kết thúc 29/02 lùi về 28/02 khi năm kết thúc không nhuận(year: 2027, from: "2027-12-01", to: "2028-02-29") — ĐẠT
- U-624b · Hai đợt vắt năm: cặp chồng lấn hiện đúng hai đoạn, đầu năm và cuối năm — ĐẠT
- U-622 · Đợt vắt năm kết thúc 29/02 lùi về 28/02 khi năm kết thúc không nhuận(year: 2028, from: "2028-12-01", to: "2029-02-28") — ĐẠT
- U-621 · Đợt 02/01 – 01/01 phủ trọn năm: không mốc trùng, không vòng lặp vô hạn — ĐẠT
- U-625 · Xóa đợt vắt năm thì khoảng trống mới phủ đúng hai đầu năm — ĐẠT
- U-622 · Đợt vắt năm kết thúc 29/02 lùi về 28/02 khi năm kết thúc không nhuận(year: 2026, from: "2026-12-01", to: "2027-02-28") — ĐẠT
- U-626 · Service và oracle độc lập của QC khớp nhau trên mọi bộ đợt vắt năm, 2024–2030 — ĐẠT
- U-624a · Đợt vắt năm chồng lấn một đợt thường ở đầu năm: đúng một cặp, đúng khoảng ngày — ĐẠT

## HuyHieuDang.Core.UnitTests

### Qt11PeriodStatusByYearTests

- QT11 · Đợt 29/02 ở năm không nhuận xét theo 28/02 — ĐẠT
- QT11 · Quá tải không tham số năm vẫn xét đúng năm hiện tại — ĐẠT
- QT11 · Năm đã qua thì mọi đợt là Đã qua, năm sau thì là Sắp tới — ĐẠT
- QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026(fromDay: 1, fromMonth: 10, toDay: 7, toMonth: 11, expected: Upcoming, daysLeft: 12) — ĐẠT
- QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026(fromDay: 15, fromMonth: 8, toDay: 10, toMonth: 9, expected: Past, daysLeft: null) — ĐẠT
- QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026(fromDay: 15, fromMonth: 1, toDay: 5, toMonth: 3, expected: Past, daysLeft: null) — ĐẠT
- QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026(fromDay: 1, fromMonth: 9, toDay: 30, toMonth: 9, expected: Ongoing, daysLeft: null) — ĐẠT
- QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026(fromDay: 19, fromMonth: 9, toDay: 19, toMonth: 9, expected: Ongoing, daysLeft: null) — ĐẠT

### Qt11PeriodStatusTests

- GetPeriodStatus_WhenThePeriodHasNotStarted_ReturnsUpcomingWithDaysLeft — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt11PeriodStatusTests.GetPeriodStatus_ForEveryFixtureScenario_MatchesTheExpectedStatuses(scenarioName: "core_default_T1") — ĐẠT
- GetPeriodStatus_WhenTodayIsInsideThePeriod_ReturnsOngoingWithoutDaysLeft — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt11PeriodStatusTests.GetPeriodStatus_ForEveryFixtureScenario_MatchesTheExpectedStatuses(scenarioName: "core_default_T0") — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt11PeriodStatusTests.GetPeriodStatus_ForEveryFixtureScenario_MatchesTheExpectedStatuses(scenarioName: "core_default_T0_fullCover") — ĐẠT
- GetPeriodStatus_OneDayBeforeThePeriodStarts_ReturnsOneDayLeft — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt11PeriodStatusTests.GetPeriodStatus_ForEveryFixtureScenario_MatchesTheExpectedStatuses(scenarioName: "core_default_T0_widenedP3") — ĐẠT
- BindToYear_ForAPeriodEndingOn29February_KeepsTheDayInALeapYear — ĐẠT
- GetPeriodStatus_OnTheFirstDayOfThePeriod_ReturnsOngoing — ĐẠT
- GetPeriodStatus_WhenThePeriodAlreadyEnded_ReturnsPastWithoutDaysLeft — ĐẠT
- GetPeriodStatus_OneDayAfterThePeriodEnds_ReturnsPast — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt11PeriodStatusTests.GetPeriodStatus_ForEveryFixtureScenario_MatchesTheExpectedStatuses(scenarioName: "core_default_T0_leapEdgePeriod") — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt11PeriodStatusTests.GetPeriodStatus_ForEveryFixtureScenario_MatchesTheExpectedStatuses(scenarioName: "core_default_T2") — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt11PeriodStatusTests.GetPeriodStatus_ForEveryFixtureScenario_MatchesTheExpectedStatuses(scenarioName: "core_default_T0_overlap") — ĐẠT
- GetPeriodStatus_OnTheLastDayOfThePeriod_ReturnsOngoing — ĐẠT
- GetPeriodStatus_ForAPeriodStartingOn29FebruaryOfANonLeapYear_BindsTo28February — ĐẠT

### Qt1MilestoneSequenceTests

- HuyHieuDang.Core.UnitTests.Qt1MilestoneSequenceTests.BuildMilestones_WhenStepIsLessThanOne_ThrowsBadRequest(step: -1) — ĐẠT
- BuildMilestones_WithAStepLargerThanTheRange_ReturnsOnlyTheFirstMilestone — ĐẠT
- BuildMilestones_WhenStartIsGreaterThanEnd_ThrowsBadRequest — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt1MilestoneSequenceTests.BuildMilestones_WhenStepIsLessThanOne_ThrowsBadRequest(step: 0) — ĐẠT
- BuildMilestones_WhenStartEqualsEnd_ReturnsSingleMilestone — ĐẠT
- BuildMilestones_WhenStepDoesNotLandOnEnd_StopsBeforeExceedingEnd — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt1MilestoneSequenceTests.BuildMilestones_WhenAnyBoundIsNotPositive_ThrowsBadRequest(start: 0, end: 90, step: 5) — ĐẠT
- BuildMilestones_WithStepTen_Returns7Milestones — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt1MilestoneSequenceTests.BuildMilestones_WhenAnyBoundIsNotPositive_ThrowsBadRequest(start: -30, end: 90, step: 5) — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt1MilestoneSequenceTests.BuildMilestones_WhenAnyBoundIsNotPositive_ThrowsBadRequest(start: 30, end: 0, step: 5) — ĐẠT
- BuildMilestones_WithDefaultSettings_Returns13MilestonesFrom30To90 — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt1MilestoneSequenceTests.BuildMilestones_WhenAnyBoundIsNotPositive_ThrowsBadRequest(start: 30, end: -90, step: 5) — ĐẠT

### Qt2AnniversaryTests

- GetAnniversary_WhenAdmittedOn29FebruaryAndTargetYearIsLeap_Keeps29February — ĐẠT
- GetAnniversary_ForOrdinaryDate_KeepsDayAndMonth — ĐẠT
- GetAnniversary_WithMilestoneZero_ReturnsAdmissionDate — ĐẠT
- GetAnniversary_WhenMilestoneIsNegative_ThrowsBadRequest — ĐẠT
- GetAnniversary_WhenAdmittedOn29FebruaryAndTargetYearIsNotLeap_FallsBackTo28February — ĐẠT

### Qt3PartyAgeTests

- GetPartyAge_BeforeThisYearAnniversary_DoesNotCountTheCurrentYear — ĐẠT
- GetPartyAge_ForLeapDayAdmission_CountsOn28FebruaryOfNonLeapYear — ĐẠT
- GetPartyAge_ForTheOldestMember_ReturnsAgeBeyondTheHighestMilestone — ĐẠT
- GetPartyAge_OneDayBeforeTheAnniversary_ReturnsPreviousCount — ĐẠT
- GetPartyAge_WhenAdmittedToday_ReturnsZero — ĐẠT
- GetPartyAge_ForLeapDayAdmission_OneDayBefore28February_DoesNotCountTheYear — ĐẠT
- GetPartyAge_WhenAdmissionIsInTheFuture_ReturnsZero — ĐẠT
- GetPartyAge_OnTheAnniversaryDay_CountsThatYear — ĐẠT

### Qt3aNextMilestoneTests

- GetNextMilestone_WhenPartyAgeEqualsAMilestone_ReturnsTheFollowingMilestone — ĐẠT
- GetNextMilestone_WhenPartyAgeIsJustUnderAMilestone_ReturnsThatMilestone — ĐẠT
- GetNextMilestone_WhenPartyAgeIsBelowTheFirstMilestone_ReturnsTheFirstMilestone — ĐẠT
- GetNextMilestone_WhenPartyAgeIsAboveTheHighestMilestone_ReturnsNull — ĐẠT
- GetNextAnniversary_WhenPartyAgeIsAboveTheHighestMilestone_ReturnsNull — ĐẠT
- GetNextMilestone_WithStepTen_SkipsTheMilestonesThatDisappear — ĐẠT
- GetPartyAge_MatchesTheExpectedMemberListOfTheFixtures — ĐẠT
- GetNextAnniversary_ForAMemberBelowTheFirstMilestone_ReturnsTheMilestoneDate — ĐẠT
- GetNextMilestone_MatchesTheExpectedMemberListOfTheFixtures — ĐẠT
- GetNextMilestone_WhenPartyAgeEqualsTheHighestMilestone_ReturnsNull — ĐẠT

### Qt3bAdmissionDateRangeTests

- GetLatestAdmissionDateForAge_WhenAgeIsNegative_Throws — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt3bAdmissionDateRangeTests.GetAdmissionDateRange_AgreesWithGetNextMilestoneAroundLeapDays(year: 2026, month: 2, day: 28) — ĐẠT
- GetAdmissionDateRange_ForTheFirstMilestone_HasNoUpperBound — ĐẠT
- GetLatestAdmissionDateForAge_WhenTodayIs29February_LandsOn28FebruaryOfANonLeapYear — ĐẠT
- GetAdmissionDateRange_WhenMilestoneIsNotInTheSequence_Throws — ĐẠT
- GetAdmissionDateRange_ForNone_HasNoLowerBound — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt3bAdmissionDateRangeTests.GetAdmissionDateRange_AgreesWithGetNextMilestoneAroundLeapDays(year: 2026, month: 12, day: 31) — ĐẠT
- GetLatestAdmissionDateForAge_WhenTodayIs28FebruaryOfALeapYear_ExcludesTheLeapDay — ĐẠT
- GetLatestAdmissionDateForAge_WhenTodayIs28February_IncludesTheLeapDay — ĐẠT
- GetAdmissionDateRange_ForAMiddleMilestone_IsHalfOpen — ĐẠT
- GetLatestAdmissionDateForAge_SubtractsWholeYears — ĐẠT
- GetAdmissionDateRange_FollowsTheMilestoneSettings — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt3bAdmissionDateRangeTests.GetAdmissionDateRange_AgreesWithGetNextMilestoneAroundLeapDays(year: 2024, month: 2, day: 29) — ĐẠT
- GetAdmissionDateRange_AgreesWithGetNextMilestoneOnEveryFixtureMember — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt3bAdmissionDateRangeTests.GetAdmissionDateRange_AgreesWithGetNextMilestoneAroundLeapDays(year: 2027, month: 1, day: 1) — ĐẠT
- GetLatestAdmissionDateForAge_WhenAgeIsZero_ReturnsToday — ĐẠT

### Qt4EligibilityTests

- HuyHieuDang.Core.UnitTests.Qt4EligibilityTests.GetEligibleMilestone_OverTheWholeCoreDataset_MatchesTheExpectedCounts(scenarioName: "core_default_T0_widenedP3") — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt4EligibilityTests.GetEligibleMilestone_OverTheWholeCoreDataset_MatchesTheExpectedCounts(scenarioName: "core_default_T0_leapEdgePeriod") — ĐẠT
- GetEligibleMilestone_WhenAnniversaryIsOneDayBeforeThePeriod_ReturnsNull — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt4EligibilityTests.GetEligibleMilestone_OverTheWholeCoreDataset_MatchesTheExpectedCounts(scenarioName: "core_step10_T0") — ĐẠT
- GetEligibleMilestone_ForTheMemberAtTheHighestMilestone_StillReturns90 — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt4EligibilityTests.GetEligibleMilestone_OverTheWholeCoreDataset_MatchesTheExpectedCounts(scenarioName: "core_default_T0_overlap") — ĐẠT
- GetEligibleMilestone_WhenTheMemberHasPassedTheHighestMilestone_ReturnsNull — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt4EligibilityTests.GetEligibleMilestone_OverTheWholeCoreDataset_MatchesTheExpectedCounts(scenarioName: "core_default_T0") — ĐẠT
- GetEligibleMilestone_ForAYearWithoutAnyMilestone_ReturnsNull — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt4EligibilityTests.GetEligibleMilestone_OverTheWholeCoreDataset_MatchesTheExpectedCounts(scenarioName: "core_default_T0_fullCover") — ĐẠT
- GetEligibleMilestone_WhenLeapDayAnniversaryShiftsTo28February_StillFallsInThePeriod — ĐẠT
- GetEligibleMilestone_WhenThePeriodStartsOn29FebruaryOfANonLeapYear_BindsTo28February — ĐẠT
- GetEligibleMilestone_WithStepTen_LosesTheMemberWhoseMilestoneDisappears — ĐẠT
- GetEligibleMilestone_WithStepTen_KeepsTheMemberWhoseMilestoneSurvives — ĐẠT
- GetEligibleMilestone_WhenAnniversaryEqualsTheLastDayOfThePeriod_ReturnsTheMilestone — ĐẠT
- GetEligibleMilestone_WhenAnniversaryIsOneDayAfterThePeriod_ReturnsNull — ĐẠT
- GetEligibleMilestone_WhenAnniversaryEqualsTheFirstDayOfThePeriod_ReturnsTheMilestone — ĐẠT

### Qt6PeriodWarningTests

- GetGaps_InALeapYear_EndsTheGapOnTheCorrectFebruaryDay — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt6PeriodWarningTests.BindToYear_WhenTheDayOrMonthDoesNotExist_ThrowsBadRequest(day: 30, month: 2) — ĐẠT
- GetOverlaps_WhenTwoPeriodsShareASingleDay_ReportsThem — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt6PeriodWarningTests.BindToYear_WhenTheDayOrMonthDoesNotExist_ThrowsBadRequest(day: 1, month: 13) — ĐẠT
- GetOverlaps_WhenTwoPeriodsIntersect_ReturnsThePairInOrder — ĐẠT
- BindToYear_WhenThePeriodStartsOn29February_IsStillAccepted — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt6PeriodWarningTests.BindToYear_WhenTheDayOrMonthDoesNotExist_ThrowsBadRequest(day: 31, month: 4) — ĐẠT
- GetGaps_ForTheMainPeriods_LabelsTheFirstAndLastGapByPosition — ĐẠT
- GetGaps_WhenThereIsNoPeriod_ReturnsTheWholeYearAsOneGap — ĐẠT
- BindToYear_WhenTheFromDateIsAfterTheToDate_EndsThePeriodInTheNextYear — ĐẠT
- GetOverlaps_WhenPeriodsAreBackToBack_ReturnsNoWarning — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt6PeriodWarningTests.BindToYear_WhenTheDayOrMonthDoesNotExist_ThrowsBadRequest(day: 31, month: 2) — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt6PeriodWarningTests.BindToYear_WhenTheDayOrMonthDoesNotExist_ThrowsBadRequest(day: 32, month: 1) — ĐẠT
- GetGaps_WhenThePeriodsCoverTheWholeYear_ReturnsNoGap — ĐẠT
- GetOverlaps_ForTheMainPeriods_ReturnsNoWarning — ĐẠT
- GetGaps_ForTheMainPeriods_ReturnsTheFiveExpectedGapsOf2026 — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt6PeriodWarningTests.BindToYear_WhenTheDayOrMonthDoesNotExist_ThrowsBadRequest(day: 0, month: 1) — ĐẠT
- GetGaps_WhenThereIsNoPeriod_LabelsTheWholeYearAsBeforeTheFirstPeriod — ĐẠT

### Qt6SpanningYearPeriodTests

- GetPeriodStatus_ForAYearFarInTheFuture_StillBindsToThatYear — ĐẠT
- GetEligibleMilestone_WhenTheAnniversaryFallsInTheGapOfTheYear_ReturnsNull — ĐẠT
- GetEligibleMilestone_WhenTheAnniversaryFallsInTheFirstCalendarYear_ReturnsTheMilestone — ĐẠT
- GetPeriodStatus_OnADayInsideTheOccurrenceStartedLastYear_IsOngoing — ĐẠT
- GetPeriodStatus_OnADayBetweenTwoOccurrences_CountsDownToTheNextFromDate — ĐẠT
- GetSlicesInYear_ForASpanningPeriod_ReturnsTheTailAndTheHeadOfTheYear — ĐẠT
- GetMissedMilestone_WhenTheAnniversaryFallsInTheTailOfTheSpanningPeriod_ReturnsNull — ĐẠT
- GetMissedMilestone_WhenTheAnniversaryFallsInTheMiddleOfTheYear_ReturnsTheGap — ĐẠT
- GetUpcomingPeriod_WhenTheSpanningPeriodStartedLastYear_PicksThatOccurrence — ĐẠT
- GetOverlaps_WhenAnotherPeriodTouchesTheTailOfTheSpanningPeriod_ReportsTheSharedDays — ĐẠT
- BindToYear_ForASpanningPeriod_PutsTheToDateInTheFollowingYear — ĐẠT
- GetEligibleMilestone_WhenTheAnniversaryFallsAfterTheNewYear_ReturnsTheMilestone — ĐẠT
- GetOverlaps_ForASpanningPeriodAlone_DoesNotReportThePeriodAgainstItself — ĐẠT
- GetUpcomingPeriod_WhenTheSpanningPeriodOfLastYearHasEnded_PicksThisYearOccurrence — ĐẠT
- GetGaps_ForASpanningPeriodAlone_LeavesOnlyTheMiddleOfTheYearUncovered — ĐẠT
- GetGaps_WhenTwoSpanningPeriodsCoverTheWholeYear_ReturnsNoGap — ĐẠT
- BindToYear_ForASpanningPeriodEndingOn29February_KeepsTheLeapDayInALeapYear — ĐẠT

### Qt7MissedMilestoneTests

- HuyHieuDang.Core.UnitTests.Qt7MissedMilestoneTests.GetMissedMilestone_OverTheWholeCoreDataset_MatchesTheExpectedRows(year: 2028) — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt7MissedMilestoneTests.GetMissedMilestone_OverTheWholeCoreDataset_MatchesTheExpectedRows(year: 2025) — ĐẠT
- GetMissedMilestone_WithStepTen_DropsTheMemberWhoseMilestoneDisappears — ĐẠT
- GetMissedMilestone_WhenTheAnniversaryFallsInsideAPeriod_ReturnsNull — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt7MissedMilestoneTests.GetMissedMilestone_OverTheWholeCoreDataset_MatchesTheExpectedRows(year: 2026) — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt7MissedMilestoneTests.GetMissedMilestone_OverTheWholeCoreDataset_MatchesTheExpectedRows(year: 2027) — ĐẠT
- GetMissedMilestone_WhenTheAnniversaryFallsBeforeTheFirstPeriod_LabelsItBeforeTheFirst — ĐẠT
- GetMissedMilestone_WhenTheAnniversaryFallsAfterTheLastPeriod_LabelsItAfterTheLast — ĐẠT
- GetMissedMilestone_WhenNoMilestoneFallsInTheYear_ReturnsNull — ĐẠT
- GetMissedMilestone_WhenThereIsNoPeriodAtAll_ReturnsTheWholeYearGap — ĐẠT
- GetMissedMilestone_WhenTheAnniversaryFallsBetweenTwoPeriods_NamesBothPeriods — ĐẠT

### Qt8UpcomingPeriodTests

- HuyHieuDang.Core.UnitTests.Qt8UpcomingPeriodTests.GetUpcomingPeriod_ForEveryFixtureScenario_MatchesTheExpectedPeriod(scenarioName: "core_default_T0_fullCover") — ĐẠT
- GetUpcomingPeriod_WhenTwoPeriodsOverlap_PicksTheOneWithTheEarliestStart — ĐẠT
- GetUpcomingPeriod_WhenEveryPeriodOfThisYearHasPassed_KeepsTheSameTieBreakNextYear — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt8UpcomingPeriodTests.GetUpcomingPeriod_ForEveryFixtureScenario_MatchesTheExpectedPeriod(scenarioName: "core_default_T2") — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt8UpcomingPeriodTests.GetUpcomingPeriod_ForEveryFixtureScenario_MatchesTheExpectedPeriod(scenarioName: "core_default_T0_overlap") — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt8UpcomingPeriodTests.GetUpcomingPeriod_ForEveryFixtureScenario_MatchesTheExpectedPeriod(scenarioName: "core_default_T0_widenedP3") — ĐẠT
- GetUpcomingPeriod_AtT0_ReturnsTheNextPeriodOfTheCurrentYearWithDaysLeft — ĐẠT
- GetUpcomingPeriod_WhenThereIsNoPeriod_ReturnsNull — ĐẠT
- GetUpcomingPeriod_OneDayAfterTheLastPeriodEnds_RollsOverToNextYear — ĐẠT
- GetUpcomingPeriod_WhenTwoPeriodsShareTheSameStartDate_DoesNotDependOnTheInputOrder — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt8UpcomingPeriodTests.GetUpcomingPeriod_ForEveryFixtureScenario_MatchesTheExpectedPeriod(scenarioName: "core_default_T0_noPeriod") — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt8UpcomingPeriodTests.GetUpcomingPeriod_ForEveryFixtureScenario_MatchesTheExpectedPeriod(scenarioName: "core_default_T0") — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt8UpcomingPeriodTests.GetUpcomingPeriod_ForEveryFixtureScenario_MatchesTheExpectedPeriod(scenarioName: "core_default_T0_leapEdgePeriod") — ĐẠT
- GetUpcomingPeriod_WhenEveryPeriodOfTheYearHasPassed_ReturnsTheEarliestOfNextYear — ĐẠT
- GetUpcomingPeriod_OnTheLastDayOfAPeriod_StillReturnsThatPeriodAsOngoing — ĐẠT
- GetUpcomingPeriod_WhenTwoPeriodsShareTheSameStart_PrefersTheEarlierEndThenTheName — ĐẠT
- GetUpcomingPeriod_ForAPeriodStartingOn29February_BindsNextYearTo28February — ĐẠT
- GetUpcomingPeriod_WhenTodayIsInsideAPeriod_ReturnsThatPeriodAsOngoing — ĐẠT
- HuyHieuDang.Core.UnitTests.Qt8UpcomingPeriodTests.GetUpcomingPeriod_ForEveryFixtureScenario_MatchesTheExpectedPeriod(scenarioName: "core_default_T1") — ĐẠT

## HuyHieuDang.Infrastructure.IntegrationTests

### SkeletonIntegrationTests

- DatabaseSettings_ShouldDefaultToEmptyConnectionString — ĐẠT

## HuyHieuDang.Infrastructure.UnitTests

### AwardPeriodCoverageBuilderTests

- QT6 · Đợt nằm lọt trong đợt khác: báo chồng lấn, không cắt thêm đoạn — ĐẠT
- QT6 · Bộ đợt mẫu cho đúng một cặp chồng lấn kèm khoảng ngày dùng chung — ĐẠT
- QT6 · Đợt vắt qua 31/12 để lại hai đoạn trong cùng một năm — ĐẠT
- QT6 · Bộ đợt mẫu cho đúng hai khoảng trống kèm tên hai đợt kề — ĐẠT
- QT6 · Đợt vắt năm một mình không tự chồng lấn, chỉ hở khoảng giữa năm — ĐẠT
- QT6 · Chưa cài đợt nào: dải vẫn là một khoảng trống cả năm, cảnh báo gaps rỗng — ĐẠT
- QT2 · Đợt 29/02 ở năm không nhuận lùi về 28/02 — ĐẠT
- UC-36 · Dải độ phủ liền mạch 01/01–31/12, phần chồng lấn thuộc đợt đến trước — ĐẠT

### BusinessSchemaTests

- PartyMember_FullName_ShouldUseVietnameseCollation — ĐẠT
- PartyMember_DateColumns_ShouldBeDateOnly — ĐẠT
- AppSetting_ShouldDefaultTo30_90_5_WithoutUnitName — ĐẠT
- Model_ShouldNotContainAnyEligibilityResultTable — ĐẠT
- PartyMember_OfficialAdmissionDate_ShouldBeRequired — ĐẠT
- HuyHieuDang.Infrastructure.UnitTests.BusinessSchemaTests.BusinessEntities_ShouldMapToUnderscoreTables(entityType: typeof(HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities.AwardPeriod), tableName: "award_period") — ĐẠT
- AwardPeriod_Name_ShouldBeUniqueAndCaseInsensitive — ĐẠT
- AwardPeriod_ShouldNotStoreAnyYear — ĐẠT
- Gender_ShouldOnlyOfferMaleAndFemale — ĐẠT
- HuyHieuDang.Infrastructure.UnitTests.BusinessSchemaTests.BusinessEntities_ShouldMapToUnderscoreTables(entityType: typeof(HuyHieuDang.Infrastructure.Modules.AppSettings.Entities.AppSetting), tableName: "app_setting") — ĐẠT
- HuyHieuDang.Infrastructure.UnitTests.BusinessSchemaTests.BusinessEntities_ShouldMapToUnderscoreTables(entityType: typeof(HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities.PartyMember), tableName: "party_member") — ĐẠT

### DateTimeProviderTests

- HuyHieuDang.Infrastructure.UnitTests.DateTimeProviderTests.Today_ShouldIgnoreMalformedTestVariable(value: "2026-13-40") — ĐẠT
- Now_ShouldUseVietnamOffset — ĐẠT
- TestTodayVariable_ShouldKeepTheAgreedName — ĐẠT
- HuyHieuDang.Infrastructure.UnitTests.DateTimeProviderTests.Today_ShouldIgnoreMalformedTestVariable(value: "khong-phai-ngay") — ĐẠT
- HuyHieuDang.Infrastructure.UnitTests.DateTimeProviderTests.Today_ShouldIgnoreMalformedTestVariable(value: "15/10/2026") — ĐẠT
- HuyHieuDang.Infrastructure.UnitTests.DateTimeProviderTests.Today_ShouldIgnoreMalformedTestVariable(value: "2026-10-15T00:00:00") — ĐẠT
- Now_ShouldTrackUtcClock — ĐẠT
- HuyHieuDang.Infrastructure.UnitTests.DateTimeProviderTests.Today_ShouldIgnoreMalformedTestVariable(value: "   ") — ĐẠT
- Today_ShouldBeTheVietnamCalendarDay — ĐẠT
- Today_ShouldFollowTestVariable_OutsideProduction — ĐẠT
- Today_ShouldIgnoreTestVariable_InProduction — ĐẠT

### PartyMemberImportRowValidatorTests

- 29/02 năm nhuận là ngày có thật, không bị coi là sai định dạng — ĐẠT
- Thiếu ngày vào Đảng (dự bị) → MissingOfficialAdmissionDate, không kèm lỗi định dạng — ĐẠT
- OQ-2: một dòng nhiều lỗi trả đủ mọi lý do, đúng thứ tự bảng mã lỗi — ĐẠT
- OQ-5: giới tính không phân biệt hoa thường(cell: "nam", expected: "Male") — ĐẠT
- Thiếu họ tên → MissingFullName — ĐẠT
- Ngày vào Đảng (dự bị) ở tương lai → FutureOfficialAdmissionDate — ĐẠT
- Ngày vào Đảng (dự bị) đúng bằng hôm nay là hợp lệ — ĐẠT
- Ngày sinh sau ngày vào Đảng (dự bị) → BirthDateAfterAdmissionDate — ĐẠT
- 29/02 năm không nhuận là ngày không có thật → InvalidDateFormat — ĐẠT
- OQ-10: ngày sinh bằng đúng ngày vào Đảng (dự bị) vẫn là lỗi — ĐẠT
- OQ-5: giới tính không phân biệt hoa thường(cell: "nữ", expected: "Female") — ĐẠT
- OQ-5: giới tính không phân biệt hoa thường(cell: "NỮ", expected: "Female") — ĐẠT
- Dòng đủ bốn ô hợp lệ thì không có lỗi và được chuẩn hóa — ĐẠT
- OQ-5: giới tính không phân biệt hoa thường(cell: "NAM", expected: "Male") — ĐẠT
- Ngày sinh và giới tính bỏ trống vẫn hợp lệ, trả null — ĐẠT
- OQ-1: ngày sinh 31/02/1974 không có thật → InvalidDateFormat — ĐẠT
- OQ-5: giới tính không phân biệt hoa thường(cell: "Nữ", expected: "Female") — ĐẠT
- Ngày vào Đảng (dự bị) dạng yyyy-MM-dd → InvalidDateFormat — ĐẠT
- OQ-6: ngày nhận cả một chữ số lẫn hai chữ số(birth: "9/2/1975", admission: "1/10/1996") — ĐẠT
- Giới tính lạ → InvalidGender — ĐẠT
- OQ-6: ngày nhận cả một chữ số lẫn hai chữ số(birth: "9/02/1975", admission: "01/10/1996") — ĐẠT
- OQ-6: ngày nhận cả một chữ số lẫn hai chữ số(birth: "09/02/1975", admission: "01/10/1996") — ĐẠT

### QueryFilterValidationTests

- ToanTuNullTrenTruongCoTheNull_VanLocDung — ĐẠT
- ToanTuBtwTrenNgayHopLe_VanLocDung — ĐẠT
- GiaTriEnumHopLe_VanLocDung — ĐẠT
- HuyHieuDang.Infrastructure.UnitTests.QueryFilterValidationTests.GiaTriEnumSaiKieu_Tra400(value: "$eq:") — ĐẠT
- BanIEnumerable_CungHanhVi — ĐẠT
- ToanTuIlikeVaSwTrenChuoi_VanLocDung — ĐẠT
- HuyHieuDang.Infrastructure.UnitTests.QueryFilterValidationTests.GiaTriEnumSaiKieu_Tra400(value: "$not:$eq:Khac") — ĐẠT
- GiaTriSoSaiKieu_Tra400 — ĐẠT
- TienToNotVoiGiaTriHopLe_VanLocDung — ĐẠT
- ToanTuIlikeTrenTruongKhongPhaiChuoi_Tra400 — ĐẠT
- HuyHieuDang.Infrastructure.UnitTests.QueryFilterValidationTests.GiaTriEnumSaiKieu_Tra400(value: "$in:Male,Khac") — ĐẠT
- DanhSachRong_VanTra400ChoGiaTriLa — ĐẠT
- KhongCoBoLoc_GiuNguyenDanhSach — ĐẠT
- HuyHieuDang.Infrastructure.UnitTests.QueryFilterValidationTests.GiaTriEnumSaiKieu_Tra400(value: "$eq:Khac") — ĐẠT
- ToanTuKhongTonTaiHoacThieuToanTu_Tra400 — ĐẠT
- HuyHieuDang.Infrastructure.UnitTests.QueryFilterValidationTests.GiaTriEnumSaiKieu_Tra400(value: "$eq:1") — ĐẠT
- ToanTuInHopLe_VanLocDung — ĐẠT
- TenTruongKhongTonTai_GiuNguyenHanhViCu — ĐẠT
- ToanTuInTrenTruongCoTheNull_VanLocDung — ĐẠT
- ToanTuNullTrenTruongKhongTheNull_Tra400 — ĐẠT
- ToanTuBtwHopLe_VanLocDung — ĐẠT

### SkeletonTests

- User_ShouldSupportPasswordVerification — ĐẠT
- User_ShouldBeAJwtUser — ĐẠT

### TestTodayVariableTests

- WithoutTheVariable_NothingIsForcedAndNothingIsWarned — ĐẠT
- InProduction_TheVariableIsIgnoredAndWarned — ĐẠT
- OutsideProduction_TheVariableForcesToday — ĐẠT

## HuyHieuDang.Web.IntegrationTests

### AnonymousEndpointTests

- A-007 · Chỉ POST /api/Auth/Login được [AllowAnonymous] — ĐẠT
- Mọi controller đều kế thừa BaseController nên mặc định cần đăng nhập — ĐẠT

### AuthEndpointTests

- Hạn token đúng 8 giờ theo hợp đồng mục 1.2 — ĐẠT
- Token hợp lệ nhưng chủ thể không còn tồn tại trả 401 — ĐẠT
- A-006 · Token vừa cấp dùng được ngay cho endpoint khác — ĐẠT
- A-001 · Gọi endpoint nghiệp vụ không kèm token trả 401 và không lộ dữ liệu(method: "GET", path: "/api/Auth/Me") — ĐẠT
- A-004 · JWT thiếu tiền tố Bearer trả 401 — ĐẠT
- A-005 · Sai mật khẩu và không có tài khoản trả cùng một thông báo 401 — ĐẠT
- Thiếu tài khoản hoặc mật khẩu trả 400 kèm khóa Required(username: "", password: "Kiem@Thu123", expectedProperty: "Username") — ĐẠT
- A-002 · JWT sai chữ ký trả 401 — ĐẠT
- Đăng xuất có token trả 200 và khóa Mes.User.Logout.Successfully — ĐẠT
- Thiếu tài khoản hoặc mật khẩu trả 400 kèm khóa Required(username: "admin", password: "", expectedProperty: "Password") — ĐẠT
- A-006 · Đăng nhập đúng trả 200 kèm token đúng hợp đồng — ĐẠT
- A-003 · JWT đã hết hạn trả 401 — ĐẠT
- A-001 · Gọi endpoint nghiệp vụ không kèm token trả 401 và không lộ dữ liệu(method: "POST", path: "/api/Auth/Logout") — ĐẠT

### AwardPeriodEndpointTests

- 5.1 → 5.5 · Thiếu token trả 401(method: "DELETE", path: "/api/AwardPeriods/9b7c0f9c-0000-0000-0000-00000000"···) — ĐẠT
- 5.1 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(query: "?year=1899") — ĐẠT
- 5.3 · Đợt vắt qua 31/12 được lưu, Đến ngày rơi vào năm sau — ĐẠT
- 5.4 · Sửa sang tên đợt khác trả Repeated.Name; id lạ trả NotFound — ĐẠT
- 5.3 · Thêm đợt chồng lấn vẫn là 200 và trả kèm cảnh báo — ĐẠT
- 5.1 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(query: "?year=2201") — ĐẠT
- 5.1 · Bốn đợt mẫu: sắp theo fromDate, ba trạng thái và số người đủ điều kiện — ĐẠT
- 5.2 · Lấy một đợt theo id và theo năm được hỏi; id lạ trả NotFound — ĐẠT
- 5.3 · Hai lỗi chặn lưu: thiếu tên, ngày không có thật(name: "Đợt 31/04", fromDay: 31, fromMonth: 4, toDay: 7, toMonth: 11, expectedKey: "Mes.AwardPeriod.Invalid.FromDate") — ĐẠT
- 5.5 · Xóa hẳn đợt, trả khoảng trống mới và không đụng đảng viên (QT10) — ĐẠT
- 5.1 → 5.5 · Thiếu token trả 401(method: "GET", path: "/api/AwardPeriods") — ĐẠT
- 5.3 · Hai lỗi chặn lưu: thiếu tên, ngày không có thật(name: null, fromDay: 1, fromMonth: 10, toDay: 7, toMonth: 11, expectedKey: "Mes.AwardPeriod.Required.Name") — ĐẠT
- 5.1 · Năm khác: trạng thái vẫn so với hôm nay, ngày gắn đúng năm được hỏi — ĐẠT
- 5.3 · Trùng tên không phân biệt hoa thường trả Mes.AwardPeriod.Repeated.Name — ĐẠT
- 5.1 → 5.5 · Thiếu token trả 401(method: "POST", path: "/api/AwardPeriods") — ĐẠT
- 5.1 · Dải độ phủ liền mạch 01/01–31/12, phần chồng lấn thuộc đợt đến trước — ĐẠT
- 5.3 · Hai lỗi chặn lưu: thiếu tên, ngày không có thật(name: "Đợt đến 31/11", fromDay: 1, fromMonth: 10, toDay: 31, toMonth: 11, expectedKey: "Mes.AwardPeriod.Invalid.ToDate") — ĐẠT
- 5.1 · Chưa cài đợt nào: cảnh báo gaps rỗng, dải vẫn phủ trọn 01/01–31/12 — ĐẠT
- 5.4 · Sửa đợt: giữ nguyên tên của chính nó, có hiệu lực ngay cho năm hiện tại — ĐẠT
- 5.1 · Cảnh báo liệt kê đúng một cặp chồng lấn và một khoảng trống — ĐẠT

### AwardPeriodFilterValueTests

- QC-T27-05 · Giá trị lọc lạ trên danh sách đợt trả 400, không trả cả kho(filter: "filter.ToDay=$gte:hom-qua") — ĐẠT
- QC-T27-05 · Kho rỗng cũng phải từ chối giá trị lọc sai kiểu — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trên danh sách đợt trả 400, không trả cả kho(filter: "filter.FromMonth=$eq:") — ĐẠT
- 5.1 · Tên trường không tồn tại vẫn được bỏ qua, không phải lỗi — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trên danh sách đợt trả 400, không trả cả kho(filter: "filter.FromMonth=$eq:khong-phai-so") — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trên danh sách đợt trả 400, không trả cả kho(filter: "filter.Id=$eq:khong-phai-guid") — ĐẠT
- 5.1 · Giá trị lọc hợp lệ lọc đúng; cảnh báo và độ phủ vẫn tính trên cả năm — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trên danh sách đợt trả 400, không trả cả kho(filter: "filter.SpansNextYear=$eq:co") — ĐẠT

### DashboardEndpointTests

- 6.1 · Thiếu token trả 401 — ĐẠT
- 6.1 · T2 = 01/12/2026: mọi đợt 2026 đã qua nên đợt sắp tới là Đợt 3/2 · 2027 (QT8) — ĐẠT
- 6.1 · T1 = 15/10/2026: Đợt 7/11 đang diễn ra, daysRemaining = null — ĐẠT
- 6.1 · T0 = 19/09/2026: đợt sắp tới là Đợt 7/11 · 2026, còn 12 ngày, 6 người — ĐẠT
- 6.1 · Kho trống hoàn toàn: đủ hai cảnh báo cho khối hướng dẫn ba bước (UC-13) — ĐẠT
- 6.1 · Cảnh báo của Dashboard trùng khớp với cảnh báo màn Đợt — ĐẠT
- 6.1 · Chưa cài đợt nào: upcomingPeriod = null, bảng rỗng, cảnh báo noPeriods — ĐẠT
- 6.1 · Chưa có đảng viên nào: cảnh báo noMembers, đợt sắp tới vẫn có, 0 người — ĐẠT

### EligibilityEndpointTests

- 6.2 · Bỏ trống year thì lấy năm hiện tại của máy chủ — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P1", year: 2028) — ĐẠT
- 6.2 · Id đợt lạ trả Mes.AwardPeriod.NotFound — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P2", year: 2028) — ĐẠT
- 6.2 → 6.4 · Thiếu token trả 401(path: "/api/Eligibility") — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P3", year: 2027) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P4", year: 2028) — ĐẠT
- 6.2 → 6.4 · Thiếu token trả 401(path: "/api/Eligibility/Unassigned") — ĐẠT
- 6.3 · Các năm khác của bộ lõi khớp expected.json(year: 2027) — ĐẠT
- 6.4 · Badge luôn bằng số dòng của màn Chưa thuộc đợt nào — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P1", year: 2025) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P2", year: 2027) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P3", year: 2028) — ĐẠT
- 6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Eligibility", year: 2201) — ĐẠT
- 6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Eligibility/UnassignedCount", year: 2201) — ĐẠT
- 6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Eligibility/Unassigned", year: -5) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P3", year: 2026) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P4", year: 2026) — ĐẠT
- 6.3 · Chưa thuộc đợt nào năm 2026: 7 người, đúng nhãn khoảng trống (QT7) — ĐẠT
- 6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Eligibility/Unassigned", year: 0) — ĐẠT
- 6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Eligibility", year: 1899) — ĐẠT
- 6.3 · Các năm khác của bộ lõi khớp expected.json(year: 2025) — ĐẠT
- 6.2 → 6.4 · Thiếu token trả 401(path: "/api/Eligibility/UnassignedCount") — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P1", year: 2027) — ĐẠT
- 6.3 · Các năm khác của bộ lõi khớp expected.json(year: 2028) — ĐẠT
- 6.3 · Chưa cài đợt nào: mọi người tròn mốc đều rơi vào Trước đợt đầu tiên — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P4", year: 2025) — ĐẠT
- 6.3 · Đổi Bước 5 → 10 làm danh sách còn 6 người ngay (QT1, QT5) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P2", year: 2025) — ĐẠT
- 6.2 · Nới Đến ngày Đợt 2/9 sang 30/09: đợt lên 5 người ngay, không lưu gì (QT5) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P2", year: 2026) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P1", year: 2026) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P4", year: 2027) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json(periodCode: "P3", year: 2025) — ĐẠT

### EligibilityPerformanceTests

- 6.1 → 6.4 · 10.000 đảng viên: mỗi endpoint tính toán dưới 1 giây — ĐẠT

### ExportEndpointTests

- 8.3 · Chưa thuộc đợt nào 2026: 7 dòng, có cột Khoảng trống đúng nhãn — ĐẠT
- A-709 · Không ai bị sót: file chưa thuộc đợt nào vẫn hợp lệ — ĐẠT
- A-701, A-702, A-703 · Tên file đúng quy tắc rút gọn của mục 1.9(periodCode: "P1", expectedKey: "Đợt 3/2 năm 2026") — ĐẠT
- 8.2 · Mọi đợt của năm nay đã qua: file mang đợt đầu năm sau — ĐẠT
- A-703 · Tên file chưa thuộc đợt nào là ChuaThuocDot_<năm>.xlsx — ĐẠT
- A-704, A-706, A-708, A-710 · Đợt 7/11 · 2026: tiêu đề, cột và 6 dòng dữ liệu — ĐẠT
- A-709 · Danh sách rỗng vẫn trả file hợp lệ chỉ có phần tiêu đề — ĐẠT
- A-701, A-702, A-703 · Tên file đúng quy tắc rút gọn của mục 1.9(periodCode: "P2", expectedKey: "Đợt 19/5 năm 2026") — ĐẠT
- A-707 · E01 thiếu Ngày sinh và Giới tính: hai ô rỗng, không phải dấu gạch ngang — ĐẠT
- 8.2 · Xuất Dashboard lấy đúng đợt sắp tới theo QT8 — ĐẠT
- 8.1, 8.3 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Exports/Unassigned", year: 2201) — ĐẠT
- 8.1, 8.3 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Exports/Unassigned", year: 0) — ĐẠT
- 8.1, 8.3 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Exports/Eligibility", year: 2201) — ĐẠT
- 8.1 → 8.3 · Thiếu token trả 401(path: "/api/Exports/Dashboard") — ĐẠT
- A-701, A-702, A-703 · Tên file đúng quy tắc rút gọn của mục 1.9(periodCode: "P3", expectedKey: "Đợt 2/9 năm 2026") — ĐẠT
- 8.1, 8.3 · Bỏ trống year thì lấy năm hiện tại của máy chủ — ĐẠT
- 8.1 · Id đợt lạ trả Mes.AwardPeriod.NotFound — ĐẠT
- 8.1, 8.3 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year(path: "/api/Exports/Eligibility", year: 1899) — ĐẠT
- 8.1 → 8.3 · Thiếu token trả 401(path: "/api/Exports/Unassigned") — ĐẠT
- A-701, A-702, A-703 · Tên file đúng quy tắc rút gọn của mục 1.9(periodCode: "P4", expectedKey: "Đợt 7/11 năm 2026") — ĐẠT
- 8.2 · Chưa cài đợt nào trả Mes.Dashboard.NotFound.UpcomingPeriod — ĐẠT
- A-705 · Tên đơn vị trống: dòng 1 là tên đợt, không có dòng trắng thừa — ĐẠT
- 8.1 → 8.3 · Thiếu token trả 401(path: "/api/Exports/Eligibility") — ĐẠT

### ImportEndpointTests

- 4.3 · Lỗi cấp file cũng chặn ở bước nạp(fileName: "loi-khong-phai-xlsx.xlsx", expectedProperty: "Extension") — ĐẠT
- 4.3 · Nạp file toàn lỗi: không thêm ai, không ném lỗi — ĐẠT
- 4.2 · Xem trước không ghi gì vào cơ sở dữ liệu — ĐẠT
- 4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa(fileName: "loi-rong.xlsx", expectedProperty: "Empty") — ĐẠT
- 4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa(fileName: "loi-sai-cot.xlsx", expectedProperty: "Columns") — ĐẠT
- 4.3 · Nạp loi-4-dong.xlsx: thêm 6 người, bỏ qua 4 dòng lỗi, không kiểm tra trùng — ĐẠT
- 4.2 · loi-4-dong.xlsx: 10 dòng, 6 hợp lệ, 4 lỗi ở dòng 8, 9, 10, 11 — ĐẠT
- 4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa(fileName: "loi-chi-co-tieu-de.xlsx", expectedProperty: "NoDataRows") — ĐẠT
- 4.2 · T69 · File mang tiêu đề cũ của cột thứ 4 bị chặn như mọi file sai cột — ĐẠT
- 4.1 · File mẫu hệ thống sinh khớp fixture mau-dang-vien.xlsx của QC — ĐẠT
- 4.2 · loi-moi-loai-mot-dong.xlsx: 8 dòng lỗi, mỗi loại một dòng, dòng nhiều lỗi trả đủ lý do — ĐẠT
- 4.2 · Bộ lõi 32 dòng hợp lệ, đọc được cả ngày dạng chuỗi lẫn ngày kiểu ngày(fileName: "core-hop-le.xlsx") — ĐẠT
- 4.1 · File mẫu trả file nhị phân đúng tên, 4 cột đúng thứ tự, 2 dòng ví dụ dd/MM/yyyy — ĐẠT
- 4.3 · Lỗi cấp file cũng chặn ở bước nạp(fileName: "loi-sai-cot.xlsx", expectedProperty: "Columns") — ĐẠT
- 4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa(fileName: "loi-khong-phai-xlsx.xlsx", expectedProperty: "Extension") — ĐẠT
- 4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa(fileName: "loi-dinh-dang-csv.csv", expectedProperty: "Extension") — ĐẠT
- 4.2 · bien-chuan-hoa.xlsx: cắt khoảng trắng (OQ-4), giới tính hoa thường (OQ-5), ngày một chữ số (OQ-6) — ĐẠT
- 4.2 · File quá 10 MB bị chặn trước khi mở, trả Mes.Import.Invalid.FileSize — ĐẠT
- Ba endpoint đều yêu cầu token — ĐẠT
- 4.2 · Bộ lõi 32 dòng hợp lệ, đọc được cả ngày dạng chuỗi lẫn ngày kiểu ngày(fileName: "core-hop-le-ngay-kieu-date.xlsx") — ĐẠT

### ImportTransactionTests

- 4.3 · Cùng đường đi đó nhưng không hỏng: bulk-1200.xlsx thêm đủ 1200 người — ĐẠT
- 4.3 · Hỏng đúng lúc chốt giao dịch: 32 dòng của core-hop-le.xlsx bị thu hồi hết — ĐẠT
- 4.3 · Hỏng ở lô thứ hai của bulk-1200.xlsx: 200 dòng đầu đã ghi vẫn bị thu hồi hết — ĐẠT

### PartyMemberEndpointTests

- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400(fullName: null, dateOfBirth: null, gender: null, officialAdmissionDate: "1996-10-15", messagesType: "Required", property: "FullName") — ĐẠT
- T51 · Sắp xếp theo Mốc kế tiếp; nhóm không còn mốc đứng cuối khi tăng dần — ĐẠT
- T51 · Mốc không hợp lệ trả 400 kèm khóa Invalid.NextMilestone(value: "$eq:") — ĐẠT
- 3.1 · Phân trang: mặc định 20 dòng, chọn được số dòng và số trang — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400(fullName: "Nguyễn Văn An", dateOfBirth: null, gender: "Khac", officialAdmissionDate: "1996-10-15", messagesType: "Invalid", property: "Gender") — ĐẠT
- A-001 · Mọi endpoint đảng viên không kèm token trả 401(method: "POST", path: "/api/PartyMembers/DeleteMany") — ĐẠT
- T51 · Lọc theo Mốc kế tiếp bám dãy mốc trong Cài đặt — ĐẠT
- A-001 · Mọi endpoint đảng viên không kèm token trả 401(method: "POST", path: "/api/PartyMembers") — ĐẠT
- 3.3 · Đúng ngày hôm nay là ngày vào Đảng (dự bị) hợp lệ — ĐẠT
- 3.3 · QT9 · Thêm hai người trùng hệt nhau vẫn thành hai bản ghi — ĐẠT
- T51 · Sắp xếp theo Tuổi đảng quy về Ngày vào Đảng (dự bị) theo chiều ngược lại — ĐẠT
- T51 · Mốc không hợp lệ trả 400 kèm khóa Invalid.NextMilestone(value: "$eq:33") — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400(fullName: "Nguyễn Văn An", dateOfBirth: "1996-10-16", gender: null, officialAdmissionDate: "1996-10-15", messagesType: "Invalid", property: "DateOfBirth") — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400(fullName: "Nguyễn Văn An", dateOfBirth: null, gender: null, officialAdmissionDate: null, messagesType: "Required", property: "OfficialAdmissionDate") — ĐẠT
- 3.6 · Xóa mảng chỉ có id lạ trả danh sách rỗng chứ không phải lỗi — ĐẠT
- 3.1 · Lọc giới tính Nam / Nữ; bỏ tham số thì lấy tất cả(filter: "&filter.Gender=$eq:Male", expectedCount: 1, expectedName: "Cao Văn Phúc") — ĐẠT
- T51 · Mốc không hợp lệ trả 400 kèm khóa Invalid.NextMilestone(value: "40") — ĐẠT
- 3.6 · Xóa nhiều với danh sách rỗng trả 400 — ĐẠT
- 3.1 · pageSize hoặc current không dương được kẹp về mặc định, vẫn trả 200(query: "?current=0") — ĐẠT
- A-001 · Mọi endpoint đảng viên không kèm token trả 401(method: "GET", path: "/api/PartyMembers") — ĐẠT
- T34 · DELETE /api/PartyMembers/{id} đã bị gỡ, không ai dựng lại được — ĐẠT
- 3.1 · Lọc giới tính Nam / Nữ; bỏ tham số thì lấy tất cả(filter: "&filter.Gender=$eq:Female", expectedCount: 1, expectedName: "Bùi Thị Lan") — ĐẠT
- T51 · Mốc không hợp lệ trả 400 kèm khóa Invalid.NextMilestone(value: "$gt:30") — ĐẠT
- 3.1 · Sắp xếp theo cột; cột lạ bị bỏ qua và quay về mặc định — ĐẠT
- 3.4 · Sửa id không tồn tại trả 400 Mes.PartyMember.NotFound — ĐẠT
- T51 · Mốc không hợp lệ trả 400 kèm khóa Invalid.NextMilestone(value: "$eq:abc") — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400(fullName: "Nguyễn Văn An", dateOfBirth: null, gender: null, officialAdmissionDate: "2026-09-20", messagesType: "Invalid", property: "OfficialAdmissionDate") — ĐẠT
- 3.1 · Tìm theo họ tên chứa chuỗi, không phân biệt hoa thường — ĐẠT
- 3.1 · Lọc giới tính Nam / Nữ; bỏ tham số thì lấy tất cả(filter: "", expectedCount: 3, expectedName: null) — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400(fullName: "   ", dateOfBirth: null, gender: null, officialAdmissionDate: "1996-10-15", messagesType: "Required", property: "FullName") — ĐẠT
- 3.6 · Xóa một người qua mảng một phần tử trả đúng một id, bản ghi biến mất hẳn — ĐẠT
- 3.6 · Xóa nhiều trả đúng id đã xóa, id lạ bị bỏ qua lặng lẽ — ĐẠT
- 3.4 · Sửa ghi đè cả bốn trường, kể cả xóa bằng null — ĐẠT
- 3.1 · Danh sách trả gender chuỗi và ba giá trị tuổi đảng tính theo hôm nay — ĐẠT
- T51 · Lọc theo Mốc kế tiếp chạy trong SQL nên tổng số dòng đúng — ĐẠT
- 3.1 · pageSize hoặc current không dương được kẹp về mặc định, vẫn trả 200(query: "?pageSize=0") — ĐẠT
- 3.3 · Thêm mới trả 200 kèm bản ghi vừa tạo và khóa Create.Successfully — ĐẠT
- 3.2 · Lấy một trả đúng bản ghi; id lạ trả 400 Mes.PartyMember.NotFound — ĐẠT

### PartyMemberFilterValueTests

- QC-T27-05 · Giá trị lọc lạ trả 400 với khóa Mes.Common.Invalid.Parameter(filter: "filter.Gender=Male") — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trả 400 với khóa Mes.Common.Invalid.Parameter(filter: "filter.Gender=$eq:") — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trả 400 với khóa Mes.Common.Invalid.Parameter(filter: "filter.Gender=$eq:1") — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trả 400 với khóa Mes.Common.Invalid.Parameter(filter: "filter.OfficialAdmissionDate=$gte:hom-qua") — ĐẠT
- QC-T27-05 · Giá trị lọc hợp lệ vẫn lọc đúng như trước — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trả 400 với khóa Mes.Common.Invalid.Parameter(filter: "filter.Gender=$eq:Khac") — ĐẠT
- QC-T27-05 · Giá trị lọc lạ trả 400 với khóa Mes.Common.Invalid.Parameter(filter: "filter.Gender=$in:Male,Khac") — ĐẠT

### PartyMemberSqlTranslationTests

- T51 · Lọc Mốc kế tiếp sinh ra hai bất đẳng thức trong WHERE — ĐẠT
- T51 · Sắp xếp theo cột tính ra sinh ra ORDER BY trên cột ngày(sortQuery: "PartyAge desc", isDescending: False) — ĐẠT
- T51 · Sắp xếp theo cột tính ra sinh ra ORDER BY trên cột ngày(sortQuery: "NextMilestone desc", isDescending: False) — ĐẠT
- T51 · Sắp xếp theo cột tính ra sinh ra ORDER BY trên cột ngày(sortQuery: "NextMilestone asc", isDescending: True) — ĐẠT
- T51 · Sắp xếp theo cột tính ra sinh ra ORDER BY trên cột ngày(sortQuery: "PartyAge asc", isDescending: True) — ĐẠT
- T51 · Sắp xếp theo cột tính ra sinh ra ORDER BY trên cột ngày(sortQuery: "partyAgeYears asc", isDescending: True) — ĐẠT
- T51 · Lọc None chỉ chặn cận trên — ĐẠT

### QueryParameterGuardTests

- QC-T27-04 · Lỗi ép kiểu tham số trả khóa chung, không trả câu tiếng Anh(url: "/api/Eligibility?awardPeriodId=khong-phai-guid") — ĐẠT
- QC-T27-04 · Lỗi ép kiểu tham số trả khóa chung, không trả câu tiếng Anh(url: "/api/PartyMembers?current=khong-phai-so") — ĐẠT
- QC-T27-04 · Lỗi ép kiểu tham số trả khóa chung, không trả câu tiếng Anh(url: "/api/AwardPeriods?year=2147483648") — ĐẠT
- QC-T27-03 · pageSize vượt trần bị kẹp về 200, không báo lỗi — ĐẠT
- QC-T27-03 · pageSize nhỏ hơn 1 lấy mặc định 20 — ĐẠT
- QC-T27-06 · Xem trước và lưu dùng đúng một bộ luật — ĐẠT
- QC-T27-04 · Lỗi ép kiểu tham số trả khóa chung, không trả câu tiếng Anh(url: "/api/AwardPeriods?year=khong-phai-so") — ĐẠT
- QC-T27-06 · Xem trước dãy mốc từ chối khoảng vượt trần, đúng khóa của PUT — ĐẠT
- QC-T27-04 · Khóa của FluentValidation vẫn đi thẳng ra ngoài — ĐẠT
- QC-T27-02 · Số trang nhỏ hơn 1 được coi như trang 1 — ĐẠT
- QC-T27-02 · Số trang rất lớn trả 200 với danh sách rỗng, không 500 — ĐẠT
- QC-T27-06 · Khoảng hợp lệ vẫn xem trước được bình thường — ĐẠT

### SettingsEndpointTests

- 7.2 · A-509 · Kho trống: lưu lần đầu tạo đúng một bản ghi, không nhân bản — ĐẠT
- 7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400(body: "{\"endYears\":90,\"stepYears\":5}") — ĐẠT
- 7.2 · Tên đơn vị quá 200 ký tự trả 400 Mes.AppSetting.OverLength.UnitName — ĐẠT
- 7.2 · A-504 · Mốc bắt đầu / kết thúc ≤ 0 trả đúng khóa lỗi của từng trường(startYears: 0, endYears: 90, property: "StartYears") — ĐẠT
- 7.2 · A-504 · Mốc bắt đầu / kết thúc ≤ 0 trả đúng khóa lỗi của từng trường(startYears: -1, endYears: 90, property: "StartYears") — ĐẠT
- 7.2 · A-508 · Tên đơn vị bỏ trống hoặc toàn khoảng trắng đều thành chưa đặt(unitName: "   ") — ĐẠT
- 7.4 · Xem trước dãy mốc không ghi gì xuống cơ sở dữ liệu — ĐẠT
- 7.2 · A-504 · Bước 0 hoặc âm trả 400 Mes.AppSetting.Invalid.StepYears(stepYears: 0) — ĐẠT
- 7.4 · Bỏ trống tham số nào thì lấy giá trị đang lưu của tham số đó — ĐẠT
- 7.2 · A-504 · Bước 0 hoặc âm trả 400 Mes.AppSetting.Invalid.StepYears(stepYears: -5) — ĐẠT
- 7.1 · A-501 · Kho chưa có bản ghi cài đặt nào thì đọc ra mặc định 30/90/5 — ĐẠT
- 7.1 → 7.4 · Chưa đăng nhập thì cả bốn endpoint đều trả 401 — ĐẠT
- 7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400(body: "{\"startYears\":30.5,\"endYears\":90,\"stepYears\""···) — ĐẠT
- 7.3 · A-506 · Khôi phục mặc định đưa về 30/90/5 và giữ nguyên Tên đơn vị — ĐẠT
- 7.1 · Kho đã seed: đọc ra 30/90/5, 13 mốc và tên đơn vị đang lưu — ĐẠT
- 7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400(body: "{\"startYears\":\"ba mươi\",\"endYears\":90,\"step"···) — ĐẠT
- 7.2 · A-504 · Mốc bắt đầu / kết thúc ≤ 0 trả đúng khóa lỗi của từng trường(startYears: 30, endYears: 0, property: "EndYears") — ĐẠT
- 7.2 · A-508 · Tên đơn vị bỏ trống hoặc toàn khoảng trắng đều thành chưa đặt(unitName: "") — ĐẠT
- 7.2 · A-509 · Lưu hai lần liên tiếp vẫn chỉ có đúng một bản ghi cài đặt — ĐẠT
- 7.2 · A-503 · Bắt đầu > Kết thúc trả 400 Mes.AppSetting.Invalid.Range — ĐẠT
- 7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400(body: "{\"startYears\":\"\",\"endYears\":90,\"stepYears\""···) — ĐẠT
- 7.2 · A-508 · Tên đơn vị bỏ trống hoặc toàn khoảng trắng đều thành chưa đặt(unitName: null) — ĐẠT
- 7.4 · Tham số xem trước sai trả cùng bộ khóa lỗi với 7.2 — ĐẠT
- 7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400(body: "{\"startYears\":null,\"endYears\":90,\"stepYears\""···) — ĐẠT
- 7.2 · A-507 · Lưu Tên đơn vị: đọc lại thấy ngay, Dashboard cũng thấy — ĐẠT
- 7.2 · A-502 · Đổi Bước 5 → 10: danh sách đủ điều kiện và badge đổi ngay (QT5) — ĐẠT
- 7.2 · A-504 · Mốc bắt đầu / kết thúc ≤ 0 trả đúng khóa lỗi của từng trường(startYears: 30, endYears: -30, property: "EndYears") — ĐẠT

### UpdatedAtStampTests

- Thêm mới đảng viên được đóng dấu dù không ai gán UpdatedAt — ĐẠT
- Cài đặt seed sẵn mang dấu thời gian của IDateTimeProvider, không phải giờ máy — ĐẠT
- Sửa đảng viên ghi đè dấu thời gian cũ — ĐẠT

## HuyHieuDang.Web.QcIntegrationTests

### A0AuthorizationTests

- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "DELETE", url: "/api/AwardPeriods/00000000-0000-0000-0000-00000000"···) — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "POST", url: "/api/Auth/Logout") — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "GET", url: "/api/Exports/Unassigned") — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "POST", url: "/api/PartyMembers") — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "PUT", url: "/api/AwardPeriods/00000000-0000-0000-0000-00000000"···) — ĐẠT
- A002_Jwt_sai_chu_ky_tra_401 — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "GET", url: "/api/Exports/Dashboard") — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "GET", url: "/api/AwardPeriods/00000000-0000-0000-0000-00000000"···) — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "GET", url: "/api/Dashboard") — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "POST", url: "/api/PartyMembers/DeleteMany") — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "POST", url: "/api/PartyMembers/Import/Commit") — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "POST", url: "/api/AwardPeriods") — ĐẠT
- A007_Chi_Login_duoc_phep_AllowAnonymous — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "GET", url: "/api/AwardPeriods") — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "GET", url: "/api/Exports/Eligibility?awardPeriodId=00000000-00"···) — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "GET", url: "/api/PartyMembers") — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "POST", url: "/api/Settings/RestoreDefaults") — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "GET", url: "/api/Eligibility?awardPeriodId=00000000-0000-0000-"···) — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "GET", url: "/api/Eligibility/UnassignedCount") — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "POST", url: "/api/PartyMembers/Import/Preview") — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "GET", url: "/api/Auth/Me") — ĐẠT
- A004_Thieu_tien_to_Bearer_tra_401 — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "GET", url: "/api/Settings") — ĐẠT
- A003_Jwt_het_han_tra_401 — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "GET", url: "/api/PartyMembers/Import/Template") — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "GET", url: "/api/PartyMembers/00000000-0000-0000-0000-00000000"···) — ĐẠT
- A005_Dang_nhap_sai_tra_401_va_thong_diep_chung — ĐẠT
- A006_Dang_nhap_dung_tra_token_dung_duoc_ngay — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "PUT", url: "/api/PartyMembers/00000000-0000-0000-0000-00000000"···) — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "PUT", url: "/api/Settings") — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "GET", url: "/api/Settings/Milestones") — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A0AuthorizationTests.A001_Goi_khi_chua_dang_nhap_tra_401_va_khong_lo_du_lieu(method: "GET", url: "/api/Eligibility/Unassigned") — ĐẠT

### A10ContractKeyTests

- A-906 · Bảng khóa của QC trùng khít mục 1.5 hợp đồng API — ĐẠT

### A1PartyMemberTests

- A105_Co_trang_50_va_100_dung_so_trang — ĐẠT
- A126_Lay_mot_dang_vien_theo_id — ĐẠT
- A103_Trang_cuoi_con_12_dong — ĐẠT
- A108_A109_Tim_kiem_khong_phan_biet_hoa_thuong — ĐẠT
- A121_Xoa_mot_nguoi_giam_dung_mot — ĐẠT
- A123_Xoa_id_khong_ton_tai_va_duong_xoa_mot_da_bi_go — ĐẠT
- A101_Tong_so_dang_vien_dung_1232 — ĐẠT
- A122_Xoa_nhieu_nguoi_giam_dung_so_luong — ĐẠT
- A116_Them_tay_hop_le_tang_dung_mot_nguoi — ĐẠT
- A104_Trang_vuot_qua_trang_cuoi_tra_rong_khong_loi — ĐẠT
- A114_Sap_xep_hai_chieu_dung_tren_moi_cot — ĐẠT
- A124_Xoa_danh_sach_co_id_trung_khong_dem_trung — ĐẠT
- A112_Ky_tu_dac_biet_khong_gay_loi_va_khong_bi_hieu_la_dai_dien — ĐẠT
- A115_Sap_theo_Ho_ten_dung_bang_chu_cai_tieng_Viet — ĐẠT
- A102_Trang_dau_20_dong_va_62_trang — ĐẠT
- A117_Them_tay_thieu_ho_ten_bi_tu_choi — ĐẠT
- A106_Co_trang_bat_thuong_khong_lam_treo_may_chu — ĐẠT
- A118_A119_Bien_ngay_chinh_thuc_bang_hom_nay — ĐẠT
- A107_Ghep_moi_trang_khong_trung_khong_thieu — ĐẠT
- A120_Sua_ngay_chinh_thuc_lam_moi_thu_doi_theo_ngay — ĐẠT
- A110_A111_Tim_dung_mot_nguoi_va_tim_khong_thay — ĐẠT
- A125_Tong_so_va_tuoi_dang_tinh_den_hom_nay — ĐẠT
- A113_Loc_gioi_tinh_dung_so_luong — ĐẠT

### A2AwardPeriodTests

- A211_Bo_dot_phu_kin_khong_con_canh_bao — ĐẠT
- A205_A206_A207_Ba_rang_buoc_chan_luu — ĐẠT
- A204_So_nguoi_du_dieu_kien_nam_nay_tren_tung_dong — ĐẠT
- A219_Dot_khong_ton_tai_tra_loi_nghiep_vu — ĐẠT
- A212_Noi_den_ngay_lam_danh_sach_doi_ngay — ĐẠT
- A213_Xoa_dot_khong_lam_mat_dang_vien — ĐẠT
- A202_A203_Trang_thai_dot_tai_T0_va_T1 — ĐẠT
- A214_Du_dieu_kien_Dot_7_11_nam_2026 — ĐẠT
- A201_Bon_dot_chinh_sap_theo_tu_ngay — ĐẠT
- A210_Bon_dot_chinh_canh_bao_dung_nam_khoang_trong — ĐẠT
- A208_Dot_bat_dau_29_02_hop_le_va_thu_ve_28_02_o_nam_khong_nhuan — ĐẠT
- A220_Bo_lon_khong_gay_nhieu_cho_bo_loi — ĐẠT
- A218_Nam_bat_thuong_duoc_xu_ly_tat_dinh — ĐẠT
- A217_Du_dieu_kien_Dot_3_2_nam_2026 — ĐẠT
- A215_A216_Du_dieu_kien_o_nam_khac — ĐẠT
- A209_Hai_dot_chong_lan_van_luu_duoc_va_co_canh_bao_dung_cap — ĐẠT

### A2bSpanningYearTests

- A224_Xuat_excel_dot_vat_nam_dung_ten_file_va_dong_tieu_de — ĐẠT
- A222_Nguoi_tron_moc_20_01_khong_bi_xep_vao_chua_thuoc_dot_nao — ĐẠT
- A223_Dashboard_doc_dung_lan_dien_ra_neo_o_nam_truoc — ĐẠT
- A221_Danh_sach_dot_vat_nam_gan_dung_nam_va_cho_hai_doan_do_phu — ĐẠT

### A3DashboardTests

- A304_Khong_co_dot_nao_bao_chua_cai_dot — ĐẠT
- A306_Kho_trong_hoan_toan — ĐẠT
- A307_Badge_chua_thuoc_dot_nao_bang_7 — ĐẠT
- A303_Dashboard_tai_T2_nhay_sang_nam_sau — ĐẠT
- A305_Khong_co_dang_vien_nao — ĐẠT
- A301_Dashboard_tai_T0 — ĐẠT
- A302_Dashboard_tai_T1_dang_dien_ra — ĐẠT
- A308_Canh_bao_tren_Dashboard_khop_voi_man_Dot — ĐẠT

### A4UnassignedTests

- A403_Nam_2025_va_2027_khop_ket_qua_mong_doi — ĐẠT
- A402_Doi_buoc_sang_10_con_sau_nguoi — ĐẠT
- A406_Badge_luon_khop_so_dong_cua_man_hinh — ĐẠT
- A404_Bo_dot_phu_kin_thi_khong_ai_bi_sot — ĐẠT
- A405_Sap_theo_Moc_roi_Ho_ten — ĐẠT
- A401_Nam_2026_dung_bay_nguoi_va_dung_nhan_khoang_trong — ĐẠT
- A401b_Chua_cai_dot_nao_thi_nhan_la_Truoc_dot_dau_tien — ĐẠT

### A5SettingsTests

- A510_Xem_truoc_day_moc_khong_ghi_gi_vao_co_so_du_lieu — ĐẠT
- A502_Doi_buoc_lam_moi_danh_sach_doi_theo_ngay — ĐẠT
- A509_Luu_hai_lan_van_chi_mot_ban_ghi_cai_dat — ĐẠT
- A506_Khoi_phuc_mac_dinh_khong_dung_ten_don_vi — ĐẠT
- A507_A508_Ten_don_vi_luu_duoc_va_de_trong_duoc — ĐẠT
- A501_Kho_trong_van_tra_cai_dat_mac_dinh — ĐẠT
- A503_A504_A505_Cai_dat_sai_bi_tu_choi — ĐẠT
- A511_Dang_xuat_tra_200_va_khong_huy_token_phia_may_chu — ĐẠT

### A6ImportTests

- A601_Xem_truoc_file_loi_32_dong_hop_le_va_chua_ghi_gi — ĐẠT
- A605_Chi_xem_truoc_roi_bo_thi_tong_khong_doi — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A6ImportTests.A607_A611_Loi_cap_file_bi_chan_ngay(fileName: "loi-dinh-dang-csv.csv", expectedKey: "Mes.Import.Invalid.Extension") — ĐẠT
- A621_Mot_dong_nhieu_loi_tra_du_moi_ly_do — ĐẠT
- A617_Loi_ky_thuat_giua_chung_cuon_lai_toan_bo — ĐẠT
- A613_File_hop_le_9_9MB_van_duoc_chap_nhan — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A6ImportTests.A607_A611_Loi_cap_file_bi_chan_ngay(fileName: "loi-rong.xlsx", expectedKey: "Mes.Import.Invalid.Empty") — ĐẠT
- A620_O_ngay_kieu_ngay_cua_Excel_van_doc_duoc — ĐẠT
- A619_File_mau_dung_cot_va_nap_lai_duoc — ĐẠT
- A603_Xem_truoc_file_bon_dong_loi_dung_so_dong_va_dung_ly_do — ĐẠT
- A614_Khong_gui_file_tra_400 — ĐẠT
- A609_File_chi_co_tieu_de_bao_khac_file_rong — ĐẠT
- A606_Nap_hai_lan_cung_mot_file_tang_gap_doi — ĐẠT
- A604_Nap_file_bon_dong_loi_chi_them_sau_nguoi — ĐẠT
- A615_Gui_hai_file_duoc_xu_ly_tat_dinh — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A6ImportTests.A607_A611_Loi_cap_file_bi_chan_ngay(fileName: "loi-sai-cot.xlsx", expectedKey: "Mes.Import.Invalid.Columns") — ĐẠT
- A612_File_qua_10MB_bi_chan_truoc_khi_doc_noi_dung — ĐẠT
- A618_Nap_file_1200_dong — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A6ImportTests.A607_A611_Loi_cap_file_bi_chan_ngay(fileName: "loi-khong-phai-xlsx.xlsx", expectedKey: "Mes.Import.Invalid.Extension") — ĐẠT
- A616_Cat_ket_noi_giua_luc_nap_khong_de_lai_du_lieu_nua_voi — ĐẠT
- HuyHieuDang.Web.QcIntegrationTests.A6ImportTests.A607_A611_Loi_cap_file_bi_chan_ngay(fileName: "loi-chi-co-tieu-de.xlsx", expectedKey: "Mes.Import.Invalid.NoDataRows") — ĐẠT
- A602_Nap_sau_xem_truoc_tang_dung_32 — ĐẠT

### A7ExportTests

- A710_Ba_endpoint_xuat_deu_mo_duoc_bang_thu_vien_doc_Excel — ĐẠT
- A705_Ten_don_vi_de_trong_thi_bo_dong_do — ĐẠT
- A704_Dong_tieu_de_du_ba_phan — ĐẠT
- A701_A702_Ten_file_dung_quy_uoc — ĐẠT
- A709_Xuat_danh_sach_rong_van_ra_file_hop_le — ĐẠT
- A711_Xuat_tu_Dashboard_theo_dot_sap_toi — ĐẠT
- A706_A707_A708_Noi_dung_file_khop_bang_dang_xem — ĐẠT
- A712_File_chua_thuoc_dot_nao_co_cot_khoang_trong — ĐẠT
- A703_Ten_file_chua_thuoc_dot_nao — ĐẠT

### A8DefectTests

- QcT2704_Loi_ep_kieu_tham_so_phai_tra_khoa_thong_diep — ĐẠT
- QcT2705_Gia_tri_loc_la_khong_duoc_bo_qua_lang_le — ĐẠT
- QcT2702_So_trang_lon_khong_duoc_lam_may_chu_loi_500 — ĐẠT
- QcT2707_Moi_khoa_Backend_tra_ra_deu_phai_co_trong_hop_dong — ĐẠT
- QcT2703_Co_trang_phai_bi_chan_hoac_kep_ve_muc_tran — ĐẠT
- QcT2706_Xem_truoc_day_moc_phai_co_tran — ĐẠT

### A9TechnicalTests

- A903_Bien_ep_ngay_khong_co_hieu_luc_o_Production — ĐẠT
- A906_Moi_truong_ngay_nghiep_vu_la_ngay_thuan — ĐẠT
- A903b_Ngoai_Production_bien_ep_ngay_phai_co_hieu_luc — ĐẠT
- A902_Ket_qua_khong_phu_thuoc_mui_gio_cua_tien_trinh — ĐẠT
- A904_Mot_van_dang_vien_van_tinh_duoi_mot_giay — ĐẠT
- A901_Khong_noi_nao_trong_src_doc_dong_ho_may — ĐẠT
- A905_Moi_thong_bao_loi_deu_tra_khoa_dich_duoc_va_khong_lo_noi_bo — ĐẠT

# Giao diện — bộ kiểm thử T50

## t50-bon-bang.spec.ts

- phân trang, tìm, lọc mốc, sắp xếp — ĐẠT
- 100 dòng mỗi trang vẫn cuộn tới được dòng cuối — ĐẠT
- rỗng do lọc nói đúng lý do và cho xóa bộ lọc — ĐẠT
- phân trang, tìm, lọc mốc, sắp xếp — ĐẠT
- 100 dòng mỗi trang vẫn cuộn tới được dòng cuối — ĐẠT
- phân trang, tìm, lọc mốc, sắp xếp — ĐẠT
- 100 dòng mỗi trang vẫn cuộn tới được dòng cuối — ĐẠT
- phân trang, tìm theo tên đợt, sắp xếp theo Từ ngày — ĐẠT
- 100 dòng mỗi trang vẫn cuộn tới được dòng cuối — ĐẠT

## t50-khung-man-hinh.spec.ts

- màn có bảng vừa khít màn hình, không sinh thanh cuộn cửa sổ — ĐẠT
- màn Cài đặt dài hơn màn hình vẫn cuộn tới cuối được — ĐẠT
- màn Đảng viên không bị khung mới làm cắt mất thanh phân trang — ĐẠT

## t58-quyet-dinh-giao-dien.spec.ts

- Dashboard — đủ điều kiện: chân bảng không còn "1–20 / 1.342" — ĐẠT
- Đợt trao huy hiệu: chân bảng không còn "1–20 / 1.342" — ĐẠT
- Chi tiết đợt — tab Đủ điều kiện: chân bảng không còn "1–20 / 1.342" — ĐẠT
- Chưa thuộc đợt nào: chân bảng không còn "1–20 / 1.342" — ĐẠT
- chọn 100 dòng mỗi trang cũng không làm cụm đếm hiện lại — ĐẠT
- mở ra đúng 10 dòng, ngắn hơn hẳn 20 dòng nên ít phải cuộn — ĐẠT
- vẫn đổi được sang 20 dòng và nhớ lựa chọn khi đổi trang — ĐẠT
- ba bảng chiếm trọn màn vẫn giữ 20 dòng mặc định — ĐẠT

## t59-logo.spec.ts

- thanh đầu trang hiện ảnh lá cờ mới — ĐẠT
- không còn thẻ ảnh nào trỏ tới logo cũ — ĐẠT
- biểu tượng tab trỏ tới tệp ảnh mới và tải được — ĐẠT
- thanh đầu trang không tràn ở 1280x600 — ĐẠT
- thanh đầu trang không tràn ở 1366x650 — ĐẠT
- hàng thương hiệu và hình mờ đều dùng ảnh lá cờ mới — ĐẠT
- không tràn và không đè chữ ở 1280x600 — ĐẠT
- không tràn và không đè chữ ở 1366x650 — ĐẠT
- tiêu đề ở 1440x900 ngắt đúng chỗ — mốc để so — ĐẠT

## t60-responsive.spec.ts

- Dashboard — không mất nội dung, không cuộn ngang — ĐẠT
- Đảng viên — không mất nội dung, không cuộn ngang — ĐẠT
- Import bước 1 — chọn file — không mất nội dung, không cuộn ngang — ĐẠT
- Import bước 2 — xem trước — không mất nội dung, không cuộn ngang — ĐẠT
- Import bước 3 — kết quả — không mất nội dung, không cuộn ngang — ĐẠT
- Đợt trao huy hiệu — không mất nội dung, không cuộn ngang — ĐẠT
- Chi tiết đợt — tab Thông tin — không mất nội dung, không cuộn ngang — ĐẠT
- Chi tiết đợt — tab Danh sách đủ điều kiện — không mất nội dung, không cuộn ngang — ĐẠT
- Chưa thuộc đợt nào — không mất nội dung, không cuộn ngang — ĐẠT
- Cài đặt — không mất nội dung, không cuộn ngang — ĐẠT
- Trang 404 — không mất nội dung, không cuộn ngang — ĐẠT
- thang gọn giữ đúng sàn cỡ chữ và chiều cao nút — ĐẠT
- menu trái tự thu gọn khi cửa sổ hẹp, vẫn mở lại được — ĐẠT
- modal Thêm đảng viên vừa trong khung nhìn, nút Lưu tới được — ĐẠT
- modal Thêm đợt trao huy hiệu vừa trong khung nhìn, nút Lưu tới được — ĐẠT
- Dashboard — không mất nội dung, không cuộn ngang — ĐẠT
- Đảng viên — không mất nội dung, không cuộn ngang — ĐẠT
- Import bước 1 — chọn file — không mất nội dung, không cuộn ngang — ĐẠT
- Import bước 2 — xem trước — không mất nội dung, không cuộn ngang — ĐẠT
- Import bước 3 — kết quả — không mất nội dung, không cuộn ngang — ĐẠT
- Đợt trao huy hiệu — không mất nội dung, không cuộn ngang — ĐẠT
- Chi tiết đợt — tab Thông tin — không mất nội dung, không cuộn ngang — ĐẠT
- Chi tiết đợt — tab Danh sách đủ điều kiện — không mất nội dung, không cuộn ngang — ĐẠT
- Chưa thuộc đợt nào — không mất nội dung, không cuộn ngang — ĐẠT
- Cài đặt — không mất nội dung, không cuộn ngang — ĐẠT
- Trang 404 — không mất nội dung, không cuộn ngang — ĐẠT
- thang gọn giữ đúng sàn cỡ chữ và chiều cao nút — ĐẠT
- menu trái tự thu gọn khi cửa sổ hẹp, vẫn mở lại được — ĐẠT
- modal Thêm đảng viên vừa trong khung nhìn, nút Lưu tới được — ĐẠT
- modal Thêm đợt trao huy hiệu vừa trong khung nhìn, nút Lưu tới được — ĐẠT
- Dashboard — không mất nội dung, không cuộn ngang — ĐẠT
- Đảng viên — không mất nội dung, không cuộn ngang — ĐẠT
- Import bước 1 — chọn file — không mất nội dung, không cuộn ngang — ĐẠT
- Import bước 2 — xem trước — không mất nội dung, không cuộn ngang — ĐẠT
- Import bước 3 — kết quả — không mất nội dung, không cuộn ngang — ĐẠT
- Đợt trao huy hiệu — không mất nội dung, không cuộn ngang — ĐẠT
- Chi tiết đợt — tab Thông tin — không mất nội dung, không cuộn ngang — ĐẠT
- Chi tiết đợt — tab Danh sách đủ điều kiện — không mất nội dung, không cuộn ngang — ĐẠT
- Chưa thuộc đợt nào — không mất nội dung, không cuộn ngang — ĐẠT
- Cài đặt — không mất nội dung, không cuộn ngang — ĐẠT
- Trang 404 — không mất nội dung, không cuộn ngang — ĐẠT
- thang gọn giữ đúng sàn cỡ chữ và chiều cao nút — ĐẠT
- menu trái tự thu gọn khi cửa sổ hẹp, vẫn mở lại được — ĐẠT
- modal Thêm đảng viên vừa trong khung nhìn, nút Lưu tới được — ĐẠT
- modal Thêm đợt trao huy hiệu vừa trong khung nhìn, nút Lưu tới được — ĐẠT
- Dashboard — không mất nội dung, không cuộn ngang — ĐẠT
- Đảng viên — không mất nội dung, không cuộn ngang — ĐẠT
- Import bước 1 — chọn file — không mất nội dung, không cuộn ngang — ĐẠT
- Import bước 2 — xem trước — không mất nội dung, không cuộn ngang — ĐẠT
- Import bước 3 — kết quả — không mất nội dung, không cuộn ngang — ĐẠT
- Đợt trao huy hiệu — không mất nội dung, không cuộn ngang — ĐẠT
- Chi tiết đợt — tab Thông tin — không mất nội dung, không cuộn ngang — ĐẠT
- Chi tiết đợt — tab Danh sách đủ điều kiện — không mất nội dung, không cuộn ngang — ĐẠT
- Chưa thuộc đợt nào — không mất nội dung, không cuộn ngang — ĐẠT
- Cài đặt — không mất nội dung, không cuộn ngang — ĐẠT
- Trang 404 — không mất nội dung, không cuộn ngang — ĐẠT
- thang gọn giữ đúng sàn cỡ chữ và chiều cao nút — ĐẠT
- menu trái tự thu gọn khi cửa sổ hẹp, vẫn mở lại được — ĐẠT
- modal Thêm đảng viên vừa trong khung nhìn, nút Lưu tới được — ĐẠT
- modal Thêm đợt trao huy hiệu vừa trong khung nhìn, nút Lưu tới được — ĐẠT

## t60-stt-dot.spec.ts

- bảng Đợt đánh số thứ tự nối tiếp qua các trang — ĐẠT

## t61-cheo-gop-ba-pr.spec.ts

- T58 x T60 · bảng Dashboard giữ đúng 10 dòng và tới được hết ở 1280x600 — ĐẠT
- T59 x T60 · cờ vừa trong thanh đầu trang và không đè chữ ở 1280x600 — ĐẠT
- T58 x T60 · bảng Dashboard giữ đúng 10 dòng và tới được hết ở 1366x650 — ĐẠT
- T59 x T60 · cờ vừa trong thanh đầu trang và không đè chữ ở 1366x650 — ĐẠT
- T58 x T60 · bảng Dashboard giữ đúng 10 dòng và tới được hết ở 1440x900 — ĐẠT
- T59 x T60 · cờ vừa trong thanh đầu trang và không đè chữ ở 1440x900 — ĐẠT
- T58 x T60 · bảng Dashboard giữ đúng 10 dòng và tới được hết ở 3440x1440 — ĐẠT
- T59 x T60 · cờ vừa trong thanh đầu trang và không đè chữ ở 3440x1440 — ĐẠT

# Đầu-cuối trên stack docker

## e0-tien-de.spec.ts

- E0-01 · Backend đóng băng "hôm nay" theo HUYHIEUDANG_TEST_TODAY — ĐẠT
- E0-02 · Ngày máy chủ nằm trong khoảng an toàn của bộ dữ liệu biên — ĐẠT
- E0-03 · Đồng hồ trình duyệt đóng băng đúng mốc và đúng múi giờ — ĐẠT

## e1-lan-dung-dau-tien.spec.ts

- E2E-1 · Lần dùng đầu tiên: đăng nhập → cài đặt → tạo đợt → import → Dashboard — ĐẠT

## e2-import-co-loi.spec.ts

- E2E-2 · Import có lỗi: xem trước đúng, chỉ dòng hợp lệ được nạp — ĐẠT

## e5-xuat-excel.spec.ts

- E2E-5 · Xuất Excel từ Dashboard, chi tiết đợt và Chưa thuộc đợt nào — ĐẠT

## e6-vong-doi-dang-vien.spec.ts

- E2E-6 · Vòng đời đảng viên: thêm tay → tìm → sửa → xóa nhiều dòng — ĐẠT

## e7-dot-vat-qua-nam.spec.ts

- E2E-7 · Đợt trao huy hiệu vắt qua 31/12 chạy đúng trên mọi màn — ĐẠT
- QC-T54-01 · Tiêu đề trang chi tiết đợt phải nói rõ Đến ngày thuộc năm sau — ĐẠT
