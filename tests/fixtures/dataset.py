"""Định nghĩa bộ dữ liệu biên. Mỗi bản ghi ghi rõ nó phục vụ quy tắc/ca nào.

Mọi ngày ở đây là ngày dương lịch tuyệt đối, KHÔNG tính theo "hôm nay − N năm".
Nhờ vậy bộ dữ liệu cho kết quả như nhau ở mọi thời điểm chạy, miễn là "hôm nay"
được cố định về một trong ba mốc thời gian T0/T1/T2 bên dưới.
"""

from __future__ import annotations

from datetime import date

from qt_reference import Member, Period

# ---------------------------------------------------------------- Mốc thời gian cố định
#
# Mọi tầng kiểm thử phải ép "hôm nay" về một trong ba giá trị này.
# Cách ép: xem docs/test-plan.md mục "Cố định thời gian (T-FIX)".

T0 = date(2026, 9, 19)   # mặc định. Đợt sắp tới = Đợt 7/11, trạng thái "Sắp tới · 12 ngày"
T1 = date(2026, 10, 15)  # hôm nay nằm trong Đợt 7/11 -> trạng thái "Đang diễn ra"
T2 = date(2026, 12, 1)   # mọi đợt năm 2026 đã qua -> đợt sắp tới là Đợt 3/2 của 2027

TIMEZONE = "Asia/Ho_Chi_Minh"

# ---------------------------------------------------------------- Cài đặt (AppSetting)

SETTINGS_DEFAULT = {"start_years": 30, "end_years": 90, "step_years": 5,
                    "unit_name": "Đảng ủy Phường Kiểm Thử"}
SETTINGS_STEP10 = {"start_years": 30, "end_years": 90, "step_years": 10,
                   "unit_name": "Đảng ủy Phường Kiểm Thử"}
SETTINGS_NO_UNIT = {"start_years": 30, "end_years": 90, "step_years": 5,
                    "unit_name": None}

# ---------------------------------------------------------------- Đợt trao huy hiệu
#
# Bộ chính: 4 đợt, cố ý CHỪA khoảng trống để kiểm thử QT7 và banner phủ kín (QT6).
# Đợt 3/2 mở rộng tới 05/03 để chứa 28/02 - phục vụ ca 29/02 thu về 28/02 (QT2).

PERIODS_MAIN = [
    Period("P1", "Đợt 3/2", 15, 1, 5, 3),
    Period("P2", "Đợt 19/5", 1, 5, 31, 5),
    Period("P3", "Đợt 2/9", 15, 8, 10, 9),
    Period("P4", "Đợt 7/11", 1, 10, 7, 11),
]

# Bộ phụ: biên đợt rơi đúng 29/02 -> gắn năm không nhuận phải thu về 28/02 (QT4).
PERIODS_LEAP_EDGE = [
    Period("PL1", "Đợt nhuận 29/02", 29, 2, 5, 3),
]

# Bộ phụ: hai đợt chồng lấn -> QT6 phải cảnh báo mà vẫn cho lưu.
PERIODS_OVERLAP = [
    Period("PO1", "Đợt A", 1, 10, 7, 11),
    Period("PO2", "Đợt B", 1, 11, 30, 11),
]

# Bộ phụ: phủ kín 01/01-31/12 -> banner phủ kín phải tắt, M4 phải trống.
PERIODS_FULL_COVER = [
    Period("PF1", "Nửa đầu năm", 1, 1, 30, 6),
    Period("PF2", "Nửa cuối năm", 1, 7, 31, 12),
]

# ---------------------------------------------------------------- Đảng viên - bộ lõi
#
# 31 người, mỗi người phục vụ một ca biên cụ thể. Bộ này dùng cho MỌI kiểm định
# có con số chính xác (đủ điều kiện, badge, phân bổ theo mốc).

_D = date

MEMBERS_CORE: list[Member] = [
    # --- Biên đợt: đúng Từ ngày / đúng Đến ngày / lệch 1 ngày ra ngoài (QT4, QT7)
    Member("B01", "Nguyễn Văn An", _D(1974, 3, 12), "Nam", _D(1996, 10, 1),
           "QT4 biên dưới: tròn 30 đúng Từ ngày Đợt 7/11 (01/10/2026)"),
    Member("B02", "Trần Thị Bình", _D(1973, 7, 5), "Nữ", _D(1996, 11, 7),
           "QT4 biên trên: tròn 30 đúng Đến ngày Đợt 7/11 (07/11/2026)"),
    Member("B03", "Lê Văn Cường", _D(1972, 11, 21), "Nam", _D(1996, 9, 30),
           "QT4/QT7: lệch 1 ngày TRƯỚC Từ ngày Đợt 7/11 -> khoảng trống giữa Đợt 2/9 và Đợt 7/11"),
    Member("B04", "Phạm Thị Dung", _D(1975, 2, 9), "Nữ", _D(1996, 11, 8),
           "QT4/QT7: lệch 1 ngày SAU Đến ngày Đợt 7/11 -> khoảng trống sau đợt cuối cùng"),
    Member("B05", "Hoàng Văn Em", _D(1971, 6, 30), "Nam", _D(1996, 1, 15),
           "QT4 biên dưới: tròn 30 đúng Từ ngày Đợt 3/2 (15/01/2026)"),
    Member("B06", "Vũ Thị Giang", _D(1972, 9, 14), "Nữ", _D(1996, 3, 5),
           "QT4 biên trên: tròn 30 đúng Đến ngày Đợt 3/2 (05/03/2026)"),
    Member("B07", "Đỗ Văn Hải", _D(1970, 3, 3), "Nam", _D(1996, 1, 14),
           "QT7: lệch 1 ngày trước Đợt 3/2 -> khoảng trống trước đợt đầu tiên"),
    Member("B08", "Bùi Thị Hoa", _D(1973, 12, 28), "Nữ", _D(1996, 3, 6),
           "QT7: lệch 1 ngày sau Đợt 3/2 -> khoảng trống giữa Đợt 3/2 và Đợt 19/5"),
    Member("B09", "Đinh Văn Khoa", _D(1970, 4, 17), "Nam", _D(1996, 5, 1),
           "QT4 biên dưới Đợt 19/5 (01/05/2026)"),
    Member("B10", "Hà Thị Liên", _D(1971, 8, 2), "Nữ", _D(1996, 5, 31),
           "QT4 biên trên Đợt 19/5 (31/05/2026)"),
    Member("B11", "Chu Văn Mạnh", _D(1969, 9, 19), "Nam", _D(1996, 8, 15),
           "QT4 biên dưới Đợt 2/9 (15/08/2026)"),
    Member("B12", "Trương Thị Nhàn", _D(1974, 10, 25), "Nữ", _D(1996, 9, 10),
           "QT4 biên trên Đợt 2/9 (10/09/2026)"),

    # --- 29/02 (QT2)
    Member("L01", "Ngô Văn Khánh", _D(1976, 2, 29), "Nam", _D(1996, 2, 29),
           "QT2 năm đích KHÔNG nhuận: 29/02/1996 + 30 -> 28/02/2026, nằm trong Đợt 3/2"),
    Member("L02", "Dương Thị Lan", _D(1966, 6, 11), "Nữ", _D(1988, 2, 29),
           "QT2 năm đích NHUẬN: 29/02/1988 + 40 -> 29/02/2028 (giữ nguyên 29/02). "
           "Năm 2026 không có mốc nào -> không xuất hiện ở bất kỳ danh sách nào của 2026"),
    Member("L03", "Lâm Văn Lộc", _D(1968, 2, 29), "Nam", _D(1991, 5, 20),
           "Ngày sinh 29/02 (hiển thị), tròn 35 ngày 20/05/2026 trong Đợt 19/5; "
           "mốc 35 biến mất khi Bước = 10"),

    # --- Vượt mốc lớn nhất (QT3a -> '—')
    Member("M01", "Trịnh Văn Minh", _D(1917, 1, 1), "Nam", _D(1935, 5, 1),
           "QT3a: tuổi đảng 91 > mốc lớn nhất -> Mốc kế tiếp và Ngày tròn mốc kế tiếp đều là '—'; "
           "không có mốc nào trong 2026"),
    Member("M02", "Lý Thị Nga", _D(1918, 3, 20), "Nữ", _D(1936, 5, 1),
           "QT3a + QT4: tuổi đảng đúng 90 -> Mốc kế tiếp '—', đồng thời tròn mốc 90 "
           "ngày 01/05/2026 nên vẫn đủ điều kiện Đợt 19/5"),
    Member("M03", "Phan Văn Phúc", _D(1918, 8, 8), "Nam", _D(1936, 6, 10),
           "QT3a + QT7: tròn mốc 90 ngày 10/06/2026 rơi vào khoảng trống giữa Đợt 19/5 và Đợt 2/9"),

    # --- Dưới mốc nhỏ nhất
    Member("N01", "Đặng Thị Quỳnh", _D(1978, 2, 14), "Nữ", _D(1997, 10, 1),
           "Tuổi đảng 28, chưa tới mốc 30. Tròn 30 ngày 01/10/2027 -> chỉ đủ điều kiện ở NĂM SAU, "
           "dùng cho bộ chọn năm của UC-34"),
    Member("N02", "Tạ Văn Sơn", _D(1980, 5, 6), "Nam", _D(2000, 1, 15),
           "Tuổi đảng 26, mốc kế tiếp 30 vào 15/01/2030 -> không xuất hiện ở 2025/2026/2027"),

    # --- Ô trống (UC-20 hiển thị '—', xuất Excel để rỗng)
    Member("E01", "Cao Thị Thu", None, None, _D(1996, 5, 20),
           "Ngày sinh trống + Giới tính trống, đủ điều kiện Đợt 19/5 mốc 30"),
    Member("E02", "Mai Văn Tuấn", None, "Nam", _D(1996, 8, 25),
           "Ngày sinh trống, đủ điều kiện Đợt 2/9 mốc 30"),
    Member("E03", "Võ Thị Út", _D(1975, 7, 7), None, _D(1996, 8, 22),
           "Giới tính trống, đủ điều kiện Đợt 2/9 mốc 30"),

    # --- Đổi Bước 5 -> 10 (QT1 lan truyền, E2E-3)
    Member("S01", "Hồ Thị Vân", _D(1969, 1, 19), "Nữ", _D(1991, 10, 5),
           "Mốc 35 ngày 05/10/2026 trong Đợt 7/11 -> BIẾN MẤT khi Bước = 10"),
    Member("S02", "Nguyễn Văn Xuân", _D(1962, 4, 23), "Nam", _D(1986, 10, 20),
           "Mốc 40 ngày 20/10/2026 trong Đợt 7/11 -> GIỮ NGUYÊN khi Bước = 10 (ca đối chứng)"),
    Member("S03", "Trần Thị Yến", _D(1957, 11, 16), "Nữ", _D(1981, 11, 3),
           "Mốc 45 ngày 03/11/2026 trong Đợt 7/11 -> BIẾN MẤT khi Bước = 10"),
    Member("S04", "Ngô Thị Cẩm", _D(1966, 3, 8), "Nữ", _D(1991, 7, 15),
           "Mốc 35 ngày 15/07/2026 rơi vào khoảng trống -> khi Bước = 10 thì rời luôn khỏi "
           "màn Chưa thuộc đợt nào, badge trên menu giảm. Ca đối chứng cho E2E-3"),

    # --- Khoảng trống còn lại (QT7, UC-40)
    Member("G05", "Phùng Văn Bảo", _D(1970, 12, 12), "Nam", _D(1996, 7, 1),
           "QT7: tròn 30 ngày 01/07/2026 -> khoảng trống giữa Đợt 19/5 và Đợt 2/9"),

    # --- Sắp xếp theo chữ cái tiếng Việt (UC-11, OQ-3)
    Member("C01", "Đào Văn Ân", _D(1975, 1, 1), "Nam", _D(1996, 10, 3),
           "Mốc 30 ngày 03/10/2026 trong Đợt 7/11. Chữ Đ phải sắp TRƯỚC N và T "
           "theo thứ tự tiếng Việt, nhưng SAU theo thứ tự mã Unicode -> phân biệt collation"),
    Member("X01", "Nguyễn Thị Ánh", _D(1971, 2, 2), "Nữ", _D(1996, 2, 10),
           "Mốc 30 ngày 10/02/2026, nằm giữa lòng Đợt 3/2 (không phải biên)"),
    Member("X02", "Nguyễn Văn Ẩn", _D(1968, 9, 9), "Nam", _D(1991, 2, 20),
           "Mốc 35 ngày 20/02/2026 trong Đợt 3/2; tên khác X01 chỉ ở dấu -> kiểm tra sắp xếp và tìm kiếm"),

    # --- Biên "Ngày chính thức <= hôm nay"
    Member("V01", "Lưu Thị Diễm", _D(2000, 4, 4), "Nữ", T0,
           "Ngày chính thức ĐÚNG BẰNG hôm nay (T0) -> hợp lệ, tuổi đảng 0, mốc kế tiếp 30"),
]

# ---------------------------------------------------------------- Đảng viên - bộ lớn
#
# 1200 người chỉ để kiểm thử phân trang, tìm kiếm và hiệu năng.
# QUY TẮC THIẾT KẾ: Ngày chính thức đều nằm trong 2015-2020 nên mốc 30 của họ rơi
# vào 2045-2050. Vì vậy bộ này KHÔNG BAO GIỜ làm sai lệch các con số đủ điều kiện /
# badge của bộ lõi ở các năm 2025-2030, và có thể nạp chung với bộ lõi một cách an toàn.

BULK_COUNT = 1200

_SURNAMES = ["Nguyễn", "Trần", "Lê", "Phạm", "Hoàng", "Huỳnh", "Vũ", "Võ",
             "Phan", "Trương", "Bùi", "Đặng", "Đỗ", "Ngô", "Hồ", "Dương"]
_MIDDLES = ["Văn", "Thị", "Hữu", "Ngọc", "Minh", "Quang", "Thanh", "Đức"]
_GIVENS = ["An", "Bình", "Cường", "Dũng", "Giang", "Hạnh", "Hùng", "Khánh",
           "Lan", "Mai", "Nam", "Oanh", "Phúc", "Quân", "Sơn", "Tuấn",
           "Uyên", "Vinh", "Xuân", "Yến", "Ánh", "Ân", "Ẩn", "Ước", "Ý"]


def build_bulk() -> list[Member]:
    """Sinh bộ lớn bằng số học chỉ số - không dùng RNG, nên ổn định tuyệt đối."""
    out: list[Member] = []
    for i in range(BULK_COUNT):
        surname = _SURNAMES[i % len(_SURNAMES)]
        middle = _MIDDLES[(i // len(_SURNAMES)) % len(_MIDDLES)]
        given = _GIVENS[(i // 3) % len(_GIVENS)]
        full_name = f"{surname} {middle} {given} {i + 1:04d}"

        year = 2015 + (i % 6)
        month = (i % 12) + 1
        day = (i % 28) + 1
        admission = date(year, month, day)

        # 1 trong 20 người thiếu ngày sinh, 1 trong 25 người thiếu giới tính
        dob = None if i % 20 == 0 else date(1960 + (i % 40), ((i * 7) % 12) + 1, ((i * 3) % 28) + 1)
        gender = None if i % 25 == 0 else ("Nam" if i % 2 == 0 else "Nữ")

        out.append(Member(f"K{i + 1:04d}", full_name, dob, gender, admission,
                          "Bộ lớn: chỉ dùng cho phân trang / tìm kiếm / hiệu năng"))
    return out


MEMBERS_BULK = build_bulk()

# ---------------------------------------------------------------- Dòng lỗi import (QT9)
#
# Mỗi phần tử: (họ tên, ngày sinh, giới tính, ngày chính thức, mã lỗi, lý do mong đợi)
# Giá trị ghi nguyên văn như khi gõ vào ô Excel dạng CHUỖI.

ERR_ROWS = [
    ("", "12/03/1974", "Nam", "01/10/1996",
     "E-HOTEN-TRONG", "Thiếu Họ tên"),
    ("Nguyễn Thiếu Ngày", "12/03/1974", "Nam", "",
     "E-NGAYCT-TRONG", "Thiếu Ngày vào Đảng chính thức"),
    ("Trần Sai Định Dạng", "12/03/1974", "Nữ", "1996-10-01",
     "E-NGAYCT-SAIDANG", "Sai định dạng ngày, cần dd/MM/yyyy"),
    ("Lê Ngày Tương Lai", "12/03/1974", "Nam", "20/09/2026",
     "E-NGAYCT-TUONGLAI", "Ngày vào Đảng chính thức ở tương lai (so với hôm nay T0 = 19/09/2026)"),
    ("Phạm Giới Tính Lạ", "12/03/1974", "Khác", "01/10/1996",
     "E-GIOITINH", "Giới tính không hợp lệ, chỉ nhận Nam hoặc Nữ"),
    ("Hoàng Sinh Sau", "02/01/1997", "Nam", "01/10/1996",
     "E-NGAYSINH-SAU", "Ngày sinh sau Ngày vào Đảng chính thức"),
    ("Vũ Ngày Sinh Sai", "31/02/1974", "Nữ", "01/10/1996",
     "E-NGAYSINH-SAIDANG", "Ngày sinh không tồn tại / sai định dạng (chờ chốt OQ-1)"),
    ("", "31/02/1974", "Khác", "",
     "E-NHIEU-LOI", "Một dòng nhiều lỗi cùng lúc (chờ chốt OQ-2: liệt kê hết hay chỉ lỗi đầu)"),
]

# Dòng HỢP LỆ không gây tranh cãi - dùng chèn vào các file lỗi để chứng minh
# "nạp các dòng hợp lệ". Giới tính chỉ dùng đúng "Nam" / "Nữ" / để trống, họ tên
# không có khoảng trắng dư, để số dòng hợp lệ mong đợi là con số tuyệt đối.
OK_ROWS = [
    ("Nguyễn Hợp Lệ Một", "12/03/1974", "Nam", "01/10/1996"),
    ("Trần Hợp Lệ Hai", "05/07/1973", "Nữ", "07/11/1996"),
    ("Lê Hợp Lệ Ba", "", "", "15/01/1996"),
    ("Phạm Hợp Lệ Bốn", "09/02/1975", "Nữ", "31/05/1996"),
    ("Hoàng Hợp Lệ Năm", "30/06/1971", "Nam", "15/08/1996"),
    ("Vũ Hợp Lệ Sáu", "14/09/1972", "Nữ", "10/09/1996"),
]

# Biên trên của ràng buộc "Ngày chính thức <= hôm nay": đúng bằng T0.
OK_ROW_TODAY = ("Lưu Đúng Hôm Nay", "04/04/2000", "Nữ", "19/09/2026")

# Các dòng mà hành vi CHƯA ĐƯỢC CHỐT trong tài liệu - tách riêng để không làm
# mờ con số mong đợi của các file khác. Xem OQ-4, OQ-5 trong docs/test-plan.md.
NORMALIZE_ROWS = [
    ("  Nguyễn Có Khoảng Trắng  ", "12/03/1974", "Nam", "01/10/1996",
     "OQ-4", "Họ tên có khoảng trắng đầu/cuối - có cắt bỏ (trim) không?"),
    ("Trần Giới Tính Thường", "05/07/1973", "nữ", "07/11/1996",
     "OQ-5", "Giới tính viết thường - có nhận không?"),
    ("Lê Giới Tính Hoa", "21/11/1972", "NAM", "30/09/1996",
     "OQ-5", "Giới tính viết hoa - có nhận không?"),
    ("Phạm Ngày Một Chữ Số", "9/2/1975", "Nữ", "1/10/1996",
     "OQ-6", "Ngày viết d/M/yyyy (không đủ 2 chữ số) - có nhận không?"),
]

HEADER = ["Họ tên", "Ngày sinh", "Giới tính", "Ngày vào Đảng chính thức"]
