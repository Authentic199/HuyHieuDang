using HuyHieuDang.Core.Common.Exceptions;
using HuyHieuDang.Infrastructure.Exceptions.HttpExceptions;
using HuyHieuDang.Infrastructure.Facades.Auth;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using HuyHieuDang.Infrastructure.Facades.Identity.Base;
using HuyHieuDang.Infrastructure.Facades.Persistence.Repositories;

namespace HuyHieuDang.Infrastructure.Facades.Identity.Password.Services
{
    public interface IChangePasswordService
    {
        Task<T> ChangePasswordAsync<T>(string oldPassword, string newPassword, CancellationToken cancellationToken = default)
            where T : class, IPasswordVerify;
    }

    public class ChangePasswordService : IChangePasswordService
    {
        private readonly IRepositoryWrapper repositoryWrapper;
        private readonly ICurrentUser currentUser;

        public ChangePasswordService(IRepositoryWrapper repositoryWrapper, ICurrentUser currentUser)
        {
            this.repositoryWrapper = repositoryWrapper;
            this.currentUser = currentUser;
        }

        public async Task<T> ChangePasswordAsync<T>(string oldPassword, string newPassword, CancellationToken cancellationToken = default)
            where T : class, IPasswordVerify
        {
            Guid id = currentUser.GetUserId();

            T entity = await repositoryWrapper.Repository<T>().GetByIdAsync(id, cancellationToken)
                ?? throw new UnAuthorizedException(Messages<T>.NotFound());

            bool success = BCrypt.Net.BCrypt.Verify(oldPassword, entity.Password);
            if (!success)
            {
                throw new BadRequestException(Messages<T>.Action("IsWrong.OldPassword"));
            }

            entity.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await repositoryWrapper.Repository<T>().UpdateAsync(entity, cancellationToken);
            return entity;
        }
    }
}