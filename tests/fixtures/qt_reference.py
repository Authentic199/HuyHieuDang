"""Cài đặt tham chiếu QT1-QT11 dùng làm oracle cho bộ dữ liệu kiểm thử.

Đây KHÔNG phải mã sản phẩm. Đây là bản hiện thực độc lập của các quy tắc
nghiệp vụ trong docs/2026-09-17-huyhieudang-business-design.md (v1.1) mục 3,
dùng để sinh ra các giá trị mong đợi trong tests/fixtures/data/expected.json.

Backend không được import file này. Mục đích của nó là: nếu service tính mốc
của Backend cho ra số khác expected.json thì một trong hai bên sai, và QC sẽ
soi lại cả hai.
"""

from __future__ import annotations

import calendar
from dataclasses import dataclass
from datetime import date


# ---------------------------------------------------------------- QT1


def milestones(start: int, end: int, step: int) -> list[int]:
    """QT1 - Dãy mốc huy hiệu sinh từ cài đặt (Bắt đầu, Kết thúc, Bước)."""
    if start <= 0 or end <= 0 or step < 1:
        raise ValueError("QT1: cả 3 tham số phải là số nguyên dương, Bước >= 1")
    if start > end:
        raise ValueError("QT1: Bắt đầu phải <= Kết thúc")
    out: list[int] = []
    n = start
    while n <= end:
        out.append(n)
        n += step
    return out


# ---------------------------------------------------------------- QT2


def bind_day_month(day: int, month: int, year: int) -> date:
    """Gắn năm vào một cặp ngày/tháng. 29/02 ở năm không nhuận -> 28/02."""
    if month == 2 and day == 29 and not calendar.isleap(year):
        return date(year, 2, 28)
    return date(year, month, day)


def anniversary(d: date, n: int) -> date:
    """QT2 - Ngày tròn mốc = D + N năm. 29/02 ở năm đích không nhuận -> 28/02."""
    return bind_day_month(d.day, d.month, d.year + n)


# ---------------------------------------------------------------- QT3


def party_age(d: date, today: date) -> int:
    """QT3 - Tuổi đảng: số năm tròn đã qua (hoặc đúng) ngày kỷ niệm."""
    k = today.year - d.year
    while k > 0 and anniversary(d, k) > today:
        k -= 1
    if k < 0:
        k = 0
    return k


# ---------------------------------------------------------------- QT3a


def next_milestone(d: date, today: date, ms: list[int]) -> int | None:
    """QT3a - Mốc nhỏ nhất lớn hơn tuổi đảng hiện tại; hết mốc -> None (hiển thị '—')."""
    age = party_age(d, today)
    for n in ms:
        if n > age:
            return n
    return None


# ---------------------------------------------------------------- Đợt


@dataclass(frozen=True)
class Period:
    code: str
    name: str
    from_day: int
    from_month: int
    to_day: int
    to_month: int

    def bind(self, year: int) -> tuple[date, date]:
        return (
            bind_day_month(self.from_day, self.from_month, year),
            bind_day_month(self.to_day, self.to_month, year),
        )

    @property
    def sort_key(self) -> tuple[int, int]:
        return (self.from_month, self.from_day)

    def label(self) -> str:
        return f"{self.from_day:02d}/{self.from_month:02d} – {self.to_day:02d}/{self.to_month:02d}"

    def validate(self) -> None:
        """QT6 - Từ <= Đến trong cùng một năm dương lịch, không vắt qua 31/12."""
        f, t = self.bind(2028)  # năm nhuận: cho phép 29/02 ở cả hai đầu
        if f > t:
            raise ValueError(f"QT6: đợt {self.name} có Từ ngày > Đến ngày")


# ---------------------------------------------------------------- QT4


def eligible_milestone(d: date, period: Period, year: int, ms: list[int]) -> int | None:
    """QT4 - Mốc được trao trong đợt/năm, hoặc None nếu không đủ điều kiện."""
    f, t = period.bind(year)
    for n in ms:
        a = anniversary(d, n)
        if f <= a <= t:
            return n
    return None


def eligible_list(
    members: list["Member"], period: Period, year: int, ms: list[int]
) -> list[tuple["Member", int, date]]:
    """Danh sách đủ điều kiện của một đợt trong một năm, sắp theo Mốc rồi Họ tên."""
    rows: list[tuple[Member, int, date]] = []
    for m in members:
        n = eligible_milestone(m.admission, period, year, ms)
        if n is not None:
            rows.append((m, n, anniversary(m.admission, n)))
    rows.sort(key=lambda r: (r[1], vietnamese_sort_key(r[0].full_name)))
    return rows


# ---------------------------------------------------------------- QT7


def gap_ranges(periods: list[Period], year: int) -> list[tuple[date, date, str]]:
    """Các khoảng trống chưa phủ trong năm, kèm nhãn 'Khoảng trống' của UC-40."""
    ordered = sorted(periods, key=lambda p: p.sort_key)
    bounds = [(p, *p.bind(year)) for p in ordered]
    gaps: list[tuple[date, date, str]] = []
    cursor = date(year, 1, 1)
    for idx, (p, f, t) in enumerate(bounds):
        if cursor < f:
            label = (
                "Trước đợt đầu tiên"
                if idx == 0
                else f"Giữa {bounds[idx - 1][0].name} và {p.name}"
            )
            gaps.append((cursor, date.fromordinal(f.toordinal() - 1), label))
        if t >= cursor:
            cursor = date.fromordinal(t.toordinal() + 1)
    if cursor <= date(year, 12, 31):
        # Chưa cài đợt nào thì cả năm là khoảng trống nằm trước đợt đầu tiên, không phải sau
        # đợt cuối cùng — hợp đồng API mục 1.10, CEO chốt ngày 19/09/2026.
        label = "Trước đợt đầu tiên" if not bounds else "Sau đợt cuối cùng"
        gaps.append((cursor, date(year, 12, 31), label))
    return gaps


def missed_milestone(
    d: date, periods: list[Period], year: int, ms: list[int]
) -> tuple[int, date, str] | None:
    """QT7 - Tròn mốc trong năm nhưng không rơi vào đợt nào. Trả (mốc, ngày, nhãn khoảng trống)."""
    for n in ms:
        a = anniversary(d, n)
        if a.year != year:
            continue
        if any(f <= a <= t for f, t in (p.bind(year) for p in periods)):
            continue
        label = next(
            (lbl for gf, gt, lbl in gap_ranges(periods, year) if gf <= a <= gt),
            "Không xác định",
        )
        return (n, a, label)
    return None


# ---------------------------------------------------------------- QT8


def upcoming_period(periods: list[Period], today: date) -> tuple[Period, int] | None:
    """QT8 - Đợt sắp tới: đợt của năm nay có Đến >= hôm nay và Từ nhỏ nhất.

    Mọi đợt trong năm đã qua -> đợt sớm nhất của năm sau. Không có đợt -> None.
    """
    if not periods:
        return None
    y = today.year
    candidates = [p for p in periods if p.bind(y)[1] >= today]
    if candidates:
        return (min(candidates, key=lambda p: p.bind(y)[0]), y)
    return (min(periods, key=lambda p: p.bind(y + 1)[0]), y + 1)


# ---------------------------------------------------------------- QT11


def period_status(period: Period, today: date) -> tuple[str, int | None]:
    """QT11 - Trạng thái đợt trong năm hiện tại: Đã qua / Đang diễn ra / Sắp tới + số ngày còn lại."""
    f, t = period.bind(today.year)
    if t < today:
        return ("Đã qua", None)
    if f <= today <= t:
        return ("Đang diễn ra", None)
    return ("Sắp tới", (f - today).days)


# ---------------------------------------------------------------- QT6 cảnh báo


def overlaps(periods: list[Period], year: int) -> list[tuple[str, str]]:
    """QT6 - Các cặp đợt chồng lấn nhau (cảnh báo nhưng vẫn cho lưu)."""
    ordered = sorted(periods, key=lambda p: p.sort_key)
    out: list[tuple[str, str]] = []
    for i in range(len(ordered)):
        for j in range(i + 1, len(ordered)):
            fa, ta = ordered[i].bind(year)
            fb, tb = ordered[j].bind(year)
            if fa <= tb and fb <= ta:
                out.append((ordered[i].name, ordered[j].name))
    return out


# ---------------------------------------------------------------- Sắp xếp tiếng Việt

_VN_ALPHABET = [
    "a", "à", "ả", "ã", "á", "ạ",
    "ă", "ằ", "ẳ", "ẵ", "ắ", "ặ",
    "â", "ầ", "ẩ", "ẫ", "ấ", "ậ",
    "b", "c", "d", "đ",
    "e", "è", "ẻ", "ẽ", "é", "ẹ",
    "ê", "ề", "ể", "ễ", "ế", "ệ",
    "g", "h",
    "i", "ì", "ỉ", "ĩ", "í", "ị",
    "k", "l", "m", "n",
    "o", "ò", "ỏ", "õ", "ó", "ọ",
    "ô", "ồ", "ổ", "ỗ", "ố", "ộ",
    "ơ", "ờ", "ở", "ỡ", "ớ", "ợ",
    "p", "q", "r", "s", "t",
    "u", "ù", "ủ", "ũ", "ú", "ụ",
    "ư", "ừ", "ử", "ữ", "ứ", "ự",
    "v", "x",
    "y", "ỳ", "ỷ", "ỹ", "ý", "ỵ",
]
_VN_RANK = {ch: i for i, ch in enumerate(_VN_ALPHABET)}


def vietnamese_sort_key(s: str) -> tuple:
    """Khoá sắp xếp theo thứ tự chữ cái tiếng Việt (a < ă < â < b < c < d < đ < e ...).

    Xem OQ-3 trong docs/test-plan.md: dự án chưa chốt dùng collation nào, hàm này
    thể hiện thứ tự mà người dùng Việt Nam mong đợi.
    """
    # Khoảng trắng sắp trước mọi chữ cái, giống hành vi của collation thực tế.
    return tuple(
        (-1,) if ch == " " else (_VN_RANK.get(ch, 1000 + ord(ch)),)
        for ch in s.lower()
    )


# ---------------------------------------------------------------- Đảng viên


@dataclass(frozen=True)
class Member:
    code: str
    full_name: str
    date_of_birth: date | None
    gender: str | None
    admission: date
    intent: str

    def display(self, d: date | None) -> str:
        return d.strftime("%d/%m/%Y") if d else "—"
