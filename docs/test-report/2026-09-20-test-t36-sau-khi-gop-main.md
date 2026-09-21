Báo cáo chạy toàn bộ bộ kiểm thử Backend sau khi gộp `main` vào nhánh `fix/T36-sua-loi-qc-t27`.
Lần chạy này khác lần trước ở chỗ `main` đã mang thêm dự án `HuyHieuDang.Web.QcIntegrationTests` của QC.

## Lệnh và tổng kết

```
cd BE && dotnet test HuyHieuDang.sln
```

Tổng 630 ca: 621 ĐẠT, 1 HỎNG, 8 bỏ qua.

Ca hỏng duy nhất nằm trong dự án của QC và là mâu thuẫn giữa hai yêu cầu, không phải lỗi mã nguồn —
chi tiết ở dòng tương ứng bên dưới.

## HuyHieuDang.Core.UnitTests — logic tính mốc tuổi đảng

### Qt11PeriodStatusByYearTests

- QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026 (fromDay: 1, fromMonth: 10, toDay: 7, toMonth: 11, expected: Upcoming, daysLeft: 12) — ĐẠT
- QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026 (fromDay: 1, fromMonth: 9, toDay: 30, toMonth: 9, expected: Ongoing, daysLeft: null) — ĐẠT
- QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026 (fromDay: 15, fromMonth: 1, toDay: 5, toMonth: 3, expected: Past, daysLeft: null) — ĐẠT
- QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026 (fromDay: 15, fromMonth: 8, toDay: 10, toMonth: 9, expected: Past, daysLeft: null) — ĐẠT
- QT11 · Ba trạng thái trong năm hiện tại với hôm nay cố định 19/09/2026 (fromDay: 19, fromMonth: 9, toDay: 19, toMonth: 9, expected: Ongoing, daysLeft: null) — ĐẠT
- QT11 · Năm đã qua thì mọi đợt là Đã qua, năm sau thì là Sắp tới — ĐẠT
- QT11 · Quá tải không tham số năm vẫn xét đúng năm hiện tại — ĐẠT
- QT11 · Đợt 29/02 ở năm không nhuận xét theo 28/02 — ĐẠT

### Qt11PeriodStatusTests

- Đợt kết thúc 29/02 giữ nguyên ngày trong năm nhuận — ĐẠT
- Đợt bắt đầu 29/02 ở năm không nhuận thì quy về 28/02 — ĐẠT
- Trạng thái đợt khớp kịch bản mẫu (scenarioName: "core_default_T0") — ĐẠT
- Trạng thái đợt khớp kịch bản mẫu (scenarioName: "core_default_T0_fullCover") — ĐẠT
- Trạng thái đợt khớp kịch bản mẫu (scenarioName: "core_default_T0_leapEdgePeriod") — ĐẠT
- Trạng thái đợt khớp kịch bản mẫu (scenarioName: "core_default_T0_overlap") — ĐẠT
- Trạng thái đợt khớp kịch bản mẫu (scenarioName: "core_default_T0_widenedP3") — ĐẠT
- Trạng thái đợt khớp kịch bản mẫu (scenarioName: "core_default_T1") — ĐẠT
- Trạng thái đợt khớp kịch bản mẫu (scenarioName: "core_default_T2") — ĐẠT
- Đúng ngày đầu của đợt thì đang diễn ra — ĐẠT
- Đúng ngày cuối của đợt thì vẫn đang diễn ra — ĐẠT
- Sau ngày kết thúc một ngày thì đợt đã qua — ĐẠT
- Trước ngày bắt đầu một ngày thì còn đúng một ngày — ĐẠT
- Đợt đã kết thúc thì không còn số ngày còn lại — ĐẠT
- Đợt chưa tới thì trả trạng thái sắp tới kèm số ngày còn lại — ĐẠT
- Hôm nay nằm trong đợt thì đang diễn ra, không có số ngày còn lại — ĐẠT

### Qt1MilestoneSequenceTests

- Mốc không dương thì bị từ chối (start: -30, end: 90, step: 5) — ĐẠT
- Mốc không dương thì bị từ chối (start: 0, end: 90, step: 5) — ĐẠT
- Mốc không dương thì bị từ chối (start: 30, end: -90, step: 5) — ĐẠT
- Mốc không dương thì bị từ chối (start: 30, end: 0, step: 5) — ĐẠT
- Mốc bắt đầu bằng mốc kết thúc thì chỉ có một mốc — ĐẠT
- Mốc bắt đầu lớn hơn mốc kết thúc thì bị từ chối — ĐẠT
- Bước nhảy không rơi đúng mốc cuối thì dừng trước khi vượt — ĐẠT
- Bước nhảy nhỏ hơn 1 thì bị từ chối (step: -1) — ĐẠT
- Bước nhảy nhỏ hơn 1 thì bị từ chối (step: 0) — ĐẠT
- Bước nhảy lớn hơn cả khoảng thì chỉ còn mốc đầu — ĐẠT
- Cài đặt mặc định sinh 13 mốc từ 30 tới 90 — ĐẠT
- Bước nhảy 10 sinh đúng 7 mốc — ĐẠT

### Qt2AnniversaryTests

- Ngày thường thì giữ nguyên ngày và tháng — ĐẠT
- Vào Đảng 29/02, năm đích nhuận thì giữ 29/02 — ĐẠT
- Vào Đảng 29/02, năm đích không nhuận thì lùi về 28/02 — ĐẠT
- Mốc âm thì bị từ chối — ĐẠT
- Mốc 0 trả đúng ngày vào Đảng — ĐẠT

### Qt3PartyAgeTests

- Chưa tới ngày kỷ niệm trong năm thì không tính năm đang chạy — ĐẠT
- Vào Đảng ngày 29/02 thì năm không nhuận tính vào 28/02 — ĐẠT
- Vào Đảng 29/02, trước 28/02 một ngày thì chưa cộng năm — ĐẠT
- Người lâu năm nhất có tuổi đảng vượt mốc lớn nhất — ĐẠT
- Đúng ngày kỷ niệm thì tính thêm năm đó — ĐẠT
- Trước ngày kỷ niệm một ngày thì chưa cộng thêm năm — ĐẠT
- Ngày vào Đảng ở tương lai thì tuổi đảng bằng 0 — ĐẠT
- Vào Đảng đúng hôm nay thì tuổi đảng bằng 0 — ĐẠT

### Qt3aNextMilestoneTests

- Người chưa tới mốc đầu tiên vẫn có ngày tròn mốc đúng — ĐẠT
- Vượt mốc lớn nhất thì không còn ngày tròn mốc kế tiếp — ĐẠT
- Mốc kế tiếp của cả danh sách mẫu khớp kết quả mong đợi — ĐẠT
- Tuổi đảng đúng bằng một mốc thì mốc kế tiếp là mốc sau đó — ĐẠT
- Tuổi đảng đúng bằng mốc lớn nhất thì không còn mốc kế tiếp — ĐẠT
- Tuổi đảng vượt mốc lớn nhất thì không còn mốc kế tiếp — ĐẠT
- Chưa tới mốc đầu tiên thì mốc kế tiếp là mốc đầu tiên — ĐẠT
- Tuổi đảng sát dưới một mốc thì mốc kế tiếp chính là mốc đó — ĐẠT
- Đổi bước nhảy sang 10 thì bỏ qua đúng những mốc biến mất — ĐẠT
- Tuổi đảng của cả danh sách mẫu khớp kết quả mong đợi — ĐẠT

### Qt4EligibilityTests

- Năm không rơi vào mốc nào thì không đủ điều kiện — ĐẠT
- Người đúng mốc lớn nhất vẫn được xét mốc 90 — ĐẠT
- Số người đủ điều kiện khớp kịch bản mẫu (scenarioName: "core_default_T0") — ĐẠT
- Số người đủ điều kiện khớp kịch bản mẫu (scenarioName: "core_default_T0_fullCover") — ĐẠT
- Số người đủ điều kiện khớp kịch bản mẫu (scenarioName: "core_default_T0_leapEdgePeriod") — ĐẠT
- Số người đủ điều kiện khớp kịch bản mẫu (scenarioName: "core_default_T0_overlap") — ĐẠT
- Số người đủ điều kiện khớp kịch bản mẫu (scenarioName: "core_default_T0_widenedP3") — ĐẠT
- Số người đủ điều kiện khớp kịch bản mẫu (scenarioName: "core_step10_T0") — ĐẠT
- Tròn mốc đúng ngày đầu đợt thì đủ điều kiện — ĐẠT
- Tròn mốc đúng ngày cuối đợt thì đủ điều kiện — ĐẠT
- Tròn mốc sau đợt một ngày thì không đủ điều kiện — ĐẠT
- Tròn mốc trước đợt một ngày thì không đủ điều kiện — ĐẠT
- Ngày tròn mốc 29/02 dời về 28/02 vẫn nằm trong đợt — ĐẠT
- Người đã vượt mốc lớn nhất thì không còn đủ điều kiện — ĐẠT
- Đợt bắt đầu 29/02 ở năm không nhuận thì quy về 28/02 — ĐẠT
- Bước nhảy 10 vẫn giữ người có mốc còn tồn tại — ĐẠT
- Bước nhảy 10 thì người mất mốc không còn đủ điều kiện — ĐẠT

### Qt6PeriodWarningTests

- Ngày/tháng không tồn tại thì bị từ chối (day: 0, month: 1) — ĐẠT
- Ngày/tháng không tồn tại thì bị từ chối (day: 1, month: 13) — ĐẠT
- Ngày/tháng không tồn tại thì bị từ chối (day: 30, month: 2) — ĐẠT
- Ngày/tháng không tồn tại thì bị từ chối (day: 31, month: 2) — ĐẠT
- Ngày/tháng không tồn tại thì bị từ chối (day: 31, month: 4) — ĐẠT
- Ngày/tháng không tồn tại thì bị từ chối (day: 32, month: 1) — ĐẠT
- Từ ngày sau Đến ngày thì bị từ chối — ĐẠT
- Đợt bắt đầu 29/02 vẫn được chấp nhận — ĐẠT
- Khoảng trống đầu và cuối được gán nhãn theo vị trí — ĐẠT
- Bộ đợt chính của 2026 sinh đúng năm khoảng trống mong đợi — ĐẠT
- Năm nhuận thì khoảng trống kết thúc đúng ngày của tháng Hai — ĐẠT
- Các đợt phủ kín năm thì không còn khoảng trống — ĐẠT
- Chưa cài đợt nào thì cả năm mang nhãn "Trước đợt đầu tiên" — ĐẠT
- Chưa cài đợt nào thì cả năm là một khoảng trống — ĐẠT
- Bộ đợt chính không có chồng lấn — ĐẠT
- Hai đợt sát nhau không sinh cảnh báo chồng lấn — ĐẠT
- Hai đợt giao nhau được báo thành một cặp đúng thứ tự — ĐẠT
- Hai đợt trùng đúng một ngày vẫn bị báo chồng lấn — ĐẠT

### Qt7MissedMilestoneTests

- Toàn bộ dữ liệu mẫu khớp bảng kết quả mong đợi (year: 2025) — ĐẠT
- Toàn bộ dữ liệu mẫu khớp bảng kết quả mong đợi (year: 2026) — ĐẠT
- Toàn bộ dữ liệu mẫu khớp bảng kết quả mong đợi (year: 2027) — ĐẠT
- Toàn bộ dữ liệu mẫu khớp bảng kết quả mong đợi (year: 2028) — ĐẠT
- Năm không có mốc nào rơi vào thì không ai bị lỡ — ĐẠT
- Ngày tròn mốc sau đợt cuối thì mang nhãn "Sau đợt cuối cùng" — ĐẠT
- Ngày tròn mốc trước đợt đầu thì mang nhãn "Trước đợt đầu tiên" — ĐẠT
- Ngày tròn mốc nằm giữa hai đợt thì nêu tên cả hai đợt — ĐẠT
- Ngày tròn mốc nằm trong một đợt thì không bị lỡ — ĐẠT
- Chưa có đợt nào thì khoảng trống là cả năm — ĐẠT
- Bước nhảy 10 thì người mất mốc không còn trong danh sách lỡ — ĐẠT

### Qt8UpcomingPeriodTests

- Ở mốc T0 trả đúng đợt kế tiếp trong năm kèm số ngày còn lại — ĐẠT
- Đợt bắt đầu 29/02 thì năm sau quy về 28/02 — ĐẠT
- Đợt sắp tới khớp kịch bản mẫu (scenarioName: "core_default_T0") — ĐẠT
- Đợt sắp tới khớp kịch bản mẫu (scenarioName: "core_default_T0_fullCover") — ĐẠT
- Đợt sắp tới khớp kịch bản mẫu (scenarioName: "core_default_T0_leapEdgePeriod") — ĐẠT
- Đợt sắp tới khớp kịch bản mẫu (scenarioName: "core_default_T0_noPeriod") — ĐẠT
- Đợt sắp tới khớp kịch bản mẫu (scenarioName: "core_default_T0_overlap") — ĐẠT
- Đợt sắp tới khớp kịch bản mẫu (scenarioName: "core_default_T0_widenedP3") — ĐẠT
- Đợt sắp tới khớp kịch bản mẫu (scenarioName: "core_default_T1") — ĐẠT
- Đợt sắp tới khớp kịch bản mẫu (scenarioName: "core_default_T2") — ĐẠT
- Ngày cuối của đợt vẫn trả chính đợt đó, đang diễn ra — ĐẠT
- Sau đợt cuối một ngày thì chuyển sang năm sau — ĐẠT
- Mọi đợt trong năm đã qua thì lấy đợt sớm nhất năm sau — ĐẠT
- Sang năm sau vẫn giữ nguyên quy tắc phá hòa — ĐẠT
- Chưa cài đợt nào thì không có đợt sắp tới — ĐẠT
- Hôm nay nằm trong một đợt thì trả chính đợt đó — ĐẠT
- Hai đợt chồng lấn thì chọn đợt bắt đầu sớm hơn — ĐẠT
- Cùng ngày bắt đầu thì kết quả không phụ thuộc thứ tự đầu vào — ĐẠT
- Cùng ngày bắt đầu thì ưu tiên đợt kết thúc sớm hơn, rồi tới tên — ĐẠT

## HuyHieuDang.Core.QcTests — bộ kiểm chứng logic của QC

### Qc01Qt1MilestoneTests

- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt (start: 1, end: 1, step: 1) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt (start: 1, end: 100, step: 1) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt (start: 30, end: 30, step: 5) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt (start: 30, end: 90, step: 10) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt (start: 30, end: 90, step: 5) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt (start: 30, end: 90, step: 61) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt (start: 30, end: 92, step: 5) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt (start: 30, end: 95, step: 5) — ĐẠT
- QC · Dãy mốc trùng khớp bản hiện thực độc lập của QC trên lưới cài đặt (start: 5, end: 100, step: 7) — ĐẠT
- QC · Gọi hai lần cùng cài đặt cho hai danh sách bằng nhau và tách rời nhau — ĐẠT
- QC-04 · Bước = int.MaxValue chỉ được cho ra mốc đầu, không được tràn số — ĐẠT
- U-101 · Dãy mốc luôn tăng dần và không trùng — ĐẠT
- U-101/U-102 · Dãy mốc khớp expected.json của QC (scenario: "core_default_T0") — ĐẠT
- U-101/U-102 · Dãy mốc khớp expected.json của QC (scenario: "core_step10_T0") — ĐẠT
- U-104 · Bước lớn hơn cả khoảng 30–90 thì chỉ còn mốc đầu — ĐẠT
- U-106 · Mốc cuối đúng bằng Kết thúc thì phải có trong dãy — ĐẠT
- U-107/U-108/U-109 · Cài đặt sai bị từ chối bằng lỗi nghiệp vụ, không trả dãy rỗng — ĐẠT
- U-110 · 1 / 100 / 1 cho đúng 100 mốc, không treo — ĐẠT

### Qc02Qt2AnniversaryTests

- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc (month: 1) — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc (month: 10) — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc (month: 12) — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc (month: 3) — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc (month: 5) — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc (month: 7) — ĐẠT
- QC · Ngày cuối của mọi tháng 31 ngày giữ nguyên ngày khi cộng mốc (month: 8) — ĐẠT
- QC · Đối chiếu mọi ngày của 4 năm (nhuận và không nhuận) với oracle của Q) — ĐẠT
- U-204 · 29/02/1996 + 4 năm → 2000 chia hết 400 nên vẫn nhuận — ĐẠT
- U-205 · 29/02/1896 + 4 năm → 1900 chia hết 100 nhưng KHÔNG nhuận → 28/02 — ĐẠT
- U-206 · 28/02/1996 + 32 năm → 28/02/2028, không được nhảy sang 29/02 — ĐẠT
- U-207 · 31/01/1996 + 30 năm → 31/01/2026, ngày cuối tháng không bị đụng — ĐẠT
- U-208 · Mốc 0 trả đúng ngày vào Đảng — ĐẠT

### Qc03Qt3PartyAgeTests

- QC · Mốc kế tiếp khớp oracle của QC trên cả bộ lõi ở T0/T1/T2, hai cài đặt — ĐẠT
- QC · Tuổi đảng chỉ tăng theo thời gian, không bao giờ tụt (quét bộ lõi 2024–2030) — ĐẠT
- QC · Tuổi đảng khớp oracle của QC trên mọi ngày 2024–2030 của cả bộ lõi — ĐẠT
- U-301/U-302/U-303 · Tuổi đảng quanh đúng ngày kỷ niệm tại T0 (year: 1996, month: 9, day: 18, expected: 30) — ĐẠT
- U-301/U-302/U-303 · Tuổi đảng quanh đúng ngày kỷ niệm tại T0 (year: 1996, month: 9, day: 19, expected: 30) — ĐẠT
- U-301/U-302/U-303 · Tuổi đảng quanh đúng ngày kỷ niệm tại T0 (year: 1996, month: 9, day: 20, expected: 29) — ĐẠT
- U-304/U-305/U-306 · Các mốc tuổi đảng đặc biệt của bộ lõi tại T0 (code: "M01", expected: 91) — ĐẠT
- U-304/U-305/U-306 · Các mốc tuổi đảng đặc biệt của bộ lõi tại T0 (code: "M02", expected: 90) — ĐẠT
- U-304/U-305/U-306 · Các mốc tuổi đảng đặc biệt của bộ lõi tại T0 (code: "V01", expected: 0) — ĐẠT
- U-307/U-308/U-309 · Người vào Đảng 29/02 xét ở năm không nhuận (y: 2026, m: 2, d: 27, expected: 29) — ĐẠT
- U-307/U-308/U-309 · Người vào Đảng 29/02 xét ở năm không nhuận (y: 2026, m: 2, d: 28, expected: 30) — ĐẠT
- U-307/U-308/U-309 · Người vào Đảng 29/02 xét ở năm không nhuận (y: 2026, m: 3, d: 1, expected: 30) — ĐẠT
- U-353/U-354/U-357 · Người đã vượt mốc lớn nhất hết mốc kế tiếp, kể cả khi đổi Bước (code: "M01", step: 10) — ĐẠT
- U-353/U-354/U-357 · Người đã vượt mốc lớn nhất hết mốc kế tiếp, kể cả khi đổi Bước (code: "M01", step: 5) — ĐẠT
- U-353/U-354/U-357 · Người đã vượt mốc lớn nhất hết mốc kế tiếp, kể cả khi đổi Bước (code: "M02", step: 10) — ĐẠT
- U-353/U-354/U-357 · Người đã vượt mốc lớn nhất hết mốc kế tiếp, kể cả khi đổi Bước (code: "M02", step: 5) — ĐẠT
- U-355 · L02 (29/02/1988) có mốc kế tiếp 40 rơi đúng 29/02/202) — ĐẠT
- U-356 · Mốc kế tiếp phải LỚN HƠN tuổi đảng, không được bằng — ĐẠT

### Qc04Qt4EligibilityTests

- QC · Danh sách đủ điều kiện từng đợt/từng năm khớp expected.json của QC (scenario: "core_default_T0") — ĐẠT
- QC · Danh sách đủ điều kiện từng đợt/từng năm khớp expected.json của QC (scenario: "core_step10_T0") — ĐẠT
- QC · Đủ điều kiện khớp oracle của QC trên mọi đợt × mọi năm 2020–2035 — ĐẠT
- U-408/U-409 · L02 chỉ đủ điều kiện ở năm nhuận 2028 với mốc 40 — ĐẠT
- U-411/U-412 · N01 chỉ đủ điều kiện Đợt 7/11 của năm 2027, không phải 2026 — ĐẠT
- U-413/U-414 · Đợt có Từ ngày 29/02: thu về 28/02 ở 2026, giữ 29/02 ở 2028 — ĐẠT
- U-415 · Đợt một ngày (Từ = Đến = 01/10) chỉ nhận đúng người tròn mốc hôm đ) — ĐẠT
- U-416 · Phân bổ mốc của Đợt 7/11 năm 2026: 30×3, 35×1, 40×1, 45×1 — ĐẠT
- U-416/U-417 · Số người đủ điều kiện Đợt 7/11 năm 2026 theo Bước 5 và Bước 10 (step: 10, expectedCount: 4) — ĐẠT
- U-416/U-417 · Số người đủ điều kiện Đợt 7/11 năm 2026 theo Bước 5 và Bước 10 (step: 5, expectedCount: 6) — ĐẠT
- U-418 · Không ai đủ điều kiện ở hai đợt cùng một năm — ĐẠT

### Qc05Qt5NoStoredResultTests

- U-501 · Gọi hai lần với cùng dữ liệu cho cùng kết quả, không tác dụng phụ — ĐẠT
- U-501 · Service không sửa danh sách đợt và danh sách mốc được truyền vào — ĐẠT
- U-502 · Nới Đến ngày của đợt thì người đang bị sót chuyển sang đủ điều kiện ngay — ĐẠT
- U-502 · Đổi cài đặt giữa hai lần gọi thì lần thứ hai đổi theo ngay — ĐẠT
- U-503 · Không có bảng/entity nào lưu danh sách đủ điều kiện — ĐẠT

### Qc06Qt6PeriodTests

- QC · Các khoảng trống không chồng nhau, không thủng, phủ đúng phần còn lại của năm — ĐẠT
- QC · Khoảng trống khớp oracle độc lập của QC trên mọi bộ đợt, 2024–2030 — ĐẠT
- U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày (fromDay: 1, fromMonth: 1, toDay: 31, toMonth: 12, year: 2026, from: "2026-01-01", to: "2026-12-31") — ĐẠT
- U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày (fromDay: 1, fromMonth: 10, toDay: 1, toMonth: 10, year: 2026, from: "2026-10-01", to: "2026-10-01") — ĐẠT
- U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày (fromDay: 1, fromMonth: 10, toDay: 7, toMonth: 11, year: 2026, from: "2026-10-01", to: "2026-11-07") — ĐẠT
- U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày (fromDay: 29, fromMonth: 2, toDay: 5, toMonth: 3, year: 2026, from: "2026-02-28", to: "2026-03-05") — ĐẠT
- U-601/U-603/U-604/U-605 · Các đợt hợp lệ gắn năm ra đúng ngày (fromDay: 29, fromMonth: 2, toDay: 5, toMonth: 3, year: 2028, from: "2028-02-29", to: "2028-03-05") — ĐẠT
- U-602 · Đợt có Từ ngày > Đến ngày phải bị từ chối — ĐẠT
- U-606/U-607 · Ngày/tháng không tồn tại phải báo lỗi nghiệp vụ — ĐẠT
- U-609 · Hai đợt chồng lấn vẫn được tính bình thường, chỉ là cảnh báo — ĐẠT
- U-609/U-611/U-612 · Cảnh báo chồng lấn khớp oracle của QC — ĐẠT
- U-610 · Bộ 4 đợt chính để hở đúng 5 khoảng trống ở mọi năm xét (year: 2025) — ĐẠT
- U-610 · Bộ 4 đợt chính để hở đúng 5 khoảng trống ở mọi năm xét (year: 2026) — ĐẠT
- U-610 · Bộ 4 đợt chính để hở đúng 5 khoảng trống ở mọi năm xét (year: 2027) — ĐẠT
- U-610 · Bộ 4 đợt chính để hở đúng 5 khoảng trống ở mọi năm xét (year: 2028) — ĐẠT

### Qc07Qt7MissedMilestoneTests

- QC · Người bị sót và người đủ điều kiện là hai tập rời nhau — ĐẠT
- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch (scenario: "core_default_T0", periodSet: "main") — ĐẠT
- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch (scenario: "core_default_T0_fullCover", periodSet: "fullCover") — ĐẠT
- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch (scenario: "core_default_T0_leapEdgePeriod", periodSet: "leapEdge") — ĐẠT
- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch (scenario: "core_default_T0_noPeriod", periodSet: "none") — ĐẠT
- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch (scenario: "core_default_T0_overlap", periodSet: "overlap") — ĐẠT
- QC · Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch (scenario: "core_default_T0_widenedP3", periodSet: "widened") — ĐẠT
- QC · Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp expected.json(scenario: "core_default_T0", periodSet: "main") — ĐẠT
- QC · Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp expected.json(scenario: "core_default_T0_leapEdgePeriod", periodSet: "leapEdge") — ĐẠT
- QC · Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp expected.json(scenario: "core_default_T0_overlap", periodSet: "overlap") — ĐẠT
- QC · Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp expected.json(scenario: "core_default_T0_widenedP3", periodSet: "widened") — ĐẠT
- QC-02 · Nhãn khoảng trống khi chưa cài đợt nào phải khớp expected.json — ĐẠT
- U-710 · Đổi Bước sang 10 thì S04 rời danh sách bị sót, còn 6 người — ĐẠT
- U-711 · Bộ đợt phủ kín cả năm thì không ai bị sót — ĐẠT
- U-712 · Không cài đợt nào thì 27 người của bộ lõi đều bị sót ở 2026 — ĐẠT

### Qc08Qt8UpcomingPeriodTests

- QC · Đợt sắp tới khớp oracle của QC ở mọi ngày của 2026 và 2028 — ĐẠT
- QC · Đợt sắp tới luôn có Đến ngày không nhỏ hơn hôm nay — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch (today: "2026-01-01", name: "Đợt 3/2", year: 2026) — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch (today: "2026-09-19", name: "Đợt 7/11", year: 2026) — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch (today: "2026-10-15", name: "Đợt 7/11", year: 2026) — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch (today: "2026-11-07", name: "Đợt 7/11", year: 2026) — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch (today: "2026-11-08", name: "Đợt 3/2", year: 2027) — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch (today: "2026-12-01", name: "Đợt 3/2", year: 2027) — ĐẠT
- U-801/U-802/U-803/U-804/U-805 · Đợt sắp tới tại các mốc thời gian của kế hoạch (today: "2026-12-31", name: "Đợt 3/2", year: 2027) — ĐẠT
- U-807 · Chưa cài đợt nào thì không có đợt sắp tới — ĐẠT
- U-808 · Hai đợt cùng Từ ngày phải cho kết quả tất định — ĐẠT
- U-809 · Đợt 29/02 sang năm không nhuận: thu về 28/02 và đếm ngày theo ngày đã thu — ĐẠT

### Qc09Qt11PeriodStatusTests

- QC · Chỉ trạng thái Sắp tới mới có số ngày còn lại, và luôn dương — ĐẠT
- QC · Trạng thái 4 đợt chính khớp expected.json ở T0, T1, T2 (scenario: "core_default_T0") — ĐẠT
- QC · Trạng thái 4 đợt chính khớp expected.json ở T0, T1, T2 (scenario: "core_default_T1") — ĐẠT
- QC · Trạng thái 4 đợt chính khớp expected.json ở T0, T1, T2 (scenario: "core_default_T2") — ĐẠT
- QC · Trạng thái và số ngày còn lại khớp oracle của QC ở mọi ngày 2026–2028 — ĐẠT
- U-1102/U-1107 · Số ngày còn lại đếm đúng từng ngày trước Từ ngày của Đợt 7/11 — ĐẠT

### Qc10PerformanceTests

- QC · Quét 'chưa thuộc đợt nào' cho 10.000 đảng viên dưới 1 giây — ĐẠT
- U-1201 · Tính đủ điều kiện 1 đợt / 1 năm cho 10.000 đảng viên dưới 1 giây — ĐẠT

### Qc11ClockScanTests

- A-901 · Không nơi nào trong BE/src đọc đồng hồ theo giờ máy (T-FIX-2) — ĐẠT
- A-901 · Module nghiệp vụ trong Infrastructure không đọc đồng hồ (trừ 7 dòng tầng khung) — ĐẠT
- A-901 · Service tính mốc tuổi đảng không đọc đồng hồ, chỉ nhận today qua tham số — ĐẠT
- QC-05 · Core không được đọc đồng hồ hệ thống — BỎ QUA

## HuyHieuDang.Infrastructure.UnitTests — hạ tầng và nguồn thời gian

### AwardPeriodCoverageBuilderTests

- QT2 · Đợt 29/02 ở năm không nhuận lùi về 28/02 — ĐẠT
- QT6 · Bộ đợt mẫu cho đúng hai khoảng trống kèm tên hai đợt kề — ĐẠT
- QT6 · Bộ đợt mẫu cho đúng một cặp chồng lấn kèm khoảng ngày dùng chung — ĐẠT
- QT6 · Chưa cài đợt nào: cả năm là một khoảng trống — ĐẠT
- QT6 · Đợt nằm lọt trong đợt khác: báo chồng lấn, không cắt thêm đoạn — ĐẠT
- UC-36 · Dải độ phủ liền mạch 01/01–31/12, phần chồng lấn thuộc đợt đến trước — ĐẠT

### BusinessSchemaTests

- Cài đặt mặc định 30 / 90 / 5 và chưa có tên đơn vị — ĐẠT
- Tên đợt là duy nhất, không phân biệt hoa thường — ĐẠT
- Đợt trao huy hiệu không lưu năm — ĐẠT
- Thực thể nghiệp vụ ánh xạ đúng tên bảng gạch dưới (entityType: typeof(HuyHieuDang.Infrastructure.Modules.AppSettings.Entities.AppSetting), tableName: "app_setting") — ĐẠT
- Thực thể nghiệp vụ ánh xạ đúng tên bảng gạch dưới (entityType: typeof(HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities.AwardPeriod), tableName: "award_period") — ĐẠT
- Thực thể nghiệp vụ ánh xạ đúng tên bảng gạch dưới (entityType: typeof(HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities.PartyMember), tableName: "party_member") — ĐẠT
- Giới tính chỉ nhận Nam hoặc Nữ — ĐẠT
- Không có bảng nào lưu kết quả đủ điều kiện — ĐẠT
- Các cột ngày của đảng viên là ngày thuần — ĐẠT
- Họ tên dùng đối chiếu tiếng Việt — ĐẠT
- Ngày vào Đảng chính thức là bắt buộc — ĐẠT

### DateTimeProviderTests

- Giờ hệ thống bám theo đồng hồ UTC của máy — ĐẠT
- Giờ hệ thống luôn theo múi giờ Việt Nam — ĐẠT
- Tên biến ép ngày đúng như đã chốt — ĐẠT
- Hôm nay là ngày theo lịch Việt Nam — ĐẠT
- Ngoài Production, biến HUYHIEUDANG_TEST_TODAY ép được hôm nay — ĐẠT
- Giá trị sai định dạng bị bỏ qua, dùng ngày thật (value: "   ") — ĐẠT
- Giá trị sai định dạng bị bỏ qua, dùng ngày thật (value: "15/10/2026") — ĐẠT
- Giá trị sai định dạng bị bỏ qua, dùng ngày thật (value: "2026-10-15T00:00:00") — ĐẠT
- Giá trị sai định dạng bị bỏ qua, dùng ngày thật (value: "2026-13-40") — ĐẠT
- Giá trị sai định dạng bị bỏ qua, dùng ngày thật (value: "khong-phai-ngay") — ĐẠT
- Ở Production biến ép ngày bị bỏ qua — ĐẠT

### PartyMemberImportRowValidatorTests

- 29/02 năm không nhuận là ngày không có thật → InvalidDateFormat — ĐẠT
- 29/02 năm nhuận là ngày có thật, không bị coi là sai định dạng — ĐẠT
- Dòng đủ bốn ô hợp lệ thì không có lỗi và được chuẩn hóa — ĐẠT
- Giới tính lạ → InvalidGender — ĐẠT
- Ngày chính thức dạng yyyy-MM-dd → InvalidDateFormat — ĐẠT
- Ngày chính thức đúng bằng hôm nay là hợp lệ — ĐẠT
- Ngày chính thức ở tương lai → FutureOfficialAdmissionDate — ĐẠT
- Ngày sinh sau ngày chính thức → BirthDateAfterAdmissionDate — ĐẠT
- Ngày sinh và giới tính bỏ trống vẫn hợp lệ, trả null — ĐẠT
- OQ-10: ngày sinh bằng đúng ngày chính thức vẫn là lỗi — ĐẠT
- OQ-1: ngày sinh 31/02/1974 không có thật → InvalidDateFormat — ĐẠT
- OQ-2: một dòng nhiều lỗi trả đủ mọi lý do, đúng thứ tự bảng mã lỗi — ĐẠT
- OQ-5: giới tính không phân biệt hoa thường (cell: "NAM", expected: "Male") — ĐẠT
- OQ-5: giới tính không phân biệt hoa thường (cell: "NỮ", expected: "Female") — ĐẠT
- OQ-5: giới tính không phân biệt hoa thường (cell: "Nữ", expected: "Female") — ĐẠT
- OQ-5: giới tính không phân biệt hoa thường (cell: "nam", expected: "Male") — ĐẠT
- OQ-5: giới tính không phân biệt hoa thường (cell: "nữ", expected: "Female") — ĐẠT
- OQ-6: ngày nhận cả một chữ số lẫn hai chữ số (birth: "09/02/1975", admission: "01/10/1996") — ĐẠT
- OQ-6: ngày nhận cả một chữ số lẫn hai chữ số (birth: "9/02/1975", admission: "01/10/1996") — ĐẠT
- OQ-6: ngày nhận cả một chữ số lẫn hai chữ số (birth: "9/2/1975", admission: "1/10/1996") — ĐẠT
- Thiếu họ tên → MissingFullName — ĐẠT
- Thiếu ngày chính thức → MissingOfficialAdmissionDate, không kèm lỗi định dạng — ĐẠT

### SkeletonTests

- Tài khoản người dùng dựng được thông tin cho JWT — ĐẠT
- Tài khoản người dùng kiểm tra được mật khẩu — ĐẠT

### TestTodayVariableTests

- Ở Production, biến bị bỏ qua và vẫn ghi log cảnh báo — ĐẠT
- Ngoài Production, biến ép được hôm nay và ghi log cảnh báo — ĐẠT
- Không đặt biến thì không ép ngày và không có cảnh báo nào — ĐẠT

## HuyHieuDang.Infrastructure.IntegrationTests

### SkeletonIntegrationTests

- Cấu hình cơ sở dữ liệu mặc định để trống chuỗi kết nối — ĐẠT

## HuyHieuDang.Web.IntegrationTests — API thật trên PostgreSQL thật

### AnonymousEndpointTests

- A-007 · Chỉ POST /api/Auth/Login được [AllowAnonymous] — ĐẠT
- Mọi controller đều kế thừa BaseController nên mặc định cần đăng nhập — ĐẠT

### AuthEndpointTests

- A-001 · Gọi endpoint nghiệp vụ không kèm token trả 401 và không lộ dữ liệu (method: "GET", path: "/api/Auth/Me") — ĐẠT
- A-001 · Gọi endpoint nghiệp vụ không kèm token trả 401 và không lộ dữ liệu (method: "POST", path: "/api/Auth/Logout") — ĐẠT
- A-002 · JWT sai chữ ký trả 401 — ĐẠT
- A-003 · JWT đã hết hạn trả 401 — ĐẠT
- A-004 · JWT thiếu tiền tố Bearer trả 401 — ĐẠT
- A-005 · Sai mật khẩu và không có tài khoản trả cùng một thông báo 401 — ĐẠT
- A-006 · Token vừa cấp dùng được ngay cho endpoint khác — ĐẠT
- A-006 · Đăng nhập đúng trả 200 kèm token đúng hợp đồng — ĐẠT
- Hạn token đúng 8 giờ theo hợp đồng mục 1.2 — ĐẠT
- Thiếu tài khoản hoặc mật khẩu trả 400 kèm khóa Required (username: "", password: "Kiem@Thu123", expectedProperty: "Username") — ĐẠT
- Thiếu tài khoản hoặc mật khẩu trả 400 kèm khóa Required (username: "admin", password: "", expectedProperty: "Password") — ĐẠT
- Token hợp lệ nhưng chủ thể không còn tồn tại trả 401 — ĐẠT
- Đăng xuất có token trả 200 và khóa Mes.User.Logout.Successfully — ĐẠT

### AwardPeriodEndpointTests

- 5.1 · Bốn đợt mẫu: sắp theo fromDate, ba trạng thái và số người đủ điều kiện — ĐẠT
- 5.1 · Cảnh báo liệt kê đúng một cặp chồng lấn và một khoảng trống — ĐẠT
- 5.1 · Dải độ phủ liền mạch 01/01–31/12, phần chồng lấn thuộc đợt đến trước — ĐẠT
- 5.1 · Năm khác: trạng thái vẫn so với hôm nay, ngày gắn đúng năm được hỏi — ĐẠT
- 5.1 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year (query: "?year=1899") — ĐẠT
- 5.1 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year (query: "?year=2201") — ĐẠT
- 5.1 → 5.5 · Thiếu token trả 401 (method: "DELETE", path: "/api/AwardPeriods/9b7c0f9c-0000-0000-0000-00000000"···) — ĐẠT
- 5.1 → 5.5 · Thiếu token trả 401 (method: "GET", path: "/api/AwardPeriods") — ĐẠT
- 5.1 → 5.5 · Thiếu token trả 401 (method: "POST", path: "/api/AwardPeriods") — ĐẠT
- 5.2 · Lấy một đợt theo id và theo năm được hỏi; id lạ trả NotFound — ĐẠT
- 5.3 · Ba lỗi chặn lưu: thiếu tên, ngày không có thật, đợt vắt qua năm (name: "Đợt 31/04", fromDay: 31, fromMonth: 4, toDay: 7, toMonth: 11, expectedKey: "Mes.AwardPeriod.Invalid.FromDate") — ĐẠT
- 5.3 · Ba lỗi chặn lưu: thiếu tên, ngày không có thật, đợt vắt qua năm (name: "Đợt vắt qua năm", fromDay: 15, fromMonth: 12, toDay: 20, toMonth: 1, expectedKey: "Mes.AwardPeriod.Invalid.Range") — ĐẠT
- 5.3 · Ba lỗi chặn lưu: thiếu tên, ngày không có thật, đợt vắt qua năm (name: "Đợt đến 31/11", fromDay: 1, fromMonth: 10, toDay: 31, toMonth: 11, expectedKey: "Mes.AwardPeriod.Invalid.ToDate") — ĐẠT
- 5.3 · Ba lỗi chặn lưu: thiếu tên, ngày không có thật, đợt vắt qua năm (name: null, fromDay: 1, fromMonth: 10, toDay: 7, toMonth: 11, expectedKey: "Mes.AwardPeriod.Required.Name") — ĐẠT
- 5.3 · Thêm đợt chồng lấn vẫn là 200 và trả kèm cảnh báo — ĐẠT
- 5.3 · Trùng tên không phân biệt hoa thường trả Mes.AwardPeriod.Repeated.Name — ĐẠT
- 5.4 · Sửa sang tên đợt khác trả Repeated.Name; id lạ trả NotFound — ĐẠT
- 5.4 · Sửa đợt: giữ nguyên tên của chính nó, có hiệu lực ngay cho năm hiện tại — ĐẠT
- 5.5 · Xóa hẳn đợt, trả khoảng trống mới và không đụng đảng viên (QT10) — ĐẠT

### DashboardEndpointTests

- 6.1 · Chưa cài đợt nào: upcomingPeriod = null, bảng rỗng, cảnh báo noPeriods — ĐẠT
- 6.1 · Chưa có đảng viên nào: cảnh báo noMembers, đợt sắp tới vẫn có, 0 người — ĐẠT
- 6.1 · Cảnh báo của Dashboard trùng khớp với cảnh báo màn Đợt — ĐẠT
- 6.1 · Kho trống hoàn toàn: đủ hai cảnh báo cho khối hướng dẫn ba bước (UC-13) — ĐẠT
- 6.1 · T0 = 19/09/2026: đợt sắp tới là Đợt 7/11 · 2026, còn 12 ngày, 6 người — ĐẠT
- 6.1 · T1 = 15/10/2026: Đợt 7/11 đang diễn ra, daysRemaining = null — ĐẠT
- 6.1 · T2 = 01/12/2026: mọi đợt 2026 đã qua nên đợt sắp tới là Đợt 3/2 · 2027 (QT8) — ĐẠT
- 6.1 · Thiếu token trả 401 — ĐẠT

### EligibilityEndpointTests

- 6.2 · Bỏ trống year thì lấy năm hiện tại của máy chủ — ĐẠT
- 6.2 · Id đợt lạ trả Mes.AwardPeriod.NotFound — ĐẠT
- 6.2 · Nới Đến ngày Đợt 2/9 sang 30/09: đợt lên 5 người ngay, không lưu gì (QT5) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (periodCode: "P1", year: 2025) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (periodCode: "P1", year: 2026) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (periodCode: "P1", year: 2027) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (periodCode: "P1", year: 2028) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (periodCode: "P2", year: 2025) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (periodCode: "P2", year: 2026) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (periodCode: "P2", year: 2027) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (periodCode: "P2", year: 2028) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (periodCode: "P3", year: 2025) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (periodCode: "P3", year: 2026) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (periodCode: "P3", year: 2027) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (periodCode: "P3", year: 2028) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (periodCode: "P4", year: 2025) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (periodCode: "P4", year: 2026) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (periodCode: "P4", year: 2027) — ĐẠT
- 6.2 · Đủ điều kiện từng đợt × từng năm khớp expected.json (periodCode: "P4", year: 2028) — ĐẠT
- 6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year (path: "/api/Eligibility", year: 1899) — ĐẠT
- 6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year (path: "/api/Eligibility", year: 2201) — ĐẠT
- 6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year (path: "/api/Eligibility/Unassigned", year: -5) — ĐẠT
- 6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year (path: "/api/Eligibility/Unassigned", year: 0) — ĐẠT
- 6.2 → 6.4 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year (path: "/api/Eligibility/UnassignedCount", year: 2201) — ĐẠT
- 6.2 → 6.4 · Thiếu token trả 401 (path: "/api/Eligibility") — ĐẠT
- 6.2 → 6.4 · Thiếu token trả 401 (path: "/api/Eligibility/Unassigned") — ĐẠT
- 6.2 → 6.4 · Thiếu token trả 401 (path: "/api/Eligibility/UnassignedCount") — ĐẠT
- 6.3 · Chưa cài đợt nào: mọi người tròn mốc đều rơi vào Trước đợt đầu tiên — ĐẠT
- 6.3 · Chưa thuộc đợt nào năm 2026: 7 người, đúng nhãn khoảng trống (QT7) — ĐẠT
- 6.3 · Các năm khác của bộ lõi khớp expected.json (year: 2025) — ĐẠT
- 6.3 · Các năm khác của bộ lõi khớp expected.json (year: 2027) — ĐẠT
- 6.3 · Các năm khác của bộ lõi khớp expected.json (year: 2028) — ĐẠT
- 6.3 · Đổi Bước 5 → 10 làm danh sách còn 6 người ngay (QT1, QT5) — ĐẠT
- 6.4 · Badge luôn bằng số dòng của màn Chưa thuộc đợt nào — ĐẠT

### EligibilityPerformanceTests

- 6.1 → 6.4 · 10.000 đảng viên: mỗi endpoint tính toán dưới 1 giây — ĐẠT

### ExportEndpointTests

- 8.1 · Id đợt lạ trả Mes.AwardPeriod.NotFound — ĐẠT
- 8.1 → 8.3 · Thiếu token trả 401 (path: "/api/Exports/Dashboard") — ĐẠT
- 8.1 → 8.3 · Thiếu token trả 401 (path: "/api/Exports/Eligibility") — ĐẠT
- 8.1 → 8.3 · Thiếu token trả 401 (path: "/api/Exports/Unassigned") — ĐẠT
- 8.1, 8.3 · Bỏ trống year thì lấy năm hiện tại của máy chủ — ĐẠT
- 8.1, 8.3 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year (path: "/api/Exports/Eligibility", year: 1899) — ĐẠT
- 8.1, 8.3 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year (path: "/api/Exports/Eligibility", year: 2201) — ĐẠT
- 8.1, 8.3 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year (path: "/api/Exports/Unassigned", year: 0) — ĐẠT
- 8.1, 8.3 · Năm ngoài 1900–2200 trả Mes.Query.Invalid.Year (path: "/api/Exports/Unassigned", year: 2201) — ĐẠT
- 8.2 · Chưa cài đợt nào trả Mes.Dashboard.NotFound.UpcomingPeriod — ĐẠT
- 8.2 · Mọi đợt của năm nay đã qua: file mang đợt đầu năm sau — ĐẠT
- 8.2 · Xuất Dashboard lấy đúng đợt sắp tới theo QT8 — ĐẠT
- 8.3 · Chưa thuộc đợt nào 2026: 7 dòng, có cột Khoảng trống đúng nhãn — ĐẠT
- A-701, A-702, A-703 · Tên file đúng quy tắc rút gọn của mục 1.9 (periodCode: "P1", expectedKey: "Đợt 3/2 năm 2026") — ĐẠT
- A-701, A-702, A-703 · Tên file đúng quy tắc rút gọn của mục 1.9 (periodCode: "P2", expectedKey: "Đợt 19/5 năm 2026") — ĐẠT
- A-701, A-702, A-703 · Tên file đúng quy tắc rút gọn của mục 1.9 (periodCode: "P3", expectedKey: "Đợt 2/9 năm 2026") — ĐẠT
- A-701, A-702, A-703 · Tên file đúng quy tắc rút gọn của mục 1.9 (periodCode: "P4", expectedKey: "Đợt 7/11 năm 2026") — ĐẠT
- A-703 · Tên file chưa thuộc đợt nào là ChuaThuocDot_<năm>.xlsx — ĐẠT
- A-704, A-706, A-708, A-710 · Đợt 7/11 · 2026: tiêu đề, cột và 6 dòng dữ liệu — ĐẠT
- A-705 · Tên đơn vị trống: dòng 1 là tên đợt, không có dòng trắng thừa — ĐẠT
- A-707 · E01 thiếu Ngày sinh và Giới tính: hai ô rỗng, không phải dấu gạch ngang — ĐẠT
- A-709 · Danh sách rỗng vẫn trả file hợp lệ chỉ có phần tiêu đề — ĐẠT
- A-709 · Không ai bị sót: file chưa thuộc đợt nào vẫn hợp lệ — ĐẠT

### ImportEndpointTests

- 4.1 · File mẫu hệ thống sinh khớp fixture mau-dang-vien.xlsx của QC — ĐẠT
- 4.1 · File mẫu trả file nhị phân đúng tên, 4 cột đúng thứ tự, 2 dòng ví dụ dd/MM/yyyy — ĐẠT
- 4.2 · Bộ lõi 32 dòng hợp lệ, đọc được cả ngày dạng chuỗi lẫn ngày kiểu ngày (fileName: "core-hop-le-ngay-kieu-date.xlsx") — ĐẠT
- 4.2 · Bộ lõi 32 dòng hợp lệ, đọc được cả ngày dạng chuỗi lẫn ngày kiểu ngày (fileName: "core-hop-le.xlsx") — ĐẠT
- 4.2 · File quá 10 MB bị chặn trước khi mở, trả Mes.Import.Invalid.FileSize — ĐẠT
- 4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa (fileName: "loi-chi-co-tieu-de.xlsx", expectedProperty: "NoDataRows") — ĐẠT
- 4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa (fileName: "loi-dinh-dang-csv.csv", expectedProperty: "Extension") — ĐẠT
- 4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa (fileName: "loi-khong-phai-xlsx.xlsx", expectedProperty: "Extension") — ĐẠT
- 4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa (fileName: "loi-rong.xlsx", expectedProperty: "Empty") — ĐẠT
- 4.2 · Lỗi cấp file bị chặn ngay ở bước xem trước, trả 400 kèm đúng khóa (fileName: "loi-sai-cot.xlsx", expectedProperty: "Columns") — ĐẠT
- 4.2 · Xem trước không ghi gì vào cơ sở dữ liệu — ĐẠT
- 4.2 · bien-chuan-hoa.xlsx: cắt khoảng trắng (OQ-4), giới tính hoa thường (OQ-5), ngày một chữ số (OQ-6) — ĐẠT
- 4.2 · loi-4-dong.xlsx: 10 dòng, 6 hợp lệ, 4 lỗi ở dòng 8, 9, 10, 11 — ĐẠT
- 4.2 · loi-moi-loai-mot-dong.xlsx: 8 dòng lỗi, mỗi loại một dòng, dòng nhiều lỗi trả đủ lý do — ĐẠT
- 4.3 · Lỗi cấp file cũng chặn ở bước nạp (fileName: "loi-khong-phai-xlsx.xlsx", expectedProperty: "Extension") — ĐẠT
- 4.3 · Lỗi cấp file cũng chặn ở bước nạp (fileName: "loi-sai-cot.xlsx", expectedProperty: "Columns") — ĐẠT
- 4.3 · Nạp file toàn lỗi: không thêm ai, không ném lỗi — ĐẠT
- 4.3 · Nạp loi-4-dong.xlsx: thêm 6 người, bỏ qua 4 dòng lỗi, không kiểm tra trùng — ĐẠT
- Ba endpoint đều yêu cầu token — ĐẠT

### ImportTransactionTests

- 4.3 · Cùng đường đi đó nhưng không hỏng: bulk-1200.xlsx thêm đủ 1200 người — ĐẠT
- 4.3 · Hỏng đúng lúc chốt giao dịch: 32 dòng của core-hop-le.xlsx bị thu hồi hết — ĐẠT
- 4.3 · Hỏng ở lô thứ hai của bulk-1200.xlsx: 200 dòng đầu đã ghi vẫn bị thu hồi hết — ĐẠT

### PartyMemberEndpointTests

- 3.1 · Danh sách trả gender chuỗi và ba giá trị tuổi đảng tính theo hôm nay — ĐẠT
- 3.1 · Lọc giới tính Nam / Nữ; bỏ tham số thì lấy tất cả (filter: "", expectedCount: 3, expectedName: null) — ĐẠT
- 3.1 · Lọc giới tính Nam / Nữ; bỏ tham số thì lấy tất cả (filter: "&filter.Gender=$eq:Female", expectedCount: 1, expectedName: "Bùi Thị Lan") — ĐẠT
- 3.1 · Lọc giới tính Nam / Nữ; bỏ tham số thì lấy tất cả (filter: "&filter.Gender=$eq:Male", expectedCount: 1, expectedName: "Cao Văn Phúc") — ĐẠT
- 3.1 · Phân trang: mặc định 20 dòng, chọn được số dòng và số trang — ĐẠT
- 3.1 · Sắp xếp theo cột; cột tính ra bị bỏ qua và quay về mặc định — ĐẠT
- 3.1 · Tìm theo họ tên chứa chuỗi, không phân biệt hoa thường — ĐẠT
- 3.1 · pageSize hoặc current không dương được kẹp về mặc định, vẫn trả 200 (query: "?current=0") — ĐẠT
- 3.1 · pageSize hoặc current không dương được kẹp về mặc định, vẫn trả 200 (query: "?pageSize=0") — ĐẠT
- 3.2 · Lấy một trả đúng bản ghi; id lạ trả 400 Mes.PartyMember.NotFound — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400 (fullName: "   ", dateOfBirth: null, gender: null, officialAdmissionDate: "1996-10-15", messagesType: "Required", property: "FullName") — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400 (fullName: "Nguyễn Văn An", dateOfBirth: "1996-10-16", gender: null, officialAdmissionDate: "1996-10-15", messagesType: "Invalid", property: "DateOfBirth") — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400 (fullName: "Nguyễn Văn An", dateOfBirth: null, gender: "Khac", officialAdmissionDate: "1996-10-15", messagesType: "Invalid", property: "Gender") — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400 (fullName: "Nguyễn Văn An", dateOfBirth: null, gender: null, officialAdmissionDate: "2026-09-20", messagesType: "Invalid", property: "OfficialAdmissionDate") — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400 (fullName: "Nguyễn Văn An", dateOfBirth: null, gender: null, officialAdmissionDate: null, messagesType: "Required", property: "OfficialAdmissionDate") — ĐẠT
- 3.3 · Bảng ràng buộc trả đúng khóa lỗi 400 (fullName: null, dateOfBirth: null, gender: null, officialAdmissionDate: "1996-10-15", messagesType: "Required", property: "FullName") — ĐẠT
- 3.3 · QT9 · Thêm hai người trùng hệt nhau vẫn thành hai bản ghi — ĐẠT
- 3.3 · Thêm mới trả 200 kèm bản ghi vừa tạo và khóa Create.Successfully — ĐẠT
- 3.3 · Đúng ngày hôm nay là ngày chính thức hợp lệ — ĐẠT
- 3.4 · Sửa ghi đè cả bốn trường, kể cả xóa bằng null — ĐẠT
- 3.4 · Sửa id không tồn tại trả 400 Mes.PartyMember.NotFound — ĐẠT
- 3.5 · Xóa id không tồn tại trả 400 Mes.PartyMember.NotFound — ĐẠT
- 3.5 · Xóa một trả id vừa xóa và bản ghi biến mất hẳn — ĐẠT
- 3.6 · Xóa nhiều trả đúng id đã xóa, id lạ bị bỏ qua lặng lẽ — ĐẠT
- 3.6 · Xóa nhiều với danh sách rỗng trả 400 — ĐẠT
- A-001 · Mọi endpoint đảng viên không kèm token trả 401 (method: "GET", path: "/api/PartyMembers") — ĐẠT
- A-001 · Mọi endpoint đảng viên không kèm token trả 401 (method: "POST", path: "/api/PartyMembers") — ĐẠT
- A-001 · Mọi endpoint đảng viên không kèm token trả 401 (method: "POST", path: "/api/PartyMembers/DeleteMany") — ĐẠT

### QueryParameterGuardTests

- QC-T27-02 · Số trang nhỏ hơn 1 được coi như trang 1 — ĐẠT
- QC-T27-02 · Số trang rất lớn trả 200 với danh sách rỗng, không 500 — ĐẠT
- QC-T27-03 · pageSize nhỏ hơn 1 lấy mặc định 20 — ĐẠT
- QC-T27-03 · pageSize vượt trần bị kẹp về 200, không báo lỗi — ĐẠT
- QC-T27-04 · Khóa của FluentValidation vẫn đi thẳng ra ngoài — ĐẠT
- QC-T27-04 · Lỗi ép kiểu tham số trả khóa chung, không trả câu tiếng Anh (url: "/api/AwardPeriods?year=2147483648") — ĐẠT
- QC-T27-04 · Lỗi ép kiểu tham số trả khóa chung, không trả câu tiếng Anh (url: "/api/AwardPeriods?year=khong-phai-so") — ĐẠT
- QC-T27-04 · Lỗi ép kiểu tham số trả khóa chung, không trả câu tiếng Anh (url: "/api/Eligibility?awardPeriodId=khong-phai-guid") — ĐẠT
- QC-T27-04 · Lỗi ép kiểu tham số trả khóa chung, không trả câu tiếng Anh (url: "/api/PartyMembers?current=khong-phai-so") — ĐẠT
- QC-T27-06 · Khoảng hợp lệ vẫn xem trước được bình thường — ĐẠT
- QC-T27-06 · Xem trước dãy mốc từ chối khoảng vượt trần, đúng khóa của PUT — ĐẠT
- QC-T27-06 · Xem trước và lưu dùng đúng một bộ luật — ĐẠT

### SettingsEndpointTests

- 7.1 · A-501 · Kho chưa có bản ghi cài đặt nào thì đọc ra mặc định 30/90/5 — ĐẠT
- 7.1 · Kho đã seed: đọc ra 30/90/5, 13 mốc và tên đơn vị đang lưu — ĐẠT
- 7.1 → 7.4 · Chưa đăng nhập thì cả bốn endpoint đều trả 401 — ĐẠT
- 7.2 · A-502 · Đổi Bước 5 → 10: danh sách đủ điều kiện và badge đổi ngay (QT5) — ĐẠT
- 7.2 · A-503 · Bắt đầu > Kết thúc trả 400 Mes.AppSetting.Invalid.Range — ĐẠT
- 7.2 · A-504 · Bước 0 hoặc âm trả 400 Mes.AppSetting.Invalid.StepYears (stepYears: -5) — ĐẠT
- 7.2 · A-504 · Bước 0 hoặc âm trả 400 Mes.AppSetting.Invalid.StepYears (stepYears: 0) — ĐẠT
- 7.2 · A-504 · Mốc bắt đầu / kết thúc ≤ 0 trả đúng khóa lỗi của từng trường (startYears: -1, endYears: 90, property: "StartYears") — ĐẠT
- 7.2 · A-504 · Mốc bắt đầu / kết thúc ≤ 0 trả đúng khóa lỗi của từng trường (startYears: 0, endYears: 90, property: "StartYears") — ĐẠT
- 7.2 · A-504 · Mốc bắt đầu / kết thúc ≤ 0 trả đúng khóa lỗi của từng trường (startYears: 30, endYears: -30, property: "EndYears") — ĐẠT
- 7.2 · A-504 · Mốc bắt đầu / kết thúc ≤ 0 trả đúng khóa lỗi của từng trường (startYears: 30, endYears: 0, property: "EndYears") — ĐẠT
- 7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400 (body: "{\"endYears\":90,\"stepYears\":5}") — ĐẠT
- 7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400 (body: "{\"startYears\":30.5,\"endYears\":90,\"stepYears\""···) — ĐẠT
- 7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400 (body: "{\"startYears\":\"\",\"endYears\":90,\"stepYears\""···) — ĐẠT
- 7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400 (body: "{\"startYears\":\"ba mươi\",\"endYears\":90,\"step"···) — ĐẠT
- 7.2 · A-505 · Giá trị rỗng hoặc không phải số đều bị từ chối 400 (body: "{\"startYears\":null,\"endYears\":90,\"stepYears\""···) — ĐẠT
- 7.2 · A-507 · Lưu Tên đơn vị: đọc lại thấy ngay, Dashboard cũng thấy — ĐẠT
- 7.2 · A-508 · Tên đơn vị bỏ trống hoặc toàn khoảng trắng đều thành chưa đặt (unitName: "   ") — ĐẠT
- 7.2 · A-508 · Tên đơn vị bỏ trống hoặc toàn khoảng trắng đều thành chưa đặt (unitName: "") — ĐẠT
- 7.2 · A-508 · Tên đơn vị bỏ trống hoặc toàn khoảng trắng đều thành chưa đặt (unitName: null) — ĐẠT
- 7.2 · A-509 · Kho trống: lưu lần đầu tạo đúng một bản ghi, không nhân bản — ĐẠT
- 7.2 · A-509 · Lưu hai lần liên tiếp vẫn chỉ có đúng một bản ghi cài đặt — ĐẠT
- 7.2 · Tên đơn vị quá 200 ký tự trả 400 Mes.AppSetting.OverLength.UnitName — ĐẠT
- 7.3 · A-506 · Khôi phục mặc định đưa về 30/90/5 và giữ nguyên Tên đơn vị — ĐẠT
- 7.4 · Bỏ trống tham số nào thì lấy giá trị đang lưu của tham số đó — ĐẠT
- 7.4 · Tham số xem trước sai trả cùng bộ khóa lỗi với 7.2 — ĐẠT
- 7.4 · Xem trước dãy mốc không ghi gì xuống cơ sở dữ liệu — ĐẠT

### UpdatedAtStampTests

- Cài đặt seed sẵn mang dấu thời gian của IDateTimeProvider, không phải giờ máy — ĐẠT
- Sửa đảng viên ghi đè dấu thời gian cũ — ĐẠT
- Thêm mới đảng viên được đóng dấu dù không ai gán UpdatedAt — ĐẠT

## HuyHieuDang.Web.QcIntegrationTests — bộ kiểm thử tích hợp của QC

### A0AuthorizationTests

- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "DELETE", url: "/api/AwardPeriods/00000000-0000-0000-0000-00000000"···) — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "DELETE", url: "/api/PartyMembers/00000000-0000-0000-0000-00000000"···) — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Auth/Me") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/AwardPeriods") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/AwardPeriods/00000000-0000-0000-0000-00000000"···) — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Dashboard") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Eligibility/Unassigned") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Eligibility/UnassignedCount") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Eligibility?awardPeriodId=00000000-0000-0000-"···) — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Exports/Dashboard") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Exports/Eligibility?awardPeriodId=00000000-00"···) — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Exports/Unassigned") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/PartyMembers") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/PartyMembers/00000000-0000-0000-0000-00000000"···) — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/PartyMembers/Import/Template") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Settings") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "GET", url: "/api/Settings/Milestones") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "POST", url: "/api/Auth/Logout") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "POST", url: "/api/AwardPeriods") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "POST", url: "/api/PartyMembers") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "POST", url: "/api/PartyMembers/DeleteMany") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "POST", url: "/api/PartyMembers/Import/Commit") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "POST", url: "/api/PartyMembers/Import/Preview") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "POST", url: "/api/Settings/RestoreDefaults") — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "PUT", url: "/api/AwardPeriods/00000000-0000-0000-0000-00000000"···) — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "PUT", url: "/api/PartyMembers/00000000-0000-0000-0000-00000000"···) — ĐẠT
- A001 · Goi khi chua dang nhap tra 401 va khong lo du lieu (method: "PUT", url: "/api/Settings") — ĐẠT
- A002 · Jwt sai chu ky tra 401 — ĐẠT
- A003 · Jwt het han tra 401 — ĐẠT
- A004 · Thieu tien to Bearer tra 401 — ĐẠT
- A005 · Dang nhap sai tra 401 va thong diep chung — ĐẠT
- A006 · Dang nhap dung tra token dung duoc ngay — ĐẠT
- A007 · Chi Login duoc phep AllowAnonymous — ĐẠT

### A1PartyMemberTests

- A101 · Tong so dang vien dung 1232 — ĐẠT
- A102 · Trang dau 20 dong va 62 trang — ĐẠT
- A103 · Trang cuoi con 12 dong — ĐẠT
- A104 · Trang vuot qua trang cuoi tra rong khong loi — ĐẠT
- A105 · Co trang 50 va 100 dung so trang — ĐẠT
- A106 · Co trang bat thuong khong lam treo may chu — ĐẠT
- A107 · Ghep moi trang khong trung khong thieu — ĐẠT
- A108 · A109 Tim kiem khong phan biet hoa thuong — ĐẠT
- A110 · A111 Tim dung mot nguoi va tim khong thay — ĐẠT
- A112 · Ky tu dac biet khong gay loi va khong bi hieu la dai dien — ĐẠT
- A113 · Loc gioi tinh dung so luong — ĐẠT
- A114 · Sap xep hai chieu dung tren moi cot — ĐẠT
- A115 · Sap theo Ho ten dung bang chu cai tieng Viet — ĐẠT
- A116 · Them tay hop le tang dung mot nguoi — ĐẠT
- A117 · Them tay thieu ho ten bi tu choi — ĐẠT
- A118 · A119 Bien ngay chinh thuc bang hom nay — ĐẠT
- A120 · Sua ngay chinh thuc lam moi thu doi theo ngay — ĐẠT
- A121 · Xoa mot nguoi giam dung mot — ĐẠT
- A122 · Xoa nhieu nguoi giam dung so luong — ĐẠT
- A123 · Xoa id khong ton tai tra loi nghiep vu — ĐẠT
- A124 · Xoa danh sach co id trung khong dem trung — ĐẠT
- A125 · Tong so va tuoi dang tinh den hom nay — ĐẠT
- A126 · Lay mot dang vien theo id — ĐẠT

### A2AwardPeriodTests

- A201 · Bon dot chinh sap theo tu ngay — ĐẠT
- A202 · A203 Trang thai dot tai T0 va T1 — ĐẠT
- A204 · So nguoi du dieu kien nam nay tren tung dong — ĐẠT
- A205 · A206 A207 Ba rang buoc chan luu — ĐẠT
- A208 · Dot bat dau 29 02 hop le va thu ve 28 02 o nam khong nhuan — ĐẠT
- A209 · Hai dot chong lan van luu duoc va co canh bao dung cap — ĐẠT
- A210 · Bon dot chinh canh bao dung nam khoang trong — ĐẠT
- A211 · Bo dot phu kin khong con canh bao — ĐẠT
- A212 · Noi den ngay lam danh sach doi ngay — ĐẠT
- A213 · Xoa dot khong lam mat dang vien — ĐẠT
- A214 · Du dieu kien Dot 7 11 nam 2026 — ĐẠT
- A215 · A216 Du dieu kien o nam khac — ĐẠT
- A217 · Du dieu kien Dot 3 2 nam 2026 — ĐẠT
- A218 · Nam bat thuong duoc xu ly tat dinh — ĐẠT
- A219 · Dot khong ton tai tra loi nghiep vu — ĐẠT
- A220 · Bo lon khong gay nhieu cho bo loi — ĐẠT

### A3DashboardTests

- A301 · Dashboard tai T0 — ĐẠT
- A302 · Dashboard tai T1 dang dien ra — ĐẠT
- A303 · Dashboard tai T2 nhay sang nam sau — ĐẠT
- A304 · Khong co dot nao bao chua cai dot — ĐẠT
- A305 · Khong co dang vien nao — ĐẠT
- A306 · Kho trong hoan toan — ĐẠT
- A307 · Badge chua thuoc dot nao bang 7 — ĐẠT
- A308 · Canh bao tren Dashboard khop voi man Dot — ĐẠT

### A4UnassignedTests

- A401 · Nam 2026 dung bay nguoi va dung nhan khoang trong — ĐẠT
- A401b · Chua cai dot nao thi nhan la Truoc dot dau tien — ĐẠT
- A402 · Doi buoc sang 10 con sau nguoi — ĐẠT
- A403 · Nam 2025 va 2027 khop ket qua mong doi — ĐẠT
- A404 · Bo dot phu kin thi khong ai bi sot — ĐẠT
- A405 · Sap theo Moc roi Ho ten — ĐẠT
- A406 · Badge luon khop so dong cua man hinh — ĐẠT

### A5SettingsTests

- A501 · Kho trong van tra cai dat mac dinh — ĐẠT
- A502 · Doi buoc lam moi danh sach doi theo ngay — ĐẠT
- A503 · A504 A505 Cai dat sai bi tu choi — ĐẠT
- A506 · Khoi phuc mac dinh khong dung ten don vi — ĐẠT
- A507 · A508 Ten don vi luu duoc va de trong duoc — ĐẠT
- A509 · Luu hai lan van chi mot ban ghi cai dat — ĐẠT
- A510 · Xem truoc day moc khong ghi gi vao co so du lieu — ĐẠT
- A511 · Dang xuat tra 200 va khong huy token phia may chu — ĐẠT

### A6ImportTests

- A601 · Xem truoc file loi 32 dong hop le va chua ghi gi — ĐẠT
- A602 · Nap sau xem truoc tang dung 32 — ĐẠT
- A603 · Xem truoc file bon dong loi dung so dong va dung ly do — ĐẠT
- A604 · Nap file bon dong loi chi them sau nguoi — ĐẠT
- A605 · Chi xem truoc roi bo thi tong khong doi — ĐẠT
- A606 · Nap hai lan cung mot file tang gap doi — ĐẠT
- A607 · A611 Loi cap file bi chan ngay (fileName: "loi-chi-co-tieu-de.xlsx", expectedKey: "Mes.Import.Invalid.NoDataRows") — ĐẠT
- A607 · A611 Loi cap file bi chan ngay (fileName: "loi-dinh-dang-csv.csv", expectedKey: "Mes.Import.Invalid.Extension") — ĐẠT
- A607 · A611 Loi cap file bi chan ngay (fileName: "loi-khong-phai-xlsx.xlsx", expectedKey: "Mes.Import.Invalid.Extension") — ĐẠT
- A607 · A611 Loi cap file bi chan ngay (fileName: "loi-rong.xlsx", expectedKey: "Mes.Import.Invalid.Empty") — ĐẠT
- A607 · A611 Loi cap file bi chan ngay (fileName: "loi-sai-cot.xlsx", expectedKey: "Mes.Import.Invalid.Columns") — ĐẠT
- A609 · File chi co tieu de bao khac file rong — ĐẠT
- A612 · File qua 10MB bi chan truoc khi doc noi dung — ĐẠT
- A613 · File hop le 9 9MB van duoc chap nhan — ĐẠT
- A614 · Khong gui file tra 400 — ĐẠT
- A615 · Gui hai file duoc xu ly tat dinh — ĐẠT
- A616 · Cat ket noi giua luc nap khong de lai du lieu nua voi — ĐẠT
- A617 · Loi ky thuat giua chung cuon lai toan bo — ĐẠT
- A618 · Nap file 1200 dong — ĐẠT
- A619 · File mau dung cot va nap lai duoc — ĐẠT
- A620 · O ngay kieu ngay cua Excel van doc duoc — ĐẠT
- A621 · Mot dong nhieu loi tra du moi ly do — ĐẠT

### A7ExportTests

- A701 · A702 Ten file dung quy uoc — ĐẠT
- A703 · Ten file chua thuoc dot nao — ĐẠT
- A704 · Dong tieu de du ba phan — ĐẠT
- A705 · Ten don vi de trong thi bo dong do — ĐẠT
- A706 · A707 A708 Noi dung file khop bang dang xem — ĐẠT
- A709 · Xuat danh sach rong van ra file hop le — ĐẠT
- A710 · Ba endpoint xuat deu mo duoc bang thu vien doc Excel — ĐẠT
- A711 · Xuat tu Dashboard theo dot sap toi — ĐẠT
- A712 · File chua thuoc dot nao co cot khoang trong — ĐẠT

### A8DefectTests

- QcT2702 · So trang lon khong duoc lam may chu loi 500 — BỎ QUA
- QcT2703 · Co trang phai bi chan hoac kep ve muc tran — BỎ QUA
- QcT2704 · Loi ep kieu tham so phai tra khoa thong diep — BỎ QUA
- QcT2705 · Gia tri loc la khong duoc bo qua lang le — BỎ QUA
- QcT2706 · Xem truoc day moc phai co tran — BỎ QUA
- QcT2707 · Moi khoa Backend tra ra deu phai co trong hop dong — BỎ QUA

### A9TechnicalTests

- A901 · Khong noi nao trong src doc dong ho may — ĐẠT
- A902 · Ket qua khong phu thuoc mui gio cua tien trinh — ĐẠT
- A903 · Bien ep ngay khong co hieu luc o Production — HỎNG: ca quét tĩnh đòi chuỗi HUYHIEUDANG_TEST_TODAY không xuất hiện ở BE/src, mâu thuẫn trực tiếp với QC-T27-01 vốn đòi BE đọc chính biến đó — file của QC, không tự sửa
- A903b · Ngoai Production bien ep ngay phai co hieu luc — BỎ QUA
- A904 · Mot van dang vien van tinh duoi mot giay — ĐẠT
- A905 · Moi thong bao loi deu tra khoa dich duoc va khong lo noi bo — ĐẠT
- A906 · Moi truong ngay nghiep vu la ngay thuan — ĐẠT
