import { Tooltip } from 'antd';
import dayjs from 'dayjs';

import type { CoverageSegment } from '../../api/periods';
import type {
  AwardPeriodResponse,
  AwardPeriodStatus,
  CoverageOverlap,
  IsoDate,
} from '../../types/domain';
import './CoverageStrip.css';

/**
 * Dải độ phủ 12 tháng (UC-36) — artboard 5.
 *
 * Mọi vị trí tính theo TỶ LỆ NGÀY trong năm `year`, không chia đều 12 phần:
 * tháng 2 ngắn hơn tháng 1 nên vạch tháng cũng hẹp hơn, bar mới nằm đúng chỗ.
 * Ngày "hôm nay" lấy từ máy chủ (mục 1.6 hợp đồng API), không đọc đồng hồ máy.
 *
 * Đợt vắt qua 31/12 được máy chủ cắt sẵn thành HAI đoạn trong cùng một năm —
 * đuôi ở đầu năm và đầu ở cuối năm — mang cùng `periodId`, nên khóa của mỗi
 * đoạn phải ghép thêm ngày bắt đầu.
 *
 * MÀU BAR THEO TRẠNG THÁI (quyết định 30/09/2026, lệch artboard 5). Trước đây
 * bar tô ngược nghĩa: đợt đã qua màu đỏ, đợt đang diễn ra lại vàng giống đợt
 * sắp tới. Giờ mỗi bar mang đúng màu của nhãn tương ứng ở cột Trạng thái —
 * xám "Đã qua", đỏ "Đang diễn ra", vàng "Sắp tới".
 *
 * Trạng thái của một bar tính từ khoảng ngày của CHÍNH ĐOẠN ĐÓ so với `today`,
 * bằng đúng phép so của QT11. Đợt không vắt năm chỉ có một đoạn nên luôn trùng
 * nhãn trong bảng. Đợt vắt năm có hai đoạn và mỗi đoạn tô theo khoảng riêng:
 * mọi thứ bên trái vạch Hôm nay đều đã qua, nên đoạn đuôi đầu năm không được
 * tô vàng "sắp tới". Hệ quả chấp nhận: từ 01/01 tới hết đoạn đầu năm, đoạn đó
 * tô đỏ vì lần diễn ra năm trước đang chạy thật, trong khi cột Trạng thái vẫn
 * ghi "Sắp tới" — QT11 xét lần diễn ra neo năm nay, và QT11 không đổi.
 */

interface CoverageStripProps {
  year: number;
  /** Hôm nay theo lịch máy chủ; null hoặc khác năm đang xét thì không vẽ vạch */
  today: IsoDate | null;
  segments: CoverageSegment[];
  overlaps: CoverageOverlap[];
  /**
   * Dự phòng khi `today` rỗng, sai hoặc không thuộc năm đang vẽ: lúc đó bar lấy
   * `status` của đợt trong danh sách này thay vì tự so ngày. Máy chủ đúng hợp
   * đồng thì không rơi vào trường hợp này.
   */
  periods: AwardPeriodResponse[];
}

/** Ba loại màu bar, khớp ba nhãn của cột Trạng thái. */
type BarTone = 'past' | 'ongoing' | 'upcoming';

/** Một bar đã tính sẵn vị trí, đơn vị phần trăm chiều ngang của dải. */
interface PlacedBar {
  key: string;
  label: string;
  left: number;
  width: number;
  lane: number;
  tone: BarTone;
  tooltip: string;
}

const MONTH_LABELS = ['T1', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'T8', 'T9', 'T10', 'T11', 'T12'];

/** Chiều cao của cả dải, lấy đúng 56px trong artboard 5. */
const TRACK_HEIGHT = 56;
/** Khoảng hở giữa bar và mép dải, cũng lấy từ artboard (top:6px bottom:6px). */
const TRACK_PADDING = 6;
const LANE_GAP = 4;

/** dayjs không bật sẵn plugin dayOfYear nên đếm thẳng từ mốc 01/01. */
function dayNumber(date: string, year: number): number {
  return dayjs(date).diff(dayjs(`${year}-01-01`), 'day') + 1;
}

/** Số ngày của cả năm — 366 với năm nhuận. */
function lastDayNumber(year: number): number {
  return dayNumber(`${year}-12-31`, year);
}

/** Trạng thái của một khoảng ngày so với hôm nay — đúng phép so của QT11. */
function toneOfRange(fromDate: string, toDate: string, todayIso: string): BarTone {
  // Cùng định dạng YYYY-MM-DD nên so chuỗi là so ngày, không cần dựng dayjs.
  if (toDate < todayIso) return 'past';
  if (fromDate > todayIso) return 'upcoming';
  return 'ongoing';
}

/** Màu dự phòng lấy từ `status` của đợt trong bảng. */
function toneOfStatus(status: AwardPeriodStatus | undefined): BarTone {
  if (status === 'Ongoing') return 'ongoing';
  if (status === 'Upcoming') return 'upcoming';
  return 'past';
}

/** Phần trăm bắt đầu và bề ngang của một khoảng ngày trong năm. */
function place(fromDate: string, toDate: string, year: number) {
  const total = lastDayNumber(year);
  const from = Math.max(1, dayNumber(fromDate, year));
  const to = Math.min(total, dayNumber(toDate, year));
  return {
    left: ((from - 1) / total) * 100,
    width: (Math.max(1, to - from + 1) / total) * 100,
  };
}

/**
 * Xếp các đợt vào hàng: đợt nào chồng lên đợt đã xếp thì xuống hàng dưới, nhờ
 * vậy hai đợt chồng lấn hiện rõ cả hai chứ không che nhau.
 */
function assignLanes(bars: Omit<PlacedBar, 'lane'>[]): PlacedBar[] {
  const laneEnds: number[] = [];
  return bars.map((bar) => {
    let lane = laneEnds.findIndex((end) => end <= bar.left + 0.001);
    if (lane === -1) {
      lane = laneEnds.length;
      laneEnds.push(0);
    }
    laneEnds[lane] = bar.left + bar.width;
    return { ...bar, lane };
  });
}

export function CoverageStrip({ year, today, segments, overlaps, periods }: CoverageStripProps) {
  const parsedToday = today ? dayjs(today) : null;
  const todayValid = parsedToday?.isValid() && parsedToday.year() === year ? parsedToday : null;
  const todayIso = todayValid ? todayValid.format('YYYY-MM-DD') : null;
  const statusById = new Map(periods.map((item) => [item.id, item.status]));

  const gaps = segments.filter((item) => item.type === 'Gap');
  const periodBars = assignLanes(
    segments
      .filter((item) => item.type === 'Period')
      .map((item, index) => ({
        // Đợt vắt năm cho hai đoạn cùng periodId nên khóa ghép thêm ngày bắt đầu.
        key: `${item.periodId ?? `period-${index}`}#${item.fromDate}`,
        label: item.name ?? '',
        tooltip: `${item.name ?? ''}: ${dayjs(item.fromDate).format('DD/MM')} – ${dayjs(item.toDate).format('DD/MM')}`,
        tone: todayIso
          ? toneOfRange(item.fromDate, item.toDate, todayIso)
          : toneOfStatus(item.periodId ? statusById.get(item.periodId) : undefined),
        ...place(item.fromDate, item.toDate, year),
      })),
  );

  const laneCount = Math.max(1, ...periodBars.map((bar) => bar.lane + 1));
  const laneHeight = (TRACK_HEIGHT - TRACK_PADDING * 2 - (laneCount - 1) * LANE_GAP) / laneCount;

  const todayLeft = todayValid
    ? ((dayNumber(todayValid.format('YYYY-MM-DD'), year) - 0.5) / lastDayNumber(year)) * 100
    : null;

  const months = MONTH_LABELS.map((label, index) => {
    const first = dayjs(`${year}-${String(index + 1).padStart(2, '0')}-01`);
    return { label, width: (first.daysInMonth() / lastDayNumber(year)) * 100 };
  });

  return (
    <section className="hhd-coverage" aria-label={`Độ phủ trong năm ${year}`}>
      <div className="hhd-coverage__head">
        <span className="hhd-coverage__title">Độ phủ trong năm</span>
        <div className="hhd-coverage__legend">
          <span>
            <i className="hhd-coverage__swatch hhd-coverage__swatch--past" />
            Đã qua
          </span>
          <span>
            <i className="hhd-coverage__swatch hhd-coverage__swatch--ongoing" />
            Đang diễn ra
          </span>
          <span>
            <i className="hhd-coverage__swatch hhd-coverage__swatch--upcoming" />
            Sắp tới
          </span>
          <span>
            <i className="hhd-coverage__swatch hhd-coverage__swatch--gap" />
            Khoảng trống
          </span>
          {overlaps.length > 0 ? (
            <span>
              <i className="hhd-coverage__swatch hhd-coverage__swatch--overlap" />
              Chồng lấn
            </span>
          ) : null}
          <span>
            <i className="hhd-coverage__swatch hhd-coverage__swatch--today" />
            Hôm nay
          </span>
        </div>
      </div>

      <div className="hhd-coverage__stage">
        <div className="hhd-coverage__glow" />
        <div className="hhd-coverage__track" style={{ height: TRACK_HEIGHT }}>
          {gaps.map((gap) => {
            const box = place(gap.fromDate, gap.toDate, year);
            return (
              <Tooltip
                key={`gap-${gap.fromDate}`}
                title={`Chưa đợt nào phủ: ${dayjs(gap.fromDate).format('DD/MM')} – ${dayjs(gap.toDate).format('DD/MM')}`}
              >
                <div
                  className="hhd-coverage__gap"
                  style={{ left: `${box.left}%`, width: `${box.width}%` }}
                />
              </Tooltip>
            );
          })}

          {periodBars.map((bar) => (
            <Tooltip key={bar.key} title={bar.tooltip}>
              <div
                className={`hhd-coverage__bar hhd-coverage__bar--${bar.tone}`}
                style={{
                  left: `${bar.left}%`,
                  width: `${bar.width}%`,
                  top: TRACK_PADDING + bar.lane * (laneHeight + LANE_GAP),
                  height: laneHeight,
                }}
              >
                {/* Bar hẹp quá thì chữ tràn ra ngoài — để trống, tên vẫn đọc
                    được khi rê chuột và luôn có đủ trong bảng bên dưới. */}
                {bar.width >= 7 ? <span>{bar.label}</span> : null}
              </div>
            </Tooltip>
          ))}

          {overlaps.map((overlap) => {
            const box = place(overlap.fromDate, overlap.toDate, year);
            return (
              <Tooltip
                key={`${overlap.firstPeriodId}-${overlap.secondPeriodId}`}
                title={`${overlap.firstPeriodName} và ${overlap.secondPeriodName} chồng lấn ${overlap.fromDisplay} – ${overlap.toDisplay}`}
              >
                <div
                  className="hhd-coverage__overlap"
                  style={{ left: `${box.left}%`, width: `${box.width}%` }}
                />
              </Tooltip>
            );
          })}

          {todayLeft !== null && todayValid ? (
            <>
              <div className="hhd-coverage__today-line" style={{ left: `${todayLeft}%` }} />
              <div className="hhd-coverage__today-chip" style={{ left: `${todayLeft}%` }}>
                Hôm nay {todayValid.format('DD/MM')}
              </div>
            </>
          ) : null}
        </div>
      </div>

      <div className="hhd-coverage__months">
        {months.map((month) => (
          <span key={month.label} style={{ width: `${month.width}%` }}>
            {month.label}
          </span>
        ))}
      </div>
    </section>
  );
}
