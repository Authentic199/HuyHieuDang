using AutoMapper;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Responses.Roles;

namespace HuyHieuDang.Infrastructure.Modules.Users.Responses.Users;

public class UserDetailResponse : UserResponse
{
    public RoleDetailResponse? Role { get; set; }

    /// <summary>
    /// Danh sách quyền
    /// </summary>
    public List<Permission>? Permissions { get; set; }
}

public class UserDetailResponseProfile : Profile
{
    public UserDetailResponseProfile()
    {
        CreateMap<User, UserDetailResponse>();
    }
}