namespace Whycespace.Projections.Intelligence.Estimation.PriceEstimate;

public interface IPriceEstimateViewRepository
{
    Task SaveAsync(PriceEstimateReadModel model, CancellationToken ct = default);
    Task<PriceEstimateReadModel?> GetAsync(string id, CancellationToken ct = default);
}
