# Báo cáo kiểm thử — chuỗi nhánh T08 → T14 sau khi chốt nhãn "Trước đợt đầu tiên"

Chạy trên `feat/T14-xuat-excel` sau khi CEO chốt hợp đồng API thắng và oracle của QC được sửa theo.

Lệnh đã chạy: `cd BE && dotnet build HuyHieuDang.sln` rồi `cd BE && dotnet test HuyHieuDang.sln`

Dựng: 0 lỗi, 25 cảnh báo. Kiểm thử: 467 đạt · 0 hỏng · 1 bỏ qua.

| Dự án kiểm thử | Đạt | Hỏng | Bỏ qua |
|---|---|---|---|
| HuyHieuDang.Core.UnitTests | 124 | 0 | 0 |
| HuyHieuDang.Core.QcTests | 118 | 0 | 1 |
| HuyHieuDang.Infrastructure.UnitTests | 44 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |
| HuyHieuDang.Web.IntegrationTests | 180 | 0 | 0 |

Ca bỏ qua là QC-05, giữ `Skip` theo quyết định đã chốt trước đây về `BaseEntity.CreatedAt`.

## Hai ca từng hỏng, nay đã đạt

HuyHieuDang.Core.UnitTests · Chưa cài đợt nào thì cả năm mang nhãn "Trước đợt đầu tiên" — ĐẠT
HuyHieuDang.Core.QcTests · QC-02 · Nhãn khoảng trống khi chưa cài đợt nào khớp expected.json — ĐẠT

## Việc đã làm để hai ca đó xanh

Sửa tại `feat/T12-api-tinh-toan`, một commit bốn tệp, theo đúng quyết định của CEO ngày 19/09/2026:

- `tests/fixtures/qt_reference.py` — khi danh sách đợt rỗng, nhãn khoảng trống cuối năm là "Trước đợt đầu tiên".
- `tests/fixtures/data/expected.json` — sinh lại bằng `python generate.py`, `self_check` báo OK. Thay đổi chỉ rơi vào kịch bản `core_default_T0_noPeriod`: 30 dòng `gapLabel` và 4 nhãn trong mảng khoảng trống. Bảy kịch bản còn lại giữ nguyên cả hai nhãn, đã đối chiếu từng kịch bản trước và sau khi sinh.
- `BE/tests/HuyHieuDang.Core.UnitTests/Qt6PeriodWarningTests.cs` — ca đổi tên thành `GetGaps_WhenThereIsNoPeriod_LabelsTheWholeYearAsBeforeTheFirstPeriod` và khẳng định `BeforeFirstPeriod`.
- `BE/tests/HuyHieuDang.Core.QcTests/Qc07Qt7MissedMilestoneTests.cs` — xóa khối chú thích "LỖI QC-02", giữ nguyên thân ca.

Mã nguồn `PartyMilestoneCalculator` không đổi: nó vốn đã đúng hợp đồng.

## Ca A-901

A-901 · Không nơi nào trong BE/src đọc đồng hồ theo giờ máy — ĐẠT
A-901 · Service tính mốc tuổi đảng không đọc đồng hồ, chỉ nhận today qua tham số — ĐẠT
A-901 · Module nghiệp vụ trong Infrastructure không đọc đồng hồ, trừ bảy dòng tầng khung — ĐẠT
