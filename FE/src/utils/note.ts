import { compareNumber, compareText } from '../hooks/useClientTable';
import type { EligibleMemberResponse, IsoDateTime } from '../types/domain';

/**
 * Ghi chú đảng viên (QT12) — phần dùng chung của bốn chỗ hiện ghi chú: nút ở
 * cột Thao tác, hộp của một người, form Thêm/Sửa, và khối gọn + hộp đầy đủ của
 * Dashboard và Chi tiết đợt.
 */

/** Trần ký tự của một ghi chú, đếm sau khi máy chủ cắt khoảng trắng hai đầu. */
export const NOTE_MAX_LENGTH = 500;

/**
 * Ngày ghi, lấy NGUYÊN phần `yyyy-MM-dd` đầu chuỗi.
 *
 * `noteUpdatedAt` là ngoại lệ múi giờ duy nhất của hợp đồng (mục 1.6 v1.6): máy
 * chủ trả giờ Việt Nam kèm `+07:00`. Đưa qua `dayjs` hay `Date` là quy về đồng
 * hồ trình duyệt — máy đặt sai múi giờ sẽ hiện lệch một ngày. Cắt chuỗi thì
 * ngày trên màn hình luôn đúng ngày máy chủ đã đóng.
 */
export function noteDateLabel(value: IsoDateTime | null | undefined): string | null {
  const matched = value?.match(/^(\d{4})-(\d{2})-(\d{2})/);
  if (!matched) return null;
  const [, year, month, day] = matched;
  return `Ghi ngày ${day}/${month}/${year}`;
}

/** Một dòng ghi chú đã gom đủ thứ cần hiện trong khối gọn và hộp đầy đủ. */
export interface NotedMember {
  key: string;
  fullName: string;
  milestone: number;
  note: string;
  noteUpdatedAt: IsoDateTime | null;
}

/**
 * Những người CÓ ghi chú trong danh sách đủ điều kiện, sắp Mốc rồi Họ tên đúng
 * như bảng (mục 1.8 hợp đồng API, cũng là thứ tự máy chủ trả) — sắp lại ở đây
 * để hộp ghi chú không lệ thuộc vào việc bảng đang được sắp theo cột nào.
 *
 * Cố ý nhận trọn danh sách của đợt và năm đang xem, KHÔNG nhận danh sách đã lọc:
 * khối gọn nói về cả đợt, không đổi theo ô tìm hay ô lọc của bảng.
 */
export function notedMembersOf(rows: EligibleMemberResponse[]): NotedMember[] {
  return rows
    .filter((row): row is EligibleMemberResponse & { note: string } => Boolean(row.note?.trim()))
    .map((row) => ({
      key: row.partyMemberId,
      fullName: row.fullName,
      milestone: row.milestone,
      note: row.note.trim(),
      noteUpdatedAt: row.noteUpdatedAt,
    }))
    .sort((a, b) => compareNumber(a.milestone, b.milestone) || compareText(a.fullName, b.fullName));
}

/**
 * Một dòng chữ nối các ghi chú cho khối gọn: `Họ tên: nội dung · Họ tên: …`.
 * Chỗ xuống dòng trong ghi chú đổi thành dấu cách để cả khối nằm trên một dòng;
 * phần thừa do CSS cắt bằng dấu `…`.
 */
export function noteSummaryText(members: NotedMember[]): string {
  return members
    .map((member) => `${member.fullName}: ${member.note.replace(/\s+/g, ' ')}`)
    .join(' · ');
}
