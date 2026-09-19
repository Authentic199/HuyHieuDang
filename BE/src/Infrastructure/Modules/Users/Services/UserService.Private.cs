using AutoMapper.QueryableExtensions;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Responses.Users;
using Microsoft.EntityFrameworkCore;

namespace HuyHieuDang.Infrastructure.Modules.Users.Services;

public partial class UserService
{
    private async Task<UserDetailResponse> GetReponseAsync(Guid userId)
    {
        UserDetailResponse response = await repositoryWrapper.Repository<User>()
            .Find(x => x.Id == userId)
            .Include(x => x.Role)
            .ProjectTo<UserDetailResponse>(mapper.ConfigurationProvider)
            .FirstAsync();
        response.Permissions = await gpModelPermissionService.GetAllPermissions<User>(userId).ToListAsync();
        return response;
    }
}