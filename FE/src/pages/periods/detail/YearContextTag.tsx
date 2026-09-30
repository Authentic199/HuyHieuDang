import { yearContextBadge, type YearContext } from './yearContext';

/**
 * Viên thuốc "Năm trước / Năm nay / Năm sau" cạnh khoảng ngày, đúng hình dáng
 * viên thuốc có chấm màu của artboard 5.
 */
export function YearContextTag({ context }: { context: YearContext }) {
  const tone = yearContextBadge(context);

  return (
    <span
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: 6,
        height: 'var(--hhd-tag-height)',
        padding: '0 10px',
        borderRadius: 9999,
        whiteSpace: 'nowrap',
        background: tone.background,
        color: tone.text,
        font: "600 var(--hhd-fs-14)/var(--hhd-lh-14) 'Noto Sans', sans-serif",
      }}
    >
      {tone.dot ? (
        <i
          style={{
            width: 6,
            height: 6,
            borderRadius: '50%',
            background: tone.dot,
            flex: 'none',
          }}
        />
      ) : null}
      {tone.label}
    </span>
  );
}
