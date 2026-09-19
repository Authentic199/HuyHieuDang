using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// T-FIX-1: dấu thời gian <c>UpdatedAt</c> do tầng lưu trữ đóng, lấy từ
/// <see cref="IDateTimeProvider"/> — không thực thể nào tự đọc đồng hồ máy.
/// Host kiểm thử đóng băng thời gian, nên mọi dấu phải đúng bằng giá trị cố định đó.
/// </summary>
[Collection(ApiCollection.Name)]
public class UpdatedAtStampTests
{
    private readonly HuyHieuDangApiFactory factory;

    public UpdatedAtStampTests(HuyHieuDangApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact(DisplayName = "Cài đặt seed sẵn mang dấu thời gian của IDateTimeProvider, không phải giờ máy")]
    public async Task SeededAppSetting_CarriesTheProvidersInstant()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        DateTimeOffset expected = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>().Now;
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        AppSetting appSetting = await dbContext.Set<AppSetting>().AsNoTracking().SingleAsync();

        Assert.Equal(expected.ToUnixTimeMilliseconds(), appSetting.UpdatedAt.ToUnixTimeMilliseconds());
        Assert.Equal(HuyHieuDangApiFactory.FixedToday, DateOnly.FromDateTime(expected.DateTime));
    }

    [Fact(DisplayName = "Thêm mới đảng viên được đóng dấu dù không ai gán UpdatedAt")]
    public async Task AddingAnEntity_StampsUpdatedAt()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        DateTimeOffset expected = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>().Now;
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        PartyMember member = NewMember("Nguyễn Văn Dấu Thời Gian");
        Assert.Equal(default, member.UpdatedAt);

        dbContext.Add(member);
        await dbContext.SaveChangesAsync();

        Assert.Equal(expected.ToUnixTimeMilliseconds(), member.UpdatedAt.ToUnixTimeMilliseconds());
    }

    [Fact(DisplayName = "Sửa đảng viên ghi đè dấu thời gian cũ")]
    public async Task ModifyingAnEntity_RefreshesUpdatedAt()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        DateTimeOffset expected = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>().Now;
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        PartyMember member = NewMember("Trần Thị Sửa Đổi");
        dbContext.Add(member);
        await dbContext.SaveChangesAsync();

        // Đặt lùi dấu thời gian để chứng minh lần lưu sau thật sự ghi đè, chứ không chỉ giữ nguyên.
        DateTimeOffset stale = expected.AddYears(-3);
        member.UpdatedAt = stale;
        member.FullName = "Trần Thị Sửa Đổi (đã sửa)";
        await dbContext.SaveChangesAsync();

        Assert.NotEqual(stale.ToUnixTimeMilliseconds(), member.UpdatedAt.ToUnixTimeMilliseconds());
        Assert.Equal(expected.ToUnixTimeMilliseconds(), member.UpdatedAt.ToUnixTimeMilliseconds());
    }

    private static PartyMember NewMember(string fullName) => new()
    {
        FullName = fullName,
        OfficialAdmissionDate = new DateOnly(1980, 5, 19),
    };
}
