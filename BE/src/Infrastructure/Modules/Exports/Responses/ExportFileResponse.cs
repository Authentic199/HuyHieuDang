namespace HuyHieuDang.Infrastructure.Modules.Exports.Responses;

/// <summary>
/// Một file Excel đã dựng xong, kèm đúng hai thứ controller cần để trả về theo mục 1.9 hợp đồng
/// API: tên file và nội dung. Kiểu nội dung là hằng dùng chung nên không lặp lại ở đây.
/// </summary>
/// <param name="FileName">Tên file do Backend sinh, ví dụ <c>DuDieuKien_Dot7-11_2026.xlsx</c>.</param>
/// <param name="Content">Luồng nhị phân của file, đã ở đầu luồng.</param>
public sealed record ExportFileResponse(string FileName, Stream Content);
