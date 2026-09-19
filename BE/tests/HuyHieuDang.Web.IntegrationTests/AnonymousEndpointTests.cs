using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace HuyHieuDang.Web.IntegrationTests;

/// <summary>
/// Ca A-007: liệt kê toàn bộ action của mọi controller và yêu cầu chỉ đúng một action
/// được phép gọi khi chưa đăng nhập.
/// </summary>
public class AnonymousEndpointTests
{
    private const string OnlyAnonymousAction = "AuthController.LoginAsync";

    [Fact(DisplayName = "A-007 · Chỉ POST /api/Auth/Login được [AllowAnonymous]")]
    public void Controllers_ExposeExactlyOneAnonymousAction()
    {
        string[] anonymousActions = typeof(Program).Assembly
            .GetTypes()
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
            .SelectMany(type => type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName && method.GetCustomAttribute<AllowAnonymousAttribute>() is not null)
                .Select(method => $"{type.Name}.{method.Name}"))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(new[] { OnlyAnonymousAction }, anonymousActions);
    }

    [Fact(DisplayName = "Mọi controller đều kế thừa BaseController nên mặc định cần đăng nhập")]
    public void Controllers_AllInheritAuthorizedBaseController()
    {
        Type[] strays = typeof(Program).Assembly
            .GetTypes()
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
            .Where(type => type.GetCustomAttribute<AuthorizeAttribute>(inherit: true) is null)
            .ToArray();

        Assert.Empty(strays);
    }
}
