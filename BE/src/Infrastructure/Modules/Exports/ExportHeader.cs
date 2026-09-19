using System.Globalization;

namespace HuyHieuDang.Infrastructure.Modules.Exports;

/// <summary>
/// Ba dòng chữ mở đầu mỗi file xuất ra (mục 8 hợp đồng API). Dựng sẵn thành chuỗi ở đây để
/// <see cref="ExportSheet"/> chỉ còn việc xếp ô, và để kiểm thử so từng dòng mà không phải mở file.
/// </summary>
/// <param name="UnitName">Tên đơn vị; rỗng thì dòng 1 bị bỏ hẳn, không để lại dòng trắng (A-705).</param>
/// <param name="PeriodLine">Dòng tên đợt kèm khoảng ngày đã gắn năm.</param>
/// <param name="ExportedOnLine">Dòng ngày xuất.</param>
public sealed record ExportHeader(string? UnitName, string PeriodLine, string ExportedOnLine)
{
    /// <summary>
    /// Nhãn đứng trước ngày xuất.
    /// </summary>
    public const string ExportedOnLabel = "Ngày xuất";

    /// <summary>
    /// Chữ của dòng 2 trong file "chưa thuộc đợt nào".
    /// </summary>
    public const string UnassignedTitle = "Chưa thuộc đợt nào";

    /// <summary>
    /// Dấu ngăn giữa tên đợt và khoảng ngày.
    /// </summary>
    private const string Separator = " · ";

    /// <summary>
    /// Định dạng ngày của phần tiêu đề.
    /// </summary>
    private const string DateFormat = "dd/MM/yyyy";

    /// <summary>
    /// Tiêu đề file đủ điều kiện: <c>Đợt 7/11 · 01/10/2026 – 07/11/2026</c>.
    /// </summary>
    /// <param name="unitName">Tên đơn vị trong cài đặt.</param>
    /// <param name="periodName">Tên đợt.</param>
    /// <param name="fromDate">Từ ngày đã gắn năm.</param>
    /// <param name="toDate">Đến ngày đã gắn năm.</param>
    /// <param name="exportedOn">Ngày xuất file.</param>
    /// <returns>Ba dòng chữ đầu file.</returns>
    public static ExportHeader ForEligibility(
        string? unitName, string periodName, DateOnly fromDate, DateOnly toDate, DateOnly exportedOn)
        => new(
            unitName,
            $"{periodName}{Separator}{Format(fromDate)} – {Format(toDate)}",
            ExportedOn(exportedOn));

    /// <summary>
    /// Tiêu đề file "chưa thuộc đợt nào": <c>Chưa thuộc đợt nào · năm 2026</c>.
    /// </summary>
    /// <param name="unitName">Tên đơn vị trong cài đặt.</param>
    /// <param name="year">Năm đang xét.</param>
    /// <param name="exportedOn">Ngày xuất file.</param>
    /// <returns>Ba dòng chữ đầu file.</returns>
    public static ExportHeader ForUnassigned(string? unitName, int year, DateOnly exportedOn)
        => new(
            unitName,
            string.Create(CultureInfo.InvariantCulture, $"{UnassignedTitle}{Separator}năm {year}"),
            ExportedOn(exportedOn));

    /// <summary>
    /// Dòng ngày xuất, luôn dạng <c>dd/MM/yyyy</c>.
    /// </summary>
    /// <param name="exportedOn">Ngày xuất file.</param>
    /// <returns>Chữ của dòng 3.</returns>
    private static string ExportedOn(DateOnly exportedOn) => $"{ExportedOnLabel}: {Format(exportedOn)}";

    /// <summary>
    /// Một ngày dạng <c>dd/MM/yyyy</c>.
    /// </summary>
    /// <param name="date">Ngày cần hiển thị.</param>
    /// <returns>Chuỗi ngày.</returns>
    private static string Format(DateOnly date) => date.ToString(DateFormat, CultureInfo.InvariantCulture);
}
