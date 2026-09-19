using HuyHieuDang.Core.Bases;
using HuyHieuDang.Infrastructure.Facades.Definitions;
using System.Security.Claims;

namespace HuyHieuDang.Infrastructure.Facades.Identity.Base
{
    public interface IUser
    {
        public OperationStatus Status { get; }
    }

    public interface IJwtUser : IUser, IGuidIdentify
    {
        public IEnumerable<Claim> UseClaims() => GetDefaultClaims(this);

        protected static IEnumerable<Claim> GetDefaultClaims(IJwtUser user)
             => new List<Claim>
            {
                new Claim(JwtTokenPayload.Identification, user.Id.ToString()),
                new Claim(JwtTokenPayload.ModelType, user.GetType().FullName!),
            };
    }

    /// <summary>
    /// Trạng thái hoạt động
    /// </summary>
    public enum OperationStatus : byte
    {
        /// <summary>
        /// Users are allowed to access the system.
        /// </summary>
        Active = 1,

        /// <summary>
        /// User is prohibited from accessing the system.
        /// </summary>
        Lock = 2,
    }
}