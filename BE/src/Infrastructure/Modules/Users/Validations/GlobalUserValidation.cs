using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;

namespace HuyHieuDang.Infrastructure.Modules.Users.Validations;

public static class GlobalUserValidation
{
    public static bool IsExistUserEmail(this IRepositoryWrapper repositoryWrapper, string email, Guid? exceptId = default)
    {
        return repositoryWrapper
            .Repository<User>()
            .IsExistByUnique(x => x.Email, email, exceptId);
    }

    public static bool IsExistUsername(this IRepositoryWrapper repositoryWrapper, string username, Guid? exceptId = default)
    {
        return repositoryWrapper
            .Repository<User>()
            .IsExistByUnique(x => x.Username, username, exceptId);
    }

    public static bool IsExistPermissionCodes(this IRepositoryWrapper repositoryWrapper, ICollection<string> codes)
            => repositoryWrapper.Repository<Permission>().Count(x => codes.Contains(x.Code!)) == codes.Count;

    public static bool IsExistRoleCode(this IRepositoryWrapper repositoryWrapper, string code, Guid? exceptId = default)
    {
        return repositoryWrapper
            .Repository<Role>()
            .IsExistByUnique(x => x.Code, code, exceptId);
    }

    public static bool IsExistRole(this IRepositoryWrapper repositoryWrapper, Guid id)
    {
        return repositoryWrapper
            .Repository<Role>()
            .GetById(id) != default;
    }
}