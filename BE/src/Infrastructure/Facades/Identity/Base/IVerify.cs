namespace HuyHieuDang.Infrastructure.Facades.Identity.Base
{
    public interface IPasswordVerify
    {
        public string Password { get; set; }

        public bool Verify(string password) => DefaultVerify(this, password);

        protected static bool DefaultVerify(IPasswordVerify passwordVerify, string password)
            => BCrypt.Net.BCrypt.Verify(password, passwordVerify.Password);

        public void HashPassword(string password) => DefaultHashPassword(this, password);

        protected static void DefaultHashPassword(IPasswordVerify passwordVerify, string password)
        {
            passwordVerify.Password = BCrypt.Net.BCrypt.HashPassword(password);
        }
    }

    public interface IOtpVerify
    {
        public string? OtpCode { get; set; }
    }
}