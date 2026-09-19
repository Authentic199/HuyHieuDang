using HuyHieuDang.Infrastructure.Facades.Auth;
using HuyHieuDang.Infrastructure.Facades.Auth.Jwt;
using HuyHieuDang.Infrastructure.Facades.Identity.Base;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HuyHieuDang.Infrastructure.Facades.Identity.JwtToken
{
    public interface IJwtTokenGenerator
    {
        JwtSettings GetSettingByScheme(string? scheme = null);

        string GenerateAccessToken(JwtSettings jwtSetting, IJwtUser user);

        string GenerateRefreshToken(JwtSettings jwtSetting, params Claim[] claims);

        SecurityToken? ValidateRefreshToken(JwtSettings jwtSetting, string refreshToken);

        string? GetClaimsValue(string? token, string claimType)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwtToken = null;
            try
            {
                jwtToken = handler.ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
            }
            catch
            {
                return null;
            }
        }
    }

    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        public JwtTokenGenerator(IOptions<SecuritySettings> settingOption)
        {
            AllSettings = settingOption.Value.JwtSettingOptions;
        }

        public JwtSettings GetSettingByScheme(string? scheme = null)
           => (JwtSettings?)AllSettings.GetType().GetProperty(scheme ?? nameof(JwtSettingOptions.Default))?.GetValue(AllSettings)
               ?? throw new InvalidOperationException($"The scheme is not declared from {typeof(JwtSettingOptions).FullName} or is not registered in the dependency injection container.");

        public JwtSettingOptions AllSettings { get; }

        public string GenerateAccessToken(JwtSettings jwtSetting, IJwtUser user)
        {
            SigningCredentials creds = new(jwtSetting.GetSecurityKey(), SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new(
                jwtSetting.Issuer,
                jwtSetting.IsAudience,
                expires: jwtSetting.GetAccessTokenExpired(),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken(JwtSettings jwtSetting, params Claim[] claims)
        {
            SigningCredentials creds = new(jwtSetting.GetSecurityRefreshKey(), SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new(
                jwtSetting.Issuer,
                jwtSetting.IsAudience,
                claims,
                expires: jwtSetting.GetRefreshTokenExpired(),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public SecurityToken? ValidateRefreshToken(JwtSettings jwtSetting, string refreshToken)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            var key = jwtSetting.GetSecurityRefreshKey();
            TokenValidationParameters validationParameters = new()
            {
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidIssuer = jwtSetting.Issuer,
                ValidAudience = jwtSetting.IsAudience,
                RequireExpirationTime = false,
                ValidateLifetime = true,
            };

            SecurityToken? validatedToken;

            try
            {
                tokenHandler.ValidateToken(refreshToken, validationParameters, out validatedToken);
            }
            catch (Exception)
            {
                validatedToken = null;
            }

            return validatedToken;
        }
    }
}