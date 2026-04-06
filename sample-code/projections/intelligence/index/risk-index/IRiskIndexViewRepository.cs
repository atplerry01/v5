namespace Whycespace.Projections.Intelligence.Index.RiskIndex;

public interface IRiskIndexViewRepository
{
    Task SaveAsync(RiskIndexReadModel model, CancellationToken ct = default);
    Task<RiskIndexReadModel?> GetAsync(string id, CancellationToken ct = default);
}
