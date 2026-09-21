using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Modules.Auth.Responses;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Enums;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Requests;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Nhóm 2 của hợp đồng API (mục 3.1 → 3.6, UC-20 → UC-23) chạy trên PostgreSQL thật,
/// với "hôm nay" cố định <see cref="HuyHieuDangApiFactory.FixedToday"/> nên tuổi đảng
/// trong khẳng định không lệ thuộc ngày chạy.
/// </summary>
[Collection(ApiCollection.Name)]
public class PartyMemberEndpointTests
{
    private const string BasePath = "/api/PartyMembers";
    private const string DeleteManyPath = BasePath + "/DeleteMany";

    private readonly HuyHieuDangApiFactory factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartyMemberEndpointTests"/> class.
    /// </summary>
    /// <param name="factory">Host kiểm thử dùng chung cho cả lớp.</param>
    public PartyMemberEndpointTests(HuyHieuDangApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact(DisplayName = "3.1 · Danh sách trả gender chuỗi và ba giá trị tuổi đảng tính theo hôm nay")]
    public async Task Search_ReturnsStringGenderAndComputedPartyAge()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync(
            new PartyMember { FullName = "Cao Văn Phúc", DateOfBirth = new DateOnly(1951, 4, 18), Gender = Gender.Male, OfficialAdmissionDate = new DateOnly(1976, 9, 12) },
            new PartyMember { FullName = "Bùi Thị Lan", DateOfBirth = new DateOnly(1972, 9, 2), Gender = Gender.Female, OfficialAdmissionDate = new DateOnly(1998, 6, 12) },
            new PartyMember { FullName = "Đỗ Thị Mai", OfficialAdmissionDate = new DateOnly(1930, 1, 1) });

        PagedPayload<PartyMemberPayload> page = await GetPageAsync(client, BasePath);

        Assert.Equal(3, page.PageInfo.TotalCount);

        PartyMemberPayload phuc = page.PagedData.Single(x => x.FullName == "Cao Văn Phúc");
        Assert.Equal("Male", phuc.Gender);
        Assert.Equal(50, phuc.PartyAgeYears);
        Assert.Equal(55, phuc.NextMilestone);
        Assert.Equal(new DateOnly(2031, 9, 12), phuc.NextMilestoneDate);

        PartyMemberPayload lan = page.PagedData.Single(x => x.FullName == "Bùi Thị Lan");
        Assert.Equal("Female", lan.Gender);
        Assert.Equal(28, lan.PartyAgeYears);
        Assert.Equal(30, lan.NextMilestone);
        Assert.Equal(new DateOnly(2028, 6, 12), lan.NextMilestoneDate);

        // Ô trống trả null, và người đã vượt mốc lớn nhất (90) không còn mốc kế tiếp.
        PartyMemberPayload mai = page.PagedData.Single(x => x.FullName == "Đỗ Thị Mai");
        Assert.Null(mai.Gender);
        Assert.Null(mai.DateOfBirth);
        Assert.Equal(96, mai.PartyAgeYears);
        Assert.Null(mai.NextMilestone);
        Assert.Null(mai.NextMilestoneDate);
    }

    [Fact(DisplayName = "3.1 · Phân trang: mặc định 20 dòng, chọn được số dòng và số trang")]
    public async Task Search_PagesWithChosenPageSize()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync(Enumerable.Range(1, 25)
            .Select(i => new PartyMember
            {
                FullName = $"Nguoi {i:D2}",
                OfficialAdmissionDate = new DateOnly(1990, 1, 1),
            })
            .ToArray());

        PagedPayload<PartyMemberPayload> firstDefault = await GetPageAsync(client, BasePath);
        Assert.Equal(PartyMemberQueryRequest.DefaultPageSize, firstDefault.PageInfo.PageSize);
        Assert.Equal(20, firstDefault.PagedData.Count);

        PagedPayload<PartyMemberPayload> first = await GetPageAsync(client, $"{BasePath}?current=1&pageSize=8");
        Assert.Equal(25, first.PageInfo.TotalCount);
        Assert.Equal(8, first.PageInfo.PageSize);
        Assert.Equal(1, first.PageInfo.Current);
        Assert.Equal(4, first.PageInfo.TotalPages);
        Assert.True(first.PageInfo.HasNext);
        Assert.False(first.PageInfo.HasPrevious);
        Assert.Equal(8, first.PagedData.Count);
        Assert.Equal("Nguoi 01", first.PagedData[0].FullName);

        PagedPayload<PartyMemberPayload> last = await GetPageAsync(client, $"{BasePath}?current=4&pageSize=8");
        Assert.Single(last.PagedData);
        Assert.Equal("Nguoi 25", last.PagedData[0].FullName);
        Assert.False(last.PageInfo.HasNext);
        Assert.True(last.PageInfo.HasPrevious);
    }

    // QC-T27-03: CEO chốt ngày 19/09/2026 là tham số phân trang ngoài khoảng được KẸP chứ không
    // báo lỗi, để một lần gõ nhầm không dựng banner đỏ trước mặt người dùng. Ca này trước đòi 400.
    [Theory(DisplayName = "3.1 · pageSize hoặc current không dương được kẹp về mặc định, vẫn trả 200")]
    [InlineData("?pageSize=0")]
    [InlineData("?current=0")]
    public async Task Search_WithNonPositivePaging_ClampsInsteadOfFailing(string query)
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response = await client.GetAsync(BasePath + query);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(DisplayName = "3.1 · Tìm theo họ tên chứa chuỗi, không phân biệt hoa thường")]
    public async Task Search_ByKeyword_MatchesFullNameCaseInsensitively()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync(
            new PartyMember { FullName = "Nguyễn Văn An", OfficialAdmissionDate = new DateOnly(1996, 10, 15) },
            new PartyMember { FullName = "Trần Thị Bích", OfficialAdmissionDate = new DateOnly(1996, 10, 15) },
            new PartyMember { FullName = "Lê Minh ANH", OfficialAdmissionDate = new DateOnly(1996, 10, 15) });

        PagedPayload<PartyMemberPayload> page =
            await GetPageAsync(client, $"{BasePath}?searchKeyword=an&searchFields=FullName");

        Assert.Equal(2, page.PageInfo.TotalCount);
        Assert.Equal(
            new[] { "Lê Minh ANH", "Nguyễn Văn An" },
            page.PagedData.Select(x => x.FullName).OrderBy(x => x, StringComparer.Ordinal).ToArray());
    }

    [Theory(DisplayName = "3.1 · Lọc giới tính Nam / Nữ; bỏ tham số thì lấy tất cả")]
    [InlineData("&filter.Gender=$eq:Male", 1, "Cao Văn Phúc")]
    [InlineData("&filter.Gender=$eq:Female", 1, "Bùi Thị Lan")]
    [InlineData("", 3, null)]
    public async Task Search_FiltersByGender(string filter, int expectedCount, string? expectedName)
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync(
            new PartyMember { FullName = "Cao Văn Phúc", Gender = Gender.Male, OfficialAdmissionDate = new DateOnly(1976, 9, 12) },
            new PartyMember { FullName = "Bùi Thị Lan", Gender = Gender.Female, OfficialAdmissionDate = new DateOnly(1998, 6, 12) },
            new PartyMember { FullName = "Đỗ Thị Mai", OfficialAdmissionDate = new DateOnly(1990, 1, 1) });

        PagedPayload<PartyMemberPayload> page = await GetPageAsync(client, $"{BasePath}?pageSize=50{filter}");

        Assert.Equal(expectedCount, page.PageInfo.TotalCount);

        if (expectedName is not null)
        {
            Assert.Equal(expectedName, Assert.Single(page.PagedData).FullName);
        }
    }

    [Fact(DisplayName = "3.1 · Sắp xếp theo cột; cột lạ bị bỏ qua và quay về mặc định")]
    public async Task Search_SortsBySortableColumnsOnly()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync(
            new PartyMember { FullName = "Bui Thi Lan", OfficialAdmissionDate = new DateOnly(1998, 6, 12) },
            new PartyMember { FullName = "Cao Van Phuc", OfficialAdmissionDate = new DateOnly(1976, 9, 12) },
            new PartyMember { FullName = "Do Thi Mai", OfficialAdmissionDate = new DateOnly(1990, 1, 1) });

        PagedPayload<PartyMemberPayload> descending =
            await GetPageAsync(client, $"{BasePath}?sortQuery=FullName%20desc");
        Assert.Equal(
            new[] { "Do Thi Mai", "Cao Van Phuc", "Bui Thi Lan" },
            descending.PagedData.Select(x => x.FullName).ToArray());

        PagedPayload<PartyMemberPayload> byAdmission =
            await GetPageAsync(client, $"{BasePath}?sortQuery=OfficialAdmissionDate%20asc");
        Assert.Equal(
            new[] { "Cao Van Phuc", "Do Thi Mai", "Bui Thi Lan" },
            byAdmission.PagedData.Select(x => x.FullName).ToArray());

        PagedPayload<PartyMemberPayload> unknownColumn =
            await GetPageAsync(client, $"{BasePath}?sortQuery=khongCoCot%20asc");
        Assert.Equal(
            new[] { "Bui Thi Lan", "Cao Van Phuc", "Do Thi Mai" },
            unknownColumn.PagedData.Select(x => x.FullName).ToArray());
    }

    [Fact(DisplayName = "T51 · Sắp xếp theo Tuổi đảng quy về Ngày chính thức theo chiều ngược lại")]
    public async Task Search_SortsByPartyAge()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync(
            new PartyMember { FullName = "Bui Thi Lan", OfficialAdmissionDate = new DateOnly(1998, 6, 12) },
            new PartyMember { FullName = "Cao Van Phuc", OfficialAdmissionDate = new DateOnly(1976, 9, 12) },
            new PartyMember { FullName = "Do Thi Mai", OfficialAdmissionDate = new DateOnly(1990, 1, 1) });

        // Tuổi đảng tăng dần ⟺ vào Đảng muộn nhất đứng đầu.
        foreach (string column in new[] { "PartyAge", "partyAgeYears" })
        {
            PagedPayload<PartyMemberPayload> ascending =
                await GetPageAsync(client, $"{BasePath}?sortQuery={column}%20asc");
            Assert.Equal(
                new[] { "Bui Thi Lan", "Do Thi Mai", "Cao Van Phuc" },
                ascending.PagedData.Select(x => x.FullName).ToArray());
            Assert.Equal(new[] { 28, 36, 50 }, ascending.PagedData.Select(x => x.PartyAgeYears).ToArray());
        }

        PagedPayload<PartyMemberPayload> descending =
            await GetPageAsync(client, $"{BasePath}?sortQuery=PartyAge%20desc");
        Assert.Equal(new[] { 50, 36, 28 }, descending.PagedData.Select(x => x.PartyAgeYears).ToArray());
    }

    [Fact(DisplayName = "T51 · Sắp xếp theo Mốc kế tiếp; nhóm không còn mốc đứng cuối khi tăng dần")]
    public async Task Search_SortsByNextMilestone()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync(
            new PartyMember { FullName = "Moc 30", OfficialAdmissionDate = new DateOnly(1998, 6, 12) },
            new PartyMember { FullName = "Moc 40", OfficialAdmissionDate = new DateOnly(1990, 1, 1) },
            new PartyMember { FullName = "Moc 55", OfficialAdmissionDate = new DateOnly(1976, 9, 12) },
            new PartyMember { FullName = "Khong con moc", OfficialAdmissionDate = new DateOnly(1930, 1, 1) });

        PagedPayload<PartyMemberPayload> ascending =
            await GetPageAsync(client, $"{BasePath}?sortQuery=NextMilestone%20asc");
        Assert.Equal(
            new int?[] { 30, 40, 55, null },
            ascending.PagedData.Select(x => x.NextMilestone).ToArray());

        PagedPayload<PartyMemberPayload> descending =
            await GetPageAsync(client, $"{BasePath}?sortQuery=NextMilestone%20desc");
        Assert.Equal(
            new int?[] { null, 55, 40, 30 },
            descending.PagedData.Select(x => x.NextMilestone).ToArray());
    }

    [Fact(DisplayName = "T51 · Lọc theo Mốc kế tiếp chạy trong SQL nên tổng số dòng đúng")]
    public async Task Search_FiltersByNextMilestone()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();

        // "Moc 40 bien" tròn đúng 35 năm vào hôm nay nên mốc kế tiếp của người này đã là 40.
        await SeedAsync(
            new PartyMember { FullName = "Moc 30 A", OfficialAdmissionDate = new DateOnly(1998, 6, 12) },
            new PartyMember { FullName = "Moc 30 B", OfficialAdmissionDate = new DateOnly(2020, 1, 1) },
            new PartyMember { FullName = "Moc 40 bien", OfficialAdmissionDate = new DateOnly(1991, 9, 19) },
            new PartyMember { FullName = "Moc 40", OfficialAdmissionDate = new DateOnly(1990, 1, 1) },
            new PartyMember { FullName = "Khong con moc", OfficialAdmissionDate = new DateOnly(1930, 1, 1) });

        PagedPayload<PartyMemberPayload> milestone40 =
            await GetPageAsync(client, $"{BasePath}?pageSize=1&filter.NextMilestone=$eq:40");
        Assert.Equal(2, milestone40.PageInfo.TotalCount);
        Assert.Equal(40, Assert.Single(milestone40.PagedData).NextMilestone);

        PagedPayload<PartyMemberPayload> milestone30 =
            await GetPageAsync(client, $"{BasePath}?filter.NextMilestone=$eq:30");
        Assert.Equal(2, milestone30.PageInfo.TotalCount);
        Assert.All(milestone30.PagedData, x => Assert.Equal(30, x.NextMilestone));

        PagedPayload<PartyMemberPayload> none =
            await GetPageAsync(client, $"{BasePath}?filter.NextMilestone=$eq:None");
        Assert.Equal(1, none.PageInfo.TotalCount);
        Assert.Equal("Khong con moc", none.PagedData.Single().FullName);
        Assert.Null(none.PagedData.Single().NextMilestone);

        // Lọc và tìm kiếm cùng lúc vẫn cộng dồn trong một câu SQL.
        PagedPayload<PartyMemberPayload> combined =
            await GetPageAsync(client, $"{BasePath}?searchKeyword=bien&filter.NextMilestone=$eq:40");
        Assert.Equal(1, combined.PageInfo.TotalCount);
        Assert.Equal("Moc 40 bien", combined.PagedData.Single().FullName);
    }

    [Fact(DisplayName = "T51 · Lọc theo Mốc kế tiếp bám dãy mốc trong Cài đặt")]
    public async Task Search_FiltersByNextMilestone_FollowsSettings()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        await SeedAsync(
            new PartyMember { FullName = "Tuoi dang 36", OfficialAdmissionDate = new DateOnly(1990, 1, 1) });

        // Bước 5: tuổi đảng 36 cho mốc kế tiếp 40.
        Assert.Equal(1, (await GetPageAsync(client, $"{BasePath}?filter.NextMilestone=$eq:40")).PageInfo.TotalCount);

        await UpdateMilestoneSettingsAsync(client, startYears: 30, endYears: 90, stepYears: 10);

        try
        {
            // Bước 10: mốc 35 biến mất khỏi dãy nên là 400, còn mốc kế tiếp của người này vẫn 40.
            Assert.Equal(1, (await GetPageAsync(client, $"{BasePath}?filter.NextMilestone=$eq:40")).PageInfo.TotalCount);

            HttpResponseMessage removed = await client.GetAsync($"{BasePath}?filter.NextMilestone=$eq:35");
            Assert.Equal(HttpStatusCode.BadRequest, removed.StatusCode);
        }
        finally
        {
            await UpdateMilestoneSettingsAsync(client, startYears: 30, endYears: 90, stepYears: 5);
        }
    }

    [Theory(DisplayName = "T51 · Mốc không hợp lệ trả 400 kèm khóa Invalid.NextMilestone")]
    [InlineData("$eq:33")]
    [InlineData("$eq:abc")]
    [InlineData("$eq:")]
    [InlineData("$gt:30")]
    [InlineData("40")]
    public async Task Search_WithUnknownNextMilestone_ReturnsBadRequest(string value)
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response = await client.GetAsync($"{BasePath}?filter.NextMilestone={value}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            Messages<PartyMember>.Invalid(nameof(PartyMemberResponse.NextMilestone)),
            (await response.ReadApiResponseAsync<object>()).Message);
    }

    [Fact(DisplayName = "3.3 · Thêm mới trả 200 kèm bản ghi vừa tạo và khóa Create.Successfully")]
    public async Task Create_ReturnsCreatedMember()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();

        HttpResponseMessage response = await client.PostAsJsonAsync(BasePath, new
        {
            fullName = "Nguyễn Văn An",
            dateOfBirth = "1958-03-12",
            gender = "Male",
            officialAdmissionDate = "1996-10-15",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiResponse<PartyMemberPayload> body = await response.ReadApiResponseAsync<PartyMemberPayload>();
        Assert.Equal(Messages<PartyMember>.Create(), body.Message);
        Assert.NotNull(body.Data);
        Assert.NotEqual(Guid.Empty, body.Data!.Id);
        Assert.Equal("Nguyễn Văn An", body.Data.FullName);
        Assert.Equal("Male", body.Data.Gender);
        Assert.Equal(new DateOnly(1958, 3, 12), body.Data.DateOfBirth);
        Assert.Equal(new DateOnly(1996, 10, 15), body.Data.OfficialAdmissionDate);
        Assert.Equal(29, body.Data.PartyAgeYears);
        Assert.Equal(30, body.Data.NextMilestone);
        Assert.Equal(new DateOnly(2026, 10, 15), body.Data.NextMilestoneDate);
    }

    [Fact(DisplayName = "3.3 · QT9 · Thêm hai người trùng hệt nhau vẫn thành hai bản ghi")]
    public async Task Create_DoesNotRejectDuplicates()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();

        object payload = new
        {
            fullName = "Trùng Tên Hệt",
            dateOfBirth = "1960-01-01",
            gender = "Female",
            officialAdmissionDate = "1990-05-05",
        };

        Guid first = await CreateAndReadIdAsync(client, payload);
        Guid second = await CreateAndReadIdAsync(client, payload);

        Assert.NotEqual(first, second);
        Assert.Equal(2, (await GetPageAsync(client, BasePath)).PageInfo.TotalCount);
    }

    [Theory(DisplayName = "3.3 · Bảng ràng buộc trả đúng khóa lỗi 400")]
    [InlineData(null, null, null, "1996-10-15", nameof(MessagesType.Required), nameof(PartyMember.FullName))]
    [InlineData("   ", null, null, "1996-10-15", nameof(MessagesType.Required), nameof(PartyMember.FullName))]
    [InlineData("Nguyễn Văn An", null, null, null, nameof(MessagesType.Required), nameof(PartyMember.OfficialAdmissionDate))]
    [InlineData("Nguyễn Văn An", null, null, "2026-09-20", nameof(MessagesType.Invalid), nameof(PartyMember.OfficialAdmissionDate))]
    [InlineData("Nguyễn Văn An", "1996-10-16", null, "1996-10-15", nameof(MessagesType.Invalid), nameof(PartyMember.DateOfBirth))]
    [InlineData("Nguyễn Văn An", null, "Khac", "1996-10-15", nameof(MessagesType.Invalid), nameof(PartyMember.Gender))]
    public async Task Create_WithInvalidBody_ReturnsContractKey(
        string? fullName, string? dateOfBirth, string? gender, string? officialAdmissionDate, string messagesType, string property)
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response = await client.PostAsJsonAsync(BasePath, new
        {
            fullName,
            dateOfBirth,
            gender,
            officialAdmissionDate,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        ApiResponse<object> body = await response.ReadApiResponseAsync<object>();
        Assert.Equal(
            Messages<PartyMember>.Action(Enum.Parse<MessagesType>(messagesType), property),
            body.Message);
    }

    [Fact(DisplayName = "3.3 · Đúng ngày hôm nay là ngày chính thức hợp lệ")]
    public async Task Create_WithTodayAsAdmissionDate_Succeeds()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();

        HttpResponseMessage response = await client.PostAsJsonAsync(BasePath, new
        {
            fullName = "Vào Đảng Hôm Nay",
            officialAdmissionDate = HuyHieuDangApiFactory.FixedToday.ToString("yyyy-MM-dd"),
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiResponse<PartyMemberPayload> body = await response.ReadApiResponseAsync<PartyMemberPayload>();
        Assert.Equal(0, body.Data!.PartyAgeYears);
        Assert.Equal(30, body.Data.NextMilestone);
    }

    [Fact(DisplayName = "3.2 · Lấy một trả đúng bản ghi; id lạ trả 400 Mes.PartyMember.NotFound")]
    public async Task GetById_ReturnsMemberOrNotFoundKey()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        Guid id = await CreateAndReadIdAsync(client, new
        {
            fullName = "Phạm Thị Dung",
            gender = "Female",
            officialAdmissionDate = "1986-06-05",
        });

        HttpResponseMessage found = await client.GetAsync($"{BasePath}/{id}");
        Assert.Equal(HttpStatusCode.OK, found.StatusCode);

        ApiResponse<PartyMemberPayload> body = await found.ReadApiResponseAsync<PartyMemberPayload>();
        Assert.Equal(id, body.Data!.Id);
        Assert.Equal("Female", body.Data.Gender);

        HttpResponseMessage missing = await client.GetAsync($"{BasePath}/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
        Assert.Equal(Messages<PartyMember>.NotFound(), (await missing.ReadApiResponseAsync<object>()).Message);
    }

    [Fact(DisplayName = "3.4 · Sửa ghi đè cả bốn trường, kể cả xóa bằng null")]
    public async Task Update_OverwritesAllFields()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        Guid id = await CreateAndReadIdAsync(client, new
        {
            fullName = "Tên Cũ",
            dateOfBirth = "1960-01-01",
            gender = "Male",
            officialAdmissionDate = "1990-05-05",
        });

        HttpResponseMessage response = await client.PutAsJsonAsync($"{BasePath}/{id}", new
        {
            fullName = "Tên Mới",
            dateOfBirth = (string?)null,
            gender = (string?)null,
            officialAdmissionDate = "1991-07-07",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiResponse<PartyMemberPayload> body = await response.ReadApiResponseAsync<PartyMemberPayload>();
        Assert.Equal(Messages<PartyMember>.Update(), body.Message);
        Assert.Equal(id, body.Data!.Id);
        Assert.Equal("Tên Mới", body.Data.FullName);
        Assert.Null(body.Data.DateOfBirth);
        Assert.Null(body.Data.Gender);
        Assert.Equal(new DateOnly(1991, 7, 7), body.Data.OfficialAdmissionDate);
        Assert.Equal(35, body.Data.PartyAgeYears);
    }

    [Fact(DisplayName = "3.4 · Sửa id không tồn tại trả 400 Mes.PartyMember.NotFound")]
    public async Task Update_WithUnknownId_ReturnsNotFoundKey()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response = await client.PutAsJsonAsync($"{BasePath}/{Guid.NewGuid()}", new
        {
            fullName = "Không Ai",
            officialAdmissionDate = "1990-05-05",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(Messages<PartyMember>.NotFound(), (await response.ReadApiResponseAsync<object>()).Message);
    }

    [Fact(DisplayName = "3.6 · Xóa một người qua mảng một phần tử trả đúng một id, bản ghi biến mất hẳn")]
    public async Task DeleteMany_WithSingleId_RemovesMemberPermanently()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        Guid id = await CreateAndReadIdAsync(client, new
        {
            fullName = "Sắp Bị Xóa",
            officialAdmissionDate = "1990-05-05",
        });

        HttpResponseMessage response = await client.PostAsJsonAsync(DeleteManyPath, new { ids = new[] { id } });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiResponse<IdentifiersPayload> body = await response.ReadApiResponseAsync<IdentifiersPayload>();
        Assert.Equal(Messages<PartyMember>.Delete(), body.Message);
        Assert.Equal(id, Assert.Single(body.Data!.Ids));

        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync($"{BasePath}/{id}")).StatusCode);
        Assert.Equal(0, (await GetPageAsync(client, BasePath)).PageInfo.TotalCount);
    }

    [Fact(DisplayName = "3.6 · Xóa mảng chỉ có id lạ trả danh sách rỗng chứ không phải lỗi")]
    public async Task DeleteMany_WithUnknownIdOnly_ReturnsEmptyList()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response =
            await client.PostAsJsonAsync(DeleteManyPath, new { ids = new[] { Guid.NewGuid() } });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty((await response.ReadApiResponseAsync<IdentifiersPayload>()).Data!.Ids);
    }

    [Fact(DisplayName = "T34 · DELETE /api/PartyMembers/{id} đã bị gỡ, không ai dựng lại được")]
    public async Task Delete_SingleRoute_NoLongerExists()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response = await client.DeleteAsync($"{BasePath}/{Guid.NewGuid()}");

        Assert.Contains(
            response.StatusCode,
            new[] { HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed });
    }

    [Fact(DisplayName = "3.6 · Xóa nhiều trả đúng id đã xóa, id lạ bị bỏ qua lặng lẽ")]
    public async Task DeleteMany_IgnoresUnknownIds()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();
        await ResetAsync();
        Guid first = await CreateAndReadIdAsync(client, new { fullName = "Một", officialAdmissionDate = "1990-05-05" });
        Guid second = await CreateAndReadIdAsync(client, new { fullName = "Hai", officialAdmissionDate = "1990-05-05" });
        Guid kept = await CreateAndReadIdAsync(client, new { fullName = "Ba", officialAdmissionDate = "1990-05-05" });
        Guid unknown = Guid.NewGuid();

        HttpResponseMessage response = await client.PostAsJsonAsync(DeleteManyPath, new { ids = new[] { first, unknown, second } });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiResponse<IdentifiersPayload> body = await response.ReadApiResponseAsync<IdentifiersPayload>();
        Assert.Equal(new[] { first, second }, body.Data!.Ids.ToArray());

        PagedPayload<PartyMemberPayload> remaining = await GetPageAsync(client, BasePath);
        Assert.Equal(kept, Assert.Single(remaining.PagedData).Id);
    }

    [Fact(DisplayName = "3.6 · Xóa nhiều với danh sách rỗng trả 400")]
    public async Task DeleteMany_WithEmptyList_ReturnsBadRequest()
    {
        HttpClient client = await CreateAuthenticatedClientAsync();

        HttpResponseMessage response = await client.PostAsJsonAsync(DeleteManyPath, new { ids = Array.Empty<Guid>() });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            Messages<PartyMember>.Required(nameof(DeletePartyMemberRangeRequest.Ids)),
            (await response.ReadApiResponseAsync<object>()).Message);
    }

    [Theory(DisplayName = "A-001 · Mọi endpoint đảng viên không kèm token trả 401")]
    [InlineData("GET", BasePath)]
    [InlineData("POST", BasePath)]
    [InlineData("POST", DeleteManyPath)]
    public async Task PartyMemberEndpoint_WithoutToken_ReturnsUnauthorized(string method, string path)
    {
        HttpClient client = factory.CreateClient();

        using HttpRequestMessage request = new(new HttpMethod(method), path);
        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task UpdateMilestoneSettingsAsync(
        HttpClient client, int startYears, int endYears, int stepYears)
    {
        HttpResponseMessage response = await client.PutAsJsonAsync(
            "/api/Settings",
            new { startYears, endYears, stepYears });

        response.EnsureSuccessStatusCode();
    }

    private static async Task<PagedPayload<PartyMemberPayload>> GetPageAsync(HttpClient client, string url)
    {
        HttpResponseMessage response = await client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiResponse<PagedPayload<PartyMemberPayload>> body =
            await response.ReadApiResponseAsync<PagedPayload<PartyMemberPayload>>();

        Assert.Equal(Messages<PartyMember>.Search(), body.Message);

        return body.Data!;
    }

    private static async Task<Guid> CreateAndReadIdAsync(HttpClient client, object payload)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(BasePath, payload);
        response.EnsureSuccessStatusCode();

        return (await response.ReadApiResponseAsync<PartyMemberPayload>()).Data!.Id;
    }

    /// <summary>
    /// Mỗi bài tự dọn bảng đảng viên: lớp này có host và container riêng, các bài trong lớp
    /// chạy tuần tự nên không giẫm lên nhau.
    /// </summary>
    private async Task ResetAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Set<PartyMember>().ExecuteDeleteAsync();
    }

    private async Task SeedAsync(params PartyMember[] members)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Set<PartyMember>().AddRange(members);
        await dbContext.SaveChangesAsync();
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage login = await client.PostAsJsonAsync(
            "/api/Auth/Login",
            new { username = HuyHieuDangApiFactory.AdminUsername, password = HuyHieuDangApiFactory.AdminPassword });
        login.EnsureSuccessStatusCode();

        ApiResponse<SessionPayload> body = await login.ReadApiResponseAsync<SessionPayload>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TokenTypes.Bearer, body.Data!.AccessToken);

        return client;
    }
}
