/**
 * Kết quả xem trước import giả — dựng từ `tests/fixtures/data/import-rows.json`
 * của QC, đã ánh xạ sang mã lỗi của hợp đồng API (mục 4.2, OQ-2).
 */
import type { ImportPreview } from '../api/imports';

export const MOCK_IMPORT_PREVIEW: ImportPreview = {
  fileName: 'DanhSachDangVien_MauKiemThu.xlsx',
  totalRows: 14,
  validCount: 6,
  errorCount: 8,
  validRows: [
    {
      rowNumber: 2,
      fullName: 'Nguyễn Hợp Lệ Một',
      dateOfBirth: '1974-03-12',
      gender: 'Male',
      officialAdmissionDate: '1996-10-01',
    },
    {
      rowNumber: 3,
      fullName: 'Trần Hợp Lệ Hai',
      dateOfBirth: '1973-07-05',
      gender: 'Female',
      officialAdmissionDate: '1996-11-07',
    },
    {
      rowNumber: 4,
      fullName: 'Lê Hợp Lệ Ba',
      dateOfBirth: null,
      gender: null,
      officialAdmissionDate: '1996-01-15',
    },
    {
      rowNumber: 5,
      fullName: 'Phạm Hợp Lệ Bốn',
      dateOfBirth: '1975-02-09',
      gender: 'Female',
      officialAdmissionDate: '1996-05-31',
    },
    {
      rowNumber: 6,
      fullName: 'Hoàng Hợp Lệ Năm',
      dateOfBirth: '1971-06-30',
      gender: 'Male',
      officialAdmissionDate: '1996-08-15',
    },
    {
      rowNumber: 7,
      fullName: 'Vũ Hợp Lệ Sáu',
      dateOfBirth: '1972-09-14',
      gender: 'Female',
      officialAdmissionDate: '1996-09-10',
    },
  ],
  errorRows: [
    {
      rowNumber: 8,
      fullName: '',
      dateOfBirth: '12/03/1974',
      gender: 'Nam',
      officialAdmissionDate: '01/10/1996',
      errors: [
        {
          errorCode: 'MissingFullName',
          field: 'FullName',
        },
      ],
    },
    {
      rowNumber: 9,
      fullName: 'Nguyễn Thiếu Ngày',
      dateOfBirth: '12/03/1974',
      gender: 'Nam',
      officialAdmissionDate: '',
      errors: [
        {
          errorCode: 'MissingOfficialAdmissionDate',
          field: 'OfficialAdmissionDate',
        },
      ],
    },
    {
      rowNumber: 10,
      fullName: 'Trần Sai Định Dạng',
      dateOfBirth: '12/03/1974',
      gender: 'Nữ',
      officialAdmissionDate: '1996-10-01',
      errors: [
        {
          errorCode: 'InvalidDateFormat',
          field: 'OfficialAdmissionDate',
        },
      ],
    },
    {
      rowNumber: 11,
      fullName: 'Lê Ngày Tương Lai',
      dateOfBirth: '12/03/1974',
      gender: 'Nam',
      officialAdmissionDate: '20/09/2026',
      errors: [
        {
          errorCode: 'FutureOfficialAdmissionDate',
          field: 'OfficialAdmissionDate',
        },
      ],
    },
    {
      rowNumber: 12,
      fullName: 'Phạm Giới Tính Lạ',
      dateOfBirth: '12/03/1974',
      gender: 'Khác',
      officialAdmissionDate: '01/10/1996',
      errors: [
        {
          errorCode: 'InvalidGender',
          field: 'Gender',
        },
      ],
    },
    {
      rowNumber: 13,
      fullName: 'Hoàng Sinh Sau',
      dateOfBirth: '02/01/1997',
      gender: 'Nam',
      officialAdmissionDate: '01/10/1996',
      errors: [
        {
          errorCode: 'BirthDateAfterAdmissionDate',
          field: 'DateOfBirth',
        },
      ],
    },
    {
      rowNumber: 14,
      fullName: 'Vũ Ngày Sinh Sai',
      dateOfBirth: '31/02/1974',
      gender: 'Nữ',
      officialAdmissionDate: '01/10/1996',
      errors: [
        {
          errorCode: 'InvalidDateFormat',
          field: 'DateOfBirth',
        },
      ],
    },
    {
      rowNumber: 15,
      fullName: '',
      dateOfBirth: '31/02/1974',
      gender: 'Khác',
      officialAdmissionDate: '',
      errors: [
        {
          errorCode: 'MissingFullName',
          field: 'FullName',
        },
        {
          errorCode: 'MissingOfficialAdmissionDate',
          field: 'OfficialAdmissionDate',
        },
        {
          errorCode: 'InvalidDateFormat',
          field: 'DateOfBirth',
        },
        {
          errorCode: 'InvalidGender',
          field: 'Gender',
        },
      ],
    },
  ],
};
