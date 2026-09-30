# HuyHieuDang — Tài liệu nghiệp vụ (v1)

| | |
|---|---|
| Dự án | HuyHieuDang — Hệ thống hỗ trợ xét trao Huy hiệu Đảng |
| Phiên bản tài liệu | 1.3 — 30/09/2026 (thêm ghi chú đảng viên — QT12, UC-26) |
| Loại tài liệu | Tài liệu nghiệp vụ (BRD/SRS rút gọn), đầu vào cho thiết kế UI và chia task |
| Trạng thái | Đã thống nhất với chủ dự án |
| Tài liệu liên quan | `2026-09-17-ui-design-prompt.md`, `design-system/Huy Hieu Dang - 9 man hinh.html` |

**Nhật ký thay đổi**
- v1.3 (30/09/2026): thêm tính năng **Ghi chú đảng viên** do chủ dự án yêu cầu ngày 30/09/2026. Bổ sung **QT12** (mỗi đảng viên tối đa một ghi chú 500 ký tự, có ngày ghi do hệ thống đóng). Mục 1.4 "Trong phạm vi" thêm mục 8. **UC-20**: cột Thao tác thêm nút ghi chú. **UC-21, UC-22**: modal thành 5 trường, bỏ hai dòng chữ dưới hai ô ngày, Ngày sinh và Giới tính cùng một hàng, thêm ô Ghi chú có bộ đếm, form hiện trọn ở 1366×650. **UC-26 mới**: hộp Ghi chú của một đảng viên. **UC-11, UC-34**: thêm khối gọn `Ghi chú · N người` và hộp xem đầy đủ có ô tìm; **UC-40 không** hiện ghi chú. Mục "Xuất Excel" ghi rõ không có ghi chú. Mục 5 PartyMember thêm `Note`, `NoteUpdatedAt`. Cập nhật Phụ lục C.2.
- v1.2 (30/09/2026): ghi các quyết định 30/09 của chủ dự án. UC-30: màn Đợt bỏ dòng tóm tắt dưới tiêu đề, bảng bỏ tô nền dòng theo trạng thái. UC-34: trang chi tiết đợt gộp thành một trang, không còn tab; bộ chọn năm đổi sang một ô năm `‹ 2026 ›` có bảng chọn năm, giới hạn năm hiện tại ± 100 năm. UC-40 dùng cùng bộ chọn năm đó. Cập nhật sơ đồ màn hình và Phụ lục C.2 cho khớp.
- v1.1 (19/09/2026): đối chiếu với bản thiết kế UI; bổ sung QT3a, QT11; bổ sung UC-13, UC-36, UC-51; thêm trường Tên đơn vị; chi tiết hóa ràng buộc file import; thêm Phụ lục C (đối chiếu UI ↔ use case).
- v1.0 (17/09/2026): bản đầu tiên, thống nhất qua brainstorm.

---

## 1. Tổng quan

### 1.1 Vấn đề
Cán bộ phụ trách công tác đảng viên đang lọc tay danh sách đảng viên đủ tuổi đảng cho từng đợt trao huy hiệu trên Excel. Việc này lặp lại nhiều lần trong năm (thường 4 đợt), dễ sót người, dễ tính sai mốc tuổi đảng.

### 1.2 Mục tiêu
Từ danh sách đảng viên đã import, hệ thống **tự động tính** ai tròn mốc tuổi đảng trong khoảng ngày của mỗi đợt, hiển thị và xuất ra Excel. Người dùng chỉ còn 3 việc:
1. Nạp danh sách đảng viên.
2. Cài các đợt trao huy hiệu trong năm (làm một lần, dùng nhiều năm).
3. Mở lên xem và xuất danh sách.

### 1.3 Người dùng và bối cảnh
- **1 người dùng** (cán bộ văn phòng đảng ủy / chi bộ), dùng trên **1 máy**.
- Đăng nhập bằng **1 tài khoản admin có sẵn**. Không có quản lý người dùng, không phân quyền.
- Ứng dụng web chạy cục bộ (localhost), mở bằng trình duyệt.

### 1.4 Phạm vi phiên bản 1

**Trong phạm vi**
1. Đăng nhập / đăng xuất.
2. Quản lý đảng viên: import Excel, thêm / sửa / xóa, tìm kiếm.
3. Quản lý đợt trao huy hiệu (danh sách dùng chung mọi năm, CRUD).
4. Cài đặt mốc tuổi đảng (bắt đầu / kết thúc / bước nhảy).
5. Danh sách đủ điều kiện theo đợt, theo năm: xem + xuất Excel.
6. Danh sách "chưa thuộc đợt nào" trong năm: xem + xuất Excel.
7. Dashboard: đợt sắp tới + danh sách đủ điều kiện + cảnh báo.
8. Ghi chú cho từng đảng viên (QT12).

**Ngoài phạm vi** (có thể làm ở phiên bản sau)
- Quản lý user / role.
- Đánh dấu "đã trao", chốt danh sách đợt, lưu lịch sử trao.
- Trừ tuổi đảng do gián đoạn (xóa tên rồi kết nạp lại).
- Trao sớm (ốm nặng), truy tặng.
- Trạng thái đảng viên (chuyển đi, từ trần, xóa tên…).
- In tờ trình / quyết định.
- Chống trùng khi import.

---

## 2. Thuật ngữ

| Thuật ngữ | Định nghĩa |
|---|---|
| Ngày chính thức | Ngày đảng viên được công nhận đảng viên chính thức. Là mốc tính tuổi đảng. |
| Tuổi đảng | Số năm tròn tính từ Ngày chính thức đến hôm nay. |
| Mốc huy hiệu (mốc) | Số năm tuổi đảng được trao huy hiệu (VD 30, 35, 40 …). Sinh ra từ cài đặt. |
| Ngày tròn mốc | Ngày chính thức + N năm, với N là một mốc. |
| Đợt trao huy hiệu (đợt) | Một khoảng ngày (chỉ ngày/tháng, không gắn năm) dùng để gom những người tròn mốc trong khoảng đó. Khoảng này được phép vắt qua 31/12 sang năm sau. |
| Đủ điều kiện | Đảng viên có Ngày tròn mốc rơi vào khoảng ngày của một đợt trong một năm cụ thể. |
| Chưa thuộc đợt nào | Đảng viên tròn mốc trong năm nhưng Ngày tròn mốc không rơi vào đợt nào. |
| Ghi chú | Một đoạn chữ tự do, tối đa 500 ký tự, gắn với một đảng viên. Mỗi người tối đa một ghi chú (QT12). Luôn gọi là **ghi chú**, không gọi "lưu ý", "nhận xét" hay "chú thích". |
| Ngày ghi | Thời điểm nội dung ghi chú được sửa lần cuối, do hệ thống tự đóng. Hiển thị `Ghi ngày dd/MM/yyyy`. Luôn gọi là **ngày ghi**, không gọi "ngày cập nhật" hay "ngày sửa". |

---

## 3. Quy tắc nghiệp vụ

Mọi màn hình đều dựa trên các quy tắc dưới đây. Ký hiệu: `D` = Ngày chính thức, `N` = mốc, `Y` = năm đang xét.

**QT1 – Dãy mốc huy hiệu.** Từ cài đặt (Bắt đầu, Kết thúc, Bước; mặc định 30 / 90 / 5) sinh ra dãy `{Bắt đầu, Bắt đầu + Bước, …}` cho đến khi vượt Kết thúc thì dừng. Ràng buộc: cả 3 là số nguyên dương, Bắt đầu ≤ Kết thúc, Bước ≥ 1.
Ví dụ: 30 / 90 / 5 → 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90. Đổi Bước = 10 → 30, 40, 50, 60, 70, 80, 90.

**QT2 – Ngày tròn mốc.** `Ngày tròn mốc(D, N) = D + N năm` — cùng ngày, cùng tháng, năm cộng thêm N. Nếu D là 29/02 và năm đích không nhuận thì lấy 28/02.

**QT3 – Tuổi đảng hiện tại.** Số năm tròn từ `D` đến hôm nay; chỉ tính năm thứ k khi đã qua (hoặc đúng) ngày kỷ niệm thứ k. Chỉ dùng để hiển thị trong danh sách đảng viên; không tham gia xét đợt.

**QT3a – Mốc kế tiếp.** Mốc nhỏ nhất trong dãy QT1 lớn hơn tuổi đảng hiện tại. Nếu tuổi đảng đã vượt mốc lớn nhất (hoặc không còn mốc nào phía trước) → hiển thị `—` ở cả cột Mốc kế tiếp và Ngày tròn mốc kế tiếp. Chỉ dùng để hiển thị.

**QT4 – Đủ điều kiện trong đợt, theo năm.** Với đợt `Đ` có Từ ngày `f` (ngày/tháng) và Đến ngày `t` (ngày/tháng), và năm `Y`: gắn năm để có `F = f/Y`; `T = t/Y` nếu đợt nằm gọn trong một năm, `T = t/(Y+1)` nếu đợt **vắt qua 31/12** (khi cặp `t` đứng trước cặp `f` trong vòng năm, ví dụ 01/12 – 28/02). 29/02 ở năm không nhuận → 28/02. Đảng viên đủ điều kiện khi **tồn tại mốc `N`** trong dãy QT1 sao cho `F ≤ D + N năm ≤ T`. Mốc `N` đó là huy hiệu được trao. Vì các mốc cách nhau ≥ 1 năm và một đợt luôn ngắn hơn 1 năm, mỗi người có tối đa 1 mốc trong 1 đợt.

Năm `Y` của một đợt luôn là **năm chứa Từ ngày**. "Đợt Giao thừa 2026" nghĩa là lần diễn ra 01/12/2026 – 28/02/2027.

**QT5 – Không lưu kết quả.** Danh sách đủ điều kiện được tính lại mỗi lần xem. Thêm người, sửa ngày, sửa đợt, đổi cài đặt → danh sách tự cập nhật, không cần thao tác gì thêm.

**QT6 – Đợt trao huy hiệu.** Một danh sách duy nhất, dùng chung cho mọi năm. Mỗi đợt có Tên (duy nhất), Từ ngày, Đến ngày — chỉ lưu ngày/tháng. Ràng buộc còn lại: tên không rỗng, không trùng; ngày/tháng phải có thật (29/02 hợp lệ).
Đợt **được phép vắt qua 31/12**: khi cặp Đến ngày đứng trước cặp Từ ngày trong vòng năm (ví dụ 01/12 – 28/02) thì Đến ngày thuộc năm kế tiếp. Không còn ràng buộc `Từ ≤ Đến`. Sửa đợt có hiệu lực ngay cho mọi năm, kể cả năm hiện tại.
Khi xét độ phủ của một năm `Y`, một đợt vắt năm để lại **hai phần** trong năm đó: đuôi của lần neo năm `Y-1` (từ 01/01) và đầu của lần neo năm `Y` (tới 31/12).
Hệ thống **cảnh báo nhưng vẫn cho lưu** khi: (a) hai đợt khác nhau chồng lấn nhau; (b) các đợt không phủ kín 01/01–31/12. Hai phần của cùng một đợt vắt năm không bị coi là chồng lấn.

**QT7 – Chưa thuộc đợt nào.** Với năm `Y`: đảng viên có ít nhất một mốc `N` sao cho `D + N năm` rơi trong năm `Y` nhưng không nằm trong **phần thuộc năm `Y`** của bất kỳ đợt nào — kể cả phần đuôi của đợt vắt năm neo ở `Y-1`.

**QT8 – Đợt sắp tới (Dashboard).** Xét mọi lần diễn ra neo ở năm nay và năm sau, thêm năm trước với đợt vắt năm (lần neo năm trước của nó có thể vẫn đang mở hôm nay); chọn lần có `Đến ≥ hôm nay` và `Từ` nhỏ nhất (nếu hôm nay đang nằm trong một lần diễn ra thì chính là lần đó). Không có đợt nào → Dashboard báo "Chưa cài đợt trao huy hiệu".

**QT9 – Import Excel.** Mọi dòng hợp lệ đều được **thêm mới**, không kiểm tra trùng. Dòng lỗi được liệt kê kèm số dòng và lý do; người dùng chọn "Nạp các dòng hợp lệ" hoặc "Hủy".
- Ràng buộc file: định dạng `.xlsx`, dung lượng ≤ 10 MB, đúng 4 cột theo thứ tự Họ tên · Ngày sinh · Giới tính · Ngày vào Đảng chính thức; dòng đầu là tiêu đề.
- Lỗi cấp dòng: thiếu Họ tên; thiếu Ngày chính thức; sai định dạng ngày (cần `dd/MM/yyyy`); Ngày chính thức ở tương lai; Giới tính khác Nam/Nữ khi có điền; Ngày sinh sau Ngày chính thức.
- Lỗi cấp file (chặn ngay bước 1): sai định dạng, vượt dung lượng, sai số cột, file rỗng.
- Việc nạp là **một giao dịch**: hoặc nạp hết các dòng hợp lệ, hoặc không nạp gì (khi có lỗi kỹ thuật giữa chừng).

**QT10 – Xóa.** Xóa đảng viên và xóa đợt là xóa hẳn, có hộp xác nhận. Không có thùng rác.

**QT11 – Trạng thái đợt trong năm hiện tại.** Gắn năm đang xét vào đợt rồi so với hôm nay: `Đến < hôm nay` → **Đã qua**; `Từ ≤ hôm nay ≤ Đến` → **Đang diễn ra**; `Từ > hôm nay` → **Sắp tới**, kèm số ngày còn lại.
Ngoại lệ cho đợt vắt năm: nếu lần diễn ra neo ở năm **trước** năm đang xét còn đang mở hôm nay thì trạng thái là **Đang diễn ra**. Nhờ vậy ngày 15/01 cán bộ đọc được "Đang diễn ra" chứ không phải "Sắp tới · 320 ngày" của lần kế tiếp. Dùng ở bảng danh sách đợt và thẻ Dashboard.

**QT12 – Ghi chú đảng viên.** (Quyết định 30/09/2026 của chủ dự án.)
1. Mỗi đảng viên có **tối đa một ghi chú**. Chữ tự do, được xuống dòng, không bắt buộc. Sửa ghi chú là **thay nội dung cũ**; không có sổ ghi chú nhiều dòng, không lưu lịch sử.
2. Máy chủ **cắt khoảng trắng ở hai đầu**. Rỗng sau khi cắt thì coi như **không có ghi chú**. Tối đa **500 ký tự** tính sau khi cắt.
3. **Ngày ghi** bằng thời điểm hiện tại của máy chủ mỗi khi nội dung ghi chú sau khi cắt **khác** nội dung đang lưu, kể cả từ không có sang có.
   - Nội dung không đổi (chỉ sửa trường khác của đảng viên, hoặc lưu lại y hệt) thì **ngày ghi giữ nguyên**.
   - Xóa hết ghi chú thì ghi chú và ngày ghi **cùng** về rỗng.
   - Người dùng **không sửa được** ngày ghi; hệ thống tự đóng. Hiển thị `Ghi ngày dd/MM/yyyy`.
4. **Ngày ghi dùng một kiểu viên vàng ở mọi nơi** — hộp xem đầy đủ, hộp ghi chú của một người, và form Thêm/Sửa. Không tô khác màu cho ghi chú của năm trước, vì năm đã nằm trong ngày.
5. **Ghi chú chỉ để đọc.** Không tham gia QT1–QT11: không đổi tuổi đảng, mốc kế tiếp, danh sách đủ điều kiện, nhãn trạng thái hay cảnh báo. Ghi chú **không phải** "trạng thái đảng viên" — mục đó vẫn ngoài phạm vi (mục 1.4).
6. **Ghi chú không xuất ra Excel.** Cả ba file xuất (UC-11, UC-34, UC-40) giữ nguyên bộ cột cũ.
7. **Import không nạp ghi chú.** File import vẫn đúng 4 cột; người được nạp mới có ghi chú rỗng, cán bộ ghi sau bằng UC-26.
8. Tìm kiếm ở màn Đảng viên (UC-20) **vẫn chỉ theo họ tên** — không tìm trong nội dung ghi chú. Ô tìm trong hộp xem đầy đủ (UC-11, UC-34) là chuyện khác: nó tìm trong chính hộp đó.

---

## 4. Chức năng theo module (use case)

Mỗi use case ghi: mục đích, dữ liệu trên màn hình, thao tác, kết quả. Dùng trực tiếp để phác UI.

### M0 – Đăng nhập
| UC | Tên | Mô tả |
|---|---|---|
| UC-00 | Đăng nhập | Form tài khoản / mật khẩu. Sai → thông báo chung "Sai tài khoản hoặc mật khẩu". Thành công → Dashboard. |
| UC-01 | Đăng xuất | Nút ở góc trên phải. Về màn hình đăng nhập. |

### M1 – Dashboard (trang mặc định sau đăng nhập)
| UC | Tên | Mô tả |
|---|---|---|
| UC-10 | Thẻ đợt sắp tới | Tên đợt, Từ–Đến ngày (đã gắn năm), còn bao nhiêu ngày (hoặc "đang diễn ra"), tổng số người đủ điều kiện, số người theo từng mốc (VD 30 năm: 3 · 40 năm: 1). |
| UC-11 | Bảng đủ điều kiện của đợt sắp tới | Cột: STT, Họ tên, Giới tính, Ngày sinh, Ngày chính thức, Ngày tròn mốc, Mốc huy hiệu. Sắp theo Mốc rồi Họ tên. Nút **Xuất Excel**.<br>**Khối ghi chú gọn** (QT12) nằm ở hàng đầu của thẻ danh sách, sát bên trái nút **Xuất Excel** — chỗ đang trống, nên bảng không mất dòng nào. Dòng 1: `Ghi chú · N người`. Dòng 2: nối các ghi chú dạng `Họ tên: nội dung`, phần thừa cắt bằng `…`. Nút mở rộng dạng icon, tooltip `Xem đầy đủ ghi chú`; bấm vào khối hay bấm nút đều mở **hộp xem đầy đủ**. Khối tính trên **cả** danh sách đủ điều kiện của đợt và năm đang xem, **không** phụ thuộc ô tìm hay ô lọc của bảng. Không ai có ghi chú thì **không hiện khối**.<br>**Hộp xem đầy đủ:** tiêu đề `Ghi chú — <Tên đợt> năm <năm>`, **không** có dòng đếm người. **Ô tìm** ở đầu hộp, đứng yên khi danh sách cuộn: tìm trong họ tên và nội dung ghi chú, so khớp giống ô tìm của bảng (không phân biệt hoa thường), gõ tới đâu lọc tới đó, tô sáng phần chữ khớp; không có gì khớp thì hiện `Không tìm thấy ghi chú nào khớp.`; đóng hộp rồi mở lại thì ô tìm trống. Mỗi mục gồm họ tên, thẻ mốc, **viên ngày ghi màu vàng** và toàn văn ghi chú. Thứ tự giống bảng (Mốc rồi Họ tên). Danh sách dài thì cuộn trong hộp. |
| UC-12 | Cảnh báo nhanh | Hiện khi: chưa có đảng viên nào; chưa có đợt nào; có người "chưa thuộc đợt nào" trong năm hiện tại (kèm số lượng, bấm vào để đi tới M4); các đợt chưa phủ kín (liệt kê các khoảng trống) / chồng lấn. |
| UC-13 | Hướng dẫn 3 bước (trạng thái trống) | Khi chưa có dữ liệu, thay bảng bằng khối hướng dẫn: 1 Kiểm tra cài đặt mốc → 2 Tạo các đợt trong năm → 3 Nạp danh sách đảng viên, mỗi bước có nút đi thẳng tới màn tương ứng. |

**Khung chung mọi màn** (áp dụng từ M1 đến M5):
- Sider trái dạng thanh biểu tượng (thu gọn được), 5 mục. Mục "Chưa thuộc đợt nào" có **badge** hiển thị số người bị sót trong năm hiện tại; không có ai thì ẩn badge.
- Header: tên hệ thống + **Tên đơn vị** (lấy từ Cài đặt), ngày hôm nay, tên tài khoản, nút Đăng xuất.

### M2 – Đảng viên
| UC | Tên | Mô tả |
|---|---|---|
| UC-20 | Danh sách | Dòng tóm tắt "N người · Tuổi đảng tính đến hôm nay". Cột: Họ tên, Giới tính, Ngày sinh, Ngày chính thức, Tuổi đảng (QT3), Mốc kế tiếp, Ngày tròn mốc kế tiếp (QT3a). Tìm theo tên (chứa chuỗi), lọc theo giới tính, sắp xếp theo cột, phân trang có chọn số dòng/trang. Chọn nhiều dòng để xóa; khi đang chọn hiện "Đang chọn N dòng". Ô trống hiển thị `—`. Tìm kiếm **chỉ theo họ tên**, không tìm trong ghi chú (QT12).<br>**Cột Thao tác có nút ghi chú** dạng icon, đặt **bên trái** nút Sửa (QT12). Người **có** ghi chú: nút nền vàng nhạt, icon đặc, tooltip `Ghi chú · ghi ngày dd/MM/yyyy`. Người **chưa có**: icon viền, tooltip `Thêm ghi chú`. Bấm nút mở hộp ghi chú của UC-26. |
| UC-21 | Thêm thủ công | Modal **5 trường**, bố cục từ trên xuống: **Họ tên** (bắt buộc) một hàng → **Ngày vào Đảng (dự bị)** một hàng (đây chính là Ngày chính thức, trường `officialAdmissionDate`; bắt buộc, ≤ hôm nay) → **Ngày sinh** và **Giới tính** (Nam/Nữ/để trống) **cùng một hàng**, mỗi ô một nửa → **Ghi chú** ở cuối form.<br>**Không** còn dòng chữ giải thích dưới ô "Ngày vào Đảng (dự bị)" và dưới ô "Ngày sinh" (quyết định 30/09/2026 — lệch so với ảnh dựng thử, đừng dựng lại). Ô Giới tính cao bằng ô ngày, ba lựa chọn chia đều bề ngang.<br>Ô **Ghi chú**: nhập nhiều dòng, bộ đếm `x / 500`, chặn gõ quá 500 ký tự (QT12).<br>Form phải **hiện trọn, không phải cuộn, ở khung 1366×650** (laptop của chủ dự án). |
| UC-22 | Sửa | Modal như UC-21, có sẵn dữ liệu. Sửa người đã có ghi chú thì dưới ô Ghi chú hiện **viên ngày ghi màu vàng** `Ghi ngày dd/MM/yyyy` (QT12); người dùng không sửa được ngày này. Lưu lại ghi chú y hệt thì ngày ghi không đổi. |
| UC-23 | Xóa | Một hoặc nhiều dòng đã chọn. Hộp xác nhận nêu rõ số người sẽ xóa. |
| UC-24 | Import Excel | Trang riêng, wizard 3 bước (Chọn file → Xem trước → Kết quả). **B1**: vùng kéo-thả, mô tả 4 cột kèm dòng ví dụ, ghi rõ `.xlsx` ≤ 10 MB và định dạng ngày, nút Tải file mẫu. **B2**: dòng tóm tắt "Sẽ thêm **N người mới** · M dòng lỗi bị bỏ qua" + cảnh báo "Hệ thống không kiểm tra trùng — nếu đã nạp file này trước đó, hãy Hủy"; hai tab Hợp lệ (N) / Lỗi (M); bảng lỗi có cột Dòng · Họ tên · Ngày sinh · Giới tính · Ngày chính thức · Lý do. **B3**: "Đã thêm N người, bỏ qua M dòng lỗi" + nút Về danh sách. |
| UC-25 | Tải file mẫu | File `.xlsx` có 4 cột đúng thứ tự: Họ tên · Ngày sinh · Giới tính · Ngày vào đảng chính thức. Có 1–2 dòng ví dụ. Ngày theo định dạng `dd/MM/yyyy`. **Không có cột ghi chú** (QT12). |
| UC-26 | Ghi chú của một đảng viên | Hộp mở từ **nút ghi chú** ở cột Thao tác của UC-20. Tiêu đề `Ghi chú — <Họ tên>`. Ô nhập nhiều dòng, bộ đếm `x / 500`, chặn gõ quá 500 ký tự. Dưới ô là **viên ngày ghi màu vàng** (chỉ hiện khi đã có ghi chú). Hai nút **Đóng** và **Lưu ghi chú**. Xóa hết chữ rồi bấm Lưu nghĩa là **xóa ghi chú**. Lưu xong: báo thành công và tải lại bảng. Hộp này **chỉ** đổi ghi chú, không đụng Họ tên, Ngày sinh, Giới tính, Ngày chính thức (QT12). |

### M3 – Đợt trao huy hiệu
| UC | Tên | Mô tả |
|---|---|---|
| UC-30 | Danh sách đợt | **Không có dòng tóm tắt dưới tiêu đề** (quyết định 30/09/2026 — lệch so với artboard 5, đừng dựng lại). Bảng sắp theo Từ ngày: Tên, Từ ngày (dd/MM), Đến ngày (dd/MM), **Trạng thái năm nay** (QT11), Số người đủ điều kiện **năm nay**, Thao tác. **Bảng không tô nền dòng theo trạng thái** — trạng thái chỉ hiện ở nhãn cột **Trạng thái năm nay** (quyết định 30/09/2026 — lệch so với artboard 5, đừng dựng lại). Banner cảnh báo chồng lấn / chưa phủ kín kèm liệt kê khoảng trống (QT6). Không có bộ chọn năm. |
| UC-36 | Dải độ phủ trong năm | Biểu đồ dải ngang 12 tháng phía trên bảng: mỗi đợt là một vạch màu, khoảng trống tô xám, có vạch đánh dấu "Hôm nay". Giúp nhìn ra ngay chỗ chưa phủ kín. |
| UC-31 | Thêm đợt | Modal: Tên, Từ ngày (dd/MM), Đến ngày (dd/MM). |
| UC-32 | Sửa đợt | Như UC-31. Dòng nhắc: "Thay đổi áp dụng ngay cho năm hiện tại và các năm sau". |
| UC-33 | Xóa đợt | Hộp xác nhận. |
| UC-34 | Đủ điều kiện của đợt | Trang chi tiết đợt là **một trang, không có tab** (quyết định 30/09/2026). Thứ tự trên trang: đường dẫn (breadcrumb); tiêu đề gồm tên đợt, khoảng ngày hằng năm ("01/10 – 07/11 hằng năm"; đợt vắt qua 31/12 ghi "01/12 – 28/02 năm sau, hằng năm") và hai nút **Sửa đợt**, **Xóa**; ngay dưới là thẻ danh sách đủ điều kiện. **Không còn khối "Thông tin"** — tiêu đề đã có đủ tên đợt và khoảng ngày. Thanh công cụ của thẻ: bộ chọn năm · khoảng ngày đã gắn năm · nhãn ngữ cảnh · số người · **Xuất Excel**. Bảng như UC-11.<br>**Bộ chọn năm** là một ô năm duy nhất `‹ 2026 ›`. Bấm `‹` `›` để lùi hoặc tiến một năm. Bấm vào ô năm thì mở **bảng chọn năm**: lưới 5 cột cuộn dọc, có nút **"Năm nay"** ở góc trái. Chỉ đi được trong khoảng **năm hiện tại ± 100 năm** (năm 2026 thì 1926 – 2126). Mặc định là **năm hiện tại của máy chủ**.<br>**Nhãn ngữ cảnh** cạnh khoảng ngày ghi đúng khoảng cách: **Năm nay** · **Năm sau** · **Năm trước** · **"N năm nữa"** (N ≥ 2) · **"N năm trước"** (N ≥ 2). Mọi năm tương lai ghi thêm "· chuẩn bị trước" ở số người.<br>**Khối ghi chú gọn và hộp xem đầy đủ: giống UC-11**, cùng vị trí (hàng đầu của thẻ, sát bên trái nút **Xuất Excel**). Khối tính trên cả danh sách đủ điều kiện của **đợt và năm đang xem**; đổi năm thì khối đổi theo (QT12). |

### M4 – Chưa thuộc đợt nào
| UC | Tên | Mô tả |
|---|---|---|
| UC-40 | Danh sách theo năm | Dùng **cùng bộ chọn năm như UC-34**: một ô năm `‹ 2026 ›`, bảng chọn năm khi bấm vào ô năm, giới hạn năm hiện tại ± 100 năm, mặc định năm hiện tại của máy chủ. Bảng như UC-11, thêm cột "Khoảng trống" ("Giữa Đợt A và Đợt B", "Trước đợt đầu tiên", "Sau đợt cuối cùng"). Khối gợi ý hành động: "Nới Đến ngày của đợt trước hoặc Từ ngày của đợt sau" + nút đi tới M3. Nút **Xuất Excel**. Trạng thái trống: "Không có ai bị sót trong năm YYYY." **Màn này không hiện ghi chú** — không có khối gọn, không có hộp xem đầy đủ, không có cột ghi chú (QT12, quyết định 30/09/2026). |

### M5 – Cài đặt
| UC | Tên | Mô tả |
|---|---|---|
| UC-50 | Cài mốc tuổi đảng | 3 ô số có nút −/+: Bắt đầu / Kết thúc / Bước. Xem trước dãy mốc sinh ra (QT1) ngay khi gõ, kèm số lượng mốc ("13 mốc"). Dòng nhắc: "Thay đổi ảnh hưởng ngay đến mọi danh sách đủ điều kiện · Áp dụng cho mọi năm và cho Dashboard". Nút **Khôi phục mặc định 30 / 90 / 5** và nút Lưu. |
| UC-51 | Tên đơn vị | Ô nhập Tên đơn vị (VD "Đảng ủy Phường X"), hiển thị trên header mọi màn và trên dòng tiêu đề của file Excel xuất ra. Để trống thì header chỉ hiện tên hệ thống. |

### Xuất Excel (dùng chung cho UC-11, UC-34, UC-40)
- Một file `.xlsx`, tên file `DuDieuKien_<TênĐợt>_<Năm>.xlsx` (hoặc `ChuaThuocDot_<Năm>.xlsx`); tên đợt bỏ dấu và thay ký tự đặc biệt bằng `-` (VD "Đợt 7/11" → `Dot7-11`).
- Dòng tiêu đề: Tên đơn vị (nếu có) + tên đợt + khoảng ngày (đã gắn năm) + ngày xuất.
- Các cột đúng như bảng đang xem, thứ tự sắp như đang xem. Ô trống để rỗng (không ghi `—`).
- **Không có cột ghi chú** (QT12). Ghi chú là chữ tự do dài tới 500 ký tự, có xuống dòng; đưa vào bảng Excel sẽ làm vỡ bố cục của tờ trình cán bộ in ra. Ghi chú chỉ đọc trên giao diện.

---

## 5. Mô hình dữ liệu khái niệm

Chỉ 4 thực thể nghiệp vụ + 1 thực thể tài khoản.

**PartyMember – Đảng viên**
| Trường | Kiểu | Bắt buộc | Ghi chú |
|---|---|---|---|
| FullName | text | ✔ | |
| DateOfBirth | date | – | |
| Gender | Nam / Nữ | – | |
| OfficialAdmissionDate | date | ✔ | ≤ hôm nay |
| Note | text ≤ 500 | – | QT12 — ghi chú, tối đa 500 ký tự sau khi cắt khoảng trắng hai đầu; rỗng thì để trống |
| NoteUpdatedAt | datetime | tự động | QT12 — ngày ghi; chỉ đổi khi **nội dung** Note đổi; cùng về rỗng khi xóa Note |
| CreatedAt / UpdatedAt | datetime | tự động | |

**AwardPeriod – Đợt trao huy hiệu**
| Trường | Kiểu | Bắt buộc | Ghi chú |
|---|---|---|---|
| Name | text | ✔ | duy nhất |
| FromDay, FromMonth | số | ✔ | không lưu năm |
| ToDay, ToMonth | số | ✔ | ≥ Từ ngày trong cùng năm |

**AppSetting – Cài đặt** — 1 bản ghi duy nhất: StartYears (30), EndYears (90), StepYears (5), UnitName (tên đơn vị, có thể trống).

**User – Tài khoản** — 1 bản ghi seed sẵn: Username, PasswordHash. Không có role.

**Không có bảng kết quả đợt** (QT5). Nếu sau này cần "đánh dấu đã trao", thêm bảng `AwardRecord (PartyMemberId, Milestone, Year, AwardedAt)` mà không phải sửa các bảng hiện có.

---

## 6. Điều hướng và luồng chính

### 6.1 Sơ đồ điều hướng (menu trái, 5 mục)
```
Đăng nhập
  └─ Dashboard ─────────── đợt sắp tới + bảng đủ điều kiện + cảnh báo
  ├─ Đảng viên ─────────── bảng + tìm / lọc
  │     ├─ Thêm / Sửa (modal)
  │     └─ Import Excel (wizard 3 bước)
  ├─ Đợt trao huy hiệu ─── bảng đợt + cảnh báo phủ kín
  │     ├─ Thêm / Sửa (modal)
  │     └─ Chi tiết đợt ─────── chọn năm + bảng + Xuất Excel
  ├─ Chưa thuộc đợt nào ── chọn năm + bảng + Xuất Excel
  └─ Cài đặt ───────────── 3 ô số + xem trước dãy mốc
```

### 6.2 Luồng lần dùng đầu tiên
1. Đăng nhập → Dashboard trống, hiện cảnh báo: chưa có đảng viên, chưa có đợt.
2. **Cài đặt**: kiểm tra 30 / 90 / 5, lưu hoặc giữ nguyên.
3. **Đợt trao huy hiệu**: tạo các đợt (VD 4 đợt quanh 3/2, 19/5, 2/9, 7/11). Cảnh báo phủ kín tắt khi đủ.
4. **Đảng viên** → Import Excel → tải mẫu → điền → chọn file → xem trước → nạp.
5. **Dashboard**: thấy đợt sắp tới và danh sách → Xuất Excel → làm tờ trình.

### 6.3 Luồng định kỳ (mỗi đợt)
1. Có đảng viên mới → Import hoặc thêm tay.
2. Mở Dashboard (hoặc trang chi tiết đợt) → kiểm tra → Xuất Excel.
3. Thỉnh thoảng xem "Chưa thuộc đợt nào" để chắc không sót ai.

### 6.4 Rủi ro nghiệp vụ cần thể hiện trên UI
| Rủi ro | Cách thể hiện |
|---|---|
| Import không chống trùng, nạp 2 lần sẽ có người trùng | Bước xem trước nhấn mạnh "Sẽ thêm N người **mới**"; danh sách đảng viên cho tìm theo tên để tự phát hiện trùng và xóa. |
| Sửa đợt / cài đặt ảnh hưởng tức thì mọi năm | Dòng nhắc trên form. |
| Đợt không phủ kín → sót người | Banner cảnh báo ở M3 + Dashboard; màn hình M4 để nhìn thấy người bị sót. |
| Xóa là xóa hẳn | Hộp xác nhận rõ số lượng. |

---

## 7. Yêu cầu phi chức năng
- Chạy cục bộ trên 1 máy, 1 người dùng; dữ liệu vài nghìn đảng viên — không cần tối ưu đặc biệt.
- Tính toán danh sách đủ điều kiện cho 1 đợt / 1 năm phản hồi dưới 1 giây với 10.000 đảng viên.
- Ngôn ngữ giao diện: tiếng Việt. Định dạng ngày hiển thị: `dd/MM/yyyy`; ngày/tháng của đợt: `dd/MM`.
- Sao lưu: toàn bộ dữ liệu nằm trong PostgreSQL; hướng dẫn sao lưu bằng `pg_dump` sẽ ghi trong README.

---

## Phụ lục A — Ghi chú kỹ thuật (đã chốt, dùng cho giai đoạn chia task)

| Hạng mục | Quyết định |
|---|---|
| Cấu trúc repo | `D:\Personal\Em\HuyHieuDang\` gồm `BE/`, `FE/`, `docs/` cùng cấp |
| BE | ASP.NET Core Web API, xây trên `D:\Personal\Em\maximus-webapi-boilerplate` (đổi tên dự án, bỏ phần không dùng). Giữ cấu trúc Core / Infrastructure / Migrators / Web + tests |
| FE | React + Vite + TypeScript + Ant Design, service riêng, gọi BE qua REST |
| DB | PostgreSQL |
| Deploy | 1 file `docker-compose.yml` chạy 3 service: FE (nginx), BE, PostgreSQL |
| Auth | JWT, đăng nhập cơ bản, seed 1 tài khoản admin. Không có màn hình quản lý user / role |
| Excel | Import / export bằng thư viện Excel phía BE (MiniExcel hoặc tương đương) |
| Logic tính mốc | Tách thành service thuần (không phụ thuộc DB) để unit test đầy đủ QT1–QT8, đặc biệt 29/02, biên đợt, đợt qua năm sau |

## Phụ lục B — Các bước tiếp theo
1. ~~Thiết kế UI (wireframe / mockup)~~ — **xong**, 10 artboard trong `docs/design-system/`.
2. ~~Rà soát lại use case sau khi có UI, cập nhật tài liệu~~ — **xong**, tài liệu v1.1.
3. Chia task và lập kế hoạch triển khai — xem `2026-09-19-team-and-task-plan.md`.

---

## Phụ lục C — Đối chiếu UI ↔ use case (19/09/2026)

Bản thiết kế gồm **10 artboard**: 1 Đăng nhập · 2 Dashboard · 2 Dashboard trống · 3 Đảng viên · 3 Đảng viên trống · 4 Import bước 2 (kèm B1, B3 trong cùng artboard) · 5 Đợt trao huy hiệu · 5 Chi tiết đợt · 6 Chưa thuộc đợt nào · 7 Cài đặt.

### C.1 Đã có trong UI và khớp tài liệu
UC-00, UC-10, UC-11, UC-12, UC-20, UC-24, UC-25, UC-30, UC-34, UC-40, UC-50 — đều thể hiện đúng cột, đúng thứ tự sắp xếp, đúng thông điệp cảnh báo.

### C.2 UI bổ sung → đã đưa vào tài liệu v1.1
| Thành phần trong UI | Đưa vào |
|---|---|
| Badge số người bị sót trên menu "Chưa thuộc đợt nào" | Khung chung mục 4 |
| Header có Tên đơn vị + ngày hôm nay | Khung chung mục 4, UC-51 |
| Khối hướng dẫn 3 bước ở Dashboard trống | UC-13 |
| Cột Trạng thái (Đã qua / Đang diễn ra / Sắp tới · N ngày) | QT11, UC-30 |
| Dải độ phủ 12 tháng | UC-36 |
| Tab "Thông tin" ở trang chi tiết đợt | UC-34 — **bỏ ngày 30/09/2026**, gộp vào tiêu đề trang |
| Nút "Khôi phục mặc định 30 / 90 / 5" | UC-50 |
| Ràng buộc `.xlsx` ≤ 10 MB, dòng ví dụ trong B1 | QT9 |
| Hiển thị `—` cho ô trống, cho người hết mốc | QT3a, UC-20 |
| Chọn số dòng/trang, nhãn "Đang chọn N dòng" | UC-20 |
| Nút ghi chú ở cột Thao tác, hai trạng thái và hai tooltip | QT12, UC-20 — **thêm ngày 30/09/2026** |
| Hộp `Ghi chú — <Họ tên>` với bộ đếm `x / 500` | UC-26 — **thêm ngày 30/09/2026** |
| Ô Ghi chú trong modal Thêm/Sửa đảng viên; Ngày sinh và Giới tính cùng một hàng; bỏ hai dòng chữ dưới hai ô ngày | UC-21, UC-22 — **thêm ngày 30/09/2026** |
| Viên ngày ghi màu vàng `Ghi ngày dd/MM/yyyy`, một kiểu duy nhất ở mọi nơi | QT12 — **thêm ngày 30/09/2026** |
| Khối ghi chú gọn `Ghi chú · N người` cạnh nút Xuất Excel | UC-11, UC-34 — **thêm ngày 30/09/2026** |
| Hộp xem đầy đủ ghi chú, có ô tìm và tô sáng phần khớp | UC-11, UC-34 — **thêm ngày 30/09/2026** |

### C.3 Chưa có mockup — dựng theo mô tả trong tài liệu
- Modal Thêm / Sửa đảng viên (UC-21, UC-22) và hộp xác nhận xóa (UC-23).
- Modal Thêm / Sửa đợt (UC-31, UC-32) và hộp xác nhận xóa đợt (UC-33).
- Trạng thái trống của M4 và M6 (đã có câu chữ trong thiết kế, chưa vẽ).
- Trạng thái đang tải, trạng thái lỗi mạng / lỗi máy chủ của các bảng.

Frontend Developer tự dựng các thành phần này theo đúng ngôn ngữ thiết kế của 10 artboard (Ant Design 5, bảng màu và typography trong artboard), không cần chờ thêm thiết kế.

### C.4 Điểm cần lưu ý khi dựng
- Dữ liệu trong artboard là **dữ liệu mẫu**, không phải dữ liệu thật; không hardcode.
- Ngày "Hôm nay 17/09/2026" trong thiết kế chỉ là minh hoạ, phải lấy ngày hệ thống.
- Số "1.248 người" dùng dấu chấm ngăn nghìn theo định dạng Việt Nam.
- **Chống tràn chữ của ghi chú** (QT12): mọi chỗ hiện toàn văn ghi chú đều giữ đúng xuống dòng người viết (`white-space: pre-wrap`); chuỗi dài không có dấu cách vẫn tự ngắt dòng (`overflow-wrap: anywhere`). Khối gọn ở UC-11 và UC-34 cắt phần thừa bằng dấu `…`.
- **Ảnh dựng thử của ghi chú** (đính kèm ở việc gốc) chụp trước một số sửa đổi của chủ dự án ngày 30/09/2026. Khi ảnh khác chữ trong tài liệu này thì **làm theo chữ**.
