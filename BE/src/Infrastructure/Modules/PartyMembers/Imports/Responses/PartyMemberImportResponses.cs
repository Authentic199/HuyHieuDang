using HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports.Models;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports.Responses;

/// <summary>
/// Kết quả bước xem trước (mục 4.2 hợp đồng API). Lời gọi này không ghi gì vào cơ sở dữ liệu.
/// </summary>
/// <param name="FileName">Tên file người dùng vừa gửi lên.</param>
/// <param name="TotalRows">Tổng số dòng dữ liệu đọc được, không tính dòng tiêu đề.</param>
/// <param name="ValidCount">Số dòng hợp lệ.</param>
/// <param name="ErrorCount">Số dòng bị loại.</param>
/// <param name="ValidRows">Các dòng hợp lệ đã chuẩn hóa.</param>
/// <param name="ErrorRows">Các dòng bị loại, giữ nguyên chữ thô kèm đủ lý do.</param>
public sealed record PartyMemberImportPreviewResponse(
    string FileName,
    int TotalRows,
    int ValidCount,
    int ErrorCount,
    IReadOnlyList<ImportValidRowResponse> ValidRows,
    IReadOnlyList<ImportErrorRowResponse> ErrorRows);

/// <summary>
/// Kết quả bước nạp (mục 4.3 hợp đồng API).
/// </summary>
/// <param name="ImportedCount">Số người đã thêm.</param>
/// <param name="SkippedCount">Số dòng lỗi bị bỏ qua.</param>
public sealed record PartyMemberImportCommitResponse(int ImportedCount, int SkippedCount);
