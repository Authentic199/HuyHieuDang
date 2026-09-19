using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using System.Collections.ObjectModel;

namespace HuyHieuDang.Infrastructure.Facades.Definitions;

public static class MxmPermissions
{
    /// <summary>
    /// Prefix Permissions
    /// </summary>
    public const string PrePermissions = nameof(PrePermissions);
    public const string AdminGuard = "1";
    public const string BasicGuard = "2";
    public const string GuardSeparator = ";";

    private static List<MxmPermission> GetAllValue()
        => new List<MxmPermission>()
        .Concat(Users)
        .Concat(Roles)
        .ToList();

    private static readonly MxmPermission[] Users = new MxmPermission[]
    {
         new(
                MxmResource.Users,
                MxmAction.Create,
                typeof(User).FullName!,
                JoinGuards(AdminGuard, BasicGuard),
                MxmResource.Users + MxmAction.View,
                MxmResource.Users + MxmAction.ViewDetail,
                MxmResource.Roles + MxmAction.View
            ),
         new(
                MxmResource.Users,
                MxmAction.Update,
                typeof(User).FullName!,
                JoinGuards(AdminGuard, BasicGuard),
                MxmResource.Users + MxmAction.View,
                MxmResource.Users + MxmAction.ViewDetail,
                MxmResource.Roles + MxmAction.View
            ),
         new(
                MxmResource.Users,
                MxmAction.Delete,
                typeof(User).FullName!,
                JoinGuards(AdminGuard, BasicGuard),
                MxmResource.Users + MxmAction.View,
                MxmResource.Users + MxmAction.ViewDetail
            ),
         new(
                MxmResource.Users,
                MxmAction.View,
                typeof(User).FullName!,
                JoinGuards(AdminGuard, BasicGuard)
            ),
         new(
                MxmResource.Users,
                MxmAction.ViewDetail,
                typeof(User).FullName!,
                JoinGuards(AdminGuard, BasicGuard),
                MxmResource.Users + MxmAction.View
            ),
    };

    private static readonly MxmPermission[] Roles = new MxmPermission[]
    {
           new(
                MxmResource.Roles,
                MxmAction.Create,
                typeof(User).FullName!,
                JoinGuards(AdminGuard, BasicGuard),
                MxmResource.Roles + MxmAction.View,
                MxmResource.Roles + MxmAction.ViewDetail
            ),
           new(
                MxmResource.Roles,
                MxmAction.Update,
                typeof(User).FullName!,
                JoinGuards(AdminGuard, BasicGuard),
                MxmResource.Roles + MxmAction.View,
                MxmResource.Roles + MxmAction.ViewDetail
            ),
           new(
                MxmResource.Roles,
                MxmAction.Delete,
                typeof(User).FullName!,
                JoinGuards(AdminGuard, BasicGuard),
                MxmResource.Roles + MxmAction.View,
                MxmResource.Roles + MxmAction.ViewDetail
            ),
           new(
                MxmResource.Roles,
                MxmAction.View,
                typeof(User).FullName!,
                JoinGuards(AdminGuard, BasicGuard)
            ),
           new(
                MxmResource.Roles,
                MxmAction.ViewDetail,
                typeof(User).FullName!,
                JoinGuards(AdminGuard, BasicGuard),
                MxmResource.Roles + MxmAction.View
            ),
    };

    public static IReadOnlyList<MxmPermission> All { get; } = GetAllValue();

    public static IReadOnlyList<MxmPermission> Admin { get; }
        = new ReadOnlyCollection<MxmPermission>(
            All.Where(x =>
                x.ModelType == typeof(User).FullName!
                && Array.Exists(x.Guards.Split(GuardSeparator), x => x == AdminGuard)
            ).ToArray()
        );

    public static IReadOnlyList<MxmPermission> Basic { get; }
        = new ReadOnlyCollection<MxmPermission>(
            All.Where(x =>
                x.ModelType == typeof(User).FullName!
                && Array.Exists(x.Guards.Split(GuardSeparator), x => x == BasicGuard)
            ).ToArray()
        );

    public static IEnumerable<string> GetAllPermission(IEnumerable<string> permissionCodes)
    {
        List<string> permissions = new();
        foreach (string permissionCode in permissionCodes)
        {
            if (!PermissionsValue.ContainsKey(permissionCode))
            {
                throw new InvalidOperationException($"{permissionCode} not declare in {nameof(All)}");
            }

            permissions.Add(permissionCode);
            permissions.AddRange(PermissionsValue[permissionCode].RelatePermissions);
        }

        return permissions;
    }

    public static string JoinGuards(params string[] guards)
    {
        return string.Join(GuardSeparator, guards);
    }

    private static Dictionary<string, MxmPermission> PermissionsValue => All.ToDictionary(x => x.Code, x => x);
}

public record MxmPermission(string Resource, string Action, string ModelType, string Guards = MxmPermissions.AdminGuard, params string[] RelatePermissions)
{
    public string Code => Resource + Action;

    public string Name => $"{Resource} {Action}";
}

public static class MxmAction
{
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string View = nameof(View);
    public const string ViewDetail = nameof(ViewDetail);
    public const string Export = nameof(Export);

    public const string ChangeStatus = nameof(ChangeStatus);

    public const string Join = nameof(Join);

    public const string Revoke = nameof(Revoke);
}

public static class MxmResource
{
    public const string Users = nameof(Users);
    public const string Roles = nameof(Roles);
}