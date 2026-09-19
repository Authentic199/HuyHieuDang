# Kiểm thử sau khi gộp 11 pull request vào main

CEO chạy để nghiệm thu chuỗi gộp #13 → #15 → #18 → #21 → #24 → #26 (Backend), #19 → #20 → #22 → #27 (Frontend) và #17 (contract T05b). Bản kiểm trên commit `1a36616`, trong một bản làm việc tách riêng.

## Lệnh đã chạy

```
dotnet build HuyHieuDang.sln          0 lỗi, 26 cảnh báo
dotnet test HuyHieuDang.sln --no-build
```

## Tổng kết

| Dự án test | Đạt | Hỏng | Bỏ qua |
|---|---|---|---|
| HuyHieuDang.Core.UnitTests | 123 | 1 | 0 |
| HuyHieuDang.Core.QcTests | 117 | 1 | 1 |
| HuyHieuDang.Infrastructure.UnitTests | 44 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |
| HuyHieuDang.Web.IntegrationTests | 180 | 0 | 0 |
| **Cộng** | **465** | **2** | **1** |

Hai ca hỏng cùng một nguyên nhân: mã đã đổi nhãn khoảng trống khi chưa cài đợt nào sang "Trước đợt đầu tiên" theo quyết định của CEO, nhưng kỳ vọng trong test của Backend và trong bộ dữ liệu chuẩn của QC vẫn giữ giá trị cũ. Đã mở HUYH-37 giao Backend sửa.

Từng ca của 465 ca đạt đã được ghi trong các báo cáo trước của mỗi agent trong cùng thư mục này; dưới đây chỉ chép lại hai lớp có ca hỏng.

## Qt6PeriodWarningTests

- Bộ đợt chính của năm 2026 sinh đúng năm khoảng trống — ĐẠT
- Khoảng trống đầu và cuối năm mang đúng nhãn theo vị trí — ĐẠT
- Các đợt phủ kín cả năm thì không còn khoảng trống nào — ĐẠT
- Chưa cài đợt nào thì cả năm là một khoảng trống duy nhất — ĐẠT
- Năm nhuận: khoảng trống kết thúc đúng ngày tháng Hai — ĐẠT
- Chưa cài đợt nào thì nhãn phải là "Sau đợt cuối cùng" — HỎNG: mã trả "Trước đợt đầu tiên" theo quyết định mới, ca test chưa cập nhật theo
- Đợt vắt qua 31/12 bị từ chối — ĐẠT
- Ngày hoặc tháng không có thật bị từ chối — ĐẠT
- Đợt bắt đầu ngày 29/02 vẫn được nhận — ĐẠT
- Bộ đợt chính không có cặp nào chồng lấn — ĐẠT
- Hai đợt cắt nhau thì báo đúng cặp, đúng thứ tự — ĐẠT
- Hai đợt chung đúng một ngày vẫn bị báo chồng lấn — ĐẠT
- Hai đợt sát nhau không tính là chồng lấn — ĐẠT

## Qc07Qt7MissedMilestoneTests

- U-712 · Không cài đợt nào thì 27 người của bộ lõi đều bị sót ở 2026 — ĐẠT
- U-711 · Bộ đợt phủ kín cả năm thì không ai bị sót — ĐẠT
- Số người bị sót khớp bộ dữ liệu chuẩn cho mọi bộ đợt của kế hoạch — ĐẠT
- Từng dòng bị sót (mốc, ngày, nhãn khoảng trống) khớp bộ dữ liệu chuẩn — ĐẠT
- U-710 · Đổi Bước sang 10 thì S04 rời danh sách bị sót, còn 6 người — ĐẠT
- Người bị sót và người đủ điều kiện là hai tập rời nhau — ĐẠT
- QC-02 · Nhãn khoảng trống khi chưa cài đợt nào phải khớp bộ dữ liệu chuẩn — HỎNG: bộ dữ liệu chuẩn còn ghi "Sau đợt cuối cùng", mã trả "Trước đợt đầu tiên"
- QC-05 · Core không được đọc đồng hồ hệ thống — BỎ QUA
