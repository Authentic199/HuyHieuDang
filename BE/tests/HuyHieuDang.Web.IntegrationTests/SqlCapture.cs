using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Concurrent;
using System.Data.Common;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Ghi lại câu lệnh SQL mà EF Core thực sự gửi xuống PostgreSQL, để kiểm chứng điều kiện lọc và
/// mệnh đề sắp xếp nằm trong <c>WHERE</c> / <c>ORDER BY</c> chứ không phải lọc sau khi nạp (T51).
/// </summary>
public sealed class SqlCapture
{
    private readonly ConcurrentQueue<string> commands = new();

    /// <summary>
    /// Các câu lệnh đã ghi được, theo đúng thứ tự chạy.
    /// </summary>
    public IReadOnlyList<string> Commands => commands.ToList();

    /// <summary>
    /// Xóa sạch trước khi bắt đầu đo một lời gọi.
    /// </summary>
    public void Clear() => commands.Clear();

    /// <summary>
    /// Thêm một câu lệnh vào kho.
    /// </summary>
    /// <param name="command">Nội dung câu lệnh.</param>
    public void Add(string command) => commands.Enqueue(command);
}

/// <summary>
/// Bộ chặn cắm vào EF Core để nhặt câu lệnh cho <see cref="SqlCapture"/>.
/// </summary>
public sealed class SqlCaptureInterceptor : DbCommandInterceptor
{
    private readonly SqlCapture capture;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlCaptureInterceptor"/> class.
    /// </summary>
    /// <param name="capture">Kho chứa câu lệnh.</param>
    public SqlCaptureInterceptor(SqlCapture capture)
    {
        this.capture = capture;
    }

    /// <inheritdoc/>
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
    {
        capture.Add(command.CommandText);

        return base.ReaderExecuting(command, eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        capture.Add(command.CommandText);

        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc/>
    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command, CommandEventData eventData, InterceptionResult<object> result)
    {
        capture.Add(command.CommandText);

        return base.ScalarExecuting(command, eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        capture.Add(command.CommandText);

        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }
}
