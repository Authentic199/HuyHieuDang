namespace HuyHieuDang.Web.QcIntegrationTests.Support;

/// <summary>Định vị kho mã để đọc fixture Excel và chạy các ca kiểm thử tĩnh (A-901, A-905).</summary>
public static class QcPaths
{
    /// <summary>Thư mục gốc kho mã (thư mục chứa <c>BE</c> và <c>tests</c>).</summary>
    public static string RepositoryRoot { get; } = FindRoot();

    /// <summary>Thư mục <c>BE/src</c>.</summary>
    public static string BackendSource => Path.Combine(RepositoryRoot, "BE", "src");

    /// <summary>Thư mục <c>tests/fixtures/excel</c>.</summary>
    public static string ExcelFixtures => Path.Combine(RepositoryRoot, "tests", "fixtures", "excel");

    /// <summary>Đường dẫn đầy đủ tới một file Excel mẫu.</summary>
    /// <param name="fileName">Tên file trong <c>tests/fixtures/excel</c>.</param>
    /// <returns>Đường dẫn tuyệt đối.</returns>
    public static string Excel(string fileName) => Path.Combine(ExcelFixtures, fileName);

    private static string FindRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "BE", "src", "Core"))
                && Directory.Exists(Path.Combine(directory.FullName, "tests", "fixtures")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Không tìm thấy thư mục gốc kho mã khi đi ngược từ {AppContext.BaseDirectory}.");
    }
}
