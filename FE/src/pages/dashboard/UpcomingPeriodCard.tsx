import { Button } from 'antd';
import { useNavigate } from 'react-router-dom';

import type { UpcomingPeriod } from '../../api/dashboard';
import { paths } from '../../routes/paths';
import { milestoneScale, shadow, statusBadge } from '../../theme/tokens';
import type { MilestoneBreakdown } from '../../types/domain';
import { formatDate, formatNumber } from '../../utils/format';
import { YearContextTag } from '../periods/detail/YearContextTag';

/**
 * Thẻ đợt sắp tới (UC-10, QT8) — artboard 2: bên trái là tên đợt, khoảng ngày
 * ĐÃ GẮN NĂM và số người đủ điều kiện; bên phải là phân bổ theo mốc.
 *
 * Đợt sắp tới do máy chủ chọn theo QT8 và có thể thuộc NĂM SAU (`isNextYear`)
 * khi mọi đợt năm nay đã qua — lúc đó thẻ có thêm viên thuốc "Năm sau" để cán
 * bộ không nhầm với đợt của năm đang xem.
 *
 * Chưa có đợt nào thì `upcomingPeriod = null` → thẻ hiện đúng câu thiết kế
 * "Chưa cài đợt trao huy hiệu" kèm nút sang màn Đợt trao huy hiệu.
 */

/** Màu viên thuốc số lượng theo thang "Cool Waters" (artboard 0). */
function milestoneStep(milestone: number): (typeof milestoneScale)[number] {
  let step: (typeof milestoneScale)[number] = milestoneScale[0];
  for (const candidate of milestoneScale) {
    if (milestone >= candidate.from) step = candidate;
  }
  return step;
}

/** Viên thuốc "30 năm · 2" trong khối Phân bổ theo mốc. */
function MilestonePill({ item }: { item: MilestoneBreakdown }) {
  const step = milestoneStep(item.milestone);

  return (
    <span className="hhd-dashboard__pill">
      {item.milestone} năm
      <span
        className="hhd-dashboard__pill-count"
        style={{ background: step.background, boxShadow: shadow.pill, color: step.text }}
      >
        {formatNumber(item.count)}
      </span>
    </span>
  );
}

/** Viên thuốc trạng thái: "Đang diễn ra" khi đợt đã mở, còn lại là số ngày. */
function CountdownTag({ period }: { period: UpcomingPeriod }) {
  const ongoing = period.daysRemaining === null;
  const tone = ongoing ? statusBadge.ongoing : statusBadge.upcoming;

  return (
    <span
      className="hhd-dashboard__status"
      style={{ background: tone.background, color: tone.text }}
    >
      {tone.dot ? <i style={{ background: tone.dot }} /> : null}
      {ongoing ? 'Đang diễn ra' : `Còn ${formatNumber(period.daysRemaining)} ngày`}
    </span>
  );
}

export function UpcomingPeriodCard({ period }: { period: UpcomingPeriod | null }) {
  const navigate = useNavigate();

  if (period === null) {
    return (
      <div className="hhd-dashboard__card hhd-dashboard__card--empty">
        <div>
          <div className="hhd-dashboard__overline">Đợt sắp tới</div>
          <div className="hhd-dashboard__period-name">Chưa cài đợt trao huy hiệu</div>
          <div className="hhd-dashboard__period-range">
            Hệ thống chưa biết gom người theo khoảng ngày nào.
          </div>
        </div>
        <Button onClick={() => navigate(paths.periods)}>Tạo đợt</Button>
      </div>
    );
  }

  return (
    <div className="hhd-dashboard__card">
      <div className="hhd-dashboard__card-main">
        <div className="hhd-dashboard__card-head">
          <span className="hhd-dashboard__overline">Đợt sắp tới</span>
          <CountdownTag period={period} />
          {/* Máy chủ báo đợt sắp tới rơi sang năm sau (QT8). */}
          {period.isNextYear ? <YearContextTag context="Next" /> : null}
        </div>

        <div className="hhd-dashboard__card-body">
          <div>
            <div className="hhd-dashboard__period-name">{period.name}</div>
            <div className="hhd-dashboard__period-range">
              {formatDate(period.fromDate)} – {formatDate(period.toDate)}
            </div>
          </div>
          <div className="hhd-dashboard__count">
            <div className="hhd-dashboard__count-value">{formatNumber(period.eligibleCount)}</div>
            <div className="hhd-dashboard__count-label">người đủ điều kiện</div>
          </div>
        </div>
      </div>

      <div className="hhd-dashboard__card-divider" />

      <div className="hhd-dashboard__breakdown">
        <div className="hhd-dashboard__overline">Phân bổ theo mốc</div>
        {period.milestoneBreakdown.length > 0 ? (
          <div className="hhd-dashboard__pills">
            {period.milestoneBreakdown.map((item) => (
              <MilestonePill key={item.milestone} item={item} />
            ))}
          </div>
        ) : (
          <div className="hhd-dashboard__period-range">
            Đợt này chưa có ai tròn mốc trong năm {period.year}.
          </div>
        )}
      </div>
    </div>
  );
}
