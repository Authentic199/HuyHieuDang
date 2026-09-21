# Báo cáo kiểm thử — Playwright E2E, ba mốc thời gian

Lệnh đã chạy, từ thư mục `FE/`:

```
docker compose -f e2e/docker-compose.e2e.yml up -d --build
npx playwright test -c e2e/playwright.e2e.config.ts
```

Mốc T1 và T2 chạy sau khi dựng lại dịch vụ `be` với `E2E_TODAY` tương ứng.

| Lần chạy | Mốc ép | Kết quả |
|---|---|---|
| Mốc mặc định T0 | 19/09/2026 | 13 đạt, 0 hỏng, 2 bỏ qua |
| Chạy lại lần hai, không reset tay (E-901, E-902) | 19/09/2026 | 13 đạt, 0 hỏng, 2 bỏ qua |
| Mốc T1 (E-903) | 15/10/2026 | 5 đạt, 0 hỏng, 2 bỏ qua |
| Mốc T2 (E-904) | 01/12/2026 | 5 đạt, 0 hỏng, 2 bỏ qua |

Hai ca "bỏ qua" ở mỗi lần chạy là hai ca của hai mốc còn lại — chúng tự bỏ qua khi stack không dựng ở đúng mốc của mình, kèm câu nhắc cách chạy. Không còn ca nào bỏ qua vì lỗi chưa sửa.

Trước nhánh này, chạy trên `main` cho **5 luồng đỏ**. Ba trong năm là do một lỗi duy nhất trong lớp fixture, hai còn lại đã có bản sửa nằm ở PR #57 chưa gộp.

## E0 · Tiền đề đóng băng thời gian

- Backend đóng băng "hôm nay" theo biến môi trường `HUYHIEUDANG_TEST_TODAY` — ĐẠT
- Ngày máy chủ nằm trong khoảng an toàn của bộ dữ liệu biên — ĐẠT ở mốc T0, tự bỏ qua ở mốc T1 và T2
- Đồng hồ trình duyệt đóng băng đúng mốc và đúng múi giờ — ĐẠT

## E2E-1 · Lần dùng đầu tiên

- Đăng nhập, cài đặt, tạo bốn đợt, import, Dashboard hiện đúng đợt sắp tới và đúng phân bổ mốc — ĐẠT

## E2E-2 · Import có lỗi

- Xem trước đúng số dòng hợp lệ và dòng lỗi, chỉ dòng hợp lệ được nạp; nút Hủy về thẳng danh sách và không ghi gì vào kho — ĐẠT

## E2E-3 · Đổi cài đặt lan truyền

- Đổi Bước từ 5 sang 10, mọi màn đổi theo ngay, cột Mốc kế tiếp tính lại đúng — ĐẠT

## E2E-4 · Sửa đợt lan truyền

- Nới Đến ngày của một đợt, độ phủ, cảnh báo và thẻ Dashboard đổi theo ngay — ĐẠT

## E2E-5 · Xuất Excel ba nơi

- Xuất từ Dashboard, từ chi tiết đợt và từ màn Chưa thuộc đợt nào, tên tệp và nội dung đúng — ĐẠT

## E2E-6 · Vòng đời đảng viên

- Thêm tay, tìm, sửa, xóa nhiều dòng bằng nút thùng rác — ĐẠT

## E2E-7 · Đợt trao huy hiệu vắt qua 31/12

- Tạo đợt Từ 01/12 Đến 28/02 qua modal, lưu được, chạy đúng trên mọi màn — ĐẠT
- Tiêu đề trang chi tiết đợt nói rõ Đến ngày thuộc năm sau — ĐẠT

## E9 · Chạy lại được và độc lập

- Chạy lại ở mốc T1 = 15/10/2026, Dashboard báo "Đang diễn ra" — ĐẠT (chạy ở mốc T1)
- Chạy lại ở mốc T2 = 01/12/2026, Dashboard nhảy sang Đợt 3/2 năm 2027 — ĐẠT (chạy ở mốc T2)
- Không ca nào dùng waitForTimeout — ĐẠT
- Mỗi luồng tự dựng trạng thái đầu ngay ở dòng đầu tiên — ĐẠT
