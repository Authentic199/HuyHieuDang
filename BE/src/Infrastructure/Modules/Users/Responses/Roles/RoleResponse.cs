using AutoMapper;
using HuyHieuDang.Core.Bases;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;

namespace HuyHieuDang.Infrastructure.Modules.Users.Responses.Roles;

public abstract class RoleResponse : BaseEntity<Guid>
{
    public string Code { get; set; } = default!;

    public string Name { get; set; } = default!;
}

public class RoleResponseProfile : Profile
{
    public RoleResponseProfile()
    {
        CreateMap<Role, RoleResponse>();
    }
}