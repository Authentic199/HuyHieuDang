using AutoMapper;
using AutoMapper.QueryableExtensions;
using HuyHieuDang.Core.Common.Interfaces;
using HuyHieuDang.Infrastructure.Facades.Identity.GrantPermission.Entities;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;
using HuyHieuDang.Infrastructure.Modules.Users.Responses.Permissions;
using Microsoft.EntityFrameworkCore;

namespace HuyHieuDang.Infrastructure.Modules.Users.Services
{
    public interface IPermissionService : IScopedService
    {
        Task<IEnumerable<PermissionResponse>> GetAllAsync();
    }

    public class PermissionService : IPermissionService
    {
        private readonly IRepositoryWrapper repositoryWrapper;
        private readonly IMapper mapper;

        public PermissionService(IRepositoryWrapper repositoryWrapper, IMapper mapper)
        {
            this.repositoryWrapper = repositoryWrapper;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<PermissionResponse>> GetAllAsync()
        {
            return await repositoryWrapper.Repository<Permission>().Find().ProjectTo<PermissionResponse>(mapper.ConfigurationProvider).ToListAsync();
        }
    }
}