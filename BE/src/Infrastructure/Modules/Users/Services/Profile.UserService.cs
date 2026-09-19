using AutoMapper.QueryableExtensions;
using HuyHieuDang.Infrastructure.Exceptions.HttpExceptions;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Requests.Profiles;
using HuyHieuDang.Infrastructure.Modules.Users.Responses.Users;
using Microsoft.EntityFrameworkCore;

namespace HuyHieuDang.Infrastructure.Modules.Users.Services
{
    public partial interface IUserService
    {
        Task<UserDetailResponse> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default);

        Task<UserDetailResponse> ProfileAsync(CancellationToken cancellationToken = default);
    }

    public partial class UserService
    {
        public async Task<UserDetailResponse> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
        {
            Guid userId = currentUser.GetUserId();
            User? user = await repositoryWrapper.Repository<User>().GetByIdAsync(userId, cancellationToken)
                ?? throw new UnAuthorizedException(Messages<User>.NotFound());

            mapper.Map(request, user);
            await repositoryWrapper.Repository<User>().UpdateAsync(user, cancellationToken);
            return mapper.Map<UserDetailResponse>(user);
        }

        public async Task<UserDetailResponse> ProfileAsync(CancellationToken cancellationToken = default)
        {
            Guid userId = currentUser.GetUserId();

            UserDetailResponse response = await repositoryWrapper.Repository<User>()
                .Find(x => x.Id == userId)
                .Include(x => x.Role)
                .ProjectTo<UserDetailResponse>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new UnAuthorizedException(Messages<User>.NotFound());
            response.Permissions = await gpModelPermissionService.GetAllPermissions<User>(userId).ToListAsync();
            return response;
        }
    }
}