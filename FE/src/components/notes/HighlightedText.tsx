import { Fragment } from 'react';

/**
 * Tô sáng phần chữ khớp ô tìm của hộp ghi chú đầy đủ.
 *
 * Cách so khớp phải trùng khít `matchesKeyword` của `hooks/useClientTable`:
 * chứa chuỗi, không phân biệt hoa thường, CÓ phân biệt dấu. Lọc một kiểu mà tô
 * một kiểu thì có dòng lọt vào danh sách nhưng không thấy chỗ nào sáng lên.
 */
export function HighlightedText({ text, keyword }: { text: string; keyword: string }) {
  const needle = keyword.trim();
  if (needle === '') return <>{text}</>;

  const lowerText = text.toLowerCase();
  const lowerNeedle = needle.toLowerCase();
  const parts: { value: string; matched: boolean }[] = [];

  let from = 0;
  for (;;) {
    const at = lowerText.indexOf(lowerNeedle, from);
    if (at === -1) break;
    if (at > from) parts.push({ value: text.slice(from, at), matched: false });
    parts.push({ value: text.slice(at, at + needle.length), matched: true });
    from = at + needle.length;
  }
  if (from < text.length) parts.push({ value: text.slice(from), matched: false });

  return (
    <>
      {parts.map((part, index) => (
        <Fragment key={index}>
          {part.matched ? <mark className="hhd-note-mark">{part.value}</mark> : part.value}
        </Fragment>
      ))}
    </>
  );
}
