using AutoMapper;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Requests.Users;

namespace HuyHieuDang.Infrastructure.Modules.Users.Mappings;

public class UserMapping : Profile
{
    public UserMapping()
    {
        CreateMap<MxmPermission, Permission>();
        CreateMap<Permission, Permission>();
    }
}