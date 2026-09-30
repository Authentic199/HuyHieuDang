| | |
|---|---|
| Việc | T75 — QC nghiệm thu tính năng Ghi chú đảng viên (HUYH-86) |
| Nhánh nghiệm thu | `test/T75-nghiem-thu-ghi-chu` |
| Ngày chạy | 30/09/2026 |
| Người chạy | QC |
| Nguồn nghiệm thu | HUYH-82 mục "QT12" và mục "Giao diện đã duyệt"; `docs/api-contract.md` v1.6 |

## 1. Nhánh nghiệm thu gộp những gì

Dựng từ `origin/main` tại `7ebadf6`, gộp lần lượt ba đầu nhánh đã ĐẠT gác cổng:

| PR | Issue | SHA đã gộp | Commit gộp |
|---|---|---|---|
| #73 | HUYH-83 (T72 — hợp đồng API v1.6, tài liệu nghiệp vụ v1.3) | `3da9d19` | `7d24911` |
| #77 | HUYH-84 (T73 — Backend ghi chú, ngày ghi, API) | `e1bb02b` | `8e68f75` |
| #78 | HUYH-85 (T74 — Frontend ba màn và hộp ghi chú) | `bc08a47` | `547bd15` |

Cả ba gộp sạch, không có xung đột nào phải gỡ tay.

## 2. Lệnh đã chạy và dòng tổng kết

```
cd BE && dotnet test HuyHieuDang.sln
  Passed! - Failed: 0, Passed: 157, Skipped: 0, Total: 157 - HuyHieuDang.Core.UnitTests.dll
  Passed! - Failed: 0, Passed: 129, Skipped: 1, Total: 130 - HuyHieuDang.Core.QcTests.dll
  Passed! - Failed: 0, Passed:   1, Skipped: 0, Total:   1 - HuyHieuDang.Infrastructure.IntegrationTests.dll
  Passed! - Failed: 0, Passed:  90, Skipped: 0, Total:  90 - HuyHieuDang.Infrastructure.UnitTests.dll
  Passed! - Failed: 0, Passed: 164, Skipped: 0, Total: 164 - HuyHieuDang.Web.QcIntegrationTests.dll
  Passed! - Failed: 0, Passed: 239, Skipped: 0, Total: 239 - HuyHieuDang.Web.IntegrationTests.dll
  → 780 đạt · 1 bỏ qua · 0 hỏng
```

```
cd FE && npm run typecheck:e2e      → xanh, không lỗi kiểu
cd FE && npm run build              → xanh, dựng xong trong 7,18s
```

```
cd FE && npx playwright test -c e2e/playwright.e2e.config.ts
  Mốc đang ép : 2026-09-19   Máy chủ báo : 2026-09-19   Đồng hồ: ĐÃ ĐÓNG BĂNG đúng mốc
  2 skipped
  14 passed (3.4m)
```

Hai ca bỏ qua là E-903 và E-904 — chúng cố ý chỉ chạy khi dựng lại dịch vụ `be` ở mốc T1 hoặc T2.

```
cd FE && npm run test:ui
  1 failed
    ca 5 · bỏ hai dòng chữ dưới hai ô ngày, Ngày sinh và Giới tính cùng hàng, không cuộn
  187 passed (59.5s)
```

Ca đỏ này là lỗi của chính ca kiểm thử, không phải lỗi sản phẩm — xem mục 6, QC-T75-02.

## 3. Ca mới tầng API — `HuyHieuDang.Web.QcIntegrationTests.A11NoteTests`

Chạy trên PostgreSQL thật, đồng hồ đóng băng ở ba mốc T0 / T1 / T2 nên ngày ghi đối chiếu bằng
chuỗi tuyệt đối, không so "gần bằng bây giờ". 20 ca, tất cả ĐẠT.

- A-801 · Ghi chú đúng 500 ký tự được lưu trọn vẹn — ĐẠT
- A-802 · Ghi chú 501 ký tự bị chặn ở cả ba đường ghi, bản ghi cũ nguyên vẹn — ĐẠT
- A-803 · Ghi chú toàn khoảng trắng, tab hay xuống dòng đều lưu thành rỗng — ĐẠT
- A-804 · Khoảng trắng hai đầu bị cắt, xuống dòng bên trong giữ nguyên — ĐẠT
- A-805 · Trần 500 ký tự đếm sau khi cắt, không đếm chuỗi thô gửi lên — ĐẠT
- A-810 · Thêm ghi chú đóng ngày ghi kèm độ lệch +07:00, đọc lại từ kho vẫn đúng ngày đó — ĐẠT
- A-811 · Đổi nội dung ghi chú thì ngày ghi nhảy sang mốc mới — ĐẠT
- A-812 · Lưu lại y hệt nội dung cũ thì ngày ghi giữ nguyên — ĐẠT
- A-813 · Sửa họ tên mà ghi chú không đổi thì ngày ghi giữ nguyên — ĐẠT
- A-814 · Xóa ghi chú đưa cả ngày ghi về rỗng; ghi lại thì đóng ngày mới — ĐẠT
- A-815 · Sửa đảng viên mà không gửi ghi chú là xóa ghi chú, không phải giữ nguyên — ĐẠT
- A-816 · Thêm người kèm ghi chú đóng ngày ghi ngay; không kèm thì cả hai trường rỗng — ĐẠT
- A-820 · Lưu ghi chú không đụng họ tên, ngày sinh, giới tính, ngày vào Đảng — ĐẠT
- A-821 · Lưu ghi chú cho người không tồn tại trả lỗi không tìm thấy, không phải lỗi máy chủ — ĐẠT
- A-822 · Lưu ghi chú khi chưa đăng nhập bị từ chối và kho không đổi — ĐẠT
- A-823 · Thân yêu cầu rỗng là xóa ghi chú; báo thành công bằng khóa sửa có sẵn — ĐẠT
- A-830 · Dashboard và Chi tiết đợt trả đúng ghi chú của đúng người, người khác để rỗng — ĐẠT
- A-831 · Thêm, sửa, xóa ghi chú không đổi bất cứ con số nghiệp vụ nào — ĐẠT
- A-840 · Người được nạp bằng Excel có ghi chú rỗng; file mẫu không có cột ghi chú — ĐẠT
- A-841 · Ba file Excel xuất ra không có cột ghi chú và không mang nội dung ghi chú — ĐẠT

Endpoint mới `PUT /api/PartyMembers/{id}/Note` cũng được thêm vào vòng quét A-001, nên từ nay
nó nằm trong ca "gọi mọi endpoint khi chưa đăng nhập phải bị từ chối".

Ca A-906 (bảng khóa thông điệp của QC phải trùng khít mục 1.5 hợp đồng API) bắt đúng việc hợp
đồng v1.6 thêm `Mes.PartyMember.OverLength.Note`; đã chép khóa sang bộ kiểm thử và ca chuyển xanh.

## 4. Luồng mới E2E-8 — `FE/e2e/specs/e8-ghi-chu.spec.ts`

Trình duyệt thật, Backend và PostgreSQL thật, đồng hồ hai đầu cùng đóng băng ở 19/09/2026.
Bảy bước, tất cả ĐẠT.

- E8-01 · Ghi chú từ nút ở bảng Đảng viên; trước khi ghi tooltip là "Thêm ghi chú", sau khi ghi
  nút đổi sang nền vàng và tooltip nêu đúng ngày ghi — ĐẠT
- E8-02 · Dashboard hiện khối "Ghi chú · 1 người", đứng sát bên trái nút Xuất Excel, bảng không
  mất dòng nào — ĐẠT
- E8-03 · Hộp xem đầy đủ mở đúng tiêu đề, giữ xuống dòng của người viết; ô tìm lọc đúng, không
  khớp thì báo rõ, đóng rồi mở lại thì ô tìm trống — ĐẠT
- E8-04 · Sửa nội dung ghi chú trong form Sửa thì ngày ghi đổi — ĐẠT
- E8-05 · Sửa họ tên mà giữ nguyên ghi chú thì ngày ghi không đổi — ĐẠT
- E8-06 · Xóa ghi chú thì nút về dạng viền và khối gọn trên Dashboard biến mất — ĐẠT
- E8-07 · Cả vòng ghi chú không đổi tổng người, đợt sắp tới, số đủ điều kiện, phân bổ theo mốc
  hay badge — ĐẠT

**Không phụ thuộc ngày chạy.** Mốc đóng băng giữ nguyên phần NGÀY suốt lần chạy, nên màn hình
không nói được "ngày ghi có đổi hay không" — hai bước E8-04 và E8-05 vì vậy đối chiếu giá trị
đầy đủ lấy từ máy chủ. Hợp đồng trả ngày ghi ở độ chính xác giây, nên trước hai bước đó luồng
đợi đồng hồ máy chủ sang giây mới bằng cách ghi thử lên một người đứng ngoài danh sách đủ điều
kiện; đây là chờ theo điều kiện trên trạng thái thật, không phải chờ một quãng cố định.

Ảnh bằng chứng bảy bước chính dựng lại được bằng `node e2e/scripts/chup-anh-e8.mjs`.

## 5. Hồi quy

| Bộ | Kết quả |
|---|---|
| Toàn bộ `dotnet test` | 780 đạt · 1 bỏ qua · 0 hỏng |
| `npm run typecheck:e2e` | xanh |
| `npm run build` | xanh |
| E2E-1 → E2E-7 và ca tiền đề E0 | tất cả đạt |
| E-905 và E-906 (ca về chính bộ kiểm thử) | đạt, và đã mở rộng để quét cả E2E-7 lẫn E2E-8 |
| `npm run test:ui` | 187 đạt · 1 hỏng (QC-T75-02) |

## 6. Phát hiện

### QC-T75-01 — ô tìm của màn Đảng viên thỉnh thoảng nuốt chữ vừa gõ · Nhẹ · đã sửa

1. **Quy tắc bị vi phạm:** không vi phạm quy tắc nghiệp vụ nào — đây là lỗi của bộ kiểm thử.
2. **Ca kiểm thử:** E3-09.
3. **Bước tái hiện:** chạy cả bộ E2E một lượt ở mốc T0. Lượt chạy 19:15 ngày 30/09/2026, E3-09 đỏ
   ở bước tìm "Hồ Thị Vân": ô tìm có chữ nhưng bảng vẫn giữ nguyên 32 dòng suốt 15 giây.
4. **Kết quả mong đợi:** bảng lọc còn đúng một dòng.
5. **Kết quả thực tế:** dòng đầu vẫn là "Bùi Thị Hoa". Chạy riêng E2E-3 lại thì xanh 3/3 lần.
6. **Nguyên nhân:** hàm dùng chung `searchMember` kiểm ô có giữ được chữ không, rồi mới chờ bảng.
   Màn Đảng viên còn dựng lại một nhịp sau khi chuyển menu; nhịp đó xảy ra ngay SAU lúc kiểm ô thì
   chữ bị nuốt mà không còn ai gõ lại.
7. **Đã sửa:** gộp cả ba việc — gõ, kiểm ô, kiểm bảng — vào cùng một vòng thử lại
   (`FE/e2e/fixtures/app.ts`). Chạy lại cả bộ: 14 đạt · 2 bỏ qua · 0 hỏng.

### QC-T75-02 — ca `ca 5` của bộ ui-tests đo kích thước lúc hộp thoại còn đang chạy hoạt ảnh · Nhẹ · bàn lại cho Frontend

1. **Quy tắc bị vi phạm:** không vi phạm quy tắc nghiệp vụ nào. Sản phẩm đúng mục "Giao diện đã
   duyệt" của HUYH-82 — xem mục 5 bằng chứng dưới đây.
2. **Ca kiểm thử:** `FE/ui-tests/specs/t74-ghi-chu.spec.ts` — "ca 5 · bỏ hai dòng chữ dưới hai ô
   ngày, Ngày sinh và Giới tính cùng hàng, không cuộn".
3. **Bước tái hiện:** `cd FE && npm run test:ui`, lặp lại vài lần.
4. **Kết quả mong đợi:** hai nhãn "Ngày sinh" và "Giới tính" lệch nhau không quá 1 px.
5. **Kết quả thực tế:** chập chờn — bốn lần chạy cho 1,08 px · đạt · 13,50 px · 13,53 px. Ảnh chụp
   lúc đỏ cho thấy hộp thoại còn đang phóng to dần, chữ chồng bóng. Đo lại trên hệ thống thật ở
   khung 1366×650 sau khi hoạt ảnh dừng: **|Δy| = 0,000 px** suốt 12 lần đo liên tiếp, và ô Giới
   tính nằm đúng bên phải ô Ngày sinh, cách 264 px. Nghĩa là bố cục sản phẩm đúng; ca kiểm thử đo
   quá sớm.
6. **Mức độ:** Nhẹ. Không chặn phát hành, nhưng để nguyên thì bộ ui-tests đỏ ngẫu nhiên.
7. **Đề xuất:** chờ hộp thoại đạt `opacity: 1` và hết biến hình rồi mới đọc `boundingBox()`.
   `FE/ui-tests/**` thuộc T74 nên QC không tự sửa; CEO giao lại cho Frontend.

> Ghi chú: chính lỗi này cũng làm hỏng đợt chụp ảnh E2E-8 đầu tiên — hai ảnh liên tiếp trùng khít
> từng điểm ảnh vì Playwright coi phần tử `opacity: 0` là đã hiện. `e2e/scripts/chup-anh-e8.mjs`
> đã chờ lớp phủ hết hoạt ảnh trước khi bấm máy.

## 7. Kết luận

**ĐẠT.** QT12 và mục "Giao diện đã duyệt" của HUYH-82 được thực hiện đúng ở cả ba tầng:

- Luật cắt khoảng trắng, trần 500 ký tự và cách đóng ngày ghi giống nhau trên cả ba đường ghi.
- Ngày ghi chỉ nhảy khi nội dung đổi, giữ nguyên khi chỉ sửa trường khác hoặc lưu lại y hệt, và
  về rỗng cùng lúc với ghi chú.
- Ghi chú không đụng vào bất cứ con số nào của QT1–QT11, không vào file Excel xuất ra, và người
  nạp bằng Excel có ghi chú rỗng.
- Ba màn dùng chung một kiểu viên ngày ghi; khối gọn và hộp xem đầy đủ chạy đúng trên Dashboard.

Không có lỗi mức Chặn hay Nặng. Một việc nhẹ bàn lại cho Frontend: QC-T75-02.
