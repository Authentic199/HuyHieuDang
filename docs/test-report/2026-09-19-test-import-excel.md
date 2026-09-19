# Báo cáo kiểm thử — Import Excel (T10)

Lệnh đã chạy: `cd BE && dotnet test HuyHieuDang.sln`

Kết quả: 207 đạt · 0 hỏng · 0 bỏ qua.

| Dự án kiểm thử | Đạt | Hỏng | Bỏ qua |
|---|---|---|---|
| HuyHieuDang.Core.UnitTests | 103 | 0 | 0 |
| HuyHieuDang.Infrastructure.UnitTests | 38 | 0 | 0 |
| HuyHieuDang.Infrastructure.IntegrationTests | 1 | 0 | 0 |
| HuyHieuDang.Web.IntegrationTests | 65 | 0 | 0 |

Báo cáo này ghi chi tiết ba nhóm mới của T10. Các nhóm còn lại đã có báo cáo riêng ở những lần
bàn giao trước và lần chạy này vẫn xanh nguyên.

## PartyMemberImportRowValidatorTests

Dòng đủ bốn ô hợp lệ thì không có lỗi và được chuẩn hóa — ĐẠT
Ngày sinh và giới tính bỏ trống vẫn hợp lệ, trả null — ĐẠT
Ngày chính thức đúng bằng hôm nay là hợp lệ — ĐẠT
Thiếu họ tên báo MissingFullName ở cột Họ tên — ĐẠT
Thiếu ngày chính thức báo MissingOfficialAdmissionDate, không kèm lỗi định dạng — ĐẠT
Ngày chính thức viết kiểu yyyy-MM-dd báo InvalidDateFormat — ĐẠT
Ngày chính thức ở tương lai báo FutureOfficialAdmissionDate — ĐẠT
Giới tính lạ báo InvalidGender — ĐẠT
Ngày sinh sau ngày chính thức báo BirthDateAfterAdmissionDate — ĐẠT
Ngày sinh bằng đúng ngày chính thức cũng là lỗi (OQ-10) — ĐẠT
Ngày sinh 31/02/1974 không có thật báo InvalidDateFormat (OQ-1) — ĐẠT
Một dòng sai bốn chỗ trả đủ bốn lý do, đúng thứ tự bảng mã lỗi (OQ-2) — ĐẠT
Giới tính nam, NAM, Nữ, nữ, NỮ đều nhận, không phân biệt hoa thường (OQ-5) — ĐẠT
Ngày viết một chữ số và hai chữ số đều nhận (OQ-6) — ĐẠT
Ngày 29/02 của năm nhuận là ngày có thật — ĐẠT
Ngày 29/02 của năm không nhuận báo sai định dạng — ĐẠT

## ImportEndpointTests

File mẫu trả file nhị phân đúng tên MauDanhSachDangVien.xlsx, bốn cột đúng thứ tự, hai dòng ví dụ ngày dd/MM/yyyy — ĐẠT
File mẫu hệ thống sinh khớp từng ô với fixture mau-dang-vien.xlsx của QC — ĐẠT
Xem trước loi-4-dong.xlsx: 10 dòng, 6 hợp lệ, 4 lỗi đúng ở dòng Excel 8, 9, 10, 11 và đúng lý do từng dòng — ĐẠT
Xem trước loi-moi-loai-mot-dong.xlsx: 2 hợp lệ, 8 lỗi, mỗi loại lỗi một dòng, dòng sai bốn chỗ trả đủ bốn lý do — ĐẠT
Xem trước core-hop-le.xlsx: 32 dòng hợp lệ, không dòng lỗi — ĐẠT
Xem trước core-hop-le-ngay-kieu-date.xlsx: ô ngày kiểu ngày của Excel đọc được y như ngày dạng chuỗi — ĐẠT
Xem trước bien-chuan-hoa.xlsx: cắt khoảng trắng họ tên, nhận giới tính hoa thường, nhận ngày một chữ số — ĐẠT
File .csv bị chặn với khóa Mes.Import.Invalid.Extension — ĐẠT
File đổi đuôi .xlsx nhưng ruột không phải xlsx trả 400 Mes.Import.Invalid.Extension, không phải 500 — ĐẠT
File 5 cột sai thứ tự bị chặn với khóa Mes.Import.Invalid.Columns — ĐẠT
File sheet rỗng hoàn toàn bị chặn với khóa Mes.Import.Invalid.Empty — ĐẠT
File chỉ có tiêu đề bị chặn với khóa Mes.Import.Invalid.NoDataRows — ĐẠT
File vượt 10 MB bị chặn trước khi mở, khóa Mes.Import.Invalid.FileSize — ĐẠT
Bước xem trước không ghi bản ghi nào vào cơ sở dữ liệu — ĐẠT
Nạp loi-4-dong.xlsx thêm 6 người, bỏ qua 4 dòng lỗi, dữ liệu lưu đúng ngày và giới tính — ĐẠT
Nạp lại đúng file đó lần nữa vẫn thêm mới toàn bộ, không chống trùng (QT9) — ĐẠT
Nạp loi-moi-loai-mot-dong.xlsx thêm 2 người, bỏ qua 8 dòng lỗi, không ném lỗi — ĐẠT
Lỗi cấp file cũng chặn ở bước nạp và không thêm ai — ĐẠT
Cả ba endpoint đều trả 401 khi không có token — ĐẠT

## ImportTransactionTests

Hỏng ở lô ghi thứ hai của bulk-1200.xlsx: 200 dòng đã ghi trước đó bị thu hồi hết, bảng còn 0 bản ghi — ĐẠT
Hỏng đúng lúc chốt giao dịch: 32 dòng của core-hop-le.xlsx bị thu hồi hết, bảng còn 0 bản ghi — ĐẠT
Cùng đường đi đó nhưng không hỏng: bulk-1200.xlsx thêm đủ 1200 người — ĐẠT
