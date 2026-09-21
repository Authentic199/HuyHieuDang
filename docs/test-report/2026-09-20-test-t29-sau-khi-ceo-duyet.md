# Báo cáo kiểm thử — Chạy lại T29 sau khi CEO duyệt, trên `main` đã có PR #44

| | |
|---|---|
| Task | T29 — vòng sửa theo bốn quyết định của CEO ngày 20/09/2026 |
| Người chạy | QC |
| Ngày chạy | 20/09/2026 |
| Commit `main` đã gộp vào nhánh | `003bcaf` (PR #44 — T46) |
| Nhánh chạy | `test/T29-hoi-quy` |

Bản trước của vòng này: `2026-09-20-test-hoi-quy-ba-tang-tren-main.md` (chạy trên
`main` tại `5784988`, trước khi PR #44 gộp). Từng ca của cả ba tầng nằm ở bản đó và
**không đổi**; báo cáo này ghi đúng phần đã đổi giữa hai lần chạy.

## 1. Lệnh đã chạy và kết quả

```bash
$ cd BE && dotnet test HuyHieuDang.sln
Passed! - Failed: 0, Passed: 124, Skipped: 0, Total: 124 - HuyHieuDang.Core.UnitTests.dll
Passed! - Failed: 0, Passed: 118, Skipped: 1, Total: 119 - HuyHieuDang.Core.QcTests.dll
Passed! - Failed: 0, Passed:   1, Skipped: 0, Total:   1 - HuyHieuDang.Infrastructure.IntegrationTests.dll
Passed! - Failed: 0, Passed:  44, Skipped: 0, Total:  44 - HuyHieuDang.Infrastructure.UnitTests.dll
Passed! - Failed: 0, Passed: 132, Skipped: 7, Total: 139 - HuyHieuDang.Web.QcIntegrationTests.dll
Passed! - Failed: 0, Passed: 181, Skipped: 0, Total: 181 - HuyHieuDang.Web.IntegrationTests.dll
EXIT=0
```

**600 đạt · 0 hỏng · 8 bỏ qua** trên **608** ca — giữ nguyên như lần chạy trước.

```bash
$ cd FE && npm run build
✓ built in 8.67s
BUILD_EXIT=0

$ cd FE && npm run typecheck:e2e
TYPECHECK_EXIT=0
```

## 2. Ca đã đổi ở lần chạy này

### A8DefectTests

- QC-T27-05 · Giá trị lọc sai kiểu không được bỏ qua lặng lẽ — **BỎ QUA** (đang sửa ở T47, HUYH-53)
- QC-T27-07 · Mọi khóa Backend trả ra đều phải có trong hợp đồng — **BỎ QUA** (đã sửa ở PR #46, gỡ Skip khi #46 gộp)

Hai dòng lý do `Skip` sửa theo quyết định của CEO. Ca `QC-T27-05` còn được viết rộng
ra cho đúng gốc lỗi: trước chỉ thử ba giá trị của một trường `Gender`, nay thử năm lời
gọi trên hai endpoint, phủ enum, ngày, Guid và số nguyên, và chốt mức đúng là `400`
kèm `Mes.Common.Invalid.Parameter` thay cho "400 hoặc 0 dòng". Một bản vá chỉ chữa
riêng `Gender` sẽ bị ca này bắt.

### Support/QcMessages.cs

- Thêm hằng số `Mes.Common.Invalid.Parameter` (hợp đồng v1.4) cho ca `QC-T27-05` dùng.
- Ghi chú lỗi QC-T29-02 tại chỗ: bảng `ContractKeys` là bản chép tay và đã trôi khỏi
  hợp đồng.

## 3. Ca kiểm chứng cổng kiểm kiểu e2e (E-907)

Chèn `const _qcProbe: number = "chuoi"` vào `FE/e2e/__qc_probe.ts` rồi chạy hai lệnh:

- `npm run typecheck:e2e` bắt được lỗi, thoát **2**, báo đúng 1 lỗi TS — ĐẠT
- `npm run build` **vẫn** thoát **0**, dựng xong `dist` — ĐẠT

Hai kết quả đó chứng minh điều PR #44 nhắm tới: mã kiểm thử không còn quyền làm gãy gói
sản phẩm, và lỗi kiểu trong `e2e/` bị bắt ở cổng của QC. Vì vậy
`npm run typecheck:e2e` được ghi thành **bước bắt buộc số 1** của tầng 3 trong
`docs/test-plan.md` mục 6, mang mã ca `E-907`.

## 4. Lỗi tìm thêm ở vòng này

### QC-T29-02 · Bảng khóa đối chứng của QC là bản chép tay và đã trôi khỏi hợp đồng

Mục 1.5 hợp đồng trên `main` có 34 khóa; bản chép trong `QcMessages.ContractKeys` có 36.
Hai khóa thừa là `Mes.User.Required.Username` và `Mes.User.Required.Password` — Backend
thật sự trả chúng khi đăng nhập bỏ trống ô, nhưng hợp đồng chưa liệt kê.

Hệ quả: ca `A-905` bấy lâu xanh nhờ bản chép chứ không nhờ hợp đồng, và nó che đúng hai
khóa thuộc về QC-T27-07 — đó là lý do QC báo ba khóa thiếu trong khi thực tế là bảy.

Chứng minh: đổi `ContractKeys` sang đọc thẳng `docs/api-contract.md` thì `A-905` đỏ ngay
tại đúng hai khóa đó.

```
Shouldly.ShouldAssertException : QcMessages.ContractKeys should contain
"Mes.User.Required.Username" but was actually [... 34 khóa đọc từ hợp đồng ...]
```

Bản sửa **chưa đưa vào vòng này**: đọc thẳng hợp đồng lúc này làm `A-905` đỏ trên `main`,
mà pull request của T29 phải gộp trước #46 theo thứ tự CEO chốt. Bản sửa sẽ vào cùng lúc
với việc gỡ `Skip` của `QcT2707`, ngay sau khi #46 gộp. Lý do ghi tại chỗ trong
`QcMessages.cs` để người sau không tưởng là bỏ sót.
