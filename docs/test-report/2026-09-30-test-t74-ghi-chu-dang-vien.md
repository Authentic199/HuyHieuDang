# T74 — Frontend: ghi chú đảng viên (QT12)

Nhánh `feat/T74-fe-ghi-chu`, tách từ `origin/main` tại `7ebadf6`.

Dựng theo mục "Giao diện đã duyệt" của HUYH-82 sau các sửa đổi của chủ dự án, và theo hợp đồng API **v1.6** ở PR #73 (nhánh `docs/T72-ghi-chu-hop-dong`). Backend (T73) làm song song nên chưa có API thật: mọi thứ dưới đây kiểm bằng tầng dữ liệu giả của `FE/ui-tests`. Kiểm với API thật là việc của QC (T75).

## Đã làm những gì

### Kiểu dữ liệu và lớp gọi API

- `src/types/domain.ts` — `PartyMemberResponse` và `EligibleMemberResponse` thêm `note: string | null` và `noteUpdatedAt: IsoDateTime | null`.
- `src/api/members.ts` — `MemberPayload` thêm `note`; thêm `updateMemberNote(id, note)` gọi `PUT /api/PartyMembers/{id}/Note`.
- `src/api/messages.ts` — thêm khóa `Mes.PartyMember.OverLength.Note`.
- `src/utils/note.ts` (mới) — `NOTE_MAX_LENGTH`, `noteDateLabel`, `notedMembersOf`, `noteSummaryText`.

**Ngày ghi không đi qua `dayjs`.** `noteUpdatedAt` là ngoại lệ múi giờ duy nhất của hợp đồng (mục 1.6 v1.6): máy chủ trả giờ Việt Nam kèm `+07:00`. `noteDateLabel` cắt nguyên phần `yyyy-MM-dd` đầu chuỗi bằng biểu thức chính quy, nên máy đặt sai múi giờ vẫn hiện đúng ngày máy chủ đã đóng. Ca 15 kiểm đúng chỗ này bằng một ghi chú viết lúc 01:00 giờ Việt Nam.

### Thành phần dùng chung — `src/components/notes/`

| Tệp | Việc |
|---|---|
| `NoteDatePill.tsx` | Viên vàng `Ghi ngày dd/MM/yyyy`. **Một** kiểu cho mọi nơi, kể cả ghi chú năm trước |
| `NoteField.tsx` | Ô nhập nhiều dòng + viên ngày ghi + bộ đếm `x / 500`. Dùng chung cho hộp ghi chú và form Thêm/Sửa |
| `NoteSummary.tsx` | Khối gọn `Ghi chú · N người` ở hàng đầu thẻ danh sách |
| `NotesDialog.tsx` | Hộp xem đầy đủ, có ô tìm |
| `HighlightedText.tsx` | Tô sáng phần chữ khớp ô tìm |
| `notes.css` | Màu, cỡ chữ và chống tràn cho cả năm thành phần trên |

Bộ đếm tự dựng chứ không dùng `showCount` của Ant Design: `showCount` đặt số đếm ở một góc riêng, không xếp cùng hàng với viên ngày ghi được.

### Màn Đảng viên (UC-20, UC-26)

Cột Thao tác có thêm nút ghi chú, **bên trái** nút Sửa; bề rộng cột 88 → 132 px và `--hhd-table-min-width` 1210 → 1254 px.

- Đã có ghi chú: nền vàng nhạt, viền vàng, biểu tượng đặc, tooltip `Ghi chú · ghi ngày dd/MM/yyyy`.
- Chưa có: biểu tượng viền, tooltip `Thêm ghi chú`.
- `aria-label` luôn là `Ghi chú <Họ tên>`.

Hộp `Ghi chú — <Họ tên>` (`MemberNoteModal.tsx`) gọi `PUT /PartyMembers/{id}/Note`, **không** gọi `PUT /PartyMembers/{id}`: hộp không bày Họ tên, Ngày sinh, Giới tính, Ngày vào Đảng nên không được gửi lại bốn trường người dùng không nhìn thấy. Xóa hết chữ rồi Lưu thì gửi `note: null`. Lỗi máy chủ hiện bằng `Alert` ngay trong hộp, giống form Thêm/Sửa.

### Form Thêm/Sửa đảng viên (UC-21, UC-22)

- Bỏ hẳn hai dòng `extra` dưới ô "Ngày vào Đảng (dự bị)" và ô "Ngày sinh".
- Ngày sinh và Giới tính vào chung một `Row` hai cột `span={12}`.
- Ô Ghi chú ở cuối form, `maxLength` 500, có bộ đếm; sửa người đã có ghi chú thì hiện viên ngày ghi.
- `note` đi vào `MemberPayload`: `PUT` thay trọn, không gửi `note` là **xóa** ghi chú (mục 3.4).

**Chọn `Segmented block` cho ô Giới tính**, không đổi sang Radio dạng nút. Lý do: (1) `Segmented` đã là thứ artboard vẽ, đổi sang Radio là đổi cả dáng một ô mà chủ dự án chỉ yêu cầu cân lại bố cục; (2) `block` chia đều ba lựa chọn theo bề ngang nửa hàng — ca 5 đo ba mục lệch nhau ≤ 1 px; (3) `size="large"` lấy đúng token `controlHeight` của Ant Design nên cao **bằng** ô ngày bên trái ở cả thang thường lẫn thang gọn — ca 5 đo chênh lệch chiều cao và chênh lệch tọa độ `y` đều ≤ 1 px. Radio dạng nút phải tự đặt bề rộng từng nút và tự chỉnh chiều cao, tức là viết cứng số ở một chỗ mà thang gọn không với tới.

### Khối gọn và hộp đầy đủ (UC-11, UC-34)

Một component `NoteSummary` đặt ở hai chỗ, sát bên trái nút Xuất Excel:

- `dashboard/EligibleTableCard.tsx` — hàng đầu thẻ.
- `periods/detail/EligibilityTab.tsx` — thanh công cụ.

Khối nhận **trọn** `members` máy chủ trả, không nhận `pageRows` đã qua ô tìm và ô lọc — nó nói về cả đợt và năm đang xem. Không ai có ghi chú thì không dựng khối.

Khối cao đúng `var(--hhd-control-height)` nên hàng đầu thẻ không cao thêm; ca 16 đo chiều cao hàng đầu thẻ và đếm số dòng bảng nằm trọn trong vùng nhìn thấy ở 1366×650, có khối và không có khối phải bằng nhau.

Hộp đầy đủ: tiêu đề `Ghi chú — <Tên đợt> năm <năm>`, **không** có dòng đếm người; ô tìm đứng ngoài vùng cuộn; lọc bằng chính `matchesKeyword` của `hooks/useClientTable` nên khớp y như ô tìm của bảng; tô sáng bằng cùng một phép so khớp. Hộp chỉ được dựng khi thật sự mở, nên đóng rồi mở lại là ô tìm tự trống.

### Sửa sau lượt gác cổng đầu (CEO trả lại ngày 30/09)

**Nhãn "Để trống" bị cắt ở thang chữ 16px.** Ba ô chia đều nửa hàng nên mỗi ô chỉ còn 81 px; đệm mặc định 11 px mỗi bên của `.ant-segmented-item-label` ăn mất 22 px, để lại 59 px cho chữ trong khi nhãn dài nhất cần 67 px ở cỡ 16px — Ant Design cắt thành "Để tr…". Ở thang gọn (chữ 14px) vừa khít 81 = 81 nên không lộ.

Thu đệm còn 4 px, khai trong `MembersPage.css` và buộc vào hộp bằng `className="hhd-member-form"`. **Không đổi dáng**: ba ô đã chia đều bằng `block` và chữ căn giữa, nên đệm chỉ là lề bên trong ô; giữ nguyên bề ngang hộp 560 px của artboard, giữ nguyên hai ô cùng hàng, ô Giới tính vẫn cao bằng ô ngày (đo được 40 px ở thang gọn và 48 px ở thang thường, bằng đúng ô ngày ở cùng khung).

**Khối gọn đẩy nút Xuất Excel xuống dòng ở 1280×600.** Thanh công cụ của Chi tiết đợt là `flex-wrap: wrap`, và flexbox chọn chỗ xuống dòng theo bề ngang **gốc** của từng ô chứ không theo bề ngang sau khi co. Ở 1280 px, phần lòng thanh rộng 1146 px trong khi tổng bề ngang gốc là 146 + 327 + 520 + 116 + 48 px khoảng cách = 1157 px — dư 11 px nên nút rơi xuống dòng hai, thanh cao 109 px thay vì 61 px và bảng mất một dòng.

Hạ bề ngang mong muốn của khối từ 520 px xuống **420 px** (`flex: 0 1 420px`, `min-width: 160px`). Tổng còn 1057 px, dư 89 px — đủ chỗ cả khi menu trái đang mở rộng. Dashboard không bị lỗi này (thanh không `wrap`) nhưng dùng chung một lớp nên cũng theo cỡ mới.

### Chống tràn

`.hhd-note-text` dùng `white-space: pre-wrap` và `overflow-wrap: anywhere`; khối gọn cắt bằng `text-overflow: ellipsis`.

## Giữ nguyên, đã kiểm lại

- Màn "Chưa thuộc đợt nào" không hiện ghi chú, dù `UnassignedMemberResponse` kế thừa hai trường mới.
- Ô tìm ở màn Đảng viên vẫn chỉ theo họ tên.
- Excel xuất và Import 4 cột không bị chạm.
- Ghi chú không tham gia tính toán nào: không lọc, không sắp, không đổi `partyAgeYears`, mốc, danh sách đủ điều kiện hay cảnh báo.

## Tệp KHÔNG chạm, theo phân quyền của HUYH-82 và hai bổ sung 30/09

- PR #71 (HUYH-80): `PeriodDetailPage.tsx`, `PeriodsPage.tsx`, `DashboardAlerts.tsx`, `CoverageWarningBanner.tsx`, `utils/overlapWarning.ts`.
- HUYH-87 (T76, chưa có PR): `PeriodDetailHeader.tsx`, `YearPicker.tsx`, `YearPicker.css`, `t66-bo-chon-nam.spec.ts`, `t67-cheo-luot-2.spec.ts`, `e2e/specs/e7-dot-vat-qua-nam.spec.ts`. Trong `PeriodDetail.css` không đụng luật `.hhd-period-detail__range`; luật mới thêm ở cuối khối thanh công cụ. `tokens.css` và `responsive.css` không bị chạm.
- HUYH-89 (T78, PR #74 đang mở): `CoverageStrip.tsx`, `CoverageStrip.css`, `PeriodsPage.tsx`, `PeriodsPage.css`, `t64-man-dot.spec.ts`, `t78-man-dot-mau-dai-va-ten-cot.spec.ts`.

`FE/e2e/specs/**` **không** phải sửa: `e6-vong-doi-dang-vien.spec.ts` định vị ô trong form bằng `getByLabel('Họ tên' | 'Ngày sinh' | 'Ngày vào Đảng (dự bị)')` và chọn giới tính bằng `getByText('Nam' | 'Nữ', { exact: true })` — cả hai cách đều không phụ thuộc vào việc hai ô nằm cùng hàng hay hai hàng, và không phụ thuộc hai dòng `extra` đã bỏ.

## Bộ kiểm thử

### Dữ liệu giả

`FE/ui-tests/fixtures/data.ts` — bốn ghi chú gắn theo **chỉ số trước khi sắp**, nên chạy lại là đúng những người đó:

| Người | Ghi chú | Ngày ghi |
|---|---|---|
| 0 | Ba dòng, có xuống dòng thật | năm nay |
| 1 | Một đường dẫn file dài, **không có dấu cách** | năm nay |
| 2 | Một câu ngắn | **năm trước** |
| 3 | Một câu ngắn | năm nay |

Thêm `NOTED_COUNT`, `noteDateTextOf`, `notedRowsOf` để ca kiểm thử tính ra con số mong đợi, không viết cứng.

`FE/ui-tests/fixtures/app.ts` — `/PartyMembers` trả kèm `note` và `noteUpdatedAt`; thêm route giả cho `PUT /PartyMembers/{id}/Note`. Route đó cố ý **không** ghi vào `mockData`: các ca chạy song song dùng chung mảng đó.

### Ca mới `FE/ui-tests/specs/t74-ghi-chu.spec.ts` đơn + 2 ca chạy ở bốn khung (24 lần chạy)

| Ca | Kiểm |
|---|---|
| 1 | Nút ghi chú hai trạng thái: lớp CSS, biểu tượng, vị trí bên trái nút Sửa, hai tooltip |
| 2 | Hộp một người: tiêu đề, nội dung, viên ngày ghi, bộ đếm; đếm tới 500 rồi ký tự 501 không vào được |
| 3 | Lưu gọi đúng `PUT …/Note`, thân là `{ note: "…" }` đã cắt khoảng trắng, báo thành công, hộp đóng |
| 4 | Xóa hết chữ rồi Lưu gửi `{ note: null }` |
| 5 | 1366×650 — không còn `.ant-form-item-extra`; hai nhãn Ngày sinh / Giới tính thẳng hàng; hai ô cao bằng nhau; ba lựa chọn chia đều; bộ đếm và viên ngày ghi; `.ant-modal-body` và `.ant-modal-wrap` **không cuộn** |
| 6 | 1366×650 — form Thêm cũng không cuộn, bộ đếm `0 / 500`, chưa có viên ngày ghi |
| 7 | Dashboard: khối gọn đúng số người, dòng 2 bắt đầu bằng `Họ tên: `, không còn ký tự xuống dòng, nằm sát bên trái nút Xuất Excel, không cao hơn nút |
| 8 | Không ai có ghi chú thì không có khối |
| 9 | Chi tiết đợt dùng đúng khối đó, cùng vị trí |
| 10 | Hộp đầy đủ: tiêu đề, **không** có dòng đếm người, mọi mục có viên vàng, thứ tự Mốc rồi Họ tên |
| 11 | Ô tìm: lọc theo họ tên, lọc theo nội dung, không phân biệt hoa thường, tô sáng, câu "không khớp"; đóng rồi mở lại thì ô tìm trống |
| 12 | Ô tìm nằm ngoài vùng cuộn; cuộn danh sách tới đáy mà ô tìm không nhúc nhích |
| 13 | `scrollWidth` của mọi khối toàn văn không vượt `clientWidth`; vùng cuộn không sinh thanh cuộn ngang; `white-space: pre-wrap` |
| 14 | Lọc bảng về một người, khối gọn và hộp đầy đủ vẫn nói về cả đợt |
| 15 | Ghi chú viết lúc 01:00 giờ Việt Nam vẫn hiện đúng ngày máy chủ, không lùi một ngày |
| 16 | 1366×650 — hàng đầu thẻ không cao thêm và số dòng bảng nhìn thấy không đổi khi có khối gọn |
| 17 | **Chạy ở cả bốn khung T60** — ở form Thêm **và** form Sửa: ba nhãn Giới tính đúng chữ và `scrollWidth ≤ clientWidth`; ba ô vẫn chia đều; ô Giới tính vẫn cao bằng và thẳng hàng với ô Ngày sinh |
| 18 | **Chạy ở cả bốn khung T60** — Dashboard và Chi tiết đợt: hàng đầu thẻ cao bằng nhau khi có và khi không có khối; khối vẫn cùng hàng và sát bên trái nút Xuất Excel |

### Ảnh chụp

`FE/ui-tests/scripts/chup-anh-t74.spec.ts` + `FE/ui-tests/playwright.anh.config.ts` — chụp 14 ảnh làm bằng chứng, không kiểm hành vi. Tách khỏi bộ `playwright.ui.config.ts` để lần chạy kiểm thử thường không phải chụp ảnh.

Chạy: `T50_PORT=4185 npx playwright test -c ui-tests/playwright.anh.config.ts`

## Lệnh đã chạy

Xem phần "Định nghĩa hoàn thành" trong comment kết thúc của HUYH-85 — mỗi lệnh kèm dòng tổng kết nguyên văn.

## Vì sao ca 5 và ca 16 không bắt được hai lỗi trên

Cả hai chỉ chạy ở **một** khung 1366×650. Ở khung đó chữ là 14px nên nhãn Giới tính vừa khít, và thanh công cụ còn đủ chỗ nên không xuống dòng. Ca 17 và ca 18 lấy thẳng `RESPONSIVE_VIEWPORTS` từ `playwright.ui.config.ts` nên chạy đủ bốn khung T60 và không bao giờ lệch với bộ T60.

Đã kiểm ngược: hoàn nguyên hai đoạn CSS vừa sửa rồi chạy lại hai ca này thì **ca 17 đỏ ở 1440×900 và 3440×1440, ca 18 đỏ ở 1280×600**, xanh ở các khung còn lại — trùng khít chỗ CEO đo được.
