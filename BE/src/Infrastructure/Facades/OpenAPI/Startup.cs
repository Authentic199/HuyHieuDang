using HuyHieuDang.Infrastructure.Facades.Middleware;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

namespace HuyHieuDang.Infrastructure.Facades.OpenAPI;

internal static class Startup
{
    internal static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SwaggerSettings>()
           .BindConfiguration(nameof(SwaggerSettings))
           .ValidateDataAnnotationsRecursively()
           .ValidateOnStart();

        SwaggerSettings swaggerSettings = configuration.GetSection(nameof(SwaggerSettings)).Get<SwaggerSettings>()!;
        if (swaggerSettings.Enable)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(swaggerSettings.DefaultInfos?.DocKey, new OpenApiInfo
                {
                    Version = swaggerSettings.DefaultInfos?.Version,
                    Title = swaggerSettings.DefaultInfos?.Title,
                    Description = swaggerSettings.DefaultInfos?.Description,
                });

                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    BearerFormat = "JWT",
                    Description = "Input your Bearer token to access this API",
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                });

                options.SupportNonNullableReferenceTypes();

                options.OperationFilter<SecurityRequirementsOperationFilter>();

                options.SchemaFilter<EnumTypesSchemaFilter>(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));

                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml"));
            });
            services.AddFluentValidationRulesToSwagger();
        }

        return services;
    }

    internal static IApplicationBuilder UseOpenApiDocumentation(this IApplicationBuilder app)
    {
        SwaggerSettings swaggerSettings = app.ApplicationServices.GetRequiredService<IOptions<SwaggerSettings>>().Value;

        if (swaggerSettings.Enable)
        {
            if (!(app as WebApplication)!.Environment.IsDevelopment())
            {
                app.UseSwaggerUIBasicAuthMiddleware();
            }

            app.UseSwagger();
            app.UseSwaggerUI(configure =>
            {
                configure.ConfigObject.PersistAuthorization = true;
                configure.RoutePrefix = swaggerSettings.DefaultInfos?.RoutePrefix;
                configure.SwaggerEndpoint($"/swagger/{swaggerSettings.DefaultInfos?.DocKey}/swagger.json", swaggerSettings.DefaultInfos?.DocName);
                configure.EnableDeepLinking();
                configure.DocExpansion(DocExpansion.None);
            });
        }

        return app;
    }
}