# Báo cáo kiểm thử — T12b, đồng bộ nhãn khoảng trống khi chưa cài đợt nào

Lệnh đã chạy: `cd BE && dotnet build HuyHieuDang.sln` rồi `dotnet test HuyHieuDang.sln --no-build`

Build: 0 lỗi, 26 cảnh báo (StyleCop, có sẵn từ trước).

Tổng: 468 test — ĐẠT 467, HỎNG 0, BỎ QUA 1.

- `HuyHieuDang.Core.UnitTests` — 124 đạt
- `HuyHieuDang.Core.QcTests` — 118 đạt, 1 bỏ qua
- `HuyHieuDang.Infrastructure.UnitTests` — 44 đạt
- `HuyHieuDang.Infrastructure.IntegrationTests` — 1 đạt
- `HuyHieuDang.Web.IntegrationTests` — 180 đạt

Trước lần sửa này, hai ca dưới đây hỏng trên `main` vì kỳ vọng còn giữ nhãn cũ
"Sau đợt cuối cùng", trong khi mã đã chốt "Trước đợt đầu tiên".

## Các ca đã đổi kỳ vọng

### Qt6PeriodWarningTests — QT6 và QT7

- Chưa cài đợt nào thì khoảng trống cả năm mang nhãn "Trước đợt đầu tiên" — ĐẠT
- Chưa cài đợt nào thì cả năm gom thành đúng một khoảng trống 01/01–31/12 — ĐẠT
- Bộ đợt chính cho ra đúng năm khoảng trống của năm 2026 — ĐẠT
- Khoảng trống đầu và cuối được đặt nhãn theo vị trí — ĐẠT
- Bộ đợt phủ kín cả năm thì không còn khoảng trống nào — ĐẠT
- Năm nhuận thì khoảng trống trước đợt 29/02 kết thúc đúng ngày 28/02 — ĐẠT
- Đợt có Từ ngày sau Đến ngày bị từ chối bằng lỗi nghiệp vụ — ĐẠT
- Ngày hoặc tháng không tồn tại bị từ chối bằng lỗi nghiệp vụ (6 ca) — ĐẠT
- Đợt bắt đầu 29/02 vẫn được chấp nhận — ĐẠT
- Bộ đợt chính không sinh cảnh báo chồng lấn — ĐẠT
- Hai đợt giao nhau được báo thành một cặp theo đúng thứ tự — ĐẠT
- Hai đợt chung đúng một ngày vẫn bị báo chồng lấn — ĐẠT
- Hai đợt kề sát nhau không bị coi là chồng lấn — ĐẠT

### Qc07Qt7MissedMilestoneTests — QT7 đối chiếu expected.json

- Nhãn khoảng trống khi chưa cài đợt nào khớp expected.json — ĐẠT
- Không cài đợt nào thì 27 người của bộ lõi đều bị sót ở 2026 — ĐẠT
- Bộ đợt phủ kín cả năm thì không ai bị sót — ĐẠT
- Số người bị sót khớp expected.json cho mọi bộ đợt của kế hoạch — ĐẠT
- Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp expected.json — ĐẠT
- Đổi Bước sang 10 thì S04 rời danh sách bị sót, còn 6 người — ĐẠT
- Người bị sót và người đủ điều kiện là hai tập rời nhau — ĐẠT

## Các ca còn lại

441 ca còn lại của năm dự án không đổi kỳ vọng và vẫn đạt; chi tiết ở các báo cáo
trước trong cùng thư mục này. Ca `QC-05 · Core không được đọc đồng hồ hệ thống`
vẫn ở trạng thái bỏ qua như trên `main`.

## Bộ sinh fixture

`python generate.py` chạy hai lần liên tiếp, `git status` sạch sau cả hai lần —
bộ sinh tái lập được từng byte như T25 đã chốt.
