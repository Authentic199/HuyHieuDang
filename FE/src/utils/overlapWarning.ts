import type { CoverageOverlap } from '../types/domain';

/**
 * Danh sách tên đợt cho cảnh báo chồng lấn (T70).
 *
 * Máy chủ trả chồng lấn theo TỪNG CẶP, nên một đợt dính vào hai cặp sẽ xuất
 * hiện hai lần. Câu cảnh báo chỉ nêu mỗi đợt một lần: nhận diện theo `id` chứ
 * không theo tên, vì hai đợt khác nhau vẫn có thể trùng tên.
 *
 * Thứ tự giữ đúng lần xuất hiện đầu tiên trong `warnings.overlaps` (máy chủ đã
 * sắp theo khoảng ngày); trong mỗi cặp thì đợt thứ nhất đứng trước đợt thứ hai.
 *
 * Dùng chung cho banner màn Đợt và thẻ cảnh báo ở Dashboard, để câu ở hai màn
 * luôn giống hệt nhau.
 */
export function overlappingPeriodNames(overlaps: CoverageOverlap[]): string {
  const seen = new Set<string>();
  const names: string[] = [];

  for (const overlap of overlaps) {
    for (const [id, name] of [
      [overlap.firstPeriodId, overlap.firstPeriodName],
      [overlap.secondPeriodId, overlap.secondPeriodName],
    ] as const) {
      if (seen.has(id)) continue;
      seen.add(id);
      names.push(name);
    }
  }

  return names.join(', ');
}
