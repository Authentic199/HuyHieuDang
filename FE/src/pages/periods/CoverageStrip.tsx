import { Tooltip } from 'antd';
import dayjs from 'dayjs';

import type { CoverageSegment } from '../../api/periods';
import type { AwardPeriodResponse, CoverageOverlap, IsoDate } from '../../types/domain';
import './CoverageStrip.css';

/**
 * Dải độ phủ 12 tháng (UC-36) — artboard 5.
 *
 * Mọi vị trí tính theo TỶ LỆ NGÀY trong năm `year`, không chia đều 12 phần:
 * tháng 2 ngắn hơn tháng 1 nên vạch tháng cũng hẹp hơn, bar mới nằm đúng chỗ.
 * Ngày "hôm nay" lấy từ máy chủ (mục 1.6 hợp đồng API), không đọc đồng hồ máy.
 *
 * Đợt vắt năm (Đến ngày sớm hơn Từ ngày) không có trong v1 nên không xử lý.
 */

interface CoverageStripProps {
  year: number;
  /** Hôm nay theo lịch máy chủ; null hoặc khác năm đang xét thì không vẽ vạch */
  today: IsoDate | null;
  segments: CoverageSegment[];
  overlaps: CoverageOverlap[];
  /** Dùng để biết đợt nào là đợt sắp tới — đợt đó tô vàng như artboard */
  periods: AwardPeriodResponse[];
}

/** Một bar đã tính sẵn vị trí, đơn vị phần trăm chiều ngang của dải. */
interface PlacedBar {
  key: string;
  label: string;
  left: number;
  width: number;
  lane: number;
  highlighted: boolean;
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
  // Đợt đang diễn ra / sắp tới được tô vàng như artboard 5.
  const highlightedIds = new Set(
    periods.filter((item) => item.status !== 'Past').map((item) => item.id),
  );

  const gaps = segments.filter((item) => item.type === 'Gap');
  const periodBars = assignLanes(
    segments
      .filter((item) => item.type === 'Period')
      .map((item, index) => ({
        key: item.periodId ?? `period-${index}`,
        label: item.name ?? '',
        tooltip: `${item.name ?? ''}: ${dayjs(item.fromDate).format('DD/MM')} – ${dayjs(item.toDate).format('DD/MM')}`,
        highlighted: item.periodId !== null && highlightedIds.has(item.periodId),
        ...place(item.fromDate, item.toDate, year),
      })),
  );

  const laneCount = Math.max(1, ...periodBars.map((bar) => bar.lane + 1));
  const laneHeight = (TRACK_HEIGHT - TRACK_PADDING * 2 - (laneCount - 1) * LANE_GAP) / laneCount;

  const parsedToday = today ? dayjs(today) : null;
  const todayValid = parsedToday?.isValid() && parsedToday.year() === year ? parsedToday : null;
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
            <i className="hhd-coverage__swatch hhd-coverage__swatch--period" />
            Đợt
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
                className={`hhd-coverage__bar${bar.highlighted ? ' hhd-coverage__bar--next' : ''}`}
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
