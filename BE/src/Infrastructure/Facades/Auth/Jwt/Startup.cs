using HuyHieuDang.Infrastructure.Modules.Users.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;

namespace HuyHieuDang.Infrastructure.Facades.Auth.Jwt;

internal static class Startup
{
    internal static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SecuritySettings>()
          .BindConfiguration(nameof(SecuritySettings))
          .ValidateDataAnnotationsRecursively()
          .ValidateOnStart();

        SecuritySettings securitySettings = configuration.GetRequiredSection(nameof(SecuritySettings)).Get<SecuritySettings>()!;
        var defaultSigningKey = securitySettings.JwtSettingOptions.Default.GetSecurityKey();

        services.AddAuthentication(options =>
        {
            options.DefaultChallengeScheme = JwtScheme.MultipleScheme;
            options.DefaultScheme = JwtScheme.MultipleScheme;
        })
         .AddJwtBearer(JwtScheme.Default, options =>
         {
             options.SaveToken = true;
             options.TokenValidationParameters = new()
             {
                 ValidateIssuerSigningKey = true,
                 IssuerSigningKey = defaultSigningKey,
                 ValidateIssuer = false,
                 ValidateAudience = false,
                 ClockSkew = TimeSpan.Zero,
             };
         })
         .AddPolicyScheme(JwtScheme.MultipleScheme, JwtScheme.MultipleScheme, options =>
         {
             options.ForwardDefaultSelector = context =>
             {
                 string? authorization = context.Request.Headers[HeaderNames.Authorization];
                 string scheme = JwtBearerDefaults.AuthenticationScheme;

                 if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                 {
                     var token = authorization["Bearer ".Length..].Trim();
                     var jwtHandler = new JwtSecurityTokenHandler();

                     if (jwtHandler.CanReadToken(token))
                     {
                         var jwtToken = jwtHandler.ReadJwtToken(token);
                         string? type = jwtToken.Claims.FirstOrDefault(x => x.Type == Definitions.JwtTokenPayload.ModelType)?.Value;

                         switch (type)
                         {
                             case string when typeof(User).FullName == type:
                                 scheme = JwtScheme.Default;
                                 break;

                             default:
                                 break;
                         }
                     }
                 }

                 return scheme;
             };
         });
        return services;
    }
}