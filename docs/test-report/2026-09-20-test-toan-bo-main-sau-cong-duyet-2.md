# Kiểm thử toàn bộ `main` sau cổng duyệt 2

Lệnh đã chạy, tại `BE/`:

```
dotnet test HuyHieuDang.sln
```

Trên `main` tại `dfa53e7`. Tổng **599 đạt · 0 hỏng · 8 bỏ qua**.

| Dự án kiểm thử | Đạt | Hỏng | Bỏ qua |
|---|---|---|---|
| HuyHieuDang.Core.UnitTests | 124 | 0 | 0 |
| HuyHieuDang.Core.QcTests | 118 | 0 | 1 |
| HuyHieuDang.Infrastructure.UnitTests | 44 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |
| HuyHieuDang.Web.IntegrationTests | 180 | 0 | 0 |
| HuyHieuDang.Web.QcIntegrationTests | 132 | 0 | 7 |

## Ghi chú từng lớp

Từng ca đã được liệt kê trong các báo cáo trước, không chép lại ở đây:

- Logic mốc tuổi đảng QT1–QT11: `2026-09-19-test-kiem-thu-logic-qc.md` và `2026-09-19-test-qc-sau-khi-sua-bon-loi.md`.
- Tích hợp API 28 endpoint trên PostgreSQL thật: `2026-09-20-test-tich-hop-api-qc-t27.md`.

Tám ca bỏ qua đều có mã và lý do đã chốt: QC-05 (Core không đọc đồng hồ hệ thống) và bảy ca chờ sửa lỗi T36.

## Điểm kiểm lần này

- Hai ca từng làm `main` đỏ sau đợt gộp chuỗi — nhãn khoảng trống khi chưa cài đợt nào — nay đạt, sau khi PR #29 kéo kỳ vọng về "Trước đợt đầu tiên" theo contract mục 1.10.
- Bản cài đặt đối chứng của QC trong `QcOracle.cs` vẫn gán cứng "Sau đợt cuối cùng"; commit kèm báo cáo này sửa nốt để hai bản đối chứng không lệch nhau.
