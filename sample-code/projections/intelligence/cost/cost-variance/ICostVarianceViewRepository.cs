namespace Whycespace.Projections.Intelligence.Cost.CostVariance;

public interface ICostVarianceViewRepository
{
    Task SaveAsync(CostVarianceReadModel model, CancellationToken ct = default);
    Task<CostVarianceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
