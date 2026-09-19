/**
 * Bảng tra khóa thông điệp → chữ tiếng Việt (mục 1.5 của docs/api-contract.md).
 *
 * Backend chỉ trả khóa; toàn bộ câu chữ hiển thị nằm ở đây để giao diện luôn
 * thống nhất với bản thiết kế. Thêm khóa mới thì thêm vào đúng bảng này.
 */
const MESSAGE_TEXTS: Record<string, string> = {
  // Đăng nhập
  'Mes.User.Login.Successfully': 'Đăng nhập thành công',
  'Mes.User.Login.Failed': 'Sai tài khoản hoặc mật khẩu',
  'Mes.User.Logout.Successfully': 'Đã đăng xuất',
  'Mes.User.Required.Username': 'Chưa nhập Tài khoản',
  'Mes.User.Required.Password': 'Chưa nhập Mật khẩu',

  // Đảng viên
  'Mes.PartyMember.Create.Successfully': 'Đã thêm đảng viên',
  'Mes.PartyMember.Update.Successfully': 'Đã lưu thay đổi',
  'Mes.PartyMember.Delete.Successfully': 'Đã xóa',
  'Mes.PartyMember.Import.Successfully': 'Đã nạp danh sách',
  'Mes.PartyMember.NotFound': 'Không tìm thấy đảng viên',
  'Mes.PartyMember.Required.FullName': 'Chưa nhập Họ tên',
  'Mes.PartyMember.Required.OfficialAdmissionDate': 'Chưa nhập Ngày vào Đảng chính thức',
  'Mes.PartyMember.Invalid.OfficialAdmissionDate': 'Ngày chính thức không được ở tương lai',
  'Mes.PartyMember.Invalid.DateOfBirth': 'Ngày sinh phải trước Ngày vào Đảng chính thức',
  'Mes.PartyMember.Invalid.Gender': 'Giới tính chỉ nhận Nam hoặc Nữ',

  // Đợt trao huy hiệu
  'Mes.AwardPeriod.Create.Successfully': 'Đã thêm đợt trao huy hiệu',
  'Mes.AwardPeriod.Update.Successfully': 'Đã lưu thay đổi',
  'Mes.AwardPeriod.Delete.Successfully': 'Đã xóa đợt trao huy hiệu',
  'Mes.AwardPeriod.NotFound': 'Không tìm thấy đợt trao huy hiệu',
  'Mes.AwardPeriod.Required.Name': 'Chưa nhập Tên đợt',
  'Mes.AwardPeriod.Repeated.Name': 'Tên đợt đã tồn tại',
  'Mes.AwardPeriod.Invalid.FromDate': 'Từ ngày không hợp lệ',
  'Mes.AwardPeriod.Invalid.ToDate': 'Đến ngày không hợp lệ',
  'Mes.AwardPeriod.Invalid.Range': 'Đến ngày phải bằng hoặc sau Từ ngày trong cùng một năm',

  // Cài đặt
  'Mes.AppSetting.Update.Successfully': 'Đã lưu cài đặt',
  'Mes.AppSetting.Invalid.StartYears': 'Mốc bắt đầu phải là số nguyên dương',
  'Mes.AppSetting.Invalid.EndYears': 'Mốc kết thúc phải là số nguyên dương',
  'Mes.AppSetting.Invalid.StepYears': 'Bước nhảy phải từ 1 trở lên',
  'Mes.AppSetting.Invalid.Range': 'Mốc bắt đầu phải nhỏ hơn hoặc bằng mốc kết thúc',

  // Import Excel
  'Mes.Import.Invalid.Extension': 'Chỉ nhận file .xlsx',
  'Mes.Import.Invalid.FileSize': 'File vượt quá 10 MB',
  'Mes.Import.Invalid.Columns':
    'File phải có đúng 4 cột theo thứ tự Họ tên · Ngày sinh · Giới tính · Ngày vào Đảng chính thức',
  'Mes.Import.Invalid.Empty': 'File không có dòng dữ liệu nào',

  // Khác
  'Mes.Dashboard.NotFound.UpcomingPeriod': 'Chưa cài đợt trao huy hiệu',
  'Mes.Query.Invalid.Year': 'Năm không hợp lệ',
};

/** Câu mặc định cho khóa lạ — không để lọt khóa kỹ thuật ra màn hình. */
export const FALLBACK_MESSAGE = 'Thao tác không thực hiện được';

/** Câu cho các trường hợp không có khóa (mất mạng, máy chủ lỗi). */
export const NETWORK_MESSAGE =
  'Không kết nối được tới máy chủ. Kiểm tra lại đường truyền rồi thử lại.';
export const SERVER_MESSAGE = 'Hệ thống gặp sự cố, vui lòng thử lại';

/** Tra chữ hiển thị của một khóa. */
export function messageText(key: string | null | undefined, fallback = FALLBACK_MESSAGE): string {
  if (!key) return fallback;
  return MESSAGE_TEXTS[key] ?? fallback;
}

/** Các khóa `*.Search.Successfully` / `*.Detail.Successfully` không cần hiện gì. */
export function isSilentMessage(key: string | null | undefined): boolean {
  if (!key) return true;
  return key.endsWith('.Search.Successfully') || key.endsWith('.Detail.Successfully');
}
