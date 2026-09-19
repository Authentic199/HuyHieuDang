namespace HuyHieuDang.Core.QcTests.Support;

/// <summary>Định vị thư mục mã nguồn để chạy các ca kiểm thử tĩnh (A-901).</summary>
public static class RepoPaths
{
    /// <summary>Thư mục gốc kho mã (thư mục chứa <c>BE</c> và <c>docs</c>).</summary>
    public static string RepositoryRoot { get; } = FindRoot();

    /// <summary><c>BE/src</c>.</summary>
    public static string BackendSource => Path.Combine(RepositoryRoot, "BE", "src");

    private static string FindRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "BE", "src", "Core")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Không tìm thấy thư mục gốc kho mã khi đi ngược từ {AppContext.BaseDirectory}.");
    }
}
