using HuyHieuDang.Infrastructure.Facades.Identity.Password.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HuyHieuDang.Infrastructure.Facades.Identity.Password
{
    internal static class Startup
    {
        internal static IServiceCollection AddPasswordService(this IServiceCollection services)
        {
            services.AddTransient(typeof(IChangePasswordService), typeof(ChangePasswordService));
            return services;
        }
    }
}
