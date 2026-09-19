namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Mọi lớp kiểm thử tích hợp dùng chung đúng một <see cref="HuyHieuDangApiFactory"/>.
/// Bắt buộc phải chung: factory đặt chuỗi kết nối bằng biến môi trường của tiến trình, nên hai
/// factory chạy song song sẽ ghi đè lẫn nhau và host trỏ nhầm container. Gom vào một collection
/// thì các lớp chạy tuần tự trên một container duy nhất.
/// </summary>
[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<HuyHieuDangApiFactory>
{
    /// <summary>
    /// Tên collection dùng cho thuộc tính <c>[Collection]</c>.
    /// </summary>
    public const string Name = "HuyHieuDang API";
}
