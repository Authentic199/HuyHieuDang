using AutoMapper;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;

namespace HuyHieuDang.Infrastructure.Modules.Users.Responses.Permissions
{
    public class PermissionResponse
    {
        public string Code { get; set; } = default!;

        public string Name { get; set; } = default!;

        public string Resource { get; set; } = default!;
    }

    public class PermissionResponseProfile : Profile
    {
        public PermissionResponseProfile()
        {
            CreateMap<Permission, PermissionResponse>();
        }
    }
}