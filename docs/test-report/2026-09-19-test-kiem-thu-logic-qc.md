# Báo cáo kiểm thử — Kiểm thử logic QT1–QT11 của QC (T26)

Lệnh đã chạy: `cd BE && dotnet test HuyHieuDang.sln`

Tổng: 225 test — ĐẠT 219, HỎNG 0, BỎ QUA 6.

- `HuyHieuDang.Core.QcTests` — 113 đạt, 6 bỏ qua (dự án mới của QC)
- `HuyHieuDang.Core.UnitTests` — 103 đạt (test của Backend, không sửa)
- `HuyHieuDang.Infrastructure.UnitTests` — 2 đạt
- `HuyHieuDang.Infrastructure.IntegrationTests` — 1 đạt

Sáu ca bỏ qua là sáu lỗi QC đã tìm được: mỗi ca mô tả hành vi đúng, đang đỏ, và được để
`Skip` kèm mã lỗi để bộ kiểm thử vẫn xanh cho tới khi Backend sửa. Bỏ `Skip` là ca đỏ lại ngay.

Mọi ca dùng ngày cố định T0 = 19/09/2026, T1 = 15/10/2026, T2 = 01/12/2026 và bộ dữ liệu biên
`tests/fixtures/data`. Không ca nào đọc đồng hồ máy.

## Qc01Qt1MilestoneTests — QT1 dãy mốc huy hiệu

- Bước 61 lớn hơn cả khoảng 30–90 nên chỉ còn mốc đầu — ĐẠT
- Mốc 95 đúng bằng Kết thúc thì phải nằm trong dãy — ĐẠT
- Cài 1 / 100 / 1 cho đúng 100 mốc, không treo — ĐẠT
- Dãy mốc luôn tăng dần và không có mốc trùng — ĐẠT
- Dãy mốc khớp expected.json ở cả cài đặt Bước 5 và Bước 10 — ĐẠT
- Dãy mốc khớp bản hiện thực độc lập của QC trên 9 bộ cài đặt — ĐẠT
- Cài đặt sai bị từ chối bằng lỗi nghiệp vụ, không trả dãy rỗng lặng lẽ — ĐẠT
- Gọi hai lần cùng cài đặt cho hai danh sách bằng nhau nhưng tách rời nhau — ĐẠT
- Bước bằng int.MaxValue chỉ được cho ra mốc 30 — BỎ QUA (lỗi QC-04: tràn số, trả 31 mốc trong đó 15 mốc âm)

## Qc02Qt2AnniversaryTests — QT2 ngày tròn mốc

- Năm 2000 chia hết 400 nên vẫn nhuận, giữ 29/02 — ĐẠT
- Năm 1900 chia hết 100 nhưng không nhuận nên lùi về 28/02 — ĐẠT
- Vào Đảng 28/02 không được nhảy sang 29/02 ở năm nhuận — ĐẠT
- Vào Đảng 31/01 giữ nguyên ngày 31 sau 30 năm — ĐẠT
- Ngày cuối của cả 7 tháng có 31 ngày đều giữ nguyên — ĐẠT
- Mốc 0 trả đúng ngày vào Đảng — ĐẠT
- Mọi ngày của 4 năm mẫu, 7 mốc mỗi ngày, đều khớp bản hiện thực độc lập của QC — ĐẠT

## Qc03Qt3PartyAgeTests — QT3 tuổi đảng và QT3a mốc kế tiếp

- Tuổi đảng đúng ngày kỷ niệm, trước và sau một ngày — ĐẠT
- Người vào Đảng 29/02 được tính tuổi từ 28/02 ở năm không nhuận — ĐẠT
- Người cao tuổi đảng nhất 91 năm, người đúng 90 năm, người vào Đảng hôm nay bằng 0 — ĐẠT
- Tuổi đảng chỉ tăng theo thời gian, quét cả bộ lõi qua 2.192 ngày — ĐẠT
- Tuổi đảng khớp bản hiện thực độc lập của QC trên mọi ngày 2024–2030 của cả bộ lõi — ĐẠT
- Mốc kế tiếp phải lớn hơn tuổi đảng, người tròn 30 thì mốc kế tiếp là 35 — ĐẠT
- Người vào Đảng 29/02/1988 có mốc kế tiếp 40 rơi đúng 29/02/2028 — ĐẠT
- Người đã chạm hoặc vượt mốc 90 không còn mốc kế tiếp, kể cả khi đổi Bước sang 10 — ĐẠT
- Mốc kế tiếp khớp bản hiện thực độc lập của QC ở T0, T1, T2 với hai cài đặt — ĐẠT

## Qc04Qt4EligibilityTests — QT4 đủ điều kiện trong đợt

- Đợt một ngày chỉ nhận đúng người tròn mốc hôm đó, lệch một ngày là loại — ĐẠT
- Không ai đủ điều kiện ở hai đợt trong cùng một năm, xét 4 năm — ĐẠT
- Người tròn mốc năm sau chỉ đủ điều kiện khi chọn đúng năm đó — ĐẠT
- Đợt bắt đầu 29/02 thu về 28/02 ở năm không nhuận và giữ 29/02 ở năm nhuận — ĐẠT
- Người vào Đảng 29/02/1988 chỉ đủ điều kiện ở năm nhuận 2028 với mốc 40 — ĐẠT
- Đợt 7/11 năm 2026 có 6 người với Bước 5 và 4 người với Bước 10 — ĐẠT
- Phân bổ mốc của Đợt 7/11 năm 2026 đúng 30×3, 35×1, 40×1, 45×1 — ĐẠT
- Danh sách đủ điều kiện từng đợt từng năm khớp expected.json — ĐẠT
- Đủ điều kiện khớp bản hiện thực độc lập của QC trên 7 đợt × 16 năm × 32 người — ĐẠT

## Qc05Qt5NoStoredResultTests — QT5 không lưu kết quả

- Gọi hai lần với cùng dữ liệu cho cùng kết quả, không tác dụng phụ — ĐẠT
- Service không sửa danh sách đợt và danh sách mốc được truyền vào — ĐẠT
- Đổi cài đặt giữa hai lần gọi thì lần thứ hai đổi theo ngay — ĐẠT
- Nới Đến ngày của đợt thì người đang bị sót chuyển sang đủ điều kiện ngay — ĐẠT
- Không có bảng hay entity nào lưu danh sách đủ điều kiện — ĐẠT

## Qc06Qt6PeriodTests — QT6 đợt trao huy hiệu

- Năm đợt hợp lệ (thường, một ngày, trọn năm, biên 29/02 ở hai loại năm) gắn năm ra đúng ngày — ĐẠT
- Bộ 4 đợt chính để hở đúng 5 khoảng trống ở cả 4 năm xét — ĐẠT
- Các khoảng trống không chồng nhau, không thủng, phủ đúng phần năm chưa có đợt — ĐẠT
- Khoảng trống khớp bản hiện thực độc lập của QC trên 4 bộ đợt, 2024–2030 — ĐẠT
- Cảnh báo chồng lấn khớp bản hiện thực độc lập của QC, hai đợt kề nhau không bị coi là chồng lấn — ĐẠT
- Hai đợt chồng lấn vẫn tính được bình thường, chỉ là cảnh báo — ĐẠT
- Đợt có Từ ngày sau Đến ngày phải bị từ chối — BỎ QUA (lỗi QC-01: service nhận bừa)
- Ngày tháng không tồn tại như 31/02 phải báo lỗi nghiệp vụ — BỎ QUA (lỗi QC-01: ném ArgumentOutOfRangeException)

## Qc07Qt7MissedMilestoneTests — QT7 chưa thuộc đợt nào

- Không cài đợt nào thì 27 người của bộ lõi đều bị sót ở 2026 — ĐẠT
- Bộ đợt phủ kín cả năm thì không ai bị sót — ĐẠT
- Số người bị sót khớp expected.json cho cả 6 bộ đợt của kế hoạch — ĐẠT
- Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp expected.json ở 4 bộ đợt — ĐẠT
- Đổi Bước sang 10 thì người tròn mốc 35 rời danh sách, còn 6 người — ĐẠT
- Người bị sót và người đủ điều kiện là hai tập rời nhau — ĐẠT
- Nhãn khoảng trống khi chưa cài đợt nào phải khớp expected.json — BỎ QUA (lỗi QC-02: service ghi "Trước đợt đầu tiên", QC chờ "Sau đợt cuối cùng")

## Qc08Qt8UpcomingPeriodTests — QT8 đợt sắp tới

- Đợt sắp tới khớp bản hiện thực độc lập của QC ở mọi ngày của 2026 và 2028, 4 bộ đợt — ĐẠT
- Chưa cài đợt nào thì không có đợt sắp tới — ĐẠT
- Đợt sắp tới đúng ở 7 mốc thời gian, kể cả hai biên đúng Đến ngày và ngay sau Đến ngày — ĐẠT
- Đợt 29/02 sang năm không nhuận thu về 28/02 và còn đúng 58 ngày — ĐẠT
- Đợt sắp tới không bao giờ là đợt đã kết thúc — ĐẠT
- Hai đợt cùng Từ ngày phải cho kết quả tất định — BỎ QUA (lỗi QC-03: kết quả đổi theo thứ tự nạp)

## Qc09Qt11PeriodStatusTests — QT11 trạng thái đợt

- Trạng thái và số ngày còn lại khớp bản hiện thực độc lập của QC ở mọi ngày 2026–2028 — ĐẠT
- Trạng thái 4 đợt chính khớp expected.json ở T0, T1, T2 — ĐẠT
- Số ngày còn lại đếm đúng từng ngày: 273 ngày đầu năm, 12 ngày ở T0, 1 ngày sát Từ ngày, hết số khi vào đợt — ĐẠT
- Chỉ trạng thái Sắp tới mới có số ngày còn lại và số đó luôn dương — ĐẠT

## Qc10PerformanceTests — yêu cầu phi chức năng

- Tính đủ điều kiện một đợt một năm cho 10.000 đảng viên dưới 1 giây — ĐẠT
- Quét chưa thuộc đợt nào cho 10.000 đảng viên dưới 1 giây — ĐẠT

## Qc11ClockScanTests — A-901 cấm đọc đồng hồ hệ thống

- Không nơi nào trong BE/src đọc đồng hồ theo giờ máy — ĐẠT
- Service tính mốc tuổi đảng không đọc đồng hồ, chỉ nhận ngày qua tham số — ĐẠT
- Module nghiệp vụ trong Infrastructure không đọc đồng hồ, trừ 7 dòng tầng khung đã loại trừ — ĐẠT
- Core không được đọc đồng hồ hệ thống — BỎ QUA (lỗi QC-05: `BaseEntity.cs:14` dùng `DateTimeOffset.UtcNow`)
