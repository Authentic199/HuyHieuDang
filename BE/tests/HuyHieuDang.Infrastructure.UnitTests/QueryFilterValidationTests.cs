using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Definitions;

namespace HuyHieuDang.Infrastructure.UnitTests;

/// <summary>
/// T47 · QC-T27-05: giá trị lọc không ép được về kiểu của trường phải trả 400 kèm
/// khóa <c>Mes.Common.Invalid.Parameter</c>, không được âm thầm bỏ qua bộ lọc rồi
/// trả về toàn bộ dữ liệu. Giá trị hợp lệ phải giữ nguyên hành vi cũ.
/// </summary>
public class QueryFilterValidationTests
{
    private static readonly List<Row> Rows = new()
    {
        new Row { FullName = "Nguyễn Văn An", Gender = TestGender.Male, Age = 30, JoinedOn = new DateOnly(1990, 1, 15), Note = "ghi chú" },
        new Row { FullName = "Trần Thị Bình", Gender = TestGender.Female, Age = 45, JoinedOn = new DateOnly(2000, 6, 1), Note = null },
        new Row { FullName = "Lê Văn Cường", Gender = TestGender.Male, Age = 60, JoinedOn = null, Note = null },
    };

    private enum TestGender : byte
    {
        Male = 1,
        Female = 2,
    }

    [Fact]
    public void KhongCoBoLoc_GiuNguyenDanhSach()
    {
        Assert.Equal(Rows.Count, Query().ApplyFilter(null).Count());
        Assert.Equal(Rows.Count, Query().ApplyFilter(new Dictionary<string, List<string>?>()).Count());
    }

    [Fact]
    public void GiaTriEnumHopLe_VanLocDung()
    {
        List<Row> result = Query().ApplyFilter(Filter("Gender", "$eq:Male")).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, row => Assert.Equal(TestGender.Male, row.Gender));
    }

    [Theory]
    [InlineData("$eq:Khac")]
    [InlineData("$eq:")]
    [InlineData("$eq:1")]
    [InlineData("$in:Male,Khac")]
    [InlineData("$not:$eq:Khac")]
    public void GiaTriEnumSaiKieu_Tra400(string value)
    {
        AssertBadRequest(() => Query().ApplyFilter(Filter("Gender", value)).ToList());
    }

    [Fact]
    public void GiaTriSoSaiKieu_Tra400()
    {
        AssertBadRequest(() => Query().ApplyFilter(Filter("Age", "$gte:ba-muoi")).ToList());
        AssertBadRequest(() => Query().ApplyFilter(Filter("Age", "$btw:10,abc")).ToList());
        AssertBadRequest(() => Query().ApplyFilter(Filter("Age", "$btw:10")).ToList());
    }

    [Fact]
    public void ToanTuKhongTonTaiHoacThieuToanTu_Tra400()
    {
        AssertBadRequest(() => Query().ApplyFilter(Filter("Gender", "Male")).ToList());
        AssertBadRequest(() => Query().ApplyFilter(Filter("Age", "$xyz:30")).ToList());
    }

    [Fact]
    public void ToanTuInHopLe_VanLocDung()
    {
        List<Row> result = Query().ApplyFilter(Filter("Gender", "$in:Male,Female")).ToList();

        Assert.Equal(Rows.Count, result.Count);
    }

    [Fact]
    public void ToanTuInTrenTruongCoTheNull_VanLocDung()
    {
        List<Row> result = Query().ApplyFilter(Filter("JoinedOn", "$in:1990-01-15,2000-06-01")).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ToanTuBtwHopLe_VanLocDung()
    {
        List<Row> result = Query().ApplyFilter(Filter("Age", "$btw:30,45")).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ToanTuBtwTrenNgayHopLe_VanLocDung()
    {
        List<Row> result = Query().ApplyFilter(Filter("JoinedOn", "$btw:1990-01-01,1995-01-01")).ToList();

        Assert.Single(result);
        Assert.Equal("Nguyễn Văn An", result[0].FullName);
    }

    [Fact]
    public void ToanTuIlikeVaSwTrenChuoi_VanLocDung()
    {
        Assert.Equal(2, Query().ApplyFilter(Filter("FullName", "$ilike:Văn")).Count());
        Assert.Single(Query().ApplyFilter(Filter("FullName", "$sw:Trần")));
    }

    [Fact]
    public void ToanTuIlikeTrenTruongKhongPhaiChuoi_Tra400()
    {
        AssertBadRequest(() => Query().ApplyFilter(Filter("Age", "$ilike:30")).ToList());
    }

    [Fact]
    public void ToanTuNullTrenTruongCoTheNull_VanLocDung()
    {
        Assert.Single(Query().ApplyFilter(Filter("JoinedOn", "$null")));
        Assert.Equal(2, Query().ApplyFilter(Filter("Note", "$null")).Count());
    }

    [Fact]
    public void ToanTuNullTrenTruongKhongTheNull_Tra400()
    {
        AssertBadRequest(() => Query().ApplyFilter(Filter("Age", "$null")).ToList());
    }

    [Fact]
    public void TienToNotVoiGiaTriHopLe_VanLocDung()
    {
        List<Row> result = Query().ApplyFilter(Filter("Gender", "$not:$eq:Male")).ToList();

        Assert.Single(result);
        Assert.Equal(TestGender.Female, result[0].Gender);
    }

    [Fact]
    public void TenTruongKhongTonTai_GiuNguyenHanhViCu()
    {
        Assert.Equal(Rows.Count, Query().ApplyFilter(Filter("KhongCoTruongNay", "$eq:Khac")).Count());
    }

    [Fact]
    public void BanIEnumerable_CungHanhVi()
    {
        Assert.Equal(2, Rows.AsEnumerable().ApplyFilter(Filter("Gender", "$eq:Male")).Count());
        AssertBadRequest(() => Rows.AsEnumerable().ApplyFilter(Filter("Gender", "$eq:Khac")).ToList());
    }

    [Fact]
    public void DanhSachRong_VanTra400ChoGiaTriLa()
    {
        AssertBadRequest(() => new List<Row>().AsQueryable().ApplyFilter(Filter("Gender", "$eq:Khac")).ToList());
    }

    private static void AssertBadRequest(Action action)
    {
        BadRequestException exception = Assert.Throws<BadRequestException>(action);

        Assert.Equal(Messages.Common.InvalidParameter, exception.Message);
    }

    private static IQueryable<Row> Query() => Rows.AsQueryable();

    private static Dictionary<string, List<string>?> Filter(string field, string value)
        => new() { { field, new List<string> { value } } };

    private sealed class Row
    {
        public string FullName { get; set; } = string.Empty;

        public TestGender Gender { get; set; }

        public int Age { get; set; }

        public DateOnly? JoinedOn { get; set; }

        public string? Note { get; set; }
    }
}
