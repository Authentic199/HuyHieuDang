import { yearContextBadge, type YearContext } from './yearContext';

/**
 * Viên thuốc ngữ cảnh năm cạnh khoảng ngày, đúng hình dáng viên thuốc có chấm
 * màu của artboard 5.
 *
 * `distance` là số năm cách năm máy chủ. Bỏ trống thì hiểu là 1, tức vẫn đọc
 * "Năm trước" / "Năm sau" như cũ — thẻ đợt sắp tới ở Dashboard dùng kiểu đó.
 */
export function YearContextTag({
  context,
  distance = 1,
}: {
  context: YearContext;
  distance?: number;
}) {
  const tone = yearContextBadge(context, distance);

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
