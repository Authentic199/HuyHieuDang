using HuyHieuDang.Infrastructure.Facades.OpenAPI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HuyHieuDang.Infrastructure.Facades.Middleware;

internal static class Startup
{
    public static IApplicationBuilder UseExceptionHandlerMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlerMiddleware>();

    public static IApplicationBuilder UseSwaggerUIBasicAuthMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<SwaggerUIBasicAuthMiddleware>();

    public static IApplicationBuilder UseVerifyJwtUserMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<VerifyJwtUserMiddleware>();
}