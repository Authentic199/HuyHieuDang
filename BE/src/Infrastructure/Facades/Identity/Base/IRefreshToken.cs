namespace HuyHieuDang.Infrastructure.Facades.Identity.Base
{
    public interface IRefreshToken
    {
        public string? Token { get; set; }

        public DateTime ExpireTime { get; set; }

        public Guid SessionId { get; set; }
    }
}
