# Báo cáo kiểm thử — gộp chuỗi nhánh T08 → T14

Chạy trên `feat/T14-xuat-excel` sau khi gộp xong cả sáu bước.

Lệnh đã chạy: `cd BE && dotnet build HuyHieuDang.sln` rồi `cd BE && dotnet test HuyHieuDang.sln`

Dựng: 0 lỗi, 26 cảnh báo. Kiểm thử: 465 đạt · 2 hỏng · 1 bỏ qua.

| Dự án kiểm thử | Đạt | Hỏng | Bỏ qua |
|---|---|---|---|
| HuyHieuDang.Core.UnitTests | 123 | 1 | 0 |
| HuyHieuDang.Core.QcTests | 117 | 1 | 1 |
| HuyHieuDang.Infrastructure.UnitTests | 44 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |
| HuyHieuDang.Web.IntegrationTests | 180 | 0 | 0 |

## Kết quả từng bước gộp

Mỗi bước đều chạy lại toàn bộ `dotnet test` trước khi đẩy lên.

Bước 1 · `feat/T09-api-dang-vien` ← `feat/T08-auth` — ĐẠT, 295 đạt 0 hỏng
Bước 2 · `feat/T10-import-excel` ← `feat/T09-api-dang-vien` — ĐẠT, 339 đạt 0 hỏng
Bước 3 · `feat/T11-api-dot` ← `feat/T10-import-excel` — ĐẠT, 372 đạt 0 hỏng
Bước 4 · `feat/T12-api-tinh-toan` ← `feat/T11-api-dot` — HỎNG 2 ca, bất đồng nhãn khoảng trống giữa hợp đồng API và expected.json của QC
Bước 5 · `feat/T13-api-cai-dat` ← `feat/T12-api-tinh-toan` — HỎNG 2 ca, cùng nguyên nhân bước 4
Bước 6 · `feat/T14-xuat-excel` ← `feat/T13-api-cai-dat` — HỎNG 2 ca, cùng nguyên nhân bước 4

## Hai ca hỏng còn lại

Cả hai cùng một nguyên nhân, đã có từ trước khi gộp, không phải do phép gộp sinh ra.

HuyHieuDang.Core.UnitTests · Chưa cài đợt nào thì cả năm mang nhãn "Sau đợt cuối cùng" — HỎNG: mã đang trả "Trước đợt đầu tiên" theo hợp đồng API mục 1.10
HuyHieuDang.Core.QcTests · QC-02 · Nhãn khoảng trống khi chưa cài đợt nào phải khớp expected.json — HỎNG: expected.json ghi "Sau đợt cuối cùng", mã trả "Trước đợt đầu tiên"

Chính ca QC-02 đã ghi sẵn trong mã nguồn rằng đây là bất đồng chưa ai quyết: "Hai bên phải thống nhất vì nhãn này hiện thẳng lên UC-40". Chờ CEO quyết, không tự sửa.

## Ca A-901 (điều kiện bắt buộc của việc gộp)

A-901 · Không nơi nào trong BE/src đọc đồng hồ theo giờ máy — ĐẠT
A-901 · Service tính mốc tuổi đảng không đọc đồng hồ, chỉ nhận today qua tham số — ĐẠT
A-901 · Module nghiệp vụ trong Infrastructure không đọc đồng hồ, trừ bảy dòng tầng khung — ĐẠT

Ba service phải bỏ dòng tự gán `UpdatedAt` bằng đồng hồ máy thì A-901 mới xanh, vì T08 đã chuyển việc đóng dấu sang `UpdatedAtInterceptor`: `PartyMemberService` (bước 1), `AwardPeriodService` (bước 3), `AppSettingService` (bước 5).
