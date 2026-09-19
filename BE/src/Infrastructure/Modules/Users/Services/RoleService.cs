using AutoMapper;
using AutoMapper.QueryableExtensions;
using HuyHieuDang.Core.Bases;
using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure.Facades.Common.Extensions;
using HuyHieuDang.Infrastructure.Facades.Common.Responses;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Services;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using HuyHieuDang.Infrastructure.Modules.Users.Requests.Roles;
using HuyHieuDang.Infrastructure.Modules.Users.Responses.Roles;
using HuyHieuDang.Infrastructure.Modules.Users.Validations;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HuyHieuDang.Infrastructure.Modules.Users.Services
{
    public interface IRoleService : IScopedService
    {
        Task<RoleDetailResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);

        Task<MultipleIdentiferResponse> DeleteRangeAsync(DeleteRoleRangeRequest request, CancellationToken cancellationToken = default);

        Task<RoleDetailResponse> UpdateAsync(Guid roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default);

        Task<PaginationResponse<RoleDetailResponse>> SearchAsync(QueryContainer request, CancellationToken cancellationToken = default);

        Task<RoleDetailResponse> GetAsync(Guid roleId, CancellationToken cancellationToken = default);
    }

    public class RoleService : IRoleService
    {
        private readonly IRepositoryWrapper repositoryWrapper;
        private readonly IMapper mapper;
        private readonly IGpRolePermissionService gPRolePermissionService;

        public RoleService(IRepositoryWrapper repositoryWrapper, IMapper mapper, IGpRolePermissionService gPRoleService)
        {
            this.repositoryWrapper = repositoryWrapper;
            this.mapper = mapper;
            this.gPRolePermissionService = gPRoleService;
        }

        public async Task<RoleDetailResponse> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
        {
            Role role = mapper.Map<Role>(request);
            role.RolePermissions = request.PermissionCodes!.Select(
                permissionCode => new RolePermission()
                {
                    PermissionCode = permissionCode,
                    RoleId = role.Id,
                })
                .ToList();
            role.ModelType = typeof(User).FullName!;
            await repositoryWrapper.Repository<Role>().AddAsync(role, cancellationToken);

            return await GetReponseAsync(role.Id);
        }

        public async Task<MultipleIdentiferResponse> DeleteRangeAsync(DeleteRoleRangeRequest request, CancellationToken cancellationToken = default)
        {
            IQueryable<Role> roles = repositoryWrapper
                .Repository<Role>()
                .Find(x => request.Items!.Contains(x.Id))
                .Include(x => x.ModelRoles);
            if (await roles.CountAsync(cancellationToken) != request.Items!.Count)
            {
                throw new BadRequestException(Messages<Role>.NotFound());
            }

            if (await roles.AnyAsync(x => x.ModelRoles!.Count > 0, cancellationToken))
            {
                throw new BadRequestException(Messages<Role>.WasUsed());
            }

            await repositoryWrapper.Repository<Role>().DeleteRangeAsync(roles, cancellationToken);

            return new(request.Items);
        }

        public async Task<PaginationResponse<RoleDetailResponse>> SearchAsync(QueryContainer request, CancellationToken cancellationToken = default)
            => await repositoryWrapper.Repository<Role>().Find(isAsNoTracking: true)
                    .ProjectTo<RoleDetailResponse>(mapper.ConfigurationProvider)
                    .ApplyFilter(request.Filter)
                    .ApplySearch(request.SearchKeyword, request.SearchFields, request.Filter?.Keys.ToArray())
                    .ApplySort($"{nameof(BaseEntity.CreatedAt)} desc", request.SortQuery)
                    .ToPagedListAsync(request.Current, request.PageSize, cancellationToken: cancellationToken);

        public async Task<RoleDetailResponse> GetAsync(Guid roleId, CancellationToken cancellationToken = default)
            => await repositoryWrapper.Repository<Role>().Find(x => x.Id == roleId)
                .Include(x => x.RolePermissions!).ThenInclude(x => x.Permission)
                .Include(x => x.ModelRoles)
                .ProjectTo<RoleDetailResponse>(mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new BadRequestException(Messages<Role>.NotFound());

        public async Task<RoleDetailResponse> UpdateAsync(Guid roleId, UpdateRoleRequest request, CancellationToken cancellationToken = default)
        {
            ThrowIfExistCode(request.Code!, roleId);
            Role role = await repositoryWrapper.Repository<Role>().GetByIdAsync(roleId, cancellationToken)
                ?? throw new BadRequestException(Messages<Role>.NotFound());

            try
            {
                await repositoryWrapper.BeginTransactionAsync(cancellationToken);
                await gPRolePermissionService.SyncPermissionsAsync(roleId, request.PermissionCodes!, cancellationToken);
                mapper.Map(request, role);
                await repositoryWrapper.Repository<Role>().UpdateAsync(role, cancellationToken);
                await repositoryWrapper.CommitTransactionAsync(cancellationToken);
                return await GetReponseAsync(role.Id);
            }
            catch (Exception ex)
            {
                await repositoryWrapper.RollbackTransactionAsync(cancellationToken);
                Log.Error(ex, "{action} failed", nameof(UpdateAsync));
                throw new InternalServerException(Messages<Role>.Update(false));
            }
        }

        private void ThrowIfExistCode(string code, Guid roleId)
        {
            if (repositoryWrapper.IsExistRoleCode(code, roleId))
            {
                throw new BadRequestException(Messages<Role>.AlreadyExist(x => x.Code));
            }
        }

        private async Task<RoleDetailResponse> GetReponseAsync(Guid roleId)
        {
            return await repositoryWrapper.Repository<Role>()
                .Find(x => x.Id == roleId)
                .Include(x => x.RolePermissions!).ThenInclude(x => x.Permission)
                .Include(x => x.ModelRoles)
                .ProjectTo<RoleDetailResponse>(mapper.ConfigurationProvider)
                .FirstAsync();
        }
    }
}