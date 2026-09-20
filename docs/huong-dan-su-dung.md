# Hướng dẫn sử dụng — Hệ thống hỗ trợ xét trao Huy hiệu Đảng

## Tài liệu này dùng để làm gì

Tài liệu này hướng dẫn cán bộ dùng phần mềm để lập danh sách đảng viên **đủ điều kiện** nhận Huy hiệu Đảng theo từng **đợt trao huy hiệu**, rồi xuất ra file Excel để làm tờ trình.

Bạn không cần biết gì về máy tính ngoài việc dùng trình duyệt và Excel. Mỗi phần dưới đây là một việc cụ thể, làm theo thứ tự từ trên xuống.

Đọc phần nào:

- Lần đầu tiên dùng phần mềm, chưa có dữ liệu gì → đọc **Phần A**.
- Phần mềm đã chạy rồi, nay đến kỳ trao huy hiệu → đọc **Phần B**.
- Gặp một từ không rõ nghĩa → xem **Phần C**.
- Thấy số liệu lạ, hoặc phần mềm báo lỗi → xem **Phần D** và **Phần E**.

---

## Mục lục

- [Trước khi bắt đầu](#truoc-khi-bat-dau)
- [Phần A — Lần đầu tiên dùng](#phan-a--lan-dau-tien-dung)
  - [A1. Đăng nhập](#a1-dang-nhap)
  - [A2. Kiểm tra mốc tuổi đảng trong Cài đặt](#a2-kiem-tra-moc-tuoi-dang-trong-cai-dat)
  - [A3. Tạo các đợt trao huy hiệu](#a3-tao-cac-dot-trao-huy-hieu)
  - [A4. Nạp danh sách đảng viên](#a4-nap-danh-sach-dang-vien)
  - [A5. Xem Dashboard và xuất Excel](#a5-xem-dashboard-va-xuat-excel)
- [Phần B — Việc làm định kỳ mỗi đợt](#phan-b--viec-lam-dinh-ky-moi-dot)
  - [B1. Cập nhật đảng viên](#b1-cap-nhat-dang-vien)
  - [B2. Kiểm tra danh sách và xuất Excel](#b2-kiem-tra-danh-sach-va-xuat-excel)
  - [B3. Rà soát người chưa thuộc đợt nào](#b3-ra-soat-nguoi-chua-thuoc-dot-nao)
- [Phần C — Giải thích từ ngữ](#phan-c--giai-thich-tu-ngu)
- [Phần D — Những chỗ dễ nhầm](#phan-d--nhung-cho-de-nham)
- [Phần E — Khi gặp trục trặc](#phan-e--khi-gap-truc-trac)
- [Sao lưu dữ liệu](#sao-luu-du-lieu)
- [Phụ lục — Danh sách ảnh màn hình](#phu-luc--danh-sach-anh-man-hinh)

---

<a id="truoc-khi-bat-dau"></a>
## Trước khi bắt đầu

**Bạn cần có:** địa chỉ phần mềm (ví dụ `http://localhost:5173`), tên tài khoản và mật khẩu. Người cài đặt phần mềm sẽ đưa cho bạn ba thứ này.

**Màn hình nào cũng có ba phần giống nhau:**

- **Thanh menu bên trái** có 5 mục: **Dashboard**, **Đảng viên**, **Đợt trao huy hiệu**, **Chưa thuộc đợt nào**, **Cài đặt**.
- Mục **Chưa thuộc đợt nào** có thể hiện một **con số nhỏ màu đỏ**. Đó là số người tròn mốc trong năm nay nhưng không rơi vào đợt nào. Không có ai thì con số này ẩn đi.
- **Thanh trên cùng** hiện tên hệ thống, **tên đơn vị** của bạn, ngày hôm nay, tên tài khoản và nút **Đăng xuất**.

<!-- ảnh: màn Dashboard đã có dữ liệu, chụp đủ chiều ngang để thấy cả menu trái 5 mục và thanh trên cùng; mục "Chưa thuộc đợt nào" đang có badge đỏ -->
> **[Ảnh 1]** Khung màn hình chung: menu trái 5 mục và thanh trên cùng — `docs/images/01-khung-man-hinh.png`

**Một điều quan trọng cần nhớ ngay:** phần mềm **không lưu sẵn** danh sách đủ điều kiện. Mỗi lần bạn mở một danh sách, phần mềm tính lại từ đầu. Vì vậy chỉ cần sửa ngày của một người, sửa một đợt, hay đổi mốc trong Cài đặt là mọi danh sách đổi theo ngay. Bạn không phải bấm nút "tính lại" nào cả.

---

<a id="phan-a--lan-dau-tien-dung"></a>
## Phần A — Lần đầu tiên dùng

Phần này làm **một lần duy nhất**, theo đúng thứ tự 5 bước. Làm sai thứ tự thì các bước sau sẽ hiện danh sách trống.

<a id="a1-dang-nhap"></a>
### A1. Đăng nhập

1. Mở trình duyệt, gõ địa chỉ phần mềm.
2. Nhập ô **Tài khoản** và ô **Mật khẩu**. Muốn nhìn thấy mật khẩu vừa gõ thì bấm chữ **Hiện** ở cuối ô; bấm **Ẩn** để giấu lại.
3. Bấm nút **Đăng nhập**.

Vào được thì phần mềm mở thẳng **Dashboard**.

Nếu sai, phần mềm chỉ báo một câu chung: *"Sai tài khoản hoặc mật khẩu"*. Phần mềm cố ý không nói sai ở chỗ nào, để người lạ không dò được tài khoản. Gõ lại cho kỹ, chú ý phím Caps Lock.

<!-- ảnh: màn Đăng nhập, ô Tài khoản đã điền, ô Mật khẩu để trống, thấy rõ nút Đăng nhập -->
> **[Ảnh 2]** Màn hình Đăng nhập — `docs/images/02-dang-nhap.png`

**Lần đầu vào, Dashboard sẽ trống.** Đó là đúng — bạn chưa nhập gì cả. Phần mềm hiện khối **Bắt đầu với ba bước** kèm nút đi thẳng tới từng màn hình: **Mở Cài đặt**, **Thêm đợt**, **Import Excel**. Ba bước đó chính là A2, A3, A4 dưới đây.

<!-- ảnh: Dashboard khi cơ sở dữ liệu còn trắng — thấy khối "Bắt đầu với ba bước" và các thẻ cảnh báo "Chưa có đảng viên nào", "Chưa cài đợt trao huy hiệu" -->
> **[Ảnh 3]** Dashboard khi chưa có dữ liệu, kèm khối Bắt đầu với ba bước — `docs/images/03-dashboard-trong.png`

<a id="a2-kiem-tra-moc-tuoi-dang-trong-cai-dat"></a>
### A2. Kiểm tra mốc tuổi đảng trong Cài đặt

**Mốc tuổi đảng** là các năm tuổi đảng được trao huy hiệu: 30 năm, 40 năm, 45 năm… Phần mềm không bắt bạn gõ từng mốc. Bạn chỉ cho ba con số, phần mềm tự sinh ra cả dãy.

1. Bấm **Cài đặt** ở menu trái.
2. Nhìn thẻ **Mốc tuổi đảng** với ba ô số:
   - **Bắt đầu (năm)** — mốc đầu tiên. Mặc định **30**.
   - **Kết thúc (năm)** — mốc cuối cùng. Mặc định **90**.
   - **Bước (năm)** — khoảng cách giữa hai mốc liền nhau. Mặc định **5**.
3. Ngay dưới ba ô là phần **xem trước dãy mốc**. Với 30 / 90 / 5, phần mềm hiện: 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90.
4. Dãy này đúng với quy định của đơn vị bạn thì **không phải sửa gì**.
5. Cần sửa thì gõ số mới, hoặc bấm dấu **+** / **−** ở hai đầu ô. Phần xem trước đổi theo ngay khi bạn gõ, **trước khi** bấm Lưu. Xem thấy đúng rồi hãy bấm **Lưu**.

Bấm nhầm thì có nút **Khôi phục mặc định 30 / 90 / 5** đưa ba ô về như cũ.

Ba ô đều phải là **số nguyên dương**, và **Bắt đầu** không được lớn hơn **Kết thúc**. Gõ sai thì phần mềm báo ngay dưới ô và không cho lưu.

**Nhân tiện, điền luôn Tên đơn vị.** Cũng trên màn hình này, thẻ thứ hai là **Tên đơn vị**, ví dụ *"Đảng ủy Phường X"*. Tên này hiện trên thanh trên cùng của mọi màn hình, và quan trọng hơn: nó được in thành **dòng đầu tiên** của mọi file Excel bạn xuất ra. Để trống cũng được — khi đó file Excel không có dòng tên đơn vị.

**Chỉ có một nút Lưu cho cả màn hình.** Nút **Lưu** ở góc dưới bên phải lưu cả mốc tuổi đảng lẫn tên đơn vị. Sửa xong cả hai thẻ rồi bấm Lưu một lần là đủ.

<!-- ảnh: màn Cài đặt, ba ô 30/90/5, dãy xem trước 13 mốc, khung cảnh báo vàng, ô Tên đơn vị điền "Đảng ủy Phường X", nút Lưu đang bật -->
> **[Ảnh 4]** Màn hình Cài đặt: ba ô số, dãy mốc xem trước và ô Tên đơn vị — `docs/images/04-cai-dat-moc.png`

> ⚠️ **Đổi mốc là đổi mọi thứ.** Ngay trên màn hình có khung vàng nhắc điều này: *"Thay đổi ảnh hưởng ngay đến mọi danh sách đủ điều kiện"*. Ba con số này áp dụng cho **mọi năm**, kể cả các đợt đã qua. Đổi Bước từ 5 thành 10 là lập tức mất các mốc 35, 45, 55… và danh sách đủ điều kiện ngắn lại. Chỉ đổi khi đơn vị thực sự có quy định khác.

<a id="a3-tao-cac-dot-trao-huy-hieu"></a>
### A3. Tạo các đợt trao huy hiệu

**Đợt trao huy hiệu** là một khoảng ngày trong năm để gom những người tròn mốc trong khoảng đó. Đợt **chỉ lưu ngày và tháng**, không lưu năm — tạo một lần rồi dùng cho mọi năm về sau.

1. Bấm **Đợt trao huy hiệu** ở menu trái.
2. Bấm nút **+ Thêm đợt** ở góc trên bên phải. Chưa có đợt nào thì bấm nút **+ Thêm đợt đầu tiên** ở giữa màn hình.
3. Cửa sổ **Thêm đợt trao huy hiệu** hiện ra. Điền ba ô:
   - **Tên đợt** — ví dụ *"Đợt 7/11"*. Tên không được trùng với đợt đã có.
   - **Từ ngày** — chỉ ngày và tháng, ví dụ `01/10`.
   - **Đến ngày** — chỉ ngày và tháng, ví dụ `07/11`.
4. Bấm **Thêm đợt**.
5. Làm lại cho từng đợt. Thông thường mỗi năm có 4 đợt, quanh các ngày **3/2**, **19/5**, **2/9** và **7/11**.

Một đợt **được phép vắt qua ngày 31/12**. Nếu Đến ngày đứng trước Từ ngày trong vòng năm — ví dụ Từ 01/12, Đến 28/02 — phần mềm hiểu là đợt kết thúc ở **năm sau**, và dòng nhắc trong ô nhập ghi rõ điều đó. Bảng danh sách đợt cũng ghi "năm sau" cạnh Đến ngày.

<!-- ảnh: cửa sổ "Thêm đợt trao huy hiệu" đang mở, Tên đợt "Đợt 7/11", Từ ngày 01/10, Đến ngày 07/11 -->
> **[Ảnh 5]** Cửa sổ Thêm đợt trao huy hiệu — `docs/images/05-them-dot.png`

**Đọc bảng danh sách đợt:**

| Cột | Nghĩa |
|---|---|
| Tên đợt | Tên bạn đặt |
| Từ ngày · Đến ngày | Khoảng ngày trong năm, dạng `dd/MM` |
| Trạng thái *(kèm năm nay)* | **Đã qua**, **Đang diễn ra**, hoặc **Sắp tới** kèm số ngày còn lại |
| Đủ điều kiện năm nay | Số người đủ điều kiện của đợt đó trong năm nay |
| Thao tác | Ba nút: **Xem chi tiết**, **Sửa**, **Xóa** |

Bảng sắp theo **Từ ngày**. Dòng của đợt đang diễn ra hoặc sắp tới được tô nền vàng nhạt.

**Dải độ phủ** phía trên bảng là một thanh ngang 12 tháng. Mỗi đợt là một vạch màu, chỗ nào **xám** là chưa có đợt nào phủ. Có vạch đánh dấu **Hôm nay**. Nhìn thanh này là thấy ngay đơn vị còn hở tháng nào.

**Phần mềm có thể hiện banner cảnh báo màu vàng ngay trên dải độ phủ:**

- *Có đợt chồng lấn nhau* — hai đợt có ngày trùng nhau.
- *Các đợt chưa phủ kín cả năm* — có khoảng ngày không thuộc đợt nào, banner liệt kê rõ các khoảng trống đó.

Cả hai **chỉ là nhắc nhở, phần mềm vẫn cho lưu**. Nhưng nên xử lý: chỗ không đợt nào phủ chính là chỗ người ta bị sót. Xem thêm **B3**.

<!-- ảnh: màn Đợt trao huy hiệu với 3 đợt (cố ý thiếu một đợt để có khoảng trống) — thấy banner vàng "Các đợt chưa phủ kín cả năm", dải độ phủ 12 tháng có mảng xám, và bảng 4 đợt -->
> **[Ảnh 6]** Danh sách đợt, dải độ phủ và banner cảnh báo — `docs/images/06-danh-sach-dot.png`

**Sửa hoặc xóa đợt:** bấm biểu tượng bút chì (**Sửa**) hoặc thùng rác (**Xóa**) ở cột **Thao tác**. Xóa thì phần mềm hỏi lại *"Xóa đợt … ?"*; bấm **Xóa đợt này** để đồng ý, **Để lại** để thôi. Sửa hay xóa đợt đều **có hiệu lực ngay cho mọi năm**.

<a id="a4-nap-danh-sach-dang-vien"></a>
### A4. Nạp danh sách đảng viên

Có hai cách. Danh sách dài thì dùng **Import Excel**; thêm vài người thì nhập tay.

#### Cách 1 — Import từ file Excel (dùng cho danh sách dài)

1. Bấm **Đảng viên** ở menu trái, rồi bấm **Import Excel**. Phần mềm mở trang **Import danh sách đảng viên** gồm **3 bước**: **Chọn file** → **Xem trước** → **Kết quả**.

**Bước 1 — Chọn file**

2. Bấm **Tải file mẫu**. Bạn nhận một file `.xlsx` có sẵn **4 cột đúng thứ tự** và **hai dòng ví dụ** (*Nguyễn Văn Mẫu*, *Trần Thị Mẫu*).
3. Mở file mẫu bằng Excel, **xóa hai dòng ví dụ**, điền danh sách thật vào. Giữ nguyên dòng tiêu đề và thứ tự cột:

| Cột | Bắt buộc | Cách ghi |
|---|---|---|
| Họ tên | **Có** | Ghi đầy đủ, ví dụ `Nguyễn Văn An` |
| Ngày sinh | Không | `dd/MM/yyyy`, ví dụ `12/03/1958` |
| Giới tính | Không | `Nam` hoặc `Nữ`, hoặc để trống |
| Ngày vào Đảng chính thức | **Có** | `dd/MM/yyyy`, ví dụ `15/10/1996`. Không được là ngày trong tương lai |

4. Lưu file. File phải là **`.xlsx`** và **không quá 10 MB**.
5. Quay lại phần mềm, kéo file thả vào khung **Kéo thả file Excel vào đây**, hoặc bấm **chọn file từ máy**.
6. Bấm **Tiếp tục ›**.

<!-- ảnh: Import bước 1 — vùng kéo thả đã nhận file "danh-sach-mau.xlsx", bảng mô tả 4 cột bên dưới, nút Tải file mẫu và nút Tiếp tục -->
> **[Ảnh 7]** Bước 1 — chọn file và nút Tải file mẫu — `docs/images/07-import-b1.png`

**Bước 2 — Xem trước**

7. Phần mềm đọc file và hiện dòng tóm tắt: *"Sẽ thêm **N** người mới · **M** dòng lỗi bị bỏ qua"*, kèm câu nhắc *"Hệ thống không kiểm tra trùng — nếu đã nạp file này trước đó, hãy Hủy."*
8. Có hai tab: **Hợp lệ (N)** và **Lỗi (M)**.
9. Mở tab **Hợp lệ** để soát lại những dòng sắp được nạp.

<!-- ảnh: Import bước 2, tab Hợp lệ đang mở, thấy dòng tóm tắt và bảng vài người hợp lệ -->
> **[Ảnh 8]** Bước 2 — tóm tắt và tab Hợp lệ — `docs/images/08-import-b2-hop-le.png`

10. Mở tab **Lỗi** để xem bảng liệt kê từng dòng hỏng. Bảng có cột **Dòng** (số dòng trong file Excel), bốn cột dữ liệu gốc — ô sai bị tô đỏ — và cột **Lý do**.
11. Còn sửa được thì bấm **Hủy**, sửa file Excel rồi làm lại từ bước 1. Chấp nhận bỏ qua các dòng lỗi thì bấm **Nạp N dòng hợp lệ**.

<!-- ảnh: Import bước 2, tab Lỗi đang mở, có ít nhất 3 dòng lỗi khác loại, thấy rõ cột Dòng và cột Lý do, ô sai tô đỏ -->
> **[Ảnh 9]** Bước 2 — tab Lỗi và cột Lý do — `docs/images/09-import-b2-loi.png`

**Các lý do lỗi và cách sửa:**

| Lý do phần mềm ghi | Cách sửa |
|---|---|
| Thiếu họ tên | Điền họ tên vào ô đang trống |
| Thiếu ngày vào Đảng chính thức | Điền ngày. Đây là ô bắt buộc |
| Sai định dạng ngày (cần dd/MM/yyyy) | Ghi đúng `dd/MM/yyyy`. Ngày không có thật như `31/02/1974` cũng bị báo lỗi |
| Ngày chính thức ở tương lai | Kiểm tra lại, thường là gõ nhầm năm |
| Giới tính chỉ nhận Nam hoặc Nữ | Sửa thành `Nam` hoặc `Nữ`, hoặc để trống |
| Ngày sinh phải trước ngày vào Đảng chính thức | Kiểm tra lại hai ngày, thường là gõ đổi chỗ |

Một dòng sai nhiều chỗ thì cột **Lý do** ghi đủ cả, ngăn nhau bằng dấu `;`. Bạn sửa một lượt là xong, không phải nạp đi nạp lại nhiều vòng.

**Bước 3 — Kết quả**

12. Phần mềm báo: *"Đã thêm **N** người"* và *"Bỏ qua **M** dòng lỗi"*.
13. Bấm **Về danh sách đảng viên** để xem lại, hoặc **Import file khác** nếu còn file nữa.

<!-- ảnh: Import bước 3 — dấu tích xanh, câu "Đã thêm 24 người", hai nút Import file khác và Về danh sách đảng viên -->
> **[Ảnh 10]** Bước 3 — kết quả nạp — `docs/images/10-import-b3.png`

#### Cách 2 — Thêm từng người bằng tay

1. Bấm **Đảng viên**, rồi bấm **+ Thêm**.
2. Cửa sổ **Thêm đảng viên** hiện ra. Điền:
   - **Họ tên** — bắt buộc.
   - **Ngày vào Đảng chính thức** — bắt buộc, dạng `dd/mm/yyyy`. Phần mềm không cho chọn ngày sau hôm nay.
   - **Ngày sinh** — để trống cũng được. Phải trước ngày vào Đảng chính thức.
   - **Giới tính** — bấm chọn **Nam**, **Nữ** hoặc **Để trống**.
3. Bấm **Thêm vào danh sách**.

<!-- ảnh: cửa sổ "Thêm đảng viên" đã điền Họ tên "Nguyễn Văn An", ngày vào Đảng 15/10/1996, giới tính Nam -->
> **[Ảnh 11]** Cửa sổ Thêm đảng viên — `docs/images/11-them-dang-vien.png`

**Đọc bảng danh sách đảng viên:**

| Cột | Nghĩa |
|---|---|
| Họ tên · Giới tính · Ngày sinh | Thông tin đảng viên |
| Ngày chính thức | Ngày vào Đảng chính thức |
| Tuổi đảng | Số năm tuổi đảng tính đến hôm nay |
| Mốc kế tiếp | Mốc tuổi đảng sắp tới của người đó |
| Ngày tròn mốc kế tiếp | Ngày người đó tròn mốc kế tiếp |
| Thao tác | Nút **Sửa** hình bút chì |

Bấm vào tên cột để đổi cách sắp xếp. Cột **Tuổi đảng** là số phần mềm tự tính nên không sắp xếp được.

Phía trên bảng có ô **Tìm theo họ tên…** và ô lọc **Giới tính: Tất cả / Nam / Nữ**.

**Sửa một người:** bấm nút bút chì ở cột **Thao tác**. Cửa sổ **Sửa đảng viên** hiện ra với dữ liệu cũ. Sửa xong bấm **Lưu thay đổi**.

<!-- ảnh: màn Đảng viên có khoảng 8 người, thấy ô Tìm theo họ tên, ô lọc Giới tính, đủ các cột kể cả Tuổi đảng và Mốc kế tiếp -->
> **[Ảnh 12]** Danh sách đảng viên — `docs/images/12-dang-vien.png`

**Xóa người khỏi danh sách:**

1. Tích vào ô vuông ở đầu dòng của người cần xóa. Tích một người cũng được, nhiều người cũng được.
2. Dòng được tích đổi sang nền vàng nhạt, và phía trên bảng hiện dòng chữ *"Đang chọn N dòng"*.
3. Bấm nút **hình thùng rác màu đỏ** ở đầu trang, cạnh nút **+ Thêm**. Nút này chỉ hiện khi có ít nhất một dòng được tích.
4. Phần mềm hỏi lại: *"Xóa N người khỏi danh sách?"*. Bấm **Xóa N người** để đồng ý, **Để lại** để thôi.

Đổi trang hoặc đổi bộ lọc thì các dòng đang tích bị bỏ đánh dấu. Đó là cố ý, để bạn không xóa nhầm người không còn nhìn thấy trên màn hình.

> ⚠️ **Xóa là xóa hẳn.** Không có thùng rác, không khôi phục lại được. Đọc kỹ con số trong hộp xác nhận trước khi bấm đồng ý.

<!-- ảnh: màn Đảng viên đang tích chọn 3 người (3 dòng nền vàng), thấy chữ "Đang chọn 3 dòng", nút thùng rác đỏ ở đầu trang, và hộp xác nhận xóa đang mở -->
> **[Ảnh 13]** Tích chọn 3 người, nút thùng rác đỏ và hộp xác nhận xóa — `docs/images/13-xoa-nhieu.png`

<a id="a5-xem-dashboard-va-xuat-excel"></a>
### A5. Xem Dashboard và xuất Excel

Xong A2, A3, A4 là phần mềm đã đủ dữ liệu để làm việc.

1. Bấm **Dashboard** ở menu trái.
2. Phía trên là thẻ **Đợt sắp tới**: tên đợt, khoảng ngày đã gắn năm, còn bao nhiêu ngày nữa (hoặc chữ **Đang diễn ra**), số **người đủ điều kiện**, và phần **Phân bổ theo mốc** — ví dụ *30 năm: 3 · 40 năm: 1*.
3. Bên dưới là bảng **Danh sách đủ điều kiện — <tên đợt> năm <năm>** của chính đợt đó.

**Phần mềm tự chọn đợt sắp tới như sau:** gắn năm hiện tại vào tất cả các đợt, rồi lấy đợt gần nhất chưa kết thúc. Hôm nay đang nằm trong một đợt thì lấy chính đợt đó. Mọi đợt trong năm đã qua hết thì phần mềm lấy **đợt sớm nhất của năm sau**, và gắn nhãn **Năm sau** lên thẻ — nên đừng ngạc nhiên khi thấy năm sau hiện ra vào tháng 12.

**Đọc bảng đủ điều kiện:**

| Cột | Nghĩa |
|---|---|
| STT | Số thứ tự |
| Họ tên · Giới tính · Ngày sinh | Thông tin đảng viên |
| Ngày vào Đảng chính thức | Ngày được công nhận đảng viên chính thức |
| Ngày tròn mốc | Ngày người đó tròn số năm tuổi đảng của mốc |
| Mốc huy hiệu | Huy hiệu được trao: 30 năm, 40 năm… |

Danh sách sắp theo **mốc huy hiệu tăng dần**, trong cùng một mốc thì theo **họ tên đầy đủ** theo bảng chữ cái tiếng Việt.

4. Bấm **Xuất Excel** ở góc trên bên phải bảng. Trình duyệt tải về một file tên dạng `DuDieuKien_Dot7-11_2026.xlsx`.

**File Excel gồm:** dòng 1 là tên đơn vị (bỏ dòng này nếu bạn để trống Tên đơn vị), dòng 2 là tên đợt kèm khoảng ngày, dòng 3 là ngày xuất, dòng 4 để trống, dòng 5 là tiêu đề 7 cột, từ dòng 6 là dữ liệu. Ô nào không có dữ liệu thì để trống.

5. Mở file bằng Excel, chỉnh trình bày theo mẫu tờ trình của đơn vị rồi in.

<!-- ảnh: Dashboard đầy đủ dữ liệu — thẻ "Đợt sắp tới" có số người và phân bổ theo mốc, bảng Danh sách đủ điều kiện bên dưới, nút Xuất Excel -->
> **[Ảnh 14]** Dashboard đầy đủ: thẻ Đợt sắp tới và bảng Danh sách đủ điều kiện — `docs/images/14-dashboard-day-du.png`

<!-- ảnh: file DuDieuKien_Dot7-11_2026.xlsx mở bằng Excel, thấy rõ 3 dòng tiêu đề, dòng trống, dòng tên cột và vài dòng dữ liệu -->
> **[Ảnh 15]** File Excel xuất ra, mở bằng Excel — `docs/images/15-file-excel.png`

Đến đây phần chuẩn bị đã xong. Những lần sau bạn chỉ làm theo **Phần B**.

---

<a id="phan-b--viec-lam-dinh-ky-moi-dot"></a>
## Phần B — Việc làm định kỳ mỗi đợt

Đây là việc lặp lại mỗi kỳ trao huy hiệu. Chỉ ba bước, thường mất mười lăm phút.

<a id="b1-cap-nhat-dang-vien"></a>
### B1. Cập nhật đảng viên

Từ đợt trước đến nay có đảng viên mới chuyển đến, hoặc có người mới được công nhận chính thức thì thêm họ vào:

- Vài người → **Đảng viên** → **+ Thêm** → điền → **Thêm vào danh sách**.
- Cả danh sách dài → **Đảng viên** → **Import Excel** → làm như **A4**.

Người đã chuyển đi hoặc từ trần thì tích chọn rồi bấm nút thùng rác đỏ để xóa.

Nhân tiện sửa luôn những chỗ sai bạn phát hiện: gõ nhầm ngày, sai tên. Sửa xong là mọi danh sách tự đúng theo.

> ⚠️ **Nhớ lại: import không kiểm tra trùng.** Trước khi nạp một file, hãy dùng ô **Tìm theo họ tên…** để kiểm tra vài cái tên xem đã có chưa. Lỡ nạp trùng thì tích chọn các dòng thừa rồi xóa.

<a id="b2-kiem-tra-danh-sach-va-xuat-excel"></a>
### B2. Kiểm tra danh sách và xuất Excel

Có hai đường, chọn đường nào cũng ra cùng một danh sách.

**Đường 1 — qua Dashboard.** Nhanh nhất khi bạn làm cho **đợt sắp tới**.

1. Bấm **Dashboard**.
2. Đối chiếu thẻ **Đợt sắp tới**: đúng đợt bạn cần chưa?
3. Xem bảng bên dưới, bấm **Xuất Excel**.

**Đường 2 — qua màn hình đợt.** Dùng khi bạn cần **một đợt cụ thể**, hoặc cần **năm khác**.

1. Bấm **Đợt trao huy hiệu**.
2. Bấm nút **Xem chi tiết** ở cột Thao tác của đợt cần xem. Trang chi tiết đợt có hai tab: **Thông tin** và **Danh sách đủ điều kiện**.
3. Mở tab **Danh sách đủ điều kiện**.
4. Trên tab có **bộ chọn năm** gồm ba năm liền nhau, ví dụ **2025 · 2026 · 2027**. Mặc định là năm nay. Chọn năm sau để chuẩn bị trước; phần mềm gắn nhãn **Năm sau** cạnh khoảng ngày và ghi *"N người · chuẩn bị trước"* để bạn không nhầm.
5. Bấm **Xuất Excel**.

<!-- ảnh: trang chi tiết đợt, tab "Danh sách đủ điều kiện" đang mở, bộ chọn năm 2025·2026·2027 với 2026 đang chọn, bảng danh sách và nút Xuất Excel -->
> **[Ảnh 16]** Trang chi tiết đợt: tab Danh sách đủ điều kiện và bộ chọn năm — `docs/images/16-chi-tiet-dot.png`

**Nên đối chiếu trước khi in:** số người ghi ở chân bảng có khớp số dòng không, các mốc có hợp lý không. Thấy thiếu người thì sang **B3**.

<a id="b3-ra-soat-nguoi-chua-thuoc-dot-nao"></a>
### B3. Rà soát người chưa thuộc đợt nào

Đây là bước dễ bỏ qua nhất, nhưng là bước tránh sót người.

**"Chưa thuộc đợt nào" nghĩa là gì:** người đó **có** tròn mốc tuổi đảng trong năm, nhưng ngày tròn mốc **không rơi vào khoảng ngày của bất kỳ đợt nào**. Họ không xuất hiện trong bất cứ danh sách đủ điều kiện nào. Nếu bạn không mở màn hình này, họ bị bỏ quên cả năm.

1. Nhìn menu trái. Mục **Chưa thuộc đợt nào** có con số đỏ không? Có nghĩa là đang có người bị sót.
2. Bấm vào mục đó.
3. Chọn năm ở bộ chọn năm phía trên bên phải (mặc định năm nay).
4. Bảng **Bị sót trong năm <năm>** hiện danh sách, giống bảng đủ điều kiện nhưng có thêm cột **Khoảng trống** cho biết người đó rơi vào chỗ hở nào: *"Giữa Đợt A và Đợt B"*, *"Trước đợt đầu tiên"*, hoặc *"Sau đợt cuối cùng"*. Bảng sắp theo **Ngày tròn mốc** rồi **Họ tên**.

<!-- ảnh: màn Chưa thuộc đợt nào với 3–4 người, thấy bộ chọn năm, khối gợi ý nới đợt, và cột Khoảng trống -->
> **[Ảnh 17]** Màn hình Chưa thuộc đợt nào, có cột Khoảng trống — `docs/images/17-chua-thuoc-dot-nao.png`

**Xử lý thế nào:** phần mềm gợi ý ngay trên màn hình — **nới Đến ngày của đợt trước, hoặc nới Từ ngày của đợt sau**, sao cho khoảng trống được phủ. Có nút đi thẳng sang màn hình **Đợt trao huy hiệu**.

1. Bấm nút đó, bấm **Sửa** ở đợt cần nới, cho khoảng ngày rộng ra.
2. Bấm **Lưu thay đổi**.
3. Quay lại **Chưa thuộc đợt nào**. Con số phải giảm hoặc về không.
4. Quay lại **B2** và xuất lại file Excel — danh sách giờ đã có thêm những người vừa được phủ.

Không muốn sửa đợt cũng được: bạn đã **nhìn thấy** họ, và có thể bấm **Xuất Excel** ngay trên màn hình này để có danh sách riêng (tên file dạng `ChuaThuocDot_2026.xlsx`) mà xử lý bằng cách khác.

Màn hình ghi *"Không có ai bị sót trong năm 2026."* là tốt — năm đó các đợt đã phủ hết.

---

<a id="phan-c--giai-thich-tu-ngu"></a>
## Phần C — Giải thích từ ngữ

| Từ | Nghĩa |
|---|---|
| **Ngày vào Đảng chính thức** | Ngày ghi trong quyết định công nhận đảng viên chính thức. Mọi tính toán đều đếm từ ngày này. Ở bảng Đảng viên và trong file Excel, cột này ghi gọn là **Ngày chính thức**. |
| **Tuổi đảng** | Số năm tròn tính từ ngày vào Đảng chính thức đến hôm nay. |
| **Mốc tuổi đảng** (gọi tắt: mốc) | Số năm tuổi đảng được trao huy hiệu: 30, 35, 40… Sinh ra từ ba ô số trong Cài đặt. |
| **Ngày tròn mốc** | Ngày vào Đảng chính thức cộng thêm số năm của mốc. Ai vào Đảng ngày `05/03/1980` thì tròn mốc 45 năm vào `05/03/2025`. |
| **Đợt trao huy hiệu** (gọi tắt: đợt) | Một khoảng ngày trong năm dùng để gom những người tròn mốc trong khoảng đó. Chỉ lưu ngày và tháng, dùng chung cho mọi năm. |
| **Đủ điều kiện** | Có ngày tròn mốc rơi vào khoảng ngày của một đợt, trong một năm cụ thể. |
| **Chưa thuộc đợt nào** | Có tròn mốc trong năm, nhưng ngày tròn mốc không rơi vào đợt nào. |

**Một người có thể đủ điều kiện ở hai đợt trong cùng một năm không?** Không. Các mốc cách nhau ít nhất một năm, còn một đợt luôn ngắn hơn một năm, nên mỗi người có nhiều nhất một mốc trong một đợt.

**Ngày 29/02 thì tính sao?** Người vào Đảng ngày 29/02 mà năm tròn mốc không nhuận thì phần mềm lấy **28/02** của năm đó.

---

<a id="phan-d--nhung-cho-de-nham"></a>
## Phần D — Những chỗ dễ nhầm

**1. Import nạp trùng người.** Phần mềm không kiểm tra trùng. Nạp một file hai lần là có hai bản của mỗi người. Trước khi nạp, tìm thử vài cái tên trong danh sách đảng viên. Lỡ nạp trùng thì tích chọn các dòng thừa rồi xóa.

**2. Sửa đợt hoặc sửa Cài đặt có tác dụng ngay lập tức, cho mọi năm.** Kể cả các đợt đã qua. Sửa xong mà thấy con số ở Dashboard nhảy thì đó là phần mềm chạy đúng, không phải lỗi.

**3. Đợt không phủ kín thì sót người.** Đây là cách sót người phổ biến nhất, và phần mềm không tự sửa giúp. Nhìn **dải độ phủ** ở màn hình đợt, và mở **Chưa thuộc đợt nào** mỗi kỳ.

**4. Xóa là xóa hẳn.** Không có thùng rác. Đọc con số trong hộp xác nhận trước khi đồng ý.

**5. Số ở Dashboard khác số ở màn hình đợt.** Thường là do khác năm: Dashboard luôn hiện **đợt sắp tới** — có thể đã sang năm sau — còn trang chi tiết đợt hiện năm bạn tự chọn. Kiểm tra lại bộ chọn năm.

**6. Danh sách trống mà đáng lẽ phải có người.** Kiểm tra theo thứ tự: đã nhập đảng viên chưa (màn **Đảng viên**), đã tạo đợt chưa (màn **Đợt trao huy hiệu**), mốc trong **Cài đặt** có đúng không, và đang xem **năm** nào.

**7. Tích chọn rồi đổi trang thì mất dấu tích.** Phần mềm cố ý bỏ đánh dấu khi bạn đổi trang hoặc đổi bộ lọc, để tránh xóa nhầm. Xóa xong từng trang một.

---

<a id="phan-e--khi-gap-truc-trac"></a>
## Phần E — Khi gặp trục trặc

| Hiện tượng | Xử lý |
|---|---|
| Báo *"Sai tài khoản hoặc mật khẩu"* | Gõ lại, chú ý phím Caps Lock. Vẫn không được thì hỏi người cài đặt phần mềm. |
| Báo *"Phiên làm việc đã hết hạn"* và đẩy về màn hình đăng nhập | Phiên làm việc dài 8 giờ, hết hạn thì phải đăng nhập lại. Dữ liệu đã lưu không mất. |
| Báo *"Không kết nối được tới máy chủ"* | Máy chủ chưa chạy hoặc mạng nội bộ có vấn đề. Báo người quản trị. |
| Không tải được file Excel | Kiểm tra mục tải xuống của trình duyệt; trình duyệt có thể đã chặn. Cho phép tải rồi bấm **Xuất Excel** lại. |
| Import báo *"Chỉ nhận file .xlsx"* | File đang là `.xls` hoặc `.csv`. Mở bằng Excel rồi lưu lại dạng `.xlsx`. |
| Import báo *"File vượt quá 10 MB"* | Chia danh sách thành nhiều file nhỏ rồi nạp lần lượt. |
| Import báo *"File phải có đúng 4 cột…"* | Sai số cột hoặc sai thứ tự cột. Tải lại **file mẫu** và chép dữ liệu sang. |
| Import báo *"File không có dòng dữ liệu nào"* | File chỉ có dòng tiêu đề. Điền dữ liệu rồi nạp lại. |
| Màn hình trắng hoặc không phản hồi | Tải lại trang (phím `F5`). Vẫn vậy thì báo người quản trị. |
| Mất dữ liệu, cần khôi phục | Việc này do người quản trị làm. Xem phần **Sao lưu dữ liệu** ngay dưới. |

Trục trặc không nằm trong bảng trên thì ghi lại **bạn đang làm gì**, **màn hình nào**, **phần mềm báo chữ gì**, chụp màn hình rồi gửi cho người quản trị.

---

<a id="sao-luu-du-lieu"></a>
## Sao lưu dữ liệu

Việc sao lưu **không làm trên giao diện**. Người quản trị chạy lệnh `pg_dump` trên máy đặt phần mềm.

Cách sao lưu, cách khôi phục và cách chuyển dữ liệu sang máy khác ghi trong `README.md`, mục **3. Sao lưu và khôi phục dữ liệu**.

Việc của cán bộ dùng phần mềm chỉ là: **nhắc người quản trị sao lưu định kỳ**, nhất là trước mỗi lần nạp một file Excel lớn.

---

<a id="phu-luc--danh-sach-anh-man-hinh"></a>
## Phụ lục — Danh sách ảnh màn hình

Phần chữ đã xong. **Ảnh màn hình chưa chụp.**

Mỗi chỗ cần ảnh trong tài liệu này có hai dòng: một chú thích `<!-- ảnh: … -->` mô tả cảnh cần chụp, và một dòng `> **[Ảnh N]** …`. Khi có ảnh, thay **nguyên dòng `> **[Ảnh N]** …`** bằng:

```markdown
![Mô tả ngắn](images/NN-ten-anh.png)
```

Giữ lại dòng `<!-- ảnh: … -->` để lần sau chụp lại cho đúng cảnh cũ. Ảnh đặt trong `docs/images/`, đặt tên đúng như cột **Tên tệp** dưới đây.

| # | Cảnh cần chụp | Tên tệp |
|---|---|---|
| 1 | Dashboard đã có dữ liệu, chụp đủ chiều ngang: menu trái 5 mục, badge đỏ ở "Chưa thuộc đợt nào", thanh trên cùng | `01-khung-man-hinh.png` |
| 2 | Màn hình Đăng nhập, ô Tài khoản đã điền, ô Mật khẩu để trống | `02-dang-nhap.png` |
| 3 | Dashboard khi cơ sở dữ liệu còn trắng: khối "Bắt đầu với ba bước" và các thẻ cảnh báo | `03-dashboard-trong.png` |
| 4 | Cài đặt: ba ô 30 / 90 / 5, dãy mốc xem trước, khung cảnh báo vàng, ô Tên đơn vị "Đảng ủy Phường X" | `04-cai-dat-moc.png` |
| 5 | Cửa sổ "Thêm đợt trao huy hiệu" đã điền Đợt 7/11, 01/10 – 07/11 | `05-them-dot.png` |
| 6 | Màn Đợt trao huy hiệu với 4 đợt nhưng cố ý còn khoảng hở: banner vàng "Các đợt chưa phủ kín cả năm", dải độ phủ có mảng xám, bảng đợt | `06-danh-sach-dot.png` |
| 7 | Import bước 1: vùng kéo thả đã nhận file, bảng mô tả 4 cột, nút Tải file mẫu và Tiếp tục | `07-import-b1.png` |
| 8 | Import bước 2, tab **Hợp lệ**: dòng tóm tắt "Sẽ thêm N người mới…" và bảng dòng hợp lệ | `08-import-b2-hop-le.png` |
| 9 | Import bước 2, tab **Lỗi**: ít nhất 3 dòng lỗi khác loại, thấy rõ cột Dòng, cột Lý do, ô sai tô đỏ | `09-import-b2-loi.png` |
| 10 | Import bước 3: dấu tích xanh, "Đã thêm N người", hai nút cuối | `10-import-b3.png` |
| 11 | Cửa sổ "Thêm đảng viên" đã điền đủ bốn ô | `11-them-dang-vien.png` |
| 12 | Màn Đảng viên khoảng 8 người: ô Tìm theo họ tên, ô lọc Giới tính, đủ các cột kể cả Tuổi đảng và Mốc kế tiếp | `12-dang-vien.png` |
| 13 | Màn Đảng viên đang tích chọn 3 người: 3 dòng nền vàng, chữ "Đang chọn 3 dòng", nút thùng rác đỏ ở đầu trang, hộp xác nhận xóa đang mở | `13-xoa-nhieu.png` |
| 14 | Dashboard đầy đủ: thẻ "Đợt sắp tới" có số người và Phân bổ theo mốc, bảng Danh sách đủ điều kiện, nút Xuất Excel | `14-dashboard-day-du.png` |
| 15 | File `DuDieuKien_Dot7-11_2026.xlsx` mở bằng Excel: 3 dòng tiêu đề, dòng trống, dòng tên cột, vài dòng dữ liệu | `15-file-excel.png` |
| 16 | Trang chi tiết đợt, tab "Danh sách đủ điều kiện": bộ chọn năm ba năm, bảng danh sách, nút Xuất Excel | `16-chi-tiet-dot.png` |
| 17 | Màn Chưa thuộc đợt nào với 3–4 người: bộ chọn năm, khối gợi ý nới đợt, cột Khoảng trống | `17-chua-thuoc-dot-nao.png` |

**Khi chụp ảnh, nhớ:**

- Chạy bằng `docker compose`, dùng **dữ liệu mẫu tự tạo**. Tên người phải là tên không có thật. Không dùng dữ liệu thật của đơn vị nào.
- Đặt **Tên đơn vị** là *"Đảng ủy Phường X"* cho thống nhất giữa mọi ảnh.
- Chụp đủ chiều ngang màn hình để thấy cả menu trái.
- Dữ liệu mẫu nên cố ý để **hở một khoảng giữa hai đợt**, để ảnh 6 và ảnh 17 có cái để chỉ.

---

## Nguồn của tài liệu này

Phần chữ dựng theo `docs/2026-09-17-huyhieudang-business-design.md` (v1.1) — luồng 6.2 và 6.3 — `docs/api-contract.md`, và câu chữ đọc thẳng từ mã nguồn giao diện tại `FE/src/pages/`. Giao diện thật khác tài liệu chỗ nào thì sửa tài liệu này, và ghi một dòng vào `CHANGELOG.md`.
