using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Modules.Eligibility.Requests;
using HuyHieuDang.Infrastructure.Modules.Exports;
using HuyHieuDang.Infrastructure.Modules.Exports.Responses;
using HuyHieuDang.Infrastructure.Modules.Exports.Services;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports;
using Microsoft.AspNetCore.Mvc;

namespace HuyHieuDang.Web.Controllers;

/// <summary>
/// Xuất Excel ba loại (mục 8 hợp đồng API, UC-11, UC-34, UC-40). Cả ba trả file nhị phân, không
/// bọc trong <c>SuccessResultWrapper</c>; khi lỗi thì middleware trả JSON theo mục 1.4 như mọi
/// endpoint khác (mục 1.9).
/// </summary>
public class ExportsController : BaseController
{
    private readonly IExportService exportService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportsController"/> class.
    /// </summary>
    /// <param name="exportService">Nghiệp vụ xuất Excel.</param>
    public ExportsController(IExportService exportService)
    {
        this.exportService = exportService;
    }

    /// <summary>
    /// Xuất danh sách đủ điều kiện của một đợt trong một năm (mục 8.1, UC-34).
    /// </summary>
    /// <param name="request">Id đợt và năm đang xét; bỏ trống năm thì lấy năm hiện tại.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>File <c>DuDieuKien_&lt;TênĐợtRútGọn&gt;_&lt;Năm&gt;.xlsx</c>.</returns>
    [HttpGet("Eligibility")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<FileResult> ExportEligibilityAsync(
        [FromQuery] EligibilityQueryRequest request, CancellationToken cancellationToken)
        => ToFile(await exportService.ExportEligibilityAsync(request, cancellationToken));

    /// <summary>
    /// Xuất đúng danh sách đang hiện trên Dashboard, tức đợt sắp tới theo QT8 (mục 8.2, UC-11).
    /// </summary>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>File của đợt sắp tới, năm có thể là năm sau.</returns>
    [HttpGet("Dashboard")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<FileResult> ExportDashboardAsync(CancellationToken cancellationToken)
        => ToFile(await exportService.ExportDashboardAsync(cancellationToken));

    /// <summary>
    /// Xuất danh sách chưa thuộc đợt nào của một năm (mục 8.3, UC-40). Không có ai bị sót thì vẫn
    /// trả file hợp lệ chỉ có phần tiêu đề.
    /// </summary>
    /// <param name="request">Năm đang xét; bỏ trống thì lấy năm hiện tại.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>File <c>ChuaThuocDot_&lt;Năm&gt;.xlsx</c>.</returns>
    [HttpGet("Unassigned")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResultWrapper), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<FileResult> ExportUnassignedAsync(
        [FromQuery] EligibilityYearQueryRequest request, CancellationToken cancellationToken)
        => ToFile(await exportService.ExportUnassignedAsync(request, cancellationToken));

    /// <summary>
    /// Trả file nhị phân kèm tên file do Backend sinh. Header <c>Content-Disposition</c> tự ghi
    /// để có đúng cả hai dạng <c>filename=</c> và <c>filename*=UTF-8''</c> của mục 1.9; vì vậy
    /// không truyền tên file cho <c>File(...)</c> nữa, tránh MVC ghi đè bằng bản không ngoặc kép.
    /// </summary>
    /// <param name="file">File đã dựng xong.</param>
    /// <returns>Kết quả file của MVC.</returns>
    private FileResult ToFile(ExportFileResponse file)
    {
        Response.Headers.ContentDisposition = ExportFileName.ContentDisposition(file.FileName);

        return File(file.Content, PartyMemberImportFile.ExcelContentType);
    }
}
