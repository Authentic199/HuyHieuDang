# Báo cáo kiểm thử — Service tính mốc tuổi đảng (T07)

Lệnh đã chạy: `cd BE && dotnet test HuyHieuDang.sln`

Tổng: 106 test — ĐẠT 106, HỎNG 0, BỎ QUA 0.

- `HuyHieuDang.Core.UnitTests` — 103 đạt
- `HuyHieuDang.Infrastructure.UnitTests` — 2 đạt
- `HuyHieuDang.Infrastructure.IntegrationTests` — 1 đạt

Mọi ca dùng ngày cố định T0 = 19/09/2026, T1 = 15/10/2026, T2 = 01/12/2026 và bộ dữ liệu
biên trong `tests/fixtures/data`.

## Qt1MilestoneSequenceTests — QT1 dãy mốc huy hiệu

- Cài đặt mặc định 30/90/5 sinh đúng 13 mốc từ 30 đến 90 — ĐẠT
- Đổi Bước sang 10 sinh đúng 7 mốc — ĐẠT
- Bước không chia hết khoảng thì dừng trước khi vượt mốc kết thúc — ĐẠT
- Bắt đầu bằng Kết thúc thì chỉ có một mốc — ĐẠT
- Mốc bắt đầu bằng 0 bị từ chối — ĐẠT
- Mốc bắt đầu âm bị từ chối — ĐẠT
- Mốc kết thúc bằng 0 bị từ chối — ĐẠT
- Mốc kết thúc âm bị từ chối — ĐẠT
- Bước bằng 0 bị từ chối — ĐẠT
- Bước âm bị từ chối — ĐẠT
- Mốc bắt đầu lớn hơn mốc kết thúc bị từ chối — ĐẠT

## Qt2AnniversaryTests — QT2 ngày tròn mốc

- Ngày thường cộng N năm giữ nguyên ngày và tháng — ĐẠT
- Vào Đảng 29/02, năm đích không nhuận thì lấy 28/02 — ĐẠT
- Vào Đảng 29/02, năm đích nhuận thì giữ nguyên 29/02 — ĐẠT
- Mốc 0 trả về chính ngày vào Đảng — ĐẠT
- Mốc âm bị từ chối — ĐẠT

## Qt3PartyAgeTests — QT3 tuổi đảng hiện tại

- Chưa tới ngày kỷ niệm trong năm thì chưa tính năm đó — ĐẠT
- Đúng ngày kỷ niệm thì tính luôn năm đó — ĐẠT
- Trước ngày kỷ niệm một ngày thì vẫn giữ số cũ — ĐẠT
- Vào Đảng đúng hôm nay thì tuổi đảng bằng 0 — ĐẠT
- Vào Đảng 29/02, năm không nhuận tính tròn vào 28/02 — ĐẠT
- Vào Đảng 29/02, trước 28/02 một ngày thì chưa tính — ĐẠT
- Người cao tuổi đảng nhất cho ra 91 năm, vượt mốc lớn nhất — ĐẠT
- Ngày vào Đảng ở tương lai cho tuổi đảng bằng 0 — ĐẠT

## Qt3aNextMilestoneTests — QT3a mốc kế tiếp

- Tuổi đảng dưới mốc đầu tiên thì mốc kế tiếp là 30 — ĐẠT
- Tuổi đảng sát dưới một mốc thì mốc kế tiếp chính là mốc đó — ĐẠT
- Tuổi đảng đúng bằng một mốc thì nhảy sang mốc sau — ĐẠT
- Tuổi đảng đúng bằng mốc lớn nhất thì không còn mốc kế tiếp — ĐẠT
- Tuổi đảng vượt mốc lớn nhất thì không còn mốc kế tiếp — ĐẠT
- Vượt mốc lớn nhất thì ngày tròn mốc kế tiếp cũng trống — ĐẠT
- Người chưa tới mốc đầu tiên có ngày tròn mốc kế tiếp đúng ngày dự kiến — ĐẠT
- Đổi Bước sang 10 làm mốc kế tiếp nhảy từ 35 sang 40 — ĐẠT
- Mốc kế tiếp của cả 32 đảng viên khớp bảng của QC — ĐẠT
- Tuổi đảng của cả 32 đảng viên khớp bảng của QC — ĐẠT

## Qt4EligibilityTests — QT4 đủ điều kiện trong đợt

- Tròn mốc đúng Từ ngày của đợt thì đủ điều kiện — ĐẠT
- Tròn mốc đúng Đến ngày của đợt thì đủ điều kiện — ĐẠT
- Tròn mốc trước Từ ngày một ngày thì không đủ điều kiện — ĐẠT
- Tròn mốc sau Đến ngày một ngày thì không đủ điều kiện — ĐẠT
- Ngày tròn mốc lùi từ 29/02 về 28/02 vẫn rơi trong đợt — ĐẠT
- Đợt bắt đầu 29/02, gắn năm không nhuận thì thu về 28/02 — ĐẠT
- Người đúng mốc 90 vẫn được trao huy hiệu 90 — ĐẠT
- Người đã vượt mốc lớn nhất thì không còn mốc nào trong năm xét — ĐẠT
- Đổi Bước sang 10 làm người có mốc 35 rời khỏi danh sách — ĐẠT
- Đổi Bước sang 10 vẫn giữ người có mốc 40 — ĐẠT
- Năm không có mốc nào rơi vào thì không đủ điều kiện — ĐẠT
- Tổng số và phân bổ theo mốc khớp bảng của QC — kịch bản mặc định tại T0 — ĐẠT
- Tổng số và phân bổ theo mốc khớp bảng của QC — kịch bản Bước 10 — ĐẠT
- Tổng số và phân bổ theo mốc khớp bảng của QC — kịch bản đợt biên 29/02 — ĐẠT
- Tổng số và phân bổ theo mốc khớp bảng của QC — kịch bản phủ kín cả năm — ĐẠT
- Tổng số và phân bổ theo mốc khớp bảng của QC — kịch bản hai đợt chồng lấn — ĐẠT
- Tổng số và phân bổ theo mốc khớp bảng của QC — kịch bản nới rộng Đợt 2/9 — ĐẠT

## Qt6PeriodWarningTests — QT6 cảnh báo chồng lấn và khoảng trống

- Bốn đợt chính để lại đúng 5 khoảng trống trong năm 2026 — ĐẠT
- Khoảng trống đầu, giữa và cuối được gắn đúng nhãn — ĐẠT
- Hai đợt phủ kín 01/01–31/12 thì không còn khoảng trống — ĐẠT
- Không có đợt nào thì cả năm là một khoảng trống — ĐẠT
- Năm nhuận thì khoảng trống kết thúc đúng ngày tháng Hai — ĐẠT
- Bốn đợt chính không chồng lấn nhau — ĐẠT
- Hai đợt giao nhau được báo thành một cặp — ĐẠT
- Hai đợt dùng chung đúng một ngày vẫn bị báo chồng lấn — ĐẠT
- Hai đợt nối tiếp nhau, không chung ngày nào, thì không bị báo — ĐẠT

## Qt7MissedMilestoneTests — QT7 chưa thuộc đợt nào

- Tròn mốc trước đợt đầu tiên được gắn nhãn "Trước đợt đầu tiên" — ĐẠT
- Tròn mốc giữa hai đợt được gắn nhãn nêu tên cả hai đợt — ĐẠT
- Tròn mốc sau đợt cuối cùng được gắn nhãn "Sau đợt cuối cùng" — ĐẠT
- Tròn mốc nằm trong một đợt thì không vào danh sách này — ĐẠT
- Năm không có mốc nào thì không vào danh sách này — ĐẠT
- Không cài đợt nào thì mọi mốc trong năm đều bị bỏ lỡ — ĐẠT
- Đổi Bước sang 10 làm người có mốc 35 rời khỏi danh sách — ĐẠT
- Danh sách năm 2025 khớp bảng của QC — ĐẠT
- Danh sách năm 2026 khớp bảng của QC — ĐẠT
- Danh sách năm 2027 khớp bảng của QC — ĐẠT
- Danh sách năm 2028 khớp bảng của QC — ĐẠT

## Qt8UpcomingPeriodTests — QT8 đợt sắp tới

- Tại T0 trả về Đợt 7/11 của năm nay kèm 12 ngày còn lại — ĐẠT
- Hôm nay nằm trong một đợt thì chính đợt đó là đợt sắp tới — ĐẠT
- Mọi đợt trong năm đã qua thì lấy đợt sớm nhất của năm sau — ĐẠT
- Đúng ngày cuối của đợt thì đợt đó vẫn đang diễn ra — ĐẠT
- Sau ngày cuối của đợt cuối cùng một ngày thì chuyển sang năm sau — ĐẠT
- Chưa cài đợt nào thì không có đợt sắp tới — ĐẠT
- Đợt bắt đầu 29/02, sang năm không nhuận thì bắt đầu 28/02 — ĐẠT
- Hai đợt chồng lấn thì chọn đợt có Từ ngày sớm hơn — ĐẠT
- Đợt sắp tới khớp bảng của QC — kịch bản mặc định tại T0 — ĐẠT
- Đợt sắp tới khớp bảng của QC — kịch bản tại T1 — ĐẠT
- Đợt sắp tới khớp bảng của QC — kịch bản tại T2 — ĐẠT
- Đợt sắp tới khớp bảng của QC — kịch bản đợt biên 29/02 — ĐẠT
- Đợt sắp tới khớp bảng của QC — kịch bản phủ kín cả năm — ĐẠT
- Đợt sắp tới khớp bảng của QC — kịch bản hai đợt chồng lấn — ĐẠT
- Đợt sắp tới khớp bảng của QC — kịch bản nới rộng Đợt 2/9 — ĐẠT
- Đợt sắp tới khớp bảng của QC — kịch bản không có đợt nào — ĐẠT

## Qt11PeriodStatusTests — QT11 trạng thái đợt trong năm hiện tại

- Đợt đã kết thúc được báo Đã qua, không có số ngày còn lại — ĐẠT
- Hôm nay nằm trong đợt thì báo Đang diễn ra — ĐẠT
- Đúng ngày đầu của đợt thì báo Đang diễn ra — ĐẠT
- Đúng ngày cuối của đợt thì báo Đang diễn ra — ĐẠT
- Sau ngày cuối một ngày thì báo Đã qua — ĐẠT
- Đợt chưa bắt đầu được báo Sắp tới kèm số ngày còn lại — ĐẠT
- Trước ngày bắt đầu một ngày thì còn đúng 1 ngày — ĐẠT
- Đợt bắt đầu 29/02, gắn năm không nhuận thì thu về 28/02 — ĐẠT
- Đợt kết thúc 29/02 gắn vào năm nhuận thì giữ nguyên 29/02 — ĐẠT
- Trạng thái mọi đợt khớp bảng của QC — kịch bản mặc định tại T0 — ĐẠT
- Trạng thái mọi đợt khớp bảng của QC — kịch bản tại T1 — ĐẠT
- Trạng thái mọi đợt khớp bảng của QC — kịch bản tại T2 — ĐẠT
- Trạng thái mọi đợt khớp bảng của QC — kịch bản đợt biên 29/02 — ĐẠT
- Trạng thái mọi đợt khớp bảng của QC — kịch bản phủ kín cả năm — ĐẠT
- Trạng thái mọi đợt khớp bảng của QC — kịch bản hai đợt chồng lấn — ĐẠT
- Trạng thái mọi đợt khớp bảng của QC — kịch bản nới rộng Đợt 2/9 — ĐẠT

## SkeletonTests và SkeletonIntegrationTests — bộ khung sẵn có

- Tài khoản người dùng băm và xác minh được mật khẩu — ĐẠT
- Kiểu người dùng dùng được cho JWT — ĐẠT
- Bộ khung tích hợp khởi động được — ĐẠT

## Kiểm chứng bộ test có thật sự bắt lỗi

Thử cố tình phá bốn quy tắc trong service rồi chạy lại, mỗi lần đều có test hỏng:

| Lỗi cố tình gây ra | Số test hỏng |
|---|---|
| Bỏ quy tắc 29/02 lùi về 28/02 | 22 |
| Biến biên đợt thành loại trừ ngày đầu | 9 |
| Không chuyển sang năm sau khi mọi đợt đã qua | 5 |
| Tính mốc kế tiếp bao gồm cả mốc bằng tuổi đảng | 3 |

Khôi phục mã gốc thì cả 103 test xanh trở lại.
