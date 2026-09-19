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
  - [B1. Cập nhật đảng viên mới](#b1-cap-nhat-dang-vien-moi)
  - [B2. Kiểm tra danh sách và xuất Excel](#b2-kiem-tra-danh-sach-va-xuat-excel)
  - [B3. Rà soát người chưa thuộc đợt nào](#b3-ra-soat-nguoi-chua-thuoc-dot-nao)
- [Phần C — Giải thích từ ngữ](#phan-c--giai-thich-tu-ngu)
- [Phần D — Những chỗ dễ nhầm](#phan-d--nhung-cho-de-nham)
- [Phần E — Khi gặp trục trặc](#phan-e--khi-gap-truc-trac)
- [Phụ lục — Danh sách ảnh màn hình](#phu-luc--danh-sach-anh-man-hinh)

---

<a id="truoc-khi-bat-dau"></a>
## Trước khi bắt đầu

**Bạn cần có:** địa chỉ phần mềm (ví dụ `http://localhost:5173`), tên tài khoản và mật khẩu. Người cài đặt phần mềm sẽ đưa cho bạn ba thứ này.

**Màn hình nào cũng có ba phần giống nhau:**

- **Thanh menu bên trái** có 5 mục: **Dashboard**, **Đảng viên**, **Đợt trao huy hiệu**, **Chưa thuộc đợt nào**, **Cài đặt**. Bấm vào biểu tượng mũi tên để thu gọn hoặc mở rộng thanh này.
- Mục **Chưa thuộc đợt nào** có thể hiện một **con số nhỏ màu đỏ**. Đó là số người tròn mốc trong năm nay nhưng không rơi vào đợt nào. Không có ai thì con số này ẩn đi.
- **Thanh trên cùng** hiện tên hệ thống, **tên đơn vị** của bạn, ngày hôm nay, tên tài khoản và nút **Đăng xuất**.

> **[Ảnh 1]** Khung màn hình chung: menu trái 5 mục và thanh trên cùng — `docs/img/huong-dan/01-khung-man-hinh.png`

**Một điều quan trọng cần nhớ ngay:** phần mềm **không lưu sẵn** danh sách đủ điều kiện. Mỗi lần bạn mở một danh sách, phần mềm tính lại từ đầu. Vì vậy chỉ cần sửa ngày của một người, sửa một đợt, hay đổi mốc trong Cài đặt là mọi danh sách đổi theo ngay. Bạn không phải bấm nút "tính lại" nào cả.

---

<a id="phan-a--lan-dau-tien-dung"></a>
## Phần A — Lần đầu tiên dùng

Phần này làm **một lần duy nhất**, theo đúng thứ tự 5 bước. Làm sai thứ tự thì các bước sau sẽ hiện danh sách trống.

<a id="a1-dang-nhap"></a>
### A1. Đăng nhập

1. Mở trình duyệt, gõ địa chỉ phần mềm.
2. Nhập tên tài khoản và mật khẩu.
3. Bấm **Đăng nhập**.

Vào được thì phần mềm mở thẳng **Dashboard**.

Nếu sai, phần mềm chỉ báo một câu chung: *"Sai tài khoản hoặc mật khẩu"*. Phần mềm cố ý không nói sai ở chỗ nào, để người lạ không dò được tài khoản. Gõ lại cho kỹ, chú ý phím Caps Lock.

> **[Ảnh 2]** Màn hình Đăng nhập — `docs/img/huong-dan/02-dang-nhap.png`

**Lần đầu vào, Dashboard sẽ trống.** Đó là đúng — bạn chưa nhập gì cả. Phần mềm hiện khối **hướng dẫn 3 bước** kèm nút đi thẳng tới từng màn hình. Ba bước đó chính là A2, A3, A4 dưới đây.

> **[Ảnh 3]** Dashboard khi chưa có dữ liệu, kèm khối hướng dẫn 3 bước — `docs/img/huong-dan/03-dashboard-trong.png`

<a id="a2-kiem-tra-moc-tuoi-dang-trong-cai-dat"></a>
### A2. Kiểm tra mốc tuổi đảng trong Cài đặt

**Mốc tuổi đảng** là các năm tuổi đảng được trao huy hiệu: 30 năm, 40 năm, 45 năm… Phần mềm không bắt bạn gõ từng mốc. Bạn chỉ cho ba con số, phần mềm tự sinh ra cả dãy.

1. Bấm **Cài đặt** ở menu trái.
2. Nhìn ba ô số:
   - **Bắt đầu** — mốc đầu tiên. Mặc định **30**.
   - **Kết thúc** — mốc cuối cùng. Mặc định **90**.
   - **Bước** — khoảng cách giữa hai mốc liền nhau. Mặc định **5**.
3. Ngay dưới ba ô là phần **xem trước dãy mốc**. Với 30 / 90 / 5, phần mềm hiện: 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90 — kèm dòng "13 mốc".
4. Dãy này đúng với quy định của đơn vị bạn thì **không phải sửa gì**. Bấm **Lưu** hoặc rời màn hình đều được.
5. Cần sửa thì gõ số mới. Phần xem trước đổi theo ngay khi bạn gõ, **trước khi** bấm Lưu. Xem thấy đúng rồi hãy bấm **Lưu**.

Bấm nhầm thì có nút **Khôi phục mặc định 30 / 90 / 5** đưa ba ô về như cũ.

Ba ô đều phải là **số nguyên dương**, và **Bắt đầu** không được lớn hơn **Kết thúc**. Gõ sai thì phần mềm báo ngay và không cho lưu.

> **[Ảnh 4]** Màn hình Cài đặt: ba ô số và phần xem trước dãy mốc — `docs/img/huong-dan/04-cai-dat-moc.png`

**Nhân tiện, điền luôn Tên đơn vị.** Cũng trên màn hình này có ô **Tên đơn vị**, ví dụ *"Đảng ủy Phường X"*. Tên này hiện trên thanh trên cùng của mọi màn hình, và quan trọng hơn: nó được in thành **dòng đầu tiên** của mọi file Excel bạn xuất ra. Để trống cũng được — khi đó file Excel không có dòng tên đơn vị.

> ⚠️ **Đổi mốc là đổi mọi thứ.** Ba con số này ảnh hưởng tới **mọi danh sách, mọi năm**, kể cả các đợt đã qua. Đổi Bước từ 5 thành 10 là lập tức mất các mốc 35, 45, 55… và danh sách đủ điều kiện ngắn lại. Chỉ đổi khi đơn vị thực sự có quy định khác.

<a id="a3-tao-cac-dot-trao-huy-hieu"></a>
### A3. Tạo các đợt trao huy hiệu

**Đợt trao huy hiệu** là một khoảng ngày trong năm để gom những người tròn mốc trong khoảng đó. Đợt **chỉ lưu ngày và tháng**, không lưu năm — tạo một lần rồi dùng cho mọi năm về sau.

1. Bấm **Đợt trao huy hiệu** ở menu trái.
2. Bấm nút **Thêm đợt**.
3. Điền ba ô:
   - **Tên** — ví dụ *"Đợt 7/11"*. Tên không được trùng với đợt đã có.
   - **Từ ngày** — dạng ngày/tháng, ví dụ `01/10`.
   - **Đến ngày** — dạng ngày/tháng, ví dụ `07/11`.
4. Bấm **Lưu**.
5. Làm lại cho từng đợt. Thông thường mỗi năm có 4 đợt, quanh các ngày **3/2**, **19/5**, **2/9** và **7/11**.

Một đợt phải **nằm gọn trong một năm**: Từ ngày không được sau Đến ngày, và đợt không được vắt qua ngày 31/12 sang 01/01.

> **[Ảnh 5]** Danh sách đợt trao huy hiệu và ô Thêm đợt — `docs/img/huong-dan/05-them-dot.png`

**Đọc bảng danh sách đợt:**

| Cột | Nghĩa |
|---|---|
| Tên | Tên đợt bạn đặt |
| Từ ngày · Đến ngày | Khoảng ngày trong năm, dạng `dd/MM` |
| Trạng thái năm nay | **Đã qua**, **Đang diễn ra**, hoặc **Sắp tới** kèm số ngày còn lại |
| Số người đủ điều kiện năm nay | Đếm theo năm hiện tại |

**Dải độ phủ** phía trên bảng là một thanh ngang 12 tháng. Mỗi đợt là một vạch màu, chỗ nào **xám** là chưa có đợt nào phủ. Có vạch đánh dấu **Hôm nay**. Nhìn thanh này là thấy ngay đơn vị còn hở tháng nào.

> **[Ảnh 6]** Dải độ phủ 12 tháng, có khoảng trống màu xám — `docs/img/huong-dan/06-dai-do-phu.png`

**Phần mềm có thể hiện banner cảnh báo màu vàng:**

- *Hai đợt chồng lấn nhau* — hai đợt có ngày trùng nhau.
- *Các đợt chưa phủ kín cả năm* — có khoảng ngày không thuộc đợt nào, banner liệt kê rõ các khoảng trống đó.

Cả hai **chỉ là nhắc nhở, phần mềm vẫn cho lưu**. Nhưng nên xử lý: chỗ không đợt nào phủ chính là chỗ người ta bị sót. Xem thêm **B3**.

<a id="a4-nap-danh-sach-dang-vien"></a>
### A4. Nạp danh sách đảng viên

Có hai cách. Danh sách dài thì dùng **Import Excel**; thêm vài người thì **nhập tay**.

#### Cách 1 — Import từ file Excel (dùng cho danh sách dài)

1. Bấm **Đảng viên** ở menu trái, rồi bấm **Import Excel**. Phần mềm mở một trang riêng gồm **3 bước**.

**Bước 1 — Chọn file**

2. Bấm **Tải file mẫu**. Bạn nhận một file `.xlsx` có sẵn **4 cột đúng thứ tự** và một vài dòng ví dụ.
3. Mở file mẫu bằng Excel, **xóa các dòng ví dụ**, điền danh sách thật vào. Giữ nguyên dòng tiêu đề và thứ tự cột:

| Cột | Bắt buộc | Cách ghi |
|---|---|---|
| Họ tên | **Có** | Ghi đầy đủ, ví dụ `Nguyễn Văn An` |
| Ngày sinh | Không | `dd/MM/yyyy`, ví dụ `05/03/1950` |
| Giới tính | Không | `Nam` hoặc `Nữ` |
| Ngày vào đảng chính thức | **Có** | `dd/MM/yyyy`. Không được là ngày trong tương lai |

4. Lưu file. File phải là **`.xlsx`** và **không quá 10 MB**.
5. Quay lại phần mềm, kéo file thả vào khung, hoặc bấm để chọn file.

> **[Ảnh 7]** Bước 1 — vùng kéo thả file và nút Tải file mẫu — `docs/img/huong-dan/07-import-b1.png`

**Bước 2 — Xem trước**

6. Phần mềm đọc file và hiện dòng tóm tắt: *"Sẽ thêm **N người mới** · M dòng lỗi bị bỏ qua"*.
7. Có hai tab: **Hợp lệ (N)** và **Lỗi (M)**. Mở tab **Lỗi** để xem bảng liệt kê từng dòng hỏng, có cột **Dòng** (số dòng trong file Excel) và cột **Lý do**.
8. Đọc kỹ dòng cảnh báo: **phần mềm không kiểm tra trùng**. Nạp cùng một file hai lần là có hai bản của mỗi người.
9. Còn sửa được thì bấm **Hủy**, sửa file Excel rồi làm lại từ bước 1. Chấp nhận bỏ qua các dòng lỗi thì bấm **Nạp các dòng hợp lệ**.

> **[Ảnh 8]** Bước 2 — dòng tóm tắt, tab Hợp lệ và tab Lỗi — `docs/img/huong-dan/08-import-b2.png`

**Các lý do lỗi thường gặp:**

| Lý do | Cách sửa |
|---|---|
| Thiếu Họ tên | Điền họ tên vào ô đang trống |
| Thiếu ngày vào Đảng chính thức | Điền ngày. Đây là ô bắt buộc |
| Sai định dạng ngày | Ghi đúng `dd/MM/yyyy`. Ngày không có thật như `31/02/1974` cũng bị báo lỗi |
| Ngày vào Đảng chính thức ở tương lai | Kiểm tra lại, có thể gõ nhầm năm |
| Giới tính không phải Nam hoặc Nữ | Sửa thành `Nam` hoặc `Nữ`, hoặc để trống |
| Ngày sinh sau ngày vào Đảng chính thức | Kiểm tra lại hai ngày, thường là gõ đổi chỗ |

Một dòng sai nhiều chỗ thì cột **Lý do** ghi đủ cả, ngăn nhau bằng dấu `;`. Bạn sửa một lượt là xong, không phải nạp đi nạp lại nhiều vòng.

**Bước 3 — Kết quả**

10. Phần mềm báo: *"Đã thêm N người, bỏ qua M dòng lỗi"*.
11. Bấm **Về danh sách** để xem lại.

> **[Ảnh 9]** Bước 3 — kết quả nạp — `docs/img/huong-dan/09-import-b3.png`

#### Cách 2 — Thêm từng người bằng tay

1. Bấm **Đảng viên**, rồi bấm **Thêm**.
2. Điền 4 ô: **Họ tên** (bắt buộc), **Ngày sinh**, **Giới tính**, **Ngày vào Đảng chính thức** (bắt buộc, không được sau ngày hôm nay).
3. Bấm **Lưu**.

Sửa thì bấm vào dòng cần sửa, form hiện ra có sẵn dữ liệu cũ.

**Xóa:** tích chọn một hoặc nhiều dòng rồi bấm **Xóa**. Phần mềm hỏi lại và nói rõ sẽ xóa bao nhiêu người.

> ⚠️ **Xóa là xóa hẳn.** Không có thùng rác, không khôi phục lại được. Đọc kỹ con số trong hộp xác nhận trước khi bấm đồng ý.

> **[Ảnh 10]** Danh sách đảng viên và ô Thêm / Sửa — `docs/img/huong-dan/10-dang-vien.png`

<a id="a5-xem-dashboard-va-xuat-excel"></a>
### A5. Xem Dashboard và xuất Excel

Xong A2, A3, A4 là phần mềm đã đủ dữ liệu để làm việc.

1. Bấm **Dashboard** ở menu trái.
2. Phía trên là **thẻ đợt sắp tới**: tên đợt, khoảng ngày đã gắn năm, còn bao nhiêu ngày nữa (hoặc chữ "đang diễn ra"), tổng số người đủ điều kiện, và số người theo từng mốc — ví dụ *30 năm: 3 · 40 năm: 1*.
3. Bên dưới là **bảng danh sách đủ điều kiện** của chính đợt đó.

**Phần mềm tự chọn đợt sắp tới như sau:** gắn năm hiện tại vào tất cả các đợt, rồi lấy đợt gần nhất chưa kết thúc. Hôm nay đang nằm trong một đợt thì lấy chính đợt đó. Mọi đợt trong năm đã qua hết thì phần mềm lấy **đợt sớm nhất của năm sau** — nên đừng ngạc nhiên khi thấy năm sau hiện ra vào tháng 12.

**Đọc bảng đủ điều kiện:**

| Cột | Nghĩa |
|---|---|
| STT | Số thứ tự |
| Họ tên · Giới tính · Ngày sinh | Thông tin đảng viên |
| Ngày chính thức | Ngày vào Đảng chính thức |
| Ngày tròn mốc | Ngày người đó tròn số năm tuổi đảng của mốc |
| Mốc huy hiệu | Huy hiệu được trao: 30 năm, 40 năm… |

Danh sách sắp theo **mốc huy hiệu tăng dần**, trong cùng một mốc thì theo **Họ tên đầy đủ** theo bảng chữ cái tiếng Việt.

4. Bấm **Xuất Excel**. Trình duyệt tải về một file `.xlsx`.

**File Excel gồm:** dòng 1 là tên đơn vị (bỏ dòng này nếu bạn để trống Tên đơn vị), dòng 2 là tên đợt kèm khoảng ngày, dòng 3 là ngày xuất, dòng 4 để trống, dòng 5 là tiêu đề cột, từ dòng 6 là dữ liệu. Ô nào không có dữ liệu thì để trống.

5. Mở file bằng Excel, chỉnh trình bày theo mẫu tờ trình của đơn vị rồi in.

> **[Ảnh 11]** Dashboard đầy đủ: thẻ đợt sắp tới và bảng đủ điều kiện — `docs/img/huong-dan/11-dashboard-day-du.png`

> **[Ảnh 12]** File Excel xuất ra, mở bằng Excel — `docs/img/huong-dan/12-file-excel.png`

Đến đây phần chuẩn bị đã xong. Những lần sau bạn chỉ làm theo **Phần B**.

---

<a id="phan-b--viec-lam-dinh-ky-moi-dot"></a>
## Phần B — Việc làm định kỳ mỗi đợt

Đây là việc lặp lại mỗi kỳ trao huy hiệu. Chỉ ba bước, thường mất mười lăm phút.

<a id="b1-cap-nhat-dang-vien-moi"></a>
### B1. Cập nhật đảng viên mới

Từ đợt trước đến nay có đảng viên mới chuyển đến, hoặc có người mới được công nhận chính thức thì thêm họ vào:

- Vài người → **Đảng viên** → **Thêm** → điền 4 ô → **Lưu**.
- Cả danh sách dài → **Đảng viên** → **Import Excel** → làm như **A4**.

Nhân tiện sửa luôn những chỗ sai bạn phát hiện: gõ nhầm ngày, sai tên. Sửa xong là mọi danh sách tự đúng theo.

> ⚠️ **Nhớ lại: import không chống trùng.** Trước khi nạp một file, hãy dùng ô **tìm theo tên** trong danh sách đảng viên để kiểm tra vài cái tên xem đã có chưa. Lỡ nạp trùng thì tích chọn các dòng thừa rồi xóa.

<a id="b2-kiem-tra-danh-sach-va-xuat-excel"></a>
### B2. Kiểm tra danh sách và xuất Excel

Có hai đường, chọn đường nào cũng ra cùng một danh sách.

**Đường 1 — qua Dashboard.** Nhanh nhất khi bạn làm cho **đợt sắp tới**.

1. Bấm **Dashboard**.
2. Đối chiếu thẻ đợt sắp tới: đúng đợt bạn cần chưa?
3. Xem bảng bên dưới, bấm **Xuất Excel**.

**Đường 2 — qua màn hình đợt.** Dùng khi bạn cần **một đợt cụ thể**, hoặc cần **năm khác**.

1. Bấm **Đợt trao huy hiệu**.
2. Bấm vào tên đợt cần xem. Trang chi tiết đợt có hai tab.
3. Mở tab **Danh sách đủ điều kiện**.
4. Trên tab có **bộ chọn năm** ba nút: **năm trước · năm nay · năm sau**. Mặc định là năm nay. Chọn **năm sau** để chuẩn bị trước; phần mềm ghi rõ nhãn *"Năm sau · chuẩn bị trước"* để bạn không nhầm.
5. Bấm **Xuất Excel**.

> **[Ảnh 13]** Trang chi tiết đợt, tab Danh sách đủ điều kiện và bộ chọn năm — `docs/img/huong-dan/13-chi-tiet-dot.png`

**Nên đối chiếu trước khi in:** tổng số người trên thẻ có khớp bảng không, các mốc có hợp lý không. Thấy thiếu người thì sang **B3**.

<a id="b3-ra-soat-nguoi-chua-thuoc-dot-nao"></a>
### B3. Rà soát người chưa thuộc đợt nào

Đây là bước dễ bỏ qua nhất, nhưng là bước tránh sót người.

**"Chưa thuộc đợt nào" nghĩa là gì:** người đó **có** tròn mốc tuổi đảng trong năm, nhưng ngày tròn mốc **không rơi vào khoảng ngày của bất kỳ đợt nào**. Họ không xuất hiện trong bất cứ danh sách đủ điều kiện nào. Nếu bạn không mở màn hình này, họ bị bỏ quên cả năm.

1. Nhìn menu trái. Mục **Chưa thuộc đợt nào** có con số đỏ không? Có nghĩa là đang có người bị sót.
2. Bấm vào mục đó.
3. Chọn năm ở bộ chọn năm phía trên (mặc định năm nay).
4. Bảng hiện danh sách, giống bảng đủ điều kiện nhưng có thêm cột **Khoảng trống** cho biết người đó rơi vào chỗ hở nào: *"Giữa Đợt A và Đợt B"*, *"Trước đợt đầu tiên"*, hoặc *"Sau đợt cuối cùng"*.

> **[Ảnh 14]** Màn hình Chưa thuộc đợt nào, có cột Khoảng trống — `docs/img/huong-dan/14-chua-thuoc-dot-nao.png`

**Xử lý thế nào:** phần mềm gợi ý ngay trên màn hình — **nới Đến ngày của đợt trước, hoặc nới Từ ngày của đợt sau**, sao cho khoảng trống được phủ. Có nút đi thẳng sang màn hình **Đợt trao huy hiệu**.

1. Bấm nút đó, sửa đợt cho khoảng ngày rộng ra.
2. Bấm **Lưu**.
3. Quay lại **Chưa thuộc đợt nào**. Con số phải giảm hoặc về không.
4. Quay lại **B2** và xuất lại file Excel — danh sách giờ đã có thêm những người vừa được phủ.

Không muốn sửa đợt cũng được: bạn đã **nhìn thấy** họ, và có thể bấm **Xuất Excel** ngay trên màn hình này để có danh sách riêng mà xử lý bằng cách khác.

Màn hình ghi *"Không có ai bị sót trong năm YYYY"* là tốt — năm đó các đợt đã phủ hết.

---

<a id="phan-c--giai-thich-tu-ngu"></a>
## Phần C — Giải thích từ ngữ

| Từ | Nghĩa |
|---|---|
| **Ngày vào Đảng chính thức** | Ngày đảng viên được công nhận là đảng viên chính thức. Mọi tính toán đều đếm từ ngày này. Trên bảng và trong file Excel, cột này ghi gọn là **Ngày chính thức**. |
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

**5. Số ở Dashboard khác số ở màn hình đợt.** Thường là do khác năm: Dashboard luôn hiện **đợt sắp tới** — có thể đã sang năm sau — còn màn hình đợt hiện năm bạn tự chọn. Kiểm tra lại bộ chọn năm.

**6. Danh sách trống mà đáng lẽ phải có người.** Kiểm tra theo thứ tự: đã nhập đảng viên chưa (màn **Đảng viên**), đã tạo đợt chưa (màn **Đợt trao huy hiệu**), mốc trong **Cài đặt** có đúng không, và đang xem **năm** nào.

---

<a id="phan-e--khi-gap-truc-trac"></a>
## Phần E — Khi gặp trục trặc

| Hiện tượng | Xử lý |
|---|---|
| Báo *"Sai tài khoản hoặc mật khẩu"* | Gõ lại, chú ý phím Caps Lock. Vẫn không được thì hỏi người cài đặt phần mềm. |
| Đang làm thì bị đẩy về màn hình đăng nhập | Phiên làm việc đã hết hạn. Đăng nhập lại là tiếp tục được. Dữ liệu đã lưu không mất. |
| Không tải được file Excel | Kiểm tra mục tải xuống của trình duyệt; trình duyệt có thể đã chặn. Cho phép tải rồi bấm **Xuất Excel** lại. |
| Import báo lỗi cả file, không hiện bảng | Thường do: file không phải `.xlsx`, file nặng quá 10 MB, sai số cột hoặc sai thứ tự cột, hoặc file rỗng. Tải lại **file mẫu** và chép dữ liệu sang. |
| Màn hình trắng hoặc không phản hồi | Tải lại trang (phím `F5`). Vẫn vậy thì báo người quản trị. |
| Mất dữ liệu, cần khôi phục | Việc này do người quản trị làm. Cách sao lưu và khôi phục ghi trong `README.md`. |

Trục trặc không nằm trong bảng trên thì ghi lại **bạn đang làm gì**, **màn hình nào**, **phần mềm báo chữ gì**, chụp màn hình rồi gửi cho người quản trị.

---

<a id="phu-luc--danh-sach-anh-man-hinh"></a>
## Phụ lục — Danh sách ảnh màn hình

Phần chữ đã xong. **Ảnh màn hình chưa chụp** — sẽ chụp sau khi Frontend nối xong API thật (T24), vì ảnh chụp trên dữ liệu giả sẽ không khớp với những gì cán bộ nhìn thấy.

Mỗi chỗ cần ảnh trong tài liệu này được đánh dấu bằng một dòng `> **[Ảnh N]** …`. Khi có ảnh, thay nguyên dòng đó bằng:

```markdown
![Mô tả ngắn](img/huong-dan/NN-ten-anh.png)
```

Ảnh đặt trong `docs/img/huong-dan/`, đặt tên đúng như cột **Tên tệp** dưới đây.

| # | Nội dung cần chụp | Tên tệp |
|---|---|---|
| 1 | Khung màn hình chung: menu trái 5 mục, thanh trên cùng | `01-khung-man-hinh.png` |
| 2 | Màn hình Đăng nhập | `02-dang-nhap.png` |
| 3 | Dashboard khi chưa có dữ liệu, kèm hướng dẫn 3 bước | `03-dashboard-trong.png` |
| 4 | Cài đặt: ba ô số và phần xem trước dãy mốc | `04-cai-dat-moc.png` |
| 5 | Danh sách đợt trao huy hiệu và ô Thêm đợt | `05-them-dot.png` |
| 6 | Dải độ phủ 12 tháng, có khoảng trống màu xám | `06-dai-do-phu.png` |
| 7 | Import bước 1 — vùng kéo thả và nút Tải file mẫu | `07-import-b1.png` |
| 8 | Import bước 2 — tóm tắt, tab Hợp lệ và tab Lỗi | `08-import-b2.png` |
| 9 | Import bước 3 — kết quả nạp | `09-import-b3.png` |
| 10 | Danh sách đảng viên và ô Thêm / Sửa | `10-dang-vien.png` |
| 11 | Dashboard đầy đủ: thẻ đợt sắp tới và bảng đủ điều kiện | `11-dashboard-day-du.png` |
| 12 | File Excel xuất ra, mở bằng Excel | `12-file-excel.png` |
| 13 | Trang chi tiết đợt, tab Danh sách đủ điều kiện, bộ chọn năm | `13-chi-tiet-dot.png` |
| 14 | Màn hình Chưa thuộc đợt nào, có cột Khoảng trống | `14-chua-thuoc-dot-nao.png` |

**Khi chụp ảnh, nhớ:** dùng dữ liệu mẫu sạch (tên người không có thật), đặt Tên đơn vị là *"Đảng ủy Phường X"* cho thống nhất, và chụp đủ chiều ngang màn hình để thấy cả menu trái.

---

## Nguồn của tài liệu này

Phần chữ dựng theo `docs/2026-09-17-huyhieudang-business-design.md` (v1.1) — luồng 6.2 và 6.3 — và `docs/api-contract.md`. Giao diện thật khác tài liệu chỗ nào thì sửa tài liệu này, và ghi một dòng vào `CHANGELOG.md`.
