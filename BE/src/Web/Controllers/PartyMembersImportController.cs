using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports.Models;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports.Responses;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports.Services;
using Microsoft.AspNetCore.Mvc;

namespace HuyHieuDang.Web.Controllers;

/// <summary>
/// Nhập danh sách đảng viên từ Excel — ba bước của wizard (UC-24) và file mẫu (UC-25).
/// Tách khỏi <see cref="PartyMembersController"/> vì đường dẫn có thêm đoạn <c>Import</c>,
/// trong khi bộ khung gắn <c>api/[controller]</c> cho mọi controller.
/// </summary>
[Route("api/PartyMembers/Import")]
public class PartyMembersImportController : BaseController
{
    private readonly IPartyMemberImportService importService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartyMembersImportController"/> class.
    /// </summary>
    /// <param name="importService">Nghiệp vụ nhập Excel.</param>
    public PartyMembersImportController(IPartyMemberImportService importService)
    {
        this.importService = importService;
    }

    /// <summary>
    /// Tải file mẫu <c>MauDanhSachDangVien.xlsx</c>: bốn cột đúng thứ tự, hai dòng ví dụ (UC-25).
    /// </summary>
    /// <returns>File nhị phân, không bọc trong lớp vỏ phản hồi (mục 1.9 hợp đồng API).</returns>
    [HttpGet("Template")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public FileResult GetTemplate()
        => File(
            importService.BuildTemplate(),
            PartyMemberImportFile.ExcelContentType,
            PartyMemberImportFile.TemplateFileName);

    /// <summary>
    /// Xem trước file nhập: trả dòng hợp lệ và dòng lỗi kèm đủ lý do (UC-24 bước 2).
    /// Không ghi gì vào cơ sở dữ liệu.
    /// </summary>
    /// <param name="file">File <c>.xlsx</c> người dùng chọn.</param>
    /// <returns>Kết quả xem trước.</returns>
    [HttpPost("Preview")]
    [ProducesResponseType(typeof(SuccessResultWrapper<PartyMemberImportPreviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<SuccessResultWrapper<PartyMemberImportPreviewResponse>> Preview(IFormFile? file)
        => OkWrapper(
            importService.Preview(OpenOrThrow(file), file!.FileName, file.Length),
            Messages<PartyMemberImport>.Search());

    /// <summary>
    /// Nạp file nhập: thêm mới mọi dòng hợp lệ trong một giao dịch, bỏ qua dòng lỗi,
    /// không kiểm tra trùng (UC-24 bước 3, QT9).
    /// </summary>
    /// <param name="file">Đúng file vừa xem trước.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Số người đã thêm và số dòng lỗi bị bỏ qua.</returns>
    [HttpPost("Commit")]
    [ProducesResponseType(typeof(SuccessResultWrapper<PartyMemberImportCommitResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SuccessResultWrapper<PartyMemberImportCommitResponse>>> CommitAsync(
        IFormFile? file, CancellationToken cancellationToken)
        => OkWrapper(
            await importService.CommitAsync(OpenOrThrow(file), file!.FileName, file.Length, cancellationToken),
            Messages<PartyMember>.Import());

    /// <summary>
    /// Mở luồng đọc của file gửi lên; thiếu file cũng là lỗi cấp file, trả <c>400</c> chứ không
    /// để lọt xuống dưới thành <c>500</c>.
    /// </summary>
    /// <param name="file">File trong trường <c>file</c> của biểu mẫu.</param>
    /// <returns>Luồng nội dung file.</returns>
    private static Stream OpenOrThrow(IFormFile? file)
        => file is null || file.Length == 0
            ? throw new BadRequestException(Messages<PartyMemberImport>.Invalid(PartyMemberImport.EmptyProperty))
            : file.OpenReadStream();
}
