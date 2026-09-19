# Báo cáo kiểm thử — Sửa bốn lỗi QC-01…QC-04 của T26

Lệnh đã chạy: `cd BE && dotnet test HuyHieuDang.sln`

Tổng: 117 test — ĐẠT 117, HỎNG 0, BỎ QUA 0.

- `HuyHieuDang.Core.UnitTests` — 114 đạt (103 cũ + 11 ca mới của lần sửa này)
- `HuyHieuDang.Infrastructure.UnitTests` — 2 đạt
- `HuyHieuDang.Infrastructure.IntegrationTests` — 1 đạt

Trước khi sửa, 10 trong 11 ca mới đều hỏng đúng như mô tả lỗi của QC.

## Các ca mới thêm

### Qt1MilestoneSequenceTests — QT1, lỗi QC-04

- Bước lớn hơn cả khoảng chỉ cho ra mốc đầu tiên, không tràn số thành mốc âm — ĐẠT

### Qt6PeriodWarningTests — QT6 và QT7, lỗi QC-01 và QC-02

- Chưa cài đợt nào thì khoảng trống cả năm mang nhãn "Sau đợt cuối cùng" — ĐẠT
- Đợt có Từ ngày sau Đến ngày bị từ chối bằng lỗi nghiệp vụ — ĐẠT
- Ngày 31/02 không tồn tại nên bị từ chối bằng lỗi nghiệp vụ — ĐẠT
- Ngày 30/02 không tồn tại nên bị từ chối bằng lỗi nghiệp vụ — ĐẠT
- Ngày 32/01 không tồn tại nên bị từ chối bằng lỗi nghiệp vụ — ĐẠT
- Ngày 31/04 không tồn tại nên bị từ chối bằng lỗi nghiệp vụ — ĐẠT
- Tháng 13 không tồn tại nên bị từ chối bằng lỗi nghiệp vụ — ĐẠT
- Ngày 0 không tồn tại nên bị từ chối bằng lỗi nghiệp vụ — ĐẠT
- Đợt bắt đầu 29/02 vẫn được chấp nhận vì năm nhuận có ngày này — ĐẠT

### Qt8UpcomingPeriodTests — QT8, lỗi QC-03

- Hai đợt cùng Từ ngày cho cùng một kết quả dù đảo thứ tự nạp danh sách — ĐẠT

## Các ca cũ

103 ca của lần bàn giao trước vẫn đạt, chi tiết ở
`docs/test-report/2026-09-19-test-tinh-moc-tuoi-dang.md`. Không ca nào phải sửa lại kỳ vọng
sau bốn thay đổi này.

## Đối chiếu với bộ test của QC

Gộp tạm nhánh `test/T26-kiem-thu-logic` vào một nhánh nháp, bỏ `Skip` của bốn lỗi rồi chạy
`dotnet test HuyHieuDang.sln`:

- `HuyHieuDang.Core.QcTests` — 118 đạt, 1 bỏ qua (QC-05 đã được chốt loại trừ)
- Năm ca từng bị bỏ qua nay đều đạt:
  - U-602 · Đợt có Từ ngày > Đến ngày phải bị từ chối — ĐẠT
  - U-606/U-607 · Ngày/tháng không tồn tại phải báo lỗi nghiệp vụ — ĐẠT
  - QC-04 · Bước = int.MaxValue chỉ được cho ra mốc đầu, không được tràn số — ĐẠT
  - U-808 · Hai đợt cùng Từ ngày phải cho kết quả tất định — ĐẠT
  - QC-02 · Nhãn khoảng trống khi chưa cài đợt nào phải khớp expected.json — ĐẠT

Nhánh nháp đã xoá; nhánh `feat/T07-tinh-moc-tuoi-dang` không chứa tệp nào của QC.
