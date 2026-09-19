using System.Collections.ObjectModel;
using System.Globalization;
using HuyHieuDang.Infrastructure.Modules.Eligibility.Responses;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Enums;
using MiniExcelLibs;

namespace HuyHieuDang.Infrastructure.Modules.Exports;

/// <summary>
/// Dựng nội dung một sheet Excel xuất ra đúng bố cục mục 8 hợp đồng API: ba dòng tiêu đề, một
/// dòng trống, một dòng đầu cột rồi dữ liệu. Lớp thuần — không chạm cơ sở dữ liệu, không đọc
/// đồng hồ; mọi thứ nhận qua tham số nên kiểm thử dựng lại được từng ô.
/// </summary>
public static class ExportSheet
{
    /// <summary>
    /// Tên sheet duy nhất của mọi file xuất ra.
    /// </summary>
    public const string SheetName = "DanhSach";

    /// <summary>
    /// Tên cột cuối, chỉ có ở file "chưa thuộc đợt nào".
    /// </summary>
    public const string GapColumn = "Khoảng trống";

    /// <summary>
    /// Bảy cột của file đủ điều kiện, đúng thứ tự bảng đang xem.
    /// </summary>
    public static readonly ReadOnlyCollection<string> EligibilityColumns = new(new[]
    {
        "STT",
        "Họ tên",
        "Giới tính",
        "Ngày sinh",
        "Ngày chính thức",
        "Ngày tròn mốc",
        "Mốc huy hiệu",
    });

    /// <summary>
    /// Tám cột của file "chưa thuộc đợt nào".
    /// </summary>
    public static readonly ReadOnlyCollection<string> UnassignedColumns
        = new(EligibilityColumns.Append(GapColumn).ToArray());

    /// <summary>
    /// Định dạng ngày của mọi ô ngày trong file (mục 8 hợp đồng API).
    /// </summary>
    private const string DateFormat = "dd/MM/yyyy";

    /// <summary>
    /// Dựng file đủ điều kiện: bảy cột, không có cột Khoảng trống.
    /// </summary>
    /// <param name="header">Ba dòng chữ đầu file.</param>
    /// <param name="members">Danh sách đã sắp sẵn theo mục 1.8; giữ nguyên thứ tự nhận được.</param>
    /// <returns>Luồng nhị phân của file, đã ở đầu luồng.</returns>
    public static Stream BuildEligibility(ExportHeader header, IReadOnlyList<EligibleMemberResponse> members)
    {
        ArgumentNullException.ThrowIfNull(members);

        return Build(header, EligibilityColumns, members.Select(Cells));
    }

    /// <summary>
    /// Dựng file "chưa thuộc đợt nào": bảy cột như trên, thêm cột Khoảng trống ở cuối.
    /// </summary>
    /// <param name="header">Ba dòng chữ đầu file.</param>
    /// <param name="members">Danh sách đã sắp sẵn theo mục 1.8; giữ nguyên thứ tự nhận được.</param>
    /// <returns>Luồng nhị phân của file, đã ở đầu luồng.</returns>
    public static Stream BuildUnassigned(ExportHeader header, IReadOnlyList<UnassignedMemberResponse> members)
    {
        ArgumentNullException.ThrowIfNull(members);

        return Build(
            header,
            UnassignedColumns,
            members.Select((member, index) => Cells(member, index).Append(GapText(member.Gap))));
    }

    /// <summary>
    /// Chữ của cột "Khoảng trống", đúng ba khuôn của mục 1.10 hợp đồng API.
    /// </summary>
    /// <param name="gap">Khoảng trống chứa ngày tròn mốc.</param>
    /// <returns>Chữ hiển thị trong ô.</returns>
    public static string GapText(UnassignedGapResponse gap)
    {
        ArgumentNullException.ThrowIfNull(gap);

        return gap.Type switch
        {
            UnassignedGapType.BeforeFirst => "Trước đợt đầu tiên",
            UnassignedGapType.AfterLast => "Sau đợt cuối cùng",
            _ => $"Giữa {gap.PreviousPeriodName} và {gap.NextPeriodName}",
        };
    }

    /// <summary>
    /// Ô Giới tính: <c>Nam</c> / <c>Nữ</c>. Để trống cho ra ô rỗng, không phải dấu gạch ngang.
    /// </summary>
    /// <param name="gender">Giới tính trong dữ liệu.</param>
    /// <returns>Chữ của ô, hoặc <see langword="null"/> khi để trống.</returns>
    public static string? GenderText(Gender? gender) => gender switch
    {
        Gender.Male => "Nam",
        Gender.Female => "Nữ",
        _ => null,
    };

    /// <summary>
    /// Ô ngày dạng <c>dd/MM/yyyy</c>; ngày để trống cho ra ô rỗng.
    /// </summary>
    /// <param name="date">Ngày cần ghi.</param>
    /// <returns>Chữ của ô, hoặc <see langword="null"/> khi để trống.</returns>
    public static string? DateText(DateOnly? date)
        => date?.ToString(DateFormat, CultureInfo.InvariantCulture);

    /// <summary>
    /// Ghép toàn bộ sheet rồi giao cho MiniExcel ghi ra một sheet duy nhất.
    /// </summary>
    /// <param name="header">Ba dòng chữ đầu file.</param>
    /// <param name="columns">Tên các cột, đúng thứ tự.</param>
    /// <param name="dataRows">Các dòng dữ liệu, mỗi dòng đủ số ô như <paramref name="columns"/>.</param>
    /// <returns>Luồng nhị phân của file, đã ở đầu luồng.</returns>
    private static Stream Build(
        ExportHeader header, IReadOnlyList<string> columns, IEnumerable<IEnumerable<object?>> dataRows)
    {
        ArgumentNullException.ThrowIfNull(header);

        List<Dictionary<string, object?>> rows = new();

        // Tên đơn vị trống thì bỏ hẳn dòng, không để lại dòng trắng (A-705).
        if (!string.IsNullOrWhiteSpace(header.UnitName))
        {
            rows.Add(TextRow(columns.Count, header.UnitName));
        }

        rows.Add(TextRow(columns.Count, header.PeriodLine));
        rows.Add(TextRow(columns.Count, header.ExportedOnLine));
        rows.Add(TextRow(columns.Count, null));
        rows.Add(Row(columns.Count, columns));
        rows.AddRange(dataRows.Select(cells => Row(columns.Count, cells)));

        MemoryStream stream = new();
        stream.SaveAs(rows, printHeader: false, sheetName: SheetName);
        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }

    /// <summary>
    /// Bảy ô chung của một người, dùng lại cho cả hai loại file.
    /// </summary>
    /// <param name="member">Một dòng của danh sách.</param>
    /// <param name="index">Vị trí trong danh sách, bắt đầu từ 0.</param>
    /// <returns>Bảy ô đúng thứ tự cột.</returns>
    private static IEnumerable<object?> Cells(EligibleMemberResponse member, int index)
    {
        yield return index + 1;
        yield return member.FullName;
        yield return GenderText(member.Gender);
        yield return DateText(member.DateOfBirth);
        yield return DateText(member.OfficialAdmissionDate);
        yield return DateText(member.MilestoneDate);
        yield return member.Milestone;
    }

    /// <summary>
    /// Một dòng chỉ có chữ ở ô đầu, các ô sau để rỗng cho đủ bề ngang sheet.
    /// </summary>
    /// <param name="width">Số cột của sheet.</param>
    /// <param name="text">Chữ của ô đầu; <see langword="null"/> cho dòng trống.</param>
    /// <returns>Một dòng đã đủ số ô.</returns>
    private static Dictionary<string, object?> TextRow(int width, string? text)
        => Row(width, Enumerable.Range(0, width).Select(index => index == 0 ? text : null));

    /// <summary>
    /// Đổi một dãy ô thành dòng của MiniExcel, khóa từ điển là số thứ tự cột. Mọi dòng dùng chung
    /// bộ khóa này để các ô thẳng cột với nhau.
    /// </summary>
    /// <param name="width">Số cột của sheet.</param>
    /// <param name="cells">Giá trị từng ô.</param>
    /// <returns>Một dòng đã đủ số ô.</returns>
    private static Dictionary<string, object?> Row(int width, IEnumerable<object?> cells)
    {
        Dictionary<string, object?> row = new(width);
        int index = 0;

        foreach (object? cell in cells)
        {
            row[ColumnKey(index)] = cell;
            index++;
        }

        for (; index < width; index++)
        {
            row[ColumnKey(index)] = null;
        }

        return row;
    }

    /// <summary>
    /// Khóa cột của MiniExcel; hai chữ số để thứ tự khóa trùng thứ tự cột.
    /// </summary>
    /// <param name="index">Vị trí cột, bắt đầu từ 0.</param>
    /// <returns>Khóa cột.</returns>
    private static string ColumnKey(int index) => index.ToString("D2", CultureInfo.InvariantCulture);
}
