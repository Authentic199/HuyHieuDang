using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;

namespace HuyHieuDang.Infrastructure.Facades.OpenAPI;

// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
public class SwaggerUIBasicAuthMiddleware
{
    private readonly RequestDelegate next;
    private readonly IConfiguration configuration;

    public SwaggerUIBasicAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        this.next = next;
        this.configuration = configuration;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        SwaggerSettings swaggerSettings = configuration.GetRequiredSection(nameof(SwaggerSettings)).Get<SwaggerSettings>()!;

        string routePrefix = swaggerSettings.DefaultInfos.RoutePrefix;

        if (httpContext.Request.Path.StartsWithSegments($"/{routePrefix}", StringComparison.Ordinal))
        {
            string authHeader = httpContext.Request.Headers[HeaderNames.Authorization].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.Ordinal))
            {
                SetUnauthorizedResponse(httpContext);
                return;
            }

            string encodedCredentials = authHeader[6..]; // Remove "Basic " prefix
            string decodedCredentials = Encoding.ASCII.GetString(Convert.FromBase64String(encodedCredentials));

            string expectedCredentials = $"{swaggerSettings.Credentials.UserName}:{swaggerSettings.Credentials.Password}";

            if (decodedCredentials == expectedCredentials)
            {
                await next(httpContext);
            }
            else
            {
                SetUnauthorizedResponse(httpContext);
            }
        }
        else
        {
            await next(httpContext);
        }
    }

    private static void SetUnauthorizedResponse(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"Swagger UI\"");
    }
}
