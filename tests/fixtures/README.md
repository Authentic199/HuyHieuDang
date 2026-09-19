# tests/fixtures — bộ dữ liệu biên dùng chung cho Backend, Frontend và QC

Bộ dữ liệu này là **hợp đồng dữ liệu kiểm thử** của dự án. Backend và Frontend dùng
chính các tệp ở đây; QC dùng `data/expected.json` làm kết quả mong đợi. Nếu mã nguồn
cho ra số khác `expected.json` thì một trong hai bên sai — không được sửa fixture cho
khớp mã nguồn mà không đối chiếu lại tài liệu nghiệp vụ trước.

Nguồn sự thật: `docs/2026-09-17-huyhieudang-business-design.md` v1.1, mục 3 (QT1–QT11)
và mục 4 (UC-xx). Kế hoạch kiểm thử: `docs/test-plan.md`.

## Tệp nào dùng làm gì

```
tests/fixtures/
├── qt_reference.py        Cài đặt tham chiếu QT1–QT11 (oracle). Backend KHÔNG import file này.
├── dataset.py             Định nghĩa dữ liệu: mốc thời gian, cài đặt, đợt, 32 ca biên, bộ 1200.
├── generate.py            Sinh toàn bộ data/, excel/, sql/. Tự kiểm định bằng assert.
├── requirements.txt
├── data/
│   ├── expected.json      ★ Kết quả mong đợi cho mọi kịch bản. Tệp quan trọng nhất.
│   ├── members-core.json  32 đảng viên biên, kèm trường `intent` giải thích ca nào.
│   ├── members-bulk.json  1200 đảng viên cho phân trang.
│   ├── periods.json       Các bộ đợt trao huy hiệu.
│   ├── settings.json      Cài đặt mặc định / Bước = 10 / không có Tên đơn vị.
│   ├── import-rows.json   Các dòng import hợp lệ và lỗi, kèm lý do mong đợi.
│   └── excel-manifest.json  Mỗi tệp Excel dùng cho ca nào, bao nhiêu dòng hợp lệ / lỗi.
├── excel/                 Các tệp .xlsx dùng cho import (xem bảng dưới).
└── sql/                   Seed SQL cho kiểm thử tích hợp (xem mục "Seed SQL").
```

## Sinh lại

```bash
cd tests/fixtures
pip install -r requirements.txt
python generate.py                  # sinh mọi thứ đã commit
python generate.py --with-oversize  # sinh thêm excel/loi-qua-10mb.xlsx (không commit)
```

`generate.py` chạy `self_check()` trước khi ghi tệp: mọi con số trong `expected.json`
đều do `qt_reference.py` tính ra rồi đối chiếu với các `assert` viết tay suy từ tài
liệu nghiệp vụ. Script **không xuất tệp nếu một assert nào sai**.

## Mốc thời gian cố định

Bộ dữ liệu này chỉ đúng khi "hôm nay" được ép về một trong ba giá trị sau. Cách ép:
xem mục **Cố định thời gian (T-FIX)** trong `docs/test-plan.md`.

| Mốc | Giá trị | Vì sao cần |
|---|---|---|
| **T0** | `2026-09-19` | Mặc định. Đợt sắp tới = Đợt 7/11, trạng thái "Sắp tới · còn 12 ngày" |
| **T1** | `2026-10-15` | Hôm nay nằm trong Đợt 7/11 → trạng thái "Đang diễn ra" (QT11) |
| **T2** | `2026-12-01` | Mọi đợt 2026 đã qua → đợt sắp tới là Đợt 3/2 của **2027** (QT8 nhánh năm sau) |

Múi giờ bắt buộc: `Asia/Ho_Chi_Minh`. Chạy ở UTC sẽ lệch một ngày vào đầu/cuối ngày
và làm các ca biên đúng-ngày sai kết quả.

Mọi ngày trong bộ dữ liệu là ngày dương lịch **tuyệt đối**, không phải "hôm nay − N năm".
Nhờ vậy bộ dữ liệu không mục theo thời gian.

## Bộ đợt chính và các khoảng trống (năm 2026)

| Đợt | Từ – Đến | Trạng thái tại T0 |
|---|---|---|
| Đợt 3/2 | 15/01 – 05/03 | Đã qua |
| Đợt 19/5 | 01/05 – 31/05 | Đã qua |
| Đợt 2/9 | 15/08 – 10/09 | Đã qua |
| Đợt 7/11 | 01/10 – 07/11 | Sắp tới · 12 ngày |

Đợt 3/2 cố ý kéo tới 05/03 để chứa 28/02 — phục vụ ca 29/02 thu về 28/02 (QT2).

Năm khoảng trống: 01/01–14/01 · 06/03–30/04 · 01/06–14/08 · 11/09–30/09 · 08/11–31/12.
Banner "chưa phủ kín" (QT6) phải liệt kê đúng năm khoảng này.

## Con số mong đợi quan trọng (cài đặt 30/90/5, tại T0, chỉ nạp bộ lõi)

| Hạng mục | Giá trị |
|---|---|
| Đủ điều kiện Đợt 3/2 · 2026 | 5 người — mốc 30: 4, mốc 35: 1 |
| Đủ điều kiện Đợt 19/5 · 2026 | 5 người — mốc 30: 3, mốc 35: 1, mốc 90: 1 |
| Đủ điều kiện Đợt 2/9 · 2026 | 4 người — mốc 30: 4 |
| Đủ điều kiện Đợt 7/11 · 2026 | 6 người — mốc 30: 3, 35: 1, 40: 1, 45: 1 |
| Chưa thuộc đợt nào · 2026 (badge) | 7 người |
| Tổng số đảng viên khi nạp cả bộ lớn | 1232 |

Đổi **Bước 5 → 10**: Đợt 7/11 còn **4 người** (mất Hồ Thị Vân mốc 35 và Trần Thị Yến
mốc 45), badge còn **6** (mất Ngô Thị Cẩm mốc 35).

Nới **Đến ngày Đợt 2/9 từ 10/09 → 30/09**: Lê Văn Cường chuyển từ "Chưa thuộc đợt nào"
sang Đợt 2/9 → đợt có **5 người**, badge còn **6**.

## Bộ lớn nạp chung với bộ lõi có an toàn không — CÓ

1200 người trong `bulk-1200.xlsx` đều có Ngày chính thức trong 2015–2020, nên mốc 30 của
họ rơi vào 2045–2050. Họ **không bao giờ** xuất hiện trong danh sách đủ điều kiện hay
"chưa thuộc đợt nào" của các năm 2025–2030. Vì vậy nạp cả hai bộ vẫn giữ nguyên mọi con
số ở bảng trên; chỉ tổng số người và số trang thay đổi.

## Các tệp Excel

| Tệp | Dòng hợp lệ | Dòng lỗi | Dùng cho |
|---|---|---|---|
| `mau-dang-vien.xlsx` | 2 | 0 | UC-25 — đối chiếu với file mẫu do hệ thống sinh |
| `core-hop-le.xlsx` | 32 | 0 | Bộ lõi, ngày ghi dạng **chuỗi** `dd/MM/yyyy`. File chính của E2E-1 |
| `core-hop-le-ngay-kieu-date.xlsx` | 32 | 0 | Cùng dữ liệu, ô ngày là **kiểu ngày** của Excel. Trình đọc phải nhận cả hai |
| `bulk-1200.xlsx` | 1200 | 0 | Phân trang, tìm kiếm, hiệu năng |
| `loi-4-dong.xlsx` | 6 | 4 | **E2E-2**. Dòng lỗi ở dòng Excel 8, 9, 10, 11 |
| `loi-moi-loai-mot-dong.xlsx` | 2 | 8 | Mỗi loại lỗi cấp dòng một dòng (QT9) |
| `bien-chuan-hoa.xlsx` | ? | ? | Hành vi **chưa chốt** — OQ-4, OQ-5, OQ-6 |
| `loi-sai-cot.xlsx` | — | — | Lỗi cấp file: 5 cột, sai thứ tự |
| `loi-rong.xlsx` | — | — | Lỗi cấp file: sheet rỗng hoàn toàn |
| `loi-chi-co-tieu-de.xlsx` | — | — | Lỗi cấp file: chỉ có tiêu đề, 0 dòng dữ liệu |
| `loi-khong-phai-xlsx.xlsx` | — | — | Lỗi cấp file: tệp văn bản đổi phần mở rộng. Phải trả 400, **không phải 500** |
| `loi-dinh-dang-csv.csv` | — | — | Lỗi cấp file: chỉ nhận `.xlsx` |
| `loi-qua-10mb.xlsx` | — | — | Lỗi cấp file: 11 MB. **Không commit** — chạy `python generate.py --with-oversize` |

Lý do không commit tệp 11 MB: nó là dữ liệu nhồi thêm, không mang thông tin nghiệp
vụ nào, và làm kho mã phình lên vô ích. Nó được sinh lại tất định từ `core-hop-le.xlsx`
cộng một phần nhị phân 11 MB. Kịch bản CI phải chạy `--with-oversize` trước khi chạy
ca kiểm thử dung lượng.

## Seed SQL

`sql/` là **bản tạm**, sinh từ biến `DB` ở đầu `generate.py`. Tên bảng và tên cột đang
đoán theo quy ước `UnderscoreTable` của boilerplate (`party_members`, `award_periods`,
`app_settings`). Sau khi migration của T06 chốt tên thật:

1. Sửa biến `DB` trong `generate.py` (một chỗ duy nhất).
2. Chạy lại `python generate.py`.

`Id` là GUID tất định để kiểm thử tham chiếu được: bộ lõi `c0000000-0000-4000-8000-<số>`,
bộ lớn `b0000000-…`, đợt `d0000000-…`, cài đặt `50000000-…-000000000001`.

Giới tính ghi dạng chuỗi `'Nam'` / `'Nữ'` / `NULL`. Nếu Backend lưu enum số thì cột
`gender` trong seed phải đổi tương ứng — ghi rõ trong PR của T06.

**Dạng chuẩn là JSON**, không phải SQL. Kiểm thử tích hợp nên nạp qua `members-core.json`
(gọi API hoặc EF trực tiếp) để không phụ thuộc tên cột; SQL chỉ để dựng nhanh bằng tay
hoặc `docker compose exec postgres psql`.
