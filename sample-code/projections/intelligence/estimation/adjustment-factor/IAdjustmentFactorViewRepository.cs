namespace Whycespace.Projections.Intelligence.Estimation.AdjustmentFactor;

public interface IAdjustmentFactorViewRepository
{
    Task SaveAsync(AdjustmentFactorReadModel model, CancellationToken ct = default);
    Task<AdjustmentFactorReadModel?> GetAsync(string id, CancellationToken ct = default);
}
