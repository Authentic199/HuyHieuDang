-- Sinh tự động bởi tests/fixtures/generate.py - KHÔNG sửa tay.
-- Tên bảng/cột lấy từ biến DB trong generate.py; nếu migration đổi tên,
-- sửa ở đó rồi chạy lại script.

TRUNCATE TABLE party_members, award_periods RESTART IDENTITY CASCADE;
DELETE FROM app_settings;
