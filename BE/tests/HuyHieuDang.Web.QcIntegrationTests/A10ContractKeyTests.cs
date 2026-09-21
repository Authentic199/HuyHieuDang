using System.Text.RegularExpressions;
using HuyHieuDang.Web.QcIntegrationTests.Support;

namespace HuyHieuDang.Web.QcIntegrationTests;

/// <summary>
/// Bảng khóa thông điệp trong mã kiểm thử là bản **chép tay** của mục 1.5 hợp đồng API. Hai bản
/// chép tay thì sớm muộn cũng lệch nhau: hợp đồng lên v1.4 thêm năm khóa mà bảng chép không đổi,
/// nên ca QC-T27-07 đỏ suốt mà không ai biết vì nó đang nằm sau <c>Skip</c>.
/// <para>
/// Ca dưới đây đọc thẳng bảng trong <c>docs/api-contract.md</c> và bắt hai bên phải trùng khít.
/// Thêm khóa vào hợp đồng mà quên chép sang đây là đỏ ngay, không cần chờ ai phát hiện.
/// </para>
/// </summary>
public sealed class A10ContractKeyTests
{
    /// <summary>Dòng bảng Markdown mở đầu bằng một khóa thông điệp.</summary>
    private static readonly Regex KeyRow = new(@"^\| *`(Mes\.[^`]+)` *\|", RegexOptions.Compiled);

    /// <summary>Tiêu đề mục 1.5 và tiêu đề mục kế tiếp, để cắt đúng đoạn bảng.</summary>
    private static readonly Regex SectionHeading = new(@"^#{2,4} *1\.(\d+)", RegexOptions.Compiled);

    /// <summary>
    /// A-906 · Bảng khóa trong mã kiểm thử phải trùng khít mục 1.5 hợp đồng API.
    /// </summary>
    [Fact(DisplayName = "A-906 · Bảng khóa của QC trùng khít mục 1.5 hợp đồng API")]
    public void ContractKeys_ShouldMatchTheTableInTheApiContract()
    {
        HashSet<string> fromDocument = ReadContractKeys();

        fromDocument.ShouldNotBeEmpty("không đọc được bảng khóa mục 1.5 của docs/api-contract.md");

        string[] thieu = fromDocument.Except(QcMessages.ContractKeys, StringComparer.Ordinal).Order().ToArray();
        string[] thua = QcMessages.ContractKeys.Except(fromDocument, StringComparer.Ordinal).Order().ToArray();

        thieu.ShouldBeEmpty(
            $"hợp đồng có mà QcMessages.ContractKeys thiếu: {string.Join(", ", thieu)}");
        thua.ShouldBeEmpty(
            $"QcMessages.ContractKeys có mà hợp đồng không ghi: {string.Join(", ", thua)}");
    }

    private static HashSet<string> ReadContractKeys()
    {
        string path = Path.Combine(QcPaths.RepositoryRoot, "docs", "api-contract.md");
        HashSet<string> keys = new(StringComparer.Ordinal);
        bool inside = false;

        foreach (string line in File.ReadLines(path))
        {
            Match heading = SectionHeading.Match(line);
            if (heading.Success)
            {
                inside = heading.Groups[1].Value == "5";
                continue;
            }

            if (!inside)
            {
                continue;
            }

            Match row = KeyRow.Match(line);
            if (row.Success)
            {
                keys.Add(row.Groups[1].Value);
            }
        }

        return keys;
    }
}
