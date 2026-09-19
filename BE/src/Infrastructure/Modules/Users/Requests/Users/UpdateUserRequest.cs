using AutoMapper;
using FluentValidation;
using HuyHieuDang.Infrastructure.Facades.Common.Attributes;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;

namespace HuyHieuDang.Infrastructure.Modules.Users.Requests.Users;

[MessageDisplay(nameof(User))]
public class UpdateUserRequest : UserRequest
{
}

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator(IRepositoryWrapper repositoryWrapper)
    {
        Include(new BaseUserRequestValidator(repositoryWrapper));
    }
}

public class UpdateUserRequestProfile : Profile
{
    public UpdateUserRequestProfile()
    {
        CreateMap<UpdateUserRequest, User>();
    }
}