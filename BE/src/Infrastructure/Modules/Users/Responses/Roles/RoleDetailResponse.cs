using AutoMapper;
using HuyHieuDang.Core.Bases;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;

namespace HuyHieuDang.Infrastructure.Modules.Users.Responses.Roles;

public class RoleDetailResponse : RoleResponse
{
    public int NumberOfAssign { get; set; }

    public List<Permission> Permissions { get; set; } = default!;
}

public class RoleDetailResponseProfile : Profile
{
    public RoleDetailResponseProfile()
    {
        CreateMap<Role, RoleDetailResponse>()
            .ForMember(dest => dest.NumberOfAssign, opt => opt.MapFrom(src => src.ModelRoles!.Count))
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.RolePermissions!.Select(x => x.Permission).ToList()));
    }
}