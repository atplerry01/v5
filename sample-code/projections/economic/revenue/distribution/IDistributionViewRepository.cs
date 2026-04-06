namespace Whycespace.Projections.Economic.Revenue.Distribution;

public interface IDistributionViewRepository
{
    Task SaveAsync(DistributionReadModel model, CancellationToken ct = default);
    Task<DistributionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
