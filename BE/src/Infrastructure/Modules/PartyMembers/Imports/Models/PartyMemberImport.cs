using HuyHieuDang.Infrastructure.Facades.Common.Attributes;

namespace HuyHieuDang.Infrastructure.Modules.PartyMembers.Imports.Models;

/// <summary>
/// Chỉ dùng để sinh nhóm khóa thông điệp <c>Mes.Import.*</c> cho những lỗi cấp file của
/// nhập Excel (mục 1.5 và 4.2 hợp đồng API). Không phải thực thể, không có bảng.
/// </summary>
[MessageDisplay("Import")]
public sealed class PartyMemberImport
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PartyMemberImport"/> class.
    /// Không ai dựng thể hiện: lớp chỉ tồn tại để làm tham số kiểu cho <c>Messages&lt;T&gt;</c>.
    /// </summary>
    private PartyMemberImport()
    {
    }

    /// <summary>
    /// Tên thuộc tính sinh khóa <c>Mes.Import.Invalid.Extension</c> — không phải tệp <c>.xlsx</c>.
    /// </summary>
    public const string ExtensionProperty = "Extension";

    /// <summary>
    /// Tên thuộc tính sinh khóa <c>Mes.Import.Invalid.FileSize</c> — tệp vượt quá 10 MB.
    /// </summary>
    public const string FileSizeProperty = "FileSize";

    /// <summary>
    /// Tên thuộc tính sinh khóa <c>Mes.Import.Invalid.Columns</c> — sai số cột hoặc sai thứ tự cột.
    /// </summary>
    public const string ColumnsProperty = "Columns";

    /// <summary>
    /// Tên thuộc tính sinh khóa <c>Mes.Import.Invalid.Empty</c> — không đọc được ô nào.
    /// </summary>
    public const string EmptyProperty = "Empty";

    /// <summary>
    /// Tên thuộc tính sinh khóa <c>Mes.Import.Invalid.NoDataRows</c> — có tiêu đề nhưng không có dòng dữ liệu (OQ-7).
    /// </summary>
    public const string NoDataRowsProperty = "NoDataRows";
}
