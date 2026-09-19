using HuyHieuDang.Infrastructure.Facades.Persistence.Contexts;

namespace HuyHieuDang.Infrastructure.Facades.Persistence.Initialization;

internal interface IDbInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken);
}

internal class DbInitializer : IDbInitializer
{
    private readonly ApplicationDbContext applicationDbContext;
    private readonly CustomSeederRunner seederRunner;

    public DbInitializer(CustomSeederRunner seederRunner, ApplicationDbContext applicationDbContext)
    {
        this.seederRunner = seederRunner;
        this.applicationDbContext = applicationDbContext;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (await applicationDbContext.Database.CanConnectAsync(cancellationToken))
        {
            await seederRunner.RunSeedersAsync(cancellationToken);
        }
    }
}
