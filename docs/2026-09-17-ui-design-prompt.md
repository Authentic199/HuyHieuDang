# Prompt thiết kế UI — HuyHieuDang

> Dán toàn bộ phần dưới dấu gạch vào Claude Design. Nên đính kèm thêm file
> `2026-09-17-huyhieudang-business-design.md` để Claude Design bám sát quy tắc nghiệp vụ.

---

Bạn là UI/UX designer. Hãy thiết kế giao diện web cho ứng dụng quản trị nội bộ **HuyHieuDang — Hệ thống hỗ trợ xét trao Huy hiệu Đảng**. Hãy tạo mockup ở độ trung thực cao (high-fidelity), desktop-first (1440×900), tiếng Việt, theo ngôn ngữ thiết kế **Ant Design 5** vì frontend sẽ dựng bằng React + Ant Design. Ưu tiên dùng đúng component của Ant Design (Layout Sider, Table, Modal, Steps, Tabs, Statistic, Alert, Form, DatePicker, InputNumber, Tag, Empty) để dev dựng lại được 1:1.

## 1. Bối cảnh

- Người dùng: **một** cán bộ văn phòng đảng ủy, không rành công nghệ, dùng trên một máy. Mục tiêu: 4 lần/năm mở lên xem ai đủ tuổi đảng để trao huy hiệu, xuất Excel đem đi làm tờ trình.
- Không có phân quyền, chỉ một tài khoản admin.
- Tông màu: trang trọng, tin cậy, sạch. Gợi ý màu chủ đạo đỏ đậm (#C8102E hoặc tương đương) dùng tiết chế cho điểm nhấn; nền trắng/xám nhạt; không lòe loẹt. Có thể dùng ngôi sao vàng làm hoạ tiết logo nhỏ.
- Font: hệ thống (Inter/Segoe UI). Chữ rõ, cỡ chữ bảng tối thiểu 14px vì người dùng lớn tuổi.
- Định dạng ngày: `dd/MM/yyyy`. Ngày của "đợt" chỉ có ngày/tháng: `dd/MM`.

## 2. Khái niệm nghiệp vụ cần hiểu để thiết kế

- **Đảng viên** có: Họ tên, Ngày sinh (có thể trống), Giới tính (Nam/Nữ/trống), **Ngày vào đảng chính thức** (bắt buộc).
- **Mốc huy hiệu**: dãy số năm sinh từ cài đặt Bắt đầu / Kết thúc / Bước (mặc định 30 / 90 / 5 → 30, 35, 40 … 90).
- **Đợt trao huy hiệu**: khoảng ngày trong năm, chỉ lưu ngày/tháng, dùng chung mọi năm (VD "Đợt 19/5": 01/03 → 19/05). Sửa đợt là áp dụng ngay cho năm hiện tại.
- **Đủ điều kiện**: đảng viên có (Ngày chính thức + N năm) rơi trong khoảng ngày của đợt trong năm đang xét. N là mốc được trao.
- **Chưa thuộc đợt nào**: người tròn mốc trong năm nhưng ngày tròn mốc không rơi vào đợt nào (do các đợt không phủ kín cả năm).
- Danh sách đủ điều kiện **không lưu**, luôn tính lại; chỉ xem và xuất Excel, không có "đánh dấu đã trao".

## 3. Bộ khung chung

- Layout: Sider trái cố định (thu gọn được) + Header mỏng (tên hệ thống, tên đơn vị, nút Đăng xuất) + Content.
- Menu trái 5 mục, thứ tự cố định: **Dashboard · Đảng viên · Đợt trao huy hiệu · Chưa thuộc đợt nào · Cài đặt**.
- Bảng "Danh sách đủ điều kiện" dùng lại ở 3 nơi, cột giống nhau: STT · Họ tên · Giới tính · Ngày sinh · Ngày vào đảng chính thức · Ngày tròn mốc · Mốc huy hiệu (hiển thị dạng Tag, VD "30 năm"). Sắp theo Mốc rồi Họ tên. Luôn có nút **Xuất Excel** ở góc phải trên bảng.

## 4. Các màn hình cần thiết kế (theo thứ tự ưu tiên)

### Màn 1 — Đăng nhập
Card giữa màn hình: logo + tên hệ thống, ô Tài khoản, ô Mật khẩu, nút Đăng nhập. Trạng thái lỗi: thông báo chung "Sai tài khoản hoặc mật khẩu".

### Màn 2 — Dashboard (trang mặc định sau đăng nhập) — **quan trọng nhất**
- Khu cảnh báo (Alert) trên cùng, chỉ hiện khi có: "Chưa có đảng viên nào", "Chưa cài đợt trao huy hiệu", "Có 3 người chưa thuộc đợt nào trong năm 2026 → Xem", "Các đợt chưa phủ kín cả năm → Điều chỉnh".
- Thẻ **Đợt sắp tới**: tên đợt, khoảng ngày đã gắn năm (01/03/2026 – 19/05/2026), trạng thái "Còn 42 ngày" hoặc "Đang diễn ra", tổng số người đủ điều kiện (số lớn), phân bổ theo mốc dạng chip: "30 năm · 3", "40 năm · 1", "50 năm · 2".
- Bảng đủ điều kiện của đợt đó (bộ cột chung) + nút Xuất Excel.
- Thiết kế thêm **trạng thái trống** cho lần dùng đầu tiên (chưa có gì) với hướng dẫn 3 bước: Cài đặt → Tạo đợt → Import đảng viên.

### Màn 3 — Đảng viên
- Thanh công cụ: ô tìm theo tên, Select lọc giới tính, nút **Import Excel** (primary), nút **Thêm**, nút Xóa đã chọn (chỉ hiện khi có dòng được tick).
- Bảng: checkbox · Họ tên · Giới tính · Ngày sinh · Ngày chính thức · Tuổi đảng · Mốc kế tiếp · Ngày tròn mốc kế tiếp · Thao tác (Sửa / Xóa). Phân trang, sort theo cột.
- Modal Thêm/Sửa: 4 trường (Họ tên*, Ngày sinh, Giới tính, Ngày vào đảng chính thức*). Trạng thái lỗi validate.
- Hộp xác nhận xóa nêu rõ "Xóa 3 đảng viên đã chọn? Không thể hoàn tác."

### Màn 4 — Import Excel (wizard 3 bước, Ant Design Steps)
- Bước 1 Chọn file: vùng kéo-thả, link "Tải file mẫu", ghi rõ 4 cột và định dạng ngày.
- Bước 2 Xem trước: dòng tóm tắt nổi bật "Sẽ thêm **125 người mới** · 4 dòng lỗi bị bỏ qua"; Tabs "Hợp lệ (125)" / "Lỗi (4)"; bảng lỗi có cột Dòng · Họ tên · Lý do (VD "Thiếu ngày vào đảng chính thức", "Sai định dạng ngày", "Ngày chính thức ở tương lai"). Nút Quay lại / Nạp dữ liệu / Hủy.
- Bước 3 Kết quả: "Đã thêm 125 người, bỏ qua 4 dòng lỗi" + nút Về danh sách.

### Màn 5 — Đợt trao huy hiệu
- Banner cảnh báo (nếu có): "Đợt 19/5 và Đợt 2/9 chồng lấn nhau" / "Các đợt chưa phủ kín: trống từ 04/02 đến 28/02".
- Bảng: Tên đợt · Từ ngày (dd/MM) · Đến ngày (dd/MM) · Số người đủ điều kiện năm nay · Thao tác. Nút **Thêm đợt**.
- Có thể thêm một dải thời gian (timeline ngang 12 tháng) minh hoạ các đợt phủ đến đâu, khoảng trống tô xám — tùy chọn nhưng rất hữu ích.
- Modal Thêm/Sửa đợt: Tên, Từ ngày (chọn ngày/tháng), Đến ngày (chọn ngày/tháng). Dòng nhắc: "Thay đổi áp dụng ngay cho năm hiện tại và các năm sau."
- **Trang chi tiết đợt** với Tabs: "Thông tin" / "Danh sách đủ điều kiện". Tab thứ hai có bộ chọn Năm (mặc định năm nay, cho chọn năm trước/sau) + bảng chung + Xuất Excel.

### Màn 6 — Chưa thuộc đợt nào
- Bộ chọn Năm, câu giải thích ngắn "Những người tròn mốc trong năm nhưng không rơi vào đợt nào. Hãy điều chỉnh khoảng ngày các đợt để phủ kín."
- Bảng chung + cột "Khoảng trống" (VD "Giữa Đợt 3/2 và Đợt 19/5"). Xuất Excel. Trạng thái trống: "Không có ai bị sót trong năm 2026 🎉".

### Màn 7 — Cài đặt
- Card "Mốc tuổi đảng": 3 InputNumber Bắt đầu / Kết thúc / Bước, bên dưới xem trước dãy mốc sinh ra dạng Tag (30 · 35 · 40 · … · 90) cập nhật khi gõ. Dòng nhắc "Thay đổi ảnh hưởng ngay đến mọi danh sách đủ điều kiện." Nút Lưu.

## 5. Yêu cầu đầu ra

- Mỗi màn một artboard, đặt tên theo số màn ở trên; Dashboard và Đảng viên làm thêm trạng thái trống.
- Thêm 1 artboard "Design system nhỏ": màu, typography, Tag mốc huy hiệu, nút chính/phụ, Alert 3 loại.
- Dữ liệu mẫu tiếng Việt thật (họ tên Việt, ngày tháng hợp lý với tuổi đảng 30–60).
- Không thiết kế mobile ở giai đoạn này.
