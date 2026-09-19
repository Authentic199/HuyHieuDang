using AutoMapper;
using AutoMapper.QueryableExtensions;
using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure.Facades.Auth;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Common.Responses;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.Base;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Services;
using HuyHieuDang.Infrastructure.Facades.Identity.JwtToken;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.Auth.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Requests.Authentications;
using HuyHieuDang.Infrastructure.Modules.Users.Requests.Users;
using HuyHieuDang.Infrastructure.Modules.Users.Responses.Authentications;
using HuyHieuDang.Infrastructure.Modules.Users.Responses.Users;
using HuyHieuDang.Infrastructure.Modules.Users.Validations;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HuyHieuDang.Infrastructure.Modules.Users.Services;

public partial interface IUserService : IScopedService
{
    Task<UserDetailResponse> DetailAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<PaginationResponse<UserDetailResponse>> SearchAsync(QueryContainer request, CancellationToken cancellationToken = default);

    Task<UserDetailResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<UserDetailResponse> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task<MultipleIdentiferResponse> DeleteRangeAsync(DeleteUserRequest request, CancellationToken cancellationToken = default);
}

public partial class UserService : IUserService
{
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IGpModelRoleService gpModelRoleService;
    private readonly IGpModelPermissionService gpModelPermissionService;
    private readonly IMapper mapper;
    private readonly ICurrentUser currentUser;
    private readonly IJwtTokenGenerator tokenGenerator;

    public UserService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IGpModelRoleService gpModelRoleService, ICurrentUser currentUser, IJwtTokenGenerator tokenGenerator, IGpModelPermissionService gpModelPermissionService)
    {
        this.repositoryWrapper = repositoryWrapper;
        this.mapper = mapper;
        this.gpModelRoleService = gpModelRoleService;
        this.currentUser = currentUser;
        this.tokenGenerator = tokenGenerator;
        this.gpModelPermissionService = gpModelPermissionService;
    }

    public async Task<UserDetailResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        User user = mapper.Map<User>(request);
        user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        await repositoryWrapper.BeginTransactionAsync(cancellationToken);
        try
        {
            await repositoryWrapper.Repository<User>().AddAsync(user, cancellationToken);
            await gpModelRoleService.AssignRoleAsync<User>(user.Id, user.RoleId!.Value, cancellationToken);
            await repositoryWrapper.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await repositoryWrapper.RollbackTransactionAsync(cancellationToken);
            throw new InternalServerException(Messages<User>.Create(false), ex);
        }

        return await GetReponseAsync(user.Id);
    }

    public async Task<UserDetailResponse> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        User? user = await repositoryWrapper.Repository<User>().GetByIdAsync(userId, cancellationToken)
            ?? throw new BadRequestException(Messages<User>.NotFound());

        if (repositoryWrapper.IsExistUserEmail(request.Email!, userId))
        {
            throw new BadRequestException(Messages<User>.AlreadyExist(x => x.Email));
        }

        await repositoryWrapper.BeginTransactionAsync(cancellationToken);
        try
        {
            if (request.RoleId != user.RoleId)
            {
                await gpModelRoleService.SyncRolesAsync<User>(user.Id, new List<Guid> { request.RoleId }, cancellationToken);
            }

            mapper.Map(request, user);

            await repositoryWrapper.Repository<User>().UpdateAsync(user, cancellationToken);

            await repositoryWrapper.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await repositoryWrapper.RollbackTransactionAsync(cancellationToken);
            throw new InternalServerException(Messages<User>.Update(false), ex);
        }

        return await GetReponseAsync(user.Id);
    }

    public async Task<UserDetailResponse> DetailAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        UserDetailResponse response = await repositoryWrapper.Repository<User>()
            .Find(x => x.Id == userId)
            .Include(x => x.Role)
            .ProjectTo<UserDetailResponse>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new BadRequestException(Messages<User>.NotFound());
        response.Permissions = await gpModelPermissionService.GetAllPermissions<User>(userId).ToListAsync(cancellationToken);
        return response;
    }

    public async Task<PaginationResponse<UserDetailResponse>> SearchAsync(QueryContainer request, CancellationToken cancellationToken = default)
    {
        return await repositoryWrapper.Repository<User>()
            .Find()
            .ProjectTo<UserDetailResponse>(mapper.ConfigurationProvider)
            .ApplyFilter(request.Filter)
            .ApplySort($"{nameof(User.CreatedAt)} {OrderTypeAcronym.Desc}", request.SortQuery)
            .ApplySearch(request.SearchKeyword, request.SearchFields, request.Filter?.Keys.ToArray())
            .ToPagedListAsync(request.Current, request.PageSize, cancellationToken: cancellationToken);
    }

    public async Task<MultipleIdentiferResponse> DeleteRangeAsync(DeleteUserRequest request, CancellationToken cancellationToken = default)
    {
        ICollection<User> users = await repositoryWrapper.Repository<User>().Find(x => request.Items!.Contains(x.Id)).ToArrayAsync(cancellationToken);
        if (users.Count != request.Items!.Count)
        {
            throw new BadRequestException(Messages<User>.NotFound());
        }

        await repositoryWrapper.BeginTransactionAsync(cancellationToken);
        try
        {
            await gpModelRoleService.RemoveModelRoleAsync<User>(request.Items, cancellationToken);
            await repositoryWrapper.Repository<User>().DeleteRangeAsync(users, cancellationToken);
            await repositoryWrapper.CommitTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await repositoryWrapper.RollbackTransactionAsync(cancellationToken);
            throw new InternalServerException(Messages<User>.Delete(false), ex);
        }

        return new(request.Items);
    }
}