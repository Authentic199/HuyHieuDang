using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;
using HuyHieuDang.Infrastructure.Modules.AppSettings.Entities;
using HuyHieuDang.Infrastructure.Modules.AwardPeriods.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Entities;
using HuyHieuDang.Infrastructure.Modules.PartyMembers.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace HuyHieuDang.Infrastructure.UnitTests;

/// <summary>
/// Kiểm tra lược đồ nghiệp vụ được cấu hình đúng tài liệu (mục 5) mà không cần chạm cơ sở dữ liệu:
/// mô hình EF Core được dựng ngoại tuyến từ <see cref="ApplicationDbContext"/>.
/// </summary>
public class BusinessSchemaTests
{
    private static readonly IModel Model = BuildModel();

    [Theory]
    [InlineData(typeof(PartyMember), "party_member")]
    [InlineData(typeof(AwardPeriod), "award_period")]
    [InlineData(typeof(AppSetting), "app_setting")]
    public void BusinessEntities_ShouldMapToUnderscoreTables(Type entityType, string tableName)
    {
        IEntityType? entity = Model.FindEntityType(entityType);

        Assert.NotNull(entity);
        Assert.Equal(tableName, entity!.GetTableName());
    }

    [Fact]
    public void PartyMember_DateColumns_ShouldBeDateOnly()
    {
        IEntityType entity = Model.FindEntityType(typeof(PartyMember))!;

        Assert.Equal("date", entity.FindProperty(nameof(PartyMember.DateOfBirth))!.GetColumnType());
        Assert.Equal("date", entity.FindProperty(nameof(PartyMember.OfficialAdmissionDate))!.GetColumnType());
    }

    [Fact]
    public void PartyMember_OfficialAdmissionDate_ShouldBeRequired()
    {
        IEntityType entity = Model.FindEntityType(typeof(PartyMember))!;

        Assert.False(entity.FindProperty(nameof(PartyMember.OfficialAdmissionDate))!.IsNullable);
        Assert.False(entity.FindProperty(nameof(PartyMember.FullName))!.IsNullable);
        Assert.True(entity.FindProperty(nameof(PartyMember.DateOfBirth))!.IsNullable);
        Assert.True(entity.FindProperty(nameof(PartyMember.Gender))!.IsNullable);
    }

    [Fact]
    public void PartyMember_FullName_ShouldUseVietnameseCollation()
    {
        IEntityType entity = Model.FindEntityType(typeof(PartyMember))!;

        // OQ-3: `Đào Văn Ân` phải đứng trước `Nguyễn Văn An`.
        Assert.Equal("vi-x-icu", entity.FindProperty(nameof(PartyMember.FullName))!.GetCollation());
    }

    [Fact]
    public void AwardPeriod_Name_ShouldBeUniqueAndCaseInsensitive()
    {
        IEntityType entity = Model.FindEntityType(typeof(AwardPeriod))!;
        IIndex? nameIndex = entity.GetIndexes()
            .FirstOrDefault(x => x.Properties.Count == 1 && x.Properties[0].Name == nameof(AwardPeriod.Name));

        Assert.NotNull(nameIndex);
        Assert.True(nameIndex!.IsUnique);
        Assert.Equal("citext", entity.FindProperty(nameof(AwardPeriod.Name))!.GetColumnType());
    }

    [Fact]
    public void AwardPeriod_ShouldNotStoreAnyYear()
    {
        IEntityType entity = Model.FindEntityType(typeof(AwardPeriod))!;

        // QT6: đợt lặp lại hằng năm nên chỉ lưu ngày/tháng.
        Assert.DoesNotContain(entity.GetProperties(), x => x.Name.Contains("Year", StringComparison.Ordinal));
    }

    [Fact]
    public void AppSetting_ShouldDefaultTo30_90_5_WithoutUnitName()
    {
        AppSetting appSetting = new();

        Assert.Equal(30, appSetting.StartYears);
        Assert.Equal(90, appSetting.EndYears);
        Assert.Equal(5, appSetting.StepYears);
        Assert.Null(appSetting.UnitName);
    }

    [Fact]
    public void Gender_ShouldOnlyOfferMaleAndFemale()
    {
        // Mục 1.10 hợp đồng API: "Male" | "Female" | null.
        Assert.Equal(new[] { nameof(Gender.Male), nameof(Gender.Female) }, Enum.GetNames<Gender>());
    }

    [Fact]
    public void Model_ShouldNotContainAnyEligibilityResultTable()
    {
        // QT5: danh sách đủ điều kiện luôn tính lại, không bao giờ lưu.
        Assert.DoesNotContain(
            Model.GetEntityTypes(),
            x => x.GetTableName()?.Contains("eligib", StringComparison.OrdinalIgnoreCase) == true);
    }

    private static IModel BuildModel()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=khong-ket-noi;Database=khong-ket-noi;Username=x;Password=x;")
            .Options;

        using ApplicationDbContext dbContext = new(options);

        // Mô hình thiết kế giữ đủ chú thích (collation, kiểu cột) mà mô hình chạy đã lược bớt.
        return dbContext.GetService<IDesignTimeModel>().Model;
    }
}
