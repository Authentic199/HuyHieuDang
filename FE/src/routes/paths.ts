/** Đường dẫn của 5 mục menu, theo sơ đồ điều hướng mục 6.1. */
export const paths = {
  login: '/dang-nhap',
  dashboard: '/',
  members: '/dang-vien',
  membersImport: '/dang-vien/import',
  periods: '/dot-trao-huy-hieu',
  periodDetail: '/dot-trao-huy-hieu/:id',
  uncovered: '/chua-thuoc-dot-nao',
  settings: '/cai-dat',
} as const;

export function periodDetailPath(id: string): string {
  return `/dot-trao-huy-hieu/${id}`;
}
