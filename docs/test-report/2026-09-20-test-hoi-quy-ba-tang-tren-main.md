# Báo cáo kiểm thử — Hồi quy ba tầng trên `main` (T29)

| | |
|---|---|
| Task | T29 — Vòng sửa lỗi, kiểm thử hồi quy và báo cáo chất lượng cuối |
| Người chạy | QC |
| Ngày chạy | 20/09/2026 |
| Commit `main` đã chạy | `5784988` (gộp PR #42) |
| Nhánh chạy | `test/T29-hoi-quy` |

Ba lệnh đã chạy:

```bash
cd BE && dotnet test HuyHieuDang.sln                                   # tầng 1 + tầng 2
cd FE && npm run build                                                 # dựng gói giao diện
cd FE && npx playwright test -c e2e/playwright.e2e.config.ts           # tầng 3
```

Tổng ba tầng: **610 đạt · 0 hỏng · 11 bỏ qua** trên **621** ca.

| Tầng | Bộ kiểm thử | Đạt | Hỏng | Bỏ qua |
|---|---|---|---|---|
| 1 | HuyHieuDang.Core.UnitTests | 124 | 0 | 0 |
| 1 | HuyHieuDang.Core.QcTests | 118 | 0 | 1 |
| 1 | HuyHieuDang.Infrastructure.UnitTests | 44 | 0 | 0 |
| 2 | HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |
| 2 | HuyHieuDang.Web.IntegrationTests | 181 | 0 | 0 |
| 2 | HuyHieuDang.Web.QcIntegrationTests | 132 | 0 | 7 |
| 3 | Playwright end-to-end (`FE/e2e`) | 10 | 0 | 3 |
| **Tổng** | | **610** | **0** | **11** |

Mỗi dòng dưới đây là một ca kiểm thử; các biến thể `Theory` của cùng một ca gộp vào một dòng.

---

## Tầng 1 · HuyHieuDang.Core.UnitTests — logic thuần QT1–QT11 (Backend viết)

### Qt4EligibilityTests

- Số người đủ điều kiện của cả bộ lõi khớp kết quả mong đợi (6 biến thể) — ĐẠT
- Ngày tròn mốc trước Từ ngày một ngày thì không đủ điều kiện — ĐẠT
- Người đúng mốc lớn nhất vẫn đủ điều kiện với mốc 90 — ĐẠT
- Ngày tròn mốc sau Đến ngày một ngày thì không đủ điều kiện — ĐẠT
- Năm không có mốc nào rơi vào thì không ai đủ điều kiện — ĐẠT
- Đợt bắt đầu 29/02 ở năm không nhuận thì thu về 28/02 — ĐẠT
- Ngày tròn mốc đúng bằng Từ ngày của đợt thì vẫn đủ điều kiện — ĐẠT
- Đổi Bước sang 10 thì người có mốc biến mất rời danh sách — ĐẠT
- Người đã vượt mốc lớn nhất thì không còn đủ điều kiện — ĐẠT
- Ngày tròn mốc đúng bằng Đến ngày của đợt thì vẫn đủ điều kiện — ĐẠT
- Đổi Bước sang 10 thì người có mốc còn lại vẫn ở trong danh sách — ĐẠT
- Ngày tròn mốc 29/02 lùi về 28/02 vẫn rơi trong đợt — ĐẠT

### Qt8UpcomingPeriodTests

- Đợt sắp tới khớp kết quả mong đợi ở mọi kịch bản của bộ dữ liệu (8 biến thể) — ĐẠT
- Tại T0 đợt sắp tới là đợt kế tiếp trong năm, kèm số ngày còn lại — ĐẠT
- Chưa cài đợt nào thì không có đợt sắp tới — ĐẠT
- Một ngày sau khi đợt cuối đóng thì đợt sắp tới nhảy sang năm sau — ĐẠT
- Hai đợt cùng Từ ngày thì ưu tiên đợt đóng sớm hơn, rồi tới tên đợt — ĐẠT
- Hai đợt cùng Từ ngày cho kết quả tất định, không phụ thuộc thứ tự nạp — ĐẠT
- Sang năm sau vẫn phân định hai đợt trùng Từ ngày theo đúng quy tắc đó — ĐẠT
- Hai đợt chồng lấn thì chọn đợt có Từ ngày sớm hơn — ĐẠT
- Mọi đợt trong năm đã qua thì lấy đợt sớm nhất của năm sau — ĐẠT
- Đợt bắt đầu 29/02 sang năm không nhuận thì thu về 28/02 — ĐẠT
- Hôm nay nằm trong một đợt thì đợt sắp tới chính là đợt đó, Đang diễn ra — ĐẠT
- Đúng ngày cuối của đợt thì đợt đó vẫn là Đang diễn ra — ĐẠT

### Qt3aNextMilestoneTests

- Mốc kế tiếp của cả 32 đảng viên khớp kết quả mong đợi — ĐẠT
- Người đã vượt mốc lớn nhất thì không còn ngày tròn mốc kế tiếp — ĐẠT
- Người dưới mốc đầu tiên vẫn có ngày tròn mốc kế tiếp đúng — ĐẠT
- Tuổi đảng đúng bằng một mốc thì mốc kế tiếp là mốc sau nó — ĐẠT
- Tuổi đảng dưới mốc đầu tiên thì mốc kế tiếp là mốc đầu tiên — ĐẠT
- Tuổi đảng ngay dưới một mốc thì mốc kế tiếp chính là mốc đó — ĐẠT
- Tuổi đảng của cả 32 đảng viên khớp kết quả mong đợi — ĐẠT
- Tuổi đảng đúng bằng mốc lớn nhất thì không còn mốc kế tiếp — ĐẠT
- Đổi Bước sang 10 thì các mốc biến mất bị bỏ qua khi tìm mốc kế tiếp — ĐẠT
- Tuổi đảng vượt mốc lớn nhất thì không còn mốc kế tiếp — ĐẠT

### Qt11PeriodStatusByYearTests

- QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026 (5 biến thể) — ĐẠT
- QT11 · Đợt 29/02 ở năm không nhuận xét theo 28/02 — ĐẠT
- QT11 · Quá tải không tham số năm vẫn xét đúng năm hiện tại — ĐẠT
- QT11 · Năm đã qua thì mọi đợt là Đã qua, năm sau thì là Sắp tới — ĐẠT

### Qt1MilestoneSequenceTests

- Bắt đầu hoặc Kết thúc không dương thì bị từ chối bằng lỗi nghiệp vụ (4 biến thể) — ĐẠT
- Bước lớn hơn cả khoảng 30–90 thì dãy chỉ còn mốc đầu — ĐẠT
- Bắt đầu bằng Kết thúc thì dãy chỉ có đúng một mốc — ĐẠT
- Bước nhỏ hơn 1 thì bị từ chối bằng lỗi nghiệp vụ (2 biến thể) — ĐẠT
- Bước không rơi đúng Kết thúc thì dãy dừng trước, không vượt quá — ĐẠT
- Bắt đầu lớn hơn Kết thúc thì bị từ chối bằng lỗi nghiệp vụ — ĐẠT
- Cài đặt mặc định cho đúng 13 mốc, từ 30 tới 90 — ĐẠT
- Bước 10 cho đúng 7 mốc — ĐẠT

### Qt11PeriodStatusTests

- Đợt chưa mở thì là Sắp tới kèm số ngày còn lại — ĐẠT
- Đúng ngày đầu của đợt thì trạng thái là Đang diễn ra — ĐẠT
- Trạng thái đợt khớp kết quả mong đợi ở mọi kịch bản của bộ dữ liệu (7 biến thể) — ĐẠT
- Đợt bắt đầu 29/02 ở năm không nhuận thì xét theo 28/02 — ĐẠT
- Một ngày sau khi đợt đóng thì trạng thái là Đã qua — ĐẠT
- Đúng ngày cuối của đợt thì trạng thái vẫn là Đang diễn ra — ĐẠT
- Đợt kết thúc 29/02 giữ nguyên ngày khi gắn vào năm nhuận — ĐẠT
- Đợt đã đóng thì là Đã qua và không có số ngày còn lại — ĐẠT
- Một ngày trước khi đợt mở thì đếm ngược còn đúng một ngày — ĐẠT
- Hôm nay nằm trong đợt thì là Đang diễn ra, không đếm ngược — ĐẠT

### Qt6PeriodWarningTests

- Đợt bắt đầu 29/02 vẫn được chấp nhận — ĐẠT
- Ngày hoặc tháng không có thật bị từ chối bằng lỗi nghiệp vụ (6 biến thể) — ĐẠT
- Năm nhuận thì khoảng trống kết thúc đúng ngày tháng Hai — ĐẠT
- Hai đợt nối đuôi nhau, không dùng chung ngày nào, thì không bị báo — ĐẠT
- Bộ bốn đợt chính để hở đúng năm khoảng trống của năm 2026 — ĐẠT
- Bộ đợt phủ kín cả năm thì không còn khoảng trống nào — ĐẠT
- Chưa cài đợt nào thì cả năm là một khoảng trống — ĐẠT
- Bộ bốn đợt chính không chồng lấn nên không có cảnh báo — ĐẠT
- Đợt có Từ ngày sau Đến ngày bị từ chối bằng lỗi nghiệp vụ — ĐẠT
- Hai đợt cắt nhau thì cảnh báo nêu đúng cặp đợt theo thứ tự — ĐẠT
- Chưa cài đợt nào thì nhãn khoảng trống là Trước đợt đầu tiên — ĐẠT
- Khoảng trống đầu năm và cuối năm được đặt nhãn theo vị trí — ĐẠT
- Hai đợt chỉ dùng chung đúng một ngày cũng bị báo chồng lấn — ĐẠT

### Qt2AnniversaryTests

- Mốc âm bị từ chối bằng lỗi nghiệp vụ — ĐẠT
- Vào Đảng 29/02, năm đích nhuận thì giữ nguyên 29/02 — ĐẠT
- Ngày thường cộng mốc thì giữ nguyên ngày và tháng — ĐẠT
- Mốc 0 trả đúng ngày vào Đảng — ĐẠT
- Vào Đảng 29/02, năm đích không nhuận thì lùi về 28/02 — ĐẠT

### Qt3PartyAgeTests

- Chưa tới ngày kỷ niệm trong năm thì chưa được tính thêm năm — ĐẠT
- Vào Đảng đúng hôm nay thì tuổi đảng bằng 0 — ĐẠT
- Một ngày trước ngày kỷ niệm thì tuổi đảng vẫn là con số cũ — ĐẠT
- Ngày vào Đảng ở tương lai thì tuổi đảng bằng 0 — ĐẠT
- Đúng ngày kỷ niệm thì được tính thêm năm đó — ĐẠT
- Vào Đảng 29/02 thì năm không nhuận tính tuổi từ 28/02 — ĐẠT
- Người cao tuổi đảng nhất cho ra con số vượt mốc lớn nhất — ĐẠT
- Vào Đảng 29/02, một ngày trước 28/02 thì chưa được tính thêm năm — ĐẠT

### Qt7MissedMilestoneTests

- Ngày tròn mốc rơi trong một đợt thì người đó không bị sót — ĐẠT
- Danh sách người bị sót của cả bộ lõi khớp kết quả mong đợi (4 biến thể) — ĐẠT
- Chưa cài đợt nào thì người bị sót mang nhãn khoảng trống cả năm — ĐẠT
- Trong năm không có mốc nào rơi vào thì người đó không bị sót — ĐẠT
- Ngày tròn mốc rơi giữa hai đợt thì nhãn nêu tên cả hai đợt kề — ĐẠT
- Đổi Bước sang 10 thì người có mốc biến mất rời danh sách bị sót — ĐẠT
- Ngày tròn mốc rơi trước đợt đầu tiên thì mang nhãn Trước đợt đầu tiên — ĐẠT
- Ngày tròn mốc rơi sau đợt cuối thì mang nhãn Sau đợt cuối cùng — ĐẠT

## Tầng 1 · HuyHieuDang.Core.QcTests — bản đối chứng độc lập của QC

### Qc03Qt3PartyAgeTests

- U-353/U-354/U-357 · Người đã vượt mốc lớn nhất hết mốc kế tiếp, kể cả khi đổi Bước (4 biến thể) — ĐẠT
- U-307/U-308/U-309 · Người vào Đảng 29/02 xét ở năm không nhuận (3 biến thể) — ĐẠT
- U-304/U-305/U-306 · Các mốc tuổi đảng đặc biệt của bộ lõi tại T0 (3 biến thể) — ĐẠT
- QC · Tuổi đảng khớp oracle của QC trên mọi ngày 2024–2030 của cả bộ lõi — ĐẠT
- U-355 · L02 (29/02/1988) có mốc kế tiếp 40 rơi đúng 29/02/2028 — ĐẠT
- U-301/U-302/U-303 · Tuổi đảng quanh đúng ngày kỷ niệm tại T0 (3 biến thể) — ĐẠT
- QC · Tuổi đảng chỉ tăng theo thời gian, không bao giờ tụt (quét bộ lõi 2024–2030) — ĐẠT
- U-356 · Mốc kế tiếp phải LỚN HƠN tuổi đảng, không được bằng — ĐẠT
- QC · Mốc kế tiếp khớp oracle của QC trên cả bộ lõi ở T0/T1/T2, hai cài đặt — ĐẠT

### Qc01Qt1MilestoneTests

- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt (9 biến thể) — ĐẠT
- U-110 · 1 / 100 / 1 cho đúng 100 mốc, không treo — ĐẠT
- QC · Gọi hai lần cùng cài đặt cho hai danh sách bằng nhau và tách rời nhau — ĐẠT
- U-106 · Mốc cuối đúng bằng Kết thúc thì phải có trong dãy — ĐẠT
- U-104 · Bước lớn hơn cả khoảng 30–90 thì chỉ còn mốc đầu — ĐẠT
- U-101/U-102 · Dãy mốc khớp expected.json của QC (2 biến thể) — ĐẠT
- U-107/U-108/U-109 · Cài đặt sai bị từ chối bằng lỗi nghiệp vụ, không trả dãy rỗng — ĐẠT
- QC-04 · Bước = int.MaxValue chỉ được cho ra mốc đầu, không được tràn số — ĐẠT
- U-101 · Dãy mốc luôn tăng dần và không trùng — ĐẠT

### Qc06Qt6PeriodTests

- U-610 · Bộ 4 đợt chính để hở đúng 5 khoảng trống ở mọi năm xét (4 biến thể) — ĐẠT
- U-602 · Đợt có Từ ngày > Đến ngày phải bị từ chối — ĐẠT
- U-606/U-607 · Ngày/tháng không tồn tại phải báo lỗi nghiệp vụ — ĐẠT
- U-609/U-611/U-612 · Cảnh báo chồng lấn khớp oracle của QC — ĐẠT
- U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày (5 biến thể) — ĐẠT
- QC · Khoảng trống khớp oracle độc lập của QC trên mọi bộ đợt, 2024–2030 — ĐẠT
- U-609 · Hai đợt chồng lấn vẫn được tính bình thường, chỉ là cảnh báo — ĐẠT
- QC · Các khoảng trống không chồng nhau, không thủng, phủ đúng phần còn lại của năm — ĐẠT

### Qc07Qt7MissedMilestoneTests

- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch (6 biến thể) — ĐẠT
- U-712 · Không cài đợt nào thì 27 người của bộ lõi đều bị sót ở 2026 — ĐẠT
- QC · Người bị sót và người đủ điều kiện là hai tập rời nhau — ĐẠT
- U-711 · Bộ đợt phủ kín cả năm thì không ai bị sót — ĐẠT
- QC · Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp expected.json (4 biến thể) — ĐẠT
- U-710 · Đổi Bước sang 10 thì S04 rời danh sách bị sót, còn 6 người — ĐẠT
- QC-02 · Nhãn khoảng trống khi chưa cài đợt nào phải khớp expected.json — ĐẠT

### Qc04Qt4EligibilityTests

- QC · Đủ điều kiện khớp oracle của QC trên mọi đợt × mọi năm 2020–2035 — ĐẠT
- U-416/U-417 · Số người đủ điều kiện Đợt 7/11 năm 2026 theo Bước 5 và Bước 10 (2 biến thể) — ĐẠT
- U-413/U-414 · Đợt có Từ ngày 29/02: thu về 28/02 ở 2026, giữ 29/02 ở 2028 — ĐẠT
- QC · Danh sách đủ điều kiện từng đợt/từng năm khớp expected.json của QC (2 biến thể) — ĐẠT
- U-418 · Không ai đủ điều kiện ở hai đợt cùng một năm — ĐẠT
- U-416 · Phân bổ mốc của Đợt 7/11 năm 2026: 30×3, 35×1, 40×1, 45×1 — ĐẠT
- U-415 · Đợt một ngày (Từ = Đến = 01/10) chỉ nhận đúng người tròn mốc hôm đó — ĐẠT
- U-411/U-412 · N01 chỉ đủ điều kiện Đợt 7/11 của năm 2027, không phải 2026 — ĐẠT
- U-408/U-409 · L02 chỉ đủ điều kiện ở năm nhuận 2028 với mốc 40 — ĐẠT

### Qc08Qt8UpcomingPeriodTests

- QC · Đợt sắp tới luôn có Đến ngày không nhỏ hơn hôm nay — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch (7 biến thể) — ĐẠT
- U-808 · Hai đợt cùng Từ ngày phải cho kết quả tất định — ĐẠT
- U-809 · Đợt 29/02 sang năm không nhuận: thu về 28/02 và đếm ngày theo ngày đã thu — ĐẠT
- U-807 · Chưa cài đợt nào thì không có đợt sắp tới — ĐẠT
- QC · Đợt sắp tới khớp oracle của QC ở mọi ngày của 2026 và 2028 — ĐẠT

### Qc11ClockScanTests

- A-901 · Module nghiệp vụ trong Infrastructure không đọc đồng hồ (trừ 7 dòng tầng khung) — ĐẠT
- A-901 · Không nơi nào trong BE/src đọc đồng hồ theo giờ máy (T-FIX-2) — ĐẠT
- QC-05 · Core không được đọc đồng hồ hệ thống — BỎ QUA (chưa ghi lý do)
- A-901 · Service tính mốc tuổi đảng không đọc đồng hồ, chỉ nhận today qua tham số — ĐẠT

### Qc02Qt2AnniversaryTests

- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc (7 biến thể) — ĐẠT
- QC · Đối chiếu mọi ngày của 4 năm (nhuận và không nhuận) với oracle của QC — ĐẠT
- U-205 · 29/02/1896 + 4 năm → 1900 chia hết 100 nhưng KHÔNG nhuận → 28/02 — ĐẠT
- U-206 · 28/02/1996 + 32 năm → 28/02/2028, không được nhảy sang 29/02 — ĐẠT
- U-207 · 31/01/1996 + 30 năm → 31/01/2026, ngày cuối tháng không bị đụng — ĐẠT
- U-204 · 29/02/1996 + 4 năm → 2000 chia hết 400 nên vẫn nhuận — ĐẠT
- U-208 · Mốc 0 trả đúng ngày vào Đảng — ĐẠT

### Qc05Qt5NoStoredResultTests

- U-501 · Gọi hai lần với cùng dữ liệu cho cùng kết quả, không tác dụng phụ — ĐẠT
- U-503 · Không có bảng/entity nào lưu danh sách đủ điều kiện — ĐẠT
- U-502 · Đổi cài đặt giữa hai lần gọi thì lần thứ hai đổi theo ngay — ĐẠT
- U-501 · Service không sửa danh sách đợt và danh sách mốc được truyền vào — ĐẠT
- U-502 · Nới Đến ngày của đợt thì người đang bị sót chuyển sang đủ điều kiện ngay — ĐẠT

### Qc10PerformanceTests

- U-1201 · Tính đủ điều kiện 1 đợt / 1 năm cho 10.000 đảng viên dưới 1 giây — ĐẠT
- QC · Quét 'chưa thuộc đợt nào' cho 10.000 đảng viên dưới 1 giây — ĐẠT

### Qc09Qt11PeriodStatusTests

- QC · Trạng thái 4 đợt chính khớp expected.json ở T0, T1, T2 (3 biến thể) — ĐẠT
- QC · Chỉ trạng thái Sắp tới mới có số ngày còn lại, và luôn dương — ĐẠT
- QC · Trạng thái và số ngày còn lại khớp oracle của QC ở mọi ngày 2026–2028 — ĐẠT
- U-1102/U-1107 · Số ngày còn lại đếm đúng từng ngày trước Từ ngày của Đợt 7/11 — ĐẠT

## Tầng 1 · HuyHieuDang.Infrastructure.UnitTests — đọc Excel, dấu thời gian, lược đồ

### BusinessSchemaTests

- Cài đặt mặc định là 30 / 90 / 5 và không có tên đơn vị — ĐẠT
- Ngày vào Đảng chính thức là cột bắt buộc — ĐẠT
- Mọi bảng nghiệp vụ đặt tên theo kiểu gạch dưới (3 biến thể) — ĐẠT
- Cột Họ tên sắp xếp theo bảng chữ cái tiếng Việt — ĐẠT
- Giới tính chỉ có Nam và Nữ — ĐẠT
- Không có bảng nào lưu kết quả đủ điều kiện, đúng QT5 — ĐẠT
- Đợt trao huy hiệu không lưu năm, chỉ lưu ngày và tháng — ĐẠT
- Cột ngày của đảng viên là ngày thuần, không kèm giờ — ĐẠT
- Tên đợt là duy nhất và không phân biệt hoa thường — ĐẠT

### PartyMemberImportRowValidatorTests

- OQ-5: giới tính không phân biệt hoa thường (5 biến thể) — ĐẠT
- OQ-6: ngày nhận cả một chữ số lẫn hai chữ số (3 biến thể) — ĐẠT
- Ngày chính thức đúng bằng hôm nay là hợp lệ — ĐẠT
- OQ-2: một dòng nhiều lỗi trả đủ mọi lý do, đúng thứ tự bảng mã lỗi — ĐẠT
- Ngày chính thức ở tương lai → FutureOfficialAdmissionDate — ĐẠT
- OQ-10: ngày sinh bằng đúng ngày chính thức vẫn là lỗi — ĐẠT
- OQ-1: ngày sinh 31/02/1974 không có thật → InvalidDateFormat — ĐẠT
- 29/02 năm nhuận là ngày có thật, không bị coi là sai định dạng — ĐẠT
- Ngày sinh và giới tính bỏ trống vẫn hợp lệ, trả null — ĐẠT
- Ngày chính thức dạng yyyy-MM-dd → InvalidDateFormat — ĐẠT
- 29/02 năm không nhuận là ngày không có thật → InvalidDateFormat — ĐẠT
- Dòng đủ bốn ô hợp lệ thì không có lỗi và được chuẩn hóa — ĐẠT
- Thiếu họ tên → MissingFullName — ĐẠT
- Ngày sinh sau ngày chính thức → BirthDateAfterAdmissionDate — ĐẠT
- Thiếu ngày chính thức → MissingOfficialAdmissionDate, không kèm lỗi định dạng — ĐẠT
- Giới tính lạ → InvalidGender — ĐẠT

### AwardPeriodCoverageBuilderTests

- QT6 · Bộ đợt mẫu cho đúng một cặp chồng lấn kèm khoảng ngày dùng chung — ĐẠT
- QT2 · Đợt 29/02 ở năm không nhuận lùi về 28/02 — ĐẠT
- QT6 · Đợt nằm lọt trong đợt khác: báo chồng lấn, không cắt thêm đoạn — ĐẠT
- QT6 · Bộ đợt mẫu cho đúng hai khoảng trống kèm tên hai đợt kề — ĐẠT
- QT6 · Chưa cài đợt nào: dải vẫn là một khoảng trống cả năm, cảnh báo gaps rỗng — ĐẠT
- UC-36 · Dải độ phủ liền mạch 01/01–31/12, phần chồng lấn thuộc đợt đến trước — ĐẠT

### SkeletonTests

- Tài khoản admin là người dùng JWT hợp lệ — ĐẠT
- Tài khoản admin kiểm tra được mật khẩu — ĐẠT

### DateTimeProviderTests

- Đồng hồ hệ thống chạy khớp giờ UTC — ĐẠT
- Hôm nay là ngày theo lịch Việt Nam — ĐẠT
- Đồng hồ hệ thống dùng múi giờ Việt Nam UTC+7 — ĐẠT

## Tầng 2 · HuyHieuDang.Infrastructure.IntegrationTests

### SkeletonIntegrationTests

- Cấu hình cơ sở dữ liệu mặc định để trống chuỗi kết nối — ĐẠT

## Tầng 2 · HuyHieuDang.Web.IntegrationTests — API trên PostgreSQL thật (Backend viết)

### SettingsEndpointTests

- 7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400 (5 biến thể) — ĐẠT
- 7.2 · A-508 · Tên đơn vị bỏ trống hoặc toàn khoảng trắng đều thành chưa đặt (3 biến thể) — ĐẠT
- 7.2 · Tên đơn vị quá 200 ký tự trả 400 Mes.AppSetting.OverLength.UnitName — ĐẠT
- 7.1 · A-501 · Kho chưa có bản ghi cài đặt nào thì đọc ra mặc định 30/90/5 — ĐẠT
- 7.2 · A-509 · Kho trống: lưu lần đầu tạo đúng một bản ghi, không nhân bản — ĐẠT
- 7.2 · A-504 · Bước 0 hoặc âm trả 400 Mes.AppSetting.Invalid.StepYears (2 biến thể) — ĐẠT
- 7.4 · Tham số xem trước sai trả cùng bộ khóa lỗi với 7.2 — ĐẠT
- 7.2 · A-509 · Lưu hai lần liên tiếp vẫn chỉ có đúng một bản ghi cài đặt — ĐẠT
- 7.2 · A-504 · Mốc bắt đầu / kết thúc ≤ 0 trả đúng khóa lỗi của từng trường (4 biến thể) — ĐẠT
- 7.2 · A-502 · Đổi Bước 5 → 10: danh sách đủ điều kiện và badge đổi ngay (QT5) — ĐẠT
- 7.3 · A-506 · Khôi phục mặc định đưa về 30/90/5 và giữ nguyên Tên đơn vị — ĐẠT
- 7.2 · A-507 · Lưu Tên đơn vị: đọc lại thấy ngay, Dashboard cũng thấy — ĐẠT
- 7.1 · Kho đã seed: đọc ra 30/90/5, 13 mốc và tên đơn vị đang lưu — ĐẠT
- 7.4 · Bỏ trống tham số nào thì lấy giá trị đang lưu của tham số đó — ĐẠT
- 7.1 → 7.4 · Chưa đăng nhập thì cả bốn endpoint đều trả 401 — ĐẠT
- 7.4 · Xem trước dãy mốc không ghi gì xuống cơ sở dữ liệu — ĐẠT
- 7.2 · A-503 · Bắt đầu > Kết thúc trả 400 Mes.AppSetting.Invalid.Range — ĐẠT

### PartyMemberEndpointTests

- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400 (6 biến thể) — ĐẠT
- 3.3 · Thêm mới trả 200 kèm bản ghi vừa tạo và khóa Create.Successfully — ĐẠT
- 3.1 · Lọc giới tính Nam / Nữ; bỏ tham số thì lấy tất cả (3 biến thể) — ĐẠT
- 3.6 · Xóa nhiều trả đúng id đã xóa, id lạ bị bỏ qua lặng lẽ — ĐẠT
- 3.5 · Xóa id không tồn tại trả 400 Mes.PartyMember.NotFound — ĐẠT
- 3.1 · pageSize hoặc current không dương trả 400 (2 biến thể) — ĐẠT
- 3.2 · Lấy một trả đúng bản ghi; id lạ trả 400 Mes.PartyMember.NotFound — ĐẠT
- 3.1 · Tìm theo họ tên chứa chuỗi, không phân biệt hoa thường — ĐẠT
- 3.4 · Sửa id không tồn tại trả 400 Mes.PartyMember.NotFound — ĐẠT
- 3.1 · Sắp xếp theo cột; cột tính ra bị bỏ qua và quay về mặc định — ĐẠT
- A-001 · Mọi endpoint đảng viên không kèm token trả 401 (3 biến thể) — ĐẠT
- 3.5 · Xóa một trả id vừa xóa và bản ghi biến mất hẳn — ĐẠT
- 3.1 · Phân trang: mặc định 20 dòng, chọn được số dòng và số trang — ĐẠT
- 3.1 · Danh sách trả gender chuỗi và ba giá trị tuổi đảng tính theo hôm nay — ĐẠT
- 3.3 · QT9 · Thêm hai người trùng hệt nhau vẫn thành hai bản ghi — ĐẠT
- 3.4 · Sửa ghi đè cả bốn trường, kể cả xóa bằng null — ĐẠT
- 3.6 · Xóa nhiều với danh sách rỗng trả 400 — ĐẠT
- 3.3 · Đúng ngày hôm nay là ngày chính thức hợp lệ — ĐẠT

### ImportTransactionTests

- 4.3 · Hỏng đúng lúc chốt giao dịch: 32 dòng của core-hop-le.xlsx bị thu hồi hết — ĐẠT
- 4.3 · Hỏng ở lô thứ hai của bulk-1200.xlsx: 200 dòng đầu đã ghi vẫn bị thu hồi hết — ĐẠT
- 4.3 · Cùng đường đi đó nhưng không hỏng: bulk-1200.xlsx thêm đủ 1200 người — ĐẠT

### EligibilityEndpointTests

- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (16 biến thể) — ĐẠT
- 6.2 → 6.4 · Thiếu token trả 401 (3 biến thể) — ĐẠT
- 6.3 · Các năm khác của bộ lõi khớp expected.json (3 biến thể) — ĐẠT
- 6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year (5 biến thể) — ĐẠT
- 6.2 · Id đợt lạ trả Mes.AwardPeriod.NotFound — ĐẠT
- 6.3 · Đổi Bước 5 → 10 làm danh sách còn 6 người ngay (QT1, QT5) — ĐẠT
- 6.3 · Chưa thuộc đợt nào năm 2026: 7 người, đúng nhãn khoảng trống (QT7) — ĐẠT
- 6.2 · Nới Đến ngày Đợt 2/9 sang 30/09: đợt lên 5 người ngay, không lưu gì (QT5) — ĐẠT
- 6.2 · Bỏ trống year thì lấy năm hiện tại của máy chủ — ĐẠT
- 6.3 · Chưa cài đợt nào: mọi người tròn mốc đều rơi vào Trước đợt đầu tiên — ĐẠT
- 6.4 · Badge luôn bằng số dòng của màn Chưa thuộc đợt nào — ĐẠT

### AuthEndpointTests

- Thiếu tài khoản hoặc mật khẩu trả 400 kèm khóa Required (2 biến thể) — ĐẠT
- A-004 · JWT thiếu tiền tố Bearer trả 401 — ĐẠT
- A-002 · JWT sai chữ ký trả 401 — ĐẠT
- Hạn token đúng 8 giờ theo hợp đồng mục 1.2 — ĐẠT
- Đăng xuất có token trả 200 và khóa Mes.User.Logout.Successfully — ĐẠT
- A-001 · Gọi endpoint nghiệp vụ không kèm token trả 401 và không lộ dữ liệu (2 biến thể) — ĐẠT
- A-006 · Đăng nhập đúng trả 200 kèm token đúng hợp đồng — ĐẠT
- A-006 · Token vừa cấp dùng được ngay cho endpoint khác — ĐẠT
- A-003 · JWT đã hết hạn trả 401 — ĐẠT
- Token hợp lệ nhưng chủ thể không còn tồn tại trả 401 — ĐẠT
- A-005 · Sai mật khẩu và không có tài khoản trả cùng một thông báo 401 — ĐẠT

### ExportEndpointTests

- A-701, A-702, A-703 · Tên file đúng quy tắc rút gọn của mục 1.9 (4 biến thể) — ĐẠT
- 8.1, 8.3 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year (4 biến thể) — ĐẠT
- 8.1 · Id đợt lạ trả Mes.AwardPeriod.NotFound — ĐẠT
- 8.2 · Chưa cài đợt nào trả Mes.Dashboard.NotFound.UpcomingPeriod — ĐẠT
- A-705 · Tên đơn vị trống: dòng 1 là tên đợt, không có dòng trắng thừa — ĐẠT
- 8.1 → 8.3 · Thiếu token trả 401 (3 biến thể) — ĐẠT
- A-704, A-706, A-708, A-710 · Đợt 7/11 · 2026: tiêu đề, cột và 6 dòng dữ liệu — ĐẠT
- A-709 · Không ai bị sót: file chưa thuộc đợt nào vẫn hợp lệ — ĐẠT
- 8.2 · Xuất Dashboard lấy đúng đợt sắp tới theo QT8 — ĐẠT
- 8.2 · Mọi đợt của năm nay đã qua: file mang đợt đầu năm sau — ĐẠT
- A-707 · E01 thiếu Ngày sinh và Giới tính: hai ô rỗng, không phải dấu gạch ngang — ĐẠT
- 8.3 · Chưa thuộc đợt nào 2026: 7 dòng, có cột Khoảng trống đúng nhãn — ĐẠT
- A-703 · Tên file chưa thuộc đợt nào là ChuaThuocDot_<năm>.xlsx — ĐẠT
- A-709 · Danh sách rỗng vẫn trả file hợp lệ chỉ có phần tiêu đề — ĐẠT
- 8.1, 8.3 · Bỏ trống year thì lấy năm hiện tại của máy chủ — ĐẠT

### DashboardEndpointTests

- 6.1 · Kho trống hoàn toàn: đủ hai cảnh báo cho khối hướng dẫn ba bước (UC-13) — ĐẠT
- 6.1 · Cảnh báo của Dashboard trùng khớp với cảnh báo màn Đợt — ĐẠT
- 6.1 · T0 = 19/09/2026: đợt sắp tới là Đợt 7/11 · 2026, còn 12 ngày, 6 người — ĐẠT
- 6.1 · Chưa cài đợt nào: upcomingPeriod = null, bảng rỗng, cảnh báo noPeriods — ĐẠT
- 6.1 · T1 = 15/10/2026: Đợt 7/11 đang diễn ra, daysRemaining = null — ĐẠT
- 6.1 · T2 = 01/12/2026: mọi đợt 2026 đã qua nên đợt sắp tới là Đợt 3/2 · 2027 (QT8) — ĐẠT
- Gọi Dashboard không kèm token trả 401 — ĐẠT
- 6.1 · Chưa có đảng viên nào: cảnh báo noMembers, đợt sắp tới vẫn có, 0 người — ĐẠT

### AwardPeriodEndpointTests

- 5.3 · Thêm đợt chồng lấn vẫn là 200 và trả kèm cảnh báo — ĐẠT
- 5.1 → 5.5 · Thiếu token trả 401 (3 biến thể) — ĐẠT
- 5.3 · Ba lỗi chặn lưu: thiếu tên, ngày không có thật, đợt vắt qua năm (4 biến thể) — ĐẠT
- 5.1 · Năm khác: trạng thái vẫn so với hôm nay, ngày gắn đúng năm được hỏi — ĐẠT
- 5.1 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year (2 biến thể) — ĐẠT
- 5.1 · Bốn đợt mẫu: sắp theo fromDate, ba trạng thái và số người đủ điều kiện — ĐẠT
- 5.1 · Chưa cài đợt nào: cảnh báo gaps rỗng, dải vẫn phủ trọn 01/01–31/12 — ĐẠT
- 5.4 · Sửa đợt: giữ nguyên tên của chính nó, có hiệu lực ngay cho năm hiện tại — ĐẠT
- 5.2 · Lấy một đợt theo id và theo năm được hỏi; id lạ trả NotFound — ĐẠT
- 5.1 · Dải độ phủ liền mạch 01/01–31/12, phần chồng lấn thuộc đợt đến trước — ĐẠT
- 5.5 · Xóa hẳn đợt, trả khoảng trống mới và không đụng đảng viên (QT10) — ĐẠT
- 5.4 · Sửa sang tên đợt khác trả Repeated.Name; id lạ trả NotFound — ĐẠT
- 5.1 · Cảnh báo liệt kê đúng một cặp chồng lấn và một khoảng trống — ĐẠT
- 5.3 · Trùng tên không phân biệt hoa thường trả Mes.AwardPeriod.Repeated.Name — ĐẠT

### ImportEndpointTests

- 4.3 · Lỗi cấp file cũng chặn ở bước nạp (2 biến thể) — ĐẠT
- 4.3 · Nạp loi-4-dong.xlsx: thêm 6 người, bỏ qua 4 dòng lỗi, không kiểm tra trùng — ĐẠT
- 4.2 · loi-4-dong.xlsx: 10 dòng, 6 hợp lệ, 4 lỗi ở dòng 8, 9, 10, 11 — ĐẠT
- 4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa (5 biến thể) — ĐẠT
- 4.2 · Bộ lõi 32 dòng hợp lệ, đọc được cả ngày dạng chuỗi lẫn ngày kiểu ngày (2 biến thể) — ĐẠT
- Ba endpoint đều yêu cầu token — ĐẠT
- 4.2 · loi-moi-loai-mot-dong.xlsx: 8 dòng lỗi, mỗi loại một dòng, dòng nhiều lỗi trả đủ lý do — ĐẠT
- 4.2 · File quá 10 MB bị chặn trước khi mở, trả Mes.Import.Invalid.FileSize — ĐẠT
- 4.3 · Nạp file toàn lỗi: không thêm ai, không ném lỗi — ĐẠT
- 4.1 · File mẫu hệ thống sinh khớp fixture mau-dang-vien.xlsx của QC — ĐẠT
- 4.1 · File mẫu trả file nhị phân đúng tên, 4 cột đúng thứ tự, 2 dòng ví dụ dd/MM/yyyy — ĐẠT
- 4.2 · bien-chuan-hoa.xlsx: cắt khoảng trắng (OQ-4), giới tính hoa thường (OQ-5), ngày một chữ số (OQ-6) — ĐẠT
- 4.2 · Xem trước không ghi gì vào cơ sở dữ liệu — ĐẠT

### AnonymousEndpointTests

- Chỉ đúng một hành động được phép gọi khi chưa đăng nhập — ĐẠT
- Mọi controller đều kế thừa BaseController nên mặc định cần đăng nhập — ĐẠT

### UpdatedAtStampTests

- Thêm mới đảng viên được đóng dấu dù không ai gán UpdatedAt — ĐẠT
- Cài đặt seed sẵn mang dấu thời gian của IDateTimeProvider, không phải giờ máy — ĐẠT
- Sửa đảng viên ghi đè dấu thời gian cũ — ĐẠT

### EligibilityPerformanceTests

- 6.1 → 6.4 · 10.000 đảng viên: mỗi endpoint tính toán dưới 1 giây — ĐẠT

## Tầng 2 · HuyHieuDang.Web.QcIntegrationTests — API trên PostgreSQL thật (QC viết)

### A6ImportTests

- A-606 · Nạp cùng một file hai lần làm tổng tăng gấp đôi — QT9 nói rõ hệ thống **không** kiểm tra trùng, nên đây là hành vi đúng chứ không phải lỗi — ĐẠT
- A607_A611_Loi_cap_file_bi_chan_ngay (5 biến thể) — ĐẠT
- A-613 · File hợp lệ nặng 9,9 MB — biên dưới của ràng buộc — vẫn được chấp nhận — ĐẠT
- A-621 · Mỗi loại lỗi cấp dòng một dòng: bảng lỗi phải nêu đủ mọi lý do của một dòng, không dừng ở lý do đầu tiên (OQ-2) — ĐẠT
- A-602 · Nạp sau xem trước làm tổng tăng đúng 32 — ĐẠT
- A-616 · Cắt kết nối giữa lúc nạp không để lại dữ liệu nửa vời: sau khi hủy, số người phải là 0 hoặc trọn vẹn 1200, không bao giờ ở giữa (QT9 — nạp là một giao dịch) — ĐẠT
- A-609 · Thông báo của "chỉ có tiêu đề" phải khác thông báo của "file rỗng" (OQ-7) — ĐẠT
- A-604 · Nạp file có 4 dòng lỗi chỉ thêm đúng 6 người — ĐẠT
- A-618 · Nạp file 1200 dòng chạy trọn vẹn trong thời gian hợp lý — ĐẠT
- A-620 · Ô ngày kiểu ngày của Excel (số serial) cũng đọc được, đủ 32 dòng hợp lệ — ĐẠT
- A-619 · File mẫu (UC-25): đúng 4 cột đúng thứ tự, có dòng ví dụ, ngày dd/MM/yyyy, và **nạp lại chính file mẫu đó phải hợp lệ** — ĐẠT
- A-601 · Xem trước file lõi: 32 dòng hợp lệ, 0 lỗi, và **chưa** ghi vào cơ sở dữ liệu — ĐẠT
- A-603 · File có 4 dòng lỗi: xem trước báo 6 hợp lệ · 4 lỗi, đúng số dòng Excel 8/9/10/11 và đúng lý do từng dòng — ĐẠT
- A-605 · Chỉ xem trước rồi bỏ ngang thì tổng không đổi — ĐẠT
- A-614 · Không gửi file nào là lỗi 400 nói được, không phải 500 — ĐẠT
- A-615 · Gửi hai file cùng lúc được xử lý tất định, không phải lỗi 500 — ĐẠT
- A-612 · File 11 MB bị chặn bằng khóa dung lượng. File này cố ý không phải Excel hợp lệ: trả đúng khóa dung lượng chứng tỏ máy chủ chặn **trước khi** đọc nội dung — ĐẠT
- A-617 · Ép một lỗi kỹ thuật ở dòng cuối bằng một ràng buộc cơ sở dữ liệu tạm thời: toàn bộ lời gọi phải cuộn lại, tổng không đổi — ĐẠT

### A3DashboardTests

- A-305 · Không có đảng viên nào: cảnh báo đúng, bảng rỗng, đợt vẫn hiện — ĐẠT
- A-303 · Dashboard tại T2: mọi đợt của 2026 đã qua nên đợt sắp tới là Đợt 3/2 của **2027** (QT8), và cờ isNextYear phải bật — ĐẠT
- A-301 · Dashboard tại T0: đợt sắp tới là Đợt 7/11 năm 2026, còn 12 ngày, 6 người — ĐẠT
- A-308 · Cảnh báo chồng lấn và chưa phủ kín trên Dashboard phải khớp từng chữ với cảnh báo ở màn Đợt — cùng một nguồn tính — ĐẠT
- A-307 · Badge "chưa thuộc đợt nào" của năm nay đúng 7 người — ĐẠT
- A-306 · Kho trống hoàn toàn: cả hai cảnh báo bật, đủ dữ liệu cho khối 3 bước (UC-13) — ĐẠT
- A-304 · Không có đợt nào: báo "chưa cài đợt", không trả đợt rỗng giả, không lỗi 500 — ĐẠT
- A-302 · Dashboard tại T1: Đợt 7/11 đang diễn ra, không còn đếm ngược — ĐẠT

### A2AwardPeriodTests

- A-201 · Bốn đợt chính trả về đúng bốn dòng, sắp theo Từ ngày tăng dần — ĐẠT
- A-219 · Hỏi danh sách đủ điều kiện của đợt không tồn tại trả lỗi nghiệp vụ rõ ràng — ĐẠT
- A-212 · Nới Đến ngày của Đợt 2/9 từ 10/09 sang 30/09 làm danh sách đủ điều kiện và badge đổi ngay, không cần thao tác nào khác (QT5, QT6) — ĐẠT
- A-220 · Nạp thêm bộ lớn không làm lệch con số của bộ lõi — ĐẠT
- A-202 và A-203 · Trạng thái đợt tại T0 và T1 (QT11) — ĐẠT
- A-208 · Đợt 29/02 – 05/03 là hợp lệ; gắn năm không nhuận thì Từ ngày thu về 28/02, năm nhuận thì giữ 29/02 (QT4, mục 1.10 hợp đồng API) — ĐẠT
- A-211 · Bộ đợt phủ kín 01/01 – 31/12 không còn cảnh báo chưa phủ kín — ĐẠT
- A-214 · Đợt 7/11 năm 2026 có đúng 6 người, đúng phân bổ mốc, và sắp theo Mốc rồi tên gọi (mục 1.8 hợp đồng API) — ĐẠT
- A-204 · Cột "Đủ điều kiện năm nay" của từng dòng đúng 5 · 5 · 4 · 6 — ĐẠT
- A-213 · Xóa đợt làm đợt biến mất nhưng **không** đụng tới đảng viên nào (QT5) — ĐẠT
- A-209 · Hai đợt chồng lấn vẫn **lưu được**, kèm cảnh báo nêu đúng cặp đợt (QT6 — cảnh báo không bao giờ chặn lưu) — ĐẠT
- A-217 · Đợt 3/2 năm 2026 có Ngô Văn Khánh với ngày tròn mốc 28/02/2026 — ĐẠT
- A-218 · Năm ngoài khoảng cho phép được xử lý tất định, không phải lỗi 500 — ĐẠT
- A-205 → A-207 · Ba thứ chặn lưu: Từ sau Đến, trùng tên, ngày/tháng không có thật — ĐẠT
- A-210 · Bốn đợt chính để hở đúng 5 khoảng trống, liệt kê đúng từng khoảng — ĐẠT
- A-215 và A-216 · Đợt 7/11 năm 2027 và Đợt 3/2 năm 2028 (ngày tròn mốc 29/02) — ĐẠT

### A7ExportTests

- A-705 · Tên đơn vị để trống thì tiêu đề bỏ hẳn dòng đó, không để dòng trắng lạ — ĐẠT
- A-711 · Xuất từ Dashboard lấy đúng đợt sắp tới theo QT8, kể cả khi đợt đó thuộc năm sau (mục 8.2 hợp đồng API), và báo lỗi nói được khi chưa cài đợt nào — ĐẠT
- A-703 · Tên file của danh sách chưa thuộc đợt nào — ĐẠT
- A-712 · File "chưa thuộc đợt nào" có thêm cột cuối "Khoảng trống", nội dung khớp bảng đang xem — ĐẠT
- A-706 → A-708 · Đúng 6 dòng dữ liệu cho Đợt 7/11 · 2026, đúng cột, đúng thứ tự, ô trống để **rỗng** chứ không ghi dấu gạch ngang — ĐẠT
- A-701 và A-702 · Tên file bỏ dấu, đổi / thành -, gắn năm — ĐẠT
- A-704 · Dòng tiêu đề: tên đơn vị, tên đợt kèm khoảng ngày **đã gắn năm**, và ngày xuất bằng đúng hôm nay của máy chủ — ĐẠT
- A-709 · Xuất khi danh sách rỗng vẫn ra file có tiêu đề và 0 dòng dữ liệu — ĐẠT
- A-710 · Mở file bằng thư viện đọc Excel thật: đọc được, đúng một sheet, không cảnh báo hỏng. Kiểm cả ba endpoint xuất — ĐẠT

### A0AuthorizationTests

- A-001 · Gọi mọi endpoint nghiệp vụ không kèm JWT phải bị từ chối bằng 401 (27 biến thể) — ĐẠT
- A-002 · Token ký bằng khóa khác phải bị từ chối — ĐẠT
- A-006 · Đăng nhập đúng trả token dùng được ngay cho endpoint khác — ĐẠT
- A-003 · Token đúng khóa nhưng đã hết hạn phải bị từ chối — ĐẠT
- A-004 · Token gửi kèm mà thiếu tiền tố Bearer phải bị từ chối — ĐẠT
- A-005 · Sai mật khẩu và không có tài khoản phải cho **cùng một** thông điệp chung, không tiết lộ tài khoản nào có thật (UC-00) — ĐẠT
- A-007 · Ca kiểm tra tĩnh: quét mọi action của mọi controller, chỉ Auth/Login được phép mang  — ĐẠT

### A1PartyMemberTests

- A-116 · Thêm tay hợp lệ làm tổng tăng đúng 1 — ĐẠT
- A-123 · Xóa id không tồn tại trả lỗi nghiệp vụ rõ ràng, không phải lỗi 500 — ĐẠT
- A-124 · Xóa danh sách có id trùng nhau: không đếm trùng, không lỗi. Hợp đồng mục 3.6 chốt id lạ bị bỏ qua lặng lẽ — ĐẠT
- A-120 · Sửa Ngày chính thức làm tuổi đảng, mốc kế tiếp và danh sách đủ điều kiện đổi theo ngay, không cần thao tác nào khác (QT5) — ĐẠT
- A-126 · Lấy một đảng viên theo id (mục 3.2 hợp đồng API): trả đúng người đó, đúng các giá trị tính ra như trên danh sách; id lạ trả lỗi nghiệp vụ chứ không phải 404 — ĐẠT
- A-118 và A-119 · Ngày chính thức đúng bằng hôm nay là hợp lệ; sau hôm nay một ngày thì không (biên "≤ hôm nay theo lịch máy chủ") — ĐẠT
- A-104 · Trang vượt quá trang cuối trả danh sách rỗng, không phải lỗi 500 — ĐẠT
- A-125 · Dòng tóm tắt: tổng số khớp, và tuổi đảng trên từng dòng là tuổi tính đến hôm nay chứ không phải giá trị lưu sẵn (QT3, QT5) — ĐẠT
- A-108 và A-109 · Tìm "Nguyễn" và "nguyễn" đều ra 79 người (không phân biệt hoa thường) — ĐẠT
- A-117 · Thêm tay thiếu Họ tên bị từ chối bằng khóa thông điệp đúng — ĐẠT
- A-122 · Xóa nhiều người một lần làm tổng giảm đúng số lượng, xóa hẳn (QT10) — ĐẠT
- A-121 · Xóa một người làm tổng giảm đúng 1 — ĐẠT
- A-105 · Cỡ trang 50 và 100 cho đúng số trang và đúng số dòng trang cuối — ĐẠT
- A-110 và A-111 · Tìm đúng một người, và tìm chuỗi không tồn tại ra danh sách rỗng — ĐẠT
- A-103 · Trang cuối của cỡ trang 20 còn đúng 12 dòng — ĐẠT
- A-115 · Sắp theo Họ tên dùng đối chiếu tiếng Việt (OQ-3, mục 1.7 hợp đồng API): "Đào Văn Ân" đứng trước "Nguyễn Văn An", không theo mã Unicode — ĐẠT
- A-113 · Lọc giới tính Nam / Nữ / trống đúng 592 / 590 / 50 — ĐẠT
- A-106 · Cỡ trang 0, âm hay rất lớn phải bị chặn hoặc kẹp về mức trần — không treo, không trả cả kho dữ liệu về trong một lời gọi — ĐẠT
- A-102 · Trang đầu 20 dòng, tổng 1232, 62 trang — ĐẠT
- A-101 · Nạp bộ lõi và bộ lớn rồi lấy danh sách phải ra đúng 1232 người — ĐẠT
- A-107 · Ghép mọi trang lại phải ra đúng 1232 người, không trùng không thiếu — bắt lỗi phân trang thiếu sắp xếp tất định — ĐẠT
- A-114 · Sắp xếp theo từng cột, hai chiều: chiều nghịch phải đúng là chiều thuận đảo ngược, và ô trống không làm sai thứ tự — ĐẠT
- A-112 · Ký tự đặc biệt của SQL và của mẫu LIKE phải được tham số hóa: trả kết quả bình thường, không lỗi, và không kéo về cả kho như khi % bị hiểu là ký tự đại diện — ĐẠT

### A9TechnicalTests

- A-901 · Quét toàn bộ BE/src: không nơi nào ngoài lớp provider đọc đồng hồ máy (T-FIX-1). Vi phạm là lỗi chặn — ĐẠT
- A-906 · Mọi trường ngày nghiệp vụ trên dây là ngày thuần yyyy-MM-dd, không phải mốc thời gian có giờ (mục 1.6 hợp đồng API — dùng DateTime sẽ lệch một ngày) — ĐẠT
- A-903 · Biến môi trường ép ngày **không** được có hiệu lực ở Production. Hiện BE/src không đọc biến này ở bất cứ đâu, nên yêu cầu bảo mật của T-FIX-4 đang được thỏa mãn — ĐẠT
- A-904 · Tính danh sách đủ điều kiện của một đợt trong một năm với 10.000 đảng viên trong cơ sở dữ liệu phải xong dưới 1 giây (mục 7 tài liệu nghiệp vụ) — ĐẠT
- A-902 · Kết quả không phụ thuộc múi giờ của tiến trình (T-FIX-2): provider tự quy đổi sang Asia/Ho_Chi_Minh thay vì đọc múi giờ máy, nên chạy ở TZ=UTC cho cùng kết quả — ĐẠT
- A-905 · Mọi lỗi trả cho người dùng đều mang khóa thông điệp có trong bảng mục 1.5 hợp đồng API — tức Frontend dịch được sang tiếng Việt — và không lộ dấu vết ngăn xếp, tên kiểu .NET hay tên bảng/cột — ĐẠT
- A-903b · Mặt còn lại của T-FIX-4: ngoài Production, biến môi trường phải ép được "hôm nay" để Playwright chạy được ở T28. Hiện chưa cài đặt nên ca này để Skip kèm mã lỗi — BỎ QUA (QC-T27-01 · đã sửa ở PR #38, xác minh xanh 20/09/2026, chờ PR vào main)

### A8DefectTests

- QC-T27-04 · A-905 · Lỗi ép kiểu tham số do tầng gắn dữ liệu sinh ra trả câu tiếng Anh ("The value '2147483648' is not valid for Year.") thay vì một khóa Mes.*. Mục 1.5 hợp đồng API chốt Backend chỉ trả khóa, Frontend mới dựng chữ; câu tiếng Anh này hiện thẳng lên banner của người dùng — BỎ QUA (QC-T27-04 · đã sửa ở PR #35, xác minh xanh 20/09/2026, chờ PR vào main)
- QC-T27-07 · A-905 · Ba khóa thông điệp Backend thật sự trả ra nhưng bảng mục 1.5 hợp đồng API không có, nên Frontend chỉ hiện câu mặc định "Thao tác không thực hiện được" thay vì nói rõ chỗ sai — BỎ QUA (QC-T27-07 · CÒN MỞ, hợp đồng API mục 1.5 vẫn thiếu khóa)
- QC-T27-06 · A-905 · GET /api/Settings/Milestones?start=1&end=1000000&step=1 trả 200 với một triệu mốc, gần 7 MB JSON. Ô xem trước gọi endpoint này sau mỗi lần gõ phím, nên một lần gõ nhầm là treo cả trình duyệt — BỎ QUA (QC-T27-06 · đã sửa ở PR #35, xác minh xanh 20/09/2026, chờ PR vào main)
- QC-T27-03 · A-106 · Cỡ trang không có trần: gửi pageSize=1000000000 vẫn trả 200 kèm **toàn bộ** kho dữ liệu trong một phản hồi. Kế hoạch kiểm thử đòi "bị chặn hoặc kẹp về mức trần" — BỎ QUA (QC-T27-03 · đã sửa ở PR #35, xác minh xanh 20/09/2026, chờ PR vào main)
- QC-T27-02 · A-104, A-106 · Số trang lớn làm tràn số nguyên khi nhân với cỡ trang: máy chủ trả 500 kèm nguyên văn Npgsql.PostgresException: OFFSET must not be negative và cả dấu vết ngăn xếp. Kế hoạch kiểm thử đòi trang vượt quá trang cuối trả danh sách rỗng, không phải lỗi 500 — BỎ QUA (QC-T27-02 · đã sửa ở PR #35, xác minh xanh 20/09/2026, chờ PR vào main)
- QC-T27-05 · A-113 · Bộ lọc giới tính mang giá trị lạ (filter.Gender=$eq:Khac) hoặc rỗng bị bỏ qua lặng lẽ và trả **toàn bộ** danh sách. Người dùng tưởng đang lọc nhưng đang nhìn cả kho; hoặc phải trả 400, hoặc phải trả 0 dòng — BỎ QUA (QC-T27-05 · CÒN MỞ, chưa ai sửa)

### A4UnassignedTests

- A-401b · Khi **chưa cài đợt nào**, hợp đồng API mục 1.10 chốt mọi người tròn mốc trong năm mang gap.type = "BeforeFirst" với hai tên đợt null — ĐẠT
- A-403 · Năm 2025 và 2027 khớp expected.json — ĐẠT
- A-405 · Sắp theo Mốc huy hiệu tăng dần, trong cùng mốc thì theo Họ tên đầy đủ theo bảng chữ cái tiếng Việt — ĐẠT
- A-404 · Bộ đợt phủ kín thì không ai bị sót — ĐẠT
- A-402 · Đổi Bước sang 10 làm danh sách còn 6 người (QT1, QT5) — ĐẠT
- A-406 · Badge trên menu và số dòng của màn hình luôn khớp — hai con số phải đến từ cùng một nguồn tính. Kiểm ở bốn trạng thái dữ liệu khác nhau — ĐẠT
- A-401 · Năm 2026 có đúng 7 người, mỗi người mang đúng nhãn khoảng trống — ĐẠT

### A5SettingsTests

- A-506 · Khôi phục mặc định đưa mốc về 30 / 90 / 5 và **không** đụng tên đơn vị — ĐẠT
- A-509 · Gọi lưu hai lần liên tiếp vẫn chỉ có đúng một bản ghi cài đặt — ĐẠT
- A-502 · Lưu 30 / 90 / 10: dãy mốc đổi và **mọi** danh sách đủ điều kiện đổi theo ngay, không cần thao tác nào khác (QT1, QT5) — ĐẠT
- A-511 · Đăng xuất (mục 2.2 hợp đồng API): trả 200 và không hủy token phía máy chủ — JWT không trạng thái, việc xóa token là của Frontend — ĐẠT
- A-503 → A-505 · Cài đặt sai bị từ chối bằng đúng khóa thông điệp — ĐẠT
- A-510 · Xem trước dãy mốc (mục 7.4 hợp đồng API): không ghi gì vào cơ sở dữ liệu và từ chối tham số sai giống endpoint lưu — ĐẠT
- A-507 và A-508 · Tên đơn vị hiện ở phiên đăng nhập; để trống vẫn hợp lệ. Phần tiêu đề file Excel do ca A-704 và A-705 kiểm — ĐẠT
- A-501 · Kho trống vẫn đọc được cài đặt, trả đúng mặc định 30 / 90 / 5 — ĐẠT

---

## Tầng 3 · Playwright end-to-end trên trình duyệt thật (`FE/e2e`)

Chromium 1440×900, `vi-VN`, `Asia/Ho_Chi_Minh`, chạy với Backend và PostgreSQL 16
thật dựng bằng `FE/e2e/docker-compose.e2e.yml`. Mỗi luồng gồm nhiều `test.step`
mang đúng mã ca của `docs/test-plan.md` mục 6 — tổng 83 bước.

### E0 · Tiền đề đóng băng thời gian

- E0-01 · Backend đóng băng "hôm nay" theo `HUYHIEUDANG_TEST_TODAY` — BỎ QUA (QC-T28-01 · đã sửa ở PR #38, xác minh xanh 20/09/2026, chờ PR vào main)
- E0-02 · Ngày máy chủ nằm trong khoảng an toàn của bộ dữ liệu biên — ĐẠT
- E0-03 · Đồng hồ trình duyệt đóng băng đúng mốc và đúng múi giờ — ĐẠT

### Sáu luồng nghiệp vụ bắt buộc

- E2E-1 · Lần dùng đầu tiên: đăng nhập → Dashboard trống → cài mốc → tạo 4 đợt → import → Dashboard đúng đợt và đúng danh sách (19 bước) — ĐẠT
- E2E-2 · Import có lỗi: xem trước đúng số dòng hợp lệ và đúng lý do từng lỗi, nạp chỉ thêm dòng hợp lệ (15 bước) — ĐẠT
- E2E-3 · Đổi Bước từ 5 sang 10 lan truyền ngay sang chi tiết đợt, Dashboard, badge và màn Chưa thuộc đợt nào (10 bước) — ĐẠT
- E2E-4 · Nới Đến ngày của một đợt kéo người từ Chưa thuộc đợt nào sang danh sách đủ điều kiện, badge giảm đúng (12 bước) — ĐẠT
- E2E-5 · Xuất Excel từ Dashboard, chi tiết đợt và Chưa thuộc đợt nào: tên tệp, dòng tiêu đề, số dòng và từng ô khớp màn hình (10 bước) — ĐẠT
- E2E-6 · Vòng đời đảng viên: thêm tay → tìm → sửa ngày vào Đảng → xóa nhiều dòng, tổng giảm đúng (17 bước) — ĐẠT

### E9 · Ca về tính chạy-lại-được của chính bộ kiểm thử

- E-903 · Chạy lại toàn bộ ở mốc T1 = 15/10/2026 — BỎ QUA (QC-T28-01 · đã sửa ở PR #38; ca thật sẽ viết khi PR vào main)
- E-904 · Chạy lại toàn bộ ở mốc T2 = 01/12/2026 — BỎ QUA (cùng lý do với E-903)
- E-905 · Không ca nào dùng `waitForTimeout` để chờ cho chắc — ĐẠT
- E-906 · Mỗi luồng tự dựng trạng thái đầu ngay ở dòng đầu tiên — ĐẠT
