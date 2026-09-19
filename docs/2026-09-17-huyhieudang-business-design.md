# HuyHieuDang — Tài liệu nghiệp vụ (v1)

| | |
|---|---|
| Dự án | HuyHieuDang — Hệ thống hỗ trợ xét trao Huy hiệu Đảng |
| Phiên bản tài liệu | 1.1 — 19/09/2026 (đối chiếu với bản thiết kế UI 10 artboard) |
| Loại tài liệu | Tài liệu nghiệp vụ (BRD/SRS rút gọn), đầu vào cho thiết kế UI và chia task |
| Trạng thái | Đã thống nhất với chủ dự án |
| Tài liệu liên quan | `2026-09-17-ui-design-prompt.md`, `design-system/Huy Hieu Dang - 9 man hinh.html` |

**Nhật ký thay đổi**
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
| Đợt trao huy hiệu (đợt) | Một khoảng ngày trong năm (chỉ ngày/tháng, không gắn năm) dùng để gom những người tròn mốc trong khoảng đó. |
| Đủ điều kiện | Đảng viên có Ngày tròn mốc rơi vào khoảng ngày của một đợt trong một năm cụ thể. |
| Chưa thuộc đợt nào | Đảng viên tròn mốc trong năm nhưng Ngày tròn mốc không rơi vào đợt nào. |

---

## 3. Quy tắc nghiệp vụ

Mọi màn hình đều dựa trên các quy tắc dưới đây. Ký hiệu: `D` = Ngày chính thức, `N` = mốc, `Y` = năm đang xét.

**QT1 – Dãy mốc huy hiệu.** Từ cài đặt (Bắt đầu, Kết thúc, Bước; mặc định 30 / 90 / 5) sinh ra dãy `{Bắt đầu, Bắt đầu + Bước, …}` cho đến khi vượt Kết thúc thì dừng. Ràng buộc: cả 3 là số nguyên dương, Bắt đầu ≤ Kết thúc, Bước ≥ 1.
Ví dụ: 30 / 90 / 5 → 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90. Đổi Bước = 10 → 30, 40, 50, 60, 70, 80, 90.

**QT2 – Ngày tròn mốc.** `Ngày tròn mốc(D, N) = D + N năm` — cùng ngày, cùng tháng, năm cộng thêm N. Nếu D là 29/02 và năm đích không nhuận thì lấy 28/02.

**QT3 – Tuổi đảng hiện tại.** Số năm tròn từ `D` đến hôm nay; chỉ tính năm thứ k khi đã qua (hoặc đúng) ngày kỷ niệm thứ k. Chỉ dùng để hiển thị trong danh sách đảng viên; không tham gia xét đợt.

**QT3a – Mốc kế tiếp.** Mốc nhỏ nhất trong dãy QT1 lớn hơn tuổi đảng hiện tại. Nếu tuổi đảng đã vượt mốc lớn nhất (hoặc không còn mốc nào phía trước) → hiển thị `—` ở cả cột Mốc kế tiếp và Ngày tròn mốc kế tiếp. Chỉ dùng để hiển thị.

**QT4 – Đủ điều kiện trong đợt, theo năm.** Với đợt `Đ` có Từ ngày `f` (ngày/tháng) và Đến ngày `t` (ngày/tháng), và năm `Y`: gắn năm để có `F = f/Y`, `T = t/Y` (29/02 ở năm không nhuận → 28/02). Đảng viên đủ điều kiện khi **tồn tại mốc `N`** trong dãy QT1 sao cho `F ≤ D + N năm ≤ T`. Mốc `N` đó là huy hiệu được trao. Vì các mốc cách nhau ≥ 1 năm và một đợt luôn ngắn hơn 1 năm, mỗi người có tối đa 1 mốc trong 1 đợt.

**QT5 – Không lưu kết quả.** Danh sách đủ điều kiện được tính lại mỗi lần xem. Thêm người, sửa ngày, sửa đợt, đổi cài đặt → danh sách tự cập nhật, không cần thao tác gì thêm.

**QT6 – Đợt trao huy hiệu.** Một danh sách duy nhất, dùng chung cho mọi năm. Mỗi đợt có Tên (duy nhất), Từ ngày, Đến ngày — chỉ lưu ngày/tháng. Ràng buộc: nằm trọn trong một năm dương lịch, `Từ ≤ Đến` (không cho vắt qua 31/12 → 01/01). Sửa đợt có hiệu lực ngay cho mọi năm, kể cả năm hiện tại.
Hệ thống **cảnh báo nhưng vẫn cho lưu** khi: (a) hai đợt chồng lấn nhau; (b) các đợt không phủ kín 01/01–31/12.

**QT7 – Chưa thuộc đợt nào.** Với năm `Y`: đảng viên có ít nhất một mốc `N` sao cho `D + N năm` rơi trong năm `Y` nhưng không nằm trong khoảng của bất kỳ đợt nào (đã gắn năm `Y`).

**QT8 – Đợt sắp tới (Dashboard).** Gắn năm hiện tại vào tất cả đợt; chọn đợt có `Đến ≥ hôm nay` và `Từ` nhỏ nhất (nếu hôm nay đang nằm trong một đợt thì chính là đợt đó). Nếu mọi đợt trong năm đã qua → lấy đợt sớm nhất của **năm sau**. Không có đợt nào → Dashboard báo "Chưa cài đợt trao huy hiệu".

**QT9 – Import Excel.** Mọi dòng hợp lệ đều được **thêm mới**, không kiểm tra trùng. Dòng lỗi được liệt kê kèm số dòng và lý do; người dùng chọn "Nạp các dòng hợp lệ" hoặc "Hủy".
- Ràng buộc file: định dạng `.xlsx`, dung lượng ≤ 10 MB, đúng 4 cột theo thứ tự Họ tên · Ngày sinh · Giới tính · Ngày vào Đảng chính thức; dòng đầu là tiêu đề.
- Lỗi cấp dòng: thiếu Họ tên; thiếu Ngày chính thức; sai định dạng ngày (cần `dd/MM/yyyy`); Ngày chính thức ở tương lai; Giới tính khác Nam/Nữ khi có điền; Ngày sinh sau Ngày chính thức.
- Lỗi cấp file (chặn ngay bước 1): sai định dạng, vượt dung lượng, sai số cột, file rỗng.
- Việc nạp là **một giao dịch**: hoặc nạp hết các dòng hợp lệ, hoặc không nạp gì (khi có lỗi kỹ thuật giữa chừng).

**QT10 – Xóa.** Xóa đảng viên và xóa đợt là xóa hẳn, có hộp xác nhận. Không có thùng rác.

**QT11 – Trạng thái đợt trong năm hiện tại.** Gắn năm hiện tại vào đợt rồi so với hôm nay: `Đến < hôm nay` → **Đã qua**; `Từ ≤ hôm nay ≤ Đến` → **Đang diễn ra**; `Từ > hôm nay` → **Sắp tới**, kèm số ngày còn lại. Dùng ở bảng danh sách đợt và thẻ Dashboard.

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
| UC-11 | Bảng đủ điều kiện của đợt sắp tới | Cột: STT, Họ tên, Giới tính, Ngày sinh, Ngày chính thức, Ngày tròn mốc, Mốc huy hiệu. Sắp theo Mốc rồi Họ tên. Nút **Xuất Excel**. |
| UC-12 | Cảnh báo nhanh | Hiện khi: chưa có đảng viên nào; chưa có đợt nào; có người "chưa thuộc đợt nào" trong năm hiện tại (kèm số lượng, bấm vào để đi tới M4); các đợt chưa phủ kín (liệt kê các khoảng trống) / chồng lấn. |
| UC-13 | Hướng dẫn 3 bước (trạng thái trống) | Khi chưa có dữ liệu, thay bảng bằng khối hướng dẫn: 1 Kiểm tra cài đặt mốc → 2 Tạo các đợt trong năm → 3 Nạp danh sách đảng viên, mỗi bước có nút đi thẳng tới màn tương ứng. |

**Khung chung mọi màn** (áp dụng từ M1 đến M5):
- Sider trái dạng thanh biểu tượng (thu gọn được), 5 mục. Mục "Chưa thuộc đợt nào" có **badge** hiển thị số người bị sót trong năm hiện tại; không có ai thì ẩn badge.
- Header: tên hệ thống + **Tên đơn vị** (lấy từ Cài đặt), ngày hôm nay, tên tài khoản, nút Đăng xuất.

### M2 – Đảng viên
| UC | Tên | Mô tả |
|---|---|---|
| UC-20 | Danh sách | Dòng tóm tắt "N người · Tuổi đảng tính đến hôm nay". Cột: Họ tên, Giới tính, Ngày sinh, Ngày chính thức, Tuổi đảng (QT3), Mốc kế tiếp, Ngày tròn mốc kế tiếp (QT3a). Tìm theo tên (chứa chuỗi), lọc theo giới tính, sắp xếp theo cột, phân trang có chọn số dòng/trang. Chọn nhiều dòng để xóa; khi đang chọn hiện "Đang chọn N dòng". Ô trống hiển thị `—`. |
| UC-21 | Thêm thủ công | Modal 4 trường: Họ tên (bắt buộc), Ngày sinh, Giới tính (Nam/Nữ/để trống), Ngày chính thức (bắt buộc, ≤ hôm nay). |
| UC-22 | Sửa | Modal như UC-21, có sẵn dữ liệu. |
| UC-23 | Xóa | Một hoặc nhiều dòng đã chọn. Hộp xác nhận nêu rõ số người sẽ xóa. |
| UC-24 | Import Excel | Trang riêng, wizard 3 bước (Chọn file → Xem trước → Kết quả). **B1**: vùng kéo-thả, mô tả 4 cột kèm dòng ví dụ, ghi rõ `.xlsx` ≤ 10 MB và định dạng ngày, nút Tải file mẫu. **B2**: dòng tóm tắt "Sẽ thêm **N người mới** · M dòng lỗi bị bỏ qua" + cảnh báo "Hệ thống không kiểm tra trùng — nếu đã nạp file này trước đó, hãy Hủy"; hai tab Hợp lệ (N) / Lỗi (M); bảng lỗi có cột Dòng · Họ tên · Ngày sinh · Giới tính · Ngày chính thức · Lý do. **B3**: "Đã thêm N người, bỏ qua M dòng lỗi" + nút Về danh sách. |
| UC-25 | Tải file mẫu | File `.xlsx` có 4 cột đúng thứ tự: Họ tên · Ngày sinh · Giới tính · Ngày vào đảng chính thức. Có 1–2 dòng ví dụ. Ngày theo định dạng `dd/MM/yyyy`. |

### M3 – Đợt trao huy hiệu
| UC | Tên | Mô tả |
|---|---|---|
| UC-30 | Danh sách đợt | Dòng tóm tắt "N đợt · dùng chung cho mọi năm, chỉ lưu ngày/tháng". Bảng sắp theo Từ ngày: Tên, Từ ngày (dd/MM), Đến ngày (dd/MM), **Trạng thái năm nay** (QT11), Số người đủ điều kiện **năm nay**, Thao tác. Banner cảnh báo chồng lấn / chưa phủ kín kèm liệt kê khoảng trống (QT6). Không có bộ chọn năm. |
| UC-36 | Dải độ phủ trong năm | Biểu đồ dải ngang 12 tháng phía trên bảng: mỗi đợt là một vạch màu, khoảng trống tô xám, có vạch đánh dấu "Hôm nay". Giúp nhìn ra ngay chỗ chưa phủ kín. |
| UC-31 | Thêm đợt | Modal: Tên, Từ ngày (dd/MM), Đến ngày (dd/MM). |
| UC-32 | Sửa đợt | Như UC-31. Dòng nhắc: "Thay đổi áp dụng ngay cho năm hiện tại và các năm sau". |
| UC-33 | Xóa đợt | Hộp xác nhận. |
| UC-34 | Đủ điều kiện của đợt | Trang chi tiết đợt có breadcrumb + 2 tab: **Thông tin** (tên, khoảng ngày "01/10 – 07/11 hằng năm", nút Sửa/Xóa) và **Danh sách đủ điều kiện**. Tab thứ hai có bộ chọn năm dạng segmented (năm trước · năm nay · năm sau), hiển thị khoảng ngày đã gắn năm và nhãn ngữ cảnh ("Năm sau · chuẩn bị trước"). Bảng như UC-11 + **Xuất Excel**. |

### M4 – Chưa thuộc đợt nào
| UC | Tên | Mô tả |
|---|---|---|
| UC-40 | Danh sách theo năm | Bộ chọn năm dạng segmented (mặc định năm hiện tại). Bảng như UC-11, thêm cột "Khoảng trống" ("Giữa Đợt A và Đợt B", "Trước đợt đầu tiên", "Sau đợt cuối cùng"). Khối gợi ý hành động: "Nới Đến ngày của đợt trước hoặc Từ ngày của đợt sau" + nút đi tới M3. Nút **Xuất Excel**. Trạng thái trống: "Không có ai bị sót trong năm YYYY." |

### M5 – Cài đặt
| UC | Tên | Mô tả |
|---|---|---|
| UC-50 | Cài mốc tuổi đảng | 3 ô số có nút −/+: Bắt đầu / Kết thúc / Bước. Xem trước dãy mốc sinh ra (QT1) ngay khi gõ, kèm số lượng mốc ("13 mốc"). Dòng nhắc: "Thay đổi ảnh hưởng ngay đến mọi danh sách đủ điều kiện · Áp dụng cho mọi năm và cho Dashboard". Nút **Khôi phục mặc định 30 / 90 / 5** và nút Lưu. |
| UC-51 | Tên đơn vị | Ô nhập Tên đơn vị (VD "Đảng ủy Phường X"), hiển thị trên header mọi màn và trên dòng tiêu đề của file Excel xuất ra. Để trống thì header chỉ hiện tên hệ thống. |

### Xuất Excel (dùng chung cho UC-11, UC-34, UC-40)
- Một file `.xlsx`, tên file `DuDieuKien_<TênĐợt>_<Năm>.xlsx` (hoặc `ChuaThuocDot_<Năm>.xlsx`); tên đợt bỏ dấu và thay ký tự đặc biệt bằng `-` (VD "Đợt 7/11" → `Dot7-11`).
- Dòng tiêu đề: Tên đơn vị (nếu có) + tên đợt + khoảng ngày (đã gắn năm) + ngày xuất.
- Các cột đúng như bảng đang xem, thứ tự sắp như đang xem. Ô trống để rỗng (không ghi `—`).

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
  │     └─ [Tab] Đủ điều kiện ── chọn năm + bảng + Xuất Excel
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
2. Mở Dashboard (hoặc tab Đủ điều kiện của đợt) → kiểm tra → Xuất Excel.
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
| Tab "Thông tin" ở trang chi tiết đợt | UC-34 |
| Nút "Khôi phục mặc định 30 / 90 / 5" | UC-50 |
| Ràng buộc `.xlsx` ≤ 10 MB, dòng ví dụ trong B1 | QT9 |
| Hiển thị `—` cho ô trống, cho người hết mốc | QT3a, UC-20 |
| Chọn số dòng/trang, nhãn "Đang chọn N dòng" | UC-20 |

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
