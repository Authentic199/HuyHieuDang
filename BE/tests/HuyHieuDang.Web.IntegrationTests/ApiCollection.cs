namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Gom mọi lớp kiểm thử cần API thật vào một bộ, để cả bộ dùng chung đúng một container
/// PostgreSQL thay vì mỗi lớp dựng một cái.
/// </summary>
[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<HuyHieuDangApiFactory>
{
    /// <summary>
    /// Tên bộ, dùng ở thuộc tính <c>[Collection]</c> của từng lớp.
    /// </summary>
    public const string Name = "API";
}
