using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports.Models;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports.Responses;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports.Services;

/// <summary>
/// Nhập danh sách đảng viên từ Excel: tải file mẫu, xem trước và nạp (UC-24, UC-25, QT9).
/// </summary>
public interface IPartyMemberImportService : IScopedService
{
    /// <summary>Sinh file mẫu bốn cột kèm hai dòng ví dụ (UC-25).</summary>
    /// <returns>Luồng nhị phân của file mẫu.</returns>
    Stream BuildTemplate();

    /// <summary>Xem trước: tách dòng hợp lệ và dòng lỗi, không ghi gì vào cơ sở dữ liệu (UC-24 bước 2).</summary>
    /// <param name="stream">Luồng nội dung file.</param>
    /// <param name="fileName">Tên file người dùng gửi lên.</param>
    /// <param name="length">Dung lượng file, tính bằng byte.</param>
    /// <returns>Danh sách dòng hợp lệ và dòng lỗi kèm lý do.</returns>
    PartyMemberImportPreviewResponse Preview(Stream stream, string? fileName, long length);

    /// <summary>Nạp: thêm mới mọi dòng hợp lệ trong một giao dịch, không kiểm tra trùng (UC-24 bước 3, QT9).</summary>
    /// <param name="stream">Luồng nội dung đúng file vừa xem trước.</param>
    /// <param name="fileName">Tên file người dùng gửi lên.</param>
    /// <param name="length">Dung lượng file, tính bằng byte.</param>
    /// <param name="cancellationToken">Thẻ hủy.</param>
    /// <returns>Số người đã thêm và số dòng lỗi bị bỏ qua.</returns>
    Task<PartyMemberImportCommitResponse> CommitAsync(
        Stream stream, string? fileName, long length, CancellationToken cancellationToken = default);
}

/// <inheritdoc cref="IPartyMemberImportService"/>
public class PartyMemberImportService : IPartyMemberImportService
{
    /// <summary>
    /// Số bản ghi mỗi lần ghi xuống cơ sở dữ liệu. Cả loạt nằm trong cùng một giao dịch nên
    /// chia lô chỉ để không dồn 1200 bản ghi vào một câu lệnh.
    /// </summary>
    public const int BatchSize = 200;

    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IDateTimeProvider dateTimeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartyMemberImportService"/> class.
    /// </summary>
    /// <param name="repositoryWrapper">Cổng truy cập dữ liệu.</param>
    /// <param name="dateTimeProvider">Nguồn thời gian của hệ thống.</param>
    public PartyMemberImportService(IRepositoryWrapper repositoryWrapper, IDateTimeProvider dateTimeProvider)
    {
        this.repositoryWrapper = repositoryWrapper;
        this.dateTimeProvider = dateTimeProvider;
    }

    /// <inheritdoc/>
    public Stream BuildTemplate() => PartyMemberImportFile.BuildTemplate();

    /// <inheritdoc/>
    public PartyMemberImportPreviewResponse Preview(Stream stream, string? fileName, long length)
    {
        PartyMemberImportFile.EnsureFileAllowed(fileName, length);

        IReadOnlyList<ImportRawRow> rows = PartyMemberImportFile.ReadRows(stream);
        DateOnly today = dateTimeProvider.Today;

        List<ImportValidRowResponse> validRows = new();
        List<ImportErrorRowResponse> errorRows = new();

        foreach (ImportRawRow row in rows)
        {
            IReadOnlyList<ImportRowErrorResponse> errors =
                PartyMemberImportRowValidator.Validate(row, today, out ImportValidRowResponse? validRow);

            if (validRow is not null)
            {
                validRows.Add(validRow);
                continue;
            }

            errorRows.Add(new ImportErrorRowResponse(
                row.RowNumber, row.FullName, row.DateOfBirth, row.Gender, row.OfficialAdmissionDate, errors));
        }

        return new PartyMemberImportPreviewResponse(
            fileName ?? string.Empty, rows.Count, validRows.Count, errorRows.Count, validRows, errorRows);
    }

    /// <inheritdoc/>
    public async Task<PartyMemberImportCommitResponse> CommitAsync(
        Stream stream, string? fileName, long length, CancellationToken cancellationToken = default)
    {
        // Đọc và kiểm tra lại từ đầu: máy chủ không giữ trạng thái giữa xem trước và nạp.
        PartyMemberImportPreviewResponse preview = Preview(stream, fileName, length);

        if (preview.ValidCount == 0)
        {
            return new PartyMemberImportCommitResponse(0, preview.ErrorCount);
        }

        DateTimeOffset now = dateTimeProvider.Now;
        List<PartyMember> entities = preview.ValidRows
            .Select(row => new PartyMember
            {
                FullName = row.FullName,
                DateOfBirth = row.DateOfBirth,
                Gender = row.ToGender(),
                OfficialAdmissionDate = row.OfficialAdmissionDate,
                UpdatedAt = now,
            })
            .ToList();

        await repositoryWrapper.BeginTransactionAsync(cancellationToken);

        try
        {
            for (int offset = 0; offset < entities.Count; offset += BatchSize)
            {
                await repositoryWrapper.Repository<PartyMember>()
                    .AddRangeAsync(entities.Skip(offset).Take(BatchSize), cancellationToken);
            }

            await repositoryWrapper.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            // Lỗi kỹ thuật giữa chừng: trả cơ sở dữ liệu về đúng trạng thái trước khi nạp.
            await repositoryWrapper.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }

        return new PartyMemberImportCommitResponse(preview.ValidCount, preview.ErrorCount);
    }
}
