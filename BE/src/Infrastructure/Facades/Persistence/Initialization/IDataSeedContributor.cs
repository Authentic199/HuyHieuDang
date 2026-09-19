namespace HuyHieuDang.Infrastructure.Facades.Persistence.Initialization;

public interface IDataSeedContributor
{
    Task SeedAsync(CancellationToken cancellationToken);
}
