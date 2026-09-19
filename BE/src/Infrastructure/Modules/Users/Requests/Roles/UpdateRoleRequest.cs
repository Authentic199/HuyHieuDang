using AutoMapper;
using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;

namespace HuyHieuDang.Infrastructure.Modules.Users.Requests.Roles;

public class UpdateRoleRequest : RoleRequest
{
    public string? Code { get; set; }
}

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator(IRepositoryWrapper repositoryWrapper)
    {
        Include(new RoleBaseRequestValidator(repositoryWrapper));
    }
}

public class UpdateRoleRequestProfile : Profile
{
    public UpdateRoleRequestProfile()
    {
        CreateMap<UpdateRoleRequest, Role>();
    }
}