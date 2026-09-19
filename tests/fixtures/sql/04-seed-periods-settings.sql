-- Sinh tự động bởi tests/fixtures/generate.py - KHÔNG sửa tay.
-- Tên bảng/cột lấy từ biến DB trong generate.py; nếu migration đổi tên,
-- sửa ở đó rồi chạy lại script.

INSERT INTO award_periods (id, name, from_day, from_month, to_day, to_month, created_at) VALUES
  ('d0000000-0000-4000-8000-000000000001', 'Đợt 3/2', 15, 1, 5, 3, '2026-09-19T00:00:00+07:00'),
  ('d0000000-0000-4000-8000-000000000002', 'Đợt 19/5', 1, 5, 31, 5, '2026-09-19T00:00:00+07:00'),
  ('d0000000-0000-4000-8000-000000000003', 'Đợt 2/9', 15, 8, 10, 9, '2026-09-19T00:00:00+07:00'),
  ('d0000000-0000-4000-8000-000000000004', 'Đợt 7/11', 1, 10, 7, 11, '2026-09-19T00:00:00+07:00');

INSERT INTO app_settings (id, start_years, end_years, step_years, unit_name, created_at) VALUES
  ('50000000-0000-4000-8000-000000000001', 30, 90, 5, 'Đảng ủy Phường Kiểm Thử', '2026-09-19T00:00:00+07:00');
