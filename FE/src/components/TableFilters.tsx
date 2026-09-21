import { SearchOutlined } from '@ant-design/icons';
import { Input, Select } from 'antd';

import {
  ALL_MILESTONES,
  type MilestoneFilter,
  type MilestoneOption,
} from '../hooks/useClientTable';
import './TableFilters.css';

/**
 * Ô tìm và ô lọc mốc dùng chung cho các bảng làm việc ở máy khách.
 *
 * Hình dáng lấy đúng thanh công cụ của màn Đảng viên (`Input size="large"` có
 * kính lúp và dấu xóa, `Select size="large"` có nhãn dính trước giá trị) để bốn
 * màn nhìn như một hệ, bác cán bộ không phải học lại từng màn.
 */

interface TableSearchInputProps {
  value: string;
  onChange: (value: string) => void;
  /** Ví dụ "Tìm theo họ tên…" */
  placeholder: string;
  /** Nhãn cho trình đọc màn hình, ví dụ "Tìm theo họ tên" */
  ariaLabel: string;
  disabled?: boolean;
}

export function TableSearchInput({
  value,
  onChange,
  placeholder,
  ariaLabel,
  disabled,
}: TableSearchInputProps) {
  return (
    <Input
      className="hhd-table-filters__search"
      size="large"
      allowClear
      prefix={<SearchOutlined />}
      placeholder={placeholder}
      aria-label={ariaLabel}
      value={value}
      disabled={disabled}
      onChange={(event) => onChange(event.target.value)}
    />
  );
}

interface MilestoneFilterSelectProps {
  value: MilestoneFilter;
  onChange: (value: MilestoneFilter) => void;
  /** Chỉ những mốc có thật trong dữ liệu đang xem, do useClientTable dựng */
  options: MilestoneOption[];
  disabled?: boolean;
}

/** Ô lọc mốc tuổi đảng — chỉ bày những mốc thật sự có người. */
export function MilestoneFilterSelect({
  value,
  onChange,
  options,
  disabled,
}: MilestoneFilterSelectProps) {
  return (
    <Select<MilestoneFilter>
      className="hhd-table-filters__milestone"
      size="large"
      value={value}
      options={options}
      disabled={disabled}
      aria-label="Lọc theo mốc huy hiệu"
      onChange={onChange}
      // Trong ô đóng viết gọn "Mốc: Tất cả"; mở ra thì dòng đầu là "Tất cả mốc".
      labelRender={({ label, value: selected }) => (
        <>Mốc: {selected === ALL_MILESTONES ? 'Tất cả' : label}</>
      )}
    />
  );
}
