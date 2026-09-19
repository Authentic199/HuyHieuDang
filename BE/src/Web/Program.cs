using HuyHieuDang.Infrastructure;
using HuyHieuDang.Infrastructure.Facades.Common.Converters;
using HuyHieuDang.Infrastructure.Facades.Logging;
using HuyHieuDang.Web.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Serilog;
using System.Text.Json.Serialization;

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddConfigurations().RegisterSerilog();

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new DateTimeOffsetConverter());
        options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
    })
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(new { message = context.ModelState?.FirstOrDefault(x => x.Value.ValidationState is ModelValidationState.Invalid).Value?.Errors[0].ErrorMessage }));

    var app = builder.Build();

    await app.Services.InitializeDatabasesAsync();

    app.UseInfrastructure(builder.Configuration);

    app.Run();

    app.Lifetime.ApplicationStopping.Register(() =>
    {
        StaticLogger.EnsureInitialized();
        Log.Information("Application Stopping...");
        Log.CloseAndFlush();
    });
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    StaticLogger.EnsureInitialized();
    Log.Fatal(ex, "Unhandled Exception");
}
finally
{
    StaticLogger.EnsureInitialized();
    Log.Information("Server Shutting Down...");
    await Log.CloseAndFlushAsync();
}