using Microsoft.Extensions.DependencyInjection;

namespace HuyHieuDang.Infrastructure.Facades.Persistence.Initialization;

internal class CustomSeederRunner
{
    private readonly IDataSeedContributor[] seeders;

    public CustomSeederRunner(IServiceProvider serviceProvider) =>
        seeders = serviceProvider.GetServices<IDataSeedContributor>().ToArray();

    public async Task RunSeedersAsync(CancellationToken cancellationToken)
    {
        foreach (IDataSeedContributor seeder in seeders)
        {
            await seeder.SeedAsync(cancellationToken);
        }
    }
}
