using HuyHieuDang.Infrastructure.Facades.Identity.Base;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;

namespace HuyHieuDang.Infrastructure.UnitTests;

/// <summary>
/// Kiểm tra bộ khung đã được nối dây đúng sau khi tách khỏi boilerplate.
/// </summary>
public class SkeletonTests
{
    [Fact]
    public void User_ShouldSupportPasswordVerification()
    {
        User user = new() { Username = "admin" };
        ((IPasswordVerify)user).HashPassword("Admin@123");

        Assert.True(((IPasswordVerify)user).Verify("Admin@123"));
        Assert.False(((IPasswordVerify)user).Verify("sai-mat-khau"));
    }

    [Fact]
    public void User_ShouldBeAJwtUser()
    {
        Assert.True(typeof(IJwtUser).IsAssignableFrom(typeof(User)));
    }
}
