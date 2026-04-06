namespace Whycespace.Projections.Business.Portfolio.Rebalance;

public interface IRebalanceViewRepository
{
    Task SaveAsync(RebalanceReadModel model, CancellationToken ct = default);
    Task<RebalanceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
