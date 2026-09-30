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
   * Cuộn hàng chứa năm đang xem vào giữa và đặt tiêu điểm vào đúng ô đó, để bấm
   * mũi tên là đi được ngay. Làm đúng một lần, khi bảng vừa mở.
   *
   * Lớp phủ của Ant Design dựng nội dung trong lúc còn ẩn, khi đó lưới chưa có
   * kích thước thật — gán `scrollTop` lúc ấy không ăn. Nên chờ qua vài khung
   * hình cho tới khi lưới đã cao thật rồi mới cuộn.
   */
  useEffect(() => {
    let frame = 0;
    let attempts = 0;

    function center() {
      const grid = gridRef.current;
      const cell = activeRef.current;
      if (!grid || !cell) return;
      if (grid.clientHeight === 0 || grid.scrollHeight <= grid.clientHeight) {
        if (attempts < 30) {
          attempts += 1;
          frame = window.requestAnimationFrame(center);
        }
        return;
      }
      // Không dùng scrollIntoView: nó cuộn cả trang bên ngoài lớp phủ.
      grid.scrollTop = cell.offsetTop - grid.clientHeight / 2 + cell.offsetHeight / 2;
      cell.focus({ preventScroll: true });
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

      <div className="hhd-year-panel__grid" ref={gridRef} role="grid" aria-label="Danh sách năm">
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
