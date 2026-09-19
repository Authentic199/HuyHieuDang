using Humanizer;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace HuyHieuDang.Infrastructure.Facades.Auth.Jwt
{
    public static class JwtScheme
    {
        public const string Default = JwtBearerDefaults.AuthenticationScheme;
        public const string MultipleScheme = nameof(MultipleScheme);
    }
}
