import { LeftOutlined, RightOutlined } from '@ant-design/icons';
import { Popover } from 'antd';
import { useCallback, useEffect, useRef, useState } from 'react';
import type { KeyboardEvent as ReactKeyboardEvent } from 'react';

import './YearPicker.css';

/**
 * Bộ chọn năm dùng chung của màn Chi tiết đợt (UC-34) và màn Chưa thuộc đợt nào
 * (UC-40) — quyết định của chủ dự án ngày 30/09 thay cho bộ Segmented ba năm.
 *
 * Một rãnh xám chứa `‹ [năm] ›`. Bấm `‹ ›` để lùi hoặc tiến một năm; bấm chính
 * ô năm thì mở bảng chọn năm, để nhảy thẳng tới năm xa thay vì bấm nhiều lần.
 *
 * Năm mặc định và mọi giới hạn đều tính từ năm hiện tại của MÁY CHỦ, không lấy
 * đồng hồ trình duyệt. Chưa biết năm máy chủ thì cả bộ chọn bị khóa.
 */

/** Khoảng năm mà API nhận (`Mes.Query.Invalid.Year`) — không vượt qua được. */
const API_MIN_YEAR = 1900;
const API_MAX_YEAR = 2200;

/** Đi xa nhất 100 năm mỗi phía tính từ năm máy chủ. */
const REACH_YEARS = 100;

/** Mỗi hàng trong bảng chọn có 5 năm, căn theo bội số của 5: 2020–2024, 2025–2029… */
const COLUMNS = 5;

interface YearBounds {
  minYear: number;
  maxYear: number;
}

function boundsOf(serverYear: number): YearBounds {
  return {
    minYear: Math.max(API_MIN_YEAR, serverYear - REACH_YEARS),
    maxYear: Math.min(API_MAX_YEAR, serverYear + REACH_YEARS),
  };
}

function clamp(year: number, { minYear, maxYear }: YearBounds): number {
  return Math.min(maxYear, Math.max(minYear, year));
}

export interface YearPickerProps {
  /** Năm đang xem; null khi chưa biết năm máy chủ */
  value: number | null;
  /** Năm hiện tại của MÁY CHỦ — gốc của mọi giới hạn và của nút "Năm nay" */
  serverYear: number | null;
  onChange: (year: number) => void;
}

export function YearPicker({ value, serverYear, onChange }: YearPickerProps) {
  const [open, setOpen] = useState(false);
  const yearButtonRef = useRef<HTMLButtonElement>(null);
  /** Chỉ trả tiêu điểm khi bảng vừa đóng, không trả ngay lần dựng đầu tiên. */
  const wasOpen = useRef(false);

  useEffect(() => {
    if (wasOpen.current && !open) yearButtonRef.current?.focus();
    wasOpen.current = open;
  }, [open]);

  const ready = serverYear !== null && value !== null;
  const bounds = serverYear === null ? null : boundsOf(serverYear);

  const canGoBack = ready && bounds !== null && value > bounds.minYear;
  const canGoForward = ready && bounds !== null && value < bounds.maxYear;

  function step(delta: number) {
    if (!ready || bounds === null) return;
    const next = clamp(value + delta, bounds);
    if (next !== value) onChange(next);
  }

  function pick(year: number) {
    setOpen(false);
    if (year !== value) onChange(year);
  }

  return (
    <div className="hhd-year-picker" role="group" aria-label="Chọn năm">
      <button
        type="button"
        className="hhd-year-picker__step"
        aria-label="Lùi một năm"
        disabled={!canGoBack}
        onClick={() => step(-1)}
      >
        <LeftOutlined />
      </button>

      <Popover
        open={open && ready}
        onOpenChange={setOpen}
        trigger="click"
        placement="bottomLeft"
        arrow={false}
        destroyOnHidden
        // Tắt hiệu ứng phóng to của lớp phủ. Trong lúc nó chạy, lưới chưa có
        // kích thước thật và trình duyệt còn cuộn thêm một nhịp, nên vị trí cuộn
        // đặt lúc mở bị đạp đổ — bảng nhảy về sai hàng. Bảng chọn năm hiện ra
        // tức thì cũng đúng tinh thần thiết kế: bấm một lần là thấy ngay.
        transitionName=""
        rootClassName="hhd-year-panel-root"
        content={
          ready && bounds !== null && serverYear !== null ? (
            <YearPanel
              value={value}
              serverYear={serverYear}
              bounds={bounds}
              onPick={pick}
              onClose={() => setOpen(false)}
            />
          ) : null
        }
      >
        <button
          type="button"
          ref={yearButtonRef}
          className="hhd-year-picker__year"
          aria-label={`Chọn năm khác, đang xem ${value ?? ''}`}
          aria-haspopup="dialog"
          aria-expanded={open && ready}
          disabled={!ready}
        >
          {value ?? '—'}
        </button>
      </Popover>

      <button
        type="button"
        className="hhd-year-picker__step"
        aria-label="Tiến một năm"
        disabled={!canGoForward}
        onClick={() => step(1)}
      >
        <RightOutlined />
      </button>
    </div>
  );
}

interface YearPanelProps {
  value: number;
  serverYear: number;
  bounds: YearBounds;
  onPick: (year: number) => void;
  onClose: () => void;
}

/**
 * Bảng chọn năm: đầu bảng có nút "Năm nay" và câu ghi rõ khoảng chọn được, thân
 * bảng là lưới 5 cột cuộn dọc. Khi mở, lưới cuộn sẵn để hàng chứa năm đang xem
 * nằm giữa — không phải tự đi tìm.
 */
function YearPanel({ value, serverYear, bounds, onPick, onClose }: YearPanelProps) {
  const { minYear, maxYear } = bounds;
  /** Lưới căn theo bội số của 5 nên hàng đầu và hàng cuối thừa ra vài ô mờ. */
  const firstYear = Math.floor(minYear / COLUMNS) * COLUMNS;
  const lastYear = Math.ceil((maxYear + 1) / COLUMNS) * COLUMNS - 1;
  const years = Array.from({ length: lastYear - firstYear + 1 }, (_, index) => firstYear + index);

  /** Ô đang nhận tiêu điểm trong lưới — bàn phím di chuyển ô này. */
  const [activeYear, setActiveYear] = useState(value);
  const gridRef = useRef<HTMLDivElement>(null);
  const activeRef = useRef<HTMLButtonElement>(null);
  /**
   * Cuộn hàng chứa năm đang xem vào giữa lưới, và trao tiêu điểm cho lưới để
   * bấm phím mũi tên là đi được ngay.
   *
   * Ba điều dễ sai, ca "hàng của năm đang xem nằm giữa lưới" canh cả ba:
   *
   * 1. `cell.offsetTop` chỉ đúng khi CHÍNH LƯỚI là `offsetParent` của ô năm —
   *    vì vậy `.hhd-year-panel__grid` phải giữ `position: relative`. Bỏ dòng đó
   *    thì `offsetParent` rơi ra lớp phủ của Ant Design, `offsetTop` cộng thừa
   *    cả đệm bảng lẫn dòng đầu "Năm nay", lưới cuộn lố gần một hàng và đẩy năm
   *    đang xem lên hàng thứ hai.
   * 2. Tiêu điểm trao cho lưới, KHÔNG cho ô năm: ô năm có tiêu điểm thì trình
   *    duyệt kéo nó lên đầu vùng cuộn, đạp đổ phép căn giữa. Lưới nhận phím
   *    thay cho ô là đủ — bộ nghe phím nằm ở gốc bảng, còn ô năm chỉ nhận tiêu
   *    điểm khi người dùng thật sự bấm mũi tên, lúc đó cuộn theo là đúng ý.
   * 3. Lớp phủ dựng nội dung trong lúc còn ẩn, khi đó lưới chưa có kích thước
   *    thật nên gán `scrollTop` không ăn. Vòng dưới đây thử lại mỗi khung hình
   *    cho tới khi vị trí cuộn đứng yên vài khung liền. (Hiệu ứng phóng to của
   *    lớp phủ cũng đã tắt bằng `transitionName=""` ở trên, vì trong lúc nó
   *    chạy mọi số đo đều còn đang thay đổi.)
   */
  useEffect(() => {
    /** Số khung hình liền nhau vị trí cuộn phải đứng yên thì mới coi là xong. */
    const STABLE_FRAMES = 5;
    /** Trần an toàn, khoảng một giây — không để vòng chạy mãi. */
    const MAX_FRAMES = 60;

    let frame = 0;
    let attempts = 0;
    let stable = 0;

    function center() {
      const grid = gridRef.current;
      const cell = activeRef.current;
      if (!grid || !cell) return;
      attempts += 1;

      // Trao tiêu điểm ngay khung hình đầu: bấm phím mũi tên là đi được luôn,
      // không phải đợi vòng căn giữa bên dưới chạy xong.
      if (attempts === 1) grid.focus({ preventScroll: true });

      if (grid.clientHeight > 0 && grid.scrollHeight > grid.clientHeight) {
        // Đưa tâm ô về đúng tâm lưới. Trình duyệt tự kẹp lại khi năm sát hai đầu
        // khoảng và lưới hết chỗ cuộn.
        // Không dùng scrollIntoView: nó cuộn cả trang bên ngoài lớp phủ.
        const want = cell.offsetTop + cell.offsetHeight / 2 - grid.clientHeight / 2;
        if (Math.abs(grid.scrollTop - want) <= 1) {
          stable += 1;
        } else {
          grid.scrollTop = want;
          stable = 0;
        }
      }

      // Còn đang phóng to thì hộp bao còn thấp hơn chiều cao thật của lưới.
      const grown = grid.getBoundingClientRect().height >= grid.clientHeight - 1;
      if ((!grown || stable < STABLE_FRAMES) && attempts < MAX_FRAMES) {
        frame = window.requestAnimationFrame(center);
      }
    }

    center();
    return () => window.cancelAnimationFrame(frame);
  }, []);

  const moveTo = useCallback(
    (year: number) => {
      const next = clamp(year, bounds);
      setActiveYear(next);
      // Tiêu điểm đi theo ô, và trình duyệt tự cuộn ô đó vào tầm nhìn của lưới.
      window.requestAnimationFrame(() => activeRef.current?.focus());
    },
    [bounds],
  );

  function handleKeyDown(event: ReactKeyboardEvent<HTMLDivElement>) {
    if (event.key === 'Escape') {
      event.preventDefault();
      onClose();
      return;
    }
    const moves: Record<string, number> = {
      ArrowLeft: -1,
      ArrowRight: 1,
      ArrowUp: -COLUMNS,
      ArrowDown: COLUMNS,
    };
    const delta = moves[event.key];
    if (delta !== undefined) {
      event.preventDefault();
      moveTo(activeYear + delta);
    }
  }

  return (
    <div className="hhd-year-panel" role="dialog" aria-label="Chọn năm" onKeyDown={handleKeyDown}>
      <div className="hhd-year-panel__head">
        <button type="button" className="hhd-year-panel__today" onClick={() => onPick(serverYear)}>
          Năm nay
        </button>
        <span className="hhd-year-panel__range">
          Chọn trong {minYear} – {maxYear}
        </span>
      </div>

      {/* `tabIndex={-1}`: lưới tự nhận tiêu điểm khi bảng mở, để phím mũi tên đi
          được ngay mà không phải kéo tiêu điểm vào một ô — xem `center()`. */}
      <div
        className="hhd-year-panel__grid"
        ref={gridRef}
        role="grid"
        aria-label="Danh sách năm"
        tabIndex={-1}
      >
        {years.map((year) => {
          const outOfRange = year < minYear || year > maxYear;
          const selected = year === value;
          const isServerYear = year === serverYear && !selected;
          const active = year === activeYear;

          return (
            <button
              key={year}
              type="button"
              ref={active ? activeRef : undefined}
              className={[
                'hhd-year-panel__cell',
                selected ? 'hhd-year-panel__cell--selected' : '',
                isServerYear ? 'hhd-year-panel__cell--today' : '',
              ]
                .filter(Boolean)
                .join(' ')}
              disabled={outOfRange}
              tabIndex={active ? 0 : -1}
              aria-current={selected ? 'true' : undefined}
              onFocus={() => setActiveYear(year)}
              onClick={() => onPick(year)}
            >
              {year}
            </button>
          );
        })}
      </div>
    </div>
  );
}
