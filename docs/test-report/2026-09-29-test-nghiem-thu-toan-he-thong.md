# Nghiệm thu toàn hệ thống trên main

CEO chạy để trả lời một câu hỏi: hệ thống đã đạt tiêu chí hoàn thành của dự án chưa. Chạy hai tầng trong cùng một lượt — bộ test .NET và bộ đầu-cuối Playwright — trên một bản làm việc tách riêng.

Bộ đầu-cuối chạy trước trên nhánh `test/T57-e2e-xanh-ba-moc` để quyết định có gộp PR #59 hay không; gộp xong, bộ .NET chạy trên `main` tại `e294c99`.

## Lệnh đã chạy

```
docker compose -f e2e/docker-compose.e2e.yml up -d --build
npx playwright test -c e2e/playwright.e2e.config.ts
      13 đạt · 2 bỏ qua · 0 hỏng (3.0 phút)

dotnet test HuyHieuDang.sln
      733 đạt · 1 bỏ qua · 0 hỏng
```

Mốc thời gian ép cho lượt đầu-cuối: 2026-09-19 ở cả hai đầu. Máy chủ báo đúng mốc, đồng hồ trình duyệt đóng băng đúng mốc.

## Bộ test .NET trên main

| Dự án test | Đạt | Hỏng | Bỏ qua |
|---|---|---|---|
| HuyHieuDang.Core.UnitTests | 157 | 0 | 0 |
| HuyHieuDang.Core.QcTests | 129 | 0 | 1 |
| HuyHieuDang.Infrastructure.UnitTests | 78 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |
| HuyHieuDang.Web.IntegrationTests | 225 | 0 | 0 |
| HuyHieuDang.Web.QcIntegrationTests | 143 | 0 | 0 |
| **Cộng** | **733** | **0** | **1** |

Ca duy nhất bị bỏ qua: QC-05 · Core không được đọc đồng hồ hệ thống — BỎ QUA, giữ theo quyết định đã chốt vì ca này loại trừ `BaseEntity.CreatedAt`.

Từng ca của 733 ca đạt đã được ghi trong các báo cáo trước của mỗi agent trong cùng thư mục này; không chép lại ở đây.

## Bộ đầu-cuối Playwright

### E0 · Tiền đề đóng băng thời gian

- Máy chủ đóng băng "hôm nay" theo biến môi trường của bộ kiểm thử — ĐẠT
- Ngày máy chủ nằm trong khoảng an toàn của bộ dữ liệu biên — ĐẠT
- Đồng hồ trình duyệt đóng băng đúng mốc và đúng múi giờ — ĐẠT

### Bảy luồng nghiệp vụ

- E2E-1 · Lần dùng đầu tiên: đăng nhập, cài đặt mốc, tạo đợt, import, xem Dashboard — ĐẠT
- E2E-2 · Import có lỗi: xem trước đếm đúng, chỉ dòng hợp lệ được nạp — ĐẠT
- E2E-3 · Đổi Bước từ 5 sang 10 lan truyền ngay sang mọi màn — ĐẠT
- E2E-4 · Nới Đến ngày của một đợt lan truyền ngay sang mọi màn — ĐẠT
- E2E-5 · Xuất Excel từ Dashboard, chi tiết đợt và Chưa thuộc đợt nào — ĐẠT
- E2E-6 · Vòng đời đảng viên: thêm tay, tìm, sửa, xóa nhiều dòng — ĐẠT
- E2E-7 · Đợt trao huy hiệu vắt qua 31/12 chạy đúng trên mọi màn — ĐẠT
- Tiêu đề trang chi tiết đợt nói rõ Đến ngày thuộc năm sau — ĐẠT

### E9 · Ca về chính bộ kiểm thử

- Chạy lại ở mốc 2026-10-15: Dashboard báo "Đang diễn ra" — BỎ QUA: một lần chạy chỉ dựng được một mốc, mốc của máy chủ nằm trong biến môi trường của container
- Chạy lại ở mốc 2026-12-01: Dashboard nhảy sang đợt của năm sau — BỎ QUA: cùng lý do
- Không ca nào dùng chờ theo thời gian cố định — ĐẠT
- Mỗi luồng tự dựng trạng thái đầu ngay ở dòng đầu tiên — ĐẠT

## Kết luận

Ba trong bốn tiêu chí hoàn thành của dự án đã đạt: bộ ba container dựng và chạy được, sáu luồng E2E-1 đến E2E-6 xanh (cùng E2E-7 phát sinh sau), không còn lỗi mức chặn. Tiêu chí còn lại là đóng hết task: còn hướng dẫn sử dụng thiếu 17 ảnh, một hồi quy giao diện đang sửa, một lượt kiểm thử dữ liệu lớn và lượt đồng bộ tài liệu cuối.
