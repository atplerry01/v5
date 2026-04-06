namespace Whycespace.Projections.Decision.Risk.Threshold;

public interface IThresholdViewRepository
{
    Task SaveAsync(ThresholdReadModel model, CancellationToken ct = default);
    Task<ThresholdReadModel?> GetAsync(string id, CancellationToken ct = default);
}
