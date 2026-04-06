namespace Whycespace.Projections.Intelligence.Estimation.CostEstimate;

public interface ICostEstimateViewRepository
{
    Task SaveAsync(CostEstimateReadModel model, CancellationToken ct = default);
    Task<CostEstimateReadModel?> GetAsync(string id, CancellationToken ct = default);
}
